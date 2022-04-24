using System.Collections.Generic;
using FoldEngine.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace FoldEngine.Graphics {
    public static class EffectManager {
        
        private static readonly EffectImporter Importer = new EffectImporter();
        private static readonly EffectProcessor Processor = new EffectProcessor();
        private static readonly PipelineManager Pm = new PipelineManager(string.Empty, string.Empty, string.Empty);
        private static readonly PipelineProcessorContext Ppc = new PipelineProcessorContext(Pm, new PipelineBuildEvent());

        static EffectManager() {
            Pm.Profile = FoldGame.Game.Graphics.GraphicsProfile;
            Pm.Platform = FoldGame.Game.Core.TargetPlatform;
        }
        
        public static Effect Compile(string path, out string source) {
            EffectContent content = Importer.Import(path, null);
            source = content.EffectCode;
            CompiledEffectContent cecontent = Processor.Process(content, Ppc);
            return new Effect(FoldGame.Game.GraphicsDevice, cecontent.GetEffectCode());
        }
    }
}