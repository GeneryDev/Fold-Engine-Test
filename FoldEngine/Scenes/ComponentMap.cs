using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Components;

namespace FoldEngine.Scenes
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
        /// The map containing the lists of components.<br></br>
        /// The component lists are identified by a component Type.<br></br>
        /// The list may not exist if there are no entities with components of said type.<br></br>
        /// <br></br>
        /// Each component list is sorted by Entity ID on insertion, to make retrieval a O(lg n) operation,
        /// where n is the number of components of the same type that exist in the scene.
        /// </summary>
        private readonly Dictionary<Type, List<Component>> Map = new Dictionary<Type, List<Component>>();

        /// <summary>
        /// A queue for components that are added during the current update loop.<br></br>
        /// At the end of the update loop, ComponentViews are updated with these new components
        /// </summary>
        private readonly Queue<Component> QueuedToAdd = new Queue<Component>();
        /// <summary>
        /// A queue for components that are removed during the current update loop.<br></br>
        /// At the end of the update loop, ComponentViews are updated to remove these components
        /// </summary>
        private readonly Queue<Component> QueuedToRemove = new Queue<Component>();

        /// <summary>
        /// A list of active component views in this scene.
        /// </summary>
        private readonly List<ComponentView> Views = new List<ComponentView>();
        
        /// <summary>
        /// Creates a ComponentMap attached to the given scene.
        /// </summary>
        /// <param name="scene"></param>
        internal ComponentMap(Scene scene) => this.Scene = scene;

        /// <summary>
        /// Instantiates a component of type T and attaches it to the entity of the given ID.
        /// </summary>
        /// <typeparam name="T">The type of the component to create. Must have a default constructor.</typeparam>
        /// <param name="entityId">The entity ID to attach the new component to</param>
        /// <returns>The newly created component</returns>
        public T CreateComponent<T>(long entityId) where T : Component, new()
        {
            T newComponent = new T
            {
                EntityId = entityId,
                Scene = Scene
            };
            InsertComponent(newComponent);
            return newComponent;
        }

        /// <summary>
        /// Inserts the given component into the appropriate component list for its type, already sorted.<br></br>
        /// An exception will be thrown if there is already a component of the same type belonging to the same entity.
        /// </summary>
        /// <param name="component">The component to insert</param>
        private void InsertComponent(Component component)
        {
            long entityId = component.EntityId;
            Type componentType = component.GetType();

            List<Component> listToInsertIn;
            if(Map.ContainsKey(componentType))
            {
                listToInsertIn = Map[componentType];
            } else
            {
                listToInsertIn = new List<Component>();
                Map[componentType] = listToInsertIn;
            }

            //Insert sorted
            int insertionIndex = FindIndexForEntityId(entityId, listToInsertIn);
            if(insertionIndex < listToInsertIn.Count && listToInsertIn[insertionIndex].EntityId == entityId)
            {
                throw new Exception("The entity already has a component of type " + component.GetType().Name);
            }
            listToInsertIn.Insert(insertionIndex, component);

            QueuedToAdd.Enqueue(component);
        }
        
        /// <summary>
        /// Removes a component of the given type from the specified entity ID, if one exists.
        /// </summary>
        /// <typeparam name="T">The type of component to remove</typeparam>
        /// <param name="entityId">The ID of the entity whose component should be removed</param>
        public void RemoveComponent<T>(long entityId) where T : Component
        {
            Type componentType = typeof(T);
            List<Component> listToRemoveFrom;
            if (!Map.ContainsKey(componentType))
            {
                return;
            }
            listToRemoveFrom = Map[componentType];
            int removalIndex = FindIndexForEntityId(entityId, listToRemoveFrom);
            if(removalIndex < listToRemoveFrom.Count && listToRemoveFrom[removalIndex].EntityId == entityId)
            {
                Component component = listToRemoveFrom[removalIndex];
                listToRemoveFrom.RemoveAt(removalIndex);
                QueuedToRemove.Enqueue(component);
            } else
            {
                //Component not found
            }
        }

        /// <summary>
        /// Removes the given component from its entity.
        /// Unused.
        /// </summary>
        /// <param name="component">The component to remove from its entity</param>
        private void RemoveComponent(Component component)
        {
            Type componentType = component.GetType();

            List<Component> listToRemoveFrom;

            if (!Map.ContainsKey(componentType))
            {
                FoldUtil.Assert(false, "RemoveComponent is called with a registered component type");
                return;
            }
            listToRemoveFrom = Map[componentType];

            bool removed = listToRemoveFrom.Remove(component);
            FoldUtil.Assert(removed, "RemoveComponent is called with a registered component");

            QueuedToRemove.Enqueue(component);
        }

        /// <summary>
        /// Retrieves the component of type T attached to the entity of the given ID.<br></br>
        /// May return null if the entity has no such component.<br></br>
        /// <br></br>
        /// O(lg n) operation where n is the number of components of the same type that exist in the scene.
        /// </summary>
        /// <typeparam name="T">The type of component to search for</typeparam>
        /// <param name="entityId">The ID of the entity whose component is to be queried</param>
        /// <returns>The component of type T belonging to the entity of the given ID. May return null if such a component or entity does not exist.</returns>
        public T GetComponent<T>(long entityId) where T : Component
        {
            Type componentType = typeof(T);
            if (!Map.ContainsKey(componentType)) return null;

            List<Component> listToSearch = Map[componentType];
            if (listToSearch.Count == 0) return null;

            int foundIndex = FindIndexForEntityId(entityId, listToSearch);
            if (foundIndex < listToSearch.Count && listToSearch[foundIndex].EntityId == entityId)
            {
                return (T) listToSearch[foundIndex];
            }
            return null;
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
            Views.Add(view);

            if (Map.ContainsKey(watchingType))
            {
                foreach (Component c in Map[watchingType])
                {
                    view.AddComponent(c);
                }
            }

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
            Views.Add(view);

            foreach (Type watchingType in view.Watching)
            {
                if (Map.ContainsKey(watchingType))
                {
                    foreach (Component c in Map[watchingType])
                    {
                        view.AddComponent(c);
                    }
                }
            }

            return view;
        }

        /// <summary>
        /// Disconnects the given view from this component map, making its contents not update when components are added or removed.
        /// </summary>
        /// <param name="view">The view to disconnect from this component map</param>
        public void DisconnectView(ComponentView view)
        {
            Views.Remove(view);
        }

        /// <summary>
        /// Called at the end of the update cycle, will update all the connected views
        /// to reflect the components that have been added or removed during that update.
        /// 
        /// TODO
        /// </summary>
        internal void Flush()
        {
            {
                Component toAdd;
                while(QueuedToAdd.Count > 0)
                {
                    toAdd = QueuedToAdd.Dequeue();

                    foreach(ComponentView view in Views)
                    {
                        if(view.Matches(toAdd))
                        {
                            view.AddComponent(toAdd);
                        }
                    }
                }
            }

            {
                Component toRemove;
                while(QueuedToRemove.Count > 0)
                {
                    toRemove = QueuedToRemove.Dequeue();

                    foreach (ComponentView view in Views)
                    {
                        if (view.Matches(toRemove))
                        {
                            view.RemoveComponent(toRemove);
                        }
                    }
                }
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
    }
}
