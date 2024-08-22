﻿using System.Collections.Generic;
using Content.Client.UserInterface.Controls;
using Content.Shared.SS220.DarkForces.Narsi.Buildings.Altar.Abilities;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;

namespace Content.Client.SS220.DarkForces.Narsi.Buildings.Altar.Abilities;

[GenerateTypedNameReferences]
public sealed partial class NarsiAbilitiesWindow : FancyWindow
{
    private readonly NarsiAbilitiesBoundInterface _bui;

    public NarsiAbilitiesWindow(NarsiAbilitiesBoundInterface bui)
    {
        RobustXamlLoader.Load(this);
        _bui = bui;

        Description.SetMessage(FormattedMessage.EscapeText("Способности культистов - ключ к призыву Нар'Си.\nСпособности могут быть полезны в различных ситуациях, таких как: защита, нападение, проникновение.\nКаждая способность имеет три уровня, описание которых вы можете увидеть здесь.\nЧтобы прокачивать способности, нужно получить Очки Крови, которые выдаются за задания культа"));
        SetupSplitContainer();
    }

    private void SetupSplitContainer()
    {
        SplitContainer.ResizeMode = SplitContainer.SplitResizeMode.RespectChildrenMinSize;
        SplitContainer.SplitWidth = 2;
        SplitContainer.SplitEdgeSeparation = 1f;
        SplitContainer.StretchDirection = SplitContainer.SplitStretchDirection.TopLeft;
    }

    public void UpdateState(NarsiAbilitiesState state)
    {
        ClosedAbilities.RemoveAllChildren();
        OpenedAbilities.RemoveAllChildren();

        AddToControl(OpenedAbilities, state.OpenedAbilities, state.BloodScore, false, state.IsLeader);
        AddToControl(ClosedAbilities, state.ClosedAbilities, state.BloodScore, true, state.IsLeader);
    }

    private void AddToControl(BoxContainer container, List<NarsiAbilityUIModel> items, int bloodScore, bool isClosed, bool isLeader)
    {
        foreach (var item in items)
        {
            var control = new NarsiAbilityControl(
                id: item.Id,
                name: item.Name,
                description: item.Description,
                levelDescription: item.LevelDescription,
                level: item.Level,
                requiredBloodScore: item.RequiredBloodScore,
                icon: item.Icon,
                buttonState: GetButtonState(isClosed, isLeader, bloodScore, item.RequiredBloodScore),
                bui: _bui
            );
            container.AddChild(control);
        }
    }

    private NarsiAbilityControl.ButtonState GetButtonState(bool isClosed, bool isLeader, int bloodScore, int requiredBloodScore)
    {
        if (!isClosed)
            return NarsiAbilityControl.ButtonState.Learn;

        if (bloodScore < requiredBloodScore)
            return NarsiAbilityControl.ButtonState.ClosedNotEnoughPoints;

        if (isLeader)
            return NarsiAbilityControl.ButtonState.ClosedLeader;

        return NarsiAbilityControl.ButtonState.ClosedNotLeader;
    }
}
