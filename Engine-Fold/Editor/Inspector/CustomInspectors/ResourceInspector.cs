using FoldEngine.Gui;
using FoldEngine.Resources;

namespace FoldEngine.Editor.Inspector.CustomInspectors;

[CustomInspector(typeof(Resource))]
public class ResourceInspector : CustomInspector<Resource>
{
    protected override void RenderInspectorBefore(Resource obj, GuiPanel panel)
    {
        panel.Element<GuiLabel>().Text("Identifier: " + obj.Identifier).FontSize(9).TextAlignment(-1);
    }
}