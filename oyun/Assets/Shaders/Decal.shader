// Zemine yatik ince desen (Bolge 7): kodla uretilen kucuk doku. Iki kullanim:
//  _Mode 0 (carpma): ayak izi. Doku 0.5 gri = zemin aynen kalir, koyu = zemin koyulasir (cukurun ici), acik = aciklasir (kenar).
//     Zeminin isigini ve rengini kendisi tasir: golgede de, gunde de zeminle uyumlu.
//  _Mode 1 (ekleme): Serce'nin isigi. Zemini aydinlatir.
// _A: gorunurluk (0 yok, 1 tam); izlerin belirmesi ve isigin sonmesi bununla.
Shader "MarsKod/Decal"
{
    Properties
    {
        _MainTex ("Doku", 2D) = "white" {}
        _Color ("Renk", Color) = (1, 1, 1, 1)
        _A ("Gorunurluk", Float) = 1
        _Mode ("0 carpma, 1 ekleme", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Kaynak", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Hedef", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-10" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _A;
                float _Mode;
                float _SrcBlend;
                float _DstBlend;
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
                half4 t = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;
                half k = t.a * _A * i.color.a;   // kose rengi alfasi: tek tek izin derinligi (koyuluk)
                if (_Mode < 0.5) return half4(lerp(half3(0.5, 0.5, 0.5), t.rgb, k), 1.0);
                return half4(t.rgb * k, 1.0);
            }
            ENDHLSL
        }
    }
}
