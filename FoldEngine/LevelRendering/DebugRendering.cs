using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Rendering {
    [GameSystem("fold:debug_renderer", ProcessingCycles.Render)]
    public class DebugRendering : GameSystem {
        private ComponentIterator<Camera> _cameras;
        private Vector2 _lastIntersection;

        public bool LineIntersection = false;
        public bool SnapToLine = true;
        
        internal override void Initialize() {
            _cameras = CreateComponentIterator<Camera>(IterationFlags.None);
        }
        
        public override void OnRender(IRenderingUnit renderer) {
            _cameras.Reset();

            while(_cameras.Next()) {

                ref Camera camera = ref _cameras.GetComponent();
                ref Transform view = ref _cameras.GetCoComponent<Transform>();
                
                Vector2 cameraPos = view.Position;
                Complex cameraRotNegativeComplex = Complex.FromRotation(-view.Rotation);

                IRenderingLayer layer = renderer.Layers[camera.RenderToLayer];
                
                float thickness = 6;

                Line lineA = new Line(new Vector2(-145, -50), new Vector2(55, 55));
                Vector2 perpendicularA = (Vector2)(((Complex) (lineA.To - lineA.From)).Normalized * Complex.Imaginary) * thickness/2;

                layer.Surface.Draw(new DrawQuadInstruction(
                    renderer.Textures["main:pixel.white"],
                    RenderingLayer.WorldToScreen(layer,
                        (Complex) (lineA.From + perpendicularA - cameraPos)
                        * cameraRotNegativeComplex),
                    RenderingLayer.WorldToScreen(layer,
                        (Complex) (lineA.From - perpendicularA - cameraPos)
                        * cameraRotNegativeComplex),
                    RenderingLayer.WorldToScreen(layer,
                        (Complex) (lineA.To + perpendicularA - cameraPos)
                        * cameraRotNegativeComplex),
                    RenderingLayer.WorldToScreen(layer,
                        (Complex) (lineA.To - perpendicularA - cameraPos)
                        * cameraRotNegativeComplex),
                    Vector2.Zero,
                    Vector2.Zero,
                    Vector2.Zero,
                    Vector2.Zero
                ));


                if(LineIntersection) {
                    Line lineB = new Line(new Vector2(12, 120), RenderingLayer.ScreenToWorld(layer, Mouse.GetState().Position.ToVector2()));
                    Vector2 perpendicularB = (Vector2)(((Complex) (lineB.To - lineB.From)).Normalized * Complex.Imaginary) * thickness/2;

                    layer.Surface.Draw(new DrawQuadInstruction(
                        renderer.Textures["main:pixel.white"],
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (lineB.From + perpendicularB - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (lineB.From - perpendicularB - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (lineB.To + perpendicularB - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (lineB.To - perpendicularB - cameraPos)
                            * cameraRotNegativeComplex),
                        Vector2.Zero,
                        Vector2.Zero,
                        Vector2.Zero,
                        Vector2.Zero
                    ));
                    Vector2? intersection = lineA.Intersect(lineB, true, true);
                    
                    Color pointColor = Color.Lime;

                    if(!intersection.HasValue) {
                        intersection = _lastIntersection;
                        pointColor = Color.Red;
                    } else {
                        _lastIntersection = intersection.Value;
                    }

                    layer.Surface.Draw(new DrawQuadInstruction(
                        renderer.Textures["main:pixel.white"],
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (intersection + new Vector2(-1,-1)*thickness - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (intersection + new Vector2(1,-1)*thickness - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (intersection + new Vector2(-1,1)*thickness - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (intersection + new Vector2(1,1)*thickness - cameraPos)
                            * cameraRotNegativeComplex),
                        Vector2.Zero,
                        Vector2.Zero,
                        Vector2.Zero,
                        Vector2.Zero,
                        pointColor
                    ));
                    
                    _lastIntersection = intersection.Value;
                }

                if(SnapToLine) {
                    Vector2 snapped =
                        lineA.SnapPointToLine(
                            RenderingLayer.ScreenToWorld(layer, Mouse.GetState().Position.ToVector2()), true);
                    
                    layer.Surface.Draw(new DrawQuadInstruction(
                        renderer.Textures["main:pixel.white"],
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (snapped + new Vector2(-1,-1)*thickness - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (snapped + new Vector2(1,-1)*thickness - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (snapped + new Vector2(-1,1)*thickness - cameraPos)
                            * cameraRotNegativeComplex),
                        RenderingLayer.WorldToScreen(layer,
                            (Complex) (snapped + new Vector2(1,1)*thickness - cameraPos)
                            * cameraRotNegativeComplex),
                        Vector2.Zero,
                        Vector2.Zero,
                        Vector2.Zero,
                        Vector2.Zero,
                        Color.Aqua
                    ));
                }

                
            }
        }
    }
}