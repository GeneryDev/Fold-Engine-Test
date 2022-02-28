using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.IO;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Resources {

    public interface IResourceCollection : ISelfSerializer {
        bool Exists(ref ResourceLocation location);
        bool IsEmpty { get; }
        Type ResourceType { get; }
        void Save();
    }

    public class ResourceCollection<T> : IResourceCollection where T : Resource, new() {
        
        // Counter that increments each time any resource inside this collection changes position in the Resources array.
        // Compare this against the generation in resource locations to determine whether the index needs to be
        // recalculated from the identifier string or not.
        protected int Generation = 1;

        public Type ResourceType => typeof(T);

        protected readonly List<T> Resources = new List<T>();

        public bool IsEmpty => Resources.Count == 0;

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

        public void Serialize(SaveOperation writer) {
            writer.WriteCompound((ref SaveOperation.Compound c) => {
                foreach(T entry in Resources) {
                    c.WriteMember(entry.Identifier, () => {
                        GenericSerializer.Serialize(entry, writer);
                    });
                }
            });
        }

        public void Deserialize(LoadOperation reader) {
            reader.ReadCompound(c => {
                foreach(string identifier in c.MemberNames) {
                    T newT = Create(identifier);
                    c.StartReadMember(identifier);
                    GenericSerializer.Deserialize(newT, reader);
                }
            });
        }
        
        public void Save() {
            foreach(T resource in Resources) {
                resource.Save();
            }
        }
    }

    public struct ResourceLocation {
        [DoNotSerialize]
        public string Identifier;
        [DoNotSerialize]
        public CachedValue<int> IndexIntoCollection;

        public ResourceLocation(string identifier) {
            Identifier = identifier;
            IndexIntoCollection = default;
        }
    }
    
    public abstract class Resource {
        public const string Extension = "foldresource";
        
        public string Identifier { get; protected internal set; }
        protected internal CachedValue<int> SystemsKeepingAlive;
        
        private static readonly Dictionary<Type, ConstructorInfo> Constructors = new Dictionary<Type, ConstructorInfo>();
        
        public static IResourceCollection CreateCollectionForType(Type resourceType) {
            if(!Constructors.ContainsKey(resourceType)) {
                Constructors[resourceType] =
                    typeof(ResourceCollection<>).MakeGenericType(resourceType).GetConstructor(new Type[0]);
            }

            return (IResourceCollection) Constructors[resourceType].Invoke(new object[0]);
        }


        public void Save() {
            string resourceFolder = Path.Combine("resources", AttributeOf(GetType()).DirectoryName);
            string path = Path.Combine(resourceFolder, Identifier);
            path = Path.ChangeExtension(path, Extension);
            Save(path);
        }

        public void Save(string path) {
            var writer = new SaveOperation(Data.Out.Stream(path));
            GenericSerializer.Serialize(this, writer);
            writer.Close();            
        }
        
        private static Dictionary<Type, ResourceAttribute> _attributes;

        private static void Populate() {
            if(_attributes != null) return;
            _attributes = new Dictionary<Type, ResourceAttribute>();
            
            var assembly = Assembly.GetEntryAssembly();
            foreach(Type type in assembly.GetTypes()) {
                if(type.IsSubclassOf(typeof(Resource))) {
                    _attributes[type] = type.GetCustomAttribute<ResourceAttribute>() ?? new ResourceAttribute(type.Name.ToLowerInvariant());
                }
            }
        }

        public static ResourceAttribute AttributeOf(Type type) {
            Populate();
            return _attributes[type];
        }

        public static ResourceAttribute AttributeOf<T>() where T : Resource {
            return AttributeOf(typeof(T));
        }

        public static Dictionary<Type, ResourceAttribute>.KeyCollection GetAllTypes() {
            Populate();
            return _attributes.Keys;
        }
    }

    public sealed class ResourceAttribute : Attribute {
        public readonly string DirectoryName;

        public ResourceAttribute(string directoryName) {
            DirectoryName = directoryName;
        }
    }
}