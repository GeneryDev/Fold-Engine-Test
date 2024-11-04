using System.Collections.Generic;
using FmodForFoxes;
using FoldEngine.Audio;
using Sound = FoldEngine.Audio.Sound;

namespace FoldEngine.Interfaces {
    public class AudioUnit {
        IGameCore Core { get; }
        private readonly List<SoundInstance> _instances = new List<SoundInstance>();

        public AudioUnit(IGameCore core)
        {
            this.Core = core;
            FmodManager.Init(core.FoldGame.RuntimeConfig.FmodNativeLibrary, FmodInitMode.Core, "data");
        }

        public SoundInstance CreateInstance(Sound sound) {
            var instance = new SoundInstance(this, sound);
            _instances.Add(instance);

            return instance;
        }

        public void Update() {
            for(int i = 0; i < _instances.Count; i++) {
                SoundInstance instance = _instances[i];
                instance.Update();
                if(!instance.InUse) {
                    _instances.RemoveAt(i);
                    i--;
                }
            }
        }

        public void StopAll() {
            foreach(SoundInstance instance in _instances) instance.Stop();
            _instances.Clear();
        }
    }
}