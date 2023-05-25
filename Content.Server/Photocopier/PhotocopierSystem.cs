using Content.Shared.Photocopier;
using Content.Shared.Containers.ItemSlots;
using Content.Server.UserInterface;
using Content.Server.Power.Components;
using Content.Server.Paper;
using Content.Server.Power.EntitySystems;
using Robust.Shared.Containers;
using Robust.Server.GameObjects;

/*
TODO: Implement visuals - printing animation, powered off state
*/

namespace Content.Server.Photocopier;

public sealed class PhotocopierSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotocopierComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<PhotocopierComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<PhotocopierComponent, EntInsertedIntoContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<PhotocopierComponent, EntRemovedFromContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<PhotocopierComponent, PowerChangedEvent>(OnPowerChanged);

        // UI
        SubscribeLocalEvent<PhotocopierComponent, AfterActivatableUIOpenEvent>(OnToggleInterface);
        SubscribeLocalEvent<PhotocopierComponent, PhotocopierCopyMessage>(OnCopyButtonPressed);
        SubscribeLocalEvent<PhotocopierComponent, PhotocopierStopMessage>(OnStopButtonPressed);
    }

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

        component.DataToCopy = new DataToCopy(paper.Content, metadata.EntityName, metadata.EntityPrototype?.ID, paper.StampState, paper.StampedBy);
        component.CopiesQueued = Math.Min(args.Amount, component.MaxQueueLength);
    }

    private void OnStopButtonPressed(EntityUid uid, PhotocopierComponent component, PhotocopierStopMessage args)
    {
        StopPrinting(uid, component);
        TryUpdateVisualState(uid, component);
    }

    private void StopPrinting(EntityUid uid, PhotocopierComponent component)
    {
        var uiUpdateNeeded = component.CopiesQueued > 0;

        component.CopiesQueued = 0;
        component.PrintingTimeRemaining = 0;
        component.DataToCopy = null;
        _itemSlotsSystem.SetLock(uid, component.PaperSlot, false);

        if (uiUpdateNeeded)
            UpdateUserInterface(uid, component);
    }

    private void SpawnPaperCopy(EntityUid uid, PhotocopierComponent? component = null)
    {
        if (!Resolve(uid, ref component) || component.CopiesQueued == 0)
            return;

        component.CopiesQueued--;

        var printout = component.DataToCopy;
        if (printout != null)
        {
            var entityToSpawn = printout.PrototypeId.Length == 0 ? "Paper" : printout.PrototypeId;
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
                metadata.EntityName = printout.Name;
        }
    }

    private void ProcessPrinting(EntityUid uid, float frameTime, PhotocopierComponent component)
    {
        if (component.PrintingTimeRemaining > 0)
        {
            component.PrintingTimeRemaining -= frameTime;

            bool isPrinted = component.PrintingTimeRemaining <= 0;
            if (isPrinted)
            {
                SpawnPaperCopy(uid, component);

                if (component.CopiesQueued <= 0)
                {
                    _itemSlotsSystem.SetLock(uid, component.PaperSlot, false);
                }

                UpdateUserInterface(uid, component);
                TryUpdateVisualState(uid, component);
            }

            return;
        }

        if (component.CopiesQueued > 0)
        {
            component.PrintingTimeRemaining = component.PrintingTime;
            _itemSlotsSystem.SetLock(uid, component.PaperSlot, true);
            _audioSystem.PlayPvs(component.PrintSound, uid);
            UpdateUserInterface(uid, component);
            TryUpdateVisualState(uid, component);
        }
    }

    public void TryUpdateVisualState(EntityUid uid, PhotocopierComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var state = PhotocopierVisualState.Powered;

        if (component.CopiesQueued > 0)
            state = PhotocopierVisualState.Printing;
        else if (!this.IsPowered(uid, EntityManager))
            state = PhotocopierVisualState.Off;

        var item = component.PaperSlot.Item;
        var gotItem = item != null;
        var combinedState = new PhotocopierCombinedVisualState(state, gotItem);

        _appearanceSystem.SetData(uid, PhotocopierVisuals.VisualState, combinedState);
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
            component.CopiesQueued);

        _userInterface.TrySetUiState(uid, PhotocopierUiKey.Key, state);
    }

    private void UpdateVisuals(EntityUid uid, PhotocopierComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
    }
}
