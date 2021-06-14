using System;
using System.Collections.Generic;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Editor.Systems {
    public class GuiEnvironment {
        public Point MousePos;
        public ButtonAction MouseLeft = ButtonAction.Default;
        public ButtonAction MouseRight = ButtonAction.Default;
            
        private List<GuiPanel> _allPanels = new List<GuiPanel>();

        private GuiPanel _pressedPanel;
        
        public ActionPerformer PerformAction;
        public IRenderingLayer Layer { get; set; }

        public void Input(InputUnit inputUnit) {
            if(MouseLeft == ButtonAction.Default) {
                MouseLeft = new ButtonAction(inputUnit.Devices.Mouse.LeftButton);
                MouseRight = new ButtonAction(inputUnit.Devices.Mouse.RightButton);
            }

            MousePos = Mouse.GetState().Position;
            if(Layer != null) MousePos = Layer.WindowToLayer(MousePos.ToVector2()).ToPoint();
            if(MouseLeft.Pressed) {
                for(int i = _allPanels.Count - 1; i >= 0; i--) {
                    GuiPanel panel = _allPanels[i];
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
        
        public void Update() {
            
        }

        public GuiPanel Panel(Rectangle bounds) {
            var panel = new GuiPanel(this) {
                Bounds = bounds
            };
            _allPanels.Add(panel);
            return panel;
        }

        public delegate void ActionPerformer(int actionId, long data);
    }
}