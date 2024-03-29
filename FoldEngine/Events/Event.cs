﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace FoldEngine.Events {
    public static class Event {
        public delegate void EventListener<T>(ref T evt);

        private static Dictionary<Type, string> _typeToIdentifierMap;
        private static Dictionary<string, Type> _identifierToTypeMap;

        public static string IdentifierOf(Type type) {
            if(_typeToIdentifierMap == null) _typeToIdentifierMap = new Dictionary<Type, string>();

            if(!_typeToIdentifierMap.ContainsKey(type)) {
                object[] matchingAttributes = type.GetCustomAttributes(typeof(EventAttribute), false);
                if(matchingAttributes.Length == 0) throw new ArgumentException($"Type '{type}' is not an event type");
                _typeToIdentifierMap[type] =
                    (matchingAttributes[0] as EventAttribute).EventIdentifier;
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
            foreach(Type type in assembly.GetTypes())
                if(type.IsValueType) {
                    object[] attributes;
                    if((attributes = type.GetCustomAttributes(typeof(EventAttribute), false)).Length > 0) {
                        string thisIdentifier = (attributes[0] as EventAttribute).EventIdentifier;
                        _identifierToTypeMap[thisIdentifier] = type;
                    }
                }
        }

        public static void PopulateIdentifiers() {
            if(_identifierToTypeMap == null) {
                PopulateDictionaryWithAssembly(Assembly.GetAssembly(typeof(Event)));
                PopulateDictionaryWithAssembly(Assembly.GetEntryAssembly());
            }
        }
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