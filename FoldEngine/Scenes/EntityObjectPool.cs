using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine.Scenes
{
    internal class EntityObjectPool
    {
        private readonly Scene _scene;
        private readonly List<Entity> _entityObjects = new List<Entity>();

        public EntityObjectPool(Scene scene) => this._scene = scene;

        private Entity InstantiateEntityObject(long entityId)
        {
            return new Entity(_scene, entityId);
        }

        internal Entity GetOrCreateEntityObject(long entityId)
        {
            if (_entityObjects.Count == 0)
            {
                Entity ent = InstantiateEntityObject(entityId);
                _entityObjects.Add(ent);
                return ent;
            }

            int minIndex = 0; // inclusive
            int maxIndex = _entityObjects.Count; // exclusive

            if(entityId < _entityObjects[minIndex].EntityId)
            {
                //Entity ID is lower than the lower bound
                Entity ent = InstantiateEntityObject(entityId);
                _entityObjects.Insert(0, ent);
                return ent;
            }
            if (entityId > _entityObjects[maxIndex - 1].EntityId)
            {
                //Entity ID is greater than the upper bound
                Entity ent = InstantiateEntityObject(entityId);
                _entityObjects.Add(ent);
                return ent;
            }

            while (minIndex < maxIndex)
            {
                int pivotIndex = (minIndex + maxIndex) / 2;

                long pivotId = _entityObjects[pivotIndex].EntityId;
                if (pivotId == entityId)
                {
                    return _entityObjects[pivotIndex];
                }
                else if (entityId > pivotId)
                {
                    minIndex = pivotIndex + 1;
                }
                else
                {
                    maxIndex = pivotIndex;
                }
            }
            {
                Entity ent = InstantiateEntityObject(entityId);
                _entityObjects.Insert(minIndex, ent);
                return ent;
            }
        }
    }
}
