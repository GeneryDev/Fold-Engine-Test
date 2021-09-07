using System;
using EntryProject.Util;
using FoldEngine.Components;
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

        private Transform _movePivot;

        private void EnsurePivotExists() {
            if(!_movePivot.IsNotNull) {
                _movePivot = Transform.InitializeComponent(Scene, 0);
            }
        }

        public override void Render(IRenderingUnit renderer) {
            EnsurePivotExists();
            
            EditorBase editorBase = Scene.Systems.Get<EditorBase>();
            _movePivot.LocalPosition = default;
            bool any = false;
            foreach(long entityId in editorBase.EditingEntity) {
                if(entityId == -1) continue;
                any = true;

                Entity entity = new Entity(Scene, entityId);
            
                _movePivot.LocalPosition += entity.Transform.Position;
            }

            if(any) {
                _movePivot.LocalPosition /= editorBase.EditingEntity.Count;
                
                Vector2 origin = _movePivot.LocalPosition;
                Complex rotation = (_movePivot.Apply(Vector2.UnitX) - origin).Normalized();
                
                RenderArrow(renderer, origin, origin + (Vector2)((Complex)Vector2.UnitX * rotation), Color.Red, new Color(255, 200, 200), out _, 100);
                RenderArrow(renderer, origin, origin + (Vector2)((Complex)Vector2.UnitY * rotation), Color.Lime, new Color(200, 255, 200), out _, 100);
            }
        }

        private void RenderArrow(IRenderingUnit renderer, Vector2 start, Vector2 end, Color defaultColor, Color hoverColor, out bool hovered, float fixedLength = 0) {
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
            float hoverDistance = 16;
            
            //Check hover
            Line line = new Line(start, end);
            Vector2 mousePosLayerSpace = renderer.GizmoLayer.WindowToLayer(Environment.MousePos.ToVector2());
            hovered = line.DistanceFromPoint(mousePosLayerSpace, true) <= hoverDistance;
            
            //Render line

            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                A = new Vector3(start + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
                Texture = renderer.WhiteTexture,
                Color = hovered ? hoverColor : defaultColor
            });
            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
                B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
                Texture = renderer.WhiteTexture,
                Color = hovered ? hoverColor : defaultColor
            });
            
            // Render head
            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                A = new Vector3(end, 0),
                B = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
                C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
                Texture = renderer.WhiteTexture,
                Color = hovered ? hoverColor : defaultColor
            });
        }
    }
}