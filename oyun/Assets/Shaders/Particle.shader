// Parcacik tanesi (kar, kivilcim, toz): yumusak kenarli yuvarlak nokta. Rengi ve saydamligi parcacik sisteminden gelir
// (tane basina renk); resim dosyasi gerekmez. _Core: ortadaki dolu kismin yaricapi (0 = cok yumusak, 0.9 = sert kenarli).
Shader "MarsKod/Particle"
{
    Properties
    {
        _Core ("Dolu orta (0-1)", Float) = 0.35
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
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
                float _Core;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float r = length(i.uv - 0.5) * 2.0;
                float a = 1.0 - smoothstep(_Core, 1.0, r);
                return half4(i.color.rgb, i.color.a * a);
            }
            ENDHLSL
        }
    }
}
