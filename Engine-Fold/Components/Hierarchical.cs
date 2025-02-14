﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using FoldEngine.Editor.Inspector;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util;

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
    [DoNotSerialize] [HideInInspector] public readonly bool IsNotNull;
    public bool IsNull => !IsNotNull;

    /// <summary>
    ///     Reference to the scene this component belongs to.
    /// </summary>
    public Scene Scene { get; internal init; }

    /// <summary>
    ///     ID of the entity that owns this hierarchical component (-1 for null)
    /// </summary>
    [field: EntityId]
    public long EntityId { get; }

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

    [Name("Active")]
    private bool _active = true;

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            InvalidateActiveCache();
        }
    }
    
    [HideInInspector] [DoNotSerialize] public bool? CachedActiveFlag;
    [HideInInspector] [DoNotSerialize] public bool? _prevCachedActiveFlag;

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
        PreviousSiblingId = -1;
        ParentId = entityId;
        NextSiblingId = -1;
        if (entityId != -1) AddChild(ref MutableParent, ref this);
        else InvalidateActiveCache();
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
        parent.InvokeChangedEvent(EntityHierarchyChangedEvent.Type.ChildAdded);
        child.InvokeChangedEvent(EntityHierarchyChangedEvent.Type.ParentChanged);
        parent.InvalidateActiveCache();
        child.InvalidateActiveCache();
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

    public int IndexInParent
    {
        get
        {
            if (!HasParent) return -1;
            var index = 0;
            var sibling = Parent.FirstChild;
            while (sibling.IsNotNull)
            {
                if (sibling.EntityId == this.EntityId) return index;
                sibling = sibling.NextSibling;
                index++;
            }
            return -1;
        }
    }

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

                    firstChild.ParentId = -1;
                    InvokeChangedEvent(EntityHierarchyChangedEvent.Type.ChildRemoved);
                    firstChild.InvokeChangedEvent(EntityHierarchyChangedEvent.Type.ParentChanged);
                    firstChild.InvalidateActiveCache();
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
            InvalidateActiveCache();
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
                    
                    if(HasParent) MutableParent.InvokeChangedEvent(EntityHierarchyChangedEvent.Type.ChildRemoved);
                    nextSibling.InvokeChangedEvent(EntityHierarchyChangedEvent.Type.ParentChanged);
                    nextSibling.InvalidateActiveCache();

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

    public void InvalidateActiveCache()
    {
        if (IsNull) return;
        if (CachedActiveFlag == null) return;
        CachedActiveFlag = null;
        foreach (long childId in GetChildren(true))
        {
            ref var child = ref Scene.Components.GetComponent<Hierarchical>(childId);
            child.InvalidateActiveCache();
        }
    }

    public void InvokeChangedEvent(EntityHierarchyChangedEvent.Type type)
    {
        Scene?.Events?.Invoke(new EntityHierarchyChangedEvent()
        {
            EntityId = EntityId,
            ChangeType = type
        });
    }

    public bool IsActiveInHierarchy()
    {
        if (CachedActiveFlag != null) return CachedActiveFlag.Value;
        bool returnValue = Active && (!HasParent || MutableParent.IsActiveInHierarchy());
        CachedActiveFlag = returnValue;
        if (_prevCachedActiveFlag != null && _prevCachedActiveFlag != CachedActiveFlag)
        {
            InvokeChangedEvent(EntityHierarchyChangedEvent.Type.ActiveStateChanged);
        }
        _prevCachedActiveFlag = CachedActiveFlag;
        return returnValue;
    }

    public HierarchicalEnumerable GetChildren(bool includeInactive = false)
    {
        return new HierarchicalEnumerable(Scene, FirstChildId, includeInactive);
    }
}

public struct HierarchicalEnumerable
{
    private Scene _scene;
    private long _firstId;
    private long _childId;
    private long _nextId;
    private bool _includeInactive;

    public HierarchicalEnumerable(Scene scene, long firstId, bool includeInactive)
    {
        this._scene = scene;
        this._firstId = firstId;
        this._childId = -1;
        this._nextId = -1;
        this._includeInactive = includeInactive;
    }
    
    public bool MoveNext()
    {
        while (true)
        {
            if (_childId == -1 && _firstId != -1)
            {
                _childId = _firstId;
            }
            else if (_childId == -1)
            {
                return false;
            }
            else
            {
                _childId = _nextId;
                _nextId = -1;
            }

            if (_childId == -1) return false;
            ref var currentHierarchical = ref _scene.Components.GetComponent<Hierarchical>(_childId);
            _nextId = currentHierarchical.NextSiblingId;
            if (!_includeInactive && !currentHierarchical.IsActiveInHierarchy()) continue;
            return true;
        }
    }

    public void Reset()
    {
        _childId = _firstId;
        _nextId = -1;
    }

    public long ChildId => _childId;
    public ref Hierarchical ChildHierarchical => ref _scene.Components.GetComponent<Hierarchical>(_childId);
    public Entity Child => new Entity(_scene, _childId);
    public long Current => _childId;

    public void Dispose()
    {
    }

    public HierarchicalEnumerable GetEnumerator()
    {
        return this;
    }
}

public class HierarchicalSerializer : CustomComponentSerializer
{
    public override bool HandlesComponentType(Type type)
    {
        return type == typeof(Hierarchical);
    }

    public override bool Serialize(object component, SaveOperation writer)
    {
        var hierarchical = (Hierarchical)component;

        writer.WriteCompound((ref SaveOperation.Compound c) =>
        {
            c.WriteMember(nameof(Hierarchical.Active), hierarchical.Active);
            c.WriteMember(nameof(Hierarchical.ParentId), hierarchical.ParentId);
            c.WriteMember(nameof(Hierarchical.IndexInParent), hierarchical.IndexInParent);
        });
        return true;
    }

    public override void ScenePreDeserialize(Scene scene, LoadOperation reader)
    {
        reader.Options.Set(HierarchicalMemory.Instance, new HierarchicalMemory());
    }

    public override bool Deserialize(ComponentSet componentSet, long entityId, LoadOperation reader)
    {
        var hComponentSet = ((ComponentSet<Hierarchical>)componentSet);

        var boxedEntry = new StrongBox<HierarchicalMemory.Entry>(new HierarchicalMemory.Entry());
        boxedEntry.Value.EntityId = entityId;
        
        reader.ReadCompound(m =>
        {
            ref var component = ref hComponentSet.Get(entityId);
            switch (m.Name)
            {
                case nameof(Hierarchical.Active):
                {
                    component.Active = reader.ReadBoolean();
                    break;
                }
                case nameof(Hierarchical.ParentId):
                {
                    long parentId = reader.ReadInt64();
                    if (parentId != -1 && reader.Options.Has(DeserializeRemapIds.Instance))
                    {
                        parentId = reader.Options.Get(DeserializeRemapIds.Instance).TransformId(parentId);
                    }

                    boxedEntry.Value.ParentId = parentId;
                    break;
                }
                case nameof(Hierarchical.IndexInParent):
                {
                    int index = reader.ReadInt32();
                    boxedEntry.Value.Index = index;
                    break;
                }
                default:
                {
                    m.Skip();
                    break;
                }
            }
        });

        reader.Options.Get(HierarchicalMemory.Instance).Entries.Add(boxedEntry.Value);
        return true;
    }

    public override void ScenePostDeserialize(Scene scene, LoadOperation reader)
    {
        var memory = reader.Options.Get(HierarchicalMemory.Instance);
        
        memory.Entries.Sort((a, b) => a.Index - b.Index);

        foreach (var entry in memory.Entries)
        {
            ref var component = ref scene.Components.GetComponent<Hierarchical>(entry.EntityId);
            if(entry.ParentId != -1)
                component.SetParent(entry.ParentId);
        }
    }
}

public class HierarchicalMemory : Field<HierarchicalMemory>
{
    public static readonly HierarchicalMemory Instance = new();

    public List<Entry> Entries = new();

    public struct Entry
    {
        public long EntityId;
        public long ParentId;
        public int Index;
    }

}