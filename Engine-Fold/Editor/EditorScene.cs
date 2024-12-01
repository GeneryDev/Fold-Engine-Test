﻿using System;
using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Gui.Styles;
using FoldEngine.Gui.Systems;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor;

public class EditorScene : Scene
{
    public EditorScene(IGameCore core) : base(core, "fold:editor")
    {
        Initialize();
    }

    public void Initialize()
    {
        BuildStyles();
        
        Systems.Add<EditorBase>();
        Systems.Add<EditorTabSystem>();
        Systems.Add<EditorCameraSyncSystem>();
        Systems.Add<SubSceneSystem>();
        
        Systems.Add<ControlLayoutSystem>();
        Systems.Add<ControlRenderer>();
        Systems.Add<ControlInterfaceSystem>();
        Systems.Add<ControlPopupSystem>();
        Systems.Add<TooltipSystem>();
        Systems.Add<TabSystem>();

        var entViewport = CreateEntity("Viewport");
        entViewport.AddComponent<Viewport>() = new Viewport
        {
            RenderGroupName = "editor",
            RenderLayerName = "editor_gui"
        };
        entViewport.AddComponent<Control>().RequestLayout = true;
        entViewport.AddComponent<AnchoredControl>() = new AnchoredControl()
        {
            AnchorRight = 1,
            AnchorBottom = 1
        };

        var docksContainer = CreateEntity("Docks");
        docksContainer.Hierarchical.SetParent(entViewport);
        docksContainer.AddComponent<Control>();
        docksContainer.AddComponent<AnchoredControl>() = new AnchoredControl()
        {
            AnchorRight = 1,
            AnchorBottom = 1
        };
        docksContainer.AddComponent<BorderContainer>() = new BorderContainer()
        {
            NorthPanelId = CreateDock("Top", out var topTabs, out var topContainer, Vector2.UnitY).EntityId,
            WestPanelId = CreateDock("Left", out var leftTabs, out var leftContainer, Vector2.UnitX).EntityId,
            EastPanelId = CreateDock("Right", out var rightTabs, out var rightContainer, -Vector2.UnitX).EntityId,
            SouthPanelId = CreateDock("Bottom", out var bottomTabs, out var bottomContainer, -Vector2.UnitY).EntityId
        };

        Entity CreateDock(string name, out long dockTabBarEntityId, out long dockContentEntityId, Vector2 resizeDirection)
        {
            const int dockMargin = 4;
            const int resizerSize = 8;
            
            var dock = CreateEntity(name);
            ref var dockControl = ref dock.AddComponent<Control>();
            dockControl.ZOrder = 0;
            dockControl.MinimumSize = dockControl.Size = new Vector2(160, 80);
            dock.AddComponent<BoxControl>().Color = new Color(Random.Shared.Next(256), Random.Shared.Next(256), Random.Shared.Next(256), 120);
            dock.Hierarchical.SetParent(docksContainer);

            var dockContent = CreateEntity("Dock Content");
            dockContent.AddComponent<Control>();
            dockContent.AddComponent<AnchoredControl>() = new AnchoredControl
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                OffsetTop = dockMargin,
                OffsetLeft = dockMargin,
                OffsetRight = -dockMargin,
                OffsetBottom = -dockMargin
            };
            dockContent.AddComponent<BorderContainer>();
            dockContent.Hierarchical.SetParent(dock);

            var tabBar = CreateEntity("Tab Bar");
            dockContent.GetComponent<BorderContainer>().NorthPanelId = tabBar.EntityId;
            tabBar.Hierarchical.SetParent(dockContent);
            tabBar.AddComponent<Control>();
            tabBar.AddComponent<FlowContainer>().HSeparation = 2;
            tabBar.AddComponent<TabList>();
            
            var tabContent = CreateEntity("Tab Content");
            tabContent.Hierarchical.SetParent(dockContent);
            tabContent.AddComponent<Control>();
            // tabContent.AddComponent<BoxControl>().Color = new Color(37, 37, 38, 255);
            tabBar.AddComponent<TabSwitcher>().ContainerEntityId = tabContent.EntityId;
            dockContent.AddComponent<EditorTabDropTarget>().TabBarId = tabBar.EntityId;

            dockTabBarEntityId = tabBar.EntityId;
            dockContentEntityId = tabContent.EntityId;

            var dockResizer = CreateEntity("Resizer");
            dockResizer.AddComponent<Control>() = new Control()
            {
                ZOrder = 2
            };
            dockResizer.AddComponent<AnchoredControl>() = new AnchoredControl()
            {
                AnchorTop = resizeDirection.Y == 0 ? 0 : (resizeDirection.Y / 2) + 0.5f,
                AnchorBottom = resizeDirection.Y == 0 ? 1 : (resizeDirection.Y / 2) + 0.5f,

                AnchorLeft = resizeDirection.X == 0 ? 0 : (resizeDirection.X / 2) + 0.5f,
                AnchorRight = resizeDirection.X == 0 ? 1 : (resizeDirection.X / 2) + 0.5f,
                
                OffsetTop = -resizerSize * (resizeDirection.Y > 0 ? 1 : 0),
                OffsetLeft = -resizerSize * (resizeDirection.X > 0 ? 1 : 0),
                OffsetRight = resizerSize * (resizeDirection.X < 0 ? 1 : 0),
                OffsetBottom = resizerSize * (resizeDirection.Y < 0 ? 1 : 0)
            };
            dockResizer.AddComponent<ResizeHandleControl>() = new ResizeHandleControl()
            {
                ResizeDirection = resizeDirection,
                MinimumSize = dockControl.MinimumSize,
                EntityToResize = dock.EntityId
            };
            dockResizer.Hierarchical.SetParent(dock);
            
            return dock;
        }

        Entity CreateTab(string name, string icon, long linkedControl)
        {
            var tab = CreateEntity("Tab");
            tab.AddComponent<Control>().MinimumSize = new Vector2(0, 14);
            tab.AddComponent<ButtonControl>() = new ButtonControl
            {
                Text = name,
                Alignment = Alignment.Begin,
                Icon = new ResourceIdentifier(icon),
                Style = new ResourceIdentifier("editor:tab"),
                KeepPressedOutside = true
            };
            tab.AddComponent<Tab>() = new Tab()
            {
                DeselectedButtonStyle = "editor:tab",
                SelectedButtonStyle = "editor:tab.selected",
                LinkedEntityId = linkedControl
            };
            tab.AddComponent<EditorTab>();
            return tab;
        }

        Entity CreateEditorView(string name, string icon, long tabBarId, long tabContainerId)
        {
            var view = CreateEntity(name);
            view.AddComponent<Control>().ZOrder = 0;
            view.AddComponent<AnchoredControl>() = new()
            {
                AnchorRight = 1,
                AnchorBottom = 1
            };
            view.Hierarchical.SetParent(tabContainerId);
            
            CreateTab(name, icon, view.EntityId).Hierarchical.SetParent(tabBarId);
            return view;
        }

        Entity CreateToolbarView(long tabBarId, long tabContainerId)
        {
            var view = CreateEditorView("Toolbar", "editor/cog", tabBarId, tabContainerId);

            view.AddComponent<FlowContainer>() = new FlowContainer()
            {
                HSeparation = 4,
                Alignment = Alignment.Begin
            };

            void AddToolbarButton(string name, string icon)
            {
                var btnEntity = CreateEntity("Toolbar Button");
                btnEntity.AddComponent<Control>() = new Control()
                {
                    MinimumSize = new Vector2(24, 24)
                };
                btnEntity.AddComponent<ButtonControl>() = new ButtonControl()
                {
                    Icon = new ResourceIdentifier(icon),
                    KeepPressedOutside = true
                };
                btnEntity.AddComponent<SimpleTooltip>() = new SimpleTooltip()
                {
                    Text = name
                };
                btnEntity.Hierarchical.SetParent(view);
            }
            
            AddToolbarButton("Hand","editor/hand");
            AddToolbarButton("Move","editor/move");
            AddToolbarButton("Scale","editor/scale");
            AddToolbarButton("Rotate","editor/rotate");
            AddToolbarButton("Select","editor/cursor");
            AddToolbarButton("Play Scene","editor/play");
            AddToolbarButton("Pause Scene","editor/pause");

            return view;
        }

        Entity CreateTempView(string name, string icon, long tabBarId, long tabContainerId)
        {
            var view = CreateEditorView(name, icon, tabBarId, tabContainerId);

            view.AddComponent<FlowContainer>() = new FlowContainer()
            {
                HSeparation = 4,
                Alignment = Alignment.Center
            };

            var label = CreateEntity("Temp Label");
            label.AddComponent<Control>();
            label.AddComponent<LabelControl>() = new LabelControl()
            {
                Text = name
            };
            label.Hierarchical.SetParent(view);

            return view;
        }
        
        CreateToolbarView(topTabs, topContainer);
        CreateTempView("Hierarchy", "editor/hierarchy", leftTabs, leftContainer);
        CreateTempView("Systems", "editor/cog", leftTabs, leftContainer);
        CreateTempView("Inspector", "editor/info", rightTabs, rightContainer);
        CreateTempView("Debug Actions", "editor/info", rightTabs, rightContainer);
        CreateTempView("Resources", "editor/checkmark", bottomTabs, bottomContainer);
        CreateTempView("Scene List", "editor/menu", bottomTabs, bottomContainer);
    }

    private void BuildStyles()
    {
        var defaultButtonStyle = Resources.Create<ButtonStyle>("editor:button");
        
        var tabButtonStyle = Resources.Create<ButtonStyle>("editor:tab");
        tabButtonStyle.FontSize = 7;
        tabButtonStyle.IconMaxWidth = 8;
        tabButtonStyle.IconTextSeparation = 4;
        tabButtonStyle.MarginLeft = 2;
        tabButtonStyle.MarginRight = 6;
        tabButtonStyle.TextColor = new Color(255, 255, 255, 150);
        tabButtonStyle.NormalColor = new Color(45, 45, 48);
        
        var tabSelectedButtonStyle = Resources.Create<ButtonStyle>("editor:tab.selected");
        tabSelectedButtonStyle.FontSize = 7;
        tabSelectedButtonStyle.IconMaxWidth = 8;
        tabSelectedButtonStyle.IconTextSeparation = 4;
        tabSelectedButtonStyle.MarginLeft = 2;
        tabSelectedButtonStyle.MarginRight = 6;
        tabSelectedButtonStyle.NormalColor = new Color(37, 37, 38);
    }
}