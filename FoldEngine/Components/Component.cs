using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Scenes;

namespace FoldEngine.Components
{
    public abstract class Component

    {
        //Instance
        internal Scene Scene;
        internal long EntityId;

        private Entity _owner = null;
        public Entity Owner
        {
            get
            {
                if(_owner == null)
                {
                    _owner = Scene.EntityObjectPool.GetOrCreateEntityObject(EntityId);
                }
                return _owner;
            }
        }

        public override string ToString()
        {
            return $"C[{IdentifierOf(this.GetType())}]:{EntityId}";
        }


        //Static
        private static Dictionary<Type, string> TypeToIdentifierMap = null;
        private static Dictionary<string, Type> IdentifierToTypeMap = null;

        /// <summary>
        /// Retrieves the identifier of the given component type
        /// </summary>
        /// <param name="type">The Type of the component to retrieve the identifier from</param>
        /// <returns>The identifier for the given type, if it exists</returns>
        public static string IdentifierOf(Type type)
        {
            if(TypeToIdentifierMap == null)
            {
                TypeToIdentifierMap = new Dictionary<Type, string>();
            }
            if(!TypeToIdentifierMap.ContainsKey(type))
            {
                TypeToIdentifierMap[type] = (type.GetCustomAttributes(typeof(ComponentAttribute), false).First() as ComponentAttribute).ComponentName;
            }
            return TypeToIdentifierMap[type];
        }

        /// <summary>
        /// Retrieves the identifier of the given component type
        /// </summary>
        /// <typeparam name="T">The component type of which to retrieve the identifier</typeparam>
        /// <returns>The identifier for the given type, if it exists</returns>
        public static string IdentifierOf<T>() where T : Component
        {
            return IdentifierOf(typeof(T));
        }

        public static Type TypeForIdentifier(string identifier)
        {
            PopulateIdentifiers();
            return IdentifierToTypeMap[identifier];
        }

        /// <summary>
        /// Searches the given assembly and caches all the component types and their annotated identifiers
        /// </summary>
        /// <param name="assembly">The assembly to search for components</param>
        public static void PopulateDictionaryWithAssembly(Assembly assembly)
        {
            if (IdentifierToTypeMap == null) IdentifierToTypeMap = new Dictionary<string, Type>();
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(Component).IsAssignableFrom(type))
                {
                    object[] attributes;
                    if ((attributes = type.GetCustomAttributes(typeof(ComponentAttribute), false)).Length > 0)
                    {
                        string thisIdentifier = (attributes[0] as ComponentAttribute).ComponentName;
                        IdentifierToTypeMap[thisIdentifier] = type;
                    }
                }
            }
        }

        /// <summary>
        /// Searches the entry assembly for component types and caches their identifiers
        /// </summary>
        public static void PopulateIdentifiers()
        {
            if (IdentifierToTypeMap == null)
            {
                PopulateDictionaryWithAssembly(Assembly.GetEntryAssembly());
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
}
