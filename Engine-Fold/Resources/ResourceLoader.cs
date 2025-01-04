using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace FoldEngine.Resources;

public class ResourceLoader
{
    private readonly ResourceCollections _resources;
    private readonly Thread _mainThread;
    private readonly Thread _schedulerThread;

    private ConcurrentDictionary<ResourceLoadKey, ResourceLoadTask> _activeTasks = new();
    private ConcurrentQueue<ResourceLoadTask> _startQueue = new();
    private ConcurrentQueue<ResourceLoadTask> _completedQueue = new();

    public ResourceLoader(ResourceCollections resources)
    {
        _resources = resources;
        _mainThread = Thread.CurrentThread;
        _schedulerThread = new Thread(SchedulerLoop);
        _schedulerThread.Start();
    }

    public ResourceStatus GetStatusOfResource(Type type, string identifier)
    {
        _activeTasks.TryGetValue(new(type, identifier), out var task);
        return task?.Status ?? ResourceStatus.Inactive;
    }

    public ResourceStatus GetStatusOfResource<T>(string identifier)
    {
        _activeTasks.TryGetValue(new(typeof(T), identifier), out var task);
        return task?.Status ?? ResourceStatus.Inactive;
    }

    //MAIN THREAD
    public void NeedsLoaded<T>(string identifier, string path) where T : Resource, new()
    {
        if (GetStatusOfResource<T>(identifier) == ResourceStatus.Inactive)
        {
            var task = new ResourceLoadTask
            {
                Type = typeof(T),
                Identifier = identifier,
                Path = path,
                Status = ResourceStatus.Loading
            };
            _activeTasks[new ResourceLoadKey(typeof(T), identifier)] = task;
            _startQueue.Enqueue(task);
        }
    }

    public void NeedsLoaded(Type type, string identifier, string path)
    {
        if (GetStatusOfResource(type, identifier) == ResourceStatus.Inactive)
        {
            var task = new ResourceLoadTask
            {
                Type = type,
                Identifier = identifier,
                Path = path,
                Status = ResourceStatus.Loading
            };
            _activeTasks[new ResourceLoadKey(type, identifier)] = task;
            _startQueue.Enqueue(task);
        }
    }
    
    //MAIN THREAD
    public void Await(Type type, string identifier)
    {
        while (GetStatusOfResource(type, identifier) == ResourceStatus.Loading)
        {
            // Wait
        }
    }
    //MAIN THREAD
    public void Await<T>(string identifier)
    {
        while (GetStatusOfResource<T>(identifier) == ResourceStatus.Loading)
        {
            // Wait
        }
    }

    //MAIN THREAD
    public void AddLoadCallback<T>(string identifier, ResourceCollections.OnResourceLoaded callback)
        where T : Resource, new()
    {
        var key = new ResourceLoadKey(typeof(T), identifier);
        _activeTasks[key]?.Callbacks.Add(callback);
    }

    //MAIN THREAD
    public void Update()
    {
        foreach (var key in _activeTasks.Keys)
        {
            var task = _activeTasks[key];
            switch (task.Status)
            {
                case ResourceStatus.Complete:
                case ResourceStatus.Error:
                {
                    TaskFinished(task);
                    _activeTasks.TryRemove(key, out task);
                    break;
                }
                case ResourceStatus.Inactive:
                {
                    Console.WriteLine("Blasphemy!");
                    break;
                }
                case ResourceStatus.Loading:
                default:
                    break;
            }
        }
    }

    //MAIN THREAD
    private void TaskFinished(ResourceLoadTask task)
    {
        if(task.Status == ResourceStatus.Complete)
            TaskCompleted(task);
        else 
            TaskError(task);
    }

    //MAIN THREAD
    private void TaskCompleted(ResourceLoadTask task)
    {
        foreach (ResourceCollections.OnResourceLoaded callback in task.Callbacks) callback(task.CompletedResource);

        task.Status = ResourceStatus.Inactive;
    }

    //MAIN THREAD
    private void TaskError(ResourceLoadTask task)
    {
        task.Status = ResourceStatus.Inactive;
    }
    
    private void SchedulerLoop()
    {
        while (_mainThread.IsAlive)
        {
            // Consume start queue, check for tasks to start
            while (_startQueue.TryDequeue(out var task))
            {
                Console.WriteLine($"Queued task to load {task.Identifier}");
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    task.Load();
                    _completedQueue.Enqueue(task);
                });
            }

            // Consume completed queue, check for tasks that have completed and add their resources to the collection
            while (_completedQueue.TryDequeue(out var task))
            {
                if (task.CompletedResource != null)
                {
                    _resources.Attach(task.CompletedResource);
                    task.Status = ResourceStatus.Complete;
                }
                //TODO handle errors
            }
        }
    }

    private record struct ResourceLoadKey(Type Type, string Identifier)
    {
    }
}

public class ResourceLoadTask
{
    public readonly List<ResourceCollections.OnResourceLoaded> Callbacks = new();

    public string Identifier; // Input
    public string Path; // Input
    public Type Type; // Input
    public Resource CompletedResource; // Worker write, Main read (Main reset)
    public ResourceStatus Status; // Worker write, Main read (Main reset) 

    public void Load()
    {
        try
        {
            var emptyConstructor = Type.GetConstructor(Type.EmptyTypes);
            var resource = (Resource) emptyConstructor!.Invoke(Array.Empty<object>());

            resource.Identifier = this.Identifier;
            resource.DeserializeResource(this.Path);
            resource.ResourcePath = this.Path;

            this.CompletedResource = resource;
        }
        catch (Exception x)
        {
            Console.WriteLine("Could not load resource '" + this.Identifier + "': " + x.Message);
            this.Status = ResourceStatus.Error;
        }
    }
}

public enum ResourceStatus
{
    Loading,
    Complete,
    Error,
    Inactive
}