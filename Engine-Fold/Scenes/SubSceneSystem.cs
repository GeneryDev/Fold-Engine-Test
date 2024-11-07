using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Systems;

namespace FoldEngine.Scenes;

[GameSystem("fold:sub_scenes", ProcessingCycles.All, true)]
public class SubSceneSystem : GameSystem
{
    private ComponentIterator<SubScene> _subSceneComponents;

    [HideInInspector] private List<SubSceneInstance> _instances = new();

    public override void Initialize()
    {
        _subSceneComponents = CreateComponentIterator<SubScene>(IterationFlags.Ordered);
    }

    private int GetInstanceIndexForEntity(long entityId)
    {
        for (var i = 0; i < _instances.Count; i++)
        {
            if (_instances[i].EntityId == entityId)
            {
                //this is it
                return i;
            }
        }

        return -1;
    }

    private int GetOrCreateInstanceIndexForEntity(long entityId)
    {
        int instanceIndex = GetInstanceIndexForEntity(entityId);
        if (instanceIndex == -1)
        {
            instanceIndex = _instances.Count;
            _instances.Add(new SubSceneInstance() {EntityId = entityId});
        }

        return instanceIndex;
    }
    
    public override void OnInput()
    {
        foreach (var instance in _instances)
        {
            if(!Scene.Components.HasComponent<InactiveComponent>(instance.EntityId))
                instance.Scene?.Input();
        }
    }

    public override void OnUpdate()
    {
        foreach (var instance in _instances)
        {
            if(!Scene.Components.HasComponent<InactiveComponent>(instance.EntityId))
                instance.Scene?.Update();
        }
    }

    public override void OnFixedUpdate()
    {
        for (int i = 0; i < _instances.Count; i++)
        {
            var instance = _instances[i];
            if (!Scene.Components.HasComponent<SubScene>(instance.EntityId))
            {
                // Component removed, discard scene and remove instance
                DiscardScene(instance.Scene);
                _instances.RemoveAt(i);
                i--;
                continue;
            } else if (Scene.Components.GetComponent<SubScene>(instance.EntityId) is { } component && component.SceneIdentifier.Identifier != instance.SceneIdentifier.Identifier)
            {
                // Component changed scene identifier, discard scene and reset instance
                DiscardScene(instance.Scene);
                _instances[i] = instance with { SceneIdentifier = default, Scene = null };
            }
        }
        
        _subSceneComponents.Reset();
        while (_subSceneComponents.Next())
        {
            long entityId = _subSceneComponents.GetEntityId();
            var component = _subSceneComponents.GetComponent();
            
            int instanceIndex = GetOrCreateInstanceIndexForEntity(entityId);
            var instance = _instances[instanceIndex];

            if (instance.SceneIdentifier.Identifier == null)
            {
                instance.SceneIdentifier = component.SceneIdentifier;
                _instances[instanceIndex] = instance;
            }

            if (instance.Scene == null && Scene.Resources.Get<Scene>(ref instance.SceneIdentifier, null) is {} loadedScene)
            {
                instance.Scene = loadedScene;
                Scene.Resources.Detach(instance.Scene);
                Scene.Core.Resources.Detach(instance.Scene);
                _instances[instanceIndex] = instance;
            }
        }
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        foreach (var instance in _instances)
        {
            if(!Scene.Components.HasComponent<InactiveComponent>(instance.EntityId))
                instance.Scene?.Render(renderer);
        }
    }

    public override void PollResources()
    {
        foreach (var instance in _instances)
        {
            instance.Scene?.Systems.PollResources();
        }
    }

    private void DiscardScene(Scene scene)
    {
        if (scene == null) return;
        Console.WriteLine($"Discard scene: {scene.Identifier}");
    }

    public Scene GetSceneForEntityId(long entityId)
    {
        int instanceIndex = GetInstanceIndexForEntity(entityId);
        if (instanceIndex >= 0) return _instances[instanceIndex].Scene;
        return null;
    }

    private struct SubSceneInstance
    {
        public long EntityId;
        public ResourceIdentifier SceneIdentifier;
        public Scene Scene;
    }
}