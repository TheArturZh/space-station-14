using Robust.Shared.GameStates;

namespace Content.Shared.SS220.ButtScan;

[NetworkedComponent, RegisterComponent]
public sealed class ButtScanComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("buttTexturePath")]
    public string ButtTexturePath = "/Textures/SS220/Interface/Butts/human.png";
}
