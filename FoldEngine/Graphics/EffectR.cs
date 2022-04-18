using System;
using FoldEngine.IO;
using FoldEngine.Resources;
using Microsoft.Xna.Framework.Graphics;

namespace FoldEngine.Graphics {
    [Resource(directoryName: "effects", extensions: "fx")]
    public class EffectR : Resource {
        public Effect Effect { get; private set; }

        public override bool CanSerialize => false;

        public override void DeserializeResource(string path) {
            try {
                Effect = EffectManager.Compile(Data.In.Path(path));
                Console.WriteLine("effect loaded");
            } catch(Exception x) {
                Console.WriteLine($"Error compiling shader {Identifier}: {x.Message}");
            }
        }
    }
}