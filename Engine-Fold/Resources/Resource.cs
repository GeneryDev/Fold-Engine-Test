using System;
using System.IO;
using FoldEngine.IO;
using FoldEngine.Serialization;
using FoldEngine.Util;

namespace FoldEngine.Resources;

public abstract class Resource
{
    public const string ExtensionBinary = "foldres";
    public const string ExtensionJson = "foldjres";
    
    protected internal long LastAccessTime = Time.Now;
    public string Identifier { get; protected internal set; }
    public string ResourcePath;

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
        string extension = Path.GetExtension(path).Trim('.');
        var format = extension switch
        {
            ExtensionBinary => StorageFormat.Binary,
            ExtensionJson => StorageFormat.Json,
            _ => StorageFormat.Binary
        };
        var reader = LoadOperation.Create(Data.In.Stream(path), format);
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
        string extension = Path.GetExtension(path).Trim('.');
        var format = extension switch
        {
            ExtensionBinary => StorageFormat.Binary,
            ExtensionJson => StorageFormat.Json,
            _ => StorageFormat.Binary
        };
        var writer = SaveOperation.Create(Data.Out.Stream(path), format);
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