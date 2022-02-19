using System;
using System.Collections.Generic;
using EntryProject.Util;

namespace FoldEngine.Resources {

    public interface IResourceCollection {
        bool Exists(ref ResourceLocation location);
    }

    public class ResourceCollection<T> : IResourceCollection where T : Resource, new() {
        
        // Counter that increments each time any resource inside this collection changes position in the Resources array.
        // Compare this against the generation in resource locations to determine whether the index needs to be
        // recalculated from the identifier string or not.
        protected int Generation = 1;

        protected readonly List<T> Resources = new List<T>();

        private int IndexForIdentifier(string identifier) {
            Console.WriteLine("Retrieving index for identifier '" + identifier + "'");
            for(int i = 0; i < Resources.Count; i++) {
                if(Resources[i].Identifier == identifier) return i;
            }

            return -1;
        }

        private void UpdateResourceLocation(ref ResourceLocation location) {
            if(!location.IndexIntoCollection.IsValid(Generation)) {
                int indexIntoCollection = location.IndexIntoCollection.Get(Generation) - 1;
                if(indexIntoCollection == -1) {
                    indexIntoCollection = IndexForIdentifier(location.Identifier);
                    location.IndexIntoCollection.Set(indexIntoCollection + 1, Generation);
                }
            }
        }

        public T Get(ref ResourceLocation location, T def = null) {
            if(location.Identifier == null) return def;
            UpdateResourceLocation(ref location);
            
            int indexIntoCollection = location.IndexIntoCollection.Get(Generation) - 1;
            return indexIntoCollection != -1 ? Resources[indexIntoCollection] : def;
        }

        public bool Exists(ref ResourceLocation location) {
            if(location.Identifier == null) return false;
            UpdateResourceLocation(ref location);
            
            return location.IndexIntoCollection.Get(Generation) != -1;
        }

        public T Create(string identifier) {
            int existingIndex = IndexForIdentifier(identifier);
            if(existingIndex != -1) throw new ArgumentException("Resource '" + identifier + "' already exists!");
            var newT = new T();
            newT.Identifier = identifier;
            Resources.Add(newT);
            Generation++;
            return newT;
        }

        public void Unload(T t) {

            Generation++;
        }
    }

    public struct ResourceLocation {
        public string Identifier;
        
        public CachedValue<int> IndexIntoCollection;

        public ResourceLocation(string identifier) {
            Identifier = identifier;
            IndexIntoCollection = default;
        }
    }
    
    public abstract class Resource {
        public string Identifier { get; protected internal set; }
        protected internal CachedValue<int> SystemsKeepingAlive;
    }
}