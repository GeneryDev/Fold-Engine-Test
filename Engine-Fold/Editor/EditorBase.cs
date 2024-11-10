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

    public bool InspectSelf = false;
    private long _currentSceneTabId = -1;
    private EditorTab _selfTab;
    private EditorTab _nullTab;
    private Transform _selfCameraTransform;
    private Transform _nullTransform;
    
    public ComponentIterator<EditorTab> TabIterator;

    public ref EditorTab CurrentTab
    {
        get
        {
            if (InspectSelf)
                return ref _selfTab;
            if (_currentSceneTabId != -1 && Scene.Components.HasComponent<EditorTab>(_currentSceneTabId))
                return ref Scene.Components.GetComponent<EditorTab>(_currentSceneTabId);
            return ref _nullTab;
        }
    }

    public Scene CurrentScene
    {
        get
        {
            if (InspectSelf)
                return Scene;
            if (_currentSceneTabId != -1 && Scene.Components.HasComponent<SubScene>(_currentSceneTabId))
                return Scene.Components.GetComponent<SubScene>(_currentSceneTabId).Scene;
            return null;
        }
    }

    public ref Transform CurrentCameraTransform
    {
        get
        {
            if (InspectSelf)
                return ref _selfCameraTransform;
            if (_currentSceneTabId != -1 && Scene.Components.HasComponent<EditorTab>(_currentSceneTabId))
            {
                var tab = Scene.Components.GetComponent<EditorTab>(_currentSceneTabId);
                return ref Scene.Components.GetComponent<Transform>(tab.EditorCameraEntityId);
            }
            return ref _nullTransform;
        }
    }

    public override void SubscribeToEvents()
    {
        Subscribe<WindowSizeChangedEvent>((ref WindowSizeChangedEvent evt) => { Environment.LayoutValidated = false; });
    }

    public override void Initialize()
    {
        _selfTab = new EditorTab()
        {
            SceneTransactions = new TransactionManager<Scene>(Scene)
        };
        
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

        if (_currentSceneTabId == -1)
        {
            _currentSceneTabId = tabEntity.EntityId;
        }
        else
        {
            tabEntity.AddComponent<InactiveComponent>();
        }
        
        var editorCameraEntity = Scene.CreateEntity("Editor Camera");
        editorCameraEntity.AddComponent<Camera>();
        editorCameraEntity.Transform.SetParent(tabEntity.EntityId);

        tab.EditorCameraEntityId = editorCameraEntity.EntityId;
    }

    public void SelectTab(long tabId)
    {
        _currentSceneTabId = tabId;

        TabIterator.Reset();
        while (TabIterator.Next())
        {
            var entity = new Entity(Scene, TabIterator.GetEntityId());
            entity.Active = entity.EntityId == tabId;
        }
    }
}