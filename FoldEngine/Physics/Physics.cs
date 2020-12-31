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
                GravityMultiplier = 1,
                Mass = 1
            };
        }

        public void ApplyForce(Vector2 force, Vector2 point) {
            if(force == default) return;
            Complex diff = ((((Complex) force.Normalized()) / (Complex) point.Normalized())).Normalized;
            
            Vector2 accel = (force / Mass) * -diff.A;
            float torque = (point.Length()*force.Length() / 10000) * diff.B;
            
            AccelerationFromForce += accel;
            Torque += torque;
        }
    }
}