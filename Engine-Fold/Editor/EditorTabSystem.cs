using FoldEngine.Gui.Components;
using FoldEngine.Gui.Events;
using FoldEngine.Gui.Styles;
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
                if (Scene.Components.HasComponent<ButtonControl>(evt.SourceEntityId))
                {
                    ref var tabButton = ref Scene.Components.GetComponent<ButtonControl>(evt.SourceEntityId);
                    
                    var dragVisualEntity = Scene.CreateEntity("Drag Visual");
                    dragVisualEntity.AddComponent<Control>().ZOrder = 100;
                    dragVisualEntity.AddComponent<AnchoredControl>() = new AnchoredControl()
                    {
                        AnchorLeft = 0.5f,
                        AnchorRight = 0.5f
                    };
                    dragVisualEntity.Hierarchical.SetParent(evt.DragOperationEntityId);
                    
                    var buttonStyle = Scene.Resources.Get(ref tabButton.Style, ButtonStyle.Default);
                    dragVisualEntity.AddComponent<LabelControl>() = new LabelControl()
                    {
                        Text = tabButton.Text,
                        FontSize = buttonStyle.FontSize
                    };
                }
                evt.HasData = true;
            }
        });
    }
}