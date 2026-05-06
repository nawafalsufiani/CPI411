float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;

float4 DiffuseColor;
float DiffuseIntensity;

float3 CameraPosition;
float3 LightPosition;

float Shininess;
float4 SpecularColor;
float SpecularIntensity;

struct VertexShaderInput
{
    float4 Position : POSITION;
    float4 Normal : NORMAL;
};

struct PhongVertexShaderOutput
{
    float4 Position : POSITION;
    float4 Normal : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
};

PhongVertexShaderOutput PhongVertex(VertexShaderInput input)
{
    PhongVertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.WorldPosition = worldPosition;
    output.Normal = input.Normal;
    return output;
}

float4 PhongPixel(PhongVertexShaderOutput input) : COLOR0
{
    float3 Light = normalize(LightPosition - input.WorldPosition.xyz);
    float3 camP = normalize(CameraPosition - input.WorldPosition.xyz);
    float3 Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
    float3 Reflect = reflect(-Light, Normal);

    float4 ambient = AmbientColor * AmbientIntensity;
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(Normal, Light));
    float4 specular = pow(max(0, dot(camP, Reflect)), Shininess) * SpecularColor * SpecularIntensity;

    float4 color = saturate(ambient + diffuse + specular);
    color.a = 1;
    return color;
}

technique Phong
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 PhongVertex();
        PixelShader = compile ps_4_0 PhongPixel();
    }
}