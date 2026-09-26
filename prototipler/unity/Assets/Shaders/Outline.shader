// Oyuncak gorunumu icin ince koyu dis cizgi (ters kabuk yontemi). Kalinlik ekranda piksel olarak sabit kalir.
// Yumusak normaller TEXCOORD3'te gelir (MeshFactory hepsini doldurur).
Shader "MarsKod/Outline"
{
    Properties
    {
        _OutlineColor ("Renk", Color) = (0.16, 0.11, 0.13, 1)
        _Width ("Kalinlik (piksel / 1000 yukseklik)", Float) = 1.7
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+10" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _Width;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 smoothNormal : TEXCOORD3; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 ws = TransformObjectToWorld(v.positionOS.xyz);
                float4 clip = TransformWorldToHClip(ws);
                float3 nWS = TransformObjectToWorldNormal(v.smoothNormal);
                float3 nVS = TransformWorldToViewDir(nWS, true);
                float2 nClip = mul((float2x2)UNITY_MATRIX_P, nVS.xy);
                float len = length(nClip);
                nClip = len > 1e-5 ? nClip / len : float2(0, 0);
                float px = _Width * _ScreenParams.y / 1000.0;
                clip.xy += nClip * px * 2.0 / _ScreenParams.xy * clip.w;
                o.positionCS = clip;
                return o;
            }

            half4 frag (Varyings i) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }
    }
}
