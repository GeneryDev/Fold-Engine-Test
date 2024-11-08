using System;
using FoldEngine.Commands;
using FoldEngine.Editor.Gui;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class ChangeSystemOrderTransaction : Transaction<EditorEnvironment>
{
    private Type _sysType;
    private int _fromIndex;
    private int _toIndex;

    public ChangeSystemOrderTransaction(Type sysType, int fromIndex, int toIndex)
    {
        this._sysType = sysType;
        this._fromIndex = fromIndex;
        this._toIndex = toIndex;
    }

    public override bool Redo(EditorEnvironment target)
    {
        if (target.EditingScene.Systems.Get(_sysType) != null)
        {
            target.EditingScene.Core.CommandQueue.Enqueue(new ChangeSystemOrderCommand(target.EditingScene, _sysType, _toIndex));
        }
        else
        {
            SceneEditor.ReportEditorGameConflict();
        }

        return true;
    }

    public override bool Undo(EditorEnvironment target)
    {
        if (target.EditingScene.Systems.Get(_sysType) != null)
        {
            target.EditingScene.Core.CommandQueue.Enqueue(new ChangeSystemOrderCommand(target.EditingScene, _sysType, _fromIndex));
        }
        else
        {
            SceneEditor.ReportEditorGameConflict();
        }

        return true;
    }
}