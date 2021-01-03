﻿using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [GameSystem("fold:physics.simple", ProcessingCycles.Update)]
    public class SimplePhysicsSystem : GameSystem {
        private ComponentIterator<Physics> _physicsObjects;
        private ComponentIterator<BoxCollider> _colliders;
        
        public Vector2 Gravity = new Vector2(0, -18f);
        
        internal override void Initialize() {
            _physicsObjects = CreateComponentIterator<Physics>(IterationFlags.None);
            _colliders = CreateComponentIterator<BoxCollider>(IterationFlags.None);
        }
        
        public override void OnUpdate() {
            CalculateForcesAndCollision();
            SubmitDynamics();
        }

        private void CalculateForcesAndCollision() {
            _physicsObjects.Reset();
            while(_physicsObjects.Next()) {
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
                ref Physics physics = ref _physicsObjects.GetComponent();

                Vector2 transformPosition = transform.Position;

                if(!physics.Static) {
                    physics.Velocity += Gravity * physics.GravityMultiplier * Time.DeltaTime;                    
                }

                ICollider collider = null;

                if(_physicsObjects.HasCoComponent<BoxCollider>()) {
                    collider = _physicsObjects.GetCoComponent<BoxCollider>();
                } else if(_physicsObjects.HasCoComponent<MeshCollider>()) {
                    collider = _physicsObjects.GetCoComponent<MeshCollider>();
                }

                if(collider != null) {
                    float colliderReach = collider.GetReach(ref transform);

                    _colliders.Reset();
                    while(_colliders.Next()) {
                        if(_colliders.GetEntityId() == _physicsObjects.GetEntityId()) continue; //Skip if self
                        
                        ref Transform otherTransform = ref _colliders.GetCoComponent<Transform>();
                        ref Physics otherPhysics = ref _colliders.GetCoComponent<Physics>();
                        ref BoxCollider otherCollider = ref _colliders.GetComponent();
                        
                        
                        //Skip if too far
                        float otherColliderReach = otherCollider.GetReach(ref otherTransform);
                        if(Vector2.DistanceSquared(transformPosition, otherTransform.Position)
                           > Math.Pow(colliderReach + otherColliderReach, 2)) continue;

                        //Compute intersections
                        Polygon.PolygonIntersectionVertex[][] intersections =
                            Polygon.ComputePolygonIntersection(collider.GetVertices(ref transform),
                                otherCollider.GetVertices(ref otherTransform));
                        
                        Vector2 relativeVelocity = physics.Velocity - otherPhysics.Velocity;

                        if(relativeVelocity != default) {
                            Vector2 moveDirection = relativeVelocity.Normalized();
                            float largestCrossSection = 0;
                            Vector2 surfaceNormal = default;
                            float largestOtherFaceLengthSquared = 0;

                            Vector2 tempNormalStart = default;
                            
                            if(intersections != null && intersections.Length != 0) {
                                foreach(Polygon.PolygonIntersectionVertex[] intersection in intersections) {
                                    for(int i = 0; i < intersection.Length; i++) {
                                        Polygon.PolygonIntersectionVertex current = intersection[i];
                                        Polygon.PolygonIntersectionVertex next = intersection[(i+1) % intersection.Length];

                                        var face = new Line(current.Position, next.Position);
                                        Vector2 normal = face.Normal;
                                        
                                        if(next.IsFromB
                                           && current.VertexIndexA != next.VertexIndexA
                                           && Vector2.Dot(normal, moveDirection) <= 0
                                           && face.MagnitudeSqr > largestOtherFaceLengthSquared) {
                                            surfaceNormal = normal;

                                            tempNormalStart = face.Center;

                                            largestOtherFaceLengthSquared = face.MagnitudeSqr;
                                            
                                            float crossSection = Polygon.ComputeLargestCrossSection(intersection, normal);

                                            largestCrossSection = crossSection;
                                        }
                                        //Draw gizmos
                                        Owner.DrawGizmo(current.Position, next.Position, Color.Fuchsia);
                                    }
                                }
                            
                                Line gizmoLine = new Line(transformPosition, otherTransform.Position);
                                Owner.DrawGizmo(gizmoLine.From + gizmoLine.Normal * 0.1f, gizmoLine.To + gizmoLine.Normal * 0.1f, Color.Red, Color.Black);
                            }

                            float restitution = 0.4f; //TODO get from components
                            float friction = 0.2f; //TODO get from components
                            
                            if(!largestCrossSection.Equals(float.NaN) && surfaceNormal != default) {
                                if(!physics.Static) {
                                    Owner.DrawGizmo(tempNormalStart, tempNormalStart + surfaceNormal, Color.Gold);
                                }
                                    
                                Complex surfaceNormalComplex = surfaceNormal;

                                float positionDelta = -((Complex) (transformPosition - physics.PreviousPosition)
                                                        / (Complex) surfaceNormal).A;
                                    
                                if(otherCollider.ThickFaces || positionDelta >= largestCrossSection - 0.00001) {
                                    physics.ContactDisplacement = surfaceNormal * largestCrossSection;

                                    Vector2 expectedVelocity =
                                        (((Complex) physics.Velocity) / surfaceNormalComplex).ScaleAxes(
                                            -restitution,
                                            1 - friction)
                                        * surfaceNormalComplex;
                                    Vector2 velocityDelta = expectedVelocity - physics.Velocity;
                                    Vector2 force = velocityDelta * physics.Mass / Time.DeltaTime;
                                    physics.ApplyForce(force, Vector2.Zero);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SubmitDynamics() {
            _physicsObjects.Reset();
            while(_physicsObjects.Next()) {
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
                ref Physics physics = ref _physicsObjects.GetComponent();

                Vector2 oldPos = transform.Position;
                
                if(!physics.Static) {
                    physics.Velocity += physics.AccelerationFromForce * Time.DeltaTime;
                    transform.Position = oldPos + physics.Velocity * Time.DeltaTime + physics.ContactDisplacement;
                }
                physics.ContactDisplacement = default;
                physics.AccelerationFromForce = default;
                physics.Torque = default;

                physics.PreviousPosition = oldPos;
            }
        }
    }
}