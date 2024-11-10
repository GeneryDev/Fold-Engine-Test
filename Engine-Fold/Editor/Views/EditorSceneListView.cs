using System;
using FoldEngine.Commands;
using FoldEngine.Editor.ImmediateGui;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Views;

public class EditorSceneListView : EditorView
{
    public EditorSceneListView()
    {
        Icon = new ResourceIdentifier("editor/menu");
    }

    public override string Name => "Scene List";

    public override void Render(IRenderingUnit renderer)
    {
        ContentPanel.MayScroll = true;

        var editorBase = (ContentPanel.Environment as EditorEnvironment)?.EditorBase;
        var tabIterator = editorBase?.TabIterator;
        if (tabIterator == null) return;
        
        tabIterator.Reset();
        long? changeTab = null;
        while (tabIterator.Next())
        {
            string sceneName = tabIterator.GetCoComponent<SubScene>().Scene.Name;
            if (ContentPanel.Button(sceneName, 14).IsPressed())
            {
                changeTab = tabIterator.GetEntityId();
            }
        }
        if(changeTab.HasValue)
            editorBase.SelectTab(changeTab.Value);
    }
}