using FoldEngine.Interfaces;
using FoldEngine.Systems;

using System;
using System.Collections.Generic;
using System.Text;
using FoldEngine.Serialization;

namespace FoldEngine.Scenes
{
    public class SystemMap : ISelfSerializer {
        private readonly Scene _owner;

        private readonly List<GameSystem> _all = new List<GameSystem>();

        private readonly List<GameSystem> _inputSystems = new List<GameSystem>();
        private readonly List<GameSystem> _updateSystems = new List<GameSystem>();
        private readonly List<GameSystem> _renderSystems = new List<GameSystem>();

        private readonly Queue<GameSystem> _queuedToAdd = new Queue<GameSystem>();
        private readonly Queue<GameSystem> _queuedToRemove = new Queue<GameSystem>();

        public SystemMap(Scene owner) => _owner = owner;

        public void Add<T>() where T : GameSystem, new() {
            _queuedToAdd.Enqueue(new T());
        }

        public void Add(GameSystem sys) {
            _queuedToAdd.Enqueue(sys);
        }

        public void Remove(GameSystem sys) {
            _queuedToRemove.Enqueue(sys);
        }

        private void UpdateProcessingGroups() {
            _inputSystems.Clear();
            _updateSystems.Clear();
            _renderSystems.Clear();

            foreach(GameSystem sys in _all) {
                if(sys.ProcessingCycles.HasFlag(ProcessingCycles.Input)) {
                    _inputSystems.Add(sys);
                }

                if(sys.ProcessingCycles.HasFlag(ProcessingCycles.Update)) {
                    _updateSystems.Add(sys);
                }

                if(sys.ProcessingCycles.HasFlag(ProcessingCycles.Render)) {
                    _renderSystems.Add(sys);
                }
            }
        }

        private void AddDirectly(GameSystem sys) {
            _all.Add(sys);
            if(sys.ProcessingCycles.HasFlag(ProcessingCycles.Input)) {
                _inputSystems.Add(sys);
            }

            if(sys.ProcessingCycles.HasFlag(ProcessingCycles.Update)) {
                _updateSystems.Add(sys);
            }

            if(sys.ProcessingCycles.HasFlag(ProcessingCycles.Render)) {
                _renderSystems.Add(sys);
            }

            sys.Owner = _owner;
            sys.Initialize();
            sys.SubscribeToEvents();
        }

        private void RemoveDirectly(GameSystem sys) {
            _all.Remove(sys);
            _inputSystems.Remove(sys);
            _updateSystems.Remove(sys);
            _renderSystems.Remove(sys);

            sys.Owner = null;
            //todo remove any listeners and disconnect any views
        }

        internal void Flush() {
            while(_queuedToAdd.Count > 0) {
                AddDirectly(_queuedToAdd.Dequeue());
            }

            while(_queuedToRemove.Count > 0) {
                RemoveDirectly(_queuedToRemove.Dequeue());
            }
        }

        internal void InvokeInput() {
            foreach(GameSystem sys in _inputSystems) {
                sys.OnInput();
            }
        }

        internal void InvokeUpdate() {
            foreach(GameSystem sys in _updateSystems) {
                sys.OnUpdate();
                _owner.Events.FlushAfterSystem();
            }
            _owner.Events.FlushEnd();
        }

        internal void InvokeRender(IRenderingUnit renderer) {
            foreach(GameSystem sys in _renderSystems) {
                sys.OnRender(renderer);
            }
        }

        public T Get<T>() where T : GameSystem {
            foreach(GameSystem sys in _all) {
                if(sys is T system) return system;
            }

            return null;
        }

        public Type WorkingType => this.GetType();
        
        public void Serialize(SaveOperation writer) {
            writer.WriteOpenCompound(1);
            writer.WriteMember(nameof(_all), () => {
                writer.Write(_all.Count);
                foreach(GameSystem sys in _all) {
                    writer.Write(sys.SystemName);
                }
            });
        }

        public void Deserialize(LoadOperation reader) {
            foreach(GameSystem sys in _all) {
                Remove(sys);
            }
            reader.ReadCompound(c => {
                c.StartReadMember(nameof(_all));
                int count = reader.ReadInt32();
                for(int i = 0; i < count; i++) {
                    string sysName = reader.ReadString();
                    Add(GameSystem.CreateForIdentifier(sysName));
                    Console.WriteLine("System: " + sysName);
                }
            });
        }
    }
}
