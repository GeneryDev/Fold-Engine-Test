using System;
using System.Collections.Generic;
using FoldEngine.Graphics;
using Microsoft.Xna.Framework;

namespace FoldEngine.Text;

public class FontSet : IFont
{
    private readonly List<FontSetEntry> _entries = new List<FontSetEntry>();
    public int Generation { get; private set; }

    public void RenderString(string text, out RenderedText rendered, float size)
    {
        PickFontForSize(size).RenderString(text, out rendered, size);
    }

    public void DrawString(string text, RenderSurface surface, Point start, Color color, float size)
    {
        PickFontForSize(size).DrawString(text, surface, start, color, size);
    }

    public FontSet AddFont(IFont font, float defaultSize)
    {
        _entries.Add(new FontSetEntry
        {
            Font = font,
            DefaultSize = defaultSize
        });
        Generation++;
        return this;
    }

    public IFont PickFontForSize(float size)
    {
        IFont closestFont = null;
        float closestDistance = float.MaxValue;

        foreach (FontSetEntry entry in _entries)
        {
            float ratio = size / entry.DefaultSize;
            float roundedRatio = (float)Math.Round(ratio);

            float ratioDistance = Math.Abs(roundedRatio - ratio);
            if (ratioDistance < closestDistance)
            {
                closestDistance = ratioDistance;
                closestFont = entry.Font;
            }
        }

        return closestFont;
    }


    private struct FontSetEntry
    {
        public IFont Font;
        public float DefaultSize;
    }
}