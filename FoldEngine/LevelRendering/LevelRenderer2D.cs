using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor;
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
    [GameSystem("fold:level_renderer.2d", ProcessingCycles.Render, runWhenPaused: true)]
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

            if(Owner.EditorComponents != null) {
                anyCamera = true;
                RenderCamera(Owner.EditorComponents.EditorCamera, Owner.EditorComponents.EditorTransform, renderer, false);
            } else {
                while(_cameras.Next()) {
                    anyCamera = true;

                    ref Camera camera = ref _cameras.GetComponent();
                    ref Transform view = ref _cameras.GetCoComponent<Transform>();
                
                    RenderCamera(camera, view, renderer, true);
                }
            }

            if(!anyCamera) {
                Console.WriteLine("No cameras in scene");
            }
        }

        private void RenderCamera(Camera camera, Transform view, IRenderingUnit renderer, bool setMainCameraId = false) {
            (float viewX, float viewY) = view.Position;
            Complex cameraRotateScale = Complex.FromRotation(-view.Rotation);

            var viewMatrix = new Matrix(
                cameraRotateScale.A / view.LocalScale.X, cameraRotateScale.B / view.LocalScale.Y, 0, 0,
                (cameraRotateScale * Complex.Imaginary).A / view.LocalScale.X,
                (cameraRotateScale * Complex.Imaginary).B / view.LocalScale.Y, 0, 0,
                0, 0, 1, 0,
                -viewX / view.LocalScale.X, -viewY / view.LocalScale.Y, 0, 1
            );

            Owner.GizmoTransformMatrix = viewMatrix;
            if(setMainCameraId) {
                Owner.MainCameraId = _cameras.GetEntityId();
            }

            IRenderingLayer layer = !string.IsNullOrEmpty(camera.RenderToLayer)
                ? renderer.MainGroup[camera.RenderToLayer]
                : renderer.WorldLayer;
            if(layer == null) return;

            _meshRenderables.Reset();

            while(_meshRenderables.Next()) {
                ref Transform transform = ref _meshRenderables.GetCoComponent<Transform>();
                ref MeshRenderable meshRenderable = ref _meshRenderables.GetComponent();

                if(meshRenderable.MeshIdentifier == null || meshRenderable.TextureIdentifier == null) continue;

                ITexture texture = renderer.Textures[meshRenderable.TextureIdentifier];

                foreach(MeshCollection.Triangle triangle in Owner.Meshes.GetTrianglesForMesh(meshRenderable
                    .MeshIdentifier)) {
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

        public static void DrawOutline(Entity entity) {
            if(entity.HasComponent<MeshRenderable>()) DrawOutline(entity.Scene, entity.Transform, entity.GetComponent<MeshRenderable>(), new Color(250, 110, 30));
        }

        public static void DrawOutline(Scene scene, Transform transform, MeshRenderable meshRenderable, Color outlineColor) {
            if(meshRenderable.MeshIdentifier == null || meshRenderable.TextureIdentifier == null) return;

            Vector2 firstVertex = default;
            Vector2 prevVertex = default;
            bool first = true;
            foreach(var localVertex in scene.Meshes.GetVerticesForMesh(meshRenderable.MeshIdentifier)) {
                Vector2 vertex = transform.Apply(localVertex);
                if(first) {
                    firstVertex = vertex;
                } else {
                    scene.DrawGizmo(prevVertex, vertex, outlineColor);
                }

                first = false;
                prevVertex = vertex;
            }
            scene.DrawGizmo(prevVertex, firstVertex, outlineColor);
        }
        
        public long ListEntitiesIntersectingPosition(Vector2 worldPos) {
            _meshRenderables.Reset();

            while(_meshRenderables.Next()) {
                ref Transform transform = ref _meshRenderables.GetCoComponent<Transform>();
                ref MeshRenderable meshRenderable = ref _meshRenderables.GetComponent();

                if(meshRenderable.MeshIdentifier == null || meshRenderable.TextureIdentifier == null) continue;

                if(meshRenderable.Contains(worldPos, ref transform)) {
                    return _meshRenderables.GetEntityId();
                }
            }

            return -1;
        }
    }
}