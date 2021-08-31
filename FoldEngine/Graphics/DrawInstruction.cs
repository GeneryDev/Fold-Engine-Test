using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics
{
    public struct DrawRectInstruction
    {
        public ITexture Texture;
        public Rectangle DestinationRectangle;
        public Rectangle? SourceRectangle;
        public Color? Color;

        public DrawRectInstruction(ITexture texture, Vector2 destination, Rectangle? sourceRectangle = null) {
            Texture = texture;
            DestinationRectangle = new Rectangle(destination.ToPoint(),
                new Point(sourceRectangle?.Width ?? texture.Width,
                    sourceRectangle?.Height ?? texture.Height));
            SourceRectangle = sourceRectangle;
            Color = null;
        }

        public DrawRectInstruction(ITexture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle = null) {
            Texture = texture;
            DestinationRectangle = destinationRectangle;
            SourceRectangle = sourceRectangle;
            Color = null;
        }
        
        public static implicit operator DrawQuadInstruction(DrawRectInstruction instruction) {
            return new DrawQuadInstruction() {
                Texture = instruction.Texture,
                A = new Vector3(instruction.DestinationRectangle.Left, instruction.DestinationRectangle.Bottom, 50),
                B = new Vector3(instruction.DestinationRectangle.Left, instruction.DestinationRectangle.Top, 50),
                C = new Vector3(instruction.DestinationRectangle.Right, instruction.DestinationRectangle.Bottom, 50),
                D = new Vector3(instruction.DestinationRectangle.Right, instruction.DestinationRectangle.Top, 50),
                TexA = new Vector2(instruction.SourceRectangle?.Left ?? 0, instruction.SourceRectangle?.Bottom ?? 1),
                TexB = new Vector2(instruction.SourceRectangle?.Left ?? 0, instruction.SourceRectangle?.Top ?? 0),
                TexC = new Vector2(instruction.SourceRectangle?.Right ?? 1, instruction.SourceRectangle?.Bottom ?? 1),
                TexD = new Vector2(instruction.SourceRectangle?.Right ?? 1, instruction.SourceRectangle?.Top ?? 0),
                Color = instruction.Color
            };
        }
        
        
    }

    public struct DrawQuadInstruction {
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

        public Color? Color {
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
            Color? colorD = null) {
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
            Color? colorD = null) {
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
        }
    }

    public struct DrawTriangleInstruction {
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
        
        public Color Color {
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
            Color? colorC = null) {
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
            Color? colorC = null) {
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
        }
    }

}
