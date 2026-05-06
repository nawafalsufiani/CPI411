
/*



this shader.fx implements LambertPixel.

We get the main parameter from the CPU side, including the world matrix, view matrix, projection matrix, and the inverse transpose of the world matrix for normal transformation.
We also get the light parameters, including the ambient color and intensity, diffuse color and intensity, and the light position.
Then we define the vertex shader input structure, which includes the position and normal of the vertex.
In the vertex shader, we transform the vertex position to world space, then to view space, and finally to projection space. We also transform the normal vector using the inverse transpose of the world matrix to ensure it is correctly transformed for lighting calculations.
In the pixel shader, we calculate the ambient and diffuse lighting based on the normal and light direction. We then combine these to get the final color of the pixel, which is returned as the output.
 Finally, we define a technique that uses the vertex and pixel shaders we just implemented.
It will render the 3D model which finish the first step in the Lab probject.


*/
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;

float4 DiffuseColor;
float DiffuseIntensity;

float3 LightPosition;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
};

struct LambertVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
};

LambertVertexShaderOutput LambertVertex(VertexShaderInput input)
{
    LambertVertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);

    output.Position = mul(mul(worldPosition, View), Projection);
    output.WorldPosition = worldPosition;

    output.Normal = normalize(mul(input.Normal, (float3x3) WorldInverseTranspose));

    return output;
}

float4 LambertPixel(LambertVertexShaderOutput input) : COLOR0
{
    float3 N = normalize(input.Normal);
    float3 L = normalize(LightPosition - input.WorldPosition.xyz);

    float4 ambient = AmbientColor * AmbientIntensity;

    float NdotL = max(dot(N, L), 0.0f);
    float4 diffuse = DiffuseColor * DiffuseIntensity * NdotL;

    float4 finalColor = saturate(ambient + diffuse);
    finalColor.a = 1.0f;

    return finalColor;
}

technique shader
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 LambertVertex();
        PixelShader = compile ps_4_0 LambertPixel();
    }
}