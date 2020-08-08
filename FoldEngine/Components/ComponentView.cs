using System;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine.Components
{
    public abstract class ComponentView
    {
        public delegate void ComponentUpdated(ComponentAttachment component);

        public event ComponentUpdated ComponentAdded;
        public event ComponentUpdated ComponentRemoved;

        internal abstract void AddComponent(ComponentAttachment component);
        internal abstract void RemoveComponent(ComponentAttachment component);
        internal abstract bool Matches(ComponentAttachment component);

        protected void InvokeComponentAdded(ComponentAttachment component)
        {
            ComponentAdded?.Invoke(component);
        }
        protected void InvokeComponentRemoved(ComponentAttachment component)
        {
            ComponentRemoved?.Invoke(component);
        }
    }
}
