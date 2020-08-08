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



        internal static Rectangle Convert(IRenderingLayer layer, Rectangle rect)
        {
            Rectangle destination = layer.Destination;

            double scaleX = (double)destination.Width / layer.LayerSize.X;
            double scaleY = (double)destination.Height / layer.LayerSize.Y;

            rect.X = (int)Math.Floor(destination.X + rect.X * scaleX);
            rect.Y = (int)Math.Floor(destination.Y + rect.Y * scaleY);
            rect.Width = (int)Math.Round(rect.Width * scaleX);
            rect.Height = (int)Math.Round(rect.Height * scaleY);

            return rect;
        }

        internal Point Convert(IRenderingLayer layer, Point point)
        {
            Rectangle destination = layer.Destination;

            double scaleX = (double)destination.Width / layer.LayerSize.X;
            double scaleY = (double)destination.Height / layer.LayerSize.Y;

            point.X = (int)Math.Floor(destination.X + point.X * scaleX);
            point.Y = (int)Math.Floor(destination.Y + point.Y * scaleY);

            return point;
        }
    }
}
