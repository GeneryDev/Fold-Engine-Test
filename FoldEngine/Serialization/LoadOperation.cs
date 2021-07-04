using System;
using System.Collections.Generic;
using System.IO;
using EntryProject.Util;
using FoldEngine.Scenes;

namespace FoldEngine.Serialization {
    public class LoadOperation {
        private readonly string _path;
        private readonly BinaryReader _reader;
        public readonly FieldCollection Options = new FieldCollection();
        
        private readonly List<string> _stringPool = new List<string>();
        
        public SerializerSuite SerializerSuite = SerializerSuite.Instance;
        
        public long Current {
            get => _reader.BaseStream.Seek(0, SeekOrigin.Current);
            set => _reader.BaseStream.Seek((int) value, SeekOrigin.Begin);
        }

        public LoadOperation(Stream input) {
            _reader = new BinaryReader(input);
            
            ReadHeader();
        }

        public LoadOperation(string path) {
            _path = path;

            _reader = new BinaryReader(new FileStream(_path, FileMode.Open));

            ReadHeader();
        }

        private void ReadHeader() {
            long stringPoolOffset = _reader.ReadInt64();
            _reader.BaseStream.Seek(stringPoolOffset, SeekOrigin.Begin);
            int stringPoolSize = _reader.ReadInt32();
            _stringPool.Capacity = stringPoolSize;
            for(int i = 0; i < stringPoolSize; i++) {
                _stringPool.Add(_reader.ReadString());
            }

            _reader.BaseStream.Seek(8, SeekOrigin.Begin);
        }


        public void Close() {
            _reader.Close();
        }

        public void Dispose() {
            _reader.Dispose();
        }

        public long Seek(int offset, SeekOrigin origin) {
            return _reader.BaseStream.Seek(offset, origin);
        }

        public int PeekChar() {
            return _reader.PeekChar();
        }

        public bool ReadBoolean() {
            return _reader.ReadBoolean();
        }

        public byte ReadByte() {
            return _reader.ReadByte();
        }

        public sbyte ReadSByte() {
            return _reader.ReadSByte();
        }

        public char ReadChar() {
            return _reader.ReadChar();
        }

        public short ReadInt16() {
            return _reader.ReadInt16();
        }

        public ushort ReadUInt16() {
            return _reader.ReadUInt16();
        }

        public int ReadInt32() {
            return _reader.ReadInt32();
        }

        public uint ReadUInt32() {
            return _reader.ReadUInt32();
        }

        public long ReadInt64() {
            return _reader.ReadInt64();
        }

        public ulong ReadUInt64() {
            return _reader.ReadUInt64();
        }

        public float ReadSingle() {
            return _reader.ReadSingle();
        }

        public double ReadDouble() {
            return _reader.ReadDouble();
        }

        public decimal ReadDecimal() {
            return _reader.ReadDecimal();
        }

        public string ReadString() {
            return _stringPool[_reader.ReadInt32()];
        }

        public T Read<T>() {
            return SerializerSuite.Read<T>(this);
        }

        public object Read(Type type) {
            return SerializerSuite.Read(type, this);
        }

        public void ReadCompound(CompoundReader reader) {
            int memberCount = _reader.ReadInt32();
            var compound = new Compound() {
                LoadOperation = this,
                MemberCount = memberCount,
                MemberNames = new string[memberCount],
                MemberDataOffsets = new long[memberCount],
                MemberDataLengths = new int[memberCount]
            };
            
            for(int i = 0; i < memberCount; i++) {
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

        public void ReadArray(ArrayReader reader) {
            int memberCount = _reader.ReadInt32();
            var array = new Array() {
                LoadOperation = this,
                MemberCount = memberCount,
                MemberDataOffsets = new long[memberCount],
                MemberDataLengths = new int[memberCount]
            };
            
            for(int i = 0; i < memberCount; i++) {
                int byteLength = ReadInt32();
                array.MemberDataOffsets[i] = Current;
                array.MemberDataLengths[i] = byteLength;
                Current += byteLength;
            }

            long end = Current;

            reader(array);

            Current = end;
        }

        public delegate void CompoundReader(Compound c);
        public delegate void ArrayReader(Array c);

        public struct Compound {
            public LoadOperation LoadOperation;
            public int MemberCount;
            public string[] MemberNames;
            public long[] MemberDataOffsets;
            public int[] MemberDataLengths;

            public bool HasMember(string name) {
                foreach(string memberName in MemberNames) {
                    if(memberName == name) return true;
                }
                return false;
            }

            private int IndexOfMember(string name) {
                for(int i = 0; i < MemberCount; i++) {
                    if(MemberNames[i] == name) return i;
                }
                return -1;
            }

            public void StartReadMember(string name) {
                int memberIndex = IndexOfMember(name);
                
                if(memberIndex == -1) throw new ArgumentException($"No such member exists: {name}");

                LoadOperation.Current = MemberDataOffsets[memberIndex];
            }

            public T GetMember<T>(string name) {
                int memberIndex = IndexOfMember(name);
                
                if(memberIndex == -1) throw new ArgumentException($"No such member exists: {name}");

                LoadOperation.Current = MemberDataOffsets[memberIndex];

                return LoadOperation.SerializerSuite.Read<T>(LoadOperation);
            }

            public List<T> GetListMember<T>(string name) {
                int memberIndex = IndexOfMember(name);
                
                if(memberIndex == -1) throw new ArgumentException($"No such member exists: {name}");

                LoadOperation.Current = MemberDataOffsets[memberIndex];

                return LoadOperation.SerializerSuite.ReadList<T>(LoadOperation);
            }

            public void DeserializeMember(string name, ISelfSerializer selfSerializer) {
                int memberIndex = IndexOfMember(name);
                
                if(memberIndex == -1) throw new ArgumentException($"No such member exists: {name}");

                LoadOperation.Current = MemberDataOffsets[memberIndex];

                selfSerializer.Deserialize(LoadOperation);
            }
        }

        public struct Array {
            public LoadOperation LoadOperation;
            public int MemberCount;
            public long[] MemberDataOffsets;
            public int[] MemberDataLengths;

            public bool HasMember(int index) {
                return index < MemberCount;
            }

            public void StartReadMember(int index) {
                LoadOperation.Current = MemberDataOffsets[index];
            }

            public T GetMember<T>(int index) {
                LoadOperation.Current = MemberDataOffsets[index];

                return LoadOperation.SerializerSuite.Read<T>(LoadOperation);
            }

            public List<T> GetListMember<T>(int index) {
                LoadOperation.Current = MemberDataOffsets[index];

                return LoadOperation.SerializerSuite.ReadList<T>(LoadOperation);
            }

            public void DeserializeMember(int index, ISelfSerializer selfSerializer) {
                LoadOperation.Current = MemberDataOffsets[index];

                selfSerializer.Deserialize(LoadOperation);
            }
        }
    }
}