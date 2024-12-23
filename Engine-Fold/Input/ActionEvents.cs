using FoldEngine.Events;

namespace FoldEngine.Input;

[Event("fold:input_action", EventFlushMode.Immediate)]
public struct InputActionEvent
{
    public string ActionName;
    public int PlayerIndex;
    public InputActionTriggerCause TriggerCause;
    public bool Consumed;

    public void Consume()
    {
        Consumed = true;
    }
}

public enum InputActionTriggerCause
{
    Tap,
    Press,
    Echo,
    Release
}