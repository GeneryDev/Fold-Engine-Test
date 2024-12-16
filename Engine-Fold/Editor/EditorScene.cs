using System;
using FoldEngine.Editor.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Panels;
using FoldEngine.Editor.Systems;
using FoldEngine.Editor.Views;
using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Controls.Containers;
using FoldEngine.Gui.Styles;
using FoldEngine.Gui.Systems;
using FoldEngine.ImmediateGui;
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
        Systems.Add<EditorSceneViewSystem>();
        Systems.Add<EditorCameraSyncSystem>();
        Systems.Add<EditorContextMenuSystem>();
        Systems.Add<EditorActionSystem>();
        Systems.Add<EditorToolSystem>();
        Systems.Add<SubSceneSystem>();
        
        Systems.Add<ControlLayoutSystem>();
        Systems.Add<ControlInterfaceSystem>();
        Systems.Add<ControlPopupSystem>();
        Systems.Add<StandardControlsSystem>();
        Systems.Add<TooltipSystem>();
        Systems.Add<TabSystem>();
        
        Systems.Add<ImmediateGuiSystem>();

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
            SouthPanelId = CreateDock("Bottom", out var bottomTabs, out var bottomContainer, -Vector2.UnitY).EntityId,
            CornerBiasSouthEast = BorderContainer.CornerBias.Vertical
        };
        CreateDock("Center", out var centerTabs, out var centerContainer, Vector2.Zero);

        Entity CreateDock(string name, out long dockTabBarEntityId, out long dockContentEntityId, Vector2 resizeDirection)
        {
            const int dockMargin = 4;
            const int resizerSize = 8;
            
            var dock = CreateEntity(name);
            ref var dockControl = ref dock.AddComponent<Control>();
            dockControl.ZOrder = 0;
            dockControl.MinimumSize = dockControl.Size = new Vector2(160, 80);
            dock.AddComponent<BoxControl>().Color = new Color(45, 45, 48);
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
            tabContent.AddComponent<BoxControl>().Color = new Color(37, 37, 38, 255);
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
            if (resizeDirection.LengthSquared() > 0)
            {
                dockResizer.AddComponent<ResizeHandleControl>() = new ResizeHandleControl()
                {
                    ResizeDirection = resizeDirection,
                    MinimumSize = dockControl.MinimumSize,
                    EntityToResize = dock.EntityId
                };
            }
            dockResizer.Hierarchical.SetParent(dock);
            
            return dock;
        }

        EditorPanels.Toolbar(this, topTabs, topContainer);
        EditorPanels.Immediate<EditorToolbarView>(this, "Toolbar (old)", "editor/cog", topTabs, topContainer);
        EditorPanels.Immediate<EditorHierarchyView>(this, "Hierarchy", "editor/hierarchy", leftTabs, leftContainer);
        EditorPanels.Immediate<EditorSystemsView>(this, "Systems", "editor/cog", leftTabs, leftContainer);
        EditorPanels.Immediate<EditorInspectorView>(this, "Inspector", "editor/info", rightTabs, rightContainer);
        EditorPanels.Immediate<EditorDebugActionsView>(this, "Debug Actions", "editor/info", rightTabs, rightContainer);
        EditorPanels.Immediate<EditorResourcesView>(this, "Resources", "editor/checkmark", bottomTabs, bottomContainer);
        EditorPanels.Immediate<EditorSceneListView>(this, "Scene List", "editor/menu", bottomTabs, bottomContainer);
        EditorPanels.SceneView(this, centerTabs, centerContainer);
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
        
        var contextMenuItemStyle = Resources.Create<ButtonStyle>("editor:context_menu_item");
        contextMenuItemStyle.FontSize = 9;
        contextMenuItemStyle.IconMaxWidth = 8;
        contextMenuItemStyle.IconTextSeparation = 4;
        contextMenuItemStyle.MarginLeft = 6;
        contextMenuItemStyle.MarginRight = 6;
        
        var dropdownItemStyle = Resources.Create<ButtonStyle>("editor:dropdown_item");
        dropdownItemStyle.FontSize = 9;
        dropdownItemStyle.IconMaxWidth = 8;
        dropdownItemStyle.IconTextSeparation = 4;
        dropdownItemStyle.MarginLeft = 6;
        dropdownItemStyle.MarginRight = 6;
        
    }
}