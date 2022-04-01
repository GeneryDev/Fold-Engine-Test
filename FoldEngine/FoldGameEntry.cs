using FoldEngine.Interfaces;

namespace FoldEngine {
    public sealed class FoldGameEntry {
        public static void StartGame(IGameCore core) {
            using(FoldGame game = core.FoldGame) game.Run();
        }
    }
}
