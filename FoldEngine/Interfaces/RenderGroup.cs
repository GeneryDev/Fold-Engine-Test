using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Interfaces {
    public class RenderGroup {
        private readonly IRenderingUnit _renderingUnit;
        public Point Size { get; set; } //analogous to window size
        public Rectangle Bounds => _renderingUnit.GetGroupBounds(this);
        
        public readonly List<Dependency> Dependencies = new List<Dependency>();

        private Dictionary<string, IRenderingLayer> _layers = new Dictionary<string, IRenderingLayer>();

        public IRenderingLayer this[string layerName] {
            get => _layers[layerName];
            set {
                _layers[layerName] = value;
                value.Group = this;
            }
        }

        public RenderGroup(IRenderingUnit renderingUnit) {
            _renderingUnit = renderingUnit;
        }


        public void Begin() {
            foreach(IRenderingLayer layer in _layers.Values) {
                layer.Surface.Begin();
            }
            foreach(Dependency dependency in Dependencies) {
                dependency.Group.Begin();
            }
        }
        public void End() {
            foreach(Dependency dependency in Dependencies) {
                dependency.Group.End();
            }
            foreach(IRenderingLayer layer in _layers.Values) {
                layer.Surface.End();
            }
        }

        public void Present(SpriteBatch spriteBatch) {
            Rectangle groupBounds = Bounds;
            foreach(IRenderingLayer layer in _layers.Values) {
                Vector2 start = layer.Destination.Location.ToVector2();
                Vector2 end = start + layer.Destination.Size.ToVector2();
                
                start *= groupBounds.Size.ToVector2() / Size.ToVector2();
                end *= groupBounds.Size.ToVector2() / Size.ToVector2();

                start += groupBounds.Location.ToVector2();
                end += groupBounds.Location.ToVector2();
                
                spriteBatch.Draw(layer.Surface.Target, new Rectangle(start.ToPoint(), (end - start).ToPoint()), Color.White);
            }
            foreach(Dependency dependency in Dependencies) {
                dependency.Group.Present(spriteBatch);
            }
        }

        public class Dependency {
            public RenderGroup Group;
            public Rectangle Destination;
        }

        public Rectangle? GetBounds(RenderGroup renderGroup) {
            if(renderGroup == this) return new Rectangle(Point.Zero, Size);
            foreach(Dependency dependency in Dependencies) {
                var bounds = dependency.Group.GetBounds(renderGroup);
                if(bounds.HasValue) {
                    Vector2 start = bounds.Value.Location.ToVector2();
                    Vector2 end = start + bounds.Value.Size.ToVector2();

                    start *= dependency.Destination.Size.ToVector2() / dependency.Group.Size.ToVector2();
                    end *= dependency.Destination.Size.ToVector2() / dependency.Group.Size.ToVector2();

                    start += dependency.Destination.Location.ToVector2();
                    end += dependency.Destination.Location.ToVector2();

                    return new Rectangle(start.ToPoint(), (end - start).ToPoint());
                }
            }

            return null;
        }
    }
}