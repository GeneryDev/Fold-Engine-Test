using System;
using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Input;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keyboard = FoldEngine.Input.Keyboard;
using Mouse = FoldEngine.Input.Mouse;

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

    [DoNotSerialize] public ControlScheme InterfaceControlScheme = new ControlScheme("ui");

    
    public override void Initialize()
    {
        _viewports = CreateComponentIterator<Viewport>(IterationFlags.None);
        _controls = CreateComponentIterator<Control>(IterationFlags.None);
        
        Array.Fill(_buttonPressMemory, MouseButtonPressMemory.Empty);

        SetupControlScheme();
    }

    private void SetupControlScheme()
    {
        Keyboard keyboard = Scene.Core.InputUnit.Devices.Keyboard;
        Mouse mouse = Scene.Core.InputUnit.Devices.Mouse;
        
        InterfaceControlScheme.AddDevice(keyboard);
        InterfaceControlScheme.AddDevice(mouse);

        InterfaceControlScheme.PutAction("ui.scroll.up", new ChangeAction(mouse.ScrollWheel, 0.5f, null));
        InterfaceControlScheme.PutAction("ui.scroll.down", new ChangeAction(mouse.ScrollWheel, null, -1.5f));
    }

    public override void SubscribeToEvents()
    {
        Subscribe((ref FocusRequestedEvent evt) =>
        {
            TryGrabFocus(evt.EntityId);
        });
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
            HandleViewportInput(ref viewport);
            break;
        }
    }

    private long GetViewportIdForControl(long entityId)
    {
        _viewports.Reset();
        while (_viewports.Next())
        {
            return _viewports.GetEntityId();
        }

        return -1;
    }

    private void HandleViewportInput(ref Viewport viewport)
    {
        var layer = viewport.GetLayer(Scene.Core.RenderingUnit);
        if (layer == null) return;

        _prevMousePos = _mousePos;
        _mousePos = Microsoft.Xna.Framework.Input.Mouse.GetState().Position;
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

        long prevHoverTargetId = viewport.HoverTargetId;

        viewport.MousePos = _mousePos;
        viewport.HoverTargetId = GetControlAtPoint(_mousePos);
        
        HandleMouseMotionEvents(ref viewport);

        if (prevHoverTargetId != viewport.HoverTargetId)
        {
            if (prevHoverTargetId != -1)
            {
                Scene.Events.Invoke(new MouseExitedEvent(prevHoverTargetId, _mousePos));
            }
            if (viewport.HoverTargetId != -1)
            {
                Scene.Events.Invoke(new MouseEnteredEvent(viewport.HoverTargetId, _mousePos));
            }
        }
        
        HandleMouseEvents(MouseLeft, MouseButtonEvent.LeftButton, handleFocus: true);
        HandleMouseEvents(MouseMiddle, MouseButtonEvent.MiddleButton);
        HandleMouseEvents(MouseRight, MouseButtonEvent.RightButton);
        if (_buttonPressMemory[MouseButtonEvent.LeftButton].DragDataId != -1 && Escape.Consume())
        {
            CancelDragOperation(ref _buttonPressMemory[MouseButtonEvent.LeftButton]);
        }

        if (InterfaceControlScheme.Get<ChangeAction>("ui.scroll.up"))
        {
            // scroll up
            OnScroll(ref viewport, -1);
        }
        if (InterfaceControlScheme.Get<ChangeAction>("ui.scroll.down"))
        {
            // scroll down
            OnScroll(ref viewport, 1);
        }
        HandleControlInput(viewport.FocusOwnerId);
        
        // Console.WriteLine($"Hover target: {viewport.HoverTargetId}");

        //
        // FocusOwner?.OnInput(ControlScheme);
    }

    private void OnScroll(ref Viewport viewport, int dir)
    {
        long hoverEntityId = viewport.HoverTargetId;

        while (hoverEntityId != -1)
        {
            if (!Scene.Components.HasComponent<Hierarchical>(hoverEntityId)) break;

            if (Scene.Components.HasTrait<Scrollable>(hoverEntityId))
            {
                var evt = new MouseScrolledEvent()
                {
                    EntityId = hoverEntityId,
                    Position = viewport.MousePos,
                    Amount = dir
                };

                evt = Scene.Events.Invoke(evt);
                if (evt.Consumed) break;
            }

            if (!Scene.Components.HasComponent<Hierarchical>(hoverEntityId)) break;
            ref var hierarchical = ref Scene.Components.GetComponent<Hierarchical>(hoverEntityId);
            hoverEntityId = hierarchical.ParentId;
        }
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
                Microsoft.Xna.Framework.Input.Mouse.SetCursor(buttonMemory.DropTargetId == -1 ? MouseCursor.No : MouseCursor.Arrow);
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

    private void HandleMouseEvents(ButtonAction mouseButton, int buttonIndex, bool handleFocus = false)
    {
        if (mouseButton.Pressed)
        {
            long onEntityId = GetControlAtPoint(_mousePos);
            _buttonPressMemory[buttonIndex] = new MouseButtonPressMemory()
            {
                Down = true,
                ControlId = onEntityId,
                MousePos = _mousePos,
                DragStarted = false
            };
            
            SendMousePressEvent(onEntityId, buttonIndex, handleFocus);
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
            SendMouseReleaseEvent(buttonMemory.ControlId, buttonIndex);
            _buttonPressMemory[buttonIndex] = MouseButtonPressMemory.Empty;
        }
    }

    private void TryGrabFocus(long entityId)
    {
        long viewportId = GetViewportIdForControl(entityId);
        if (viewportId == -1 || !Scene.Components.HasComponent<Viewport>(viewportId)) return;
        ref var viewport = ref Scene.Components.GetComponent<Viewport>(viewportId);
        
        long prevFocusOwnerId = viewport.FocusOwnerId;

        viewport.FocusOwnerId = entityId;
        
        if (prevFocusOwnerId != viewport.FocusOwnerId)
        {
            if (prevFocusOwnerId != -1)
            {
                Scene.Events.Invoke(new FocusLostEvent()
                {
                    EntityId = prevFocusOwnerId
                });
            }
            if (viewport.FocusOwnerId != -1)
            {
                Scene.Events.Invoke(new FocusGainedEvent()
                {
                    EntityId = viewport.FocusOwnerId
                });
            }
        }
    }

    private void HandleControlInput(long entityId)
    {
        while (entityId != -1)
        {
            if (!Scene.Components.HasComponent<Control>(entityId)) break;
            if (Scene.Components.HasTrait<InputCaptor>(entityId))
            {
                Scene.Events.Invoke(new HandleInputsEvent()
                {
                    EntityId = entityId,
                });
            }
                
            // pass to parent
            var hierarchical = Scene.Components.GetComponent<Hierarchical>(entityId);
            entityId = hierarchical.ParentId;
        }
    }

    private void SendMousePressEvent(long entityId, int buttonIndex, bool focus)
    {
        while (entityId != -1)
        {
            if (!Scene.Components.HasComponent<Control>(entityId)) break;
            ref var control = ref Scene.Components.GetComponent<Control>(entityId);
            var mouseFilter = GetMouseFilterMode(entityId, ref control);
            var focusMode = GetFocusMode(entityId, ref control);
            if (focus && focusMode != Control.FocusGrabMode.None)
            {
                TryGrabFocus(entityId);
                focus = false;
            }
            if (mouseFilter != Control.MouseFilterMode.Ignore)
            {
                // Console.WriteLine($"Call pressed event on {onEntityId}");
                var evt = Scene.Events.Invoke(new MouseButtonEvent
                {
                    Type = MouseButtonEventType.Pressed,
                    EntityId = entityId,
                    Position = _mousePos,
                    Button = buttonIndex
                });
            }
            if (mouseFilter == Control.MouseFilterMode.Stop) break;
                
            // pass to parent
            var hierarchical = Scene.Components.GetComponent<Hierarchical>(entityId);
            entityId = hierarchical.ParentId;
        }
    }

    private void SendMouseReleaseEvent(long entityId, int buttonIndex)
    {
        while (entityId != -1)
        {
            if (!Scene.Components.HasComponent<Control>(entityId)) break;
            ref var control = ref Scene.Components.GetComponent<Control>(entityId);
            var mouseFilter = GetMouseFilterMode(entityId, ref control);
            if (mouseFilter != Control.MouseFilterMode.Ignore)
            {
                // Console.WriteLine($"Call pressed event on {onEntityId}");
                var evt = Scene.Events.Invoke(new MouseButtonEvent
                {
                    Type = MouseButtonEventType.Released,
                    EntityId = entityId,
                    Position = _mousePos,
                    Button = buttonIndex
                });
            }
            if (mouseFilter == Control.MouseFilterMode.Stop) break;
                
            // pass to parent
            var hierarchical = Scene.Components.GetComponent<Hierarchical>(entityId);
            entityId = hierarchical.ParentId;
        }
    }

    private Control.MouseFilterMode GetMouseFilterMode(long entityId, ref Control control)
    {
        if (control.MouseFilter != Control.MouseFilterMode.Auto)
        {
            return control.MouseFilter;
        }

        if (Scene.Components.HasTrait<MouseFilterDefaultStop>(entityId)) return Control.MouseFilterMode.Stop;
        if (Scene.Components.HasTrait<MouseFilterDefaultPass>(entityId)) return Control.MouseFilterMode.Pass;
        return Control.MouseFilterMode.Ignore;
    }

    private Control.FocusGrabMode GetFocusMode(long entityId, ref Control control)
    {
        if (control.FocusMode != Control.FocusGrabMode.Auto)
        {
            return control.FocusMode;
        }

        if (Scene.Components.HasTrait<FocusModeDefaultAll>(entityId)) return Control.FocusGrabMode.All;
        if (Scene.Components.HasTrait<FocusModeDefaultClick>(entityId)) return Control.FocusGrabMode.Click;
        return Control.FocusGrabMode.None;
    }

    public bool AnyMouseDown()
    {
        return MouseLeft.Down || MouseRight.Down || MouseMiddle.Down;
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

    public bool IsMouseButtonDown(int btnIndex)
    {
        return _buttonPressMemory[btnIndex].Down;
    }

    public MouseButtonMask GetDownMouseButtonMask()
    {
        return (IsMouseButtonDown(MouseButtonEvent.LeftButton)
            ? MouseButtonMask.LeftButton
            : 0) | (IsMouseButtonDown(MouseButtonEvent.MiddleButton)
            ? MouseButtonMask.MiddleButton
            : 0) | (IsMouseButtonDown(MouseButtonEvent.RightButton)
            ? MouseButtonMask.RightButton
            : 0);
    }

    private struct MouseButtonPressMemory
    {
        public static readonly MouseButtonPressMemory Empty = new()
        {
            ControlId = -1
        };

        public bool Down;
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