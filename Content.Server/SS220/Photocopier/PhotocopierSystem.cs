using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.SS220.Photocopier;
using Content.Shared.SS220.Photocopier.Forms;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Server.UserInterface;
using Content.Server.Power.Components;
using Content.Server.Paper;
using Content.Server.Power.EntitySystems;
using Content.Server.SS220.Photocopier.Forms;
using Content.Shared.Humanoid;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Systems;
using Robust.Server.GameObjects;

namespace Content.Server.SS220.Photocopier;

public sealed class PhotocopierSystem : EntitySystem
{
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    private FormManager? _specificFormManager;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _specificFormManager = _sysMan.GetEntitySystem<FormManager>();

        SubscribeLocalEvent<PhotocopierComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<PhotocopierComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<PhotocopierComponent, EntInsertedIntoContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<PhotocopierComponent, EntRemovedFromContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<PhotocopierComponent, PowerChangedEvent>(OnPowerChanged);

        // UI
        SubscribeLocalEvent<PhotocopierComponent, AfterActivatableUIOpenEvent>(OnToggleInterface);
        SubscribeLocalEvent<PhotocopierComponent, PhotocopierPrintMessage>(OnPrintButtonPressed);
        SubscribeLocalEvent<PhotocopierComponent, PhotocopierCopyMessage>(OnCopyButtonPressed);
        SubscribeLocalEvent<PhotocopierComponent, PhotocopierStopMessage>(OnStopButtonPressed);
        SubscribeLocalEvent<PhotocopierComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<PhotocopierComponent, PhotocopierRefreshUiMessage>(OnRefreshUiMessage);
    }

    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PhotocopierComponent, ApcPowerReceiverComponent>();
        while (query.MoveNext(out var uid, out var photocopier, out var receiver))
        {
            if (!receiver.Powered)
                continue;

            ProcessPrinting(uid, frameTime, photocopier);
        }
    }

    /// <summary>
    /// Try to get a HumanoidAppearanceComponent of one of the creatures that are on top of the photocopier at the moment.
    /// Returns null if there are none.
    /// </summary>
    private HumanoidAppearanceComponent? GetCreatureOnTop(EntityUid uid, PhotocopierComponent? component, TransformComponent? xform)
    {
        if (!Resolve(uid, ref component, ref xform, false))
            return null;

        var map = xform.MapID;

        var bounds = _physics.GetWorldAABB(uid);

        // We shrink the box greatly to ensure it only intersects with the objects that are on top of the photocopier.
        // May be a hack, but at least it works reliably (on my computer)
        // lerp alpha (effective alpha will be twice as big since we perform lerp on both corners)
        var shrinkCoefficient = 0.4f;
        // lerp corners towards each other
        var boundsTR = new Vector2(bounds.TopRight.X, bounds.TopRight.Y);
        var boundsBL = new Vector2(bounds.BottomLeft.X, bounds.BottomLeft.Y);
        bounds.TopRight = (boundsBL - boundsTR) * shrinkCoefficient + boundsTR;
        bounds.BottomLeft = (boundsTR - boundsBL) * shrinkCoefficient + boundsBL;

        var intersecting = _entityLookup.GetComponentsIntersecting<HumanoidAppearanceComponent>(
            map, bounds, LookupFlags.Dynamic | LookupFlags.Sundries);

        return intersecting.Count > 0 ? intersecting.ElementAt(0) : null;
    }

    private bool TryGetTonerCartridge(
        EntityUid uid,
        PhotocopierComponent component,
        [NotNullWhen(true)] out TonerCartridgeComponent? tonerCartridgeComponent)
    {
        var tonerSlotItem = component.TonerSlot.Item;
        if (tonerSlotItem != null)
            return TryComp<TonerCartridgeComponent>(tonerSlotItem, out tonerCartridgeComponent);

        tonerCartridgeComponent = null;
        return false;
    }

    private void OnComponentInit(EntityUid uid, PhotocopierComponent component, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, PhotocopierComponent.PaperSlotId, component.PaperSlot);
        _itemSlotsSystem.AddItemSlot(uid, PhotocopierComponent.TonerSlotId, component.TonerSlot);
        TryUpdateVisualState(uid, component);
    }

    private void OnComponentRemove(EntityUid uid, PhotocopierComponent component, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, component.PaperSlot);
        _itemSlotsSystem.RemoveItemSlot(uid, component.TonerSlot);
    }

    private void OnToggleInterface(EntityUid uid, PhotocopierComponent component, AfterActivatableUIOpenEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    private static void OnExamine(EntityUid uid, PhotocopierComponent component, ExaminedEvent args)
    {
        if (component.PaperSlot.Item == null)
            return;

        args.PushText(Loc.GetString("photocopier-examine-scan-got-item"));
    }

    private void OnItemSlotChanged(EntityUid uid, PhotocopierComponent component, ContainerModifiedMessage args)
    {
        switch (args.Container.ID)
        {
            case PhotocopierComponent.PaperSlotId:
                if (component.PaperSlot.Item != null && this.IsPowered(uid, EntityManager))
                    _audioSystem.PlayPvs(component.PaperInsertSound, uid);
                break;

            case PhotocopierComponent.TonerSlotId when component.TonerSlot.Item == null:
                StopPrinting(uid, component);
                break;
        }

        UpdateUserInterface(uid, component);
        TryUpdateVisualState(uid, component);
    }

    private void OnPowerChanged(EntityUid uid, PhotocopierComponent component, ref PowerChangedEvent args)
    {
        if (!args.Powered)
        {
            StopPrinting(uid, component);
        }

        TryUpdateVisualState(uid, component);
    }

    private void OnCopyButtonPressed(EntityUid uid, PhotocopierComponent component, PhotocopierCopyMessage args)
    {
        if (!component.Initialized)
            return;

        if (component.CopiesQueued > 0)
            return;

        var copyEntity = component.PaperSlot.Item;
        if (copyEntity == null)
            return;

        if (!TryGetTonerCartridge(uid, component, out var tonerCartridge) || tonerCartridge.Charges <= 0)
            return;

        if (!TryComp<MetaDataComponent>(copyEntity, out var metadata) ||
            !TryComp<PaperComponent>(copyEntity, out var paper))
            return;

        component.DataToCopy = new Form(
            metadata.EntityName,
            paper.Content,
            prototypeId: metadata.EntityPrototype?.ID,
            stampState: paper.StampState,
            stampedBy: paper.StampedBy);

        component.IsScanning = true;
        component.CopiesQueued = Math.Min(args.Amount, component.MaxQueueLength);
        // Do it even if max queue length is <1
        if (component.CopiesQueued <= 0)
            component.CopiesQueued = 1;
    }

    private void OnPrintButtonPressed(EntityUid uid, PhotocopierComponent component, PhotocopierPrintMessage args)
    {
        if (!component.Initialized)
            return;

        if(_specificFormManager == null)
            return;

        if (component.CopiesQueued > 0)
            return;

        if (!TryGetTonerCartridge(uid, component, out var tonerCartridge) || tonerCartridge.Charges <= 0)
            return;

        component.DataToCopy = _specificFormManager.TryGetFormFromDescriptor(args.Descriptor);
        if (component.DataToCopy == null)
            return;

        component.CopiesQueued = Math.Min(args.Amount, component.MaxQueueLength);
        // Do it even if max queue length is <1
        if (component.CopiesQueued <= 0)
            component.CopiesQueued = 1;
    }

    private void OnStopButtonPressed(EntityUid uid, PhotocopierComponent component, PhotocopierStopMessage args)
    {
        StopPrinting(uid, component);
        TryUpdateVisualState(uid, component);
        UpdateUserInterface(uid, component);
    }

    /// <summary>
    /// Stops PhotocopierComponent from printing and clears queue, effectively resetting it into normal state.
    /// </summary>
    private void StopPrinting(EntityUid uid, PhotocopierComponent component)
    {
        if (component.CopiesQueued == 0)
            return;

        component.CopiesQueued = 0;
        component.PrintingTimeRemaining = 0;
        component.DataToCopy = null;
        component.IsScanning = false;

        _itemSlotsSystem.SetLock(uid, component.PaperSlot, false);
        StopPrintingSound(component);
    }

    /// <summary>
    /// Spawns a copy of paper using data cached in component.DataToCopy.
    /// </summary>
    private void SpawnPaperCopy(EntityUid uid, PhotocopierComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var printout = component.DataToCopy;
        if (printout == null)
            return;

        var entityToSpawn = string.IsNullOrEmpty(printout.PrototypeId) ? "Paper" : printout.PrototypeId;
        var printed = EntityManager.SpawnEntity(entityToSpawn, Transform(uid).Coordinates);

        if (TryComp<PaperComponent>(printed, out var paper))
        {
            _paperSystem.SetContent(printed, printout.Content);

            // Apply stamps
            if (printout.StampState != null)
            {
                foreach (var stampedBy in printout.StampedBy)
                {
                    _paperSystem.TryStamp(printed, stampedBy, printout.StampState);
                }
            }
        }

        if (!TryComp<MetaDataComponent>(printed, out var metadata))
            return;

        if (!string.IsNullOrEmpty(printout.EntityName))
            metadata.EntityName = printout.EntityName;
    }

    private void ProcessPrinting(EntityUid uid, float frameTime, PhotocopierComponent component)
    {
        if (component.PrintingTimeRemaining > 0)
        {
            if (!TryGetTonerCartridge(uid, component, out var tonerCartridge) || tonerCartridge.Charges <= 0)
            {
                StopPrinting(uid, component);
                UpdateUserInterface(uid, component);
                TryUpdateVisualState(uid, component);
                return;
            }

            component.PrintingTimeRemaining -= frameTime;

            var isPrinted = component.PrintingTimeRemaining <= 0;
            if (isPrinted)
            {
                SpawnPaperCopy(uid, component);
                tonerCartridge.Charges--;
                component.CopiesQueued--;

                if (component.CopiesQueued <= 0)
                {
                    _itemSlotsSystem.SetLock(uid, component.PaperSlot, false);
                    component.IsScanning = false;
                }

                UpdateUserInterface(uid, component);
                TryUpdateVisualState(uid, component);
            }

            return;
        }

        if (component.CopiesQueued > 0)
        {
            if (!TryGetTonerCartridge(uid, component, out var tonerCartridge) || tonerCartridge.Charges <= 0)
            {
                StopPrinting(uid, component);
                UpdateUserInterface(uid, component);
                TryUpdateVisualState(uid, component);
                return;
            }

            component.PrintingTimeRemaining = component.PrintingTime;
            component.PrintAudioStream = _audioSystem.PlayPvs(component.PrintSound, uid);

            if (component.IsScanning)
                _itemSlotsSystem.SetLock(uid, component.PaperSlot, true);

            UpdateUserInterface(uid, component);
            TryUpdateVisualState(uid, component);
        }
    }

    private void TryUpdateVisualState(EntityUid uid, PhotocopierComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var state = PhotocopierVisualState.Powered;
        var outOfToner = (!TryGetTonerCartridge(uid, component, out var tonerCartridge) || tonerCartridge.Charges <= 0);

        if (component.CopiesQueued > 0)
            state = component.IsScanning? PhotocopierVisualState.Copying : PhotocopierVisualState.Printing;
        else if (!this.IsPowered(uid, EntityManager))
            state = PhotocopierVisualState.Off;
        else if (outOfToner)
            state = PhotocopierVisualState.OutOfToner;

        var item = component.PaperSlot.Item;
        var gotItem = item != null;
        var combinedState = new PhotocopierCombinedVisualState(state, gotItem);

        _appearanceSystem.SetData(uid, PhotocopierVisuals.VisualState, combinedState);
    }

    private void OnRefreshUiMessage(EntityUid uid, PhotocopierComponent component, PhotocopierRefreshUiMessage args)
    {
        UpdateUserInterface(uid, component);
    }

    /// <summary>
    /// Stops audio stream of a printing sound, dereferences it
    /// </summary>
    private static void StopPrintingSound(PhotocopierComponent component)
    {
        component.PrintAudioStream?.Stop();
        component.PrintAudioStream = null;
    }

    private void UpdateUserInterface(EntityUid uid, PhotocopierComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        int tonerAvailable;
        int tonerCapacity;
        if (!TryGetTonerCartridge(uid, component, out var tonerCartridge))
        {
            tonerAvailable = 0;
            tonerCapacity = 0;
        }
        else
        {
            tonerAvailable = tonerCartridge.Charges;
            tonerCapacity = tonerCartridge.Capacity;
        }

        var isPaperInserted = component.PaperSlot.Item != null;

        TryComp<TransformComponent>(uid, out var xform);
        var assOnScanner = GetCreatureOnTop(uid, component, xform) != null;

        var state = new PhotocopierUiState(
            component.PaperSlot.Locked,
            isPaperInserted,
            component.CopiesQueued,
            component.FormCollections,
            tonerAvailable,
            tonerCapacity,
            assOnScanner,
            component.MaxQueueLength);

        _userInterface.TrySetUiState(uid, PhotocopierUiKey.Key, state);
    }
}
