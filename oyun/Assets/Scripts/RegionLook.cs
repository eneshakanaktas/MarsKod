using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.Rendering;

// Bolgenin havasi: isik renkleri, ortam isigi ve cizimlere (zemin, arka plan, kayalar) "hangi bolgedeyiz" bilgisi.
// Manzaranin kendisini cizimler secer (MarsSky.hlsl: PlainBackdrop / PolarSky.hlsl, Ground.shader: regolith / iceSheet).
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

    public static RegionLook For(Region r) => r == Region.PolarIce ? Polar : Plain;

    public void Apply(Light sky, Light sun)
    {
        sky.color = Mats.Hex(skyLight);
        sky.intensity = skyIntensity;
        sun.color = Mats.Hex(sunLight);
        sun.intensity = sunIntensity;
        // zemin cizimi (Ground.shader) gunes isigini buradan okur
        Shader.SetGlobalVector("_DawnDir", sun.transform.forward);
        Shader.SetGlobalVector("_DawnColor", sun.color.linear * sun.intensity);
        Shader.SetGlobalFloat("_Region", shaderRegion);

        // Yumusak ortam isigi: ustten serin gokyuzu, yanlardan bolgenin rengi
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Mats.Hex(ambientSky);
        RenderSettings.ambientEquatorColor = Mats.Hex(ambientEquator);
        RenderSettings.ambientGroundColor = Mats.Hex(ambientGround);
        var sh = new SphericalHarmonicsL2();
        sh.AddAmbientLight(Mats.Hex(probeBase).linear * AmbientLift);
        sh.AddDirectionalLight(Vector3.up, Mats.Hex(probeTop).linear * AmbientLift, 0.9f);
        RenderSettings.ambientProbe = sh;
    }
}
