using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FoldEngine.Registries;
using FoldEngine.Scenes;

namespace FoldEngine.Components;

public class ComponentRegistry : IRegistry
{
    private Dictionary<Type, ComponentDefinition> _defsByType = new();
    private Dictionary<string, ComponentDefinition> _defsByIdentifier = new();

    private Dictionary<Type, Func<Scene, long, object>> _customInitializers = new();
    private readonly Dictionary<Type, ConstructorInfo> _componentSetConstructors = new();

    /// <summary>
    ///     Retrieves the identifier of the given component type
    /// </summary>
    /// <param name="type">The Type of the component to retrieve the identifier from</param>
    /// <returns>The identifier for the given type, if it exists</returns>
    public string IdentifierOf(Type type)
    {
        if (_defsByType.TryGetValue(type, out var def)) return def.Identifier;

        throw new ArgumentException($"Type '{type}' is not a component type");
    }

    /// <summary>
    /// Checks whether the given component type has the given component trait.
    /// Throws an exception if the given component type is not valid.
    /// </summary>
    public bool HasTrait(Type componentType, Type traitType)
    {
        if (_defsByType.TryGetValue(componentType, out var def)) return def.Traits.Contains(traitType);

        throw new ArgumentException($"Type '{componentType}' is not a component type");
    }

    /// <summary>
    ///     Retrieves the identifier of the given component type
    /// </summary>
    /// <typeparam name="T">The component type of which to retrieve the identifier</typeparam>
    /// <returns>The identifier for the given type, if it exists</returns>
    public string IdentifierOf<T>() where T : struct
    {
        return IdentifierOf(typeof(T));
    }

    public Type TypeForIdentifier(string identifier)
    {
        return _defsByIdentifier[identifier].Type;
    }

    public void AcceptType(Type type)
    {
        if (!type.IsValueType) return;
        if (type.GetCustomAttribute<ComponentAttribute>(false) is { } componentAttribute)
        {
            string thisIdentifier = componentAttribute.ComponentName;
            var def = new ComponentDefinition()
            {
                Type = type,
                Identifier = thisIdentifier,
                Traits = [..componentAttribute.Traits ?? Enumerable.Empty<Type>()]
            };
            _defsByIdentifier[thisIdentifier] = def;
            _defsByType[type] = def;
        }

        if (type.GetCustomAttribute<ComponentInitializerAttribute>(false) is { } initializerAttribute)
        {
            _customInitializers[type] = initializerAttribute.Initializer;
        }
    }

    public void InitializeComponent<T>(ref T component, Scene scene, long entityId) where T : struct
    {
        if (_customInitializers != null && _customInitializers.ContainsKey(typeof(T)))
            // Console.WriteLine($"Initializing component of type {typeof(T)}");
            component = (T)_customInitializers[typeof(T)].Invoke(scene, entityId);
    }

    public ComponentSet CreateSetForType(Type componentType, Scene scene, int startingId)
    {
        if (!_componentSetConstructors.ContainsKey(componentType))
            _componentSetConstructors[componentType] =
                typeof(ComponentSet<>).MakeGenericType(componentType)
                    .GetConstructor(new[] { typeof(Scene), typeof(int) });

        return (ComponentSet)_componentSetConstructors[componentType].Invoke(new object[] { scene, startingId });
    }

    public IEnumerable<ComponentDefinition> GetAllDefinitions()
    {
        return _defsByIdentifier.Values;
    }
}

public struct ComponentDefinition
{
    public Type Type;
    public string Identifier;
    public HashSet<Type> Traits;
}