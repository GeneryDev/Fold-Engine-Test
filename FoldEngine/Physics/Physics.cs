using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [Component("fold:physics")]
    [ComponentInitializer(typeof(Physics), nameof(InitializeComponent))]
    public struct Physics {
        private Scene _scene;
        private long _entityId;
        
        public bool Static;
        
        public float GravityMultiplier;
        public float Mass;
        
        public Vector2 Velocity;
        public float AngularVelocity;

        public Vector2 AccelerationFromForce;
        public float Torque;

        public Vector2 ContactDisplacement;

        public static Physics InitializeComponent(Scene scene, long entityId) {
            return new Physics() {
                _scene = scene,
                _entityId = entityId,
                GravityMultiplier = 1,
                Mass = 1
            };
        }

        public void ApplyForce(Vector2 force, Vector2 point) {
            if(force == default) return;
            Complex diff = ((((Complex) force.Normalized()) / (Complex) point.Normalized())).Normalized;
            if(point == Vector2.Zero) diff = force.Normalized();
            
            Vector2 accel = force / Mass;
            float torque = (point.Length()*force.Length() / 1000) * diff.B;
            
            AccelerationFromForce += accel;
            Torque += torque;





            Vector2 ownerPos = _scene.Components.GetComponent<Transform>(_entityId).Position;
            _scene.Systems.Get<PhysicsSystem>()?.DrawGizmo(ownerPos + point, ownerPos + point + force / 10, Color.Red);
        }
    }
}