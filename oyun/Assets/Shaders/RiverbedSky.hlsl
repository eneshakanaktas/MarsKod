// Nehir yatagi (Bolge 7) arka plani. MarsSky.hlsl icinden eklenir; onun yardimcilarini (gurultu) ve kanyonun gokyuzunu kullanir.
// Kanyonun dibindeyiz, kurumus nehir yatagi boyunca kuzeye bakiyoruz. Iki yanda yuksek, dik kaya duvarlari: ekranin
// kenarinda en yuksek, ortaya dogru alcalir; aralarinda kanyon uzakta daralir, kivrilip gozden kaybolur. Gunes batida (solda)
// alcakta, sol duvarin ardinda: disk gorunmez, sol duvarin tepesi arkadan isir. Sol duvar tamamen golgede; sag duvarin ust
// kismi alcak gunesi alir, alt kismi sol duvarin golgesinde (golge cizgisi). Duvarlarda yatay tortu katmanlari (eski nehrin
// biriktirdigi), diplerinde dokulmus moloz. Yildiz yok, pus yok (kanyondaki gibi).
#ifndef MARSKOD_RIVERBED_INCLUDED
#define MARSKOD_RIVERBED_INCLUDED

static const float3 RIVERBED_PLAIN = float3(0.39, 0.19, 0.12);   // ufuk dibindeki nehir yatagi: zeminin uzaktaki rengi (ekrandan olculur)
static const float3 RIVERBED_SHADE = float3(0.36, 0.25, 0.25);   // golgedeki duvar (serin gok isigi)
static const float3 RIVERBED_LIT = float3(0.80, 0.52, 0.36);     // alcak gunesi alan duvar

static const float3 RIVERBED_SIDE_PLAIN = float3(0.31, 0.15, 0.10);   // yan kanyonun ufuk dibi: zeminin uzaktaki rengi (ekrandan olculur)

// Duvarin ust kenari. side: -1 sol, +1 sag. s: kenara yakinlik (0 ortadaki bosluk, 1 ekran kenari).
// Dar yan kanyonda (67-70) duvarlar yakin ve yuksek: bosluk dar, duvar hemen dikilir, gokyuzu ince bir serit kalir.
float riverbedWallTop(float x, float side, out float s)
{
    float k = MarsSideCanyon();
    float start = lerp(side < 0.0 ? 0.028 : 0.036, side < 0.0 ? 0.016 : 0.02, k);
    float height = lerp(side < 0.0 ? 0.12 : 0.13, side < 0.0 ? 0.26 : 0.28, k);   // ekranin kenarinda yuksek: kanyonun icindeyiz
    s = saturate((x * side - start) / lerp(0.21, 0.13, k));
    // basamak basamak yukselen sirt: her basamagin ustu duza yakin (eski tortu katmanlari), aralari dik
    float v = pow(s, 1.15) * 5.0 + 0.35 * vnoise(x * 11.0 + side * 5.0);
    float stepped = min((floor(v) + smoothstep(0.0, 0.3, frac(v))) / 5.0, 1.0);
    float jag = 0.004 * (vnoise(x * 48.0 + side) - 0.5) + 0.0015 * vnoise(x * 150.0);   // kirik kenar
    return _Horizon + 0.006 + height * stepped * (0.85 + 0.15 * vnoise(floor(v) * 2.7 + side)) + jag * smoothstep(0.0, 0.2, s);
}

// Uzakta, kanyonun kivrildigi yerde birbirini ortan duvarlar (ortadaki bosluktan gorunur). Dar yan kanyonda yuksek: kanyon
// hemen ileride kivrilir, gokyuzu yalniz ustte V biciminde bir aciklik kalir (bosluk ufka kadar acik bir yarik gibi durmasin).
float riverbedFarTop(float x)
{
    float d = saturate(abs(x + 0.006) / 0.06);
    float rise = lerp(0.018 * d, 0.1 + 0.05 * d, MarsSideCanyon());
    return _Horizon + 0.004 + rise + 0.004 * vnoise(x * 30.0) + 0.0012 * vnoise(x * 110.0);
}

// Duvar yuzu: yatay katmanlar, birkac belirgin acik tabaka, cikinti/oyuk, dipte golgede kalan moloz etegi.
// lit: bu noktaya gunes vuruyor mu (0..1). top: duvarin ust kenari (orada ince isikli kenar).
float3 riverbedWallFace(float2 sp, float top, float lit, float rim, float onePx)
{
    float above = sp.y - _Horizon;
    float wave = 0.003 * vnoise(sp.x * 7.0 + 3.0);
    float3 c = lerp(RIVERBED_SHADE, RIVERBED_LIT, lit);
    c *= 0.88 + 0.12 * (sin((above + wave) * 260.0) * 0.5 + 0.5);                 // ince katmanlar
    c *= 0.94 + 0.06 * (sin((above + wave) * 95.0 + 0.7) * 0.5 + 0.5);
    c *= 0.86 + 0.24 * vnoise(sp.x * 22.0);                                        // cikintilar ve oyuklar: yuz duz bir bant olmasin
    // acik tabaka: dipte degil (ufuk boyunca acik bir serit sis gibi gorunur), duvarin ortalarinda
    c = lerp(c, c * 1.28 + 0.025, smoothstep(0.82, 0.96, sin((above + wave) * 55.0 + 3.0)) * 0.8);
    float edge = 1.0 - smoothstep(0.0, 2.0 * onePx, top - sp.y);
    c = lerp(c, lerp(RIVERBED_SHADE * 1.6, RIVERBED_LIT * 1.1, max(lit, rim)), edge * 0.6);   // ust kenar
    float talus = 1.0 - smoothstep(0.004, 0.018, above + 0.004 * vnoise(sp.x * 26.0));
    c = lerp(c, lerp(c * 0.85, RIVERBED_PLAIN * 0.95, 0.5), talus);                // moloz etegi: golgede, zeminle bulusur
    return c;
}

// Ufkun alti: duz nehir yatagi; ortadan, kanyonun kivrimindan (uzak duvarlarin dibi) cikan kurumus nehir bize dogru genisler.
// Zemindeki kolun devamidir (Ground.shader riverChannel): zemin ufka yakin bantta bu resme karisir, kol kesilmeden ufka gider.
// Genislik ve egim ekrandan olculdu (zemindeki kolun kenarlari bantta bu cizgilere oturur).
static const float RIVER_FOOT = 0.008;     // ufukta yari genislik (kanyonun kivrimindan cikis)
static const float RIVER_SPREAD = 0.6;    // ufkun altina inildikce genisleme (perspektif)
static const float RIVER_REACH = 0.04;     // bu kadar asagida zemindeki kolla bulusur (ortasi ekranin ortasi)

float3 riverbedPlainBelow(float2 sp, float onePx)
{
    float below = max(_Horizon - sp.y, 0.0);
    float halfW = (RIVER_FOOT + RIVER_SPREAD * below) * (0.95 + 0.1 * vnoise(below * 260.0 + 4.0));   // kiyilar duz cizgi degil
    // hafifce kivrilarak gelir (fazlasi yukselen duman gibi gorunur): ust ucu uzak duvarlarin bosluguna, alt ucu ekranin ortasina
    float centre = lerp(-0.006, 0.0, saturate(below / RIVER_REACH)) + 0.18 * halfW * sin(below * 160.0 + 0.8);
    float d = abs(sp.x - centre);
    float aa = 1.5 * onePx;
    float inside = 1.0 - smoothstep(halfW - aa, halfW + aa, d);
    float bw = max(0.06 * halfW, aa);
    float bank = exp(-pow((d - halfW * 1.04) / bw, 2.0)) * min(1.0, 0.06 * halfW / aa);   // ince kum seti; incelince soner
    float3 c = lerp(RIVERBED_PLAIN, RIVERBED_PLAIN * 0.87, inside);
    return lerp(c, RIVERBED_PLAIN * 1.1, bank * 0.4);
}

float3 RiverbedBackdrop(float2 suv)
{
    float2 uv = MarsPictureUV(suv);
    float aspect = _SkyScreen.x / _SkyScreen.y;
    float x = (uv.x - 0.5) * aspect;
    float picturePx = _SkyScreen.y * max(_Picture.x, 1e-3);
    float pixel = 1.5 / picturePx;
    float onePx = 1.0 / picturePx;
    float2 sp = float2(x, uv.y);

    // Gokyuzu: kanyondaki gun batimi; gunes sol duvarin ardinda, oranin gogu hafif isinir
    float skyT = saturate((uv.y - (_Horizon - 0.04)) / (1.0 - _Horizon + 0.04));
    float3 col = canyonSkyColor(skyT);
    float3 skyLow = canyonSkyColor(0.0);
    float2 sunPos = float2(-0.2, _Horizon + 0.07);
    col += float3(0.45, 0.30, 0.20) * exp(-length((sp - sunPos) * float2(1.0, 1.3)) * 6.0) * 0.35;

    // Kanyonun ici, uzakta: kivrimdaki alcak duvarlar, iki duvarin golgesinde
    float farTop = riverbedFarTop(x);
    float farMask = 1.0 - smoothstep(farTop - pixel, farTop + pixel, uv.y);
    float3 farCol = lerp(float3(0.34, 0.25, 0.27), skyLow, 0.15);
    // yan kanyonda yakin ve yuksek: duvar yuzu gibi katmanli, golgede, biraz solmus
    farCol = lerp(farCol, lerp(riverbedWallFace(sp, farTop, 0.0, 0.0, onePx), skyLow, 0.2), MarsSideCanyon());
    col = lerp(col, farCol, farMask);

    // Iki yandaki yuksek duvarlar
    [unroll] for (int side = -1; side <= 1; side += 2)
    {
        float s;
        float top = riverbedWallTop(x, side, s);
        float m = (1.0 - smoothstep(top - pixel, top + pixel, uv.y)) * smoothstep(0.0, 0.03, s);
        // sag duvar: sol duvarin golge cizgisinin ustu gunes alir; sol duvar: yalnizca tepesi arkadan isir
        // dar yan kanyonda alcak gunes yalniz sag duvarin tepesine vurur
        float shadowLine = _Horizon + lerp(0.014 + 0.025 * s, 0.14 + 0.08 * s, MarsSideCanyon()) + 0.006 * (vnoise(x * 20.0) - 0.5);
        float lit = side > 0 ? smoothstep(shadowLine - 0.002, shadowLine + 0.004, uv.y) : 0.0;
        float rim = side < 0 ? 1.0 : 0.0;
        float3 c = riverbedWallFace(sp, top, lit, rim, onePx);
        c = lerp(c, skyLow, 0.02 + 0.08 * (1.0 - s));   // ortaya (uzaga) dogru cok hafif solar
        col = lerp(col, c, m);
    }

    // Ufkun alti duz nehir yatagidir: zemin ufka vardiginda bu renktedir; ortasinda kanyondan gelen kurumus nehir
    // (yan kanyonda nehir yok: kucuk izler ana yataktan ayrilip buraya sapmisti)
    float3 below = lerp(riverbedPlainBelow(sp, onePx), RIVERBED_SIDE_PLAIN, MarsSideCanyon());
    col = lerp(col, below, 1.0 - smoothstep(_Horizon - 0.004, _Horizon + 0.003, uv.y));
    return col;
}

#endif
