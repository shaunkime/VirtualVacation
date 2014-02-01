//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- maskedActorDisplacer
//
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Shader constant register mappings (scalars - float, double, Point, Color, Point3D, etc.)
//-----------------------------------------------------------------------------------------

float  ActorScale : register(C0);
float  ActorXOffset : register(C1);
float  ActorYOffset : register(C2);

float oldMeanX : register(C3);
float oldMeanY : register(C4);
float oldMeanZ : register(C5);

float newMeanX : register(C6);
float newMeanY : register(C7);
float newMeanZ : register(C8);

float oldStdDevX : register(C9);
float oldStdDevY : register(C10);
float oldStdDevZ : register(C11);

float newStdDevX : register(C12);
float newStdDevY : register(C13);
float newStdDevZ : register(C14);

float transferColorBias : register(C15);

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes)
//--------------------------------------------------------------------------------------

sampler2D backgroundSampler : register(S0);
sampler2D maskedActorSampler : register(S1);
sampler2D maskedActorDepthSampler : register(S2);
sampler2D backgroundDepthSampler : register(S3);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

static const float Sqrt3 = 1.73205080757f;
static const float Sqrt6 = 2.44948974278f;
static const float Sqrt2 = 1.41421356237f;
static const float Sqrt3Over3 = 0.57735026919f;
static const float Sqrt6Over6 = 0.40824829046f;
static const float Sqrt2Over2 = 0.70710678118f;

float3 RGBtoLMS(float3 color)
{
    color = max(color, 0.0000001f);

	float3x3 mat = float3x3 ( 
								float3(0.3811f, 0.5783f, 0.0402f),
								float3(0.1967f, 0.7244f, 0.0782f),
								float3(0.0241f, 0.1288f, 0.8444f)
							);

	return log10 ( mul(mat, color));
}

float3 LMSToDecorrellated(float3 lms)
{
	float3x3 mat = float3x3 ( 
								float3(1.0f/ Sqrt3,  1.0f/ Sqrt3,  1.0f/ Sqrt3),
								float3(1.0f/ Sqrt6,  1.0f/ Sqrt6, -2.0f/ Sqrt6),
								float3(1.0f/ Sqrt2, -1.0f/ Sqrt2,  0.0f)
							);
	return float3( mul (mat, lms));
}

float3 DecorrellatedToLMS(float3 lab)
{
	lab = lab * float3(Sqrt3Over3, Sqrt6Over6, Sqrt2Over2);

	float3x3 mat = float3x3 ( 
								float3(1.0f,  1.0f,  1.0f),
								float3(1.0f,  1.0f, -1.0f),
								float3(1.0f,  -2.0f,  0.0f)
							);
	return mul (mat, lab);
}

float3 LMSToRGB(float3 lms)
{
    lms = pow(10.0f, lms);

	float3x3 mat = float3x3 ( 
								float3(4.46790f, -3.5873f,  0.1193f),
								float3(-1.2186f,  2.3809f, -0.1624f),
								float3(0.04970f, -0.2439f,  1.2045f)
							);

	return clamp ( mul(mat, lms), 0.0f, 1.0f);
}

float3 TransferColor(float3 oldMean, float3 oldStdDev, float3 newMean, float3 newStdDev, float4 pixel, float3 decorrelatedValue)
{
    float3 scale = newStdDev / oldStdDev;
	if (pixel.a == 0)
    {
		return pixel.rgb;
    }
            
	decorrelatedValue -= oldMean;
    decorrelatedValue *= scale;
    decorrelatedValue += newMean;

    float3 lms = DecorrellatedToLMS(decorrelatedValue);
    return LMSToRGB(lms);
}

float3 ComputeDecorrelation(float4 pixel)
{
    if (pixel.a == 0)
    {
		return float3(0.0f, 0.0f, 0.0f);
	}

    float3 tempLMS = RGBtoLMS(pixel.rgb);
    float3 decorrelated = LMSToDecorrellated(tempLMS);
	return decorrelated;
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
   // Pull the sample from the maskedActor.
   float4 backgroundSample = tex2D(backgroundSampler, uv);
   float4 backgroundDepthSample = tex2D(backgroundDepthSampler, uv);


   float2 actorUV = uv;
   actorUV.x += ActorXOffset;
   actorUV.y += ActorYOffset;
   actorUV = actorUV * (1.0f / ActorScale);
   float4 maskedActorSample = tex2D(maskedActorSampler, actorUV);
   float4 maskedActorDepthSample = tex2D(maskedActorDepthSampler, actorUV);

   if (transferColorBias == 1.0f)
   {
		float3 decorrelatedValue = ComputeDecorrelation(maskedActorSample);	
		maskedActorSample.rgb = TransferColor(float3(oldMeanX, oldMeanY, oldMeanZ),
											  float3(oldStdDevX, oldStdDevY, oldStdDevZ),
											  float3(newMeanX, newMeanY, newMeanZ),
											  float3(newStdDevX, newStdDevY, newStdDevZ),
											  maskedActorSample, 
											  decorrelatedValue);
   }
   
   float actorAlpha = maskedActorSample.a;
   if (maskedActorSample.a != 0.0f && maskedActorDepthSample.r > backgroundDepthSample.r)
  	  actorAlpha = 0.0f;

   if (actorUV.x < 0.0f || actorUV.y > 1.0f || actorUV.y < 0.0f || actorUV.y > 1.0f)
	  actorAlpha = 0.0f;

   float bgAlpha = 1.0f - actorAlpha;
   float3 finalColor = backgroundSample.rgb*bgAlpha + maskedActorSample.rgb*actorAlpha; 
   return float4(finalColor.r, finalColor.g, finalColor.b, 1.0f);

}



