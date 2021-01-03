using System;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Physics {
    public interface ICollider {
        bool ThickFaces { get; }
        Vector2[] GetVertices(ref Transform transform);
        Line[] GetFaces(ref Transform transform);
        bool Contains(Vector2 point, ref Transform transform);
        Vector2 GetFarthestVertexFromOrigin(ref Transform transform);
        float GetReach(ref Transform transform);
    }

    [Component("fold:physics.box_collider")]
    [ComponentInitializer(typeof(BoxCollider), nameof(InitializeComponent))]
    public struct BoxCollider : ICollider {
        public float Width;
        public float Height;

        public bool ThickFaces { get; set; }

        public static BoxCollider InitializeComponent(Scene scene, long entityId) {
            return new BoxCollider() {
                Width = 1,
                Height = 1
            };
        }

        public Vector2[] GetVertices(ref Transform transform) {
            return new[] {
                transform.Apply(new Vector2(-Width/2, -Height/2)),
                transform.Apply(new Vector2(-Width/2, Height/2)),
                transform.Apply(new Vector2(Width/2, Height/2)),
                transform.Apply(new Vector2(Width/2, -Height/2))
            };
        }

        public Line[] GetFaces(ref Transform transform) {
            Vector2[] vertices = GetVertices(ref transform);
            return new[] {
                new Line(vertices[0], vertices[1]),
                new Line(vertices[1], vertices[2]),
                new Line(vertices[2], vertices[3]),
                new Line(vertices[3], vertices[0]),
            };
        }

        public bool Contains(Vector2 point, ref Transform transform) {
            foreach(Line line in GetFaces(ref transform)) {
                Vector2 pointCopy = point;
                Line.LayFlat(line, ref pointCopy, out _);
                if(pointCopy.Y > 0) return false;
            }
            return true;
        }

        public Vector2 GetFarthestVertexFromOrigin(ref Transform transform) {
            return transform.Apply(new Vector2(Width / 2, Height / 2));
        }

        public float GetReach(ref Transform transform) {
            return (GetFarthestVertexFromOrigin(ref transform) - transform.Position).Length() + 2;
        }
    }
}