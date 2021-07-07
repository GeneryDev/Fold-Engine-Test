using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Commands;
using FoldEngine.Editor.Transactions;
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

namespace FoldEngine.Editor.Gui {
    public class EditorEnvironment : GuiEnvironment {

        public const int FrameBorder = 4;
        public const int FrameMargin = 8;

        public sealed override List<GuiPanel> VisiblePanels { get; } = new List<GuiPanel>();
        public Dictionary<Type, EditorView> AllViews = new Dictionary<Type, EditorView>();
        
        public ViewTab DraggingViewTab { get; set; }
        public ViewListPanel HoverViewListPanel { get; set; }

        #region Dock Size Properties

        

        public int SizeNorth {
            get => _sizeNorth;
            set {
                if(_sizeNorth != value) LayoutValidated = false;
                _sizeNorth = value;
            }
        }

        public int SizeSouth {
            get => _sizeSouth;
            set {
                if(_sizeSouth != value) LayoutValidated = false;
                _sizeSouth = value;
            }
        }

        public int SizeWest {
            get => _sizeWest;
            set {
                if(_sizeWest != value) LayoutValidated = false;
                _sizeWest = value;
            }
        }

        public int SizeEast {
            get => _sizeEast;
            set {
                if(_sizeEast != value) LayoutValidated = false;
                _sizeEast = value;
            }
        }

        public bool CornerBiasNorthWest {
            get => _cornerBiasNorthWest;
            set {
                if(_cornerBiasNorthWest != value) LayoutValidated = false;
                _cornerBiasNorthWest = value;
            }
        }

        public bool CornerBiasNorthEast {
            get => _cornerBiasNorthEast;
            set {
                if(_cornerBiasNorthEast != value) LayoutValidated = false;
                _cornerBiasNorthEast = value;
            }
        }

        public bool CornerBiasSouthWest {
            get => _cornerBiasSouthWest;
            set {
                if(_cornerBiasSouthWest != value) LayoutValidated = false;
                _cornerBiasSouthWest = value;
            }
        }

        public bool CornerBiasSouthEast {
            get => _cornerBiasSouthEast;
            set {
                if(_cornerBiasSouthEast != value) LayoutValidated = false;
                _cornerBiasSouthEast = value;
            }
        }
        #endregion

        #region Dock Backing Fields

        public bool LayoutValidated = false;
        private int _sizeNorth = 96;
        private int _sizeSouth = 128;
        private int _sizeWest = 256;
        private int _sizeEast = 360;
        
        private bool _cornerBiasNorthWest = true;
        private bool _cornerBiasNorthEast = true;
        private bool _cornerBiasSouthWest = true;
        private bool _cornerBiasSouthEast = false;
        
        public BorderPanel NorthPanel;
        public BorderPanel SouthPanel;
        public BorderPanel WestPanel;
        public BorderPanel EastPanel;
        
        public GuiPanel CenterPanel;

        #endregion
        
        public readonly TransactionManager<EditorEnvironment> TransactionManager;

        public EditorEnvironment(Scene scene) : base(scene) {
            TransactionManager = new TransactionManager<EditorEnvironment>(this);

            NorthPanel = new BorderPanel(this, -Vector2.UnitY);
            SouthPanel = new BorderPanel(this, Vector2.UnitY);
            WestPanel = new BorderPanel(this, -Vector2.UnitX);
            EastPanel = new BorderPanel(this, Vector2.UnitX);
            CenterPanel = new GuiPanel(this);
            
            VisiblePanels.Add(NorthPanel);
            VisiblePanels.Add(SouthPanel);
            VisiblePanels.Add(WestPanel);
            VisiblePanels.Add(EastPanel);
            VisiblePanels.Add(CenterPanel);
            
            SetupControlScheme();
        }

        private void SetupControlScheme() {
            Keyboard keyboard = Scene.Core.InputUnit.Devices.Keyboard;
            
            ControlScheme.PutAction("editor.undo", new ButtonAction(keyboard[Keys.Z]) {Repeat = true}.Modifiers(keyboard[Keys.LeftControl]));
            ControlScheme.PutAction("editor.redo", new ButtonAction(keyboard[Keys.Y]) {Repeat = true}.Modifiers(keyboard[Keys.LeftControl]));
            
            ControlScheme.PutAction("editor.field.select_all", new ButtonAction(keyboard[Keys.A]) {Repeat = true}.Modifiers(keyboard[Keys.LeftControl]));
            ControlScheme.PutAction("editor.field.caret.left", new ButtonAction(keyboard[Keys.Left]) {Repeat = true});
            ControlScheme.PutAction("editor.field.caret.right", new ButtonAction(keyboard[Keys.Right]) {Repeat = true});
            ControlScheme.PutAction("editor.field.caret.up", new ButtonAction(keyboard[Keys.Up]) {Repeat = true});
            ControlScheme.PutAction("editor.field.caret.down", new ButtonAction(keyboard[Keys.Down]) {Repeat = true});
            ControlScheme.PutAction("editor.field.caret.home", new ButtonAction(keyboard[Keys.Home]) {Repeat = true});
            ControlScheme.PutAction("editor.field.caret.end", new ButtonAction(keyboard[Keys.End]) {Repeat = true});
            
            ControlScheme.PutAction("editor.field.caret.debug", new ButtonAction(keyboard[Keys.F1]) {Repeat = true});
        }

        public override void Input(InputUnit inputUnit) {
            base.Input(inputUnit);
            
            if(ControlScheme.Get<ButtonAction>("editor.undo").Consume()) TransactionManager.Undo();
            if(ControlScheme.Get<ButtonAction>("editor.redo").Consume()) TransactionManager.Redo();
            
            if(HoverTarget.ScrollablePanel != null) {
                if(HoverTarget.ScrollablePanel.IsAncestorOf(HoverTarget.Element)) {
                    if(Scene.Core.InputUnit.Players[0].Get<ChangeAction>("zoom.in")) {
                        HoverTarget.ScrollablePanel.Scroll(1);
                    } else if(Scene.Core.InputUnit.Players[0].Get<ChangeAction>("zoom.out")) {
                        HoverTarget.ScrollablePanel.Scroll(-1);
                    }
                }
            }
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            base.Render(renderer, layer);

            HoverViewListPanel = default;

            {
                var bounds = new Rectangle(0, 0, layer.LayerSize.X, SizeNorth);
                if(!_cornerBiasNorthWest) {
                    bounds.X += SizeWest;
                    bounds.Width -= SizeWest;
                }
                if(!_cornerBiasNorthEast) {
                    bounds.Width -= SizeEast;
                }
                NorthPanel.Bounds = bounds;
                RenderBackground(bounds, renderer, layer);
                NorthPanel.Render(renderer, layer);
            }
            {
                var bounds = new Rectangle(0, layer.LayerSize.Y - SizeSouth, layer.LayerSize.X, SizeSouth);
                if(!_cornerBiasSouthWest) {
                    bounds.X += SizeWest;
                    bounds.Width -= SizeWest;
                }
                if(!_cornerBiasSouthEast) {
                    bounds.Width -= SizeEast;
                }
                SouthPanel.Bounds = bounds;
                RenderBackground(bounds, renderer, layer);
                SouthPanel.Render(renderer, layer);
            }

            {
                var bounds = new Rectangle(0, 0, SizeWest, layer.LayerSize.Y);
                if(_cornerBiasNorthWest) {
                    bounds.Y += SizeNorth;
                    bounds.Height -= SizeNorth;
                }
                if(_cornerBiasSouthWest) {
                    bounds.Height -= SizeSouth;
                }
                WestPanel.Bounds = bounds;
                RenderBackground(bounds, renderer, layer);
                WestPanel.Render(renderer, layer);
            }
            {
                var bounds = new Rectangle(layer.LayerSize.X - SizeEast, 0, SizeEast, layer.LayerSize.Y);
                if(_cornerBiasNorthEast) {
                    bounds.Y += SizeNorth;
                    bounds.Height -= SizeNorth;
                }
                if(_cornerBiasSouthEast) {
                    bounds.Height -= SizeSouth;
                }
                EastPanel.Bounds = bounds;
                RenderBackground(bounds, renderer, layer);
                EastPanel.Render(renderer, layer);
            }
            
            DraggingViewTab?.Render(renderer, layer);

            if(ContextMenu.Showing) {
                ContextMenu.Render(renderer, layer);
            }

            if(!LayoutValidated) {
                renderer.Groups["editor"].Dependencies[0].Group.Size = new Point(renderer.WindowSize.X - SizeWest - SizeEast, renderer.WindowSize.Y - SizeNorth - SizeSouth);
                renderer.Groups["editor"].Dependencies[0].Destination = new Rectangle(SizeWest, SizeNorth, renderer.WindowSize.X - SizeWest - SizeEast, renderer.WindowSize.Y - SizeNorth - SizeSouth);
                CenterPanel.Bounds = new Rectangle(SizeWest, SizeNorth, renderer.WindowSize.X - SizeWest - SizeEast,
                    renderer.WindowSize.Y - SizeNorth - SizeSouth);
                LayoutValidated = true;
            }
            CenterPanel.Render(renderer, layer);
        }

        private void RenderBackground(Rectangle rectangle, IRenderingUnit renderer, IRenderingLayer layer) {
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                DestinationRectangle = rectangle,
                Color = new Color(45, 45, 48)
            });
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                DestinationRectangle = rectangle.Grow(-FrameBorder),
                Color = new Color(37, 37, 38)
            });
        }

        public void AddView<T>(BorderPanel preferredPanel = null) where T : EditorView, new() {
            T view = new T {Scene = Scene};
            view.Initialize();

            AllViews[view.GetType()] = view;

            preferredPanel?.ViewLists[0].AddView(view);
        }

        public T GetView<T>() where T : EditorView {
            return AllViews[typeof(T)] as T;
        }
    }

    public class BorderPanel : GuiPanel {

        public Vector2 Side;

        public List<ViewListPanel> ViewLists = new List<ViewListPanel>();
        
        public BorderPanel(EditorEnvironment editorEnvironment, Vector2 side) : base(editorEnvironment) {
            this.Side = side;
            ViewLists.Add(new ViewListPanel(editorEnvironment));
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            Reset();
            
            for(int i = 0; i < ViewLists.Count; i++) {
                ViewListPanel viewList = ViewLists[i];
                ResetLayoutPosition();
                LayoutPosition.X += i * Bounds.Width / ViewLists.Count;
                Element(viewList);
                viewList.Bounds = Bounds;
                viewList.Bounds.Width = Bounds.Width / ViewLists.Count;
                viewList.Bounds = viewList.Bounds.Grow(-EditorEnvironment.FrameBorder);
            }
            
            ResetLayoutPosition();
            
            Element<GuiResizer>().Side(-Side);
            
            base.Render(renderer, layer);
        }
    }

    public class ViewListPanel : GuiPanel {
        
        public List<EditorView> Views = new List<EditorView>();
        public EditorView ActiveView = null;
        
        public ViewListPanel(GuiEnvironment environment) : base(environment) { }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Environment is EditorEnvironment editorEnvironment && Bounds.Contains(Environment.MousePos)) {
                editorEnvironment.HoverViewListPanel = this;
            }
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = new Color(45, 45, 48),
                DestinationRectangle = new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, ViewTab.TabHeight)
            });
            
            Reset();
            foreach(EditorView view in Views) {
                Element<ViewTab>().View(view, this);
            }
            ResetLayoutPosition();

            if(ActiveView == null && Views.Count > 0) {
                ActiveView = Views[0];
            }

            if(ActiveView != null) {
                if(ActiveView.ContentPanel == null) {
                    ActiveView.ContentPanel = new GuiPanel(Environment);
                }
                
                Element(ActiveView.ContentPanel);

                ActiveView.ContentPanel.Bounds = Bounds;
                ActiveView.ContentPanel.Bounds.Y += ViewTab.TabHeight;
                ActiveView.ContentPanel.Bounds.Height -= ViewTab.TabHeight;
                ActiveView.ContentPanel.Bounds = ActiveView.ContentPanel.Bounds.Grow(-EditorEnvironment.FrameMargin);

                
                ActiveView.ContentPanel.Reset();
                ActiveView.Render(renderer);
            }
            
            base.Render(renderer, layer);
        }

        public void AddView(EditorView view) {
            Views.Add(view);
            ActiveView = view;
        }

        public void RemoveView(EditorView view) {
            Views.Remove(view);
            if(ActiveView == view) {
                if(Views.Count > 0) {
                    ActiveView = Views[Views.Count - 1];
                } else {
                    ActiveView = null;
                }
            }
        }
    }

    public class GuiResizer : GuiElement {
        private Vector2 _side;

        public override Point Displacement => Point.Zero;

        public override void Reset(GuiPanel parent) {
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
            Bounds.Height = parent.Bounds.Height;

            if(_side.X != 0) {
                Bounds.Width = EditorEnvironment.FrameMargin;
                if(_side.X > 0) Bounds.X += parent.Bounds.Width - Bounds.Width;
            }

            if(_side.Y != 0) {
                Bounds.Height = EditorEnvironment.FrameMargin;
                if(_side.Y > 0) Bounds.Y += parent.Bounds.Height - Bounds.Height;
            }
            
        }
        
        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.Element = this;
            }
            
            var color = new Color(140, 140, 145);
            bool pressed = Pressed();
            if(pressed || Rollover) {
                Rectangle drawBounds = Bounds;
                if(_side.X != 0) {
                    drawBounds.Width = 2;
                    if(_side.X > 0) drawBounds.X += Bounds.Width - 2;
                }
                if(_side.Y != 0) {
                    drawBounds.Height = 2;
                    if(_side.Y > 0) drawBounds.Y += Bounds.Height - 2;
                }
                layer.Surface.Draw(new DrawRectInstruction() {
                    Texture = renderer.WhiteTexture,
                    Color = pressed ? Color.White : color,
                    DestinationRectangle = drawBounds
                });
            }
            if(pressed) {
                if(Parent.Environment is EditorEnvironment environment) {
                    if(_side == Vector2.UnitX) {
                        environment.SizeWest = Math.Max(1,environment.MousePos.X+1);
                    } else if(_side == -Vector2.UnitX) {
                        environment.SizeEast = Math.Max(1,environment.Layer.LayerSize.X - environment.MousePos.X);
                    }
                    
                    if(_side == Vector2.UnitY) {
                        environment.SizeNorth = Math.Max(1,environment.MousePos.Y+1);
                    } else if(_side == -Vector2.UnitY) {
                        environment.SizeSouth = Math.Max(1,environment.Layer.LayerSize.Y - environment.MousePos.Y);
                    }
                }
            }
        }

        public GuiResizer Side(Vector2 side) {
            _side = side;
            return this;
        }
    }
}