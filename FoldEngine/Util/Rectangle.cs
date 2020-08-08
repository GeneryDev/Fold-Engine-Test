using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine.Util.Deprecated
{
    public struct Rectangle
    {
        public int X, Y, Width, Height;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public Rectangle(VectorInt2 origin, VectorInt2 size) : this(origin.X, origin.Y, size.X, size.Y) { }

        public static implicit operator Microsoft.Xna.Framework.Rectangle(Rectangle thiz)
        {
            return new Microsoft.Xna.Framework.Rectangle(thiz.X, thiz.Y, thiz.Width, thiz.Height);
        }
    }
}
