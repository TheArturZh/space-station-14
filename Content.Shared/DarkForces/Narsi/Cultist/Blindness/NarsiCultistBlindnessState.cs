﻿using Robust.Shared.Serialization;

namespace Content.Shared.SecretStation.DarkForces.Narsi.Cultist.Blindness;

[Serializable, NetSerializable]
public enum NarsiCultistBlindnessStatus : byte
{
    Status
}

[Serializable, NetSerializable]
public enum NarsiCultistBlindnessState : byte
{
    Blindness,
    Empty
}
