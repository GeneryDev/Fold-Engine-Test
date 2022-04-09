using System.Collections.Generic;
using ChaiFoxes.FMODAudio;
using FoldEngine.Audio;
using FoldEngine.Resources;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Sound = FoldEngine.Audio.Sound;

namespace FoldEngine.Interfaces {
    public class AudioUnit {
        private readonly List<SoundInstance> _instances = new List<SoundInstance>();

        public AudioUnit() {
            FMODManager.Init(FMODMode.Core, "data");
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
            foreach(SoundInstance instance in _instances) {
                instance.Stop();
            }
            _instances.Clear();
        }
    }
}