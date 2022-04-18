//#ifdef SM4
//
//// Macros for targetting shader model 4.0 (DX11)
//
//#define TECHNIQUE(name, vsname, psname ) \
//	technique name { pass { VertexShader = compile vs_4_0_level_9_1 vsname (); PixelShader = compile ps_4_0_level_9_1 psname(); } }
//
//#define BEGIN_CONSTANTS     cbuffer Parameters : register(b0) {
//#define MATRIX_CONSTANTS
//#define END_CONSTANTS       };
//
//#define _vs(r)
//#define _ps(r)
//#define _cb(r)
//
//#define DECLARE_TEXTURE(Name, index) \
//    Texture2D<float4> Name : register(t##index); \
//    sampler Name##Sampler : register(s##index)
//
//#define DECLARE_CUBEMAP(Name, index) \
//    TextureCube<float4> Name : register(t##index); \
//    sampler Name##Sampler : register(s##index)
//
//#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
//#define SAMPLE_CUBEMAP(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
//
//
//#else
//
//
//// Macros for targetting shader model 2.0 (DX9)
//
//#define TECHNIQUE(name, vsname, psname ) \
//	technique name { pass { VertexShader = compile vs_2_0 vsname (); PixelShader = compile ps_2_0 psname(); } }
//
//#define BEGIN_CONSTANTS
//#define MATRIX_CONSTANTS
//#define END_CONSTANTS
//
//#define _vs(r)  : register(vs, r)
//#define _ps(r)  : register(ps, r)
//#define _cb(r)
//
//#define DECLARE_TEXTURE(Name, index) \
//    sampler2D Name : register(s##index);
//
//#define DECLARE_CUBEMAP(Name, index) \
//    samplerCUBE Name : register(s##index);
//
//#define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name, texCoord)
//#define SAMPLE_CUBEMAP(Name, texCoord)  texCUBE(Name, texCoord)
//
//
//#endif
//
//DECLARE_TEXTURE(Texture, 0);
//
//
//BEGIN_CONSTANTS
//MATRIX_CONSTANTS
//
//    float4x4 MatrixTransform    _vs(c0) _cb(c0);
//
//END_CONSTANTS
//
//
//struct VSOutput
//{
//	float4 position		: SV_Position;
//	float4 color		: COLOR0;
//    float2 texCoord		: TEXCOORD0;
//};
//
//VSOutput SpriteVertexShader(	float4 position	: POSITION0,
//								float4 color	: COLOR0,
//								float2 texCoord	: TEXCOORD0)
//{
//	VSOutput output;
//    output.position = mul(position, MatrixTransform);
//	output.color = color;
//	output.texCoord = texCoord;
//	return output;
//}
//
//
//float4 SpritePixelShader(VSOutput input) : SV_Target0
//{
//    return SAMPLE_TEXTURE(Texture, input.texCoord) * input.color;
//}
//
//TECHNIQUE( SpriteBatch, SpriteVertexShader, SpritePixelShader );


#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

sampler2D Texture : register(s0);
float4x4 MatrixTransform;
float time;

struct VertexInput {
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 TexCoord : TEXCOORD0;
};
struct PixelInput {
    float4 Position : SV_Position0;
    float4 Color : COLOR0;
    float4 TexCoord : TEXCOORD0;
};

PixelInput SpriteVertexShader(VertexInput v) {
    PixelInput output;

    v.Position[0] += sin(time * 3.14 + sin(v.Position[1] / 31)) * 25;

    output.Position = mul(v.Position, MatrixTransform);
    output.Color = v.Color;
    output.TexCoord = v.TexCoord;
    return output;
}
float4 SpritePixelShader(PixelInput p) : COLOR0 {
    if(p.Position[0] % 2 <= 1) {
        return tex2D(Texture, p.TexCoord) * p.Color;
    } else {
        return float4(1, 0, 0, 1);
    }
}

technique SpriteBatch {
    pass {
        VertexShader = compile VS_SHADERMODEL SpriteVertexShader();
        PixelShader = compile PS_SHADERMODEL SpritePixelShader();
    }
}