using FoldEngine.Components;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [Component("fold:physics")]
    [ComponentInitializer(typeof(Physics), nameof(InitializeComponent))]
    public struct Physics {
        public bool Static;
        
        public float GravityMultiplier;
        public float Mass;
        
        public Vector2 Velocity;
        public float AngularVelocity;

        public static Physics InitializeComponent(Scene scene, long entityId) {
            return new Physics() {
                GravityMultiplier = 1,
                Mass = 1
            };
        }
    }
}