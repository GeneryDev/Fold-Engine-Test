using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Editor.Inspector.CustomInspectors;
using FoldEngine.Events;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Systems;

namespace FoldEngine.Registries;

public class RegistryUnit
{
    public IGameCore Core { get; }
    public ComponentRegistry Components { get; }
    public SystemRegistry Systems { get; }
    public EventRegistry Events { get; }
    public ResourceRegistry Resources { get; }
    public CustomInspectorRegistry CustomInspectors { get; }

    private readonly List<Assembly> _populatedAssemblies = [];
    
    public RegistryUnit(IGameCore core)
    {
        Core = core;
        
        Components = new ComponentRegistry();
        Systems = new SystemRegistry();
        Events = new EventRegistry();
        Resources = new ResourceRegistry();
        CustomInspectors = new CustomInspectorRegistry();
    }

    public void Initialize()
    {
        PopulateFromAssembly(Assembly.GetAssembly(typeof(RegistryUnit)));
        PopulateFromAssembly(Assembly.GetAssembly(Core.GetType()));
        PopulateFromAssembly(Assembly.GetEntryAssembly());

        if (Core.FoldGame.RuntimeConfig.AdditionalAssembliesForReflection is { } additionalAssemblies)
        {
            foreach (var assembly in additionalAssemblies)
            {
                PopulateFromAssembly(assembly);
            }
        }
    }

    private void PopulateFromAssembly(Assembly assembly)
    {
        if (assembly == null) return;
        if (_populatedAssemblies.Contains(assembly)) return;
        _populatedAssemblies.Add(assembly);
        
        foreach(var type in assembly.GetTypes())
        {
            AcceptType(type);
        }
    }

    private void AcceptType(Type type)
    {
        Components.AcceptType(type);
        Systems.AcceptType(type);
        Events.AcceptType(type);
        Resources.AcceptType(type);
        CustomInspectors.AcceptType(type);
    }
}