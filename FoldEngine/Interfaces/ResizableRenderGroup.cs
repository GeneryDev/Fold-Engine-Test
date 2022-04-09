using Microsoft.Xna.Framework;

namespace FoldEngine.Interfaces {
    public class ResizableRenderGroup : RenderGroup {
        private Point _size;

        public ResizableRenderGroup(RenderGroup group) : base(group.RenderingUnit) {
            this.Dependencies.Add(new Dependency() {
                Group = group
            });
            this["child"] = new DependencyRenderingLayer(0);
            AdjustDependency(Dependencies[0]);
        }

        public override Point Size {
            get => _size;
            set {
                _size = value;
                foreach(Dependency dependency in Dependencies) {
                    AdjustDependency(dependency);
                }
            }
        } //analogous to window size

        private void AdjustDependency(Dependency dependency) {
            float maxAspectRatio = (float) Size.Y / Size.X;
            float subAspectRatio = (float) dependency.Group.Size.Y / dependency.Group.Size.X;

            if(subAspectRatio <= maxAspectRatio) {
                int height = (int) (Size.X * subAspectRatio);
                dependency.Destination = new Rectangle(0, Size.Y / 2 - height / 2, Size.X, height);
            } else {
                int width = (int) (Size.Y / subAspectRatio);
                dependency.Destination = new Rectangle(Size.X / 2 - width / 2, 0, width, Size.Y);
            }
        }

        public override void WindowSizeChanged(Point oldSize, Point newSize) {
            base.WindowSizeChanged(oldSize, newSize);
            Size = newSize;
        }
    }
}