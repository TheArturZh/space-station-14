using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.DarkReaper;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedDarkReaperSystem), Friend = AccessPermissions.ReadWrite, Other = AccessPermissions.Read)]
public sealed partial class DarkReaperComponent : Component
{
    [ViewVariables, DataField, AutoNetworkedField]
    public EntProtoId PortalEffectPrototype = "DarkReaperPortalEffect";

    /// <summary>
    /// Wheter the Dark Reaper is currently in physical form or not.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool PhysicalForm = false;

    /// <summary>
    /// Max progression stage
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public int MaxStage = 3;

    /// <summary>
    /// Progression stage
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public int CurrentStage = 1;

    /// ABILITY STATS ///

    /// STUN

    /// <summary>
    /// For how long reaper emites a bright red glow
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public TimeSpan StunGlareLength = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Stun ability radius, in tiles
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public float StunAbilityRadius = 3;

    /// <summary>
    /// Radius in which stun ability breaks lights
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public float StunAbilityLightBreakRadius = 4.5f;

    /// <summary>
    /// Duration of the stun that is applied by the ability
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Stun ability sound
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public SoundSpecifier StunAbilitySound = new SoundPathSpecifier("/Audio/SS220/DarkReaper/jnec_scrm.ogg");

    /// ROFL

    /// <summary>
    /// Rofl sound
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public SoundSpecifier RolfAbilitySound = new SoundPathSpecifier("/Audio/SS220/DarkReaper/jnec_rolf.ogg", new()
    {
        MaxDistance = 7
    });

    /// MATERIALIZE

    /// <summary>
    /// How long reaper can stay materialized, depending on stage
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public List<TimeSpan> MaterializeDurations = new()
    {
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(40)
    };

    [ViewVariables, DataField, AutoNetworkedField]
    public float MaterialMovementSpeed = 4f;

    [ViewVariables, DataField, AutoNetworkedField]
    public float UnMaterialMovementSpeed = 7f;

    /// STAGE PROGRESSION

    /// <summary>
    /// DamageSpecifier for melee damage that Dark Reaper does at every stage.
    /// </summary>
    [ViewVariables, DataField, AutoNetworkedField]
    public List<Dictionary<string, FixedPoint2>> StageMeleeDamage = new()
    {
        // Stage 1
        new()
        {
            { "Slash", 10 },
            { "Piercing", 2 }
        },

        // Stage 2
        new()
        {
            { "Slash", 12 },
            { "Piercing", 4 }
        },

        // Stage 3
        new()
        {
            { "Slash", 14 },
            { "Piercing", 8 }
        }
    };

    /// ABILITIES ///
    [DataField]
    public EntProtoId RoflAction = "ActionDarkReaperRofl";
    [DataField]
    public EntProtoId StunAction = "ActionDarkReaperStun";
    [DataField]
    public EntProtoId ConsumeAction = "ActionDarkReaperConsume";
    [DataField]
    public EntProtoId MaterializeAction = "ActionDarkReaperMaterialize";

    [DataField, AutoNetworkedField]
    public EntityUid? RoflActionEntity;
    [DataField, AutoNetworkedField]
    public EntityUid? StunActionEntity;
    [DataField, AutoNetworkedField]
    public EntityUid? ConsumeActionEntity;
    [DataField, AutoNetworkedField]
    public EntityUid? MaterializeActionEntity;

    // ABILITY STATES ///
    [ViewVariables, AutoNetworkedField]
    public TimeSpan? StunScreamStart;
}

[Serializable, NetSerializable]
public enum DarkReaperVisual
{
    Stage,
    PhysicalForm,
    StunEffect,
    GhostCooldown,
}
