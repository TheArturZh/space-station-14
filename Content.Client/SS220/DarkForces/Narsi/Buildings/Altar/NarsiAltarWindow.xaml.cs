using Content.Client.UserInterface.Controls;
using Content.Shared.SecretStation.DarkForces.Narsi.Buildings.Altar;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.SecretStationClient.DarkForces.Narsi.Buildings.Altar;

[GenerateTypedNameReferences]
public sealed partial class NarsiAltarWindow : FancyWindow
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private readonly NarsiAltarBoundInterface _bui;

    public NarsiAltarWindow(NarsiAltarBoundInterface bui)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _bui = bui;

        OpenAbilities.OnPressed += _ => _bui.OpenAbilities();
        OpenRituals.OnPressed += _ => _bui.OpenRituals();
    }

    public void UpdateState(NarsiAltarUIState state)
    {
        BloodScore.Text = state.BloodScore.ToString();
    }
}
