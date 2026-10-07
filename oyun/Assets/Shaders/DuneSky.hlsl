// Kum tepeleri (Bolge 4) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, sekiller) kullanir.
// Oyundaki ilk gunduz (Bolge 3 gun dogumuyla bitti): Mars'in gercek gunduz gokyuzu, tozlu karamela rengi; yildiz yok.
// Gunes kucuk ve beyazimsi, cevresinde mavimsi bir hale (Mars'ta toz isigi boyle dagitir). Ufukta kat kat, keskin sirtli
// kum tepeleri: ruzgar yonunde yavas yukselir, sirttan sonra dik iner (kayma yuzu gunese bakar, aydinlik); sirtlardan kum savrulur.
// Uzak tepelerin ustunde yavasca dolasan toz seytanlari (kucuk kum hortumlari).
#ifndef MARSKOD_DUNE_INCLUDED
#define MARSKOD_DUNE_INCLUDED

static const float3 DUNE_PLAIN = float3(0.60, 0.41, 0.30);   // yakin tepelerin dibindeki puslu kumun rengi (zeminin pusu bu renge solar)
static const float2 DUNE_SUN = float2(0.15, 0.085);          // gunes: x, ufka gore yukseklik

float3 duneSkyColor(float t)   // t: 0 ufuk, 1 tepe
{
    float3 low = float3(0.72, 0.52, 0.40), mid = float3(0.47, 0.32, 0.26), top = float3(0.19, 0.14, 0.16);
    return t < 0.3 ? lerp(low, mid, t / 0.3) : lerp(mid, top, saturate((t - 0.3) / 0.7));
}

// Kum tepesi profili (0..1): soldan (ruzgar yonu) yavas yukselir, yumusak bir sirttan sonra daha dik iner. f: tepenin icindeki yer
static const float DUNE_CREST = 0.64;
float duneProfile(float u, out float f)
{
    float i = floor(u);
    f = frac(u);
    float w = f < DUNE_CREST ? 0.5 * f / DUNE_CREST : 0.5 + 0.5 * (f - DUNE_CREST) / (1.0 - DUNE_CREST);
    return pow(sin(w * 3.14159), 1.4) * (0.7 + 0.3 * hash11(i * 7.3 + 1.0));
}

#define DUNE_LAYERS 3
static const float kDuneBase[DUNE_LAYERS] = { 0.006, -0.010, -0.030 };   // ufka gore; 0 en uzak
static const float kDuneAmp[DUNE_LAYERS]  = { 0.018, 0.026, 0.040 };
static const float kDuneFreq[DUNE_LAYERS] = { 9.0, 5.5, 3.2 };
static const float kDuneSeed[DUNE_LAYERS] = { 1.3, 7.7, 3.1 };
static const float kDuneHaze[DUNE_LAYERS] = { 0.6, 0.4, 0.2 };

float duneCrest(int k, float x, out float f)
{
    float h = duneProfile(x * kDuneFreq[k] + kDuneSeed[k], f) + 0.06 * vnoise(x * kDuneFreq[k] * 4.0 + kDuneSeed[k]);
    return _Horizon + kDuneBase[k] + kDuneAmp[k] * h;
}

float3 DuneBackdrop(float2 suv)
{
    float2 uv = MarsPictureUV(suv);
    float aspect = _SkyScreen.x / _SkyScreen.y;
    float x = (uv.x - 0.5) * aspect;
    float picturePx = _SkyScreen.y * max(_Picture.x, 1e-3);
    float pixel = 1.5 / picturePx;
    float onePx = 1.0 / picturePx;
    float2 sp = float2(x, uv.y);
    float t = _SkyTime;

    // Gokyuzu: tozlu gunduz
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = duneSkyColor(skyT);
    float3 skyLow = duneSkyColor(0.0);

    // Gunes: kucuk beyazimsi disk, yakin cevresi mavimsi, genis hale sicak
    float2 sunPos = float2(DUNE_SUN.x, _Horizon + DUNE_SUN.y);
    float dSun = length(sp - sunPos);
    col += float3(0.55, 0.40, 0.30) * exp(-dSun * 6.0) * 0.45;
    col = lerp(col, float3(0.78, 0.86, 1.0), exp(-dSun * 45.0) * 0.6);
    float disc = 1.0 - smoothstep(0.0075 - pixel, 0.0075 + pixel, dSun);
    col = lerp(col, float3(1.0, 0.98, 0.94), disc);

    // Gokyuzunde suruklenen ince toz seritleri
    float hb = (uv.y - (_Horizon + 0.03)) / 0.06;
    float2 hp = float2(x * 4.0 - t * 0.05, uv.y * 18.0);
    float wisps = smoothstep(0.45, 0.85, fbm(hp + float2(fbm(hp * 0.6 + t * 0.02) * 1.5, 0.0)));
    col = lerp(col, float3(0.80, 0.60, 0.46), exp(-hb * hb) * wisps * 0.3);

    // Kum tepeleri: uzaktan yakina; araya (en uzak sira ustunde) toz seytanlari
    [unroll] for (int k = 0; k < DUNE_LAYERS; k++)
    {
        float f;
        float crest = duneCrest(k, x, f);
        float m = 1.0 - smoothstep(crest - pixel, crest + pixel, uv.y);
        float below = crest - uv.y;
        // kayma yuzu saga, gunese bakar; iki tepenin birlestigi cukurda golgeye yumusakca doner (renk siniri olusmasin)
        float slip = smoothstep(DUNE_CREST - 0.1, DUNE_CREST + 0.08, f) * (1.0 - smoothstep(0.86, 1.0, f));
        float3 c = lerp(float3(0.64, 0.42, 0.30), float3(0.86, 0.60, 0.42), slip);
        c *= 1.0 - 0.22 * smoothstep(0.0, kDuneAmp[k] * 1.2, below);        // dibe dogru koyulasir
        c += float3(1.0, 0.82, 0.62) * (1.0 - smoothstep(0.0, 2.0 * onePx, below)) * 0.22;   // sirt cizgisi
        if (k == DUNE_LAYERS - 1) c = lerp(c, DUNE_PLAIN, smoothstep(0.004, 0.04, below));    // en yakin sira: dibi duz kuma karisir
        c = lerp(c, skyLow, kDuneHaze[k]);
        col = lerp(col, c, m);

        // sirttan savrulan kum: tepenin en yuksek yerinden saga (ruzgar yonune) dogru incelen bulut
        float above = uv.y - crest;
        if (above > 0.0 && above < 0.02)
        {
            float plume = smoothstep(0.45, 0.8, fbm(float2((x - t * 0.03) * 60.0 + kDuneSeed[k], above * 400.0 - t * 0.5)));
            float nearCrest = exp(-above / (0.003 + kDuneAmp[k] * 0.12)) * exp(-max(f - DUNE_CREST, 0.0) * 10.0) * step(DUNE_CREST - 0.08, f);
            col = lerp(col, lerp(skyLow, float3(0.88, 0.64, 0.46), 0.5), plume * nearCrest * 0.5);
        }

        if (k == 0)
        {
            // toz seytanlari: en uzak siranin ustunde, yavasca yer degistirir
            [unroll] for (int j = 0; j < 2; j++)
            {
                float xc = 0.2 * sin(t * (0.02 + 0.012 * j) + j * 2.4) + (j == 0 ? -0.06 : 0.09);
                float fc;
                float baseY = duneCrest(0, xc, fc) - 0.004;
                float hgt = uv.y - baseY;
                const float H = 0.055;
                if (hgt > -0.002 && hgt < H)
                {
                    float w = 0.0022 + hgt * 0.10;
                    float lean = hgt * 0.25 * sin(t * 0.3 + j);
                    float dx = (x - xc - lean) / w;
                    float body = (1.0 - smoothstep(0.15, 1.0, abs(dx))) * smoothstep(-0.002, 0.006, hgt) * (1.0 - smoothstep(H * 0.4, H, hgt));
                    float swirl = 0.85 + 0.15 * sin(hgt * 500.0 - t * 4.0 + dx * 2.0);
                    col = lerp(col, float3(0.80, 0.60, 0.46), body * swirl * 0.3);
                }
            }
        }
    }

    // Ufuk hizasinda alcak toz pusu
    float lb = (uv.y - (_Horizon - 0.01)) / 0.03;
    col = lerp(col, float3(0.76, 0.56, 0.42), exp(-lb * lb) * 0.18);
    return col;
}

#endif
