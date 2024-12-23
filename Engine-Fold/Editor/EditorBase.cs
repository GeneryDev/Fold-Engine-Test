using FoldEngine.Commands;
using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Views;
using FoldEngine.Events;
using FoldEngine.Gui;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Events;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Rendering;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Systems;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor;

[GameSystem("fold:editor.base", ProcessingCycles.All, true)]
public class EditorBase : GameSystem
{
    public bool InspectSelf = false;
    private long _currentSceneTabId = -1;
    private SubScene _nullSubScene;
    
    private EditorSceneTab _selfSceneTab;
    private EditorSceneTab _nullSceneTab = new EditorSceneTab();
    
    private Transform _selfCameraTransform;
    private Transform _nullTransform;
    
    [DoNotSerialize]
    public ComponentIterator<EditorSceneTab> TabIterator;
    private ComponentIterator<EditorSceneTabContainer> _tabContainerIter;

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
        _tabContainerIter = CreateComponentIterator<EditorSceneTabContainer>(IterationFlags.IncludeInactive);
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
        
        var loaderEntity = Scene.CreateEntity($"Scene Loader: {editedScene.Identifier}");
        ref var subScene = ref loaderEntity.AddComponent<SubScene>();
        subScene.Scene = editedScene;
        subScene.Render = true;
        subScene.Update = false;
        subScene.ProcessInputs = false;

        ref var loader = ref loaderEntity.AddComponent<EditorSceneTab>();
        loader.Scene = editedScene;
        loader.SceneTransactions = new TransactionManager<Scene>(editedScene);

        if (_currentSceneTabId == -1)
        {
            _currentSceneTabId = loaderEntity.EntityId;
        }
        else
        {
            loaderEntity.Hierarchical.Active = false;
        }
        
        var editorCameraEntity = Scene.CreateEntity("Editor Camera");
        editorCameraEntity.AddComponent<Camera>();
        editorCameraEntity.Hierarchical.SetParent(loaderEntity.EntityId);

        loader.EditorCameraEntityId = editorCameraEntity.EntityId;



        var tabEntity = Scene.CreateEntity($"Tab: {editedScene.Identifier}");
        tabEntity.SetComponent(new Control()
        {
            MinimumSize = new Vector2(0, 14)
        });
        tabEntity.SetComponent(new ButtonControl()
        {
            Text = editedScene.Identifier,
            Alignment = Alignment.Begin,
            Style = new ResourceIdentifier("editor:scene_tab"),
            KeepPressedOutside = true
        });
        tabEntity.SetComponent(new Tab()
        {
            DeselectedButtonStyle = "editor:scene_tab",
            SelectedButtonStyle = "editor:scene_tab.selected",
            LinkedEntityId = loaderEntity.EntityId
        });
        
        _tabContainerIter.Reset();
        while (_tabContainerIter.Next())
        {
            ref var tabContainer = ref _tabContainerIter.GetComponent();
            loaderEntity.Hierarchical.SetParent(_tabContainerIter.GetEntityId());

            long tabListId = tabContainer.TabListId;
            if (tabListId != -1)
            {
                tabEntity.Hierarchical.SetParent(tabListId);
            }
            break;
        }
    }

    public void CloseScene(long tabEntityId)
    {
        if (!Scene.Components.HasComponent<Tab>(tabEntityId)) return;
        ref var tab = ref Scene.Components.GetComponent<Tab>(tabEntityId);
        long sceneLoaderId = tab.LinkedEntityId;
        if (!Scene.Components.HasComponent<EditorSceneTab>(sceneLoaderId)) return;
        ref var sceneTabInfo = ref Scene.Components.GetComponent<EditorSceneTab>(sceneLoaderId);
        Scene.DeleteEntity(tabEntityId);
        Scene.DeleteEntity(sceneLoaderId);
        if(sceneTabInfo.EditorCameraEntityId != -1)
            Scene.DeleteEntity(sceneTabInfo.EditorCameraEntityId);
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

    public override void SubscribeToEvents()
    {
        Subscribe((ref TabSelectedEvent evt) =>
        {
            if (!Scene.Components.HasComponent<Tab>(evt.TabId)) return;
            ref var tab = ref Scene.Components.GetComponent<Tab>(evt.TabId);
            if (Scene.Components.HasComponent<EditorSceneTab>(tab.LinkedEntityId))
            {
                _currentSceneTabId = tab.LinkedEntityId;
            }
        });
    }
}