using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Events;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;

namespace FoldEngine.Systems {
    public abstract class GameSystem {
        private static Dictionary<Type, string> _typeToIdentifierMap;
        private static Dictionary<string, Type> _identifierToTypeMap;
        private static Dictionary<string, ConstructorInfo> _identifierToConstructorMap;

        private readonly GameSystemAttribute _attribute;
        private readonly List<EventUnsubscriber> _eventUnsubscribers = new List<EventUnsubscriber>();

        protected GameSystem() {
            _attribute = (GameSystemAttribute) GetType().GetCustomAttribute(typeof(GameSystemAttribute));
        }

        public Scene Scene { get; internal set; }
        public string SystemName => _attribute.SystemName;
        public ProcessingCycles ProcessingCycles => _attribute.ProcessingCycles;
        public bool RunWhenPaused => _attribute.RunWhenPaused;

        public virtual void OnInput() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnRender(IRenderingUnit renderer) { }

        public virtual void PollResources() { }

        protected MultiComponentIterator CreateComponentIterator(params Type[] watchingTypes) {
            return Scene.Components.CreateMultiIterator(watchingTypes);
        }

        protected ComponentIterator CreateComponentIterator(Type watchingType, IterationFlags flags) {
            return Scene.Components.CreateIterator(watchingType, flags);
        }

        protected ComponentIterator<T> CreateComponentIterator<T>(IterationFlags flags) where T : struct {
            return Scene.Components.CreateIterator<T>(flags);
        }

        public virtual void Initialize() { }

        public virtual void SubscribeToEvents() { }


        internal void UnsubscribeFromEvents() {
            foreach(EventUnsubscriber obj in _eventUnsubscribers) obj.Unsubscribe();
            _eventUnsubscribers.Clear();
        }

        protected void Subscribe<T>(Event.EventListener<T> action) where T : struct {
            _eventUnsubscribers.Add(Scene.Events.Subscribe(action));
        }

        public static string IdentifierOf(Type type) {
            if(_typeToIdentifierMap == null) _typeToIdentifierMap = new Dictionary<Type, string>();

            if(!_typeToIdentifierMap.ContainsKey(type)) {
                object[] matchingAttributes = type.GetCustomAttributes(typeof(GameSystemAttribute), false);
                if(matchingAttributes.Length == 0)
                    throw new ArgumentException($"Type '{type}' is not a game system type");
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
            if(_identifierToConstructorMap == null)
                _identifierToConstructorMap = new Dictionary<string, ConstructorInfo>();
            foreach(Type type in assembly.GetTypes())
                if(type.IsSubclassOf(typeof(GameSystem))) {
                    object[] attributes;
                    if((attributes = type.GetCustomAttributes(typeof(GameSystemAttribute), false)).Length > 0) {
                        string thisIdentifier = (attributes[0] as GameSystemAttribute).SystemName;
                        _identifierToTypeMap[thisIdentifier] = type;
                        _identifierToConstructorMap[thisIdentifier] = type.GetConstructor(new Type[0]);
                    }
                }
        }

        public static void PopulateIdentifiers() {
            if(_identifierToTypeMap == null) {
                PopulateDictionaryWithAssembly(Assembly.GetAssembly(typeof(GameSystem)));
                PopulateDictionaryWithAssembly(Assembly.GetEntryAssembly());
            }
        }
    }


    [Flags]
    public enum ProcessingCycles {
        None = 0,
        Input = 1,
        FixedUpdate = 2,
        Update = 4,
        Render = 8,
        All = Input | FixedUpdate | Update | Render
    }

    public static class ProcessingCyclesExt {
        public static bool Has(this ProcessingCycles t, ProcessingCycles mask) {
            return (t & mask) != 0;
        }
    }

    public sealed class GameSystemAttribute : Attribute {
        public readonly ProcessingCycles ProcessingCycles;
        public readonly bool RunWhenPaused;
        public readonly string SystemName;

        public GameSystemAttribute(string identifier, ProcessingCycles processingCycles, bool runWhenPaused = false) {
            SystemName = identifier;
            ProcessingCycles = processingCycles;
            RunWhenPaused = runWhenPaused;
        }
    }

    public sealed class ListeningAttribute : Attribute {
        public readonly Type[] EventTypes;

        public ListeningAttribute(params Type[] eventTypes) {
            EventTypes = eventTypes;
        }
    }
}