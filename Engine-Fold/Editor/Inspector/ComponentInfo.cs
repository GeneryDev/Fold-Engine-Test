﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Editor.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.ImmediateGui.Fields;
using FoldEngine.Editor.ImmediateGui.Fields.Text;
using FoldEngine.Editor.Views;
using FoldEngine.Gui;
using FoldEngine.ImmediateGui;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Inspector;

public class ComponentInfo
{
    private static readonly Dictionary<Type, ComponentInfo> _allInfos = new Dictionary<Type, ComponentInfo>();
    public readonly List<ComponentMember> Members = new List<ComponentMember>();

    public Type ComponentType;
    public bool HideInInspector;
    public string Name;

    private ComponentInfo(Type componentType)
    {
        ComponentType = componentType;

        if (componentType.GetCustomAttribute<NameAttribute>() is NameAttribute nameAttribute)
            Name = nameAttribute.Name;
        else
            Name = componentType.Name;

        HideInInspector = componentType.GetCustomAttribute<HideInInspector>() != null;

        if (!HideInInspector)
            foreach (FieldInfo fieldInfo in componentType.GetFields())
            {
                if (fieldInfo.IsStatic) continue;
                if (fieldInfo.GetCustomAttribute<HideInInspector>() != null) continue;
                Members.Add(new ComponentMember(fieldInfo));
            }

        foreach (ComponentMember member in Members) member.ComponentInfoComplete(this);
    }

    public ComponentMember this[string name]
    {
        get
        {
            foreach (ComponentMember member in Members)
                if (member.FieldInfo.Name == name)
                    return member;

            return null;
        }
    }

    public static ComponentInfo Get(Type type)
    {
        if (!_allInfos.ContainsKey(type)) return _allInfos[type] = new ComponentInfo(type);
        return _allInfos[type];
    }

    public static ComponentInfo Get<T>() where T : struct
    {
        return Get(typeof(T));
    }
}

public class ComponentMember
{
    public delegate bool ComponentPredicate(Scene scene, long id, object obj);

    public delegate void InspectorElementProvider(
        GuiPanel parentPanel,
        ComponentMember member,
        object startingValue);

    public static Dictionary<Type, InspectorElementProvider> TypeToInspectorElementProvider =
        new Dictionary<Type, InspectorElementProvider>();

    public static readonly InspectorElementProvider EnumInspectorElementProvider =
        (parentPanel, member, startingValue) =>
        {
            if (parentPanel.Element<ValueDropdown>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth, 1)
                .Text(startingValue.ToString())
                .FontSize(9)
                .IsPressed(out Point p))
                parentPanel.Scene.Systems.Get<EditorContextMenuSystem>().Show(p, m =>
                {
                    foreach (object value in member.FieldInfo.FieldType.GetEnumValues())
                        m.Button(value.ToString()).AddComponent<LegacyActionComponent>() = new LegacyActionComponent()
                        {
                            Action = member.CreateAction(parentPanel).ForcedValue(value),
                            Element = parentPanel
                        };
                }, buttonStyle: "editor:dropdown_item", textAlignment: Alignment.Center);
        };

    private readonly InspectorElementProvider _createInspectorElement;
    private long _createNextForId = -1;

    private object _createNextForObject;
    private ComponentPredicate _showCondition;
    public FieldInfo FieldInfo;

    public string Name;

    static ComponentMember()
    {
        // Checkbox
        // bool
        SetDefaultInspectorElementProvider<bool>((parentPanel, member, startingValue) =>
            parentPanel.Element<Checkbox>()
                .Value((bool)startingValue)
                .EditedAction(member.CreateAction(parentPanel)));

        // Text Field
        // strings and various numeric types
        InspectorElementProvider textFieldProvider = (parentPanel, member, startingValue) => parentPanel
                .Element<TextField>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth)
                .Value(startingValue?.ToString() ?? "")
                .EditedAction(member.CreateAction(parentPanel))
            ;
        SetDefaultInspectorElementProvider<string>(textFieldProvider);
        SetDefaultInspectorElementProvider<int>(textFieldProvider);
        SetDefaultInspectorElementProvider<long>(textFieldProvider);
        SetDefaultInspectorElementProvider<float>(textFieldProvider);
        SetDefaultInspectorElementProvider<double>(textFieldProvider);
        SetDefaultInspectorElementProvider<ResourceIdentifier>((parentPanel, member, startingValue) => parentPanel
            .Element<TextField>()
            .FieldSpacing(ComponentMemberLabel.LabelWidth)
            .Value(((ResourceIdentifier)startingValue).Identifier ?? "")
            .EditedAction(member.CreateAction(parentPanel))
        );


        // Multiple Text Fields
        // Vector2
        SetDefaultInspectorElementProvider<Vector2>((parentPanel, member, startingValue) =>
        {
            parentPanel.Element<TextField>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth, 2)
                .Value(((Vector2)startingValue).X.ToString(CultureInfo.InvariantCulture))
                .EditedAction(member.CreateAction(parentPanel, 0));

            parentPanel.Element<TextField>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth, 2)
                .Value(((Vector2)startingValue).Y.ToString(CultureInfo.InvariantCulture))
                .EditedAction(member.CreateAction(parentPanel, 1));
        });

        // Color
        SetDefaultInspectorElementProvider<Color>((parentPanel, member, startingValue) =>
        {
            parentPanel.Element<TextField>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                .Value(((Color)startingValue).R.ToString(CultureInfo.InvariantCulture))
                .EditedAction(member.CreateAction(parentPanel, 0));
            parentPanel.Element<TextField>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                .Value(((Color)startingValue).G.ToString(CultureInfo.InvariantCulture))
                .EditedAction(member.CreateAction(parentPanel, 1));
            parentPanel.Element<TextField>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                .Value(((Color)startingValue).B.ToString(CultureInfo.InvariantCulture))
                .EditedAction(member.CreateAction(parentPanel, 2));
            parentPanel.Element<TextField>()
                .FieldSpacing(ComponentMemberLabel.LabelWidth, 4)
                .Value(((Color)startingValue).A.ToString(CultureInfo.InvariantCulture))
                .EditedAction(member.CreateAction(parentPanel, 3));
        });
    }

    public ComponentMember(FieldInfo fieldInfo)
    {
        FieldInfo = fieldInfo;
        if (fieldInfo.GetCustomAttribute<NameAttribute>() is NameAttribute nameAttribute)
            Name = nameAttribute.Name;
        else
            Name = fieldInfo.Name;

        if (TypeToInspectorElementProvider.ContainsKey(fieldInfo.FieldType))
            _createInspectorElement = TypeToInspectorElementProvider[fieldInfo.FieldType];
        else if (fieldInfo.FieldType.IsEnum) _createInspectorElement = EnumInspectorElementProvider;
    }

    public void ComponentInfoComplete(ComponentInfo componentInfo)
    {
        foreach (ShowOnlyIf showOnlyIf in FieldInfo.GetCustomAttributes<ShowOnlyIf>())
        {
            ComponentMember conditionMember = componentInfo[showOnlyIf.FieldName];
            if (conditionMember != null)
                AddShowCondition((scene, id, obj) => Equals(scene.Components
                    .Sets[componentInfo.ComponentType]
                    .GetFieldValue(id, conditionMember.FieldInfo), showOnlyIf.Value));
            else
                Console.WriteLine(
                    $"Invalid ShowOnlyIf attribute for field {Name} of component {componentInfo.Name}: Field {showOnlyIf.FieldName} does not exist");
        }

        foreach (ShowOnlyIf.Not showOnlyIf in FieldInfo.GetCustomAttributes<ShowOnlyIf.Not>())
        {
            ComponentMember conditionMember = componentInfo[showOnlyIf.FieldName];
            if (conditionMember != null)
                AddShowCondition((scene, id, obj) => !Equals(scene.Components
                    .Sets[componentInfo.ComponentType]
                    .GetFieldValue(id, conditionMember.FieldInfo), showOnlyIf.Value));
            else
                Console.WriteLine(
                    $"Invalid ShowOnlyIf attribute for field {Name} of component {componentInfo.Name}: Field {showOnlyIf.FieldName} does not exist");
        }
    }

    private void AddShowCondition(ComponentPredicate condition)
    {
        if (_showCondition == null)
        {
            _showCondition = condition;
        }
        else
        {
            ComponentPredicate oldCondition = _showCondition;
            _showCondition = (scene, id, obj) => oldCondition(scene, id, obj) && condition(scene, id, obj);
        }
    }

    public bool ShouldShowInInspector(Scene scene, long id)
    {
        return _showCondition == null || _showCondition(scene, id, null);
    }

    public ComponentMember ForObject(object obj)
    {
        _createNextForObject = obj;
        _createNextForId = -1;
        return this;
    }

    public ComponentMember ForEntity(ComponentSet set, long id)
    {
        _createNextForObject = set;
        _createNextForId = id;
        return this;
    }

    public void CreateInspectorElement(GuiPanel parentPanel, object startingValue)
    {
        if (_createInspectorElement != null)
            _createInspectorElement(parentPanel, this, startingValue);
        else
            parentPanel.Label(startingValue?.ToString() ?? "", 9).TextAlignment(-1).UseTextCache(false);
    }

    public SetFieldAction CreateAction(GuiPanel panel, int index = 0)
    {
        SetFieldAction action;
        if (_createNextForId != -1)
            action = panel.Environment.ActionPool.Claim<SetComponentFieldAction>()
                .ComponentSet((ComponentSet)_createNextForObject)
                .Id(_createNextForId);
        else
            action = panel.Environment.ActionPool.Claim<SetObjectFieldAction>()
                .Object(_createNextForObject);

        action.FieldInfo(FieldInfo);
        action.Index(index);
        return action;
    }

    public static void SetDefaultInspectorElementProvider<T>(InspectorElementProvider provider)
    {
        TypeToInspectorElementProvider[typeof(T)] = provider;
    }
}