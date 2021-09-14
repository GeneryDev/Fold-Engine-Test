using System;
using System.Collections.Generic;
using EntryProject.Editor.Gui.Hierarchy;
using EntryProject.Util;
using FoldEngine.Editor.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Gui {
    public abstract class GuiEnvironment : IDisposable {

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
        public ControlScheme ControlScheme = new ControlScheme("Gui");
        public List<GuiElement> DraggingElements = new List<GuiElement>();
        
        public abstract List<GuiPanel> VisiblePanels { get; }
        

        public GuiPopupMenu ContextMenu;
        
        public readonly ObjectPoolCollection<IGuiAction> ActionPool = new ObjectPoolCollection<IGuiAction>();
        
        // Renderer
        public IRenderingUnit Renderer { get; set; }
        public IRenderingLayer BaseLayer { get; set; }
        public IRenderingLayer OverlayLayer { get; set; }
        

        public GuiEnvironment(Scene scene) {
            Scene = scene;
            ContextMenu = new GuiPopupMenu(this);
            scene.Core.FoldGame.Window.TextInput += WindowOnTextInput;
            
            ControlScheme.AddDevice(Scene.Core.InputUnit.Devices.Keyboard);
            ControlScheme.AddDevice(Scene.Core.InputUnit.Devices.Mouse);
        }

        private void WindowOnTextInput(object sender, TextInputEventArgs e) {
            if(FocusOwner != null) {
                var evt = new KeyboardEvent() {
                    Type = KeyboardEventType.Typed,
                    Character = e.Character,
                    Key = e.Key,
                    Modifiers = KeyModifiersExt.GetKeyModifiers()
                };
                // if(evt.Character == '\n') Console.WriteLine("got a newline");
                if(evt.Character == '\r') evt.Character = '\n';
                FocusOwner.OnKeyTyped(ref evt);
            }
        }

        public virtual void Input(InputUnit inputUnit) {
            if(MouseLeft == ButtonAction.Default) {
                MouseLeft = new ButtonAction(inputUnit.Devices.Mouse.LeftButton);
                MouseMiddle = new ButtonAction(inputUnit.Devices.Mouse.MiddleButton);
                MouseRight = new ButtonAction(inputUnit.Devices.Mouse.RightButton);
            }

            MousePos = Mouse.GetState().Position;
            if(BaseLayer != null) MousePos = BaseLayer.WindowToLayer(MousePos.ToVector2()).ToPoint();
            
            HandleMouseEvents(MouseLeft, MouseEvent.LeftButton);
            HandleMouseEvents(MouseMiddle, MouseEvent.MiddleButton);
            HandleMouseEvents(MouseRight, MouseEvent.RightButton);

            FocusOwner?.OnInput(ControlScheme);
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

        public void DismissPopups() {
            if(ContextMenu.Showing) {
                ContextMenu.Dismiss();
            }
        }
        
        public virtual void Update() {
            
        }

        public virtual void Render(IRenderingUnit renderer, IRenderingLayer baseLayer, IRenderingLayer overlayLayer) {
            Renderer = renderer;
            BaseLayer = baseLayer;
            OverlayLayer = overlayLayer;

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

        public void Dispose() {
            Scene.Core.FoldGame.Window.TextInput -= WindowOnTextInput;
        }
    }

    public struct HoverTarget {
        public GuiPanel ScrollablePanel;
        public GuiElement Element;
        public GuiPopupMenu PopupMenu;
        public IHierarchy Hierarchy;
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
        public KeyModifiers Modifiers;

        public bool Consumed;
    }

    public enum KeyboardEventType {
        Pressed, Released, Typed
    }

    [Flags]
    public enum KeyModifiers {
        None = 0,
        Control = 1,
        Shift = 2,
        Alt = 4,
        Meta = 8
    }

    public static class KeyModifiersExt {
        public static bool Has(this KeyModifiers t, KeyModifiers mask) {
            return (t & mask) != 0;
        }

        public static KeyModifiers GetKeyModifiers() {
            KeyboardState state = Keyboard.GetState();
            return ((state[Keys.LeftControl] == KeyState.Down || state[Keys.RightControl] == KeyState.Down) ? KeyModifiers.Control : KeyModifiers.None)
                   | ((state[Keys.LeftShift] == KeyState.Down || state[Keys.RightShift] == KeyState.Down) ? KeyModifiers.Shift : KeyModifiers.None)
                   | ((state[Keys.LeftAlt] == KeyState.Down || state[Keys.RightAlt] == KeyState.Down) ? KeyModifiers.Alt : KeyModifiers.None)
                   | ((state[Keys.LeftWindows] == KeyState.Down || state[Keys.RightWindows] == KeyState.Down) ? KeyModifiers.Meta : KeyModifiers.None)
                ; 
        }
    }
}