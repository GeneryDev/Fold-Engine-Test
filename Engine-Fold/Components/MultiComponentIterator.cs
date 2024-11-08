﻿using System;
using FoldEngine.Scenes;

namespace FoldEngine.Components;

public class MultiComponentIterator : ComponentIterator
{
    private readonly Type[] _componentTypes;

    private long _currentEntityId = -1;
    private bool _finished;
    private ComponentGrouping _grouping = ComponentGrouping.And;
    private readonly ComponentIterator[] _iterators;
    private readonly Scene _scene;

    private bool _started;

    public MultiComponentIterator(Scene scene, params Type[] componentTypes)
    {
        _scene = scene;
        foreach (Type type in componentTypes)
            if (!type.IsValueType || type.IsPrimitive)
                throw new ArgumentException("Types must be structs: " + type, nameof(componentTypes));

        _componentTypes = componentTypes;

        _iterators = new ComponentIterator[_componentTypes.Length];
        for (int i = 0; i < _iterators.Length; i++)
            _iterators[i] = CreateForType(_componentTypes[i], scene, IterationFlags.Ordered);
    }

    public override bool Started => _started;
    public override bool Finished => _finished;

    public override void Reset()
    {
        foreach (ComponentIterator iterator in _iterators) iterator.Reset();
        _currentEntityId = -1;
        _started = false;
        _finished = false;
    }

    public override bool Next()
    {
        _started = true;
        if (_grouping == ComponentGrouping.And)
        {
            //all iterators must be on the same ID

            bool mismatchingIDs = true;
            long highestId = -1;

            while (mismatchingIDs)
            {
                mismatchingIDs = false;
                foreach (ComponentIterator iterator in _iterators)
                    if (!iterator.Started || iterator.GetEntityId() != highestId)
                    {
                        bool itNext = iterator.Next();
                        if (!itNext)
                        {
                            _finished = true;
                            return false;
                        }

                        highestId = Math.Max(highestId, iterator.GetEntityId());
                        mismatchingIDs = true;
                    }
            }

            _currentEntityId = highestId;
            return true;
        }

        long lowestId = long.MaxValue;
        foreach (ComponentIterator iterator in _iterators)
        {
            bool active = false;
            if (!iterator.Started)
                active = iterator.Next();
            else if (!iterator.Finished && iterator.GetEntityId() <= _currentEntityId)
                active = iterator.Next();
            else if (!iterator.Finished) active = true;

            if (active) lowestId = Math.Min(lowestId, iterator.GetEntityId());
        }

        if (lowestId == long.MaxValue)
        {
            _finished = true;
            return false;
        }

        _currentEntityId = lowestId;
        return true;
    }

    public override long GetEntityId()
    {
        return _currentEntityId;
    }

    public bool Has<T>() where T : struct
    {
        if (!Started || Finished) return false;
        Type type = typeof(T);
        for (int i = 0; i < _componentTypes.Length; i++)
            if (_componentTypes[i] == type)
                return _iterators[i] != null
                       && _iterators[i].Started
                       && !_iterators[i].Finished
                       && _iterators[i].GetEntityId() == _currentEntityId;
        throw new ArgumentException("This MultiComponentIterator does not track components of type " + type);
    }

    public ref T Get<T>() where T : struct
    {
        if (!Started || Finished)
            throw new InvalidOperationException("This iterator has not been started or it has been exhausted");
        Type type = typeof(T);
        for (int i = 0; i < _componentTypes.Length; i++)
            if (_componentTypes[i] == type)
            {
                if (_iterators[i] != null
                    && _iterators[i].Started
                    && !_iterators[i].Finished
                    && _iterators[i].GetEntityId() == _currentEntityId)
                    return ref ((ComponentIterator<T>)_iterators[i]).GetComponent();
                throw new ArgumentException("The current entity does not have a component of type " + type);
            }

        throw new ArgumentException("This MultiComponentIterator does not track components of type " + type);
    }

    public ref TO GetCoComponent<TO>() where TO : struct
    {
        return ref _scene.Components.GetComponent<TO>(GetEntityId());
    }

    public bool HasCoComponent<TO>() where TO : struct
    {
        return _scene.Components.HasComponent<TO>(GetEntityId());
    }

    public MultiComponentIterator SetGrouping(ComponentGrouping grouping)
    {
        _grouping = grouping;
        return this;
    }
}

public enum ComponentGrouping
{
    And,
    Or
}