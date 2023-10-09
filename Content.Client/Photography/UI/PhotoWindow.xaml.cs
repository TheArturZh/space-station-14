// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using System.Numerics;
using Content.Client.Message;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Photography.UI;
[GenerateTypedNameReferences]
public sealed partial class PhotoWindow : BaseWindow
{
    private readonly FixedEye _defaultEye = new();

    public PhotoWindow()
    {
        RobustXamlLoader.Load(this);

        CloseButton.OnPressed += _ => Close();

        PhotoView.Eye = _defaultEye;
        PhotoView.ViewportSize = new Vector2i(500, 500);
        SetVisuals(null);

        BackText.SetMarkup("Written on the back:\n[italic]Test test test[/italic]");
    }

    public void SetVisuals(EyeComponent? eye)
    {
        PhotoView.Visible = eye != null;

        if (eye != null)
            PhotoView.Eye = eye.Eye;
        else
            PhotoView.Eye = _defaultEye;
    }

    // Drag by grabbing anywhere
    protected override DragMode GetDragModeFor(Vector2 relativeMousePos)
    {
        return DragMode.Move;
    }
}
