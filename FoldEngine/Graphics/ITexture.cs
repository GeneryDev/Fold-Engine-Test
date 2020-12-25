using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FoldEngine.Graphics
{
    public interface ITexture {
        int Width { get; }
        int Height { get; }
        Texture2D Source { get; }
        void DrawOnto(SpriteBatch batch, Vector2 pos);
        void DrawOnto(SpriteBatch batch, Rectangle pos);
        Vector2 ToSourceUV(Vector2 uv);
    }

    public class DirectTexture : ITexture {
        internal Microsoft.Xna.Framework.Graphics.Texture2D Texture { get; private set; }

        public Texture2D Source => Texture;
        public int Width => Texture.Width;
        public int Height => Texture.Height;

        public DirectTexture(Texture2D texture) {
            Texture = texture;
        }

        public void DrawOnto(SpriteBatch batch, Vector2 pos) {
            batch.Draw(Texture, pos, Color.White);
        }

        public void DrawOnto(SpriteBatch batch, Rectangle pos) {
            batch.Draw(Texture, pos, Color.White);
        }

        public Vector2 ToSourceUV(Vector2 uv) {
            return uv;
        }
    }

    public class AtlasedTexture : ITexture {
        internal Microsoft.Xna.Framework.Graphics.Texture2D Texture { get; private set; }
        internal Rectangle Bounds;

        public Texture2D Source => Texture;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;

        public AtlasedTexture(Texture2D texture, Rectangle bounds) {
            Texture = texture;
            Bounds = bounds;
        }

        public void DrawOnto(SpriteBatch batch, Vector2 pos) {
            batch.Draw(Texture, pos, Bounds, Color.White);
        }

        public void DrawOnto(SpriteBatch batch, Rectangle pos) {
            batch.Draw(Texture, pos, Bounds, Color.White);
        }

        public Vector2 ToSourceUV(Vector2 uv) {
            return new Vector2(
                (Bounds.X + Width*uv.X) / Texture.Width,
                (Bounds.Y + Height*uv.Y) / Texture.Height
                );
        }
    }
}
