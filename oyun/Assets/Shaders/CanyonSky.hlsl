// Kanyon (Bolge 6) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, sekiller) kullanir.
// Ikindi, kanyonun agzindayiz ve kuzeye bakiyoruz (Valles Marineris esinli): iki yanda ekranin kenarlarindan yukselen dev,
// katman katman, basamakli duvarlar; eteklerinde dokulmus moloz. Gunes batida = solda, sol duvarin kenarinin hemen ustunde:
// sol duvarin bize bakan yuzu kendi golgesinde (morumsu), sag duvarin ice bakan yuzu gunese doner (altin). Ortada kanyon
// uzaklara kivrilarak alcalir; uzak duvarlar kat kat solar, dibinde ince toz sisi. Yildiz yok.
#ifndef MARSKOD_CANYON_INCLUDED
#define MARSKOD_CANYON_INCLUDED

static const float3 CANYON_PLAIN = float3(0.50, 0.32, 0.25);   // duvarlarin dibindeki puslu kanyon tabani (zeminin pusu bu renge solar)
static const float2 CANYON_SUN = float2(-0.12, 0.085);         // gunes: x, ufka gore yukseklik (alcak sol duvarin hemen ustunde;
                                                                // ust yazilarin altinda, sol dugmelerin saginda kalir)

static const float3 CANYON_LIT = float3(0.80, 0.50, 0.31);     // gunese bakan kaya yuzu
static const float3 CANYON_SHADE = float3(0.31, 0.22, 0.26);   // golgedeki kaya yuzu (gok isigi: serin, morumsu)

float3 canyonSkyColor(float t)   // t: 0 ufuk, 1 tepe
{
    float3 low = float3(0.80, 0.57, 0.41), mid = float3(0.53, 0.38, 0.33), top = float3(0.22, 0.18, 0.23);
    return t < 0.3 ? lerp(low, mid, t / 0.3) : lerp(mid, top, saturate((t - 0.3) / 0.7));
}

// Yakin yan duvarin ust kenari. side: -1 sol, +1 sag. Ekranin kenarinda en yuksek (duz bir tepe = mesa), ortaya dogru
// basamak basamak alcalir ve kanyonun ortasinda biter. s: duvarin "ne kadar yakin/yuksek" oldugu (0 ortada, 1 kenarda).
// Sol (gunes tarafi) duvar daha alcak: gunes onun ustunde gorunur; sag duvar dev.
float canyonRim(float x, float side, out float s)
{
    float start = side < 0.0 ? -0.035 : 0.05;                    // duvarin bittigi yer (orta biraz saga kayik: kanyon kivrilir)
    s = saturate((x - start) * side / (side < 0.0 ? 0.17 : 0.19));
    float height = side < 0.0 ? 0.068 : 0.16;
    float v = s * 4.0 + 0.35 * vnoise(x * 14.0 + side * 5.0);
    float stepped = (floor(v) + smoothstep(0.62, 1.0, frac(v))) / 4.0;   // asinmis katman basamaklari
    float h = lerp(s, stepped, 0.65);
    h = min(h, 0.9 + 0.04 * vnoise(x * 9.0));                     // kenarda duz tepe
    return _Horizon - 0.014 + height * h + 0.004 * vnoise(x * 45.0 + side) + 0.0012 * vnoise(x * 160.0);
}

// Duvar yuzu: yuzden disari cikan dikey sirtlar (gunesli duvarda her sirtin batiya bakan yani parlar), yatay tortu katmanlari
// ve belirgin acik tabakalar, yuzu yaran dere yataklari, dibe dogru moloz yelpazesi.
// lit: 1 gunese bakan yuz (sag duvar), 0 golgedeki yuz (sol duvar). s: canyonRim'deki yakinlik.
float3 canyonWallFace(float2 sp, float top, float s, float lit, float onePx)
{
    float above = sp.y - _Horizon;
    float3 c = lerp(CANYON_SHADE, CANYON_LIT, lit);

    // dikey sirtlar: testere dislisi gibi; gunesli yuzde sirtin bir yani aydinlik, obur yani golgede
    float sx = sp.x * 24.0 + 1.6 * vnoise(above * 18.0 + sp.x * 3.0);
    float fs = frac(sx);
    float spurLit = smoothstep(0.0, 0.65, fs) * (1.0 - smoothstep(0.8, 0.98, fs));
    float spurShade = 0.5 + 0.5 * sin(sx * 6.2832);
    c *= lerp(0.80 + 0.36 * spurShade * 0.5, 0.66 + 0.48 * spurLit, lit);

    // katmanlar: ince bantlar + seyrek, belirgin acik tabakalar (basamaklarin ust yuzleri); hafif dalgali
    float wave = 0.004 * vnoise(sp.x * 10.0 + 3.0);
    float strata = sin((above + wave) * 220.0) * 0.5 + 0.5;
    float strata2 = sin((above + wave) * 75.0 + 0.7) * 0.5 + 0.5;
    float marker = smoothstep(0.80, 0.97, sin((above + wave) * 48.0 + 1.3));
    c *= 0.80 + 0.16 * strata + 0.14 * strata2;
    c = lerp(c, c * float3(1.35, 1.22, 1.08) + float3(0.04, 0.03, 0.02), marker);

    // dere yataklari: yuzu dikey yaran koyu oyuklar
    float g = vnoise2(float2(sp.x * 70.0, above * 6.0));
    float gully = smoothstep(0.6, 0.85, g) * smoothstep(0.0, 0.03, top - sp.y);
    c *= 1.0 - 0.4 * gully;

    // ust kenar: duvarlarin ust yuzu (mesa) gunes alir: ince sicak kenar
    float edge = 1.0 - smoothstep(0.0, 2.5 * onePx + 0.004 * (1.0 - lit), top - sp.y);
    c = lerp(c, float3(0.95, 0.68, 0.44), edge * lerp(0.75, 0.35, lit));

    // etek: dokulmus moloz yelpazesi; daha acik, puruzsuz
    float talusTop = _Horizon + 0.008 + 0.022 * s + 0.006 * vnoise(sp.x * 22.0);
    float talus = 1.0 - smoothstep(talusTop - 0.006, talusTop + 0.002, sp.y);
    float3 rubble = lerp(CANYON_SHADE * 1.25, CANYON_LIT * 0.9, lit) * (0.92 + 0.12 * vnoise2(sp * float2(260.0, 900.0)));
    c = lerp(c, rubble, talus);

    // uzaklik: ortaya (kanyonun icine) dogru duvar uzaklasir, puslanir
    return lerp(c, canyonSkyColor(0.0) * 0.92, (1.0 - s) * 0.25);
}

// Kanyonun icinde, uzakta kat kat duvarlar: her kat bir oncekinden alcak, dar ve soluk; orta cizgisi kivrilir.
#define CANYON_FAR 3
static const float kFarCenter[CANYON_FAR] = { 0.030, -0.012, 0.018 };   // 0 en uzak
static const float kFarWidth[CANYON_FAR]  = { 0.050, 0.085, 0.130 };
static const float kFarBase[CANYON_FAR]   = { -0.010, -0.014, -0.018 };
static const float kFarAmp[CANYON_FAR]    = { 0.030, 0.050, 0.075 };
static const float kFarHaze[CANYON_FAR]   = { 0.72, 0.55, 0.38 };

float canyonFarTop(int k, float x)
{
    float d = abs(x - kFarCenter[k]) / kFarWidth[k];
    float v = saturate(d) * 3.0 + 0.3 * vnoise(x * 30.0 + k * 4.0);
    float stepped = (floor(v) + smoothstep(0.6, 1.0, frac(v))) / 3.0;
    return _Horizon + kFarBase[k] + kFarAmp[k] * lerp(saturate(d), stepped, 0.5) + 0.002 * vnoise(x * 90.0 + k);
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
    float t = _SkyTime;

    // Gokyuzu: ikindi; ufukta sicak karamela
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = canyonSkyColor(skyT);
    float3 skyLow = canyonSkyColor(0.0);

    // Gunes: kucuk beyazimsi disk, yakin cevresi mavimsi (Mars'ta toz isigi boyle dagitir), genis hale altin
    float2 sunPos = float2(CANYON_SUN.x, _Horizon + CANYON_SUN.y);
    float dSun = length(sp - sunPos);
    col += float3(0.62, 0.42, 0.24) * exp(-dSun * 5.0) * 0.5;
    col = lerp(col, float3(0.78, 0.86, 1.0), exp(-dSun * 45.0) * 0.55);
    float disc = 1.0 - smoothstep(0.0075 - pixel, 0.0075 + pixel, dSun);
    col = lerp(col, float3(1.0, 0.97, 0.90), disc);

    // ince toz seritleri
    float hb = (uv.y - (_Horizon + 0.06)) / 0.08;
    float2 hp = float2(x * 4.0 - t * 0.04, uv.y * 16.0);
    float wisps = smoothstep(0.45, 0.85, fbm(hp + float2(fbm(hp * 0.6 + t * 0.02) * 1.5, 0.0)));
    col = lerp(col, float3(0.84, 0.62, 0.46), exp(-hb * hb) * wisps * 0.25);

    // Uzak kanyon: kat kat, en uzaktan yakina. Orta cizginin solu golgede, sagi gunese bakar.
    [unroll] for (int k = 0; k < CANYON_FAR; k++)
    {
        float top = canyonFarTop(k, x);
        float m = 1.0 - smoothstep(top - pixel, top + pixel, uv.y);
        float lit = smoothstep(-0.01, 0.01, x - kFarCenter[k]);
        float3 c = canyonWallFace(sp, top, 0.6, lit, onePx);
        c = lerp(c, lerp(skyLow, float3(0.62, 0.52, 0.56), 0.5), kFarHaze[k]);   // uzak: mavimsi-mor pus
        col = lerp(col, c, m);
    }

    // Kanyonun dibinde toz sisi: ortada, ufkun hemen altinda; cok yavas kayar
    float mb = (uv.y - (_Horizon - 0.012)) / 0.016;
    float mist = exp(-mb * mb) * (0.55 + 0.45 * fbm(float2(x * 18.0 - t * 0.02, uv.y * 40.0)));
    col = lerp(col, float3(0.74, 0.60, 0.56), mist * 0.55 * (1.0 - smoothstep(0.06, 0.16, abs(x - 0.01))));

    // Yakin yan duvarlar
    [unroll] for (int side = -1; side <= 1; side += 2)
    {
        float s;
        float top = canyonRim(x, side, s);
        float m = (1.0 - smoothstep(top - pixel, top + pixel, uv.y)) * smoothstep(0.0, 0.02, s);
        float3 c = canyonWallFace(sp, top, s, side > 0 ? 1.0 : 0.0, onePx);
        col = lerp(col, c, m);
    }

    // Ufuk hizasinda alcak toz pusu
    float lb = (uv.y - (_Horizon - 0.012)) / 0.03;
    col = lerp(col, float3(0.72, 0.52, 0.42), exp(-lb * lb) * 0.15);
    return col;
}

#endif
