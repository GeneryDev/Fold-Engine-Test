﻿using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Physics;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Rendering {
    [Component("fold:mesh_renderable")]
    [ComponentInitializer(typeof(MeshRenderable), nameof(InitializeComponent))]
    public struct MeshRenderable {
        public string TextureIdentifier;
        public string MeshIdentifier;
        public Matrix Matrix;
        public Vector2 UVOffset;
        public Vector2 UVScale;
        public Color Color;
        
        /// <summary>
        /// Returns an initialized mesh renderable component with all its correct default values.
        /// </summary>
        /// <param name="scene">The scene this component is being created in</param>
        /// <param name="entityId">The ID of the entity this component is being created for</param>
        /// <returns>An initialized component with all its correct default values.</returns>
        public static MeshRenderable InitializeComponent(Scene scene, long entityId)
        {
            return new MeshRenderable() { Matrix = Matrix.Identity, UVScale = Vector2.One, Color = Color.White};
        }
        
        public Line[] GetFaces(ref Transform transform) {
            Line[] faces = new Line[transform.Scene.Meshes.GetVertexCountForMesh(MeshIdentifier)];
            int i = 0;
    
            Vector2 firstVertex = default;
            Vector2 prevVertex = default;
            bool first = true;
            foreach(Vector2 localVertex in transform.Scene.Meshes.GetVerticesForMesh(MeshIdentifier)) {
                Vector2 vertex = transform.Apply(localVertex.ApplyMatrixTransform(Matrix));
                if(first) {
                    firstVertex = vertex;
                } else {
                    faces[i-1] = new Line(prevVertex, vertex);
                }

                first = false;
                prevVertex = vertex;

                i++;
            }
            faces[faces.Length-1] = new Line(prevVertex, firstVertex);

            return faces;
        }

        public bool Contains(Vector2 point, ref Transform transform) {
            foreach(Line line in GetFaces(ref transform)) {
                Vector2 pointCopy = point;
                Line.LayFlat(line, ref pointCopy, out _);
                if(pointCopy.Y > 0) return false;
            }
            return true;
        }
    }
}