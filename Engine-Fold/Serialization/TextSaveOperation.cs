using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FoldEngine.Serialization;

public class TextSaveOperation : SaveOperation
{
    private readonly TextWriter _stream;
    private readonly JsonWriter _writer;
    
    public TextSaveOperation(Stream output)
    {
        _stream = new StreamWriter(output);
        _writer = new JsonTextWriter(_stream);
        _writer.Formatting = Formatting.Indented;
    }
    
    public override void Close()
    {
        _writer.Close();
    }

    public override void Dispose()
    {
        _stream.Dispose();
    }

    public override void Flush()
    {
        _writer.Flush();
    }

    public override SaveOperation StartStruct(bool compactFormatting = true)
    {
        _writer.WriteStartArray();
        if (compactFormatting)
        {
            _writer.Formatting = Formatting.None;
        }
        return this;
    }

    public override SaveOperation EndStruct()
    {
        _writer.WriteEndArray();
        _writer.Formatting = Formatting.Indented;
        return this;
    }

    public override SaveOperation Write(bool value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(byte value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(sbyte value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(char ch)
    {
        _writer.WriteValue(ch);
        return this;
    }

    public override SaveOperation Write(double value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(decimal value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(short value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(ushort value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(int value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(uint value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(long value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(ulong value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(float value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation Write(string value)
    {
        _writer.WriteValue(value);
        return this;
    }

    public override SaveOperation WriteCompound(CompoundWriter writer)
    {
        var c = new Compound(this);

        _writer.WriteStartObject();
        writer(ref c);
        _writer.WriteEndObject();

        return this;
    }

    public override SaveOperation WriteArray(ArrayWriter writer)
    {
        var arr = new Array(this);

        _writer.WriteStartArray();
        writer(ref arr);
        _writer.WriteEndArray();

        return this;
    }

    protected override SaveOperation WriteMember(string name, object value)
    {
        // Write member name;
        if (name != null)
        {
            _writer.WritePropertyName(name);
        }
        
        SerializerSuite.Write(value, this);
        return this;
    }

    protected override SaveOperation WriteMember<T>(string name, T value)
    {
        // Write member name;
        if (name != null)
        {
            _writer.WritePropertyName(name);
        }
        
        SerializerSuite.Write(value, this);
        return this;
    }

    protected override SaveOperation WriteMember<T>(string name, List<T> value)
    {
        // Write member name;
        if (name != null)
        {
            _writer.WritePropertyName(name);
        }
        
        SerializerSuite.Write(value, this);
        return this;
    }

    protected override SaveOperation WriteMember(string name, ISelfSerializer value)
    {
        // Write member name;
        if (name != null)
        {
            _writer.WritePropertyName(name);
        }
        
        SerializerSuite.Write(value, this);
        return this;
    }

    protected override SaveOperation WriteMember(string name, Writer writer)
    {
        // Write member name;
        if (name != null)
        {
            _writer.WritePropertyName(name);
        }
        
        writer();
        return this;
    }
}