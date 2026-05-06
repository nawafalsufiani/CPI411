 
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;
float3 LightPosition;

float4 DiffuseColor;
float DiffuseIntensity;
float4 SpecularColor;
float SpecularIntensity;
float Shininess;
float EtaRatio;

float height = 2.0f;
float2 UVScale = float2(1.0f, 1.0f);

 
int MipMap = 1;
bool useSelfShadowing = false;
bool normalizeTangent = false;
bool normalizeNormalMap = false;

texture normalMap;
texture decalMap;
texture environmentMap;

 
sampler NormalSamplerMip = sampler_state
{
    texture = <normalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};
 

sampler NormalSamplerNoMip = sampler_state
{
    texture = <normalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = NONE;
    AddressU = Clamp;
    AddressV = Clamp;
};

sampler DecalSampler = sampler_state
{
    texture = <decalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

samplerCUBE SkyBoxSampler = sampler_state
{
    texture = <environmentMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float4 Tangent : TANGENT0;
    float4 Binormal : BINORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float3 Tangent : TEXCOORD1;
    float3 Binormal : TEXCOORD2;
    float2 TexCoord : TEXCOORD3;
    float3 Position3D : TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
    output.Tangent = mul(input.Tangent, WorldInverseTranspose).xyz;
    output.Binormal = mul(input.Binormal, WorldInverseTranspose).xyz;

    if (normalizeTangent)
    {
        output.Tangent = normalize(output.Tangent);
        output.Binormal = normalize(output.Binormal);
    }

    output.Position3D = worldPosition.xyz;
    output.TexCoord = input.TexCoord * UVScale;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 L = normalize(LightPosition - input.Position3D);
    float3 V = normalize(CameraPosition - input.Position3D);
    float3 N = normalize(input.Normal);
    float3 T = input.Tangent;
    float3 B = input.Binormal;
    float3 H = normalize(L + V);

    float3 normalTex;

    
    if (MipMap == 1)
        normalTex = tex2D(NormalSamplerMip, input.TexCoord).xyz;
    else
        normalTex = tex2D(NormalSamplerNoMip, input.TexCoord).xyz;

     
    normalTex = 2.0f * normalTex - 1.0f;

    
    normalTex.x *= (1.0f + 0.2f * (height - 5.0f));
    normalTex.y *= (1.0f + 0.2f * (height - 5.0f));
    normalTex.z *= (1.0f + 0.2f * (5.0f - height));

    if (normalizeNormalMap)
        normalTex = normalize(normalTex);

    float3x3 TangentToWorld;
    TangentToWorld[0] = normalize(T);
    TangentToWorld[1] = normalize(B);
    TangentToWorld[2] = normalize(N);

    float3 bumpNormal = normalize(mul(normalTex, TangentToWorld));

    float ndotl = saturate(dot(bumpNormal, L));

     
    if (useSelfShadowing && ndotl <= 0.0f)
        ndotl *= 0.3f;

    
    float3 R = refract(-V, bumpNormal, EtaRatio);

    float4 decalColor = tex2D(DecalSampler, input.TexCoord);
    float4 refractedColor = texCUBE(SkyBoxSampler, R);

    float4 diffuse = DiffuseColor * DiffuseIntensity * ndotl;
    float4 specular = 0.0f;

    if (ndotl > 0.0f)
    {
        specular = SpecularColor * SpecularIntensity *
                   pow(saturate(dot(H, bumpNormal)), Shininess);
    }

    float4 surfaceColor = decalColor * diffuse + specular;
    surfaceColor.a = 1.0f;

    return lerp(surfaceColor, refractedColor, 0.5f);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}