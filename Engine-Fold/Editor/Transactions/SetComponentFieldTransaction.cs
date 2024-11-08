﻿using System;
using System.Reflection;
using FoldEngine.Editor.Gui;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public abstract class SetFieldTransaction : Transaction<EditorEnvironment>
{
    public FieldInfo FieldInfo;
    public object NewValue;

    public object OldValue;
}

public class SetComponentFieldTransaction : SetFieldTransaction
{
    public Type ComponentType;

    public long EntityId;

    public override bool Redo(EditorEnvironment target)
    {
        target.EditingScene.Components.Sets[ComponentType].SetFieldValue(EntityId, FieldInfo, NewValue);
        return OldValue != NewValue;
    }

    public override bool Undo(EditorEnvironment target)
    {
        target.EditingScene.Components.Sets[ComponentType].SetFieldValue(EntityId, FieldInfo, OldValue);
        return OldValue != NewValue;
    }
}

public class SetObjectFieldTransaction : SetFieldTransaction
{
    public object Parent;

    public override bool Redo(EditorEnvironment target)
    {
        FieldInfo.SetValue(Parent, NewValue);
        return OldValue != NewValue;
    }

    public override bool Undo(EditorEnvironment target)
    {
        FieldInfo.SetValue(Parent, OldValue);
        return OldValue != NewValue;
    }
}