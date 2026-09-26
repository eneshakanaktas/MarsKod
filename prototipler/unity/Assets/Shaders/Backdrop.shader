// Arka plan (koyu tema): Mars'ta safaktan hemen once. Cizimin kendisi MarsSky.hlsl icinde (zeminle ortak).
Shader "MarsKod/Backdrop"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Backdrop"
            Tags { "LightMode"="SRPDefaultUnlit" }
            ZWrite Off ZTest Always Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "MarsSky.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = float4(v.uv * 2.0 - 1.0, UNITY_RAW_FAR_CLIP_VALUE, 1.0);
                o.positionCS.y *= _ProjectionParams.x;
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 col = MarsBackdrop(i.uv) * MarsVignette(i.uv);
                col += (hash21(i.uv * _ScreenParams.xy) - 0.5) / 255.0;
                #if !defined(UNITY_COLORSPACE_GAMMA)
                col = SRGBToLinear(saturate(col));
                #endif
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
