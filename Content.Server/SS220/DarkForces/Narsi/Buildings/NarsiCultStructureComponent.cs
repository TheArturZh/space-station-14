using Content.SecretStationServer.DarkForces.Narsi.Progress.Objectives.Building;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.SecretStationServer.DarkForces.Narsi.Buildings;

[RegisterComponent]
public sealed partial class NarsiCultStructureComponent : Component
{
    [DataField(required: true)]
    public NarsiBuilding Building;
}
