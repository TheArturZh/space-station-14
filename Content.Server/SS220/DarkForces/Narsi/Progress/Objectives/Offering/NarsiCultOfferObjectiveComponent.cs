using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.SecretStationServer.DarkForces.Narsi.Progress.Objectives.Offering;

[RegisterComponent]
public sealed partial class NarsiCultOfferObjectiveComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Target;
}
