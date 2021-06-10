using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Scenes;

namespace FoldEngine.Components
{
    public static class Component
    {
        private static Dictionary<Type, string> _typeToIdentifierMap = null;
        private static Dictionary<string, Type> _identifierToTypeMap = null;

        private static Dictionary<Type, Func<Scene, long, object>> _customInitializers = null;

        /// <summary>
        /// Retrieves the identifier of the given component type
        /// </summary>
        /// <param name="type">The Type of the component to retrieve the identifier from</param>
        /// <returns>The identifier for the given type, if it exists</returns>
        public static string IdentifierOf(Type type)
        {
            if(_typeToIdentifierMap == null)
            {
                _typeToIdentifierMap = new Dictionary<Type, string>();
            }
            if(!_typeToIdentifierMap.ContainsKey(type))
            {
                _typeToIdentifierMap[type] = (type.GetCustomAttributes(typeof(ComponentAttribute), false)[0] as ComponentAttribute).ComponentName;
            }
            return _typeToIdentifierMap[type];
        }

        /// <summary>
        /// Retrieves the identifier of the given component type
        /// </summary>
        /// <typeparam name="T">The component type of which to retrieve the identifier</typeparam>
        /// <returns>The identifier for the given type, if it exists</returns>
        public static string IdentifierOf<T>() where T : struct
        {
            return IdentifierOf(typeof(T));
        }

        public static Type TypeForIdentifier(string identifier)
        {
            PopulateIdentifiers();
            return _identifierToTypeMap[identifier];
        }

        /// <summary>
        /// Searches the given assembly and caches all the component types and their annotated identifiers
        /// </summary>
        /// <param name="assembly">The assembly to search for components</param>
        public static void PopulateDictionaryWithAssembly(Assembly assembly)
        {
            if (_identifierToTypeMap == null) _identifierToTypeMap = new Dictionary<string, Type>();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsValueType)
                {
                    object[] attributes;
                    if ((attributes = type.GetCustomAttributes(typeof(ComponentAttribute), false)).Length > 0)
                    {
                        string thisIdentifier = (attributes[0] as ComponentAttribute).ComponentName;
                        _identifierToTypeMap[thisIdentifier] = type;
                    }
                    if ((attributes = type.GetCustomAttributes(typeof(ComponentInitializerAttribute), false)).Length > 0)
                    {
                        if (_customInitializers == null) _customInitializers = new Dictionary<Type, Func<Scene, long, object>>();
                        _customInitializers[type] = (attributes[0] as ComponentInitializerAttribute).Initializer;
                    }
                }
            }
        }

        /// <summary>
        /// Searches the entry assembly for component types and caches their identifiers
        /// </summary>
        public static void PopulateIdentifiers()
        {
            if (_identifierToTypeMap == null)
            {
                PopulateDictionaryWithAssembly(Assembly.GetEntryAssembly());
            }
        }

        public static void InitializeComponent<T>(ref T component, Scene scene, long entityId) where T : struct
        {
            if(_customInitializers != null && _customInitializers.ContainsKey(typeof(T)))
            {
                Console.WriteLine($"Initializing component of type {typeof(T)}");
                component = (T) _customInitializers[typeof(T)].Invoke(scene, entityId);
            }
        }
    }

    public sealed class ComponentAttribute : Attribute
    {
        public readonly string ComponentName;
        public ComponentAttribute(string identifier)
        {
            ComponentName = identifier;
        }
    }

    public sealed class ComponentInitializerAttribute : Attribute
    {
        public readonly Func<Scene, long, object> Initializer;
        public ComponentInitializerAttribute(Type type, string memberName)
        {
            MethodInfo method = type.GetMethod(memberName, new Type[] { typeof(Scene), typeof(long) });
            Initializer = (scene, entityId) => method.Invoke(null, new object[] {scene, entityId});
        }
    }
}
