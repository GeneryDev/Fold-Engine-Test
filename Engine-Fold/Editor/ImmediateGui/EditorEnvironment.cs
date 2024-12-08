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
        SetupControlScheme();
    }

    private void SetupControlScheme()
    {
        Keyboard keyboard = Core.InputUnit.Devices.Keyboard;
        Mouse mouse = Core.InputUnit.Devices.Mouse;
        
        ControlScheme.AddDevice(keyboard);
        ControlScheme.AddDevice(mouse);

        ControlScheme.PutAction("editor.undo",
            new ButtonAction(keyboard[Keys.Z]) { Repeat = true }.Modifiers(keyboard[Keys.LeftControl]));
        ControlScheme.PutAction("editor.redo",
            new ButtonAction(keyboard[Keys.Y]) { Repeat = true }.Modifiers(keyboard[Keys.LeftControl]));

        ControlScheme.PutAction("editor.field.select_all",
            new ButtonAction(keyboard[Keys.A]) { Repeat = true }.Modifiers(keyboard[Keys.LeftControl]));
        ControlScheme.PutAction("editor.field.caret.left", new ButtonAction(keyboard[Keys.Left]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.right", new ButtonAction(keyboard[Keys.Right]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.up", new ButtonAction(keyboard[Keys.Up]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.down", new ButtonAction(keyboard[Keys.Down]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.home", new ButtonAction(keyboard[Keys.Home]) { Repeat = true });
        ControlScheme.PutAction("editor.field.caret.end", new ButtonAction(keyboard[Keys.End]) { Repeat = true });

        ControlScheme.PutAction("editor.field.caret.debug", new ButtonAction(keyboard[Keys.F1]) { Repeat = true });

        ControlScheme.PutAction("editor.zoom.in", new ChangeAction(mouse.ScrollWheel, 0.5f, null));
        ControlScheme.PutAction("editor.zoom.out", new ChangeAction(mouse.ScrollWheel, null, -1.5f));

        ControlScheme.PutAction("editor.movement.axis.x",
            new AnalogAction(() => (keyboard[Keys.Left].Down ? -1 : 0) + (keyboard[Keys.Right].Down ? 1 : 0)));
        ControlScheme.PutAction("editor.movement.axis.y",
            new AnalogAction(() => (keyboard[Keys.Down].Down ? -1 : 0) + (keyboard[Keys.Up].Down ? 1 : 0)));

        ControlScheme.PutAction("editor.movement.faster", new ButtonAction(keyboard[Keys.LeftShift]));
    }

    public override void Input(InputUnit inputUnit)
    {
        base.Input(inputUnit);
        var editorBase = Scene.Systems.Get<EditorBase>();

        if (ControlScheme.Get<ButtonAction>("editor.undo").Consume()) editorBase.Undo();
        if (ControlScheme.Get<ButtonAction>("editor.redo").Consume()) editorBase.Redo();

        if (HoverTarget.ScrollablePanel != null)
            if (HoverTarget.ScrollablePanel.IsAncestorOf(HoverTarget.Element))
            {
                if (ControlScheme.Get<ChangeAction>("editor.zoom.in"))
                    HoverTarget.ScrollablePanel.Scroll(1);
                else if (ControlScheme.Get<ChangeAction>("editor.zoom.out")) HoverTarget.ScrollablePanel.Scroll(-1);
            }
    }

    public override void PrepareRender(IRenderingUnit renderer, IRenderingLayer baseLayer, IRenderingLayer overlayLayer)
    {
        base.PrepareRender(renderer, baseLayer, overlayLayer);
        
        // Make the game view size zero. Supposed to be re-set to the correct size when the EditorSceneView is rendered
        renderer.Groups["editor"].Dependencies[0].Group.Size = default;
    }
}
