using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Interfaces {
    public class RenderGroup {
        public readonly List<Dependency> Dependencies = new List<Dependency>();
        public readonly IRenderingUnit RenderingUnit;

        private readonly Dictionary<string, IRenderingLayer> _layers = new Dictionary<string, IRenderingLayer>();

        public RenderGroup(IRenderingUnit renderingUnit) {
            RenderingUnit = renderingUnit;
        }

        public virtual Point Size { get; set; } //analogous to window size
        public Rectangle Bounds => RenderingUnit.GetGroupBounds(this);
        public bool IsRoot { get; set; }

        public IRenderingLayer this[string layerName] {
            get => _layers.ContainsKey(layerName) ? _layers[layerName] : null;
            set {
                _layers[layerName] = value;
                value.Group = this;
            }
        }


        public void Begin() {
            foreach(IRenderingLayer layer in _layers.Values) {
                layer.IsRoot = IsRoot;
                layer.Begin();
            }
            foreach(Dependency dependency in Dependencies) {
                dependency.Group.IsRoot = false;
                dependency.Group.Begin();
            }
        }

        public void End() {
            foreach(Dependency dependency in Dependencies) dependency.Group.End();
            foreach(IRenderingLayer layer in _layers.Values) layer.End();
        }

        public void Present(SpriteBatch spriteBatch) {
            Rectangle groupBounds = Bounds;
            foreach(IRenderingLayer layer in _layers.Values) {
                if(layer is DependencyRenderingLayer dependencyLayer) {
                    Dependencies[dependencyLayer.DependencyIndex].Group.Present(spriteBatch);
                    continue;
                }

                var start = layer.Destination.Location.ToVector2();
                Vector2 end = start + layer.Destination.Size.ToVector2();

                start *= groupBounds.Size.ToVector2() / Size.ToVector2();
                end *= groupBounds.Size.ToVector2() / Size.ToVector2();

                start += groupBounds.Location.ToVector2();
                end += groupBounds.Location.ToVector2();

                spriteBatch.Draw(layer.Surface.Target, new Rectangle(start.ToPoint(), (end - start).ToPoint()),
                    Color.White);
            }
        }

        public Rectangle? GetBounds(RenderGroup renderGroup) {
            if(renderGroup == this) return new Rectangle(Point.Zero, Size);
            foreach(Dependency dependency in Dependencies) {
                Rectangle? bounds = dependency.Group.GetBounds(renderGroup);
                if(bounds.HasValue) {
                    var start = bounds.Value.Location.ToVector2();
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

        public virtual void AddDependency(Dependency dependency) {
            Dependencies.Add(dependency);
        }

        public virtual void WindowSizeChanged(Point oldSize, Point newSize) {
            foreach(IRenderingLayer layer in _layers.Values) layer.WindowSizeChanged(oldSize, newSize);
        }

        public class Dependency {
            public Rectangle Destination;
            public RenderGroup Group;
        }
    }
}