using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Traits;

namespace FoldEngine.Editor.Components;

[Component("fold:editor.scene_view_panel", traits: [typeof(Control), typeof(MouseFilterDefaultStop), typeof(Scrollable), typeof(FocusModeDefaultAll), typeof(InputCaptor)])]
public struct EditorSceneViewPanel
{
    
}