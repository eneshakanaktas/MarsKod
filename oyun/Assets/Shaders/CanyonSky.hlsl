// Kanyon (Bolge 6) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, sekiller) kullanir.
// Mars'ta gun batimi; kanyonun agzindayiz, kuzeye bakiyoruz. Ufukta iki yanda alcak, duz tepeli, basamakli kayaliklar
// (mesa; yuzlerinde yatay tortu katmanlari), ortada kanyon uzaklara daralip mavimsi pusa karisir. Kayaliklar ufkun hemen
// ustunde kalir: ustteki yazilara uzanmaz, alanla yarismaz. Gunes batida = solda, alcakta: kucuk beyaz disk, cevresinde
// soguk mavi hale (Mars tozunun gercek etkisi). Gunes alcak ve soldan geldigi icin sol kayaliklarin ortaya bakan basamak
// yuzleri golgede, sag kayaliklarinkiler (batiya bakar) isik alir. Yildiz yok.
#ifndef MARSKOD_CANYON_INCLUDED
#define MARSKOD_CANYON_INCLUDED

static const float3 CANYON_PLAIN = float3(0.33, 0.17, 0.11);   // ufuk dibindeki kanyon tabani: zeminin uzaktaki rengi (ekrandan olculdu)
static const float2 CANYON_SUN = float2(-0.14, 0.07);           // gunes: x, ufka gore yukseklik (alcak, sol kayaliklarin ustunde;
                                                                // sol dugmelerin saginda, ust yazilarin altinda)

static const float3 CANYON_SHADE = float3(0.29, 0.21, 0.22);   // golgedeki kaya (gok isigi: serin, morumsu kahve)
static const float3 CANYON_BODY = float3(0.50, 0.31, 0.24);    // gunes tarafindaki kayanin govdesi (dogrudan isik almaz)
static const float3 CANYON_LIT = float3(0.82, 0.55, 0.38);     // alcak gunese bakan yuz ve kenarlar

float3 canyonSkyColor(float t)   // t: 0 ufuk, 1 tepe. Gun batimi: ufukta karamela, tepede koyu
{
    float3 low = float3(0.76, 0.58, 0.47), mid = float3(0.42, 0.32, 0.31), top = float3(0.15, 0.12, 0.15);
    return t < 0.25 ? lerp(low, mid, t / 0.25) : lerp(mid, top, saturate((t - 0.25) / 0.75));
}

// Mesa sirasinin ust kenari. side: -1 sol, +1 sag. Ekranin kenarinda en yuksek, ortaya dogru basamak basamak alcalir,
// kanyonun agzinda biter. Basamaklarin ustu duz, aralari dik (asinmis tortu). s: kenara yakinlik (0 ortada, 1 kenarda);
// riser: basamak yuzunun icinde mi (0..1; egik inen yuz ortaya bakar). lowerTop: bir alt basamagin ust kenari
// (basamak yuzu yalnizca bunun ustunde gorunur; altinda alt basamagin on yuzu vardir).
static const int CANYON_STEPS = 3;
float canyonMesaTop(float x, float side, out float s, out float riser, out float lowerTop)
{
    float start = side < 0.0 ? 0.035 : 0.04;                    // ortadan uzaklik: kanyonun agzi
    float width = side < 0.0 ? 0.17 : 0.17;
    float height = side < 0.0 ? 0.050 : 0.060;                  // gunes tarafi (sol) biraz alcak: gunes ustunde gorunur
    s = saturate((x * side - start) / width);
    float v = s * CANYON_STEPS + 0.32 * vnoise(x * 9.0 + side * 3.0);
    float f = frac(v);
    riser = smoothstep(0.0, 0.12, f) * (1.0 - smoothstep(0.32, 0.45, f));   // basamagin egik yuzu: her basamagin basinda
    float stepped = (floor(v) + smoothstep(0.0, 0.42, f)) / CANYON_STEPS;
    stepped = min(stepped, 1.0);
    float base = _Horizon + 0.004;
    lowerTop = base + height * floor(v) / CANYON_STEPS * (0.84 + 0.16 * vnoise((floor(v) - 1.0) * 3.1 + side));
    return base + height * stepped * (0.84 + 0.16 * vnoise(floor(v) * 3.1 + side))
         + 0.0012 * vnoise(x * 70.0 + side) * step(0.01, s);
}

// Mesa yuzu: yatay tortu katmanlari, birkac belirgin acik tabaka, dibe dogru moloz eteği, ustte isik alan ince kenar.
// lit: 0 golgedeki sira (sol), 1 gunes tarafi (sag). riser: basamak yuzu (sagda gunese bakar, solda golgede).
float3 canyonMesaFace(float2 sp, float top, float lowerTop, float lit, float riser, float onePx)
{
    float above = sp.y - _Horizon;
    float3 c = lerp(CANYON_SHADE, CANYON_BODY, lit);
    float wave = 0.002 * vnoise(sp.x * 9.0 + 3.0);
    c *= 0.84 + 0.16 * (sin((above + wave) * 300.0) * 0.5 + 0.5);                 // ince katmanlar
    c *= 0.92 + 0.08 * (sin((above + wave) * 130.0 + 0.4) * 0.5 + 0.5);
    c = lerp(c, c * 1.3 + 0.03, smoothstep(0.80, 0.96, sin((above + wave) * 70.0 + 1.3)) * 0.85);   // acik tabaka
    // basamak yuzleri: sagda alcak gunes onlara dik vurur, solda kendi golgesinde koyulasir
    riser *= smoothstep(lowerTop - 0.001, lowerTop + 0.002, sp.y);
    c = lerp(c, lerp(c * 0.8, CANYON_LIT * (0.9 + 0.1 * sin((above + wave) * 300.0)), lit), riser * 0.85);
    // ust kenar: duz tepeler gunesi yandan alir
    float edge = 1.0 - smoothstep(0.0, 2.0 * onePx, top - sp.y);
    c = lerp(c, lerp(CANYON_SHADE * 1.5, CANYON_LIT, lit), edge * lerp(0.45, 0.8, lit));
    // etek: dokulmus moloz; tabanin rengine yaklasir, kayalik zeminle bulusur
    float talus = 1.0 - smoothstep(0.004, 0.014, above + 0.003 * vnoise(sp.x * 30.0));
    c = lerp(c, lerp(c, CANYON_PLAIN, 0.55), talus);
    return c;
}

// Kanyonun ici, uzakta: ortadaki bosluktan gorunen alcak, puslu sirtlar
float canyonFarTop(float x)
{
    float d = abs(x - 0.004) / 0.09;
    float v = saturate(d) * 2.0 + 0.25 * vnoise(x * 25.0);
    float stepped = (floor(v) + smoothstep(0.0, 0.3, frac(v))) / 2.0;
    return _Horizon + 0.003 + 0.022 * stepped + 0.0015 * vnoise(x * 80.0);
}

// Bolum 60: patikanin son parcasi. Kenar kayalarinin otesindeki zemin seridi ekranda cok ince oldugu icin zemin orada
// arka plana karisir; patika burada devam eder: zemindeki patikanin hizasindan (ekranda, ufkun altinda d kadar asagida,
// ortanin ~1,1 d sagi) ufuktaki kanyon agzina (x = 0) duz bir cizgiyle daralarak gider (perspektif: genislik d ile orantili).
static const float3 CANYON_PATH = float3(0.59, 0.32, 0.19);   // zemindeki patikanin ekrandaki rengi (olculdu)
float3 canyonPathToMouth(float3 col, float x, float y, float pixel)
{
    float d = _Horizon - y;
    if (d <= 0.0) return col;
    float center = 1.08 * d, halfWidth = 0.23 * d;
    float on = 1.0 - smoothstep(halfWidth - pixel, halfWidth + pixel, abs(x - center));
    float3 c = lerp(CANYON_PLAIN, CANYON_PATH, 0.55 + 0.45 * saturate(d / 0.04));   // uzakta (ufka yakin) tabana yaklasir
    return lerp(col, c, on);
}

float3 CanyonBackdrop(float2 suv)
{
    float2 uv = MarsPictureUV(suv);
    float aspect = _SkyScreen.x / _SkyScreen.y;
    float x = (uv.x - 0.5) * aspect;
    float picturePx = _SkyScreen.y * max(_Picture.x, 1e-3);
    float pixel = 1.5 / picturePx;
    float onePx = 1.0 / picturePx;
    float2 sp = float2(x, uv.y);

    // Gokyuzu: gun batimi
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = canyonSkyColor(skyT);
    float3 skyLow = canyonSkyColor(0.0);

    // Gunes: kucuk beyaz disk, cevresinde soguk mavi hale; genis hale zayif ve sicak (ufka yakin toz)
    float2 sunPos = float2(CANYON_SUN.x, _Horizon + CANYON_SUN.y);
    float2 ds = (sp - sunPos) * float2(1.0, 1.15);
    float dSun = length(ds);
    col += float3(0.50, 0.34, 0.24) * exp(-dSun * 5.0) * 0.25;
    col = lerp(col, float3(0.55, 0.74, 0.98), exp(-dSun * 17.0) * 0.72);
    col = lerp(col, float3(0.84, 0.92, 1.0), exp(-dSun * 60.0) * 0.6);
    float disc = 1.0 - smoothstep(0.0065 - pixel, 0.0065 + pixel, dSun);
    col = lerp(col, float3(1.0, 0.99, 0.97), disc);

    // Kanyonun ici: uzak sirtlar, kanyon duvarlarinin golgesinde (sis yok: koyu, serin)
    float farTop = canyonFarTop(x);
    float farMask = 1.0 - smoothstep(farTop - pixel, farTop + pixel, uv.y);
    float3 farCol = float3(0.36, 0.27, 0.29);   // golgedeki uzak kaya: serin, morumsu kahve
    col = lerp(col, lerp(farCol, skyLow, 0.12), farMask);

    // Iki yandaki mesa siralari
    [unroll] for (int side = -1; side <= 1; side += 2)
    {
        float s, riser, lowerTop;
        float top = canyonMesaTop(x, side, s, riser, lowerTop);
        float m = (1.0 - smoothstep(top - pixel, top + pixel, uv.y)) * smoothstep(0.0, 0.03, s);
        float3 c = canyonMesaFace(sp, top, lowerTop, side > 0 ? 1.0 : 0.0, riser, onePx);
        c = lerp(c, skyLow, 0.03 + 0.07 * (1.0 - s));   // ortaya (kanyonun icine) dogru cok hafif solar: uzaklik
        col = lerp(col, c, m);
    }

    // Ufkun alti duz kanyon tabanidir: zemin ufka vardiginda bu renktedir (CANYON_PLAIN = zeminin uzaktaki rengi, ekrandan
    // olculdu). Burada kaya ya da gokyuzu kalirsa uzaktaki zemin onlari gosterir. Sis yok (Enes): taban kayaliklarin dibine
    // kadar ayni koyu renk, kayaliklarin molozlu etegiyle bulusur.
    col = lerp(col, CANYON_PLAIN, 1.0 - smoothstep(_Horizon - 0.004, _Horizon + 0.003, uv.y));
    if (_CanyonDescent.y > 0.5) col = canyonPathToMouth(col, x, uv.y, pixel);
    return col;
}

#endif
