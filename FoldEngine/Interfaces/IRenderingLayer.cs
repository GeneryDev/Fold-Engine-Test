using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Graphics;

namespace FoldEngine.Interfaces
{
    public interface IRenderingLayer
    {
        string Name { get; }
        Point LayerSize { get; }
        Rectangle Destination { get; }
        SamplerState Sampling { get; }

        RenderSurface Surface { get; set; }
    }

    public class RenderingLayer : IRenderingLayer
    {
        public string Name { get; set; }
        public Point LayerSize { get; set; }
        public Rectangle Destination { get; set; }
        public SamplerState Sampling { get; set; } = SamplerState.PointClamp;

        public RenderSurface Surface { get; set; }



        public static Rectangle WorldToScreen(IRenderingLayer layer, Rectangle rect)
        {
            (int x, int y, int width, int height) = layer.Destination;

            double scaleX = (double)width / layer.LayerSize.X;
            double scaleY = (double)height / layer.LayerSize.Y;

            rect.X = (int)Math.Floor(x + rect.X * scaleX);
            rect.Y = (int)Math.Floor(y + rect.Y * scaleY);
            rect.Width = (int)Math.Round(rect.Width * scaleX);
            rect.Height = (int)Math.Round(rect.Height * scaleY);

            return rect;
        }

        //TODO
        public static Vector2 WorldToScreen(IRenderingLayer layer, Vector2 point)
        {
            (int x, int y, int width, int height) = layer.Destination;

            double scaleX = (double)width / layer.LayerSize.X;
            double scaleY = (double)height / layer.LayerSize.Y;

            point.X = (int)Math.Round(x + point.X * scaleX) + layer.LayerSize.X/2;
            point.Y = (int)Math.Round(y + point.Y * scaleY) + layer.LayerSize.Y/2;

            return point;
        }

        public static Vector2 ScreenToWorld(IRenderingLayer layer, Vector2 point) {
            return (point - (layer.Destination.Center.ToVector2())) * (layer.LayerSize.ToVector2() / layer.Destination.Size.ToVector2());
        }
    }
}
