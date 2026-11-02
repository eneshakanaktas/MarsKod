// Kanyon dibi, eski nehir yatagi (Bolge 7) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, sekiller) kullanir.
// Mars'ta sabah; kanyonun dibindeyiz, kuzeye bakiyoruz. Gunes doguda = sagda, alcakta: kucuk beyaz disk, cevresinde soguk mavi
// hale. Gunes henuz dibe inmemis: kanyon duvarlarinin yalniz ust seridi turuncu aydinlik (karsidaki kenarin golgesi asagisini
// orter), dip serin morumsu-kahve golgede. Duvarlar alcak, ufkun hemen ustunde (ustteki yazilara uzanmaz); yuzlerinde suyun
// biraktigi duzgun, ince tortu katmanlari. Gunes sagda oldugu icin sol duvarin ortaya (doguya) bakan yuzu isik alir, sag duvar
// kendi golgesindedir (yalniz ust kenari parlar). Yildiz yok.
// _RiverScene (RiverBedTraces): 0 = 61-62 ortadaki uzak duvarda yarigin agzi ve dibe inen patika, 1 = 63-64 duvarlar geride,
// eski nehrin kivrimi uzaklasir, 2 = 65-66 sag duvarda yan kanyonun agzi (iceride alcak, golgedeki uzak duvar), 3 = 67-70 yakin
// arka duvar 3B (RiverWall): ortadaki uzak duvar cizilmez.
#ifndef MARSKOD_RIVER_INCLUDED
#define MARSKOD_RIVER_INCLUDED

static const float3 RIVER_PLAIN = float3(0.38, 0.28, 0.24);   // ufuk dibindeki golgeli yatak: zeminin uzaktaki rengi
static const float2 RIVER_SUN = float2(0.125, 0.075);         // gunes: x, ufka gore yukseklik (sag dugmelerin solunda, ust yazilarin altinda)

static const float3 RIVER_SHADE = float3(0.30, 0.24, 0.27);   // golgedeki duvar: serin, morumsu kahve
static const float3 RIVER_BODY = float3(0.40, 0.31, 0.30);    // golgedeki duvarin acik tabakalari
static const float3 RIVER_LIT = float3(0.88, 0.56, 0.36);     // gunes alan ust serit
static const float3 RIVER_GAP = float3(0.15, 0.11, 0.12);     // yarigin agzi: derin golge
static const float2 RIVER_MOUTH = float2(0.125, 0.026);        // 65-66: sag duvardaki yan kanyonun agzi (x, yari genislik)

bool RiverSceneIs(float k) { return abs(_RiverScene - k) < 0.5; }

float3 riverSkyColor(float t)   // t: 0 ufuk, 1 tepe. Sabah: ufukta soluk seftali-krem, yukarida solgun mavi-gri, tepede koyu
{
    float3 low = float3(0.80, 0.67, 0.56), mid = float3(0.46, 0.45, 0.50), top = float3(0.16, 0.17, 0.23);
    return t < 0.28 ? lerp(low, mid, t / 0.28) : lerp(mid, top, saturate((t - 0.28) / 0.72));
}

// Yan duvarin ust kenari. side: -1 sol, +1 sag. Ortada nehir yatagi acik; duvar kenara dogru iki yumusak basamakla yukselir
// (su asindirmis: koseler yuvarlak). s: kenara yakinlik (0 ortada, 1 kenarda).
float riverSideTop(float x, float side, float start, float height, out float s)
{
    s = saturate((x * side - start) / 0.13);
    float v = s * 2.0 + 0.22 * vnoise(x * 8.0 + side * 5.0);
    float stepped = min((floor(v) + smoothstep(0.0, 0.55, frac(v))) / 2.0, 1.0);
    return _Horizon + 0.004 + height * stepped * (0.88 + 0.12 * vnoise(floor(v) * 2.7 + side))
         + 0.001 * vnoise(x * 60.0 + side) * step(0.01, s);
}

// Ortadaki uzak duvar (yatagin uzaktaki dirsegi): alcak, neredeyse duz tepeli
float riverFarTop(float x, float height)
{
    return _Horizon + 0.003 + height * (0.82 + 0.18 * vnoise(x * 14.0 + 2.0)) + 0.0008 * vnoise(x * 90.0);
}

// Duvar yuzu: duzgun, ince tortu katmanlari (su cokeltisi), birkac acik tabaka; gunes alan ust serit (shadowLine'in ustu) turuncu.
// sunlit: 0 golgedeki duvar (yalniz ust kenari parlar), 1 gunese bakan duvar.
float3 riverWallFace(float2 sp, float top, float sunlit, float onePx)
{
    float above = sp.y - _Horizon;
    float wave = 0.0012 * vnoise(sp.x * 7.0 + 3.0);
    float3 c = RIVER_SHADE;
    c *= 0.88 + 0.12 * (sin((above + wave) * 420.0) * 0.5 + 0.5);                 // ince, duzgun katmanlar
    c = lerp(c, RIVER_BODY, smoothstep(0.75, 0.95, sin((above + wave) * 160.0 + 0.7)) * 0.7);   // acik tabaka
    // ust serit: karsidaki kenarin golgesi asagisini orter; golge cizgisi duvar boyunca hafifce dalgalanir
    float wallH = max(top - _Horizon, 1e-4);
    float shadowLine = top - wallH * (0.38 + 0.08 * vnoise(sp.x * 22.0));
    float lit = smoothstep(shadowLine - 1.5 * onePx, shadowLine + 1.5 * onePx, sp.y) * sunlit;
    c = lerp(c, RIVER_LIT * (0.92 + 0.08 * sin((above + wave) * 420.0)), lit);
    // ust kenar: dogudan gelen isik tepeyi yalar
    float edge = 1.0 - smoothstep(0.0, 2.0 * onePx, top - sp.y);
    c = lerp(c, RIVER_LIT * lerp(0.75, 1.0, sunlit), edge * lerp(0.55, 0.85, sunlit));
    // etek: yatagin rengine yaklasir
    float talus = 1.0 - smoothstep(0.003, 0.012, above + 0.002 * vnoise(sp.x * 30.0));
    return lerp(c, lerp(c, RIVER_PLAIN, 0.6), talus);
}

// 61-62: ortadaki uzak duvarda yarigin agzi (koyu dikey centik) ve oradan dibe inen ince zikzak patika (60'tan gelinen yer)
static const float RIVER_GAP_X = -0.018;
float3 riverDescentGap(float2 sp, float farTop, float3 c, float pixel)
{
    float above = sp.y - _Horizon;
    float wallH = farTop - _Horizon;
    float depth = saturate((farTop - sp.y) / max(wallH, 1e-4));            // 0 tepede, 1 dipte
    float halfW = lerp(0.006, 0.0008, depth);                               // agiz yukarida genis, asagi daralir
    float gap = 1.0 - smoothstep(halfW - pixel, halfW + pixel, abs(sp.x - RIVER_GAP_X));
    gap *= step(_Horizon + wallH * 0.45, sp.y);                            // yarik duvarin ust yarisinda
    c = lerp(c, RIVER_GAP, gap);
    // patika: yarigin dibinden etege zikzakla iner (acik renkli, ince)
    float zig = RIVER_GAP_X + 0.010 * (abs(frac(above * 120.0) - 0.5) * 4.0 - 1.0);
    float path = (1.0 - smoothstep(0.0, pixel * 1.2, abs(sp.x - zig))) * step(above, wallH * 0.45) * step(0.001, above);
    return lerp(c, float3(0.62, 0.48, 0.40), path * 0.8);
}

float3 RiverBackdrop(float2 suv)
{
    float2 uv = MarsPictureUV(suv);
    float aspect = _SkyScreen.x / _SkyScreen.y;
    float x = (uv.x - 0.5) * aspect;
    float picturePx = _SkyScreen.y * max(_Picture.x, 1e-3);
    float pixel = 1.5 / picturePx;
    float onePx = 1.0 / picturePx;
    float2 sp = float2(x, uv.y);

    // Gokyuzu: sabah
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = riverSkyColor(skyT);
    float3 skyLow = riverSkyColor(0.0);

    // Gunes: kucuk beyaz disk, cevresinde soguk mavi hale; genis hale sicak (ufka yakin toz)
    float2 sunPos = float2(RIVER_SUN.x, _Horizon + RIVER_SUN.y);
    float dSun = length((sp - sunPos) * float2(1.0, 1.15));
    col += float3(0.55, 0.40, 0.28) * exp(-dSun * 5.0) * 0.25;
    col = lerp(col, float3(0.58, 0.76, 0.98), exp(-dSun * 17.0) * 0.7);
    col = lerp(col, float3(0.86, 0.93, 1.0), exp(-dSun * 60.0) * 0.6);
    float disc = 1.0 - smoothstep(0.0065 - pixel, 0.0065 + pixel, dSun);
    col = lerp(col, float3(1.0, 0.99, 0.97), disc);

    // Ortadaki uzak duvar (yakin arka duvar 3B olan 67-70'te yok). 63-64'te duvarlar geride: daha alcak.
    bool wide = RiverSceneIs(1.0);
    if (!RiverSceneIs(3.0))
    {
        float farTop = riverFarTop(x, wide ? 0.009 : 0.016);
        float m = 1.0 - smoothstep(farTop - pixel, farTop + pixel, uv.y);
        float3 c = lerp(riverWallFace(sp, farTop, 0.35, onePx), skyLow, 0.12);   // gunes duvari yandan yalar; biraz uzak
        if (RiverSceneIs(0.0)) c = riverDescentGap(sp, farTop, c, pixel);
        col = lerp(col, c, m);
    }

    // Iki yandaki duvarlar: sol duvar (doguya bakar) ust seritte gunes alir, sag duvar golgede
    [unroll] for (int side = -1; side <= 1; side += 2)
    {
        float s;
        float top = riverSideTop(x, side, wide ? 0.07 : 0.045, wide ? 0.04 : 0.052, s);
        float3 c = riverWallFace(sp, top, side < 0 ? 1.0 : 0.0, onePx);
        if (side > 0 && RiverSceneIs(2.0))
        {
            // yan kanyonun agzi: duvar burada yarilir; iceride kanyonun alcak, golgedeki uzak duvari (izin gittigi yer)
            float mouth = 1.0 - smoothstep(RIVER_MOUTH.y - 0.005, RIVER_MOUTH.y, abs(x - RIVER_MOUTH.x));
            float innerTop = _Horizon + 0.004 + 0.014 * (0.85 + 0.15 * vnoise(x * 45.0));
            top = lerp(top, innerTop, mouth);
            c = lerp(c, lerp(RIVER_GAP, RIVER_SHADE, 0.45) * (0.92 + 0.08 * sin((uv.y - _Horizon) * 420.0)), mouth);
        }
        float m = (1.0 - smoothstep(top - pixel, top + pixel, uv.y)) * smoothstep(0.0, 0.03, s);
        c = lerp(c, skyLow, 0.03 + 0.06 * (1.0 - s));   // ortaya (uzaga) dogru cok hafif solar
        col = lerp(col, c, m);
    }

    // Ufkun alti golgedeki yatak: zemin ufka vardiginda bu renktedir. Sis yok.
    float below = 1.0 - smoothstep(_Horizon - 0.004, _Horizon + 0.003, uv.y);
    float3 plain = RIVER_PLAIN;
    if (wide)
    {
        // 63-64: eski nehrin kivrimi uzaklasir (zemindeki kivrimla birlesir): ufka dogru daralan acik kumlu serit
        float d = saturate((_Horizon - uv.y) / 0.08);
        float cx = 0.012 + 0.05 * d * sin(d * 5.0 + 0.6);
        float halfW = 0.003 + 0.05 * d;
        float bed = 1.0 - smoothstep(halfW * 0.8, halfW, abs(x - cx));
        plain = lerp(plain, float3(0.50, 0.41, 0.36), bed * 0.7);
    }
    return lerp(col, plain, below);
}

#endif
