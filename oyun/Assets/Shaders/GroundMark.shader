// Zemine basilmis duz isaretler (ayak izi gibi). Kendi rengi yok: alttaki zemini carparak koyulastirir ya da acar
// (Blend DstColor SrcColor = 2 x kaynak x zemin). Boylece iz, ustunde durdugu zeminin rengini (kum, kurumus camur) ve isigini
// aynen alir. Zemin ufka dogru arka plan resmine donustugu yerde iz de ayni kuralla soner (MarsBackdropFogAmount): ufuktaki
// duvarin ustune tasmaz.
Shader "MarsKod/GroundMark"
{
    Properties
    {
        _Mark ("Zemini carpan (dogrusal; 1 degismez, 1'den kucuk koyu, buyuk acik; en cok 2)", Float) = 0.5
        _FogRange ("Arka plana karisma (z basla, z bit)", Vector) = (9, 19, 0, 0)
        _Area ("Oyun alani yari boyutu (x, z); toz pusu buradan olculur", Vector) = (3, 3, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry+20" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend DstColor SrcColor
            ZWrite Off
            ZTest LEqual
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Mark;
                float4 _FogRange;
                float4 _Area;
            CBUFFER_END

            #include "MarsSky.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 posWS : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.posWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.posWS);
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 suv = i.screenPos.xy / i.screenPos.w;
                float seen = (1.0 - MarsDustHazeAmount(i.posWS, _Area.xy))
                           * (1.0 - MarsBackdropFogAmount(i.posWS, suv, _FogRange.xy, float2(0.0, 0.0)));
                float k = lerp(1.0, _Mark, seen) * 0.5;
                return half4(k, k, k, 1.0);
            }
            ENDHLSL
        }
    }
}
