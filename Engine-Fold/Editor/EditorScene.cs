using FoldEngine.Interfaces;
using FoldEngine.Scenes;

namespace FoldEngine.Editor;

public class EditorScene : Scene
{
    public EditorScene(IGameCore core) : base(core, "fold:editor")
    {
        Initialize();
    }

    public void Initialize()
    {
        Systems.Add<EditorBase>();
        Systems.Add<EditorCameraSyncSystem>();
        Systems.Add<SubSceneSystem>();
    }
}