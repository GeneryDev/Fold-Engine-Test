using System;
using FoldEngine.Serialization;
using FoldEngine.Util;

namespace FoldEngine.Resources;

public struct ResourceIdentifier
{
    [DoNotSerialize] public string Identifier;

    [DoNotSerialize] public CachedValue<int> IndexIntoCollection;

    public ResourceIdentifier(string identifier)
    {
        Identifier = identifier;
        IndexIntoCollection = default;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class ResourceIdentifierAttribute : Attribute
{
    public Type Type;

    public ResourceIdentifierAttribute(Type type)
    {
        Type = type;
    }
}