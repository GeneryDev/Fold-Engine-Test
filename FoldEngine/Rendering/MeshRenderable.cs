using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Rendering {
    [Component("fold:mesh_renderable")]
    [ComponentInitializer(typeof(MeshRenderable), nameof(InitializeComponent))]
    public struct MeshRenderable {
        public ResourceIdentifier TextureIdentifier;
        public ResourceIdentifier MeshIdentifier;
        public ResourceIdentifier EffectIdentifier;
        public Matrix Matrix;
        public Vector2 UVOffset;
        public Vector2 UVScale;
        public Color Color;
        public float ZIndex;

        /// <summary>
        ///     Returns an initialized mesh renderable component with all its correct default values.
        /// </summary>
        /// <param name="scene">The scene this component is being created in</param>
        /// <param name="entityId">The ID of the entity this component is being created for</param>
        /// <returns>An initialized component with all its correct default values.</returns>
        public static MeshRenderable InitializeComponent(Scene scene, long entityId) {
            return new MeshRenderable {Matrix = Matrix.Identity, UVScale = Vector2.One, Color = Color.White};
        }

        public Line[] GetFaces(ref Transform transform) {
            var faces = new Line[transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty).GetVertexCount()];
            int i = 0;

            Vector2 firstVertex = default;
            Vector2 prevVertex = default;
            bool first = true;
            foreach(Vector2 localVertex in transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty)
                .GetVertices()) {
                Vector2 vertex = transform.Apply(localVertex.ApplyMatrixTransform(Matrix));
                if(first)
                    firstVertex = vertex;
                else
                    faces[i - 1] = new Line(prevVertex, vertex);

                first = false;
                prevVertex = vertex;

                i++;
            }

            if(faces.Length > 0) faces[faces.Length - 1] = new Line(prevVertex, firstVertex);

            return faces;
        }

        public bool Contains(Vector2 point, ref Transform transform) {
            bool any = false;
            foreach(Line line in GetFaces(ref transform)) {
                any = true;
                Vector2 pointCopy = point;
                Line.LayFlat(line, ref pointCopy, out _);
                if(pointCopy.Y > 0) return false;
            }

            return any;
        }
    }
}