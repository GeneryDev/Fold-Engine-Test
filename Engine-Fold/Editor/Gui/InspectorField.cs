using System;
using System.Reflection;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Transactions;
using FoldEngine.Gui;

namespace FoldEngine.Editor.Gui;

public interface IInspectorField
{
    bool EditValueForType(Type type, ref object value, int index);
}

public abstract class SetFieldAction : IGuiAction
{
    protected FieldInfo _fieldInfo;

    protected object _forcedValue;
    protected int _index;
    protected bool _useForcedValue;

    public void Perform(GuiElement element, MouseEvent e)
    {
        object oldValue = GetOldValue();
        object newValue = oldValue;

        if (_useForcedValue) newValue = _forcedValue;

        if (element is IInspectorField inspectorField)
            if (!inspectorField.EditValueForType(_fieldInfo.FieldType, ref newValue, _index))
                return;

        SetFieldTransaction transaction = CreateBaseTransaction();
        transaction.FieldInfo = _fieldInfo;
        transaction.OldValue = oldValue;
        transaction.NewValue = newValue;

        ((EditorEnvironment)element.Parent.Environment).TransactionManager.InsertTransaction(transaction);
    }

    public IObjectPool Pool { get; set; }

    public SetFieldAction FieldInfo(FieldInfo fieldInfo)
    {
        _fieldInfo = fieldInfo;
        _index = 0;
        _forcedValue = null;
        _useForcedValue = false;
        return this;
    }

    public SetFieldAction Index(int index)
    {
        _index = index;
        return this;
    }

    public SetFieldAction ForcedValue(object forcedValue)
    {
        _forcedValue = forcedValue;
        _useForcedValue = true;
        return this;
    }

    protected abstract object GetOldValue();
    protected abstract SetFieldTransaction CreateBaseTransaction();
}

public class SetComponentFieldAction : SetFieldAction
{
    private long _id;
    private ComponentSet _set;

    public SetComponentFieldAction Id(long id)
    {
        _id = id;
        return this;
    }

    public SetComponentFieldAction ComponentSet(ComponentSet set)
    {
        _set = set;
        return this;
    }

    protected override object GetOldValue()
    {
        return _set.GetFieldValue(_id, _fieldInfo);
    }

    protected override SetFieldTransaction CreateBaseTransaction()
    {
        return new SetComponentFieldTransaction
        {
            ComponentType = _set.ComponentType,
            EntityId = _id
        };
    }
}

public class SetObjectFieldAction : SetFieldAction
{
    private object _obj;

    public SetObjectFieldAction Object(object obj)
    {
        _obj = obj;
        return this;
    }

    protected override object GetOldValue()
    {
        return _fieldInfo.GetValue(_obj);
    }

    protected override SetFieldTransaction CreateBaseTransaction()
    {
        return new SetObjectFieldTransaction
        {
            Parent = _obj
        };
    }
}