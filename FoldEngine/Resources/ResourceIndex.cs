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
                ResourceAttribute resourceAttribute = Resource.AttributeOf(type);
                
                Console.WriteLine($"  {type}:");
                Dictionary<string, string> paths = _identifierToPathMap[type] = new Dictionary<string, string>();
                Update(paths, Path.Combine("resources", resourceAttribute.DirectoryName), resourceAttribute);
                foreach(KeyValuePair<string, string> id in paths) {
                    Console.WriteLine($"    {id.Key} at {id.Value}");
                }
            }
        }

        private void Update(Dictionary<string, string> paths, string path, ResourceAttribute resourceAttribute, string relativeTo = null) {
            relativeTo = relativeTo ?? path;
            if(Data.In.IsDirectory(path)) {
                foreach(string entry in Data.In.ListEntries(path)) {
                    Update(paths, Path.Combine(path, Path.GetFileName(entry)), resourceAttribute, relativeTo);
                }
            } else {
                string extension = Path.GetExtension(path);
                foreach(string expectedExtension in resourceAttribute.Extensions) {
                    if(extension == "." + expectedExtension) {
                        string id = Path.ChangeExtension(path.Substring(relativeTo.Length+1), null)
                            .Replace(Path.DirectorySeparatorChar, '/');
                    
                        paths[id] = path;
                        break;
                    }
                }
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