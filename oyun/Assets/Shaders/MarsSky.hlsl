// MarsKod ortak gokyuzu/ufuk cizimi. Hem arka plan (Backdrop.shader) hem zemin (Ground.shader) kullanir;
// boylece uzaktaki zemin arka plana dikissiz karisir. Degerler C#'tan genel (global) olarak verilir.
#ifndef MARSKOD_SKY_INCLUDED
#define MARSKOD_SKY_INCLUDED

float _StarsOn;
float _Horizon;
float4 _Focus;
float4 _Sun;

// ---------- gurultu ----------
float hash11(float p) { p = frac(p * 0.1031); p *= p + 33.33; p *= p + p; return frac(p); }
float hash21(float2 p) { float3 p3 = frac(float3(p.xyx) * 0.1031); p3 += dot(p3, p3.yzx + 33.33); return frac((p3.x + p3.y) * p3.z); }
float2 hash22(float2 p) { float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973)); p3 += dot(p3, p3.yzx + 33.33); return frac((p3.xx + p3.yz) * p3.zy); }
float vnoise(float x) { float i = floor(x); float f = frac(x); float u = f * f * (3.0 - 2.0 * f); return lerp(hash11(i), hash11(i + 1.0), u); }
float vnoise2(float2 p)
{
    float2 i = floor(p); float2 f = frac(p); float2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i), b = hash21(i + float2(1, 0)), c = hash21(i + float2(0, 1)), d = hash21(i + float2(1, 1));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}
float fbm(float2 p)
{
    float s = 0.0, a = 0.5;
    [unroll] for (int i = 0; i < 4; i++) { s += vnoise2(p) * a; p = p * 2.03 + 7.1; a *= 0.5; }
    return s / 0.9375;
}
float ridge(float x, float seed)
{
    return vnoise(x + seed) * 0.62 + vnoise(x * 2.3 + seed * 1.7) * 0.28 + vnoise(x * 5.1 + seed * 3.1) * 0.10;
}

// ---------- sekiller (mesafe fonksiyonlari) ----------
float sdBox(float2 p, float2 b) { float2 d = abs(p) - b; return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0); }
float sdCircle(float2 p, float r) { return length(p) - r; }
float sdSeg(float2 p, float2 a, float2 b, float r)
{
    float2 pa = p - a, ba = b - a;
    float h = saturate(dot(pa, ba) / dot(ba, ba));
    return length(pa - ba * h) - r;
}
float glowDot(float2 sp, float2 c, float px) { float d = length(sp - c) * _ScreenParams.y; return exp(-d * d / (px * px)); }

// ---------- gokyuzu ----------
float3 skyColor(float t)
{
    float3 horizon = float3(0.22, 0.135, 0.17);
    float3 mid     = float3(0.085, 0.068, 0.13);
    float3 top     = float3(0.022, 0.026, 0.06);
    float3 c = lerp(horizon, mid, smoothstep(0.0, 0.30, t));
    return lerp(c, top, smoothstep(0.2, 1.0, t));
}

float stars(float2 uv, float density, float cellPx, float seed, float sizeMul)
{
    float scale = _ScreenParams.y / 1170.0;
    float cell = cellPx * scale;
    float2 g = uv * _ScreenParams.xy / cell;
    float2 id0 = floor(g);
    float2 f0 = frac(g);
    float sum = 0.0;
    [unroll] for (int j = -1; j <= 1; j++)
    [unroll] for (int i = -1; i <= 1; i++)
    {
        float2 id = id0 + float2(i, j) + seed;
        if (hash21(id) > density) continue;
        float2 pos = float2(i, j) + 0.15 + 0.7 * hash22(id + 3.7);
        float d = length((f0 - pos) * cell);
        float big = step(0.9, hash21(id + 11.0));
        float size = lerp(0.7, 1.3, hash21(id + 5.0)) * scale * sizeMul * (1.0 + big * 0.8);
        float core = exp(-(d * d) / (size * size));
        float gr = size * 3.0;
        float glow = big * exp(-(d * d) / (gr * gr)) * 0.5;
        float h2 = hash21(id + 17.0);
        float h3 = hash21(id + 29.0);
        float tw = 0.5 + 0.5 * sin(_Time.y * (0.6 + 1.9 * h2) + h3 * 6.2831);
        float flare = pow(saturate(sin(_Time.y * (0.25 + 0.35 * h3) + h2 * 40.0)), 24.0) * step(0.65, h2);
        float bright = lerp(0.45, 1.25, hash21(id + 41.0));
        sum += (core + glow) * bright * (0.4 + 0.6 * tw * tw + flare * 1.4);
    }
    return sum;
}

float4 rock(float2 sp, float2 c, float r, float seed, float pixel)
{
    float2 p = sp - c;
    float ang = atan2(p.y, p.x);
    float rr = r * (1.0 + 0.10 * sin(ang * 3.0 + seed) + 0.06 * sin(ang * 5.0 + seed * 2.3));
    float d = length(p * float2(1.0, 1.2));
    float m = 1.0 - smoothstep(rr - pixel, rr + pixel, d);
    float lit = saturate(0.3 + dot(normalize(p + 1e-6), normalize(float2(-0.5, -0.7))) * 0.9);
    float crater = 1.0 - 0.18 * smoothstep(0.35, 0.0, length(p / r - float2(0.25, -0.15)));
    float3 col = float3(0.66, 0.58, 0.56) * (0.22 + 0.78 * lit) * crater;
    return float4(col, m);
}

// Koloni silueti (mesafe)
float colonyShape(float2 sp, float P)
{
    float y = sp.y;
    float d = 1e5;
    // su isleme tesisi: iki depo + boru (robotun topladigi buz buraya gider)
    d = min(d, sdSeg(sp, float2(-0.197, P + 0.005), float2(-0.197, P + 0.013), 0.0062));
    d = min(d, sdSeg(sp, float2(-0.182, P + 0.005), float2(-0.182, P + 0.010), 0.0052));
    d = min(d, sdBox(sp - float2(-0.172, P + 0.0085), float2(0.008, 0.0011)));
    d = min(d, sdBox(sp - float2(-0.197, P + 0.023), float2(0.0005, 0.004)));
    // kubbeler + baglanti tupleri
    d = min(d, max(sdCircle(sp - float2(-0.125, P), 0.017), P - y));
    d = min(d, max(sdCircle(sp - float2(-0.092, P), 0.0105), P - y));
    d = min(d, max(sdCircle(sp - float2(-0.070, P), 0.0072), P - y));
    d = min(d, sdBox(sp - float2(-0.150, P + 0.0024), float2(0.012, 0.0024)));
    d = min(d, sdBox(sp - float2(-0.106, P + 0.0022), float2(0.008, 0.0022)));
    // kontrol kulesi
    d = min(d, sdBox(sp - float2(-0.055, P + 0.019), float2(0.0022, 0.019)));
    d = min(d, sdBox(sp - float2(-0.055, P + 0.041), float2(0.0070, 0.0042)) - 0.0012);
    d = min(d, sdBox(sp - float2(-0.055, P + 0.053), float2(0.0006, 0.008)));
    // radar canagi
    d = min(d, sdBox(sp - float2(-0.038, P + 0.006), float2(0.0009, 0.006)));
    d = min(d, max(sdCircle(sp - float2(-0.038, P + 0.0135), 0.0058), -dot(sp - float2(-0.038, P + 0.0135), normalize(float2(-0.6, -0.8)))));
    // roket + rampa kulesi
    d = min(d, sdSeg(sp, float2(-0.016, P + 0.006), float2(-0.016, P + 0.036), 0.0031));
    d = min(d, sdBox(sp - float2(-0.016, P + 0.005), float2(0.0056, 0.0028)));
    d = min(d, sdBox(sp - float2(-0.0065, P + 0.020), float2(0.0017, 0.020)));
    d = min(d, sdBox(sp - float2(-0.011, P + 0.030), float2(0.0045, 0.0006)));
    // gunes panelleri
    [unroll] for (int k = 0; k < 4; k++)
    {
        float px = 0.006 + k * 0.0085;
        d = min(d, sdBox(sp - float2(px, P + 0.0045), float2(0.0036, 0.0011)));
        d = min(d, sdBox(sp - float2(px, P + 0.002), float2(0.0005, 0.002)));
    }
    // zemin platformu
    d = min(d, sdBox(sp - float2(-0.085, P + 0.0006), float2(0.13, 0.0012)));
    return d;
}


// Ekran konumuna (uv, alttan 0) gore arka plan rengi (sRGB).
float3 MarsBackdrop(float2 uv)
{
    float aspect = _ScreenParams.x / _ScreenParams.y;
    float x = (uv.x - 0.5) * aspect;       // ekran yuksekligi biriminde
    float pixel = 1.5 / _ScreenParams.y;
    float onePx = 1.0 / _ScreenParams.y;
    float2 sp = float2(x, uv.y);
    float t = _Time.y;

    // Gunes: ufkun altinda, gorunmez; isigi tepelerin ardindan tasar
    float2 sunPos = float2(_Sun.x, _Horizon + _Sun.y);
    float dSun = length((sp - sunPos) * float2(0.8, 1.5));
    float breath = 1.0 + 0.04 * sin(t * 0.5);
    float sunNear = exp(-dSun * 3.5);
    float sunDim = 1.0 - 0.85 * sunNear;

    // Gokyuzu
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = skyColor(skyT);
    float starMask = smoothstep(0.06, 0.40, skyT) * sunDim;

    // Yildizlar
    float3 starCol = lerp(float3(1.0, 0.92, 0.84), float3(0.84, 0.90, 1.0), vnoise2(uv * 40.0));
    // yukari ciktikca siklasir
    float density = 0.16 + 0.32 * smoothstep(0.35, 1.0, skyT);
    col += starCol * stars(uv, density, 14.0, 0.0, 1.0) * starMask * _StarsOn;

    // Yavasca gecen uydu
    float sat = frac(t / 70.0);
    float2 satPos = float2(lerp(-0.30, 0.30, sat), lerp(0.97, 0.88, sat));
    col += float3(0.9, 0.92, 1.0) * glowDot(sp, satPos, 1.3 * _ScreenParams.y / 1170.0) * 0.8 * starMask * _StarsOn;

    // Gunes isiltisi (sicak, genis)
    col += (float3(0.66, 0.31, 0.13) * exp(-dSun * 6.5) * 1.0 + float3(0.32, 0.13, 0.10) * exp(-dSun * 2.4) * 0.65) * breath;

    // Sabit duran kayalar
    float4 rk;
    rk = rock(sp, float2(0.14, 0.935), 0.0105, 1.0, pixel); col = lerp(col, rk.rgb, rk.a);
    rk = rock(sp, float2(-0.16, 0.875), 0.0060, 4.0, pixel); col = lerp(col, rk.rgb, rk.a);
    rk = rock(sp, float2(0.05, 0.978), 0.0042, 7.0, pixel); col = lerp(col, rk.rgb, rk.a);
    rk = rock(sp, float2(-0.08, 0.952), 0.0036, 2.0, pixel); col = lerp(col, rk.rgb * 0.85, rk.a);
    rk = rock(sp, float2(0.19, 0.848), 0.0032, 9.0, pixel); col = lerp(col, rk.rgb * 0.8, rk.a);

    // Tepeler (arkadan aydinlanan siluetler)
    float3 warm = float3(0.95, 0.55, 0.28);
    float mesa = smoothstep(0.42, 0.62, ridge(x * 1.9, 3.0));
    float farH  = _Horizon - 0.018 + mesa * 0.042 + ridge(x * 9.0, 5.0) * 0.006;
    float midH  = _Horizon - 0.034 + ridge(x * 3.2, 11.0) * 0.040;
    float P = _Horizon - 0.047;   // koloni platformu
    float2 cs = sp - float2(0.045, 0.0);   // koloni biraz ortaya
    float pad = smoothstep(-0.25, -0.21, cs.x) * (1.0 - smoothstep(0.045, 0.08, cs.x));
    float nearH = lerp(_Horizon - 0.054 + ridge(x * 4.4, 23.0) * 0.026, P, pad);

    float3 farC  = float3(0.19, 0.13, 0.165) + warm * exp(-dSun * 6.0) * 0.16;
    float3 midC  = float3(0.13, 0.095, 0.125) + warm * exp(-dSun * 7.0) * 0.07;
    float3 nearC = float3(0.105, 0.08, 0.105);

    float mFar = 1.0 - smoothstep(farH - pixel, farH + pixel, uv.y);
    float rimFar = smoothstep(farH - 2.5 * onePx, farH, uv.y) * (0.04 + 0.5 * exp(-dSun * 5.0));
    col = lerp(col, farC + warm * rimFar, mFar);
    col += float3(0.18, 0.10, 0.10) * exp(-abs(uv.y - midH) * 90.0) * 0.35 * mFar;
    float mMid = 1.0 - smoothstep(midH - pixel, midH + pixel, uv.y);
    float rimMid = smoothstep(midH - 2.0 * onePx, midH, uv.y) * (0.03 + 0.35 * exp(-dSun * 6.0));
    col = lerp(col, midC + warm * rimMid, mMid);

    float mNear = 1.0 - smoothstep(nearH - pixel, nearH + pixel, uv.y);
    float rimNear = smoothstep(nearH - 2.0 * onePx, nearH, uv.y) * (0.03 + 0.30 * exp(-dSun * 6.0));

    // Ova: koyu; ufuktan tahtaya dogru yayilan yumusak gunes isigi
    float plainT = saturate((nearH - uv.y) / nearH);
    float3 plain = lerp(float3(0.085, 0.064, 0.082), float3(0.036, 0.028, 0.042), smoothstep(0.0, 1.0, plainT));
    float2 fd = float2((uv.x - _Focus.x) * aspect, (uv.y - _Focus.y) * 0.85);
    plain += float3(0.06, 0.05, 0.065) * exp(-dot(fd, fd) * 6.0);
    float pd = length(float2((x - sunPos.x) * 0.9, (nearH - uv.y) * 2.2));
    float pdSoft = length(float2((x - sunPos.x) * 0.7, (nearH - uv.y) * 1.6));
    float3 sunOnGround = (float3(0.30, 0.14, 0.07) * exp(-pd * 5.0) * 0.6 + float3(0.18, 0.09, 0.055) * exp(-pdSoft * 2.0) * 0.55) * breath;
    // koloninin bize dogru uzanan golgesi (gunes arkadan vurdugu icin)
    float shadow = 0.0;
    if (cs.x > -0.30 && cs.x < 0.12 && uv.y < P && uv.y > P - 0.05)
    {
        float depth = P - uv.y;
        float2 q = float2(cs.x + (cs.x - (sunPos.x - 0.045)) * depth * 6.0, P + depth * 3.5);
        // yumusak, bulanik gölge (yansima gibi keskin olmasin)
        float blur = 0.002 + depth * 0.35;
        float sm = 1.0 - smoothstep(-blur, blur, colonyShape(q, P));
        shadow = sm * exp(-depth * 70.0) * 0.8;
    }
    plain += sunOnGround * (1.0 - 0.85 * shadow);
    plain *= 1.0 - 0.35 * shadow;
    col = lerp(col, lerp(nearC + warm * rimNear, plain, smoothstep(0.0, 0.006, plainT)), mNear);

    // Koloni
    float3 cLights = 0.0;
    float halo = 0.0;
    if (cs.x > -0.26 && cs.x < 0.08 && uv.y > P - 0.01 && uv.y < P + 0.08)
    {
        float cm = 1.0 - smoothstep(-pixel, pixel, colonyShape(cs, P));
        // isik sadece ust kenarlara vurur (arkadan, alcaktan gelen gunes)
        float above = 1.0 - smoothstep(-pixel, pixel, colonyShape(cs + float2(0.0, 2.0 * onePx), P));
        float rim = cm * (1.0 - above) * exp(-dSun * 4.0);
        col = lerp(col, float3(0.06, 0.048, 0.062), cm);
        col += warm * rim * 0.45;


        // isiklar
        float sc = _ScreenParams.y / 1170.0;
        float lights = 0.0;
        [unroll] for (int k = -2; k <= 2; k++) lights += glowDot(cs, float2(-0.125 + k * 0.0055, P + 0.0045), 1.2 * sc) * step(0.2, hash11(k + 3.0));
        lights += glowDot(cs, float2(-0.095, P + 0.004), 1.1 * sc) + glowDot(cs, float2(-0.089, P + 0.004), 1.1 * sc);
        lights += glowDot(cs, float2(-0.070, P + 0.003), 1.0 * sc);
        lights += glowDot(cs, float2(-0.197, P + 0.009), 1.0 * sc) * 0.8 + glowDot(cs, float2(-0.182, P + 0.007), 0.9 * sc) * 0.6;
        lights += glowDot(cs, float2(-0.150, P + 0.0024), 0.9 * sc) + glowDot(cs, float2(-0.144, P + 0.0024), 0.9 * sc);
        float strip = 1.0 - smoothstep(-onePx, onePx, sdBox(cs - float2(-0.055, P + 0.0415), float2(0.0055, 0.0011)));
        lights += strip * 0.9;
        [unroll] for (int m = 0; m < 3; m++) lights += glowDot(cs, float2(-0.0065, P + 0.008 + m * 0.012), 0.9 * sc) * 0.7;
        cLights += float3(1.0, 0.74, 0.45) * saturate(lights);

        // sisin icinde dagilan hale
        halo += glowDot(cs, float2(-0.125, P + 0.005), 9.0 * sc) * 0.9;
        halo += glowDot(cs, float2(-0.092, P + 0.004), 6.0 * sc) * 0.6;
        halo += glowDot(cs, float2(-0.055, P + 0.0415), 8.0 * sc) * 0.8;
        halo += glowDot(cs, float2(-0.190, P + 0.008), 6.0 * sc) * 0.5;
        halo += glowDot(cs, float2(-0.0065, P + 0.02), 6.0 * sc) * 0.4;
        halo += glowDot(cs, float2(-0.005, P + 0.0015), 10.0 * sc) * 0.35;

        // pist isiklari: yavasca sirayla yanar
        float chase = 0.0;
        [unroll] for (int n = 0; n < 8; n++)
        {
            float on = smoothstep(0.85, 1.0, sin(t * 1.2 - n * 0.6) * 0.5 + 0.5);
            chase += glowDot(cs, float2(-0.04 + n * 0.0105, P + 0.0015), 0.9 * sc) * (0.25 + 0.75 * on);
        }
        cLights += float3(1.0, 0.62, 0.35) * chase * 0.8;

        // yanip sonen isaret isiklari
        float blinkR = pow(0.5 + 0.5 * sin(t * 2.2), 6.0);
        float blinkW = pow(0.5 + 0.5 * sin(t * 1.1 + 1.3), 10.0);
        cLights += float3(1.0, 0.32, 0.28) * glowDot(cs, float2(-0.055, P + 0.061), 1.8 * sc) * blinkR;
        cLights += float3(0.9, 0.95, 1.0) * glowDot(cs, float2(-0.016, P + 0.0395), 1.6 * sc) * blinkW;
    }

    // ufuk hizasinda alcak sis (gunese yakin daha sicak)
    col += lerp(float3(0.12, 0.08, 0.09), float3(0.30, 0.15, 0.08), exp(-dSun * 4.0)) * exp(-abs(uv.y - nearH) * 70.0) * 0.28;

    // Hafif kum firtinasi: tepelerin ve koloninin ustunden yavasca suzulen dagnik toz
    float hb = (uv.y - (_Horizon - 0.03)) / 0.06;
    float hazeBand = exp(-hb * hb);
    float2 hp = float2(x * 5.0 - t * 0.035, uv.y * 20.0);
    float warp = fbm(hp * 0.6 + float2(t * 0.015, 3.0));
    float wisps = smoothstep(0.35, 0.80, fbm(hp + float2(warp * 1.6, warp * 0.4)));
    float haze = hazeBand * (0.15 + 0.85 * wisps) * 0.6;
    float3 hazeCol = lerp(float3(0.27, 0.18, 0.18), float3(0.56, 0.31, 0.18), exp(-dSun * 3.0));
    col = lerp(col, hazeCol, haze);
    // koloni isiklari sisin icinden parlar, etraflarinda hafif hale olusur
    col += cLights * 1.3;
    col += float3(1.0, 0.66, 0.38) * halo * (0.10 + 0.35 * haze);
    return col;
}

float MarsVignette(float2 uv)
{
    float aspect = _ScreenParams.x / _ScreenParams.y;
    float2 vd = float2((uv.x - 0.5) * aspect / max(aspect, 0.4), uv.y - 0.5);
    return 1.0 - 0.35 * smoothstep(0.3, 0.85, length(vd * float2(1.4, 1.0)));
}

#endif
