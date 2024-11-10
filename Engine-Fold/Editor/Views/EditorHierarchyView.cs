using System;
using System.Collections.Generic;
using EntryProject.Editor.Gui.Hierarchy;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Gui.Hierarchy;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public class EditorHierarchyView : EditorView
{
    private static readonly List<long> EntitiesToDelete = new List<long>();

    private EditorScene _lastRenderedScene;
    private ComponentIterator<Transform> _transforms;

    public Hierarchy<long> Hierarchy;

    public EditorHierarchyView()
    {
        Icon = new ResourceIdentifier("editor/hierarchy");
    }

    public override string Name => "Hierarchy";

    public override void Initialize()
    {
    }

    public override void Render(IRenderingUnit renderer)
    {
        var editingScene = EditingScene;
        if (editingScene != _lastRenderedScene)
        {
            _transforms = editingScene?.Components.CreateIterator<Transform>(IterationFlags.IncludeInactive);
        }

        if (_transforms == null) return;
        
        if (Hierarchy == null) Hierarchy = new EntityHierarchy(ContentPanel);
        ContentPanel.MayScroll = true;

        // ContentPanel.Label("Entities", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
        // ContentPanel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
        if (ContentPanel.Button("New Entity", 14).IsPressed())
            ((EditorEnvironment)ContentPanel.Environment).TransactionManager.InsertTransaction(
                new CreateEntityTransaction(-1));
        ContentPanel.Separator();

        _transforms.Reset();
        while (_transforms.Next())
        {
            ref Transform current = ref _transforms.GetComponent();
            if (!current.Parent.IsNotNull) RenderEntity(ref current, ContentPanel, renderer);
        }
    }

    private void RenderEntity(ref Transform transform, GuiPanel panel, IRenderingUnit renderer, int depth = 0)
    {
        long entityId = transform.EntityId;

        bool hasChildren = transform.FirstChildId != -1;
        bool expanded = Hierarchy.IsExpanded(entityId);

        var entity = new Entity(EditingScene, entityId);

        HierarchyElement<long> button = panel.Element<HierarchyElement<long>>()
                .Hierarchy(Hierarchy)
                .Entity(entity, depth)
            ;

        if (hasChildren && expanded)
            foreach (ComponentReference<Transform> childTransform in transform.Children)
                if (childTransform.Has())
                    RenderEntity(ref childTransform.Get(), panel, renderer, depth + 1);

        bool selected = Hierarchy.Pressed
            ? Hierarchy.IsSelected(entityId)
            : Scene.Systems.Get<EditorBase>().EditingEntity.Contains(entity.EntityId);

        button
            .Icon(EditorResources.Get<Texture>(ref EditorIcons.Cube),
                entity.Active
                    ? (selected ? Color.White : new Color(128, 128, 128))
                    : (selected ? new Color(200, 200, 200) : new Color(80, 80, 80)))
            .Selected(selected)
            .TextColor(entity.Active ? Color.White : (selected ? new Color(200, 200, 200) : new Color(128, 128, 128)))
            ;

        switch (button.GetEvent(out Point p))
        {
            case HierarchyElement<long>.HierarchyEventType.Expand:
                (panel.Environment as EditorEnvironment)?.GetView<EditorHierarchyView>()
                    .Hierarchy.ExpandCollapse(entityId);
                break;
            case HierarchyElement<long>.HierarchyEventType.Down:
                SelectEntityDown(entityId);
                break;
            case HierarchyElement<long>.HierarchyEventType.Up:
                SelectEntityUp(entityId);
                break;
            case HierarchyElement<long>.HierarchyEventType.Context:
                ShowEntityContextMenu(entityId, p);
                break;
        }
    }

    private void SelectEntityDown(long id)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        bool wasSelected = editorBase.EditingEntity.Contains(id);

        bool control = Core.InputUnit.Devices.Keyboard.ControlDown;
        bool shift = Core.InputUnit.Devices.Keyboard.ShiftDown;

        Hierarchy.Selected.Clear();
        Hierarchy.Selected.AddRange(editorBase.EditingEntity);

        if (control)
        {
            if (wasSelected)
                Hierarchy.Selected.Remove(id);
            else
                Hierarchy.Selected.Add(id);
        }
        else
        {
            if (wasSelected)
            {
            }
            else
            {
                Hierarchy.Selected.Clear();
                Hierarchy.Selected.Add(id);
            }
        }
    }

    private void SelectEntityUp(
        long id)
    {
        var editorBase = ContentPanel.Environment.Scene.Systems.Get<EditorBase>();
        bool wasSelected = editorBase.EditingEntity.Contains(id);

        bool control = Core.InputUnit.Devices.Keyboard.ControlDown;
        bool shift = Core.InputUnit.Devices.Keyboard.ShiftDown;

        if (control)
        {
            if (wasSelected)
                editorBase.EditingEntity.Remove(id);
            else
                editorBase.EditingEntity.Add(id);
        }
        else
        {
            editorBase.EditingEntity.Clear();
            editorBase.EditingEntity.Add(id);
        }

        if (ContentPanel.Environment is EditorEnvironment editorEnvironment)
        {
            editorEnvironment.GetView<EditorInspectorView>().SetObject(null);
            editorEnvironment.SwitchToView<EditorInspectorView>();
        }
    }

    private void ShowEntityContextMenu(long id, Point point)
    {
        GuiPopupMenu contextMenu = ContentPanel.Environment.ContextMenu;
        var editorEnvironment = ((EditorEnvironment)ContentPanel.Environment);
        var editingScene = editorEnvironment.EditingScene;
        if (editingScene == null) return;

        contextMenu.Show(point, m =>
        {
            m.Button("Copy", 9).TextMargin(20).TextAlignment(-1);
            m.Button("Paste", 9).TextMargin(20).TextAlignment(-1);

            m.Separator();

            m.Button("Rename", 9).TextMargin(20).TextAlignment(-1);
            m.Button("Duplicate", 9).TextMargin(20).TextAlignment(-1);
            if (m.Button("Delete", 9).TextMargin(20).TextAlignment(-1).IsPressed())
            {
                EntitiesToDelete.Clear();
                if (editingScene.Components.HasComponent<Transform>(id))
                    editingScene.Components.GetComponent<Transform>(id)
                        .DumpHierarchy(EntitiesToDelete);

                var transactions = new CompoundTransaction<Scene>();
                foreach (long entityId in EntitiesToDelete)
                    transactions.Append(() => new DeleteEntityTransaction(entityId));

                editorEnvironment.TransactionManager.InsertTransaction(transactions);
            }

            m.Separator();

            if (m.Button("Create Child", 9).TextMargin(20).TextAlignment(-1).IsPressed())
                editorEnvironment.TransactionManager.InsertTransaction(
                    new CreateEntityTransaction(id));
        }, 120);
    }
}

public class EntityHierarchy : Hierarchy<long>
{
    public EntityHierarchy(GuiEnvironment environment) : base(environment)
    {
    }

    public EntityHierarchy(GuiPanel parent) : base(parent)
    {
    }

    public override long DefaultId { get; } = -1;

    public override void Drop()
    {
        if (DragTargetId == -1) return;
        var editingScene = ((EditorEnvironment)Environment).EditingScene;
        if (editingScene == null) return;
        Console.WriteLine("Dropping: ");

        HierarchyDropMode dropMode;
        switch (DragRelative)
        {
            case -1:
                dropMode = HierarchyDropMode.Before;
                break;
            case 0:
                dropMode = HierarchyDropMode.Inside;
                break;
            case 1:
                dropMode = IsExpanded(DragTargetId) ? HierarchyDropMode.FirstInside : HierarchyDropMode.After;
                break;
            default:
                throw new InvalidOperationException($"DragRelative can only be -1, 0 or 1, was {DragRelative}");
        }

        var transactions = new CompoundTransaction<Scene>();

        var dragTargetEntity = new Entity(editingScene, DragTargetId);

        foreach (long id in Selected)
        {
            var selectedEntity = new Entity(editingScene, id);
            if (dragTargetEntity == selectedEntity || selectedEntity.IsAncestorOf(dragTargetEntity))
            {
                Console.WriteLine("Cannot drag something into itself");
                return;
            }
        }

        foreach (long id in Selected)
        {
            var entity = new Entity(editingScene, id);
            var transaction = new ChangeEntityHierarchyTransaction(
                id,
                entity.Transform.ParentId,
                entity.Transform.NextSiblingId,
                DragTargetId,
                dropMode,
                entity.Transform.CreateSnapshot()
            );
            transactions.Append(() => transaction);
        }

        if (Environment is EditorEnvironment editorEnvironment)
            editorEnvironment.TransactionManager.InsertTransaction(transactions);
    }
}