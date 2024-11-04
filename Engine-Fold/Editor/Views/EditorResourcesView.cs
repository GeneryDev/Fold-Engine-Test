using System;
using EntryProject.Editor.Gui.Hierarchy;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Gui.Fields;
using FoldEngine.Editor.Gui.Hierarchy;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public class EditorResourcesView : EditorView
{
    public EditorResourcesView()
    {
        Icon = new ResourceIdentifier("editor/checkmark");
    }

    public override string Name => "Resources";

    public bool ShowRuntimeResources = true;
    private bool _fieldsInitialized;
    private Hierarchy<Type> _typeHierarchy;
    private Hierarchy<string> _resourceHierarchy;

    private GuiPanel _sidebar;
    private GuiPanel _main;

    private const string RuntimePrefix = "__runtime_";

    public override void Render(IRenderingUnit renderer)
    {
        if (!_fieldsInitialized)
        {
            _fieldsInitialized = true;
            _typeHierarchy = new Hierarchy<Type>(ContentPanel) { CanDrag = false };
            _resourceHierarchy = new Hierarchy<string>(ContentPanel) { CanDrag = false };
            _sidebar = new GuiPanel(ContentPanel.Environment) { MayScroll = true };
            _main = new GuiPanel(ContentPanel.Environment) { MayScroll = true };
        }

        Scene scene = ContentPanel.Environment.Scene;

        ShowRuntimeResources = ContentPanel.Element<Checkbox>().Value(ShowRuntimeResources).IsChecked();
        ContentPanel.Label("Show Runtime", 9).TextAlignment(-1);
        ContentPanel.Element<ComponentMemberBreak>();

        _sidebar.Reset();
        _main.Reset();
        ContentPanel.Element(_sidebar);
        ContentPanel.Element(_main);

        _sidebar.Bounds = new Rectangle(ContentPanel.Bounds.X, ContentPanel.Bounds.Y + 30, 100, 0);
        _sidebar.Bounds.Height = ContentPanel.Bounds.Y + ContentPanel.Bounds.Height - _sidebar.Bounds.Y;

        _main.Bounds = new Rectangle(_sidebar.Bounds.X + _sidebar.Bounds.Width, _sidebar.Bounds.Y, 0,
            _sidebar.Bounds.Height);
        _main.Bounds.Width = ContentPanel.Bounds.X + ContentPanel.Bounds.Width - _main.Bounds.X;

        foreach (Type type in scene.Core.RegistryUnit.Resources.GetAllTypes())
        {
            ResourceAttribute resourceAttribute = scene.Core.RegistryUnit.Resources.AttributeOf(type);

            HierarchyElement<Type> element = (HierarchyElement<Type>)_sidebar.Element<HierarchyElement<Type>>()
                    .Hierarchy(_typeHierarchy)
                    .Selected(_typeHierarchy.IsSelected(type))
                    .Text(resourceAttribute.Identifier)
                    .FontSize(9)
                ;
            switch (element.GetEvent())
            {
                case HierarchyElement<Type>.HierarchyEventType.Down:
                {
                    _typeHierarchy.Selected.Clear();
                    _typeHierarchy.Selected.Add(type);
                    break;
                }
            }
        }


        if (_typeHierarchy.Selected.Count > 0)
        {
            Type selectedType = _typeHierarchy.Selected[0];
            if (ShowRuntimeResources)
            {
                foreach (Resource resource in scene.Resources.GetAll(selectedType))
                {
                    string hierarchyId = RuntimePrefix + resource.Identifier;
                    CreateHierarchyElement(selectedType, resource.Identifier, hierarchyId, resource);
                }
            }

            foreach (string resourceIdentifier in scene.Core.ResourceIndex.GetIdentifiers(
                         selectedType))
            {
                CreateHierarchyElement(selectedType, resourceIdentifier);
            }
        }
    }

    private HierarchyElement<string> CreateHierarchyElement(Type type, string resourceId, string hierarchyId = null,
        Resource resource = null)
    {
        hierarchyId = hierarchyId ?? resourceId;

        var element = (HierarchyElement<string>)_main.Element<HierarchyElement<string>>()
                .Hierarchy(_resourceHierarchy)
                .Selected(_resourceHierarchy.IsSelected(hierarchyId ?? resourceId))
                .Text(resource != null ? "[R] " + resourceId : resourceId)
                .FontSize(9)
            ;

        var loaded = false;

        var identifier = new ResourceIdentifier(resourceId);

        switch (element.GetEvent())
        {
            case HierarchyElement<string>.HierarchyEventType.Down:
            {
                _resourceHierarchy.Selected.Clear();
                _resourceHierarchy.Selected.Add(hierarchyId);

                if (resource != null)
                {
                    // Runtime

                    if (ContentPanel.Environment is EditorEnvironment editorEnvironment)
                    {
                        editorEnvironment.Scene.Systems.Get<EditorBase>().EditingEntity.Clear();
                        editorEnvironment.GetView<EditorInspectorView>()
                            .SetObject(ContentPanel.Environment.Scene.Resources.Get(type, ref identifier));
                        editorEnvironment.SwitchToView<EditorInspectorView>();
                    }

                    loaded = true;
                }
                else
                {
                    // Global

                    if (ContentPanel.Environment.Scene.Core.Resources.Exists(type, ref identifier))
                    {
                        if (ContentPanel.Environment is EditorEnvironment editorEnvironment)
                        {
                            editorEnvironment.Scene.Systems.Get<EditorBase>().EditingEntity.Clear();
                            editorEnvironment.GetView<EditorInspectorView>()
                                .SetObject(ContentPanel.Environment.Scene.Core.Resources.Get(type, ref identifier));
                            editorEnvironment.SwitchToView<EditorInspectorView>();
                        }

                        loaded = true;
                    }
                    else
                    {
                        if (ContentPanel.Environment is EditorEnvironment editorEnvironment)
                        {
                            editorEnvironment.Scene.Systems.Get<EditorBase>().EditingEntity.Clear();
                            editorEnvironment.GetView<EditorInspectorView>().SetObject(new Loading()
                            {
                                Type = type,
                                Identifier = identifier
                            });
                            editorEnvironment.SwitchToView<EditorInspectorView>();
                        }
                    }
                }

                break;
            }
            default:
            {
                loaded = resource != null || ContentPanel.Environment.Scene.Core.Resources.Exists(type, ref identifier);
                break;
            }
        }

        if (!loaded)
        {
            element.TextColor(new Color(128, 128, 128));
        }

        return element;
    }
}

[Name("Loading resource...")]
public class Loading
{
    [HideInInspector] public ResourceIdentifier Identifier;
    [HideInInspector] public Type Type;
}

[CustomInspector(typeof(Loading))]
public class LoadingInspector : CustomInspector<Loading>
{
    protected override void RenderInspectorBefore(Loading obj, GuiPanel panel)
    {
        panel.Element<GuiLabel>().Text($"Type: {obj.Type}").FontSize(9).TextAlignment(-1);
        panel.Element<GuiLabel>().Text($"Identifier: {obj.Identifier.Identifier}").FontSize(9).TextAlignment(-1);

        Resource resource = panel.Environment.Scene.Core.Resources.Get(obj.Type, ref obj.Identifier);
        if (resource != null && panel.Environment is EditorEnvironment editorEnvironment)
        {
            editorEnvironment.GetView<EditorInspectorView>().SetObject(resource);
        }
    }
}