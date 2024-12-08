using System;
using FoldEngine.Editor.Components;
using FoldEngine.Editor.Events;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Editor.ImmediateGui.Hierarchy;
using FoldEngine.Editor.Transactions;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
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
        new ResourceIdentifier("editor/cog");
    }

    public virtual string Name => "Systems";
    public SystemHierarchy Hierarchy;

    private GameSystem _selectedSystem;

    public override void Render(IRenderingUnit renderer)
    {
        var editorBase = Scene.Systems.Get<EditorBase>();
        var editingTab = editorBase.CurrentSceneTab;
        if (editingTab.Scene == null) return;
        var editingScene = editingTab.Scene;
        
        if (Hierarchy == null) Hierarchy = new SystemHierarchy(ContentPanel);
        ContentPanel.MayScroll = true;

        if (ContentPanel.Button("Add System", 14).IsPressed(out Point p))
        {
            var contextMenu = Scene.Systems.Get<EditorContextMenuSystem>();
            contextMenu.Show(p, m =>
            {
                foreach (Type type in Core.RegistryUnit.Systems.GetAllTypes())
                    if (editingScene.Systems.Get(type) == null)
                    {
                        m.Button(type.Name, "editor/add").AddComponent<TransactionActionComponent>() =
                            new TransactionActionComponent(new AddSystemTransaction(type));
                    }
            });
        }

        ContentPanel.Separator();

        foreach (GameSystem sys in editingScene.Systems.AllSystems)
        {
            HierarchyElement<Type> element = (HierarchyElement<Type>)ContentPanel.Element<HierarchyElement<Type>>()
                    .Hierarchy(Hierarchy)
                    .Id(sys.GetType())
                    .Selected(_selectedSystem == sys)
                    .Icon(EditorResources.Get<Texture>(ref EditorIcons.Cog, null))
                    .Text(sys.SystemName)
                ;

            switch (element.GetEvent(out Point clickPoint))
            {
                case HierarchyElement<Type>.HierarchyEventType.Down:
                {
                    editingTab.EditingEntity?.Clear();
                    _selectedSystem = sys;
                    Scene.Events.Invoke(new ObjectInspectorRequestedEvent()
                    {
                        Object = sys
                    });

                    Hierarchy.Selected.Clear();
                    Hierarchy.Selected.Add(sys.GetType());
                    break;
                }
                case HierarchyElement<Type>.HierarchyEventType.Context:
                {
                    var contextMenu = Scene.Systems.Get<EditorContextMenuSystem>();
                    contextMenu.Show(clickPoint, m =>
                    {
                        m.Button("Remove").AddComponent<TransactionActionComponent>() =
                            new TransactionActionComponent(new RemoveSystemTransaction(sys.GetType()));
                    });
                    break;
                }
            }
        }
        Hierarchy.DrawDragLine(renderer, renderer.RootGroup["editor_gui_overlay"]);
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
        var editingScene = Environment.Scene.Systems.Get<EditorBase>().CurrentSceneTab.Scene;
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

        if (transactions.Count > 0)
            Environment.Scene.Systems.Get<EditorBase>().CurrentSceneTab.SceneTransactions.InsertTransaction(transactions);
    }
}