using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Events;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keyboard = FoldEngine.Input.Keyboard;
using Mouse = FoldEngine.Input.Mouse;

namespace FoldEngine.Editor.ImmediateGui;

[GameSystem("fold:legacy.immediate_gui", ProcessingCycles.Input | ProcessingCycles.Render)]
public class ImmediateGuiSystem : GameSystem
{
    public readonly ControlScheme ControlScheme = new ControlScheme("Gui");

    private ComponentIterator<Viewport> _viewports;
    private MultiComponentIterator _immediateControls;
    
    public override void Initialize()
    {
        _viewports = CreateComponentIterator<Viewport>(IterationFlags.None);
        _immediateControls = CreateComponentIterator(typeof(Transform), typeof(Control), typeof(ImmediateGuiControl));
        SetupControlScheme();
    }
    
    private void SetupControlScheme()
    {
        Keyboard keyboard = Scene.Core.InputUnit.Devices.Keyboard;
        Mouse mouse = Scene.Core.InputUnit.Devices.Mouse;
        
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

    public override void OnInput()
    {
        _immediateControls.Reset();
        while (_immediateControls.Next())
        {
            ref var ic = ref _immediateControls.Get<ImmediateGuiControl>();
            ic.Environment?.Input(Scene.Core.InputUnit);
        }
    }

    public override void OnRender(IRenderingUnit renderer)
    {
        IRenderingLayer layer = null;

        _viewports.Reset();
        while (_viewports.Next())
        {
            ref var viewport = ref _viewports.GetComponent();
            layer = viewport.GetLayer(renderer);
            break;
        }

        if (layer == null) return;
        
        _immediateControls.Reset();
        while (_immediateControls.Next())
        {
            ref var transform = ref _immediateControls.Get<Transform>();
            ref var control = ref _immediateControls.Get<Control>();
            ref var ic = ref _immediateControls.Get<ImmediateGuiControl>();
            
            RenderImmediateControl(renderer, layer, ref transform, ref control, ref ic);
        }
    }

    private void RenderImmediateControl(IRenderingUnit renderer,
        IRenderingLayer layer,
        ref Transform transform,
        ref Control control,
        ref ImmediateGuiControl ic)
    {
        var view = ic.View;
        if (view == null) return;
        
        var environment = ic.Environment ??= new EditorEnvironment(Scene);
        var containerPanel = ic.ContainerPanel ??= new GuiPanel(environment);

        environment.PrepareRender(renderer, layer, null);

        var controlBounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());
        var innerBounds = controlBounds.Grow(view.UseMargin ? -8 : 0);
        
        containerPanel.Reset();
        containerPanel.ResetLayoutPosition();
        containerPanel.Bounds = controlBounds;
        environment.ContentPanel = containerPanel;
        
        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = view.BackgroundColor ?? new Color(37, 37, 38, 255),
            DestinationRectangle = controlBounds
        });
        
        view.Scene = Scene;
        view.EnsurePanelExists(environment);
        
        containerPanel.Element(view.ContentPanel);
        
        view.ContentPanel.Bounds = innerBounds;
        view.ContentPanel.Reset();
        view.Render(renderer);
        
        containerPanel.Render(renderer, layer);
    }

    public override void SubscribeToEvents()
    {
        Subscribe((ref MouseButtonEvent evt) =>
        {
            if (!Scene.Components.HasComponent<ImmediateGuiControl>(evt.EntityId)) return;

            var entity = new Entity(Scene, evt.EntityId);
            var ic = entity.GetComponent<ImmediateGuiControl>();
            
            ic.Environment.MousePos = evt.Position;
        });
        Subscribe((ref MouseMovedEvent evt) =>
        {
            if (!Scene.Components.HasComponent<ImmediateGuiControl>(evt.EntityId)) return;

            var entity = new Entity(Scene, evt.EntityId);
            var ic = entity.GetComponent<ImmediateGuiControl>();
            ic.Environment.MousePos = evt.Position;
        });
    }
}