﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.SecretStation.DarkForces.Ratvar.Prototypes;
using Content.Shared.SecretStation.DarkForces.Ratvar.UI;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.SecretStationClient.DarkForces.Ratvar.Structures;

[GenerateTypedNameReferences]
public sealed partial class RatvarWorkshopWindow : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

    public Action<EntProtoId, int, int, int>? OnCraftPressed;

    private readonly SpriteSystem _spriteSystem;
    private readonly HashSet<RatvarCraftCategoryPrototype> _categories;
    private readonly Color _categoryBackgroundColor;
    private readonly Thickness _defaultMargin;

    public RatvarWorkshopWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _spriteSystem = _entitySystem.GetEntitySystem<SpriteSystem>();
        _categories = _prototype.EnumeratePrototypes<RatvarCraftCategoryPrototype>().ToHashSet();
        _defaultMargin = new Thickness(8, 8, 8, 8);

        Color.TryParse("#25252a80", out _categoryBackgroundColor);
    }

    public void UpdateState(RatvarWorkshopUIState state)
    {
        BrassCount.Text = $"{state.Brass}";
        PowerCount.Text = $"{state.Power}";
        ProgressState.Text = state.InProgress ? "В работе" : "Готово к работе";

        foreach (var child in CraftList.Children.ToArray())
        {
            child.Dispose();
        }

        CreateReceipts(state.Brass, state.Power, state.InProgress);
    }

    private void CreateReceipts(int brass, int power, bool inProgress)
    {
        foreach (var category in _categories)
        {
            var container = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
            };

            var categoryLabel = new Label
            {
                Text = $"{category.Name}",
                Margin = _defaultMargin
            };
            container.AddChild(categoryLabel);

            var panelContainer = new PanelContainer
            {
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = _categoryBackgroundColor
                },
                Margin = _defaultMargin
            };

            panelContainer.AddChild(container);

            foreach (var receiptProtId in category.Receipts)
            {
                var receipt = _prototype.Index(receiptProtId);
                var receiptContainer = new BoxContainer
                {
                    Orientation = BoxContainer.LayoutOrientation.Horizontal,
                };
                var textureRect = new TextureRect
                {
                    Texture = _spriteSystem.Frame0(receipt.Icon),
                    TextureScale = new Vector2(1.5f, 1.5f),
                    Stretch = TextureRect.StretchMode.KeepCentered,
                };
                var button = new Button
                {
                    Text = receipt.Name,
                    MaxHeight = 28f,
                    Margin = _defaultMargin,
                };
                var requirementsLabel = new Label
                {
                    Text = $"Латунь: {receipt.BrassCost}, Мощность: {receipt.PowerCost}",
                    StyleClasses = {StyleBase.StyleClassLabelSubText},
                    Align = Label.AlignMode.Right,
                    Margin = _defaultMargin,
                    HorizontalExpand = true
                };

                button.Disabled = inProgress || receipt.BrassCost > brass || receipt.PowerCost > power;
                button.OnPressed += _ =>
                    OnCraftPressed?.Invoke(receipt.EntityProduce, receipt.BrassCost, receipt.PowerCost,
                        receipt.CraftingTime);

                receiptContainer.AddChild(textureRect);
                receiptContainer.AddChild(button);
                receiptContainer.AddChild(requirementsLabel);

                container.AddChild(receiptContainer);
            }

            CraftList.AddChild(panelContainer);
        }
    }
}
