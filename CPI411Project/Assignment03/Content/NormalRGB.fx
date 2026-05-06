


float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float2 UVScale = float2(1.0f, 1.0f);

texture normalMap;

sampler NormalSampler = sampler_state
{
    texture = <normalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float4 Tangent : TANGENT0;
    float4 Binormal : BINORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float3 Tangent : TEXCOORD1;
    float3 Binormal : TEXCOORD2;
    float2 TexCoord : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
    output.Tangent = normalize(mul(input.Tangent, WorldInverseTranspose).xyz);
    output.Binormal = normalize(mul(input.Binormal, WorldInverseTranspose).xyz);

    output.TexCoord = input.TexCoord * UVScale;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 N = normalize(input.Normal);
    float3 T = normalize(input.Tangent);
    float3 B = normalize(input.Binormal);

     
    float3 normalTex = tex2D(NormalSampler, input.TexCoord).xyz;

    
    normalTex = 2.0f * normalTex - 1.0f;

    
    float3x3 TangentToWorld;
    TangentToWorld[0] = T;
    TangentToWorld[1] = B;
    TangentToWorld[2] = N;

     
    float3 worldNormal = normalize(mul(normalTex, TangentToWorld));

    
    float3 rgb = 0.5f * (worldNormal + 1.0f);

    return float4(rgb, 1.0f);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
        PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}