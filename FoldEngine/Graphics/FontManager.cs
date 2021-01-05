using System;
using System.Collections.Generic;
using FoldEngine.Text;

namespace FoldEngine.Graphics {
    public class FontManager {
        private TextureManager _textureManager;
        private Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        public FontManager(TextureManager textureManager) {
            _textureManager = textureManager;
        }
        
        public Font this[string name]
        {
            get
            {
                if (name == null) return null;
                if (!_fonts.ContainsKey(name)) {
                    Console.WriteLine("[ERROR]: Attempted to retrieve unknown font '" + name + "'");
                    return null;
                }
                return _fonts[name];
            }
            private set => _fonts[name] = value;
        }

        public Font CreateAsciiFont(string name, string textureName) {
            Font font = new Font();
            font.Textures["ascii"] = _textureManager[textureName];
            _fonts[name] = font;
            return font;
        }
    }
}