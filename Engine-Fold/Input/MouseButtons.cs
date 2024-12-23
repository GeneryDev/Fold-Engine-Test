using System;

namespace FoldEngine.Input;

public enum MouseButtons
{
    LeftButton,
    MiddleButton,
    RightButton,
    XButton1,
    XButton2,
}

[Flags]
public enum MouseButtonMask
{
    LeftButton = 1 << MouseButtons.LeftButton,
    MiddleButton = 1 << MouseButtons.MiddleButton,
    RightButton = 1 << MouseButtons.RightButton,
    XButton1 = 1 << MouseButtons.XButton1,
    XButton2 = 1 << MouseButtons.XButton2,
}

public static class MouseButtonExt
{
    public static MouseButtonMask ToMask(this MouseButtons button)
    {
        return (MouseButtonMask)(1 << ((int)button));
    }
    
    public static bool Has(this MouseButtonMask field, MouseButtons button)
    {
        return field.Has(button.ToMask());
    }
    
    public static bool Has(this MouseButtonMask field, MouseButtonMask mask)
    {
        return (field & mask) == mask;
    }
}