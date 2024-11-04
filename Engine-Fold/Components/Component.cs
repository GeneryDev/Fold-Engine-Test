using System;
using System.Reflection;
using FoldEngine.Scenes;

namespace FoldEngine.Components {
    public sealed class ComponentAttribute : Attribute {
        public readonly string ComponentName;

        public ComponentAttribute(string identifier) {
            ComponentName = identifier;
        }
    }

    public sealed class ComponentInitializerAttribute : Attribute {
        public readonly Func<Scene, long, object> Initializer;

        public ComponentInitializerAttribute(Type type, string memberName) {
            MethodInfo method = type.GetMethod(memberName, new[] {typeof(Scene), typeof(long)});
            Initializer = (scene, entityId) => method.Invoke(null, new object[] {scene, entityId});
        }
    }
}