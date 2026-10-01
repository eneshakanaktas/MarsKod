// Mars yuzeyi: tek parca, ufka uzanan zemin. Ortadaki oyun alani ayni zeminde, kare sinirlari ince ve soluk cizgilerle.
// Etrafta kraterler; uzaklastikca ayrinti azalir, toz pusu artar ve zemin arka plana (tepeler, koloni) dikissiz karisir.
// Gunes isigi + golge + ortam isigi + arkadan safak isigi.
Shader "MarsKod/Ground"
{
    Properties
    {
        _Area ("Oyun alani yari boyutu (x, z)", Vector) = (2.5, 2.0, 0, 0)
        _FogRange ("Arka plana karisma (z basla, z bit)", Vector) = (5, 14, 0, 0)
        _Frost0 ("Buz 0 (x, z, yaricap, acik)", Vector) = (0, 0, 0, 0)
        _Frost1 ("Buz 1", Vector) = (0, 0, 0, 0)
        _Frost2 ("Buz 2", Vector) = (0, 0, 0, 0)
        _Frost3 ("Buz 3", Vector) = (0, 0, 0, 0)
        _Frost4 ("Buz 4", Vector) = (0, 0, 0, 0)
        _Frost5 ("Buz 5", Vector) = (0, 0, 0, 0)
        _Crater ("Alandaki krater (x, z, yaricap)", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _Area;
            float4 _FogRange;
            float4 _Frost0, _Frost1, _Frost2, _Frost3, _Frost4, _Frost5;
            float4 _Crater;
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
            float4 _RobotPos;   // C#'tan: robotun konumu
            float4 _RobotFwd;   // C#'tan: robotun baktigi yon

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

            float frostAt(float2 xz, float2 q, float4 f)
            {
                if (f.w < 0.5) return 0.0;
                float radial = 1.0 - smoothstep(f.z * 0.3, f.z, length(xz - f.xy) + (fbm(q * 4.0) - 0.5) * 0.18);
                float patches = smoothstep(0.35, 0.6, fbm(q * 5.0 + 11.0));
                return saturate(radial * (0.55 + 0.45 * patches));
            }

            float3 regolith(float3 p, float far, float inArea)
            {
                float2 q = p.xz;
                float detail = 1.0 - far * 0.5;
                float3 a = lerp(float3(0.42, 0.25, 0.20), float3(0.52, 0.32, 0.25), fbm(q * 2.2));
                a *= 0.88 + 0.24 * fbm(q * 0.35 + 4.0);                        // genis lekeler
                a *= 1.0 + detail * 0.12 * (vnoise2(q * 38.0) - 0.5);          // ince taneler
                float speck = smoothstep(0.80, 0.90, vnoise2(q * 24.0 + 7.0)) * detail;
                a = lerp(a, float3(0.27, 0.16, 0.13), speck * 0.45);
                float light = smoothstep(0.84, 0.93, vnoise2(q * 19.0 + 19.0)) * detail;
                a = lerp(a, float3(0.62, 0.43, 0.34), light * 0.35);

                // etraftaki kraterler (oyun alaninin disinda)
                float2 cell = floor(q / 3.2);
                float2 center = (cell + 0.2 + 0.6 * hash22(cell + 5.0)) * 3.2;
                float cr = 0.25 + 0.4 * hash21(cell + 9.0);
                float d = length(q - center) / cr;
                float has = step(hash21(cell + 2.0), 0.4) * (1.0 - inArea) * (1.0 - far * 0.7);
                a *= lerp(1.0, lerp(0.82 + 0.14 * d * d, 1.0, smoothstep(0.85, 1.0, d)), has);
                a *= 1.0 + has * 0.15 * exp(-pow((d - 1.0) / 0.13, 2.0));

                // alandaki kucuk krater
                if (_Crater.z > 0.0)
                {
                    float dc = length(q - _Crater.xy) / _Crater.z;
                    a *= lerp(0.72 + 0.2 * dc * dc, 1.0, smoothstep(0.85, 1.0, dc));
                    a *= 1.0 + 0.16 * exp(-pow((dc - 1.0) / 0.12, 2.0));
                }

                // buzlanma: buzlarin cevresinde lekeler halinde
                float fr = max(max(frostAt(q, q, _Frost0), frostAt(q, q, _Frost1)), max(frostAt(q, q, _Frost2), frostAt(q, q, _Frost3)));
                fr = max(fr, max(frostAt(q, q, _Frost4), frostAt(q, q, _Frost5)));
                a = lerp(a, float3(0.66, 0.72, 0.78), fr * 0.7);
                return a;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 p = i.posWS;
                float3 n = normalize(i.nWS);
                float2 suv = i.screenPos.xy / i.screenPos.w;

                float2 dOut = max(abs(p.xz) - _Area.xy, 0.0);
                float outside = length(dOut);
                float inArea = 1.0 - smoothstep(0.0, 0.03, outside);
                float far = smoothstep(8.0, 20.0, outside);   // koloniye dogru cok gec ve yavas sadelesir

                float3 a = regolith(p, far, inArea);

                // oyun alani: ayni zemin, biraz daha duzgun; kare sinirlari ince ve soluk
                // sinirlar alanin kenarindan (-_Area) birer birim arayla; tek/cift kare sayisinda da dogru
                float gx = abs(frac(p.x + _Area.x + 0.5) - 0.5);
                float gz = abs(frac(p.z + _Area.y + 0.5) - 0.5);
                float w = 0.010 + fwidth(p.x) * 0.8;
                float gridLine = 1.0 - smoothstep(w * 0.5, w * 1.6, min(gx, gz));
                float inGrid = 1.0 - smoothstep(0.0, w * 2.0, outside);
                a *= 1.0 + 0.04 * inArea;
                a *= 1.0 - 0.2 * gridLine * inGrid;
                // alanin cevresinde hafifce ezilmis, koyu bir iz
                float edge = exp(-outside * outside / 0.02) * (1.0 - inArea);
                a *= 1.0 - 0.12 * edge;

                float3 albedo = a * a * (a * 0.305 + 0.683) + a * 0.012; // sRGB -> dogrusal (yaklasik)

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(p));
                float wrap = saturate((dot(n, mainLight.direction) + 0.2) / 1.2);
                float3 lightCol = mainLight.color * wrap * mainLight.shadowAttenuation;
                float3 amb = SampleSH(n);
                float3 dawn = _DawnColor.rgb * saturate(dot(n, -_DawnDir.xyz) + 0.15);
                // gunese dogru (uzakta) zemin hafifce isinir
                float3 warmth = float3(0.30, 0.13, 0.06) * exp(-length((p.xz - float2(0.8, 16.0)) * float2(0.16, 0.09)));
                // alani sadece kose direklerindeki kucuk lambalar hafifce aydinlatir
                float lamps = 0.0;
                [unroll] for (int k = 0; k < 4; k++)
                {
                    float2 lp = float2((k % 2 == 0 ? -1.0 : 1.0) * (_Area.x + 0.06), (k < 2 ? -1.0 : 1.0) * (_Area.y + 0.06));
                    float2 dl = p.xz - lp;
                    lamps += exp(-dot(dl, dl) / 0.18);   // sadece diregin dibi
                }
                float3 lampLight = float3(1.0, 0.55, 0.28) * lamps * 0.25;

                // robotun gozlerinden onundeki zemine dusen hafif isik
                float2 rd = p.xz - _RobotPos.xz;
                float along = dot(rd, _RobotFwd.xz);
                float lateral = dot(rd, float2(-_RobotFwd.z, _RobotFwd.x));
                // tam onune dogru acilan hüzme: robotun burnundan baslar, uzaklastikca genisler ve soner
                float beam = smoothstep(0.2, 0.45, along) * exp(-along / 1.1) * exp(-lateral * lateral / (0.02 + 0.06 * along * along));
                float3 robotLight = float3(0.55, 0.85, 0.95) * beam * 2.2;

                // isik kaynagi tepenin ardindaki gunes: zemin gunese (uzaga) dogru hafifce aydinlanir, kameraya dogru kararir
                float toSun = smoothstep(-5.0, 14.0, p.z);
                float3 sky = (lightCol + amb + dawn) * lerp(0.6, 1.2, toSun);
                float3 col = albedo * (sky + warmth * 1.3 + lampLight + robotLight);

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
            half4 frag () : SV_Target { return 0; }
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
