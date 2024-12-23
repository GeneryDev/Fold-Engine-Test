using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Util;

namespace FoldEngine.Serialization;

public abstract class LoadOperation
{
    public delegate void ArrayReader(Array c);

    public delegate void CompoundReader(Compound c);

    public readonly FieldCollection Options = new FieldCollection();

    public SerializerSuite SerializerSuite = SerializerSuite.Instance;

    public abstract void Close();

    public abstract void Dispose();

    public abstract bool ReadBoolean();

    public abstract byte ReadByte();

    public abstract sbyte ReadSByte();

    public abstract char ReadChar();

    public abstract short ReadInt16();

    public abstract ushort ReadUInt16();

    public abstract int ReadInt32();

    public abstract uint ReadUInt32();

    public abstract long ReadInt64();

    public abstract ulong ReadUInt64();

    public abstract float ReadSingle();

    public abstract double ReadDouble();

    public abstract decimal ReadDecimal();

    public abstract string ReadString();

    public abstract T Read<T>();

    public abstract object Read(Type type);

    public abstract void ReadCompound(CompoundReader reader);

    public abstract void ReadArray(ArrayReader reader);

    protected abstract void StartReadCompoundMember(Compound compound, string name);
    protected abstract void StartReadArrayMember(Array compound, int index);

    public struct Compound
    {
        public LoadOperation LoadOperation;
        public int MemberCount;
        public string[] MemberNames;
        public long[] MemberDataOffsets;
        public int[] MemberDataLengths;

        public bool HasMember(string name)
        {
            foreach (string memberName in MemberNames)
                if (memberName == name)
                    return true;
            return false;
        }

        public void StartReadMember(string name)
        {
            LoadOperation.StartReadCompoundMember(this, name);
        }

        public T GetMember<T>(string name)
        {
            StartReadMember(name);
            return LoadOperation.SerializerSuite.Read<T>(LoadOperation);
        }

        public List<T> GetListMember<T>(string name)
        {
            StartReadMember(name);
            return LoadOperation.SerializerSuite.ReadList<T>(LoadOperation);
        }

        public void DeserializeMember(string name, ISelfSerializer selfSerializer)
        {
            StartReadMember(name);
            selfSerializer.Deserialize(LoadOperation);
        }
    }

    public struct Array
    {
        public LoadOperation LoadOperation;
        public int MemberCount;
        public long[] MemberDataOffsets;
        public int[] MemberDataLengths;

        public bool HasMember(int index)
        {
            return index >= 0 && index < MemberCount;
        }

        public void StartReadMember(int index)
        {
            LoadOperation.StartReadArrayMember(this, index);
        }

        public T GetMember<T>(int index)
        {
            StartReadMember(index);
            return LoadOperation.SerializerSuite.Read<T>(LoadOperation);
        }

        public List<T> GetListMember<T>(int index)
        {
            StartReadMember(index);
            return LoadOperation.SerializerSuite.ReadList<T>(LoadOperation);
        }

        public void DeserializeMember(int index, ISelfSerializer selfSerializer)
        {
            StartReadMember(index);
            selfSerializer.Deserialize(LoadOperation);
        }
    }
}