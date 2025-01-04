using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FoldEngine.Serialization;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace FoldEngine.Resources;

[Resource("mesh")]
public class Mesh : Resource, ISelfSerializer
{
    public enum MeshInputType
    {
        Vertices,
        Triangles
    }

    private const int InitialSize = 256;

    public static readonly Mesh Empty = new Mesh(0) { Identifier = "__EMPTY" };
    private Vector2 _farthestVertexFromOrigin;

    private MeshInputType _inputType;
    private int _nextTriangleIndex;
    private int _nextVertexIndex;
    private float _radiusSquared;
    private int _triangleCount;
    private int _vertexCount;
    public int[] Indices;

    public MeshVertex[] Vertices;

    public Mesh(int size)
    {
        Vertices = new MeshVertex[size];
        Indices = new int[size];
    }

    public Mesh()
    {
    }

    public void Serialize(SaveOperation writer)
    {
        writer.WriteCompound((ref SaveOperation.Compound c) =>
        {
            c.WriteMember("Vertices", () => writer.WriteArray((ref SaveOperation.Array arr) =>
            {
                for (int i = 0; i < _vertexCount; i++) arr.WriteMember(Vertices[i]);
            }));
            c.WriteMember("Indices", () => writer.WriteArray((ref SaveOperation.Array arr) =>
            {
                for (int i = 0; i < _triangleCount * 3; i++) arr.WriteMember(Indices[i]);
            }));
        });
    }

    public void Deserialize(LoadOperation reader)
    {
        reader.ReadCompound(m =>
        {
            switch (m.Name)
            {
                case "Vertices":
                    reader.ReadArray(a =>
                        {
                            Vertices = new MeshVertex[a.MemberCount];
                            _vertexCount = Vertices.Length;
                        },
                        elem =>
                        {
                            var vertex = GenericSerializer.Deserialize(new MeshVertex(), reader);
                            Vertices[elem.Index] = vertex;
                            ProcessVertex(vertex);
                        }
                    );
                    break;
                case "Indices":
                    reader.ReadArray(a =>
                        {
                            Indices = new int[a.MemberCount];
                            _triangleCount = Indices.Length / 3;
                        },
                        elem =>
                        {
                            Indices[elem.Index] = reader.ReadInt32();
                        }
                    );
                    break;
                default:
                    m.Skip();
                    break;
            }
        });
    }

    private void ProcessVertex(MeshVertex vertex)
    {
        float distanceSquared = vertex.Position.LengthSquared();
        if (_radiusSquared < distanceSquared)
        {
            _radiusSquared = distanceSquared;
            _farthestVertexFromOrigin = vertex.Position.ToVector2();
        }
    }

    public Mesh Start(MeshInputType inputType)
    {
        Vertices = new MeshVertex[InitialSize];
        Indices = new int[InitialSize];

        _inputType = inputType;
        _nextVertexIndex = _nextTriangleIndex = 0;
        _vertexCount = 0;
        _triangleCount = 0;

        return this;
    }

    public Mesh Vertex(Vector3 pos, Vector2 uv, Color? color = null)
    {
        return Vertex(new MeshVertex(pos, color ?? Color.White, uv));
    }

    public Mesh Vertex(Vector3 pos, float friction = 1f, float restitution = 0f, bool enabled = true)
    {
        return Vertex(new MeshVertex
        {
            Position = pos,
            Friction = friction,
            Restitution = restitution,
            Enabled = enabled
        });
    }

    public Mesh Vertex(Vector2 pos, Vector2 uv, Color? color = null)
    {
        return Vertex(new Vector3(pos, 0), uv, color);
    }

    public Mesh Vertex(Vector2 pos, float friction = 1f, float restitution = 0f, bool enabled = true)
    {
        return Vertex(new Vector3(pos, 0), friction, restitution, enabled);
    }

    public Mesh Vertex(MeshVertex vertex)
    {
        Vertices[_nextVertexIndex] = vertex;

        _vertexCount++;
        ProcessVertex(vertex);

        _nextVertexIndex++;

        return this;
    }

    public Mesh End()
    {
        switch (_inputType)
        {
            case MeshInputType.Triangles:
                if (_vertexCount % 3 != 0)
                    throw new InvalidOperationException(
                        $"Mesh '_name' ended with {_vertexCount} vertices; should be a multiple of 3.");
                for (int i = 0;
                     i < _vertexCount - 2;
                     i += 3)
                    InsertTriangleIndices(i, i + 1, i + 2);

                break;
            case MeshInputType.Vertices:
                //Start triangulation
                //fan triangulation

                // for(int i = meshInfo.VertexStartIndex + 1;
                //     i < meshInfo.VertexStartIndex + meshInfo.VertexCount - 1;
                //     i++) {
                //     InsertTriangleIndices(meshInfo.VertexStartIndex, i, i+1, ref meshInfo);
                // }

                var nodes = new List<EarClippingNode>();
                for (int i = 0;
                     i < _vertexCount;
                     i++)
                    nodes.Add(new EarClippingNode(i, Vertices[i].Position));

                bool clockwise = true;
            {
                int topLeftMostIndex =
                    nodes.OrderBy(n => n.Position.X).ThenBy(n => n.Position.Y).First().VertexIndex;
                int prevIndex = topLeftMostIndex - 1;
                int nextIndex = topLeftMostIndex + 1;
                if (prevIndex < 0) prevIndex = nodes.Count - 1;
                if (nextIndex >= nodes.Count) nextIndex = 0;

                Vector3 a = nodes[prevIndex].Position;
                Vector3 b = nodes[topLeftMostIndex].Position;
                Vector3 c = nodes[nextIndex].Position;

                clockwise = (b.X - a.X) * (c.Y - b.Y) - (c.X - b.X) * (b.Y - a.Y) > 0;
            }

                Console.WriteLine("Clockwise: " + clockwise);

                if (!clockwise)
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        Vector3 mirrored = nodes[i].Position;
                        mirrored.Y *= -1;
                        EarClippingNode node = nodes[i];
                        node.Position = mirrored;
                        nodes[i] = node;
                    }

                while (nodes.Count >= 3)
                {
                    bool anyRemoved = false;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        int prevIndex = i - 1;
                        int nextIndex = i + 1;
                        if (prevIndex < 0) prevIndex = nodes.Count - 1;
                        if (nextIndex >= nodes.Count) nextIndex = 0;

                        EarClippingNode current = nodes[i];

                        // double dot = Vector3.Dot(nodes[prevIndex].Position - current.Position,
                        //     nodes[nextIndex].Position - current.Position);
                        //
                        // double dotDivMagnitudes =
                        //     dot
                        //     / (Vector3.Distance(nodes[prevIndex].Position, current.Position)
                        //        * Vector3.Distance(nodes[nextIndex].Position, current.Position));

                        // int orientation = 1;

                        Complex nextRotatedByPrevious = (Complex)(nodes[nextIndex]
                                                                      .Position
                                                                  - current.Position).ToVector2()
                                                        / (Complex)(nodes[prevIndex]
                                                                        .Position
                                                                    - current.Position).ToVector2();

                        if (nextRotatedByPrevious.B <= 0)
                            if (IsPointInsidePolygon(
                                    (nodes[prevIndex].Position + current.Position + nodes[nextIndex].Position) / 3,
                                    nodes))
                            {
                                InsertTriangleIndices(nodes[prevIndex].VertexIndex, nodes[i].VertexIndex,
                                    nodes[nextIndex].VertexIndex);
                                nodes.RemoveAt(i);
                                i--;
                                anyRemoved = true;
                            }
                    }

                    if (!anyRemoved) Console.WriteLine("None removed");
                }
                // InsertTriangleIndices(nodes[0].VertexIndex, nodes[1].VertexIndex, nodes[2].VertexIndex, ref meshInfo);


                break;
        }

        return this;
    }

    private static bool IsPointInsidePolygon(Vector3 point, IReadOnlyList<EarClippingNode> nodes)
    {
        int hits = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 a = nodes[i].Position - point;
            Vector3 b = nodes[i + 1 < nodes.Count ? i + 1 : 0].Position - point;

            if (a.Y < 0 != b.Y < 0)
            {
                double xIntersect = a.X + -(double)a.Y / ((double)b.Y - a.Y) * ((double)b.X - a.X);
                if (xIntersect >= 0) hits++;
            }
        }

        return hits % 2 != 0;
    }

    private void InsertTriangleIndices(int a, int b, int c)
    {
        Indices[_nextTriangleIndex++] = a;
        Indices[_nextTriangleIndex++] = b;
        Indices[_nextTriangleIndex++] = c;
        _triangleCount++;
    }

    public IEnumerable<MeshVertex> GetVertexInfo()
    {
        for (int i = 0; i < _vertexCount; i++) yield return Vertices[i];
    }

    public IEnumerable<Vector2> GetVertices()
    {
        for (int i = 0; i < _vertexCount; i++) yield return Vertices[i].Position.ToVector2();
    }

    public IEnumerable<Line> GetLines()
    {
        for (int i = 0; i < _vertexCount; i++)
            yield return new Line(Vertices[i].Position.ToVector2(),
                Vertices[
                        i + 1 < _vertexCount
                            ? i + 1
                            : 0]
                    .Position.ToVector2());
    }

    public IEnumerable<Tuple<Vector2, Vector2, Vector2>> GetVertexTrios()
    {
        for (int i = 0; i < _vertexCount; i++)
            yield return new Tuple<Vector2, Vector2, Vector2>(
                Vertices[
                        i - 1 >= 0
                            ? i - 1
                            : _vertexCount - 1]
                    .Position.ToVector2(),
                Vertices[i].Position.ToVector2(),
                Vertices[
                        i + 1 < _vertexCount
                            ? i + 1
                            : 0]
                    .Position.ToVector2()
            );
    }

    public TriangleEnumerator GetTriangles()
    {
        return new TriangleEnumerator(this);
    }

    public int GetVertexCount()
    {
        return _vertexCount;
    }

    public float GetRadiusSquared()
    {
        return _radiusSquared;
    }

    public float GetRadius()
    {
        return (float)Math.Sqrt(_radiusSquared);
    }

    public Vector2 GetFarthestVertexFromOrigin()
    {
        return _farthestVertexFromOrigin;
    }

    public struct TriangleEnumerator
    {
        private int i;
        private readonly Mesh mesh;

        internal TriangleEnumerator(Mesh mesh)
        {
            this.mesh = mesh;
            i = -3;
        }

        public bool MoveNext()
        {
            if (mesh._triangleCount == 0) return false;
            i += 3;
            return i < mesh._triangleCount * 3;
        }

        public void Reset()
        {
            i = 0;
        }

        public Triangle Current => new Triangle(mesh.Vertices[mesh.Indices[i]],
            mesh.Vertices[mesh.Indices[i + 1]],
            mesh.Vertices[mesh.Indices[i + 2]]);


        public TriangleEnumerator GetEnumerator()
        {
            return this;
        }
    }

    private struct EarClippingNode
    {
        public readonly int VertexIndex;
        public Vector3 Position;

        public EarClippingNode(int vertexIndex, Vector3 position)
        {
            VertexIndex = vertexIndex;
            Position = position;
        }
    }

    public struct Triangle
    {
        public MeshVertex A;
        public MeshVertex B;
        public MeshVertex C;

        public Triangle(MeshVertex a, MeshVertex b, MeshVertex c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    [GenericSerializable]
    public struct MeshVertex
    {
        public Vector3 Position;
        public bool Enabled;

        public Color Color;
        public Vector2 TextureCoordinate;

        public float Friction;
        public float Restitution;

        public MeshVertex(Vector3 pos, Color color, Vector2 uv)
        {
            Position = pos;
            Color = color;
            TextureCoordinate = uv;

            Enabled = true;
            Friction = 1;
            Restitution = 0;
        }
    }
}