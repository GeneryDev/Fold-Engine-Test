using System;
using System.Collections.Generic;
using FoldEngine.Editor.Inspector;
using FoldEngine.Scenes;

namespace FoldEngine.Components;

[Component("fold:hierarchical")]
[ComponentInitializer(typeof(Hierarchical), nameof(InitializeComponent))]
public struct Hierarchical
{
    /// <summary>
    ///     Static unique null hierarchical. Has no scene, or children
    /// </summary>
    private static Hierarchical _nullHierarchical;

    /// <summary>
    ///     Only false for null hierarchical
    ///     Double negative was chosen such that default hierarchicals have this as false by default, and thus can't be considered
    ///     not-null
    /// </summary>
    [HideInInspector] public readonly bool IsNotNull;

    public bool IsNull => !IsNotNull;

    /// <summary>
    ///     Reference to the scene this component belongs to.
    /// </summary>
    public Scene Scene { get; internal set; }

    /// <summary>
    ///     ID of the parent entity (-1 for null and hierarchicals without parent)
    /// </summary>
    [HideInInspector] [EntityId] public long ParentId;

    /// <summary>
    ///     Entity ID of this hierarchical's first child (or -1 if it has no children)
    /// </summary>
    [HideInInspector] [EntityId] public long FirstChildId;

    /// <summary>
    ///     Entity ID of this hierarchical's previous sibling (or -1 if it's the first child)
    /// </summary>
    [HideInInspector] [EntityId] public long PreviousSiblingId;

    /// <summary>
    ///     Entity ID of this hierarchical's next sibling (or -1 if it's the last child)
    /// </summary>
    [HideInInspector] [EntityId] public long NextSiblingId;

    /// <summary>
    ///     Returns an initialized hierarchical component with all its correct default values.
    /// </summary>
    /// <param name="scene">The scene this hierarchical is being created in</param>
    /// <param name="entityId">The ID of the entity this hierarchical is being created for</param>
    /// <returns>An initialized hierarchical component with all its correct default values.</returns>
    public static Hierarchical InitializeComponent(Scene scene, long entityId)
    {
        return new Hierarchical(entityId) { Scene = scene };
    }

    private Hierarchical(long entityId)
    {
        IsNotNull = true;

        Scene = null;
        EntityId = entityId;
        ParentId = -1;

        FirstChildId = -1;
        PreviousSiblingId = -1;
        NextSiblingId = -1;
    }

    /// <summary>
    ///     Returns a read-only reference to this hierarchical's parent, or a null hierarchical if this hierarchical has no parent.
    /// </summary>
    public ref readonly Hierarchical Parent
    {
        get
        {
            if (!IsNotNull) return ref _nullHierarchical;
            if (ParentId != -1 && Scene.Components.HasComponent<Hierarchical>(ParentId))
                return ref Scene.Components.GetComponent<Hierarchical>(ParentId);
            return ref _nullHierarchical;
        }
    }

    /// <summary>
    ///     Returns a mutable reference to this hierarchical's parent.
    ///     The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of
    ///     components.
    ///     Only use this property when you need to change a property on the parent hierarchical. Otherwise, use Parent.
    ///     DO NOT assign a value to the returned reference.
    /// </summary>
    public ref Hierarchical MutableParent
    {
        get
        {
            if (!IsNotNull) return ref _nullHierarchical;
            if (ParentId != -1 && Scene.Components.HasComponent<Hierarchical>(ParentId))
                return ref Scene.Components.GetComponent<Hierarchical>(ParentId);
            return ref _nullHierarchical;
        }
    }

    public bool HasParent => ParentId != -1;

    /// <summary>
    ///     Returns a read-only reference to this hierarchical's next sibling, or a null hierarchical if this hierarchical has no next
    ///     sibling.
    /// </summary>
    public ref readonly Hierarchical NextSibling
    {
        get
        {
            if (!IsNotNull) return ref _nullHierarchical;
            if (NextSiblingId != -1)
                return ref Scene.Components.GetComponent<Hierarchical>(NextSiblingId);
            return ref _nullHierarchical;
        }
    }

    /// <summary>
    ///     Returns a mutable reference to this hierarchical's next sibling.
    ///     The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of
    ///     components.
    ///     Only use this property when you need to change a property on the parent hierarchical. Otherwise, use NextSibling.
    ///     DO NOT assign a value to the returned reference.
    /// </summary>
    public ref Hierarchical MutableNextSibling
    {
        get
        {
            if (!IsNotNull) return ref _nullHierarchical;
            if (NextSiblingId != -1)
                return ref Scene.Components.GetComponent<Hierarchical>(NextSiblingId);
            return ref _nullHierarchical;
        }
    }

    /// <summary>
    ///     Returns a read-only reference to this hierarchical's first child, or a null hierarchical if this hierarchical has no first
    ///     child.
    /// </summary>
    public ref readonly Hierarchical FirstChild
    {
        get
        {
            if (!IsNotNull) return ref _nullHierarchical;
            if (FirstChildId != -1)
                return ref Scene.Components.GetComponent<Hierarchical>(FirstChildId);
            return ref _nullHierarchical;
        }
    }

    /// <summary>
    ///     Returns a mutable reference to this hierarchical's first child.
    ///     The reason the Parent property doesn't return a mutable reference is to avoid accidental reassignment of
    ///     components.
    ///     Only use this property when you need to change a property on the parent hierarchical. Otherwise, use FirstChild.
    ///     DO NOT assign a value to the returned reference.
    /// </summary>
    public ref Hierarchical MutableFirstChild
    {
        get
        {
            if (!IsNotNull) return ref _nullHierarchical;
            if (FirstChildId != -1)
                return ref Scene.Components.GetComponent<Hierarchical>(FirstChildId);
            return ref _nullHierarchical;
        }
    }

    /// <summary>
    ///     Sets the given entity as this hierarchical's parent, and adds this as a child of the given entity's hierarchical.
    /// </summary>
    /// <param name="entity">The container entity that should become this hierarchical's parent</param>
    public void SetParent(Entity entity)
    {
        SetParent(entity.EntityId);
    }

    /// <summary>
    ///     Sets the given entity as this hierarchical's parent, and adds this as a child of the given entity's hierarchical.
    /// </summary>
    /// <param name="entityId">The ID of the entity that should become this hierarchical's parent</param>
    public void SetParent(long entityId)
    {
        if (!IsNotNull) throw new InvalidOperationException();
        if (ParentId == entityId) return;
        if (ParentId != -1) Scene.Components.GetComponent<Hierarchical>(ParentId).RemoveChild(EntityId);
        ParentId = entityId;
        if (entityId != -1) AddChild(ref MutableParent, ref this);
    }

    /// <summary>
    ///     Adds a child to the given parent.
    /// </summary>
    /// <param name="parent">The hierarchical to which the given hierarchical child should be added.</param>
    /// <param name="child">The child to be added to the parent.</param>
    private static void AddChild(ref Hierarchical parent, ref Hierarchical child)
    {
        if (!parent.IsNotNull) throw new InvalidOperationException();
        if (parent.FirstChildId == -1)
        {
            parent.FirstChildId = child.EntityId;
            return;
        }

        ref Hierarchical currentChild = ref parent.Scene.Components.GetComponent<Hierarchical>(parent.FirstChildId);
        while (currentChild.NextSiblingId != -1)
            currentChild = ref parent.Scene.Components.GetComponent<Hierarchical>(currentChild.NextSiblingId);

        currentChild.NextSiblingId = child.EntityId;
        child.PreviousSiblingId = currentChild.EntityId;
    }

    public int ChildCount
    {
        get
        {
            if (FirstChildId == -1 || !Scene.Components.HasComponent<Hierarchical>(FirstChildId)) return 0;

            int count = 1;
            ref Hierarchical currentChild = ref Scene.Components.GetComponent<Hierarchical>(FirstChildId);
            while (currentChild.NextSiblingId != -1)
            {
                currentChild = ref Scene.Components.GetComponent<Hierarchical>(currentChild.NextSiblingId);
                count++;
            }

            return count;
        }
    }

    public ComponentReference<Hierarchical>[] Children
    {
        get
        {
            if (FirstChildId == -1 || !Scene.Components.HasComponent<Hierarchical>(FirstChildId))
                return Array.Empty<ComponentReference<Hierarchical>>();

            var children = new ComponentReference<Hierarchical>[ChildCount];

            ref Hierarchical currentChild = ref Scene.Components.GetComponent<Hierarchical>(FirstChildId);
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = new ComponentReference<Hierarchical>(Scene, currentChild.EntityId);
                if (i < children.Length - 1)
                    currentChild = ref Scene.Components.GetComponent<Hierarchical>(currentChild.NextSiblingId);
            }

            return children;
        }
    }


    /// <summary>
    ///     ID of the entity that owns this hierarchical component (-1 for null)
    /// </summary>
    [field: EntityId]
    public long EntityId { get; }


    public List<long> DumpHierarchy(List<long> list)
    {
        Hierarchical nextChild = FirstChild;
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
            if (Scene.Components.HasComponent<Hierarchical>(FirstChildId))
            {
                ref Hierarchical firstChild = ref Scene.Components.GetComponent<Hierarchical>(FirstChildId);
                if (childEntityId == FirstChildId)
                {
                    FirstChildId = firstChild.NextSiblingId;
                    if (Scene.Components.HasComponent<Hierarchical>(FirstChildId))
                        Scene.Components.GetComponent<Hierarchical>(FirstChildId).PreviousSiblingId = -1;
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
            if (Scene.Components.HasComponent<Hierarchical>(NextSiblingId))
            {
                ref Hierarchical nextSibling = ref Scene.Components.GetComponent<Hierarchical>(NextSiblingId);
                if (siblingEntityId == NextSiblingId)
                {
                    NextSiblingId = nextSibling.NextSiblingId;

                    nextSibling.PreviousSiblingId = -1;
                    nextSibling.ParentId = -1;
                    nextSibling.NextSiblingId = -1;

                    if (Scene.Components.HasComponent<Hierarchical>(NextSiblingId))
                        Scene.Components.GetComponent<Hierarchical>(NextSiblingId).PreviousSiblingId = EntityId;
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
}