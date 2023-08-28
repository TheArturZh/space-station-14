using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SupaKitchen;

[Serializable, NetSerializable]
public enum CookingMachineVisualState
{
    Idle,
    Cooking,
    Broken
}

[Serializable, NetSerializable]
public enum CookingMachineUiKey
{
    Key
}
