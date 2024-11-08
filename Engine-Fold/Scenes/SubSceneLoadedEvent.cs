using FoldEngine.Events;

namespace FoldEngine.Scenes;

[Event("fold:sub_scene.loaded")]
public class SubSceneLoadedEvent
{
    public Entity Entity;
    public Scene Scene;
}

[Event("fold:sub_scene.unloaded")]
public class SubSceneUnloadedEvent
{
    public Entity Entity;
    public Scene Scene;
}