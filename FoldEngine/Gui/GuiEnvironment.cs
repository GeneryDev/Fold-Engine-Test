using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Editor.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Gui {
    public abstract class GuiEnvironment {
        
        // Input
        public Point MousePos;
        public ButtonAction MouseLeft = ButtonAction.Default;
        public ButtonAction MouseMiddle = ButtonAction.Default;
        public ButtonAction MouseRight = ButtonAction.Default;
        private GuiPanel[] _pressedPanels = new GuiPanel[MouseEvent.MaxButtons];
        public HoverTarget HoverTargetPrevious;
        public HoverTarget HoverTarget;
            
        public abstract List<GuiPanel> VisiblePanels { get; }
        

        public GuiPopupMenu ContextMenu;
        
        public readonly ObjectPoolCollection<IGuiAction> ActionPool = new ObjectPoolCollection<IGuiAction>();
        
        // Renderer
        public IRenderingUnit Renderer { get; set; }
        public IRenderingLayer Layer { get; set; }
        

        public GuiEnvironment() {
            ContextMenu = new GuiPopupMenu(this);
        }
        
        public virtual void Input(InputUnit inputUnit) {
            if(MouseLeft == ButtonAction.Default) {
                MouseLeft = new ButtonAction(inputUnit.Devices.Mouse.LeftButton);
                MouseMiddle = new ButtonAction(inputUnit.Devices.Mouse.MiddleButton);
                MouseRight = new ButtonAction(inputUnit.Devices.Mouse.RightButton);
            }

            MousePos = Mouse.GetState().Position;
            if(Layer != null) MousePos = Layer.WindowToLayer(MousePos.ToVector2()).ToPoint();
            
            HandleMouseEvents(MouseLeft, MouseEvent.LeftButton);
            HandleMouseEvents(MouseMiddle, MouseEvent.MiddleButton);
            HandleMouseEvents(MouseRight, MouseEvent.RightButton);
        }

        private void HandleMouseEvents(ButtonAction mouseButton, int buttonIndex) {
            if(mouseButton.Pressed) {
                if(HoverTarget.PopupMenu != ContextMenu) {
                    DismissPopups();
                }
                
                for(int i = VisiblePanels.Count - 1; i >= 0; i--) {
                    GuiPanel panel = VisiblePanels[i];
                    if(panel.Visible && panel.Bounds.Contains(MousePos)) {
                        _pressedPanels[buttonIndex] = panel;
                        panel.OnMousePressed(new MouseEvent() {
                            Type = MouseEventType.PRESSED,
                            Position = MousePos,
                            Button = buttonIndex
                        });
                        break;
                    }
                }
            } else if(mouseButton.Released) {
                DismissPopups();
                
                _pressedPanels[buttonIndex]?.OnMouseReleased(new MouseEvent() {
                    Type = MouseEventType.RELEASED,
                    Position = MousePos,
                    Button = buttonIndex
                });
                _pressedPanels[buttonIndex] = null;
            }
        }

        public void DismissPopups() {
            if(ContextMenu.Showing) {
                ContextMenu.Dismiss();
            }
        }
        
        public virtual void Update() {
            
        }

        public virtual void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            Renderer = renderer;
            Layer = layer;

            HoverTargetPrevious = HoverTarget;
            HoverTarget = default;
        }
    }

    public struct HoverTarget {
        public GuiPanel ScrollablePanel;
        public GuiElement DeepestElement;
        public GuiPopupMenu PopupMenu;
    }

    public struct MouseEvent {
        public Point Position;
        public MouseEventType Type;
        public int Button;

        public const int LeftButton = 0;
        public const int MiddleButton = 1;
        public const int RightButton = 2;
        
        public const int MaxButtons = 3;
    }

    public enum MouseEventType {
        PRESSED, RELEASED
    }
}