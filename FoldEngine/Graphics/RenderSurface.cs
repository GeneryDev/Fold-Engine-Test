using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Graphics
{
    public class RenderSurface
    {
        internal GraphicsDevice GraphicsDevice;
        internal RenderTarget2D Target;
        internal SpriteBatch Batch;
        internal TriangleBatch TriBatch;

        public Point Size => new Point(Target.Width, Target.Height);

        public RenderSurface(GraphicsDevice graphicsDevice, int width, int height)
        {
            GraphicsDevice = graphicsDevice;
            Batch = new SpriteBatch(graphicsDevice);
            TriBatch = new TriangleBatch(graphicsDevice);
            Resize(width, height);
        }

        public void Draw(DrawRectInstruction instruction)
        {
            Batch.Draw(instruction.Texture,
                instruction.DestinationRectangle,
                instruction.SourceRectangle,
                instruction.Color ?? Color.White,
                instruction.Rotation,
                instruction.Pivot
                ?? new Vector2((float)instruction.Texture.Texture.Width / 2,
                    (float)instruction.Texture.Texture.Height / 2),
                SpriteEffects.None,
                0);
            
            // TriBatch.DrawQuad(
            //     instruction.Texture,
            //     new Vector2(instruction.DestinationRectangle.Left, instruction.DestinationRectangle.Bottom),
            //     new Vector2(instruction.DestinationRectangle.Left, instruction.DestinationRectangle.Top),
            //     new Vector2(instruction.DestinationRectangle.Right, instruction.DestinationRectangle.Bottom),
            //     new Vector2(instruction.DestinationRectangle.Right, instruction.DestinationRectangle.Top),
            //     new Vector2(instruction.SourceRectangle?.Left ?? 0, instruction.SourceRectangle?.Bottom ?? 1),
            //     new Vector2(instruction.SourceRectangle?.Left ?? 0, instruction.SourceRectangle?.Top ?? 0),
            //     new Vector2(instruction.SourceRectangle?.Right ?? 1, instruction.SourceRectangle?.Bottom ?? 1),
            //     new Vector2(instruction.SourceRectangle?.Right ?? 1, instruction.SourceRectangle?.Top ?? 0),
            //     Color.White
            // );
        }

        public void Draw(DrawQuadInstruction instruction)
        {
            TriBatch.DrawQuad(
                instruction.Texture,
                instruction.A,
                instruction.B,
                instruction.C,
                instruction.D,
                instruction.TexA,
                instruction.TexB,
                instruction.TexC,
                instruction.TexD,
                instruction.ColorA,
                instruction.ColorB,
                instruction.ColorC,
                instruction.ColorD
            );
        }

        public void Draw(DrawTriangleInstruction instruction)
        {
            TriBatch.DrawTriangle(
                instruction.Texture,
                instruction.A,
                instruction.B,
                instruction.C,
                instruction.TexA,
                instruction.TexB,
                instruction.TexC,
                instruction.ColorA,
                instruction.ColorB,
                instruction.ColorC
            );
        }

        internal void Begin()
        {
            Batch.Begin();
            TriBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
        }
        internal void End()
        {
            GraphicsDevice.SetRenderTarget(Target);
            GraphicsDevice.Clear(Color.Transparent);
            Batch.End();
            TriBatch.End();
        }

        public void Resize(int newWidth, int newHeight)
        {
            Target?.Dispose();
            Target = new RenderTarget2D(GraphicsDevice, newWidth, newHeight);
        }
    }
}
