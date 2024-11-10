using FoldEngine.Components;
using FoldEngine.Rendering;
using FoldEngine.Scenes;

namespace FoldEngine.Editor;

public class CameraOverrides
{
    // TODO Repurpose this class as "camera overrides", a field in Scene that will be used as the camera if set.
    // All editor related usages of these can simply use plain components in an Editor scene. 
    public Camera Camera;
    public Transform Transform;

    public CameraOverrides(Scene scene)
    {
        Transform = Transform.InitializeComponent(scene, -1);
        Camera = new Camera();
    }
}