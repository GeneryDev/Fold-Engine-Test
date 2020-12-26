using Microsoft.Xna.Framework;

namespace EntryProject.Util {
    public static class Extensions {
        public static Vector2 ToVector2(Vector3 vec) {
            return new Vector2(vec.X, vec.Y);
        }
    }
}