using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text;

public struct RenderedText
{
    public BitmapFont BitmapFont;
    public string Text;
    public float Size;
    public int Generation;
    public RenderedTextGlyph[] Glyphs;
    public int Width;
    public int Height;

    public bool HasValue => Glyphs != null;

    public void DrawOnto(RenderSurface surface, Point start, Color color, float scale = 1)
    {
        Update();
        foreach (RenderedTextGlyph glyph in Glyphs) glyph.DrawOnto(surface, start, color, scale, BitmapFont);
    }

    public RenderedText Update()
    {
        if (Generation != BitmapFont.Generation) TextRenderer.Instance.Render(BitmapFont, Text, out this, Size);
        return this;
    }
}

public struct RenderedTextGlyph
{
    public bool HasValue;
    public int SourceIndex;
    public Rectangle Source;
    public Rectangle Destination;

    public RenderedTextGlyph(bool hasValue) : this()
    {
        HasValue = hasValue;
    }

    public void DrawOnto(RenderSurface surface, Point start, Color color, float scale, BitmapFont bitmapFont)
    {
        ITexture texture = bitmapFont.TextureSources[SourceIndex];
        var textureSize = new Vector2(texture.Width, texture.Height);
        (int x, int y) = start;
        surface.Draw(new DrawQuadInstruction(
            texture,
            new Vector2(scale * Destination.Left + x, scale * Destination.Bottom + y),
            new Vector2(scale * Destination.Left + x, scale * Destination.Top + y),
            new Vector2(scale * Destination.Right + x, scale * Destination.Bottom + y),
            new Vector2(scale * Destination.Right + x, scale * Destination.Top + y),
            new Vector2(Source.Left, Source.Bottom) / textureSize,
            new Vector2(Source.Left, Source.Top) / textureSize,
            new Vector2(Source.Right, Source.Bottom) / textureSize,
            new Vector2(Source.Right, Source.Top) / textureSize,
            color
        ));
    }
}