using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Util;

namespace FoldEngine.Components;

public abstract class ComponentSet : ISelfSerializer
{
    public Type WorkingType => GetType();

    public abstract Type ComponentType { get; }
    public abstract void Serialize(SaveOperation writer);
    public abstract void Deserialize(LoadOperation reader);
    internal abstract void Flush();
    public abstract bool Has(long entityId);
    public abstract void Remove(long entityId);
    public abstract void CreateFor(long entityId);

    public abstract object GetBoxedComponent(long entityId);
    public abstract object GetFieldValue(long entityId, FieldInfo fieldInfo);
    public abstract void SetFieldValue(long entityId, FieldInfo fieldInfo, object value);

    public abstract void Clear();
}

public class ComponentSet<T> : ComponentSet where T : struct
{
    private const int StartingDenseSize = 16;

    private readonly List<int> _dsMarkedForRemoval = new List<int>();

    internal readonly Scene Scene;

    internal BackupComponentSet BackupSet;
    internal int CurrentTimestamp;

    internal ComponentSetEntry<T>[] Dense; //resized on flush
    internal int MaxId; //sparse, exclusive

    internal int MinId; //sparse, inclusive
    internal int N; //dense count

    internal int[]
        Sparse; //resized immediately, holds indices to the dense array. 0.. means present at that index, -1 means not present.

    public ComponentSet(Scene scene, int startingId)
    {
        Scene = scene;
        MinId = MathUtil.MaximumSetBit(startingId);
        MaxId = MathUtil.NearestPowerOfTwo(startingId - MinId) + MinId;

        Dense = new ComponentSetEntry<T>[StartingDenseSize];
        Sparse = new int[MaxId - MinId];
        for (int i = 0; i < Sparse.Length; i++) Sparse[i] = -1;

        BackupSet = new BackupComponentSet();

        // Console.WriteLine("Creating component set for type " + typeof(T));
    }

    public override Type ComponentType => typeof(T);

    public ref T Get(long entityId)
    {
        if ((int)entityId < MinId || (int)entityId >= MaxId || Sparse[(int)entityId - MinId] == -1)
            return ref BackupSet.Get(entityId);

        if (Dense[Sparse[(int)entityId - MinId]].Status.HasFlag(ComponentStatus.Retrievable)
            && Dense[Sparse[(int)entityId - MinId]].EntityId == entityId)
            return ref Dense[Sparse[(int)entityId - MinId]].Component;
        throw new ComponentRegistryException($"Component {typeof(T)} not found for entity ID {entityId}");
    }

    public override bool Has(long entityId)
    {
        if ((int)entityId < MinId || (int)entityId >= MaxId || Sparse[(int)entityId - MinId] == -1)
            return BackupSet.Has(entityId);

        return Dense[Sparse[(int)entityId - MinId]].Status.HasFlag(ComponentStatus.Retrievable)
               && Dense[Sparse[(int)entityId - MinId]].EntityId == entityId;
    }

    private bool Has(int entityId)
    {
        if (entityId < MinId || entityId >= MaxId || Sparse[entityId - MinId] == -1)
            return BackupSet.Has(entityId);

        return Dense[Sparse[entityId - MinId]].Status.HasFlag(ComponentStatus.Retrievable);
    }

    public override void CreateFor(long entityId)
    {
        Create(entityId);
    }

    public override object GetBoxedComponent(long entityId)
    {
        return Get(entityId);
    }

    public override object GetFieldValue(long entityId, FieldInfo fieldInfo)
    {
        return fieldInfo.GetValueDirect(__makeref(Get(entityId)));
    }

    public override void SetFieldValue(long entityId, FieldInfo fieldInfo, object value)
    {
        fieldInfo.SetValueDirect(__makeref(Get(entityId)), value);
    }

    public ref T Create(long entityId)
    {
        var componentRegistry = Scene.Core.RegistryUnit.Components;

        if ((int)entityId < MinId || (int)entityId >= MaxId)
        {
            // Console.WriteLine("Resizing ComponentSet to fit new entity ID");
            //Resize sparse array to fit new entity ID
            int newMinId = Math.Min(MinId, (int)entityId);
            int newMaxId = Math.Max(MaxId, (int)entityId + 1);

            newMinId = MathUtil.MaximumSetBit(newMinId);
            newMaxId = MathUtil.NearestPowerOfTwo(newMaxId - newMinId) + newMinId;

            int oldSize = Sparse.Length;
            int newSize = newMaxId - newMinId;

            Array.Resize(ref Sparse, newSize);

            int shiftElementsBy = MinId - newMinId;
            if (shiftElementsBy > 0)
            {
                Console.WriteLine(
                    $"Decreasing minimum ID of ComponentSet from {MinId} to {newMinId}; Shifting sparse array elements by {shiftElementsBy}");
                for (int i = MaxId - MinId - 1; i >= 0; i--)
                {
                    Sparse[i + shiftElementsBy] = Sparse[i];
                    Sparse[i] = -1;
                }

                // Pad the beginning with -1
                for (int i = 0; i < shiftElementsBy; i++) Sparse[i] = -1;
            }

            // Pad the end with -1
            for (int i = oldSize + shiftElementsBy; i < newSize; i++) Sparse[i] = -1;

            MinId = newMinId;
            MaxId = newMaxId;
        }

        int sparseIndex = (int)entityId - MinId;
        if (Sparse[sparseIndex] >= 0) //Is in the dense array
        {
            if (Dense[Sparse[sparseIndex]].Status.HasFlag(ComponentStatus.MarkedForRemoval)) //Marked for removal, reclaim it
            {
                Dense[Sparse[sparseIndex]].Status = ComponentStatus.Active;
                _dsMarkedForRemoval.Remove((int)entityId);
                Dense[Sparse[sparseIndex]].Component = default; //Reset component data
                Dense[Sparse[sparseIndex]].EntityId = entityId;
                ref var reclaimedComponent = ref Dense[Sparse[sparseIndex]].Component;
                componentRegistry.InitializeComponent(ref reclaimedComponent, Scene, entityId);
                Scene.Events.Invoke(new ComponentAddedEvent<T>()
                {
                    EntityId = entityId,
                    Component = reclaimedComponent
                });
                return ref reclaimedComponent;
            }

            if (Dense[Sparse[sparseIndex]].EntityId == entityId)
                throw new ComponentRegistryException(
                    $"Entity ID {entityId} already has a component of type {typeof(T)}");
            throw new ComponentRegistryException(
                $"Cannot create component {typeof(T)} for Entity ID {entityId}: An entity of a different generation already has one: {Dense[Sparse[sparseIndex]].EntityId}");
        }

        if (N < Dense.Length)
        {
            //Have space in dense
            Sparse[sparseIndex] = N;
            N++;
            Dense[N - 1].Status = ComponentStatus.Active;
            Dense[N - 1].EntityId = entityId;
            Dense[N - 1].Component = default;
            ref var newComponent = ref Dense[N - 1].Component;
            componentRegistry.InitializeComponent(ref newComponent, Scene, entityId);
            
            Scene.Events.Invoke(new ComponentAddedEvent<T>()
            {
                EntityId = entityId,
                Component = newComponent
            });
            return ref newComponent;
        }

        //Have no space in dense. Use the backup set
        ref T component = ref BackupSet.Create(entityId);
        componentRegistry.InitializeComponent<T>(ref component, Scene, entityId);
        Scene.Events.Invoke(new ComponentAddedEvent<T>()
        {
            EntityId = entityId,
            Component = component
        });
        return ref component;
    }

    public override void Remove(long entityId)
    {
        if ((int)entityId >= MinId && (int)entityId < MaxId && Sparse[(int)entityId - MinId] != -1)
        {
            _dsMarkedForRemoval.Add((int)entityId);
            ComponentStatus newStatus = ComponentStatus.Inactive;
            if (Dense[Sparse[(int)entityId - MinId]].Status == ComponentStatus.JustNowAdded
               ) //This component was added THIS tick and is being removed the same tick.
                //In this case, set its "modified timestamp" to 2 ticks from now instead of 1.
                //This has a special meaning when the component wants to be re-added.
                newStatus = ComponentStatus.Inactive;

            Dense[Sparse[(int)entityId - MinId]].Status = newStatus; //Mark as "removed" so Get<>() will skip it but iterators won't.

            //Sparse will remain pointing to a location in dense for purposes of iteration
            //Will be removed on flush
            
            Scene.Events.Invoke(new ComponentRemovedEvent<T>()
            {
                EntityId = entityId,
                Component = Dense[Sparse[(int)entityId - MinId]].Component
            });
        }
        else if(BackupSet.Has(entityId))
        {
            var component = BackupSet.Get(entityId);
            BackupSet.Remove(entityId);
            Scene.Events.Invoke(new ComponentRemovedEvent<T>()
            {
                EntityId = entityId,
                Component = component
            });
        }
    }

    /// <summary>
    ///     Flushes the modifications (additions, removals) to the component set made since the previous flush.
    ///     This readjusts the components in memory as needed, and allows new iterators to have access to the new component
    ///     list.
    /// </summary>
    internal override void Flush()
    {
        foreach (int entityId in _dsMarkedForRemoval)
        {
            int denseIndex = Sparse[entityId - MinId];
            FoldUtil.Assert(denseIndex >= 0, "Sparse array entry is not cleared until flushing");

            int lastEntityId = (int)Dense[N - 1].EntityId; //get entity ID of the last component in the dense array

            Dense[denseIndex] = Dense[N - 1]; //Move last dense entry to the index being removed
            Sparse[lastEntityId - MinId] = denseIndex; //Make the last entity's sparse entry point to the new index

            Dense[N - 1] = default; //Clear the last component in the dense array
            N--;

            Sparse[entityId - MinId] = -1; //Clear the entity off the sparse array
        }

        _dsMarkedForRemoval.Clear();


        if (N + BackupSet.QueuedComponents.Count > Dense.Length)
        {
            //Resize the dense array to make space for the components being added
            int newDenseSize = MathUtil.NearestPowerOfTwo(N + BackupSet.QueuedComponents.Count);

            Array.Resize(ref Dense, newDenseSize);
        }

        foreach (QueuedComponent<T> queued in BackupSet.QueuedComponents)
        {
            Dense[N].Component = queued.Component;
            Dense[N].EntityId = queued.EntityId;
            Dense[N].Status = ComponentStatus.Active;
            Sparse[(int)queued.EntityId - MinId] = N;
            N++;
        }

        BackupSet.Clear();

        // Set all components' status to active
        for (int denseIndex = 0; denseIndex < N; denseIndex++)
        {
            Dense[denseIndex].Status = ComponentStatus.Active;
        }

        CurrentTimestamp++;
        if (CurrentTimestamp < 0)
        {
            //oops it's been over 2 years since the game started running what the heck (assuming the game runs at 60 updates per second)
        }
    }

    public override void Serialize(SaveOperation writer)
    {
        writer.WriteArray((ref SaveOperation.Array arr) =>
        {
            for (int entityId = MinId; entityId < MaxId; entityId++)
            {
                if (!Has(entityId)) continue;
                if (writer.Options.Has(SerializeOnlyEntities.Instance))
                {
                    bool shouldSerialize = false;
                    foreach (long filteredEntityId in writer.Options.Get(SerializeOnlyEntities.Instance))
                        if ((int)filteredEntityId == entityId)
                        {
                            shouldSerialize = true;
                            break;
                        }

                    if (!shouldSerialize) continue;
                }

                arr.WriteMember(() =>
                {
                    // ReSharper disable twice AccessToModifiedClosure
                    writer.Write(Dense[Sparse[entityId - MinId]].EntityId);
                    ComponentSerializer.Serialize(Get(Dense[Sparse[entityId - MinId]].EntityId), writer);
                });
            }
        });
    }

    public override void Deserialize(LoadOperation reader)
    {
        reader.ReadArray(arr =>
        {
            for (int i = 0; i < arr.MemberCount; i++)
            {
                arr.StartReadMember(i);
                long entityId = reader.ReadInt64();
                if (reader.Options.Has(DeserializeRemapIds.Instance))
                    entityId = reader.Options.Get(DeserializeRemapIds.Instance).TransformId(entityId);
                // Console.WriteLine($"{ComponentType} for entity id " + entityId);
                if (!Has((int)entityId)) CreateFor(entityId);
                ComponentSerializer.Deserialize<T>(this, entityId, reader);
            }
        });
    }

    public override void Clear()
    {
        for (int i = 0; i < Sparse.Length; i++) Sparse[i] = -1;
        N = 0;
    }

    public void DebugPrint()
    {
        Console.WriteLine("Component set for type " + typeof(T) + ":");
        Console.WriteLine("dense: " + string.Join(",", Dense.Select(d => d.ToString())));
        Console.WriteLine("sparse: " + string.Join(",", Sparse.Select(a => a == -1 ? "_" : a.ToString())));
        Console.WriteLine($"minId: {MinId}");
        Console.WriteLine($"maxId: {MaxId}");
        Console.WriteLine();
    }

    internal class BackupComponentSet
    {
        internal readonly List<QueuedComponent<T>> QueuedComponents = new List<QueuedComponent<T>>();

        internal ref T Get(long entityId)
        {
            if (QueuedComponents.Count > 0)
            {
                int foundIndex = FindIndexForEntityId((int)entityId, QueuedComponents, w => (int)w.EntityId);
                if (foundIndex < QueuedComponents.Count && QueuedComponents[foundIndex].EntityId == entityId)
                    return ref QueuedComponents[foundIndex].Component;
            }

            throw new ComponentRegistryException($"Component {typeof(T)} not found for entity ID {entityId}");
        }

        internal ref T Create(long entityId)
        {
            int insertionIndex = FindIndexForEntityId((int)entityId, QueuedComponents, w => (int)w.EntityId);
            if (insertionIndex < QueuedComponents.Count && QueuedComponents[insertionIndex].EntityId == entityId)
                throw new ComponentRegistryException(
                    $"Entity ID {entityId} already has a component of type {typeof(T)}");

            var newWrapper = new QueuedComponent<T>
            {
                EntityId = entityId
            };
            QueuedComponents.Insert(insertionIndex, newWrapper);
            return ref newWrapper.Component;
        }

        internal void Remove(long entityId)
        {
            int foundIndex = FindIndexForEntityId((int)entityId, QueuedComponents, w => (int)w.EntityId);
            if (foundIndex < QueuedComponents.Count && QueuedComponents[foundIndex].EntityId == entityId)
                QueuedComponents.RemoveAt(foundIndex);
        }

        internal void Clear()
        {
            QueuedComponents.Clear();
        }

        public bool Has(long entityId)
        {
            int foundIndex = FindIndexForEntityId((int)entityId, QueuedComponents, w => (int)w.EntityId);
            return foundIndex < QueuedComponents.Count && QueuedComponents[foundIndex].EntityId == entityId;
        }

        public bool Has(int entityId)
        {
            int foundIndex = FindIndexForEntityId(entityId, QueuedComponents, w => (int)w.EntityId);
            return foundIndex < QueuedComponents.Count && (int)QueuedComponents[foundIndex].EntityId == entityId;
        }
    }


    public delegate int EntityIdentifierFunction<TK>(TK t);

    /// <summary>
    ///     Searches a sorted list for the index at which the specified entity ID beints or exists,
    ///     for the purposes of adding components to sorted lists or searching components in sorted lists.
    ///     O(lg n) operation where n is the length of the given list.<br></br>
    /// </summary>
    /// <param name="entityId">The entity ID to search a location for</param>
    /// <param name="list">The list of objects (components, IDs) to search in</param>
    /// <param name="identifierFunction">A function that turns the an object of type T into an entity ID</param>
    /// <returns>
    ///     An index between 0 and list.Count (both inclusive), reflecting one of two things:<br></br>
    ///     1. The index within the list on which a component beinting to the given entity ID is located (if it exists)
    ///     <br></br>
    ///     2. The index within the list where a component beinting to the given entity ID should be located were it to be
    ///     inserted into the list (if it doesn't already exist).<br></br>
    ///     It's important to check the EntityId of the element at the index returned to determine whether the entity already
    ///     has a component there or if it doesn't
    /// </returns>
    public static int FindIndexForEntityId<TK>(
        int entityId,
        List<TK> list,
        EntityIdentifierFunction<TK> identifierFunction)
    {
        if (list.Count == 0) return 0;

        int minIndex = 0; // inclusive
        int maxIndex = list.Count; // exclusive

        if (entityId < identifierFunction(list[minIndex])) return minIndex;

        if (entityId > identifierFunction(list[maxIndex - 1])) return maxIndex;

        while (minIndex < maxIndex)
        {
            int pivotIndex = (minIndex + maxIndex) / 2;

            int pivotId = identifierFunction(list[pivotIndex]);
            if (pivotId == entityId)
                return pivotIndex;
            if (entityId > pivotId)
                minIndex = pivotIndex + 1;
            else
                maxIndex = pivotIndex;
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
    internal ComponentStatus Status;
    public long EntityId;

    public T Component;

    public override string ToString()
    {
        return $"{EntityId}:{Status}[{Component}]";
    }
}

[Flags]
internal enum ComponentStatus
{
    Retrievable = 1,
    Enumerable = 2,
    MarkedForRemoval = 4,
    
    Inactive = 0,
    JustNowAdded = Retrievable,
    Active = Retrievable | Enumerable
}

public class ComponentRegistryException : Exception
{
    public ComponentRegistryException(string message) : base(message)
    {
    }
}