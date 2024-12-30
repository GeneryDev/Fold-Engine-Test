using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace FoldEngine.Resources;

public class ResourceLoader
{
    private readonly List<ResourceLoadTask> _activeTasks = new List<ResourceLoadTask>();
    private readonly List<ResourceLoadTask> _inactiveTasks = new List<ResourceLoadTask>();
    private readonly ResourceCollections _resources;

    public ResourceLoader(ResourceCollections resources)
    {
        _resources = resources;
    }

    //MAIN THREAD
    private ResourceLoadTask PrepareTask()
    {
        if (_inactiveTasks.Count > 0)
        {
            ResourceLoadTask reused = _inactiveTasks[_inactiveTasks.Count - 1];
            _inactiveTasks.RemoveAt(_inactiveTasks.Count - 1);
            _activeTasks.Add(reused);
            return reused;
        }

        var created = new ResourceLoadTask();
        _activeTasks.Add(created);
        return created;
    }

    //MAIN THREAD
    public ResourceStatus GetStatusOfResource(Type type, string identifier)
    {
        foreach (ResourceLoadTask task in _activeTasks)
            if (task.Type == type && task.Identifier == identifier)
                return task.Status;
        return ResourceStatus.Inactive;
    }

    public ResourceStatus GetStatusOfResource<T>(string identifier)
    {
        foreach (ResourceLoadTask task in _activeTasks)
            if (task.Type == typeof(T) && task.Identifier == identifier)
                return task.Status;
        return ResourceStatus.Inactive;
    }

    //MAIN THREAD
    public void NeedsLoaded<T>(string identifier, string path) where T : Resource, new()
    {
        if (GetStatusOfResource<T>(identifier) == ResourceStatus.Inactive)
        {
            ResourceLoadTask task = PrepareTask();
            task.Type = typeof(T);
            task.Identifier = identifier;
            task.Path = path;
            task.Status = ResourceStatus.Loading;

            StartLoading<T>(task);
        }
    }

    public void NeedsLoaded(Type type, string identifier, string path)
    {
        if (GetStatusOfResource(type, identifier) == ResourceStatus.Inactive)
        {
            MethodInfo methodToCall = typeof(ResourceLoader)
                .GetMethod(nameof(StartLoading), new Type[] { typeof(ResourceLoadTask) }).MakeGenericMethod(type);

            ResourceLoadTask task = PrepareTask();
            task.Type = type;
            task.Identifier = identifier;
            task.Path = path;
            task.Status = ResourceStatus.Loading;

            methodToCall.Invoke(this, new object[] { task });
        }
    }

    //MAIN THREAD
    public void StartLoading<T>(ResourceLoadTask task) where T : Resource, new()
    {
        ThreadPool.QueueUserWorkItem(_ => Load<T>(task));
    }

    //MAIN THREAD
    public void AddLoadCallback<T>(string identifier, ResourceCollections.OnResourceLoaded callback)
        where T : Resource, new()
    {
        foreach (ResourceLoadTask task in _activeTasks)
            if (task.Type == typeof(T) && task.Identifier == identifier)
                task.Callbacks.Add(callback);
    }

    //WORK THREAD
    private static void Load<T>(ResourceLoadTask task) where T : Resource, new()
    {
        try
        {
            var resource = new T { Identifier = task.Identifier };
            resource.DeserializeResource(task.Path);
            resource.ResourcePath = task.Path;

            task.CompletedResource = resource;
            task.Status = ResourceStatus.Complete;
        }
        catch (Exception x)
        {
            Console.WriteLine("Could not load resource '" + task.Identifier + "': " + x.Message);
            task.Status = ResourceStatus.Error;
        }
    }

    //MAIN THREAD
    public void Update()
    {
        for (int i = 0; i < _activeTasks.Count; i++)
        {
            ResourceLoadTask task = _activeTasks[i];
            switch (task.Status)
            {
                case ResourceStatus.Complete:
                {
                    TaskCompleted(task);
                    ResetTask(task);
                    _activeTasks.RemoveAt(i);
                    _inactiveTasks.Add(task);
                    i--;
                    break;
                }
                case ResourceStatus.Error:
                {
                    TaskError(task);
                    ResetTask(task);
                    _activeTasks.RemoveAt(i);
                    _inactiveTasks.Add(task);
                    i--;
                    break;
                }
                case ResourceStatus.Inactive:
                {
                    Console.WriteLine("Blasphemy!");
                    break;
                }
            }
        }
    }

    private void ResetTask(ResourceLoadTask task)
    {
        task.Identifier = null;
        task.Path = null;
        task.Type = null;
        task.CompletedResource = null;
        task.Status = ResourceStatus.Inactive;
        task.Callbacks.Clear();
    }

    //MAIN THREAD
    private void TaskCompleted(ResourceLoadTask task)
    {
        _resources.Attach(task.CompletedResource);

        foreach (ResourceCollections.OnResourceLoaded callback in task.Callbacks) callback(task.CompletedResource);

        task.Status = ResourceStatus.Inactive;
    }

    //MAIN THREAD
    private void TaskError(ResourceLoadTask task)
    {
        task.Status = ResourceStatus.Inactive;
    }
}

public class ResourceLoadTask
{
    public readonly List<ResourceCollections.OnResourceLoaded> Callbacks =
        new List<ResourceCollections.OnResourceLoaded>();

    public Resource CompletedResource;
    public string Identifier;
    public string Path;
    public ResourceStatus Status;
    public Type Type;
}

public enum ResourceStatus
{
    Loading,
    Complete,
    Error,
    Inactive
}