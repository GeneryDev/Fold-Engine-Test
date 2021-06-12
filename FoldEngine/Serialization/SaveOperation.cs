using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Scenes;

namespace FoldEngine.Serialization {
    public class SaveOperation {
        private readonly string _path;
        private readonly BinaryWriter _writer;
        
        private readonly List<string> _stringPool = new List<string>();
        
        public SerializerSuite SerializerSuite = SerializerSuite.Instance;

        public long Current {
            get => _writer.Seek(0, SeekOrigin.Current);
            set => _writer.Seek((int) value, SeekOrigin.Begin);
        }

        public SaveOperation(string path) {
            _path = path;

            _writer = new BinaryWriter(new FileStream(_path, FileMode.Create));
            
            _writer.Write(0L);
        }

        public SaveOperation WriteCompound(CompoundWriter writer) {
            
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

        public SaveOperation WriteArray(ArrayWriter writer) {

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

        public void Close() {
            long head = Current;
            
            // Write string pool at the end of the file
            _writer.Write(_stringPool.Count);
            foreach(string str in _stringPool) {
                _writer.Write(str);
            }

            // Write address of the string pool at the beginning
            _writer.Seek(0, SeekOrigin.Begin);
            _writer.Write(head);
            
            // End
            _writer.Close();
        }

        public void Dispose() {
            _writer.Dispose();
        }

        public void Flush() {
            _writer.Flush();
        }

        public long Seek(int offset, SeekOrigin origin) {
            return _writer.Seek(offset, origin);
        }

        public SaveOperation Write(bool value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(byte value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(sbyte value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(char ch) {
            _writer.Write(ch);
            return this;
        }

        public SaveOperation Write(double value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(decimal value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(short value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(ushort value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(int value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(uint value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(long value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(ulong value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(float value) {
            _writer.Write(value);
            return this;
        }

        public SaveOperation Write(string value) {
            int index = _stringPool.IndexOf(value);
            if(index == -1) {
                index = _stringPool.Count;
                _stringPool.Add(value);
            }
            _writer.Write(index);
            return this;
        }

        public void Write<T>(T element) {
            SerializerSuite.Write(element, this);
        }

        private SaveOperation WriteMember(string name, object value) {
            // Write member name;
            if(name != null) Write(name);
            
            // Write an int for the length in bytes (will be overwritten later)
            long lengthPosition = Current;
            Write(0);
            
            // Write the data
            SerializerSuite.Write(value, this);
            
            // Calculate length in bytes
            long endPosition = Current;
            int length = (int) (endPosition - lengthPosition - 4);
            
            // Write the length and return
            Current = lengthPosition;
            Write(length);
            Current = endPosition;

            return this;
        }

        private SaveOperation WriteMember<T>(string name, T value) {
            // Write member name;
            if(name != null) Write(name);
            
            // Write an int for the length in bytes (will be overwritten later)
            long lengthPosition = Current;
            Write(0);
            
            // Write the data
            SerializerSuite.Write(value, this);
            
            // Calculate length in bytes
            long endPosition = Current;
            int length = (int) (endPosition - lengthPosition - 4);
            
            // Write the length and return
            Current = lengthPosition;
            Write(length);
            Current = endPosition;

            return this;
        }

        private SaveOperation WriteMember<T>(string name, List<T> value) {
            // Write member name;
            if(name != null) Write(name);
            
            // Write an int for the length in bytes (will be overwritten later)
            long lengthPosition = Current;
            Write(0);
            
            // Write the data
            SerializerSuite.Write(value, this);
            
            // Calculate length in bytes
            long endPosition = Current;
            int length = (int) (endPosition - lengthPosition - 4);
            
            // Write the length and return
            Current = lengthPosition;
            Write(length);
            Current = endPosition;

            return this;
        }

        private SaveOperation WriteMember(string name, ISelfSerializer value) {
            // Write member name;
            if(name != null) Write(name);
            
            // Write an int for the length in bytes (will be overwritten later)
            long lengthPosition = Current;
            Write(0);
            
            // Write the data
            SerializerSuite.Write(value, this);
            
            // Calculate length in bytes
            long endPosition = Current;
            int length = (int) (endPosition - lengthPosition - 4);
            
            // Write the length and return
            Current = lengthPosition;
            Write(length);
            Current = endPosition;

            return this;
        }

        private SaveOperation WriteMember(string name, Writer writer) {
            // Write member name;
            if(name != null) Write(name);
            
            // Write an int for the length in bytes (will be overwritten later)
            long lengthPosition = Current;
            Write(0);
            
            // Write the data
            writer();
            
            // Calculate length in bytes
            long endPosition = Current;
            int length = (int) (endPosition - lengthPosition - 4);
            
            // Write the length and return
            Current = lengthPosition;
            Write(length);
            Current = endPosition;

            return this;
        }

        public delegate void Writer();
        
        public delegate void CompoundWriter(ref Compound c);
        public delegate void ArrayWriter(ref Array arr);

        public struct Compound {
            public SaveOperation SaveOperation;
            internal int MemberCount;

            internal Compound(SaveOperation saveOperation) {
                SaveOperation = saveOperation;
                MemberCount = 0;
            }

            public Compound WriteMember(string name, object value) {
                SaveOperation.WriteMember(name, value);
                MemberCount++;
                return this;
            }

            public Compound WriteMember<T>(string name, T value) {
                SaveOperation.WriteMember<T>(name, value);
                MemberCount++;
                return this;
            }

            public Compound WriteMember<T>(string name, List<T> value) {
                SaveOperation.WriteMember<T>(name, value);
                MemberCount++;
                return this;
            }

            public Compound WriteMember(string name, ISelfSerializer value) {
                SaveOperation.WriteMember(name, (ISelfSerializer) value);
                MemberCount++;
                return this;
            }

            public Compound WriteMember(string name, Writer writer) {
                SaveOperation.WriteMember(name, writer);
                MemberCount++;
                return this;
            }
        }
        public struct Array {
            public SaveOperation SaveOperation;
            internal int MemberCount;

            internal Array(SaveOperation saveOperation) {
                SaveOperation = saveOperation;
                MemberCount = 0;
            }

            public Array WriteMember(object value) {
                SaveOperation.WriteMember(null, value);
                MemberCount++;
                return this;
            }

            public Array WriteMember<T>(T value) {
                SaveOperation.WriteMember<T>(null, value);
                MemberCount++;
                return this;
            }

            public Array WriteMember<T>(List<T> value) {
                SaveOperation.WriteMember<T>(null, value);
                MemberCount++;
                return this;
            }

            public Array WriteMember(ISelfSerializer value) {
                SaveOperation.WriteMember(null, (ISelfSerializer) value);
                MemberCount++;
                return this;
            }

            public Array WriteMember(Writer writer) {
                SaveOperation.WriteMember(null, writer);
                MemberCount++;
                return this;
            }
        }
    }
}