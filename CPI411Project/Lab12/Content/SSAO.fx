float offset;
float rad = 0.01f;

texture RandomNormalTexture;
texture DepthAndNormalTexture;

sampler randomMap = sampler_state
{
    Texture = <RandomNormalTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
};

sampler normalMap = sampler_state
{
    Texture = <DepthAndNormalTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT RenderSceneVS(float4 vPos : POSITION0, float2 TexCoord : TEXCOORD0)
{
    VS_OUTPUT Output;
    Output.Position = vPos;

    vPos.xy = sign(vPos.xy);
    Output.TexCoord.x = (vPos.x + 1.0f) * 0.5f;
    Output.TexCoord.y = 1.0f - (vPos.y + 1.0f) * 0.5f;

    return Output;
}

float4 RenderScenePS0(VS_OUTPUT Input) : COLOR0
{
    float3 pSphere[8] =
    {
        float3(0.53812504f, 0.18565957f, -0.43192f),
        float3(0.13790712f, 0.24864247f, 0.44301823f),
        float3(0.33715037f, 0.56794053f, -0.005789503f),
        float3(-0.6999805f, -0.04511441f, -0.0019965635f),
        float3(0.06896307f, -0.15983082f, -0.85477847f),
        float3(0.056099437f, 0.006954967f, -0.1843352f),
        float3(-0.014653638f, 0.14027752f, 0.0762037f),
        float3(0.010019933f, -0.1924225f, -0.034443386f)
    };

    float3 randomNormal =
        normalize(tex2D(randomMap, Input.TexCoord * offset).xyz * 2.0f - 1.0f);

    float depth = tex2D(normalMap, Input.TexCoord).a;
    float3 normal = tex2D(normalMap, Input.TexCoord).xyz * 2.0f - 1.0f;

    float bl = 0.0f;
    float safeDepth = max(depth, 0.0001f);
    float radD = rad / safeDepth;

    for (int i = 0; i < 8; i++)
    {
        float3 ray = radD * reflect(pSphere[i], randomNormal);

        float2 sampleCoord =
            Input.TexCoord + sign(dot(ray, normal)) * ray.xy * float2(1.0f, -1.0f);

        float sampleDepth = tex2D(normalMap, sampleCoord).a;
        float3 occNormal = tex2D(normalMap, sampleCoord).xyz * 2.0f - 1.0f;

        float depthDifference = max(depth - sampleDepth, 0.0f);
        float normDiff = 1.0f - dot(occNormal, normal);

        bl += normDiff * (1.0f - depthDifference);
    }

    float ao = 1.0f - bl / 8.0f;
    return float4(ao, ao, ao, 1.0f);
}

technique SSAO
{
    pass P0
    {
        VertexShader = compile vs_4_0 RenderSceneVS();
        PixelShader = compile ps_4_0 RenderScenePS0();
    }
}