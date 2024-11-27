using FoldEngine.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Components;

[Component("fold:control.tab", traits: [typeof(Control), typeof(MousePickable)])]
[ComponentInitializer(typeof(Tab), nameof(InitializeComponent))]
public struct Tab
{
    public string DeselectedButtonStyle;
    public string SelectedButtonStyle;
    
    public Tab()
    {
        
    }

    public static Tab InitializeComponent(Scene scene, long entityId)
    {
        return new Tab();
    }
}


[Component("fold:control.tab_list", traits: [typeof(Control), typeof(MousePickable)])]
[ComponentInitializer(typeof(TabList), nameof(InitializeComponent))]
public struct TabList
{
    public long SelectedTabId = -1;
    
    public TabList()
    {
        
    }

    public static TabList InitializeComponent(Scene scene, long entityId)
    {
        return new TabList();
    }
}
