// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Robust.Shared.Utility;

namespace Content.Shared.SS220.BloomLight;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class BloomLightMaskComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public List<SpriteSpecifier> LightMasks = new() { new SpriteSpecifier.Texture(new("SS220/BloomLight/Masks/lightmask_lamp_soft.png")) };

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool Enabled = true;

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool UseLightColor = false;

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool UseShader = true;
}
