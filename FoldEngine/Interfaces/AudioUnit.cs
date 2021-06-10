using System.Collections.Generic;
using FoldEngine.Audio;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Woofer;

namespace FoldEngine.Interfaces {
    public class AudioUnit {
        public ContentManager _content;
        
        private Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();
        
        private readonly List<SoundInstance> _instances = new List<SoundInstance>();

        public AudioUnit() {
        }

        public SoundInstance CreateInstance(string identifier) {
            foreach(SoundInstance instance in _instances) {
                if(!instance.InUse && instance.Name == identifier) {
                    instance.Reset();
                    instance.InUse = true;
                    return instance;
                }
            }
            var newInstance = new SoundInstance(this, _soundEffects[identifier].CreateInstance(), identifier);
            newInstance.InUse = true;
            _instances.Add(newInstance);
            
            return newInstance;
        }

        public void Play(string identifier) {
            _soundEffects[identifier].Play();
        }

        public void Load(string name) {
            _soundEffects[name] = _content.Load<SoundEffect>(name);
        }

        public void Update() {
            foreach(SoundInstance instance in _instances) {
                instance.Update();
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