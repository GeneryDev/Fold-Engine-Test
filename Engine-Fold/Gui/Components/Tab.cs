using FoldEngine.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Scenes;

namespace FoldEngine.Gui.Components;

[Component("fold:control.tab")]
[ComponentInitializer(typeof(Tab), nameof(InitializeComponent))]
public struct Tab
{
    public string DeselectedButtonStyle;
    public string SelectedButtonStyle;

    [EntityId] public long LinkedEntityId = -1;
    
    public Tab()
    {
        
    }

    public static Tab InitializeComponent(Scene scene, long entityId)
    {
        return new Tab();
    }
}

[Component("fold:control.tab_list")]
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

[Component("fold:control.tab_switcher")]
[ComponentInitializer(typeof(TabSwitcher), nameof(InitializeComponent))]
public struct TabSwitcher
{
    [EntityId] public long ContainerEntityId = -1;
    
    public TabSwitcher()
    {
    }

    public static TabSwitcher InitializeComponent(Scene scene, long entityId)
    {
        return new TabSwitcher();
    }
}
