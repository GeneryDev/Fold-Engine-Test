using System;

namespace FoldEngine.Util {
    public static class MathUtil {
        public const double Ln2 = 0.69314718055994529;

        public static int NearestPowerOfTwo(int n) {
            return 1 << (int) Math.Ceiling(Log2(n));
        }

        public static int MaximumSetBit(int n) {
            if(n == 0) return 0;
            return 1 << (int) Math.Floor(Log2(n));
        }

        public static long NearestPowerOfTwo(long n) {
            return 1 << (int) Math.Ceiling(Log2(n));
        }

        public static long MaximumSetBit(long n) {
            if(n == 0) return 0;
            return 1 << (int) Math.Floor(Log2(n));
        }

        public static double Log2(double n) {
            return Math.Log(n) / Ln2;
        }
    }
}