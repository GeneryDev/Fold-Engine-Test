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
        private ComponentGrouping Grouping = ComponentGrouping.And;
        private Type[] ComponentTypes;
        private ComponentIterator[] Iterators;

        private long CurrentEntityId = -1;

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

            ComponentTypes = componentTypes;

            Iterators = new ComponentIterator[ComponentTypes.Length];
            for (int i = 0; i < Iterators.Length; i++)
            {
                Iterators[i] = ComponentIterator.CreateForType(ComponentTypes[i], scene, IterationFlags.Ordered);
            }
        }

        public override void Reset() {
            foreach(ComponentIterator iterator in Iterators)
            {
                iterator.Reset();
            }
            CurrentEntityId = -1;
            _started = false;
            _finished = false;
        }

        public override bool HasNext()
        {
            throw new InvalidOperationException();
        }

        public override bool Next()
        {
            _started = true;
            if (Grouping == ComponentGrouping.And)
            {
                //all iterators must be on the same ID

                bool mismatchingIDs = true;
                long highestId = -1;

                while(mismatchingIDs)
                {
                    mismatchingIDs = false;
                    foreach(ComponentIterator iterator in Iterators)
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
                CurrentEntityId = highestId;
                return true;
            } else //OR
            {
                long lowestId = long.MaxValue;
                foreach (ComponentIterator iterator in Iterators)
                {
                    bool active = true;
                    if(!iterator.Started || iterator.GetEntityId() <= CurrentEntityId)
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
                CurrentEntityId = lowestId;
                return true;
            }
        }

        public override long GetEntityId()
        {
            return CurrentEntityId;
        }

        public bool Has<T>() where T : struct
        {
            if (!Started || Finished) return false;
            Type type = typeof(T);
            for (int i = 0; i < ComponentTypes.Length; i++)
            {
                if (ComponentTypes[i] == type)
                {
                    return Iterators[i] != null && Iterators[i].GetEntityId() == CurrentEntityId;
                }
            }
            throw new ArgumentException("This MultiComponentIterator does not track components of type " + type);
        }

        public ref T Get<T>() where T : struct
        {
            if (!Started || Finished) throw new InvalidOperationException("This iterator has not been started or it has been exhausted");
            Type type = typeof(T);
            for (int i = 0; i < ComponentTypes.Length; i++)
            {
                if (ComponentTypes[i] == type)
                {
                    if (Iterators[i] != null && Iterators[i].GetEntityId() == CurrentEntityId)
                    {
                        return ref ((ComponentIterator<T>)Iterators[i]).GetComponent();
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
            Grouping = grouping;
            return this;
        }
    }

    public enum ComponentGrouping
    {
        And,
        Or
    }
}
