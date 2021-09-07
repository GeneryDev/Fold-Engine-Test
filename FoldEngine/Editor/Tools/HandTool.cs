using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Tools {
    public class HandTool : EditorTool {
        public override string Icon => "editor:hand";
        
        private bool _dragging = false;
        private Vector2 _dragStartWorldPos;

        public override void OnMousePressed(ref MouseEvent e) {
            if(Scene.EditorComponents == null) return;
            
            IRenderingLayer worldLayer = Scene.Core.RenderingUnit.WorldLayer;
            Vector2 cameraPos = worldLayer.LayerToCamera(worldLayer.WindowToLayer(e.Position.ToVector2()));
            Vector2 worldPos = Scene.EditorComponents.EditorTransform.Apply(cameraPos);

            _dragStartWorldPos = worldPos;
            _dragging = true;
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            _dragging = false;
        }

        public override void OnInput(ControlScheme controls) {
            if(Scene.EditorComponents == null) return;
            
            if(_dragging) {
                var worldLayer = Scene.Core.RenderingUnit.WorldLayer;
                Vector2 cameraRelativePos = worldLayer.LayerToCamera(worldLayer.WindowToLayer(Environment.MousePos.ToVector2()));

                ref Transform cameraTransform = ref Scene.EditorComponents.EditorTransform;

                cameraTransform.Position = _dragStartWorldPos;
                cameraTransform.Position = cameraTransform.Apply(-cameraRelativePos);
            }
        }

        public HandTool(EditorEnvironment environment) : base(environment) { }
    }
}