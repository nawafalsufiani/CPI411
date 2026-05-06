
float4x4 World; // the world matrix of the object, which transforms the object from model space to world space
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor; // the color of the ambient light ( the color of the light itself )
float AmbientIntensity; // how much strong the ambietnt color is

float4 DiffuseColor;  
float DiffuseIntensity;

float3 LightPosition;

float3 CameraPosition;
float Shininess;
float4 SpecularColor;
float SpecularIntensity;

struct VertexInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0; // Gouraud
    float3 Normal : TEXCOORD0; // Phong
    float4 WorldPosition : TEXCOORD1; // Phong
};

// --------------------
// Gouraud (Per-Vertex)
// --------------------
VertexShaderOutput GouraudVertexShaderFunction(VertexInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.WorldPosition = 0;
    output.Normal = 0;

    float3 N = normalize(mul(float4(input.Normal, 0), WorldInverseTranspose).xyz);
    float3 V = normalize(CameraPosition - worldPosition.xyz);
    float3 L = normalize(LightPosition - worldPosition.xyz);
    float3 R = reflect(-L, N);

    float4 ambient = AmbientColor * AmbientIntensity;
    float4 diffuse = DiffuseColor * DiffuseIntensity * max(0, dot(N, L));
    float4 specular = pow(max(0, dot(V, R)), Shininess) * SpecularColor * SpecularIntensity;

    float4 color = ambient + diffuse + specular;
    color.rgb = min(color.rgb, 1.0); // smoother than saturate clipping
    color.a = 1;

    output.Color = color;
    return output;
}

float4 GouraudPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return input.Color;
}

// --------------------
// Phong (Per-Fragment)
// --------------------
VertexShaderOutput PhongVertexShaderFunction(VertexInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.WorldPosition = worldPosition;
    output.Normal = input.Normal;
    output.Color = 0;

    return output;
}

float4 PhongPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 N = normalize(mul(float4(input.Normal, 0), WorldInverseTranspose).xyz);
    float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
    float3 L = normalize(LightPosition - input.WorldPosition.xyz);
    float3 R = reflect(-L, N);

    float4 ambient = AmbientColor * AmbientIntensity;
    float4 diffuse = DiffuseColor * DiffuseIntensity * max(0, dot(N, L));
    float4 specular = pow(max(0, dot(V, R)), Shininess) * SpecularColor * SpecularIntensity;

    float4 color = ambient + diffuse + specular;
    color.rgb = min(color.rgb, 1.0); // smoother than saturate
    color.a = 1;

    return color;
}

// Technique[0] We use it to show the istruction for Gouraud Shader
technique GouraudTech
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 GouraudVertexShaderFunction();
        PixelShader = compile ps_4_0 GouraudPixelShaderFunction();
    }
}

// Technique[1] We use it to show the istruction for Phong Shader
technique PhongTech
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 PhongVertexShaderFunction();
        PixelShader = compile ps_4_0 PhongPixelShaderFunction();
    }
}
