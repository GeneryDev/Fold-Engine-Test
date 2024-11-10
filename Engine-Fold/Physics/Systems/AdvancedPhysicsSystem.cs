using System;
using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Physics.Systems;

[GameSystem("fold:physics.advanced", ProcessingCycles.FixedUpdate)]
public class AdvancedPhysicsSystem : GameSystem
{
    private ComponentIterator<Collider> _colliders;
    private ComponentIterator<Physics> _physicsObjects;

    public Vector2 Gravity = new Vector2(0, -40f);

    public override void Initialize()
    {
        _physicsObjects = CreateComponentIterator<Physics>(IterationFlags.None);
        _colliders = CreateComponentIterator<Collider>(IterationFlags.None);
    }

    public override void OnFixedUpdate()
    {
        if (Mouse.GetState().LeftButton == ButtonState.Pressed) return;
        _physicsObjects.Reset();

        while (_physicsObjects.Next())
        {
            ref Transform transform = ref _physicsObjects.GetCoComponent<Transform>();
            ref Physics physics = ref _physicsObjects.GetComponent();

            if (!physics.Static)
                physics.ApplyForce(Gravity * physics.GravityMultiplier, Vector2.Zero, ForceMode.Continuous);

            if (_physicsObjects.HasCoComponent<Collider>())
            {
                ref Collider collider = ref _physicsObjects.GetCoComponent<Collider>();

                _colliders.Reset();

                while (_colliders.Next())
                {
                    if (_colliders.GetEntityId() == _physicsObjects.GetEntityId()) continue;

                    ref Transform otherTransform = ref _colliders.GetCoComponent<Transform>();
                    ref Physics otherPhysics = ref _colliders.GetCoComponent<Physics>();
                    ref Collider otherCollider = ref _colliders.GetComponent();

                    Polygon.PolygonIntersectionVertex[][] intersections = Polygon.ComputePolygonIntersection(
                        collider.GetVertices(ref transform),
                        otherCollider.GetVertices(ref otherTransform)
                    );

                    if (intersections != null && intersections.Length > 0)
                        if (!physics.Static)
                        {
                            Vector2 relativeVelocity = physics.Velocity - otherPhysics.Velocity;
                            Vector2 moveDirection = relativeVelocity.Normalized();

                            Vector2 normalSum = Vector2.Zero;
                            int totalNormals = 0;

                            float maxDisplacement = 0;
                            Vector2 totalContactVertexVelocity = Vector2.Zero;
                            int verticesCountedForTotalContactVertexVelocity = 0;

                            Vector2 currentPos = transform.Position;
                            Complex rotationByTorque =
                                Complex.FromRotation(physics.AngularVelocity * Time.FixedDeltaTime);

                            for (int i = 0; i < intersections.Length; i++)
                            {
                                maxDisplacement = Math.Max(maxDisplacement,
                                    Polygon.ComputeLargestCrossSection(intersections[i], moveDirection));

                                Vector2 totalVertexVelocity = Vector2.Zero;
                                int verticesCountedForVelocity = 0;

                                for (int j = 0; j < intersections[i].Length; j++)
                                {
                                    Polygon.PolygonIntersectionVertex vertex = intersections[i][j];
                                    Scene.DrawGizmo(vertex.Position,
                                        intersections[i][(j + 1) % intersections[i].Length].Position, Color.Gray);

                                    if (vertex.IsFromA && !vertex.IsFromB)
                                    {
                                        Vector2 projectedNextVertexPosition =
                                            (Complex)(vertex.Position - currentPos) * rotationByTorque
                                            + (Complex)currentPos;
                                        //TODO relativize to the other object
                                        Vector2 vertexVelocity = physics.Velocity
                                                                 + (projectedNextVertexPosition - vertex.Position)
                                                                 / Time.FixedDeltaTime;
                                        totalVertexVelocity += vertexVelocity;
                                        totalContactVertexVelocity += vertexVelocity;
                                        verticesCountedForVelocity++;
                                        verticesCountedForTotalContactVertexVelocity++;

                                        Scene.DrawGizmo(vertex.Position, vertex.Position + vertexVelocity,
                                            Color.Blue);
                                    }
                                }

                                if (verticesCountedForVelocity == 0)
                                {
                                    // Console.WriteLine("no A vertex counted for velocity");
                                }

                                Vector2 averageVertexVelocity = verticesCountedForVelocity == 0
                                    ? relativeVelocity
                                    : totalVertexVelocity / verticesCountedForVelocity;
                                Vector2 averageVertexMoveDirection = averageVertexVelocity.Normalized();

                                Vector2 contactPoint = Polygon.ComputeHighestPoint(intersections[i],
                                    -averageVertexMoveDirection);

                                for (int j = 0; j < intersections[i].Length; j++)
                                {
                                    Polygon.PolygonIntersectionVertex intersectionVertex = intersections[i][j];
                                    if (intersectionVertex.IsFromB)
                                    {
                                        Polygon.PolygonIntersectionVertex nextIntersectionVertex =
                                            intersections[i][(j + 1) % intersections[i].Length];

                                        if (nextIntersectionVertex.IsFromB
                                            && intersectionVertex.VertexIndexA
                                            != nextIntersectionVertex.VertexIndexA)
                                        {
                                            var line = new Line(intersectionVertex.Position,
                                                nextIntersectionVertex.Position);
                                            if (line.MagnitudeSqr > 0)
                                            {
                                                Vector2 normal = line.Normal;
                                                if (Vector2.Dot(normal, averageVertexMoveDirection) <= 0)
                                                {
                                                    normalSum += normal;
                                                    totalNormals++;
                                                    Vector2 force = normal
                                                                    * averageVertexVelocity.Length()
                                                                    * 10
                                                                    * physics.Mass;
                                                    physics.ApplyForce(
                                                        force,
                                                        contactPoint - transform.Position,
                                                        ForceMode.Continuous);
                                                    // Console.WriteLine("Applying force in direction: " + normal);
                                                    // Console.WriteLine("at location: " + contactPoint);
                                                }
                                            }
                                        }
                                    }
                                }
                            }


                            float restitution = 0.0f; //TODO get from components
                            float friction = 0.0f; //TODO get from components

                            // Console.WriteLine($"maxDisplacement = {maxDisplacement}");
                            if (!maxDisplacement.Equals(float.NaN)
                                && verticesCountedForTotalContactVertexVelocity != 0)
                                transform.Position -= moveDirection * maxDisplacement;
                            // physics.ContactDisplacement = -(totalContactVertexVelocity / verticesCountedForTotalContactVertexVelocity).Normalized() * maxDisplacement;

                            if (totalNormals != 0 && normalSum.Length() > 0)
                            {
                                Vector2 surfaceNormal = (normalSum / totalNormals).Normalized();
                                Scene.Events.Invoke(new CollisionEvent(new Entity(Scene, _physicsObjects.GetEntityId()),
                                    new Entity(Scene, _colliders.GetEntityId()), surfaceNormal));
                                Scene.Events.Invoke(new CollisionEvent(new Entity(Scene, _colliders.GetEntityId()),
                                    new Entity(Scene, _physicsObjects.GetEntityId()), -surfaceNormal));
                                // Console.WriteLine($"surfaceNormal = {surfaceNormal}");
                                // Console.WriteLine($"physics.Velocity (before) = {physics.Velocity}");
                                Vector2 expectedVelocity =
                                    ((Complex)physics.Velocity / (Complex)surfaceNormal).ScaleAxes(
                                        -restitution,
                                        1 - friction)
                                    * (Complex)surfaceNormal;
                                Vector2 velocityDelta = expectedVelocity - physics.Velocity;
                                Vector2 force = velocityDelta * physics.Mass;
                                physics.ApplyForce(force, Vector2.Zero, ForceMode.Continuous);
                                // Console.WriteLine($"physics.Velocity (after) = {physics.Velocity}");
                            }
                        }
                }
            }

            if (!physics.Static)
            {
                physics.AngularVelocity += physics.Torque * Time.FixedDeltaTime;
                physics.Velocity += physics.AccelerationFromForce * Time.FixedDeltaTime;

                transform.Rotation += physics.AngularVelocity * Time.FixedDeltaTime;
                transform.Position += physics.Velocity * Time.FixedDeltaTime;
                transform.Position += physics.ContactDisplacement;
            }

            physics.AccelerationFromForce = default;
            physics.Torque = default;
            physics.ContactDisplacement = default;
        }
    }
}