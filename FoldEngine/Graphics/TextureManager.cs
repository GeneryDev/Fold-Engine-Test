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
        private readonly Dictionary<string, FoldEngine.Graphics.Texture2DWrapper> _sprites = new Dictionary<string, FoldEngine.Graphics.Texture2DWrapper>();

        private ContentManager _content;
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        public FoldEngine.Graphics.Texture2DWrapper this[string name]
        {
            get
            {
                if (name == null) return _sprites["null"];
                if (!_sprites.ContainsKey(name))
                {
                    _sprites[name] = _sprites["null"];
                    Console.WriteLine("[WARN]: Attempted to retrieve unknown texture '" + name + "'");
                }
                return _sprites[name];
            }
            set => _sprites[name] = value;
        }

        public TextureManager(GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content)
        {
            this._graphics = graphics;
            this._graphicsDevice = graphicsDevice;
            this._spriteBatch = spriteBatch;
            this._content = content;

            RenderTarget2D nullTexture = new RenderTarget2D(graphicsDevice, 2, 2);
            nullTexture.SetData(new Color[] { Color.Magenta, Color.Black, Color.Black, Color.Magenta });
            _sprites["null"] = new FoldEngine.Graphics.Texture2DWrapper(nullTexture);
        }

        public FoldEngine.Graphics.Texture2DWrapper LoadSprite(string name)
        {
            return _sprites[name] = new Texture2DWrapper(_content.Load<Microsoft.Xna.Framework.Graphics.Texture2D>("Textures/" + name));
        }
    }
}
