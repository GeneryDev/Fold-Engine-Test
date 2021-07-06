using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Components;

namespace FoldEngine.Scenes {
    public class EntityIdAttribute : Attribute { }

    public struct Entity {
        public readonly Scene Scene;
        public readonly long EntityId;

        public ref Transform Transform => ref GetComponent<Transform>();

        public string Name {
            get => GetComponent<EntityName>().Name;
            set {
                ref EntityName component = ref GetComponent<EntityName>();
                component.Name = value;
            }
        }

        public Entity(Scene scene, long entityId) {
            Scene = scene;
            EntityId = entityId;
        }

        public ref T GetComponent<T>() where T : struct {
            return ref Scene.Components.GetComponent<T>(EntityId);
        }

        public ref T AddComponent<T>() where T : struct {
            return ref Scene.Components.CreateComponent<T>(EntityId);
        }

        public void RemoveComponent<T>() where T : struct {
            Scene.Components.RemoveComponent<T>(EntityId);
        }
    }
}
