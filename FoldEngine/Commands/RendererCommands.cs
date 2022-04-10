using System;
using System.Collections.Generic;
using FoldEngine.Editor;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;

namespace FoldEngine.Commands {
    public class SetRootRendererGroupCommand : ICommand {
        public RenderGroup Group;

        public SetRootRendererGroupCommand(RenderGroup group) {
            Group = group;
        }

        public void Execute(IGameCore core) {
            core.RenderingUnit.RootGroup = Group;
        }
    }
}