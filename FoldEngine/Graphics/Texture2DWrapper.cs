using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine.Graphics
{
    public class Texture2DWrapper
    {
        internal Microsoft.Xna.Framework.Graphics.Texture2D Texture;

        internal Texture2DWrapper(Microsoft.Xna.Framework.Graphics.Texture2D texture) => Texture = texture;

        public static implicit operator Microsoft.Xna.Framework.Graphics.Texture2D(Texture2DWrapper texture)
        {
            return texture.Texture;
        }
    }
}
