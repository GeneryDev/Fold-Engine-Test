﻿using FoldEngine.Commands;
using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui;
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
    public bool InspectSelf = false;
    private long _currentSceneTabId = -1;
    private SubScene _nullSubScene;
    
    private EditorSceneTab _selfSceneTab;
    private EditorSceneTab _nullSceneTab;
    
    private Transform _selfCameraTransform;
    private Transform _nullTransform;
    
    [DoNotSerialize]
    public ComponentIterator<EditorSceneTab> TabIterator;

    public ref EditorSceneTab CurrentSceneTab
    {
        get
        {
            if (InspectSelf)
                return ref _selfSceneTab;
            if (_currentSceneTabId != -1 && Scene.Components.HasComponent<EditorSceneTab>(_currentSceneTabId))
                return ref Scene.Components.GetComponent<EditorSceneTab>(_currentSceneTabId);
            return ref _nullSceneTab;
        }
    }

    public ref SubScene CurrentSubScene
    {
        get
        {
            if (InspectSelf)
                return ref _nullSubScene;
            if (_currentSceneTabId != -1 && Scene.Components.HasComponent<SubScene>(_currentSceneTabId))
                return ref Scene.Components.GetComponent<SubScene>(_currentSceneTabId);
            return ref _nullSubScene;
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
            if (_currentSceneTabId != -1 && Scene.Components.HasComponent<EditorSceneTab>(_currentSceneTabId))
            {
                var tab = Scene.Components.GetComponent<EditorSceneTab>(_currentSceneTabId);
                return ref Scene.Components.GetComponent<Transform>(tab.EditorCameraEntityId);
            }
            return ref _nullTransform;
        }
    }

    public override void Initialize()
    {
        _selfSceneTab = new EditorSceneTab()
        {
            SceneTransactions = new TransactionManager<Scene>(Scene),
            Scene = Scene
        };
        
        TabIterator = CreateComponentIterator<EditorSceneTab>(IterationFlags.Ordered | IterationFlags.IncludeInactive);
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        if (!renderer.Groups.ContainsKey("editor")) return;
        if (renderer.RootGroup != renderer.Groups["editor"])
        {
            Scene.Core.CommandQueue.Enqueue(new SetRootRendererGroupCommand(renderer.Groups["editor"]));
            return;
        }

        if (_currentSceneTabId != -1)
        {
            var currentTab = CurrentSceneTab;
            foreach (long entityId in CurrentSceneTab.EditingEntity)
            {
                if (currentTab.Scene.Components.HasComponent<Hierarchical>(entityId))
                {
                    var entity = new Entity(currentTab.Scene, entityId);

                    LevelRenderer2D.DrawOutline(entity);
                    ColliderGizmoRenderer.DrawColliderGizmos(entity);
                }
            }
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

        ref var tab = ref tabEntity.AddComponent<EditorSceneTab>();
        tab.Scene = editedScene;
        tab.SceneTransactions = new TransactionManager<Scene>(editedScene);

        if (_currentSceneTabId == -1)
        {
            _currentSceneTabId = tabEntity.EntityId;
        }
        else
        {
            tabEntity.Hierarchical.Active = false;
        }
        
        var editorCameraEntity = Scene.CreateEntity("Editor Camera");
        editorCameraEntity.AddComponent<Camera>();
        editorCameraEntity.Hierarchical.SetParent(tabEntity.EntityId);

        tab.EditorCameraEntityId = editorCameraEntity.EntityId;
    }

    public void SelectSceneTab(long tabId)
    {
        _currentSceneTabId = tabId;

        TabIterator.Reset();
        while (TabIterator.Next())
        {
            var entity = new Entity(Scene, TabIterator.GetEntityId());
            entity.Hierarchical.Active = entity.EntityId == tabId;
        }
    }

    public void Undo()
    {
        if (_currentSceneTabId != -1)
        {
            CurrentSceneTab.SceneTransactions.Undo();
        }
    }

    public void Redo()
    {
        if (_currentSceneTabId != -1)
        {
            CurrentSceneTab.SceneTransactions.Redo();
        }
    }
}