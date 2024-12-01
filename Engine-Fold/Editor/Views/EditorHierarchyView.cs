﻿using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.ImmediateGui.Hierarchy;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public class EditorHierarchyView : EditorView
{
    private static readonly List<long> EntitiesToDelete = new List<long>();

    private Scene _lastRenderedScene;
    private ComponentIterator<Hierarchical> _hierarchicals;

    public Hierarchy<long> Hierarchy;

    public EditorHierarchyView()
    {
        Icon = new ResourceIdentifier("editor/hierarchy");
    }

    public override string Name => "Hierarchy";

    public override void Render(IRenderingUnit renderer)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;
        if (editingTab.Scene == null) return;
        
        var editingScene = editingTab.Scene;
        if (editingScene != _lastRenderedScene)
        {
            _hierarchicals = editingScene?.Components.CreateIterator<Hierarchical>(IterationFlags.IncludeInactive);
            _lastRenderedScene = editingScene;
        }

        if (_hierarchicals == null) return;
        
        if (Hierarchy == null) Hierarchy = new EntityHierarchy(ContentPanel);
        ContentPanel.MayScroll = true;

        // ContentPanel.Label("Entities", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
        // ContentPanel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
        if (ContentPanel.Button("New Entity", 14).IsPressed())
            ((EditorEnvironment)ContentPanel.Environment).TransactionManager.InsertTransaction(
                new CreateEntityTransaction(-1));
        ContentPanel.Separator();

        _hierarchicals.Reset();
        while (_hierarchicals.Next())
        {
            ref Hierarchical current = ref _hierarchicals.GetComponent();
            if (!current.Parent.IsNotNull) RenderEntity(ref current, ContentPanel, renderer);
        }
        
        Hierarchy.DrawDragLine(renderer, renderer.RootGroup["editor_gui_overlay"]);
    }

    private void RenderEntity(ref Hierarchical hierarchical, GuiPanel panel, IRenderingUnit renderer, int depth = 0)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;
        if (editingTab.Scene == null) return;
        
        long entityId = hierarchical.EntityId;

        bool hasChildren = hierarchical.FirstChildId != -1;
        bool expanded = Hierarchy.IsExpanded(entityId);

        var entity = new Entity(editingTab.Scene, entityId);

        HierarchyElement<long> button = panel.Element<HierarchyElement<long>>()
                .Hierarchy(Hierarchy)
                .Entity(entity, depth)
            ;

        if (hasChildren && expanded)
            foreach (ComponentReference<Hierarchical> childHierarchical in hierarchical.Children)
                if (childHierarchical.Has())
                    RenderEntity(ref childHierarchical.Get(), panel, renderer, depth + 1);

        bool selected = Hierarchy.Pressed
            ? Hierarchy.IsSelected(entityId)
            : editingTab.EditingEntity.Contains(entity.EntityId);

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
                Hierarchy.ExpandCollapse(entityId);
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
        var editingTab = editorBase.CurrentSceneTab;
        if (editingTab.Scene == null) return;
        
        bool wasSelected = editingTab.EditingEntity.Contains(id);

        bool control = Core.InputUnit.Devices.Keyboard.ControlDown;
        bool shift = Core.InputUnit.Devices.Keyboard.ShiftDown;

        Hierarchy.Selected.Clear();
        Hierarchy.Selected.AddRange(editingTab.EditingEntity);

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
        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;
        if (editingTab.Scene == null) return;
        
        bool wasSelected = editingTab.EditingEntity.Contains(id);

        bool control = Core.InputUnit.Devices.Keyboard.ControlDown;
        bool shift = Core.InputUnit.Devices.Keyboard.ShiftDown;

        if (control)
        {
            if (wasSelected)
                editingTab.EditingEntity.Remove(id);
            else
                editingTab.EditingEntity.Add(id);
        }
        else
        {
            editingTab.EditingEntity.Clear();
            editingTab.EditingEntity.Add(id);
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
        var editingScene = editorEnvironment.EditingSceneTab.Scene;
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
                if (editingScene.Components.HasComponent<Hierarchical>(id))
                    editingScene.Components.GetComponent<Hierarchical>(id)
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
        var editingScene = ((EditorEnvironment)Environment).EditingSceneTab.Scene;
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
                entity.Hierarchical.ParentId,
                entity.Hierarchical.NextSiblingId,
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