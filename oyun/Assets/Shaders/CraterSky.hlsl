// Kraterli duzluk (Bolge 3) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, sekiller, yildizlar) kullanir.
// Safaktan hemen once, dev bir kraterin icindeyiz: ufukta kraterin uzak duvari (uzun, basamakli, neredeyse duz) ve ortasinda
// tek, sivri, katmanli bir dag (Mars'taki Gale krateri ve ortasindaki Sharp Dagi gibi). Gunes duvarin hemen ardinda;
// gokyuzundeki ince buz bulutlarini alttan pembe-altin boyar. Duzlukte kucuk kraterler (golgeli ic duvarlarinda sabah kiragisi),
// ufka uzanan gunes paneli siralari (camlari safagi yansitir) ve koloniye giden elektrik direkleri.
// Bolum 30 kapanisinda gunes duvarin ustunden dogar (_Sunrise 0 -> 1, RegionLook.Sunrise).
#ifndef MARSKOD_CRATER_INCLUDED
#define MARSKOD_CRATER_INCLUDED

static const float3 CRATER_PLAIN = float3(0.17, 0.11, 0.125);   // duvarin dibindeki duzlugun rengi (zeminin pusu bu renge solar)
static const float  CRATER_SUN_X = 0.075;
static const float  CRATER_PEAK_X = -0.07;
float _Sunrise;   // 0: safak oncesi, 1: gunes dogdu

float3 craterSkyColor(float t, float rise)   // t: 0 ufuk, 1 tepe
{
    float3 low = lerp(float3(0.36, 0.19, 0.22), float3(0.78, 0.52, 0.40), rise);
    float3 mid = lerp(float3(0.15, 0.10, 0.20), float3(0.46, 0.33, 0.35), rise);
    float3 top = lerp(float3(0.025, 0.025, 0.07), float3(0.18, 0.15, 0.22), rise);
    return t < 0.25 ? lerp(low, mid, t / 0.25) : lerp(mid, top, saturate((t - 0.25) / 0.75));
}

// Krater duvarinin ust kenari: uzun ve alcak; basamak basamak (sert ama yumusak gecisli), bir yerde centikli.
float craterRimTop(float x)
{
    float v = vnoise(x * 6.0 + 2.0) * 3.0;
    float terrace = (floor(v) + smoothstep(0.75, 1.0, frac(v))) / 3.0;
    float notch = (1.0 - smoothstep(0.0, 0.03, abs(x - 0.21))) * 0.007;
    return _Horizon - 0.006 + 0.011 * terrace + 0.0035 * vnoise(x * 18.0) + 0.001 * vnoise(x * 70.0) - notch;
}

// Ortadaki dag: genis, basamak basamak asinmis tortul katmanlardan bir tepe (piramit gibi duzgun degil); yuzunde ruzgarin
// oydugu vadiler, tepesi yuvarlak ve biraz solda.
float craterPeakTop(float x)
{
    float d = (x - CRATER_PEAK_X) / 0.15;
    d += 0.25 * d * d * sign(d) * 0.5;                                 // sag etek daha uzun
    float base = saturate(1.0 - d * d);
    float body = pow(base, 1.7);
    float v = body * 5.0;
    float stepped = (floor(v) + smoothstep(0.55, 1.0, frac(v))) / 5.0;   // asinmis katman basamaklari
    float h = lerp(body, stepped, 0.55);
    h -= 0.14 * pow(vnoise(x * 45.0 + 3.0), 3.0) * base;               // oyuk vadiler
    h += 0.05 * vnoise(x * 20.0 + 8.0) * base;
    return _Horizon - 0.006 + 0.044 * h;
}

// Koloniye uzanan elektrik direkleri: yakin (sag, buyuk) direkten uzak (sol, kucuk) direklere. i: 0 en yakin
float pylonDepth(float i) { return 0.06 / (1.0 + 0.9 * i); }
float pylonX(float i) { return -0.32 + 0.55 / (1.0 + 0.9 * i); }

// Direk silueti ve aralarindaki sarkik kablolar (maske 0..1)
float craterPylons(float2 sp, float plainTop, float onePx)
{
    float m = 0.0;
    [unroll] for (int i = 0; i < 6; i++)
    {
        float d0 = pylonDepth(i), d1 = pylonDepth(i + 1);
        float h0 = d0 * 0.55, h1 = d1 * 0.55;
        float x0 = pylonX(i), x1 = pylonX(i + 1);
        float b0 = plainTop - d0, b1 = plainTop - d1;
        float w = max(onePx * 0.7, h0 * 0.025);
        float arm0 = b0 + h0 * 0.88, arm1 = b1 + h1 * 0.88;
        float pole = min(sdSeg(sp, float2(x0, b0), float2(x0, b0 + h0), w),
                         sdSeg(sp, float2(x0 - h0 * 0.22, arm0), float2(x0 + h0 * 0.22, arm0), w * 0.8));
        m = max(m, 1.0 - smoothstep(-onePx, onePx, pole));
        // iki kablo: kol uclarindan bir sonraki diregin kol uclarina, ortada sarkar
        [unroll] for (int s = -1; s <= 1; s += 2)
        {
            float xa = x0 + s * h0 * 0.22, xb = x1 + s * h1 * 0.22;
            float tt = (sp.x - xa) / (xb - xa);
            if (tt > 0.0 && tt < 1.0)
            {
                float yc = lerp(arm0, arm1, tt) - h0 * 0.12 * 4.0 * tt * (1.0 - tt);
                m = max(m, (1.0 - smoothstep(0.0, onePx * 0.9, abs(sp.y - yc))) * 0.8);
            }
        }
    }
    return m;
}

float3 CraterBackdrop(float2 suv)
{
    float2 uv = MarsPictureUV(suv);
    float aspect = _SkyScreen.x / _SkyScreen.y;
    float x = (uv.x - 0.5) * aspect;
    float picturePx = _SkyScreen.y * max(_Picture.x, 1e-3);
    float pixel = 1.5 / picturePx;
    float onePx = 1.0 / picturePx;
    float2 sp = float2(x, uv.y);
    float t = _SkyTime;
    float sc = _SkyScreen.y / 1170.0;
    float rise = saturate(_Sunrise);
    float3 warm = float3(1.0, 0.55, 0.35);

    // Gunes: dogana kadar duvarin ardinda; dogarken yukselir
    float2 sunPos = float2(CRATER_SUN_X, craterRimTop(CRATER_SUN_X) - 0.016 + rise * 0.05);
    float dSun = length((sp - sunPos) * float2(0.8, 1.4));

    // Gokyuzu: ufukta pembe, yukarida lacivert; gunesin cevresi altin. Dogunca hepsi isinir.
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = craterSkyColor(skyT, rise);
    col += float3(0.75, 0.38, 0.22) * exp(-dSun * (7.0 - 3.0 * rise)) * (0.8 + 0.6 * rise);
    col += float3(0.40, 0.17, 0.20) * exp(-dSun * 2.2) * 0.5;
    float starMask = smoothstep(0.12, 0.5, skyT) * (1.0 - exp(-dSun * 3.0)) * (1.0 - rise);
    float3 starCol = lerp(float3(1.0, 0.93, 0.88), float3(0.85, 0.9, 1.0), vnoise2(uv * 40.0));
    col += starCol * stars(uv, 0.08 + 0.25 * smoothstep(0.4, 1.0, skyT), 14.0, 7.0, 1.0) * starMask * _StarsOn;

    // Ince buz bulutlari: ufkun ustunde yatay seritler, alttan safakla boyanir; cok yavas kayar
    float cb = (uv.y - (_Horizon + 0.055)) / 0.05;
    float band = exp(-cb * cb);
    if (band > 0.01)
    {
        float2 cp = float2(x * 2.6 - t * 0.004, uv.y * 34.0);
        float streak = smoothstep(0.5, 0.8, fbm(cp + float2(fbm(cp * 0.5) * 1.2, 0.0)));
        float3 cloudCol = lerp(float3(0.30, 0.21, 0.31), float3(1.0, 0.62, 0.48), saturate(exp(-dSun * 2.0) * 1.5 + rise * 0.4));
        col = lerp(col, cloudCol, streak * band * 0.75);
    }

    // Phobos: soluk, gun dogunca neredeyse kaybolur
    float4 phobos = rock(sp, float2(-0.16, 0.93), 0.0095, 2.0, pixel);
    col = lerp(col, phobos.rgb, phobos.a * (1.0 - 0.7 * rise));

    // Gunes diski (duvar onunu kapatir)
    float disc = 1.0 - smoothstep(0.0105 - pixel, 0.0105 + pixel, length(sp - sunPos));
    col = lerp(col, float3(1.0, 0.93, 0.80), disc);

    // Krater duvari + ortadaki dag
    float rimTop = craterRimTop(x);
    float peakTop = craterPeakTop(x);
    float wallTop = max(rimTop, peakTop);
    float mWall = 1.0 - smoothstep(wallTop - pixel, wallTop + pixel, uv.y);
    float3 wall = lerp(float3(0.19, 0.12, 0.16), float3(0.32, 0.21, 0.24), rise);
    float onPeak = smoothstep(rimTop - 0.002, rimTop + 0.002, uv.y) * step(rimTop, peakTop);
    float layers = sin((uv.y + vnoise(x * 30.0) * 0.0025) * 950.0) * 0.5 + 0.5;
    wall *= 1.0 + onPeak * 0.16 * (layers - 0.5);                    // dagin yuzunde tortul katmanlar
    wall += warm * onPeak * step(CRATER_PEAK_X, x) * 0.05;            // gunese bakan sag yamac
    float edge = 1.0 - smoothstep(0.0, 2.5 * onePx, wallTop - uv.y);
    wall += warm * edge * (0.08 + 0.9 * exp(-dSun * 5.0));           // ust kenar gunes yonunde parlar
    wall = lerp(wall, craterSkyColor(0.0, rise), 0.3);               // uzak: puslu
    col = lerp(col, wall, mWall);

    // Duzluk: duvarin dibinden asagi koyulasir; gunesin altinda sicak isik
    float plainTop = _Horizon - 0.016;
    float mPlain = 1.0 - smoothstep(plainTop - pixel, plainTop + pixel, uv.y);
    float depth = max(plainTop - uv.y, 0.0);
    float3 plain = lerp(CRATER_PLAIN, float3(0.07, 0.05, 0.07), smoothstep(0.0, 0.2, depth));
    plain += float3(0.30, 0.14, 0.10) * exp(-length(float2((x - sunPos.x) * 0.8, depth * 2.5)) * 6.0) * (0.35 + 0.5 * rise);

    // Kucuk kraterler (perspektif: uzakta kucuk ve yassi). Uzak ic duvar golgede, uzerinde kiragi; on dudak isik alir.
    if (depth > 0.0 && depth < 0.09)
    {
        float k = depth + 0.006;
        float2 g = float2(x / k * 2.16, log2(k) * 6.0);
        float2 id = floor(g);
        float2 dv = frac(g) - (0.3 + 0.4 * hash22(id + 9.0));
        float r = 0.18 + 0.2 * hash21(id + 2.0);
        float has = step(hash21(id + 4.0), 0.3);
        float dd = length(dv) / r;
        float bowl = 1.0 - smoothstep(0.85, 1.0, dd);
        float farWall = bowl * saturate(-dv.y / r);
        float lip = exp(-pow((dd - 1.0) / 0.18, 2.0)) * saturate(dv.y / r + 0.3);
        float3 crater = plain * (1.0 - 0.35 * bowl) + float3(0.40, 0.40, 0.52) * farWall * 0.30;
        plain = lerp(plain, crater, has) + warm * lip * 0.12 * has;
    }

    // Gunes paneli siralari: ufka dogru sikisan ince seritler; camlar gokyuzunu ve gunesi yansitir
    [unroll] for (int r = 0; r < 3; r++)
    {
        float rd = 0.007 * pow(1.8, r);
        float th = 0.0005 + rd * 0.045;
        float row = 1.0 - smoothstep(th - onePx, th + onePx, abs(depth - rd));
        float seg = step(0.2, frac(x / (rd + 0.006) * 4.0 + r * 0.37));   // paneller arasi bosluk
        float span = bandMask(x, -0.30, 0.22 - r * 0.02, 0.02) * step(0.45, vnoise(x / (rd + 0.006) * 0.5 + r * 3.0));   // tarla obek obek
        float3 glass = lerp(craterSkyColor(0.12, rise), float3(1.0, 0.75, 0.6), exp(-abs(x - sunPos.x) * 9.0) * 0.8) * 0.85;
        plain = lerp(plain, glass, row * seg * span);
    }
    col = lerp(col, plain, mPlain);

    // Elektrik direkleri ve uzakta koloninin isiklari (direkler oraya gider)
    float pyl = craterPylons(sp, plainTop, onePx);
    col = lerp(col, float3(0.05, 0.035, 0.05), pyl);
    float3 colony = float3(1.0, 0.72, 0.45) * (glowDot(sp, float2(-0.325, plainTop - 0.003), 1.1 * sc) + glowDot(sp, float2(-0.333, plainTop - 0.002), 0.9 * sc)
        + glowDot(sp, float2(-0.317, plainTop - 0.0035), 0.9 * sc) * 0.7);
    colony += float3(1.0, 0.6, 0.4) * glowDot(sp, float2(-0.325, plainTop - 0.003), 7.0 * sc) * 0.25;

    // Duvarin dibinde sabah sisi (kraterin icinde birikmis soguk hava)
    float mb = (uv.y - plainTop) / 0.012;
    float2 mp = float2(x * 6.0 - t * 0.01, uv.y * 30.0);
    float mist = exp(-mb * mb) * (0.3 + 0.7 * smoothstep(0.35, 0.8, fbm(mp)));
    col = lerp(col, lerp(float3(0.30, 0.22, 0.30), float3(0.75, 0.48, 0.40), exp(-dSun * 3.0) + rise * 0.3), mist * 0.35);

    col += colony;
    return col;
}

#endif
