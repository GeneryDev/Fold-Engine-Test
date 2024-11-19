using System;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components;

[Component("fold:control.button", traits: [typeof(Control), typeof(MousePickable)])]
[ComponentInitializer(typeof(ButtonControl), nameof(InitializeComponent))]
public struct ButtonControl
{
    public string Text;
    public int FontSize;
    public Color Color;
    public Alignment Alignment;
    public bool KeepPressedOutside;
    public MouseActionMode ActionMode;
    public MouseButtonMask ButtonMask;

    [DoNotSerialize] [HideInInspector] public RenderedText RenderedText;
    
    public Color NormalColor => new Color(37, 37, 38);
    public Color RolloverColor => Color.CornflowerBlue;
    public Color PressedColor => new Color(63, 63, 70);
    [DoNotSerialize] public bool Rollover;
    [DoNotSerialize] public bool Pressed;
    
    public ButtonControl()
    {
        Text = "";
        FontSize = 14;
        Color = Color.White;
        Alignment = Alignment.Center;
        ActionMode = MouseActionMode.Release;
        ButtonMask = MouseButtonMask.LeftButton;
    }
    
    /// <summary>
    ///     Returns an initialized button component with all its correct default values.
    /// </summary>
    public static ButtonControl InitializeComponent(Scene scene, long entityId)
    {
        return new ButtonControl();
    }

    public bool UpdateRenderedText(IRenderingUnit renderer)
    {
        if (RenderedText.HasValue && RenderedText.Text == Text && RenderedText.Size == FontSize)
        {
            // already up to date
            return false;
        }
        renderer.Fonts["default"].RenderString(Text, out RenderedText, FontSize);
        return true;
    }
}

[Flags]
public enum MouseButtonMask
{
    LeftButton = 1 << MouseButtonEvent.LeftButton,
    MiddleButton = 1 << MouseButtonEvent.MiddleButton,
    RightButton = 1 << MouseButtonEvent.RightButton
}
public enum MouseActionMode
{
    Press,
    Release
}