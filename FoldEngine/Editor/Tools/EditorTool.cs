﻿using FoldEngine.Editor.Gui;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Tools {
    public abstract class EditorTool {
        protected readonly EditorEnvironment Environment;
        protected Scene Scene => Environment.Scene;
        

        public EditorTool(EditorEnvironment environment) {
            Environment = environment;
        }

        public abstract void OnInput(ControlScheme controls);
        public abstract void OnMousePressed(ref MouseEvent e);
        public abstract void OnMouseReleased(ref MouseEvent e);
        
    }
}