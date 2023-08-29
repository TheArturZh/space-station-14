// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SupaKitchen;
using Robust.Shared.Containers;

namespace Content.Server.SS220.SupaKitchen;
public sealed partial class CookingMachineComponent : Component
{
    [ViewVariables]
    public uint CookingTimer = 0;

    [DataField("maxCookingTimer"), ViewVariables(VVAccess.ReadWrite)]
    public uint MaxCookingTimer = 30;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CookTimeRemaining;

    [ViewVariables]
    public (CookingRecipePrototype?, int) CurrentlyCookingRecipe;

    [DataField("broken"), ViewVariables]
    public bool Broken = false;

    [DataField("active"), ViewVariables]
    public bool Active = false;

    public Container Storage = default!;
}
