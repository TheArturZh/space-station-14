using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.SecretStationServer.DarkForces.Narsi.Buildings.Altar.Rituals.Polymorth;

[RegisterComponent]
public sealed partial class NarsiPolymorphComponent : Component
{
    [DataField]
    public EntityUid AltarEntityUid;

    [DataField]
    public bool ReturnToAltar;
}
