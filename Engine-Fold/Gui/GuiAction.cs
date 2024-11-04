using EntryProject.Util;

namespace FoldEngine.Gui {
    public interface IGuiAction : IPooledObject {
        void Perform(GuiElement element, MouseEvent e);
    }
}