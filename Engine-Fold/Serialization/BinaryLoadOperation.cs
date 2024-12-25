using System;
using System.Collections.Generic;
using System.IO;

namespace FoldEngine.Serialization;

public class BinaryLoadOperation : LoadOperation
{
    private readonly List<string> _stringPool = new List<string>();
    private readonly BinaryReader _reader;
    
    private long Current
    {
        get => _reader.BaseStream.Seek(0, SeekOrigin.Current);
        set => _reader.BaseStream.Seek((int)value, SeekOrigin.Begin);
    }
    
    public BinaryLoadOperation(Stream input)
    {
        _reader = new BinaryReader(input);

        ReadHeader();
    }

    public BinaryLoadOperation(string path)
    {
        _reader = new BinaryReader(new FileStream(path, FileMode.Open));

        ReadHeader();
    }
    

    private void ReadHeader()
    {
        long stringPoolOffset = _reader.ReadInt64();
        _reader.BaseStream.Seek(stringPoolOffset, SeekOrigin.Begin);
        int stringPoolSize = _reader.ReadInt32();
        _stringPool.Capacity = stringPoolSize;
        for (int i = 0; i < stringPoolSize; i++) _stringPool.Add(_reader.ReadString());

        _reader.BaseStream.Seek(8, SeekOrigin.Begin);
    }

    public long Seek(int offset, SeekOrigin origin)
    {
        return _reader.BaseStream.Seek(offset, origin);
    }
    
    
    public override void Close()
    {
        _reader.Close();
    }

    public override void Dispose()
    {
        _reader.Dispose();
    }

    public override LoadOperation StartStruct()
    {
        return this;
    }

    public override LoadOperation EndStruct()
    {
        return this;
    }

    public override bool ReadBoolean()
    {
        return _reader.ReadBoolean();
    }

    public override byte ReadByte()
    {
        return _reader.ReadByte();
    }

    public override sbyte ReadSByte()
    {
        return _reader.ReadSByte();
    }

    public override char ReadChar()
    {
        return _reader.ReadChar();
    }

    public override short ReadInt16()
    {
        return _reader.ReadInt16();
    }

    public override ushort ReadUInt16()
    {
        return _reader.ReadUInt16();
    }

    public override int ReadInt32()
    {
        return _reader.ReadInt32();
    }

    public override uint ReadUInt32()
    {
        return _reader.ReadUInt32();
    }

    public override long ReadInt64()
    {
        return _reader.ReadInt64();
    }

    public override ulong ReadUInt64()
    {
        return _reader.ReadUInt64();
    }

    public override float ReadSingle()
    {
        return _reader.ReadSingle();
    }

    public override double ReadDouble()
    {
        return _reader.ReadDouble();
    }

    public override decimal ReadDecimal()
    {
        return _reader.ReadDecimal();
    }

    public override string ReadString()
    {
        return _stringPool[_reader.ReadInt32()];
    }

    public override T Read<T>()
    {
        return SerializerSuite.Read<T>(this);
    }

    public override object Read(Type type)
    {
        return SerializerSuite.Read(type, this);
    }

    public override void ReadCompound(CompoundMemberReader reader)
    {
        int memberCount = _reader.ReadInt32();

        for (int i = 0; i < memberCount; i++)
        {
            string memberName = ReadString();
            int byteLength = ReadInt32();
            long nextAddress = Current + byteLength;
            reader(new CompoundMember()
            {
                Reader = this,
                Name = memberName
            });
            Current = nextAddress;
        }
    }

    public override void ReadArray(ArrayHeaderReader headerReader, ArrayMemberReader reader)
    {
        int memberCount = _reader.ReadInt32();

        var header = new ArrayHeader()
        {
            Reader = this,
            MemberCount = memberCount
        };
        
        if (headerReader != null)
        {
            long startAddress = Current;
            headerReader(header);
            Current = startAddress;
        }

        for (int i = 0; i < memberCount; i++)
        {
            int byteLength = ReadInt32();
            long nextAddress = Current + byteLength;
            reader(new ArrayMember()
            {
                Array = header,
                Index = i
            });
            Current = nextAddress;
        }
    }
}