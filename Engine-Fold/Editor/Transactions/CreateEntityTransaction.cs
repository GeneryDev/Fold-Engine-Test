using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class CreateEntityTransaction : Transaction<Scene>
{
    private long _newEntityId = -1;

    private readonly long _parentEntityId;

    public CreateEntityTransaction(long parentEntityId)
    {
        _parentEntityId = parentEntityId;
    }


    public override bool Redo(Scene target)
    {
        if (_newEntityId == -1)
        {
            _newEntityId = target.CreateEntityId("Unnamed Entity");
        }
        else
        {
            if (!target.ReclaimAndCreate(_newEntityId, "Unnamed Entity"))
            {
                return true;
            }
        }

        if (_parentEntityId != -1 && target.Components.HasComponent<Transform>(_parentEntityId))
        {
            target.Components.GetComponent<Transform>(_newEntityId).SetParent(_parentEntityId);
        }

        return true;
    }

    public override bool Undo(Scene target)
    {
        if (_parentEntityId != -1 && target.Components.HasComponent<Transform>(_parentEntityId))
        {
            target.Components.GetComponent<Transform>(_parentEntityId).RemoveChild(_newEntityId);
        }

        if (target.Components.HasComponent<Transform>(_newEntityId))
        {
            target.DeleteEntity(_newEntityId, true);
        }
        else
        {
            SceneEditor.ReportEditorGameConflict();
        }

        return true;
    }
}