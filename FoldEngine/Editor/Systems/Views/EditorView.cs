using FoldEngine.Interfaces;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Views {
    public abstract class EditorView {
        public Scene Scene;

        public GuiPanel ContentPanel;
        
        public abstract string Icon { get; }
        public abstract string Name { get; }

        public virtual void Initialize() { }
        public abstract void Render(IRenderingUnit renderingUnit);
    }
}