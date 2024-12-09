using System;
using System.Collections.Generic;
using FoldEngine.Editor.Tools;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keyboard = FoldEngine.Input.Keyboard;
using Mouse = FoldEngine.Input.Mouse;

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

        if (HoverTarget.ScrollablePanel != null)
            if (HoverTarget.ScrollablePanel.IsAncestorOf(HoverTarget.Element))
            {
                if (ControlScheme?.Get<ChangeAction>("editor.zoom.in") ?? false)
                    HoverTarget.ScrollablePanel.Scroll(1);
                else if (ControlScheme?.Get<ChangeAction>("editor.zoom.out") ?? false) HoverTarget.ScrollablePanel.Scroll(-1);
            }
    }

    public override void PrepareRender(IRenderingUnit renderer, IRenderingLayer baseLayer, IRenderingLayer overlayLayer)
    {
        base.PrepareRender(renderer, baseLayer, overlayLayer);
        
        // Make the game view size zero. Supposed to be re-set to the correct size when the EditorSceneView is rendered
        renderer.Groups["editor"].Dependencies[0].Group.Size = default;
    }
}
