using System.Collections.Generic;
using FoldEngine.Commands;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;
using FoldEngine.Events;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Rendering;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Systems;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor;

[GameSystem("fold:editor.base", ProcessingCycles.All, true)]
public class EditorBase : GameSystem
{
    public List<long> EditingEntity = new List<long>();
    // public override bool ShouldSave => false;

    [DoNotSerialize] public EditorEnvironment Environment;

    public long CurrentSceneTabId = -1;
    
    public ComponentIterator<EditorTab> TabIterator;

    public EditorTab CurrentTab =>
        CurrentSceneTabId != -1 && Scene.Components.HasComponent<EditorTab>(CurrentSceneTabId)
            ? Scene.Components.GetComponent<EditorTab>(CurrentSceneTabId)
            : default;

    public Scene CurrentScene => CurrentSceneTabId != -1 && Scene.Components.HasComponent<SubScene>(CurrentSceneTabId)
        ? Scene.Components.GetComponent<SubScene>(CurrentSceneTabId).Scene
        : null;

    public override void SubscribeToEvents()
    {
        Subscribe<WindowSizeChangedEvent>((ref WindowSizeChangedEvent evt) => { Environment.LayoutValidated = false; });
    }

    public override void Initialize()
    {
        Environment = new EditorEnvironment(this);

        Environment.AddView<EditorToolbarView>(Environment.NorthPanel);
        Environment.AddView<EditorHierarchyView>(Environment.WestPanel);
        Environment.AddView<EditorSystemsView>(Environment.WestPanel);
        Environment.AddView<EditorInspectorView>(Environment.EastPanel);
        Environment.AddView<EditorSceneView>(Environment.CenterPanel);
        Environment.AddView<EditorResourcesView>(Environment.SouthPanel);
        Environment.AddView<EditorDebugActionsView>(Environment.EastPanel);
        Environment.AddView<EditorSceneListView>(Environment.SouthPanel);
        // Environment.AddView<EditorTestView>(Environment.SouthPanel);

        Environment.WestPanel.ViewLists[0].ActiveView = Environment.GetView<EditorHierarchyView>();
        Environment.NorthPanel.ViewLists[0].ActiveView = Environment.GetView<EditorToolbarView>();
        Environment.SouthPanel.ViewLists[0].ActiveView = Environment.GetView<EditorResourcesView>();

        TabIterator = CreateComponentIterator<EditorTab>(IterationFlags.Ordered | IterationFlags.IncludeInactive);
    }

    public override void OnInput()
    {
        Environment.Input(Scene.Core.InputUnit);
    }

    public override void OnUpdate()
    {
        Environment.Update();
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        if (!renderer.Groups.ContainsKey("editor")) return;
        if (renderer.RootGroup != renderer.Groups["editor"])
        {
            Scene.Core.CommandQueue.Enqueue(new SetRootRendererGroupCommand(renderer.Groups["editor"]));
            return;
        }

        Environment.Render(renderer, renderer.RootGroup["editor_gui"], renderer.RootGroup["editor_gui_overlay"]);

        foreach (long entityId in EditingEntity)
            if (Scene.Components.HasComponent<Transform>(entityId))
            {
                var entity = new Entity(Scene, entityId);

                LevelRenderer2D.DrawOutline(entity);
                ColliderGizmoRenderer.DrawColliderGizmos(entity);
            }
    }

    public void OpenScene(Scene editedScene)
    {
        editedScene.Flush();
        
        var tabEntity = Scene.CreateEntity($"Tab: {editedScene.Name}");
        ref var subScene = ref tabEntity.AddComponent<SubScene>();
        subScene.Scene = editedScene;
        subScene.Render = true;
        subScene.Update = false;
        subScene.ProcessInputs = false;

        ref var tab = ref tabEntity.AddComponent<EditorTab>();
        tab.SceneTransactions = new TransactionManager<Scene>(editedScene);
        tab.EditingEntity = new List<long>();

        if (CurrentSceneTabId == -1)
        {
            CurrentSceneTabId = tabEntity.EntityId;
        }
        else
        {
            tabEntity.AddComponent<InactiveComponent>();
        }
    }

    public void SelectTab(long tabId)
    {
        CurrentSceneTabId = tabId;

        TabIterator.Reset();
        while (TabIterator.Next())
        {
            var entity = new Entity(Scene, TabIterator.GetEntityId());
            entity.Active = entity.EntityId == tabId;
        }
    }
}