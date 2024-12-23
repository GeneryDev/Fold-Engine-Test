using System;
using System.Reflection;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public abstract class SetFieldTransaction : Transaction<Scene>
{
    public FieldInfo FieldInfo;
    public PropertyInfo PropertyInfo;
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
        target.Events.Invoke(new InspectorEditedComponentEvent()
        {
            EntityId = EntityId,
            ComponentType = ComponentType,
            FieldName = FieldInfo.Name
        });
        return OldValue != NewValue;
    }

    public override bool Undo(Scene target)
    {
        target.Components.Sets[ComponentType].SetFieldValue(EntityId, FieldInfo, OldValue);
        target.Events.Invoke(new InspectorEditedComponentEvent()
        {
            EntityId = EntityId,
            ComponentType = ComponentType,
            FieldName = FieldInfo.Name
        });
        return OldValue != NewValue;
    }
}

public class SetObjectFieldTransaction : SetFieldTransaction
{
    public object Parent;

    public override bool Redo(Scene target)
    {
        FieldInfo?.SetValue(Parent, NewValue);
        PropertyInfo?.SetValue(Parent, NewValue);
        return OldValue != NewValue;
    }

    public override bool Undo(Scene target)
    {
        FieldInfo?.SetValue(Parent, OldValue);
        PropertyInfo?.SetValue(Parent, NewValue);
        return OldValue != NewValue;
    }
}