using FoldEngine.Graphics;
using FoldEngine.Interfaces;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Systems {

    [GameSystem("fold:editor.rendering", ProcessingCycles.Render)]
    class EditorRendering : GameSystem {
        public const int SidebarX = 1280 * 4 / 5;
        public const int SidebarWidth = 1280 / 5;

        public static int SidebarMargin = 8;

        // public override bool ShouldSave => false;


        public override void OnRender(IRenderingUnit renderer) {
            IRenderingLayer layer = renderer.Layers["screen"];
            Rectangle sidebar = new Rectangle(SidebarX, 0, SidebarWidth, 720);
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.Textures["main:pixel.white"],
                DestinationRectangle = sidebar,
                Color = new Color(45, 45, 48)
            });
            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.Textures["main:pixel.white"],
                DestinationRectangle = new Rectangle(sidebar.X + SidebarMargin, sidebar.Y + SidebarMargin, sidebar.Width - SidebarMargin*2, sidebar.Height - SidebarMargin*2),
                Color = new Color(37, 37, 38)
            });
        }
    }
}