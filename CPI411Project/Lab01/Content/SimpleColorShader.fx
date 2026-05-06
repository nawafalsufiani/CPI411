#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Handout style
texture MyTexture;

sampler mySampler = sampler_state
{
    Texture = <MyTexture>;
};

struct VertexPositionTexture
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
};

VSOutput MyVertexShader(VertexPositionTexture input)
{
    VSOutput output;
    output.Position = input.Position; // already clip space
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}

float4 MyPixelShader(VSOutput input) : COLOR0
{
    return tex2D(mySampler, input.TextureCoordinate);
}

technique MyTechnique
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL MyVertexShader();
        PixelShader  = compile PS_SHADERMODEL MyPixelShader();
    }
}
