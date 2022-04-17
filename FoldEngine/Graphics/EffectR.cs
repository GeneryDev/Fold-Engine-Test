using System;
using FoldEngine.Resources;
// using Microsoft.Xna.Framework.Content.Pipeline;
// using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
// using Microsoft.Xna.Framework.Content.Pipeline.Processors;
// using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
// using MonoGame.Framework.Content.Pipeline.Builder;

namespace FoldEngine.Graphics {
    [Resource(directoryName: "effects", extensions: "fx")]
    public class EffectR : Resource {
        public Effect Effect { get; private set; }

        public override bool CanSerialize => false;

        public override void DeserializeResource(string path) {
            // EffectImporter importer = new EffectImporter();
            // EffectContent content = importer.Import(Data.In.Path(path), null);
            // EffectProcessor processor = new EffectProcessor();
            // PipelineManager pm = new PipelineManager(string.Empty, string.Empty, string.Empty);
            // PipelineProcessorContext ppc = new PipelineProcessorContext(pm, new PipelineBuildEvent());
            // CompiledEffectContent cecontent = processor.Process(content, ppc);
            // ContentCompiler compiler = new ContentCompiler();
            //
            // Effect = new Effect(FoldGame.Game.GraphicsDevice, cecontent.GetEffectCode());
            Console.WriteLine("effect loaded");
            Effect = new SpriteEffect(FoldGame.Game.GraphicsDevice);
        }
    }
}