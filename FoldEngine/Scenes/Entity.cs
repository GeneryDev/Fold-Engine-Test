using System;
using FoldEngine.Components;

namespace FoldEngine.Scenes {
    public class EntityIdAttribute : Attribute { }

    public struct Entity {
        public readonly Scene Scene;
        public readonly long EntityId;

        public ref Transform Transform => ref GetComponent<Transform>();

        public bool Active {
            get => !HasComponent<InactiveComponent>();
            set {
                if(value) {
                    if(HasComponent<InactiveComponent>()) RemoveComponent<InactiveComponent>();
                } else {
                    if(!HasComponent<InactiveComponent>()) AddComponent<InactiveComponent>();
                }
            }
        }

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

        public bool HasComponent<T>() where T : struct {
            return Scene.Components.HasComponent<T>(EntityId);
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

        public void Delete() {
            Scene.DeleteEntity(EntityId);
        }

        public bool IsAncestorOf(Entity other) {
            if(!ReferenceEquals(Scene, other.Scene)) return false;
            while(other.Transform.HasParent) {
                other = new Entity(Scene, other.Transform.ParentId);
                if(other.EntityId == EntityId) return true;
            }

            return false;
        }

        public bool Equals(Entity other) {
            return ReferenceEquals(Scene, other.Scene)
                   && EntityId == other.EntityId;
        }

        public override bool Equals(object obj) {
            return obj is Entity other && Equals(other);
        }

        public override int GetHashCode() {
            return (int) EntityId;
        }

        public static bool operator ==(Entity a, Entity b) {
            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b) {
            return !a.Equals(b);
        }
    }
}