using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Util;

namespace FoldEngine.Serialization;

public abstract class LoadOperation
{
    public delegate void ArrayHeaderReader(ArrayHeader m);
    public delegate void ArrayMemberReader(ArrayMember m);

    public delegate void CompoundMemberReader(CompoundMember m);

    public readonly FieldCollection Options = new FieldCollection();

    public SerializerSuite SerializerSuite = SerializerSuite.Instance;

    public abstract void Close();

    public abstract void Dispose();
    
    public abstract LoadOperation StartStruct();

    public abstract LoadOperation EndStruct();

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

    public abstract void ReadCompound(CompoundMemberReader reader);

    public void ReadArray(ArrayMemberReader reader)
    {
        ReadArray(null, reader);
    }

    public abstract void ReadArray(ArrayHeaderReader headerReader, ArrayMemberReader reader);

    public List<T> ReadList<T>()
    {
        return SerializerSuite.ReadList<T>(this);
    }

    public void Deserialize(ISelfSerializer selfSerializer)
    {
        selfSerializer.Deserialize(this);
    }

    public ref struct CompoundMember
    {
        public LoadOperation Reader;
        public string Name;
        internal bool Skipped;

        public void Skip()
        {
            Skipped = true;
        }
    }

    public struct ArrayHeader
    {
        public LoadOperation Reader;
        public int MemberCount;
    }

    public ref struct ArrayMember
    {
        public ArrayHeader Array;
        public int Index;
        internal bool Skipped;
        
        public LoadOperation Reader => Array.Reader;

        public void Skip()
        {
            Skipped = true;
        }
    }
}