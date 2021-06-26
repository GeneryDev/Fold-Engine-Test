using System;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Gui {
    public class ViewTab : GuiElement {
        private const float LabelSize = 7;
        
        private EditorView _view;
        private ViewListPanel _viewList;
        private RenderedText _renderedName;

        private Point _dragStart;
        private bool _dragging = false;

        public const int TabHeight = 14;

        public override Point Displacement => new Point(Bounds.Width + Margin, 0);

        public override void Reset(GuiPanel parent) {
            _view = null;
            _renderedName = default;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            _renderedName = Parent.RenderString(_view.Name, LabelSize);
            Margin = 2;
            Bounds.Width = 16 + (int)_renderedName.Width + Margin*2;
            Bounds.Height = TabHeight;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.DeepestElement = this;
            }
            
            if(Pressed(MouseEvent.LeftButton) && Environment.HoverTargetPrevious.DeepestElement != this) {
                _dragging = true;
            }

            if(_dragging) {
                if(Parent.Environment is EditorEnvironment editorEnvironment) {
                    editorEnvironment.DraggingViewTab = this;
                    // if(editorEnvironment.DropTarget != null) {
                    //     _viewList.RemoveView(_view);
                    //     editorEnvironment.DropTarget.AddView(_view);
                    // }
                }
            }

            var renderingBounds = Bounds;
            if(_dragging) {
                renderingBounds = new Rectangle(Bounds.Location - Bounds.Center + Parent.Environment.MousePos, Bounds.Size);
            }

            Color defaultColor = _viewList.ActiveView == _view ? new Color(37, 37, 38) : Color.Transparent;
            
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = Pressed(MouseEvent.LeftButton) ? new Color(63, 63, 70) : Environment.HoverTargetPrevious.DeepestElement == this ? Color.CornflowerBlue : defaultColor,
                DestinationRectangle = renderingBounds
            });
            
            int x = renderingBounds.X + Margin;
            
            var iconSize = new Point(8, 8);

            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.Textures[_view.Icon],
                DestinationRectangle = new Rectangle(x, renderingBounds.Center.Y - iconSize.Y/2,  iconSize.X, iconSize.Y)
            });
            x += iconSize.X;
            x += 4;
            _renderedName.DrawOnto(layer.Surface, new Point(x, renderingBounds.Center.Y + 3), Color.White);
        }

        public ViewTab View(EditorView view, ViewListPanel viewList) {
            _view = view;
            _viewList = viewList;
            return this;
        }

        public override void OnMouseReleased(MouseEvent e) {
            if(e.Button == MouseEvent.LeftButton) {
                if(!_dragging) {
                    _viewList.ActiveView = _view;
                } else {
                    if(Parent.Environment is EditorEnvironment editorEnvironment) {
                        editorEnvironment.DraggingViewTab = null;
                        if(editorEnvironment.HoverViewListPanel != null) {
                            _viewList.RemoveView(_view);
                            editorEnvironment.HoverViewListPanel.AddView(_view);
                        }
                    }
                }
                _dragging = false;
            }
        }
    }
}