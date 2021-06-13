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

        Vector2 CameraToLayer(Vector2 point);
        Vector2 LayerToCamera(Vector2 point);
        Vector2 LayerToLayer(Vector2 point, IRenderingLayer other);
    }

    public class RenderingLayer : IRenderingLayer
    {
        public string Name { get; set; }
        public Point LayerSize { get; set; }
        public Rectangle Destination { get; set; }
        public SamplerState Sampling { get; set; } = SamplerState.PointClamp;

        public RenderSurface Surface { get; set; }


        public Vector2 CameraToLayer(Vector2 point) {
            // (int x, int y, int width, int height) = Destination;
            //
            // double scaleX = (double) width / LayerSize.X;
            // double scaleY = (double) height / LayerSize.Y;
            //
            // point.X = (int) Math.Round(x + point.X * scaleX) + LayerSize.X / 2;
            // point.Y = (int) Math.Round(y - point.Y * scaleY) + LayerSize.Y / 2;

            point.Y *= -1;
            
            point *= LayerSize.ToVector2() / Destination.Size.ToVector2();

            point += LayerSize.ToVector2() / 2f;

            return point;
        }


        public Vector2 LayerToLayer(Vector2 point, IRenderingLayer to) {
            Vector2 relativeScale = to.LayerSize.ToVector2() / this.LayerSize.ToVector2();

            return ((point - new Vector2(this.LayerSize.X / 2f, this.LayerSize.Y / 2f)) * relativeScale)
                   + new Vector2(to.LayerSize.X / 2f, to.LayerSize.Y / 2f);
        }

        public Vector2 LayerToCamera(Vector2 point) {
            return new Vector2(1, -1) * (point - LayerSize.ToVector2() / 2f) * (Destination.Size.ToVector2() / LayerSize.ToVector2());
        }
    }
}
