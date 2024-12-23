using System;
using FoldEngine.ImmediateGui;
using FoldEngine.Resources;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Inspector.CustomInspectors;

[CustomInspector(typeof(Resource))]
public class ResourceInspector : CustomInspector<Resource>
{
    protected override void RenderInspectorBefore(Resource obj, GuiPanel panel)
    {
        panel.Element<GuiLabel>().Text("Identifier: " + obj.Identifier).FontSize(9).TextAlignment(-1);
    }
    protected override void RenderInspectorAfter(Resource obj, GuiPanel panel)
    {
        if (obj is Scene scene)
        {
            if (panel.Button("Edit Scene", 14).IsPressed())
            {
                panel.Environment.Scene.Systems.Get<EditorBase>().OpenScene(scene);
            }
        }
    }
}