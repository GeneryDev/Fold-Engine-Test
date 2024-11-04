using FoldEngine.Scenes;

namespace FoldEngine.Components;

public class ComponentReference<T> where T : struct
{
    private readonly long _entityId;
    private readonly Scene _scene;

    public ComponentReference(Scene scene, long entityId)
    {
        _scene = scene;
        _entityId = entityId;
    }

    public ref T Get()
    {
        return ref _scene.Components.GetComponent<T>(_entityId);
    }

    public bool Has()
    {
        return _scene.Components.HasComponent<T>(_entityId);
    }
}