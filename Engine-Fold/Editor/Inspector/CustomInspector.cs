using System;
using System.Collections.Generic;
using System.Reflection;
using FoldEngine.Gui;

namespace FoldEngine.Editor.Inspector {
    public abstract class CustomInspector<T> : ICustomInspector {
        protected virtual void RenderInspectorBefore(T obj, GuiPanel panel) { }
        protected virtual void RenderInspectorAfter(T obj, GuiPanel panel) {}
        public void RenderInspectorBefore(object obj, GuiPanel panel) {
            if(obj is T t) RenderInspectorBefore(t, panel);
        }
        public void RenderInspectorAfter(object obj, GuiPanel panel) {
            if(obj is T t) RenderInspectorAfter(t, panel);
        }
    }

    public interface ICustomInspector {
        void RenderInspectorBefore(object obj, GuiPanel panel);
        void RenderInspectorAfter(object obj, GuiPanel panel);
    }
    
    public sealed class CustomInspectorAttribute : Attribute {
        public readonly Type Type;
        public readonly bool After;

        public CustomInspectorAttribute(Type type, bool after = false) {
            Type = type;
            After = after;
        }
    }
}