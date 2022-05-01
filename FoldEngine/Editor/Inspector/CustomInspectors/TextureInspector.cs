using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Inspector.CustomInspectors {
    [CustomInspector(typeof(Texture))]
    public class TextureInspector : CustomInspector<Texture> {
        protected override void RenderInspectorBefore(Texture obj, GuiPanel panel) {
            if(obj.Parent != null) {
                panel.Element<GuiLabel>().Text("From atlas: " + obj.Parent.Identifier).FontSize(9).TextAlignment(-1);
            }
            panel.Element<GuiLabel>().Text("Dimensions: " + obj.Width + " x " + obj.Height).FontSize(9).TextAlignment(-1);
            panel.Element<GuiImage>().Image(obj, Color.White, width: panel.Bounds.Width);
        }
    }
}