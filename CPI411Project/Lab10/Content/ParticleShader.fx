
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseCamera;

texture2D Texture;

sampler ParticleSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 ParticlePosition : POSITION1;
    float4 ParticleParameter : POSITION2; 
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, InverseCamera);

 
    worldPosition.xyz = worldPosition.xyz * sqrt(max(input.ParticleParameter.x, 0.001f));

    worldPosition += input.ParticlePosition;

    output.Position = mul(worldPosition, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);

    output.TexCoord = input.TexCoord;

    float fade = 1.0f - (input.ParticleParameter.x / input.ParticleParameter.y);
    output.Color = float4(fade, fade, fade, fade);

    return output;
}

float4 ParticlePixelShader(VertexShaderOutput input) : COLOR0
{
    float4 color = tex2D(ParticleSampler, input.TexCoord);
    color *= input.Color;
    return color;
}

technique BasicTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_4_0_level_9_1 ParticleVertexShader();
        PixelShader = compile ps_4_0_level_9_1 ParticlePixelShader();
    }
}