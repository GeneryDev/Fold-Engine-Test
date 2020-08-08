using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine.Util.Deprecated
{
    public class VectorInt2
    {
        public static readonly VectorInt2 Zero = new VectorInt2(0, 0);
        public static readonly VectorInt2 One = new VectorInt2(1, 1);

        public static readonly VectorInt2 Left = new VectorInt2(-1, 0);
        public static readonly VectorInt2 Right = new VectorInt2(1, 0);
        public static readonly VectorInt2 Up = new VectorInt2(0, 1);
        public static readonly VectorInt2 Down = new VectorInt2(0, -1);

        public int X;
        public int Y;

        public VectorInt2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public float SqrMagnitude => X * X + Y * Y;
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public static explicit operator Vector2(VectorInt2 thiz)
        {
            return new Vector2(thiz.X, thiz.Y);
        }

        public static explicit operator Microsoft.Xna.Framework.Vector2(VectorInt2 thiz)
        {
            return new Microsoft.Xna.Framework.Vector2(thiz.X, thiz.Y);
        }

        public static VectorInt2 operator +(VectorInt2 a, VectorInt2 b)
        {
            return new VectorInt2(a.X + b.X, a.Y + b.Y);
        }

        public static VectorInt2 operator -(VectorInt2 a, VectorInt2 b)
        {
            return new VectorInt2(a.X - b.X, a.Y - b.Y);
        }

        public static VectorInt2 operator +(VectorInt2 a)
        {
            return new VectorInt2(a.X, a.Y);
        }

        public static VectorInt2 operator -(VectorInt2 a)
        {
            return new VectorInt2(-a.X, -a.Y);
        }
    }
}
