using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Tools {
    public class HandTool : EditorTool {
        
        private bool _dragging = false;
        private Vector2 _dragStartWorldPos;

        public override void OnMousePressed(ref MouseEvent e) {
            IRenderingLayer worldLayer = Environment.Scene.Core.RenderingUnit.WorldLayer;
            Vector2 cameraPos = worldLayer.LayerToCamera(worldLayer.WindowToLayer(e.Position.ToVector2()));
            Vector2 worldPos = Environment.Scene.EditorComponents.EditorTransform.Apply(cameraPos);

            _dragStartWorldPos = worldPos;
            _dragging = true;
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            _dragging = false;
        }

        public override void OnInput(ControlScheme controls) {
            if(_dragging) {
                var worldLayer = Environment.Scene.Core.RenderingUnit.WorldLayer;
                Vector2 cameraRelativePos = worldLayer.LayerToCamera(worldLayer.WindowToLayer(Environment.MousePos.ToVector2()));

                ref Transform cameraTransform = ref Scene.EditorComponents.EditorTransform;

                cameraTransform.Position = _dragStartWorldPos;
                cameraTransform.Position = cameraTransform.Apply(-cameraRelativePos);
            }
        }

        public HandTool(EditorEnvironment environment) : base(environment) { }
    }
}