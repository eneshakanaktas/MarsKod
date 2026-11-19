using System.Collections;
using MarsKod.Dunya;
using UnityEngine;

// Oyunun "hissi" (Ragip, 2026-11-19; docs/tasarim/canli-cevre.md): robot bir kareye varinca iticisi zeminden toz/kar kaldirir,
// engele carpinca kucuk bir toz patlar, bir sey toplayinca parilti sacilir, bolum bitince robotun ustunden kutlama parıltısı
// yukselir. Renkler bolgeye gore (RegionFx). Oyuncu eylemine verilen tepki oldugu icin "Arka plan animasyonlari" ayarindan
// bagimsizdir; performans modunda tane sayisi azalir.
public sealed class PlayFx
{
    readonly ParticleSystem dust, sparks;
    readonly Sprinkler rnd = new Sprinkler(91);
    RegionFx look = RegionFx.For(Region.Plain);

    PlayFx(Transform parent)
    {
        dust = ParticleFx.Create("Fx-Toz", parent, 260, 0.05f);
        ParticleFx.SizeOverLife(dust, 0.55f, 1.25f);
        sparks = ParticleFx.Create("Fx-Parilti", parent, 400, 0.6f, gravity: 0.35f, glow: true);
    }

    public static PlayFx Create(Transform world) => new PlayFx(world);

    public void SetRegion(Region region) => look = RegionFx.For(region);

    static int Count(int full) => GraphicsQuality.Performance ? Mathf.Max(1, full / 2) : full;

    /// <summary>Robot kareye vardi: altindan halka seklinde alcak bir toz/kar kalkar</summary>
    public void Step(Vector3 at)
    {
        int n = Count(10);
        for (int i = 0; i < n; i++)
        {
            float a = (i + rnd.Range(-0.3f, 0.3f)) / n * Mathf.PI * 2f, speed = rnd.Range(0.35f, 0.7f);
            ParticleFx.Emit(dust, at + new Vector3(Mathf.Cos(a) * 0.12f, 0.04f, Mathf.Sin(a) * 0.12f),
                new Vector3(Mathf.Cos(a) * speed, rnd.Range(0.02f, 0.12f), Mathf.Sin(a) * speed),
                rnd.Range(0.14f, 0.24f), rnd.Range(0.45f, 0.75f), look.Dust);
        }
    }

    /// <summary>Engele carpti: carpma noktasinda bir avuc toz ve birkac kivilcim</summary>
    public void Bump(Vector3 at)
    {
        for (int i = 0, n = Count(8); i < n; i++)
            ParticleFx.Emit(dust, at + new Vector3(rnd.Range(-0.08f, 0.08f), rnd.Range(0.05f, 0.25f), rnd.Range(-0.08f, 0.08f)),
                new Vector3(rnd.Range(-0.4f, 0.4f), rnd.Range(0.1f, 0.4f), rnd.Range(-0.4f, 0.4f)), rnd.Range(0.12f, 0.2f), rnd.Range(0.4f, 0.6f), look.Dust);
        for (int i = 0, n = Count(6); i < n; i++)
            ParticleFx.Emit(sparks, at + Vector3.up * rnd.Range(0.1f, 0.3f),
                new Vector3(rnd.Range(-1f, 1f), rnd.Range(0.5f, 1.4f), rnd.Range(-1f, 1f)), rnd.Range(0.03f, 0.05f), rnd.Range(0.25f, 0.4f), Mats.Hex("#FFB25A"));
    }

    /// <summary>Toplandi: nesnenin yerinden her yone parilti sacilir, bir kismi yukari suzulur</summary>
    public void Collect(Vector3 at)
    {
        for (int i = 0, n = Count(28); i < n; i++)
        {
            float a = rnd.Range(0f, Mathf.PI * 2f), speed = rnd.Range(0.6f, 1.6f);
            ParticleFx.Emit(sparks, at + Vector3.up * 0.15f, new Vector3(Mathf.Cos(a) * speed, rnd.Range(0.8f, 2f), Mathf.Sin(a) * speed),
                rnd.Range(0.06f, 0.11f), rnd.Range(0.5f, 0.85f), rnd.Mix(look.Sparkle, Color.white));
        }
        for (int i = 0, n = Count(8); i < n; i++)
            ParticleFx.Emit(sparks, at + new Vector3(rnd.Range(-0.15f, 0.15f), rnd.Range(0.1f, 0.3f), rnd.Range(-0.15f, 0.15f)),
                new Vector3(0f, rnd.Range(1.6f, 2.4f), 0f), rnd.Range(0.05f, 0.09f), rnd.Range(0.7f, 1f), look.Sparkle);
    }

    /// <summary>Bolum bitti: robotun cevresinden uc dalga halinde parilti fiskirir (bolgenin rengi + turuncu), yavasca suzulup soner</summary>
    public IEnumerator Celebrate(Vector3 at)
    {
        var orange = Mats.Hex("#FF9A4D");
        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0, n = Count(26); i < n; i++)
            {
                float a = rnd.Range(0f, Mathf.PI * 2f), r = rnd.Range(0.15f, 0.35f), speed = rnd.Range(0.3f, 0.9f);
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                ParticleFx.Emit(sparks, at + dir * r + Vector3.up * rnd.Range(0.2f, 0.6f), dir * speed + Vector3.up * rnd.Range(2f, 3.2f),
                    rnd.Range(0.05f, 0.1f), rnd.Range(0.9f, 1.4f), rnd.Chance(0.5f) ? rnd.Mix(look.Sparkle, Color.white) : orange);
            }
            yield return Tween.Wait(0.32f);
        }
    }
}

// Bolgeye gore efekt renkleri: zeminden kalkan toz (kar, kum, tuz...) ve parilti
public readonly struct RegionFx
{
    public readonly Color Dust, Sparkle;

    RegionFx(string dust, float dustAlpha, string sparkle)
    {
        var d = Mats.Hex(dust);
        Dust = new Color(d.r, d.g, d.b, dustAlpha);
        Sparkle = Mats.Hex(sparkle);
    }

    public static RegionFx For(Region region)
    {
        switch (region)
        {
            case Region.PolarIce: return new RegionFx("#E4ECF7", 0.55f, "#9FEAFF");
            case Region.CraterField: return new RegionFx("#9C6656", 0.45f, "#FFC47A");
            case Region.Dunes: return new RegionFx("#D6A06C", 0.5f, "#FFE08A");
            case Region.AntennaHill: return new RegionFx("#A8735C", 0.45f, "#FFB070");
            case Region.Canyon: return new RegionFx("#B86C4C", 0.45f, "#FFC77A");
            case Region.RiverBed: return new RegionFx("#9C8476", 0.45f, "#B8F0FF");
            case Region.Station: return new RegionFx("#8E8E9A", 0.35f, "#7FD8FF");
            case Region.SaltLake: return new RegionFx("#F2EEE4", 0.55f, "#F4E07A");
            case Region.NightPlain: return new RegionFx("#4E4656", 0.5f, "#5CFF96");
            default: return new RegionFx("#B0684C", 0.45f, "#FFD27A");
        }
    }
}
