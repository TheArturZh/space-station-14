using Robust.Shared.GameStates;

namespace Content.Shared.SS220.ButtScan;

[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class ButtScanComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("buttTexturePath")]
    public string ButtTexturePath = "/Textures/SS220/Interface/Butts/human.png";
}
