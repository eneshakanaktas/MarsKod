using System.Collections;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.Rendering;

// Bolgenin havasi: isik renkleri, ortam isigi ve cizimlere (zemin, arka plan, kayalar) "hangi bolgedeyiz" bilgisi.
// Manzaranin kendisini cizimler secer (MarsSky.hlsl: PlainBackdrop / PolarSky.hlsl / CraterSky.hlsl / DuneSky.hlsl,
// Ground.shader: regolith / iceSheet / craterPlain / duneSand).
// Yeni bolge: buraya bir tablo + cizimlerde _Region'a gore bir dal.
public class RegionLook
{
    // cok hafif genel aydinlatma; arka planin karsiligi MarsSky.hlsl MARS_EXPOSURE
    const float AmbientLift = 1.12f;

    float shaderRegion;
    string skyLight; float skyIntensity;        // ustten gelen los gok isigi (golge verir)
    string sunLight; float sunIntensity;        // tepelerin/ucurumlarin ardindaki gunes (arkadan, golgesiz)
    string ambientSky, ambientEquator, ambientGround, probeBase, probeTop;

    // Inis ovasi: safaktan hemen once; soguk los gok isigi, arkadan sicak turuncu safak
    static readonly RegionLook Plain = new RegionLook
    {
        shaderRegion = 0f,
        skyLight = "#B9B8D8", skyIntensity = 0.58f,
        sunLight = "#FF9E66", sunIntensity = 0.53f,
        ambientSky = "#5E5B7C", ambientEquator = "#4C4352", ambientGround = "#241D25",
        probeBase = "#524C60", probeTop = "#4A4A6A",
    };

    // Kutup buzulu: mavi gun batimi; her sey soguk ve mavimsi
    static readonly RegionLook Polar = new RegionLook
    {
        shaderRegion = 1f,
        skyLight = "#C2CFEA", skyIntensity = 0.55f,
        sunLight = "#8DB4FF", sunIntensity = 0.55f,
        ambientSky = "#5A6A8E", ambientEquator = "#465064", ambientGround = "#1C2230",
        probeBase = "#4A5470", probeTop = "#46557C",
    };

    // Kraterli duzluk: safak cok yakin; gok isigi lavanta, gunes duvarin ardinda pembe-turuncu, ovadakinden biraz aydinlik
    static readonly RegionLook Crater = new RegionLook
    {
        shaderRegion = 2f,
        skyLight = "#B6B2DC", skyIntensity = 0.62f,
        sunLight = "#FF8C74", sunIntensity = 0.62f,
        ambientSky = "#605A84", ambientEquator = "#58465A", ambientGround = "#261C26",
        probeBase = "#584E66", probeTop = "#504C74",
    };

    // Kum tepeleri: oyundaki ilk gunduz (Bolge 3 gun dogumuyla bitti); tozlu, sicak, aydinlik
    static readonly RegionLook Dunes = new RegionLook
    {
        shaderRegion = 3f,
        skyLight = "#FFE4C6", skyIntensity = 0.92f,
        sunLight = "#FFC48E", sunIntensity = 0.55f,
        ambientSky = "#9C7C6E", ambientEquator = "#8A6652", ambientGround = "#3A281F",
        probeBase = "#7A5E50", probeTop = "#8E7266",
    };

    // Bolge 3 finali: oyundaki ilk gun dogumu (docs/tasarim/senaryo-bolge-03.md, kapanis). Ova safaginin aydinlanmis hali.
    static readonly RegionLook SunriseLook = new RegionLook
    {
        shaderRegion = 0f,
        skyLight = "#FFE6C8", skyIntensity = 0.95f,
        sunLight = "#FFC27A", sunIntensity = 1.0f,
        ambientSky = "#8C7E8E", ambientEquator = "#7A5E58", ambientGround = "#3A2A26",
        probeBase = "#7A6464", probeTop = "#8A7A86",
    };

    public static RegionLook For(Region r)
    {
        switch (r)
        {
            case Region.PolarIce: return Polar;
            case Region.CraterField: return Crater;
            case Region.Dunes:
            case Region.AntennaHill: return Dunes; // anten tepesinin kendi gorunusu gelene kadar kum tepeleri
            default: return Plain;
        }
    }

    public void Apply(Light sky, Light sun) => Blend(this, this, 0f, sky, sun);

    // Bu bolgenin isigindan gun dogumu isigina yavasca gecer
    public IEnumerator Sunrise(Light sky, Light sun, float duration)
    {
        yield return Tween.Run(duration, t => Blend(this, SunriseLook, Tween.InOutCubic(t), sky, sun));
    }

    static Color Mix(string a, string b, float t) => Color.Lerp(Mats.Hex(a), Mats.Hex(b), t);

    static void Blend(RegionLook a, RegionLook b, float t, Light sky, Light sun)
    {
        sky.color = Mix(a.skyLight, b.skyLight, t);
        sky.intensity = Mathf.Lerp(a.skyIntensity, b.skyIntensity, t);
        sun.color = Mix(a.sunLight, b.sunLight, t);
        sun.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
        // zemin cizimi (Ground.shader) gunes isigini buradan okur
        Shader.SetGlobalVector("_DawnDir", sun.transform.forward);
        Shader.SetGlobalVector("_DawnColor", sun.color.linear * sun.intensity);
        Shader.SetGlobalFloat("_Region", a.shaderRegion);
        // kraterli duzlukte gunes duvarin ustunden yukselir (CraterSky.hlsl); baska gecislerde 0
        Shader.SetGlobalFloat("_Sunrise", b == SunriseLook ? t : 0f);

        // Yumusak ortam isigi: ustten serin gokyuzu, yanlardan bolgenin rengi
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Mix(a.ambientSky, b.ambientSky, t);
        RenderSettings.ambientEquatorColor = Mix(a.ambientEquator, b.ambientEquator, t);
        RenderSettings.ambientGroundColor = Mix(a.ambientGround, b.ambientGround, t);
        var sh = new SphericalHarmonicsL2();
        sh.AddAmbientLight(Mix(a.probeBase, b.probeBase, t).linear * AmbientLift);
        sh.AddDirectionalLight(Vector3.up, Mix(a.probeTop, b.probeTop, t).linear * AmbientLift, 0.9f);
        RenderSettings.ambientProbe = sh;
    }
}
