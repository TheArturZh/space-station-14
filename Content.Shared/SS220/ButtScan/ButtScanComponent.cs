// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.SS220.Photocopier;
using Robust.Shared.GameStates;

namespace Content.Shared.SS220.ButtScan;

[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class ButtScanComponent : Component, IPhotocopyableComponent<ButtScanPhotocopiedData, ButtScanComponent>
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("buttTexturePath")]
    public string ButtTexturePath = "/Textures/SS220/Interface/Butts/human.png";

    public ButtScanPhotocopiedData GetPhotocopiedData()
    {
        return new ButtScanPhotocopiedData()
        {
            ButtTexturePath = ButtTexturePath
        };
    }
}

public sealed class ButtScanPhotocopiedData : PhotocopiedComponentData<ButtScanComponent>
{
    public string? ButtTexturePath;

    public override void RestoreComponentFields(ButtScanComponent component)
    {
        if (ButtTexturePath is not null)
            component.ButtTexturePath = ButtTexturePath;
    }
}
