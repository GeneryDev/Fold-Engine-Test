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
        
        private static Dictionary<Type, List<ICustomInspector>> _inspectors = null;
        private static readonly HashSet<Assembly> AssembliesPopulated = new HashSet<Assembly>();



        /// <summary>
        ///     Searches the entry assembly for component types and caches their identifiers
        /// </summary>
        public static void Populate() {
            if(_inspectors == null) {
                _inspectors = new Dictionary<Type, List<ICustomInspector>>();
                PopulateDictionaryWithAssembly(Assembly.GetAssembly(typeof(ICustomInspector)));
                PopulateDictionaryWithAssembly(Assembly.GetEntryAssembly());
            }
        }
        
        /// <summary>
        ///     Searches the given assembly and caches all the component types and their annotated identifiers
        /// </summary>
        /// <param name="assembly">The assembly to search for components</param>
        public static void PopulateDictionaryWithAssembly(Assembly assembly) {
            if(AssembliesPopulated.Contains(assembly)) return;
            AssembliesPopulated.Add(assembly);
            foreach(Type type in assembly.GetTypes()) {
                if(typeof(ICustomInspector).IsAssignableFrom(type)) {
                    object[] attributes;
                    if((attributes = type.GetCustomAttributes(typeof(CustomInspectorAttribute), false)).Length > 0) {
                        var attribute = attributes[0] as CustomInspectorAttribute;
                        if(attribute != null) {
                            if(!_inspectors.ContainsKey(attribute.Type)) _inspectors[attribute.Type] = new List<ICustomInspector>(); 
                            _inspectors[attribute.Type].Add((ICustomInspector) type.GetConstructor(new Type[0]).Invoke(new object[0]));
                        }
                    }
                }
            }
        }

        public static void RenderCustomInspectorsBefore(object obj, GuiPanel panel) {
            Populate();

            RenderCustomInspectors(obj, obj.GetType(), panel, after: false);
        }

        public static void RenderCustomInspectorsAfter(object obj, GuiPanel panel) {
            Populate();

            RenderCustomInspectors(obj, obj.GetType(), panel, after: true);
        }

        private static void RenderCustomInspectors(object obj, Type type, GuiPanel panel, bool after) {
            if(type == null) return;
            RenderCustomInspectors(obj, type.BaseType, panel, after);
            if(!_inspectors.ContainsKey(type)) return;

            foreach(ICustomInspector inspector in _inspectors[type]) {
                if(after) {
                    inspector.RenderInspectorAfter(obj, panel);
                } else {
                    inspector.RenderInspectorBefore(obj, panel);
                }
            }
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