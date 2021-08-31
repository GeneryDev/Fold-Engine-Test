using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    public class GizmoBatch {
        private readonly GraphicsDevice _device;
        private readonly GizmoBatcher _batcher;
        private BlendState _blendState;
        private SamplerState _samplerState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;
        private Effect _effect;
        private bool _beginCalled;
        private Effect _spriteEffect;
        private readonly EffectParameter _matrixTransform;
        private readonly EffectPass _spritePass;
        private Matrix? _matrix;
        private Viewport _lastViewport;
        private Matrix _projection;
        private Rectangle _tempRect = new Rectangle(0, 0, 0, 0);
        public bool NeedsHalfPixelOffset { get; set; } = true;

        public ITexture WhiteTexture {
            get => _batcher.WhiteTexture;
            set => _batcher.WhiteTexture = value;
        }
        
        public GizmoBatch(GraphicsDevice graphicsDevice)
        {
            this._device = graphicsDevice ?? throw new ArgumentNullException(nameof (graphicsDevice), "The GraphicsDevice must not be null when creating new resources.");
            this._spriteEffect = new CustomSpriteEffect(graphicsDevice);
            this._matrixTransform = this._spriteEffect.Parameters["MatrixTransform"];
            this._spritePass = this._spriteEffect.CurrentTechnique.Passes[0];
            this._batcher = new GizmoBatcher(graphicsDevice);
            this._beginCalled = false;
        }

        private void SetupMatrix(Viewport viewport) {
            Matrix.CreateOrthographicOffCenter(0.0f, viewport.Width, viewport.Height, 0.0f, 0.0f, -100f, out this._projection);
            if (NeedsHalfPixelOffset)
            {
                this._projection.M41 += -0.5f * this._projection.M11;
                this._projection.M42 += -0.5f * this._projection.M22;
            }
        }
        
        private void Setup()
        {
            GraphicsDevice graphicsDevice = this._device;
            graphicsDevice.BlendState = this._blendState;
            graphicsDevice.DepthStencilState = this._depthStencilState;
            graphicsDevice.RasterizerState = this._rasterizerState;
            graphicsDevice.SamplerStates[0] = this._samplerState;
            
            Viewport viewport = graphicsDevice.Viewport;
            if (viewport.Width != this._lastViewport.Width || viewport.Height != this._lastViewport.Height)
            {
                SetupMatrix(viewport);
                this._lastViewport = viewport;
            }
            if (this._matrix.HasValue)
                this._matrixTransform.SetValue(this._matrix.GetValueOrDefault() * this._projection);
            else
                this._matrixTransform.SetValue(this._projection);
            this._spritePass.Apply();
        }

        public void Begin(
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null)
        {
            if (this._beginCalled)
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
            // this._sortMode = sortMode;
            this._blendState = blendState ?? BlendState.AlphaBlend;
            this._samplerState = samplerState ?? SamplerState.LinearClamp;
            this._depthStencilState = depthStencilState ?? DepthStencilState.None;
            this._rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            this._effect = effect;
            this._matrix = transformMatrix;
            if (sortMode == SpriteSortMode.Immediate)
                this.Setup();
            this._beginCalled = true;
        }

        public void End()
        {
            this._beginCalled = this._beginCalled ? false : throw new InvalidOperationException("Begin must be called before calling End.");
            // if (this._sortMode != SpriteSortMode.Immediate)
                this.Setup();
            this._batcher.DrawBatch(this._effect);
        }

        private void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof (texture));
            if (!this._beginCalled)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
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
        
        private GizmoBatchItem[] _batchItemList;
        private int _batchItemCount;
        private readonly GraphicsDevice _device;
        private short[] _index;
        private VertexPositionColorTexture[] _vertexArray;
        
        public ITexture WhiteTexture { get; set; }
        
        public GizmoBatcher(GraphicsDevice device)
        {
            _device = device;
            _batchItemList = new GizmoBatchItem[InitialBatchSize];
            _batchItemCount = 0;
            for (int index = 0; index < InitialBatchSize; ++index)
                _batchItemList[index] = new GizmoBatchItem();
            EnsureArrayCapacity(InitialBatchSize);
        }

        public ref GizmoBatchItem CreateBatchItem()
        {
            if (_batchItemCount >= _batchItemList.Length)
            {
                Console.WriteLine("Resized batch item list");
                int length = _batchItemList.Length;
                int newLength = length + length / 2 + 63 & -64;
                Array.Resize(ref _batchItemList, newLength);
                EnsureArrayCapacity(Math.Min(newLength, MaxBatchSize));
            }
            return ref _batchItemList[_batchItemCount++];
        }

        private void EnsureArrayCapacity(int numBatchItems) {
            int indexCount = 2 * numBatchItems;
            if (_index != null && indexCount <= _index.Length)
                return;
            short[] newIndexArray = new short[indexCount];
            int batchIndex = 0;
            if (_index != null)
            {
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
            if (effect != null && effect.IsDisposed)
                throw new ObjectDisposedException(nameof (effect));
            if (_batchItemCount == 0)
                return;
            if(WhiteTexture == null) {
                throw new Exception("Cannot draw GizmoBatch without a texture");
            }
            Array.Sort(this._batchItemList, 0, this._batchItemCount);

            int batchedThisIteration = 0;
            int vertexIndex = 0;
            for(int batchIndex = 0; batchIndex < _batchItemCount; batchIndex++) {
                GizmoBatchItem item = _batchItemList[batchIndex];

                _vertexArray[vertexIndex] = item.VertexA;
                _vertexArray[vertexIndex+1] = item.VertexB;
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
            var primitiveCount = numVertices / 2;

            if (numVertices <= 0)
                return;
            _device.Textures[0] = this.WhiteTexture.Source;
            if (effect != null)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _device.DrawUserIndexedPrimitives(primitiveType, _vertexArray, 0, numVertices, _index, 0, primitiveCount, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            else
                _device.DrawUserIndexedPrimitives(primitiveType, _vertexArray, 0, numVertices, _index, 0, primitiveCount, VertexPositionColorTexture.VertexDeclaration);
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