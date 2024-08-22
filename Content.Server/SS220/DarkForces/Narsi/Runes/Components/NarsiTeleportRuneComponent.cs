using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.SecretStationServer.DarkForces.Narsi.Runes.Components;

[RegisterComponent]
public sealed partial class NarsiTeleportRuneComponent : Component
{
    [DataField("tag")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string Tag = "";
}
