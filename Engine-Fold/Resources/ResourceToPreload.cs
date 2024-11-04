using System;
using FoldEngine.Components;
using FoldEngine.Serialization;

namespace FoldEngine.Resources;

[Component(identifier: "fold:resource_to_preload")]
public struct ResourceToPreload
{
    public ResourceIdentifier Identifier;
    public string Type;
    [DoNotSerialize] public Type CachedType;
}