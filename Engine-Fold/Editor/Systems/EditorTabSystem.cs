using System;
using FoldEngine.Components;
using FoldEngine.Editor.Events;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Views;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Events;
using FoldEngine.Resources;
using FoldEngine.Systems;
using FoldEngine.Util;

namespace FoldEngine.Editor.Systems;

[GameSystem("fold:editor.tabs", ProcessingCycles.None)]
public class EditorTabSystem : GameSystem
{
    private ComponentIterator<EditorTab> _editorTabs;
    private ComponentIterator<ImmediateGuiControl> _immediateControls;
    
    public override void Initialize()
    {
        _editorTabs = CreateComponentIterator<EditorTab>(IterationFlags.None);
        _immediateControls = CreateComponentIterator<ImmediateGuiControl>(IterationFlags.None);
    }

    public override void SubscribeToEvents()
    {
        Subscribe((ref DragDataRequestedEvent evt) =>
        {
            if (Scene.Components.HasComponent<EditorTab>(evt.SourceEntityId))
            {
                Scene.Components.CreateComponent<EditorTabDragData>(evt.DragOperationEntityId) = new EditorTabDragData()
                {
                    TabId = evt.SourceEntityId
                };
                if (Scene.Components.HasComponent<ButtonControl>(evt.SourceEntityId) && Scene.Components.HasComponent<Control>(evt.SourceEntityId))
                {
                    ref var tabButton = ref Scene.Components.GetComponent<ButtonControl>(evt.SourceEntityId);
                    ref var tabControl = ref Scene.Components.GetComponent<Control>(evt.SourceEntityId);
                    
                    var dragVisualEntity = Scene.CreateEntity("Drag Visual");
                    dragVisualEntity.AddComponent<Control>() = new Control
                    {
                        ZOrder = 100,
                        MouseFilter = Control.MouseFilterMode.Ignore,
                        MinimumSize = tabControl.Size
                    };
                    dragVisualEntity.AddComponent<AnchoredControl>() = new AnchoredControl()
                    {
                        Anchor = new LRTB {Left = 0.5f, Right = 0.5f}
                    };
                    dragVisualEntity.Hierarchical.SetParent(evt.DragOperationEntityId);
                    
                    dragVisualEntity.AddComponent<ButtonControl>() = new ButtonControl()
                    {
                        Text = tabButton.Text,
                        Icon = new ResourceIdentifier(tabButton.Icon.Identifier),
                        Style = new ResourceIdentifier(tabButton.Style.Identifier)
                    };
                }
                evt.HasData = true;
            }
        });
        Subscribe((ref DropValidationRequestedEvent evt) =>
        {
            if (Scene.Components.HasComponent<EditorTabDragData>(evt.DragOperationEntityId) &&
                Scene.Components.HasComponent<EditorTabDropTarget>(evt.TargetEntityId))
            {
                evt.CanDrop = true;
            }
        });
        Subscribe((ref DroppedDataEvent evt) =>
        {
            if (!evt.Consumed &&
                Scene.Components.HasComponent<EditorTabDragData>(evt.DragOperationEntityId) &&
                Scene.Components.HasComponent<EditorTabDropTarget>(evt.TargetEntityId))
            {
                var dragData = Scene.Components.GetComponent<EditorTabDragData>(evt.DragOperationEntityId);
                var dropTargetPanel = Scene.Components.GetComponent<EditorTabDropTarget>(evt.TargetEntityId);
                if(dragData.TabId != -1 && Scene.Components.HasComponent<Hierarchical>(dragData.TabId))
                {
                    ref var tabHierarchical = ref Scene.Components.GetComponent<Hierarchical>(dragData.TabId);
                    tabHierarchical.SetParent(dropTargetPanel.TabBarId);

                    ref var tabList = ref Scene.Components.GetComponent<TabList>(dropTargetPanel.TabBarId);
                    tabList.SelectedTabId = dragData.TabId;
                    Scene.Events.Invoke(new TabSelectedEvent()
                    {
                        TabId = dragData.TabId,
                        TabListId = dropTargetPanel.TabBarId
                    });
                    
                    evt.Consumed = true;
                }
            }
        });
        Subscribe((ref EditorTabSwitchRequestedEvent evt) =>
        {
            _editorTabs.Reset();
            while (_editorTabs.Next())
            {
                var tab = _editorTabs.GetComponent();
                if (tab.TabName != evt.TabName) continue;
                long tabId = _editorTabs.GetEntityId();

                var hierarchical = _editorTabs.GetCoComponent<Hierarchical>();
                long tabListId = hierarchical.ParentId;
                if (!Scene.Components.HasComponent<TabList>(tabListId)) continue;
                
                ref var tabList = ref Scene.Components.GetComponent<TabList>(tabListId);
                tabList.SelectedTabId = tabId;
                Scene.Events.Invoke(new TabSelectedEvent()
                {
                    TabListId = tabListId,
                    TabId = tabId,
                });
                break;
            }
        });
        Subscribe((ref EntityInspectorRequestedEvent evt) =>
        {
            var inspector = GetEditorView<EditorInspectorView>();
            inspector?.SetObject(null);

            Scene.Events.Invoke(new EditorTabSwitchRequestedEvent() { TabName = "Inspector" });
        });
        Subscribe((ref ObjectInspectorRequestedEvent evt) =>
        {
            var inspector = GetEditorView<EditorInspectorView>();
            inspector?.SetObject(evt.Object);
            
            Scene.Events.Invoke(new EditorTabSwitchRequestedEvent() { TabName = "Inspector" });
        });
        Subscribe((ref MouseButtonEvent evt) =>
        {
            if (!Scene.Components.HasComponent<Tab>(evt.EntityId)) return;
            if (evt is not { Button: MouseButtonEvent.MiddleButton, Type: MouseButtonEventType.Pressed }) return;
            // close tab
            Scene.Systems.Get<EditorBase>().CloseScene(evt.EntityId);
        });
    }

    private T GetEditorView<T>() where T : EditorView
    {
        _immediateControls.Reset();
        while (_immediateControls.Next())
        {
            var ic = _immediateControls.GetComponent();
            if (ic.View is T t) return t;
        }

        return null;
    }
}