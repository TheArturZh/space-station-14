using Robust.Shared.GameStates;

namespace Content.Shared.SS220.DarkReaper;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DarkReaperComponent : Component
{
    /// <summary>
    /// Wheter the Dark Reaper is currently in physical form or not.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool PhysicalForm = false;
}
