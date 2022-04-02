using System;
using System.IO;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Serialization;
using Microsoft.Xna.Framework.Audio;

namespace FoldEngine.Audio {
    [Resource(directoryName: "sound", extension: "wav")]
    public class Sound : Resource {
        internal SoundEffect Effect;

        public override void Unload() {
            Effect.Dispose();
        }

        public override void DeserializeResource(Stream stream) {
            Effect = SoundEffect.FromStream(stream);
        }

        public override void DeserializeResource(LoadOperation reader) {
            throw new InvalidOperationException("Sound Effects cannot be deserialized");
        }

        public override void SerializeResource(SaveOperation writer) {
            throw new InvalidOperationException("Sound Effects cannot be serialized");
        }
    }

    public class SoundInstance {
        private readonly AudioUnit _unit;
        private readonly SoundEffectInstance _instance;
        private readonly Sound _sound;
        internal bool FreeOnStop = false;
        internal bool InUse = false;

        public SoundInstance(AudioUnit unit, Sound sound) {
            _unit = unit;
            _sound = sound;
            _instance = sound.Effect.CreateInstance();
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
            if(Playing) {
                _sound.Access();
            }
            
            if(FreeOnStop && !Playing) {
                Free();
            }
        }
    }
}