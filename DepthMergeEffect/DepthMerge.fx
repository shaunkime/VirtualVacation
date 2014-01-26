//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- LogoDisplacer
//
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Shader constant register mappings (scalars - float, double, Point, Color, Point3D, etc.)
//-----------------------------------------------------------------------------------------

float  displacement : register(C0);
float  additionalLogoOpacity : register(C1);
float4 ddxDdy : register(C6);

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);
sampler2D logoSampler : register(S1);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
   // Pull the sample from the logo.
   float4 logoSample = tex2D(logoSampler, uv);
   
   // See how far away the the red and green channels are from "gray".  Use this
   // value to shift.
   float2 fracShift = float2(0.5,0.5) - logoSample.rg;
   
   // But first modulate the shift by the alpha in the logo.  Most of the logo is
   // transparent, so this will zero out fracShift anywhere other than the logo.
   fracShift *= logoSample.a;
   
   // Calculate coordinate to sample main image at, by displacing by the logo's
   // "distance from gray".  ddxDdy is used to ensure that "displacement" is treated
   // in pixel units (it maps from pixel units to [0,1] units).
   float2 displacedCoord = uv + fracShift * displacement * float2(length(ddxDdy.xy), length(ddxDdy.zw));
    
   // Now get the main image's color at that displaced coordinate.
   float4 color = tex2D(implicitInputSampler, displacedCoord);
    
   // And mix in that color with a portion of the logo, to make the logo more clearly
   // visible.  Modulate it with "additionalLogoOpacity".
   float4 finalColor = color* (1.0f, 0.0f, 0.0f, 1.0f) + logoSample *logoSample.a * additionalLogoOpacity;
   
   return finalColor;
}


