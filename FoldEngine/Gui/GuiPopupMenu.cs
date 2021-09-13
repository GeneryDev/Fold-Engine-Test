using EntryProject.Util;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui {
    public class GuiPopupMenu : GuiPanel {
        public bool Showing = false;
        
        public GuiPopupMenu(GuiEnvironment environment) : base(environment) { }

        public void Reset(Point pos, int width = 150) {
            this.Bounds = new Rectangle(pos, new Point(width, 300));
            base.Reset();
        }

        public void Show() {
            EndPreviousElement();
            // Bounds = new Rectangle(pos, new Point(150, 300));
            Bounds.Size = ContentSize;
            Showing = true;
            Environment.VisiblePanels.Add(this);
        }

        public void Dismiss() {
            Showing = false;
            Environment.VisiblePanels.Remove(this);
        }
        
        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(!Showing) return;
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                DestinationRectangle = Bounds.Grow(2),
                Color = new Color(45, 45, 48)
            });
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                DestinationRectangle = Bounds,
                Color = new Color(37, 37, 38)
            });
            
            
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.PopupMenu = this;
            }
            
            base.Render(renderer, layer);
        }
    }
}