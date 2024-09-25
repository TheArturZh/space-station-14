// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.SS220.CruiseControl;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShuttleCruiseControlComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables]
    public Vector2 LinearInput;
}
