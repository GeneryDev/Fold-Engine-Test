using System.Collections.Generic;
using FoldEngine.Interfaces;

namespace FoldEngine.Commands {
    public class CommandQueue {
        public IGameCore Core;
        private List<ICommand> _commands = new List<ICommand>();

        public CommandQueue(IGameCore core) {
            Core = core;
        }

        public void Enqueue(ICommand command) {
            _commands.Add(command);
        }

        public void ExecuteAll() {
            foreach(ICommand command in _commands) {
                command.Execute(Core);
            }
            _commands.Clear();
        }
    }
}