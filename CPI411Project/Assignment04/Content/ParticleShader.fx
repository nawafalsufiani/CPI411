float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseCamera; // billboard rotation
float4x4 WorldInverseTranspose;

texture2D Texture;
bool UseTexture;

float3 LightPosition;
float3 CameraPosition;

float4 AmbientColor;
float AmbientIntensity;

float4 DiffuseColor;
float DiffuseIntensity;

float4 SpecularColor;
float SpecularIntensity;
float Shininess;

sampler ParticleSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 ParticlePosition : POSITION1;
    float4 ParticleParameter : POSITION2; // x = Age, y = MaxAge
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
    float3 WorldPosition : TEXCOORD1;
    float3 Normal : TEXCOORD2;
};

VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 billboardPosition = mul(input.Position, InverseCamera);

    float size = max(0.5f, sqrt(max(input.ParticleParameter.x, 0.0f)));
    billboardPosition.xyz *= size;
    billboardPosition += input.ParticlePosition;

    float4 worldPosition = mul(billboardPosition, World);

    output.Position = mul(mul(worldPosition, View), Projection);
    output.TexCoord = input.TexCoord;
    output.WorldPosition = worldPosition.xyz;

    // fake normal for billboard facing camera
    output.Normal = normalize(mul(float4(0, 0, 1, 0), InverseCamera).xyz);

    float maxAge = max(input.ParticleParameter.y, 0.0001f);
    float alpha = saturate(1.0f - (input.ParticleParameter.x / maxAge));
    output.Color = float4(1.0f, 1.0f, 1.0f, alpha);

    return output;
}

float4 ParticlePixelShader(VertexShaderOutput input) : COLOR0
{
    float4 color;

    if (UseTexture)
    {
        color = tex2D(ParticleSampler, input.TexCoord);
    }
    else
    {
        float3 N = normalize(mul(float4(input.Normal, 0.0f), WorldInverseTranspose).xyz);
        float3 L = normalize(LightPosition - input.WorldPosition);
        float3 V = normalize(CameraPosition - input.WorldPosition);
        float3 R = reflect(-L, N);

        float4 ambient = AmbientColor * AmbientIntensity;
        float4 diffuse = DiffuseColor * DiffuseIntensity * max(dot(N, L), 0);
        float4 specular = SpecularColor * SpecularIntensity *
                          pow(max(dot(V, R), 0), Shininess);

        color = saturate(ambient + diffuse + specular);
        color.a = 1.0f;
    }

    color *= input.Color;
    return color;
}

technique particle
{
    pass P0
    {
        VertexShader = compile vs_4_0 ParticleVertexShader();
        PixelShader = compile ps_4_0 ParticlePixelShader();
    }
}