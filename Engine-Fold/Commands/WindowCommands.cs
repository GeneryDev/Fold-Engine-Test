using System;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace FoldEngine.Commands {
    public class SetWindowTitleCommand : ICommand {
        private readonly string _title;

        public SetWindowTitleCommand(string title) {
            _title = title;
        }

        public void Execute(IGameCore core) {
            core.FoldGame.Window.Title = _title;
        }
    }

    public class SetWindowSizeCommand : ICommand {
        private readonly Point _size;

        public SetWindowSizeCommand(Point size) {
            _size = size;
        }

        public void Execute(IGameCore core) {
            Console.WriteLine("setting window size: " + _size);
            // foreach(IRenderingLayer layer in core.RenderingUnit.Layers.Values) {
            //     Rectangle newDestination = layer.Destination;
            //     newDestination.Offset(((_size - core.RenderingUnit.WindowSize).ToVector2() / 2).ToPoint());
            //     layer.Destination = newDestination;
            // }
            core.RenderingUnit.WindowSize = _size;
        }
    }
}