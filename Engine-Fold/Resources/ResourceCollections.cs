using System;
using System.Collections.Generic;
using FoldEngine.Interfaces;
using FoldEngine.Serialization;
using FoldEngine.Systems;

namespace FoldEngine.Resources {
    public class ResourceCollections : ISelfSerializer {
        /// <summary>
        ///     Callback for when a requested resource is loaded.
        /// </summary>
        /// <param name="resource"></param>
        public delegate void OnResourceLoaded(Resource resource);

        private readonly IGameCore _core;

        private readonly Dictionary<Type, IResourceCollection> _collections =
            new Dictionary<Type, IResourceCollection>();

        private long _lastPollTime;

        private readonly ResourceLoader _loader;
        public ResourceCollections Parent;

        public ResourceCollections(IGameCore core) {
            _core = core;
            _loader = new ResourceLoader(this);
        }

        public ResourceCollections(ResourceCollections parent) {
            Parent = parent;
            _core = parent._core;
        }

        protected ResourceCollections Root => Parent != null ? Parent.Root : this;

        public void Serialize(SaveOperation writer) {
            writer.WriteCompound((ref SaveOperation.Compound c) => {
                foreach(KeyValuePair<Type, IResourceCollection> entry in _collections) {
                    if(entry.Value.IsEmpty) continue;
                    c.WriteMember(entry.Value.ResourceType.FullName, (ISelfSerializer) entry.Value);
                }
            });
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
                    CollectionFor(resourceType, true).Deserialize(reader);
                }
            });
        }

        /// <summary>
        ///     Checks whether the resource of the given type and identifier exists and is currently loaded.
        /// </summary>
        /// <typeparam name="T">The type of the resource</typeparam>
        /// <param name="identifier">The identifier of the resource</param>
        /// <returns>True if the resource exists and is loaded, false otherwise.</returns>
        public bool Exists<T>(ref ResourceIdentifier identifier) where T : Resource, new() {
            return FindOwner<T>(ref identifier) != null;
        }

        /// <summary>
        ///     Checks whether the resource of the given type and identifier exists and is currently loaded.
        /// </summary>
        /// <param name="type">The type of the resource</param>
        /// <param name="identifier">The identifier of the resource</param>
        /// <returns>True if the resource exists and is loaded, false otherwise.</returns>
        public bool Exists(Type type, ref ResourceIdentifier identifier) {
            return FindOwner(type, ref identifier) != null;
        }

        /// <summary>
        ///     Retrieves the resource of the given type and identifier, if it is exists and is loaded.
        ///     If it is not loaded, it returns the provided default value,
        ///     and schedules the resource to be loaded from disk asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the resource</typeparam>
        /// <param name="identifier">The identifier of the resource</param>
        /// <param name="def">The default value to return in case the resource is not loaded.</param>
        /// <returns>The requested resource, if it exists; the def parameter otherwise.</returns>
        public T Get<T>(ref ResourceIdentifier identifier, T def = default) where T : Resource, new() {
            if(identifier.Identifier == null) return def;

            IResourceCollection collection = FindOwner<T>(ref identifier);
            if(collection != null)
                return ((ResourceCollection<T>) collection).Get(ref identifier, def);
            Root.StartLoad<T>(identifier.Identifier);

            return def;
        }

        /// <summary>
        ///     Retrieves the resource of the given type and identifier, if it is exists and is loaded.
        ///     If it is not loaded, it returns the provided default value,
        ///     and schedules the resource to be loaded from disk asynchronously.
        /// </summary>
        /// <param name="type">The type of the resource</param>
        /// <param name="identifier">The identifier of the resource</param>
        /// <param name="def">The default value to return in case the resource is not loaded.</param>
        /// <returns>The requested resource, if it exists; the def parameter otherwise.</returns>
        public Resource Get(Type type, ref ResourceIdentifier identifier, Resource def = default) {
            if(identifier.Identifier == null) return def;

            IResourceCollection collection = FindOwner(type, ref identifier);
            if(collection != null)
                return collection.Get(ref identifier, def);
            Root.StartLoad(type, identifier.Identifier);

            return def;
        }

        /// <summary>
        ///     Requests that a resource be loaded into memory asynchronously,
        ///     then invokes the given callback method with the resource once it is loaded.
        ///     If the resource is already in memory, the callback method is called immediately.
        /// </summary>
        /// <typeparam name="T">The type of the resource</typeparam>
        /// <param name="identifier">The identifier of the resource</param>
        /// <param name="callback">The function to invoke with the loaded resource</param>
        public void Load<T>(ref ResourceIdentifier identifier, OnResourceLoaded callback) where T : Resource, new() {
            if(identifier.Identifier == null) return;

            IResourceCollection collection = FindOwner<T>(ref identifier);
            if(collection != null) {
                callback(((ResourceCollection<T>) collection).Get(ref identifier, default));
            } else {
                Root.StartLoad<T>(identifier.Identifier);
                Root._loader.AddLoadCallback<T>(identifier.Identifier, callback);
            }
        }

        /// <summary>
        ///     Marks a resource as "in use" and prevents it from being unloaded.
        ///     This method should only be called from a <code>GameSystem</code>'s <code>PollResources</code> method.
        ///     See: <see cref="GameSystem" />
        /// </summary>
        /// <typeparam name="T">The type of the resource</typeparam>
        /// <param name="identifier">The identifier of the resource</param>
        /// <param name="preload">Whether to start loading the resource if not already in memory</param>
        public void KeepLoaded<T>(ref ResourceIdentifier identifier, bool preload = false) where T : Resource, new() {
            if(!preload && !Exists<T>(ref identifier)) return;
            Get<T>(ref identifier);
        }

        /// <summary>
        ///     Marks a resource as "in use" and prevents it from being unloaded.
        ///     This method should only be called from a <code>GameSystem</code>'s <code>PollResources</code> method.
        ///     See: <see cref="GameSystem" />
        /// </summary>
        /// <param name="type">The type of the resource</param>
        /// <param name="identifier">The identifier of the resource</param>
        /// <param name="preload">Whether to start loading the resource if not already in memory</param>
        public void KeepLoaded(Type type, ref ResourceIdentifier identifier, bool preload = false) {
            if(!preload && !Exists(type, ref identifier)) return;
            Get(type, ref identifier);
        }

        /// <summary>
        ///     Creates an empty resource of the given type and identifier.
        ///     If a resource of the same identifier already exists, an exception is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the resource</typeparam>
        /// <param name="identifier">The identifier of the resource</param>
        /// <returns>The newly-created resource</returns>
        public T Create<T>(string identifier) where T : Resource, new() {
            return CollectionFor<T>(true).Create(identifier);
        }

        /// <summary>
        ///     Adds a pre-existing resource to a collection.
        /// </summary>
        /// <typeparam name="T">The type of the resource</typeparam>
        /// <param name="resource">The resource to attach</param>
        protected internal void Attach<T>(T resource) where T : Resource, new() {
            CollectionFor<T>(true).Attach(resource);
        }

        /// <summary>
        ///     Adds a pre-existing resource to a collection.
        /// </summary>
        /// <param name="resource">The resource to attach</param>
        protected internal void Attach(Resource resource) {
            CollectionFor(resource.GetType(), true).Attach(resource);
        }

        /// <summary>
        ///     Removes a pre-existing resource from a collection.
        /// </summary>
        /// <typeparam name="T">The type of the resource</typeparam>
        /// <param name="resource">The resource to detach</param>
        protected internal void Detach<T>(T resource) where T : Resource, new() {
            CollectionFor<T>(false)?.Detach(resource);
        }

        /// <summary>
        ///     Removes a pre-existing resource from a collection.
        /// </summary>
        /// <param name="resource">The resource to detach</param>
        protected internal void Detach(Resource resource) {
            CollectionFor(resource.GetType(), false)?.Detach(resource);
        }


        private ResourceCollection<T> CollectionFor<T>(bool createIfMissing = false) where T : Resource, new() {
            Type type = typeof(T);
            return (ResourceCollection<T>) (_collections.ContainsKey(type)
                ? _collections[type]
                : createIfMissing
                    ? _collections[type] = new ResourceCollection<T>()
                    : null);
        }

        private IResourceCollection CollectionFor(Type type, bool createIfMissing = false) {
            return _collections.ContainsKey(type)
                ? _collections[type]
                : createIfMissing
                    ? _collections[type] = _core.RegistryUnit.Resources.CreateCollectionForType(type, _core)
                    : null;
        }

        public IEnumerable<Resource> GetAll(Type type) {
            IResourceCollection collection = CollectionFor(type, createIfMissing: false);
            return collection == null ? Array.Empty<Resource>() : collection.GetAll();
        }

        private IResourceCollection FindOwner<T>(ref ResourceIdentifier identifier) where T : Resource, new() {
            return FindOwner(typeof(T), ref identifier);
        }

        private IResourceCollection FindOwner(Type type, ref ResourceIdentifier identifier) {
            return _collections.ContainsKey(type) && _collections[type].Exists(identifier)
                ? _collections[type]
                : Parent?.FindOwner(type, ref identifier);
        }

        private void Clear() {
            _collections.Clear();
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

        private void StartLoad(Type type, string identifier) {
            if(Parent != null) {
                Parent.StartLoad(type, identifier);
                return;
            }

            if(_core.ResourceIndex.Exists(type, identifier)) {
                string path = _core.ResourceIndex.GetPathForIdentifier(type, identifier);
                _loader.NeedsLoaded(type, identifier, path);
            }
        }

        public void InvalidateCaches() {
            foreach(IResourceCollection collection in _collections.Values) collection.InvalidateCaches();

            Parent?.InvalidateCaches();
        }

        public void SaveAll() {
            foreach(IResourceCollection collection in _collections.Values) collection.Save();
            Parent?.SaveAll();


            // if(Parent == null) {
            //     foreach(IResourceCollection collection in _collections.Values) {
            //         collection.Save();
            //     }
            // } else {
            //     Parent.SaveAll();
            // }
        }

        private void PollResources() {
            _lastPollTime = Time.Now;

            if(_core.ActiveScene != null)
                foreach(GameSystem sys in _core.ActiveScene.Systems.AllSystems)
                    sys.PollResources();

            foreach(IResourceCollection collection in _collections.Values) collection.UnloadUnused();
        }

        public void Update() {
            if(Time.Now > _lastPollTime + 5_000) PollResources();

            _loader.Update();
        }
    }
}