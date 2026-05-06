float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 ClipPos : TEXCOORD0;
};

VertexShaderOutput DepthVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    float4 clipPos = mul(viewPos, Projection);

    output.Position = clipPos;
    output.ClipPos = clipPos;

    return output;
}

float4 DepthPS(VertexShaderOutput input) : COLOR0
{
    float depth = input.ClipPos.z / input.ClipPos.w;
    depth = depth * 0.5f + 0.5f;

    return float4(depth, depth, depth, 1.0f);
}

technique DepthTechnique
{
    pass P0
    {
        VertexShader = compile vs_4_0 DepthVS();
        PixelShader = compile ps_4_0 DepthPS();
    }
}