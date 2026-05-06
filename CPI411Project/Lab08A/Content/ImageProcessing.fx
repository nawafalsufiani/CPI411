float4x4 MatrixTransform;
float imageWidth;
float imageHeight;

texture2D modelTexture;
texture2D filterTexture;
float offset;

sampler TextureSampler : register(s0) = sampler_state
{
    Texture = <modelTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler FilterSampler : register(s1) = sampler_state
{
    Texture = <filterTexture>;
    AddressU = WRAP;
    AddressV = WRAP;
};

struct VS_Output
{
    float4 Pos : SV_POSITION;
    float2 UV0 : TEXCOORD0;
    float4 UV1 : TEXCOORD1;
};

VS_Output vtsSh(float4 inPos : POSITION, float2 inTex : TEXCOORD0)
{
    VS_Output Out;
    Out.Pos = mul(inPos, MatrixTransform);
    Out.UV0 = inTex;
    Out.UV1 = float4(3.0 / imageWidth, 0, 0, 3.0 / imageHeight);
    return Out;
}

float4 pslSh(VS_Output In) : SV_Target
{
    float4 tex = tex2D(TextureSampler, In.UV0);

    tex.rgb = ceil(tex.rgb * 8.0) / 8.0;

    return tex;
}

technique ImageProcessing
{
    pass Pass1
    {
        VertexShader = compile vs_4_0_level_9_1 vtsSh();
        PixelShader = compile ps_4_0_level_9_1 pslSh();
    }
}