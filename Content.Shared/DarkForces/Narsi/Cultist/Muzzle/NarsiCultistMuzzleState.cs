using Robust.Shared.Serialization;

namespace Content.Shared.SecretStation.DarkForces.Narsi.Cultist.Muzzle;

[Serializable, NetSerializable]
public enum NarsiCultistMuzzleStatus : byte
{
    Status
}

[Serializable, NetSerializable]
public enum NarsiCultistMuzzleState : byte
{
    Muzzle,
    Empty
}
