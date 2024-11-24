using Microsoft.Xna.Framework;

namespace FoldEngine.Util;

public struct Rect2
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
    
    public Vector2 Position
    {
        get => new(X, Y);
        set => (X, Y) = value;
    }

    public Vector2 Size
    {
        get => new(Width, Height);
        set => (Width, Height) = (value);
    }

    public Vector2 Center => Position + Size / 2f;

    public Rect2(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Rect2(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    public void Deconstruct(out float x, out float y, out float width, out float height)
    {
        x = this.X;
        y = this.Y;
        width = this.Width;
        height = this.Height;
    }
}