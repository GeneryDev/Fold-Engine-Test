using FoldEngine.Scenes;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoldEngine.Components
{
    public abstract class ComponentIterator
    {
        /// <summary>
        /// Resets this component iterator so it can be used again, with the components that are active this loop.
        /// </summary>
        public abstract void Reset();
        /// <summary>
        /// Returns true if there is at least one more component after the currently selected component.
        /// That is, if a call to Next() would return true, but without advancing the iterator.
        /// </summary>
        /// <returns>true if there are more components after the current one, false otherwise.</returns>
        //public abstract bool HasNext();
        /// <summary>
        /// Advances this iterator, and returns true if a component is selected as a result.
        /// If it returns false, it means it has reached the end of the component list.
        /// </summary>
        /// <returns>true if this iterator hasn't reached the end, after advancing once.</returns>
        public abstract bool Next();
        /// <summary>
        /// Fetches the entity ID of the currently selected component.
        /// Throws an exception if Next() hasn't been called since last reset, or if there are no more components to list.
        /// </summary>
        /// <returns>The entity ID of the currently selected component.</returns>
        public abstract long GetEntityId();

        /// <summary>
        /// Whether Next() has been called since having been last reset.
        /// </summary>
        public abstract bool Started { get; }
        /// <summary>
        /// Whether Next() has been called and returned false, meaning the last component was reached the previous iteration.
        /// </summary>
        public abstract bool Finished { get; }

        private static readonly Dictionary<Type, ConstructorInfo> Constructors = new Dictionary<Type, ConstructorInfo>();

        internal static ConstructorInfo GetConstructorForType(Type type)
        {
            if (Constructors.ContainsKey(type)) return Constructors[type];
            ConstructorInfo constructor = typeof(ComponentIterator<>).MakeGenericType(type).GetConstructor(new Type[] { typeof(Scene), typeof(IterationFlags) });
            Constructors[type] = constructor;
            return constructor;
        }

        /// <summary>
        /// Creates a component iterator for the specified component type.
        /// </summary>
        /// <param name="type">The ValueType of the component for which this iterator will be created</param>
        /// <param name="scene">The scene in which the iterator should operate</param>
        /// <param name="flags">The flags to use for this iterator</param>
        /// <returns>A new ComponentIterator that iterates through <code>type</code> components in <code>scene</code>, with <code>flags</code></returns>
        internal static ComponentIterator CreateForType(Type type, Scene scene, IterationFlags flags)
        {
            return (ComponentIterator)GetConstructorForType(type).Invoke(new object[] { scene, flags });
        }
    }

    /// <summary>
    /// An implementation of ComponentIterator for iterating through single components of a given type T
    /// </summary>
    /// <typeparam name="T">The component type this iterator will search for</typeparam>
    public class ComponentIterator<T> : ComponentIterator where T : struct
    {
        /// <summary>
        /// The scene this iterator should operate in.
        /// </summary>
        private readonly Scene Scene;
        /// <summary>
        /// The ComponentSet of the Scene that contains the components of type T. May be null if there are no components of that type in the scene.
        /// </summary>
        private ComponentSet<T> Set; //set may be null
        /// <summary>
        /// The flags for this iterator.
        /// </summary>
        private readonly IterationFlags Flags;
        /// <summary>
        /// The game timestamp at which this iterator started iterating through the scene's components
        /// </summary>
        private int IterationTimestamp;


        public ComponentIterator(Scene scene, IterationFlags flags)
        {
            Scene = scene;
            Flags = flags;
            Reset();
        }

        /// <summary>
        /// The index within the set's sparse array that this iterator is currently located in.
        /// May not be set if this iterator is unordered.
        /// </summary>
        private int sparseIndex = -1;
        /// <summary>
        /// The index within the set's dense array that this iterator is currently located in.
        /// </summary>
        private int denseIndex = -1;

        private bool _started;
        public override bool Started => _started;

        private bool _finished;
        public override bool Finished => _finished;

        public override void Reset()
        {
            sparseIndex = -1;
            denseIndex = -1;
            if(Set == null && Scene.Components.Map.ContainsKey(typeof(T)))
            {
                Set = (ComponentSet<T>)Scene.Components.Map[typeof(T)];
            }
            if (Set != null) IterationTimestamp = Set.CurrentTimestamp;
            _started = false;
            _finished = false;
        }

        public override bool Next()
        {
            _started = true;
            if (Set == null)
            {
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
                    _finished = false;
                }
                else
                {
                    denseIndex = Set.dense.Length;
                    _finished = true;
                }
            }
            else
            {
                denseIndex++;
                _finished = denseIndex >= Set.dense.Length;
            }

            return !_finished;
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
