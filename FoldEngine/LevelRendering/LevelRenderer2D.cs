using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sandbox.Components;

namespace FoldEngine.Rendering {
    [GameSystem("fold:level_renderer.2d", ProcessingCycles.Render)]
    public class LevelRenderer2D : GameSystem {
        private ComponentIterator<Camera> _cameras;
        private ComponentIterator<MeshRenderable> _meshRenderables;

        internal override void Initialize() {
            _cameras = CreateComponentIterator<Camera>(IterationFlags.None);
            _meshRenderables = CreateComponentIterator<MeshRenderable>(IterationFlags.Ordered);
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
                
                Vector2 cameraScale = view.LocalScale;

                IRenderingLayer layer = renderer.Layers[camera.RenderToLayer];

                _meshRenderables.Reset();

                while(_meshRenderables.Next()) {
                    ref Transform transform = ref _meshRenderables.GetCoComponent<Transform>();
                    ref MeshRenderable meshRenderable = ref _meshRenderables.GetComponent();

                    ITexture texture = renderer.Textures[meshRenderable.TextureIdentifier];
                    
                    foreach(MeshCollection.Triangle triangle in Owner.Meshes.GetTrianglesForMesh(meshRenderable.MeshIdentifier)) {

                        var vertexA = Extensions.ToVector2(
                            (Matrix.CreateTranslation(triangle.A.Position) * meshRenderable.Matrix)
                            .Translation);
                        var vertexB = Extensions.ToVector2(
                            (Matrix.CreateTranslation(triangle.B.Position) * meshRenderable.Matrix)
                            .Translation);
                        var vertexC = Extensions.ToVector2(
                            (Matrix.CreateTranslation(triangle.C.Position) * meshRenderable.Matrix)
                            .Translation);

                        layer.Surface.Draw(new DrawTriangleInstruction(
                            texture,
                            RenderingLayer.WorldToScreen(layer,
                                (Vector2)((Complex) (transform.Apply(vertexA) - cameraPos)
                                          * cameraRotNegativeComplex) / cameraScale),
                            RenderingLayer.WorldToScreen(layer,
                                (Vector2)((Complex) (transform.Apply(vertexB) - cameraPos)
                                          * cameraRotNegativeComplex) / cameraScale),
                            RenderingLayer.WorldToScreen(layer,
                                (Vector2)((Complex) (transform.Apply(vertexC) - cameraPos)
                                          * cameraRotNegativeComplex) / cameraScale),
                            triangle.A.TextureCoordinate * meshRenderable.UVScale + meshRenderable.UVOffset,
                            triangle.B.TextureCoordinate * meshRenderable.UVScale + meshRenderable.UVOffset,
                            triangle.C.TextureCoordinate * meshRenderable.UVScale + meshRenderable.UVOffset,
                            Extensions.MultiplyColor(triangle.A.Color, meshRenderable.Color),
                            Extensions.MultiplyColor(triangle.B.Color, meshRenderable.Color),
                            Extensions.MultiplyColor(triangle.C.Color, meshRenderable.Color)
                        ));
                    }
                }
            }

            if(!anyCamera) {
                Console.WriteLine("No cameras in scene");
            }
        }
    }
}