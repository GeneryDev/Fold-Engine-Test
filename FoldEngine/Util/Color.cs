using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine.Util.Deprecated
{
    public struct Color
    {
        public static readonly Color White = new Color(1, 1, 1);

        public float R, G, B, A;

        public Color(float r, float g, float b, float a = 1.0f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static implicit operator Microsoft.Xna.Framework.Color(Color thiz)
        {
            return new Microsoft.Xna.Framework.Color(thiz.R, thiz.G, thiz.B, thiz.A);
        }
    }
}
