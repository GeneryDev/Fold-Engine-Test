using FoldEngine.Components;
using FoldEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using FoldEngine.Events;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Systems {
    public abstract class GameSystem {
        public Scene Owner { get; internal set; }

        private readonly GameSystemAttribute _attribute;
        public string SystemName => _attribute.SystemName;
        public ProcessingCycles ProcessingCycles => _attribute.ProcessingCycles;
        public readonly Type[] Listening;

        protected GameSystem() {
            _attribute = (GameSystemAttribute) this.GetType().GetCustomAttribute(typeof(GameSystemAttribute));
            Listening = ((ListeningAttribute) this.GetType().GetCustomAttribute(typeof(ListeningAttribute)))?.EventTypes ?? new Type[0];
        }

        public virtual void OnInput() { }
        public virtual void OnUpdate() { }
        public virtual void OnRender(Interfaces.IRenderingUnit renderer) { }

        protected MultiComponentIterator CreateComponentIterator(params Type[] watchingTypes) {
            return Owner.Components.CreateMultiIterator(watchingTypes);
        }

        protected ComponentIterator CreateComponentIterator(Type watchingType, IterationFlags flags) {
            return Owner.Components.CreateIterator(watchingType, flags);
        }

        protected ComponentIterator<T> CreateComponentIterator<T>(IterationFlags flags) where T : struct {
            return Owner.Components.CreateIterator<T>(flags);
        }

        internal virtual void Initialize() { }

        public virtual void SubscribeToEvents() {}

        private List<EventUnsubscriber> EventUnsubscribers = new List<EventUnsubscriber>();

        internal void UnsubscribeFromEvents() {
            foreach(EventUnsubscriber obj in EventUnsubscribers) {
                obj.Unsubscribe();
            }
            EventUnsubscribers.Clear();
        }
        
        protected void Subscribe<T>(Event.EventListener<T> action) where T : struct {
            EventUnsubscribers.Add(Owner.Events.Subscribe(action));
        }
        
        
        
        
        private static Dictionary<Type, string> _typeToIdentifierMap = null;
        private static Dictionary<string, Type> _identifierToTypeMap = null;
        private static Dictionary<string, ConstructorInfo> _identifierToConstructorMap = null;

        public static string IdentifierOf(Type type) {
            if(_typeToIdentifierMap == null) {
                _typeToIdentifierMap = new Dictionary<Type, string>();
            }

            if(!_typeToIdentifierMap.ContainsKey(type)) {
                object[] matchingAttributes = type.GetCustomAttributes(typeof(GameSystemAttribute), false);
                if(matchingAttributes.Length == 0) throw new ArgumentException($"Type '{type}' is not a game system type");
                _typeToIdentifierMap[type] =
                    (matchingAttributes[0] as GameSystemAttribute).SystemName;
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

        public static GameSystem CreateForIdentifier(string identifier) {
            PopulateIdentifiers();
            return (GameSystem) _identifierToConstructorMap[identifier].Invoke(new object[0]);
        }

        public static void PopulateDictionaryWithAssembly(Assembly assembly) {
            if(_identifierToTypeMap == null) _identifierToTypeMap = new Dictionary<string, Type>();
            if(_identifierToConstructorMap == null) _identifierToConstructorMap = new Dictionary<string, ConstructorInfo>();
            foreach(Type type in assembly.GetTypes()) {
                if(type.IsSubclassOf(typeof(GameSystem))) {
                    object[] attributes;
                    if((attributes = type.GetCustomAttributes(typeof(GameSystemAttribute), false)).Length > 0) {
                        string thisIdentifier = (attributes[0] as GameSystemAttribute).SystemName;
                        _identifierToTypeMap[thisIdentifier] = type;
                        _identifierToConstructorMap[thisIdentifier] = type.GetConstructor(new Type[0]);
                    }
                }
            }
        }

        public static void PopulateIdentifiers() {
            if(_identifierToTypeMap == null) {
                PopulateDictionaryWithAssembly(Assembly.GetEntryAssembly());
            }
        }
    }


    [Flags]
    public enum ProcessingCycles {
        None = 0,
        Input = 1,
        Update = 2,
        Render = 4,
        All = Input | Update | Render,
    }

    public sealed class GameSystemAttribute : Attribute {
        public readonly string SystemName;
        public readonly ProcessingCycles ProcessingCycles;

        public GameSystemAttribute(string identifier, ProcessingCycles processingCycles) {
            SystemName = identifier;
            ProcessingCycles = processingCycles;
        }
    }

    public sealed class ListeningAttribute : Attribute {
        public readonly Type[] EventTypes;

        public ListeningAttribute(params Type[] eventTypes) {
            EventTypes = eventTypes;
        }
    }
}