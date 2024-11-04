using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Registries;

namespace FoldEngine.Systems;

public partial class SystemRegistry : IRegistry
{
    private Dictionary<Type, string> _typeToIdentifierMap = new();
    private Dictionary<string, Type> _identifierToTypeMap = new();
    private Dictionary<string, ConstructorInfo> _identifierToConstructorMap = new();

    public string IdentifierOf(Type type)
    {
        if (_typeToIdentifierMap.TryGetValue(type, out string value)) return value;

        throw new ArgumentException($"Type '{type}' is not a game system type");
    }

    public string IdentifierOf<T>() where T : struct
    {
        return IdentifierOf(typeof(T));
    }

    public Type TypeForIdentifier(string identifier)
    {
        return _identifierToTypeMap[identifier];
    }

    public GameSystem CreateForIdentifier(string identifier)
    {
        return (GameSystem)_identifierToConstructorMap[identifier].Invoke(Array.Empty<object>());
    }

    public GameSystem CreateForType(Type type)
    {
        return CreateForIdentifier(IdentifierOf(type));
    }

    public IEnumerable<Type> GetAllTypes()
    {
        return _identifierToTypeMap.Values;
    }

    public void AcceptType(Type type)
    {
        if (!type.IsSubclassOf(typeof(GameSystem))) return;
        if (type.GetCustomAttribute<GameSystemAttribute>(false) is { } attribute)
        {
            string thisIdentifier = attribute.SystemName;
            _identifierToTypeMap[thisIdentifier] = type;
            _typeToIdentifierMap[type] = thisIdentifier;
            _identifierToConstructorMap[thisIdentifier] = type.GetConstructor(Type.EmptyTypes);
        }
    }
}