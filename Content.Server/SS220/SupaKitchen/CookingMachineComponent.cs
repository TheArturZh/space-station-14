// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Robust.Shared.Containers;

namespace Content.Server.SS220.SupaKitchen;
public sealed partial class CookingMachineComponent : Component
{
    [ViewVariables]
    public uint CookingTimer = 0;

    [DataField("maxCookingTimer"), ViewVariables(VVAccess.ReadWrite)]
    public uint MaxCookingTimer = 30;

    [DataField("broken"), ViewVariables(VVAccess.ReadOnly)]
    public bool Broken = false;

    public Container Storage = default!;
}
