using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [Component("fold:physics.mesh_collider")]
    public struct MeshCollider : ICollider {
        public string MeshIdentifier;

        public bool ThickFaces { get; set; }
        
        public Vector2[] GetVertices(ref Transform transform) {
            Vector2[] vertices = new Vector2[transform.Scene.Meshes.GetVertexCountForMesh(MeshIdentifier)];
            int i = 0;
            foreach(Vector2 vertex in transform.Scene.Meshes.GetVerticesForMesh(MeshIdentifier)) {
                vertices[i] = transform.Apply(vertex);
                i++;
            }

            return vertices;
        }

        public Line[] GetFaces(ref Transform transform) {
            Line[] faces = new Line[transform.Scene.Meshes.GetVertexCountForMesh(MeshIdentifier)];
            int i = 0;
            
            
            Vector2 firstVertex = default;
            Vector2 prevVertex = default;
            bool first = true;
            foreach(var localVertex in transform.Scene.Meshes.GetVerticesForMesh(MeshIdentifier)) {
                Vector2 vertex = transform.Apply(localVertex);
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
            return transform.Scene.Meshes.IsPointInsidePolygon(transform.ApplyReverse(point), MeshIdentifier);
        }

        public Vector2 GetFarthestVertexFromOrigin(ref Transform transform) {
            return transform.Apply(transform.Scene.Meshes.GetFarthestVertexFromOrigin(MeshIdentifier));
        }

        public float GetReach(ref Transform transform) {
            return (GetFarthestVertexFromOrigin(ref transform) - transform.Position).Length();
        }
    }
}