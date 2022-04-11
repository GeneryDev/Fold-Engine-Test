namespace FoldEngine.Util {
    public static class Mathf {
        public static float Lerp(float a, float b, float t) {
            return t * (b - a) + a;
        }
    }
}