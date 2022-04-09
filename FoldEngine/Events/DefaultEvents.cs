using Microsoft.Xna.Framework;

namespace FoldEngine.Events {

    [Event("fold:window.size_changed", EventFlushMode.End)]
    public struct WindowSizeChangedEvent {
        public Point OldSize;
        public Point NewSize;
        
        public WindowSizeChangedEvent(Point oldSize, Point newSize) {
            this.OldSize = oldSize;
            this.NewSize = newSize;
        }
    }
}