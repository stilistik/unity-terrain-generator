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

        const static int maxColorCount = 8;
        int baseColorCount;
        float3 baseColors[maxColorCount];
        float startHeights[maxColorCount];
        float minHeight;
        float maxHeight;

        struct Input
        {
            float3 worldPos;
        };

        float inverseLerp(float minimumValue, float maximumValue, float value) {
            return saturate((value - minimumValue) / (maximumValue - minimumValue));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            for (int i = 0; i < baseColorCount; i++) {
                float drawStrength = saturate(sign(heightPercent - startHeights[i]));
                o.Albedo = o.Albedo * (1 - drawStrength) + drawStrength * baseColors[i];
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
