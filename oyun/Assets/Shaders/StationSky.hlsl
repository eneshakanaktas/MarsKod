// Arastirma istasyonu (Bolge 8) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, sekiller) kullanir.
// _StationScene (StationTraces): 0 = 71 kapi onu, disarisi. Kanyonun ucu, ogleden sonra: Bolge 7'nin sabahindan sonraki gun.
//   Gunes solda, yuksekce; iki yandaki kanyon duvarlari ortada ufka iner (ortayi istasyonun 3B cephesi kapatir). Sol duvarin
//   ortaya bakan yuzu golgede, sag duvarin yuzu gunes alir (sicak turuncu). Gok ufukta acik kum, tepede soluk bej-pembe, en ustte koyu.
//   1 = 72-80 ic mekan. Bu dal yalniz 3B duvarlarin (StationRoom) kapatamadigi yerde gorunur: tavan. Koyu serin gri-mavi, birkac
//   kalin kiris, sarkan kablo, uzakta tek tuk sonuk gosterge isigi. Gokyuzu yok.
#ifndef MARSKOD_STATION_INCLUDED
#define MARSKOD_STATION_INCLUDED

static const float3 STATION_SAND = float3(0.50, 0.37, 0.29);   // 0: kanyon ucunun kumu, ufka dogru (zeminin uzaktaki rengi)
static const float3 STATION_DARK = float3(0.13, 0.13, 0.15);   // 1: ic mekanin koyu zemini (zemin buna solar)
static const float2 STATION_SUN = float2(-0.32, 0.12);         // 0: gunes (x, ufka gore yukseklik): solda, yuksekce, kadrajin disinda (yalniz halesi)

static const float3 STATION_WALL_SHADE = float3(0.40, 0.27, 0.23);   // golgedeki kanyon duvari
static const float3 STATION_WALL_LIT = float3(0.86, 0.56, 0.38);     // gunes alan duvar

bool StationIndoor() { return _StationScene > 0.5; }

float3 StationPlain() { return StationIndoor() ? STATION_DARK : STATION_SAND; }

float3 stationSkyColor(float t)   // t: 0 ufuk, 1 tepe. Ogleden sonra: ufukta acik kum, ortada soluk bej-pembe, tepede koyu (ust yazilar okunsun)
{
    float3 low = float3(0.86, 0.71, 0.57), mid = float3(0.70, 0.55, 0.50), top = float3(0.26, 0.21, 0.23);
    return t < 0.3 ? lerp(low, mid, t / 0.3) : lerp(mid, top, saturate((t - 0.3) / 0.7));
}

// Yan duvarin ust kenari. side: -1 sol, +1 sag. Kanyonun ucu: kenarda yuksek, ortaya dogru basamaklarla ufka iner.
float stationSideTop(float x, float side)
{
    float s = saturate((x * side - 0.03) / 0.2);                     // 0 ortada, 1 kenarda
    float v = s * 3.0 + 0.25 * vnoise(x * 9.0 + side * 3.0);
    float stepped = (floor(v) + smoothstep(0.0, 0.5, frac(v))) / 3.0;
    return _Horizon + 0.002 + 0.075 * stepped * (0.9 + 0.1 * vnoise(floor(v) * 3.1 + side)) + 0.0008 * vnoise(x * 70.0 + side);
}

// Kanyon duvarinin yuzu: yatay tortu katmanlari; lit: 0 golgede, 1 gunes aliyor. Ust kenar her iki duvarda da isigi yakalar.
float3 stationWallFace(float2 sp, float top, float lit, float onePx)
{
    float above = sp.y - _Horizon;
    float wave = 0.0015 * vnoise(sp.x * 6.0 + 1.0);
    float layers = 0.88 + 0.12 * (sin((above + wave) * 360.0) * 0.5 + 0.5);
    float3 c = lerp(STATION_WALL_SHADE, STATION_WALL_LIT, lit) * layers;
    c = lerp(c, c * 1.12, smoothstep(0.7, 0.95, sin((above + wave) * 140.0 + 1.3)) * 0.6);   // acik tabaka
    float edge = 1.0 - smoothstep(0.0, 2.0 * onePx, top - sp.y);
    c = lerp(c, STATION_WALL_LIT, edge * 0.7);
    float talus = 1.0 - smoothstep(0.002, 0.012, above + 0.002 * vnoise(sp.x * 30.0));   // etek: kuma karisir
    return lerp(c, lerp(c, STATION_SAND, 0.6), talus);
}

float3 stationOutside(float2 uv, float x, float pixel, float onePx)
{
    float2 sp = float2(x, uv.y);
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = stationSkyColor(skyT);

    // Gunes: kucuk, beyaz disk; cevresinde sicak, genis hale (havadaki toz)
    float2 sunPos = float2(STATION_SUN.x, _Horizon + STATION_SUN.y);
    float dSun = length(sp - sunPos);
    col += float3(0.50, 0.36, 0.24) * exp(-dSun * 6.0) * 0.35;
    col = lerp(col, float3(1.0, 0.93, 0.82), exp(-dSun * 45.0) * 0.55);
    col = lerp(col, float3(1.0, 0.99, 0.96), 1.0 - smoothstep(0.006 - pixel, 0.006 + pixel, dSun));

    // Iki yandaki duvar: sag duvarin yuzu gunese (sola) bakar
    [unroll] for (int side = -1; side <= 1; side += 2)
    {
        float top = stationSideTop(x, side);
        float m = 1.0 - smoothstep(top - pixel, top + pixel, uv.y);
        float3 c = stationWallFace(sp, top, side > 0 ? 1.0 : 0.15, onePx);
        col = lerp(col, c, m);
    }

    float below = 1.0 - smoothstep(_Horizon - 0.004, _Horizon + 0.003, uv.y);
    return lerp(col, STATION_SAND, below);
}

float3 stationInside(float2 uv, float x, float pixel, float onePx)
{
    float above = uv.y - _Horizon;
    float3 col = lerp(float3(0.11, 0.12, 0.15), float3(0.06, 0.065, 0.085), saturate(above / 0.25));
    col *= 0.94 + 0.06 * vnoise2(float2(x * 40.0, uv.y * 12.0));
    // yatay panel derzleri
    col *= 1.0 - 0.18 * (1.0 - smoothstep(0.0, onePx * 1.5, abs(frac(uv.y * 22.0) - 0.5) - 0.47));
    // kalin kirisler: koyu, alt kenari hafif kehribar yansima
    [unroll] for (int k = 0; k < 3; k++)
    {
        float y = _Horizon + 0.05 + k * 0.07;
        float d = uv.y - y;
        float beam = 1.0 - smoothstep(0.012 - pixel, 0.012 + pixel, abs(d));
        col = lerp(col, float3(0.05, 0.05, 0.06), beam);
        col += float3(0.22, 0.14, 0.06) * (1.0 - smoothstep(0.0, 2.5 * onePx, abs(d + 0.012))) * 0.4;
    }
    // sarkan kablolar: kirisin altindan asagi kivrilan ince koyu cizgiler
    [unroll] for (int c = 0; c < 3; c++)
    {
        float cx = -0.16 + c * 0.13 + 0.02 * hash11(c + 3.0);
        float y0 = _Horizon + 0.038 + c * 0.07;
        float t = saturate((cx + 0.05 - x) / 0.05);
        float cy = y0 - 0.02 * sin(t * 3.1416);
        float cable = (1.0 - smoothstep(0.0, pixel * 1.2, abs(uv.y - cy))) * step(cx, x) * step(x, cx + 0.05);
        col = lerp(col, float3(0.03, 0.03, 0.035), cable * 0.9);
    }
    // uzakta tek tuk sonuk gosterge isiklari
    float2 cell = floor(float2(x, uv.y) * float2(28.0, 28.0));
    float2 local = frac(float2(x, uv.y) * 28.0) - 0.5;
    float has = step(0.93, hash21(cell + 7.0));
    float3 lampCol = hash21(cell + 11.0) > 0.5 ? float3(1.0, 0.62, 0.24) : float3(0.9, 0.22, 0.16);
    col += lampCol * has * exp(-dot(local, local) * 60.0) * 0.35;

    float below = 1.0 - smoothstep(_Horizon - 0.004, _Horizon + 0.003, uv.y);
    return lerp(col, STATION_DARK, below);
}

float3 StationBackdrop(float2 suv)
{
    float2 uv = MarsPictureUV(suv);
    float aspect = _SkyScreen.x / _SkyScreen.y;
    float x = (uv.x - 0.5) * aspect;
    float picturePx = _SkyScreen.y * max(_Picture.x, 1e-3);
    float pixel = 1.5 / picturePx;
    float onePx = 1.0 / picturePx;
    return StationIndoor() ? stationInside(uv, x, pixel, onePx) : stationOutside(uv, x, pixel, onePx);
}

#endif
