using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Registries;

namespace FoldEngine.Events;

public class EventRegistry : IRegistry
{
    private readonly Dictionary<Type, string> _typeToIdentifierMap = new();
    private readonly Dictionary<string, Type> _identifierToTypeMap = new();

    public string IdentifierOf(Type type) {
        if (_typeToIdentifierMap.TryGetValue(type, out string value)) return value;

        throw new ArgumentException($"Type '{type}' is not an event type");
    }

    public string IdentifierOf<T>() where T : struct {
        return IdentifierOf(typeof(T));
    }

    public Type TypeForIdentifier(string identifier) {
        return _identifierToTypeMap[identifier];
    }

    public void AcceptType(Type type)
    {
        if (!type.IsValueType) return;

        if (type.GetCustomAttribute<EventAttribute>(false) is { } attribute)
        {
            string thisIdentifier = attribute.EventIdentifier;
            _identifierToTypeMap[thisIdentifier] = type;
            _typeToIdentifierMap[type] = thisIdentifier;
        }
    }
}