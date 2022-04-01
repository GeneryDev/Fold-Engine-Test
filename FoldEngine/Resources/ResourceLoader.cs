using System;
using System.Collections.Generic;
using System.Threading;
using FoldEngine.IO;
using FoldEngine.Serialization;

namespace FoldEngine.Resources {
    public class ResourceLoader {
        private ResourceCollections _resources;
        private List<ResourceLoadTask> _activeTasks = new List<ResourceLoadTask>();
        private List<ResourceLoadTask> _inactiveTasks = new List<ResourceLoadTask>();

        public ResourceLoader(ResourceCollections resources) {
            this._resources = resources;
        }

        //MAIN THREAD
        private ResourceLoadTask PrepareTask() {
            if(_inactiveTasks.Count > 0) {
                ResourceLoadTask reused = _inactiveTasks[_inactiveTasks.Count - 1];
                _inactiveTasks.RemoveAt(_inactiveTasks.Count - 1);
                _activeTasks.Add(reused);
                return reused;
            } else {
                var created = new ResourceLoadTask();
                _activeTasks.Add(created);
                return created;
            }
        }

        //MAIN THREAD
        public ResourceStatus GetStatusOfResource(Type type, string identifier) {
            foreach(ResourceLoadTask task in _activeTasks) {
                if(task.Type == type && task.Identifier == identifier) return task.Status;
            }
            return ResourceStatus.Inactive;
        }
        public ResourceStatus GetStatusOfResource<T>(string identifier) {
            foreach(ResourceLoadTask task in _activeTasks) {
                if(task.Type == typeof(T) && task.Identifier == identifier) return task.Status;
            }
            return ResourceStatus.Inactive;
        }

        //MAIN THREAD
        public void NeedsLoaded<T>(string identifier, string path) where T : Resource, new() {
            if(GetStatusOfResource<T>(identifier) == ResourceStatus.Inactive) {
                ResourceLoadTask task = PrepareTask();
                task.Type = typeof(T);
                task.Identifier = identifier;
                task.Path = path;
                task.Status = ResourceStatus.Loading;

                StartLoading<T>(task);
            }
        }

        //MAIN THREAD
        private void StartLoading<T>(ResourceLoadTask task) where T : Resource, new() {
            ThreadPool.QueueUserWorkItem(_ => Load<T>(task));
        }
        
        //MAIN THREAD
        public void AddLoadCallback<T>(string identifier, ResourceCollections.OnResourceLoaded callback) where T : Resource, new() {
            foreach(ResourceLoadTask task in _activeTasks) {
                if(task.Type == typeof(T) && task.Identifier == identifier) {
                    task.Callbacks.Add(callback);
                }
            }
        }

        //WORK THREAD
        private void Load<T>(ResourceLoadTask task) where T : Resource, new() {
            LoadOperation reader = null;
            try {
                reader = new LoadOperation(Data.In.Stream(task.Path));
                var resource = new T {Identifier = task.Identifier};
                GenericSerializer.Deserialize(resource, reader);
                Thread.Sleep(1000);
                reader.Close();

                task.CompletedResource = resource;
                task.Status = ResourceStatus.Complete;
            } catch(Exception x) {
                Console.WriteLine("Could not load resource '" + task.Identifier + "': " + x.Message);
                task.Status = ResourceStatus.Error;
            } finally {
                reader?.Close();
            }
        }

        //MAIN THREAD
        public void Update() {
            for(int i = 0; i < _activeTasks.Count; i++) {
                ResourceLoadTask task = _activeTasks[i];
                switch(task.Status) {
                    case ResourceStatus.Complete: {
                        TaskCompleted(task);
                        ResetTask(task);
                        _activeTasks.RemoveAt(i);
                        _inactiveTasks.Add(task);
                        i--;
                        break;
                    }
                    case ResourceStatus.Error: {
                        TaskError(task);
                        ResetTask(task);
                        _activeTasks.RemoveAt(i);
                        _inactiveTasks.Add(task);
                        i--;
                        break;
                    }
                    case ResourceStatus.Inactive: {
                        Console.WriteLine("Blasphemy!");
                        break;
                    }
                }
            }
        }

        private void ResetTask(ResourceLoadTask task) {
            task.Identifier = null;
            task.Path = null;
            task.Type = null;
            task.CompletedResource = null;
            task.Status = ResourceStatus.Inactive;
            task.Callbacks.Clear();
        }

        //MAIN THREAD
        private void TaskCompleted(ResourceLoadTask task) {
            _resources.Insert(task.CompletedResource);

            foreach(ResourceCollections.OnResourceLoaded callback in task.Callbacks) {
                callback(task.CompletedResource);
            }
            
            task.Status = ResourceStatus.Inactive;
        }

        //MAIN THREAD
        private void TaskError(ResourceLoadTask task) {
            task.Status = ResourceStatus.Inactive;
        }

    }

    public class ResourceLoadTask {
        public Type Type;
        public string Identifier;
        public string Path;
        public Resource CompletedResource;
        public ResourceStatus Status;
        public readonly List<ResourceCollections.OnResourceLoaded> Callbacks = new List<ResourceCollections.OnResourceLoaded>();
    }

    public enum ResourceStatus {
        Loading,
        Complete,
        Error,
        Inactive
    }
}