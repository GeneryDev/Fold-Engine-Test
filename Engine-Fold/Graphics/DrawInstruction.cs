using Microsoft.Xna.Framework;

namespace FoldEngine.Graphics;

public struct DrawRectInstruction
{
    public ITexture Texture;
    public Rectangle DestinationRectangle;
    public Rectangle? SourceRectangle;
    public Color? Color;
    public float Z;

    public DrawRectInstruction(ITexture texture, Vector2 destination, Rectangle? sourceRectangle = null, float z = 0)
    {
        Texture = texture;
        DestinationRectangle = new Rectangle(destination.ToPoint(),
            new Point(sourceRectangle?.Width ?? texture.Width,
                sourceRectangle?.Height ?? texture.Height));
        SourceRectangle = sourceRectangle;
        Color = null;
        Z = z;
    }

    public DrawRectInstruction(
        ITexture texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle = null, float z = 0)
    {
        Texture = texture;
        DestinationRectangle = destinationRectangle;
        SourceRectangle = sourceRectangle;
        Color = null;
        Z = z;
    }

    public static implicit operator DrawQuadInstruction(DrawRectInstruction instruction)
    {
        return new DrawQuadInstruction
        {
            Texture = instruction.Texture,
            A = new Vector3(instruction.DestinationRectangle.Left, instruction.DestinationRectangle.Bottom, instruction.Z),
            B = new Vector3(instruction.DestinationRectangle.Left, instruction.DestinationRectangle.Top, instruction.Z),
            C = new Vector3(instruction.DestinationRectangle.Right, instruction.DestinationRectangle.Bottom, instruction.Z),
            D = new Vector3(instruction.DestinationRectangle.Right, instruction.DestinationRectangle.Top, instruction.Z),
            TexA = new Vector2(instruction.SourceRectangle?.Left ?? 0, instruction.SourceRectangle?.Bottom ?? 1),
            TexB = new Vector2(instruction.SourceRectangle?.Left ?? 0, instruction.SourceRectangle?.Top ?? 0),
            TexC = new Vector2(instruction.SourceRectangle?.Right ?? 1, instruction.SourceRectangle?.Bottom ?? 1),
            TexD = new Vector2(instruction.SourceRectangle?.Right ?? 1, instruction.SourceRectangle?.Top ?? 0),
            Color = instruction.Color
        };
    }
}

public struct DrawQuadInstruction
{
    public ITexture Texture;
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    public Vector3 D;
    public Vector2 TexA;
    public Vector2 TexB;
    public Vector2 TexC;
    public Vector2 TexD;
    public Color? ColorA;
    public Color? ColorB;
    public Color? ColorC;
    public Color? ColorD;
    public EffectR Effect;

    public Color? Color
    {
        set => ColorA = ColorB = ColorC = ColorD = value;
    }

    public DrawQuadInstruction(
        ITexture texture,
        Vector2 a,
        Vector2 b,
        Vector2 c,
        Vector2 d,
        Vector2 texA,
        Vector2 texB,
        Vector2 texC,
        Vector2 texD,
        Color? colorA = null,
        Color? colorB = null,
        Color? colorC = null,
        Color? colorD = null,
        EffectR effect = null)
    {
        Texture = texture;
        A = new Vector3(a, 0);
        B = new Vector3(b, 0);
        C = new Vector3(c, 0);
        D = new Vector3(d, 0);
        TexA = texA;
        TexB = texB;
        TexC = texC;
        TexD = texD;
        ColorA = colorA;
        ColorB = colorB;
        ColorC = colorC;
        ColorD = colorD;
        Effect = effect;
    }

    public DrawQuadInstruction(
        ITexture texture,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d,
        Vector2 texA,
        Vector2 texB,
        Vector2 texC,
        Vector2 texD,
        Color? colorA = null,
        Color? colorB = null,
        Color? colorC = null,
        Color? colorD = null,
        EffectR effect = null)
    {
        Texture = texture;
        A = a;
        B = b;
        C = c;
        D = d;
        TexA = texA;
        TexB = texB;
        TexC = texC;
        TexD = texD;
        ColorA = colorA;
        ColorB = colorB;
        ColorC = colorC;
        ColorD = colorD;
        Effect = effect;
    }
}

public struct DrawTriangleInstruction
{
    public ITexture Texture;
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    public Vector2 TexA;
    public Vector2 TexB;
    public Vector2 TexC;
    public Color? ColorA;
    public Color? ColorB;
    public Color? ColorC;
    public EffectR Effect;

    public Color Color
    {
        set => ColorA = ColorB = ColorC = value;
    }

    public DrawTriangleInstruction(
        ITexture texture,
        Vector2 a,
        Vector2 b,
        Vector2 c,
        Vector2 texA,
        Vector2 texB,
        Vector2 texC,
        Color? colorA = null,
        Color? colorB = null,
        Color? colorC = null,
        EffectR effect = null)
    {
        Texture = texture;
        A = new Vector3(a, 0);
        B = new Vector3(b, 0);
        C = new Vector3(c, 0);
        TexA = texA;
        TexB = texB;
        TexC = texC;
        ColorA = colorA;
        ColorB = colorB;
        ColorC = colorC;
        Effect = effect;
    }

    public DrawTriangleInstruction(
        ITexture texture,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector2 texA,
        Vector2 texB,
        Vector2 texC,
        Color? colorA = null,
        Color? colorB = null,
        Color? colorC = null,
        EffectR effect = null)
    {
        Texture = texture;
        A = a;
        B = b;
        C = c;
        TexA = texA;
        TexB = texB;
        TexC = texC;
        ColorA = colorA;
        ColorB = colorB;
        ColorC = colorC;
        Effect = effect;
    }
}