using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using FoldEngine.Scenes;

namespace FoldEngine.Events {
    public static class Event {
        private static Dictionary<Type, string> _typeToIdentifierMap = null;
        private static Dictionary<string, Type> _identifierToTypeMap = null;

        public static string IdentifierOf(Type type) {
            if(_typeToIdentifierMap == null) {
                _typeToIdentifierMap = new Dictionary<Type, string>();
            }

            if(!_typeToIdentifierMap.ContainsKey(type)) {
                _typeToIdentifierMap[type] =
                    (type.GetCustomAttributes(typeof(EventAttribute), false)[0] as EventAttribute).EventIdentifier;
            }

            return _typeToIdentifierMap[type];
        }

        public static string IdentifierOf<T>() where T : struct {
            return IdentifierOf(typeof(T));
        }

        public static Type TypeForIdentifier(string identifier) {
            PopulateIdentifiers();
            return _identifierToTypeMap[identifier];
        }

        public static void PopulateDictionaryWithAssembly(Assembly assembly) {
            if(_identifierToTypeMap == null) _identifierToTypeMap = new Dictionary<string, Type>();
            foreach(Type type in assembly.GetTypes()) {
                if(type.IsValueType) {
                    object[] attributes;
                    if((attributes = type.GetCustomAttributes(typeof(EventAttribute), false)).Length > 0) {
                        string thisIdentifier = (attributes[0] as EventAttribute).EventIdentifier;
                        _identifierToTypeMap[thisIdentifier] = type;
                    }
                }
            }
        }

        public static void PopulateIdentifiers() {
            if(_identifierToTypeMap == null) {
                PopulateDictionaryWithAssembly(Assembly.GetEntryAssembly());
            }
        }

        public delegate void EventListener<T>(ref T evt);
    }

    public sealed class EventAttribute : Attribute {
        public readonly string EventIdentifier;
        public readonly EventFlushMode FlushMode;

        public EventAttribute(string eventIdentifier, EventFlushMode flushMode = EventFlushMode.AfterSystem) {
            EventIdentifier = eventIdentifier;
            FlushMode = flushMode;
        }
    }

    public enum EventFlushMode {
        Immediate,
        AfterSystem,
        End
    }
}