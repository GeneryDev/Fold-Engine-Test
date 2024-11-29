using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using FoldEngine.Editor.ImmediateGui.Hierarchy;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.ImmediateGui;

public abstract class GuiEnvironment : IDisposable
{
    public readonly ObjectPoolCollection<IGuiAction> ActionPool = new ObjectPoolCollection<IGuiAction>();

    private long _dismissPopupsWhen;
    private readonly GuiPanel[] _pressedPanels = new GuiPanel[MouseEvent.MaxButtons];


    public GuiPopupMenu ContextMenu;
    public ControlScheme ControlScheme = new ControlScheme("Gui");
    public HoverTarget HoverTarget;
    public HoverTarget HoverTargetPrevious;
    public ButtonAction MouseLeft = ButtonAction.Default;
    public ButtonAction MouseMiddle = ButtonAction.Default;
    public ButtonAction MouseRight = ButtonAction.Default;

    // Input
    public Point MousePos;

    public Scene Scene;
    public IGameCore Core => Scene.Core;
    public ResourceCollections EditorResources => Scene.Resources;


    public GuiEnvironment(Scene scene)
    {
        Scene = scene;
        ContextMenu = new GuiPopupMenu(this);
        scene.Core.FoldGame.Window.TextInput += WindowOnTextInput;

        ControlScheme.AddDevice(Core.InputUnit.Devices.Keyboard);
        ControlScheme.AddDevice(Core.InputUnit.Devices.Mouse);
    }

    public GuiElement FocusOwner { get; private set; }

    public abstract List<GuiPanel> VisiblePanels { get; }

    // Renderer
    public IRenderingUnit Renderer { get; set; }
    public IRenderingLayer BaseLayer { get; set; }
    public IRenderingLayer OverlayLayer { get; set; }

    public void Dispose()
    {
        Scene.Core.FoldGame.Window.TextInput -= WindowOnTextInput;
    }

    private void WindowOnTextInput(object sender, TextInputEventArgs e)
    {
        if (FocusOwner != null)
        {
            var evt = new KeyboardEvent
            {
                Type = KeyboardEventType.Typed,
                Character = e.Character,
                Key = e.Key,
                Modifiers = KeyModifiersExt.GetKeyModifiers()
            };
            // if(evt.Character == '\n') Console.WriteLine("got a newline");
            if (evt.Character == '\r') evt.Character = '\n';
            FocusOwner.OnKeyTyped(ref evt);
        }
    }

    public virtual void Input(InputUnit inputUnit)
    {
        if (MouseLeft == ButtonAction.Default)
        {
            MouseLeft = new ButtonAction(inputUnit.Devices.Mouse.LeftButton);
            MouseMiddle = new ButtonAction(inputUnit.Devices.Mouse.MiddleButton);
            MouseRight = new ButtonAction(inputUnit.Devices.Mouse.RightButton);
        }

        MousePos = Mouse.GetState().Position;
        if (BaseLayer != null)
            try
            {
                MousePos = BaseLayer.WindowToLayer(MousePos.ToVector2()).ToPoint();
            }
            catch (Exception ignore)
            {
                Console.WriteLine(ignore.Message);
            }

        HandleMouseEvents(MouseLeft, MouseEvent.LeftButton);
        HandleMouseEvents(MouseMiddle, MouseEvent.MiddleButton);
        HandleMouseEvents(MouseRight, MouseEvent.RightButton);
        
        if (_dismissPopupsWhen == Time.TotalFixedTicks) DismissPopups();

        FocusOwner?.OnInput(ControlScheme);
    }

    private void HandleMouseEvents(ButtonAction mouseButton, int buttonIndex)
    {
        if (mouseButton.Pressed)
        {
            if (HoverTarget.PopupMenu != ContextMenu) DismissPopups();

            for (int i = VisiblePanels.Count - 1; i >= 0; i--)
            {
                GuiPanel panel = VisiblePanels[i];
                if (panel.Visible && panel.Bounds.Contains(MousePos))
                {
                    _pressedPanels[buttonIndex] = panel;

                    var evt = new MouseEvent
                    {
                        Type = MouseEventType.Pressed,
                        Position = MousePos,
                        Button = buttonIndex,
                        When = Time.Now
                    };

                    panel.OnMousePressed(ref evt);
                    break;
                }
            }
        }
        else if (mouseButton.Released)
        {
            if (ContextMenu.Showing)
            {
                _dismissPopupsWhen = Time.TotalFixedTicks + 2;
            }

            var evt = new MouseEvent
            {
                Type = MouseEventType.Released,
                Position = MousePos,
                Button = buttonIndex,
                When = Time.Now
            };

            _pressedPanels[buttonIndex]?.OnMouseReleased(ref evt);
            _pressedPanels[buttonIndex] = null;
        }
    }

    public void DismissPopups()
    {
        if (ContextMenu.Showing) ContextMenu.Dismiss();
    }

    public virtual void Update()
    {
    }

    public virtual void Render(IRenderingUnit renderer, IRenderingLayer baseLayer, IRenderingLayer overlayLayer)
    {
        Renderer = renderer;
        BaseLayer = baseLayer;
        OverlayLayer = overlayLayer;

        HoverTargetPrevious = HoverTarget;
        HoverTarget = default;
    }

    public void SetFocusedElement(GuiElement element)
    {
        if (FocusOwner != element)
        {
            FocusOwner?.OnFocusLost();

            FocusOwner = element;

            FocusOwner?.OnFocusGained();
        }
    }
}

public struct HoverTarget
{
    public GuiPanel ScrollablePanel;
    public GuiElement Element;
    public GuiPopupMenu PopupMenu;
    public IHierarchy Hierarchy;
}

public struct MouseEvent
{
    public Point Position;
    public MouseEventType Type;
    public int Button;
    public long When;

    public bool Consumed;

    public const int LeftButton = 0;
    public const int MiddleButton = 1;
    public const int RightButton = 2;

    public const int MaxButtons = 3;
}

public enum MouseEventType
{
    Pressed,
    Released
}

public struct KeyboardEvent
{
    public KeyboardEventType Type;
    public char Character;
    public Keys Key;
    public KeyModifiers Modifiers;

    public bool Consumed;
}

public enum KeyboardEventType
{
    Pressed,
    Released,
    Typed
}

[Flags]
public enum KeyModifiers
{
    None = 0,
    Control = 1,
    Shift = 2,
    Alt = 4,
    Meta = 8
}

public static class KeyModifiersExt
{
    [Pure]
    public static bool Has(this KeyModifiers a, KeyModifiers flag)
    {
        return ((uint)a & (uint)flag) == (uint)flag;
    }
    
    public static KeyModifiers GetKeyModifiers()
    {
        KeyboardState state = Keyboard.GetState();
        return (state[Keys.LeftControl] == KeyState.Down || state[Keys.RightControl] == KeyState.Down
                   ? KeyModifiers.Control
                   : KeyModifiers.None)
               | (state[Keys.LeftShift] == KeyState.Down || state[Keys.RightShift] == KeyState.Down
                   ? KeyModifiers.Shift
                   : KeyModifiers.None)
               | (state[Keys.LeftAlt] == KeyState.Down || state[Keys.RightAlt] == KeyState.Down
                   ? KeyModifiers.Alt
                   : KeyModifiers.None)
               | (state[Keys.LeftWindows] == KeyState.Down || state[Keys.RightWindows] == KeyState.Down
                   ? KeyModifiers.Meta
                   : KeyModifiers.None)
            ;
    }
}