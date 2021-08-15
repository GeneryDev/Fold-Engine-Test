using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views {
    public abstract class EditorView {
        public Scene Scene;

        public GuiPanel ContentPanel;
        
        public abstract string Icon { get; }
        public abstract string Name { get; }

        public virtual bool UseMargin => true;
        public virtual Color? BackgroundColor => null;

        public virtual void Initialize() { }
        public abstract void Render(IRenderingUnit renderer);
    }
}