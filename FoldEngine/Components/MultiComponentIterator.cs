using FoldEngine.Scenes;

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoldEngine.Components
{
    public class MultiComponentIterator : ComponentIterator
    {
        private ComponentGrouping _grouping = ComponentGrouping.And;
        private Type[] _componentTypes;
        private ComponentIterator[] _iterators;

        private long _currentEntityId = -1;

        private bool _started = false;
        private bool _finished = false;

        public override bool Started => _started;
        public override bool Finished => _finished;

        public MultiComponentIterator(Scene scene, params Type[] componentTypes)
        {
            foreach(Type type in componentTypes)
            {
                if(!type.IsValueType || type.IsPrimitive)
                {
                    throw new ArgumentException("Types must be structs: " + type, "componentTypes");
                }
            }

            _componentTypes = componentTypes;

            _iterators = new ComponentIterator[_componentTypes.Length];
            for (int i = 0; i < _iterators.Length; i++)
            {
                _iterators[i] = ComponentIterator.CreateForType(_componentTypes[i], scene, IterationFlags.Ordered);
            }
        }

        public override void Reset() {
            foreach(ComponentIterator iterator in _iterators)
            {
                iterator.Reset();
            }
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

                while(mismatchingIDs)
                {
                    mismatchingIDs = false;
                    foreach(ComponentIterator iterator in _iterators)
                    {
                        if(!iterator.Started || iterator.GetEntityId() != highestId)
                        {
                            bool itNext = iterator.Next();
                            if(!itNext)
                            {
                                _finished = true;
                                return false;
                            }
                            highestId = Math.Max(highestId, iterator.GetEntityId());
                            mismatchingIDs = true;
                        }
                    }
                }
                _currentEntityId = highestId;
                return true;
            } else //OR
            {
                long lowestId = long.MaxValue;
                foreach (ComponentIterator iterator in _iterators)
                {
                    bool active = false;
                    if(!iterator.Started || (!iterator.Finished && iterator.GetEntityId() <= _currentEntityId))
                    {
                        active = iterator.Next();
                    }

                    if(active) lowestId = Math.Min(lowestId, iterator.GetEntityId());
                }
                if(lowestId == long.MaxValue)
                {
                    _finished = true;
                    return false;
                }
                _currentEntityId = lowestId;
                return true;
            }
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
            {
                if (_componentTypes[i] == type)
                {
                    return _iterators[i] != null && _iterators[i].Started && !_iterators[i].Finished && _iterators[i].GetEntityId() == _currentEntityId;
                }
            }
            throw new ArgumentException("This MultiComponentIterator does not track components of type " + type);
        }

        public ref T Get<T>() where T : struct
        {
            if (!Started || Finished) throw new InvalidOperationException("This iterator has not been started or it has been exhausted");
            Type type = typeof(T);
            for (int i = 0; i < _componentTypes.Length; i++)
            {
                if (_componentTypes[i] == type)
                {
                    if (_iterators[i] != null && _iterators[i].Started && !_iterators[i].Finished && _iterators[i].GetEntityId() == _currentEntityId)
                    {
                        return ref ((ComponentIterator<T>)_iterators[i]).GetComponent();
                    }
                    else
                    {
                        throw new ArgumentException("The current entity does not have a component of type " + type);
                    }
                }
            }
            throw new ArgumentException("This MultiComponentIterator does not track components of type " + type);
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
}
