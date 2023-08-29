using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SupaKitchen;

[NetSerializable, Serializable]
public sealed class CookingMachineUpdateUserInterfaceState : BoundUserInterfaceState
{
    public EntityUid[] ContainedSolids;
    public bool IsMachineBusy;
    public uint CurrentCookTime;

    public CookingMachineUpdateUserInterfaceState(EntityUid[] containedSolids,
        bool isMachineBusy, uint currentCookTime)
    {
        ContainedSolids = containedSolids;
        IsMachineBusy = isMachineBusy;
        CurrentCookTime = currentCookTime;
    }

}

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
