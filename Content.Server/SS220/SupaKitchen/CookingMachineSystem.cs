// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Construction;
using Content.Server.Hands.Systems;
using Content.Server.Power.Components;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Destructible;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.SS220.SupaKitchen;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
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
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly CookingInstrumentSystem _cookingInstrument = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CookingMachineComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CookingMachineComponent, InteractUsingEvent>(OnInteractUsing, after: new[] { typeof(AnchorableSystem) });
        SubscribeLocalEvent<CookingMachineComponent, SolutionChangedEvent>(OnSolutionChange);
        SubscribeLocalEvent<CookingMachineComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<CookingMachineComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<CookingMachineComponent, RefreshPartsEvent>(OnRefreshParts);
        SubscribeLocalEvent<CookingMachineComponent, UpgradeExamineEvent>(OnUpgradeExamine);

        // UI event listeners
        SubscribeLocalEvent<CookingMachineComponent, CookingMachineStartCookMessage>((u, c, m) => StartCooking(u, c, m.Session.AttachedEntity));
        SubscribeLocalEvent<CookingMachineComponent, CookingMachineEjectMessage>(OnEjectMessage);
        SubscribeLocalEvent<CookingMachineComponent, CookingMachineEjectSolidIndexedMessage>(OnEjectIndex);
        SubscribeLocalEvent<CookingMachineComponent, CookingMachineSelectCookTimeMessage>(OnSelectTime);
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
            StopCooking(uid, component);
            _sharedContainer.EmptyContainer(component.Storage);
        }
        UpdateUserInterfaceState(uid, component);
    }

    private void OnInteractUsing(EntityUid uid, CookingMachineComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!(TryComp<ApcPowerReceiverComponent>(uid, out var apc) && apc.Powered))
        {
            _popupSystem.PopupEntity(
                Loc.GetString("cooking-machine-component-interact-using-no-power", ("machine", MetaData(uid).EntityName)),
                uid,
                args.User);
            return;
        }

        if (component.Broken)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("cooking-machine-component-interact-using-broken", ("machine", MetaData(uid).EntityName)),
                uid,
                args.User);
            return;
        }

        if (!HasComp<ItemComponent>(args.Used))
        {
            _popupSystem.PopupEntity(Loc.GetString("cooking-machine-component-interact-using-transfer-fail"),
                uid,
                args.User);
            return;
        }

        args.Handled = true;
        _handsSystem.TryDropIntoContainer(args.User, args.Used, component.Storage);
        UpdateUserInterfaceState(uid, component);
    }

    private void OnSolutionChange(EntityUid uid, CookingMachineComponent component, SolutionChangedEvent args)
    {
        UpdateUserInterfaceState(uid, component);
    }

    private void OnBreak(EntityUid uid, CookingMachineComponent component, BreakageEventArgs args)
    {
        component.Broken = true;
        SetAppearance(uid, CookingMachineVisualState.Broken, component);
        StopCooking(uid, component);
        _sharedContainer.EmptyContainer(component.Storage);
        UpdateUserInterfaceState(uid, component);
    }

    private void OnRefreshParts(EntityUid uid, CookingMachineComponent component, RefreshPartsEvent args)
    {
        var cookRating = args.PartRatings[component.MachinePartCookTimeMultiplier];
        component.CookTimeMultiplier = MathF.Pow(component.CookTimeScalingConstant, cookRating - 1);
    }

    private void OnUpgradeExamine(EntityUid uid, CookingMachineComponent component, UpgradeExamineEvent args)
    {
        args.AddPercentageUpgrade("cooking-machine-component-upgrade-cook-time", component.CookTimeMultiplier);
    }

    #region ui_messages
    private void OnEjectMessage(EntityUid uid, CookingMachineComponent component, CookingMachineEjectMessage args)
    {
        if (!HasContents(component) || component.Active)
            return;

        _sharedContainer.EmptyContainer(component.Storage);
        _audio.PlayPvs(component.ClickSound, uid, AudioParams.Default.WithVolume(-2));
        UpdateUserInterfaceState(uid, component);
    }

    private void OnEjectIndex(EntityUid uid, CookingMachineComponent component, CookingMachineEjectSolidIndexedMessage args)
    {
        if (!HasContents(component) || component.Active)
            return;

        component.Storage.Remove(args.EntityID);
        UpdateUserInterfaceState(uid, component);
    }

    private void OnSelectTime(EntityUid uid, CookingMachineComponent component, CookingMachineSelectCookTimeMessage args)
    {
        if (!HasContents(component) || component.Active || !(TryComp<ApcPowerReceiverComponent>(uid, out var apc) && apc.Powered))
            return;

        // some validation to prevent trollage
        if (args.NewCookTime % 5 != 0 || args.NewCookTime > component.MaxCookingTimer)
            return;

        component.CurrentCookTimeButtonIndex = args.ButtonIndex;
        component.CookingTimer = args.NewCookTime;
        _audio.PlayPvs(component.ClickSound, uid, AudioParams.Default.WithVolume(-2));
        UpdateUserInterfaceState(uid, component);
    }
    #endregion

    public static bool HasContents(CookingMachineComponent component)
    {
        return component.Storage.ContainedEntities.Any();
    }

    public void SetAppearance(EntityUid uid, CookingMachineVisualState state, CookingMachineComponent component)
    {
        var display = component.Broken ? CookingMachineVisualState.Broken : state;
        _appearance.SetData(uid, PowerDeviceVisuals.VisualState, display);
    }

    public void UpdateUserInterfaceState(EntityUid uid, CookingMachineComponent component)
    {
        var ui = _userInterface.GetUiOrNull(uid, CookingMachineUiKey.Key);
        if (ui == null)
            return;

        UserInterfaceSystem.SetUiState(ui, new CookingMachineUpdateUserInterfaceState(
            component.Storage.ContainedEntities.ToArray(),
            component.Active,
            component.CurrentCookTimeButtonIndex,
            component.CookingTimer
        ));
    }

    public void Break(EntityUid uid, CookingMachineComponent component)
    {
        component.Broken = true;
        SetAppearance(uid, CookingMachineVisualState.Broken, component);
        _audio.PlayPvs(component.ItemBreakSound, uid);
    }

    public void StartCooking(EntityUid uid, CookingMachineComponent component, EntityUid? whoStarted = null)
    {
        if (!TryComp<CookingInstrumentComponent>(uid, out var cookingInstrument))
            return;

        if (!HasContents(component) || component.Active)
            return;

        var solidsDict = new Dictionary<string, int>();
        var reagentDict = new Dictionary<string, FixedPoint2>();

        foreach (var item in component.Storage.ContainedEntities.ToList())
        {
            var ev = new ProcessedInCookingMachineEvent(uid, component, item, whoStarted);
            RaiseLocalEvent(uid, ev);

            if (ev.Handled)
            {
                UpdateUserInterfaceState(uid, component);
                return;
            }

            var metaData = MetaData(item); //this still begs for cooking refactor
            if (metaData.EntityPrototype == null)
                continue;

            if (solidsDict.ContainsKey(metaData.EntityPrototype.ID))
                solidsDict[metaData.EntityPrototype.ID]++;
            else
                solidsDict.Add(metaData.EntityPrototype.ID, 1);

            if (!TryComp<SolutionContainerManagerComponent>(item, out var solMan))
                continue;

            foreach (var (_, solution) in solMan.Solutions)
            {
                foreach (var reagent in solution.Contents)
                {
                    if (reagentDict.ContainsKey(reagent.ReagentId))
                        reagentDict[reagent.ReagentId] += reagent.Quantity;
                    else
                        reagentDict.Add(reagent.ReagentId, reagent.Quantity);
                }
            }
        }

        // Check recipes
        var portionedRecipe = _cookingInstrument.GetSatisfiedPortionedRecipe(
            cookingInstrument, solidsDict, reagentDict, component.CookingTimer);

        _audio.PlayPvs(component.StartCookingSound, uid);
        component.CookTimeRemaining = component.CookingTimer * component.CookTimeMultiplier;
        component.CurrentlyCookingRecipe = portionedRecipe;
        UpdateUserInterfaceState(uid, component);

        SetAppearance(uid, CookingMachineVisualState.Cooking, component);

        component.PlayingStream =
            _audio.PlayPvs(component.LoopingSound, uid, AudioParams.Default.WithLoop(true).WithMaxDistance(5));

        component.Active = true;
    }

    public void StopCooking(EntityUid uid, CookingMachineComponent component)
    {
        component.Active = false;
        component.CookTimeRemaining = 0;
        component.CurrentlyCookingRecipe = (null, 0);
        UpdateUserInterfaceState(uid, component);
        SetAppearance(uid, CookingMachineVisualState.Idle, component);
        component.PlayingStream?.Stop();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CookingMachineComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.Active)
                continue;

            //check if there's still cook time left
            component.CookTimeRemaining -= frameTime;
            if (component.CookTimeRemaining > 0)
                continue;

            //this means the cooking machine has finished cooking.
            var ev = new BeforeCookingMachineFinished(component);
            RaiseLocalEvent(uid, ev);

            if (component.CurrentlyCookingRecipe.Item1 != null)
            {
                var coords = Transform(uid).Coordinates;
                for (var i = 0; i < component.CurrentlyCookingRecipe.Item2; i++)
                {
                    _cookingInstrument.SubtractContents(component.Storage, component.CurrentlyCookingRecipe.Item1);
                    Spawn(component.CurrentlyCookingRecipe.Item1.Result, coords);
                }
            }

            _sharedContainer.EmptyContainer(component.Storage);
            StopCooking(uid, component);
            _audio.PlayPvs(component.FoodDoneSound, uid, AudioParams.Default.WithVolume(-1));
        }
    }
}
