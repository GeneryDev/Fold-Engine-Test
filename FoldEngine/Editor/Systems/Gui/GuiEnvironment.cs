using System;
using System.Collections.Generic;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Editor.Views {
    public abstract class GuiEnvironment {
        public Point MousePos;
        public ButtonAction MouseLeft = ButtonAction.Default;
        public ButtonAction MouseRight = ButtonAction.Default;
            
        public abstract List<GuiPanel> VisiblePanels { get; }

        private GuiPanel _pressedPanel;
        
        public IRenderingUnit Renderer { get; set; }
        public IRenderingLayer Layer { get; set; }

        public virtual void Input(InputUnit inputUnit) {
            if(MouseLeft == ButtonAction.Default) {
                MouseLeft = new ButtonAction(inputUnit.Devices.Mouse.LeftButton);
                MouseRight = new ButtonAction(inputUnit.Devices.Mouse.RightButton);
            }

            MousePos = Mouse.GetState().Position;
            if(Layer != null) MousePos = Layer.WindowToLayer(MousePos.ToVector2()).ToPoint();
            if(MouseLeft.Pressed) {
                for(int i = VisiblePanels.Count - 1; i >= 0; i--) {
                    GuiPanel panel = VisiblePanels[i];
                    if(panel.Visible && panel.Bounds.Contains(MousePos)) {
                        _pressedPanel = panel;
                        panel.OnMousePressed(MousePos);
                        break;
                    }
                }
            } else if(MouseLeft.Released) {
                _pressedPanel?.OnMouseReleased(MousePos);
                _pressedPanel = null;
            }
        }
        
        public virtual void Update() {
            
        }

        public virtual void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            Renderer = renderer;
            Layer = layer;
        }
    }
}