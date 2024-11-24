using System;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Containers;
using FoldEngine.Gui.Systems;
using FoldEngine.Interfaces;
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
        docksContainer.Transform.SetParent(entViewport);
        docksContainer.AddComponent<Control>();
        docksContainer.AddComponent<AnchoredControl>() = new AnchoredControl()
        {
            AnchorRight = 1,
            AnchorBottom = 1
        };
        docksContainer.AddComponent<BorderContainer>() = new BorderContainer()
        {
            NorthPanelId = CreateDock("Top").EntityId,
            WestPanelId = CreateDock("Left").EntityId,
            EastPanelId = CreateDock("Right").EntityId,
            SouthPanelId = CreateDock("Bottom").EntityId
        };

        Entity CreateDock(string name)
        {
            var dock = CreateEntity(name);
            ref var dockControl = ref dock.AddComponent<Control>();
            dockControl.MinimumSize = dockControl.Size = new Vector2(160, 80);
            // dock.AddComponent<BoxControl>().Color = new Color(Random.Shared.Next(256), Random.Shared.Next(256), Random.Shared.Next(256), 120);
            dock.Transform.SetParent(docksContainer);
            return dock;
        }
    }
}