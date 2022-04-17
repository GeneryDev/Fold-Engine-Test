using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    public class TriangleBatch {
        private readonly TriangleBatcher _batcher;
        private readonly GraphicsDevice _device;
        private readonly EffectParameter _matrixTransform;
        private readonly EffectPass _spritePass;
        private bool _beginCalled;
        private BatcherParams _activeParams;
        
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private Effect _effect;
        private Viewport _lastViewport;
        private Matrix? _matrix;
        private Matrix _projection;
        private RasterizerState _rasterizerState;
        private SamplerState _samplerState;
        private readonly Effect _spriteEffect;

        public TriangleBatch(GraphicsDevice graphicsDevice) {
            _device = graphicsDevice
                      ?? throw new ArgumentNullException(nameof(graphicsDevice),
                          "The GraphicsDevice must not be null when creating new resources.");
            _spriteEffect = new CustomSpriteEffect(graphicsDevice);
            _matrixTransform = _spriteEffect.Parameters["MatrixTransform"];
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];
            _batcher = new TriangleBatcher(graphicsDevice);
            _beginCalled = false;
        }

        public bool NeedsHalfPixelOffset { get; set; } = true;

        private void SetupMatrix(Viewport viewport) {
            Matrix.CreateOrthographicOffCenter(0.0f, viewport.Width, viewport.Height, 0.0f, 0.0f, -100f,
                out _projection);
            if(NeedsHalfPixelOffset) {
                _projection.M41 += -0.5f * _projection.M11;
                _projection.M42 += -0.5f * _projection.M22;
            }
        }

        private void Setup() {
            GraphicsDevice graphicsDevice = _device;
            graphicsDevice.BlendState = _blendState;
            graphicsDevice.DepthStencilState = _depthStencilState;
            graphicsDevice.RasterizerState = _rasterizerState;
            graphicsDevice.SamplerStates[0] = _samplerState;

            Viewport viewport = graphicsDevice.Viewport;
            if(viewport.Width != _lastViewport.Width || viewport.Height != _lastViewport.Height) {
                SetupMatrix(viewport);
                _lastViewport = viewport;
            }

            if(_matrix.HasValue)
                _matrixTransform.SetValue(_matrix.GetValueOrDefault() * _projection);
            else
                _matrixTransform.SetValue(_projection);
            _spritePass.Apply();
        }

        public void QuickBegin(
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null) {
            BatcherParams param = new BatcherParams(blendState, samplerState, depthStencilState, rasterizerState, effect);
            if(_beginCalled && param == _activeParams) return;
            End();
            _activeParams = param;
            if(_beginCalled)
                throw new InvalidOperationException(
                    "Begin cannot be called again until End has been successfully called.");
            
            // this._sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;
            // _matrix = transformMatrix;
            // if(sortMode == SpriteSortMode.Immediate)
                // Setup();
            _beginCalled = true;
        }

        public void End() {
            if(!_beginCalled) return;
            _beginCalled = _beginCalled
                ? false
                : throw new InvalidOperationException("Begin must be called before calling End.");
            // if (this._sortMode != SpriteSortMode.Immediate)
            Setup();
            _batcher.DrawBatch(_effect);
        }

        private void CheckValid(Texture2D texture) {
            if(texture == null)
                throw new ArgumentNullException(nameof(texture));
            if(!_beginCalled)
                throw new InvalidOperationException(
                    "Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

        public void DrawTriangle(
            Texture2D texture,
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 texA,
            Vector2 texB,
            Vector2 texC,
            Color? colorA = null,
            Color? colorB = null,
            Color? colorC = null) {
            DrawTriangle(texture, new Vector3(a, 0), new Vector3(b, 0), new Vector3(c, 0), texA, texB, texC, colorA,
                colorB, colorC);
        }

        public void DrawTriangle(
            Texture2D texture,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector2 texA,
            Vector2 texB,
            Vector2 texC,
            Color? colorA = null,
            Color? colorB = null,
            Color? colorC = null) {
            CheckValid(texture);

            colorA = colorA ?? Color.White;
            colorB = colorB ?? colorA;
            colorC = colorC ?? colorA;

            ref TriangleBatchItem item = ref _batcher.CreateBatchItem();

            item.Texture = texture;
            item.VertexA.Color = colorA.Value;
            item.VertexB.Color = colorB.Value;
            item.VertexC.Color = colorC.Value;

            item.VertexA.Position = a;
            item.VertexB.Position = b;
            item.VertexC.Position = c;

            item.VertexA.TextureCoordinate = texA;
            item.VertexB.TextureCoordinate = texB;
            item.VertexC.TextureCoordinate = texC;
        }

        public void DrawQuad(
            Texture2D texture,
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d,
            Vector2 texA,
            Vector2 texB,
            Vector2 texC,
            Vector2 texD,
            Color? colorA = null,
            Color? colorB = null,
            Color? colorC = null,
            Color? colorD = null) {
            DrawQuad(texture, new Vector3(a, 0), new Vector3(b, 0), new Vector3(c, 0), new Vector3(d, 0), texA, texB,
                texC, texC, colorA, colorB, colorC, colorD);
        }

        public void DrawQuad(
            Texture2D texture,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            Vector2 texA,
            Vector2 texB,
            Vector2 texC,
            Vector2 texD,
            Color? colorA = null,
            Color? colorB = null,
            Color? colorC = null,
            Color? colorD = null) {
            CheckValid(texture);

            colorA = colorA ?? Color.White;
            colorB = colorB ?? colorA;
            colorC = colorC ?? colorA;
            colorD = colorD ?? colorA;

            ref TriangleBatchItem item0 = ref _batcher.CreateBatchItem();
            ref TriangleBatchItem item1 = ref _batcher.CreateBatchItem();

            item0.Texture = texture;
            item0.VertexA.Color = colorA.Value;
            item0.VertexB.Color = colorB.Value;
            item0.VertexC.Color = colorC.Value;

            item0.VertexA.Position = a;
            item0.VertexB.Position = b;
            item0.VertexC.Position = c;

            item0.VertexA.TextureCoordinate = texA;
            item0.VertexB.TextureCoordinate = texB;
            item0.VertexC.TextureCoordinate = texC;

            item1.Texture = texture;
            item1.VertexA.Color = colorB.Value;
            item1.VertexB.Color = colorD.Value;
            item1.VertexC.Color = colorC.Value;

            item1.VertexA.Position = b;
            item1.VertexB.Position = d;
            item1.VertexC.Position = c;

            item1.VertexA.TextureCoordinate = texB;
            item1.VertexB.TextureCoordinate = texD;
            item1.VertexC.TextureCoordinate = texC;
        }

        public void Clear() {
            _batcher.Clear();
        }
    }

    public class TriangleBatcher {
        private const int InitialBatchSize = 256;
        private const int MaxBatchSize = short.MaxValue / 6;
        private const int InitialVertexArraySize = 256;
        private readonly GraphicsDevice _device;
        private int _batchItemCount;

        private TriangleBatchItem[] _batchItemList;
        private short[] _index;
        private VertexPositionColorTexture[] _vertexArray;

        private readonly bool WireframeMode = false;

        public TriangleBatcher(GraphicsDevice device) {
            _device = device;
            _batchItemList = new TriangleBatchItem[InitialBatchSize];
            _batchItemCount = 0;
            EnsureArrayCapacity(InitialBatchSize);
        }

        public ref TriangleBatchItem CreateBatchItem() {
            if(_batchItemCount >= _batchItemList.Length) {
                Console.WriteLine("Resized TriangleBatchItem list");
                int length = _batchItemList.Length;
                int newLength = (length + length / 2 + 63) & -64;
                Array.Resize(ref _batchItemList, newLength);
                EnsureArrayCapacity(Math.Min(newLength, MaxBatchSize));
            }

            return ref _batchItemList[_batchItemCount++];
        }

        private void EnsureArrayCapacity(int numBatchItems) {
            int indexCount = 3 * numBatchItems;
            if(_index != null && indexCount <= _index.Length)
                return;
            var newIndexArray = new short[indexCount];
            int batchIndex = 0;
            if(_index != null) {
                _index.CopyTo(newIndexArray, 0);
                batchIndex = _index.Length / 3;
            }

            for(int i = batchIndex; i < numBatchItems; i++) {
                int vertexArrayIndex = (short) (i * 3);
                newIndexArray[i * 3] = (short) vertexArrayIndex;
                newIndexArray[i * 3 + 1] = (short) (vertexArrayIndex + 1);
                newIndexArray[i * 3 + 2] = (short) (vertexArrayIndex + 2);
            }

            _index = newIndexArray;
            _vertexArray = new VertexPositionColorTexture[3 * numBatchItems];
        }

        public void DrawBatch(Effect effect) {
            if(effect != null && effect.IsDisposed)
                throw new ObjectDisposedException(nameof(effect));
            if(_batchItemCount == 0)
                return;


            int batchedThisIteration = 0;
            int vertexIndex = 0;
            Texture2D texture = null;
            for(int batchIndex = 0; batchIndex < _batchItemCount; batchIndex++) {
                TriangleBatchItem item = _batchItemList[batchIndex];

                if(texture != null && !ReferenceEquals(item.Texture, texture)) {
                    FlushVertexArray(batchedThisIteration * (WireframeMode ? 4 : 3), effect, texture);
                    batchedThisIteration = 0;
                    vertexIndex = 0;
                }

                texture = item.Texture;

                _vertexArray[vertexIndex] = item.VertexA;
                _vertexArray[vertexIndex + 1] = item.VertexB;
                _vertexArray[vertexIndex + 2] = item.VertexC;
                if(WireframeMode) _vertexArray[vertexIndex + 3] = item.VertexA;
                vertexIndex += WireframeMode ? 4 : 3;

                batchedThisIteration++;
                if(batchedThisIteration >= MaxBatchSize) {
                    FlushVertexArray(batchedThisIteration * (WireframeMode ? 4 : 3), effect, texture);
                    batchedThisIteration = 0;
                    vertexIndex = 0;
                }
            }

            if(batchedThisIteration > 0) {
                FlushVertexArray(batchedThisIteration * (WireframeMode ? 4 : 3), effect, texture);
                batchedThisIteration = 0;
                vertexIndex = 0;
            }

            _batchItemCount = 0;
        }

        private void FlushVertexArray(
            int numVertices,
            Effect effect,
            Microsoft.Xna.Framework.Graphics.Texture texture) {
            var primitiveType = PrimitiveType.TriangleList;
            int primitiveCount = numVertices / 3;

            if(WireframeMode) {
                primitiveType = PrimitiveType.LineStrip;
                primitiveCount = numVertices - 1;
            }

            if(numVertices <= 0)
                return;
            _device.Textures[0] = texture;
            if(effect != null)
                foreach(EffectPass pass in effect.CurrentTechnique.Passes) {
                    pass.Apply();
                    _device.DrawUserIndexedPrimitives(primitiveType, _vertexArray, 0, numVertices, _index, 0,
                        primitiveCount, VertexPositionColorTexture.VertexDeclaration);
                }
            else
                _device.DrawUserIndexedPrimitives(primitiveType, _vertexArray, 0, numVertices, _index, 0,
                    primitiveCount, VertexPositionColorTexture.VertexDeclaration);
        }

        public void Clear() {
            _batchItemCount = 0;
        }
    }

    public struct TriangleBatchItem {
        public Texture2D Texture;
        public VertexPositionColorTexture VertexA;
        public VertexPositionColorTexture VertexB;
        public VertexPositionColorTexture VertexC;
    }

    public class CustomSpriteEffect : SpriteEffect {
        public CustomSpriteEffect(GraphicsDevice device) : base(device) { }
        protected CustomSpriteEffect(SpriteEffect cloneSource) : base(cloneSource) { }

        protected override void OnApply() { }
    }

    internal struct BatcherParams {
        public BlendState BlendState;
        public SamplerState SamplerState;
        public DepthStencilState DepthStencilState;
        public RasterizerState RasterizerState;
        public Effect Effect;

        public BatcherParams(BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect) {
            BlendState = blendState;
            SamplerState = samplerState;
            DepthStencilState = depthStencilState;
            RasterizerState = rasterizerState;
            Effect = effect;
        }

        public bool Equals(BatcherParams other) {
            return Equals(BlendState, other.BlendState)
                   && Equals(SamplerState, other.SamplerState)
                   && Equals(DepthStencilState, other.DepthStencilState)
                   && Equals(RasterizerState, other.RasterizerState)
                   && Equals(Effect, other.Effect);
        }

        public override bool Equals(object obj) {
            return obj is BatcherParams other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (BlendState != null ? BlendState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SamplerState != null ? SamplerState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DepthStencilState != null ? DepthStencilState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RasterizerState != null ? RasterizerState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Effect != null ? Effect.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(BatcherParams a, BatcherParams b) {
            return a.Equals(b);
        }

        public static bool operator !=(BatcherParams a, BatcherParams b) {
            return !(a == b);
        }
    }
}