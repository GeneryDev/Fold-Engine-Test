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
        /// Creates a single-component view that watches components of the specified type.<br></br>
        /// The given view will be updated at the end of the update loop with any components that have been added or removed during that update.<br></br>
        /// </summary>
        /// <param name="watchingType">The type of component to watch</param>
        /// <returns>A simple component view that watches for the given type, connected to this component map</returns>
        public SimpleComponentView CreateView(Type watchingType)
        {
            SimpleComponentView view = new SimpleComponentView(watchingType);
            /*Views.Add(view);

            if (Map.ContainsKey(watchingType))
            {
                foreach (Component c in Map[watchingType])
                {
                    view.AddComponent(c);
                }
            }*/

            return view;
        }

        /// <summary>
        /// Creates a multi-component view that watches components of the specified types.<br></br>
        /// This results in a view that will only contain components of the given types that belong to entities that have all the specified component types.<br></br>
        /// The given view will be updated at the end of the update loop to reflect any components that have been added or removed during that update.
        /// </summary>
        /// <param name="watchingTypes">The types of components to watch</param>
        /// <returns>A multi-component view that watches for intersections of the given component types, connected to this component map</returns>
        public MultiComponentView CreateView(params Type[] watchingTypes)
        {
            MultiComponentView view = new MultiComponentView(watchingTypes);
            /*Views.Add(view);

            foreach (Type watchingType in view.Watching)
            {
                if (Map.ContainsKey(watchingType))
                {
                    foreach (Component c in Map[watchingType])
                    {
                        view.AddComponent(c);
                    }
                }
            }*/

            return view;
        }

        /// <summary>
        /// Disconnects the given view from this component map, making its contents not update when components are added or removed.
        /// </summary>
        /// <param name="view">The view to disconnect from this component map</param>
        public void DisconnectView(ComponentView view)
        {
            //Views.Remove(view);
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

        /// <summary>
        /// Searches a list of components for the index at which the specified entity ID belongs or exists,
        /// for the purposes of adding components to sorted lists or searching components in sorted lists.
        /// 
        /// O(lg n) operation where n is the length of the given component list.<br></br>
        /// </summary>
        /// <param name="entityId">The entity ID to search a location for</param>
        /// <param name="components">The list of components to search in</param>
        /// <returns>An index between 0 and components.Count (both inclusive), reflecting one of two things:<br></br>
        /// 1. The index within the list on which a component belonging to the given entity ID is located (if it exists)<br></br>
        /// 2. The index within the list where a component belonging to the given entity ID should be located were it to be inserted into the list (if it doesn't already exist).<br></br>
        /// It's important to check the EntityId of the element at the index returned to determine whether the entity already has a component there or if it doesn't</returns>
        public static int FindIndexForEntityId(long entityId, List<Component> components)
        {
            if (components.Count == 0) return 0;

            int minIndex = 0; // inclusive
            int maxIndex = components.Count; // exclusive

            if (entityId < components[minIndex].EntityId)
            {
                return minIndex;
            }
            if (entityId > components[maxIndex - 1].EntityId)
            {
                return maxIndex;
            }

            while (minIndex < maxIndex)
            {
                int pivotIndex = (minIndex + maxIndex) / 2;

                long pivotId = components[pivotIndex].EntityId;
                if (pivotId == entityId)
                {
                    return pivotIndex;
                }
                else if (entityId > pivotId)
                {
                    minIndex = pivotIndex + 1;
                }
                else
                {
                    maxIndex = pivotIndex;
                }
            }

            return minIndex;
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
