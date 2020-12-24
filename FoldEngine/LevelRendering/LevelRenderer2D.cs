using System;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Sandbox.Components;

namespace FoldEngine.Rendering {
    [GameSystem("fold:level_renderer.2d", ProcessingCycles.Render)]
    public class LevelRenderer2D : GameSystem {
        private ComponentIterator<Camera> _cameras;
        private ComponentIterator<LevelRenderable> _renderables;

        internal override void Initialize() {
            _cameras = CreateComponentIterator<Camera>(IterationFlags.None);
            _renderables = CreateComponentIterator<LevelRenderable>(IterationFlags.Ordered);
        }

        public override void OnRender(IRenderingUnit renderer) {
            bool anyCamera = false;

            _cameras.Reset();

            while(_cameras.Next()) {
                anyCamera = true;

                ref Camera camera = ref _cameras.GetComponent();
                ref Transform view = ref _cameras.GetCoComponent<Transform>();
                
                // view.LocalRotation += Time.DeltaTime;
                
                Vector2 cameraPos = view.Position;
                float cameraRot = view.Rotation;
                Complex cameraRotNegativeComplex = Complex.FromRotation(-cameraRot);

                IRenderingLayer layer = renderer.Layers[camera.RenderToLayer];
                
                layer.Surface.Draw(new DrawTriangleInstruction(
                    renderer.Textures["test"],
                    RenderingLayer.Convert(layer, 8*new Vector2(-4 + -1f, 1.75f + 1f)),
                    RenderingLayer.Convert(layer, 8*new Vector2(-4 + 0, 1.75f + -0.732f)),
                    RenderingLayer.Convert(layer, 8*new Vector2(-4 + 1f, 1.75f + 1f)),
                    new Vector2(0.5f,0.5f),
                    new Vector2(0.5f,0.5f),
                    new Vector2(0.5f,0.5f),
                    Color.Lime,
                    Color.Red,
                    Color.Yellow
                ));
                
                _renderables.Reset();

                while(_renderables.Next()) {
                    // ref Transform transform = ref _renderables.GetCoComponent<Transform>();
                    //
                    // Vector2 globalPos = transform.Position;
                    // float globalRot = transform.Rotation;
                    //
                    // Vector2 relativePos = globalPos - cameraPos;
                    //
                    // Vector2 rotatedRelativePos = (Complex)relativePos * Complex.FromRotation(-cameraRot);
                    // float relativeRot = globalRot - cameraRot;
                    //
                    // Vector2 screenPos = RenderingLayer.Convert(layer, rotatedRelativePos);
                    // float width = 16;
                    // float height = 16;
                    //
                    // transform.Apply(transform.LocalPosition);
                    //
                    // layer.Surface.Draw(new DrawRectInstruction() {
                    //     Texture = renderer.Textures["test"],
                    //     DestinationRectangle = new Rectangle((int)Math.Round(screenPos.X), (int) Math.Round(screenPos.Y), (int) width, (int) height),
                    //     Rotation = relativeRot
                    // });
                    
                    ref Transform transform = ref _renderables.GetCoComponent<Transform>();

                    float w = 4f;
                    float h = 4f;
                    
                    layer.Surface.Draw(new DrawQuadInstruction(
                        renderer.Textures["test"],
                        RenderingLayer.Convert(layer, (Complex)(transform.ApplyLocal(new Vector2(-w/2, h/2)) - cameraPos) * cameraRotNegativeComplex),
                        RenderingLayer.Convert(layer, (Complex)(transform.ApplyLocal(new Vector2(-w/2, -h/2)) - cameraPos) * cameraRotNegativeComplex),
                        RenderingLayer.Convert(layer, (Complex)(transform.ApplyLocal(new Vector2(w/2, h/2)) - cameraPos) * cameraRotNegativeComplex),
                        RenderingLayer.Convert(layer, (Complex)(transform.ApplyLocal(new Vector2(w/2, -h/2)) - cameraPos) * cameraRotNegativeComplex),
                        new Vector2(0, 0),
                        new Vector2(0, 1),
                        new Vector2(1, 0),
                        new Vector2(1, 1)
                    ));
                    
                    
                    layer.Surface.Draw(new DrawRectInstruction(renderer.Textures.GetAtlasTexture("main"), new Vector2(0, 0)));
                }

                
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.Textures["test"],
                    DestinationRectangle = new Rectangle(0,0,2,2)
                });
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.Textures["test"],
                    DestinationRectangle = new Rectangle(160,90,2,2)
                });
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.Textures["test"],
                    DestinationRectangle = new Rectangle(320,180,2,2)
                });
                
            }

            if(!anyCamera) {
                Console.WriteLine("No cameras in scene");
            }

            // Texture2DWrapper testSprite = renderer.Textures["test"];
            // renderer.Layers["level"]
            //     .Surface.Draw(new DrawInstruction {
            //         Texture = testSprite,
            //         DestinationRectangle = new Rectangle((int) Time.TotalTime, 16, 16, 16),
            //
            //         Rotation = Time.TotalTime,
            //         Pivot = Vector2.Zero
            //     });
            // renderer.Layers["level"]
            //     .Surface.Draw(new DrawInstruction {
            //         Texture = testSprite,
            //         DestinationRectangle = new Rectangle((int) Time.TotalTime, 0, 8, 8)
            //     });
            // renderer.Layers["hud"]
            //     .Surface.Draw(new DrawInstruction {
            //         Texture = testSprite,
            //         DestinationRectangle = new Rectangle((int) Time.TotalTime, 16, 16, 16)
            //     });
        }
    }
}