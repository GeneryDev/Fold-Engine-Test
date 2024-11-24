using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using FoldEngine.Editor.Inspector;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Components;

[Component("fold:transform_2d")]
[ComponentInitializer(typeof(Transform), nameof(InitializeComponent))]
public struct Transform
{
    /// <summary>
    ///     Static unique null transform. Has no scene, or children
    /// </summary>
    private static Transform _nullTransform;

    /// <summary>
    ///     Only false for null transforms
    ///     Double negative was chosen such that default transforms have this as false by default, and thus can't be considered
    ///     not-null
    /// </summary>
    [HideInInspector] public readonly bool IsNotNull;

    public bool IsNull => !IsNotNull;

    /// <summary>
    ///     Reference to the scene this component belongs to.
    /// </summary>
    public Scene Scene { get; internal set; }

    /// <summary>
    ///     Position in 2D space where this entity is located, relative to its parent
    /// </summary>
    [Name("Position")] public Vector2 LocalPosition;

    /// <summary>
    ///     Complex number used to quickly multiply coordinates for rotations.
    /// </summary>
    private Complex _rotationComplex;

    private readonly float _rotationUsedInComplex;

    /// <summary>
    ///     Complex number used to quickly multiply coordinates for rotations.
    /// </summary>
    public Complex RotationComplex
    {
        get
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_rotationUsedInComplex != LocalRotation) _rotationComplex = Complex.FromRotation(LocalRotation);

            return _rotationComplex;
        }
    }

    /// <summary>
    ///     This transform's local rotation, in radians.
    /// </summary>
    [Name("Rotation")] public float LocalRotation;

    /// <summary>
    ///     2D vector for the scale of this entity
    /// </summary>
    [Name("Scale")] public Vector2 LocalScale;

    /// <summary>
    ///     Returns an initialized transform component with all its correct default values.
    /// </summary>
    /// <param name="scene">The scene this transform is being created in</param>
    /// <param name="entityId">The ID of the entity this transform is being created for</param>
    /// <returns>An initialized transform component with all its correct default values.</returns>
    public static Transform InitializeComponent(Scene scene, long entityId)
    {
        return new Transform(entityId) { Scene = scene };
    }

    private Transform(long entityId)
    {
        IsNotNull = true;
        Scene = null;
        EntityId = entityId;

        _rotationUsedInComplex = float.NaN;

        LocalPosition = Vector2.Zero;
        _rotationComplex = new Complex(1, 0);
        LocalScale = new Vector2(1, 1);

        LocalRotation = 0;
    }

    public long ParentId
    {
        get
        {
            if (Scene != null && EntityId != -1 && Scene.Components.HasComponent<Hierarchical>(EntityId) &&
                Scene.Components.GetComponent<Hierarchical>(EntityId).ParentId is var parentId)
                return parentId;
            return -1;
        }
    }

    /// <summary>
    ///     Returns a read-only reference to this transform's parent, or RootTransform if this transform has no parent.
    /// </summary>
    public ref readonly Transform Parent
    {
        get
        {
            long parentId = ParentId;
            if (parentId != -1)
                return ref Scene.Components.GetComponent<Transform>(parentId);
            return ref _nullTransform;
        }
    }

    /// <summary>
    ///     Returns a mutable reference to this transform's parent.
    ///     The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of
    ///     components.
    ///     Only use this property when you need to change a property on the parent transform. Otherwise, use Parent.
    ///     DO NOT assign a value to the returned reference.
    /// </summary>
    public ref Transform MutableParent
    {
        get
        {
            long parentId = ParentId;
            if (parentId != -1)
                return ref Scene.Components.GetComponent<Transform>(parentId);
            return ref _nullTransform;
        }
    }

    public bool HasParent => ParentId != -1;

    /// <summary>
    ///     This transform's absolute rotation, in radians.
    /// </summary>
    public float Rotation
    {
        get
        {
            if (IsNull) return 0f;
            float total = LocalRotation;
            ref readonly Transform current = ref Parent;
            while (!current.IsNull)
            {
                total += current.LocalRotation;
                current = ref current.Parent;
            }

            return total;
        }
        set
        {
            if (IsNull) throw new InvalidOperationException();
            LocalRotation = value - Parent.Rotation;
        }
    }

    /// <summary>
    ///     This transform's absolute position in 2D space
    /// </summary>
    public Vector2 Position
    {
        get => Apply(Vector2.Zero);
        set
        {
            if (IsNull) throw new InvalidOperationException();
            LocalPosition = value - Parent.Apply(Vector2.Zero);
        }
    }

    /// <summary>
    ///     ID of the entity that owns this transform (-1 for null)
    /// </summary>
    [field: EntityId]
    public long EntityId { get; }

    /// <summary>
    ///     Applies this transformation to the given point in 2D space.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    [Pure]
    public Vector2 Apply(Vector2 point)
    {
        if (IsNull) return point;
        ref readonly Transform current = ref this;

        while (!current.IsNull)
        {
            point = (Vector2)((Complex)(point * current.LocalScale) * current.RotationComplex)
                    + current.LocalPosition;
            current = ref current.Parent;
        }

        return point;
    }

    /// <summary>
    ///     Undoes this transformation from the given point in 2D space.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    [Pure]
    public Vector2 Relativize(Vector2 point)
    {
        if (IsNull) return point;
        ref readonly Transform current = ref this;

        return (Vector2)((Complex)(Parent.Relativize(point) - current.LocalPosition) / current.RotationComplex)
               / current.LocalScale;
    }

    public override string ToString()
    {
        return !IsNull ? $"fold:transform_2d|{LocalPosition}" : "fold:transform_2d|NULL";
    }

    public Transform CreateSnapshot()
    {
        Transform snapshot = InitializeComponent(Scene, EntityId);
        snapshot.LocalPosition = Position;
        snapshot.LocalRotation = Rotation;
        snapshot.LocalScale = LocalScale;
        return snapshot;
    }

    public void RestoreSnapshot(Transform snapshot)
    {
        Position = snapshot.LocalPosition;
        Rotation = snapshot.LocalRotation;
        LocalScale = snapshot.LocalScale;
    }
}