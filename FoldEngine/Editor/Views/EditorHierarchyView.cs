using System;
using System.Collections.Generic;
using EntryProject.Editor.Gui.Hierarchy;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Gui.Hierarchy;
using FoldEngine.Editor.Transactions;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;
using Sandbox.Components;
using Woofer;

namespace FoldEngine.Editor.Views {
    public class EditorHierarchyView : EditorView {
        
        private ComponentIterator<Transform> _transforms;

        public Hierarchy<long> Hierarchy;

        public override string Icon => "editor:hierarchy";
        public override string Name => "Hierarchy";

        public override void Initialize() {
            _transforms = Scene.Components.CreateIterator<Transform>(IterationFlags.None);
        }

        public override void Render(IRenderingUnit renderer) {
            if(Hierarchy == null) {
                Hierarchy = new EntityHierarchy(ContentPanel);
            }
            ContentPanel.MayScroll = true;
            
            // ContentPanel.Label("Entities", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
            // ContentPanel.Button("Back").Action(SceneEditor.Actions.ChangeToMenu, 0);
            ContentPanel.Button("New Entity", 14).LeftAction<CreateEntityAction>().Id(-1);;
            ContentPanel.Separator();
            
            _transforms.Reset();
            while(_transforms.Next()) {
                ref Transform current = ref _transforms.GetComponent();
                if(!current.Parent.IsNotNull) {
                    RenderEntity(ref current, ContentPanel, renderer);
                }
            }
        }

        private void RenderEntity(ref Transform transform, GuiPanel panel, IRenderingUnit renderer, int depth = 0) {
            long entityId = transform.EntityId;
            
            bool hasChildren = transform.FirstChildId != -1;
            bool expanded = Hierarchy.IsExpanded(entityId);
            
            Entity entity = new Entity(Scene, entityId);

            bool selected = Hierarchy.Pressed
                ? Hierarchy.IsSelected(entityId)
                : Scene.Systems.Get<EditorBase>().EditingEntity.Contains(entity.EntityId);

            var button = panel.Element<HierarchyElement<long>>()
                    .Hierarchy(Hierarchy)
                    .Entity(entity, depth)
                    .Icon(renderer.Textures["editor:cube"], selected ? Color.White : new Color(128, 128, 128))
                    .Selected(selected)
                    ;

            
            if(hasChildren && expanded) {
                foreach(ComponentReference<Transform> childTransform in transform.Children) {
                    if(childTransform.Has()) RenderEntity(ref childTransform.Get(), panel, renderer, depth + 1);
                }
            }
        }
    }

    public class DebugAction : IGuiAction {
        public IObjectPool Pool { get; set; }
        public void Perform(GuiElement element, MouseEvent e) {
            Console.WriteLine("debug action");
        }
    }

    public class DeleteEntityAction : IGuiAction {
        public IObjectPool Pool { get; set; }

        private long _entityId;

        public DeleteEntityAction Id(long entityId) {
            _entityId = entityId;
            return this;
        }
        
        private static List<long> _entitiesToDelete = new List<long>();
        
        public void Perform(GuiElement element, MouseEvent e) {
            _entitiesToDelete.Clear();
            if(element.Environment.Scene.Components.HasComponent<Transform>(_entityId)) {
                element.Environment.Scene.Components.GetComponent<Transform>(_entityId).DumpHierarchy(_entitiesToDelete);
            }

            CompoundTransaction<EditorEnvironment> transactions = new CompoundTransaction<EditorEnvironment>();
            foreach(long entityId in _entitiesToDelete) {
                transactions.Append(() => new DeleteEntityTransaction(entityId));
            }
            
            ((EditorEnvironment) element.Environment).TransactionManager.InsertTransaction(transactions);
        }
    }

    public class CreateEntityAction : IGuiAction {
        public IObjectPool Pool { get; set; }

        private long _entityId;

        public CreateEntityAction Id(long entityId) {
            _entityId = entityId;
            return this;
        }
        
        public void Perform(GuiElement element, MouseEvent e) {
            ((EditorEnvironment) element.Environment).TransactionManager.InsertTransaction(new CreateEntityTransaction(_entityId));
        }
    }

    public class EntityHierarchy : Hierarchy<long> {
        public EntityHierarchy(GuiEnvironment environment) : base(environment) { }
        public EntityHierarchy(GuiPanel parent) : base(parent) { }

        public override void Drop() {
            Console.WriteLine("Dropping: ");

            HierarchyDropMode dropMode;
            switch(DragRelative) {
                case -1: dropMode = HierarchyDropMode.Before;
                    break;
                case 0: dropMode = HierarchyDropMode.Inside;
                    break;
                case 1: dropMode = IsExpanded(DragTargetId) ? HierarchyDropMode.FirstInside : HierarchyDropMode.After;
                    break;
                default: throw new InvalidOperationException($"DragRelative can only be -1, 0 or 1, was {DragRelative}");
            }
            
            CompoundTransaction<EditorEnvironment> transactions = new CompoundTransaction<EditorEnvironment>();

            var dragTargetEntity = new Entity(Environment.Scene, DragTargetId);
            
            foreach(long id in Selected) {
                var selectedEntity = new Entity(Environment.Scene, id);
                if(dragTargetEntity == selectedEntity || selectedEntity.IsAncestorOf(dragTargetEntity)) {
                    Console.WriteLine("Cannot drag something into itself");
                    return;
                }
            }

            foreach(long id in Selected) {
                var entity = new Entity(Environment.Scene, id);
                var transaction = new ChangeEntityHierarchyTransaction(
                    entityId: id,
                    previousParent: entity.Transform.ParentId,
                    previousNextSibling: entity.Transform.NextSiblingId,
                    nextEntity: DragTargetId,
                    nextRelationship: dropMode,
                    snapshot: entity.Transform.CreateSnapshot()
                    );
                transactions.Append(() => transaction);
            }

            if(Environment is EditorEnvironment editorEnvironment) {
                editorEnvironment.TransactionManager.InsertTransaction(transactions);
            }
        }
    }
}