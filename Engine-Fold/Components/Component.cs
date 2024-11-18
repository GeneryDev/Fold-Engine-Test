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

    public ComponentInitializerAttribute(Type type, string memberName)
    {
        MethodInfo method = type.GetMethod(memberName, new[] { typeof(Scene), typeof(long) });
        Initializer = (scene, entityId) => method.Invoke(null, new object[] { scene, entityId });
    }
}