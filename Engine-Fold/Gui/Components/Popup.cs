using System;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Gui.Components;

[Component("fold:control.popup")]
[ComponentInitializer(typeof(Popup))]
public struct Popup
{
    [EntityId] public long SourceEntityId = -1;
    public PopupClickCondition DismissOnClick = PopupClickCondition.Outside;
    public bool ConsumeClickOnDismiss = false;

    public Popup()
    {
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