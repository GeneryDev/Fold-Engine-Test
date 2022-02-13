using System;
using System.Collections.Generic;

namespace FoldEngine.Resources {

    public interface IResourceCollection {
        bool Exists(ref ResourceLocation location);
    }

    public class ResourceCollection<T> : IResourceCollection where T : Resource, new() {
        
        // Counter that increments each time any resource inside this collection changes position in the _units array.
        // Compare this against the generation in resource locations to determine whether the index needs to be
        // recalculated from the name string or not. 
        protected int Generation = 0;

        protected readonly Dictionary<string, T> Resources = new Dictionary<string, T>();

        // public T this[string key] => Resources[key];

        public ResourceCollection() {
        }

        public T Get(ref ResourceLocation location, T def = null) {
            return Resources[location.Identifier] ?? def;
        }

        public bool Exists(ref ResourceLocation location) {
            return Resources.ContainsKey(location.Identifier);
        }

        public T Create(string identifier) {
            var newT = new T();
            Resources[identifier] = newT;
            return newT;
        }
    }

    public struct ResourceLocation {
        public string Identifier;
        
        internal int IndexIntoCollection;
        internal int Generation;

        public ResourceLocation(string identifier) {
            Identifier = identifier;
            IndexIntoCollection = 0;
            Generation = 0;
        }
    }
    
    public abstract class Resource {
    
    }
}