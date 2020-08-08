using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine.Scenes
{
    internal class EntityObjectPool
    {
        private readonly Scene Scene;
        private readonly List<Entity> EntityObjects = new List<Entity>();

        public EntityObjectPool(Scene scene) => this.Scene = scene;

        private Entity InstantiateEntityObject(long entityId)
        {
            return new Entity(Scene, entityId);
        }

        internal Entity GetOrCreateEntityObject(long entityId)
        {
            if (EntityObjects.Count == 0)
            {
                Entity ent = InstantiateEntityObject(entityId);
                EntityObjects.Add(ent);
                return ent;
            }

            int minIndex = 0; // inclusive
            int maxIndex = EntityObjects.Count; // exclusive

            if(entityId < EntityObjects[minIndex].EntityId)
            {
                //Entity ID is lower than the lower bound
                Entity ent = InstantiateEntityObject(entityId);
                EntityObjects.Insert(0, ent);
                return ent;
            }
            if (entityId > EntityObjects[maxIndex - 1].EntityId)
            {
                //Entity ID is greater than the upper bound
                Entity ent = InstantiateEntityObject(entityId);
                EntityObjects.Add(ent);
                return ent;
            }

            while (minIndex < maxIndex)
            {
                int pivotIndex = (minIndex + maxIndex) / 2;

                long pivotId = EntityObjects[pivotIndex].EntityId;
                if (pivotId == entityId)
                {
                    return EntityObjects[pivotIndex];
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
                EntityObjects.Insert(minIndex, ent);
                return ent;
            }
        }
    }
}
