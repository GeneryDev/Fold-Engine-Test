using FoldEngine.Scenes;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoldEngine.Components
{
    public abstract class ComponentIterator
    {
        public abstract void Reset();
        public abstract long GetEntityId();
        public abstract bool HasNext();
        public abstract bool Next();

        public abstract bool Started { get; }
        public abstract bool Finished { get; }


        private static readonly Dictionary<Type, ConstructorInfo> Constructors = new Dictionary<Type, ConstructorInfo>();

        internal static ConstructorInfo GetConstructorForType(Type type)
        {
            if (Constructors.ContainsKey(type)) return Constructors[type];
            ConstructorInfo constructor = typeof(ComponentIterator<>).MakeGenericType(type).GetConstructor(new Type[] { typeof(Scene), typeof(IterationFlags) });
            Constructors[type] = constructor;
            return constructor;
        }

        internal static ComponentIterator CreateForType(Type type, Scene scene, IterationFlags flags)
        {
            return (ComponentIterator)GetConstructorForType(type).Invoke(new object[] { scene, flags });
        }
    }
    public class ComponentIterator<T> : ComponentIterator where T : struct
    {
        private readonly Scene Scene;
        private ComponentSet<T> Set; //set may be null
        private readonly IterationFlags Flags;
        private int IterationTimestamp;



        public ComponentIterator(Scene scene, IterationFlags flags)
        {
            Scene = scene;
            Flags = flags;
            Reset();
        }

        private int sparseIndex = -1;
        private int denseIndex = -1;
        //private bool Finished => Set == null || denseIndex >= Set.dense.Length;
        private bool _started;
        private bool _finished;

        public override bool Started => _started;

        public override bool Finished => _finished;

        public override void Reset()
        {
            sparseIndex = -1;
            denseIndex = -1;
            if(Set != null) IterationTimestamp = Set.CurrentTimestamp;
            if(Set == null && Scene.Components.Map.ContainsKey(typeof(T)))
            {
                Set = (ComponentSet<T>)Scene.Components.Map[typeof(T)];
            }
            _started = false;
            _finished = false;
        }

        public override bool Next()
        {
            if (Set == null)
            {
                _started = true;
                _finished = true;
                return false;
            }
            if (Flags.HasFlag(IterationFlags.Ordered))
            {
                do
                {
                    sparseIndex++;
                }
                while (sparseIndex < Set.sparse.Length && (Set.sparse[sparseIndex] == -1 || Set.dense[Set.sparse[sparseIndex]].ModifiedTimestamp == IterationTimestamp));
                // ModifiedTimestamp < IterationTimestamp: Component was added before this "tick" (or it was removed and recovered this same tick)
                // ModifiedTimestamp == IterationTimestamp: Component was added this very tick (so skip it)
                // ModifiedTimestamp > IterationTimestamp: Component was marked for removal this very tick (but it should still be iterated through)

                if (sparseIndex < Set.sparse.Length)
                {
                    denseIndex = Set.sparse[sparseIndex];
                }
                else
                {
                    denseIndex = Set.dense.Length;
                }
            }
            else
            {
                denseIndex++;
            }

            _started = true;
            _finished = denseIndex >= Set.dense.Length;

            return !_finished;
        }

        public override bool HasNext()
        {
            return Set != null && denseIndex < Set.dense.Length;
        }

        /*public ref ComponentSetEntry<T> GetSignature() //do not expose set entries lol
        {
            if (HasNext())
            {
                ref ComponentSetEntry<T> result = ref Set.dense[denseIndex];
                AdvanceHead();
                return ref result;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }*/

        public override long GetEntityId()
        {
            if(Started && !Finished)
            {
                return Set.dense[denseIndex].EntityId;
            } else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public ref T GetComponent()
        {
            if (Started && !Finished)
            {
                ref T result = ref Set.dense[denseIndex].Component;
                return ref result;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
    }

    [Flags]
    public enum IterationFlags
    {
        Ordered
    }
}
