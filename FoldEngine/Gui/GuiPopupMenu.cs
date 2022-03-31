﻿using System;
using EntryProject.Util;
using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui {
    public class GuiPopupMenu : GuiPanel {
        public bool Showing = false;
        private Action<GuiPopupMenu> _renderer; 
        
        public GuiPopupMenu(GuiEnvironment environment) : base(environment) { }

        public void Show(Point pos, Action<GuiPopupMenu> renderer, int width = 150) {
            this.Bounds = new Rectangle(pos, new Point(width, 300));
            _renderer = renderer;
            _renderer(this);
            
            Showing = true;
            Environment.VisiblePanels.Add(this);
        }

        public void Dismiss() {
            Showing = false;
            Environment.VisiblePanels.Remove(this);
        }
        
        public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default) {
            if(!Showing) return;
            Reset();
            _renderer(this);
            EndPreviousElement();
            // Bounds = new Rectangle(pos, new Point(150, 300));
            Bounds.Size = ContentSize;
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                DestinationRectangle = Bounds.Grow(2).Translate(offset),
                Color = new Color(45, 45, 48)
            });
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                DestinationRectangle = Bounds.Translate(offset),
                Color = new Color(37, 37, 38)
            });
            
            
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.PopupMenu = this;
            }
            
            base.Render(renderer, layer, offset);
        }
    }
}