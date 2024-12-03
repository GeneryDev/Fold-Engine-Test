using System;
using System.Reflection;
using FoldEngine.Components;
using FoldEngine.Editor.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.ImmediateGui.Fields;
using FoldEngine.Editor.Inspector;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public class EditorInspectorView : EditorView
{
    private object _object;

    public EditorInspectorView()
    {
        Icon = new ResourceIdentifier("editor/info");
    }

    public override string Name => "Inspector";

    public override void Render(IRenderingUnit renderer)
    {
        ContentPanel.MayScroll = true;
        long id = -1;
        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;
        if (editingTab.Scene != null)
        {
            if (editingTab.EditingEntity.Count == 1) id = editingTab.EditingEntity[0];
        }

        if (id != -1 && (editingTab.Scene?.Components.HasComponent<Hierarchical>(id) ?? false))
            RenderEntityView(renderer, new Entity(editingTab.Scene, id));
        else if (_object != null) RenderObjectView(renderer);
        // ContentPanel.Label(Scene.Name, 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
    }

    private void RenderEntityView(IRenderingUnit renderer, Entity entity)
    {
        long id = entity.EntityId;
        ContentPanel.Label(entity.Name, 14)
            .TextAlignment(-1)
            .Icon(EditorResources.Get<Texture>(ref EditorIcons.Cube));
        if (entity.EntityId >= int.MaxValue)
        {
            ContentPanel.Label($"ID: {id} ({(int)id})", 7).TextAlignment(-1);
        }
        else
        {
            ContentPanel.Label($"ID: {id}", 7).TextAlignment(-1);
        }

        bool active = entity.Hierarchical.Active;
        if (active != ContentPanel.Element<Checkbox>().Value(active).IsChecked())
        {
            Scene.Systems.Get<EditorBase>().CurrentSceneTab.SceneTransactions.InsertTransaction(
                new SetEntityActiveTransaction(id, !active, active));
        }

        ContentPanel.Label("Active", 9).TextAlignment(-1);
        ContentPanel.Element<ComponentMemberBreak>();
        // ContentPanel.Spacing(12);

        foreach (ComponentSet set in entity.Scene.Components.Sets.Values)
            if (set.Has(id))
            {
                ComponentInfo componentInfo = ComponentInfo.Get(set.ComponentType);
                if (componentInfo.HideInInspector) continue;

                ContentPanel.Spacing(5);

                ContentPanel.Element<ComponentHeader>()
                    .Info(componentInfo)
                    .Id(id);

                // ContentPanel.Label(componentInfo.Name, 14).TextAlignment(-1);

                Core.RegistryUnit.CustomInspectors.RenderCustomInspectorsBefore(set.GetBoxedComponent(id),
                    ContentPanel);

                foreach (ComponentMember member in componentInfo.Members)
                {
                    if (!member.ShouldShowInInspector(entity.Scene, id)) continue;
                    object value = set.GetFieldValue(id, member.FieldInfo);
                    // ContentPanel
                    //     .Label(
                    //         StringBuilder
                    //             .Clear()
                    //             .Append(member.Name)
                    //             .Append(StringUtil.Repeat(" ", Math.Max(0, 32 - member.Name.Length)))
                    //             .Append(value)
                    //             .ToString(),
                    //         9)
                    //     .TextAlignment(-1)
                    //     .UseTextCache(false);

                    ContentPanel.Element<ComponentMemberLabel>().Member(member);

                    member.ForEntity(set, id).CreateInspectorElement(ContentPanel, value);

                    ContentPanel.Element<ComponentMemberBreak>();
                    // ContentPanel.Spacing(5);
                }

                Core.RegistryUnit.CustomInspectors.RenderCustomInspectorsAfter(set.GetBoxedComponent(id),
                    ContentPanel);
            }

        if (ContentPanel.Button("Add Component", 14).IsPressed(out Point p))
        {
            GuiPopupMenu contextMenu = ContentPanel.Environment.ContextMenu;
            contextMenu.Show(p, m =>
            {
                foreach (ComponentDefinition def in Core.RegistryUnit.Components.GetAllDefinitions())
                {
                    if (def.Type.GetCustomAttribute<HideInInspector>() == null &&
                        !entity.Scene.Components.HasComponent(def.Type, id))
                    {
                        m.Button(def.Type.Name, "editor/add").AddComponent<TransactionActionComponent>() =
                            new TransactionActionComponent(new AddComponentTransaction(def.Type, id));
                    }
                }
            });
        }
    }

    private void RenderObjectView(IRenderingUnit renderer)
    {
        ComponentInfo info = ComponentInfo.Get(_object.GetType());
        if (_object is Resource resource) resource.Access();

        ContentPanel.Label(info.Name, 14)
            .TextAlignment(-1)
            .Icon(EditorResources.Get<Texture>(ref EditorIcons.Cog));
        ContentPanel.Spacing(12);

        Core.RegistryUnit.CustomInspectors.RenderCustomInspectorsBefore(_object, ContentPanel);

        foreach (ComponentMember member in info.Members)
        {
            object value = member.FieldInfo.GetValue(_object);

            ContentPanel.Element<ComponentMemberLabel>().Member(member);

            member.ForObject(_object).CreateInspectorElement(ContentPanel, value);

            ContentPanel.Element<ComponentMemberBreak>();
        }

        Core.RegistryUnit.CustomInspectors.RenderCustomInspectorsAfter(_object, ContentPanel);
    }

    public void SetObject(object obj)
    {
        _object = obj;
    }

    public object GetObject()
    {
        return _object;
    }
}

public class ComponentMemberLabel : GuiLabel
{
    public const int LabelWidth = 140;

    private ComponentMember _member;

    public override void Reset(GuiPanel parent)
    {
        base.Reset(parent);
        FontSize(9);
        TextAlignment(-1);

        UseTextCache(true);
    }

    public ComponentMemberLabel Member(ComponentMember member)
    {
        _member = member;
        Text(member.Name);
        return this;
    }

    public override void Displace(ref Point layoutPosition)
    {
        layoutPosition.X += LabelWidth;
    }
}

public class ComponentMemberBreak : GuiElement
{
    public override void Reset(GuiPanel parent)
    {
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
    }

    public override void Displace(ref Point layoutPosition)
    {
        layoutPosition.X = Parent.Bounds.X;
        layoutPosition.Y += 20;
    }
}

public class ComponentHeader : GuiLabel
{
    private long _id;
    private ComponentInfo _info;

    public ComponentHeader Info(ComponentInfo info)
    {
        _info = info;
        Text(info.Name);
        FontSize(14);
        TextAlignment(-1);
        return this;
    }

    public ComponentHeader Id(long id)
    {
        _id = id;
        return this;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        base.AdjustSpacing(parent);
        Margin = 8;
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        if (e.Button == MouseEvent.RightButton)
            Environment.ContextMenu.Show(e.Position, m =>
            {
                m.Button("Remove").AddComponent<TransactionActionComponent>() =
                    new TransactionActionComponent(new RemoveComponentTransaction(_info.ComponentType, _id));
            });
        base.OnMouseReleased(ref e);
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = new Color(63, 63, 70),
            DestinationRectangle = Bounds.Translate(offset)
        });
        // _textColor = Rollover ? Color.CornflowerBlue : Color.White;
        base.Render(renderer, layer, offset);
    }
}