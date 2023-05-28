using Content.Shared.SS220.Photocopier;
using Content.Shared.SS220.Photocopier.Forms;
using Content.Shared.Containers.ItemSlots;
using Content.Server.UserInterface;
using Content.Server.Power.Components;
using Content.Server.Paper;
using Content.Server.Power.EntitySystems;
using Content.Server.SS220.Photocopier.Forms;
using Robust.Shared.Containers;
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

    private void OnComponentInit(EntityUid uid, PhotocopierComponent component, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, PhotocopierComponent.PaperSlotId, component.PaperSlot);
        TryUpdateVisualState(uid, component);
    }

    private void OnComponentRemove(EntityUid uid, PhotocopierComponent component, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, component.PaperSlot);
    }

    private void OnToggleInterface(EntityUid uid, PhotocopierComponent component, AfterActivatableUIOpenEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    private void OnItemSlotChanged(EntityUid uid, PhotocopierComponent component, ContainerModifiedMessage args)
    {
        UpdateUserInterface(uid, component);
        TryUpdateVisualState(uid, component);

        if (component.PaperSlot.Item != null && this.IsPowered(uid, EntityManager))
            _audioSystem.PlayPvs(component.PaperInsertSound, uid);
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

        if (!TryComp<MetaDataComponent>(copyEntity, out var metadata) ||
            !TryComp<PaperComponent>(copyEntity, out var paper))
            return;

        component.DataToCopy = new Form(
            metadata.EntityName,
            paper.Content,
            prototypeId: metadata.EntityPrototype?.ID,
            stampState: paper.StampState,
            stampedBy: paper.StampedBy);

        component.CopiesQueued = Math.Min(args.Amount, component.MaxQueueLength);
        component.IsScanning = true;
    }

    private void OnPrintButtonPressed(EntityUid uid, PhotocopierComponent component, PhotocopierPrintMessage args)
    {
        if (!component.Initialized)
            return;

        if(_specificFormManager == null)
            return;

        if (component.CopiesQueued > 0)
            return;

        component.DataToCopy = _specificFormManager.TryGetFormFromDescriptor(args.Descriptor);
        if (component.DataToCopy == null)
            return;

        component.CopiesQueued = Math.Min(args.Amount, component.MaxQueueLength);
    }

    private void OnStopButtonPressed(EntityUid uid, PhotocopierComponent component, PhotocopierStopMessage args)
    {
        StopPrinting(uid, component);
        TryUpdateVisualState(uid, component);
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
        UpdateUserInterface(uid, component);
    }

    /// <summary>
    /// Spawns paper copy from a queue.
    /// </summary>
    private void SpawnPaperCopy(EntityUid uid, PhotocopierComponent? component = null)
    {
        if (!Resolve(uid, ref component) || component.CopiesQueued == 0)
            return;

        component.CopiesQueued--;

        var printout = component.DataToCopy;
        if (printout != null)
        {
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

            if (TryComp<MetaDataComponent>(printed, out var metadata))
            {
                if (!string.IsNullOrEmpty(printout.EntityName))
                    metadata.EntityName = printout.EntityName;
            }
        }
    }

    private void ProcessPrinting(EntityUid uid, float frameTime, PhotocopierComponent component)
    {
        if (component.PrintingTimeRemaining > 0)
        {
            component.PrintingTimeRemaining -= frameTime;

            var isPrinted = component.PrintingTimeRemaining <= 0;
            if (isPrinted)
            {
                SpawnPaperCopy(uid, component);

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

        if (component.CopiesQueued > 0)
            state = component.IsScanning? PhotocopierVisualState.Copying : PhotocopierVisualState.Printing;
        else if (!this.IsPowered(uid, EntityManager))
            state = PhotocopierVisualState.Off;

        var item = component.PaperSlot.Item;
        var gotItem = item != null;
        var combinedState = new PhotocopierCombinedVisualState(state, gotItem);

        _appearanceSystem.SetData(uid, PhotocopierVisuals.VisualState, combinedState);
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

        var isPaperInserted = component.PaperSlot.Item != null;
        var state = new PhotocopierUiState(
            component.PaperSlot.Locked,
            isPaperInserted,
            1.0f,
            component.CopiesQueued,
            component.FormCollections);

        _userInterface.TrySetUiState(uid, PhotocopierUiKey.Key, state);
    }
}
