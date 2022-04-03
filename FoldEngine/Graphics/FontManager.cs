using System;
using System.Collections.Generic;
using System.IO;
using FoldEngine.Interfaces;
using FoldEngine.IO;
using FoldEngine.Resources;
using FoldEngine.Text;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Shard.SampleLanguages;
using Shard.Scripts;
using Shard.Scripts.CompilerModules;
using Shard.Scripts.Requests;
using Shard.Scripts.Types;
using Shard.Scripts.Types.Functions;

namespace FoldEngine.Graphics {
    public class FontManager {
        private IGameCore _core;
        private Dictionary<string, IFont> _fonts = new Dictionary<string, IFont>();

        public FontManager(IGameCore core) {
            _core = core;
        }
        
        public IFont this[string name]
        {
            get
            {
                if (name == null) return null;
                if (!_fonts.ContainsKey(name)) {
                    Console.WriteLine("[ERROR]: Attempted to retrieve unknown font '" + name + "'");
                    return null;
                }
                return _fonts[name];
            }
            private set => _fonts[name] = value;
        }

        public BitmapFont LoadFont(FontDefinition definition) {
            BitmapFont bitmapFont = new BitmapFont();
            _fonts[definition.Identifier] = bitmapFont;

            JObject root = definition.Root;
            
            bitmapFont.LineHeight = root["line_height"]?.Value<int>() ?? bitmapFont.LineHeight;
            bitmapFont.DefaultSize = root["default_size"]?.Value<float>() ?? 9;
            string setName = root["font_set"]?.Value<string>();
            if(setName != null) {
                if(!_fonts.ContainsKey(setName)) {
                    _fonts[setName] = new FontSet();
                }

                if(_fonts[setName] is FontSet fontSet) {
                    fontSet.AddFont(bitmapFont, bitmapFont.DefaultSize);
                } else {
                    throw new InvalidOperationException($"Cannot create a font set named {setName}: another non-set font with that name already exists");
                }
            }

            if(root["characters"] is JArray charsArr) {
                foreach(JObject rawCharEntry in charsArr) {
                    string sourceName = (string) rawCharEntry["source"];
                    
                    var sourceIdentifier = new ResourceIdentifier($"font/{definition.Identifier}/{sourceName}");
                    
                    _core.Resources.Load<TextureR>(ref sourceIdentifier, t => {
                        t.NeverUnload();
                        
                        int sourceIndex = bitmapFont.AddTextureSource(sourceName, (ITexture) t);

                        FontContextInfo.SourceTex = bitmapFont.TextureSources[sourceIndex].Source;

                        JToken charOrRange = rawCharEntry["char"];
                        char start;
                        char end;
                        if(charOrRange is JArray rawRange) {
                            start = ((string) rawRange[0])[0];
                            end = ((string) rawRange[1])[0];
                        } if(charOrRange is JObject rawRangeObj) {
                            start = ((string) rawRangeObj["from"])[0];
                            end = ((string) rawRangeObj["to"])[0];
                        } else {
                            start = end = ((string) charOrRange)[0];
                        }
                        FontContextInfo.RangeFrom = start;
                        FontContextInfo.RangeTo = end;

                        var heightEvaluator = EvaluatorInt(rawCharEntry["height"]);
                        var ascentEvaluator = EvaluatorInt(rawCharEntry["ascent"]);
                        var widthEvaluator = EvaluatorInt(rawCharEntry["width"]);
                        var advancementEvaluator = EvaluatorInt(rawCharEntry["advancement"]);

                        var rawUVArr = rawCharEntry["uv"] as JArray;
                        
                        var uvXEvaluator = EvaluatorInt(rawUVArr[0]);
                        var uvYEvaluator = EvaluatorInt(rawUVArr[1]);

                        for(char c = start; c <= end; c++) {
                            GlyphInfo glyphInfo = new GlyphInfo() {NotNull = true};
                            glyphInfo.SourceIndex = sourceIndex;

                            glyphInfo.Height = glyphInfo.Source.Height = FontContextInfo.Height = heightEvaluator(c);
                            glyphInfo.Ascent = ascentEvaluator(c);
                            
                            glyphInfo.Source.X = FontContextInfo.UVX = uvXEvaluator(c);
                            glyphInfo.Source.Y = FontContextInfo.UVY = uvYEvaluator(c);
                            
                            glyphInfo.Width = glyphInfo.Source.Width = widthEvaluator(c);
                            glyphInfo.Advancement = advancementEvaluator(c);

                            bitmapFont[c] = glyphInfo;
                        }
                        
                        FontContextInfo.Clear();
                    });
                    
                }
            }
            // Console.WriteLine(root);
            return bitmapFont;
        }

        private static Func<char, int> EvaluatorInt(JToken token) {
            switch(token.Type) {
                case JTokenType.Integer: return _ => token.Value<int>();
                case JTokenType.String: {
                    string raw = token.Value<string>();

                    FontContextCompiler.OnEnd = sw => sw.Write<Shard.Scripts.Instructions.Convert.I32>();
                    Script s = FontContextCompiler.Compile(raw);

                    return c => {
                        FontContextInfo.Code = c;
                        s.Run();
                        return s.EvaluationStack.PeekInt32();
                    };
                }
                default: {
                    throw new ArgumentException("Invalid token type: " + token.Type);
                }
            }
        }

        private static Func<char, float> EvaluatorFloat(JToken token) {
            switch(token.Type) {
                case JTokenType.Integer: 
                case JTokenType.Float: return _ => token.Value<float>();
                case JTokenType.String: {
                    string raw = token.Value<string>();

                    FontContextCompiler.OnEnd = sw => sw.Write<Shard.Scripts.Instructions.Convert.F32>();
                    Script s = FontContextCompiler.Compile(raw);

                    return c => {
                        FontContextInfo.Code = c;
                        s.Run();
                        return s.EvaluationStack.PeekFloat32();
                    };
                }
                default: {
                    throw new ArgumentException("Invalid token type: " + token.Type);
                }
            }
        }

        private static BasicExpressionLanguage.Compiler FontContextCompiler;

        static FontManager() {
            FontContextCompiler = new BasicExpressionLanguage.Compiler();
            FontContextCompiler
                .AddRequestIdentifier<CodeRequest>(Primitives.Int32)
                .AddRequestIdentifier<RangeFromRequest>(Primitives.Int32)
                .AddRequestIdentifier<RangeToRequest>(Primitives.Int32);

            FontContextCompiler
                .AddRequestFunction<AutoWidthRequest>(FormalParameter.CreateList(Primitives.Int32), Primitives.Int32);

            FontContextCompiler
                .ImportModule<ArithmeticModule>()
                .ImportModule<BitwiseModule>()
                .ImportModule<LogicalModule>()
                .ImportModule<StringModule>()
                .ImportModule<ConditionalOperatorModule>()
                ;
        }

        public static class FontContextInfo {
            public static int Code;
            public static int RangeFrom;
            public static int RangeTo;
            
            public static int Height;
            public static int UVX;
            public static int UVY;
            
            public static Texture2D SourceTex;
            public static int[] SourceData;

            public static void Clear() {
                Code = RangeFrom = RangeTo = 0;
                Height = UVX = UVY = 0;
                SourceTex = null;
                SourceData = null;
            }
        }
        
        [RequestHandler("code")]
        public class CodeRequest : RequestHandler {
            public override void Perform(Script script) {
                script.EvaluationStack.Push(FontContextInfo.Code);
            }
        }
        
        [RequestHandler("range.from")]
        public class RangeFromRequest : RequestHandler {
            public override void Perform(Script script) {
                script.EvaluationStack.Push(FontContextInfo.RangeFrom);
            }
        }
        
        [RequestHandler("range.to")]
        public class RangeToRequest : RequestHandler {
            public override void Perform(Script script) {
                script.EvaluationStack.Push(FontContextInfo.RangeTo);
            }
        }
        
        [RequestHandler("auto.width")]
        public class AutoWidthRequest : RequestHandler {
            public override void Perform(Script script) {
                int maxWidth = script.EvaluationStack.PopInt32();
                
                if(FontContextInfo.SourceData == null) {
                    FontContextInfo.SourceData =
                        new int[FontContextInfo.SourceTex.Width * FontContextInfo.SourceTex.Height];
                    FontContextInfo.SourceTex.GetData<int>(FontContextInfo.SourceData);
                }

                for(int x = FontContextInfo.UVX + maxWidth - 1; x >= FontContextInfo.UVX; x--) {
                    for(int y = FontContextInfo.UVY; y < FontContextInfo.UVY + FontContextInfo.Height; y++) {
                        uint pixelColor = (uint)FontContextInfo.SourceData[y * FontContextInfo.SourceTex.Width + x];
                        uint alpha = pixelColor & 0xFF;
                        if(alpha > 0) {
                            script.EvaluationStack.Push(x - FontContextInfo.UVX + 1);
                            return;
                        }
                    }
                }
                
                script.EvaluationStack.Push(0);
            }
        }

        public void LoadAll() {
            foreach(string fontName in _core.ResourceIndex.GetIdentifiers<FontDefinition>()) {
                var identifier = new ResourceIdentifier(fontName);
                _core.Resources.Load<FontDefinition>(ref identifier, d => {
                    LoadFont((FontDefinition) d);
                });
            }
        }
    }
}