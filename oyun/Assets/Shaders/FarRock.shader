// Alanin disina serpistirilen suslu kayalar. Isik zeminle ayni (gunes + golge + ortam + safak); uzakta ve ufka
// yaklastikca zeminle ayni kuralla arka plana karisir (MarsFadeToBackdrop). Boylece ustunde durduklari zemin
// gokyuzune donustugu yerde kayalar da kaybolur, havada asili kalmaz.
Shader "MarsKod/FarRock"
{
    Properties
    {
        _BaseColor ("Renk", Color) = (0.42, 0.25, 0.2, 1)
        _FogRange ("Arka plana karisma (z basla, z bit)", Vector) = (9, 19, 0, 0)
        _Area ("Oyun alani yari boyutu (x, z); toz pusu buradan olculur", Vector) = (3, 3, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _FogRange;
            float4 _Area;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "MarsSky.hlsl"

            float4 _DawnDir;    // C#'tan: safak isiginin gittigi yon
            float4 _DawnColor;  // C#'tan: safak isiginin rengi (dogrusal) * siddet

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 posWS : TEXCOORD0;
                float3 nWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.posWS = TransformObjectToWorld(v.positionOS.xyz);
                o.nWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = TransformWorldToHClip(o.posWS);
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 p = i.posWS;
                float3 n = normalize(i.nWS);
                float2 suv = i.screenPos.xy / i.screenPos.w;

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(p));
                float3 lightCol = mainLight.color * saturate(dot(n, mainLight.direction)) * mainLight.shadowAttenuation;
                float3 dawn = _DawnColor.rgb * saturate(dot(n, -_DawnDir.xyz));
                float3 col = _BaseColor.rgb * (lightCol + SampleSH(n) + dawn);
                col *= MarsVignette(suv);
                return half4(MarsFadeToBackdrop(col, p, suv, _FogRange.xy, _Area.xy), 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection;

            float4 vert (float4 positionOS : POSITION, float3 normalOS : NORMAL) : SV_POSITION
            {
                float3 ws = TransformObjectToWorld(positionOS.xyz);
                float3 nws = TransformObjectToWorldNormal(normalOS);
                float4 c = TransformWorldToHClip(ApplyShadowBias(ws, nws, _LightDirection));
                #if UNITY_REVERSED_Z
                c.z = min(c.z, UNITY_NEAR_CLIP_VALUE);
                #else
                c.z = max(c.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return c;
            }
            half frag () : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 vert (float4 positionOS : POSITION) : SV_POSITION { return TransformObjectToHClip(positionOS.xyz); }
            half frag () : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
