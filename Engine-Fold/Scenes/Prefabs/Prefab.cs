using System;
using FoldEngine.Components;
using FoldEngine.Resources;
using FoldEngine.Serialization;
using FoldEngine.Util;

namespace FoldEngine.Scenes.Prefabs;

[Component("fold:prefab")]
public struct Prefab
{
    [ResourceIdentifier(typeof(PackedScene))] public ResourceIdentifier Identifier;

    public PrefabLoadMode LoadMode;
}

public enum PrefabLoadMode
{
    Replace,
    AsChild
}

public class PrefabSerializer : CustomComponentSerializer
{
    public override bool HandlesComponentType(Type type)
    {
        return type == typeof(Prefab);
    }

    public override void ScenePreSerialize(Scene scene, SaveOperation writer)
    {
        if (writer.Options.Get(CollapsePrefabs.Instance).IdsFromPrefabs is { } idsFromPrefabs)
        {
            var iterator = scene.Components.CreateIterator<FromPrefab>(IterationFlags.None);
            iterator.Reset();
            while (iterator.Next())
            {
                idsFromPrefabs.Add(iterator.GetEntityId());
            }
        }
    }

    public override bool Deserialize(ComponentSet componentSet, long entityId, LoadOperation reader)
    {
        if (reader.Options.Get(ExpandPrefabs.Instance).IdsWithPrefabs is { } idsWithPrefabs)
        {
            idsWithPrefabs.Add(entityId);
        }
        return false;
    }

    public override void ScenePostDeserialize(Scene scene, LoadOperation reader)
    {
        if (reader.Options.Get(ExpandPrefabs.Instance).IdsWithPrefabs is { } idsWithPrefabs)
        {
            foreach (long entityId in idsWithPrefabs)
            {
                if (!scene.Components.HasComponent<Prefab>(entityId)) continue;
                ref var prefabComponent = ref scene.Components.GetComponent<Prefab>(entityId);
                var packedScene = scene.Resources.AwaitGet<PackedScene>(ref prefabComponent.Identifier);
                
                InstantiatePrefab(scene, entityId, ref prefabComponent, packedScene);
            }
        }
    }

    private void InstantiatePrefab(Scene scene, long entityId, ref Prefab prefabComponent, PackedScene packedScene)
    {
        if (packedScene == null) return;

        packedScene.Instantiate(scene, entityId, prefabComponent.LoadMode);
    }
}