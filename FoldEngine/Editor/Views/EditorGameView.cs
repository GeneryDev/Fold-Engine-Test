using System;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Tools;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views {
    public class EditorGameView : EditorView {
        public override string Icon => "editor:play";
        public override string Name => "Game";

        public override bool UseMargin => false;
        public override Color? BackgroundColor => Color.Black;

        public override void Render(IRenderingUnit renderer) {
            renderer.Groups["editor"].Dependencies[0].Group.Size = ContentPanel.Bounds.Size;
            renderer.Groups["editor"].Dependencies[0].Destination = ContentPanel.Bounds;
        }

        public override void EnsurePanelExists(GuiEnvironment environment) {
            if(ContentPanel == null) {
                ContentPanel = new GameViewPanel(this, environment);
            }
        }
    }

    public class GameViewPanel : GuiPanel {
        public override bool Focusable => true;

        private bool dragging = false;
        private Vector2 dragStartWorldPos;

        private EditorTool SelectedTool;
        private EditorTool ForcedTool;

        private EditorTool ActiveTool => ForcedTool ?? SelectedTool;
        
        public GameViewPanel(EditorGameView editorGameView, GuiEnvironment environment) : base(environment) {
            MayScroll = true;
            SelectedTool = new HandTool(environment as EditorEnvironment);
        }

        public override void OnMousePressed(ref MouseEvent e) {
            ActiveTool?.OnMousePressed(ref e);
            base.OnMousePressed(ref e);
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            base.OnMouseReleased(ref e);
            ActiveTool?.OnMouseReleased(ref e);
        }

        public override void Scroll(int dir) {
            if(Environment.Scene.EditorComponents != null) {
                ref Transform cameraTransform = ref Environment.Scene.EditorComponents.EditorTransform;

                var worldLayer = Environment.Scene.Core.RenderingUnit.WorldLayer;
                Vector2 cameraRelativePos =
                    worldLayer.LayerToCamera(worldLayer.WindowToLayer(Environment.MousePos.ToVector2()));
                Vector2 pivot = cameraTransform.Apply(cameraRelativePos);
                
                cameraTransform.LocalScale -= cameraTransform.LocalScale * 0.05f * dir;
                
                cameraTransform.Position = pivot;
                cameraTransform.Position = cameraTransform.Apply(-cameraRelativePos);
            }
        }

        public override void OnInput(ControlScheme controls) {
            Vector2 move = controls.Get<AnalogAction>("editor.movement.axis.x") * Vector2.UnitX
                           + controls.Get<AnalogAction>("editor.movement.axis.y") * Vector2.UnitY;
            if(move != default) {
                float speed = 250f;
                if(controls.Get<ButtonAction>("editor.movement.faster").Down) {
                    speed *= 4;
                }

                speed *= Environment.Scene.EditorComponents.EditorTransform.LocalScale.X;
                
                Environment.Scene.EditorComponents.EditorTransform.Position += move * speed * Time.DeltaTime;
            }
            
            ActiveTool?.OnInput(controls);
        }
    }
}