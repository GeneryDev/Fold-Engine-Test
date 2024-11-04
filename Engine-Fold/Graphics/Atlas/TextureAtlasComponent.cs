﻿using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui;
using FoldEngine.Resources;
using FoldEngine.Serialization;
using Microsoft.Xna.Framework;

namespace FoldEngine.Graphics.Atlas;

[Component("fold:texture_atlas")]
public struct TextureAtlasComponent
{
    public string Group;

    [DoNotSerialize] [HideInInspector] public bool Generated;

    [DoNotSerialize] [HideInInspector] public int WaitingForLoad;

    [DoNotSerialize] [HideInInspector] public TextureAtlas Atlas;

    [DoNotSerialize] [HideInInspector] public ResourceIdentifier AtlasIdentifier;
}

[CustomInspector(typeof(TextureAtlasComponent))]
public class TextureAtlasComponentInspector : CustomInspector<TextureAtlasComponent>
{
    protected override void RenderInspectorAfter(TextureAtlasComponent obj, GuiPanel panel)
    {
        if (obj.Generated)
        {
            var texture = panel.Environment.Scene.Resources.Get<Texture>(ref obj.AtlasIdentifier);
            panel.Element<GuiLabel>().Text("Generated").FontSize(9).TextAlignment(-1);
            panel.Element<GuiLabel>().Text("Dimensions: " + texture.Width + " x " + texture.Height).FontSize(9)
                .TextAlignment(-1);
            panel.Element<GuiImage>().Image(texture, Color.White, width: panel.Bounds.Width);
        }
        else
        {
            panel.Element<GuiLabel>().Text("Not yet generated").FontSize(9).TextAlignment(-1);
        }
    }
}