using System;
using FoldEngine.Interfaces;
using Microsoft.Xna.Framework.Audio;

namespace FoldEngine.Audio {
    public class SoundInstance {
        private readonly AudioUnit _unit;
        private readonly SoundEffectInstance _instance;
        public readonly string Name;
        internal bool FreeOnStop = false;
        internal bool InUse = false;

        public SoundInstance(AudioUnit unit, SoundEffectInstance instance, string name) {
            _unit = unit;
            _instance = instance;
            Name = name;
            // Console.WriteLine($"Created SoundInstance {name}");
        }

        public bool Playing => _instance.State == SoundState.Playing;

        public bool Looping {
            get => _instance.IsLooped;
            set => _instance.IsLooped = value;
        }

        public float Volume {
            get => _instance.Volume;
            set => _instance.Volume = value;
        }

        public float Pitch {
            get => _instance.Pitch;
            set => _instance.Pitch = value;
        }

        public float Pan {
            get => _instance.Pan;
            set => _instance.Pan = value;
        }

        public void Play() {
            _instance.Play();
        }

        public void Pause() => _instance.Pause();
        public void Resume() => _instance.Resume();
        public void Stop() => _instance.Stop();

        public void Free() {
            // Console.WriteLine($"Freed {Name}");
            _instance.Stop();
            FreeOnStop = false;
            InUse = false;
        }

        public void PlayOnce() {
            _instance.Play();
            FreeOnStop = true;
        }

        public void Reset() {
            Volume = 1;
            Pan = 0;
            Pitch = 0;
            Looping = false;
            FreeOnStop = false;
        }

        public void Update() {
            if(FreeOnStop && !Playing) {
                Free();
            }
        }
    }
}