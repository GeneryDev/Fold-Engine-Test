using System.Collections.Generic;

namespace FoldEngine.Scenes;

public class EntityIdRemapper
{
    private readonly Dictionary<long, long> _map = new Dictionary<long, long>();
    private readonly Scene _scene;
    public bool CreateNewIfNotPresent = true;

    public EntityIdRemapper(Scene scene = null)
    {
        _scene = scene;
    }

    public long TransformId(long oldId)
    {
        if (!_map.ContainsKey(oldId))
        {
            if (CreateNewIfNotPresent)
                return _map[oldId] = _scene.CreateEntityId("a");
            return oldId;
        }

        return _map[oldId];
    }
}