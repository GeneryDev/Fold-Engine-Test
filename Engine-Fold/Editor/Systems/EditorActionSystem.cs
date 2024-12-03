using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Editor.Components;
using FoldEngine.Editor.Components.Traits;
using FoldEngine.Editor.Events;
using FoldEngine.Editor.Transactions;
using FoldEngine.Gui.Events;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Systems;

[GameSystem("fold:editor.actions", ProcessingCycles.None)]
public class EditorActionSystem : GameSystem
{
    private static readonly List<long> EntitiesToDelete = new List<long>();
    
    public override void SubscribeToEvents()
    {
        Subscribe((ref ButtonPressedEvent evt) =>
        {
            if (Scene.Components.HasTrait<EditorAction>(evt.EntityId))
            {
                Scene.Events.Invoke(new EditorActionTriggered()
                {
                    EntityId = evt.EntityId
                });
            }
        });
        Subscribe((ref EditorActionTriggered evt) =>
        {
            var entity = new Entity(Scene, evt.EntityId);
            if (entity.HasComponent<LegacyActionComponent>())
            {
                ref var component = ref entity.GetComponent<LegacyActionComponent>();
                component.Action.Perform(component.Element, default);
            }
            if (entity.HasComponent<TransactionActionComponent>())
            {
                ref var component = ref entity.GetComponent<TransactionActionComponent>();
                
                Scene.Systems.Get<EditorBase>().CurrentSceneTab.SceneTransactions.InsertTransaction(component.Transaction);
            }
            if (entity.HasComponent<EntityActionComponent>())
            {
                PerformEntityAction(ref entity.GetComponent<EntityActionComponent>());
            }
        });
    }

    private void PerformEntityAction(ref EntityActionComponent component)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;
        if (editingTab.Scene == null) return;
        var editingScene = editingTab.Scene;

        switch (component.Type)
        {
            case EntityActionComponent.ActionType.Delete:
            {
                EntitiesToDelete.Clear();
                if (editingScene.Components.HasComponent<Hierarchical>(component.AffectedEntityId))
                    editingScene.Components.GetComponent<Hierarchical>(component.AffectedEntityId)
                        .DumpHierarchy(EntitiesToDelete);

                var transactions = new CompoundTransaction<Scene>();
                foreach (long entityId in EntitiesToDelete)
                    transactions.Append(() => new DeleteEntityTransaction(entityId));

                Scene.Systems.Get<EditorBase>().CurrentSceneTab.SceneTransactions.InsertTransaction(transactions);
                break;
            }
            case EntityActionComponent.ActionType.CreateChild:
            {
                Scene.Systems.Get<EditorBase>().CurrentSceneTab.SceneTransactions.InsertTransaction(
                    new CreateEntityTransaction(component.AffectedEntityId));
                break;
            }
            case EntityActionComponent.ActionType.None:
            default:
                break;
        }
    }
}