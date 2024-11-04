using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Gui;
using FoldEngine.Registries;

namespace FoldEngine.Editor.Inspector.CustomInspectors;

public class CustomInspectorRegistry : IRegistry
{
    private Dictionary<Type, List<ICustomInspector>> _inspectors = new();

    public void AcceptType(Type type)
    {
        if (typeof(ICustomInspector).IsAssignableFrom(type))
        {
            if (type.GetCustomAttribute<CustomInspectorAttribute>() is { } attribute)
            {
                if (!_inspectors.ContainsKey(attribute.Type))
                    _inspectors[attribute.Type] = new List<ICustomInspector>();
                _inspectors[attribute.Type]
                    .Add((ICustomInspector)type.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
            }
        }
    }

    public void RenderCustomInspectorsBefore(object obj, GuiPanel panel)
    {
        RenderCustomInspectors(obj, obj.GetType(), panel, after: false);
    }

    public void RenderCustomInspectorsAfter(object obj, GuiPanel panel)
    {
        RenderCustomInspectors(obj, obj.GetType(), panel, after: true);
    }

    private void RenderCustomInspectors(object obj, Type type, GuiPanel panel, bool after)
    {
        if (type == null) return;
        RenderCustomInspectors(obj, type.BaseType, panel, after);
        if (!_inspectors.ContainsKey(type)) return;

        foreach (ICustomInspector inspector in _inspectors[type])
        {
            if (after)
            {
                inspector.RenderInspectorAfter(obj, panel);
            }
            else
            {
                inspector.RenderInspectorBefore(obj, panel);
            }
        }
    }
}