﻿using Microsoft.Xna.Framework;

namespace FoldEngine.Util;

public static class Extensions
{
    public static Vector2 ToVector2(this Vector3 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }

    public static Vector2 ApplyMatrixTransform(this Vector2 vec, Matrix matrix)
    {
        return (Matrix.CreateTranslation(new Vector3(vec, 0)) * matrix).Translation.ToVector2();
    }

    public static Color MultiplyColor(Color color0, Color color1)
    {
        byte r = (byte)(color0.R * color1.R / 255);
        byte g = (byte)(color0.G * color1.G / 255);
        byte b = (byte)(color0.B * color1.B / 255);
        byte a = (byte)(color0.A * color1.A / 255);
        return new Color(r, g, b, a);
    }

    public static Vector2 Normalized(this Vector2 vec)
    {
        vec.Normalize();
        return vec;
    }

    public static Rectangle Grow(this Rectangle rectangle, int amount)
    {
        (int x, int y, int width, int height) = rectangle;
        return new Rectangle(
            x - amount,
            y - amount,
            width + amount * 2,
            height + amount * 2
        );
    }

    public static Rectangle Translate(this Rectangle rectangle, Point offset)
    {
        (int x, int y, int width, int height) = rectangle;
        return new Rectangle(
            x + offset.X,
            y + offset.Y,
            width,
            height
        );
    }

    public static Point Min(this Point a, Point b)
    {
        (int x, int y) = a;
        (int x2, int y2) = b;
        return new Point(
            x < x2 ? x : x2,
            y < y2 ? y : y2
        );
    }

    public static Point Max(this Point a, Point b)
    {
        (int x, int y) = a;
        (int x2, int y2) = b;
        return new Point(
            x > x2 ? x : x2,
            y > y2 ? y : y2
        );
    }
}