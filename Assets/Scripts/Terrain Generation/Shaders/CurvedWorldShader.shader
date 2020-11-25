// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/CurvedWorldShader"
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
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int maxLayerCount = 8;
        const static float epsilon = 1E-4;

        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        int layerCount;
        float3 baseColours[maxLayerCount];
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColourStrength[maxLayerCount];
        float baseTextureScales[maxLayerCount];
        float baseBrightnessCorrection[maxLayerCount];

        float curveStrength;
        float curveFalloff;

        float minHeight;
        float maxHeight;

        float inverseLerp(float minVal, float maxVal, float value){
            return saturate((value - minVal) / (maxVal - minVal));
        }

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float4 curveWorld(float4 position){
            curveStrength *= 0.0001;

            position = mul(unity_ObjectToWorld, position);

            float3 origin = _WorldSpaceCameraPos;

            float dist = position.z - origin.z;
            dist = max(0, dist - curveFalloff);
            float offset = curveStrength * dist * dist;

            position.y -= offset;

            return mul(unity_WorldToObject, position);
        }

        float3 inverseCurveWorld(float3 position){
            curveStrength *= 0.0001;

            position = mul(unity_ObjectToWorld, position);

            float3 origin = _WorldSpaceCameraPos;

            float dist = position.z - origin.z;
            dist = max(0, dist - curveFalloff);
            float offset = curveStrength * dist * dist;

            position.y += offset;

            return mul(unity_WorldToObject, position);
        }
        
        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex){
            //Triplanar Mapping
            float3 scaledWorldPos = worldPos/scale;
            
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;

            return xProjection + yProjection + zProjection;
        }

        void vert (inout appdata_full v) {
            v.vertex = curveWorld(v.vertex);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

            for(int i = 0; i < layerCount; i++) {
                float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, (heightPercent - baseStartHeights[i]));
                float3 baseColour = baseColours[i] * baseColourStrength[i];
                float3 rawWorldPos = float3(IN.worldPos.x, inverseCurveWorld(IN.worldPos).y, IN.worldPos.z);
                float3 baseTexture = saturate(triplanar(rawWorldPos, baseTextureScales[i], blendAxes, i) + baseBrightnessCorrection[i]);
                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColour + baseTexture) * drawStrength;
            }

        }
        ENDCG
    }
    FallBack "Diffuse"
}
