using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Components;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class AddComponentTransaction : Transaction<Scene>
{
    private readonly long _entityId;

    private readonly Type _type;

    public AddComponentTransaction(Type type, long entityId)
    {
        _type = type;
        _entityId = entityId;
    }

    public override bool Redo(Scene target)
    {
        if (target.Components.HasComponent<Hierarchical>(_entityId)
            && !target.Components.HasComponent(_type, _entityId))
            target.Components.CreateComponent(_type, _entityId);
        else
            SceneEditor.ReportEditorGameConflict();

        return true;
    }

    public override bool Undo(Scene target)
    {
        if (target.Components.HasComponent(_type, _entityId))
            target.Components.RemoveComponent(_type, _entityId);
        else
            SceneEditor.ReportEditorGameConflict();

        return true;
    }
}

public class RemoveComponentTransaction : Transaction<Scene>
{
    private readonly long _entityId;

    private byte[] _serializedData;

    private readonly Type _type;

    public RemoveComponentTransaction(Type type, long entityId)
    {
        _type = type;
        _entityId = entityId;
    }

    public override bool Redo(Scene target)
    {
        if (_serializedData == null)
        {
            var stream = new MemoryStream();

            var saveOp = new SaveOperation(stream);
            saveOp.Options.Set(SerializeOnlyEntities.Instance, new List<long> { _entityId });
            saveOp.Options.Set(SerializeOnlyComponents.Instance, new List<Type> { _type });

            target.Serialize(saveOp);

            saveOp.Close();
            _serializedData = stream.GetBuffer();
            saveOp.Dispose();
        }

        if (target.Components.HasComponent(_type, _entityId))
            target.Components.RemoveComponent(_type, _entityId);
        else
            SceneEditor.ReportEditorGameConflict();

        return true;
    }

    public override bool Undo(Scene target)
    {
        if (_serializedData == null) throw new InvalidOperationException("Cannot call Undo before Redo");

        if (target.Components.HasComponent<Hierarchical>(_entityId)
            && !target.Components.HasComponent(_type, _entityId))
        {
            var loadOp = new LoadOperation(new MemoryStream(_serializedData));

            target.Deserialize(loadOp);

            loadOp.Close();
            loadOp.Dispose();
        }
        else
        {
            SceneEditor.ReportEditorGameConflict();
        }

        return true;
    }
}