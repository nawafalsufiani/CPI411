float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;
Texture2D decalMap;
TextureCube environmentMap;

float3 DiffuseLightDirection;

float4 AmbientColor;
float AmbientIntensity;
float4 DiffuseColor;
float DiffuseIntensity;

float EtaRatio;
float Reflectivity;
float Shininess;
float4 SpecularColor;
float SpecularIntensity = 1;

float3 FresnelEtaRatio;
float FresnelPower;
float FresnelScale;
float FresnelBias;



samplerCUBE SkyBoxSampler = sampler_state
{
	texture = <environmentMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VSInput
{
	float4 Position : SV_Position0;
	float4 normal : NORMAL0;
};

struct VSOutput
{
	float4 Position: POSITION0;
	float2 texCoord: TEXCOORD0;
	float3 R: TEXCOORD1;
};


sampler tsampler1 = sampler_state
{
    texture = <decalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};



struct FVSOutput
{
	float4 Position:  POSITION0;
	float reflectionFactor : COLOR;
	float3 R: TEXCOORD0;
	float3 TRed: TEXCOORD1;
	float3 TGreen: TEXCOORD2;
	float3 TBlue: TEXCOORD3;
};

VSOutput MainVS(VSInput input)
{
	VSOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projPosition = mul(viewPosition, Projection);
	output.Position = projPosition;

	float3 N = normalize(mul(input.normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = reflect(I, N);
	

	return output;
}

float4 MainPX(VSOutput input) : COLOR0
{
	float4 reflectedColor = texCUBE(SkyBoxSampler, input.R);
	float4 decalColor = tex2D(tsampler1, input.texCoord);
	return lerp(decalColor, reflectedColor, Reflectivity);
}

VSOutput RefractionVertexShaderFunction(VSInput input)
{
	VSOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projPosition = mul(viewPosition, Projection);
	output.Position = projPosition;

	float3 N = normalize(mul(input.normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = refract(I, N, EtaRatio);
	

	return output;
}

float4 RefractionPixelShaderFunction(VSOutput input) : COLOR0
{
	float4 refractedColor = texCUBE(SkyBoxSampler, input.R);
	float4 decalColor = tex2D(tsampler1, input.texCoord);
	return lerp(decalColor, refractedColor, 0.5);
}

FVSOutput DispersionVertexShaderFunction(VSInput input)
{
	FVSOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projPosition = mul(viewPosition, Projection);
	output.Position = projPosition;

	float3 N = normalize(mul(input.normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = 0;
	output.TRed = refract(I, N, FresnelEtaRatio.x);
	output.TGreen = refract(I, N, FresnelEtaRatio.y);
	output.TBlue = refract(I, N, FresnelEtaRatio.z);
	output.reflectionFactor = 0;

	return output;
}

float4 DispersionPixelShaderFunction(FVSOutput input) :COLOR0
{
	float4 refractedColor;
	refractedColor.r = texCUBE(SkyBoxSampler, input.TRed).r;
	refractedColor.g = texCUBE(SkyBoxSampler, input.TGreen).g;
	refractedColor.b = texCUBE(SkyBoxSampler, input.TBlue).b;
	refractedColor.a = 1;

	return refractedColor;
}

FVSOutput FresnelVertexShaderFunction(VSInput input) 
{
	FVSOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projPosition = mul(viewPosition, Projection);
	output.Position = projPosition;

	float3 N = normalize(mul(input.normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = reflect(I, N);
	output.TRed = refract(I, N, FresnelEtaRatio.x);
	output.TGreen = refract(I, N, FresnelEtaRatio.y);
	output.TBlue = refract(I, N, FresnelEtaRatio.z);
	output.reflectionFactor = FresnelBias + FresnelScale * pow(1 + dot(I, N), FresnelPower);
	

	return output;
}

float4 FresnelPixelShaderFunction(FVSOutput input) :COLOR0
{
	float4 reflectedColor = texCUBE(SkyBoxSampler, input.R);
	float4 refractedColor;
	refractedColor.r = texCUBE(SkyBoxSampler, input.TRed).r;
	refractedColor.g = texCUBE(SkyBoxSampler, input.TGreen).g;
	refractedColor.b = texCUBE(SkyBoxSampler, input.TBlue).b;
	refractedColor.a = 1;

	return lerp(refractedColor, reflectedColor, input.reflectionFactor);
	
}

technique Reflection
{
	pass Pass1 {
		VertexShader = compile vs_4_0 MainVS();
		PixelShader = compile ps_4_0 MainPX();
	}
}

technique Refraction
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 RefractionVertexShaderFunction();
        PixelShader = compile ps_4_0 RefractionPixelShaderFunction();
    }
}

technique Dispersion
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DispersionVertexShaderFunction();
        PixelShader = compile ps_4_0 DispersionPixelShaderFunction();
    }
}

technique Fresnel
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 FresnelVertexShaderFunction();
        PixelShader = compile ps_4_0 FresnelPixelShaderFunction();
    }
}