using System.Collections.Generic;
using FoldEngine.Interfaces;

namespace FoldEngine.Input;

public class InputAction
{
    private List<InputEventFilter> _eventFilters;
    
    // Settings
    public bool Holdable = false;
    
    public bool CanEcho = false;
    public int EchoIntervalMs = 40;
    public int EchoStartDelayMs = 400;

    public int BufferTimeMs = 0;
    
    // State
    public bool Down = false;
    private long _pressStartTimeMs;
    private long _lastEchoTimeMs;
    private bool _echoStarted = false;

    public void HandleInputEvent(ref InputEvent evt, string actionName, int playerIndex, InputUnit inputUnit)
    {
        if (!Match(evt)) return;

        if (Holdable && Down) return; // already down, don't send multiple presses for the same action

        _pressStartTimeMs = Time.TotalTimeMs;
        _echoStarted = false;
        _lastEchoTimeMs = 0;
        
        var actionEvt = new InputActionEvent()
        {
            PlayerIndex = playerIndex,
            ActionName = actionName,
            TriggerCause = Holdable ? InputActionTriggerCause.Press : InputActionTriggerCause.Tap
        };
        inputUnit.InvokeActionEvent(actionEvt);
        if (actionEvt.Consumed)
        {
            evt.Consume();
        }
    }

    public void Update(string actionName, int playerIndex, InputUnit inputUnit)
    {
        if (Holdable && Down)
        {
            if (IsDown(inputUnit))
            {
                if (CanEcho)
                {
                    UpdateEchoes(actionName, playerIndex, inputUnit);
                }
            }
            else
            {
                // release
                Down = false;
                
                inputUnit.InvokeActionEvent(new InputActionEvent()
                {
                    PlayerIndex = playerIndex,
                    ActionName = actionName,
                    TriggerCause = InputActionTriggerCause.Release
                });
            }
        }
    }

    private void UpdateEchoes(string actionName, int playerIndex, InputUnit inputUnit)
    {
        int echoCount = 0;
        long now = Time.TotalTimeMs;
        if (!_echoStarted)
        {
            if (now - _pressStartTimeMs >= EchoStartDelayMs)
            {
                echoCount++;
                _echoStarted = true;
                _lastEchoTimeMs = _pressStartTimeMs + EchoStartDelayMs;
            }
        }

        if (_echoStarted)
        {
            while (now - _lastEchoTimeMs >= EchoIntervalMs)
            {
                echoCount++;
                _lastEchoTimeMs += EchoIntervalMs;
            }
        }

        while (echoCount-- > 0)
        {
            inputUnit.InvokeActionEvent(new InputActionEvent()
            {
                PlayerIndex = playerIndex,
                ActionName = actionName,
                TriggerCause = InputActionTriggerCause.Echo
            });
        }
    }

    public bool Match(InputEvent evt)
    {
        if (_eventFilters == null) return false;
        foreach (var filter in _eventFilters)
        {
            if (filter.Match(evt)) return true;
        }
        return false;
    }

    public bool IsDown(InputUnit inputUnit)
    {
        if (!Holdable) return false;
        if (_eventFilters == null) return false;
        
        foreach (var filter in _eventFilters)
        {
            if (filter.IsDown(inputUnit)) return true;
        }
        return false;
    }
}