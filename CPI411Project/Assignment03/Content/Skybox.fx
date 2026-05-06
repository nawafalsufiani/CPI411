float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

TextureCube SkyBoxTexture;

samplerCUBE SkyBoxSampler = sampler_state
{
    texture = <SkyBoxTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VertexInput
{
    float4 Position : POSITION0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float3 TextureCoordinate : TEXCOORD0;
};

VertexOutput VS(VertexInput input)
{
    VertexOutput output;

    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);

    output.TextureCoordinate = worldPos.xyz - CameraPosition;

    return output;
}

float4 PS(VertexOutput input) : SV_TARGET
{
    return texCUBE(SkyBoxSampler, normalize(input.TextureCoordinate));
}

technique SkyBox
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VS();
        PixelShader = compile ps_4_0 PS();
    }
}