using FoldEngine.Util;

namespace FoldEngine.ImmediateGui;

public interface IGuiAction : IPooledObject
{
    void Perform(GuiElement element, MouseEvent e);
}