using System;
using System.Collections.Generic;

namespace FoldEngine.Resources {
    public class ResourceCollections {
        public ResourceCollections Parent;
        
        private Dictionary<Type, IResourceCollection> _collections = new Dictionary<Type, IResourceCollection>();

        public ResourceCollections() {
        }
        
        public ResourceCollections(ResourceCollections parent) {
            Parent = parent;
        }

        private ResourceCollection<T> CollectionFor<T>(bool createIfMissing = false) where T : Resource, new() {
            Type type = typeof(T);
            return (ResourceCollection<T>) (_collections.ContainsKey(type)
                ? _collections[type]
                : createIfMissing ? (_collections[type] = new ResourceCollection<T>()) : null);
        }

        public IResourceCollection FindOwner<T>(ref ResourceLocation location) where T : Resource, new() {
            Type type = typeof(T);
            return (_collections.ContainsKey(type) && _collections[type].Exists(ref location))
                ? _collections[type]
                : Parent?.FindOwner<T>(ref location);
        }

        public T Get<T>(ref ResourceLocation location, T def = default) where T : Resource, new() {
            IResourceCollection collection = FindOwner<T>(ref location);
            if(collection != null) {
                return ((ResourceCollection<T>) collection).Get(ref location, def);
            }

            return def;
        }

        public T Create<T>(string identifier) where T : Resource, new() {
            return CollectionFor<T>(true).Create(identifier);
        }
    }
}