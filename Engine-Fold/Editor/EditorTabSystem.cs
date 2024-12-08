using System;
using FoldEngine.Components;
using FoldEngine.Editor.Events;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Controls;
using FoldEngine.Gui.Events;
using FoldEngine.Gui.Styles;
using FoldEngine.Resources;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor;

[GameSystem("fold:editor.tabs", ProcessingCycles.None)]
public class EditorTabSystem : GameSystem
{
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
                        AnchorLeft = 0.5f,
                        AnchorRight = 0.5f
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
            Console.WriteLine($"TODO: Editor tab switch: {evt.TabName}");
        });
        Subscribe((ref EntityInspectorRequestedEvent evt) =>
        {
            Console.WriteLine($"TODO: inspect entities");
        });
        Subscribe((ref ObjectInspectorRequestedEvent evt) =>
        {
            Console.WriteLine($"TODO: inspect object");
        });
    }
}