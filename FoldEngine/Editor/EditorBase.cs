using System;
using FoldEngine.Commands;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Physics;
using FoldEngine.Rendering;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Woofer;

namespace FoldEngine.Editor {

    [GameSystem("fold:editor.base", ProcessingCycles.All, runWhenPaused: true)]
    public class EditorBase : GameSystem {
        // public override bool ShouldSave => false;
        
        public EditorEnvironment Environment;
        public long EditingEntity = -1;
        
        public override void SubscribeToEvents() {
            Subscribe<WindowSizeChangedEvent>((ref WindowSizeChangedEvent evt) => {
                Environment.LayoutValidated = false;
            });
        }

        internal override void Initialize() {
            Environment = new EditorEnvironment(Owner);

            Environment.AddView<EditorToolbarView>(Environment.NorthPanel);
            Environment.AddView<EditorHierarchyView>(Environment.WestPanel);
            Environment.AddView<EditorSystemsView>(Environment.WestPanel);
            Environment.AddView<EditorInspectorView>(Environment.EastPanel);
            Environment.AddView<EditorGameView>(Environment.CenterPanel);
            Environment.AddView<EditorTestView>(Environment.SouthPanel);
        }

        public override void OnInput() {
            Environment.Input(Owner.Core.InputUnit);
        }

        public override void OnUpdate() {
            Environment.Update();
        }

        public override void OnRender(IRenderingUnit renderer) {
            Environment.Render(renderer, renderer.RootGroup["editor_gui"], renderer.RootGroup["editor_gui_overlay"]);

            if(EditingEntity != -1 && Owner.Components.HasComponent<Transform>(EditingEntity)) {
                Entity entity = new Entity(Owner, EditingEntity);
                
                LevelRenderer2D.DrawOutline(entity);
                ColliderGizmoRenderer.DrawColliderGizmos(entity);
                
                Owner.DrawGizmo(entity.Transform.Position, 1, Color.AntiqueWhite);
            }
        }
    }
}