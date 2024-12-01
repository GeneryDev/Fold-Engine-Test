using System;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace FoldEngine.Gui.Systems;

[GameSystem("fold:control.interface", ProcessingCycles.Input, true)]
public class ControlInterfaceSystem : GameSystem
{
    private static readonly Point NullMousePos = new Point(-1, -1);

    public float DragStartDistance = 10;
    
    [DoNotSerialize] [HideInInspector] public ButtonAction MouseLeft = ButtonAction.Default;
    [DoNotSerialize] [HideInInspector] public ButtonAction MouseMiddle = ButtonAction.Default;
    [DoNotSerialize] [HideInInspector] public ButtonAction MouseRight = ButtonAction.Default;
    [DoNotSerialize] [HideInInspector] public ButtonAction Escape = ButtonAction.Default;

    [DoNotSerialize] private Point _prevMousePos = NullMousePos;
    [DoNotSerialize] private Point _mousePos = NullMousePos;
    [DoNotSerialize] private readonly MouseButtonPressMemory[] _buttonPressMemory = new MouseButtonPressMemory[MouseButtonEvent.MaxButtons];
    
    private ComponentIterator<Viewport> _viewports;
    private ComponentIterator<Control> _controls;

    
    public override void Initialize()
    {
        _viewports = CreateComponentIterator<Viewport>(IterationFlags.None);
        _controls = CreateComponentIterator<Control>(IterationFlags.None);
        
        Array.Fill(_buttonPressMemory, MouseButtonPressMemory.Empty);
    }

    public override void OnInput()
    {
        var inputUnit = Scene.Core.InputUnit;
        
        if (MouseLeft == ButtonAction.Default)
        {
            MouseLeft = new ButtonAction(inputUnit.Devices.Mouse.LeftButton);
            MouseMiddle = new ButtonAction(inputUnit.Devices.Mouse.MiddleButton);
            MouseRight = new ButtonAction(inputUnit.Devices.Mouse.RightButton);
            Escape = new ButtonAction(inputUnit.Devices.Keyboard[Keys.Escape]);
        }

        _viewports.Reset();
        while (_viewports.Next())
        {
            ref var viewport = ref _viewports.GetComponent();
            HandleViewport(ref viewport);
            break;
        }
    }

    private void HandleViewport(ref Viewport viewport)
    {
        var layer = viewport.GetLayer(Scene.Core.RenderingUnit);
        if (layer == null) return;

        _prevMousePos = _mousePos;
        _mousePos = Mouse.GetState().Position;
        try
        {
            _mousePos = layer.WindowToLayer(_mousePos.ToVector2()).ToPoint();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        if (_prevMousePos == NullMousePos)
        {
            _prevMousePos = _mousePos;
        }

        viewport.PrevHoverTargetId = viewport.HoverTargetId;
        viewport.HoverTargetId = GetControlAtPoint(_mousePos);
        
        HandleMouseMotionEvents(ref viewport);

        if (viewport.PrevHoverTargetId != viewport.HoverTargetId)
        {
            if (viewport.PrevHoverTargetId != -1)
            {
                Scene.Events.Invoke(new MouseExitedEvent(viewport.PrevHoverTargetId, _mousePos));
            }
            if (viewport.HoverTargetId != -1)
            {
                Scene.Events.Invoke(new MouseEnteredEvent(viewport.HoverTargetId, _mousePos));
            }
        }
        
        HandleMouseEvents(MouseLeft, MouseButtonEvent.LeftButton);
        HandleMouseEvents(MouseMiddle, MouseButtonEvent.MiddleButton);
        HandleMouseEvents(MouseRight, MouseButtonEvent.RightButton);
        if (_buttonPressMemory[MouseButtonEvent.LeftButton].DragDataId != -1 && Escape.Consume())
        {
            CancelDragOperation(ref _buttonPressMemory[MouseButtonEvent.LeftButton]);
        }
        
        // Console.WriteLine($"Hover target: {viewport.HoverTargetId}");

        //
        // FocusOwner?.OnInput(ControlScheme);
    }

    private void HandleMouseMotionEvents(ref Viewport viewport)
    {
        var mouseDelta = _mousePos - _prevMousePos;

        if (mouseDelta == Point.Zero) return;
        Scene.Events.Invoke(new MouseMovedEvent
        {
            EntityId = viewport.HoverTargetId,
            Position = _mousePos,
            Delta = mouseDelta
        });
        for (var btnIndex = 0; btnIndex < MouseButtonEvent.MaxButtons; btnIndex++)
        {
            var buttonMemory = _buttonPressMemory[btnIndex];
            if (buttonMemory.ControlId == -1) continue;

            var diffFromPressPoint = _mousePos - buttonMemory.MousePos;

            var startDrag = false;
            if (btnIndex == MouseButtonEvent.LeftButton && !buttonMemory.DragStarted && diffFromPressPoint.ToVector2().LengthSquared() >= DragStartDistance*DragStartDistance)
            {
                // start drag
                startDrag = true;
                buttonMemory.DragStarted = true;
                _buttonPressMemory[btnIndex] = buttonMemory;
            }

            if (buttonMemory.DragDataId != -1)
            {
                Scene.Components.GetComponent<Transform>(buttonMemory.DragDataId).Position = _mousePos.ToVector2();
                buttonMemory.DropTargetId = FindDropTarget(buttonMemory.DragDataId, GetControlAtPoint(_mousePos));
                Mouse.SetCursor(buttonMemory.DropTargetId == -1 ? MouseCursor.No : MouseCursor.Arrow);
                _buttonPressMemory[btnIndex] = buttonMemory;
            }
            
            Scene.Events.Invoke(new MouseDraggedEvent()
            {
                EntityId = buttonMemory.ControlId,
                Position = _mousePos,
                Delta = mouseDelta,
                Button = btnIndex
            });

            if (startDrag && Scene.Components.HasTrait<DragOperationStarter>(buttonMemory.ControlId))
            {
                long dragDataId = AttemptStartDragOperation(buttonMemory.ControlId);

                if(dragDataId != -1) {
                    buttonMemory.DragDataId = dragDataId;
                    _buttonPressMemory[btnIndex] = buttonMemory;
                }
            }
        }
    }

    private long AttemptStartDragOperation(long sourceId)
    {
        var operationEntity = Scene.CreateEntity("Drag Operation");
        operationEntity.AddComponent<Control>() = new Control
        {
            RequestLayout = true,
            MouseFilter = Control.MouseFilterMode.Ignore
        };
        
        var dragRequestEvt = Scene.Events.Invoke(new DragDataRequestedEvent()
        {
            SourceEntityId = sourceId,
            DragOperationEntityId = operationEntity.EntityId
        });

        if (!dragRequestEvt.HasData)
        {
            Scene.DeleteEntity(operationEntity.EntityId, recursively: true);
            return -1;
        }
        Console.WriteLine("START DRAG WITH DATA");

        return operationEntity.EntityId;
    }

    private void CancelDragOperation(ref MouseButtonPressMemory buttonMemory)
    {
        if (buttonMemory.DropTargetId != -1)
        {
            Scene.Events.Invoke(new DragOperationCanceledEvent()
            {
                DragOperationEntityId = buttonMemory.DragDataId,
                TargetEntityId = buttonMemory.DropTargetId
            });
        }
        if(buttonMemory.DragDataId != -1)
            Scene.DeleteEntity(buttonMemory.DragDataId, recursively: true);

        buttonMemory.DropTargetId = -1;
        buttonMemory.DragDataId = -1;
    }

    private long FindDropTarget(long dragDataId, long targetId)
    {
        while (targetId != -1)
        {
            Console.WriteLine($"Validating drop on {targetId}");
            var validationRequestEvt = Scene.Events.Invoke(new DropValidationRequestedEvent() { TargetEntityId = targetId, DragOperationEntityId = dragDataId });

            if (validationRequestEvt.CanDrop)
            {
                return targetId;
            }

            if (Scene.Components.HasComponent<Hierarchical>(targetId))
            {
                var hierarchical = Scene.Components.GetComponent<Hierarchical>(targetId);
                targetId = hierarchical.ParentId;
            }
            else
            {
                targetId = -1;
            }
        }

        return -1;
    }

    private void SubmitDragOperation(long dragDataId, long target)
    {
        Console.WriteLine($"DROP: {dragDataId}");
        Scene.Events.Invoke(new DroppedDataEvent()
        {
            TargetEntityId = target,
            DragOperationEntityId = dragDataId,
            Consumed = false
        });
        Scene.DeleteEntity(dragDataId, recursively: true);
    }

    private void HandleMouseEvents(ButtonAction mouseButton, int buttonIndex)
    {
        if (mouseButton.Pressed)
        {
            long onEntityId = GetControlAtPoint(_mousePos);
            _buttonPressMemory[buttonIndex] = new MouseButtonPressMemory()
            {
                ControlId = onEntityId,
                MousePos = _mousePos,
                DragStarted = false
            };
            Scene.Events.Invoke(new MouseButtonEvent
            {
                Type = MouseButtonEventType.Pressed,
                EntityId = onEntityId,
                Position = _mousePos,
                Button = buttonIndex
            });
        }
        else if (mouseButton.Released)
        {
            var buttonMemory = _buttonPressMemory[buttonIndex];
            if (buttonMemory.DragStarted && buttonMemory.DragDataId != -1)
            {
                // DROP
                long dropTarget = FindDropTarget(buttonMemory.DragDataId, GetControlAtPoint(_mousePos));
                if (dropTarget != -1)
                {
                    SubmitDragOperation(buttonMemory.DragDataId, dropTarget);
                }
                else
                {
                    CancelDragOperation(ref buttonMemory);
                }
            }
            // TODO drag drop
            Scene.Events.Invoke(new MouseButtonEvent
            {
                Type = MouseButtonEventType.Released,
                EntityId = buttonMemory.ControlId,
                Position = _mousePos,
                Button = buttonIndex
            });
            _buttonPressMemory[buttonIndex] = MouseButtonPressMemory.Empty;
        }
    }

    private Control.MouseFilterMode GetMouseFilterMode(long entityId, ref Control control)
    {
        if (control.MouseFilter != Control.MouseFilterMode.Auto)
        {
            return control.MouseFilter;
        }

        if (Scene.Components.HasTrait<MouseFilterDefaults>(entityId)) return Control.MouseFilterMode.Stop;
        if (Scene.Components.HasTrait<MouseFilterDefaultIgnore>(entityId)) return Control.MouseFilterMode.Ignore;
        return Control.MouseFilterMode.Ignore;
    }

    private long GetControlAtPoint(Point point)
    {
        _controls.Reset();

        float? topZ = 0;
        long topEntity = -1;
        
        while (_controls.Next())
        {
            ref var transform = ref _controls.GetCoComponent<Transform>();
            ref var control = ref _controls.GetComponent();

            if (topEntity == -1 || control.ZOrder >= topZ)
            {
                var mode = GetMouseFilterMode(_controls.GetEntityId(), ref control);
                if (mode == Control.MouseFilterMode.Ignore) continue;
                // TODO pass vs stop
                
                var position = transform.Position;
                var containsPoint = (position.X <= point.X && point.X <= position.X + control.Size.X) &&
                                    (position.Y <= point.Y && point.Y <= position.Y + control.Size.Y);
                
                if (containsPoint)
                {
                    topZ = control.ZOrder;
                    topEntity = _controls.GetEntityId();
                }
            }
        }

        return topEntity;
    }

    private struct MouseButtonPressMemory
    {
        public static readonly MouseButtonPressMemory Empty = new()
        {
            ControlId = -1
        };
        
        public long ControlId;
        public Point MousePos;
        public bool DragStarted;
        public long DragDataId = -1;
        public long DropTargetId = -1;

        public MouseButtonPressMemory()
        {
        }
    }
}