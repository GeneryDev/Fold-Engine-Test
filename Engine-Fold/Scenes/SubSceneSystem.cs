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

    public override void Initialize()
    {
        _subSceneComponents = CreateComponentIterator<SubScene>(IterationFlags.Ordered);
    }

    public override void OnInput()
    {
        _subSceneComponents.Reset();
        while (_subSceneComponents.Next())
        {
            ref var instance = ref _subSceneComponents.GetComponent();
            if(!_subSceneComponents.HasCoComponent<InactiveComponent>())
                instance.Scene?.Input();
        }
    }

    public override void OnUpdate()
    {
        _subSceneComponents.Reset();
        while (_subSceneComponents.Next())
        {
            ref var instance = ref _subSceneComponents.GetComponent();
            if(!_subSceneComponents.HasCoComponent<InactiveComponent>())
                instance.Scene?.Update();
        }
    }

    public override void OnFixedUpdate()
    {
        _subSceneComponents.Reset();
        while (_subSceneComponents.Next())
        {
            ref var component = ref _subSceneComponents.GetComponent();
            
            if (component.SceneIdentifier.Identifier != component.LoadedSceneIdentifier.Identifier)
            {
                // Component changed scene identifier, discard scene and reset instance
                DiscardScene(component.Scene);
                component.LoadedSceneIdentifier = default;
                component.Scene = null;
            }
            
            if (component.LoadedSceneIdentifier.Identifier == null)
            {
                component.LoadedSceneIdentifier = component.SceneIdentifier;
            }

            if (component.Scene == null && Scene.Resources.Get<Scene>(ref component.LoadedSceneIdentifier, null) is {} loadedScene)
            {
                component.Scene = loadedScene;
                Scene.Resources.Detach(component.Scene);
                Scene.Core.Resources.Detach(component.Scene);
            }
        }
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        _subSceneComponents.Reset();
        while (_subSceneComponents.Next())
        {
            ref var instance = ref _subSceneComponents.GetComponent();
            if(!_subSceneComponents.HasCoComponent<InactiveComponent>())
                instance.Scene?.Render(renderer);
        }
    }

    public override void PollResources()
    {
        _subSceneComponents.Reset();
        while (_subSceneComponents.Next())
        {
            ref var instance = ref _subSceneComponents.GetComponent();
            instance.Scene?.Systems.PollResources();
        }
    }

    private void DiscardScene(Scene scene)
    {
        if (scene == null) return;
        Console.WriteLine($"Discard scene: {scene.Identifier}");
    }

    public override void SubscribeToEvents()
    {
        base.SubscribeToEvents();
        Subscribe((ref ComponentRemovedEvent<SubScene> evt) =>
        {
            DiscardScene(evt.Component.Scene);
        });
    }
}