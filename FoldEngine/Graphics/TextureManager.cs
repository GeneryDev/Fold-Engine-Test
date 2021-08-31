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
        private readonly Dictionary<string, FoldEngine.Graphics.ITexture> _sprites = new Dictionary<string, FoldEngine.Graphics.ITexture>();

        private ContentManager _content;
        internal GraphicsDevice _device;

        public readonly Dictionary<string, TextureAtlas> Atlases = new Dictionary<string, TextureAtlas>();

        public FoldEngine.Graphics.ITexture this[string name]
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

        public TextureManager(GraphicsDevice device, ContentManager content)
        {
            this._device = device;
            this._content = content;

            RenderTarget2D nullTexture = new RenderTarget2D(device, 2, 2);
            nullTexture.SetData(new Color[] { Color.Magenta, Color.Black, Color.Black, Color.Magenta });
            _sprites["null"] = new FoldEngine.Graphics.DirectTexture(nullTexture);
        }

        public FoldEngine.Graphics.ITexture LoadTexture(string name)
        {
            return _sprites[name] = new DirectTexture(_content.Load<Microsoft.Xna.Framework.Graphics.Texture2D>("Textures/" + name));
        }

        public TextureAtlas CreateAtlas(string name) {
            return Atlases[name] = new TextureAtlas(name, this);
        }

        public ITexture CreateSubTexture(string parentName, string childName, Rectangle bounds) {
            ITexture parent = this[parentName];
            Rectangle subBounds = parent.CreateSubBounds(bounds);
            var subTexture = new AtlasedTexture(parent.Source, subBounds);
            this[parentName + "." + childName] = subTexture;
            return subTexture;
        }
    }
}
