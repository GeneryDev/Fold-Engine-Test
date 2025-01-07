using FoldEngine.Events;

namespace FoldEngine.Scenes.Prefabs;

[Event("fold:prefab_instance_request", EventFlushMode.End)]
public struct PrefabInstanceUpdateRequestEvent
{
    [EntityId] public long EntityId;
}