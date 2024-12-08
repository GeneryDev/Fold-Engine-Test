using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics;

public class TriangleBatch
{
    public static bool UseHalfPixelOffset = false;

    private readonly TriangleBatcher _batcher;
    private readonly GraphicsDevice _device;
    private bool _beginCalled;
    private BatcherParams _activeParams;

    private Matrix _projection;
    private readonly CustomSpriteEffect _spriteEffect;

    public TriangleBatch(GraphicsDevice graphicsDevice)
    {
        _device = graphicsDevice
                  ?? throw new ArgumentNullException(nameof(graphicsDevice),
                      "The GraphicsDevice must not be null when creating new resources.");
        _spriteEffect = new CustomSpriteEffect(graphicsDevice);
        _batcher = new TriangleBatcher(graphicsDevice);
        _beginCalled = false;
    }

    private void Setup()
    {
        GraphicsDevice graphicsDevice = _device;
        graphicsDevice.BlendState = _activeParams.BlendState;
        graphicsDevice.DepthStencilState = _activeParams.DepthStencilState;
        graphicsDevice.RasterizerState = _activeParams.RasterizerState;
        graphicsDevice.SamplerStates[0] = _activeParams.SamplerState;

        _projection = _spriteEffect.SetupMatrix();
    }

    public void QuickBegin(
        RenderTarget2D renderTarget,
        BlendState blendState = null,
        SamplerState samplerState = null,
        DepthStencilState depthStencilState = null,
        RasterizerState rasterizerState = null,
        Effect effect = null)
    {
        blendState = blendState ?? BlendState.NonPremultiplied;
        samplerState = samplerState ?? SamplerState.LinearClamp;
        depthStencilState = depthStencilState ?? DepthStencilState.None;
        rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;

        var requestedParams = new BatcherParams(blendState, samplerState, depthStencilState, rasterizerState, effect);
        if (_beginCalled && requestedParams == _activeParams) return;
        End(renderTarget);
        if (_beginCalled)
            throw new InvalidOperationException(
                "Begin cannot be called again until End has been successfully called.");

        _activeParams = requestedParams;

        effect?.Parameters["MatrixTransform"]?.SetValue(_projection);
        _beginCalled = true;
    }

    public void End(RenderTarget2D renderTarget)
    {
        if (!_beginCalled) return;
        _beginCalled = _beginCalled
            ? false
            : throw new InvalidOperationException("Begin must be called before calling End.");
        _device.SetRenderTarget(renderTarget);
        Setup();
        _batcher.DrawBatch(_activeParams.Effect ?? _spriteEffect);
    }

    private void CheckValid(Texture2D texture)
    {
        if (texture == null)
            throw new ArgumentNullException(nameof(texture));
        if (!_beginCalled)
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
        Color? colorC = null)
    {
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
        Color? colorC = null)
    {
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
        Color? colorD = null)
    {
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
        Color? colorD = null)
    {
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

    public void Clear()
    {
        _batcher.Clear();
    }
}

public class TriangleBatcher
{
    private const int InitialBatchSize = 256;
    private const int MaxBatchSize = short.MaxValue / 6;
    private const int InitialVertexArraySize = 256;
    private readonly GraphicsDevice _device;
    private int _batchItemCount;

    private TriangleBatchItem[] _batchItemList;
    private short[] _index;
    private VertexPositionColorTexture[] _vertexArray;

    private readonly bool WireframeMode = false;

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
            Console.WriteLine($"Resized TriangleBatchItem list to fit {_batchItemCount}");
            int length = _batchItemList.Length;
            int newLength = (length + length / 2 + 63) & -64;
            Array.Resize(ref _batchItemList, newLength);
            EnsureArrayCapacity(Math.Min(newLength, MaxBatchSize));
        }

        return ref _batchItemList[_batchItemCount++];
    }

    private void EnsureArrayCapacity(int numBatchItems)
    {
        int indexCount = 3 * numBatchItems;
        if (_index != null && indexCount <= _index.Length)
            return;
        var newIndexArray = new short[indexCount];
        int batchIndex = 0;
        if (_index != null)
        {
            _index.CopyTo(newIndexArray, 0);
            batchIndex = _index.Length / 3;
        }

        for (int i = batchIndex; i < numBatchItems; i++)
        {
            int vertexArrayIndex = (short)(i * 3);
            newIndexArray[i * 3] = (short)vertexArrayIndex;
            newIndexArray[i * 3 + 1] = (short)(vertexArrayIndex + 1);
            newIndexArray[i * 3 + 2] = (short)(vertexArrayIndex + 2);
        }

        _index = newIndexArray;
        _vertexArray = new VertexPositionColorTexture[3 * numBatchItems];
    }

    public void DrawBatch(Effect effect)
    {
        if (effect != null && effect.IsDisposed)
            throw new ObjectDisposedException(nameof(effect));
        if (_batchItemCount == 0)
            return;


        int batchedThisIteration = 0;
        int vertexIndex = 0;
        Texture2D texture = null;
        for (int batchIndex = 0; batchIndex < _batchItemCount; batchIndex++)
        {
            TriangleBatchItem item = _batchItemList[batchIndex];

            if (texture != null && !ReferenceEquals(item.Texture, texture))
            {
                FlushVertexArray(batchedThisIteration * (WireframeMode ? 4 : 3), effect, texture);
                batchedThisIteration = 0;
                vertexIndex = 0;
            }

            texture = item.Texture;

            _vertexArray[vertexIndex] = item.VertexA;
            _vertexArray[vertexIndex + 1] = item.VertexB;
            _vertexArray[vertexIndex + 2] = item.VertexC;
            if (WireframeMode) _vertexArray[vertexIndex + 3] = item.VertexA;
            vertexIndex += WireframeMode ? 4 : 3;

            batchedThisIteration++;
            if (batchedThisIteration >= MaxBatchSize)
            {
                FlushVertexArray(batchedThisIteration * (WireframeMode ? 4 : 3), effect, texture);
                batchedThisIteration = 0;
                vertexIndex = 0;
            }
        }

        if (batchedThisIteration > 0)
        {
            FlushVertexArray(batchedThisIteration * (WireframeMode ? 4 : 3), effect, texture);
            batchedThisIteration = 0;
            vertexIndex = 0;
        }

        _batchItemCount = 0;
    }

    private void FlushVertexArray(
        int numVertices,
        Effect effect,
        Microsoft.Xna.Framework.Graphics.Texture texture)
    {
        var primitiveType = PrimitiveType.TriangleList;
        int primitiveCount = numVertices / 3;

        if (WireframeMode)
        {
            primitiveType = PrimitiveType.LineStrip;
            primitiveCount = numVertices - 1;
        }

        if (numVertices <= 0)
            return;

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.Textures[0] = texture;
            _device.DrawUserIndexedPrimitives(primitiveType, _vertexArray, 0, numVertices, _index, 0,
                primitiveCount, VertexPositionColorTexture.VertexDeclaration);
        }
    }

    public void Clear()
    {
        _batchItemCount = 0;
    }
}

public struct TriangleBatchItem
{
    public Texture2D Texture;
    public VertexPositionColorTexture VertexA;
    public VertexPositionColorTexture VertexB;
    public VertexPositionColorTexture VertexC;
}

public class CustomSpriteEffect : SpriteEffect
{
    private EffectParameter _matrixParam;
    private Viewport _lastViewport;
    private Matrix _projection;
    private float _zNearPlane;
    private float _zFarPlane;

    public CustomSpriteEffect(GraphicsDevice device, float nearPlane = 100, float farPlane = -100)
        : base(device)
    {
        _matrixParam = Parameters["MatrixTransform"];
        _zNearPlane = nearPlane;
        _zFarPlane = farPlane;
    }

    /// <summary>
    /// An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.
    /// </summary>
    public Matrix? TransformMatrix { get; set; }

    public override Effect Clone()
    {
        return new CustomSpriteEffect(GraphicsDevice, _zNearPlane, _zFarPlane);
    }

    public Matrix SetupMatrix()
    {
        var vp = GraphicsDevice.Viewport;
        if ((vp.Width != _lastViewport.Width) || (vp.Height != _lastViewport.Height))
        {
            Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, _zNearPlane, _zFarPlane, out _projection);

            if (TriangleBatch.UseHalfPixelOffset)
            {
                _projection.M41 += -0.5f * _projection.M11;
                _projection.M42 += -0.5f * _projection.M22;
            }

            _lastViewport = vp;
        }

        if (TransformMatrix.HasValue)
            _matrixParam.SetValue(TransformMatrix.GetValueOrDefault() * _projection);
        else
            _matrixParam.SetValue(_projection);

        return _projection;
    }

    protected override void OnApply()
    {
    }
}

internal struct BatcherParams
{
    public BlendState BlendState;
    public SamplerState SamplerState;
    public DepthStencilState DepthStencilState;
    public RasterizerState RasterizerState;
    public Effect Effect;

    public BatcherParams(BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState,
        RasterizerState rasterizerState, Effect effect)
    {
        BlendState = blendState;
        SamplerState = samplerState;
        DepthStencilState = depthStencilState;
        RasterizerState = rasterizerState;
        Effect = effect;
    }

    public bool Equals(BatcherParams other)
    {
        return Equals(BlendState, other.BlendState)
               && Equals(SamplerState, other.SamplerState)
               && Equals(DepthStencilState, other.DepthStencilState)
               && Equals(RasterizerState, other.RasterizerState)
               && Equals(Effect, other.Effect);
    }

    public override bool Equals(object obj)
    {
        return obj is BatcherParams other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (BlendState != null ? BlendState.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (SamplerState != null ? SamplerState.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (DepthStencilState != null ? DepthStencilState.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (RasterizerState != null ? RasterizerState.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Effect != null ? Effect.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator ==(BatcherParams a, BatcherParams b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(BatcherParams a, BatcherParams b)
    {
        return !(a == b);
    }
}