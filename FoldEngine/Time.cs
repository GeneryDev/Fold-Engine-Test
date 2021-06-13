using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoldEngine
{
    public static class Time
    {
        public static float DeltaTime { get; internal set; }
        public static float TotalTime { get; internal set; }

        public static double DeltaTimeD { get; internal set; }
        public static double TotalTimeD { get; internal set; }
        
        public static float FramesPerSecond { get; internal set; }
        
        public static long Now { get; internal set; }
        public static long Frame { get; internal set; }

        internal static void Update(GameTime gameTime) {
            Time.DeltaTimeD = gameTime.ElapsedGameTime.TotalSeconds;
            Time.TotalTimeD = gameTime.TotalGameTime.TotalSeconds;

            DeltaTime = (float) DeltaTimeD;
            TotalTime = (float) TotalTimeD;

            Now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
