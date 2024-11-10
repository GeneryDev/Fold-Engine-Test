using FoldEngine.Components;
using FoldEngine.Editor.Gui;
using FoldEngine.Scenes;
using FoldEngine.Util.Transactions;

namespace FoldEngine.Editor.Transactions;

public class SetEntityTransformTransaction : Transaction<Scene>
{
    private Transform _after;
    private readonly Transform _before;
    private readonly long _entityId;

    public SetEntityTransformTransaction(Transform before)
    {
        _before = _after = before;
        _entityId = before.EntityId;
    }

    public void UpdateAfter(Transform after)
    {
        _after = after;
    }

    public override bool Redo(Scene target)
    {
        ref Transform targetTransform = ref target.Components.GetComponent<Transform>(_entityId);

        bool any = false;

        if (_before.LocalPosition != _after.LocalPosition)
        {
            targetTransform.Position = _after.LocalPosition;
            any = true;
        }

        if (_before.LocalRotation != _after.LocalRotation)
        {
            targetTransform.Rotation = _after.LocalRotation;
            any = true;
        }

        if (_before.LocalScale != _after.LocalScale)
        {
            targetTransform.LocalScale = _after.LocalScale;
            any = true;
        }

        return any;
    }

    public override bool Undo(Scene target)
    {
        ref Transform targetTransform = ref target.Components.GetComponent<Transform>(_entityId);

        bool any = false;

        if (_before.LocalPosition != _after.LocalPosition)
        {
            targetTransform.Position = _before.LocalPosition;
            any = true;
        }

        if (_before.LocalRotation != _after.LocalRotation)
        {
            targetTransform.Rotation = _before.LocalRotation;
            any = true;
        }

        if (_before.LocalScale != _after.LocalScale)
        {
            targetTransform.LocalScale = _before.LocalScale;
            any = true;
        }

        return any;
    }

    public override bool RedoOnInsert(Scene target)
    {
        return true;
    }
}