using System.Collections.Generic;
using System.IO;
using FoldEngine.Components;
using FoldEngine.Resources;
using FoldEngine.Scenes.Prefabs;
using FoldEngine.Serialization;

namespace FoldEngine.Scenes;

public class PackedResource<T> : Resource, ISelfSerializer where T : Resource, new()
{
    public override bool CanSerialize => true;
    protected byte[] SerializedBytes;

    public PackedResource()
    {
    }

    public PackedResource(T instance)
    {
        Pack(instance);
    }

    public void Pack(T instance)
    {
        var stream = new MemoryStream();
        var reserializer = SaveOperation.Create(stream, StorageFormat.Binary);
        instance.SerializeResource(reserializer);
        PostPack(instance);

        reserializer.Close();
        SerializedBytes = stream.GetBuffer();
        reserializer.Dispose();
    }
    
    protected virtual void PostPack(T instance) {}

    public void Deserialize(LoadOperation reader)
    {
        var instance = new T();
        instance.DeserializeResource(reader);
        
        Pack(instance);
    }
    
    public void Serialize(SaveOperation writer)
    {
        Instantiate().SerializeResource(writer);
    }

    public T Instantiate()
    {
        var stream = new MemoryStream(SerializedBytes);
        var deserializer = LoadOperation.Create(stream, StorageFormat.Binary);
        var instance = new T();
        instance.DeserializeResource(deserializer);
        
        deserializer.Close();
        stream.Close();
        deserializer.Dispose();

        return instance;
    }
}

[Resource("packed_scene", directoryName: "scene", preferredExtension: ExtensionJson)]
public class PackedScene : PackedResource<Scene>
{
    public readonly List<long> TopLevelEntityIds = new();
    
    public void Instantiate(Scene scene, long ownerEntityId = -1, PrefabLoadMode loadMode = PrefabLoadMode.Replace)
    {
        var stream = new MemoryStream(SerializedBytes);
        var deserializer = LoadOperation.Create(stream, StorageFormat.Binary);
        
        deserializer.Options.Set(LoadAsPrefab.Instance, new LoadAsPrefab()
        {
            OwnerEntityId = ownerEntityId,
            LoadMode = loadMode
        });
        var idRemapper = new EntityIdRemapper(scene);
        deserializer.Options.Set(DeserializeRemapIds.Instance, idRemapper);
        if (ownerEntityId != -1 && loadMode == PrefabLoadMode.Replace && TopLevelEntityIds.Count > 0)
        {
            idRemapper.SetMapping(TopLevelEntityIds[0], ownerEntityId);
        }
        deserializer.Options.Set(ResolveComponentConflicts.Instance, ComponentConflictResolution.Skip);
        
        scene.DeserializeResource(deserializer);
        
        deserializer.Close();
        stream.Close();
        deserializer.Dispose();
    }

    protected override void PostPack(Scene instance)
    {
        TopLevelEntityIds.Clear();
        var hierarchicals = instance.Components.CreateIterator<Hierarchical>(IterationFlags.None);
        hierarchicals.Reset();
        while (hierarchicals.Next())
        {
            ref var hierarchical = ref hierarchicals.GetComponent();
            if (!hierarchical.HasParent)
            {
                TopLevelEntityIds.Add(hierarchicals.GetEntityId());
            }
        }
    }
}