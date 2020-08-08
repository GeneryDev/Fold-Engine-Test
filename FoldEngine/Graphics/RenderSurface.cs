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

        public Point Size => new Point(Target.Width, Target.Height);

        public RenderSurface(GraphicsDevice graphicsDevice, int width, int height)
        {
            GraphicsDevice = graphicsDevice;
            Batch = new SpriteBatch(graphicsDevice);
            Resize(width, height);
        }

        public void Draw(Texture2DWrapper texture, Rectangle destinationRectangle, Color? color)
        {
            Batch.Draw(texture.Texture, destinationRectangle, color ?? Microsoft.Xna.Framework.Color.White);
        }

        public void Draw(Texture2DWrapper texture, Vector2 position, Color? color)
        {
            Batch.Draw(texture.Texture, position, color ?? Microsoft.Xna.Framework.Color.White);
        }

        public void Draw(Texture2DWrapper texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color? color)
        {
            Batch.Draw(texture.Texture, destinationRectangle, sourceRectangle, color ?? Microsoft.Xna.Framework.Color.White);
        }

        public void Draw(Texture2DWrapper texture, Vector2 position, Rectangle? sourceRectangle, Color? color)
        {
            Batch.Draw(texture.Texture, position, sourceRectangle, color ?? Microsoft.Xna.Framework.Color.White);
        }

        public void Draw(Texture2DWrapper texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color? color, float rotation, Vector2 origin)
        {
            Batch.Draw(texture.Texture, destinationRectangle, sourceRectangle, color ?? Microsoft.Xna.Framework.Color.White, rotation, origin, SpriteEffects.None, 0.0f);
        }

        public void Draw(Texture2DWrapper texture, Vector2 position, Rectangle? sourceRectangle, Color? color, float rotation, Vector2 origin, float scale)
        {
            Batch.Draw(texture.Texture, position, sourceRectangle, color ?? Microsoft.Xna.Framework.Color.White, rotation, origin, scale, SpriteEffects.None, 0.0f);
        }

        public void Draw(Texture2DWrapper texture, Vector2 position, Rectangle? sourceRectangle, Color? color, float rotation, Vector2 origin, Vector2 scale)
        {
            Batch.Draw(texture.Texture, position, sourceRectangle, color ?? Microsoft.Xna.Framework.Color.White, rotation, origin, scale, SpriteEffects.None, 0.0f);
        }

        internal void Begin()
        {
            Batch.Begin();
        }
        internal void End()
        {
            GraphicsDevice.SetRenderTarget(Target);
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
            Batch.End();
        }

        public void Resize(int newWidth, int newHeight)
        {
            Target?.Dispose();
            Target = new RenderTarget2D(GraphicsDevice, newWidth, newHeight);
        }
    }
}
