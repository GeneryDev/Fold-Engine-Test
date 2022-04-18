using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    public class GizmoBatch {
        private readonly GizmoBatcher _batcher;
        private readonly GraphicsDevice _device;
        private bool _beginCalled;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private Effect _effect;
        private RasterizerState _rasterizerState;
        private SamplerState _samplerState;
        private readonly CustomSpriteEffect _spriteEffect;

        public GizmoBatch(GraphicsDevice graphicsDevice) {
            _device = graphicsDevice
                      ?? throw new ArgumentNullException(nameof(graphicsDevice),
                          "The GraphicsDevice must not be null when creating new resources.");
            _spriteEffect = new CustomSpriteEffect(graphicsDevice);
            _batcher = new GizmoBatcher(graphicsDevice);
            _beginCalled = false;
        }

        public bool NeedsHalfPixelOffset { get; set; } = true;

        public ITexture WhiteTexture {
            get => _batcher.WhiteTexture;
            set => _batcher.WhiteTexture = value;
        }

        private void Setup() {
            GraphicsDevice graphicsDevice = _device;
            graphicsDevice.BlendState = _blendState;
            graphicsDevice.DepthStencilState = _depthStencilState;
            graphicsDevice.RasterizerState = _rasterizerState;
            graphicsDevice.SamplerStates[0] = _samplerState;

            _spriteEffect.SetupMatrix();
        }

        public void Begin(
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null) {
            if(_beginCalled)
                throw new InvalidOperationException(
                    "Begin cannot be called again until End has been successfully called.");
            // this._sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;
            if(sortMode == SpriteSortMode.Immediate)
                Setup();
            _beginCalled = true;
        }

        public void End() {
            _beginCalled = _beginCalled
                ? false
                : throw new InvalidOperationException("Begin must be called before calling End.");
            // if (this._sortMode != SpriteSortMode.Immediate)
            Setup();
            _batcher.DrawBatch(_effect ?? _spriteEffect);
        }

        private void CheckValid(Texture2D texture) {
            if(texture == null)
                throw new ArgumentNullException(nameof(texture));
            if(!_beginCalled)
                throw new InvalidOperationException(
                    "Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

        public void DrawLine(Vector2 a, Vector2 b, Color? colorA = null, Color? colorB = null, float zOrder = 0) {
            colorA = colorA ?? Color.White;
            colorB = colorB ?? colorA;

            ref GizmoBatchItem item = ref _batcher.CreateBatchItem();

            item.SortKey = zOrder;

            item.VertexA.Position = new Vector3(a, 0);
            item.VertexB.Position = new Vector3(b, 0);

            item.VertexA.Color = colorA.Value;
            item.VertexB.Color = colorB.Value;

            item.VertexA.TextureCoordinate = WhiteTexture?.ToSourceUV(Vector2.Zero) ?? Vector2.Zero;
            item.VertexB.TextureCoordinate = WhiteTexture?.ToSourceUV(Vector2.One) ?? Vector2.One;
        }
    }

    public class GizmoBatcher {
        private const int InitialBatchSize = 256;
        private const int MaxBatchSize = 5461;
        private const int InitialVertexArraySize = 256;
        private readonly GraphicsDevice _device;
        private int _batchItemCount;

        private GizmoBatchItem[] _batchItemList;
        private short[] _index;
        private VertexPositionColorTexture[] _vertexArray;

        public GizmoBatcher(GraphicsDevice device) {
            _device = device;
            _batchItemList = new GizmoBatchItem[InitialBatchSize];
            _batchItemCount = 0;
            for(int index = 0; index < InitialBatchSize; ++index)
                _batchItemList[index] = new GizmoBatchItem();
            EnsureArrayCapacity(InitialBatchSize);
        }

        public ITexture WhiteTexture { get; set; }

        public ref GizmoBatchItem CreateBatchItem() {
            if(_batchItemCount >= _batchItemList.Length) {
                Console.WriteLine("Resized batch item list");
                int length = _batchItemList.Length;
                int newLength = (length + length / 2 + 63) & -64;
                Array.Resize(ref _batchItemList, newLength);
                EnsureArrayCapacity(Math.Min(newLength, MaxBatchSize));
            }

            return ref _batchItemList[_batchItemCount++];
        }

        private void EnsureArrayCapacity(int numBatchItems) {
            int indexCount = 2 * numBatchItems;
            if(_index != null && indexCount <= _index.Length)
                return;
            var newIndexArray = new short[indexCount];
            int batchIndex = 0;
            if(_index != null) {
                _index.CopyTo(newIndexArray, 0);
                batchIndex = _index.Length / 2;
            }

            for(int i = batchIndex; i < numBatchItems; i++) {
                int vertexArrayIndex = (short) (i * 2);
                newIndexArray[i * 2] = (short) vertexArrayIndex;
                newIndexArray[i * 2 + 1] = (short) (vertexArrayIndex + 1);
            }

            _index = newIndexArray;
            _vertexArray = new VertexPositionColorTexture[2 * numBatchItems];
        }

        public void DrawBatch(Effect effect) {
            if(effect != null && effect.IsDisposed)
                throw new ObjectDisposedException(nameof(effect));
            if(_batchItemCount == 0)
                return;
            if(WhiteTexture == null) throw new Exception("Cannot draw GizmoBatch without a texture");
            Array.Sort(_batchItemList, 0, _batchItemCount);

            int batchedThisIteration = 0;
            int vertexIndex = 0;
            for(int batchIndex = 0; batchIndex < _batchItemCount; batchIndex++) {
                GizmoBatchItem item = _batchItemList[batchIndex];

                _vertexArray[vertexIndex] = item.VertexA;
                _vertexArray[vertexIndex + 1] = item.VertexB;
                vertexIndex += 2;

                batchedThisIteration++;
                if(batchedThisIteration > MaxBatchSize) {
                    FlushVertexArray(batchedThisIteration * 2, effect);
                    batchedThisIteration = 0;
                    vertexIndex = 0;
                }
            }

            if(batchedThisIteration > 0) {
                FlushVertexArray(batchedThisIteration * 2, effect);
                batchedThisIteration = 0;
                vertexIndex = 0;
            }

            _batchItemCount = 0;
        }

        private void FlushVertexArray(int numVertices, Effect effect) {
            var primitiveType = PrimitiveType.LineList;
            int primitiveCount = numVertices / 2;

            if(numVertices <= 0)
                return;
            
            foreach(EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                _device.Textures[0] = WhiteTexture.Source;
                _device.DrawUserIndexedPrimitives(primitiveType, _vertexArray, 0, numVertices, _index, 0,
                    primitiveCount, VertexPositionColorTexture.VertexDeclaration);
            }
        }
    }

    public struct GizmoBatchItem : IComparable<GizmoBatchItem> {
        public float SortKey;
        public VertexPositionColorTexture VertexA;
        public VertexPositionColorTexture VertexB;

        public int CompareTo(GizmoBatchItem other) {
            return SortKey.CompareTo(other.SortKey);
        }
    }
}