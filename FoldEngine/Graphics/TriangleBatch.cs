using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    public class TriangleBatch {
        private readonly GraphicsDevice _device;
        private readonly TriangleBatcher _batcher;
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
        
        public TriangleBatch(GraphicsDevice graphicsDevice)
        {
            this._device = graphicsDevice ?? throw new ArgumentNullException(nameof (graphicsDevice), "The GraphicsDevice must not be null when creating new resources.");
            this._spriteEffect = new CustomSpriteEffect(graphicsDevice);
            this._matrixTransform = this._spriteEffect.Parameters["MatrixTransform"];
            this._spritePass = this._spriteEffect.CurrentTechnique.Passes[0];
            this._batcher = new TriangleBatcher(graphicsDevice);
            this._beginCalled = false;
        }

        private void SetupMatrix(Viewport viewport) {
            Matrix.CreateOrthographicOffCenter(0.0f, viewport.Width, viewport.Height, 0.0f, 0.0f, -1f, out this._projection);
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
            
            CheckValid(texture);
            
            colorA = colorA ?? Color.White;
            colorB = colorB ?? colorA;
            colorC = colorC ?? colorA;

            ref TriangleBatchItem item = ref _batcher.CreateBatchItem();
            
            item.Texture = texture;
            item.VertexA.Color = colorA.Value;
            item.VertexB.Color = colorB.Value;
            item.VertexC.Color = colorC.Value;

            item.VertexA.Position = new Vector3(a, 0);
            item.VertexB.Position = new Vector3(b, 0);
            item.VertexC.Position = new Vector3(c, 0);

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

            item0.VertexA.Position = new Vector3(a, 0);
            item0.VertexB.Position = new Vector3(b, 0);
            item0.VertexC.Position = new Vector3(c, 0);

            item0.VertexA.TextureCoordinate = texA;
            item0.VertexB.TextureCoordinate = texB;
            item0.VertexC.TextureCoordinate = texC;
            
            item1.Texture = texture;
            item1.VertexA.Color = colorB.Value;
            item1.VertexB.Color = colorD.Value;
            item1.VertexC.Color = colorC.Value;

            item1.VertexA.Position = new Vector3(b, 0);
            item1.VertexB.Position = new Vector3(d, 0);
            item1.VertexC.Position = new Vector3(c, 0);

            item1.VertexA.TextureCoordinate = texB;
            item1.VertexB.TextureCoordinate = texD;
            item1.VertexC.TextureCoordinate = texC;
        }
    }

    public class TriangleBatcher {
        private const int InitialBatchSize = 256;
        private const int MaxBatchSize = short.MaxValue / 6;
        private const int InitialVertexArraySize = 256;
        
        private TriangleBatchItem[] _batchItemList;
        private int _batchItemCount;
        private readonly GraphicsDevice _device;
        private short[] _index;
        private VertexPositionColorTexture[] _vertexArray;
        
        public TriangleBatcher(GraphicsDevice device)
        {
            _device = device;
            _batchItemList = new TriangleBatchItem[InitialBatchSize];
            _batchItemCount = 0;
            EnsureArrayCapacity(InitialBatchSize);
        }

        public ref TriangleBatchItem CreateBatchItem()
        {
            if (_batchItemCount >= _batchItemList.Length)
            {
                Console.WriteLine("Resized TriangleBatchItem list");
                int length = _batchItemList.Length;
                int newLength = length + length / 2 + 63 & -64;
                Array.Resize(ref _batchItemList, newLength);
                EnsureArrayCapacity(Math.Min(newLength, MaxBatchSize));
            }
            return ref _batchItemList[_batchItemCount++];
        }

        private void EnsureArrayCapacity(int numBatchItems) {
            int indexCount = 3 * numBatchItems;
            if (_index != null && indexCount <= _index.Length)
                return;
            short[] newIndexArray = new short[indexCount];
            int batchIndex = 0;
            if (_index != null)
            {
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
            if (effect != null && effect.IsDisposed)
                throw new ObjectDisposedException(nameof (effect));
            if (_batchItemCount == 0)
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
                _vertexArray[vertexIndex+1] = item.VertexB;
                _vertexArray[vertexIndex+2] = item.VertexC;
                if(WireframeMode) _vertexArray[vertexIndex+3] = item.VertexA;
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

        private bool WireframeMode = false;

        private void FlushVertexArray(int numVertices, Effect effect, Texture texture) {
            var primitiveType = PrimitiveType.TriangleList;
            var primitiveCount = numVertices / 3;

            if(WireframeMode) {
                primitiveType = PrimitiveType.LineStrip;
                primitiveCount = numVertices-1;
            }
            
            if (numVertices <= 0)
                return;
            _device.Textures[0] = texture;
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

    public struct TriangleBatchItem {
        public Texture2D Texture;
        public VertexPositionColorTexture VertexA;
        public VertexPositionColorTexture VertexB;
        public VertexPositionColorTexture VertexC;
    }

    public class CustomSpriteEffect : SpriteEffect {
        public CustomSpriteEffect(GraphicsDevice device) : base(device) { }
        protected CustomSpriteEffect(SpriteEffect cloneSource) : base(cloneSource) { }

        protected override void OnApply() {
            
        }
    }
}