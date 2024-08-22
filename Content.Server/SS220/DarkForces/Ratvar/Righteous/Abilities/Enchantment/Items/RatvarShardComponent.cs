using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.SecretStationServer.DarkForces.Ratvar.Righteous.Abilities.Enchantment.Items;

[RegisterComponent]
public sealed partial class RatvarShardComponent : Component
{
    [DataField]
    public string TileId = "FloorBrass";

    [DataField]
    public float ConvertRange = 3f;
}
