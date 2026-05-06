float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
};

VertexOutput VSMain(VertexInput input)
{
    VertexOutput output;

    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);

    output.Normal = normalize(mul(input.Normal, (float3x3) World));

    return output;
}

float4 PSMain(VertexOutput input) : COLOR0
{
    float3 lightDir = normalize(float3(1, -1, -1));
    float lightAmount = saturate(dot(normalize(input.Normal), -lightDir));

    return float4(lightAmount, lightAmount, lightAmount, 1.0);
}

technique Basic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0_level_9_1 VSMain();
        PixelShader = compile ps_4_0_level_9_1 PSMain();
    }
}