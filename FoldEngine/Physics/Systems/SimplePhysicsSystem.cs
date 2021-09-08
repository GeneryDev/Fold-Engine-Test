using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Events;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Shard.Scripts.Instructions;

namespace FoldEngine.Physics {
    [GameSystem("fold:physics.simple", ProcessingCycles.FixedUpdate)]
    public class SimplePhysicsSystem : GameSystem {
        public static readonly bool DrawCollisionGizmos = false;
        
        private ComponentIterator<Physics> _physicsObjects;
        private ComponentIterator<Collider> _colliders;
        
        public Vector2 Gravity = new Vector2(0, -27);
        public float FaceNormalVelocityDotTolerance = 1f;
        
        internal override void Initialize() {
            _physicsObjects = CreateComponentIterator<Physics>(IterationFlags.None);
            _colliders = CreateComponentIterator<Collider>(IterationFlags.None);
        }
        
        public override void OnFixedUpdate() {
            ApplyDynamics();
            CalculateForcesAndCollision();
            ApplyContactDisplacement();
            ApplyAndResetForces();
        }

        private void CalculateForcesAndCollision() {
            _physicsObjects.Reset();
            while(_physicsObjects.Next()) {
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
                ref Physics physics = ref _physicsObjects.GetComponent();

                Vector2 transformPosition = transform.Position;
                float positionDelta = (transform.Position - physics.PreviousPosition).Length();

                if(!physics.Static) {
                    physics.ApplyForce(Gravity * physics.GravityMultiplier * physics.Mass, default, ForceMode.Continuous);
                }

                Collider collider = default;

                if(_physicsObjects.HasCoComponent<Collider>()) {
                    collider = _physicsObjects.GetCoComponent<Collider>();
                }

                if(collider.Type != ColliderType.None) {
                    Vector2[] colliderVertices = collider.GetVertices(ref transform);
                    float colliderReach = collider.GetReach(ref transform);

                    _colliders.Reset();
                    while(_colliders.Next()) {
                        if(_colliders.GetEntityId() == _physicsObjects.GetEntityId()) continue; //Skip if self
                        
                        ref Transform otherTransform = ref _colliders.GetCoComponent<Transform>();
                        if(!_colliders.HasCoComponent<Physics>()) continue;
                        ref Physics otherPhysics = ref _colliders.GetCoComponent<Physics>();
                        ref Collider otherCollider = ref _colliders.GetComponent();
                        if(otherCollider.Type == ColliderType.None) continue;
                        
                        //Skip if too far
                        float otherColliderReach = otherCollider.GetReach(ref otherTransform);
                        if(Vector2.DistanceSquared(transformPosition, otherTransform.Position)
                           > Math.Pow(colliderReach + otherColliderReach, 2)) continue;
                        
                        Vector2 relativeVelocity = physics.Velocity - otherPhysics.Velocity;

                        if(relativeVelocity != default) {
                            //Compute intersections
                            Polygon.PolygonIntersectionVertex[][] intersections =
                                Polygon.ComputePolygonIntersection(colliderVertices,
                                    otherCollider.GetVertices(ref otherTransform));
                            
                            Vector2 moveDirection = relativeVelocity.Normalized();
                            float largestCrossSection = 0;
                            Vector2 surfaceNormalSum = default;
                            Vector2 surfaceNormalSumFallback = default;
                            float totalSurfaceNormalFaceLength = 0;
                            float totalSurfaceNormalFaceLengthFallback = 0;

                            Vector2 tempNormalStart = default;
                            
                            if(intersections != null && intersections.Length != 0) {
                                foreach(Polygon.PolygonIntersectionVertex[] intersection in intersections) {
                                    for(int i = 0; i < intersection.Length; i++) {
                                        Polygon.PolygonIntersectionVertex current = intersection[i];
                                        Polygon.PolygonIntersectionVertex next = intersection[(i+1) % intersection.Length];

                                        var face = new Line(current.Position, next.Position);
                                        float faceLength = face.Magnitude;
                                        Vector2 normal = face.Normal;

                                        float normalMoveDot = Vector2.Dot(normal, moveDirection);

                                        // Console.WriteLine(normalMoveDot);
                                        
                                        if((current.IsFromB && next.IsFromB)
                                           && normalMoveDot <= FaceNormalVelocityDotTolerance) {
                                            
                                            bool validFace;

                                            if(otherCollider.ThickFaces) validFace = true;
                                            else {
                                                float crossSection = Polygon.ComputeLargestCrossSection(intersection, normal);
                                                validFace = positionDelta >= crossSection - otherCollider.ThinFaceTolerance;
                                            }

                                            if(validFace) {
                                                if(current.IsFromA && next.IsFromA) {
                                                    surfaceNormalSumFallback += normal * faceLength;
                                                    totalSurfaceNormalFaceLengthFallback += faceLength;
                                                } else {
                                                    surfaceNormalSum += normal * faceLength;
                                                    totalSurfaceNormalFaceLength += faceLength;
                                                }
                                                
                                                tempNormalStart = current.Position;
                                            }
                                            
                                        }
                                        //Draw gizmos
                                        if(DrawCollisionGizmos) {
                                            Owner.DrawGizmo(current.Position, next.Position, Color.Fuchsia, zOrder: 1);
                                        }
                                    }

                                    if(totalSurfaceNormalFaceLength == 0) {
                                        totalSurfaceNormalFaceLength = totalSurfaceNormalFaceLengthFallback;
                                        surfaceNormalSum = surfaceNormalSumFallback;
                                    }

                                    if(totalSurfaceNormalFaceLength != 0) {
                                        float crossSection = Polygon.ComputeLargestCrossSection(intersection, surfaceNormalSum / totalSurfaceNormalFaceLength);
                                        largestCrossSection = Math.Max(crossSection, largestCrossSection);
                                    }
                                }
                            
                                // Line gizmoLine = new Line(transformPosition, otherTransform.Position);
                                // Owner.DrawGizmo(gizmoLine.From + gizmoLine.Normal * 0.1f, gizmoLine.To + gizmoLine.Normal * 0.1f, Color.Red, Color.Black);
                                
                                float restitution = Math.Max(physics.Restitution, otherPhysics.Restitution);
                                float friction = otherPhysics.Friction;
                                float damping = 1f;
                            
                                if(!largestCrossSection.Equals(float.NaN) && totalSurfaceNormalFaceLength > 0) {
                                    Complex surfaceNormalComplex = surfaceNormalSum / totalSurfaceNormalFaceLength;
                                    
                                    Owner.Events.Invoke(new CollisionEvent(_physicsObjects.GetEntityId(), _colliders.GetEntityId(), surfaceNormalComplex));
                                    Owner.Events.Invoke(new CollisionEvent(_colliders.GetEntityId(), _physicsObjects.GetEntityId(), -(Vector2)surfaceNormalComplex));

                                    if(!physics.Static) {
                                        if(DrawCollisionGizmos) {
                                            Owner.DrawGizmo(tempNormalStart, tempNormalStart + surfaceNormalSum / totalSurfaceNormalFaceLength, Color.Gold);
                                        }

                                        physics.ContactDisplacement += surfaceNormalSum / totalSurfaceNormalFaceLength * largestCrossSection;

                                        float velocityInNormalDirection = (((Complex) (physics.Velocity - otherPhysics.Velocity)) / surfaceNormalComplex).A;
                                        float velocityInNormalPerpendicular = (((Complex) (physics.Velocity - otherPhysics.Velocity)) / surfaceNormalComplex).B;
                                        
                                        // Vector2 normalForce = ((Vector2)surfaceNormalComplex.Normalized) * physics.Mass * -velocityInNormalDirection;
                                        // physics.ApplyForce(normalForce, default, ForceMode.Continuous);

                                        physics.ApplyForce((Vector2)surfaceNormalComplex.Normalized * -damping * velocityInNormalDirection * (1+restitution) * physics.Mass, default, ForceMode.Instant);
                                        physics.ApplyForce((Vector2)(surfaceNormalComplex.Normalized * Complex.Imaginary) * -velocityInNormalPerpendicular * friction * physics.Mass, default, ForceMode.Instant);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private const bool UseVerletIntegration = false;

        private void ApplyDynamics() {
            _physicsObjects.Reset();
            while(_physicsObjects.Next()) {
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
                ref Physics physics = ref _physicsObjects.GetComponent();

                Vector2 oldPos = transform.Position;
                
                if(!physics.Static) {
                    if(UseVerletIntegration) {
                        transform.Position = oldPos + physics.Velocity * Time.FixedDeltaTime + (0.5f * physics.PreviousAcceleration * Time.FixedDeltaTime * Time.FixedDeltaTime);
                    } else {
                        transform.Position = oldPos + physics.Velocity * Time.FixedDeltaTime;
                    }
                }

                physics.PreviousPosition = oldPos;
            }
        }

        private void ApplyContactDisplacement() {
            _physicsObjects.Reset();
            while(_physicsObjects.Next()) {
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
                ref Physics physics = ref _physicsObjects.GetComponent();

                if(!physics.Static) {
                    transform.Position += physics.ContactDisplacement;
                }
                physics.ContactDisplacement = default;
            }
        }

        private void ApplyAndResetForces() {
            _physicsObjects.Reset();
            while(_physicsObjects.Next()) {
                ref Physics physics = ref _physicsObjects.GetComponent();

                if(!physics.Static) {
                    if(UseVerletIntegration) {
                        physics.Velocity += (physics.AccelerationFromForce + physics.PreviousAcceleration) * 0.5f * Time.FixedDeltaTime;
                    } else {
                        physics.Velocity += physics.AccelerationFromForce * Time.FixedDeltaTime;
                    }
                } else {
                    physics.Velocity = default;
                }

                physics.PreviousAcceleration = physics.AccelerationFromForce;
                physics.AccelerationFromForce = default;
                physics.Torque = default;
            }
        }
    }

    [Event("collision")]
    public struct CollisionEvent {
        public long First;
        public long Second;
        public Vector2 Normal;

        public CollisionEvent(long first, long second, Vector2 normal) {
            this.First = first;
            this.Second = second;
            this.Normal = normal;
        }
    }
}