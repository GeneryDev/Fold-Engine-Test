using System;
using FoldEngine.Components;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Components;

[Component("fold:control.popup")]
[ComponentInitializer(typeof(Popup), nameof(InitializeComponent))]
public struct Popup
{
    public PopupClickCondition DismissOnClick = PopupClickCondition.Outside;
    public bool ConsumeClickOnDismiss = false;

    public Popup()
    {
    }

    public static Popup InitializeComponent(Scene scene, long entityId)
    {
        return new Popup();
    }

    [Flags]
    public enum PopupClickCondition
    {
        Never = 0,
        
        Inside = 1,
        Outside = 2,
        
        Always = Inside | Outside
    }
}