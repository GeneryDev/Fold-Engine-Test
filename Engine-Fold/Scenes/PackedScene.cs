using System.IO;
using FoldEngine.Resources;
using FoldEngine.Serialization;

namespace FoldEngine.Scenes;

public class PackedResource<T> : Resource, ISelfSerializer where T : Resource, new()
{
    public override bool CanSerialize => true;
    private byte[] _serializedBytes;

    public void Deserialize(LoadOperation reader)
    {
        var temp = new T();
        
        temp.DeserializeResource(reader);
        
        var stream = new MemoryStream();
        var reserializer = SaveOperation.Create(stream, StorageFormat.Binary);
        temp.SerializeResource(reserializer);

        reserializer.Close();
        _serializedBytes = stream.GetBuffer();
        reserializer.Dispose();
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