using System;
using System.Collections.Generic;
using System.Threading;
using FoldEngine.Interfaces;
using FoldEngine.Serialization;

namespace FoldEngine.Resources {
    public class ResourceCollections : ISelfSerializer {
        private readonly IGameCore _core;
        
        private ResourceLoader _loader;
        public ResourceCollections Parent;
        
        private Dictionary<Type, IResourceCollection> _collections = new Dictionary<Type, IResourceCollection>();

        public ResourceCollections(IGameCore core) {
            this._core = core;
            this._loader = new ResourceLoader(this);
        }
        
        public ResourceCollections(ResourceCollections parent) {
            Parent = parent;
            this._core = parent._core;
        }

        private ResourceCollection<T> CollectionFor<T>(bool createIfMissing = false) where T : Resource, new() {
            Type type = typeof(T);
            return (ResourceCollection<T>) (_collections.ContainsKey(type)
                ? _collections[type]
                : createIfMissing ? (_collections[type] = new ResourceCollection<T>()) : null);
        }

        private IResourceCollection CollectionFor(Type type, bool createIfMissing = false) {
            return _collections.ContainsKey(type)
                ? _collections[type]
                : createIfMissing ? (_collections[type] = Resource.CreateCollectionForType(type)) : null;
        }

        public IResourceCollection FindOwner<T>(ref ResourceIdentifier identifier) where T : Resource, new() {
            Type type = typeof(T);
            return (_collections.ContainsKey(type) && _collections[type].Exists(identifier))
                ? _collections[type]
                : Parent?.FindOwner<T>(ref identifier);
        }

        public T Get<T>(ref ResourceIdentifier identifier, T def = default) where T : Resource, new() {
            if(identifier.Identifier == null) return def;
            
            IResourceCollection collection = FindOwner<T>(ref identifier);
            if(collection != null) {
                return ((ResourceCollection<T>) collection).Get(ref identifier, def);
            } else {
                (Parent ?? this).StartLoad<T>(identifier.Identifier);
            }

            return def;
        }

        public T Create<T>(string identifier) where T : Resource, new() {
            return CollectionFor<T>(true).Create(identifier);
        }

        public void Insert<T>(T resource) where T : Resource, new() {
            CollectionFor<T>(true).Insert(resource);
        }

        public void Insert(Resource resource) {
            (CollectionFor(resource.GetType(), true)).Insert(resource);
        }

        public void Serialize(SaveOperation writer) {
            writer.WriteCompound((ref SaveOperation.Compound c) => {
                foreach(KeyValuePair<Type, IResourceCollection> entry in _collections) {
                    if(entry.Value.IsEmpty) continue;
                    c.WriteMember(entry.Value.ResourceType.FullName, (ISelfSerializer) entry.Value);
                }
            });
        }

        private void Clear() {
            _collections.Clear();
        }

        public void Deserialize(LoadOperation reader) {
            Clear();
            reader.ReadCompound(c => {
                foreach(string rawResourceType in c.MemberNames) {
                    var resourceType = Type.GetType(rawResourceType);
                    if(resourceType == null) {
                        Console.WriteLine("[WARN] Unknown resource type: " + rawResourceType + ". Skipping");
                        continue;
                    }

                    c.StartReadMember(rawResourceType);
                    CollectionFor(resourceType, createIfMissing: true).Deserialize(reader);
                }
            });
        }

        private void StartLoad<T>(string identifier) where T : Resource, new() {
            if(Parent != null) {
                Parent.StartLoad<T>(identifier);
                return;
            }
            if(_core.ResourceIndex.Exists(typeof(T), identifier)) {
                string path = _core.ResourceIndex.GetPathForIdentifier(typeof(T), identifier);
                _loader.NeedsLoaded<T>(identifier, path);
            }
        }

        public void UnloadAll() {

            foreach(IResourceCollection collection in _collections.Values) {
                collection.Unload();
            }

            Parent?.UnloadAll();
        }

        public void SaveAll() {
            
            foreach(IResourceCollection collection in _collections.Values) {
                collection.Save();
            }
            Parent?.SaveAll();
            
            
            // if(Parent == null) {
            //     foreach(IResourceCollection collection in _collections.Values) {
            //         collection.Save();
            //     }
            // } else {
            //     Parent.SaveAll();
            // }
        }

        public void Update() {
            _loader.Update();
        }
    }
}