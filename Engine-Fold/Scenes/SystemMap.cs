using System;
using System.Collections.Generic;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Serialization;
using FoldEngine.Systems;

namespace FoldEngine.Scenes;

public class SystemMap : ISelfSerializer
{
    private readonly List<GameSystem> _all = new List<GameSystem>();
    private readonly List<GameSystem> _fixedUpdateSystems = new List<GameSystem>();

    private readonly List<GameSystem> _inputSystems = new List<GameSystem>();
    private readonly Scene _owner;

    private bool _queueModifications = false;
    private readonly Queue<GameSystem> _queuedToAdd = new Queue<GameSystem>();
    private readonly Queue<GameSystem> _queuedToRemove = new Queue<GameSystem>();
    private readonly List<GameSystem> _renderSystems = new List<GameSystem>();
    private readonly List<GameSystem> _updateSystems = new List<GameSystem>();

    public float Accumulator;

    public SystemMap(Scene owner)
    {
        _owner = owner;
    }

    public Type WorkingType => GetType();
    public IEnumerable<GameSystem> AllSystems => _all;

    public void Serialize(SaveOperation writer)
    {
        writer.WriteCompound((ref SaveOperation.Compound c) =>
        {
            //Old format, list of strings:
            // c.WriteMember(nameof(_all), () => {
            //     int count = _all.Count;
            //     if(writer.Options.Has(SerializeExcludeSystems.Instance)) {
            //         count = 0;
            //         foreach(GameSystem sys in _all) {
            //             if(writer.Options.Get(SerializeExcludeSystems.Instance).Contains(sys.GetType())) continue;
            //             count++;
            //         }
            //     }
            //     writer.Write(count);
            //     
            //     foreach(GameSystem sys in _all) {
            //         if(count != _all.Count
            //            && writer.Options.Get(SerializeExcludeSystems.Instance).Contains(sys.GetType())) continue;
            //         writer.Write(sys.SystemName);
            //     }
            // });
            c.WriteMember(nameof(AllSystems), () =>
            {
                writer.WriteCompound((ref SaveOperation.Compound c2) =>
                {
                    foreach (GameSystem sys in _all)
                    {
                        if (writer.Options.Get(SerializeExcludeSystems.Instance)?.Contains(sys.GetType()) ?? false)
                            continue;
                        c2.WriteMember(sys.SystemName, sys);
                    }
                });
            });
        });
    }

    public void Deserialize(LoadOperation reader)
    {
        if (reader.Options.Get(DeserializeClearScene.Instance))
            foreach (GameSystem sys in _all)
                Remove(sys);

        reader.ReadCompound(c =>
        {
            if (c.HasMember(nameof(_all)))
            {
                //Old format, list of strings:
                c.StartReadMember(nameof(_all));
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string sysName = reader.ReadString();
                    Add(_owner.Core.RegistryUnit.Systems.CreateForIdentifier(sysName));
                }
            }
            else
            {
                c.StartReadMember(nameof(AllSystems));
                reader.ReadCompound(c2 =>
                {
                    foreach (string sysName in c2.MemberNames)
                    {
                        GameSystem sys = _owner.Core.RegistryUnit.Systems.CreateForIdentifier(sysName);
                        Add(sys);
                        c2.StartReadMember(sysName);
                        GenericSerializer.Deserialize(sys, reader);
                    }
                });
            }
        });
    }

    public void Add<T>() where T : GameSystem, new()
    {
        var newSys = new T();
        if(_queueModifications)
            _queuedToAdd.Enqueue(newSys);
        else
            AddDirectly(newSys);
    }

    public void Add(GameSystem sys)
    {
        _queuedToAdd.Enqueue(sys);
    }

    public void Remove(GameSystem sys)
    {
        if(_queueModifications)
            _queuedToRemove.Enqueue(sys);
        else
            RemoveDirectly(sys);
    }

    public void Remove<T>() where T : GameSystem, new()
    {
        Remove(Get<T>());
    }

    private void UpdateProcessingGroups()
    {
        _inputSystems.Clear();
        _fixedUpdateSystems.Clear();
        _updateSystems.Clear();
        _renderSystems.Clear();

        foreach (GameSystem sys in _all)
        {
            if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Input)) _inputSystems.Add(sys);

            if (sys.ProcessingCycles.HasFlag(ProcessingCycles.FixedUpdate)) _fixedUpdateSystems.Add(sys);

            if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Update)) _updateSystems.Add(sys);

            if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Render)) _renderSystems.Add(sys);
        }
    }

    internal void AddDirectly(GameSystem sys)
    {
        _all.Add(sys);
        if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Input)) _inputSystems.Add(sys);

        if (sys.ProcessingCycles.HasFlag(ProcessingCycles.FixedUpdate)) _fixedUpdateSystems.Add(sys);

        if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Update)) _updateSystems.Add(sys);

        if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Render)) _renderSystems.Add(sys);

        sys.Scene = _owner;
        sys.Initialize();
        sys.SubscribeToEvents();
    }

    private void RemoveDirectly(GameSystem sys)
    {
        _all.Remove(sys);
        _inputSystems.Remove(sys);
        _updateSystems.Remove(sys);
        _fixedUpdateSystems.Remove(sys);
        _renderSystems.Remove(sys);

        sys.UnsubscribeFromEvents();
        sys.Scene = null;
    }

    internal void Flush()
    {
        while (_queuedToAdd.Count > 0) AddDirectly(_queuedToAdd.Dequeue());

        while (_queuedToRemove.Count > 0) RemoveDirectly(_queuedToRemove.Dequeue());
    }

    internal void InvokeInput()
    {
        _queueModifications = true;
        foreach (GameSystem sys in _inputSystems)
            if (!_owner.Paused || sys.RunWhenPaused)
            {
                sys.OnInput();
                _owner.Events.FlushAfterSystem();
            }
        _queueModifications = false;
    }

    internal void InvokeUpdate()
    {
        _queueModifications = true;
        foreach (GameSystem sys in _updateSystems)
            if (!_owner.Paused || sys.RunWhenPaused)
            {
                sys.OnUpdate();
                _owner.Events.FlushAfterSystem();
            }

        _queueModifications = false;
        _owner.Events.FlushEnd();
    }

    internal void InvokeFixedUpdate()
    {
        Accumulator += Time.DeltaTime;
        while (Accumulator >= Time.FixedDeltaTime)
        {
            _queueModifications = true;
            foreach (GameSystem sys in _fixedUpdateSystems)
                if (!_owner.Paused || sys.RunWhenPaused)
                {
                    sys.OnFixedUpdate();
                    _owner.Events.FlushAfterSystem();
                }
            _queueModifications = false;

            Accumulator -= Time.FixedDeltaTime;

            _owner.Events.FlushEnd();
        }
    }

    internal void InvokeRender(IRenderingUnit renderer)
    {
        _queueModifications = true;
        foreach (GameSystem sys in _renderSystems)
            if (!_owner.Paused || sys.RunWhenPaused)
            {
                sys.OnRender(renderer);
                _owner.Events.FlushAfterSystem();
            }
        _queueModifications = false;
    }

    internal void PollResources()
    {
        _queueModifications = true;
        foreach (GameSystem sys in _all)
            sys.PollResources();
        _queueModifications = false;
    }

    public T Get<T>() where T : GameSystem
    {
        foreach (GameSystem sys in _all)
            if (sys is T system)
                return system;

        return null;
    }

    public GameSystem Get(Type type)
    {
        foreach (GameSystem sys in _all)
            if (type.IsInstanceOfType(sys))
                return sys;

        return null;
    }

    public int GetSystemIndex(Type type)
    {
        int i = 0;
        foreach (GameSystem sys in _all)
        {
            if (type.IsInstanceOfType(sys))
                return i;
            i++;
        }

        return -1;
    }

    public void ChangeSystemOrder(Type sysType, int toIndex)
    {
        GameSystem sys = Get(sysType);
        if (sys == null)
            throw new ArgumentException(
                $"Cannot change order of system {sysType}: such system doesn't exist in the scene");
        _all.Remove(sys);
        _all.Insert(Math.Max(0, Math.Min(toIndex, _all.Count)), sys);
        UpdateProcessingGroups();
        ResubscribeAll();
    }

    private void ResubscribeAll()
    {
        foreach (GameSystem sys in _all)
        {
            sys.UnsubscribeFromEvents();
            sys.SubscribeToEvents();
        }
    }
}