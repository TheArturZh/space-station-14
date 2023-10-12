using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.SS220.DarkReaper;

public abstract class SharedDarkReaperSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DarkReaperComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<DarkReaperComponent, ComponentShutdown>(OnCompShutdown);

        SubscribeLocalEvent<DarkReaperComponent, MeleeHitEvent>(OnMeleeHit);

        // actions
        SubscribeLocalEvent<DarkReaperComponent, ReaperRoflEvent>(OnRoflAction);
        SubscribeLocalEvent<DarkReaperComponent, ReaperConsumeEvent>(OnConsumeAction);
        SubscribeLocalEvent<DarkReaperComponent, ReaperMaterializeEvent>(OnMaterializeAction);
        SubscribeLocalEvent<DarkReaperComponent, ReaperStunEvent>(OnStunAction);
        SubscribeLocalEvent<DarkReaperComponent, AfterMaterialize>(OnAfterMaterialize);
    }

    // Action bindings
    private void OnRoflAction(EntityUid uid, DarkReaperComponent comp, ReaperRoflEvent args)
    {
        args.Handled = true;

        DoRoflAbility(uid, comp);
    }

    private void OnConsumeAction(EntityUid uid, DarkReaperComponent comp, ReaperConsumeEvent args)
    {
        args.Handled = true;
    }

    private void OnMaterializeAction(EntityUid uid, DarkReaperComponent comp, ReaperMaterializeEvent args)
    {
        DoMaterialize(uid, comp);
    }

    private void OnStunAction(EntityUid uid, DarkReaperComponent comp, ReaperStunEvent args)
    {
        args.Handled = true;
        DoStunAbility(uid, comp);
    }

    // Actions
    protected virtual void DoStunAbility(EntityUid uid, DarkReaperComponent comp)
    {
        _audio.PlayPredicted(comp.StunAbilitySound, uid, uid);

        var entities = _lookup.GetEntitiesInRange(uid, comp.StunAbilityRadius);
        foreach (var entity in entities)
        {
            _stun.TryParalyze(entity, comp.StunDuration, true);
        }
    }

    protected virtual void DoRoflAbility(EntityUid uid, DarkReaperComponent comp)
    {
        _audio.PlayPredicted(comp.RolfAbilitySound, uid, uid);
    }

    protected void DoMaterialize(EntityUid uid, DarkReaperComponent comp)
    {
        if (!comp.PhysicalForm)
        {
            EntityUid? portalEntity = null;
            if (_net.IsServer)
            {
                if (_prototype.HasIndex<EntityPrototype>(comp.PortalEffectPrototype))
                {
                    portalEntity = Spawn(comp.PortalEffectPrototype, MapCoordinates.Nullspace);
                }
            }

            NetEntity? portalNetEntity = portalEntity.HasValue ? GetNetEntity(portalEntity) : null;

            var doafterArgs = new DoAfterArgs(
                EntityManager,
                uid,
                TimeSpan.FromSeconds(3),
                new AfterMaterialize(portalNetEntity),
                uid
            )
            {
                BreakOnDamage = false,
                BreakOnTargetMove = false,
                BreakOnUserMove = true,
                NeedHand = false,
                BlockDuplicate = true
            };

            var started = _doAfter.TryStartDoAfter(doafterArgs);

            if (portalEntity.HasValue)
            {
                if (started)
                    _transform.SetCoordinates(portalEntity.Value, Transform(uid).Coordinates);
                else
                    QueueDel(portalEntity);
            }
        }
        else
        {
            ChangeForm(uid, comp, false);
        }
    }

    private void OnAfterMaterialize(EntityUid uid, DarkReaperComponent comp, AfterMaterialize args)
    {
        args.Handled = true;

        if (args.PortalEntity.HasValue)
            QueueDel(GetEntity(args.PortalEntity.Value));

        if (!args.Cancelled)
            ChangeForm(uid, comp, true);
    }

    // Update loop
    public override void Update(float delta)
    {
        base.Update(delta);
    }

    // Crap
    protected virtual void OnCompInit(EntityUid uid, DarkReaperComponent comp, ComponentInit args)
    {
        UpdateStageAppearance(uid, comp);
        UpdateDamage(uid, comp);
        ChangeForm(uid, comp, comp.PhysicalForm);

        // Make tests crash & burn if stupid things are done
        DebugTools.Assert(comp.MaxStage >= 1, "DarkReaperComponent.MaxStage must always be equal or greater than 1.");
    }

    protected virtual void OnCompShutdown(EntityUid uid, DarkReaperComponent comp, ComponentShutdown args)
    {
    }

    public virtual void ChangeForm(EntityUid uid, DarkReaperComponent comp, bool isMaterial)
    {
        comp.PhysicalForm = isMaterial;
        Dirty(uid, comp);

        if (TryComp<FixturesComponent>(uid, out var fixturesComp))
        {
            if (fixturesComp.Fixtures.TryGetValue("fix1", out var fixture))
            {
                var mask = (int) (isMaterial ? CollisionGroup.MobMask : CollisionGroup.GhostImpassable);
                var layer = (int) (isMaterial ? CollisionGroup.MobLayer : CollisionGroup.None);
                _physics.SetCollisionMask(uid, "fix1", fixture, mask);
                _physics.SetCollisionLayer(uid, "fix1", fixture, layer);
            }
        }

        _eye.SetDrawFov(uid, isMaterial);
        _appearance.SetData(uid, DarkReaperVisual.PhysicalForm, isMaterial);

        if (isMaterial)
        {
            _tag.AddTag(uid, "DoorBumpOpener");
        }
        else
        {
            _tag.RemoveTag(uid, "DoorBumpOpener");
        }

        UpdateDamage(uid, comp);
        UpdateMovementSpeed(uid, comp);
    }

    public void ChangeStage(EntityUid uid, DarkReaperComponent comp, int stage)
    {
        comp.CurrentStage = stage;
        UpdateStageAppearance(uid, comp);
        UpdateDamage(uid, comp);
    }

    private void UpdateStageAppearance(EntityUid uid, DarkReaperComponent comp)
    {
        _appearance.SetData(uid, DarkReaperVisual.Stage, comp.CurrentStage);
    }

    private void UpdateDamage(EntityUid uid, DarkReaperComponent comp)
    {
        if (!TryComp<MeleeWeaponComponent>(uid, out var weapon))
            return;

        if (!comp.PhysicalForm || !comp.StageMeleeDamage.TryGetValue(comp.CurrentStage, out var damageSet))
        {
            damageSet = new();
        }

        weapon.Damage = new()
        {
            DamageDict = damageSet
        };
    }

    private void UpdateMovementSpeed(EntityUid uid, DarkReaperComponent comp)
    {
        if (!TryComp<MovementSpeedModifierComponent>(uid, out var modifComp))
            return;

        var speed = comp.PhysicalForm ? comp.MaterialMovementSpeed : comp.UnMaterialMovementSpeed;
        _speedModifier.ChangeBaseSpeed(uid, speed, speed, modifComp.Acceleration, modifComp);
    }

    private void OnMeleeHit(EntityUid uid, DarkReaperComponent comp, MeleeHitEvent args)
    {
        if (!comp.PhysicalForm)
            args.IsHit = false;
    }
}

[Serializable, NetSerializable]
public sealed partial class AfterMaterialize : DoAfterEvent
{
    public NetEntity? PortalEntity;
    public override DoAfterEvent Clone() => this;

    public AfterMaterialize(NetEntity? portalEntity)
    {
        PortalEntity = portalEntity;
    }
}
