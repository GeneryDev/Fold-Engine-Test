﻿using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics;

public class RenderSurface
{
    public GizmoBatch GizBatch;
    internal GraphicsDevice GraphicsDevice;
    internal IRenderingLayer Layer;
    internal IRenderingUnit RenderingUnit;
    internal RenderTarget2D Target;
    internal TriangleBatch TriBatch;

    public RenderSurface(IRenderingLayer layer, int width, int height)
    {
        Layer = layer;
        GraphicsDevice = layer.RenderingUnit.Core.FoldGame.GraphicsDevice;
        RenderingUnit = layer.RenderingUnit;
        TriBatch = new TriangleBatch(GraphicsDevice);
        GizBatch = new GizmoBatch(GraphicsDevice);
        Resize(width, height);
    }

    public RenderSurface(GraphicsDevice graphicsDevice, IRenderingUnit renderingUnit, int width, int height)
    {
        GraphicsDevice = graphicsDevice;
        RenderingUnit = renderingUnit;
        TriBatch = new TriangleBatch(graphicsDevice);
        GizBatch = new GizmoBatch(graphicsDevice);
        Resize(width, height);
    }

    public Point Size => new Point(Target.Width, Target.Height);

    public void Draw(DrawQuadInstruction instruction)
    {
        if (instruction.Texture == null) return;
        TriBatch.QuickBegin(Target, samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default,
            effect: instruction.Effect?.Effect);
        TriBatch.DrawQuad(
            instruction.Texture.Source,
            instruction.A,
            instruction.B,
            instruction.C,
            instruction.D,
            instruction.Texture.ToSourceUV(instruction.TexA),
            instruction.Texture.ToSourceUV(instruction.TexB),
            instruction.Texture.ToSourceUV(instruction.TexC),
            instruction.Texture.ToSourceUV(instruction.TexD),
            instruction.ColorA,
            instruction.ColorB,
            instruction.ColorC,
            instruction.ColorD
        );
    }

    public void Draw(DrawTriangleInstruction instruction)
    {
        if (instruction.Texture == null) return;
        TriBatch.QuickBegin(Target, samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default,
            effect: instruction.Effect?.Effect);
        TriBatch.DrawTriangle(
            instruction.Texture.Source,
            instruction.A,
            instruction.B,
            instruction.C,
            instruction.Texture.ToSourceUV(instruction.TexA),
            instruction.Texture.ToSourceUV(instruction.TexB),
            instruction.Texture.ToSourceUV(instruction.TexC),
            instruction.ColorA,
            instruction.ColorB,
            instruction.ColorC
        );
    }

    internal void Begin()
    {
        GraphicsDevice.SetRenderTarget(Target);
        GraphicsDevice.Clear(Layer?.Color ?? Color.Transparent);

        GizBatch.WhiteTexture = RenderingUnit.WhiteTexture;
        // TriBatch.QuickBegin(samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);
        // TriBatch.QuickBegin(samplerState: SamplerState.PointClamp, depthStencilState: DepthStencilState.Default);
        GizBatch.Begin(samplerState: SamplerState.PointClamp);
    }

    internal void End()
    {
        GraphicsDevice.SetRenderTarget(Target);
        TriBatch.End(Target);
        GizBatch.End();
    }

    public void Resize(int newWidth, int newHeight)
    {
        Target?.Dispose();
        Target = new RenderTarget2D(GraphicsDevice, newWidth, newHeight, false, SurfaceFormat.Color,
            DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);
        //TODO MAKE THIS NOT PRESEVE CONTENTS
        //Tweak the TriBatcher to support batching with different parameters together
    }
}