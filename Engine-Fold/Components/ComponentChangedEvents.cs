using FoldEngine.Events;

namespace FoldEngine.Components;

[Event("fold:component.added")]
public struct ComponentAddedEvent<T> where T : struct
{
    public long EntityId;
    public T Component;
}
[Event("fold:component.removed")]
public struct ComponentRemovedEvent<T> where T : struct
{
    public long EntityId;
    public T Component;
}