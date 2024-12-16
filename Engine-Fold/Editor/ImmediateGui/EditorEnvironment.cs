using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.ImmediateGui;

public class EditorEnvironment : GuiEnvironment
{
    public override GuiPanel ContentPanel { get; set; }

    public EditorEnvironment(Scene scene) : base(scene)
    {
    }

    public override void Input(InputUnit inputUnit)
    {
        base.Input(inputUnit);
        var editorBase = Scene.Systems.Get<EditorBase>();

        if (ControlScheme?.Get<ButtonAction>("editor.undo").Consume() ?? false) editorBase.Undo();
        if (ControlScheme?.Get<ButtonAction>("editor.redo").Consume() ?? false) editorBase.Redo();
    }

    public void HandleScroll(int dir)
    {
        if (HoverTarget.ScrollablePanel != null)
            if (HoverTarget.ScrollablePanel.IsAncestorOf(HoverTarget.Element))
            {
                HoverTarget.ScrollablePanel.Scroll(dir);
            }
    }
}
