using FoldEngine.Components;
using FoldEngine.Resources;

namespace FoldEngine.Scenes.Prefabs;

[Component("fold:prefab")]
public struct Prefab
{
    [ResourceIdentifier(typeof(PackedScene))] public ResourceIdentifier Identifier;

    public PrefabLoadMode LoadMode;
}

public enum PrefabLoadMode
{
    Replace,
    AsChild
}