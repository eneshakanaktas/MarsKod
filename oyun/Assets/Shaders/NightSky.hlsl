// Gece ovasi (Bolge 10) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, yildizlar) kullanir.
// Renkler Bolge 10 paletinden (docs/superpowers/plans/2026-11-15-bolge-9-10-gorunus.md): murdum gece (onceki bolgelerin lacivert ve
// lavantasi degil), ufukta zumrut yesili kutup isigi (Mars'ta gercek), ayni genel yonden dusen yesil meteorlar, gokteki TEK mavi sey
// Dunya (76'daki gizli yildizla ayni mavi). Ufukta volkan (Bolge 9'dakinden yakin), eteginde 97'den itibaren kapinin silueti.
// _NightScene (NightPlainTraces): 0 = 91-92, 1 = 93-96 (mavi nokta fark edilir: daha parlak), 2 = 97-100 (kapinin silueti).
#ifndef MARSKOD_NIGHT_INCLUDED
#define MARSKOD_NIGHT_INCLUDED

float _NightScene;
float _VolcanoBurst;   // bolum girisi (LevelArrival, VolcanoSky.cs): 0 kapali, 0-1 patlamanin ilerleyisi

static const float3 NIGHT_PLAIN = float3(0.20, 0.15, 0.23);    // ufuk dibindeki ova: zeminin uzaktaki rengi (koyu murdum-gri)
static const float2 NIGHT_EARTH = float2(-0.115, 0.10);         // Dunya: x, ufka gore yukseklik (sol dugmelerin sagi, basligin alti; volkanin solu)
static const float3 NIGHT_EARTH_COL = float3(0.357, 0.561, 0.878);   // #5B8FE0
static const float3 NIGHT_AURORA = float3(0.18, 0.91, 0.42);    // #2EE86A
static const float3 NIGHT_AURORA_SOFT = float3(0.09, 0.72, 0.31);   // #18B850
static const float NIGHT_VOLCANO_X = 0.06;

float3 nightSkyColor(float t)   // t: 0 ufuk, 1 tepe. Ufukta murdum, yukari koyulasir
{
    float3 low = float3(0.23, 0.094, 0.25), mid = float3(0.14, 0.06, 0.16), top = float3(0.06, 0.03, 0.075);
    return t < 0.3 ? lerp(low, mid, t / 0.3) : lerp(mid, top, saturate((t - 0.3) / 0.7));
}

float nightVolcanoTop(float x)
{
    const float h = 0.10;
    float d = abs(x - NIGHT_VOLCANO_X) / (h * 4.0);
    float cone = min(h * saturate(1.0 - d), h * 0.86);
    return _Horizon + cone * (0.97 + 0.03 * vnoise(x * 70.0));
}

// Kutup isigi: ufkun ustunde yavasca dalgalanan perde; dikey isinlar, alt kenari parlak, yukari soner
float3 nightAurora(float2 sp, float t)
{
    float above = sp.y - _Horizon;
    float base = 0.03 + 0.03 * fbm(float2(sp.x * 1.6 + t * 0.015, 3.0));      // perdenin alt kenari
    float height = 0.16 + 0.10 * fbm(float2(sp.x * 1.1 - t * 0.01, 9.0));
    float y = (above - base) / height;
    float curtain = smoothstep(-0.05, 0.05, y) * (1.0 - smoothstep(0.1, 1.0, y));
    float rays = 0.55 + 0.45 * vnoise(sp.x * 70.0 + t * 0.3) * vnoise(sp.x * 23.0 - t * 0.2);
    float bright = smoothstep(0.25, 0.75, fbm(float2(sp.x * 0.9 + t * 0.02, 1.0)));   // perde yer yer kalin, yer yer bos
    float edge = exp(-pow(y / 0.06, 2.0));                                            // alt kenar: daha parlak
    float a = curtain * rays * (0.25 + 0.75 * bright);
    return NIGHT_AURORA_SOFT * a * 0.62 + NIGHT_AURORA * edge * bright * 0.45;
}

// Meteorlar: hep ayni genel yonden (sag ustten sol asagiya) duser; kisa omurlu yesil cizgiler
float3 nightMeteors(float2 sp, float t, float pixel)
{
    float3 c = 0.0;
    const float2 dir = float2(-0.55, -0.835);
    [unroll] for (int k = 0; k < 4; k++)
    {
        float rate = 0.11 + 0.05 * hash11(k * 7.0 + 1.0);
        float cycle = t * rate + hash11(k * 3.0 + 5.0);
        float phase = frac(cycle);
        float idx = floor(cycle);
        float life = 0.10;
        if (phase > life) continue;
        float2 start = float2(-0.35 + 0.8 * hash11(idx * 13.0 + k), _Horizon + 0.30 + 0.25 * hash11(idx * 17.0 + k * 3.0));
        float p = phase / life;
        float2 head = start + dir * (0.05 + 0.20 * p);
        float2 tail = head - dir * 0.08;
        float2 ba = head - tail;
        float h = saturate(dot(sp - tail, ba) / dot(ba, ba));
        float d = length(sp - tail - ba * h);
        float streak = exp(-pow(d / (pixel * 0.9), 2.0)) * h * h * (1.0 - p * 0.6);
        c += lerp(NIGHT_AURORA, float3(0.72, 1.0, 0.82), h * h) * streak * 1.4;
    }
    return c;
}

// Volkan patlamasi (bolum girisi): agizda kizil parlama, yukari fiskiran ve geri dusen lav damlalari. b: patlamanin ilerleyisi (0-1)
static const float NIGHT_VOLCANO_TOP = 0.084;
float3 nightEruption(float2 sp, float b, float sc)
{
    float2 top = float2(NIGHT_VOLCANO_X, _Horizon + NIGHT_VOLCANO_TOP);
    float flare = sin(saturate(b) * 3.14159);
    float3 lava = float3(1.0, 0.42, 0.10);
    float3 c = lava * (glowDot(sp, top, 5.0 * sc) * 1.8 + glowDot(sp, top, 20.0 * sc) * 0.55) * flare;
    [unroll] for (int i = 0; i < 16; i++)
    {
        float fi = i;
        float start = hash11(fi * 7.31) * 0.45;
        float k = saturate((b - start) / 0.5);
        float ang = lerp(-0.75, 0.75, hash11(fi * 3.71));
        float speed = lerp(0.07, 0.15, hash11(fi * 5.13));
        float2 p = top + float2(sin(ang) * speed * k, cos(ang) * speed * k - 0.22 * k * k);
        float alive = step(0.001, k) * (1.0 - step(0.999, k));
        c += lerp(lava, float3(1.0, 0.85, 0.5), hash11(fi * 2.3)) * glowDot(sp, p, 1.8 * sc) * 1.4 * alive;
    }
    return c;
}

float3 NightBackdrop(float2 suv)
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

    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = nightSkyColor(skyT);

    // Yildizlar: cok ve sik; Samanyolu gibi capraz, biraz yogun bir serit
    float band = exp(-pow((sp.y - _Horizon - 0.45 - sp.x * 0.5) / 0.12, 2.0));
    float starMask = smoothstep(0.05, 0.3, skyT);
    float3 starCol = lerp(float3(1.0, 0.95, 0.98), float3(0.92, 1.0, 0.95), vnoise2(uv * 30.0));
    col += starCol * stars(uv, 0.18 + 0.25 * band + 0.15 * smoothstep(0.4, 1.0, skyT), 12.0, 7.0, 1.0) * starMask * _StarsOn;
    col += (shootingStar(sp, t, 5.0, 0.53, onePx) + shootingStar(sp, t, 8.0, 0.29, onePx)) * starMask * _StarsOn;   // meteor yagmuru gecesi: sik kayan yildiz
    col += float3(0.10, 0.06, 0.12) * band * starMask * 0.6;

    col += nightAurora(sp, t);
    col += nightMeteors(sp, t, pixel) * _StarsOn;

    // Dunya: gokteki tek mavi nokta; 93'ten itibaren daha parlak (fark edilir)
    float earthOn = _NightScene > 0.5 ? 1.0 : 0.6;
    float2 earth = float2(NIGHT_EARTH.x, _Horizon + NIGHT_EARTH.y);
    col += NIGHT_EARTH_COL * (glowDot(sp, earth, 3.2 * sc) * 1.8 + glowDot(sp, earth, 12.0 * sc) * 0.45) * earthOn;

    // Volkan: koyu murdum siluet, tepesi kutup isigindan hafif yesil
    float vTop = nightVolcanoTop(x);
    float mV = (1.0 - smoothstep(vTop - pixel, vTop + pixel, uv.y)) * step(_Horizon - 0.01, uv.y);
    float3 volcano = float3(0.075, 0.035, 0.085);
    float rim = 1.0 - smoothstep(0.0, 4.0 * pixel, vTop - uv.y);
    volcano += NIGHT_AURORA_SOFT * rim * 0.18;
    // 97-100: eteginde kapinin silueti (isiksiz; yalniz kenari kutup isigini yakalar)
    float2 door = sp - float2(NIGHT_VOLCANO_X + 0.012, _Horizon + 0.008);
    float dDoor = sdBox(door, float2(0.0045, 0.006));
    float doorOn = _NightScene > 1.5 ? 1.0 : 0.0;
    volcano = lerp(volcano, float3(0.035, 0.02, 0.04), (1.0 - smoothstep(-pixel, pixel, dDoor)) * doorOn);
    volcano += NIGHT_AURORA_SOFT * exp(-pow(dDoor / (pixel * 1.2), 2.0)) * 0.35 * doorOn;
    // patlarken yamaclardan lav iner gibi tepesi kizarir
    volcano += float3(0.75, 0.18, 0.05) * exp(-max(_Horizon + NIGHT_VOLCANO_TOP - uv.y, 0.0) / 0.025) * sin(saturate(_VolcanoBurst) * 3.14159);
    col = lerp(col, volcano, mV);
    col += nightEruption(sp, _VolcanoBurst, sc);

    // Ova: ufukta murdum-gri, asagi koyulasir; ufkun dibinde kutup isiginin soluk yesil yansimasi
    float depth = max(_Horizon - uv.y, 0.0);
    float3 plain = lerp(NIGHT_PLAIN, float3(0.12, 0.08, 0.13), smoothstep(0.0, 0.2, depth));
    plain += NIGHT_AURORA_SOFT * exp(-depth * 60.0) * 0.08;
    float below = 1.0 - smoothstep(_Horizon - 0.004, _Horizon + 0.003, uv.y);
    return lerp(col, plain, below);
}

#endif
