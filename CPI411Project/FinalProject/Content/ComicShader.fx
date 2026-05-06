// parameters got from CPU

/*

This Comic shader is about how to detect eges by using combination of the normal buffer and depth buffer
And then use those information to draw the edges


After that we use the original rednerd color texture to get the light  and shadow position information to draw the lines

using the hatch lines texture which is in the comic book style
*/



//Where to draw the rendered into the screen
float4x4 MatrixTransform;
//size of the images get from C# (CPU ) side
float imageWidth;
float imageHeight;

// textures from C#  
//the normal texture model 
texture2D modelTexture;
//the normal texure We nee it to detect the edge of the model
texture2D normalTexture;
//the depth texture We nee it to detect the edge of the model
texture2D depthTexture;


// samplers
/*

We use samplers so we can access the textures in the shaders

Each one is stored in the register s0, s1, s2 
*/


sampler TextureSampler : register(s0) = sampler_state
{
    Texture = <modelTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler NormalSampler : register(s1) = sampler_state
{
    Texture = <normalTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler DepthSampler : register(s2) = sampler_state
{
    Texture = <depthTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};


/*
Vertex shader input and output structure

*/
struct VS_Output
{
    float4 Pos : SV_POSITION;
    float2 UV0 : TEXCOORD0;
    float4 UV1 : TEXCOORD1;
};

// vertex shader
VS_Output vtsSh(float4 inPos : POSITION, float2 inTex : TEXCOORD0)
{
    VS_Output Out;

    Out.Pos = mul(inPos, MatrixTransform);
    Out.UV0 = inTex;

    // edge thickness
    //float4(x, y, z, w)  It's about how much moves to calculatw the cordinate  We use only X and W
    //X for detection the edges in Width and W for detection thee Height
    Out.UV1 = float4(4.0 / imageWidth, 0, 0, 4.0 / imageHeight);

    return Out;
}

float4 pslSh(VS_Output In) : SV_Target
{
    float2 uv = In.UV0;
    float dx = In.UV1.x;
    float dy = In.UV1.w;

    // original rendered color
    float4 tex = tex2D(TextureSampler, uv);

    
    /*

    reading the pixels
    

*/
    
    // normal samples
    float3 n0 = tex2D(NormalSampler, uv).rgb; //current pixel
    float3 n1 = tex2D(NormalSampler, uv + float2(-dx, 0)).rgb; //left pixel
    float3 n2 = tex2D(NormalSampler, uv + float2(dx, 0)).rgb; //right pixel
    float3 n3 = tex2D(NormalSampler, uv + float2(0, -dy)).rgb; //top pixel
    float3 n4 = tex2D(NormalSampler, uv + float2(0, dy)).rgb; //bottom pixel

    // depth samples
    float d0 = tex2D(DepthSampler, uv).r; //current pixel
    float d1 = tex2D(DepthSampler, uv + float2(-dx, 0)).r; //left pixel
    float d2 = tex2D(DepthSampler, uv + float2(dx, 0)).r; //right pixel
    float d3 = tex2D(DepthSampler, uv + float2(0, -dy)).r; //top pixel
    float d4 = tex2D(DepthSampler, uv + float2(0, dy)).r; //bottom pixel

    // edge detector from normals
    /*
    This section calculates the edge detection based on the normal map by comparing the normals of the current pixel with its neighbors.
    
    
             n3
             up
    n1 left  n0  right n2
            down
             n4
    
    if result 0 means no edge. if more than 0 there is edge
    
    calculating all direction and we some it the result . the more bigger number the stronger edge
    */
    
    float normalEdge = 0.0; //initial value 0
    normalEdge += length(n0 - n1);
    normalEdge += length(n0 - n2);
    normalEdge += length(n0 - n3);
    normalEdge += length(n0 - n4);

    // edge from depth
    
      /*
    This section calculates the edge detection based on the normal map by comparing the depth of the current pixel with its neighbors.
    
    
             d3
             up
    d1 left  d0  right d2
            down
             d4
    
    if result 0 means no edge. if more than 0 there is edge
    
    calculating all direction and we some it the result . the more bigger number the stronger edge
    */
    
    
    float depthEdge = 0.0;
    depthEdge += abs(d0 - d1);
    depthEdge += abs(d0 - d2);
    depthEdge += abs(d0 - d3);
    depthEdge += abs(d0 - d4);

    // combine normal eges and depth edge to get the final edge
    float edge = saturate(normalEdge * 1.8 + depthEdge * 20.0);

    // grayscale brightness from original render
    float gray = dot(tex.rgb, float3(0.299, 0.587, 0.114));

    // Backround color
    float3 finalColor = float3(1.0, 1.0, 1.0);

    // screen-space coordinates for hatch lines
    float2 p = uv * float2(imageWidth, imageHeight);

    // hatch directions
    //fmod is the reminder  for example, 5/3 = 1 and the reminder is 2, so fmod(5, 3) = 2
    //12 is the number of pixel to draw the lines.
    //diagnolal lines. /  
    
    
    
    float hatch1 = step(fmod(p.x + p.y, 18.0), 1.0);  
    //same as hatch 1 but less value  resulting in less space means darker lines. /
    float hatch2 = step(fmod(p.x + p.y, 14.0), 1.0);
    //
    float hatch3 = step(fmod(p.x + p.y, 10.0), 1.0);
    
    
    

    float hatch = 0.0;
    


    // light shadow -> few lines
    if (gray < 0.70)
        hatch += hatch1;

    // medium shadow -> more lines
    if (gray < 0.50)
        hatch += hatch2;

    // dark shadow -> dense lines
    if (gray < 0.30)
        hatch += hatch3;

    hatch = saturate(hatch);

    // apply marker hatch lines
    /*

    lerp is used to blend between 2 valuses.  
    here we blend the hatch line with the black color to get the black  hatch line color
*/
    finalColor = lerp(finalColor, float3(0.0, 0.0, 0.0), hatch);

    // add black edges
        /*

    lerp is used to blend between 2 valuses.  
    here we blend the line with the black color to get the black line color
*/
    finalColor = lerp(finalColor, float3(0.0, 0.0, 0.0), edge);

   

    return float4(finalColor, 1.0);
}

technique Shader
{
    pass Pass1
    {

        VertexShader = compile vs_4_0 vtsSh();
        PixelShader = compile ps_4_0 pslSh();
    }
}