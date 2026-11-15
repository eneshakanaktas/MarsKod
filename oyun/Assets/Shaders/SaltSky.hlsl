// Tuz golu (Bolge 9) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, sekiller) kullanir.
// Renkler Bolge 9 paletinden (docs/superpowers/plans/2026-11-15-bolge-9-10-gorunus.md): onceki bolgelerde olmayan sampanya-altin
// gok, sedef beyazi gol, zeytin tonu golge. Gunduz; gunes solda, yuksekte. Ufuk kaybolur: gok ile gol ufukta ayni acik altina
// karisir, gol gogu yansitir. Ufukta volkan silueti (oklarin gosterdigi dag): _SaltScene buyuklugu (0 uzak/kucuk, 1 yakin/buyuk;
// SaltLakeTraces). Golde volkanin soluk, ters yansimasi.
#ifndef MARSKOD_SALT_INCLUDED
#define MARSKOD_SALT_INCLUDED

float _SaltScene;   // volkanin yakinligi: 81'de ~0,2, 88'de ~0,6, 90'da ~0,85 (SaltLakeTraces)

static const float3 SALT_PLAIN = float3(0.95, 0.92, 0.80);    // ufuk dibindeki gol: zeminin uzaktaki rengi (gogun ufkuyla neredeyse ayni)
static const float2 SALT_SUN = float2(-0.27, 0.30);           // gunes: x, ufka gore yukseklik (solda, yuksekte; ust yazilarin altinda)
static const float SALT_VOLCANO_X = 0.07;

float3 saltSkyColor(float t)   // t: 0 ufuk, 1 tepe. Ufukta neredeyse beyaz krem, ortada sampanya-altin, tepede koyu zeytin-altin (ust yazilar okunsun)
{
    float3 low = float3(1.00, 0.965, 0.84), mid = float3(0.95, 0.886, 0.66), top = float3(0.36, 0.33, 0.20);
    return t < 0.32 ? lerp(low, mid, t / 0.32) : lerp(mid, top, saturate((t - 0.32) / 0.68));
}

// Volkan: genis etekli, tepesi kesik koni; ust kenari hafif dalgali. h: yukseklik (ufka gore)
float saltVolcanoTop(float x, float h)
{
    float w = h * 4.2;
    float d = abs(x - SALT_VOLCANO_X) / w;
    float cone = h * saturate(1.0 - d) * (1.0 - 0.15 * smoothstep(0.0, 0.18, 0.18 - d));   // tepe kesik (krater)
    cone = min(cone, h * 0.86);
    return _Horizon + cone * (0.97 + 0.03 * vnoise(x * 60.0));
}

float3 SaltBackdrop(float2 suv)
{
    float2 uv = MarsPictureUV(suv);
    float aspect = _SkyScreen.x / _SkyScreen.y;
    float x = (uv.x - 0.5) * aspect;
    float picturePx = _SkyScreen.y * max(_Picture.x, 1e-3);
    float pixel = 1.5 / picturePx;
    float2 sp = float2(x, uv.y);

    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = saltSkyColor(skyT);

    // Gunes: kucuk beyaz-altin disk, cevresinde genis altin hale
    float2 sunPos = float2(SALT_SUN.x, _Horizon + SALT_SUN.y);
    float dSun = length(sp - sunPos);
    col += float3(0.45, 0.38, 0.12) * exp(-dSun * 5.0) * 0.35;
    col = lerp(col, float3(1.0, 0.96, 0.78), exp(-dSun * 40.0) * 0.6);
    col = lerp(col, float3(1.0, 1.0, 0.95), 1.0 - smoothstep(0.007 - pixel, 0.007 + pixel, dSun));

    // Ince, yuksek bulut izleri (altin pus): tepede gorunmez
    float wisp = smoothstep(0.55, 0.85, fbm(float2(x * 2.2 + _SkyTime * 0.004, uv.y * 9.0)));
    col = lerp(col, float3(1.0, 0.97, 0.86), wisp * 0.25 * (1.0 - smoothstep(0.25, 0.6, skyT)));

    // Volkan: uzakta, pusla gogun rengine yakin (zeytin-gri); yaklastikca koyulasir
    float h = lerp(0.018, 0.075, saturate(_SaltScene));
    float vTop = saltVolcanoTop(x, h);
    float haze = lerp(0.62, 0.30, saturate(_SaltScene));
    float3 volcano = lerp(float3(0.49, 0.49, 0.36), saltSkyColor(0.0), haze);
    float lit = smoothstep(SALT_VOLCANO_X + 0.02, SALT_VOLCANO_X - 0.06, x);   // gunese (sola) bakan yamac biraz acik
    volcano *= 0.92 + 0.12 * lit;
    float mV = (1.0 - smoothstep(vTop - pixel, vTop + pixel, uv.y)) * step(_Horizon, uv.y);
    col = lerp(col, volcano, mV);

    // Gol: ufkun altinda gogun yansimasi; ufuk cizgisi yumusak (kaybolur). Asagi indikce sedefe doner.
    float depth = max(_Horizon - uv.y, 0.0);
    float3 mirrorSky = saltSkyColor(saturate(depth * 4.0));
    float3 lake = lerp(mirrorSky, SALT_PLAIN, smoothstep(0.0, 0.06, depth));
    // volkanin ters yansimasi: soluk, biraz kisa (ufkun altinda, volkanin boyunun 0,8'i kadar)
    float reflH = (vTop - _Horizon) * 0.8;
    float mR = (1.0 - smoothstep(-pixel, pixel, depth - reflH)) * step(uv.y, _Horizon) * step(1e-4, reflH);
    lake = lerp(lake, lerp(volcano, lake, 0.55), mR);
    lake *= 0.98 + 0.03 * vnoise2(float2(x * 30.0, uv.y * 90.0));
    float below = 1.0 - smoothstep(_Horizon - 0.006, _Horizon + 0.002, uv.y);
    return lerp(col, lake, below);
}

#endif
