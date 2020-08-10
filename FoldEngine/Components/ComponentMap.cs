using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Scenes;

namespace FoldEngine.Components
{
    /// <summary>
    /// A class that describes an object dedicated to the retrieval, creation, destruction and storage of game components. 
    /// It stores components in lists of their type, sorted on insertion for O(lg n) insertion, retrieval and removal.
    /// Component Maps also handle Component Views, updating them with components added or removed, whenever Flush() is called, usually at the end of the update cycle.
    /// </summary>
    public class ComponentMap
    {
        /// <summary>
        /// The scene this component map belongs to
        /// </summary>
        private readonly Scene Scene;
        /// <summary>
        /// The map containing the component sets.<br></br>
        /// The set may not exist if there are no entities with components of said type.<br></br>
        /// </summary>
        internal readonly Dictionary<Type, ComponentSet> Map = new Dictionary<Type, ComponentSet>();

        /// <summary>
        /// Creates a ComponentMap attached to the given scene.
        /// </summary>
        /// <param name="scene"></param>
        internal ComponentMap(Scene scene) => Scene = scene;

        /// <summary>
        /// Instantiates a component of type T and attaches it to the entity of the given ID.
        /// </summary>
        /// <typeparam name="T">The type of the component to create. Must have a default constructor.</typeparam>
        /// <param name="entityId">The entity ID to attach the new component to</param>
        /// <returns>The newly created component</returns>
        public ref T CreateComponent<T>(long entityId) where T : struct
        {
            Type componentType = typeof(T);
            if (!Map.ContainsKey(componentType))
            {
                Map[componentType] = new ComponentSet<T>(entityId);
            }
            return ref ((ComponentSet<T>)Map[componentType]).Create(entityId);
        }

        /// <summary>
        /// Removes a component of the given type from the specified entity ID, if one exists.
        /// </summary>
        /// <typeparam name="T">The type of component to remove</typeparam>
        /// <param name="entityId">The ID of the entity whose component should be removed</param>
        public void RemoveComponent<T>(long entityId) where T : struct
        {
            Type componentType = typeof(T);
            if (Map.ContainsKey(componentType))
            {
                ((ComponentSet<T>)Map[componentType]).Remove(entityId);
            }
            else
            {
                //Component type not registered
            }
        }

        /// <summary>
        /// Retrieves the component of type T attached to the entity of the given ID.<br></br>
        /// Will throw a ComponentRegistryException if the entity does not have a component of that type.
        /// <br></br>
        /// O(lg n) operation where n is the number of components of the same type that exist in the scene.
        /// </summary>
        /// <typeparam name="T">The type of component to search for</typeparam>
        /// <param name="entityId">The ID of the entity whose component is to be queried</param>
        /// <returns>The component of type T belonging to the entity of the given ID. Will throw a ComponentRegistryException if such a component or entity does not exist.</returns>
        public ref T GetComponent<T>(long entityId) where T : struct
        {
            Type componentType = typeof(T);
            if (!Map.ContainsKey(componentType))
            {
                throw new ComponentRegistryException($"Component {componentType} not found for entity ID {entityId}");
                //return null;
            }

            return ref ((ComponentSet<T>)Map[componentType]).Get(entityId);
        }

        public ComponentIterator<T> CreateIterator<T>(IterationFlags flags) where T : struct
        {
            return new ComponentIterator<T>(Scene, flags);
        }

        public ComponentIterator CreateIterator(Type type, IterationFlags flags)
        {
            return ComponentIterator.CreateForType(type, Scene, flags);
        }

        public MultiComponentIterator CreateMultiIterator(params Type[] types)
        {
            return new MultiComponentIterator(Scene, types);
        }

        /// <summary>
        /// Called at the end of the update cycle, will update all the connected views
        /// to reflect the components that have been added or removed during that update.
        /// 
        /// TODO
        /// </summary>
        internal void Flush()
        {
            foreach (ComponentSet set in Map.Values)
            {
                set.Flush();
            }
        }

        public void DebugPrint<T>() where T : struct
        {
            Type componentType = typeof(T);
            if (Map.ContainsKey(componentType))
            {
                ((ComponentSet<T>)Map[componentType]).DebugPrint();
            }
            else
            {
                //Component type not registered
            }
        }
    }
}
