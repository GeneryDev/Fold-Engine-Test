﻿using System;
using Microsoft.Xna.Framework;

namespace FoldEngine.Util;

public struct Complex
{
    public float A; //Real component
    public float B; //Imaginary component

    public float SqrMagnitude => A * A + B * B;
    public float Magnitude => (float)Math.Sqrt(SqrMagnitude);
    public Complex Inverse => new Complex(A / SqrMagnitude, -B / SqrMagnitude);

    public Complex Normalized => new Complex(A / Magnitude, B / Magnitude);
    public float Angle => (float)Math.Atan2(B, A);

    public static readonly Complex Real = new Complex(1, 0);
    public static readonly Complex Imaginary = new Complex(0, 1);

    public Complex(float real, float imaginary)
    {
        A = real;
        B = imaginary;
    }

    public static Complex operator +(Complex a, Complex b)
    {
        return new Complex(a.A + b.A, a.B + b.B);
    }

    public static Complex operator -(Complex a, Complex b)
    {
        return new Complex(a.A - b.A, a.B - b.B);
    }

    public static Complex operator *(Complex a, Complex b)
    {
        return new Complex(a.A * b.A - a.B * b.B, a.A * b.B + a.B * b.A);
    }

    public static Complex operator /(Complex a, Complex b)
    {
        return a * b.Inverse;
    }

    public static implicit operator Vector2(Complex a)
    {
        return new Vector2(a.A, a.B);
    }

    public static implicit operator Complex(Vector2 a)
    {
        return new Complex(a.X, a.Y);
    }


    public static Complex FromRotation(float radians)
    {
        float x = (float)Math.Cos(radians);
        float y = (float)Math.Sin(radians);
        return new Complex(x, y);
    }

    public Complex ScaleAxes(float realScale, float imaginaryScale)
    {
        return new Complex(A * realScale, B * imaginaryScale);
    }
}