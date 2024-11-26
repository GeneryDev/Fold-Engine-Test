using System;
using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Containers;
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
        Systems.Add<EditorCameraSyncSystem>();
        Systems.Add<SubSceneSystem>();
        
        Systems.Add<ControlLayoutSystem>();
        Systems.Add<ControlRenderer>();
        Systems.Add<ControlInterfaceSystem>();

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
            NorthPanelId = CreateDock("Top", out var topDock, Vector2.UnitY).EntityId,
            WestPanelId = CreateDock("Left", out var leftDock, Vector2.UnitX).EntityId,
            EastPanelId = CreateDock("Right", out var rightDock, -Vector2.UnitX).EntityId,
            SouthPanelId = CreateDock("Bottom", out var bottomDock, -Vector2.UnitY).EntityId
        };

        Entity CreateDock(string name, out long dockContentEntityId, Vector2 resizeDirection)
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

            CreateTab("Hierarchy", "editor/hierarchy").Hierarchical.SetParent(tabBar);
            CreateTab("Systems", "editor/cog").Hierarchical.SetParent(tabBar);

            dockContentEntityId = dockContent.EntityId;

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

        Entity CreateTab(string name, string icon)
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
            return tab;
        }
        
        Entity CreateEditorView(string name)
        {
            const int viewMargin = 5;
            var view = CreateEntity(name);
            view.AddComponent<Control>().ZOrder = 0;
            view.AddComponent<BoxControl>().Color = new Color(37, 37, 38, 255);
            return view;
        }

        Entity CreateToolbarView()
        {
            var view = CreateEditorView("Toolbar");

            view.AddComponent<FlowContainer>() = new FlowContainer()
            {
                HSeparation = 4,
                Alignment = Alignment.Begin
            };

            void AddToolbarButton(string icon)
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
                btnEntity.Hierarchical.SetParent(view);
            }
            
            AddToolbarButton("editor/hand");
            AddToolbarButton("editor/move");
            AddToolbarButton("editor/scale");
            AddToolbarButton("editor/rotate");
            AddToolbarButton("editor/cursor");
            AddToolbarButton("editor/play");
            AddToolbarButton("editor/pause");

            return view;
        }
        
        CreateToolbarView().Hierarchical.SetParent(topDock);
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
        tabButtonStyle.NormalColor = new Color(45, 45, 48);
        tabButtonStyle.PressedColor = new Color(37, 37, 38);
    }
}