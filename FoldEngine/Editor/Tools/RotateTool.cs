using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Tools {
    public class RotateTool : SelectTool {
        public override string Icon => "editor:rotate";

        private Transform _movePivot;
        private bool _dragging = false;

        private bool hoveringRing = false;

        private Vector2 _pressMousePivotPosition;
        private List<Vector2> _pressEntityPivotPosition = new List<Vector2>();
        private List<Vector2> _pressEntityScale = new List<Vector2>();
        private List<SetEntityTransformTransaction> _transactions = new List<SetEntityTransformTransaction>();

        public RotateTool(EditorEnvironment environment) : base(environment) { }

        public override void OnMousePressed(ref MouseEvent e) {
            if(hoveringRing) {
                Vector2 mouseWorldPos =
                    Scene.MainCameraTransform.Apply(Environment.Renderer.GizmoLayer.LayerToCamera(
                        Environment.Renderer.GizmoLayer.WindowToLayer(e.Position.ToVector2())));
                _pressMousePivotPosition = _movePivot.Relativize(mouseWorldPos);
                
                _pressEntityPivotPosition.Clear();
                _pressEntityScale.Clear();
                _transactions.Clear();
                EditorBase editorBase = Scene.Systems.Get<EditorBase>();
                foreach(long entityId in editorBase.EditingEntity) {
                    if(entityId == -1) continue;

                    Entity entity = new Entity(Scene, entityId);

                    Vector2 relativeEntityPos = _movePivot.Relativize(entity.Transform.Position);
                    
                    _pressEntityPivotPosition.Add(relativeEntityPos);
                    _pressEntityScale.Add(entity.Transform.LocalScale);
                    
                    var transaction = new SetEntityTransformTransaction(entity.Transform.CreateSnapshot());
                    Environment.TransactionManager.InsertTransaction(transaction);
                    _transactions.Add(transaction);
                }

                _dragging = true;
            } else {
                base.OnMousePressed(ref e);
            }
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            _dragging = false;
            _transactions.Clear();
            _pressEntityPivotPosition.Clear();
            base.OnMouseReleased(ref e);
        }

        private void EnsurePivotExists() {
            if(!_movePivot.IsNotNull) {
                _movePivot = Transform.InitializeComponent(Scene, 0);
            }
        }

        public override void Render(IRenderingUnit renderer) {
            EnsurePivotExists();
            
            bool any = false;
            
            EditorBase editorBase = Scene.Systems.Get<EditorBase>();
            if(_dragging) {
                Vector2 mouseWorldPos =
                    Scene.MainCameraTransform.Apply(Environment.Renderer.GizmoLayer.LayerToCamera(
                        Environment.Renderer.GizmoLayer.WindowToLayer(Environment.MousePos.ToVector2())));

                Vector2 newScale = _movePivot.Relativize(mouseWorldPos) / _pressMousePivotPosition;

                if(newScale.X == 0) newScale.X = 1;
                if(newScale.Y == 0) newScale.Y = 1;

                _movePivot.LocalScale = newScale;
                int i = 0;
                foreach(long entityId in editorBase.EditingEntity) {
                    if(entityId == -1) continue;
                    any = true;
                
                    Entity entity = new Entity(Scene, entityId);
                
                    entity.Transform.Position = _movePivot.Position;
                    entity.Transform.Position = _movePivot.Apply(_pressEntityPivotPosition[i]);
                    
                    entity.Transform.LocalScale = _pressEntityScale[i] * newScale;
                    
                    _transactions[i].UpdateAfter(entity.Transform.CreateSnapshot());
                
                    i++;
                }
                
                _movePivot.LocalScale = Vector2.One;
            } else {
                _movePivot.LocalPosition = default;
                foreach(long entityId in editorBase.EditingEntity) {
                    if(entityId == -1) continue;
                    any = true;

                    Entity entity = new Entity(Scene, entityId);
                
                    _movePivot.LocalPosition += entity.Transform.Position;
                    _movePivot.Rotation = entity.Transform.Rotation;
                }
                if(any) _movePivot.LocalPosition /= editorBase.EditingEntity.Count;
            }

            if(any) {
                
                Vector2 origin = _movePivot.LocalPosition;
                Complex rotation = (_movePivot.Apply(Vector2.UnitX) - origin).Normalized();

                RenderLine(renderer,
                    origin,
                    origin + (Vector2) ((Complex) Vector2.UnitX * rotation),
                    Color.Blue,
                    120
                    );
                
                RenderRing(renderer,
                    origin,
                    origin + (Vector2) ((Complex) Vector2.UnitX * rotation),
                    Color.Blue,
                    new Color(200,
                        200,
                        255),
                    0, (float) (Math.PI / 2),
                    out hoveringRing,
                    _dragging ? hoveringRing : (bool?) null,
                    120);
            }
        }
        
        private void RenderLine(IRenderingUnit renderer, Vector2 start, Vector2 end, Color color, float fixedLength = 0) {
            start = renderer.GizmoLayer.CameraToLayer(Scene.MainCameraTransform.Relativize(start));
            end = renderer.GizmoLayer.CameraToLayer(Scene.MainCameraTransform.Relativize(end));

            if(fixedLength > 0) {
                end = (end - start).Normalized() * fixedLength + start;
            }

            Vector2 dir = (end - start).Normalized();
            Complex dirComplex = dir;

            float thickness = 2;
            
            //Render line

            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                A = new Vector3(start + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                Texture = renderer.WhiteTexture,
                Color = color
            });
            renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
                Texture = renderer.WhiteTexture,
                Color = color
            });
            
        }

        private void RenderRing(IRenderingUnit renderer, Vector2 start, Vector2 end, Color defaultColor, Color hoverColor, float startRotation, float endRotation, out bool hovered, bool? forceHoverState, float fixedRadius = 0) {
            start = renderer.GizmoLayer.CameraToLayer(Scene.MainCameraTransform.Relativize(start));
            end = renderer.GizmoLayer.CameraToLayer(Scene.MainCameraTransform.Relativize(end));

            if(fixedRadius > 0) {
                end = (end - start).Normalized() * fixedRadius + start;
            }
            

            Vector2 dir = (end - start).Normalized();
            Complex dirComplex = dir;

            float thickness = 2;
            float hoverDistance = 16;

            //Check hover
            if(!forceHoverState.HasValue) {
                Vector2 mousePosLayerSpace = renderer.GizmoLayer.WindowToLayer(Environment.MousePos.ToVector2());
                float distance = Math.Abs(Vector2.Distance(start, mousePosLayerSpace)
                                          - Vector2.Distance(start, end));
                hovered = distance <= hoverDistance;
            } else {
                hovered = forceHoverState.Value;
            }
            //
            // //Render line
            //
            // renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
            //     A = new Vector3(start + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            //     B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            //     C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
            //     Texture = renderer.WhiteTexture,
            //     Color = hovered ? hoverColor : defaultColor
            // });
            // renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
            //     A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
            //     B = new Vector3(start - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2, 0),
            //     C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * thickness / 2 - dir * headLength, 0),
            //     Texture = renderer.WhiteTexture,
            //     Color = hovered ? hoverColor : defaultColor
            // });
            //
            // // Render head
            //
            // renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
            //     A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
            //     B = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
            //     C = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2, 0),
            //     Texture = renderer.WhiteTexture,
            //     Color = hovered ? hoverColor : defaultColor
            // });
            // renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
            //     A = new Vector3(end + (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2, 0),
            //     B = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2 - dir * headLength, 0),
            //     C = new Vector3(end - (Vector2)(dirComplex * Complex.Imaginary) * headWidth / 2, 0),
            //     Texture = renderer.WhiteTexture,
            //     Color = hovered ? hoverColor : defaultColor
            // });
            
            
            int segments = 24;
            
            Vector2 endInner = end - dir * thickness / 2;
            Vector2 endOuter = end + dir * thickness / 2;

            Complex delta = Complex.FromRotation((float) (Math.PI * (360f / segments) / 180));
            for(int i = 0; i < segments; i++) {
                Vector2 nextInner = (Vector2) ((Complex) (endInner - start) * delta) + start;
                Vector2 nextOuter = (Vector2) ((Complex) (endOuter - start) * delta) + start;
                
                renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                    A = new Vector3(endInner, 0),
                    B = new Vector3(endOuter, 0),
                    C = new Vector3(nextInner, 0),
                    Texture = renderer.WhiteTexture,
                    Color = hovered ? hoverColor : defaultColor
                });
                
                renderer.GizmoLayer.Surface.Draw(new DrawTriangleInstruction() {
                    A = new Vector3(endOuter, 0),
                    B = new Vector3(nextOuter, 0),
                    C = new Vector3(nextInner, 0),
                    Texture = renderer.WhiteTexture,
                    Color = hovered ? hoverColor : defaultColor
                });

                endInner = nextInner;
                endOuter = nextOuter;
            }
        }
    }
}