﻿using System;
using Microsoft.Xna.Framework;

namespace FoldEngine;

public static class Time
{
    public static float FixedDeltaTime = 1.0f / 120;
    public static double FixedDeltaTimeD = 1.0 / 120;
    public static float DeltaTime { get; internal set; }
    public static double DeltaTimeD { get; internal set; }

    public static float TotalTime { get; internal set; }
    public static double TotalTimeD { get; internal set; }
    
    public static long TotalTimeMs { get; internal set; }

    public static float FramesPerSecond { get; internal set; }

    public static long Now { get; internal set; }
    public static long TotalFrames { get; internal set; }
    public static long TotalFixedTicks { get; internal set; }

    internal static void Update(GameTime gameTime)
    {
        DeltaTimeD = gameTime.ElapsedGameTime.TotalSeconds;
        TotalTimeD = gameTime.TotalGameTime.TotalSeconds;
        TotalTimeMs = (long)gameTime.TotalGameTime.TotalMilliseconds;

        DeltaTime = (float)DeltaTimeD;
        TotalTime = (float)TotalTimeD;

        Now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}