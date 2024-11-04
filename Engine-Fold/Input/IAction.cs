using System;

namespace FoldEngine.Input;

public interface IAction
{
}

public class ButtonAction : IAction
{
    public static readonly ButtonAction Default = new ButtonAction(new ButtonInfo(() => false));

    private readonly ButtonInfo _buttonInfo;

    private ButtonInfo[] _modifiers;

    public int BufferTime = 16; // ms
    public long ConsumeTime;

    public bool Repeat = false;
    public int RepeatInterval = 40;
    public int RepeatStartDelay = 400;
    public bool WhenDown = true;

    public ButtonAction(ButtonInfo buttonInfo)
    {
        _buttonInfo = buttonInfo;
    }

    public bool Consumed => _buttonInfo.Down == WhenDown
                            && ConsumeTime >= _buttonInfo.Since
                            && (!Repeat || Time.Now - _buttonInfo.Since < RepeatStartDelay);

    public bool Pressed => _buttonInfo.Down && _buttonInfo.SinceTick == Time.TotalFixedTicks;
    public bool Down => _buttonInfo.Down;
    public bool Released => !_buttonInfo.Down && _buttonInfo.SinceTick == Time.TotalFixedTicks;

    public ButtonAction Modifiers(params ButtonInfo[] modifiers)
    {
        _modifiers = modifiers;
        return this;
    }

    public bool Consume()
    {
        if (_modifiers != null)
            foreach (ButtonInfo modifier in _modifiers)
                if (!modifier.Down)
                    return false;
        if (_buttonInfo.Down == WhenDown
            && !Consumed
            && (Repeat && Time.Now - _buttonInfo.Since >= RepeatStartDelay
                ? Time.Now - ConsumeTime >= RepeatInterval
                : _buttonInfo.MillisecondsElapsed <= BufferTime))
        {
            ConsumeTime = Time.Now;
            return true;
        }

        return false;
    }
}

public class AnalogAction : IAction
{
    public static readonly AnalogAction Default = new AnalogAction(() => 0);

    private readonly Func<float> _provider;

    public AnalogAction(Func<float> provider)
    {
        _provider = provider;
    }

    public static implicit operator float(AnalogAction action)
    {
        return action._provider();
    }
}

public class ChangeAction : IAction
{
    public static readonly ChangeAction Default = new ChangeAction(new AnalogInfo1(() => 0), 1, 1);

    private readonly IAnalogInfo _analog;
    private readonly int _axis;
    private float? _max;
    private float? _min;

    public ChangeAction(IAnalogInfo analog, float? min, float? max, int axis = 0)
    {
        _analog = analog;
        _min = min;
        _max = max;
        _axis = axis;
    }

    public static implicit operator bool(ChangeAction action)
    {
        if (action._analog.LastChangedTime != Time.Now) return false;
        float change = action._analog.GetChange(action._axis);
        if (action._min.HasValue && change < action._min) return false;
        if (action._max.HasValue && change > action._max) return false;
        return true;
    }
}