﻿using System;

namespace FoldEngine.Input;

public interface IInputInfo
{
    void Update();
}

public class ButtonInfo : IInputInfo
{
    public bool Down;
    public Func<bool> Lookup;
    public long Since;
    public long SinceTick;

    public ButtonInfo(Func<bool> lookup)
    {
        Lookup = lookup;
        Update();
    }

    public long MillisecondsElapsed => Time.Now - Since;

    public void Update()
    {
        bool nowDown = Lookup();
        if (nowDown != Down)
        {
            Since = Time.Now;
            SinceTick = Time.TotalFixedTicks;
            Down = nowDown;
        }
    }
}