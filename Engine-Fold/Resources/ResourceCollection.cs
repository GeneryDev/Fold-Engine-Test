using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Interfaces;
using FoldEngine.IO;
using FoldEngine.Serialization;
using FoldEngine.Util;

namespace FoldEngine.Resources;

public interface IResourceCollection : ISelfSerializer
{
    IGameCore Core { get; set; }
    bool IsEmpty { get; }
    Type ResourceType { get; }
    bool Exists(ResourceIdentifier identifier);
    Resource Get(ref ResourceIdentifier identifier, Resource def = null);
    void Save();
    void InvalidateCaches();
    void Attach(Resource resource);
    void Detach(Resource resource);
    void UnloadUnused();
    IEnumerable<Resource> GetAll();
}

public class ResourceCollection<T> : IResourceCollection where T : Resource, new()
{
    protected readonly List<T> Resources = new List<T>();

    // Counter that increments each time any resource inside this collection changes position in the Resources array.
    // Compare this against the generation in resource locations to determine whether the index needs to be
    // recalculated from the identifier string or not.
    protected int Generation = 1;

    public Type ResourceType => typeof(T);

    public IGameCore Core { get; set; }
    public bool IsEmpty => Resources.Count == 0;

    public bool Exists(ResourceIdentifier identifier)
    {
        if (identifier.Identifier == null) return false;
        UpdateResourceIdentifier(ref identifier);

        return identifier.IndexIntoCollection.Get(Generation) - 1 != -1;
    }

    public void Attach(Resource resource)
    {
        Attach((T)resource);
    }

    public void Detach(Resource resource)
    {
        Detach((T)resource);
    }

    public void UnloadUnused()
    {
        int unloadTime = Core.RegistryUnit.Resources.AttributeOf<T>()?.UnloadTime ?? 5000;
        for (int i = 0; i < Resources.Count; i++)
        {
            T resource = Resources[i];
            if (Time.Now - unloadTime >= resource.LastAccessTime)
                if (resource.Unload())
                {
                    Console.WriteLine("UNLOADING UNUSED RESOURCE " + resource.Identifier);
                    Resources.RemoveAt(i);
                    i--;
                }
        }

        InvalidateCaches();
    }

    public IEnumerable<Resource> GetAll()
    {
        for (int i = 0; i < Resources.Count; i++)
        {
            T resource = Resources[i];
            yield return resource;
        }

        yield break;
    }

    public void InvalidateCaches()
    {
        Generation++;
    }

    public void Serialize(SaveOperation writer)
    {
        writer.WriteCompound((ref SaveOperation.Compound c) =>
        {
            foreach (T entry in Resources)
                if (entry.CanSerialize)
                    c.WriteMember(entry.Identifier, () => { entry.SerializeResource(writer); });
        });
    }

    public void Deserialize(LoadOperation reader)
    {
        reader.ReadCompound(c =>
        {
            foreach (string identifier in c.MemberNames)
            {
                c.StartReadMember(identifier);
                Create(identifier, reader);
            }
        });
    }

    public void Save()
    {
        var attribute = Core.RegistryUnit.Resources.AttributeOf(typeof(T));
        foreach (T resource in Resources)
        {
            if (resource.CanSerialize)
            {
                Console.WriteLine($"Serializing resource: {resource.Identifier} of type {typeof(T)}");
                resource.Save(attribute.CreateResourcePath(resource.Identifier));
            }
        }
    }

    private int IndexForIdentifier(string identifier)
    {
        // Console.WriteLine("Retrieving index for identifier '" + identifier + "'");
        for (int i = 0; i < Resources.Count; i++)
            if (Resources[i].Identifier == identifier)
                return i;

        return -1;
    }

    private void UpdateResourceIdentifier(ref ResourceIdentifier identifier)
    {
        if (!identifier.IndexIntoCollection.IsValid(Generation))
        {
            int indexIntoCollection = identifier.IndexIntoCollection.Get(Generation) - 1;
            if (indexIntoCollection == -1)
            {
                indexIntoCollection = IndexForIdentifier(identifier.Identifier);
                identifier.IndexIntoCollection.Set(indexIntoCollection + 1, Generation);
            }
        }
    }

    public T Get(ref ResourceIdentifier identifier, T def = null)
    {
        if (identifier.Identifier == null) return def;
        UpdateResourceIdentifier(ref identifier);

        int indexIntoCollection = identifier.IndexIntoCollection.Get(Generation) - 1;
        if (indexIntoCollection != -1)
        {
            T resource = Resources[indexIntoCollection];
            resource.Access();
            return resource;
        }

        return def;
    }

    public Resource Get(ref ResourceIdentifier identifier, Resource def = null)
    {
        return Get(ref identifier, (T)def);
    }

    public T Create(string identifier, LoadOperation reader = null)
    {
        int existingIndex = IndexForIdentifier(identifier);
        if (existingIndex != -1) throw new ArgumentException("Resource '" + identifier + "' already exists!");
        var newT = new T { Identifier = identifier };
        if (reader != null) newT.DeserializeResource(reader);
        Resources.Add(newT);
        InvalidateCaches();
        return newT;
    }

    public void Attach(T resource)
    {
        string identifier = resource.Identifier;
        int existingIndex = IndexForIdentifier(identifier);
        if (existingIndex != -1) throw new ArgumentException("Resource '" + identifier + "' already exists!");

        Resources.Add(resource);
        InvalidateCaches();
    }

    public void Detach(T resource)
    {
        string identifier = resource.Identifier;
        int existingIndex = IndexForIdentifier(identifier);
        if (existingIndex != -1)
        {
            Resources.RemoveAt(existingIndex);
            InvalidateCaches();
        }
    }
}

public struct ResourceIdentifier
{
    [DoNotSerialize] public string Identifier;

    [DoNotSerialize] public CachedValue<int> IndexIntoCollection;

    public ResourceIdentifier(string identifier)
    {
        Identifier = identifier;
        IndexIntoCollection = default;
    }
}

public abstract class Resource
{
    protected internal long LastAccessTime = Time.Now;
    public string Identifier { get; protected internal set; }

    public virtual bool CanSerialize { get; } = true;

    public virtual void Access()
    {
        if (LastAccessTime < Time.Now) LastAccessTime = Time.Now;
    }

    public void NeverUnload()
    {
        LastAccessTime = long.MaxValue;
    }

    public virtual bool Unload()
    {
        return true;
    }

#if DEBUG
    ~Resource()
    {
        Console.WriteLine("Finalized resource " + Identifier);
    }
#endif

    public virtual void SerializeResource(SaveOperation writer)
    {
        if (!CanSerialize) throw new InvalidOperationException($"{GetType().Name} cannot be serialized");
        GenericSerializer.Serialize(this, writer);
    }

    public virtual void DeserializeResource(LoadOperation reader)
    {
        if (!CanSerialize) throw new InvalidOperationException($"{GetType().Name} cannot be deserialized");
        GenericSerializer.Deserialize(this, reader);
    }

    public virtual void DeserializeResource(string path)
    {
        var reader = new LoadOperation(Data.In.Stream(path));
        try
        {
            GenericSerializer.Deserialize(this, reader);
        }
        finally
        {
            reader.Close();
        }
    }

    public void Save(string path, FieldCollection.Configurator configurator = null)
    {
        var writer = new SaveOperation(Data.Out.Stream(path));
        configurator?.Invoke(writer.Options);
        try
        {
            SerializeResource(writer);
        }
        finally
        {
            writer.Close();
        }
    }
}

public sealed class ResourceAttribute : Attribute
{
    public readonly string Identifier;
    public readonly string DirectoryName;
    public readonly string[] Extensions;
    public readonly int UnloadTime; //ms
    public Type ResourceType;

    public ResourceAttribute(string identifier, string directoryName = null, int unloadTime = 5000,
        params string[] extensions)
    {
        Identifier = identifier;
        DirectoryName = directoryName ?? identifier;
        UnloadTime = unloadTime;
        if (extensions == null || extensions.Length == 0)
            Extensions = new[] { "foldresource" };
        else
            Extensions = extensions;
    }

    public string CreateResourcePath(string resourceIdentifier)
    {
        string resourceFolder = Path.Combine("resources", DirectoryName);
        string path = Path.Combine(resourceFolder, $"{resourceIdentifier}.{Extensions[0]}");
        return path;
    }
}