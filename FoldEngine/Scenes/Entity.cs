using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Components;

namespace FoldEngine.Scenes
{
    public class Entity
    {
        public readonly Scene Scene;
        internal long EntityId;

        public Transform Transform => GetComponent<Transform>();

        public Entity(Scene scene, long entityId)
        {
            Scene = scene;
            EntityId = entityId;
        }

        public T GetComponent<T>() where T : struct
        {
            return Scene.Components.GetComponent<T>(EntityId);
        }

        public T AddComponent<T>() where T : struct
        {
            return Scene.Components.CreateComponent<T>(EntityId);
        }

        public void RemoveComponent<T>() where T : struct
        {
            Scene.Components.RemoveComponent<T>(EntityId);
        }






        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj is Entity ent) {
                return ent.EntityId == this.EntityId && ent.Scene == this.Scene;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return EntityId.GetHashCode() + 16777216 * (Scene.GetHashCode());
        }
    }
}
