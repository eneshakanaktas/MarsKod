// Buz toplaninca karenin uzerinde yayilan ince halka.
Shader "MarsKod/Ring"
{
    Properties
    {
        _Color ("Renk", Color) = (0.75, 0.95, 1, 1)
        _R ("Yaricap (0-1)", Float) = 0.5
        _W ("Kalinlik", Float) = 0.06
        _A ("Gorunurluk", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _R;
                float _W;
                float _A;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float r = length(i.uv - 0.5) * 2.0;
                float k = (r - _R) / _W;
                float ring = exp(-k * k);
                return half4(_Color.rgb, _Color.a * ring * _A);
            }
            ENDHLSL
        }
    }
}
