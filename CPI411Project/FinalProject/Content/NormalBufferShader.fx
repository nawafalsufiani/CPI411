float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 NormalW : TEXCOORD0;
};

VertexShaderOutput NormalVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);

    output.NormalW = normalize(mul(float4(input.Normal, 0.0f), WorldInverseTranspose).xyz);

    return output;
}

float4 NormalPS(VertexShaderOutput input) : COLOR0
{
    float3 n = normalize(input.NormalW);
    n = n * 0.5f + 0.5f;

    return float4(n, 1.0f);
}

technique NormalTechnique
{
    pass P0
    {
        VertexShader = compile vs_4_0 NormalVS();
        PixelShader = compile ps_4_0 NormalPS();
    }
}