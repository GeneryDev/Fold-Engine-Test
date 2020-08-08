using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine.Graphics
{
    public class TextureManager
    {
        private readonly Dictionary<string, FoldEngine.Graphics.Texture2DWrapper> sprites = new Dictionary<string, FoldEngine.Graphics.Texture2DWrapper>();

        private ContentManager content;
        private GraphicsDeviceManager graphics;
        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;

        public FoldEngine.Graphics.Texture2DWrapper this[string name]
        {
            get
            {
                if (name == null) return sprites["null"];
                if (!sprites.ContainsKey(name))
                {
                    sprites[name] = sprites["null"];
                    Console.WriteLine("[WARN]: Attempted to retrieve unknown texture '" + name + "'");
                }
                return sprites[name];
            }
            set => sprites[name] = value;
        }

        public TextureManager(GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content)
        {
            this.graphics = graphics;
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;
            this.content = content;

            RenderTarget2D nullTexture = new RenderTarget2D(graphicsDevice, 2, 2);
            nullTexture.SetData(new Color[] { Color.Magenta, Color.Black, Color.Black, Color.Magenta });
            sprites["null"] = new FoldEngine.Graphics.Texture2DWrapper(nullTexture);
        }

        public FoldEngine.Graphics.Texture2DWrapper LoadSprite(string name)
        {
            return sprites[name] = new Texture2DWrapper(content.Load<Microsoft.Xna.Framework.Graphics.Texture2D>(name));
        }
    }
}
