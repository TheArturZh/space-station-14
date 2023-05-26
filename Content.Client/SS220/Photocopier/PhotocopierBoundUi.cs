using Content.Shared.SS220.Photocopier;
using Content.Client.SS220.Photocopier.UI;
using Content.Shared.Containers.ItemSlots;

using Robust.Client.GameObjects;

namespace Content.Client.SS220.Photocopier;

public sealed class PhotocopierBoundUi : BoundUserInterface
{
    private PhotocopierWindow? _window;

    public PhotocopierBoundUi(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new PhotocopierWindow();
        _window.OpenCentered();

        _window.OnClose += Close;
        _window.PrintButtonPressed += OnPrintButtonPressed;
        _window.CopyButtonPressed += OnCopyButtonPressed;
        _window.EjectButtonPressed += OnEjectButtonPressed;
        _window.StopButtonPressed += OnStopButtonPressed;
    }

    private void OnPrintButtonPressed(int amount, string collection, string group, string formName)
    {

    }

    private void OnCopyButtonPressed(int amount)
    {
        SendMessage(new PhotocopierCopyMessage(amount));
    }

    private void OnEjectButtonPressed()
    {
        SendMessage(new ItemSlotButtonPressedEvent(PhotocopierComponent.PaperSlotId, true, false));
    }

    private void OnStopButtonPressed()
    {
        SendMessage(new PhotocopierStopMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not PhotocopierUiState cast)
            return;

        _window.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if(disposing)
            _window?.Dispose();
    }
}
