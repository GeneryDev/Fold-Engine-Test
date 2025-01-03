using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics;

[Component("fold:physics")]
[ComponentInitializer(typeof(Physics))]
public struct Physics
{
    public bool Static;

    public float Mass = 1;

    [ShowOnlyIf.Not(nameof(Static), true)] public float GravityMultiplier = 1;

    [ShowOnlyIf.Not(nameof(Static), true)] public Vector2 Velocity;

    [HideInInspector] public float AngularVelocity;

    [HideInInspector] public Vector2 PreviousPosition;

    [HideInInspector] public Vector2 PreviousVelocity;

    [HideInInspector] public Vector2 AccelerationFromForce;

    [HideInInspector] public Vector2 PreviousAcceleration;

    [HideInInspector] public float Torque;

    [HideInInspector] public Vector2 ContactDisplacement;

    public float Restitution = 0.0f;
    public float Friction = 0.03f;

    public Vector2 LinearMomentum => Static ? Vector2.Zero : Mass * Velocity;

    public Physics()
    {
    }

    public void ApplyForce(Vector2 force, Vector2 point, ForceMode mode, Color? gizmoColor = null)
    {
        if (force == default) return;
        if (Static) return;

        if (mode == ForceMode.Instant) force /= Time.FixedDeltaTime;

        Complex diff = ((Complex)force.Normalized() / (Complex)point.Normalized()).Normalized;
        if (point == Vector2.Zero) diff = force.Normalized();

        Vector2 accel = force / Mass;
        float torque = point.Length() * force.Length() / 1000 * diff.B;

        AccelerationFromForce += accel;
        Torque += torque;
    }
}

public enum ForceMode
{
    Continuous,
    Instant
}