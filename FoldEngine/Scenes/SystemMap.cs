using FoldEngine.Systems;

using System;
using System.Collections.Generic;
using System.Text;

namespace FoldEngine.Scenes
{
    public class SystemMap
    {
        private readonly Scene Owner;

        private readonly List<GameSystem> All = new List<GameSystem>();

        private readonly List<GameSystem> InputSystems = new List<GameSystem>();
        private readonly List<GameSystem> UpdateSystems = new List<GameSystem>();
        private readonly List<GameSystem> RenderSystems = new List<GameSystem>();

        private readonly Queue<GameSystem> QueuedToAdd = new Queue<GameSystem>();
        private readonly Queue<GameSystem> QueuedToRemove = new Queue<GameSystem>();

        public SystemMap(Scene owner) => Owner = owner;

        public void Add<T>() where T : GameSystem, new()
        {
            QueuedToAdd.Enqueue(new T());
        }
        public void Remove(GameSystem sys)
        {
            QueuedToRemove.Enqueue(sys);
        }

        private void UpdateProcessingGroups()
        {
            InputSystems.Clear();
            UpdateSystems.Clear();
            RenderSystems.Clear();

            foreach(GameSystem sys in All)
            {
                if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Input))
                {
                    InputSystems.Add(sys);
                }
                if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Update))
                {
                    UpdateSystems.Add(sys);
                }
                if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Render))
                {
                    RenderSystems.Add(sys);
                }
            }
        }

        private void AddDirectly(GameSystem sys)
        {
            All.Add(sys);
            if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Input))
            {
                InputSystems.Add(sys);
            }
            if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Update))
            {
                UpdateSystems.Add(sys);
            }
            if (sys.ProcessingCycles.HasFlag(ProcessingCycles.Render))
            {
                RenderSystems.Add(sys);
            }

            sys.Owner = Owner;
            sys.Initialize();
        }

        private void RemoveDirectly(GameSystem sys)
        {
            All.Remove(sys);
            InputSystems.Remove(sys);
            UpdateSystems.Remove(sys);
            RenderSystems.Remove(sys);
            sys.Owner = null;
            //todo remove any listeners and disconnect any views
        }

        internal void Flush()
        {
            {
                GameSystem toAdd;
                while (QueuedToAdd.Count > 0)
                {
                    toAdd = QueuedToAdd.Dequeue();

                    AddDirectly(toAdd);
                }
            }
            {
                GameSystem toRemove;
                while (QueuedToRemove.Count > 0)
                {
                    toRemove = QueuedToRemove.Dequeue();

                    RemoveDirectly(toRemove);
                }
            }
        }

        internal void InvokeInput()
        {
            foreach (GameSystem sys in InputSystems)
            {
                sys.OnInput();
            }
        }

        internal void InvokeUpdate()
        {
            foreach (GameSystem sys in UpdateSystems)
            {
                sys.OnUpdate();
            }
        }

        internal void InvokeRender() //TODO needed arguments
        {
            foreach (GameSystem sys in RenderSystems)
            {
                sys.OnRender();
            }
        }
    }
}
