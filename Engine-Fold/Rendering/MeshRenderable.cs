using FoldEngine.Components;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Rendering;

[Component("fold:mesh_renderable")]
[ComponentInitializer(typeof(MeshRenderable))]
public struct MeshRenderable
{
    [ResourceIdentifier(typeof(Texture))] public ResourceIdentifier TextureIdentifier;
    [ResourceIdentifier(typeof(Mesh))] public ResourceIdentifier MeshIdentifier;
    [ResourceIdentifier(typeof(Effect))] public ResourceIdentifier EffectIdentifier;
    public Matrix Matrix = Matrix.Identity;
    public Vector2 UVOffset;
    public Vector2 UVScale = Vector2.One;
    public Color Color = Color.White;
    public float ZIndex;

    public MeshRenderable()
    {
    }

    public Line[] GetFaces(ref Transform transform)
    {
        var faces = new Line[transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty).GetVertexCount()];
        int i = 0;

        Vector2 firstVertex = default;
        Vector2 prevVertex = default;
        bool first = true;
        foreach (Vector2 localVertex in transform.Scene.Resources.Get<Mesh>(ref MeshIdentifier, Mesh.Empty)
                     .GetVertices())
        {
            Vector2 vertex = transform.Apply(localVertex.ApplyMatrixTransform(Matrix));
            if (first)
                firstVertex = vertex;
            else
                faces[i - 1] = new Line(prevVertex, vertex);

            first = false;
            prevVertex = vertex;

            i++;
        }

        if (faces.Length > 0) faces[faces.Length - 1] = new Line(prevVertex, firstVertex);

        return faces;
    }

    public bool Contains(Vector2 point, ref Transform transform)
    {
        bool any = false;
        foreach (Line line in GetFaces(ref transform))
        {
            any = true;
            Vector2 pointCopy = point;
            Line.LayFlat(line, ref pointCopy, out _);
            if (pointCopy.Y > 0) return false;
        }

        return any;
    }
}