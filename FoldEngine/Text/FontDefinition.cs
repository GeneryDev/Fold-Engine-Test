﻿using System;
using FoldEngine.IO;
using FoldEngine.Resources;
using Newtonsoft.Json.Linq;

namespace FoldEngine.Text {
    [Resource(directoryName: "font", extensions: "json")]
    public class FontDefinition : Resource {
        public JObject Root;
        
        public override void DeserializeResource(string path) {
            if(!(JObject.Parse(Data.In.ReadString(path))["font"] is JObject root)) {
                throw new FormatException($"Expected \"font\" object in font {Identifier}");
            }

            Root = root;
        }

        public override bool CanSerialize => false;
    }
}