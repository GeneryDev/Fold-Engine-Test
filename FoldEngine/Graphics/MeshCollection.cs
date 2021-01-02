using System;
using System.Collections.Generic;
using System.Linq;
using EntryProject.Util;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    public class MeshCollection {
        private const int InitialSize = 256*3;
        
        private VertexPositionColorTexture[] _vertices;
        private int[] _indices;
        
        private readonly Dictionary<string, MeshInfo> _meshInfos = new Dictionary<string, MeshInfo>();

        private string _currentMesh = null;
        private int _nextVertexIndex = 0;
        private int _nextTriangleIndex = 0;

        public MeshCollection() {
            _vertices = new VertexPositionColorTexture[InitialSize];
            _indices = new int[InitialSize];
        }

        public MeshCollection Start(string name, MeshInputType inputType) {
            _meshInfos[name] = new MeshInfo() {VertexStartIndex = _nextVertexIndex, TriangleStartIndex = _nextTriangleIndex, VertexCount = 0, InputType = inputType};
            _currentMesh = name;
            
            return this;
        }

        public MeshCollection Vertex(Vector3 pos, Vector2 uv, Color? color = null) {
            _vertices[_nextVertexIndex] = new VertexPositionColorTexture(pos, color ?? Color.White, uv);
            // _indices[_nextTriangleIndex] = _nextVertexIndex;
            
            MeshInfo meshInfo = _meshInfos[_currentMesh];
            meshInfo.VertexCount++;
            _meshInfos[_currentMesh] = meshInfo;
            
            _nextVertexIndex++;
            
            return this;
        }

        public MeshCollection Vertex(Vector2 pos, Vector2 uv, Color? color = null) {
            _vertices[_nextVertexIndex] = new VertexPositionColorTexture(new Vector3(pos, 0), color ?? Color.White, uv);
            // _indices[_nextTriangleIndex] = _nextVertexIndex;
            
            MeshInfo meshInfo = _meshInfos[_currentMesh];
            meshInfo.VertexCount++;
            _meshInfos[_currentMesh] = meshInfo;
            
            _nextVertexIndex++;
            
            return this;
        }

        public void End() {
            MeshInfo meshInfo = _meshInfos[_currentMesh];
            switch(meshInfo.InputType) {
                case MeshInputType.Triangles:
                    if(meshInfo.VertexCount % 3 != 0) {
                        throw new InvalidOperationException($"Mesh '{_currentMesh}' ended with {_meshInfos[_currentMesh].VertexCount} vertices; should be a multiple of 3.");
                    }
                    for(int i = meshInfo.VertexStartIndex;
                        i < meshInfo.VertexStartIndex + meshInfo.VertexCount - 2;
                        i += 3) {
                        InsertTriangleIndices(i, i+1, i+2, ref meshInfo);
                    }

                    break;
                case MeshInputType.Vertices:
                    //Start triangulation
                    //fan triangulation

                    // for(int i = meshInfo.VertexStartIndex + 1;
                    //     i < meshInfo.VertexStartIndex + meshInfo.VertexCount - 1;
                    //     i++) {
                    //     InsertTriangleIndices(meshInfo.VertexStartIndex, i, i+1, ref meshInfo);
                    // }
                    
                    List<EarClippingNode> nodes = new List<EarClippingNode>();
                    for(int i = meshInfo.VertexStartIndex;
                        i < meshInfo.VertexStartIndex + meshInfo.VertexCount;
                        i++) {
                        nodes.Add(new EarClippingNode(i, _vertices[i].Position));
                    }

                    bool clockwise = true;
                    
                    {
                        int topLeftMostIndex =
                            nodes.OrderBy(n => n.Position.X).ThenBy(n => n.Position.Y).First().VertexIndex - meshInfo.VertexStartIndex;
                        int prevIndex = topLeftMostIndex - 1;
                        int nextIndex = topLeftMostIndex + 1;
                        if(prevIndex < 0) prevIndex = nodes.Count - 1;
                        if(nextIndex >= nodes.Count) nextIndex = 0;

                        Vector3 a = nodes[prevIndex].Position;
                        Vector3 b = nodes[topLeftMostIndex].Position;
                        Vector3 c = nodes[nextIndex].Position;
                        
                        clockwise = (b.X - a.X) * (c.Y - b.Y) - (c.X - b.X) * (b.Y - a.Y) > 0;
                    }

                    Console.WriteLine("Clockwise: " + clockwise);

                    if(!clockwise) {
                        for(int i = 0; i < nodes.Count; i++) {
                            Vector3 mirrored = nodes[i].Position;
                            mirrored.Y *= -1;
                            EarClippingNode node = nodes[i];
                            node.Position = mirrored;
                            nodes[i] = node;
                        }
                    }

                    while(nodes.Count >= 3) {
                        bool anyRemoved = false;
                        for(int i = 0; i < nodes.Count; i++) {
                            int prevIndex = i - 1;
                            int nextIndex = i + 1;
                            if(prevIndex < 0) prevIndex = nodes.Count - 1;
                            if(nextIndex >= nodes.Count) nextIndex = 0;

                            EarClippingNode current = nodes[i];

                            // double dot = Vector3.Dot(nodes[prevIndex].Position - current.Position,
                            //     nodes[nextIndex].Position - current.Position);
                            //
                            // double dotDivMagnitudes =
                            //     dot
                            //     / (Vector3.Distance(nodes[prevIndex].Position, current.Position)
                            //        * Vector3.Distance(nodes[nextIndex].Position, current.Position));

                            // int orientation = 1;

                            Complex nextRotatedByPrevious = ((Complex) (nodes[nextIndex]
                                                                            .Position
                                                                        - current.Position).ToVector2()
                                                             / (Complex) (nodes[prevIndex]
                                                                              .Position
                                                                          - current.Position).ToVector2());
                            
                            if(nextRotatedByPrevious.B <= 0) {
                                if(IsPointInsidePolygon(
                                    (nodes[prevIndex].Position + current.Position + nodes[nextIndex].Position) / 3,
                                    nodes)) {
                                    InsertTriangleIndices(nodes[prevIndex].VertexIndex, nodes[i].VertexIndex, nodes[nextIndex].VertexIndex, ref meshInfo);
                                    nodes.RemoveAt(i);
                                    i--;                                    
                                    anyRemoved = true;
                                }
                            }
                        }

                        if(!anyRemoved) {
                            Console.WriteLine("None removed");
                        }
                    }
                    // InsertTriangleIndices(nodes[0].VertexIndex, nodes[1].VertexIndex, nodes[2].VertexIndex, ref meshInfo);


                    break;
            }

            _meshInfos[_currentMesh] = meshInfo;
            _currentMesh = null;
        }

        private static bool IsPointInsidePolygon(Vector3 point, IReadOnlyList<EarClippingNode> nodes) {
            int hits = 0;
            for(int i = 0; i < nodes.Count; i++) {
                Vector3 a = nodes[i].Position - point;
                Vector3 b = nodes[i+1 < nodes.Count ? i+1 : 0].Position - point;

                if(a.Y < 0 != b.Y < 0) {
                    double xIntersect = (double)a.X + (-(double)a.Y / ((double)b.Y - a.Y)) * ((double)b.X - a.X);
                    if(xIntersect >= 0) {
                        hits++;
                    }
                }
            }

            return hits % 2 != 0;
        }

        private void InsertTriangleIndices(int a, int b, int c, ref MeshInfo meshInfo) {
            _indices[_nextTriangleIndex++] = a;
            _indices[_nextTriangleIndex++] = b;
            _indices[_nextTriangleIndex++] = c;
            meshInfo.TriangleCount++;
        }

        public IEnumerable<VertexPositionColorTexture> GetVertexInfoForMesh(string name) {
            MeshInfo meshInfo = _meshInfos[name];
            for(int i = meshInfo.VertexStartIndex; i < meshInfo.VertexStartIndex + meshInfo.VertexCount; i++) {
                yield return _vertices[i];
            }
        }

        public IEnumerable<Vector2> GetVerticesForMesh(string name) {
            MeshInfo meshInfo = _meshInfos[name];
            for(int i = meshInfo.VertexStartIndex; i < meshInfo.VertexStartIndex + meshInfo.VertexCount; i++) {
                yield return _vertices[i].Position.ToVector2();
            }
        }

        public IEnumerable<Line> GetLinesForMesh(string name) {
            MeshInfo meshInfo = _meshInfos[name];
            for(int i = meshInfo.VertexStartIndex; i < meshInfo.VertexStartIndex + meshInfo.VertexCount; i++) {
                yield return new Line(_vertices[i].Position.ToVector2(),
                    _vertices[
                            i + 1 < meshInfo.VertexStartIndex + meshInfo.VertexCount
                                ? i + 1
                                : meshInfo.VertexStartIndex]
                        .Position.ToVector2());
            }
        }

        public IEnumerable<Tuple<Vector2, Vector2, Vector2>> GetVertexTriosForMesh(string name) {
            MeshInfo meshInfo = _meshInfos[name];
            for(int i = meshInfo.VertexStartIndex; i < meshInfo.VertexStartIndex + meshInfo.VertexCount; i++) {
                yield return new Tuple<Vector2, Vector2, Vector2>(
                    _vertices[
                            i - 1 >= meshInfo.VertexStartIndex
                                ? i - 1
                                : meshInfo.VertexStartIndex + meshInfo.VertexCount - 1]
                        .Position.ToVector2(),
                    _vertices[i].Position.ToVector2(),
                    _vertices[
                            i + 1 < meshInfo.VertexStartIndex + meshInfo.VertexCount
                                ? i + 1
                                : meshInfo.VertexStartIndex]
                        .Position.ToVector2()
                );
            }
        }

        public IEnumerable<Triangle> GetTrianglesForMesh(string name) {
            MeshInfo meshInfo = _meshInfos[name];
            for(int i = meshInfo.TriangleStartIndex; i < meshInfo.TriangleStartIndex + meshInfo.TriangleCount * 3; i += 3) {
                yield return new Triangle(_vertices[_indices[i]], _vertices[_indices[i+1]], _vertices[_indices[i+2]]);
            }
        }

        public int GetVertexCountForMesh(string name) {
            return _meshInfos[name].VertexCount;
        }

        private struct MeshInfo {
            public int VertexStartIndex;
            public int TriangleStartIndex;
            public int VertexCount;
            public int TriangleCount;
            public MeshInputType InputType;
        }

        private struct EarClippingNode {
            public int VertexIndex;
            public Vector3 Position;

            public EarClippingNode(int vertexIndex, Vector3 position) {
                VertexIndex = vertexIndex;
                this.Position = position;
            }
        }

        public enum MeshInputType {
            Vertices, Triangles
        }

        public struct Triangle {
            public VertexPositionColorTexture A;
            public VertexPositionColorTexture B;
            public VertexPositionColorTexture C;

            public Triangle(VertexPositionColorTexture a, VertexPositionColorTexture b, VertexPositionColorTexture c) {
                A = a;
                B = b;
                C = c;
            }
        }
    }
}