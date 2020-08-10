using FoldEngine.Util;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FoldEngine.Components
{
    public abstract class ComponentSet {
        internal abstract void Flush();
    }
    public class ComponentSet<T> : ComponentSet where T : struct
    {
        private const int StartingDenseSize = 16;

        internal ComponentSetEntry<T>[] dense; //resized on flush
        internal int n; //dense count

        internal int[] sparse; //resized immediately, holds indices to the dense array. 0.. means present at that index, -1 means not present.
        internal long minId; //sparse, inclusive
        internal long maxId; //sparse, exclusive

        internal BackupComponentSet BackupSet;
        internal int CurrentTimestamp;

        private readonly List<long> IDsMarkedForRemoval = new List<long>();

        public ComponentSet(long startingId)
        {
            minId = MathUtil.MaximumSetBit(startingId);
            maxId = MathUtil.NearestPowerOfTwo(startingId - minId) + minId;

            dense = new ComponentSetEntry<T>[StartingDenseSize];
            sparse = new int[maxId - minId];
            for(int i = 0; i < sparse.Length; i++)
            {
                sparse[i] = -1;
            }

            BackupSet = new BackupComponentSet();
        }

        public ref T Get(long entityId)
        {
            if(entityId >= minId && entityId < maxId && sparse[entityId - minId] != -1)
            {
                if(dense[sparse[entityId - minId]].ModifiedTimestamp <= CurrentTimestamp)
                {
                    return ref dense[sparse[entityId - minId]].Component;
                } else
                {
                    //This component is in the process of being removed, so ignore it ()
                    throw new ComponentRegistryException($"Component {typeof(T)} not found for entity ID {entityId}");
                }
            }
            return ref BackupSet.Get(entityId);
        }

        public ref T Create(long entityId)
        {
            if (entityId < minId || entityId >= maxId)
            {
                Console.WriteLine("Resizing ComponentSet to fit new entity ID");
                //Resize sparse array to fit new entity ID
                long newMinId = Math.Min(minId, entityId);
                long newMaxId = Math.Max(maxId, entityId + 1);

                newMinId = MathUtil.MaximumSetBit(newMinId);
                newMaxId = MathUtil.NearestPowerOfTwo(newMaxId - newMinId) + newMinId;

                int oldSize = sparse.Length;
                int newSize = (int)(newMaxId - newMinId);

                Array.Resize(ref sparse, newSize);

                int shiftElementsBy = (int)(minId - newMinId);
                if(shiftElementsBy > 0)
                {
                    Console.WriteLine($"Decreasing minimum ID of ComponentSet from {minId} to {newMinId}; Shifting sparse array elements by {shiftElementsBy}");
                    for(int i = (int)(maxId - minId - 1); i >= 0; i--)
                    {
                        sparse[i + shiftElementsBy] = sparse[i];
                        sparse[i] = -1;
                    }
                }
                for(int i = oldSize + shiftElementsBy; i < newSize; i++)
                {
                    sparse[i] = -1;
                }

                minId = newMinId;
                maxId = newMaxId;
            }

            long sparseIndex = entityId - minId;
            if (sparse[sparseIndex] >= 0) //Is in the dense array
            {
                if(dense[sparseIndex].ModifiedTimestamp > CurrentTimestamp) //Marked for removal, reclaim it
                {
                    dense[sparseIndex].ModifiedTimestamp -= 2; //Subtract 2 because:
                    //if ModifiedTimestamp == CurrentTimestamp + 1: it was added prior to this tick and thus should be enumerated
                    //if ModifiedTimestamp == CurrentTimestamp + 2: it was added THIS tick and thus should NOT be enumerated
                    IDsMarkedForRemoval.Remove(entityId);
                    dense[sparseIndex].Component = default; //Reset component data
                    return ref dense[sparseIndex].Component;
                } else
                {
                    throw new ComponentRegistryException($"Entity ID {entityId} already has a component of type {typeof(T)}");
                }
            }
            if(n < dense.Length)
            {
                //Have space in dense
                sparse[sparseIndex] = n;
                n++;
                dense[n - 1].ModifiedTimestamp = CurrentTimestamp;
                dense[n - 1].EntityId = entityId;
                dense[n - 1].Component = default;
                return ref dense[n - 1].Component;
            } else
            {
                //Have no space in dense. Use the backup set
                return ref BackupSet.Create(entityId);
            }
        }

        public void Remove(long entityId)
        {
            if (entityId >= minId && entityId < maxId && sparse[entityId - minId] != -1)
            {
                IDsMarkedForRemoval.Add(entityId);
                int timestampOffset = 1;
                if(dense[sparse[entityId - minId]].ModifiedTimestamp == CurrentTimestamp)
                {
                    //This component was added THIS tick and is being removed the same tick.
                    //In this case, set its "modified timestamp" to 2 ticks from now instead of 1.
                    //This has a special meaning when the component wants to be re-added.
                    timestampOffset = 2;
                }
                dense[sparse[entityId - minId]].ModifiedTimestamp = CurrentTimestamp + timestampOffset; //Mark as "removed" so Get<>() will skip it but iterators won't.

                //Sparse will remain pointing to a location in dense for purposes of iteration
                //Will be removed on flush
            } else
            {
                BackupSet.Remove(entityId);
            }
        }

        /// <summary>
        /// Flushes the modifications (additions, removals) to the component set made since the previous flush.
        /// This readjusts the components in memory as needed, and allows new iterators to have access to the new component list.
        /// </summary>
        internal override void Flush()
        {
            foreach(long entityId in IDsMarkedForRemoval)
            {
                int denseIndex = sparse[entityId - minId];
                FoldUtil.Assert(denseIndex >= 0, "Sparse array entry is not cleared until flushing");

                long lastEntityId = dense[n - 1].EntityId; //get entity ID of the last component in the dense array

                dense[denseIndex] = dense[n - 1]; //Move last dense entry to the index being removed
                sparse[lastEntityId - minId] = denseIndex; //Make the last entity's sparse entry point to the new index

                dense[n - 1] = default; //Clear the last component in the dense array
                n--;

                sparse[entityId - minId] = -1; //Clear the entity off the sparse array
            }
            IDsMarkedForRemoval.Clear();


            if(n + BackupSet.QueuedComponents.Count > dense.Length)
            {
                //Resize the dense array to make space for the components being added
                int newDenseSize = MathUtil.NearestPowerOfTwo(n + BackupSet.QueuedComponents.Count);

                Array.Resize(ref dense, newDenseSize);
            }

            foreach(QueuedComponent<T> queued in BackupSet.QueuedComponents)
            {
                dense[n].Component = queued.Component;
                dense[n].EntityId = queued.EntityId;
                dense[n].ModifiedTimestamp = CurrentTimestamp;
                sparse[queued.EntityId - minId] = n;
                n++;
            }
            BackupSet.Clear();


            CurrentTimestamp++;
            if(CurrentTimestamp < 0)
            {
                //oops it's been over 2 years since the game started running what the heck (assuming the game runs at 60 updates per second)

            }
        }

        public void DebugPrint()
        {
            Console.WriteLine("Component set for type " + typeof(T) + ":");
            Console.WriteLine("dense: " + string.Join(",", dense.Select(d => d.ToString(CurrentTimestamp))));
            Console.WriteLine("sparse: " + string.Join(",", sparse.Select(a => a == -1 ? "_" : a.ToString())));
            Console.WriteLine($"minId: {minId}");
            Console.WriteLine($"maxId: {maxId}");
            Console.WriteLine();
        }

        internal class BackupComponentSet
        {
            internal List<QueuedComponent<T>> QueuedComponents = new List<QueuedComponent<T>>();

            internal ref T Get(long entityId)
            {
                if (QueuedComponents.Count > 0)
                {
                    int foundIndex = FindIndexForEntityId(entityId, QueuedComponents, w => w.EntityId);
                    if (foundIndex < QueuedComponents.Count && QueuedComponents[foundIndex].EntityId == entityId)
                    {
                        return ref QueuedComponents[foundIndex].Component;
                    }
                }
                throw new ComponentRegistryException($"Component {typeof(T)} not found for entity ID {entityId}");
            }

            internal ref T Create(long entityId)
            {
                int insertionIndex = FindIndexForEntityId(entityId, QueuedComponents, w => w.EntityId);
                if (insertionIndex < QueuedComponents.Count && QueuedComponents[insertionIndex].EntityId == entityId)
                {
                    throw new ComponentRegistryException($"Entity ID {entityId} already has a component of type {typeof(T)}");
                }
                QueuedComponent<T> newWrapper = new QueuedComponent<T>
                {
                    EntityId = entityId
                };
                QueuedComponents.Insert(insertionIndex, newWrapper);
                return ref newWrapper.Component;
            }

            internal void Remove(long entityId)
            {
                int foundIndex = FindIndexForEntityId(entityId, QueuedComponents, w => w.EntityId);
                if (foundIndex < QueuedComponents.Count && QueuedComponents[foundIndex].EntityId == entityId)
                {
                    QueuedComponents.RemoveAt(foundIndex);
                }
            }

            internal void Clear()
            {
                QueuedComponents.Clear();
            }
        }


        public delegate long EntityIdentifierFunction<K>(K t);

        /// <summary>
        /// Searches a sorted list for the index at which the specified entity ID belongs or exists,
        /// for the purposes of adding components to sorted lists or searching components in sorted lists.
        /// 
        /// O(lg n) operation where n is the length of the given list.<br></br>
        /// </summary>
        /// <param name="entityId">The entity ID to search a location for</param>
        /// <param name="list">The list of objects (components, IDs) to search in</param>
        /// <param name="identifierFunction">A function that turns the an object of type T into an entity ID</param>
        /// <returns>An index between 0 and list.Count (both inclusive), reflecting one of two things:<br></br>
        /// 1. The index within the list on which a component belonging to the given entity ID is located (if it exists)<br></br>
        /// 2. The index within the list where a component belonging to the given entity ID should be located were it to be inserted into the list (if it doesn't already exist).<br></br>
        /// It's important to check the EntityId of the element at the index returned to determine whether the entity already has a component there or if it doesn't</returns>
        public static int FindIndexForEntityId<K>(long entityId, List<K> list, EntityIdentifierFunction<K> identifierFunction)
        {
            if (list.Count == 0) return 0;

            int minIndex = 0; // inclusive
            int maxIndex = list.Count; // exclusive

            if (entityId < identifierFunction(list[minIndex]))
            {
                return minIndex;
            }
            if (entityId > identifierFunction(list[maxIndex - 1]))
            {
                return maxIndex;
            }

            while (minIndex < maxIndex)
            {
                int pivotIndex = (minIndex + maxIndex) / 2;

                long pivotId = identifierFunction(list[pivotIndex]);
                if (pivotId == entityId)
                {
                    return pivotIndex;
                }
                else if (entityId > pivotId)
                {
                    minIndex = pivotIndex + 1;
                }
                else
                {
                    maxIndex = pivotIndex;
                }
            }

            return minIndex;
        }
    }
    internal class QueuedComponent<T>
    {
        public T Component;
        public long EntityId;
    }

    public struct ComponentSetEntry<T> where T : struct
    {
        internal int ModifiedTimestamp;
        public long EntityId;

        public T Component;

        public override string ToString()
        {
            return $"{EntityId}:[{Component}]";
        }

        internal string ToString(int currentTimestamp)
        {
            return $"{EntityId}:{ModifiedTimestamp - currentTimestamp}[{Component}]";
        }
    }

    public class ComponentRegistryException : Exception
    {
        public ComponentRegistryException(string message) : base(message)
        {
        }
    }
}
