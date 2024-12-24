using System.Collections.Generic;
using System.IO;
using FoldEngine.Util;

namespace FoldEngine.Serialization;

public abstract class SaveOperation
{
    public delegate void ArrayWriter(ref Array arr);

    public delegate void CompoundWriter(ref Compound c);

    public delegate void Writer();

    public readonly FieldCollection Options = new FieldCollection();

    public SerializerSuite SerializerSuite = SerializerSuite.Instance;


    public abstract void Close();

    public abstract void Dispose();

    public abstract void Flush();
    
    public abstract SaveOperation StartStruct();

    public abstract SaveOperation EndStruct();

    public abstract SaveOperation Write(bool value);

    public abstract SaveOperation Write(byte value);

    public abstract SaveOperation Write(sbyte value);

    public abstract SaveOperation Write(char ch);

    public abstract SaveOperation Write(double value);

    public abstract SaveOperation Write(decimal value);

    public abstract SaveOperation Write(short value);

    public abstract SaveOperation Write(ushort value);

    public abstract SaveOperation Write(int value);

    public abstract SaveOperation Write(uint value);

    public abstract SaveOperation Write(long value);

    public abstract SaveOperation Write(ulong value);

    public abstract SaveOperation Write(float value);

    public abstract SaveOperation Write(string value);

    public abstract SaveOperation WriteCompound(CompoundWriter writer);

    public abstract SaveOperation WriteArray(ArrayWriter writer);

    public void Write<T>(T element)
    {
        SerializerSuite.Write(element, this);
    }

    protected abstract SaveOperation WriteMember(string name, object value);

    protected abstract SaveOperation WriteMember<T>(string name, T value);

    protected abstract SaveOperation WriteMember<T>(string name, List<T> value);

    protected abstract SaveOperation WriteMember(string name, ISelfSerializer value);

    protected abstract SaveOperation WriteMember(string name, Writer writer);

    public struct Compound
    {
        public SaveOperation SaveOperation;
        internal int MemberCount;

        internal Compound(SaveOperation saveOperation)
        {
            SaveOperation = saveOperation;
            MemberCount = 0;
        }

        public Compound WriteMember(string name, object value)
        {
            SaveOperation.WriteMember(name, value);
            MemberCount++;
            return this;
        }

        public Compound WriteMember<T>(string name, T value)
        {
            SaveOperation.WriteMember<T>(name, value);
            MemberCount++;
            return this;
        }

        public Compound WriteMember<T>(string name, List<T> value)
        {
            SaveOperation.WriteMember<T>(name, value);
            MemberCount++;
            return this;
        }

        public Compound WriteMember(string name, ISelfSerializer value)
        {
            SaveOperation.WriteMember(name, value);
            MemberCount++;
            return this;
        }

        public Compound WriteMember(string name, Writer writer)
        {
            SaveOperation.WriteMember(name, writer);
            MemberCount++;
            return this;
        }
    }

    public struct Array
    {
        public SaveOperation SaveOperation;
        internal int MemberCount;

        internal Array(SaveOperation saveOperation)
        {
            SaveOperation = saveOperation;
            MemberCount = 0;
        }

        public Array WriteMember(object value)
        {
            SaveOperation.WriteMember(null, value);
            MemberCount++;
            return this;
        }

        public Array WriteMember<T>(T value)
        {
            SaveOperation.WriteMember<T>(null, value);
            MemberCount++;
            return this;
        }

        public Array WriteMember<T>(List<T> value)
        {
            SaveOperation.WriteMember<T>(null, value);
            MemberCount++;
            return this;
        }

        public Array WriteMember(ISelfSerializer value)
        {
            SaveOperation.WriteMember(null, value);
            MemberCount++;
            return this;
        }

        public Array WriteMember(Writer writer)
        {
            SaveOperation.WriteMember(null, writer);
            MemberCount++;
            return this;
        }
    }
}