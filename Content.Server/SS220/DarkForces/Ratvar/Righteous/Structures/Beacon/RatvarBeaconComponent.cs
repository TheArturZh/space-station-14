using System;
using System.Collections.Generic;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.SecretStationServer.DarkForces.Ratvar.Righteous.Structures.Beacon;

[RegisterComponent]
public sealed partial class RatvarBeaconComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public DamageSpecifier HealingDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            {"Blunt", -7},
            {"Slash", -7},
            {"Piercing", -7},
            {"Heat", -7},
            {"Cold", -7},
            {"Shock", -7},
            {"Burn", -7}
        }
    };

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan LastHealTick;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan LastPowerTick;

    [DataField]
    public int PowerPerTick = 2;
}
