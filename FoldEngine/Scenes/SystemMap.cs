using System;
using System.Collections.Generic;
using FoldEngine.Interfaces;
using FoldEngine.Serialization;
using FoldEngine.Systems;

namespace FoldEngine.Scenes {
    public class SystemMap : ISelfSerializer {
        private readonly List<GameSystem> _all = new List<GameSystem>();
        private readonly List<GameSystem> _fixedUpdateSystems = new List<GameSystem>();

        private readonly List<GameSystem> _inputSystems = new List<GameSystem>();
        private readonly Scene _owner;

        private readonly Queue<GameSystem> _queuedToAdd = new Queue<GameSystem>();
        private readonly Queue<GameSystem> _queuedToRemove = new Queue<GameSystem>();
        private readonly List<GameSystem> _renderSystems = new List<GameSystem>();
        private readonly List<GameSystem> _updateSystems = new List<GameSystem>();

        public float Accumulator;

        public SystemMap(Scene owner) {
            _owner = owner;
        }

        public Type WorkingType => GetType();
        public IEnumerable<GameSystem> AllSystems => _all;

        public void Serialize(SaveOperation writer) {
            writer.WriteCompound((ref SaveOperation.Compound c) => {
                c.WriteMember(nameof(_all), () => {
                    int count = _all.Count;
                    if(writer.Options.Has(SerializeExcludeSystems.Instance)) {
                        count = 0;
                        foreach(GameSystem sys in _all) {
                            if(writer.Options.Get(SerializeExcludeSystems.Instance).Contains(sys.GetType())) continue;
                            count++;
                        }
                    }
                    writer.Write(count);
                    
                    foreach(GameSystem sys in _all) {
                        if(count != _all.Count
                           && writer.Options.Get(SerializeExcludeSystems.Instance).Contains(sys.GetType())) continue;
                        writer.Write(sys.SystemName);
                    }
                });
            });
        }

        public void Deserialize(LoadOperation reader) {
            if(reader.Options.Get(DeserializeClearScene.Instance))
                foreach(GameSystem sys in _all)
                    Remove(sys);

            reader.ReadCompound(c => {
                c.StartReadMember(nameof(_all));
                int count = reader.ReadInt32();
                for(int i = 0; i < count; i++) {
                    string sysName = reader.ReadString();
                    Add(GameSystem.CreateForIdentifier(sysName));
                }
            });
        }

        public void Add<T>() where T : GameSystem, new() {
            _queuedToAdd.Enqueue(new T());
        }

        public void Add(GameSystem sys) {
            _queuedToAdd.Enqueue(sys);
        }

        public void Remove(GameSystem sys) {
            _queuedToRemove.Enqueue(sys);
        }

        public void Remove<T>() where T : GameSystem, new() {
            Remove(Get<T>());
        }

        private void UpdateProcessingGroups() {
            _inputSystems.Clear();
            _fixedUpdateSystems.Clear();
            _updateSystems.Clear();
            _renderSystems.Clear();

            foreach(GameSystem sys in _all) {
                if(sys.ProcessingCycles.Has(ProcessingCycles.Input)) _inputSystems.Add(sys);

                if(sys.ProcessingCycles.Has(ProcessingCycles.FixedUpdate)) _fixedUpdateSystems.Add(sys);

                if(sys.ProcessingCycles.Has(ProcessingCycles.Update)) _updateSystems.Add(sys);

                if(sys.ProcessingCycles.Has(ProcessingCycles.Render)) _renderSystems.Add(sys);
            }
        }

        private void AddDirectly(GameSystem sys) {
            _all.Add(sys);
            if(sys.ProcessingCycles.Has(ProcessingCycles.Input)) _inputSystems.Add(sys);

            if(sys.ProcessingCycles.Has(ProcessingCycles.FixedUpdate)) _fixedUpdateSystems.Add(sys);

            if(sys.ProcessingCycles.Has(ProcessingCycles.Update)) _updateSystems.Add(sys);

            if(sys.ProcessingCycles.Has(ProcessingCycles.Render)) _renderSystems.Add(sys);

            sys.Scene = _owner;
            sys.Initialize();
            sys.SubscribeToEvents();
        }

        private void RemoveDirectly(GameSystem sys) {
            _all.Remove(sys);
            _inputSystems.Remove(sys);
            _updateSystems.Remove(sys);
            _fixedUpdateSystems.Remove(sys);
            _renderSystems.Remove(sys);

            sys.UnsubscribeFromEvents();
            sys.Scene = null;
        }

        internal void Flush() {
            while(_queuedToAdd.Count > 0) AddDirectly(_queuedToAdd.Dequeue());

            while(_queuedToRemove.Count > 0) RemoveDirectly(_queuedToRemove.Dequeue());
        }

        internal void InvokeInput() {
            foreach(GameSystem sys in _inputSystems)
                if(!_owner.Paused || sys.RunWhenPaused) {
                    sys.OnInput();
                    _owner.Events.FlushAfterSystem();
                }
        }

        internal void InvokeUpdate() {
            foreach(GameSystem sys in _updateSystems)
                if(!_owner.Paused || sys.RunWhenPaused) {
                    sys.OnUpdate();
                    _owner.Events.FlushAfterSystem();
                }

            _owner.Events.FlushEnd();
        }

        internal void InvokeFixedUpdate() {
            Accumulator += Time.DeltaTime;
            while(Accumulator >= Time.FixedDeltaTime) {
                foreach(GameSystem sys in _fixedUpdateSystems)
                    if(!_owner.Paused || sys.RunWhenPaused) {
                        sys.OnFixedUpdate();
                        _owner.Events.FlushAfterSystem();
                    }

                Accumulator -= Time.FixedDeltaTime;

                _owner.Events.FlushEnd();
            }
        }

        internal void InvokeRender(IRenderingUnit renderer) {
            foreach(GameSystem sys in _renderSystems)
                if(!_owner.Paused || sys.RunWhenPaused) {
                    sys.OnRender(renderer);
                    _owner.Events.FlushAfterSystem();
                }
        }

        public T Get<T>() where T : GameSystem {
            foreach(GameSystem sys in _all)
                if(sys is T system)
                    return system;

            return null;
        }

        public GameSystem Get(Type type) {
            foreach(GameSystem sys in _all)
                if(type.IsInstanceOfType(sys))
                    return sys;

            return null;
        }
    }
}