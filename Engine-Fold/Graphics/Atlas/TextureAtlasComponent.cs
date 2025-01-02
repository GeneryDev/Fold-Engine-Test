using FoldEngine.Components;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.Inspector;
using FoldEngine.ImmediateGui;
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

    [DoNotSerialize] [HideInInspector] [ResourceIdentifier(typeof(Texture))] public ResourceIdentifier AtlasIdentifier;
}

[CustomInspector(typeof(TextureAtlasComponent))]
public class TextureAtlasComponentInspector : CustomInspector<TextureAtlasComponent>
{
    protected override void RenderInspectorAfter(TextureAtlasComponent obj, GuiPanel panel)
    {
        if (obj.Generated)
        {
            var editorEnvironment = panel.Environment as EditorEnvironment;
            var scene = editorEnvironment?.Scene ?? panel.Environment.Scene;
            var texture = scene.Resources.Get<Texture>(ref obj.AtlasIdentifier);
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