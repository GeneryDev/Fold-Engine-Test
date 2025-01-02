using FoldEngine.Components;

namespace FoldEngine.Scenes.Prefabs;

[Component("fold:from_prefab")]
public struct FromPrefab
{
    [EntityId] public long PrefabInstanceId;
}