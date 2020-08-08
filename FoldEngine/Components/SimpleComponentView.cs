using FoldEngine.Scenes;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine.Components
{
    public class SimpleComponentView : ComponentView, IEnumerable<Component>
    {
        internal Type Watching;

        private readonly List<Component> WatchedComponents = new List<Component>();

        internal SimpleComponentView(Type watchingType)
        {
            Watching = watchingType;
        }

        internal override void AddComponent(Component component)
        {
            int insertionIndex = ComponentMap.FindIndexForEntityId(component.EntityId, WatchedComponents);

            if(insertionIndex >= WatchedComponents.Count || WatchedComponents[insertionIndex] != component)
            {
                WatchedComponents.Insert(insertionIndex, component);
                InvokeComponentAdded(component);
            }
        }
        internal override void RemoveComponent(Component component)
        {
            int removalIndex = ComponentMap.FindIndexForEntityId(component.EntityId, WatchedComponents);
            if(removalIndex < WatchedComponents.Count && WatchedComponents[removalIndex] == component)
            {
                InvokeComponentRemoved(component);
                WatchedComponents.RemoveAt(removalIndex);
            }
        }

        internal override bool Matches(Component component)
        {
            return component.GetType() == Watching;
        }

        public IEnumerator<Component> GetEnumerator() => ((IEnumerable<Component>)WatchedComponents).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)WatchedComponents).GetEnumerator();
    }
}
