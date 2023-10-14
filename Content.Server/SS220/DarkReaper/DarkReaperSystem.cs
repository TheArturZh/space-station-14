using Content.Server.Actions;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.SS220.DarkReaper;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Server.SS220.DarkReaper;

public sealed class DarkReaperSystem : SharedDarkReaperSystem
{
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private const int MaxBooEntities = 30;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void ChangeForm(EntityUid uid, DarkReaperComponent comp, bool isMaterial)
    {
        var isTransitioning = comp.PhysicalForm != isMaterial;
        base.ChangeForm(uid, comp, isMaterial);

        if (isTransitioning && !isMaterial)
        {
            if (comp.ActivePortal != null)
            {
                QueueDel(comp.ActivePortal);
                comp.ActivePortal = null;
            }
        }
    }

    protected override void OnCompInit(EntityUid uid, DarkReaperComponent comp, ComponentInit args)
    {
        base.OnCompInit(uid, comp, args);

        _actions.AddAction(uid, ref comp.RoflActionEntity, comp.RoflAction);
        _actions.AddAction(uid, ref comp.StunActionEntity, comp.StunAction);
        _actions.AddAction(uid, ref comp.ConsumeActionEntity, comp.ConsumeAction);
        _actions.AddAction(uid, ref comp.MaterializeActionEntity, comp.MaterializeAction);
    }

    protected override void OnCompShutdown(EntityUid uid, DarkReaperComponent comp, ComponentShutdown args)
    {
        base.OnCompShutdown(uid, comp, args);

        _actions.RemoveAction(uid, comp.RoflActionEntity);
        _actions.RemoveAction(uid, comp.StunActionEntity);
        _actions.RemoveAction(uid, comp.ConsumeActionEntity);
        _actions.RemoveAction(uid, comp.MaterializeActionEntity);
    }

    protected override void DoStunAbility(EntityUid uid, DarkReaperComponent comp)
    {
        base.DoStunAbility(uid, comp);

        // Destroy lights in radius
        var lightQuery = GetEntityQuery<PoweredLightComponent>();
        var entities = _lookup.GetEntitiesInRange(uid, comp.StunAbilityLightBreakRadius);

        foreach (var entity in entities)
        {
            if (!lightQuery.TryGetComponent(entity, out var lightComp))
                continue;

            _poweredLight.TryDestroyBulb(entity, lightComp);
        }
    }

    protected override void DoRoflAbility(EntityUid uid, DarkReaperComponent comp)
    {
        base.DoRoflAbility(uid, comp);

        // Make lights blink
        var entities = _lookup.GetEntitiesInRange(uid, 6);

        var booCounter = 0;
        foreach (var ent in entities)
        {
            var handled = _ghost.DoGhostBooEvent(ent);

            if (handled)
                booCounter++;

            if (booCounter >= MaxBooEntities)
                break;
        }
    }
}
