﻿using System;
using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui.Hierarchy;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class ChangeEntityHierarchyTransaction : Transaction<Scene>
{
    private readonly long _entityId = -1;

    private readonly long _nextEntity;
    private readonly HierarchyDropMode _nextRelationship;
    private long _previousNextSibling;

    private long _previousParent;

    private readonly Transform _snapshot;

    public ChangeEntityHierarchyTransaction(
        long entityId,
        long previousParent,
        long previousNextSibling,
        long nextEntity,
        HierarchyDropMode nextRelationship,
        Transform snapshot)
    {
        _entityId = entityId;
        _previousParent = previousParent;
        _previousNextSibling = previousNextSibling;
        _nextEntity = nextEntity;
        _nextRelationship = nextRelationship;
        _snapshot = snapshot;
    }

    public override bool Redo(Scene target)
    {
        var entity = new Entity(target, _entityId);

        UnlinkFromHierarchy(entity);

        switch (_nextRelationship)
        {
            case HierarchyDropMode.Inside:
            {
                entity.Hierarchical.SetParent(_nextEntity);
                break;
            }
        }

        entity.Transform.RestoreSnapshot(_snapshot);

        return true;
    }

    public override bool Undo(Scene target)
    {
        var entity = new Entity(target, _entityId);

        UnlinkFromHierarchy(entity);
        if (_previousParent != -1)
        {
            if (target.Components.HasComponent<Hierarchical>(_previousParent))
            {
                entity.Hierarchical.SetParent(_previousParent);
            }
            else
            {
                SceneEditor.ReportEditorGameConflict();
            }
        }

        entity.Transform.RestoreSnapshot(_snapshot);

        Console.WriteLine("undo");
        return true;
    }

    private static void UnlinkFromHierarchy(Entity entity)
    {
        entity.Hierarchical.SetParent(-1);
    }
}

public class SetEntityActiveTransaction : Transaction<Scene>
{
    private readonly long _entityId = -1;

    private bool _newActive;
    private bool _prevActive;

    public SetEntityActiveTransaction(
        long entityId,
        bool active,
        bool prevActive
        )
    {
        _entityId = entityId;
        _newActive = active;
        _prevActive = prevActive;
    }

    public override bool Redo(Scene target)
    {
        var entity = new Entity(target, _entityId);
        entity.Hierarchical.Active = _newActive;
        return true;
    }

    public override bool Undo(Scene target)
    {
        var entity = new Entity(target, _entityId);
        entity.Hierarchical.Active = _prevActive;
        return true;
    }
}