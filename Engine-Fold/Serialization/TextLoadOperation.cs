using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FoldEngine.Serialization;

public class TextLoadOperation : LoadOperation
{
    private JTokenReader _reader;
    
    public TextLoadOperation(Stream output)
    {
        using (var streamReader = new StreamReader(output))
        {
            string asString = streamReader.ReadToEnd();
            _reader = new JTokenReader(JToken.Parse(asString));
        }
    }
    
    public override void Close()
    {
        // _stream.Close();
    }

    public override void Dispose()
    {
        // _stream.Dispose();
    }

    public override LoadOperation StartStruct()
    {
        ReadToken(JsonToken.StartArray);
        return this;
    }

    public override LoadOperation EndStruct()
    {
        ReadToken(JsonToken.EndArray);
        return this;
    }

    public override bool ReadBoolean()
    {
        return _reader.ReadAsBoolean().Value;
    }

    public override byte ReadByte()
    {
        return (byte)_reader.ReadAsInt32().Value;
    }

    public override sbyte ReadSByte()
    {
        return (sbyte)_reader.ReadAsInt32().Value;
    }

    public override char ReadChar()
    {
        return _reader.ReadAsString()[0];
    }

    public override short ReadInt16()
    {
        return ((short)_reader.ReadAsInt32().Value);
    }

    public override ushort ReadUInt16()
    {
        return ((ushort)_reader.ReadAsInt32().Value);
    }

    public override int ReadInt32()
    {
        return _reader.ReadAsInt32().Value;
    }

    public override uint ReadUInt32()
    {
        return ((uint)_reader.ReadAsDecimal().Value);
    }

    public override long ReadInt64()
    {
        return ((long)_reader.ReadAsDecimal().Value);
    }

    public override ulong ReadUInt64()
    {
        return ((ulong)_reader.ReadAsDecimal().Value);
    }

    public override float ReadSingle()
    {
        return ((float)_reader.ReadAsDouble().Value);
    }

    public override double ReadDouble()
    {
        return _reader.ReadAsDouble().Value;
    }

    public override decimal ReadDecimal()
    {
        return _reader.ReadAsDecimal().Value;
    }

    public override string ReadString()
    {
        return _reader.ReadAsString();
    }

    public override T Read<T>()
    {
        return SerializerSuite.Read<T>(this);
    }

    public override object Read(Type type)
    {
        return SerializerSuite.Read(type, this);
    }

    private bool ReadToken(JsonToken type)
    {
        if (_reader.Read())
        {
            if (_reader.TokenType == type) return true;
        }

        throw new FormatException($"Expected token type {type}, found {_reader.TokenType};");
    }

    public override void ReadCompound(CompoundMemberReader reader)
    {
        int outerDepth = _reader.Depth;
        ReadToken(JsonToken.StartObject);
        int innerDepth = outerDepth+1;

        while (_reader.Read() && !(_reader.TokenType == JsonToken.EndObject && _reader.Depth == outerDepth))
        {
            if (_reader.TokenType == JsonToken.PropertyName && _reader.Depth == innerDepth)
            {
                var memberName = (string)_reader.Value;

                var member = new CompoundMember()
                {
                    Reader = this,
                    Name = memberName
                };
                reader(member);
                if (member.Skipped)
                {
                    _reader.Skip();
                }
            }
        }
    }

    public override void ReadArray(ArrayHeaderReader headerReader, ArrayMemberReader reader)
    {
        ReadToken(JsonToken.StartArray);

        var header = new ArrayHeader()
        {
            Reader = this,
            MemberCount = (_reader.CurrentToken as JArray)?.Count ?? -1
        };
        headerReader?.Invoke(header);

        var index = 0;

        while (index < header.MemberCount)
        {
            var member = new ArrayMember()
            {
                Array = header,
                Index = index++
            };
            
            reader(member);
            if (member.Skipped)
            {
                _reader.Skip();
            }
        }
    }
}