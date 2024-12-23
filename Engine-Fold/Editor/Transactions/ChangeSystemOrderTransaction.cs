﻿using System;
using FoldEngine.Commands;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class ChangeSystemOrderTransaction : Transaction<Scene>
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

    public override bool Redo(Scene target)
    {
        if (target.Systems.Get(_sysType) != null)
        {
            target.Core.CommandQueue.Enqueue(new ChangeSystemOrderCommand(target, _sysType, _toIndex));
        }
        else
        {
            SceneEditor.ReportEditorGameConflict();
        }

        return true;
    }

    public override bool Undo(Scene target)
    {
        if (target.Systems.Get(_sysType) != null)
        {
            target.Core.CommandQueue.Enqueue(new ChangeSystemOrderCommand(target, _sysType, _fromIndex));
        }
        else
        {
            SceneEditor.ReportEditorGameConflict();
        }

        return true;
    }
}