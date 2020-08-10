using FoldEngine.Components;
using FoldEngine.Scenes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FoldEngine.Systems
{
    public abstract class GameSystem
    {
        public string SystemName { get; private set; }

        public Scene Owner { get; internal set; }

        private readonly GameSystemAttribute Attribute;
        public ProcessingCycles ProcessingCycles => Attribute.ProcessingCycles;

        public GameSystem()
        {
            Attribute = (GameSystemAttribute)this.GetType().GetCustomAttribute(typeof(GameSystemAttribute));
        }

        public virtual void OnInput() { }
        public virtual void OnUpdate() { }
        public virtual void OnRender() { }

        protected MultiComponentIterator CreateComponentIterator(params Type[] watchingTypes)
        {
            return Owner.Components.CreateMultiIterator(watchingTypes);
        }
        protected ComponentIterator CreateComponentIterator(Type watchingType, IterationFlags flags)
        {
            return Owner.Components.CreateIterator(watchingType, flags);
        }
        protected ComponentIterator CreateComponentIterator<T>(IterationFlags flags) where T : struct
        {
            return Owner.Components.CreateIterator<T>(flags);
        }

        protected MultiComponentView CreateComponentView(params Type[] watchingTypes)
        {
            return Owner.Components.CreateView(watchingTypes);
        }
        protected SimpleComponentView CreateComponentView(Type watchingType)
        {
            return Owner.Components.CreateView(watchingType);
        }

        internal virtual void Initialize() { }
    }


    [Flags]
    public enum ProcessingCycles
    {
        None = 0,
        Input = 1,
        Update = 2,
        Render = 4,
        All = Input | Update | Render,
    }

    public sealed class GameSystemAttribute : Attribute
    {
        public readonly string SystemName;
        public readonly ProcessingCycles ProcessingCycles;
        public GameSystemAttribute(string identifier, ProcessingCycles processingCycles)
        {
            SystemName = identifier;
            ProcessingCycles = processingCycles;
        }
    }
    public sealed class ListeningAttribute : Attribute
    {
        public readonly string[] Watching;
        public ListeningAttribute(params Type[] watching)
        {
            //TODO event class
            if (!watching.All(t => typeof(Component).IsAssignableFrom(t))) throw new ArgumentException("watching");
            Watching = watching.Select(t => Component.IdentifierOf(t)).ToArray();
        }
    }
}
