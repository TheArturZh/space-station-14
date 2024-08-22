using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.SecretStationServer.DarkForces.Ratvar.Righteous.Progress.Objectives.Beacon;

[RegisterComponent]
public sealed partial class RatvarBeaconObjectiveComponent : Component
{
    [DataField]
    public float RequiredCount = 5f;
}
