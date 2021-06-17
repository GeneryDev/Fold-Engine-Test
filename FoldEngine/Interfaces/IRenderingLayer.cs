using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Graphics;
using Woofer;

namespace FoldEngine.Interfaces
{
    public interface IRenderingLayer {
        IRenderingUnit RenderingUnit { get; }
        RenderGroup Group { get; set; }
        
        string Name { get; }
        Point LayerSize { get; }
        Vector2 LogicalSize { get; }
        Rectangle Destination { get; set; }
        SamplerState Sampling { get; }

        RenderSurface Surface { get; set; }
        Color? Color { get; set; }

        Vector2 CameraToLayer(Vector2 point);
        Vector2 LayerToCamera(Vector2 point);
        Vector2 LayerToLayer(Vector2 point, IRenderingLayer other);

        Vector2 WindowToLayer(Vector2 point);
        Vector2 LayerToWindow(Vector2 point);
    }

    public class RenderingLayer : IRenderingLayer {
        public IRenderingUnit RenderingUnit { get; }
        public RenderGroup Group { get; set; }
        private Point _layerSize;

        public RenderingLayer(IRenderingUnit renderer) {
            this.RenderingUnit = renderer;
        }

        public string Name { get; set; }

        public Point LayerSize {
            get => _layerSize;
            set {
                if(Surface != null) {
                    Surface.Resize(value.X, value.Y);
                } else {
                    Surface = new RenderSurface(this, value.X, value.Y);
                }
                _layerSize = value;
            }
        }

        public Vector2 LogicalSize { get; set; }
        public Rectangle Destination { get; set; }
        public SamplerState Sampling { get; set; } = SamplerState.PointClamp;

        public RenderSurface Surface { get; set; }
        public Color? Color { get; set; }


        public Vector2 CameraToLayer(Vector2 point) {
            // (int x, int y, int width, int height) = Destination;
            //
            // double scaleX = (double) width / LayerSize.X;
            // double scaleY = (double) height / LayerSize.Y;
            //
            // point.X = (int) Math.Round(x + point.X * scaleX) + LayerSize.X / 2;
            // point.Y = (int) Math.Round(y - point.Y * scaleY) + LayerSize.Y / 2;

            point.Y *= -1;
            
            point *= LayerSize.ToVector2() / LogicalSize;

            point += LayerSize.ToVector2() / 2f;

            return point;
        }


        public Vector2 LayerToLayer(Vector2 point, IRenderingLayer to) {
            Vector2 relativeScale = to.LayerSize.ToVector2() / this.LayerSize.ToVector2();

            return ((point - new Vector2(this.LayerSize.X / 2f, this.LayerSize.Y / 2f)) * relativeScale)
                   + new Vector2(to.LayerSize.X / 2f, to.LayerSize.Y / 2f);
        }

        public Vector2 LayerToCamera(Vector2 point) {
            return new Vector2(1, -1) * (point - LayerSize.ToVector2() / 2f) * (LogicalSize / LayerSize.ToVector2());
        }

        public Vector2 WindowToLayer(Vector2 point) {
            Rectangle groupBounds = Group.Bounds;
            
            point -= groupBounds.Location.ToVector2();
            point /= groupBounds.Size.ToVector2();
            point *= Group.Size.ToVector2();
            point -= Destination.Location.ToVector2();
            point /= Destination.Size.ToVector2();
            point *= LayerSize.ToVector2();
            return point;
        }

        public Vector2 LayerToWindow(Vector2 point) {
            Rectangle groupBounds = Group.Bounds;
            
            point /= LayerSize.ToVector2();
            point *= Destination.Size.ToVector2();
            point += Destination.Location.ToVector2();
            point /= Group.Size.ToVector2();
            point *= groupBounds.Size.ToVector2();
            point += groupBounds.Location.ToVector2();
            return point;
        }
    }
}
