using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Resources;
using FoldEngine.Serialization;

namespace FoldEngine.Scenes;

[Component("fold:sub_scene")]
public struct SubScene
{
    public ResourceIdentifier SceneIdentifier;
    [DoNotSerialize] [HideInInspector] public ResourceIdentifier LoadedSceneIdentifier;
    [DoNotSerialize] [HideInInspector] public Scene Scene;
}