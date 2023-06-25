// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using System.Collections.Immutable;
using Content.Client.Message;
using Content.Client.SS220.Photocopier.Forms;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Controls.FancyTree;
using Content.Shared.SS220.Photocopier;
using Content.Shared.SS220.Photocopier.Forms;
using Content.Shared.SS220.Photocopier.Forms.FormManagerShared;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.SS220.Photocopier.UI;

[GenerateTypedNameReferences]
public sealed partial class PhotocopierWindow : FancyWindow
{
    // Constants
    private const int SelectedLabelCharLimit = 23;

    private const float TonerBarColorValue = 50.2f / 100;
    private const float TonerBarColorSaturation = 48.44f / 100;
    private const float TonerBarFullHue = 118.06f / 360;
    private const float TonerBarEmptyHue = 0;

    private readonly Vector2 _groupIconSize = new Vector2(30, 30);

    // Dependencies
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;

    // Actions
    public event Action? EjectButtonPressed;
    public event Action? StopButtonPressed;
    public event Action<int>? CopyButtonPressed;
    public event Action<int, FormDescriptor>? PrintButtonPressed;
    public event Action? RefreshButtonPressed;

    // State
    private int _copyAmount = 1;
    private int _maxCopyAmount = 10;
    private bool _canPrint;
    private HashSet<string> _lastAvailableFormCollectionIds = new();

    /// <summary>
    /// Used for tree repopulation and FormDescriptor construction
    /// </summary>
    private readonly ImmutableList<FormCollection> _collections;

    public PhotocopierWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        Tree.TreeScroll.ReturnMeasure = false;

        var specificFormManager = _sysMan.GetEntitySystem<FormManager>();
        _collections = specificFormManager.GetImmutableFormsTree();
        OnTreeSelectionChanged(Tree.SelectedItem);

        AmountLineEdit.OnTextChanged += OnAmountChanged;
        Tree.OnSelectedItemChanged += OnTreeSelectionChanged;

        IncreaseButton.OnPressed += IncreaseCopyAmount;
        DecreaseButton.OnPressed += DecreaseCopyAmount;
        PrintButton.OnPressed += OnPrintButtonPressed;
        EjectButton.OnPressed += _ => EjectButtonPressed?.Invoke();
        CopyButton.OnPressed += _ => CopyButtonPressed?.Invoke(_copyAmount);
        StopButton.OnPressed += _ => StopButtonPressed?.Invoke();
        RefreshButton.OnPressed += _ => RefreshButtonPressed?.Invoke();
        SearchBar.OnTextChanged += _ => RepopulateTreeWithAvailableCollections();
    }

    private void UpdatePrintButton()
    {
        PrintButton.Disabled = !_canPrint || Tree.SelectedItem is not { Metadata: FormDescriptor };
    }

    private void MakeCopyAmountValid()
    {
        if (_copyAmount > _maxCopyAmount)
            _copyAmount = _maxCopyAmount;

        // yep, we do it even if _maxCopyAmount is less than 1
        if (_copyAmount < 1)
            _copyAmount = 1;

        // Doesn't call OnAmountChanged as of 05.06.2023, and I hope it stays this way
        AmountLineEdit.Text = _copyAmount.ToString();
    }

    private void IncreaseCopyAmount(BaseButton.ButtonEventArgs args)
    {
        _copyAmount++;
        MakeCopyAmountValid();
    }

    private void DecreaseCopyAmount(BaseButton.ButtonEventArgs args)
    {
        _copyAmount--;
        MakeCopyAmountValid();
    }

    private void SetTonerBarValue(float value)
    {
        TonerBar.Value = value;

        const float tonerBarHueDiff = TonerBarFullHue - TonerBarEmptyHue;
        var tonerBarFillHue = TonerBarEmptyHue + tonerBarHueDiff * value;
        var tonerBarFillColor = Color.FromHsv(new Vector4(
            tonerBarFillHue, TonerBarColorSaturation, TonerBarColorValue, 1));

        TonerBar.ForegroundStyleBoxOverride ??= new StyleBoxFlat();
        var fgStyleOverride = (StyleBoxFlat) TonerBar.ForegroundStyleBoxOverride;
        fgStyleOverride.BackgroundColor = tonerBarFillColor;
    }

    public void UpdateState(PhotocopierUiState state)
    {
        _maxCopyAmount = state.MaxQueueLength;

        var isPrinting = state.PrintQueueLength > 0;
        _canPrint = state is { TonerAvailable: > 0 } && !isPrinting;
        var thereIsSomethingToCopy = state.IsPaperInserted || state.ButtIsOnScanner;
        CopyButton.Disabled = !_canPrint || !thereIsSomethingToCopy;
        StopButton.Disabled = !isPrinting;
        UpdatePrintButton();

        // Update toner amount
        SetTonerBarValue(state.TonerCapacity == 0 ? 0 : (float) state.TonerAvailable / state.TonerCapacity);
        ChargePercentage.Text = Loc.GetString(
            "photocopier-ui-toner-remaining",
            ("percentage", (int) (TonerBar.Value * 100)),
            ("lists", state.TonerAvailable));

        // Update scanner status
        EjectButton.Disabled = !state.IsPaperInserted || state.IsSlotLocked;
        var stateMarkupLocId = state switch
        {
            { IsPaperInserted: true } => "photocopier-ui-scan-surface-item",
            { ButtIsOnScanner: true } => "photocopier-ui-scan-surface-posterior",
            _ => "photocopier-ui-scan-surface-empty"
        };
        PaperStatusLabel.SetMarkup(Loc.GetString(stateMarkupLocId));

        // Update printer status
        string statusLabelText;
        if (isPrinting)
        {
            var queueLenString = (state.PrintQueueLength - 1).ToString();
            statusLabelText = Loc.GetString("photocopier-ui-status-printing", ("queue", queueLenString));
        }
        else if (state.TonerAvailable <= 0)
            statusLabelText = Loc.GetString("photocopier-ui-status-out");
        else
            statusLabelText = Loc.GetString("photocopier-ui-status-idle");

        StatusLabel.SetMarkup(statusLabelText);

        // Update form tree
        if (!state.AvailableFormCollections.SetEquals(_lastAvailableFormCollectionIds))
        {
            _lastAvailableFormCollectionIds = state.AvailableFormCollections;
            RepopulateTreeWithAvailableCollections();
        }
    }

    private void RepopulateTreeWithAvailableCollections()
    {
        List<FormCollection> availableFormTree = new();

        foreach (var collection in _collections)
        {
            if (!_lastAvailableFormCollectionIds.Contains(collection.CollectionId))
                continue;

            availableFormTree.Add(collection);
        }

        RepopulateTree(availableFormTree);
    }

    private void RepopulateTree(List<FormCollection> collections)
    {
        Tree.Clear();

        Dictionary<string, TreeItem> existingGroups = new();

        foreach (var collection in collections)
        {
            foreach (var formGroup in collection.Groups)
            {
                if (existingGroups.TryGetValue(formGroup.GroupId, out var existingGroupEntry))
                {
                    AddEntry(null, formGroup, collection.CollectionId, existingGroupEntry);
                }
                else
                {
                    var groupEntry = AddEntry(null, formGroup, collection.CollectionId);
                    if (groupEntry is not null)
                        existingGroups.Add(formGroup.GroupId, groupEntry);
                }
            }
        }

        //expand when searching
        Tree.SetAllExpanded(!string.IsNullOrWhiteSpace(SearchBar.Text));
    }

    private TreeItem? AddEntry(TreeItem? parent, FormGroup group, string collectionId, TreeItem? useAsEntry = null)
    {
        // Use SortedDictionary so entries are sorted alphabetically
        SortedDictionary<string, (Form, FormDescriptor)> childEntries = new();

        foreach (var form in group.Forms)
        {
            var title = form.Value.PhotocopierTitle;
            if (!string.IsNullOrWhiteSpace(SearchBar.Text) &&
                !title.Contains(SearchBar.Text, StringComparison.OrdinalIgnoreCase))
                continue;

            var descriptor = new FormDescriptor(
                collectionId,
                group.GroupId,
                form.Key
            );

            childEntries.Add(title, (form.Value, descriptor));
        }

        // don't create group if it is empty/everything is filtered out
        if (childEntries.Count <= 0)
            return null;

        TreeItem item;
        if (useAsEntry is null)
        {
            item = Tree.AddItem(parent);
            item.Label.Text = group.Name;
            item.Label.Modulate = group.Color;
        }
        else
        {
            item = useAsEntry;
        }

        if (group.IconPath is not null)
        {
            item.Icon.TexturePath = group.IconPath;
            item.Icon.Stretch = TextureRect.StretchMode.Scale;
            item.Icon.MinSize = _groupIconSize;
            item.Icon.Visible = true;
        }

        foreach (var formData in childEntries)
        {
            AddEntry(item, formData.Value.Item1, formData.Value.Item2);
        }

        return item;
    }

    private void AddEntry(TreeItem? parent, Form entry, FormDescriptor descriptor)
    {
        var item = Tree.AddItem(parent);
        item.Label.Text = entry.PhotocopierTitle;
        item.Metadata = descriptor;

        if (parent is not null)
            item.Label.Modulate = parent.Label.Modulate;
    }

    private void OnTreeSelectionChanged(TreeItem? item)
    {
        UpdatePrintButton();

        string labelText;
        if (item is { Metadata: FormDescriptor })
        {
            labelText = item.Label.Text ?? Loc.GetString("photocopier-ui-no-doc-title");
            if (labelText.Length > SelectedLabelCharLimit)
            {
                labelText = labelText[..(SelectedLabelCharLimit - 3)] + "...";
            }
        }
        else
            labelText = Loc.GetString("photocopier-ui-no-doc");

        SelectedFormLabel.SetMarkup(labelText);
    }

    private void OnAmountChanged(LineEdit.LineEditEventArgs args)
    {
        if (!int.TryParse(AmountLineEdit.Text, out var newCopyAmount))
            newCopyAmount = 1;

        if (_copyAmount == newCopyAmount)
            return;

        _copyAmount = newCopyAmount;
        MakeCopyAmountValid();
    }

    private void OnPrintButtonPressed(BaseButton.ButtonEventArgs args)
    {
        var item = Tree.SelectedItem;
        var metadata = item?.Metadata;

        if (metadata is FormDescriptor descriptor)
        {
            PrintButtonPressed?.Invoke(_copyAmount, descriptor);
        }
    }
}
