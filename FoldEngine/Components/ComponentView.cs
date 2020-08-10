using System;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine.Components
{
    public abstract class ComponentView
    {
        public delegate void ComponentUpdated(Component component);

        public event ComponentUpdated ComponentAdded;
        public event ComponentUpdated ComponentRemoved;

        internal abstract void AddComponent(Component component);
        internal abstract void RemoveComponent(Component component);
        internal abstract bool Matches(Component component);

        protected void InvokeComponentAdded(Component component)
        {
            ComponentAdded?.Invoke(component);
        }
        protected void InvokeComponentRemoved(Component component)
        {
            ComponentRemoved?.Invoke(component);
        }
    }
}
