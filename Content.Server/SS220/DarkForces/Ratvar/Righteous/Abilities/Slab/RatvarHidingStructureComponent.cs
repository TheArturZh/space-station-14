﻿using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.SecretStationServer.DarkForces.Ratvar.Righteous.Abilities.Slab;

[RegisterComponent]
public sealed partial class RatvarHidingStructureComponent : Component
{
    [DataField]
    public EntityUid? OriginalStructure;

    [DataField]
    public EntityUid? HidingSlab;
}
