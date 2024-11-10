using System;
using FoldEngine.Components;
using FoldEngine.Interfaces;
using FoldEngine.Rendering;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Util;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor;

public class EditorScene : Scene
{
    public EditorScene(IGameCore core) : base(core, "fold:editor")
    {
        Initialize();
    }

    public void Initialize()
    {
        BuildMeshes();
        
        Systems.Add<EditorBase>();
        Systems.Add<SubSceneSystem>();

        var editorCameraEntity = CreateEntity("Editor Camera");
        editorCameraEntity.AddComponent<Camera>();
    }

    private void BuildMeshes()
    {
        Resources.Create<Mesh>("square")
            .Start(Mesh.MeshInputType.Vertices)
            .Vertex(new Vector2(-0.5f, -0.5f), new Vector2(0, 1))
            .Vertex(new Vector2(-0.5f, 0.5f), new Vector2(0, 0))
            .Vertex(new Vector2(0.5f, 0.5f), new Vector2(1, 0))
            .Vertex(new Vector2(0.5f, -0.5f), new Vector2(1, 1))
            .End();

        Mesh circle = Resources.Create<Mesh>("circle").Start(Mesh.MeshInputType.Vertices);
        const int segments = 12;
        Complex step = Complex.FromRotation(-(float)(Math.PI * 2 / segments));
        var current = new Complex(0.5f, 0);
        for (int i = 0; i < segments; i++)
        {
            circle.Vertex(current, current + new Complex(0.5f, 0.5f));
            current *= step;
        }

        circle.End();
    }
}