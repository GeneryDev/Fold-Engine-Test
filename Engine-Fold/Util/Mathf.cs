using System;

namespace FoldEngine.Util;

public static class Mathf
{
    private static readonly Random Rand = new Random();

    public static float Lerp(float a, float b, float t)
    {
        return t * (b - a) + a;
    }

    public static float Random()
    {
        return (float)Rand.NextDouble();
    }

    public static float Random(float min, float max)
    {
        return Lerp(min, max, Random());
    }
}