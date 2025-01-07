using System;
using System.Collections.Generic;
using FoldEngine.Components;
using FoldEngine.Editor.Transactions;
using FoldEngine.Systems;

namespace FoldEngine.Scenes.Prefabs;

[GameSystem("fold:prefabs", ProcessingCycles.None)]
public class PrefabSystem : GameSystem
{
    private ComponentIterator<FromPrefab> _fromPrefabs;
    
    public override void Initialize()
    {
        _fromPrefabs = CreateComponentIterator<FromPrefab>(IterationFlags.None);
    }

    public override void SubscribeToEvents()
    {
        base.SubscribeToEvents();
        Subscribe((ref InspectorEditedComponentEvent evt) =>
        {
            if (evt.ComponentType != typeof(PrefabInstance)) return;
            Scene.Events.Invoke(new PrefabInstanceUpdateRequestEvent() { EntityId = evt.EntityId });
        });
        Subscribe((ref PrefabInstanceUpdateRequestEvent evt) =>
        {
            PackPrefab(evt.EntityId);
            UnpackPrefab(evt.EntityId);
        });
    }

    private readonly Queue<long> _tempPackQueue = new();
    private readonly Queue<long> _tempEntitiesToDelete = new();
    
    public void PackPrefab(long entityId)
    {
        long packedEntityId = entityId;
        
        _tempPackQueue.Enqueue(entityId);

        while (_tempPackQueue.TryDequeue(out entityId))
        {
            if (!Scene.Components.HasComponent<PrefabInstance>(entityId)) continue;

            if (packedEntityId == entityId)
            {
                ref var component = ref Scene.Components.GetComponent<PrefabInstance>(entityId);

                if (component.PersistentComponents != null)
                {
                    foreach (var set in Scene.Components.Sets.Values)
                    {
                        if (set.Has(entityId) && !component.PersistentComponents.Contains(Scene.Core.RegistryUnit.Components.IdentifierOf(set.ComponentType)))
                        {
                            set.Remove(entityId);
                        }
                    }
                }

                component.PersistentComponents = null;
            }
            
            _fromPrefabs.Reset();
            while (_fromPrefabs.Next())
            {
                if(_fromPrefabs.GetComponent().PrefabInstanceId == entityId)
                    _tempEntitiesToDelete.Enqueue(_fromPrefabs.GetEntityId());
            }
        }

        while (_tempEntitiesToDelete.TryDequeue(out entityId))
        {
            Scene.DeleteEntity(entityId, recursively: true);
        }
    }

    public void UnpackPrefab(long entityId)
    {
        UnpackPrefab(Scene, entityId);
    }

    public static void UnpackPrefab(Scene scene, long entityId)
    {
        if (!scene.Components.HasComponent<PrefabInstance>(entityId)) return;
        ref var component = ref scene.Components.GetComponent<PrefabInstance>(entityId);
        
        var packedScene = scene.Resources.AwaitGet<PackedScene>(ref component.Identifier);
                
        if (packedScene == null) return;

        component.PersistentComponents ??= new List<string>();
        component.PersistentComponents.Clear();
        foreach (var set in scene.Components.Sets.Values)
        {
            if (set.Has(entityId))
            {
                component.PersistentComponents.Add(scene.Core.RegistryUnit.Components.IdentifierOf(set.ComponentType));
            }
        }
        
        packedScene.Instantiate(scene, entityId, component.LoadMode);
    }
}