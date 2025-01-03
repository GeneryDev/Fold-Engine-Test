using FoldEngine.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Components;

[Component("fold:control.tab")]
[ComponentInitializer(typeof(Tab))]
public struct Tab
{
    public string DeselectedButtonStyle;
    public string SelectedButtonStyle;

    [EntityId] public long LinkedEntityId = -1;
    
    public Tab()
    {
    }
}

[Component("fold:control.tab_list")]
[ComponentInitializer(typeof(TabList))]
public struct TabList
{
    public long SelectedTabId = -1;
    
    public TabList()
    {
    }
}

[Component("fold:control.tab_switcher")]
[ComponentInitializer(typeof(TabSwitcher))]
public struct TabSwitcher
{
    [EntityId] public long ContainerEntityId = -1;
    
    public TabSwitcher()
    {
    }
}
