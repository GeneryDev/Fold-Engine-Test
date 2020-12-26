using System;
using Microsoft.Xna.Framework;

namespace EntryProject.Util {
    public class Extensions {
        public static Vector2 ToVector2(Vector3 vec) {
            return new Vector2(vec.X, vec.Y);
        }

        public static Color MultiplyColor(Color color0, Color color1) {
            byte r = (byte) ((color0.R * color1.R) / 255);
            byte g = (byte) ((color0.G * color1.G) / 255);
            byte b = (byte) ((color0.B * color1.B) / 255);
            byte a = (byte) ((color0.A * color1.A) / 255);
            return new Color(r, g, b, a);
        }
    }
}