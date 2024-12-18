﻿using System;
using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Gui.Systems;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.ImmediateGui;

[GameSystem("fold:editor.context_menus", ProcessingCycles.None)]
public class EditorContextMenuSystem : GameSystem
{
    private long _popupId = -1;
    private string _buttonStyle;
    private Entity _buttonContainer;
    private Alignment _textAlignment;

    public void Show(Point pos, Action<EditorContextMenuSystem> renderer, int minWidth = 150, string buttonStyle = "editor:context_menu_item", Alignment textAlignment = Alignment.Begin)
    {
        var popupSystem = Scene.Systems.Get<ControlPopupSystem>();
        long popupId = popupSystem.CreatePopup(-1, pos, sendBuildRequestEvent: false);

        _popupId = popupId;
        _buttonStyle = buttonStyle;
        _textAlignment = textAlignment;

        var popupOutlinePanel = Scene.CreateEntity("Context Menu Outline Panel");
        popupOutlinePanel.Hierarchical.SetParent(popupId);
        popupOutlinePanel.AddComponent<Control>() = new Control()
        {
            RequestLayout = true,
            ZOrder = 90
        };
        popupOutlinePanel.AddComponent<BorderContainer>() = new BorderContainer()
        {
            NorthPanelId = CreateMargin(2, popupOutlinePanel.EntityId),
            WestPanelId = CreateMargin(2, popupOutlinePanel.EntityId),
            EastPanelId = CreateMargin(2, popupOutlinePanel.EntityId),
            SouthPanelId = CreateMargin(2, popupOutlinePanel.EntityId)
        };
        popupOutlinePanel.AddComponent<BoxControl>() = new BoxControl()
        {
            Color = new Color(45, 45, 48)
        };

        var buttonContainer = Scene.CreateEntity("Context Menu Button Container");
        buttonContainer.Hierarchical.SetParent(popupOutlinePanel);
        buttonContainer.AddComponent<Control>() = new Control()
        {
            MinimumSize = new Vector2(minWidth, 1),
            ZOrder = 91
        };
        buttonContainer.AddComponent<StackContainer>() = new StackContainer()
        {
            Vertical = true,
            Separation = 4
        };
        buttonContainer.AddComponent<BoxControl>() = new BoxControl()
        {
            Color = new Color(37, 37, 38)
        };
        _buttonContainer = buttonContainer;

        renderer(this);
    }

    private long CreateMargin(int size, long parent)
    {
        var margin = Scene.CreateEntity("Margin");
        margin.AddComponent<Control>() = new Control()
        {
            MinimumSize = new Vector2(size, size),
        };
        margin.Hierarchical.SetParent(parent);
        return margin.EntityId;
    }

    public Entity Button(string text, string icon = null)
    {
        icon ??= "editor/blank";
        var button = Scene.CreateEntity("Button");
        button.AddComponent<Control>() = new Control()
        {
            ZOrder = 92,
            MinimumSize = new Vector2(1, 15)
        };
        button.AddComponent<ButtonControl>() = new ButtonControl()
        {
            Text = text,
            Alignment = _textAlignment,
            Icon = new ResourceIdentifier(icon),
            Style = new ResourceIdentifier(_buttonStyle)
        };
        button.AddComponent<DismissPopupOnPress>() = new DismissPopupOnPress()
        {
            PopupId = _popupId
        };
        button.Hierarchical.SetParent(_buttonContainer);
        return button;
    }

    public void Separator()
    {
        var separator = Scene.CreateEntity("Separator");
        separator.AddComponent<Control>() = new Control()
        {
            MinimumSize = new Vector2(2, 2),
            ZOrder = 92
        };
        separator.AddComponent<BoxControl>() = new BoxControl()
        {
            Color = new Color(45, 45, 48)
        };
        separator.Hierarchical.SetParent(_buttonContainer);
    }
}