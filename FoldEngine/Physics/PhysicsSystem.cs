using System;
using System.Diagnostics;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Physics {
    [GameSystem("fold:physics", ProcessingCycles.Update)]
    public class PhysicsSystem : GameSystem {
        private ComponentIterator<Physics> _physicsObjects;
        private ComponentIterator<MeshCollider> _colliders;

        public Vector2 Gravity = new Vector2(0, -400f);
        
        internal override void Initialize() {
            _physicsObjects = CreateComponentIterator<Physics>(IterationFlags.None);
            _colliders = CreateComponentIterator<MeshCollider>(IterationFlags.None);
        }

        public override void OnUpdate() {
            if(Mouse.GetState().LeftButton == ButtonState.Pressed) return;
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

                    _colliders.Reset();
                    
                    while(_colliders.Next()) {
                        if(_colliders.GetEntityId() == _physicsObjects.GetEntityId()) continue;
                        
                        ref Transform otherTransform = ref _colliders.GetCoComponent<Transform>();
                        ref Physics otherPhysics = ref _colliders.GetCoComponent<Physics>();
                        ref MeshCollider otherCollider = ref _colliders.GetComponent();

                        Polygon.PolygonIntersectionVertex[][] intersections = Polygon.ComputePolygonIntersection(
                            Owner.Meshes, collider.MeshIdentifier, transform,
                            otherCollider.MeshIdentifier, otherTransform);

                        if(intersections != null && intersections.Length > 0) {

                            if(!physics.Static) {
                                Vector2 moveDirection = (physics.Velocity - otherPhysics.Velocity).Normalized();

                                Vector2 normalSum = Vector2.Zero;
                                int totalNormals = 0;

                                float maxDisplacement = 0;
                                
                                for(int i = 0; i < intersections.Length; i++) {
                                    maxDisplacement = Math.Max(maxDisplacement,
                                        Polygon.ComputeLargestCrossSection(intersections[i], moveDirection));
                                    
                                    for(int j = 0; j < intersections[i].Length; j++) {
                                        var intersectionVertex = intersections[i][j];
                                        if(intersectionVertex.IsFromB) {
                                            var nextIntersectionVertex =
                                                intersections[i][(j + 1) % intersections[i].Length];
                                            
                                            if(nextIntersectionVertex.IsFromB && intersectionVertex.VertexIndexA != nextIntersectionVertex.VertexIndexA) {
                                                Line line = new Line(intersectionVertex.Position,
                                                    nextIntersectionVertex.Position);
                                                if(line.MagnitudeSqr > 0) {
                                                    Vector2 normal = line.Normal;
                                                    if(Vector2.Dot(normal, moveDirection) <= 0) {
                                                        normalSum += normal;
                                                        totalNormals++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                

                                float restitution = 0.0f; //TODO get from components
                                float friction = 0.0f; //TODO get from components

                                // Console.WriteLine($"maxDisplacement = {maxDisplacement}");
                                if(!maxDisplacement.Equals(float.NaN)) {
                                    transform.Position -= moveDirection * maxDisplacement;
                                }
                                
                                if(totalNormals != 0 && normalSum.Length() > 0) {
                                    Vector2 surfaceNormal = (normalSum / totalNormals).Normalized();
                                    // Console.WriteLine($"surfaceNormal = {surfaceNormal}");
                                    // Console.WriteLine($"physics.Velocity (before) = {physics.Velocity}");
                                    physics.Velocity =
                                        (((Complex) physics.Velocity) / (Complex) surfaceNormal).ScaleAxes(
                                            -restitution,
                                            1 - friction)
                                        * (Complex) surfaceNormal;
                                    // Console.WriteLine($"physics.Velocity (after) = {physics.Velocity}");
                                    
                                } else {
                                    Console.WriteLine("No surface normals?");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}