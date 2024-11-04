using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class CreateEntityTransaction : Transaction<EditorEnvironment>
{
    private long _newEntityId = -1;

    private readonly long _parentEntityId;

    public CreateEntityTransaction(long parentEntityId)
    {
        _parentEntityId = parentEntityId;
    }


    public override bool Redo(EditorEnvironment target)
    {
        if (_newEntityId == -1)
        {
            _newEntityId = target.Scene.CreateEntityId("Unnamed Entity");
        }
        else
        {
            if (!target.Scene.ReclaimAndCreate(_newEntityId, "Unnamed Entity"))
            {
                return true;
            }
        }

        if (_parentEntityId != -1 && target.Scene.Components.HasComponent<Transform>(_parentEntityId))
        {
            target.Scene.Components.GetComponent<Transform>(_newEntityId).SetParent(_parentEntityId);
        }

        return true;
    }

    public override bool Undo(EditorEnvironment target)
    {
        if (_parentEntityId != -1 && target.Scene.Components.HasComponent<Transform>(_parentEntityId))
        {
            target.Scene.Components.GetComponent<Transform>(_parentEntityId).RemoveChild(_newEntityId);
        }

        if (target.Scene.Components.HasComponent<Transform>(_newEntityId))
        {
            target.Scene.DeleteEntity(_newEntityId, true);
        }
        else
        {
            SceneEditor.ReportEditorGameConflict();
        }

        return true;
    }
}