﻿using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.SecretStationServer.DarkForces.Ratvar.Righteous.Progress.Objectives.Convert;

[RegisterComponent]
public sealed partial class RatvarConvertObjectiveComponent : Component
{
    [DataField]
    public float RequiredCount = 5f;
}
