using System;
using System.Collections.Generic;
using EntryProject.Util;
using FoldEngine.Editor.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Gui {
    public abstract class GuiEnvironment {

        public Scene Scene;
        
        // Input
        public Point MousePos;
        public ButtonAction MouseLeft = ButtonAction.Default;
        public ButtonAction MouseMiddle = ButtonAction.Default;
        public ButtonAction MouseRight = ButtonAction.Default;
        private GuiPanel[] _pressedPanels = new GuiPanel[MouseEvent.MaxButtons];
        public HoverTarget HoverTargetPrevious;
        public HoverTarget HoverTarget;
        public GuiElement FocusOwner { get; private set; }
            
        public abstract List<GuiPanel> VisiblePanels { get; }
        

        public GuiPopupMenu ContextMenu;
        
        public readonly ObjectPoolCollection<IGuiAction> ActionPool = new ObjectPoolCollection<IGuiAction>();
        
        // Renderer
        public IRenderingUnit Renderer { get; set; }
        public IRenderingLayer Layer { get; set; }
        

        public GuiEnvironment(Scene scene) {
            Scene = scene;
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

            HandleKeyboardEvents();
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

                        var evt = new MouseEvent() {
                            Type = MouseEventType.Pressed,
                            Position = MousePos,
                            Button = buttonIndex
                        };
                        
                        panel.OnMousePressed(ref evt);
                        break;
                    }
                }
            } else if(mouseButton.Released) {
                DismissPopups();

                var evt = new MouseEvent() {
                    Type = MouseEventType.Released,
                    Position = MousePos,
                    Button = buttonIndex
                };
                
                _pressedPanels[buttonIndex]?.OnMouseReleased(ref evt);
                _pressedPanels[buttonIndex] = null;
            }
        }

        private void HandleKeyboardEvents() {
            
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

        public void SetFocusedElement(GuiElement element) {
            if(FocusOwner != element) {
                FocusOwner?.OnFocusLost();

                FocusOwner = element;
                
                FocusOwner?.OnFocusGained();
            }
        }
    }

    public struct HoverTarget {
        public GuiPanel ScrollablePanel;
        public GuiElement Element;
        public GuiPopupMenu PopupMenu;
    }

    public struct MouseEvent {
        public Point Position;
        public MouseEventType Type;
        public int Button;

        public bool Consumed;

        public const int LeftButton = 0;
        public const int MiddleButton = 1;
        public const int RightButton = 2;
        
        public const int MaxButtons = 3;
    }

    public enum MouseEventType {
        Pressed, Released
    }

    public struct KeyboardEvent {
        public KeyboardEventType Type;
        public char Character;
        public Keys Key;

        public bool Consumed;
    }

    public enum KeyboardEventType {
        Pressed, Released, Typed
    }
}