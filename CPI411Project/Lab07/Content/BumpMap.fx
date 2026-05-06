float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;
float3 LightPosition;

float AmbientColor = 0.2f;
float AmbientIntensity = 1.0f;

float4 DiffuseColor;
float DiffuseIntensity;

float4 SpecularColor;
float SpecularIntensity;
float Shininess;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float3 Position3D : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Normal = normalize(mul(float4(input.Normal, 0.0f), WorldInverseTranspose).xyz);
    output.Position3D = worldPosition.xyz;
    output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 L = normalize(LightPosition - input.Position3D);
    float3 V = normalize(CameraPosition - input.Position3D);
    float3 N = normalize(input.Normal);
    float3 H = normalize(L + V);

    float4 ambient = float4(AmbientColor, AmbientColor, AmbientColor, 1.0f) * AmbientIntensity;

    float4 diffuse = DiffuseColor * DiffuseIntensity * saturate(dot(N, L));
    diffuse.a = 1.0f;

    float4 specular = SpecularColor * SpecularIntensity * pow(saturate(dot(N, H)), Shininess);
    specular.a = 1.0f;

    float4 finalColor = ambient + diffuse + specular;
    finalColor.a = 1.0f;

    return saturate(finalColor);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}