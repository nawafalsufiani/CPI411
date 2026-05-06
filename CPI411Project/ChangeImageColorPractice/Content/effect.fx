
/*

image recienved from mono game 
_spriteBatch.Draw(_image, new Vector2(0,0), Color.White);

means will get image, position color. In struct needs match same as the moongame send


*/

//here where the image is stored in the GPU
sampler TextureSampler : register(s0); //الصورة تخزن هنا 


struct PSInput
{
    float4 Position : SV_POSITION; // يخزن موقع البكسل
    float4 Color : COLOR0; // يخزن لون السبرايت
    float2 TexCoord : TEXCOORD0; // يخزن إحداثيات الصورة
};

float4 MainPS(PSInput input) : COLOR
{
    
    float4 tex = tex2D(TextureSampler, input.TexCoord);
   // return tex * input.Color;
    
    float4 colorRed = float4(1, 0, 0, 1); // اللون الأحمر
    return tex *colorRed;
    //return tex2D(TextureSampler, input.TexCoord) * input.Color;
}

technique SpriteBatch
{
    pass Pass1
    {
        PixelShader = compile ps_4_0_level_9_1 MainPS();
    }
}