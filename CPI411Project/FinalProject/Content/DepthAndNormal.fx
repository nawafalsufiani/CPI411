float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Position2D : TEXCOORD0;
    float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Position2D : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};

VertexShaderOutput DepthMapVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(mul(mul(input.Position, World), View), Projection);
    output.Position2D = output.Position;
    output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);

    return output;
}

float4 DepthMapPixelShader(VertexShaderOutput input) : COLOR0
{
    float4 projTexCoord = input.Position2D / input.Position2D.w;
    projTexCoord.xy = 0.5f * projTexCoord.xy + float2(0.5f, 0.5f);
    projTexCoord.y = 1.0f - projTexCoord.y;

    float depth = 1.0f - projTexCoord.z;

    float4 color;
    color.rgb = normalize(input.Normal.xyz) * 0.5f + 0.5f;
    color.a = (depth > 0.0f) ? depth : 0.0f;

    return color;
}

technique DepthAndNormal
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_1 DepthMapVertexShader();
        PixelShader = compile ps_4_0_level_9_1 DepthMapPixelShader();
    }
}

