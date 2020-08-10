
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine.Components
{
    public class MultiComponentView : ComponentView, IEnumerable<MultiComponentViewGroup>
    {
        private ComponentGrouping Grouping = ComponentGrouping.And;
        internal Type[] Watching;

        private readonly List<Component> WatchedComponents = new List<Component>();

        internal MultiComponentView(Type[] watchingTypes)
        {
            Watching = watchingTypes;
        }

        internal override void AddComponent(Component component)
        {
            int insertionIndex = ComponentMap.FindIndexForEntityId(component.EntityId, WatchedComponents);
            while(insertionIndex > 0 && insertionIndex < WatchedComponents.Count && WatchedComponents[insertionIndex-1].EntityId == component.EntityId)
            {
                if(WatchedComponents[insertionIndex] == component)
                {
                    return; //already in view
                }
                insertionIndex--;
            }
            if(Watching.Length > 1)
            {
                for(int typeIndex = 0; typeIndex < Watching.Length && insertionIndex < WatchedComponents.Count; typeIndex++)
                {
                    if(Watching[typeIndex] == component.GetType())
                    {
                        if(WatchedComponents[insertionIndex] == component)
                        {
                            return; //already in view
                        }
                        break;
                    }
                    if (Watching[typeIndex] == WatchedComponents[insertionIndex].GetType())
                    {
                        insertionIndex++;
                    }
                }
            }

            WatchedComponents.Insert(insertionIndex, component);
            InvokeComponentAdded(component);
        }
        internal override void RemoveComponent(Component component)
        {
            InvokeComponentRemoved(component);
            WatchedComponents.Remove(component);
        }
        internal override bool Matches(Component component)
        {
            Type componentType = component.GetType();
            foreach(Type watchingType in Watching) {
                if(watchingType == componentType)
                {
                    return true;
                }
            }
            return false;
        }

        public MultiComponentView SetGrouping(ComponentGrouping grouping)
        {
            Grouping = grouping;
            return this;
        }

        public IEnumerator<MultiComponentViewGroup> GetEnumerator()
        {
            MultiComponentViewGroup group = new MultiComponentViewGroup(Watching);
            int index = 0;
            if(Grouping == ComponentGrouping.And)
            {
                while(index < WatchedComponents.Count - Watching.Length + 1)
                {
                    if(WatchedComponents[index].EntityId == WatchedComponents[index + Watching.Length - 1].EntityId)
                    {
                        //This entity is complete, all Watching.Length components are here
                        for (int i = 0; i < Watching.Length; i++)
                        {
                            group.Pack(i, WatchedComponents[index + i]);
                        }
                        group.EntityId = WatchedComponents[index].EntityId;
                        yield return group;
                        index += Watching.Length;
                    } else
                    {
                        //This entity doesn't have all the necessary components
                        index++;
                    }
                }
            } else //Or
            {
                while(index < WatchedComponents.Count)
                {
                    long entityId = WatchedComponents[index].EntityId;

                    group.EntityId = entityId;

                    for(int watchingTypeIndex = 0; watchingTypeIndex < Watching.Length; watchingTypeIndex++)
                    {
                        Type watchingType = Watching[watchingTypeIndex];

                        Component component = WatchedComponents[index];
                        if(component.EntityId == entityId && component.GetType() == watchingType)
                        {
                            group.Pack(watchingTypeIndex, component);
                            index++;
                        } else
                        {
                            //This entity doesn't have a component of this particular type. Pack null and skip.
                            group.Pack(watchingTypeIndex, null);
                        }
                    }

                    yield return group;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct MultiComponentViewGroup
    {
        internal Type[] Types;
        internal Component[] Components;

        public long EntityId { get; internal set; }

        internal MultiComponentViewGroup(Type[] types)
        {
            EntityId = -1;
            Types = types;
            Components = new Component[Types.Length];
        }

        internal void Pack(int index, Component component)
        {
            Components[index] = component;
        }

        public T Get<T>() where T : Component
        {
            for(int i = 0; i < Types.Length; i++)
            {
                if(Types[i] == typeof(T))
                {
                    return (T)Components[i];
                }
            }
            throw new Exception($"This view does not contain components of type {Component.IdentifierOf<T>()}");
        }
    }
}
