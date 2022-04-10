using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    public class EffectManager {
        //TODO make this a resource.
        //See: https://community.monogame.net/t/solved-is-it-possible-to-directly-load-a-effect-from-a-class-files-string/10486
        private readonly Dictionary<string, Effect> _effects = new Dictionary<string, Effect>();

        private readonly ContentManager _content;

        public EffectManager(ContentManager content) {
            _content = content;
        }

        public Effect this[string name] {
            get => _effects.ContainsKey(name) ? _effects[name] : null;
            set => _effects[name] = value;
        }

        public Effect LoadEffect(string name) {
            return _effects[name] = _content.Load<Effect>("Effects/" + name);
        }
    }
}