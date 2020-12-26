using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    public class MeshCollection {
        private const int InitialSize = 256*3;
        
        private VertexPositionColorTexture[] _vertices;
        private Dictionary<string, MeshInfo> _meshInfos = new Dictionary<string, MeshInfo>();
        private string _currentMesh = null;
        private int _nextIndex = 0;

        public MeshCollection() {
            _vertices = new VertexPositionColorTexture[InitialSize];
        }

        public MeshCollection Start(string name) {
            _meshInfos[name] = new MeshInfo() {Index = _nextIndex, VertexCount = 0};
            _currentMesh = name;
            
            return this;
        }

        public MeshCollection Vertex(Vector3 pos, Vector2 uv, Color? color = null) {
            _vertices[_nextIndex] = new VertexPositionColorTexture(pos, color ?? Color.White, uv);
            
            MeshInfo meshInfo = _meshInfos[_currentMesh];
            meshInfo.VertexCount++;
            _meshInfos[_currentMesh] = meshInfo;
            
            _nextIndex++;
            
            return this;
        }

        public MeshCollection Vertex(Vector2 pos, Vector2 uv, Color? color = null) {
            _vertices[_nextIndex] = new VertexPositionColorTexture(new Vector3(pos, 0), color ?? Color.White, uv);
            
            MeshInfo meshInfo = _meshInfos[_currentMesh];
            meshInfo.VertexCount++;
            _meshInfos[_currentMesh] = meshInfo;
            
            _nextIndex++;
            
            return this;
        }

        public void End() {
            if(_meshInfos[_currentMesh].VertexCount % 3 != 0) {
                throw new InvalidOperationException($"Mesh '{_currentMesh}' ended with {_meshInfos[_currentMesh].VertexCount} vertices; should be a multiple of 3.");
            }
            _currentMesh = null;
        }

        public IEnumerable<VertexPositionColorTexture> GetVerticesForMesh(string name) {
            MeshInfo meshInfo = _meshInfos[name];
            for(int i = meshInfo.Index; i < meshInfo.Index + meshInfo.VertexCount; i++) {
                yield return _vertices[i];
            }
        }

        public IEnumerable<Triangle> GetTrianglesForMesh(string name) {
            MeshInfo meshInfo = _meshInfos[name];
            for(int i = meshInfo.Index; i < meshInfo.Index + meshInfo.VertexCount - 2; i += 3) {
                yield return new Triangle(_vertices[i], _vertices[i+1], _vertices[i+2]);
            }
        }

        private struct MeshInfo {
            public int Index;
            public int VertexCount;
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