using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Events;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Systems;

[GenericSerializable]
public abstract class GameSystem
{
    [DoNotSerialize] private readonly GameSystemAttribute _attribute;
    [DoNotSerialize] private readonly List<EventUnsubscriber> _eventUnsubscribers = new List<EventUnsubscriber>();

    protected GameSystem()
    {
        _attribute = (GameSystemAttribute)GetType().GetCustomAttribute(typeof(GameSystemAttribute));
    }

    [DoNotSerialize] public Scene Scene { get; internal set; }
    [DoNotSerialize] public string SystemName => _attribute.SystemName;
    [DoNotSerialize] public ProcessingCycles ProcessingCycles => _attribute.ProcessingCycles;
    [DoNotSerialize] public bool RunWhenPaused => _attribute.RunWhenPaused;

    public virtual void OnInput()
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnFixedUpdate()
    {
    }

    public virtual void OnRender(IRenderingUnit renderer)
    {
    }

    public virtual void PollResources()
    {
    }

    protected MultiComponentIterator CreateComponentIterator(params Type[] watchingTypes)
    {
        return Scene.Components.CreateMultiIterator(watchingTypes);
    }

    protected ComponentIterator CreateComponentIterator(Type watchingType, IterationFlags flags)
    {
        return Scene.Components.CreateIterator(watchingType, flags);
    }

    protected ComponentIterator<T> CreateComponentIterator<T>(IterationFlags flags) where T : struct
    {
        return Scene.Components.CreateIterator<T>(flags);
    }

    public virtual void Initialize()
    {
    }

    public virtual void SubscribeToEvents()
    {
    }


    internal void UnsubscribeFromEvents()
    {
        foreach (EventUnsubscriber obj in _eventUnsubscribers) obj.Unsubscribe();
        _eventUnsubscribers.Clear();
    }

    protected void Subscribe<T>(EventListener<T> action) where T : struct
    {
        Subscribe(Scene.Events, action);
    }

    protected void Subscribe<T>(Scene scene, EventListener<T> action) where T : struct
    {
        Subscribe(scene.Events, action);
    }

    protected void Subscribe<T>(EventMap eventMap, EventListener<T> action) where T : struct
    {
        _eventUnsubscribers.Add(eventMap.Subscribe(action));
    }
}

[Flags]
public enum ProcessingCycles
{
    None = 0,
    Input = 1,
    FixedUpdate = 2,
    Update = 4,
    Render = 8,
    All = Input | FixedUpdate | Update | Render
}

public static class ProcessingCyclesExt
{
    public static bool Has(this ProcessingCycles a, ProcessingCycles flag)
    {
        return ((uint)a & (uint)flag) == (uint)flag;
    }
}

public sealed class GameSystemAttribute : Attribute
{
    public readonly ProcessingCycles ProcessingCycles;
    public readonly bool RunWhenPaused;
    public readonly string SystemName;

    public GameSystemAttribute(string identifier, ProcessingCycles processingCycles, bool runWhenPaused = false)
    {
        SystemName = identifier;
        ProcessingCycles = processingCycles;
        RunWhenPaused = runWhenPaused;
    }
}

public sealed class ListeningAttribute : Attribute
{
    public readonly Type[] EventTypes;

    public ListeningAttribute(params Type[] eventTypes)
    {
        EventTypes = eventTypes;
    }
}