﻿using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sandbox.Components;

namespace FoldEngine.Rendering {
    [GameSystem("fold:level_renderer.2d", ProcessingCycles.Render)]
    public class LevelRenderer2D : GameSystem {
        private ComponentIterator<Camera> _cameras;
        private ComponentIterator<LevelRenderable> _renderables;
        private ComponentIterator<MeshRenderable> _meshRenderables;

        internal override void Initialize() {
            _cameras = CreateComponentIterator<Camera>(IterationFlags.None);
            _renderables = CreateComponentIterator<LevelRenderable>(IterationFlags.Ordered);
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
                
                _meshRenderables.Reset();

                while(_meshRenderables.Next()) {
                    ref Transform transform = ref _meshRenderables.GetCoComponent<Transform>();
                    ref MeshRenderable meshRenderable = ref _meshRenderables.GetComponent();

                    ITexture texture = renderer.Textures[meshRenderable.TextureIdentifier];
                    
                    foreach(var triangle in Owner.Meshes.GetTrianglesForMesh(meshRenderable.MeshIdentifier)) {

                        Vector2 vertexA = Extensions.ToVector2(
                            (Matrix.CreateTranslation(triangle.A.Position) * meshRenderable.Matrix)
                            .Translation);
                        Vector2 vertexB = Extensions.ToVector2(
                            (Matrix.CreateTranslation(triangle.B.Position) * meshRenderable.Matrix)
                            .Translation);
                        Vector2 vertexC = Extensions.ToVector2(
                            (Matrix.CreateTranslation(triangle.C.Position) * meshRenderable.Matrix)
                            .Translation);

                        layer.Surface.Draw(new DrawTriangleInstruction(
                            texture,
                            RenderingLayer.Convert(layer,
                                (Complex) (transform.Apply(vertexA) - cameraPos)
                                * cameraRotNegativeComplex),
                            RenderingLayer.Convert(layer,
                                (Complex) (transform.Apply(vertexB) - cameraPos)
                                * cameraRotNegativeComplex),
                            RenderingLayer.Convert(layer,
                                (Complex) (transform.Apply(vertexC) - cameraPos)
                                * cameraRotNegativeComplex),
                            triangle.A.TextureCoordinate,
                            triangle.B.TextureCoordinate,
                            triangle.C.TextureCoordinate
                        ));
                        // layer.Surface.Draw(new DrawTriangleInstruction(
                        //     texture,
                        //     RenderingLayer.Convert(layer,
                        //         (Complex) (transform.Apply(new Vector2(triangle.A.Position.X * meshRenderable.Scale.X + meshRenderable.Offset.X,
                        //                        triangle.A.Position.Y * meshRenderable.Scale.Y + meshRenderable.Offset.Y))
                        //                    - cameraPos)
                        //         * cameraRotNegativeComplex),
                        //     RenderingLayer.Convert(layer,
                        //         (Complex) (transform.Apply(new Vector2(triangle.B.Position.X * meshRenderable.Scale.X + meshRenderable.Offset.X,
                        //                        triangle.B.Position.Y * meshRenderable.Scale.Y + meshRenderable.Offset.Y))
                        //                    - cameraPos)
                        //         * cameraRotNegativeComplex),
                        //     RenderingLayer.Convert(layer,
                        //         (Complex) (transform.Apply(new Vector2(triangle.C.Position.X * meshRenderable.Scale.X + meshRenderable.Offset.X,
                        //                        triangle.C.Position.Y * meshRenderable.Scale.Y + meshRenderable.Offset.Y))
                        //                    - cameraPos)
                        //         * cameraRotNegativeComplex),
                        //     triangle.A.TextureCoordinate,
                        //     triangle.B.TextureCoordinate,
                        //     triangle.C.TextureCoordinate
                        // ));
                    }
                }
                
            }

            if(!anyCamera) {
                Console.WriteLine("No cameras in scene");
            }
        }
    }
}