using ChaiFoxes.FMODAudio;
using FoldEngine.Interfaces;
using FoldEngine.Resources;

namespace FoldEngine.Audio {
    [Resource("sound", 5000, "wav", "ogg", "mp3")]
    public class Sound : Resource {
        internal ChaiFoxes.FMODAudio.Sound Effect;

        public override bool CanSerialize => false;

        public override bool Unload() {
            Effect.Dispose();
            return true;
        }

        public override void DeserializeResource(string path) {
            Effect = CoreSystem.LoadSound(path);
        }
    }

    public class SoundInstance {
        private readonly Sound _sound;
        private readonly AudioUnit _unit;
        private Channel _instance;
        internal bool FreeOnStop = false;
        internal bool InUse = true;

        public SoundInstance(AudioUnit unit, Sound sound) {
            _unit = unit;
            _sound = sound;
            _instance = sound.Effect.Play();
            // Console.WriteLine($"Created SoundInstance {name}");
        }

        public bool Playing => _instance.IsPlaying;

        public bool Looping {
            get => _instance.Looping;
            set => _instance.Looping = value;
        }

        public float Volume {
            get => _instance.Volume;
            set => _instance.Volume = value;
        }

        public float Pitch {
            get => _instance.Pitch;
            set => _instance.Pitch = value;
        }

        public float LowPass {
            get => _instance.LowPass;
            set => _instance.LowPass = value;
        }

        public void Pause() {
            _instance.Pause();
        }

        public void Resume() {
            _instance.Resume();
        }

        public void Stop() {
            _instance.Stop();
        }

        public void Free() {
            // Console.WriteLine($"Freed {Name}");
            _instance.Stop();
            InUse = false;
        }

        public void PlayOnce() { }

        public void Reset() {
            Volume = 1;
            Pitch = 0;
            Looping = false;
        }

        public void Update() {
            if(Playing) _sound.Access();

            if(!Playing && !_instance.Paused) Free();
        }
    }
}