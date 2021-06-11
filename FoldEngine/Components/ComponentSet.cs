using FoldEngine.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FoldEngine.Scenes;

namespace FoldEngine.Components {
    public abstract class ComponentSet {
        internal abstract void Flush();
        public abstract bool Has(int entityId);
        public abstract void Remove(int entityId);
    }


    public class ComponentSet<T> : ComponentSet where T : struct {
        private const int StartingDenseSize = 16;

        internal readonly Scene Scene;

        internal ComponentSetEntry<T>[] Dense; //resized on flush
        internal int N; //dense count

        internal int[]
            Sparse; //resized immediately, holds indices to the dense array. 0.. means present at that index, -1 means not present.

        internal int MinId; //sparse, inclusive
        internal int MaxId; //sparse, exclusive

        internal BackupComponentSet BackupSet;
        internal int CurrentTimestamp;

        private readonly List<int> _dsMarkedForRemoval = new List<int>();

        public ComponentSet(Scene scene, int startingId) {
            this.Scene = scene;
            MinId = MathUtil.MaximumSetBit(startingId);
            MaxId = MathUtil.NearestPowerOfTwo(startingId - MinId) + MinId;

            Dense = new ComponentSetEntry<T>[StartingDenseSize];
            Sparse = new int[MaxId - MinId];
            for(int i = 0; i < Sparse.Length; i++) {
                Sparse[i] = -1;
            }

            BackupSet = new BackupComponentSet();
        }

        public ref T Get(int entityId) {
            if(entityId < MinId || entityId >= MaxId || Sparse[entityId - MinId] == -1)
                return ref BackupSet.Get(entityId);

            if(Dense[Sparse[entityId - MinId]].ModifiedTimestamp <= CurrentTimestamp) {
                return ref Dense[Sparse[entityId - MinId]].Component;
            } else {
                //This component is in the process of being removed, so ignore it ()
                throw new ComponentRegistryException($"Component {typeof(T)} not found for entity ID {entityId}");
            }
        }

        public override bool Has(int entityId) {
            if(entityId < MinId || entityId >= MaxId || Sparse[entityId - MinId] == -1)
                return BackupSet.Has(entityId);

            return Dense[Sparse[entityId - MinId]].ModifiedTimestamp <= CurrentTimestamp;
        }

        public ref T Create(int entityId) {
            if(entityId < MinId || entityId >= MaxId) {
                Console.WriteLine("Resizing ComponentSet to fit new entity ID");
                //Resize sparse array to fit new entity ID
                int newMinId = Math.Min(MinId, entityId);
                int newMaxId = Math.Max(MaxId, entityId + 1);

                newMinId = MathUtil.MaximumSetBit(newMinId);
                newMaxId = MathUtil.NearestPowerOfTwo(newMaxId - newMinId) + newMinId;

                int oldSize = Sparse.Length;
                int newSize = (int) (newMaxId - newMinId);

                Array.Resize(ref Sparse, newSize);

                int shiftElementsBy = (int) (MinId - newMinId);
                if(shiftElementsBy > 0) {
                    Console.WriteLine(
                        $"Decreasing minimum ID of ComponentSet from {MinId} to {newMinId}; Shifting sparse array elements by {shiftElementsBy}");
                    for(int i = (int) (MaxId - MinId - 1); i >= 0; i--) {
                        Sparse[i + shiftElementsBy] = Sparse[i];
                        Sparse[i] = -1;
                    }
                }

                for(int i = oldSize + shiftElementsBy; i < newSize; i++) {
                    Sparse[i] = -1;
                }

                MinId = newMinId;
                MaxId = newMaxId;
            }

            int sparseIndex = entityId - MinId;
            if(Sparse[sparseIndex] >= 0) //Is in the dense array
            {
                if(Dense[sparseIndex].ModifiedTimestamp > CurrentTimestamp) //Marked for removal, reclaim it
                {
                    Dense[sparseIndex].ModifiedTimestamp -= 2; //Subtract 2 because:
                    //if ModifiedTimestamp == CurrentTimestamp + 1: it was added prior to this tick and thus should be enumerated
                    //if ModifiedTimestamp == CurrentTimestamp + 2: it was added THIS tick and thus should NOT be enumerated
                    _dsMarkedForRemoval.Remove(entityId);
                    Dense[sparseIndex].Component = default; //Reset component data
                    Component.InitializeComponent(ref Dense[sparseIndex].Component, Scene, entityId);
                    return ref Dense[sparseIndex].Component;
                } else {
                    throw new ComponentRegistryException(
                        $"Entity ID {entityId} already has a component of type {typeof(T)}");
                }
            }

            if(N < Dense.Length) {
                //Have space in dense
                Sparse[sparseIndex] = N;
                N++;
                Dense[N - 1].ModifiedTimestamp = CurrentTimestamp;
                Dense[N - 1].EntityId = entityId;
                Dense[N - 1].Component = default;
                Component.InitializeComponent(ref Dense[N - 1].Component, Scene, entityId);
                return ref Dense[N - 1].Component;
            } else {
                //Have no space in dense. Use the backup set
                return ref BackupSet.Create(entityId);
            }
        }

        public override void Remove(int entityId) {
            if(entityId >= MinId && entityId < MaxId && Sparse[entityId - MinId] != -1) {
                _dsMarkedForRemoval.Add(entityId);
                int timestampOffset = 1;
                if(Dense[Sparse[entityId - MinId]].ModifiedTimestamp == CurrentTimestamp) {
                    //This component was added THIS tick and is being removed the same tick.
                    //In this case, set its "modified timestamp" to 2 ticks from now instead of 1.
                    //This has a special meaning when the component wants to be re-added.
                    timestampOffset = 2;
                }

                Dense[Sparse[entityId - MinId]].ModifiedTimestamp =
                    CurrentTimestamp + timestampOffset; //Mark as "removed" so Get<>() will skip it but iterators won't.

                //Sparse will remain pointing to a location in dense for purposes of iteration
                //Will be removed on flush
            } else {
                BackupSet.Remove(entityId);
            }
        }

        /// <summary>
        /// Flushes the modifications (additions, removals) to the component set made since the previous flush.
        /// This readjusts the components in memory as needed, and allows new iterators to have access to the new component list.
        /// </summary>
        internal override void Flush() {
            foreach(int entityId in _dsMarkedForRemoval) {
                int denseIndex = Sparse[entityId - MinId];
                FoldUtil.Assert(denseIndex >= 0, "Sparse array entry is not cleared until flushing");

                int lastEntityId = Dense[N - 1].EntityId; //get entity ID of the last component in the dense array

                Dense[denseIndex] = Dense[N - 1]; //Move last dense entry to the index being removed
                Sparse[lastEntityId - MinId] = denseIndex; //Make the last entity's sparse entry point to the new index

                Dense[N - 1] = default; //Clear the last component in the dense array
                N--;

                Sparse[entityId - MinId] = -1; //Clear the entity off the sparse array
            }

            _dsMarkedForRemoval.Clear();


            if(N + BackupSet.QueuedComponents.Count > Dense.Length) {
                //Resize the dense array to make space for the components being added
                int newDenseSize = MathUtil.NearestPowerOfTwo(N + BackupSet.QueuedComponents.Count);

                Array.Resize(ref Dense, newDenseSize);
            }

            foreach(QueuedComponent<T> queued in BackupSet.QueuedComponents) {
                Dense[N].Component = queued.Component;
                Dense[N].EntityId = queued.EntityId;
                Dense[N].ModifiedTimestamp = CurrentTimestamp;
                Sparse[queued.EntityId - MinId] = N;
                N++;
            }

            BackupSet.Clear();


            CurrentTimestamp++;
            if(CurrentTimestamp < 0) {
                //oops it's been over 2 years since the game started running what the heck (assuming the game runs at 60 updates per second)
            }
        }

        public void DebugPrint() {
            Console.WriteLine("Component set for type " + typeof(T) + ":");
            Console.WriteLine("dense: " + string.Join(",", Dense.Select(d => d.ToString(CurrentTimestamp))));
            Console.WriteLine("sparse: " + string.Join(",", Sparse.Select(a => a == -1 ? "_" : a.ToString())));
            Console.WriteLine($"minId: {MinId}");
            Console.WriteLine($"maxId: {MaxId}");
            Console.WriteLine();
        }

        internal class BackupComponentSet {
            internal readonly List<QueuedComponent<T>> QueuedComponents = new List<QueuedComponent<T>>();

            internal ref T Get(int entityId) {
                if(QueuedComponents.Count > 0) {
                    int foundIndex = FindIndexForEntityId(entityId, QueuedComponents, w => w.EntityId);
                    if(foundIndex < QueuedComponents.Count && QueuedComponents[foundIndex].EntityId == entityId) {
                        return ref QueuedComponents[foundIndex].Component;
                    }
                }

                throw new ComponentRegistryException($"Component {typeof(T)} not found for entity ID {entityId}");
            }

            internal ref T Create(int entityId) {
                int insertionIndex = FindIndexForEntityId(entityId, QueuedComponents, w => w.EntityId);
                if(insertionIndex < QueuedComponents.Count && QueuedComponents[insertionIndex].EntityId == entityId) {
                    throw new ComponentRegistryException(
                        $"Entity ID {entityId} already has a component of type {typeof(T)}");
                }

                var newWrapper = new QueuedComponent<T> {
                    EntityId = entityId
                };
                QueuedComponents.Insert(insertionIndex, newWrapper);
                return ref newWrapper.Component;
            }

            internal void Remove(int entityId) {
                int foundIndex = FindIndexForEntityId(entityId, QueuedComponents, w => w.EntityId);
                if(foundIndex < QueuedComponents.Count && QueuedComponents[foundIndex].EntityId == entityId) {
                    QueuedComponents.RemoveAt(foundIndex);
                }
            }

            internal void Clear() {
                QueuedComponents.Clear();
            }

            public bool Has(int entityId) {
                int foundIndex = FindIndexForEntityId(entityId, QueuedComponents, w => w.EntityId);
                return foundIndex < QueuedComponents.Count && QueuedComponents[foundIndex].EntityId == entityId;
            }
        }


        public delegate int EntityIdentifierFunction<TK>(TK t);

        /// <summary>
        /// Searches a sorted list for the index at which the specified entity ID beints or exists,
        /// for the purposes of adding components to sorted lists or searching components in sorted lists.
        /// 
        /// O(lg n) operation where n is the length of the given list.<br></br>
        /// </summary>
        /// <param name="entityId">The entity ID to search a location for</param>
        /// <param name="list">The list of objects (components, IDs) to search in</param>
        /// <param name="identifierFunction">A function that turns the an object of type T into an entity ID</param>
        /// <returns>An index between 0 and list.Count (both inclusive), reflecting one of two things:<br></br>
        /// 1. The index within the list on which a component beinting to the given entity ID is located (if it exists)<br></br>
        /// 2. The index within the list where a component beinting to the given entity ID should be located were it to be inserted into the list (if it doesn't already exist).<br></br>
        /// It's important to check the EntityId of the element at the index returned to determine whether the entity already has a component there or if it doesn't</returns>
        public static int FindIndexForEntityId<TK>(
            int entityId,
            List<TK> list,
            EntityIdentifierFunction<TK> identifierFunction) {
            if(list.Count == 0) return 0;

            int minIndex = 0; // inclusive
            int maxIndex = list.Count; // exclusive

            if(entityId < identifierFunction(list[minIndex])) {
                return minIndex;
            }

            if(entityId > identifierFunction(list[maxIndex - 1])) {
                return maxIndex;
            }

            while(minIndex < maxIndex) {
                int pivotIndex = (minIndex + maxIndex) / 2;

                int pivotId = identifierFunction(list[pivotIndex]);
                if(pivotId == entityId) {
                    return pivotIndex;
                } else if(entityId > pivotId) {
                    minIndex = pivotIndex + 1;
                } else {
                    maxIndex = pivotIndex;
                }
            }

            return minIndex;
        }
    }

    internal class QueuedComponent<T> {
        public T Component;
        public int EntityId;
    }

    public struct ComponentSetEntry<T> where T : struct {
        internal int ModifiedTimestamp;
        public int EntityId;

        public T Component;

        public override string ToString() {
            return $"{EntityId}:[{Component}]";
        }

        internal string ToString(int currentTimestamp) {
            return $"{EntityId}:{ModifiedTimestamp - currentTimestamp}[{Component}]";
        }
    }

    public class ComponentRegistryException : Exception {
        public ComponentRegistryException(string message) : base(message) { }
    }
}