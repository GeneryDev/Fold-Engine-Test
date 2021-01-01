using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoldEngine.Interfaces;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Graphics
{
    public class RenderSurface
    {
        internal GraphicsDevice GraphicsDevice;
        internal IRenderingUnit RenderingUnit;
        internal RenderTarget2D Target;
        internal TriangleBatch TriBatch;
        internal GizmoBatch GizBatch;

        public Point Size => new Point(Target.Width, Target.Height);

        public RenderSurface(GraphicsDevice graphicsDevice, IRenderingUnit renderingUnit, int width, int height)
        {
            GraphicsDevice = graphicsDevice;
            RenderingUnit = renderingUnit;
            TriBatch = new TriangleBatch(graphicsDevice);
            GizBatch = new GizmoBatch(graphicsDevice);
            Resize(width, height);
        }

        public void Draw(DrawRectInstruction instruction)
        {
            TriBatch.DrawQuad(
                instruction.Texture.Source,
                new Vector2(instruction.DestinationRectangle.Left, instruction.DestinationRectangle.Bottom),
                new Vector2(instruction.DestinationRectangle.Left, instruction.DestinationRectangle.Top),
                new Vector2(instruction.DestinationRectangle.Right, instruction.DestinationRectangle.Bottom),
                new Vector2(instruction.DestinationRectangle.Right, instruction.DestinationRectangle.Top),
                instruction.Texture.ToSourceUV(new Vector2(instruction.SourceRectangle?.Left ?? 0, instruction.SourceRectangle?.Bottom ?? 1)),
                instruction.Texture.ToSourceUV(new Vector2(instruction.SourceRectangle?.Left ?? 0, instruction.SourceRectangle?.Top ?? 0)),
                instruction.Texture.ToSourceUV(new Vector2(instruction.SourceRectangle?.Right ?? 1, instruction.SourceRectangle?.Bottom ?? 1)),
                instruction.Texture.ToSourceUV(new Vector2(instruction.SourceRectangle?.Right ?? 1, instruction.SourceRectangle?.Top ?? 0)),
                Color.White
            );
        }

        public void Draw(DrawQuadInstruction instruction)
        {
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
            GizBatch.WhiteTexture = RenderingUnit.WhiteTexture;
            TriBatch.Begin(samplerState: SamplerState.PointClamp);
            GizBatch.Begin(samplerState: SamplerState.PointClamp);
        }
        internal void End()
        {
            GraphicsDevice.SetRenderTarget(Target);
            GraphicsDevice.Clear(Color.Transparent);
            TriBatch.End();
            GizBatch.End();
        }

        public void Resize(int newWidth, int newHeight)
        {
            Target?.Dispose();
            Target = new RenderTarget2D(GraphicsDevice, newWidth, newHeight);
        }
    }
}
