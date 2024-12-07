using System;
using System.Reflection;
using FoldEngine.Scenes;

namespace FoldEngine.Components;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ComponentAttribute : Attribute
{
    public readonly string ComponentName;
    public readonly Type[] Traits;

    public ComponentAttribute(string identifier, Type[] traits = null)
    {
        ComponentName = identifier;
        Traits = traits;
    }
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public sealed class ComponentTraitAttribute : Attribute
{
    public readonly string ComponentTraitName;

    public ComponentTraitAttribute(string identifier)
    {
        ComponentTraitName = identifier;
    }
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ComponentInitializerAttribute : Attribute
{
    public readonly Func<Scene, long, object> Initializer;

    public ComponentInitializerAttribute(Type type, string memberName = null)
    {
        if (memberName != null)
        {
            MethodInfo method = type.GetMethod(memberName, new[] { typeof(Scene), typeof(long) });
            Initializer = (scene, entityId) => method.Invoke(null, new object[] { scene, entityId });
        }
        else
        {
            // use default constructor
            var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
            if (constructor != null)
            {
                object defaultInstance = constructor.Invoke(Array.Empty<object>());
                Initializer = (scene, entityId) => defaultInstance;
            }
            else
            {
                throw new ArgumentException(
                    $"Component type {type} does not have a public default constructor, cannot create a valid ComponentInitializer",
                    nameof(type));
            }
        }
    }
}