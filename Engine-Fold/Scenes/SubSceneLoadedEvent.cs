using FoldEngine.Events;

namespace FoldEngine.Scenes;

[Event("fold:sub_scene.loaded")]
public struct SubSceneLoadedEvent
{
    public Entity Entity;
    public Scene Scene;
}

[Event("fold:sub_scene.unloaded")]
public struct SubSceneUnloadedEvent
{
    public Entity Entity;
    public Scene Scene;
}