using System;
using EntryProject.Util;
using FoldEngine.Editor.Gui;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Tools {
    public class MoveTool : SelectTool {
        public override string Icon => "editor:move";

        public MoveTool(EditorEnvironment environment) : base(environment) { }

        public override void OnMousePressed(ref MouseEvent e) {
            base.OnMousePressed(ref e);
        }

        public override void Render(IRenderingUnit renderer) {
            long editingEntity = Scene.Systems.Get<EditorBase>().EditingEntity;
            if(editingEntity == -1) return;
            
            Entity entity = new Entity(Scene, editingEntity);
            Vector2 origin = entity.Transform.Position;
            Complex rotation = (entity.Transform.Apply(Vector2.UnitX) - origin).Normalized();
            
            RenderArrow(renderer, origin, origin + (Vector2)((Complex)Vector2.UnitX * rotation), Color.Red, 100);
            RenderArrow(renderer, origin, origin + (Vector2)((Complex)Vector2.UnitY * rotation), Color.Lime, 100);
        }

        private void RenderArrow(IRenderingUnit renderer, Vector2 start, Vector2 end, Color color, float fixedLength = 0) {
            start = renderer.GizmoLayer.CameraToLayer(Scene.EditorComponents.EditorTransform.Relativize(start));
            end = renderer.GizmoLayer.CameraToLayer(Scene.EditorComponents.EditorTransform.Relativize(end));

            if(fixedLength > 0) {
                end = (end - start).Normalized() * fixedLength + start;
            }

            Vector2 dir = (end - start).Normalized();
            Complex dirComplex = dir;

            float thickness = 2;
            float headLength = 20;
            float headWidth = 16;
            
            //Render line

            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                A = new Vector3(start + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
                Texture = renderer.WhiteTexture,
                Color = color
            });
            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
                B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
                Texture = renderer.WhiteTexture,
                Color = color
            });
            
            // Render head
            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                A = new Vector3(end, 0),
                B = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
                C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
                Texture = renderer.WhiteTexture,
                Color = color
            });
        }
    }
}