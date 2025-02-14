﻿using System;
using System.Collections.Generic;
using FoldEngine.Scenes;
using FoldEngine.Scenes.Prefabs;
using FoldEngine.Serialization;

namespace FoldEngine.Components;

/// <summary>
///     A class that describes an object dedicated to the retrieval, creation, destruction and storage of game components.
///     It stores components in lists of their type, sorted on insertion for O(lg n) insertion, retrieval and removal.
///     Component Maps also handle Component Views, updating them with components added or removed, whenever Flush() is
///     called, usually at the end of the update cycle.
/// </summary>
public class ComponentMap : ISelfSerializer
{
    /// <summary>
    ///     The scene this component map belongs to
    /// </summary>
    private readonly Scene _scene;

    /// <summary>
    ///     The map containing the component sets.<br></br>
    ///     The set may not exist if there are no entities with components of said type.<br></br>
    /// </summary>
    internal readonly Dictionary<Type, ComponentSet> Sets = new Dictionary<Type, ComponentSet>();

    /// <summary>
    ///     Creates a ComponentMap attached to the given scene.
    /// </summary>
    /// <param name="scene"></param>
    internal ComponentMap(Scene scene)
    {
        _scene = scene;
    }

    public Type WorkingType => GetType();

    public void Serialize(SaveOperation writer)
    {
        writer.WriteCompound((ref SaveOperation.Compound c) =>
        {
            foreach (KeyValuePair<Type, ComponentSet> entry in Sets)
            {
                if (writer.Options.Has(SerializeOnlyComponents.Instance))
                    if (!writer.Options.Get(SerializeOnlyComponents.Instance).Contains(entry.Key))
                        continue;
                c.WriteMember(_scene.Core.RegistryUnit.Components.IdentifierOf(entry.Key),
                    (ISelfSerializer)entry.Value);
            }
        });
    }

    public void Deserialize(LoadOperation reader)
    {
        if (reader.Options.Get(DeserializeClearScene.Instance))
        {
            Console.WriteLine("Clearing sets");
            foreach (ComponentSet set in Sets.Values) set.Clear();
        }
        reader.ReadCompound(m =>
        {
            var componentIdentifier = m.Name;
            Type componentType = _scene.Core.RegistryUnit.Components.TypeForIdentifier(componentIdentifier);
            if (componentType == null)
            {
                Console.WriteLine($"Warning: Component identifier '{componentIdentifier}' not recognized");
                m.Skip();
                return;
            }

            ComponentSet set;

            if (Sets.ContainsKey(componentType))
            {
                set = Sets[componentType];
            }
            else
            {
                set = _scene.Core.RegistryUnit.Components.CreateSetForType(componentType, _scene, 0);
                Sets[componentType] = set;
            }

            reader.Deserialize(set);
        });
    }

    /// <summary>
    ///     Instantiates a component of type T and attaches it to the entity of the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the component to create.</typeparam>
    /// <param name="entityId">The entity ID to attach the new component to</param>
    /// <returns>The newly created component</returns>
    public ref T CreateComponent<T>(long entityId) where T : struct
    {
        Type componentType = typeof(T);
        if (!Sets.ContainsKey(componentType)) Sets[componentType] = new ComponentSet<T>(_scene, (int)entityId);

        return ref ((ComponentSet<T>)Sets[componentType]).Create(entityId);
    }

    /// <summary>
    ///     Instantiates a component of type componentType and attaches it to the entity of the given ID.
    /// </summary>
    /// <param name="componentType">The type of the component to create.</param>
    /// <param name="entityId">The entity ID to attach the new component to</param>
    /// <returns>The newly created component</returns>
    public void CreateComponent(Type componentType, long entityId)
    {
        if (!Sets.ContainsKey(componentType))
            Sets[componentType] =
                _scene.Core.RegistryUnit.Components.CreateSetForType(componentType, _scene, (int)entityId);

        Sets[componentType].CreateFor(entityId);
    }

    /// <summary>
    ///     Removes a component of the given type from the specified entity ID, if one exists.
    /// </summary>
    /// <typeparam name="T">The type of component to remove</typeparam>
    /// <param name="entityId">The ID of the entity whose component should be removed</param>
    public void RemoveComponent<T>(long entityId) where T : struct
    {
        Type componentType = typeof(T);
        if (Sets.ContainsKey(componentType))
        {
            ((ComponentSet<T>)Sets[componentType]).Remove(entityId);
        }
    }

    /// <summary>
    ///     Removes a component of the given type from the specified entity ID, if one exists.
    /// </summary>
    /// <param name="type">The type of component to remove</param>
    /// <param name="entityId">The ID of the entity whose component should be removed</param>
    public void RemoveComponent(Type type, long entityId)
    {
        if (Sets.ContainsKey(type))
        {
            Sets[type].Remove(entityId);
        }
    }

    /// <summary>
    ///     Removes all components from the specified entity ID.
    /// </summary>
    /// <param name="entityId">The ID of the entity whose components should be removed</param>
    public void RemoveAllComponents(long entityId)
    {
        foreach (ComponentSet set in Sets.Values) set.Remove(entityId);
    }

    /// <summary>
    ///     Returns whether any entities have any components assigned
    /// </summary>
    public bool AnyEntities()
    {
        foreach (ComponentSet set in Sets.Values)
        {
            if (set.HasAny()) return true;
        }

        return false;
    }

    /// <summary>
    ///     Retrieves the component of type T attached to the entity of the given ID.<br></br>
    ///     Will throw a ComponentRegistryException if the entity does not have a component of that type.
    ///     <br></br>
    /// </summary>
    /// <typeparam name="T">The type of component to search for</typeparam>
    /// <param name="entityId">The ID of the entity whose component is to be queried</param>
    /// <returns>
    ///     The component of type T belonging to the entity of the given ID. Will throw a ComponentRegistryException if
    ///     such a component or entity does not exist.
    /// </returns>
    public ref T GetComponent<T>(long entityId) where T : struct
    {
        Type componentType = typeof(T);
        if (!Sets.ContainsKey(componentType))
            throw new ComponentRegistryException($"Component {componentType} not found for entity ID {entityId}");
        //return null;

        return ref ((ComponentSet<T>)Sets[componentType]).Get(entityId);
    }

    /// <summary>
    ///     Checks whether or not a component of type T is attached to the entity of the given ID.<br></br>
    ///     <br></br>
    /// </summary>
    /// <typeparam name="T">The type of component to search for</typeparam>
    /// <param name="entityId">The ID of the entity whose component is to be queried</param>
    /// <returns>true if the entity has the specified component type, false otherwise.</returns>
    public bool HasComponent<T>(long entityId) where T : struct
    {
        Type componentType = typeof(T);
        return Sets.ContainsKey(componentType) && ((ComponentSet<T>)Sets[componentType]).Has(entityId);
    }

    /// <summary>
    /// Checks whether the entity with the given ID has any component with the given trait, given by type T.
    /// </summary>
    /// <param name="entityId">The ID of the entity whose components' traits are to be queried</param>
    /// <typeparam name="T">The type of Component Trait to search for</typeparam>
    /// <returns>true if the entity has a component with the specified trait, false otherwise</returns>
    public bool HasTrait<T>(long entityId) where T : struct
    {
        Type traitType = typeof(T);
        var componentsRegistry = _scene.Core.RegistryUnit.Components;
        foreach (KeyValuePair<Type,ComponentSet> entry in Sets)
        {
            if (componentsRegistry.HasTrait(entry.Key, traitType) && entry.Value.Has(entityId))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Checks whether or not a component of the given type is attached to the entity of the given ID.<br></br>
    ///     <br></br>
    /// </summary>
    /// <param name="type">The type of component to search for</param>
    /// <param name="entityId">The ID of the entity whose component is to be queried</param>
    /// <returns>true if the entity has the specified component type, false otherwise.</returns>
    public bool HasComponent(Type type, long entityId)
    {
        return Sets.ContainsKey(type) && Sets[type].Has(entityId);
    }

    public ComponentIterator<T> CreateIterator<T>(IterationFlags flags) where T : struct
    {
        return new ComponentIterator<T>(_scene, flags);
    }

    public ComponentIterator CreateIterator(Type type, IterationFlags flags)
    {
        return ComponentIterator.CreateForType(type, _scene, flags);
    }

    public MultiComponentIterator CreateMultiIterator(params Type[] types)
    {
        return new MultiComponentIterator(_scene, types);
    }

    /// <summary>
    ///     Called at the end of the update cycle, will update all the connected views
    ///     to reflect the components that have been added or removed during that update.
    /// </summary>
    internal void Flush()
    {
        foreach (ComponentSet set in Sets.Values) set.Flush();
    }

    public void DebugPrint<T>() where T : struct
    {
        Type componentType = typeof(T);
        if (Sets.ContainsKey(componentType))
        {
            ((ComponentSet<T>)Sets[componentType]).DebugPrint();
        }
    }
}