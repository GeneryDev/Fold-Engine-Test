using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoldEngine.Interfaces;

namespace FoldEngine
{
    public sealed class FoldGameEntry
    {
        public static void StartGame(IGameController controller)
        {
            using (var game = new FoldGame(controller))
                game.Run();
        }
    }
}
