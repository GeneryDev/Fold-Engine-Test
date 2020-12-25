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
                
                Vector2 cameraPos = view.Position;
                Complex cameraRotNegativeComplex = Complex.FromRotation(-view.Rotation);

                IRenderingLayer layer = renderer.Layers[camera.RenderToLayer];
                
                _renderables.Reset();

                while(_renderables.Next()) {
                    ref Transform transform = ref _renderables.GetCoComponent<Transform>();

                    float w = 4f;
                    float h = 4f;
                    
                    layer.Surface.Draw(new DrawQuadInstruction(
                        renderer.Textures["main:beacon"],
                        RenderingLayer.Convert(layer, (Complex)(transform.Apply(new Vector2(-w/2, h/2)) - cameraPos) * cameraRotNegativeComplex),
                        RenderingLayer.Convert(layer, (Complex)(transform.Apply(new Vector2(-w/2, -h/2)) - cameraPos) * cameraRotNegativeComplex),
                        RenderingLayer.Convert(layer, (Complex)(transform.Apply(new Vector2(w/2, h/2)) - cameraPos) * cameraRotNegativeComplex),
                        RenderingLayer.Convert(layer, (Complex)(transform.Apply(new Vector2(w/2, -h/2)) - cameraPos) * cameraRotNegativeComplex),
                        new Vector2(0, 0),
                        new Vector2(0, 1),
                        new Vector2(1, 0),
                        new Vector2(1, 1)
                    ));
                }
                
            }

            if(!anyCamera) {
                Console.WriteLine("No cameras in scene");
            }
        }
    }
}