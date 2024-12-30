using System;
using System.IO;
using FoldEngine.Resources;

namespace FoldEngine.Serialization;

public readonly struct StorageFormat
{
    public readonly string Extension;
    public readonly Func<Stream, SaveOperation> SaveOperationProvider;
    public readonly Func<Stream, LoadOperation> LoadOperationProvider;

    public static readonly StorageFormat Binary = new StorageFormat(Resource.ExtensionBinary,
        stream => new BinarySaveOperation(stream),
        stream => new BinaryLoadOperation(stream));
    public static readonly StorageFormat Json = new StorageFormat(Resource.ExtensionJson,
        stream => new TextSaveOperation(stream),
        stream => new TextLoadOperation(stream));

    public StorageFormat(string extension, Func<Stream, SaveOperation> saveOperationProvider, Func<Stream, LoadOperation> loadOperationProvider)
    {
        Extension = extension;
        SaveOperationProvider = saveOperationProvider;
        LoadOperationProvider = loadOperationProvider;
    }

    public bool Equals(StorageFormat other)
    {
        return Extension == other.Extension;
    }

    public override bool Equals(object obj)
    {
        return obj is StorageFormat other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (Extension != null ? Extension.GetHashCode() : 0);
    }

    public static bool operator ==(StorageFormat left, StorageFormat right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StorageFormat left, StorageFormat right)
    {
        return !left.Equals(right);
    }
}