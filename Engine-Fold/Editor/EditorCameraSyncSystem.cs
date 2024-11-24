using FoldEngine.Components;
using FoldEngine.Rendering;
using FoldEngine.Scenes;
using FoldEngine.Systems;

namespace FoldEngine.Editor;

[GameSystem("fold:editor.camera_sync", ProcessingCycles.Update, true)]
public class EditorCameraSyncSystem : GameSystem
{
    private ComponentIterator<Camera> _cameras;
    
    public override void Initialize()
    {
        _cameras = CreateComponentIterator<Camera>(IterationFlags.IncludeInactive);
    }

    public override void OnUpdate()
    {
        _cameras.Reset();

        while (_cameras.Next())
        {
            ref var camera = ref _cameras.GetComponent();
            ref var transform = ref _cameras.GetCoComponent<Transform>();
            ref var hierarchical = ref _cameras.GetCoComponent<Hierarchical>();
            if (!hierarchical.HasParent) continue;

            var parentEntity = new Entity(Scene, hierarchical.ParentId);
            if (parentEntity.HasComponent<EditorTab>() && parentEntity.HasComponent<SubScene>())
            {
                ref var tab = ref parentEntity.GetComponent<EditorTab>();
                var subScene = parentEntity.GetComponent<SubScene>().Scene;
                if (subScene == null) continue;
                if (tab is { PreviewSceneCamera: false, Playing: false })
                {
                    subScene.CameraOverrides ??= new CameraOverrides(subScene);
                    subScene.CameraOverrides.Transform.RestoreSnapshot(transform);

                    subScene.CameraOverrides.Camera.SnapPosition = camera.SnapPosition;
                    subScene.CameraOverrides.Camera.RenderToLayer = camera.RenderToLayer;
                }
                else
                {
                    subScene.CameraOverrides = null;
                }
            }
        }
    }
}