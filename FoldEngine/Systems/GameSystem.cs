using FoldEngine.Components;
using FoldEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FoldEngine.Systems {
    public abstract class GameSystem {
        public Scene Owner { get; internal set; }

        private readonly GameSystemAttribute _attribute;
        public string SystemName => _attribute.SystemName;
        public ProcessingCycles ProcessingCycles => _attribute.ProcessingCycles;

        protected GameSystem() {
            _attribute = (GameSystemAttribute) this.GetType().GetCustomAttribute(typeof(GameSystemAttribute));
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
        public readonly string[] Watching;

        public ListeningAttribute(params Type[] watching) {
            //TODO event class
            if(!watching.All(t => typeof(Component).IsAssignableFrom(t))) throw new ArgumentException("watching");
            Watching = watching.Select(t => Component.IdentifierOf(t)).ToArray();
        }
    }
}