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

float4 main(float2 uv : TEXCOORD) : COLOR
{
   // Pull the sample from the maskedActor.
   float4 backgroundSample = tex2D(backgroundSampler, uv);
   float4 backgroundDepthSample = tex2D(backgroundDepthSampler, uv);


   float2 actorUV = uv;
   //actorUV.x += - 0.5f*ActorScale;
   //actorUV.y += - 0.5f*ActorScale;
   //actorUV = actorUV * (1.0f / ActorScale);
   actorUV.x +=  ActorXOffset;
   actorUV.y +=  ActorYOffset;
   float4 maskedActorSample = tex2D(maskedActorSampler, actorUV);
   float4 maskedActorDepthSample = tex2D(maskedActorDepthSampler, actorUV);
   
   float actorAlpha = maskedActorSample.a;
   if (maskedActorSample.a != 0.0f && maskedActorDepthSample.r > backgroundDepthSample.r)
  	  actorAlpha = 0.0f;

   if (actorUV.x < 0.0f || actorUV.y > 1.0f || actorUV.y < 0.0f || actorUV.y > 1.0f)
	  actorAlpha = 0.0f;

   float bgAlpha = 1.0f - actorAlpha;
   float3 finalColor = backgroundSample.rgb*bgAlpha + maskedActorSample.rgb*actorAlpha; 
   return float4(finalColor.r, finalColor.g, finalColor.b, 1.0f);

   //float3 test = backgroundDepthSample.rgb;
   //float3 test = maskedActorSample.rgb;
   //float3 test = maskedActorDepthSample.rgb;
   
   //return float4(test.r, test.g, test.b, 1.0f);
}


