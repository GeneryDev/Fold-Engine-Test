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
    ///     ID of the parent entity (-1 for null and transforms without parent)
    /// </summary>
    [HideInInspector] [EntityId] public long ParentId;

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
    ///     Entity ID of this transform's first child (or -1 if it has no children)
    /// </summary>
    [HideInInspector] [EntityId] public long FirstChildId;

    /// <summary>
    ///     Entity ID of this transform's previous sibling (or -1 if it's the first child)
    /// </summary>
    [HideInInspector] [EntityId] public long PreviousSiblingId;

    /// <summary>
    ///     Entity ID of this transform's next sibling (or -1 if it's the last child)
    /// </summary>
    [HideInInspector] [EntityId] public long NextSiblingId;

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
        ParentId = -1;

        FirstChildId = -1;
        _rotationUsedInComplex = float.NaN;
        PreviousSiblingId = -1;
        NextSiblingId = -1;

        LocalPosition = Vector2.Zero;
        _rotationComplex = new Complex(1, 0);
        LocalScale = new Vector2(1, 1);

        LocalRotation = 0;
    }

    /// <summary>
    ///     Returns a read-only reference to this transform's parent, or RootTransform if this transform has no parent.
    /// </summary>
    public ref readonly Transform Parent
    {
        get
        {
            if (!IsNotNull) return ref _nullTransform;
            if (ParentId != -1 && Scene.Components.HasComponent<Transform>(ParentId))
                return ref Scene.Components.GetComponent<Transform>(ParentId);
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
            if (!IsNotNull) return ref _nullTransform;
            if (ParentId != -1 && Scene.Components.HasComponent<Transform>(ParentId))
                return ref Scene.Components.GetComponent<Transform>(ParentId);
            return ref _nullTransform;
        }
    }

    public bool HasParent => ParentId != -1;

    /// <summary>
    ///     Returns a read-only reference to this transform's next sibling, or RootTransform if this transform has no next
    ///     sibling.
    /// </summary>
    public ref readonly Transform NextSibling
    {
        get
        {
            if (!IsNotNull) return ref _nullTransform;
            if (NextSiblingId != -1)
                return ref Scene.Components.GetComponent<Transform>(NextSiblingId);
            return ref _nullTransform;
        }
    }

    /// <summary>
    ///     Returns a mutable reference to this transform's next sibling.
    ///     The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of
    ///     components.
    ///     Only use this property when you need to change a property on the parent transform. Otherwise, use NextSibling.
    ///     DO NOT assign a value to the returned reference.
    /// </summary>
    public ref Transform MutableNextSibling
    {
        get
        {
            if (!IsNotNull) return ref _nullTransform;
            if (NextSiblingId != -1)
                return ref Scene.Components.GetComponent<Transform>(NextSiblingId);
            return ref _nullTransform;
        }
    }

    /// <summary>
    ///     Returns a read-only reference to this transform's first child, or RootTransform if this transform has no first
    ///     child.
    /// </summary>
    public ref readonly Transform FirstChild
    {
        get
        {
            if (!IsNotNull) return ref _nullTransform;
            if (FirstChildId != -1)
                return ref Scene.Components.GetComponent<Transform>(FirstChildId);
            return ref _nullTransform;
        }
    }

    /// <summary>
    ///     Returns a mutable reference to this transform's first child.
    ///     The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of
    ///     components.
    ///     Only use this property when you need to change a property on the parent transform. Otherwise, use FirstChild.
    ///     DO NOT assign a value to the returned reference.
    /// </summary>
    public ref Transform MutableFirstChild
    {
        get
        {
            if (!IsNotNull) return ref _nullTransform;
            if (FirstChildId != -1)
                return ref Scene.Components.GetComponent<Transform>(FirstChildId);
            return ref _nullTransform;
        }
    }

    /// <summary>
    ///     Sets the given entity as this transform's parent, and adds this as a child of the given entity's transform.
    /// </summary>
    /// <param name="entity">The container entity that should become this transform's parent</param>
    public void SetParent(Entity entity)
    {
        SetParent(entity.EntityId);
    }

    /// <summary>
    ///     Sets the given entity as this transform's parent, and adds this as a child of the given entity's transform.
    /// </summary>
    /// <param name="entityId">The ID of the entity that should become this transform's parent</param>
    public void SetParent(long entityId)
    {
        if (!IsNotNull) throw new InvalidOperationException();
        if (ParentId == entityId) return;
        if (ParentId != -1) Scene.Components.GetComponent<Transform>(ParentId).RemoveChild(EntityId);
        ParentId = entityId;
        if (entityId != -1) AddChild(ref MutableParent, ref this);
    }

    /// <summary>
    ///     Adds a child to the given parent.
    /// </summary>
    /// <param name="parent">The transform to which the given transform child should be added.</param>
    /// <param name="child">The child to be added to the parent.</param>
    private static void AddChild(ref Transform parent, ref Transform child)
    {
        if (!parent.IsNotNull) throw new InvalidOperationException();
        if (parent.FirstChildId == -1)
        {
            parent.FirstChildId = child.EntityId;
            return;
        }

        ref Transform currentChild = ref parent.Scene.Components.GetComponent<Transform>(parent.FirstChildId);
        while (currentChild.NextSiblingId != -1)
            currentChild = ref parent.Scene.Components.GetComponent<Transform>(currentChild.NextSiblingId);

        currentChild.NextSiblingId = child.EntityId;
        child.PreviousSiblingId = currentChild.EntityId;
    }

    public int ChildCount
    {
        get
        {
            if (FirstChildId == -1 || !Scene.Components.HasComponent<Transform>(FirstChildId)) return 0;

            int count = 1;
            ref Transform currentChild = ref Scene.Components.GetComponent<Transform>(FirstChildId);
            while (currentChild.NextSiblingId != -1)
            {
                currentChild = ref Scene.Components.GetComponent<Transform>(currentChild.NextSiblingId);
                count++;
            }

            return count;
        }
    }

    public ComponentReference<Transform>[] Children
    {
        get
        {
            if (FirstChildId == -1 || !Scene.Components.HasComponent<Transform>(FirstChildId))
                return Array.Empty<ComponentReference<Transform>>();

            var children = new ComponentReference<Transform>[ChildCount];

            ref Transform currentChild = ref Scene.Components.GetComponent<Transform>(FirstChildId);
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = new ComponentReference<Transform>(Scene, currentChild.EntityId);
                if (i < children.Length - 1)
                    currentChild = ref Scene.Components.GetComponent<Transform>(currentChild.NextSiblingId);
            }

            return children;
        }
    }


    /// <summary>
    ///     This transform's absolute rotation, in radians.
    /// </summary>
    public float Rotation
    {
        get
        {
            if (!IsNotNull) return 0f;
            float total = LocalRotation;
            ref readonly Transform current = ref Parent;
            while (current.IsNotNull)
            {
                total += current.LocalRotation;
                current = ref current.Parent;
            }

            return total;
        }
        set
        {
            if (!IsNotNull) throw new InvalidOperationException();
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
            if (!IsNotNull) throw new InvalidOperationException();
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
        if (!IsNotNull) return point;
        ref readonly Transform current = ref this;

        while (current.IsNotNull)
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
        if (!IsNotNull) return point;
        ref readonly Transform current = ref this;

        return (Vector2)((Complex)(Parent.Relativize(point) - current.LocalPosition) / current.RotationComplex)
               / current.LocalScale;
    }

    public override string ToString()
    {
        return IsNotNull ? $"fold:transform_2d|{LocalPosition}" : "fold:transform_2d|NULL";
    }

    public List<long> DumpHierarchy(List<long> list)
    {
        Transform nextChild = FirstChild;
        while (nextChild.IsNotNull)
        {
            nextChild.DumpHierarchy(list);
            nextChild = nextChild.NextSibling;
        }

        list.Add(EntityId);

        return list;
    }

    public void RemoveChild(long childEntityId)
    {
        if (FirstChildId != -1)
        {
            if (Scene.Components.HasComponent<Transform>(FirstChildId))
            {
                ref Transform firstChild = ref Scene.Components.GetComponent<Transform>(FirstChildId);
                if (childEntityId == FirstChildId)
                {
                    FirstChildId = firstChild.NextSiblingId;
                    if (Scene.Components.HasComponent<Transform>(FirstChildId))
                        Scene.Components.GetComponent<Transform>(FirstChildId).PreviousSiblingId = -1;
                }
                else
                {
                    firstChild.RemoveSibling(childEntityId);
                }
            }
            else
            {
                FirstChildId = -1;
            }
        }
    }

    private void RemoveSibling(long siblingEntityId)
    {
        if (NextSiblingId != -1)
        {
            if (Scene.Components.HasComponent<Transform>(NextSiblingId))
            {
                ref Transform nextSibling = ref Scene.Components.GetComponent<Transform>(NextSiblingId);
                if (siblingEntityId == NextSiblingId)
                {
                    NextSiblingId = nextSibling.NextSiblingId;

                    nextSibling.PreviousSiblingId = -1;
                    nextSibling.ParentId = -1;
                    nextSibling.NextSiblingId = -1;

                    if (Scene.Components.HasComponent<Transform>(NextSiblingId))
                        Scene.Components.GetComponent<Transform>(NextSiblingId).PreviousSiblingId = EntityId;
                }
                else
                {
                    nextSibling.RemoveSibling(siblingEntityId);
                }
            }
            else
            {
                NextSiblingId = -1;
            }
        }
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