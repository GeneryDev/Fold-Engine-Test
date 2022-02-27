using System;
using System.Collections.Generic;
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
        public List<long> EditingEntity = new List<long>();
        
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
            Environment.AddView<EditorResourcesView>(Environment.SouthPanel);
            Environment.AddView<EditorTestView>(Environment.SouthPanel);

            Environment.WestPanel.ViewLists[0].ActiveView = Environment.GetView<EditorHierarchyView>();
            Environment.SouthPanel.ViewLists[0].ActiveView = Environment.GetView<EditorResourcesView>();
        }

        public override void OnInput() {
            Environment.Input(Owner.Core.InputUnit);
        }

        public override void OnUpdate() {
            Environment.Update();
        }

        public override void OnRender(IRenderingUnit renderer) {
            Environment.Render(renderer, renderer.RootGroup["editor_gui"], renderer.RootGroup["editor_gui_overlay"]);

            foreach(long entityId in EditingEntity) {
                if(Owner.Components.HasComponent<Transform>(entityId)) {
                    Entity entity = new Entity(Owner, entityId);
                
                    LevelRenderer2D.DrawOutline(entity);
                    ColliderGizmoRenderer.DrawColliderGizmos(entity);
                }
            }
        }
    }
}