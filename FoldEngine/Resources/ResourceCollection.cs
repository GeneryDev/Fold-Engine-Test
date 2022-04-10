using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.IO;
using FoldEngine.Serialization;

namespace FoldEngine.Resources {
    public interface IResourceCollection : ISelfSerializer {
        bool IsEmpty { get; }
        Type ResourceType { get; }
        bool Exists(ResourceIdentifier identifier);
        void Save();
        void InvalidateCaches();
        void Attach(Resource resource);
        void Detach(Resource resource);
        void UnloadUnused();
    }

    public class ResourceCollection<T> : IResourceCollection where T : Resource, new() {
        protected readonly List<T> Resources = new List<T>();

        // Counter that increments each time any resource inside this collection changes position in the Resources array.
        // Compare this against the generation in resource locations to determine whether the index needs to be
        // recalculated from the identifier string or not.
        protected int Generation = 1;

        public Type ResourceType => typeof(T);

        public bool IsEmpty => Resources.Count == 0;

        public bool Exists(ResourceIdentifier identifier) {
            if(identifier.Identifier == null) return false;
            UpdateResourceIdentifier(ref identifier);

            return identifier.IndexIntoCollection.Get(Generation) - 1 != -1;
        }

        public void Attach(Resource resource) {
            Attach((T) resource);
        }

        public void Detach(Resource resource) {
            Detach((T) resource);
        }

        public void UnloadUnused() {
            int unloadTime = Resource.AttributeOf<T>()?.UnloadTime ?? 5000;
            for(int i = 0; i < Resources.Count; i++) {
                T resource = Resources[i];
                if(Time.Now - unloadTime >= resource.LastAccessTime)
                    if(resource.Unload()) {
                        Console.WriteLine("UNLOADING UNUSED RESOURCE " + resource.Identifier);
                        Resources.RemoveAt(i);
                        i--;
                    }
            }

            InvalidateCaches();
        }

        public void InvalidateCaches() {
            Generation++;
        }

        public void Serialize(SaveOperation writer) {
            writer.WriteCompound((ref SaveOperation.Compound c) => {
                foreach(T entry in Resources)
                    if(entry.CanSerialize)
                        c.WriteMember(entry.Identifier, () => { entry.SerializeResource(writer); });
            });
        }

        public void Deserialize(LoadOperation reader) {
            reader.ReadCompound(c => {
                foreach(string identifier in c.MemberNames) {
                    c.StartReadMember(identifier);
                    Create(identifier, reader);
                }
            });
        }

        public void Save() {
            foreach(T resource in Resources) resource.Save();
        }

        private int IndexForIdentifier(string identifier) {
            // Console.WriteLine("Retrieving index for identifier '" + identifier + "'");
            for(int i = 0; i < Resources.Count; i++)
                if(Resources[i].Identifier == identifier)
                    return i;

            return -1;
        }

        private void UpdateResourceIdentifier(ref ResourceIdentifier identifier) {
            if(!identifier.IndexIntoCollection.IsValid(Generation)) {
                int indexIntoCollection = identifier.IndexIntoCollection.Get(Generation) - 1;
                if(indexIntoCollection == -1) {
                    indexIntoCollection = IndexForIdentifier(identifier.Identifier);
                    identifier.IndexIntoCollection.Set(indexIntoCollection + 1, Generation);
                }
            }
        }

        public T Get(ref ResourceIdentifier identifier, T def = null) {
            if(identifier.Identifier == null) return def;
            UpdateResourceIdentifier(ref identifier);

            int indexIntoCollection = identifier.IndexIntoCollection.Get(Generation) - 1;
            if(indexIntoCollection != -1) {
                T resource = Resources[indexIntoCollection];
                resource.Access();
                return resource;
            }

            return def;
        }

        public T Create(string identifier, LoadOperation reader = null) {
            int existingIndex = IndexForIdentifier(identifier);
            if(existingIndex != -1) throw new ArgumentException("Resource '" + identifier + "' already exists!");
            var newT = new T {Identifier = identifier};
            if(reader != null) newT.DeserializeResource(reader);
            Resources.Add(newT);
            InvalidateCaches();
            return newT;
        }

        public void Attach(T resource) {
            string identifier = resource.Identifier;
            int existingIndex = IndexForIdentifier(identifier);
            if(existingIndex != -1) throw new ArgumentException("Resource '" + identifier + "' already exists!");

            Resources.Add(resource);
            InvalidateCaches();
        }

        public void Detach(T resource) {
            string identifier = resource.Identifier;
            int existingIndex = IndexForIdentifier(identifier);
            if(existingIndex != -1) {
                Resources.RemoveAt(existingIndex);
                InvalidateCaches();
            }
        }
    }

    public struct ResourceIdentifier {
        [DoNotSerialize]
        public string Identifier;

        [DoNotSerialize]
        public CachedValue<int> IndexIntoCollection;

        public ResourceIdentifier(string identifier) {
            Identifier = identifier;
            IndexIntoCollection = default;
        }
    }

    public abstract class Resource {
        private static readonly Dictionary<Type, ConstructorInfo>
            Constructors = new Dictionary<Type, ConstructorInfo>();

        private static Dictionary<Type, ResourceAttribute> _attributes;
        protected internal long LastAccessTime = Time.Now;
        public string Identifier { get; protected internal set; }

        public virtual bool CanSerialize { get; } = true;

        public static IResourceCollection CreateCollectionForType(Type resourceType) {
            if(!Constructors.ContainsKey(resourceType))
                Constructors[resourceType] =
                    typeof(ResourceCollection<>).MakeGenericType(resourceType).GetConstructor(new Type[0]);

            return (IResourceCollection) Constructors[resourceType].Invoke(new object[0]);
        }

        public virtual void Access() {
            if(LastAccessTime < Time.Now) LastAccessTime = Time.Now;
        }

        public void NeverUnload() {
            LastAccessTime = long.MaxValue;
        }

        public virtual bool Unload() {
            return true;
        }

#if DEBUG
        ~Resource() {
            Console.WriteLine("Finalized resource " + Identifier);
        }
#endif

        public virtual void SerializeResource(SaveOperation writer) {
            if(!CanSerialize) throw new InvalidOperationException($"{GetType().Name} cannot be serialized");
            GenericSerializer.Serialize(this, writer);
        }

        public virtual void DeserializeResource(LoadOperation reader) {
            if(!CanSerialize) throw new InvalidOperationException($"{GetType().Name} cannot be deserialized");
            GenericSerializer.Deserialize(this, reader);
        }

        public virtual void DeserializeResource(string path) {
            var reader = new LoadOperation(Data.In.Stream(path));
            try {
                GenericSerializer.Deserialize(this, reader);
            } finally {
                reader.Close();
            }
        }

        public void Save() {
            ResourceAttribute resourceAttribute = AttributeOf(GetType());
            string resourceFolder = Path.Combine("resources", resourceAttribute.DirectoryName);
            string path = Path.Combine(resourceFolder, Identifier);
            path = Path.ChangeExtension(path, resourceAttribute.Extensions[0]);
            Save(path);
        }

        public void Save(string path) {
            var writer = new SaveOperation(Data.Out.Stream(path));
            try {
                SerializeResource(writer);
            } finally {
                writer.Close();
            }
        }

        private static void Populate() {
            if(_attributes != null) return;
            _attributes = new Dictionary<Type, ResourceAttribute>();

            PopulateDictionaryWithAssembly(Assembly.GetAssembly(typeof(Component)));
            PopulateDictionaryWithAssembly(Assembly.GetEntryAssembly());
        }

        private static void PopulateDictionaryWithAssembly(Assembly assembly) {
            foreach(Type type in assembly.GetTypes())
                if(type.IsSubclassOf(typeof(Resource)))
                    _attributes[type] = type.GetCustomAttribute<ResourceAttribute>()
                                        ?? new ResourceAttribute(type.Name.ToLowerInvariant());
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
        public readonly string[] Extensions;
        public readonly int UnloadTime; //ms

        public ResourceAttribute(string directoryName, int unloadTime = 5000, params string[] extensions) {
            DirectoryName = directoryName;
            UnloadTime = unloadTime;
            if(extensions == null || extensions.Length == 0)
                Extensions = new[] {"foldresource"};
            else
                Extensions = extensions;
        }
    }
}