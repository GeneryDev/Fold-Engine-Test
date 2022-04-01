using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.IO;

namespace FoldEngine.Resources {
    public class ResourceIndex {
        private Dictionary<Type, Dictionary<string, string>> _identifierToPathMap = new Dictionary<Type, Dictionary<string, string>>();

        public void Update() {
            _identifierToPathMap.Clear();

            Console.WriteLine("Resources available: ");
            
            foreach(Type type in Resource.GetAllTypes()) {
                Console.WriteLine($"  {type}:");
                Dictionary<string, string> paths = _identifierToPathMap[type] = new Dictionary<string, string>();
                Update(paths, Path.Combine("resources", Resource.AttributeOf(type).DirectoryName));
                foreach(KeyValuePair<string, string> id in paths) {
                    Console.WriteLine($"    {id.Key} at {id.Value}");
                }
            }
        }

        private void Update(Dictionary<string, string> paths, string path, string relativeTo = null) {
            relativeTo = relativeTo ?? path;
            if(Data.In.IsDirectory(path)) {
                foreach(string entry in Data.In.ListEntries(path)) {
                    Update(paths, Path.Combine(path, Path.GetFileName(entry)), relativeTo);
                }
            } else if(Path.GetExtension(path) == "." + Resource.Extension) {
                string id = Path.ChangeExtension(path.Substring(relativeTo.Length+1), null)
                    .Replace(Path.DirectorySeparatorChar, '/');
                
                paths[id] = path;
            }
        }

        public bool Exists(Type type, string identifier) {
            return _identifierToPathMap.ContainsKey(type) && _identifierToPathMap[type].ContainsKey(identifier);
        }

        public string GetPathForIdentifier(Type type, string identifier) {
            return _identifierToPathMap.ContainsKey(type) && _identifierToPathMap[type].ContainsKey(identifier) ? _identifierToPathMap[type][identifier] : null;
        }
    }
}