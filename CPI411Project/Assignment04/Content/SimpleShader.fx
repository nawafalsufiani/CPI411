
//shader world,view,projection
float4x4  World;
float4x4  View;
float4x4  Projection;
float4x4  WorldInverseTranspose;
//colors and light
//float 4 because  red,gren,bluew, alpha
float4 AmbientColor;
//color intesnty
float  AmbientIntensity;
//diffuse  L direction
float3 DiffuseLightDirection;
// diffuse light
float4 DiffuseColor;
float  DiffuseIntensity;

//float camera location and light location
float3 CameraPosition;
float3 LightPosition;
//specular, shininess, specular color and intensity
float  Shininess;
float4 SpecularColor;
float  SpecularIntensity = 1;


float4 LightColor;
float LightIntensity;

//input and output structure for vertex shader
/*
  it worksas the monogame send buffer to HLSL as input vertixshader
the method VS take the input from buffer using algorithm returning the an output which will be the input in the
pexil shader method.

thern the pexil shader methd use the formula to calculate the color.
each shader has it;s own formulas. But same concept. The goal output color to the GPU.

*/

struct VertexShaderInput
{
	float4 Position: POSITION;
	float4 Normal: NORMAL;
};



struct PhongVertexShaderOutput
{
	float4 Position: POSITION;
	float4 Normal: TEXCOORD0;
	float4 WorldPosition: TEXCOORD1;
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


PhongVertexShaderOutput PhongBlinnVertex(VertexShaderInput input) 
{
	PhongVertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = input.Normal;

	return output;
}

PhongVertexShaderOutput SchlickVertex(VertexShaderInput input) 
{
	PhongVertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = input.Normal;

	return output;
}

PhongVertexShaderOutput HalfLifeVertex(VertexShaderInput input) 
{
	PhongVertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = input.Normal;

	return output;
}

//return the color to GPU
float4 PhongPixel(PhongVertexShaderOutput input):COLOR0
{
	float3 Light = normalize(LightPosition);
	float3 camP = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 Normal = mul(input.Normal, WorldInverseTranspose).xyz;
    float3 Reflect = reflect(-Light, Normal);
	float4 ambient = AmbientColor * AmbientIntensity;
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(Normal, Light));
    float4 specular = pow(max(0, dot(camP, Reflect)), Shininess) * SpecularColor * SpecularIntensity;
	float4 color = saturate(ambient + diffuse + specular);
	color.a = 1;
	return color;
}




technique Phong{
	pass Pass1 {
		VertexShader = compile vs_4_0 PhongVertex();
		PixelShader = compile ps_4_0 PhongPixel();
	}
}


