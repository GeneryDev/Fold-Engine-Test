using System.IO;
using FoldEngine.Resources;
using FoldEngine.Serialization;

namespace FoldEngine.Scenes;

public class PackedResource<T> : Resource, ISelfSerializer where T : Resource, new()
{
    public override bool CanSerialize => true;
    private byte[] _serializedBytes;

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

        reserializer.Close();
        _serializedBytes = stream.GetBuffer();
        reserializer.Dispose();
    }

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
        var stream = new MemoryStream(_serializedBytes);
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
}