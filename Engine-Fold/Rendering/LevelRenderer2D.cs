﻿using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Rendering;

[GameSystem("fold:level_renderer.2d", ProcessingCycles.Render, true)]
public class LevelRenderer2D : GameSystem
{
    private ComponentIterator<Camera> _cameras;
    private ComponentIterator<MeshRenderable> _meshRenderables;

    public override void Initialize()
    {
        _cameras = CreateComponentIterator<Camera>(IterationFlags.None);
        _meshRenderables = CreateComponentIterator<MeshRenderable>(IterationFlags.Ordered);
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        bool anyCamera = false;

        _cameras.Reset();

        if (Scene.CameraOverrides != null)
        {
            anyCamera = true;
            RenderCamera(Scene.CameraOverrides.Camera, Scene.CameraOverrides.Transform, renderer,
                false);
        }
        else
        {
            while (_cameras.Next())
            {
                anyCamera = true;

                ref Camera camera = ref _cameras.GetComponent();
                ref Transform view = ref _cameras.GetCoComponent<Transform>();

                RenderCamera(camera, view, renderer, true);
            }
        }

        if (!anyCamera) Console.WriteLine("No cameras in scene");
    }

    private List<RenderableKey> _entitiesToRender = new List<RenderableKey>();

    private void RenderCamera(
        Camera camera,
        Transform view,
        IRenderingUnit renderer,
        bool setMainCameraId = false)
    {
        (float viewX, float viewY) = view.Position;
        if (camera.SnapPosition > 0)
        {
            viewX = (float)(Math.Round(viewX / camera.SnapPosition) * camera.SnapPosition);
            viewY = (float)(Math.Round(viewY / camera.SnapPosition) * camera.SnapPosition);
        }

        Complex cameraRotateScale = Complex.FromRotation(-view.Rotation);

        //Translate
        Matrix viewMatrix = new Matrix( //Translate
                                1, 0, 0, 0,
                                0, 1, 0, 0,
                                0, 0, 1, 0,
                                -viewX, -viewY, 0, 1
                            )
                            * new Matrix( //Rotate
                                cameraRotateScale.A, cameraRotateScale.B, 0, 0,
                                -cameraRotateScale.B, cameraRotateScale.A, 0, 0,
                                0, 0, 1, 0,
                                0, 0, 0, 1
                            )
                            * new Matrix( //Scale
                                1 / view.LocalScale.X, 0, 0, 0,
                                0, 1 / view.LocalScale.Y, 0, 0,
                                0, 0, 1, 0,
                                0, 0, 0, 1
                            );

        Scene.GizmoTransformMatrix = viewMatrix;
        if (setMainCameraId) Scene.MainCameraId = _cameras.GetEntityId();

        IRenderingLayer layer = !string.IsNullOrEmpty(camera.RenderToLayer)
            ? renderer.MainGroup[camera.RenderToLayer]
            : renderer.WorldLayer;
        if (layer == null) return;

        _meshRenderables.Reset();
        _entitiesToRender.Clear();

        while (_meshRenderables.Next())
        {
            ref MeshRenderable meshRenderable = ref _meshRenderables.GetComponent();
            var effect = Scene.Resources.Get<EffectR>(ref meshRenderable.EffectIdentifier, null);
            int sortKey = effect?.Order ?? 0;
            _entitiesToRender.Add(new RenderableKey()
            {
                EntityId = _meshRenderables.GetEntityId(),
                SortKey = sortKey
            });
        }

        _entitiesToRender.Sort((a, b) => a.SortKey - b.SortKey);

        for (var i = 0; i < _entitiesToRender.Count; i++)
        {
            var entity = new Entity(Scene, _entitiesToRender[i].EntityId);
            Transform transform = entity.Transform;
            ref MeshRenderable meshRenderable = ref entity.GetComponent<MeshRenderable>();
            if (camera.SnapPosition > 0)
            {
                Vector2 pos = transform.Position;
                pos.X = (float)(Math.Round(pos.X / camera.SnapPosition) * camera.SnapPosition);
                pos.Y = (float)(Math.Round(pos.Y / camera.SnapPosition) * camera.SnapPosition);
                transform.Position = pos;
            }

            if (meshRenderable.MeshIdentifier.Identifier == null
                || meshRenderable.TextureIdentifier.Identifier == null) continue;

            ITexture texture = Scene.Resources.Get<Texture>(ref meshRenderable.TextureIdentifier, Texture.Missing);
            if (texture == null) continue;

            foreach (Mesh.Triangle triangle in Scene.Resources
                         .Get<Mesh>(ref meshRenderable.MeshIdentifier, Mesh.Empty)
                         .GetTriangles())
            {
                Vector2 vertexA = triangle.A.Position.ToVector2().ApplyMatrixTransform(meshRenderable.Matrix);
                Vector2 vertexB = triangle.B.Position.ToVector2().ApplyMatrixTransform(meshRenderable.Matrix);
                Vector2 vertexC = triangle.C.Position.ToVector2().ApplyMatrixTransform(meshRenderable.Matrix);

                layer.Surface.Draw(new DrawTriangleInstruction(
                    texture,
                    new Vector3(layer.CameraToLayer(transform.Apply(vertexA).ApplyMatrixTransform(viewMatrix)),
                        meshRenderable.ZIndex),
                    new Vector3(layer.CameraToLayer(transform.Apply(vertexB).ApplyMatrixTransform(viewMatrix)),
                        meshRenderable.ZIndex),
                    new Vector3(layer.CameraToLayer(transform.Apply(vertexC).ApplyMatrixTransform(viewMatrix)),
                        meshRenderable.ZIndex),
                    triangle.A.TextureCoordinate * meshRenderable.UVScale + meshRenderable.UVOffset,
                    triangle.B.TextureCoordinate * meshRenderable.UVScale + meshRenderable.UVOffset,
                    triangle.C.TextureCoordinate * meshRenderable.UVScale + meshRenderable.UVOffset,
                    Extensions.MultiplyColor(triangle.A.Color, meshRenderable.Color),
                    Extensions.MultiplyColor(triangle.B.Color, meshRenderable.Color),
                    Extensions.MultiplyColor(triangle.C.Color, meshRenderable.Color),
                    Scene.Resources.Get<EffectR>(ref meshRenderable.EffectIdentifier, null)
                ));
            }
        }
    }

    public static void DrawOutline(Entity entity)
    {
        if (entity.HasComponent<MeshRenderable>())
            DrawOutline(entity.Scene, entity.Transform, entity.GetComponent<MeshRenderable>(),
                new Color(250, 110, 30));
    }

    public static void DrawOutline(
        Scene scene,
        Transform transform,
        MeshRenderable meshRenderable,
        Color outlineColor)
    {
        if (meshRenderable.MeshIdentifier.Identifier == null
            || meshRenderable.TextureIdentifier.Identifier == null) return;

        Vector2 firstVertex = default;
        Vector2 prevVertex = default;
        bool first = true;
        foreach (Vector2 localVertex in scene.Resources.Get<Mesh>(ref meshRenderable.MeshIdentifier, Mesh.Empty)
                     .GetVertices())
        {
            Vector2 vertex = transform.Apply(localVertex);
            if (first)
                firstVertex = vertex;
            else
                scene.DrawGizmo(prevVertex, vertex, outlineColor);

            first = false;
            prevVertex = vertex;
        }

        scene.DrawGizmo(prevVertex, firstVertex, outlineColor);
    }

    public long ListEntitiesIntersectingPosition(Vector2 worldPos)
    {
        _meshRenderables.Reset();

        while (_meshRenderables.Next())
        {
            ref Transform transform = ref _meshRenderables.GetCoComponent<Transform>();
            ref MeshRenderable meshRenderable = ref _meshRenderables.GetComponent();

            if (meshRenderable.MeshIdentifier.Identifier == null
                || meshRenderable.TextureIdentifier.Identifier == null) continue;

            if (meshRenderable.Contains(worldPos, ref transform)) return _meshRenderables.GetEntityId();
        }

        return -1;
    }

    private struct RenderableKey
    {
        public long EntityId;
        public int SortKey;
    }
}