using System.Collections.Generic;
using System.IO;

namespace FoldEngine.Serialization;

public class BinarySaveOperation : SaveOperation
{
    private readonly List<string> _stringPool = new List<string>();
    private readonly BinaryWriter _writer;
    
    private long Current
    {
        get => _writer.Seek(0, SeekOrigin.Current);
        set => _writer.Seek((int)value, SeekOrigin.Begin);
    }
    
    public BinarySaveOperation(Stream output)
    {
        _writer = new BinaryWriter(output);

        _writer.Write(0L);
    }

    public BinarySaveOperation(string path)
    {
        _writer = new BinaryWriter(new FileStream(path, FileMode.Create));

        _writer.Write(0L);
    }
    
    public override void Close()
    {
        long head = Current;

        // Write string pool at the end of the file
        _writer.Write(_stringPool.Count);
        foreach (string str in _stringPool) _writer.Write(str);

        // Write address of the string pool at the beginning
        _writer.Seek(0, SeekOrigin.Begin);
        _writer.Write(head);

        // End
        _writer.Close();
    }

    public override void Dispose()
    {
        _writer.Dispose();
    }

    public override void Flush()
    {
        _writer.Flush();
    }

    public override SaveOperation StartStruct()
    {
        return this;
    }

    public override SaveOperation EndStruct()
    {
        return this;
    }

    public override SaveOperation Write(bool value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(byte value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(sbyte value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(char ch)
    {
        _writer.Write(ch);
        return this;
    }

    public override SaveOperation Write(double value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(decimal value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(short value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(ushort value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(int value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(uint value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(long value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(ulong value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(float value)
    {
        _writer.Write(value);
        return this;
    }

    public override SaveOperation Write(string value)
    {
        int index = _stringPool.IndexOf(value);
        if (index == -1)
        {
            index = _stringPool.Count;
            _stringPool.Add(value);
        }

        _writer.Write(index);
        return this;
    }
    
    public override SaveOperation WriteCompound(CompoundWriter writer)
    {
        var c = new Compound(this);

        long lengthOffset = Current;
        _writer.Write(0);

        writer(ref c);
        int length = c.MemberCount;

        long endOffset = Current;
        Current = lengthOffset;
        _writer.Write(length);
        Current = endOffset;

        return this;
    }

    public override SaveOperation WriteArray(ArrayWriter writer)
    {
        var arr = new Array(this);

        long lengthOffset = Current;
        _writer.Write(0);

        writer(ref arr);
        int length = arr.MemberCount;

        long endOffset = Current;
        Current = lengthOffset;
        _writer.Write(length);
        Current = endOffset;

        return this;
    }

    protected override SaveOperation WriteMember(string name, object value)
    {
        // Write member name;
        if (name != null) Write(name);

        // Write an int for the length in bytes (will be overwritten later)
        long lengthPosition = Current;
        Write(0);

        // Write the data
        SerializerSuite.Write(value, this);

        // Calculate length in bytes
        long endPosition = Current;
        int length = (int)(endPosition - lengthPosition - 4);

        // Write the length and return
        Current = lengthPosition;
        Write(length);
        Current = endPosition;

        return this;
    }

    protected override SaveOperation WriteMember<T>(string name, T value)
    {
        // Write member name;
        if (name != null) Write(name);

        // Write an int for the length in bytes (will be overwritten later)
        long lengthPosition = Current;
        Write(0);

        // Write the data
        SerializerSuite.Write(value, this);

        // Calculate length in bytes
        long endPosition = Current;
        int length = (int)(endPosition - lengthPosition - 4);

        // Write the length and return
        Current = lengthPosition;
        Write(length);
        Current = endPosition;

        return this;
    }

    protected override SaveOperation WriteMember<T>(string name, List<T> value)
    {
        // Write member name;
        if (name != null) Write(name);

        // Write an int for the length in bytes (will be overwritten later)
        long lengthPosition = Current;
        Write(0);

        // Write the data
        SerializerSuite.Write(value, this);

        // Calculate length in bytes
        long endPosition = Current;
        int length = (int)(endPosition - lengthPosition - 4);

        // Write the length and return
        Current = lengthPosition;
        Write(length);
        Current = endPosition;

        return this;
    }

    protected override SaveOperation WriteMember(string name, ISelfSerializer value)
    {
        // Write member name;
        if (name != null) Write(name);

        // Write an int for the length in bytes (will be overwritten later)
        long lengthPosition = Current;
        Write(0);

        // Write the data
        SerializerSuite.Write(value, this);

        // Calculate length in bytes
        long endPosition = Current;
        int length = (int)(endPosition - lengthPosition - 4);

        // Write the length and return
        Current = lengthPosition;
        Write(length);
        Current = endPosition;

        return this;
    }

    protected override SaveOperation WriteMember(string name, Writer writer)
    {
        // Write member name;
        if (name != null) Write(name);

        // Write an int for the length in bytes (will be overwritten later)
        long lengthPosition = Current;
        Write(0);

        // Write the data
        writer();

        // Calculate length in bytes
        long endPosition = Current;
        int length = (int)(endPosition - lengthPosition - 4);

        // Write the length and return
        Current = lengthPosition;
        Write(length);
        Current = endPosition;

        return this;
    }
}