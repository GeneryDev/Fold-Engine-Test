using System;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor {
    public class ViewTab : GuiElement {
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
            _renderedName = Parent.RenderString(_view.Name);
            Margin = 2;
            Bounds.Width = 16 + (int)_renderedName.Width + Margin*2;
            Bounds.Height = TabHeight;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Pressed && !Bounds.Contains(Parent.Environment.MousePos)) {
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
                Color = Pressed ? new Color(63, 63, 70) : Bounds.Contains(Parent.Environment.MousePos) ? Color.CornflowerBlue : defaultColor,
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
            _renderedName.DrawOnto(layer.Surface, new Point(x, renderingBounds.Center.Y + 3), Color.White, 1);
        }

        public ViewTab View(EditorView view, ViewListPanel viewList) {
            _view = view;
            _viewList = viewList;
            return this;
        }

        public override void OnMousePressed(Point pos) {
            
            base.OnMousePressed(pos);
        }

        public override void OnMouseReleased(Point pos) {
            if(!_dragging) {
                _viewList.ActiveView = _view;
            } else {
                if(Parent.Environment is EditorEnvironment editorEnvironment) {
                    editorEnvironment.DraggingViewTab = null;
                    if(editorEnvironment.HoverTarget.ViewListPanel != null) {
                        _viewList.RemoveView(_view);
                        editorEnvironment.HoverTarget.ViewListPanel.AddView(_view);
                    }
                }
            }
            _dragging = false;
        }
    }
}