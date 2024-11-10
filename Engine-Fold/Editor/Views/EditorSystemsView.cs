using System;
using EntryProject.Editor.Gui.Hierarchy;
using FoldEngine.Editor.Gui;
using FoldEngine.Editor.Gui.Hierarchy;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Systems;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Views;

public class EditorSystemsView : EditorView
{
    public EditorSystemsView()
    {
        Icon = new ResourceIdentifier("editor/cog");
    }

    public override string Name => "Systems";
    public SystemHierarchy Hierarchy;

    public override void Render(IRenderingUnit renderer)
    {
        if (Hierarchy == null) Hierarchy = new SystemHierarchy(ContentPanel);
        ContentPanel.MayScroll = true;

        if (ContentPanel.Button("Add System", 14).IsPressed(out Point p))
        {
            GuiPopupMenu contextMenu = ContentPanel.Environment.ContextMenu;
            contextMenu.Show(p, m =>
            {
                foreach (Type type in Core.RegistryUnit.Systems.GetAllTypes())
                    if (EditingScene.Systems.Get(type) == null && m.Button(type.Name, 9).IsPressed())
                        ((EditorEnvironment)ContentPanel.Environment).TransactionManager.InsertTransaction(
                            new AddSystemTransaction(type));
            });
        }

        ContentPanel.Separator();

        var editorEnvironment = (EditorEnvironment)ContentPanel.Environment;

        foreach (GameSystem sys in EditingScene.Systems.AllSystems)
        {
            HierarchyElement<Type> element = (HierarchyElement<Type>)ContentPanel.Element<HierarchyElement<Type>>()
                    .Hierarchy(Hierarchy)
                    .Id(sys.GetType())
                    .Selected(editorEnvironment.GetView<EditorInspectorView>().GetObject() == sys)
                    .Icon(EditorResources.Get<Texture>(ref EditorIcons.Cog, null))
                    .Text(sys.SystemName)
                ;

            switch (element.GetEvent(out Point clickPoint))
            {
                case HierarchyElement<Type>.HierarchyEventType.Down:
                {
                    editorEnvironment.Scene.Systems.Get<EditorBase>().EditingEntity.Clear();
                    editorEnvironment.GetView<EditorInspectorView>().SetObject(sys);
                    editorEnvironment.SwitchToView<EditorInspectorView>();

                    Hierarchy.Selected.Clear();
                    Hierarchy.Selected.Add(sys.GetType());
                    break;
                }
                case HierarchyElement<Type>.HierarchyEventType.Context:
                {
                    GuiPopupMenu contextMenu = ContentPanel.Environment.ContextMenu;
                    contextMenu.Show(clickPoint, m =>
                    {
                        if (m.Button("Remove", 14).IsPressed())
                            ((EditorEnvironment)ContentPanel.Environment).TransactionManager.InsertTransaction(
                                new RemoveSystemTransaction(sys.GetType()));
                    });
                    break;
                }
            }
        }
    }
}

public class SystemHierarchy : Hierarchy<Type>
{
    public SystemHierarchy(GuiEnvironment environment) : base(environment)
    {
    }

    public SystemHierarchy(GuiPanel parent) : base(parent)
    {
    }

    public override Type DefaultId { get; } = null;
    public override bool CanDragInto { get; set; } = false;

    public override void Drop()
    {
        if (DragTargetId == null) return;
        var editingScene = ((EditorEnvironment)Environment).EditingScene;
        if (editingScene == null) return;

        var transactions = new CompoundTransaction<Scene>();

        foreach (Type sysType in Selected)
        {
            int fromIndex = editingScene.Systems.GetSystemIndex(sysType);

            int toIndex = editingScene.Systems.GetSystemIndex(DragTargetId);
            if (DragRelative == 1) toIndex++;

            if (toIndex == fromIndex) continue;

            if (toIndex > fromIndex) toIndex--;

            var transaction = new ChangeSystemOrderTransaction(sysType, fromIndex, toIndex);
            transactions.Append(() => transaction);
        }

        if (transactions.Count > 0 && Environment is EditorEnvironment editorEnvironment)
            editorEnvironment.TransactionManager.InsertTransaction(transactions);
    }
}