using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Resources;
using FoldEngine.Serialization;

namespace FoldEngine.Scenes;

[Component("fold:sub_scene")]
public struct SubScene
{
    [ResourceIdentifier(typeof(PackedScene))] public ResourceIdentifier SceneIdentifier;
    [DoNotSerialize] [HideInInspector] public ResourceIdentifier LoadedSceneIdentifier;
    [DoNotSerialize] [HideInInspector] public Scene Scene;

    public bool Render;
    public bool Update;
    public bool ProcessInputs;
}