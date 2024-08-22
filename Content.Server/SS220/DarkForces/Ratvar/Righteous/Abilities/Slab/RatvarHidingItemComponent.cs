﻿using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.SecretStationServer.DarkForces.Ratvar.Righteous.Abilities.Slab;

[RegisterComponent]
public sealed partial class RatvarHidingItemComponent : Component
{
    [DataField]
    public EntityUid? OriginalItem;
}
