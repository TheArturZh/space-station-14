using Content.Server.Hands.Systems;
using Content.Server.Power.Components;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.SS220.SupaKitchen;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using System.Linq;

namespace Content.Server.SS220.SupaKitchen;
public sealed class CookingMachineSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedContainerSystem _sharedContainer = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CookingMachineComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CookingMachineComponent, InteractUsingEvent>(OnInteractUsing, after: new[] { typeof(AnchorableSystem) });
        SubscribeLocalEvent<CookingMachineComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<CookingMachineComponent, BreakageEventArgs>(OnBreak);
    }

    private void OnInit(EntityUid uid, CookingMachineComponent component, ComponentInit ags)
    {
        component.Storage = _container.EnsureContainer<Container>(uid, "cooking_machine_entity_container");
    }

    private void OnPowerChanged(EntityUid uid, CookingMachineComponent component, ref PowerChangedEvent args)
    {
        if (!args.Powered)
        {
            SetAppearance(uid, CookingMachineVisualState.Idle, component);
            _sharedContainer.EmptyContainer(component.Storage);
        }
        //UpdateUserInterfaceState(uid, component);
    }

    private void OnInteractUsing(EntityUid uid, CookingMachineComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;
        if (!(TryComp<ApcPowerReceiverComponent>(uid, out var apc) && apc.Powered))
        {
            _popupSystem.PopupEntity(Loc.GetString("microwave-component-interact-using-no-power"), uid, args.User);
            return;
        }

        if (component.Broken)
        {
            _popupSystem.PopupEntity(Loc.GetString("microwave-component-interact-using-broken"), uid, args.User);
            return;
        }

        if (!HasComp<ItemComponent>(args.Used))
        {
            _popupSystem.PopupEntity(Loc.GetString("microwave-component-interact-using-transfer-fail"), uid, args.User);
            return;
        }

        args.Handled = true;
        _handsSystem.TryDropIntoContainer(args.User, args.Used, component.Storage);
        //UpdateUserInterfaceState(uid, component);
    }

    private void OnBreak(EntityUid uid, CookingMachineComponent component, BreakageEventArgs args)
    {
        component.Broken = true;
        SetAppearance(uid, CookingMachineVisualState.Broken, component);
        _sharedContainer.EmptyContainer(component.Storage);
        //UpdateUserInterfaceState(uid, component);
    }

    public static bool HasContents(CookingMachineComponent component)
    {
        return component.Storage.ContainedEntities.Any();
    }

    public void SetAppearance(EntityUid uid, CookingMachineVisualState state, CookingMachineComponent component)
    {
        var display = component.Broken ? CookingMachineVisualState.Broken : state;
        _appearance.SetData(uid, PowerDeviceVisuals.VisualState, display);
    }
}
