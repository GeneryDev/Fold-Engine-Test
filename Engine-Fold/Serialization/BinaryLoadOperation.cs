﻿using System;
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

    public override void ReadCompound(CompoundReader reader)
    {
        int memberCount = _reader.ReadInt32();
        var compound = new Compound
        {
            LoadOperation = this,
            MemberCount = memberCount,
            MemberNames = new string[memberCount],
            MemberDataOffsets = new long[memberCount],
            MemberDataLengths = new int[memberCount]
        };

        for (int i = 0; i < memberCount; i++)
        {
            compound.MemberNames[i] = ReadString();
            int byteLength = ReadInt32();
            compound.MemberDataOffsets[i] = Current;
            compound.MemberDataLengths[i] = byteLength;
            Current += byteLength;
        }

        long end = Current;

        reader(compound);

        Current = end;
    }

    public override void ReadArray(ArrayReader reader)
    {
        int memberCount = _reader.ReadInt32();
        var array = new Array
        {
            LoadOperation = this,
            MemberCount = memberCount,
            MemberDataOffsets = new long[memberCount],
            MemberDataLengths = new int[memberCount]
        };

        for (int i = 0; i < memberCount; i++)
        {
            int byteLength = ReadInt32();
            array.MemberDataOffsets[i] = Current;
            array.MemberDataLengths[i] = byteLength;
            Current += byteLength;
        }

        long end = Current;

        reader(array);

        Current = end;
    }

    private int IndexOfCompoundMember(Compound compound, string name)
    {
        for (int i = 0; i < compound.MemberCount; i++)
            if (compound.MemberNames[i] == name)
                return i;
        return -1;
    }

    protected override void StartReadCompoundMember(Compound compound, string name)
    {
        int memberIndex = IndexOfCompoundMember(compound, name);

        if (memberIndex == -1) throw new ArgumentException($"No such member exists: {name}");

        Current = compound.MemberDataOffsets[memberIndex];
    }

    protected override void StartReadArrayMember(Array compound, int index)
    {
        Current = compound.MemberDataOffsets[index];
    }
}