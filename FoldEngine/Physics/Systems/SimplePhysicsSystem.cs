using System;
using System.Collections;
using System.Collections.Generic;
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
        public static readonly bool DrawCollisionGizmos = true;
        
        private ComponentIterator<Physics> _physicsObjects;
        private ComponentIterator<Collider> _colliders;
        
        public Vector2 Gravity = new Vector2(0, -27);
        public float FaceNormalVelocityDotTolerance = 0f;

        private List<CollisionEvent> _collisions = new List<CollisionEvent>();
        
        internal override void Initialize() {
            _physicsObjects = CreateComponentIterator<Physics>(IterationFlags.None);
            _colliders = CreateComponentIterator<Collider>(IterationFlags.None);
        }
        
        public override void OnFixedUpdate() {
            ApplyDynamics();
            CollisionDetectionAndResponse();
            // CalculateForcesAndCollision();
            // if(_collisions.Count > 0) CollisionResponse();
            // ApplyContactDisplacement();
            ApplyAndResetForces();
        }
        
        private List<IntersectionData> _nearbyIntersections = new List<IntersectionData>();

        private void CollisionDetectionAndResponse() {
            _physicsObjects.Reset();
            while(_physicsObjects.Next()) {
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
                ref Physics physics = ref _physicsObjects.GetComponent();
                
                Vector2 transformPosition = transform.Position;
                
                if(!physics.Static) {
                    physics.ApplyForce(Gravity * physics.GravityMultiplier * physics.Mass, default, ForceMode.Continuous);
                }
                if(DrawCollisionGizmos) {
                    Scene.DrawGizmo(transformPosition, transformPosition + physics.Velocity.Normalized() * 1, Color.Gold, zOrder: 1);
                }
                
                Collider collider = default;

                if(_physicsObjects.HasCoComponent<Collider>()) {
                    collider = _physicsObjects.GetCoComponent<Collider>();
                }

                if(collider.Type == ColliderType.None) continue;
                
                float colliderReach = collider.GetReach(ref transform);
                
                //Get list of nearby colliders
                _nearbyIntersections.Clear();
                
                _colliders.Reset();
                while(_colliders.Next()) {
                    //Skip if self
                    if(_colliders.GetEntityId() == _physicsObjects.GetEntityId()) continue;
                    
                    ref Collider otherCollider = ref _colliders.GetComponent();
                    //Skip if no collider
                    if(otherCollider.Type == ColliderType.None) continue;

                    //Skip static-static pairs
                    if(physics.Static) {
                        if(!_colliders.HasCoComponent<Physics>()) continue; 
                        ref Physics otherPhysics = ref _colliders.GetCoComponent<Physics>();
                        if(otherPhysics.Static) continue;
                    }
                    
                    //Skip if too far
                    ref Transform otherTransform = ref _colliders.GetCoComponent<Transform>();
                    float otherColliderReach = otherCollider.GetReach(ref otherTransform);
                    if(Vector2.DistanceSquared(transformPosition, otherTransform.Position)
                       > Math.Pow(colliderReach + otherColliderReach, 2)) continue;
                    
                    _nearbyIntersections.Add(new IntersectionData(_colliders.GetEntityId()));
                }

                if(_nearbyIntersections.Count == 0) continue;
                
                //Calculate initial intersection of each nearby collider
                CalculateIntersections(collider.GetVertices(ref transform), physics.Velocity, physics.PreviousPosition - transformPosition);

                while(_nearbyIntersections.Count > 0) {
                    // if(_nearbyIntersections[0].Intersections == null) break; //No more intersections
                    int i = 0;
                    for(; i < _nearbyIntersections.Count; i++) {
                        if(_nearbyIntersections[i].Intersections != null) break;
                    }
                    if(i >= _nearbyIntersections.Count) break;
                    
                    CollisionResponse(_nearbyIntersections[i], new Entity(Scene, _physicsObjects.GetEntityId()));
                    
                    _nearbyIntersections.RemoveAt(i);
                    CalculateIntersections(collider.GetVertices(ref transform), physics.Velocity, physics.PreviousPosition - transform.Position);
                }
            }
        }

        private void CalculateIntersections(Vector2[] colliderVertices, Vector2 velocity, Vector2 positionDelta) {
            for(int i = 0; i < _nearbyIntersections.Count; i++) {
                IntersectionData data = _nearbyIntersections[i];
                Entity other = new Entity(Scene, data.Other);

                ref Transform otherTransform = ref other.Transform;
                ref Physics otherPhysics = ref other.GetComponent<Physics>();
                ref Collider otherCollider = ref other.GetComponent<Collider>();
                    
                Polygon.PolygonIntersectionVertex[][] intersections =
                    Polygon.ComputePolygonIntersection(colliderVertices,
                        otherCollider.GetVertices(ref otherTransform));
                if(intersections != null && intersections.Length == 0) intersections = null;
                data.Intersections = intersections;
                data.Area = -1;

                if(data.Intersections != null) {
                    float area = 0;
                    foreach(Polygon.PolygonIntersectionVertex[] intersection in data.Intersections) {
                        area += Polygon.ComputePolygonArea(intersection);

                        for(int j = 0; j < intersection.Length; j++) {
                            
                            Polygon.PolygonIntersectionVertex current = intersection[j];
                            Polygon.PolygonIntersectionVertex next = intersection[(j + 1) % intersection.Length];
                            
                            if(current.IsFromB && next.IsFromB) {
                                data.ContactPoint = current.Position;
                            }
                        }
                    }

                    data.Area = area;

                    Vector2 vecAToCompare = Gravity.Normalized();

                    if(!float.IsNaN(vecAToCompare.X)) {
                        data.SortKey = -(((Complex) data.ContactPoint) / (Complex) vecAToCompare).A;
                    } else {
                        data.SortKey = 0;
                    }
                    
                    Vector2 vecBToCompare = velocity.Normalized();

                    if(!float.IsNaN(vecBToCompare.X)) {
                        data.SortKeyB = -(((Complex) data.ContactPoint) / (Complex) vecBToCompare).A;
                    } else {
                        data.SortKeyB = 0;
                    }
                }

                _nearbyIntersections[i] = data;
            }
            // _nearbyIntersections.Sort((a, b) => Math.Sign(a.Area - b.Area)); //ascending, bad surface behavior, good squishing behavior
            // _nearbyIntersections.Sort((a, b) => Math.Sign(b.Area - a.Area)); //descending, good surface behavior, bad squishing behavior
            _nearbyIntersections.Sort((a, b) => {
                int signA = Math.Sign(b.SortKey - a.SortKey);
                if(signA != 0) return signA;
                return Math.Sign(b.SortKeyB - a.SortKeyB);
            });
        }

        private void CollisionResponse(IntersectionData data, Entity entity) {
            ref Transform transform = ref entity.Transform;
            ref Physics physics = ref entity.GetComponent<Physics>();
            ref Collider collider = ref entity.GetComponent<Collider>();

            Entity other = new Entity(entity.Scene, data.Other);
            ref Transform otherTransform = ref other.Transform;
            ref Physics otherPhysics = ref other.GetComponent<Physics>();
            ref Collider otherCollider = ref other.GetComponent<Collider>();
            
            float positionDelta = (transform.Position - physics.PreviousPosition).Length();
            Vector2 relativeVelocity = physics.Velocity - otherPhysics.Velocity;
            if(relativeVelocity == default) return;
            
            Vector2 moveDirection = relativeVelocity.Normalized();
            float largestNormalDepth = 0;
            float largestMoveDepth = 0;
            Vector2 surfaceNormal = default;
            float maxSurfaceNormalFaceLength = 0;

            Vector2 tempNormalStart = default;

            foreach(Polygon.PolygonIntersectionVertex[] intersection in data.Intersections) {
                for(int i = 0; i < intersection.Length; i++) {
                    Polygon.PolygonIntersectionVertex current = intersection[i];
                    Polygon.PolygonIntersectionVertex next = intersection[(i + 1) % intersection.Length];

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
                                if(maxSurfaceNormalFaceLength == 0) {
                                    surfaceNormal = normal;
                                    maxSurfaceNormalFaceLength = faceLength;
                                }

                                // surfaceNormalSumFallback += normal * faceLength;
                                // totalSurfaceNormalFaceLengthFallback += faceLength;
                            } else {
                                if(faceLength > maxSurfaceNormalFaceLength) {
                                    surfaceNormal = normal;
                                    maxSurfaceNormalFaceLength = faceLength;
                                }
                            }

                            tempNormalStart = current.Position;
                        }

                    }

                    //Draw gizmos
                    if(DrawCollisionGizmos) {
                        Scene.DrawGizmo(current.Position, next.Position, Color.Fuchsia, zOrder: 1);
                    }
                }

                if(maxSurfaceNormalFaceLength != 0) {
                    float moveDepth = Polygon.ComputeLargestCrossSection(intersection, moveDirection);
                    largestMoveDepth = Math.Max(moveDepth, largestMoveDepth);

                    float normalDepth = Polygon.ComputeLargestCrossSection(intersection, surfaceNormal);
                    largestNormalDepth = Math.Max(normalDepth, largestNormalDepth);
                }
            }

            Line gizmoLine = new Line(tempNormalStart, tempNormalStart + surfaceNormal);
            Scene.DrawGizmo(gizmoLine.From + gizmoLine.Normal * 0.1f, gizmoLine.To + gizmoLine.Normal * 0.1f,
                Color.Red, Color.Black);

            if(!largestMoveDepth.Equals(float.NaN) && maxSurfaceNormalFaceLength > 0) {
                Complex surfaceNormalComplex = surfaceNormal;
                if(largestNormalDepth <= 0) return;
                if(Math.Abs(Vector2.Dot(moveDirection, surfaceNormal)) <= 0.01f) return;

                var collision = new CollisionEvent() {
                    First = entity.EntityId,
                    Second = other.EntityId,
                    Normal = surfaceNormalComplex,
                    Direction = moveDirection,
                    NormalDepth = largestNormalDepth,
                    DirectionDepth = largestMoveDepth,
                    Separation = relativeVelocity.Length() - largestMoveDepth
                };
                
                if(!physics.Static) {
                    float friction = otherPhysics.Friction;
                    float restitution = Math.Max(physics.Restitution, otherPhysics.Restitution);
                    if(AttemptDisplacement(entity, ref physics, -collision.Direction.Normalized() * collision.DirectionDepth, collision.Normal.Normalized(), friction)) {
                        ApplyContactForces(ref physics, ref otherPhysics, collision.Normal, friction, restitution);
                    }
                }
                
                Scene.Events.Invoke(collision);
            }
        }

        private void CalculateForcesAndCollision() {
            _collisions.Clear();
            _physicsObjects.Reset();
            while(_physicsObjects.Next()) {
                ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
                ref Physics physics = ref _physicsObjects.GetComponent();

                Vector2 transformPosition = transform.Position;
                float positionDelta = (transform.Position - physics.PreviousPosition).Length();

                if(!physics.Static) {
                    physics.ApplyForce(Gravity * physics.GravityMultiplier * physics.Mass, default, ForceMode.Continuous);
                }
                if(DrawCollisionGizmos) {
                    Scene.DrawGizmo(transformPosition, transformPosition + physics.Velocity.Normalized() * 1, Color.Gold, zOrder: 1);
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
                            float largestNormalDepth = 0;
                            float largestMoveDepth = 0;
                            Vector2 surfaceNormal = default;
                            float maxSurfaceNormalFaceLength = 0;

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
                                                    if(maxSurfaceNormalFaceLength == 0) {
                                                        surfaceNormal = normal;
                                                        maxSurfaceNormalFaceLength = faceLength;
                                                    }
                                                    // surfaceNormalSumFallback += normal * faceLength;
                                                    // totalSurfaceNormalFaceLengthFallback += faceLength;
                                                } else {
                                                    if(faceLength > maxSurfaceNormalFaceLength) {
                                                        surfaceNormal = normal;
                                                        maxSurfaceNormalFaceLength = faceLength;
                                                    }
                                                }
                                                
                                                tempNormalStart = current.Position;
                                            }
                                            
                                        }
                                        //Draw gizmos
                                        if(DrawCollisionGizmos) {
                                            Scene.DrawGizmo(current.Position, next.Position, Color.Fuchsia, zOrder: 1);
                                        }
                                    }

                                    if(maxSurfaceNormalFaceLength != 0) {
                                        float moveDepth = Polygon.ComputeLargestCrossSection(intersection, moveDirection);
                                        largestMoveDepth = Math.Max(moveDepth, largestMoveDepth);
                                        
                                        float normalDepth = Polygon.ComputeLargestCrossSection(intersection, surfaceNormal);
                                        largestNormalDepth = Math.Max(normalDepth, largestNormalDepth);
                                    }
                                }
                            
                                Line gizmoLine = new Line(tempNormalStart, tempNormalStart + surfaceNormal);
                                Scene.DrawGizmo(gizmoLine.From + gizmoLine.Normal * 0.1f, gizmoLine.To + gizmoLine.Normal * 0.1f, Color.Red, Color.Black);
                                
                                if(!largestMoveDepth.Equals(float.NaN) && maxSurfaceNormalFaceLength > 0) {
                                    Complex surfaceNormalComplex = surfaceNormal;
                                    if(largestNormalDepth <= 0) continue;
                                    if(Math.Abs(Vector2.Dot(moveDirection, surfaceNormal)) <= 0.01f) continue;
                                    
                                    _collisions.Add(new CollisionEvent() {
                                        First = _physicsObjects.GetEntityId(),
                                        Second = _colliders.GetEntityId(),
                                        Normal = surfaceNormalComplex,
                                        Direction = moveDirection,
                                        NormalDepth = largestNormalDepth,
                                        DirectionDepth = largestMoveDepth,
                                        Separation = relativeVelocity.Length() - largestMoveDepth
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private const bool UseVerletIntegration = true;

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

        private static readonly Comparison<CollisionEvent> SortBySeparation = (a, b) => Math.Sign(b.Separation - a.Separation);

        private void CollisionResponse() {
            // if(_collisions.Count > 3 && Keyboard.GetState().IsKeyDown(Keys.A)) {
            //     Console.WriteLine("COLLISION");
            // }
            _collisions.Sort(SortBySeparation);

            foreach(CollisionEvent collision in _collisions) {
                Entity a = new Entity(Scene, collision.First);
                Entity b = new Entity(Scene, collision.Second);

                ref Physics physicsA = ref a.GetComponent<Physics>();
                ref Physics physicsB = ref b.GetComponent<Physics>();

                if(!physicsA.Static) {
                    float friction = physicsB.Friction;
                    float restitution = Math.Max(physicsA.Restitution, physicsB.Restitution);
                    if(AttemptDisplacement(a, ref physicsA, -collision.Direction.Normalized() * collision.DirectionDepth, collision.Normal.Normalized(), friction)) {
                        ApplyContactForces(ref physicsA, ref physicsB, collision.Normal, friction, restitution);
                    }
                }
                
                Scene.Events.Invoke(collision);
            }
        }

        private bool AttemptDisplacement(Entity entity, ref Physics physics, Vector2 amount, Vector2 surfaceNormal, float friction) {
            Complex normalComplex = surfaceNormal;
            
            float displacementInNormalDirection = ((Complex)amount / normalComplex).A;
            float displacementInTangentDirection = ((Complex)amount / normalComplex).B;
            displacementInTangentDirection *= (friction * Time.FixedDeltaTime);
            
            amount = new Complex(displacementInNormalDirection, displacementInTangentDirection) * normalComplex;

            if(true) {
                entity.Transform.Position += amount;
                return true;
            }
            
            float remainingDisplacement = amount.Length() - ((Complex)physics.ContactDisplacement / (Complex) amount.Normalized()).A;
            if(remainingDisplacement > 0) {
                Vector2 amountToDisplace = amount.Normalized() * remainingDisplacement;
                physics.ContactDisplacement += amountToDisplace;
                entity.Transform.Position += amountToDisplace;
                return true;
            }

            return false;
        }

        private void ApplyContactForces(ref Physics physicsA, ref Physics physicsB, Vector2 normal, float friction, float restitution) {
            Complex normalComplex = normal;
            
            float damping = 1f;
            
            float velocityInNormalDirection = (((Complex) (physicsA.Velocity - physicsB.Velocity)) / normalComplex).A;
            float velocityInTangentDirection = (((Complex) (physicsA.Velocity - physicsB.Velocity)) / normalComplex).B;
            
            // Vector2 normalForce = ((Vector2)surfaceNormalComplex.Normalized) * physics.Mass * -velocityInNormalDirection;
            // physics.ApplyForce(normalForce, default, ForceMode.Continuous);
            
            Vector2 normalForce = normal.Normalized()
                                  * -damping
                                  * velocityInNormalDirection
                                  * physicsA.Mass;
            
            Vector2 staticFriction =
                (Vector2) (normalComplex.Normalized * Complex.Imaginary)
                * -velocityInTangentDirection
                * physicsA.Mass;
            
            bool staticCapped = false;
            if(staticFriction.Length() > friction * normalForce.Length()) {
                staticCapped = true;
                staticFriction = staticFriction.Normalized() * friction * normalForce.Length();
            }
            
            Vector2 kineticFriction =
                ((Vector2) (normalComplex.Normalized * Complex.Imaginary)
                 * -velocityInTangentDirection).Normalized()
                * friction
                * normalForce.Length();
            
            Vector2 frictionForce = staticCapped ? kineticFriction : staticFriction;
            
            // physics.ApplyForce(normalForce, default, ForceMode.Instant);
            // physics.ApplyForce(frictionForce, default, ForceMode.Instant);
            
            physicsA.Velocity += (normalForce + frictionForce) / physicsA.Mass;
            
            physicsA.ApplyForce(normalForce * restitution, default, ForceMode.Instant);
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
                physics.PreviousVelocity = physics.Velocity;
                physics.Torque = default;

                physics.ContactDisplacement = default;
            }
        }
        
        private struct IntersectionData {
            public long Other;
            public Polygon.PolygonIntersectionVertex[][] Intersections;
            public float Area;
            public Vector2 ContactPoint;
            public float SortKey;
            public float SortKeyB;

            public IntersectionData(long otherId) {
                Other = otherId;
                Intersections = null;
                Area = 0;
                ContactPoint = default;
                SortKey = 0;
                SortKeyB = 0;
            }
        }
    }

    [Event("collision")]
    public struct CollisionEvent {
        public long First;
        public long Second;
        public Vector2 Normal;
        public Vector2 Direction;
        public float NormalDepth;
        public float DirectionDepth;
        public float Separation;

        public CollisionEvent(long first, long second, Vector2 normal, float depth = 0, float separation = 0) {
            this.First = first;
            this.Second = second;
            this.Normal = normal;
            this.Direction = normal;
            this.NormalDepth = depth;
            this.DirectionDepth = depth;
            this.Separation = separation;
        }
    }
}