using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.ImmediateGui;

public class ViewTab : GuiElement
{
    private const float LabelSize = 7;

    public const int TabHeight = 14;
    private bool _dragging;

    private Point _dragStart;
    private RenderedText _renderedName;

    private EditorView _view;
    private ViewListPanel _viewList;

    public override void Reset(GuiPanel parent)
    {
        _view = null;
        _renderedName = default;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        _renderedName = Parent.RenderString(_view.Name, LabelSize);
        Margin = 2;
        Bounds.Width = 16 + _renderedName.Width + Margin * 2;
        Bounds.Height = TabHeight;
    }

    public override void Displace(ref Point layoutPosition)
    {
        layoutPosition += new Point(Bounds.Width + Margin, 0);
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (Bounds.Contains(Environment.MousePos)) Environment.HoverTarget.Element = this;

        if (Pressed(MouseEvent.LeftButton) && Environment.HoverTargetPrevious.Element != this) _dragging = true;

        Rectangle renderingBounds = Bounds;
        if (_dragging)
        {
            offset += Parent.Environment.MousePos - Bounds.Center;
            renderingBounds = renderingBounds.Translate(offset);
            layer = renderer.RootGroup["editor_gui_overlay"];
        }


        Color defaultColor = _viewList.ActiveView == _view ? new Color(37, 37, 38) : Color.Transparent;

        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = Pressed(MouseEvent.LeftButton) ? new Color(63, 63, 70) :
                Rollover ? Color.CornflowerBlue : defaultColor,
            DestinationRectangle = renderingBounds
        });

        int x = renderingBounds.X + Margin;

        var iconSize = new Point(8, 8);

        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = Environment.EditorResources.Get<Texture>(ref _view.Icon),
            DestinationRectangle =
                new Rectangle(x, renderingBounds.Center.Y - iconSize.Y / 2, iconSize.X, iconSize.Y)
        });
        x += iconSize.X;
        x += 4;
        _renderedName.DrawOnto(layer.Surface, new Point(x, renderingBounds.Center.Y + 3), Color.White);
    }

    public ViewTab View(EditorView view, ViewListPanel viewList)
    {
        _view = view;
        _viewList = viewList;
        return this;
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        if (e.Button == MouseEvent.LeftButton)
        {
            if (!_dragging)
            {
                _viewList.ActiveView = _view;
            }
            else
            {
                if (Parent.Environment is EditorEnvironment editorEnvironment)
                {
                    if (editorEnvironment.HoverViewListPanel != null)
                    {
                        _viewList.RemoveView(_view);
                        editorEnvironment.HoverViewListPanel.AddView(_view);
                    }
                }
            }

            _dragging = false;
        }
    }
}