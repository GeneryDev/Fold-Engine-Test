using System;
using System.Reflection;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public abstract class SetFieldTransaction : Transaction<Scene>
{
    public FieldInfo FieldInfo;
    public object NewValue;

    public object OldValue;
}

public class SetComponentFieldTransaction : SetFieldTransaction
{
    public Type ComponentType;

    public long EntityId;

    public override bool Redo(Scene target)
    {
        target.Components.Sets[ComponentType].SetFieldValue(EntityId, FieldInfo, NewValue);
        return OldValue != NewValue;
    }

    public override bool Undo(Scene target)
    {
        target.Components.Sets[ComponentType].SetFieldValue(EntityId, FieldInfo, OldValue);
        return OldValue != NewValue;
    }
}

public class SetObjectFieldTransaction : SetFieldTransaction
{
    public object Parent;

    public override bool Redo(Scene target)
    {
        FieldInfo.SetValue(Parent, NewValue);
        return OldValue != NewValue;
    }

    public override bool Undo(Scene target)
    {
        FieldInfo.SetValue(Parent, OldValue);
        return OldValue != NewValue;
    }
}