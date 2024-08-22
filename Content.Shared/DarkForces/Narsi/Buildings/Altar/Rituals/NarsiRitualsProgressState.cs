﻿using Robust.Shared.Serialization;

namespace Content.Shared.SecretStation.DarkForces.Narsi.Buildings.Altar.Rituals;

[Serializable, NetSerializable]
public enum NarsiRitualsProgressState
{
    Idle,
    Working,
    Delay
}
