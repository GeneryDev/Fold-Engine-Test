using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FoldEngine.IO;
using Newtonsoft.Json.Linq;

namespace FoldEngine.Resources {
    public class ResourceIndex {
        private const string GroupDirectoryName = "groups";
        private Dictionary<Type, Dictionary<string, string>> _identifierToPathMap = new Dictionary<Type, Dictionary<string, string>>();
        private Dictionary<Type, Dictionary<string, HashSet<string>>> _groups = new Dictionary<Type, Dictionary<string, HashSet<string>>>();


        public void Update() {
            _identifierToPathMap.Clear();
            _groups.Clear();
            
            HashSet<string> groupsNeedExpanding = new HashSet<string>();

            foreach(Type type in Resource.GetAllTypes()) {
                ResourceAttribute resourceAttribute = Resource.AttributeOf(type);
                
                Dictionary<string, string> paths = _identifierToPathMap[type] = new Dictionary<string, string>();
                ScanResources(paths, Path.Combine("resources", resourceAttribute.DirectoryName), resourceAttribute);
                
                Dictionary<string, HashSet<string>> groups = _groups[type] = new Dictionary<string, HashSet<string>>();
                ScanGroups(groups, paths, groupsNeedExpanding, Path.Combine("resources", resourceAttribute.DirectoryName, GroupDirectoryName), resourceAttribute);

                while(groupsNeedExpanding.Count > 0) {
                    string groupId = null;
                    
                    //just iterating to get the first value since there's no other way to do it idk, and I want to avoid concurrent modification exceptions as I'll be removing values
                    foreach(string id in groupsNeedExpanding) { groupId = id; break; }

                    ExpandGroup(groups, groupsNeedExpanding, groupId);
                }
                groupsNeedExpanding.Clear();
            }

#if DEBUG
            Console.WriteLine("Resources available: ");
            foreach(Type type in Resource.GetAllTypes()) {
                Dictionary<string, string> paths = _identifierToPathMap[type];
                if(paths.Count == 0) continue;
                Console.WriteLine($"  {type}:");
                foreach(KeyValuePair<string, string> id in paths) {
                    Console.WriteLine($"    - {id.Key} at {id.Value}");
                }
            }
            
            Console.WriteLine("Groups available: ");
            foreach(Type type in Resource.GetAllTypes()) {
                Dictionary<string, HashSet<string>> groups = _groups[type];
                if(groups.Count == 0) continue;
                Console.WriteLine($"  {type}:");
                foreach(KeyValuePair<string, HashSet<string>> group in groups) {
                    Console.WriteLine($"    {group.Key}:");
                    foreach(string value in group.Value) {
                        Console.WriteLine($"      - {value}");
                    }
                }
            }
#endif
        }

        private static void ScanResources(
            Dictionary<string, string> paths,
            string path,
            ResourceAttribute resourceAttribute,
            string relativeTo = null) {
            
            relativeTo = relativeTo ?? path;
            if(Data.In.IsDirectory(path)) {
                foreach(string entry in Data.In.ListEntries(path)) {
                    if(relativeTo == path && Path.GetFileName(entry) == GroupDirectoryName) continue;
                    ScanResources(paths, Path.Combine(path, Path.GetFileName(entry)), resourceAttribute, relativeTo);
                }
            } else if(Data.In.Exists(path)) {
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

        private static void ScanGroups(
            Dictionary<string, HashSet<string>> groups,
            Dictionary<string, string> resources,
            HashSet<string> groupsNeedExpanding,
            string path,
            ResourceAttribute resourceAttribute,
            string relativeTo = null) {
            
            relativeTo = relativeTo ?? path;
            if(Data.In.IsDirectory(path)) {
                foreach(string entry in Data.In.ListEntries(path)) {
                    ScanGroups(groups, resources, groupsNeedExpanding, Path.Combine(path, Path.GetFileName(entry)), resourceAttribute, relativeTo);
                }
            } else if(Data.In.Exists(path)) {
                string extension = Path.GetExtension(path);
                if(extension == ".json") {
                    string id = "#" + Path.ChangeExtension(path.Substring(relativeTo.Length+1), null)
                        .Replace(Path.DirectorySeparatorChar, '/');

                    try {
                        HashSet<string> values = ParseGroup(id, path, resources, resourceAttribute, out bool needsExpanding);
                        groups[id] = values;
                        if(needsExpanding) {
                            groupsNeedExpanding.Add(id);
                        }
                    } catch(Exception x) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error trying to load " + resourceAttribute.DirectoryName + " group: " + id + "\n" + x.Message);
                        Console.ResetColor();
                    }
                }
            }
        }

        private static HashSet<string> ParseGroup(string id, string path, Dictionary<string, string> resources, ResourceAttribute resourceAttribute, out bool needsExpanding) {
            JObject root = JObject.Parse(Data.In.ReadString(path));
            HashSet<string> values = new HashSet<string>();
            needsExpanding = false;
            
            if(root["resources"] is JArray rawResources) {
                foreach(JToken jToken in rawResources) {
                    var rawValue = (JValue) jToken;
                    string value = rawValue.Value<string>();
                    if(value == null) continue;

                    if(value.StartsWith("#")) {
                        values.Add(value);
                        needsExpanding = true;
                    } else {
                        if(value.Contains("*")) {
                            string regexPattern = Regex.Escape(value)
                                .Replace("\\*", "*")
                                .Replace("**", "(?:[^/]+\\/?)+")
                                .Replace("*", "(?:[^/])");

                            foreach(string valueCandidate in resources.Keys) {
                                if(Regex.IsMatch(valueCandidate, regexPattern)) {
                                    values.Add(valueCandidate);
                                }
                            }
                        } else {
                            if(resources.ContainsKey(value)) {
                                values.Add(value);
                            } else {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("Unknown value in " + resourceAttribute.DirectoryName + " group " + id + ": " + value);
                                Console.ResetColor();
                            }
                        }
                    }
                }
            }
            
            return values;
        }

        private static void ExpandGroup(Dictionary<string,HashSet<string>> groups, HashSet<string> groupsNeedExpanding, string groupId) {
            if(!groupsNeedExpanding.Contains(groupId)) return;
            groupsNeedExpanding.Remove(groupId);

            groups[groupId]
                .RemoveWhere(v => {
                    if(v.StartsWith("#") && groups.ContainsKey(v)) {
                        if(groupsNeedExpanding.Contains(v)) ExpandGroup(groups, groupsNeedExpanding, v);
                        groups[groupId].UnionWith(groups[v]);
                        return true;
                    }

                    return false;
                });
        }

        public bool Exists(Type type, string identifier) {
            return (_identifierToPathMap.ContainsKey(type) && _identifierToPathMap[type].ContainsKey(identifier)) || (_groups.ContainsKey(type) && _groups[type].ContainsKey(identifier));
        }

        public bool Exists<T>(string identifier) where T : Resource {
            return Exists(typeof(T), identifier);
        }

        public string GetPathForIdentifier(Type type, string identifier) {
            return _identifierToPathMap.ContainsKey(type) && _identifierToPathMap[type].ContainsKey(identifier) ? _identifierToPathMap[type][identifier] : null;
        }

        public bool GroupContains<T>(string group, T resource) where T : Resource {
            return GroupContains(group, typeof(T), resource);
        }

        public bool GroupContains(string group, Type type, Resource resource) {
            return resource != null
                   && _groups.ContainsKey(type)
                   && _groups[type].ContainsKey(group)
                   && _groups[type][group].Contains(resource.Identifier);
        }
        
        public IEnumerable<string> GetIdentifiers<T>() {
            Type type = typeof(T);
            return GetIdentifiers(type);
        }
        
        public IEnumerable<string> GetIdentifiers(Type type) {
            if(_groups.ContainsKey(type)) {
                foreach(string value in _identifierToPathMap[type].Keys) {
                    yield return value;
                }
            }
        }
        
        public IEnumerable<string> GetIdentifiersInGroup<T>(string group) {
            Type type = typeof(T);
            return GetIdentifiersInGroup(type, group);
        }
        
        public IEnumerable<string> GetIdentifiersInGroup(Type type, string group) {
            if(_groups.ContainsKey(type) && _groups[type].ContainsKey(group)) {
                foreach(string value in _groups[type][group]) {
                    yield return value;
                }
            }
        }
    }
}