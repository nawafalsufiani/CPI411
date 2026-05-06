float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;

texture decalMap;
texture environmentMap;

sampler tsampler1 = sampler_state
{
    texture = <decalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

samplerCUBE SkyBoxSampler = sampler_state
{
    texture = <environmentMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
    float3 R : TEXCOORD1;
};

VSOutput MainVS(VSInput input)
{
    VSOutput output;

    float4 worldPos = mul(input.Position, World);

    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);

    output.TexCoord = input.TexCoord;

    float3 N = normalize(mul(float4(input.Normal, 0), WorldInverseTranspose).xyz);
    float3 I = normalize(worldPos.xyz - CameraPosition);

    output.R = reflect(I, N);

    return output;
}

float4 MainPS(VSOutput input) : SV_Target
{
    float4 reflectedColor = texCUBE(SkyBoxSampler, input.R);
    float4 decalColor = tex2D(tsampler1, input.TexCoord);

    return lerp(decalColor, reflectedColor, 0.5);
}

technique ReflectionTechnique
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MainPS();
    }
}