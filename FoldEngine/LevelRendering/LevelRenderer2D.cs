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
                
                (float viewX, float viewY) = view.Position;
                Complex cameraRotateScale = Complex.FromRotation(-view.Rotation);
                
                var viewMatrix = new Matrix(
                    cameraRotateScale.A / view.LocalScale.X,                     cameraRotateScale.B / view.LocalScale.Y,                     0, 0,
                    (cameraRotateScale*Complex.Imaginary).A / view.LocalScale.X, (cameraRotateScale*Complex.Imaginary).B / view.LocalScale.Y, 0, 0,
                    0,                                       0,                                       1, 0,
                    -viewX / view.LocalScale.X,            -viewY / view.LocalScale.Y,            0, 1
                );

                Owner.GizmoTransformMatrix = viewMatrix;
                Owner.MainCameraId = _cameras.GetEntityId();

                IRenderingLayer layer = camera.RenderToLayer != null ? renderer.Layers[camera.RenderToLayer] : renderer.WorldLayer;

                _meshRenderables.Reset();

                while(_meshRenderables.Next()) {
                    ref Transform transform = ref _meshRenderables.GetCoComponent<Transform>();
                    ref MeshRenderable meshRenderable = ref _meshRenderables.GetComponent();

                    ITexture texture = renderer.Textures[meshRenderable.TextureIdentifier];
                    
                    foreach(MeshCollection.Triangle triangle in Owner.Meshes.GetTrianglesForMesh(meshRenderable.MeshIdentifier)) {
                        Vector2 vertexA = triangle.A.Position.ToVector2().ApplyMatrixTransform(meshRenderable.Matrix);
                        Vector2 vertexB = triangle.B.Position.ToVector2().ApplyMatrixTransform(meshRenderable.Matrix);
                        Vector2 vertexC = triangle.C.Position.ToVector2().ApplyMatrixTransform(meshRenderable.Matrix);

                        layer.Surface.Draw(new DrawTriangleInstruction(
                            texture,
                            layer.CameraToLayer(transform.Apply(vertexA).ApplyMatrixTransform(viewMatrix)),
                            layer.CameraToLayer(transform.Apply(vertexB).ApplyMatrixTransform(viewMatrix)),
                            layer.CameraToLayer(transform.Apply(vertexC).ApplyMatrixTransform(viewMatrix)),
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