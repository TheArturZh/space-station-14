// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Robust.Client.AutoGenerated;
using Robust.Client.Console;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Player;

namespace Content.Client.SS220.Administration.UI.Tabs.AdminTab;

[GenerateTypedNameReferences]
public partial class AddObjectiveWindow : DefaultWindow
{
    private ICommonSession? _selectedAntagonist;
    private string _input = "";

    public AddObjectiveWindow()
    {
        RobustXamlLoader.Load(this);
    }

    protected override void EnteredTree()
    {
        ApplyButton.OnPressed += OnApplyButtonPressed;
        ObjectiveLineEdit.OnTextChanged += OnObjectiveLineEditChanged;
    }

    public void SetAntagonist(ICommonSession antagonistPlayerName)
    {
        _selectedAntagonist = antagonistPlayerName;
    }

    private void OnObjectiveLineEditChanged(LineEdit.LineEditEventArgs args)
    {
        _input = args.Text;
    }

    private void OnApplyButtonPressed(BaseButton.ButtonEventArgs args)
    {
        if (_input.Length == 0)
            return;

        IoCManager.Resolve<IClientConsoleHost>().ExecuteCommand($"addobjective {_selectedAntagonist} {_input}");
    }
}
