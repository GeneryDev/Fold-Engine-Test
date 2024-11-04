using FoldEngine.Interfaces;

namespace FoldEngine.Commands {
    public interface ICommand {
        void Execute(IGameCore core);
    }
}