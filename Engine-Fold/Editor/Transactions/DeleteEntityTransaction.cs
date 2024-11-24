using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class DeleteEntityTransaction : Transaction<Scene>
{
    private readonly long _entityId;
    private long _parentEntityId;
    private byte[] _serializedData;

    public DeleteEntityTransaction(long entityId)
    {
        _entityId = entityId;
    }

    public override bool Redo(Scene target)
    {
        if (_serializedData == null)
        {
            var stream = new MemoryStream();

            var saveOp = new SaveOperation(stream);
            saveOp.Options.Set(SerializeOnlyEntities.Instance, new List<long> { _entityId });

            target.Serialize(saveOp);

            saveOp.Close();
            _serializedData = stream.GetBuffer();
            saveOp.Dispose();
        }

        if (target.Components.HasComponent<Hierarchical>(_entityId))
        {
            ref Hierarchical hierarchical = ref target.Components.GetComponent<Hierarchical>(_entityId);
            _parentEntityId = hierarchical.ParentId;

            if (_parentEntityId != -1 && target.Components.HasComponent<Hierarchical>(_parentEntityId))
                target.Components.GetComponent<Hierarchical>(_parentEntityId).RemoveChild(_entityId);
            target.DeleteEntity(_entityId, true);
        }
        else
        {
            SceneEditor.ReportEditorGameConflict();
        }


        return true;
    }

    public override bool Undo(Scene target)
    {
        if (_serializedData == null) throw new InvalidOperationException("Cannot call Undo before Redo");

        if (target.Reclaim(_entityId))
        {
            var loadOp = new LoadOperation(new MemoryStream(_serializedData));

            target.Deserialize(loadOp);

            loadOp.Close();
            loadOp.Dispose();

            if (_parentEntityId != -1)
                target.Components.GetComponent<Hierarchical>(_entityId).SetParent(_parentEntityId);
        }

        return true;
    }
}