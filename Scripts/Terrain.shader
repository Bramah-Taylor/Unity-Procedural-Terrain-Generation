Shader "Custom/Terrain" 
{
	Properties 
	{

	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const static int maxLayerCount = 8;
		const static float epsilon = 1E-4;

		int layerCount;								// Number of texture height layers
		float3 baseColours[maxLayerCount];			// Colours for each layer
		float baseStartHeights[maxLayerCount];		// Lowest height values for each layer
		float baseBlends[maxLayerCount];			// Blend values for each layer
		float baseColourStrength[maxLayerCount];	// Tint strength - the contribution of the base colour to the output pixel colour
		float baseTextureScales[maxLayerCount];		// Scale factors for each layer's texture

		float minHeight;
		float maxHeight;

		UNITY_DECLARE_TEX2DARRAY(baseTextures);

		struct Input 
		{
			float3 worldPos;
			float3 worldNormal;
		};

		float inverseLerp(float a, float b, float value) 
		{
			return saturate((value-a)/(b-a));
		}

		// Triplanar mapping function - samples texture down each axis
		float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) 
		{
			float3 scaledWorldPos = worldPos / scale;
			float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
			float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
			float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
			return xProjection + yProjection + zProjection;
		}

		// Surface function - analogous to pixel shader output function
		void surf (Input IN, inout SurfaceOutputStandard output) 
		{
			// Find out how high up this pixel is relative to the minimum and maximum heights
			float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
			float3 blendAxes = abs(IN.worldNormal);
			blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

			// Now calculate the contributions of each texture to the final colour output
			// This involves triplanar mapping for each texture, which is a lot of texture sampling operations
			// This probably isn't very performant, we'll see if it can be changed later if it's a problem
			for (int i = 0; i < layerCount; i ++) 
			{
				float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - baseStartHeights[i]);

				float3 baseColour = baseColours[i] * baseColourStrength[i];
				float3 textureColour = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1-baseColourStrength[i]);

				output.Albedo = output.Albedo * (1-drawStrength) + (baseColour+textureColour) * drawStrength;
			}

		
		}


		ENDCG
	}
	FallBack "Diffuse"
}
