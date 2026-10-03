// Kutup buzulu (Bolge 2) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu, sekiller, yildizlar) kullanir.
// Mars'ta gun batimi mavidir: gunes uzak buz ucurumlarinin ardina batarken cevresi mavi parlar, gokyuzunun geri kalani los.
// Ufukta kat kat buz ucurumlari (Mars kutbundaki katmanli buz), onlerinde duz buz ovasi. Ovada saplanip kalmis bir aracin
// yardim isigi yanip soner, ondan sinyal halkalari yayilir (Bolum 10'da telsizin yakaladigi sinyal). Koloni burada gorunmez.
#ifndef MARSKOD_POLAR_INCLUDED
#define MARSKOD_POLAR_INCLUDED

static const float3 POLAR_PLAIN = float3(0.25, 0.29, 0.39);   // ufuk dibindeki buz ovasinin rengi (zeminin pusu da bu renge solar)
static const float  POLAR_SUN_X = 0.11;
static const float2 POLAR_BEACON = float2(-0.12, -0.042);      // saplanan arac: ufka gore
float _PolarRoverHere;   // 1: arac alanin arkasinda (Traces/PolarTraces, Bolum 14-20); ufuktaki uzak hali gizlenir

float3 polarSkyColor(float t)   // t: 0 ufuk, 1 tepe
{
    float3 low = float3(0.27, 0.32, 0.46), mid = float3(0.12, 0.14, 0.25), top = float3(0.025, 0.035, 0.09);
    return t < 0.3 ? lerp(low, mid, t / 0.3) : lerp(mid, top, saturate((t - 0.3) / 0.7));
}

// Uzak ve yakin ucurum sirasinin ust kenari (fotograf koordinatinda). Duz tepeli yaylalar, aralarinda dik inisler.
float polarFarTop(float x)
{
    // yer yer yuksek yaylalar, aralarinda alcak, yumusak sirtlar
    float plateau = smoothstep(0.48, 0.56, vnoise(x * 3.4 + 7.0));
    return _Horizon - 0.022 + 0.040 * plateau + 0.012 * vnoise(x * 7.0 + 3.0) + 0.0015 * vnoise(x * 40.0);
}
float polarNearTop(float x)
{
    // yalnizca yanlarda: ortada gunes ve ova acik kalsin
    float side = smoothstep(0.14, 0.24, abs(x));
    float plateau = smoothstep(0.35, 0.5, vnoise(x * 3.1 + 21.0));
    return _Horizon - 0.032 + side * (0.012 + 0.030 * plateau) + 0.003 * vnoise(x * 30.0 + 5.0);
}

// Ucurum yuzu: kat kat (acik buz / tozlu koyu seritler), ustte kar, ust kenari gunes yonunde parlar
float3 polarCliffFace(float2 sp, float top, float3 ice, float3 dust, float dSun, float onePx)
{
    float depth = top - sp.y;
    float wobble = vnoise(sp.x * 18.0) * 0.004;
    float layer = sin((sp.y + wobble) * 600.0 + vnoise(sp.y * 900.0) * 2.0) * 0.5 + 0.5;
    float3 c = lerp(ice, dust, smoothstep(0.55, 0.95, layer) * 0.3);
    c = lerp(float3(0.62, 0.70, 0.84), c, smoothstep(0.0, 0.006, depth));   // tepedeki kar ortusu
    float rim = 1.0 - smoothstep(0.0, 2.5 * onePx, depth);
    c += float3(0.55, 0.72, 1.0) * rim * (0.15 + 0.9 * exp(-dSun * 6.0));
    return c;
}

// Saplanan arac silueti (bir yana yatik): govde, kabin, anten. q: aracin dibine gore
float polarRoverShape(float2 q)
{
    float a = 0.14;
    q = float2(q.x * cos(a) + q.y * sin(a), -q.x * sin(a) + q.y * cos(a));
    float d = sdBox(q - float2(0.0, 0.0035), float2(0.011, 0.0028)) - 0.0008;
    d = min(d, sdBox(q - float2(-0.004, 0.0078), float2(0.0045, 0.0022)) - 0.0006);
    d = min(d, sdSeg(q, float2(0.006, 0.006), float2(0.008, 0.017), 0.0004));
    d = min(d, sdCircle(q - float2(-0.008, 0.0006), 0.0021));
    d = min(d, sdCircle(q - float2(0.008, 0.0006), 0.0021));
    return d;
}

float3 PolarBackdrop(float2 suv)
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

    // Gunes: uzak ucurumun kenarina yarisi gomulu
    float2 sunPos = float2(POLAR_SUN_X, polarFarTop(POLAR_SUN_X) + 0.002);
    float dSun = length((sp - sunPos) * float2(0.8, 1.4));

    // Gokyuzu: los, ufukta mavi-gri; gunesin cevresi mavi parlar
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = polarSkyColor(skyT);
    col = lerp(col, float3(0.17, 0.15, 0.20), 0.35 * smoothstep(0.1, 0.5, dSun) * (1.0 - smoothstep(0.3, 0.8, skyT)));
    float starMask = smoothstep(0.15, 0.55, skyT) * (1.0 - exp(-dSun * 3.0));
    float3 starCol = lerp(float3(1.0, 0.95, 0.9), float3(0.85, 0.9, 1.0), vnoise2(uv * 40.0));
    col += starCol * stars(uv, 0.10 + 0.25 * smoothstep(0.4, 1.0, skyT), 14.0, 3.0, 1.0) * starMask * _StarsOn;
    col += float3(0.30, 0.52, 0.95) * exp(-dSun * 11.0) * 0.9 + float3(0.16, 0.28, 0.55) * exp(-dSun * 3.5) * 0.35;
    float disc = 1.0 - smoothstep(0.0105 - pixel, 0.0105 + pixel, length(sp - sunPos));
    col = lerp(col, float3(0.92, 0.97, 1.0), disc);

    // Mars'in uydulari (ovadaki gibi)
    float4 phobos = rock(sp, float2(0.14, 0.935), 0.0105, 1.0, pixel); col = lerp(col, phobos.rgb * float3(0.85, 0.9, 1.05), phobos.a);
    col += float3(0.95, 0.95, 1.0) * glowDot(sp, float2(-0.17, 0.885), 1.6 * sc) * 0.9 * starMask;

    // Uzak ucurumlar: puslu, gokyuzune yakin renkte
    float farTop = polarFarTop(x);
    float mFar = 1.0 - smoothstep(farTop - pixel, farTop + pixel, uv.y);
    float3 farFace = polarCliffFace(sp, farTop, float3(0.36, 0.43, 0.58), float3(0.25, 0.25, 0.32), dSun, onePx);
    col = lerp(col, lerp(farFace, polarSkyColor(0.0), 0.45), mFar);

    // Buz ovasi: ufukta POLAR_PLAIN, asagi indikce hafifce koyulasir; gunesin altinda parlak bir yansima seridi
    float plainTop = _Horizon - 0.032;
    float mPlain = 1.0 - smoothstep(plainTop - pixel, plainTop + pixel, uv.y);
    float depth = max(plainTop - uv.y, 0.0);
    float3 plain = lerp(POLAR_PLAIN, float3(0.18, 0.21, 0.30), smoothstep(0.0, 0.25, depth));
    plain *= 0.97 + 0.06 * vnoise2(float2(x * 14.0, uv.y * 60.0));
    float glint = exp(-pow((x - sunPos.x) / (0.006 + depth * 0.5), 2.0)) * exp(-depth * 25.0);
    plain += float3(0.45, 0.62, 0.95) * glint * 0.6;
    col = lerp(col, plain, mPlain);

    // Yakin ucurumlar (yalnizca yanlarda; ortada ova acik): daha koyu ve keskin, dibi ovanin icinde kaybolur
    float nearTop = polarNearTop(x);
    float hasCliff = smoothstep(0.004, 0.009, nearTop - plainTop);
    float mNear = (1.0 - smoothstep(nearTop - pixel, nearTop + pixel, uv.y)) * smoothstep(plainTop - 0.02, plainTop, uv.y) * hasCliff;
    float3 nearFace = polarCliffFace(sp, nearTop, float3(0.30, 0.37, 0.52), float3(0.17, 0.17, 0.23), dSun, onePx);
    col = lerp(col, nearFace, mNear);

    // Saplanan arac + yardim isigi + sinyal halkalari
    float2 rv = float2(POLAR_BEACON.x, _Horizon + POLAR_BEACON.y);
    float roverFar = 1.0 - _PolarRoverHere;
    float roverM = (1.0 - smoothstep(-pixel, pixel, polarRoverShape(sp - rv))) * roverFar;
    col = lerp(col, float3(0.07, 0.08, 0.12), roverM);
    float2 tip = rv + float2(0.0095, 0.0175);
    float phase = fmod(t, 3.0);                                     // uc kisa yanip sonme, sonra bekleme
    float blink = phase < 1.2 ? step(frac(phase / 0.4), 0.5) : 0.0;
    float3 beacon = float3(1.0, 0.30, 0.25) * (glowDot(sp, tip, 1.6 * sc) * (0.2 + 1.2 * blink) + glowDot(sp, tip, 7.0 * sc) * 0.35 * blink);
    float wave = frac(t / 3.0);
    float ringD = length((sp - tip) * float2(1.0, 1.6));
    float ring = exp(-pow((ringD - wave * 0.06) * picturePx / 1.3, 2.0)) * (1.0 - wave) * 0.55;
    beacon += float3(1.0, 0.45, 0.35) * ring;
    beacon *= roverFar;

    // Savrulan kar: ufkun ustunden yavasca gecen ince beyaz pus
    float hb = (uv.y - (_Horizon - 0.025)) / 0.05;
    float2 hp = float2(x * 5.0 - t * 0.06, uv.y * 22.0);
    float warp = fbm(hp * 0.6 + float2(t * 0.02, 5.0));
    float wisps = smoothstep(0.4, 0.85, fbm(hp + float2(warp * 1.6, warp * 0.4)));
    col = lerp(col, float3(0.55, 0.62, 0.76), exp(-hb * hb) * wisps * 0.35);

    col += beacon * 1.3;
    return col;
}

#endif
