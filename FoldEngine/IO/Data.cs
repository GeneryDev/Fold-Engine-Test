using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FoldEngine.IO {
    public static class Data {
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;

        private static string InData(string path) {
#if DEBUG
            return Path.Combine(
                Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : "../../../../../",
                "data", path);
#else
            return Path.Combine("data", path);
#endif
        }

        public static class In {
            public static string ReadString(string path) {
                path = InData(path);
                return File.ReadAllText(path, DefaultEncoding);
            }

            public static byte[] ReadBytes(string path) {
                path = InData(path);
                return File.ReadAllBytes(path);
            }

            public static FileStream Stream(string path) {
                path = InData(path);
                return File.OpenRead(path);
            }

            public static bool Exists(string path) {
                path = InData(path);
                return File.Exists(path) || Directory.Exists(path);
            }

            public static bool IsDirectory(string path) {
                path = InData(path);
                return Directory.Exists(path);
            }

            public static IEnumerable<string> ListEntries(string path) {
                path = InData(path);
                return Directory.EnumerateFileSystemEntries(path);
            }
        }

        public static class Out {
            private static void PrepareForWriting(string path) {
                string parent = Path.GetDirectoryName(path);
                if(parent != null) Directory.CreateDirectory(parent);
            }

            public static void WriteString(string path, string contents) {
                path = InData(path);
                PrepareForWriting(path);
                File.WriteAllText(path, contents, DefaultEncoding);
            }

            public static void WriteBytes(string path, byte[] contents) {
                path = InData(path);
                PrepareForWriting(path);
                File.WriteAllBytes(path, contents);
            }

            public static FileStream Stream(string path) {
                path = InData(path);
                PrepareForWriting(path);
                return File.OpenWrite(path);
            }
        }
    }
}