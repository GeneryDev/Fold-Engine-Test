using System;
using EntryProject.Editor.Gui.Hierarchy;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class ChangeEntityHierarchyTransaction : Transaction<EditorEnvironment>
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

    public override bool Redo(EditorEnvironment target)
    {
        var entity = new Entity(target.Scene, _entityId);

        UnlinkFromHierarchy(entity);

        switch (_nextRelationship)
        {
            case HierarchyDropMode.Inside:
            {
                entity.Transform.SetParent(_nextEntity);
                break;
            }
        }

        entity.Transform.RestoreSnapshot(_snapshot);

        return true;
    }

    public override bool Undo(EditorEnvironment target)
    {
        var entity = new Entity(target.Scene, _entityId);

        UnlinkFromHierarchy(entity);

        entity.Transform.RestoreSnapshot(_snapshot);

        Console.WriteLine("undo");
        return true;
    }

    private static void UnlinkFromHierarchy(Entity entity)
    {
        entity.Transform.SetParent(-1);
    }
}