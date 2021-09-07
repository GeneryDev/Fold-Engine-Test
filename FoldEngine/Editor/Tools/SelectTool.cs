using System;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Views;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Editor.Tools {
    public class SelectTool : EditorTool {
        public override string Icon => "editor:cursor";
        
        public SelectTool(EditorEnvironment environment) : base(environment) { }
        
        public override void OnInput(ControlScheme controls) {
        }

        public override void OnMousePressed(ref MouseEvent e) {
            ref Transform cameraTransform = ref Scene.EditorComponents.EditorTransform;
            
            var worldLayer = Environment.Scene.Core.RenderingUnit.WorldLayer;
            Vector2 cameraRelativePos =
                worldLayer.LayerToCamera(worldLayer.WindowToLayer(Environment.MousePos.ToVector2()));
            Vector2 worldPos = cameraTransform.Apply(cameraRelativePos);

            long intersectingEntities = Scene.Systems.Get<LevelRenderer2D>().ListEntitiesIntersectingPosition(worldPos);

            EditorBase editorBase = Scene.Systems.Get<EditorBase>();
            if(!Scene.Core.InputUnit.Devices.Keyboard[Keys.LeftControl].Down
               && !Scene.Core.InputUnit.Devices.Keyboard[Keys.RightControl].Down) {
                editorBase.EditingEntity.Clear();
            }

            if(intersectingEntities != -1 && !editorBase.EditingEntity.Contains(intersectingEntities)) editorBase.EditingEntity.Add(intersectingEntities);
            Environment.SwitchToView(Environment.GetView<EditorInspectorView>());
        }

        public override void OnMouseReleased(ref MouseEvent e) {
        }
    }
}