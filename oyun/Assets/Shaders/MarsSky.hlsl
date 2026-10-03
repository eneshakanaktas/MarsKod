// MarsKod ortak gokyuzu/ufuk cizimi. Arka plan (Backdrop.shader), zemin (Ground.shader) ve uzak kayalar (FarRock.shader) kullanir;
// boylece uzaktaki zemin arka plana dikissiz karisir. Degerler C#'tan genel (global) olarak verilir.
#ifndef MARSKOD_SKY_INCLUDED
#define MARSKOD_SKY_INCLUDED

float _StarsOn;
float _Region;    // bolge (RegionLook.cs): 0 inis ovasi, 1 kutup buzulu
bool MarsPolar() { return _Region > 0.5; }
// Cok hafif genel aydinlatma: arka plan ve zeminin karistigi ova rengi ayni oranda (yoksa yeni renk siniri olusur)
static const float MARS_EXPOSURE = 1.06;
float _Horizon;   // ufuk: fotograf koordinatinda (asagida)
float4 _Picture;  // x: fotografin olcegi (1 = tam boy), y: dikey kayma (ekranin -1..1 biriminde)
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

// Ana koloninin buyuk kubbesi (koloniye gore; y platforma eklenir). Hikaye izleri burada (colonyTraces).
static const float2 BIG_DOME = float2(-0.128, 0.0);
static const float BIG_DOME_R = 0.023;
static const float TRACE_SCALE = BIG_DOME_R / 0.017;   // izler 0,017'lik kubbeye gore cizildi, kubbeyle birlikte buyur

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
    d = min(d, max(sdCircle(sp - float2(BIG_DOME.x, P), BIG_DOME_R), P - y));
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

// ---- Cevredeki koloniler ve ayrik yapilar ----
// Ortadaki koloniden farkli gorunurler; fotograf kuculunce (klavye acik) kenarlarda acıga cikarlar.
// Yakindakiler ana koloniyle ayni duzlemde (P) durur; uzaktakiler daha kucuk (FAR_SCALE), orta tepelerin onunde.
static const float COLONY_X  = 0.045;   // ana koloni
static const float MINING_X  = 0.205;   // maden ocagi (yakin, sag)
static const float HABITAT_X = -0.262;  // yasam kuleleri (yakin, sol)
static const float GREEN_X   = 0.335;   // sera ciftligi (uzak, sag)
static const float MAST_X    = 0.415;   // haberlesme diregi (uzak, sag)
static const float LANDER_X  = -0.400;  // inis pisti (uzak, sol)
static const float SCOPES_X  = -0.330;  // radyo teleskoplari (uzak, sol)
static const float FUEL_X    = -0.345;  // yakit tesisi (orta sira, sol)
static const float SOLAR_X   = 0.290;   // gunes tarlasi (orta sira, sag)
static const float CARGO_X   = -0.235;  // yuk deposu + drone pedi (on sira, sol)
static const float GARAGE_X  = 0.370;   // arac garaji (on sira, sag)
static const float DISH_X    = 0.140;   // buyuk canak anten (on sira, sag)
static const float FAR_BASE  = -0.036;  // uzak yapilarin zemini (_Horizon'a gore)
static const float FAR_SCALE = 0.6;
// Ovadaki yapi siralari: 0 = ufuk (ana koloniyle ayni), 1 = orta, 2 = on (alana biraz yakin; daha asagida, daha buyuk)
#define SITE_ROWS 3
static const float kRowBase[SITE_ROWS]  = { -0.047, -0.056, -0.066 };
static const float kRowScale[SITE_ROWS] = { 1.0, 1.2, 1.4 };
static const float3 kRowColor[SITE_ROWS] = { float3(0.060, 0.048, 0.062), float3(0.046, 0.036, 0.048), float3(0.038, 0.030, 0.040) };

float bandMask(float x, float a, float b, float soft) { return smoothstep(a - soft, a, x) * (1.0 - smoothstep(b, b + soft, x)); }
// tepesi (0, h) tabani 2w olan ucgen
float sdCone(float2 p, float w, float h) { p.x = abs(p.x); float l = length(float2(h, w)); return max(-p.y, dot(p, float2(h, w) / l) - w * h / l); }
float sdHalfEllipse(float2 p, float2 r) { return max((length(p / r) - 1.0) * min(r.x, r.y), -p.y); }
// yerel koordinattaki isik (q, c), fotografta s kadar kucultulmus yapi icin
float siteGlow(float2 q, float2 c, float px, float s) { return glowDot(q * s, c * s, px); }

// Maden ocagi: sondaj kulesi, cevher silosu, tasima bandi, isci barinagi (q: tabanin ortasina gore)
float miningShape(float2 q)
{
    float d = 1e5;
    float2 qa = float2(abs(q.x), q.y);
    d = min(d, sdSeg(qa, float2(0.009, 0.0), float2(0.0025, 0.050), 0.0011));
    [unroll] for (int k = 0; k < 4; k++)
    {
        float y0 = 0.006 + k * 0.011, y1 = y0 + 0.011;
        d = min(d, sdSeg(q, float2(-(0.009 - 0.13 * y0), y0), float2(0.009 - 0.13 * y1, y1), 0.0005));
    }
    d = min(d, sdBox(q - float2(0.0, 0.0515), float2(0.0042, 0.0018)));
    d = min(d, sdBox(q - float2(0.0, 0.058), float2(0.0005, 0.005)));
    d = min(d, sdBox(q - float2(0.0, 0.0035), float2(0.012, 0.0035)) - 0.0008);
    d = min(d, sdBox(q - float2(0.031, 0.012), float2(0.0085, 0.012)));
    d = min(d, sdCone(q - float2(0.031, 0.024), 0.0095, 0.007));
    d = min(d, sdSeg(q, float2(0.010, 0.006), float2(0.024, 0.027), 0.0009));
    d = min(d, sdBox(q - float2(0.017, 0.0085), float2(0.0005, 0.0085)));
    d = min(d, sdBox(q - float2(-0.026, 0.0045), float2(0.011, 0.0045)) - 0.0010);
    d = min(d, sdBox(q - float2(0.002, 0.0006), float2(0.046, 0.0012)));
    return d;
}

// Yasam kuleleri: iki silindir kule, aralarinda kopru, anten diregi
float habitatShape(float2 q)
{
    float d = sdSeg(q, float2(0.0, 0.0075), float2(0.0, 0.031), 0.0078);
    d = min(d, sdSeg(q, float2(0.019, 0.006), float2(0.019, 0.019), 0.0062));
    d = min(d, sdBox(q - float2(0.0095, 0.0135), float2(0.006, 0.0017)));
    d = min(d, sdBox(q - float2(-0.017, 0.022), float2(0.0006, 0.022)));
    d = min(d, sdBox(q - float2(-0.017, 0.036), float2(0.0035, 0.0005)));
    d = min(d, sdBox(q - float2(-0.017, 0.030), float2(0.0025, 0.0005)));
    d = min(d, sdBox(q - float2(0.002, 0.0006), float2(0.030, 0.0012)));
    return d;
}

float greenhouseVaults(float2 q)
{
    float d = 1e5;
    [unroll] for (int k = -1; k <= 1; k++) d = min(d, sdHalfEllipse(q - float2(k * 0.022, 0.0), float2(0.0098, 0.0075)));
    return d;
}

// Sera ciftligi: uc uzun tonoz sera + koridor
float greenhouseShape(float2 q)
{
    float d = greenhouseVaults(q);
    d = min(d, sdBox(q - float2(0.0, 0.0018), float2(0.030, 0.0018)));
    d = min(d, sdBox(q - float2(0.0, 0.0006), float2(0.036, 0.0012)));
    return d;
}

float mastShape(float2 q)
{
    float2 qa = float2(abs(q.x), q.y);
    float d = sdSeg(q, float2(0.0, 0.0), float2(0.0, 0.050), 0.0009);
    d = min(d, sdSeg(qa, float2(0.0045, 0.0), float2(0.0, 0.016), 0.0006));
    d = min(d, max(sdCircle(q - float2(0.0035, 0.032), 0.0045), q.x - 0.0035));
    return d;
}

// Inis pisti ve uzerinde duran inis araci
float landerShape(float2 q)
{
    float2 qa = float2(abs(q.x), q.y);
    float d = sdBox(q - float2(0.0, 0.0008), float2(0.028, 0.0016));
    d = min(d, sdSeg(q, float2(0.0, 0.011), float2(0.0, 0.019), 0.0055));
    d = min(d, sdCone(q - float2(0.0, 0.020), 0.0055, 0.009));
    d = min(d, sdSeg(qa, float2(0.004, 0.010), float2(0.011, 0.0016), 0.0007));
    d = min(d, sdBox(q - float2(0.0, 0.0045), float2(0.0028, 0.0022)));
    return d;
}

// Gokyuzune bakan canak, yandan: sig bir kase (m: agzinin ortasi, w: yari genisligi; agzi sol-yukari bakar)
static const float2 DISH_UP = float2(-0.45, 0.893);
float sdDish(float2 q, float2 m, float w)
{
    float R = w / 0.66;
    float2 c = m + DISH_UP * R * 0.75;
    return max(sdCircle(q - c, R), dot(q - c, DISH_UP) + R * 0.75);
}

float scopesShape(float2 q)
{
    float d = 1e5;
    [unroll] for (int k = -1; k <= 1; k++)
    {
        float2 o = float2(k * 0.016, 0.0);
        d = min(d, sdBox(q - o - float2(0.0, 0.004), float2(0.0007, 0.004)));
        d = min(d, sdDish(q - o, float2(0.0, 0.0095), 0.0055));
    }
    d = min(d, sdBox(q - float2(0.0, 0.0006), float2(0.024, 0.0012)));
    return d;
}

// Yakit tesisi: ayakli tank, alevli baca, kontrol binasi, borular
float fuelPlantShape(float2 q)
{
    float d = sdCircle(q - float2(0.0, 0.014), 0.008);
    d = min(d, sdSeg(float2(abs(q.x), q.y), float2(0.006, 0.0), float2(0.004, 0.009), 0.0007));
    d = min(d, sdBox(q - float2(0.016, 0.014), float2(0.0012, 0.014)));
    d = min(d, sdBox(q - float2(-0.019, 0.004), float2(0.008, 0.004)) - 0.0008);
    d = min(d, sdSeg(q, float2(-0.011, 0.006), float2(-0.006, 0.011), 0.0006));
    d = min(d, sdSeg(q, float2(0.007, 0.012), float2(0.015, 0.008), 0.0006));
    return d;
}

// Gunes tarlasi: egik panel sirasi + cevirici kulubesi
float solarFarmShape(float2 q)
{
    float d = 1e5;
    [unroll] for (int k = -2; k <= 2; k++)
    {
        float x = k * 0.012;
        d = min(d, sdSeg(q, float2(x - 0.005, 0.003), float2(x + 0.004, 0.0065), 0.0008));
        d = min(d, sdBox(q - float2(x, 0.0018), float2(0.0004, 0.0018)));
    }
    d = min(d, sdBox(q - float2(0.034, 0.003), float2(0.004, 0.003)));
    return d;
}

// Yuk deposu: ust uste konteynerler, vinc, drone pedi
float cargoDepotShape(float2 q)
{
    float d = sdBox(q - float2(-0.012, 0.0035), float2(0.0085, 0.0035));
    d = min(d, sdBox(q - float2(0.006, 0.0035), float2(0.0085, 0.0035)));
    d = min(d, sdBox(q - float2(-0.004, 0.0105), float2(0.0085, 0.0035)));
    d = min(d, sdBox(q - float2(0.024, 0.016), float2(0.0008, 0.016)));
    d = min(d, sdSeg(q, float2(0.024, 0.031), float2(-0.004, 0.031), 0.0007));
    d = min(d, sdBox(q - float2(0.002, 0.026), float2(0.0002, 0.005)));
    d = min(d, sdBox(q - float2(-0.030, 0.0008), float2(0.006, 0.0008)));
    return d;
}

// Arac garaji (tonoz) ve onunde park etmis arac
float garageShape(float2 q)
{
    float d = sdHalfEllipse(q, float2(0.020, 0.010));
    d = min(d, sdBox(q - float2(0.029, 0.0028), float2(0.0055, 0.0016)) - 0.0006);
    d = min(d, sdCircle(q - float2(0.025, 0.0012), 0.0013));
    d = min(d, sdCircle(q - float2(0.033, 0.0012), 0.0013));
    return d;
}

float bigDishShape(float2 q)
{
    float d = sdSeg(float2(abs(q.x), q.y), float2(0.006, 0.0), float2(0.0, 0.012), 0.0008);
    d = min(d, sdDish(q, float2(0.0, 0.017), 0.011));
    d = min(d, sdSeg(q, float2(0.0, 0.017), float2(0.0, 0.017) + DISH_UP * 0.010, 0.0005));
    return d;
}

// Ana kolonininkiyle ayni duzlemdeki yapilar (siluet + golge bunu kullanir)
float nearSitesShape(float2 sp, float P)
{
    float d = 1e5;
    if (sp.x > -0.205 && sp.x < 0.14)       d = colonyShape(sp - float2(COLONY_X, 0.0), P);
    else if (sp.x > 0.15 && sp.x < 0.265)   d = miningShape(sp - float2(MINING_X, P));
    else if (sp.x > -0.30 && sp.x < -0.205) d = habitatShape(sp - float2(HABITAT_X, P));
    return d;
}

float farSitesShape(float2 sp, float H)
{
    float y = H + FAR_BASE;
    float d = 1e5;
    if (sp.x > 0.30 && sp.x < 0.37)           d = greenhouseShape((sp - float2(GREEN_X, y)) / FAR_SCALE);
    else if (sp.x > 0.40 && sp.x < 0.43)      d = mastShape((sp - float2(MAST_X, y)) / FAR_SCALE);
    else if (sp.x > -0.425 && sp.x < -0.375)  d = landerShape((sp - float2(LANDER_X, y)) / FAR_SCALE);
    else if (sp.x > -0.355 && sp.x < -0.305)  d = scopesShape((sp - float2(SCOPES_X, y)) / FAR_SCALE);
    return d * FAR_SCALE;
}

// Ovadaki bir sira (row) yapilarin mesafesi; tabani _Horizon + kRowBase, boyu kRowScale
float rowSitesShape(int row, float2 sp, float H)
{
    float base = H + kRowBase[row];
    float s = kRowScale[row];
    float d = 1e5;
    if (row == 0) d = nearSitesShape(sp, base);
    else if (row == 1)
    {
        if (sp.x > -0.385 && sp.x < -0.305)     d = fuelPlantShape((sp - float2(FUEL_X, base)) / s) * s;
        else if (sp.x > 0.24 && sp.x < 0.345)   d = solarFarmShape((sp - float2(SOLAR_X, base)) / s) * s;
    }
    else
    {
        if (sp.x > -0.285 && sp.x < -0.19)      d = cargoDepotShape((sp - float2(CARGO_X, base)) / s) * s;
        else if (sp.x > 0.335 && sp.x < 0.43)   d = garageShape((sp - float2(GARAGE_X, base)) / s) * s;
        else if (sp.x > 0.115 && sp.x < 0.165)  d = bigDishShape((sp - float2(DISH_X, base)) / s) * s;
    }
    return d;
}

// Yapilarin isiklari: her koloninin kendi rengi var (ana koloni sicak turuncu). halo: sisin icinde dagilan parlaklik.
float3 miningLights(float2 q, float t, float sc, inout float3 halo)
{
    float3 cool = float3(0.85, 0.92, 1.0);
    float blink = pow(0.5 + 0.5 * sin(t * 1.7 + 0.8), 8.0);
    float3 L = float3(1.0, 0.30, 0.25) * glowDot(q, float2(0.0, 0.063), 1.6 * sc) * blink;
    L += cool * glowDot(q, float2(-0.004, 0.010), 1.3 * sc);
    [unroll] for (int k = 0; k < 4; k++)
    {
        float f = frac(t * 0.25 + k * 0.25);
        L += float3(1.0, 0.70, 0.40) * glowDot(q, lerp(float2(0.010, 0.0072), float2(0.024, 0.0282), f), 0.8 * sc) * 0.7 * sin(3.14159 * f);
    }
    L += float3(1.0, 0.82, 0.60) * (glowDot(q, float2(-0.031, 0.005), 1.0 * sc) + glowDot(q, float2(-0.021, 0.005), 1.0 * sc));
    L += float3(1.0, 0.65, 0.30) * glowDot(q, float2(0.031, 0.008), 0.9 * sc) * 0.6;
    halo += cool * glowDot(q, float2(-0.004, 0.010), 8.0 * sc) * 0.6;
    halo += float3(1.0, 0.3, 0.25) * glowDot(q, float2(0.0, 0.063), 6.0 * sc) * blink * 0.3;
    return L;
}

float3 habitatLights(float2 q, float t, float sc, float onePx, inout float3 halo)
{
    float3 cool = float3(0.72, 0.86, 1.0);
    float3 L = 0.0;
    [unroll] for (int r = 0; r < 3; r++)
    {
        float y = 0.013 + r * 0.007;
        L += cool * glowDot(q, float2(-0.0035, y), 0.9 * sc) * step(0.3, hash11(r * 2.0 + 5.0));
        L += cool * glowDot(q, float2(0.0035, y), 0.9 * sc) * step(0.3, hash11(r * 2.0 + 6.0));
    }
    L += cool * (glowDot(q, float2(0.019, 0.011), 0.9 * sc) + glowDot(q, float2(0.019, 0.017), 0.9 * sc));
    L += cool * (1.0 - smoothstep(-onePx, onePx, sdBox(q - float2(0.0095, 0.0135), float2(0.005, 0.0005)))) * 0.8;
    L += float3(0.9, 0.95, 1.0) * glowDot(q, float2(-0.017, 0.0445), 1.5 * sc) * pow(0.5 + 0.5 * sin(t * 1.4 + 2.0), 10.0);
    halo += cool * glowDot(q, float2(0.004, 0.020), 9.0 * sc) * 0.6;
    return L;
}

float3 farSitesLights(float2 sp, float H, float t, float sc, inout float3 halo)
{
    float s = FAR_SCALE;
    float y = H + FAR_BASE;
    float3 L = 0.0;
    if (sp.x > 0.28 && sp.x < 0.39)
    {
        // sera: bitki buyutme isiklari (pembe-mor)
        float2 q = (sp - float2(GREEN_X, y)) / s;
        float3 pink = float3(1.0, 0.45, 0.80);
        [unroll] for (int k = -1; k <= 1; k++)
        {
            L += pink * siteGlow(q, float2(k * 0.022, 0.003), 1.1 * sc, s) * 0.8;
            halo += pink * siteGlow(q, float2(k * 0.022, 0.003), 7.0 * sc, s) * 0.45;
        }
    }
    else if (sp.x > 0.39 && sp.x < 0.44)
    {
        float2 q = (sp - float2(MAST_X, y)) / s;
        L += float3(1.0, 0.30, 0.25) * siteGlow(q, float2(0.0, 0.051), 1.4 * sc, s) * pow(0.5 + 0.5 * sin(t * 2.6 + 4.0), 8.0);
    }
    else if (sp.x > -0.44 && sp.x < -0.36)
    {
        // pist kenari isiklari sirayla yanar (yesil)
        float2 q = (sp - float2(LANDER_X, y)) / s;
        float3 green = float3(0.45, 1.0, 0.60);
        [unroll] for (int k = 0; k < 5; k++)
        {
            float on = smoothstep(0.8, 1.0, 0.5 + 0.5 * sin(t * 2.0 - k * 0.9));
            L += green * siteGlow(q, float2(-0.026 + k * 0.013, 0.0018), 1.0 * sc, s) * (0.3 + 0.7 * on);
        }
        L += float3(1.0, 0.9, 0.75) * siteGlow(q, float2(0.0, 0.016), 0.9 * sc, s);
        halo += green * siteGlow(q, float2(0.0, 0.002), 7.0 * sc, s) * 0.3;
    }
    if (sp.x > -0.36 && sp.x < -0.30)
    {
        float2 q = (sp - float2(SCOPES_X, y)) / s;
        L += float3(1.0, 0.30, 0.25) * siteGlow(q, float2(0.0, 0.009), 1.0 * sc, s) * pow(0.5 + 0.5 * sin(t * 1.6 + 1.0), 8.0);
    }
    return L * 0.8;
}

// Orta ve on siradaki yapilarin isiklari
float3 rowSitesLights(float2 sp, float H, float t, float sc, inout float3 halo)
{
    float3 L = 0.0;
    float3 warm = float3(1.0, 0.78, 0.52);
    float3 red = float3(1.0, 0.30, 0.25);
    if (sp.x > -0.39 && sp.x < -0.30)
    {
        // yakit tesisi: bacadaki alev titrer
        float s = kRowScale[1];
        float2 q = (sp - float2(FUEL_X, H + kRowBase[1])) / s;
        float flick = 0.75 + 0.25 * sin(t * 13.0) * sin(t * 7.3 + 1.0);
        float3 flame = float3(1.0, 0.55, 0.20);
        L += flame * siteGlow(q, float2(0.016, 0.0295), 1.5 * sc, s) * flick;
        halo += flame * siteGlow(q, float2(0.016, 0.030), 7.0 * sc, s) * 0.5 * flick;
        L += warm * (siteGlow(q, float2(-0.022, 0.005), 0.9 * sc, s) + siteGlow(q, float2(-0.016, 0.005), 0.9 * sc, s));
        L += warm * siteGlow(q, float2(0.0, 0.010), 0.8 * sc, s) * 0.5;
    }
    else if (sp.x > 0.24 && sp.x < 0.35)
    {
        float s = kRowScale[1];
        float2 q = (sp - float2(SOLAR_X, H + kRowBase[1])) / s;
        L += float3(0.6, 1.0, 0.7) * siteGlow(q, float2(0.034, 0.004), 0.8 * sc, s) * (0.6 + 0.4 * step(0.5, frac(t * 0.7)));
    }
    if (sp.x > -0.29 && sp.x < -0.18)
    {
        // yuk deposu: pedin kenar isiklari (amber), vinc tepesi, konteyner lambalari
        float s = kRowScale[2];
        float2 q = (sp - float2(CARGO_X, H + kRowBase[2])) / s;
        float3 amber = float3(1.0, 0.65, 0.25);
        float pulse = 0.4 + 0.6 * pow(0.5 + 0.5 * sin(t * 2.4), 4.0);
        L += amber * (siteGlow(q, float2(-0.0355, 0.0018), 0.9 * sc, s) + siteGlow(q, float2(-0.0245, 0.0018), 0.9 * sc, s)) * pulse;
        L += red * siteGlow(q, float2(0.024, 0.0325), 1.3 * sc, s) * pow(0.5 + 0.5 * sin(t * 1.9 + 0.5), 8.0);
        L += warm * (siteGlow(q, float2(-0.012, 0.0055), 0.8 * sc, s) + siteGlow(q, float2(0.006, 0.0055), 0.8 * sc, s)) * 0.7;
        halo += amber * siteGlow(q, float2(-0.030, 0.002), 7.0 * sc, s) * 0.35 * pulse;
    }
    else if (sp.x > 0.33 && sp.x < 0.44)
    {
        // garaj: acik kapidan isik, aracin farlari
        float s = kRowScale[2];
        float2 q = (sp - float2(GARAGE_X, H + kRowBase[2])) / s;
        float door = 1.0 - smoothstep(-0.0006, 0.0006, sdBox(q - float2(0.006, 0.0032), float2(0.0035, 0.0032)));
        L += warm * door * 0.22;
        L += float3(0.9, 0.95, 1.0) * siteGlow(q, float2(0.0348, 0.0030), 1.1 * sc, s);
        halo += warm * siteGlow(q, float2(0.006, 0.003), 8.0 * sc, s) * 0.35;
        halo += float3(0.9, 0.95, 1.0) * siteGlow(q, float2(0.038, 0.0028), 6.0 * sc, s) * 0.3;
    }
    else if (sp.x > 0.11 && sp.x < 0.17)
    {
        float s = kRowScale[2];
        float2 q = (sp - float2(DISH_X, H + kRowBase[2])) / s;
        L += red * siteGlow(q, float2(0.0, 0.017) + DISH_UP * 0.0105, 1.2 * sc, s) * pow(0.5 + 0.5 * sin(t * 1.2 + 3.0), 10.0);
    }
    return L;
}

// ---- Yuk tasiyan dronelar ----
// Duraklar arasinda rastgele sirayla gider: kalkar, yay cizerek ucar, iner. x, y (_Horizon'a gore), boy.
#define DRONE_STOPS 12
#define DRONE_COUNT 5
static const float3 kDroneStops[DRONE_STOPS] =
{
    float3(-0.152, -0.026, 1.0),   // ana koloni: su tesisi
    float3(-0.080, -0.028, 1.0),   // ana koloni: kubbeler
    float3( 0.050, -0.040, 1.0),   // ana koloni: rampa
    float3( 0.236, -0.014, 1.0),   // maden silosu
    float3(-0.262, -0.006, 1.0),   // yasam kulesi
    float3( 0.335, -0.0295, 0.6),  // sera
    float3(-0.389, -0.0335, 0.6),  // inis pisti
    float3(-0.345, -0.0276, 1.2),  // yakit tanki
    float3( 0.331, -0.0468, 1.2),  // gunes tarlasi kulubesi
    float3(-0.271, -0.0618, 1.4),  // yuk deposu pedi
    float3(-0.241, -0.0444, 1.4),  // konteyner ustu
    float3( 0.370, -0.0500, 1.4)   // garaj catisi
};

float droneRawStop(float n, float seed) { return floor(hash11(n * 3.17 + seed * 41.3) * DRONE_STOPS); }
float droneStop(float n, float seed)
{
    float a = droneRawStop(n, seed);
    if (a == droneRawStop(n - 1.0, seed)) a = fmod(a + 1.0, DRONE_STOPS);
    return a;
}

// Dronun konumu (xy) ve boyu (z)
float3 dronePose(float t, float seed, float legTime, float H)
{
    float tt = t / legTime + seed * 0.37;
    float n = floor(tt), u = frac(tt);
    float3 a = kDroneStops[(int)droneStop(n, seed)];
    float3 b = kDroneStops[(int)droneStop(n + 1.0, seed)];
    float e = smoothstep(0.12, 0.88, u);
    float lift = 0.008 * smoothstep(0.0, 0.12, u) * (1.0 - smoothstep(0.88, 1.0, u));
    float arc = 0.035 * abs(b.x - a.x) * sin(3.14159 * e);
    float bob = 0.0005 * sin(t * 2.3 + seed * 5.0) * smoothstep(0.0, 0.05, lift);
    float2 pos = lerp(a.xy, b.xy, e) + float2(0.0, H + lift + arc + bob);
    return float3(pos, lerp(a.z, b.z, e));
}

// Siluet col'a cizilir, isiklar lights'a eklenir (sisin icinden parlasin diye en son)
void drawDrone(float2 sp, float t, float seed, float legTime, float H, float sc, float pixel, inout float3 col, inout float3 lights)
{
    float3 pose = dronePose(t, seed, legTime, H);
    float2 p = sp - pose.xy;
    if (dot(p, p) > 0.0004) return;
    float k = pose.z;
    float d = sdBox(p, float2(0.0030, 0.0007) * k);
    d = min(d, sdBox(p - float2(0.0, -0.0026) * k, float2(0.0012, 0.0011) * k));
    d = min(d, sdBox(p - float2(0.0, -0.0012) * k, float2(0.0002, 0.0008) * k));
    col = lerp(col, float3(0.05, 0.04, 0.05), (1.0 - smoothstep(-pixel, pixel, d)) * 0.9);

    // isiklar nokta gibidir: fotograf kuculunce silueti kadar kuculmez, uzakta da fark edilsin
    float g = k * sc / sqrt(max(_Picture.x, 0.2));
    float ph = frac(t * 0.8 + seed * 0.61);
    float strobe = exp(-pow((ph - 0.05) / 0.02, 2.0)) + exp(-pow((ph - 0.17) / 0.02, 2.0));
    lights += float3(1.0, 0.25, 0.20) * glowDot(sp, pose.xy + float2(-0.0034, 0.0002) * k, 1.4 * g);
    lights += float3(0.30, 1.0, 0.45) * glowDot(sp, pose.xy + float2(0.0034, 0.0002) * k, 1.4 * g);
    lights += float3(1.0, 1.0, 1.0) * glowDot(sp, pose.xy + float2(0.0, 0.0009) * k, 1.8 * g) * strobe * 1.5;
    lights += float3(1.0, 0.95, 0.9) * glowDot(sp, pose.xy + float2(0.0, 0.0009) * k, 7.0 * g) * strobe * 0.25;
    lights += float3(1.0, 0.75, 0.45) * glowDot(sp, pose.xy - float2(0.0, 0.0045) * k, 5.0 * g) * 0.12;
}


// ---- Enerji hucresi lambalari (ColonyPower.cs) ----
// Ana koloninin onundeki ovada bir sira lamba: bolumdeki her hucre icin bir tane. Sonukken soluk bir yuva,
// hucre koloniye ulasinca yesil-sari yanar; yandigi an buyuk bir parlama yayilip soner.
#define MAX_POWER_LAMPS 12
float4 _PowerLamps[MAX_POWER_LAMPS];   // xy: fotograftaki konum (y ufka gore), z: yandigi an (_Time.y; < 0 sonuk)
float _PowerLampCount;

float3 powerLamps(float2 sp, float t, float sc, float onePx, inout float3 halo)
{
    float3 c = 0.0;
    int n = (int)_PowerLampCount;
    float litEnd = -1e5;   // son yanan lambanin x'i (lambalar soldan saga sirayla yanar)
    for (int k = 0; k < MAX_POWER_LAMPS; k++)
    {
        if (k >= n) break;
        float4 lamp = _PowerLamps[k];
        float2 p = float2(lamp.x, _Horizon + lamp.y);
        if (lamp.z < 0.0)
        {
            c += float3(0.30, 0.34, 0.20) * glowDot(sp, p, 1.3 * sc) * 0.8;
            continue;
        }
        litEnd = p.x;
        float flash = exp(-max(t - lamp.z, 0.0) * 2.2);
        float pulse = 0.85 + 0.15 * sin(t * 2.0 + k * 0.7);
        c += float3(0.82, 1.0, 0.42) * glowDot(sp, p, (1.6 + 1.2 * flash) * sc) * pulse;
        halo += float3(0.55, 0.85, 0.25) * (glowDot(sp, p, 5.0 * sc) * 0.30 + glowDot(sp, p, 18.0 * sc) * flash);
    }
    // lambalari baglayan ince kablo: yanan kisim yesil, kalani soluk
    float2 first = float2(_PowerLamps[0].x, _Horizon + _PowerLamps[0].y);
    float last = _PowerLamps[max(n - 1, 0)].x;
    float wire = (1.0 - smoothstep(0.4, 1.0, abs(sp.y - first.y) / onePx)) * bandMask(sp.x, first.x, last, onePx);
    c += lerp(float3(0.10, 0.11, 0.08), float3(0.40, 0.55, 0.18), step(sp.x, litEnd)) * wire;
    return c;
}

// ---- Koloni izleri (Traces.cs; docs/tasarim/senaryo-bolge-01.md) ----
// Ikisi de ana koloninin buyuk kubbesinde: Bolum 6'da ardina kadar acik bir kapi (ici karanlik, kimse yok),
// Bolum 7'de bolum bitince kubbe sera gibi icten yanar, camin ardinda tek bir kuru saksi bitkisi gorunur.
float _TraceDoor;         // 1: kapi acik
float _TraceGreenhouse;   // seranin isiginin yandigi an (_Time.y); 0 sonuk
float _TraceLeaf;         // 1: Bolge 2 sonunda acilan yeni yaprak (kalici)

float greenhouseLight(float t)
{
    if (_TraceGreenhouse <= 0.0) return 0.0;
    float a = t - _TraceGreenhouse;
    float flicker = a < 0.6 ? step(0.5, frac(a * 7.0)) : 1.0;   // floresan gibi iki uc kez titreyip yanar
    return saturate(a / 0.6) * flicker;
}

// Kuru saksi bitkisi: saksi + sarkik dallar (kubbe merkezine gore)
float driedPlantShape(float2 q, float onePx)
{
    float w = max(0.0007, 1.0 * onePx);
    float d = sdBox(q - float2(0.0, 0.0030), float2(0.0032 - q.y * 0.18, 0.0030));   // asagi dogru daralan saksi
    d = min(d, sdBox(q - float2(0.0, 0.0062), float2(0.0038, 0.0006)));
    d = min(d, sdSeg(q, float2(0.0, 0.0064), float2(0.0008, 0.0128), w));
    d = min(d, sdSeg(q, float2(0.0004, 0.0100), float2(-0.0055, 0.0124), w));
    d = min(d, sdSeg(q, float2(-0.0055, 0.0124), float2(-0.0085, 0.0082), w));
    d = min(d, sdSeg(q, float2(0.0006, 0.0114), float2(0.0060, 0.0132), w));
    d = min(d, sdSeg(q, float2(0.0060, 0.0132), float2(0.0090, 0.0092), w));
    d = min(d, sdSeg(q, float2(0.0008, 0.0128), float2(0.0024, 0.0108), w));   // ucta tek bir boynu bukuk yaprak
    return d;
}

void colonyTraces(float2 cs, float P, float t, float sc, float pixel, float onePx, inout float3 col)
{
    // q: 0,017'lik kubbeye gore koordinat; kenar yumusatma da ayni olcege cevrilir
    float2 q = (cs - float2(BIG_DOME.x, P)) / TRACE_SCALE;
    pixel /= TRACE_SCALE;
    onePx /= TRACE_SCALE;
    float R = 0.017;
    float3 lampCol = float3(1.0, 0.80, 0.55);

    float g = greenhouseLight(t);
    if (g > 0.0)
    {
        float inside = 1.0 - smoothstep(-pixel, pixel, length(q) - (R - 1.2 * onePx));
        inside *= step(0.0, q.y);
        float3 glass = lerp(float3(0.62, 0.86, 0.55), float3(0.30, 0.48, 0.34), saturate(q.y / R));
        // cam kubbenin iskeleti: iki meridyen + bir yatay halka
        float ribs = abs(length(q / float2(0.0085, R)) - 1.0) * 0.0085;
        ribs = min(ribs, abs(q.y - 0.0095));
        glass *= 1.0 - 0.22 * (1.0 - smoothstep(0.4 * onePx, 1.0 * onePx, ribs));   // soluk: bitkiyle karismasin
        col = lerp(col, glass, inside * g);
        float plant = 1.0 - smoothstep(-pixel, pixel, driedPlantShape(q, onePx));
        col = lerp(col, float3(0.055, 0.035, 0.025), plant * inside * g);
        // yeni yaprak: bitkinin tepesinde, kuru dallarin arasinda yesil bir tomurcuk
        float leaf = 1.0 - smoothstep(-pixel, pixel, length((q - float2(0.0030, 0.0146)) * float2(1.0, 1.7)) - 0.0030);
        col = lerp(col, float3(0.08, 0.42, 0.10), leaf * _TraceLeaf * inside * g);   // koyu yesil: camin acik yesiliyle karismasin
        col += float3(0.45, 0.80, 0.40) * glowDot(cs, float2(BIG_DOME.x, P + 0.008 * TRACE_SCALE), 18.0 * sc) * 0.35 * g;
    }

    if (_TraceDoor > 0.5)
    {
        float2 lampPos = float2(BIG_DOME.x, P + 0.0122 * TRACE_SCALE);
        float opening = sdBox(q - float2(0.0, 0.0050), float2(0.0030, 0.0050));
        // kapi kanadi bize dogru acilmis: yandan ince gorunur, lambanin isigini alir
        float panel = sdBox(q - float2(0.0047, 0.0049), float2(0.0015, 0.0047));
        // lambanin onde kuma dusurdugu isik havuzu (kapinin onu bos)
        float2 pool = (q - float2(0.0, -0.0035)) / float2(0.016, 0.0036);
        float poolLight = exp(-dot(pool, pool)) * step(q.y, 0.0);

        col += float3(0.60, 0.40, 0.22) * poolLight * 0.55;
        col = lerp(col, float3(0.42, 0.33, 0.27), 1.0 - smoothstep(-pixel, pixel, panel));
        col = lerp(col, lampCol, (1.0 - smoothstep(0.0, 1.2 * onePx, abs(opening))) * 0.85);   // aydinlik kasa
        col = lerp(col, float3(0.006, 0.005, 0.009), 1.0 - smoothstep(-pixel, pixel, opening + 0.8 * onePx));   // ici kapkaranlik
        col += lampCol * (glowDot(cs, lampPos, 1.8 * sc) + glowDot(cs, lampPos, 8.0 * sc) * 0.35);
    }
}

// Sahne tek bir fotograf gibi davranir: arayuz (klavye, ipucu) alani daraltinca kamera yerinden oynamaz, yalnizca
// gorus acisi genisler; bu, fotografi kucultup kaydirmakla ayni seydir (Oyun.FitCamera). Arka plan da ayni fotografin
// parcasidir: ekran konumu fotograf konumuna cevrilir, boylece koloni ile zemin birlikte kuculur, birbirine gore kaymaz.
float2 MarsPictureUV(float2 suv)
{
    float2 n = suv * 2.0 - 1.0;
    n = float2(n.x, n.y - _Picture.y) / max(_Picture.x, 1e-3);
    return n * 0.5 + 0.5;
}

// Inis ovasi (Bolge 1) arka plani: ekran konumuna (suv, alttan 0) gore renk (sRGB).
float3 PlainBackdrop(float2 suv)
{
    float2 uv = MarsPictureUV(suv);
    float aspect = _ScreenParams.x / _ScreenParams.y;
    float x = (uv.x - 0.5) * aspect;       // fotograf yuksekligi biriminde
    float picturePx = _ScreenParams.y * max(_Picture.x, 1e-3); // ekrandaki bir pikselin fotograftaki karsiligi (kenar yumusatma)
    float pixel = 1.5 / picturePx;
    float onePx = 1.0 / picturePx;
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

    // Mars'in iki uydusu: Phobos (yakin, kraterli kucuk yumru) ve Deimos (uzak; Mars'tan parlak bir yildiz gibi gorunur)
    float4 phobos = rock(sp, float2(0.14, 0.935), 0.0105, 1.0, pixel); col = lerp(col, phobos.rgb, phobos.a);
    col += float3(0.95, 0.92, 0.86) * glowDot(sp, float2(-0.17, 0.885), 1.6 * _ScreenParams.y / 1170.0) * 0.9 * starMask;

    // Tepeler (arkadan aydinlanan siluetler)
    float3 warm = float3(0.95, 0.55, 0.28);
    float mesa = smoothstep(0.42, 0.62, ridge(x * 1.9, 3.0));
    float farH  = _Horizon - 0.018 + mesa * 0.042 + ridge(x * 9.0, 5.0) * 0.006;
    float midH  = _Horizon - 0.034 + ridge(x * 3.2, 11.0) * 0.040;
    float P = _Horizon - 0.047;   // koloni platformu
    float2 cs = sp - float2(COLONY_X, 0.0);   // koloni biraz ortaya
    // yakin yapilarin altinda tepe duzlesir; uzak yapilarin onunde alcalir ki gorunsunler
    float pad = smoothstep(-0.25, -0.21, cs.x) * (1.0 - smoothstep(0.045, 0.08, cs.x));
    pad = max(pad, bandMask(x, MINING_X - 0.047, MINING_X + 0.049, 0.02));
    pad = max(pad, bandMask(x, HABITAT_X - 0.032, HABITAT_X + 0.034, 0.015));
    float nearH = lerp(_Horizon - 0.054 + ridge(x * 4.4, 23.0) * 0.026, P, pad);
    float valley = max(bandMask(x, GREEN_X - 0.03, GREEN_X + 0.03, 0.02), max(bandMask(x, MAST_X - 0.01, MAST_X + 0.01, 0.015), bandMask(x, LANDER_X - 0.025, LANDER_X + 0.025, 0.02)));
    nearH = lerp(nearH, min(nearH, _Horizon - 0.046), valley);

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

    // Uzak yapilar: orta tepelerin onunde, kucuk ve puslu
    float3 siteLights = 0.0;
    float3 siteHalo = 0.0;
    float sc = _ScreenParams.y / 1170.0;
    if (abs(x) > 0.28 && uv.y > _Horizon + FAR_BASE - 0.005 && uv.y < _Horizon + FAR_BASE + 0.04)
    {
        float farD = farSitesShape(sp, _Horizon);
        float fm = 1.0 - smoothstep(-pixel, pixel, farD);
        float fAbove = 1.0 - smoothstep(-pixel, pixel, farSitesShape(sp + float2(0.0, 1.5 * onePx), _Horizon));
        col = lerp(col, float3(0.080, 0.060, 0.080), fm);
        col += warm * fm * (1.0 - fAbove) * (exp(-dSun * 4.0) + 0.35) * 0.45;
        // sera tonozlari icten pembe isikla aydinlanir
        if (x > 0.30 && x < 0.37)
        {
            float vm = 1.0 - smoothstep(-pixel, pixel, greenhouseVaults((sp - float2(GREEN_X, _Horizon + FAR_BASE)) / FAR_SCALE) * FAR_SCALE);
            col = lerp(col, float3(0.38, 0.15, 0.31), vm * 0.8);
        }
    }
    siteLights += farSitesLights(sp, _Horizon, t, sc, siteHalo);

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
    // yapilarin bize dogru uzanan golgesi (gunes arkadan vurdugu icin)
    float shadow = 0.0;
    [unroll] for (int r = 0; r < SITE_ROWS; r++)
    {
        float base = _Horizon + kRowBase[r];
        if (abs(x) < 0.45 && uv.y < base && uv.y > base - 0.05)
        {
            float depth = base - uv.y;
            float2 q = float2(x + (x - sunPos.x) * depth * 6.0, base + depth * 3.5);
            // yumusak, bulanik gölge (yansima gibi keskin olmasin)
            float blur = 0.002 + depth * 0.35;
            float sm = 1.0 - smoothstep(-blur, blur, rowSitesShape(r, q, _Horizon));
            shadow = max(shadow, sm * exp(-depth * 70.0 / kRowScale[r]) * 0.8);
        }
    }
    plain += sunOnGround * (1.0 - 0.85 * shadow);
    plain *= 1.0 - 0.35 * shadow;
    col = lerp(col, lerp(nearC + warm * rimNear, plain, smoothstep(0.0, 0.03, plainT)), mNear);

    // Koloni ve yakindaki yapilar (siluet)
    float3 cLights = 0.0;
    float halo = 0.0;
    bool nearBand = uv.y > P - 0.01 && uv.y < P + 0.08;
    float frontMask = 0.0;   // orta/on siradaki yapilar: toz pusu onlari daha az orter (bize daha yakinlar)
    [unroll] for (int row = 0; row < SITE_ROWS; row++)
    {
        float base = _Horizon + kRowBase[row];
        if (abs(x) < 0.45 && uv.y > base - 0.01 && uv.y < base + 0.08)
        {
            float cm = 1.0 - smoothstep(-pixel, pixel, rowSitesShape(row, sp, _Horizon));
            // isik sadece ust kenarlara vurur (arkadan, alcaktan gelen gunes)
            float above = 1.0 - smoothstep(-pixel, pixel, rowSitesShape(row, sp + float2(0.0, 2.0 * onePx), _Horizon));
            bool mainColony = row == 0 && cs.x > -0.26 && cs.x < 0.08;
            float rim = cm * (1.0 - above) * (exp(-dSun * 4.0) + (mainColony ? 0.0 : 0.35));
            col = lerp(col, kRowColor[row], cm);
            if (row > 0) frontMask = max(frontMask, cm);
            col += warm * rim * 0.45;
        }
    }
    if (abs(x) > 0.10 && uv.y > _Horizon - 0.075 && uv.y < _Horizon - 0.01) siteLights += rowSitesLights(sp, _Horizon, t, sc, siteHalo);
    if (x > 0.14 && x < 0.28 && nearBand) siteLights += miningLights(sp - float2(MINING_X, P), t, sc, siteHalo);
    if (x > -0.31 && x < -0.20 && nearBand) siteLights += habitatLights(sp - float2(HABITAT_X, P), t, sc, onePx, siteHalo);

    // Ana koloninin isiklari
    if (cs.x > -0.26 && cs.x < 0.08 && nearBand)
    {
        float lights = 0.0;
        float traceHidesWindows = max(_TraceDoor, step(0.0001, _TraceGreenhouse));   // kapi ya da saksi ortadaki pencerelerin yerinde
        [unroll] for (int k = -2; k <= 2; k++)
            lights += glowDot(cs, float2(BIG_DOME.x + k * 0.0055 * TRACE_SCALE, P + 0.0045), 1.2 * sc) * step(0.2, hash11(k + 3.0)) * (abs(k) <= 1 ? 1.0 - traceHidesWindows : 1.0);
        lights += glowDot(cs, float2(-0.095, P + 0.004), 1.1 * sc) + glowDot(cs, float2(-0.089, P + 0.004), 1.1 * sc);
        lights += glowDot(cs, float2(-0.070, P + 0.003), 1.0 * sc);
        lights += glowDot(cs, float2(-0.197, P + 0.009), 1.0 * sc) * 0.8 + glowDot(cs, float2(-0.182, P + 0.007), 0.9 * sc) * 0.6;
        lights += glowDot(cs, float2(-0.150, P + 0.0024), 0.9 * sc) + glowDot(cs, float2(-0.144, P + 0.0024), 0.9 * sc);
        float strip = 1.0 - smoothstep(-onePx, onePx, sdBox(cs - float2(-0.055, P + 0.0415), float2(0.0055, 0.0011)));
        lights += strip * 0.9;
        [unroll] for (int m = 0; m < 3; m++) lights += glowDot(cs, float2(-0.0065, P + 0.008 + m * 0.012), 0.9 * sc) * 0.7;
        cLights += float3(1.0, 0.74, 0.45) * saturate(lights);

        // sisin icinde dagilan hale
        halo += glowDot(cs, float2(BIG_DOME.x, P + 0.005), 9.0 * sc) * 0.9;
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

    // Enerji hucresi lambalari (yalnizca enerji bolumlerinde; ufkun hemen altindaki serit)
    float3 powerLights = 0.0;
    float3 powerHalo = 0.0;
    if (_PowerLampCount > 0.5 && abs(uv.y - (_Horizon - 0.05)) < 0.03) powerLights = powerLamps(sp, t, sc, onePx, powerHalo);

    // Koloniler arasinda yuk tasiyan dronelar
    [unroll] for (int dr = 0; dr < DRONE_COUNT; dr++)
        drawDrone(sp, t, dr + 1.0, 11.0 + fmod(dr * 3.7, 6.0), _Horizon, sc, pixel, col, siteLights);

    // ufuk hizasinda alcak sis (gunese yakin daha sicak)
    col += lerp(float3(0.12, 0.08, 0.09), float3(0.30, 0.15, 0.08), exp(-dSun * 4.0)) * exp(-abs(uv.y - nearH) * 45.0) * 0.30;

    // Hafif kum firtinasi: tepelerin ve koloninin ustunden yavasca suzulen dagnik toz
    float hb = (uv.y - (_Horizon - 0.03)) / 0.06;
    float hazeBand = exp(-hb * hb);
    float2 hp = float2(x * 5.0 - t * 0.035, uv.y * 20.0);
    float warp = fbm(hp * 0.6 + float2(t * 0.015, 3.0));
    float wisps = smoothstep(0.35, 0.80, fbm(hp + float2(warp * 1.6, warp * 0.4)));
    float haze = hazeBand * (0.15 + 0.85 * wisps) * 0.6 * (1.0 - 0.6 * frontMask);
    float3 hazeCol = lerp(float3(0.27, 0.18, 0.18), float3(0.56, 0.31, 0.18), exp(-dSun * 3.0));
    col = lerp(col, hazeCol, haze);
    // koloni isiklari sisin icinden parlar, etraflarinda hafif hale olusur
    col += cLights * 1.3;
    col += float3(1.0, 0.66, 0.38) * halo * (0.10 + 0.35 * haze);
    col += siteLights * 1.6;
    col += siteHalo * (0.16 + 0.35 * haze);
    col += powerLights * 1.4 + powerHalo * 0.5;
    if (abs(cs.x - BIG_DOME.x) < 0.035 && abs(uv.y - P) < 0.03) colonyTraces(cs, P, t, sc, pixel, onePx, col);
    return col * MARS_EXPOSURE;
}

#include "PolarSky.hlsl"

// Ekran konumuna (suv, alttan 0) gore arka plan rengi (sRGB): bolgenin kendi manzarasi
float3 MarsBackdrop(float2 suv)
{
    float3 col;
    [branch] if (MarsPolar()) col = PolarBackdrop(suv);
    else col = PlainBackdrop(suv);
    return col;
}

float MarsVignette(float2 uv)
{
    float aspect = _ScreenParams.x / _ScreenParams.y;
    float2 vd = float2((uv.x - 0.5) * aspect / max(aspect, 0.4), uv.y - 0.5);
    return 1.0 - 0.35 * smoothstep(0.3, 0.85, length(vd * float2(1.4, 1.0)));
}

// Toz pusu: oyun alanindan (area: yari boyut x, z) uzaklastikca zemin solar ve ufuktaki ovanin rengine yaklasir;
// en cok koloniye dogru, yanlarda daha az, kameraya dogru cok az. Gercek uzakliga bagli oldugu icin fotograf
// kuculse de (klavye acik) gecis yumusak kalir; zemin ufka vardiginda arka planla zaten ayni renktedir.
float3 MarsDustHaze(float3 col, float3 posWS, float2 area)
{
    float2 o = max(abs(posWS.xz) - area, 0.0);
    if (posWS.z < 0.0) o.y *= 0.4;
    float dist = length(o * float2(0.8, 1.0));
    float haze = smoothstep(0.4, 7.0, dist) * 0.8;
    // ufuk dibindeki ovanin rengi (ova: ekrandan olculdu; kutup: PolarSky.hlsl)
    float3 plainCol = SRGBToLinear(MarsPolar() ? POLAR_PLAIN : float3(0.176, 0.110, 0.106) * MARS_EXPOSURE);
    return lerp(col, plainCol, haze);
}

// Zemindeki her sey (zemin, uzaktaki kayalar) ayni kuralla arka plana karisir: once toz pusu, sonra uzaklastikca
// (fogRange: z basla, z bit) ve ekranda cizilen ufka yaklastikca. Kamera geri cekilip zemin ekranda yukari kaysa da
// zemin nerede gokyuzune donuyorsa ustundeki kaya da orada kaybolur (havada asili kaya kalmaz). Renkler dogrusal.
float3 MarsFadeToBackdrop(float3 col, float3 posWS, float2 suv, float2 fogRange, float2 area)
{
    col = MarsDustHaze(col, posWS, area);
    float distFog = smoothstep(fogRange.x, fogRange.y, posWS.z);
    float scrFog = smoothstep(_Horizon - 0.15, _Horizon - 0.06, MarsPictureUV(suv).y);
    float fog = max(distFog, scrFog);
    if (fog > 0.001)
    {
        float3 bd = SRGBToLinear(saturate(MarsBackdrop(suv) * MarsVignette(suv)));
        col = lerp(col, bd, fog);
    }
    return col;
}

// Kutup buzulunda cevredeki kaya ve esyalarin ustune kar yagmistir; yanlari soguk, mavimsi. Renkler dogrusal.
float3 MarsRegionSurface(float3 albedo, float3 n)
{
    if (!MarsPolar()) return albedo;
    float3 cold = dot(albedo, float3(0.3, 0.5, 0.2)) * float3(0.85, 0.95, 1.2);
    return lerp(cold, float3(0.55, 0.62, 0.74), smoothstep(0.45, 0.75, n.y));
}

#endif
