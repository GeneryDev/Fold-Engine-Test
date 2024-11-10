using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Editor.Tools;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keyboard = FoldEngine.Input.Keyboard;
using Mouse = FoldEngine.Input.Mouse;

namespace FoldEngine.Editor.Gui;

public class EditorEnvironment : GuiEnvironment
{
    public const int FrameBorder = 4;
    public const int FrameMargin = 8;

    public readonly List<EditorTool> Tools = new List<EditorTool>();


    public EditorBase EditorBase;
    public TransactionManager<Scene> TransactionManager => EditorBase.CurrentTab.SceneTransactions;
    public List<EditorView> AllViews = new List<EditorView>();
    public EditorTool ForcedTool;
    public EditorTool SelectedTool;
    
    public ref EditorTab EditingTab => ref EditorBase.CurrentTab;

    public EditorEnvironment(EditorBase editor) : base(editor.Scene)
    {
        EditorBase = editor;

        NorthPanel = new BorderPanel(this, -Vector2.UnitY);
        SouthPanel = new BorderPanel(this, Vector2.UnitY);
        WestPanel = new BorderPanel(this, -Vector2.UnitX);
        EastPanel = new BorderPanel(this, Vector2.UnitX);
        CenterPanel = new BorderPanel(this, Vector2.Zero);

        VisiblePanels.Add(NorthPanel);
        VisiblePanels.Add(SouthPanel);
        VisiblePanels.Add(WestPanel);
        VisiblePanels.Add(EastPanel);
        VisiblePanels.Add(CenterPanel);

        SetupControlScheme();
        SetupTools();
    }

    public sealed override List<GuiPanel> VisiblePanels { get; } = new List<GuiPanel>();

    public ViewListPanel HoverViewListPanel { get; set; }

    public EditorTool ActiveTool => ForcedTool ?? SelectedTool;

    private void SetupControlScheme()
    {
        Keyboard keyboard = Core.InputUnit.Devices.Keyboard;
        Mouse mouse = Core.InputUnit.Devices.Mouse;

        ControlScheme.PutAction("editor.undo",
            new ButtonAction(keyboard[Keys.Z]) { Repeat = true }.Modifiers(keyboard[Keys.LeftControl]));
        ControlScheme.PutAction("editor.redo",
            new ButtonAction(keyboard[Keys.Y]) { Repeat = true }.Modifiers(keyboard[Keys.LeftControl]));

        ControlScheme.PutAction("editor.field.select_all",
            new ButtonAction(keyboard[Keys.A]) { Repeat = true }.Modifiers(keyboard[Keys.LeftControl]));
        ControlScheme.PutAction("editor.field.caret.left", new ButtonAction(keyboard[Keys.Left]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.right", new ButtonAction(keyboard[Keys.Right]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.up", new ButtonAction(keyboard[Keys.Up]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.down", new ButtonAction(keyboard[Keys.Down]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.home", new ButtonAction(keyboard[Keys.Home]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.end", new ButtonAction(keyboard[Keys.End]) { Repeat = true });

        ControlScheme.PutAction("editor.field.caret.debug", new ButtonAction(keyboard[Keys.F1]) { Repeat = true });

        ControlScheme.PutAction("editor.zoom.in", new ChangeAction(mouse.ScrollWheel, 0.5f, null));
        ControlScheme.PutAction("editor.zoom.out", new ChangeAction(mouse.ScrollWheel, null, -1.5f));

        ControlScheme.PutAction("editor.movement.axis.x",
            new AnalogAction(() => (keyboard[Keys.Left].Down ? -1 : 0) + (keyboard[Keys.Right].Down ? 1 : 0)));
        ControlScheme.PutAction("editor.movement.axis.y",
            new AnalogAction(() => (keyboard[Keys.Down].Down ? -1 : 0) + (keyboard[Keys.Up].Down ? 1 : 0)));

        ControlScheme.PutAction("editor.movement.faster", new ButtonAction(keyboard[Keys.LeftShift]));
    }

    private void SetupTools()
    {
        Tools.Add(new HandTool(this));
        Tools.Add(new MoveTool(this));
        Tools.Add(new ScaleTool(this));
        Tools.Add(new RotateTool(this));
        Tools.Add(SelectedTool = new SelectTool(this));
    }

    public override void Input(InputUnit inputUnit)
    {
        base.Input(inputUnit);

        if (ControlScheme.Get<ButtonAction>("editor.undo").Consume()) EditorBase.Undo();
        if (ControlScheme.Get<ButtonAction>("editor.redo").Consume()) EditorBase.Redo();

        if (HoverTarget.ScrollablePanel != null)
            if (HoverTarget.ScrollablePanel.IsAncestorOf(HoverTarget.Element))
            {
                if (ControlScheme.Get<ChangeAction>("editor.zoom.in"))
                    HoverTarget.ScrollablePanel.Scroll(1);
                else if (ControlScheme.Get<ChangeAction>("editor.zoom.out")) HoverTarget.ScrollablePanel.Scroll(-1);
            }
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer baseLayer, IRenderingLayer overlayLayer)
    {
        base.Render(renderer, baseLayer, overlayLayer);

        HoverViewListPanel = default;

        renderer.Groups["editor"].Dependencies[0].Group.Size = default;

        if (!LayoutValidated)
        {
            CenterPanel.Bounds = new Rectangle(SizeWest, SizeNorth, renderer.WindowSize.X - SizeWest - SizeEast,
                renderer.WindowSize.Y - SizeNorth - SizeSouth);
            LayoutValidated = true;
        }

        CenterPanel.Render(renderer, baseLayer);

        {
            var bounds = new Rectangle(0, 0, baseLayer.LayerSize.X, SizeNorth);
            if (!_cornerBiasNorthWest)
            {
                bounds.X += SizeWest;
                bounds.Width -= SizeWest;
            }

            if (!_cornerBiasNorthEast) bounds.Width -= SizeEast;
            NorthPanel.Bounds = bounds;
            NorthPanel.Render(renderer, baseLayer);
        }
        {
            var bounds = new Rectangle(0, baseLayer.LayerSize.Y - SizeSouth, baseLayer.LayerSize.X, SizeSouth);
            if (!_cornerBiasSouthWest)
            {
                bounds.X += SizeWest;
                bounds.Width -= SizeWest;
            }

            if (!_cornerBiasSouthEast) bounds.Width -= SizeEast;
            SouthPanel.Bounds = bounds;
            SouthPanel.Render(renderer, baseLayer);
        }

        {
            var bounds = new Rectangle(0, 0, SizeWest, baseLayer.LayerSize.Y);
            if (_cornerBiasNorthWest)
            {
                bounds.Y += SizeNorth;
                bounds.Height -= SizeNorth;
            }

            if (_cornerBiasSouthWest) bounds.Height -= SizeSouth;
            WestPanel.Bounds = bounds;
            WestPanel.Render(renderer, baseLayer);
        }
        {
            var bounds = new Rectangle(baseLayer.LayerSize.X - SizeEast, 0, SizeEast, baseLayer.LayerSize.Y);
            if (_cornerBiasNorthEast)
            {
                bounds.Y += SizeNorth;
                bounds.Height -= SizeNorth;
            }

            if (_cornerBiasSouthEast) bounds.Height -= SizeSouth;
            EastPanel.Bounds = bounds;
            EastPanel.Render(renderer, baseLayer);
        }

        foreach (GuiElement panel in DraggingElements) panel?.Render(renderer, overlayLayer);

        if (ContextMenu.Showing) ContextMenu.Render(renderer, overlayLayer);
    }

    public void AddView<T>(BorderPanel preferredPanel = null) where T : EditorView, new()
    {
        var view = new T { Scene = Scene };
        view.Initialize();

        AllViews.Add(view);

        preferredPanel?.ViewLists[0].AddView(view);
    }

    public T GetView<T>() where T : EditorView
    {
        foreach (EditorView view in AllViews)
            if (view is T viewT)
                return viewT;
        return null;
    }

    public bool SwitchToView<T>() where T : EditorView
    {
        return SwitchToView(GetView<T>());
    }

    public bool SwitchToView(EditorView view)
    {
        foreach (BorderPanel borderPanel in VisiblePanels)
        {
            foreach (ViewListPanel viewListPanel in borderPanel.ViewLists)
                if (viewListPanel.ContainsView(view))
                {
                    viewListPanel.SwitchToView(view);
                    return true;
                }
        }

        return false;
    }

    #region Dock Size Properties

    public int SizeNorth
    {
        get => _sizeNorth;
        set
        {
            if (_sizeNorth != value) LayoutValidated = false;
            _sizeNorth = value;
        }
    }

    public int SizeSouth
    {
        get => _sizeSouth;
        set
        {
            if (_sizeSouth != value) LayoutValidated = false;
            _sizeSouth = value;
        }
    }

    public int SizeWest
    {
        get => _sizeWest;
        set
        {
            if (_sizeWest != value) LayoutValidated = false;
            _sizeWest = value;
        }
    }

    public int SizeEast
    {
        get => _sizeEast;
        set
        {
            if (_sizeEast != value) LayoutValidated = false;
            _sizeEast = value;
        }
    }

    public bool CornerBiasNorthWest
    {
        get => _cornerBiasNorthWest;
        set
        {
            if (_cornerBiasNorthWest != value) LayoutValidated = false;
            _cornerBiasNorthWest = value;
        }
    }

    public bool CornerBiasNorthEast
    {
        get => _cornerBiasNorthEast;
        set
        {
            if (_cornerBiasNorthEast != value) LayoutValidated = false;
            _cornerBiasNorthEast = value;
        }
    }

    public bool CornerBiasSouthWest
    {
        get => _cornerBiasSouthWest;
        set
        {
            if (_cornerBiasSouthWest != value) LayoutValidated = false;
            _cornerBiasSouthWest = value;
        }
    }

    public bool CornerBiasSouthEast
    {
        get => _cornerBiasSouthEast;
        set
        {
            if (_cornerBiasSouthEast != value) LayoutValidated = false;
            _cornerBiasSouthEast = value;
        }
    }

    #endregion

    #region Dock Backing Fields

    public bool LayoutValidated;
    private int _sizeNorth = 96;
    private int _sizeSouth = 128;
    private int _sizeWest = 256;
    private int _sizeEast = 360;

    private bool _cornerBiasNorthWest = true;
    private bool _cornerBiasNorthEast = true;
    private bool _cornerBiasSouthWest = true;
    private bool _cornerBiasSouthEast;

    public BorderPanel NorthPanel;
    public BorderPanel SouthPanel;
    public BorderPanel WestPanel;
    public BorderPanel EastPanel;

    public BorderPanel CenterPanel;

    #endregion
}

public class BorderPanel : GuiPanel
{
    public Vector2 Side;

    public List<ViewListPanel> ViewLists = new List<ViewListPanel>();

    public BorderPanel(EditorEnvironment editorEnvironment, Vector2 side) : base(editorEnvironment)
    {
        Side = side;
        ViewLists.Add(new ViewListPanel(editorEnvironment));
    }

    private void RenderBackground(IRenderingUnit renderer, IRenderingLayer layer, Point offset)
    {
        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            DestinationRectangle = Bounds,
            Color = new Color(45, 45, 48)
        });
        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            DestinationRectangle = Bounds.Grow(-EditorEnvironment.FrameBorder),
            Color = new Color(37, 37, 38)
        });
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        Reset();
        RenderBackground(renderer, layer, offset);

        for (int i = 0; i < ViewLists.Count; i++)
        {
            ViewListPanel viewList = ViewLists[i];
            ResetLayoutPosition();
            LayoutPosition.X += i * Bounds.Width / ViewLists.Count;
            Element(viewList);
            viewList.Bounds = Bounds;
            viewList.Bounds.Width = Bounds.Width / ViewLists.Count;
            viewList.Bounds = viewList.Bounds.Grow(-EditorEnvironment.FrameBorder);
        }

        ResetLayoutPosition();

        if (Side != default) Element<GuiResizer>().Side(-Side);

        base.Render(renderer, layer, offset);
    }
}

public class ViewListPanel : GuiPanel
{
    public EditorView ActiveView;

    public List<EditorView> Views = new List<EditorView>();

    public ViewListPanel(GuiEnvironment environment) : base(environment)
    {
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (Environment is EditorEnvironment editorEnvironment && Bounds.Contains(Environment.MousePos))
            editorEnvironment.HoverViewListPanel = this;

        Color? bgColor = ActiveView?.BackgroundColor;
        if (bgColor.HasValue)
            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = renderer.WhiteTexture,
                Color = bgColor.Value,
                DestinationRectangle = Bounds.Translate(offset)
            });

        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = new Color(45, 45, 48),
            DestinationRectangle =
                new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, ViewTab.TabHeight).Translate(offset)
        });

        Reset();
        foreach (EditorView view in Views) Element<ViewTab>().View(view, this);
        ResetLayoutPosition();

        if (ActiveView == null && Views.Count > 0) ActiveView = Views[0];

        if (ActiveView != null)
        {
            ActiveView.EnsurePanelExists(Environment);

            Element(ActiveView.ContentPanel);

            ActiveView.ContentPanel.Bounds = Bounds;
            ActiveView.ContentPanel.Bounds.Y += ViewTab.TabHeight;
            ActiveView.ContentPanel.Bounds.Height -= ViewTab.TabHeight;
            if (ActiveView.UseMargin)
                ActiveView.ContentPanel.Bounds =
                    ActiveView.ContentPanel.Bounds.Grow(-EditorEnvironment.FrameMargin);

            ActiveView.ContentPanel.Reset();
            ActiveView.Render(renderer);
        }

        base.Render(renderer, layer, offset);
    }

    public void AddView(EditorView view)
    {
        Views.Add(view);
        ActiveView = view;
    }

    public void RemoveView(EditorView view)
    {
        Views.Remove(view);
        if (ActiveView == view)
        {
            if (Views.Count > 0)
                ActiveView = Views[Views.Count - 1];
            else
                ActiveView = null;
        }
    }

    public bool ContainsView(EditorView view)
    {
        return Views.Contains(view);
    }

    public void SwitchToView(EditorView view)
    {
        ActiveView = view;
    }
}

public class GuiResizer : GuiElement
{
    private Vector2 _side;

    public override void Reset(GuiPanel parent)
    {
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        Bounds.Width = parent.Bounds.Width;
        Bounds.Height = parent.Bounds.Height;

        if (_side.X != 0)
        {
            Bounds.Width = EditorEnvironment.FrameMargin;
            if (_side.X > 0) Bounds.X += parent.Bounds.Width - Bounds.Width;
        }

        if (_side.Y != 0)
        {
            Bounds.Height = EditorEnvironment.FrameMargin;
            if (_side.Y > 0) Bounds.Y += parent.Bounds.Height - Bounds.Height;
        }
    }

    public override void Displace(ref Point layoutPosition)
    {
    }

    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (Bounds.Contains(Environment.MousePos)) Environment.HoverTarget.Element = this;

        var color = new Color(140, 140, 145);
        bool pressed = Pressed();
        if (pressed || Rollover)
        {
            Rectangle drawBounds = Bounds;
            if (_side.X != 0)
            {
                drawBounds.Width = 2;
                if (_side.X > 0) drawBounds.X += Bounds.Width - 2;
            }

            if (_side.Y != 0)
            {
                drawBounds.Height = 2;
                if (_side.Y > 0) drawBounds.Y += Bounds.Height - 2;
            }

            layer.Surface.Draw(new DrawRectInstruction
            {
                Texture = renderer.WhiteTexture,
                Color = pressed ? Color.White : color,
                DestinationRectangle = drawBounds
            });
        }

        if (pressed)
            if (Parent.Environment is EditorEnvironment environment)
            {
                if (_side == Vector2.UnitX)
                    environment.SizeWest = Math.Max(1, environment.MousePos.X + 1);
                else if (_side == -Vector2.UnitX)
                    environment.SizeEast = Math.Max(1, environment.BaseLayer.LayerSize.X - environment.MousePos.X);

                if (_side == Vector2.UnitY)
                    environment.SizeNorth = Math.Max(1, environment.MousePos.Y + 1);
                else if (_side == -Vector2.UnitY)
                    environment.SizeSouth = Math.Max(1, environment.BaseLayer.LayerSize.Y - environment.MousePos.Y);
            }
    }

    public GuiResizer Side(Vector2 side)
    {
        _side = side;
        return this;
    }
}