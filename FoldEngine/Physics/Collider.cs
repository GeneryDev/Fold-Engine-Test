using System;
using EntryProject.Editor.Inspector;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    [Component("fold:physics.collider")]
    [ComponentInitializer(typeof(Collider), nameof(InitializeComponent))]
    public struct Collider {
        public ColliderType Type;
        [ShowOnlyIf.Not(nameof(Type), ColliderType.None)]
        [ShowOnlyIf(nameof(Type), ColliderType.Mesh)]
        public ResourceIdentifier MeshIdentifier;
        
        [ShowOnlyIf.Not(nameof(Type), ColliderType.None)]
        [ShowOnlyIf(nameof(Type), ColliderType.Box)]
        public float Width;
        [ShowOnlyIf.Not(nameof(Type), ColliderType.None)]
        [ShowOnlyIf(nameof(Type), ColliderType.Box)]
        public float Height;
        
        [ShowOnlyIf.Not(nameof(Type), ColliderType.None)]
        public bool ThickFaces;
        [ShowOnlyIf.Not(nameof(Type), ColliderType.None)]
        [ShowOnlyIf(nameof(ThickFaces), false)]
        public float ThinFaceTolerance;

        public static Collider InitializeComponent(Scene scene, long entityId) {
            return new Collider() {
                Type = ColliderType.Box,
                Width = 1,
                Height = 1,
                ThinFaceTolerance = 0.1f
            };
        }

        public void SetBox(float width, float height) {
            Type = ColliderType.Box;
            Width = width;
            Height = height;
        }

        public void SetMesh(string meshIdentifier) {
            Type = ColliderType.Mesh;
            MeshIdentifier = new ResourceIdentifier(meshIdentifier);
        }

        public Vector2[] GetVertices(ref Transform transform) {
            switch(Type) {
                case ColliderType.None: {
                    return new Vector2[0];
                }
                case ColliderType.Box: return new[] {
                    transform.Apply(new Vector2(-Width/2, -Height/2)),
                    transform.Apply(new Vector2(-Width/2, Height/2)),
                    transform.Apply(new Vector2(Width/2, Height/2)),
                    transform.Apply(new Vector2(Width/2, -Height/2))
                };
                case ColliderType.Mesh: {
                    Vector2[] vertices = new Vector2[transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty).GetVertexCount()];
                    int i = 0;
                    foreach(Vector2 vertex in transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty).GetVertices()) {
                        vertices[i] = transform.Apply(vertex);
                        i++;
                    }

                    return vertices;
                }
                default: throw new InvalidOperationException();
            }
        }

        public Line[] GetFaces(ref Transform transform) {
            switch(Type) {
                case ColliderType.None: {
                    return new Line[0];
                }
                case ColliderType.Box: {
                    Vector2[] vertices = GetVertices(ref transform);
                    return new[] {
                        new Line(vertices[0], vertices[1]),
                        new Line(vertices[1], vertices[2]),
                        new Line(vertices[2], vertices[3]),
                        new Line(vertices[3], vertices[0]),
                    };
                }
                case ColliderType.Mesh: {
                    Line[] faces = new Line[transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty).GetVertexCount()];
                    int i = 0;
            
                    Vector2 firstVertex = default;
                    Vector2 prevVertex = default;
                    bool first = true;
                    foreach(Vector2 localVertex in transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty).GetVertices()) {
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
                    if(faces.Length > 0) faces[faces.Length-1] = new Line(prevVertex, firstVertex);

                    return faces;
                }
                default: throw new InvalidOperationException();
            }
        }

        public bool Contains(Vector2 point, ref Transform transform) {
            switch(Type) {
                case ColliderType.None: {
                    return false;
                }
                case ColliderType.Box:
                case ColliderType.Mesh: {
                    bool any = false;
                    foreach(Line line in GetFaces(ref transform)) {
                        any = true;
                        Vector2 pointCopy = point;
                        Line.LayFlat(line, ref pointCopy, out _);
                        if(pointCopy.Y > 0) return false;
                    }
                    return any;
                }
                default: throw new InvalidOperationException();
            }
        }

        public Vector2 GetFarthestVertexFromOrigin(ref Transform transform) {
            switch(Type) {
                case ColliderType.None: {
                    return transform.Position;
                }
                case ColliderType.Box: {
                    return transform.Apply(new Vector2(Width / 2, Height / 2));
                }
                case ColliderType.Mesh: {
                    return transform.Apply(transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty).GetFarthestVertexFromOrigin());
                }
                default: throw new InvalidOperationException();
            }
        }

        public float GetReach(ref Transform transform) {
            switch(Type) {
                case ColliderType.None: {
                    return 0;
                }
                case ColliderType.Box: {
                    return (GetFarthestVertexFromOrigin(ref transform) - transform.Position).Length() + 2;
                }
                case ColliderType.Mesh: {
                    return (GetFarthestVertexFromOrigin(ref transform) - transform.Position).Length();
                }
                default: throw new InvalidOperationException();
            }
        }
    }

    public enum ColliderType {
        None,
        Box,
        Mesh
    }
}