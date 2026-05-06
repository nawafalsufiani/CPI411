
//Lambert


float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;

float3 LightPosition; 

float4 DiffuseColor;
float DiffuseIntensity;

struct VertexInput
{
    float4 Position : POSITION;
    float4 Normal : NORMAL;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
};

VertexOutput VS(VertexInput input)
{
    VertexOutput output;

  
    float4 worldPos = mul(input.Position, World);
    output.WorldPos = worldPos.xyz;


    output.Position = mul(mul(worldPos, View), Projection);

    
    float3 normal = mul(input.Normal, WorldInverseTranspose).xyz;
    output.Normal = normal;

    return output;
}

float4 PS(VertexOutput input) : SV_Target
{
   
    float3 n = normalize(input.Normal);
    float3 lightDir = normalize(LightPosition - input.WorldPos);

    
    float lightIntensity = saturate(dot(n, lightDir));

    float4 diffuse = DiffuseColor * DiffuseIntensity * lightIntensity;
    float4 ambient = AmbientColor * AmbientIntensity;

    return saturate(diffuse + ambient);
}

technique MyTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VS();
        PixelShader = compile ps_4_0 PS();
    }
}