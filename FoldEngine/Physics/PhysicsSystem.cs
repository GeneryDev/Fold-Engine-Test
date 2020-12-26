using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [GameSystem("fold:physics", ProcessingCycles.Update)]
    public class PhysicsSystem : GameSystem {
        private ComponentIterator<Physics> _physicsObjects;
        private ComponentIterator<MeshCollider> _colliders;

        public Vector2 Gravity = new Vector2(0, -90f);
        
        internal override void Initialize() {
            _physicsObjects = CreateComponentIterator<Physics>(IterationFlags.None);
            _colliders = CreateComponentIterator<MeshCollider>(IterationFlags.None);
        }

        public override void OnUpdate() {
            _physicsObjects.Reset();

            while(_physicsObjects.Next()) {
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
                ref Physics physics = ref _physicsObjects.GetComponent();

                if(!physics.Static) {
                    transform.Position += physics.Velocity * Time.DeltaTime;
                    physics.Velocity += Gravity * physics.GravityMultiplier * Time.DeltaTime;                    
                }

                if(_physicsObjects.HasCoComponent<MeshCollider>()) {
                    ref MeshCollider collider = ref _physicsObjects.GetCoComponent<MeshCollider>();

                    foreach(Line lineLocal in Owner.Meshes.GetLinesForMesh(collider.MeshIdentifier)) {
                        Line line = new Line(transform.Apply(lineLocal.From), transform.Apply(lineLocal.To));
                        
                        _colliders.Reset();

                        while(_colliders.Next()) {
                            if(_colliders.GetEntityId() == _physicsObjects.GetEntityId()) continue;

                            ref Transform otherTransform = ref _colliders.GetCoComponent<Transform>();
                            ref Physics otherPhysics = ref _colliders.GetCoComponent<Physics>();
                            ref MeshCollider otherCollider = ref _colliders.GetComponent();

                            foreach(Line otherLineLocal in Owner.Meshes.GetLinesForMesh(otherCollider.MeshIdentifier)) {
                                Line otherLine = new Line(otherTransform.Apply(otherLineLocal.From), otherTransform.Apply(otherLineLocal.To));

                                Vector2? intersection = line.Intersect(otherLine, true, true);
                                
                                if(intersection.HasValue) {
                                    Console.WriteLine("Intersection: " + intersection);

                                    Line lineCopy = line;
                                    Line.LayFlat(otherLine, ref lineCopy, out _);

                                    if(!physics.Static) {
                                        
                                        if(lineCopy.From.Y < 0) {
                                            transform.Position += -lineCopy.From.Y * (otherLine.Normal);
                                            physics.Velocity =
                                                (((Complex) physics.Velocity) / (Complex) otherLine.Normal).ScaleAxes(0,
                                                    1)
                                                * (Complex) otherLine.Normal;
                                        } else if(lineCopy.To.Y < 0) {
                                            transform.Position += -lineCopy.To.Y * otherLine.Normal;
                                            physics.Velocity =
                                                (((Complex) physics.Velocity) / (Complex) otherLine.Normal).ScaleAxes(0,
                                                    1)
                                                * (Complex) otherLine.Normal;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }
        }
    }
}