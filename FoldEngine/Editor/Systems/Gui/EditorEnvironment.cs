﻿using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Commands;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Systems {
    public class EditorEnvironment : GuiEnvironment {

        public const int FrameBorder = 4;
        public const int FrameMargin = 8;
        
        public sealed override List<GuiPanel> VisiblePanels { get; } = new List<GuiPanel>();

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

        public bool LayoutValidated = false;
        private int _sizeNorth = 50;
        private int _sizeSouth = 128;
        private int _sizeWest = 256;
        private int _sizeEast = 256;
        
        private bool _cornerBiasNorthWest = true;
        private bool _cornerBiasNorthEast = true;
        private bool _cornerBiasSouthWest = true;
        private bool _cornerBiasSouthEast = false;
        
        public GuiPanel NorthPanel;
        public GuiPanel SouthPanel;
        public GuiPanel WestPanel;
        public GuiPanel EastPanel;

        public EditorEnvironment() {
            PerformAction = (int actionId, long data) => {
                switch(actionId) {
                    case SceneEditor.Actions.ChangeToMenu: {
                        Console.WriteLine($"Change to view {data}");
                        // Owner.Events.Invoke(new ForceModalChangeEvent(_modalTypes[(int) data]));
                        break;
                    }
                    case SceneEditor.Actions.Save: {
                        Console.WriteLine("Save");
                        break;
                    }
                    case SceneEditor.Actions.ExpandCollapseEntity: {
                        // Owner.Systems.Get<EditorEntitiesList>().ExpandCollapseEntity(data);
                        break;
                    }
                    case SceneEditor.Actions.Test: {
                        // Owner.Core.CommandQueue.Enqueue(new SetWindowSizeCommand(new Point(1920, 1040)));
                        break;
                    }
                }
            };
            
            NorthPanel = new BorderPanel(this, -Vector2.UnitY);
            SouthPanel = new BorderPanel(this, Vector2.UnitY);
            WestPanel = new BorderPanel(this, -Vector2.UnitX);
            EastPanel = new BorderPanel(this, Vector2.UnitX);
            
            VisiblePanels.Add(NorthPanel);
            VisiblePanels.Add(SouthPanel);
            VisiblePanels.Add(WestPanel);
            VisiblePanels.Add(EastPanel);
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            base.Render(renderer, layer);

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

            if(!LayoutValidated) {
                renderer.Groups["editor"].Dependencies[0].Group.Size = new Point(renderer.WindowSize.X - SizeWest - SizeEast, renderer.WindowSize.Y - SizeNorth - SizeSouth);
                renderer.Groups["editor"].Dependencies[0].Destination = new Rectangle(SizeWest, SizeNorth, renderer.WindowSize.X - SizeWest - SizeEast, renderer.WindowSize.Y - SizeNorth - SizeSouth);
                LayoutValidated = true;
            }
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
    }

    public class BorderPanel : GuiPanel {

        public Vector2 Side;
        
        public BorderPanel(EditorEnvironment editorEnvironment, Vector2 side) : base(editorEnvironment) {
            this.Side = side;
        }

        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            Reset();
            Element<GuiResizer>().Side(-Side);
            if(Side == Vector2.UnitX) {
                Label("Owner.Name", 2).TextAlignment(-1).Icon(renderer.Textures["editor:cog"]);
                Button("Save");
                Separator();
                Button("Entities").Action(SceneEditor.Actions.ChangeToMenu, 1);
                Button("Systems").Action(SceneEditor.Actions.ChangeToMenu, 2);
                Button("Edit Save Data").Action(SceneEditor.Actions.Test, 0);
                Button("Quit");
            }
            
            base.Render(renderer, layer);
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
            var color = new Color(140, 140, 145);
            bool pressed = Pressed;
            if(pressed || Bounds.Contains(Parent.Environment.MousePos)) {
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
                        environment.SizeWest = environment.MousePos.X+1;
                    } else if(_side == -Vector2.UnitX) {
                        environment.SizeEast = environment.Layer.LayerSize.X - environment.MousePos.X;
                    }
                    
                    if(_side == Vector2.UnitY) {
                        environment.SizeNorth = environment.MousePos.Y+1;
                    } else if(_side == -Vector2.UnitY) {
                        environment.SizeSouth = environment.Layer.LayerSize.Y - environment.MousePos.Y;
                    }
                }
            }
        }

        public GuiResizer Side(Vector2 side) {
            _side = side;
            return this;
        }

        public override void OnMousePressed(Point pos) {
        }
    }
}