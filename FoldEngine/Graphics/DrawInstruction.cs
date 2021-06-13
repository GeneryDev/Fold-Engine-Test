using Microsoft.Xna.Framework;

using System;
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
    }

    public struct DrawQuadInstruction {
        public ITexture Texture;
        public Vector2 A;
        public Vector2 B;
        public Vector2 C;
        public Vector2 D;
        public Vector2 TexA;
        public Vector2 TexB;
        public Vector2 TexC;
        public Vector2 TexD;
        public Color? ColorA;
        public Color? ColorB;
        public Color? ColorC;
        public Color? ColorD;

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
        public Vector2 A;
        public Vector2 B;
        public Vector2 C;
        public Vector2 TexA;
        public Vector2 TexB;
        public Vector2 TexC;
        public Color? ColorA;
        public Color? ColorB;
        public Color? ColorC;

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
