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

        int layerCount;
        float3 tints[maxLayerCount];
        float tintStrengths[maxLayerCount];
        float startHeights[maxLayerCount];
        float blendStrengths[maxLayerCount];
        float textureScales[maxLayerCount];
        float minHeight;
        float maxHeight;

        UNITY_DECLARE_TEX2DARRAY(textures);

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float inverseLerp(float minimumValue, float maximumValue, float value) {
            return saturate((value - minimumValue) / (maximumValue - minimumValue));
        }

        float3 triplanar(float3 worldPos, float textureScale, float3 blendAxes, int textureIndex) {
            float3 scaledWorldPos = worldPos / textureScale;
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(textures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(textures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(textures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            return xProjection + yProjection + zProjection;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= (blendAxes.x + blendAxes.y + blendAxes.z);

            for (int i = 0; i < layerCount; i++) {
                float drawStrength = inverseLerp(-blendStrengths[i]/2, blendStrengths[i]/2, heightPercent - startHeights[i]);
                float3 baseColor = tints[i] * tintStrengths[i];
                float3 textureColor = triplanar(IN.worldPos, textureScales[i], blendAxes, i) * (1 - tintStrengths[i]);
                o.Albedo = o.Albedo * (1 - drawStrength) + drawStrength * (baseColor + textureColor);
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
