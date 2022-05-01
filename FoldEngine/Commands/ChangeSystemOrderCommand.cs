using System;
using FoldEngine.Interfaces;

namespace FoldEngine.Commands {
    public class ChangeSystemOrderCommand : ICommand {
        private Type _sysType;
        private int _toIndex;
        
        public ChangeSystemOrderCommand(Type sysType, int toIndex) {
            this._sysType = sysType;
            this._toIndex = toIndex;
        }
        
        public void Execute(IGameCore core) {
            core.ActiveScene.Systems.ChangeSystemOrder(_sysType, _toIndex);
        }
    }
}