using FoldEngine.Components;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Events;
using FoldEngine.Resources;
using FoldEngine.Systems;

namespace FoldEngine.Gui.Systems;

[GameSystem("fold:control.tabs", ProcessingCycles.Update)]
public class TabSystem : GameSystem
{
    private ComponentIterator<TabList> _tabLists;

    public override void Initialize()
    {
        _tabLists = CreateComponentIterator<TabList>(IterationFlags.None);
    }

    public override void OnUpdate()
    {
        _tabLists.Reset();
        while (_tabLists.Next())
        {
            ref var tabList = ref _tabLists.GetComponent();
            long tabListId = _tabLists.GetEntityId();
            var hierarchical = _tabLists.GetCoComponent<Hierarchical>();

            bool reselect = tabList.SelectedTabId == -1 // No tab selected
                            || !Scene.Components.HasComponent<Tab>(tabList.SelectedTabId) // selected tab no longer has tab component
                            || Scene.Components.GetComponent<Hierarchical>(tabList.SelectedTabId).ParentId != tabListId; // selected tab no longer a child of this list

            foreach (long childId in hierarchical.GetChildren())
            {
                if (!Scene.Components.HasComponent<Tab>(childId)) continue;

                if (reselect)
                {
                    tabList.SelectedTabId = childId;
                    
                    reselect = false;
                    Scene.Events.Invoke(new TabSelectedEvent()
                    {
                        TabListId = tabListId,
                        TabId = tabList.SelectedTabId
                    });
                }
            }

            if (reselect && tabList.SelectedTabId != -1)
            {
                tabList.SelectedTabId = -1;
                Scene.Events.Invoke(new TabSelectedEvent()
                {
                    TabListId = tabListId,
                    TabId = tabList.SelectedTabId
                });
            }
        }
    }

    public override void SubscribeToEvents()
    {
        Subscribe((ref ButtonPressedEvent evt) =>
        {
            if (!Scene.Components.HasComponent<Tab>(evt.EntityId)) return;
            if (!Scene.Components.HasComponent<Hierarchical>(evt.EntityId)) return;
            
            ref var tabHierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.EntityId);
            if (!tabHierarchical.HasParent) return;

            long parentId = tabHierarchical.ParentId;
            if (!Scene.Components.HasComponent<TabList>(parentId)) return;

            ref var tabList = ref Scene.Components.GetComponent<TabList>(parentId);

            tabList.SelectedTabId = evt.EntityId;
            Scene.Events.Invoke(new TabSelectedEvent()
            {
                TabListId = parentId,
                TabId = tabList.SelectedTabId
            });
        });
        Subscribe((ref TabSelectedEvent evt) =>
        {
            if (!Scene.Components.HasComponent<TabList>(evt.TabListId)) return;
            if (!Scene.Components.HasComponent<Hierarchical>(evt.TabListId)) return;
            ref var tabListHierarchical = ref Scene.Components.GetComponent<Hierarchical>(evt.TabListId);

            foreach (long childId in tabListHierarchical.GetChildren())
            {
                if (!Scene.Components.HasComponent<Tab>(childId)) return;
                ref var tab = ref Scene.Components.GetComponent<Tab>(childId);

                if (Scene.Components.HasComponent<ButtonControl>(childId))
                {
                    ref var button = ref Scene.Components.GetComponent<ButtonControl>(childId);
                    bool selected = evt.TabId == childId;
                    string buttonStyle = selected ? tab.SelectedButtonStyle : tab.DeselectedButtonStyle;
                    if (buttonStyle != null)
                    {
                        button.Style = new ResourceIdentifier(buttonStyle);
                    }
                }
            }
        });
        Subscribe((ref TabSelectedEvent evt) =>
        {
            if (!Scene.Components.HasComponent<TabSwitcher>(evt.TabListId)) return;
            if (!Scene.Components.HasComponent<Hierarchical>(evt.TabListId)) return;
            
            ref var tabSwitcher = ref Scene.Components.GetComponent<TabSwitcher>(evt.TabListId);
            long containerId = tabSwitcher.ContainerEntityId;
            if (containerId == -1 || !Scene.Components.HasComponent<Hierarchical>(containerId)) return;
            ref var containerHierarchical = ref Scene.Components.GetComponent<Hierarchical>(containerId);
            
            long newContentId = -1;
            if (Scene.Components.HasComponent<Tab>(evt.TabId))
            {
                newContentId = Scene.Components.GetComponent<Tab>(evt.TabId).LinkedEntityId;
            }

            foreach (long childId in containerHierarchical.GetChildren())
            {
                if (Scene.Components.HasComponent<Hierarchical>(childId))
                {
                    if (childId != newContentId)
                    {
                        Scene.Components.GetComponent<Hierarchical>(childId).Active = false;
                    }
                }
            }

            if (newContentId != -1 && Scene.Components.HasComponent<Hierarchical>(newContentId) && Scene.Components.HasComponent<Control>(newContentId))
            {
                ref var contentHierarchical = ref Scene.Components.GetComponent<Hierarchical>(newContentId);

                if (contentHierarchical.ParentId != containerId)
                {
                    contentHierarchical.SetParent(containerId);
                }
                
                contentHierarchical.Active = true;
            }
        });
    }
}