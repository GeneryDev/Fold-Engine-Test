using System;
using FoldEngine.Components;
using FoldEngine.Resources;
using FoldEngine.Systems;

namespace FoldEngine.Graphics.Atlas;

[GameSystem("fold:texture_atlas", ProcessingCycles.Update, true)]
public class TextureAtlasSystem : GameSystem
{
    private ComponentIterator<TextureAtlasComponent> _atlases;

    public override void Initialize()
    {
        _atlases = Scene.Components.CreateIterator<TextureAtlasComponent>(IterationFlags.None);
    }

    public override void OnUpdate()
    {
        _atlases.Reset();
        while (_atlases.Next())
        {
            long entityId = _atlases.GetEntityId();
            ref TextureAtlasComponent atlasComponent = ref _atlases.GetComponent();
            if (atlasComponent.Generated) continue;
            if (atlasComponent.Group == null || !atlasComponent.Group.StartsWith("#")) continue;
            if (Scene.Core.ResourceIndex.Exists<Texture>(atlasComponent.Group))
            {
                if (atlasComponent.Atlas == null)
                {
                    atlasComponent.Atlas = new TextureAtlas(atlasComponent.Group, Scene.Resources);

                    foreach (string rawIdentifier in Scene.Core.ResourceIndex.GetIdentifiersInGroup<Texture>(
                                 atlasComponent
                                     .Group))
                    {
                        atlasComponent.WaitingForLoad++;
                        var identifier = new ResourceIdentifier(rawIdentifier);
                        Scene.Resources.Load<Texture>(ref identifier, texture =>
                        {
                            ref TextureAtlasComponent atlasComponent1 =
                                ref Scene.Components.GetComponent<TextureAtlasComponent>(entityId);
                            atlasComponent1.WaitingForLoad--;
                            atlasComponent1.Atlas.AddTexture(texture.Identifier, (Texture)texture);
                        });
                    }
                }
                else if (atlasComponent.WaitingForLoad == 0)
                {
                    atlasComponent.AtlasIdentifier = new ResourceIdentifier(atlasComponent.Atlas.Pack());
                    Console.WriteLine("Generated atlas " + atlasComponent.Group);
                    atlasComponent.Generated = true;
                    atlasComponent.Atlas = null;
                }
            }
        }
    }

    public override void PollResources()
    {
        _atlases.Reset();
        while (_atlases.Next())
        {
            ref TextureAtlasComponent atlasComponent = ref _atlases.GetComponent();
            if (atlasComponent.Generated) continue;
            if (atlasComponent.Group == null || !atlasComponent.Group.StartsWith("#")) continue;
            if (atlasComponent.Atlas == null) continue;

            if (Scene.Core.ResourceIndex.Exists<Texture>(atlasComponent.Group))
                foreach (string rawIdentifier in Scene.Core.ResourceIndex.GetIdentifiersInGroup<Texture>(
                             atlasComponent
                                 .Group))
                {
                    var identifier = new ResourceIdentifier(rawIdentifier);
                    Scene.Resources.KeepLoaded<Texture>(ref identifier);
                }
        }
    }
}