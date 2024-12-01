using System;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Gui.Components;

[Component("fold:control.popup")]
[ComponentInitializer(typeof(Popup), nameof(InitializeComponent))]
public struct Popup
{
    [EntityId] public long SourceEntityId = -1;
    public PopupClickCondition DismissOnClick = PopupClickCondition.Outside;
    public bool ConsumeClickOnDismiss = false;

    [HideInInspector] [DoNotSerialize] public bool SuppressDismissUntilNextRelease = false;

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