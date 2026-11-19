using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsKod.Dunya;
using UnityEngine;

// Bolgelerin giris gosterileri (LevelArrival.cs; docs/tasarim/canli-cevre.md "Bolum girisi"). Her biri ~2-3 sn.
// Yeni bolge: IArrivalShow'u uygulayan bir sinif + For'a bir satir.
public static class ArrivalShows
{
    public static IArrivalShow For(Region region)
    {
        switch (region)
        {
            case Region.Plain: return new MeteorShower();
            case Region.PolarIce: return new FrostGrowth();
            case Region.CraterField: return new DustGust();
            case Region.Dunes: return new SandRise();
            case Region.AntennaHill: return new RollIn();
            case Region.Canyon: return new CliffDrop();
            case Region.RiverBed: return new GhostRiver();
            case Region.Station: return new TeleportBeam();
            case Region.SaltLake: return new SaltBloom();
            case Region.NightPlain: return new VolcanoBurst();
            default: return null;
        }
    }

    // Ortak: tohumlu karisik sira (her bolumde ayni sira; gosteri her giriste ayni oynar)
    internal static List<Arrival> Shuffled(IReadOnlyList<Arrival> items, int seed)
    {
        var rnd = new System.Random(seed + items.Count);
        return items.OrderBy(_ => rnd.Next()).ToList();
    }
}

// Bolge 1 (inis ovasi): parcalar ve kayalar gokten atesli meteor gibi duser; carpinca kivilcim, toz halkasi, isik halkasi
public sealed class MeteorShower : IArrivalShow
{
    static readonly Color Fire = Mats.Hex("#FF8A3D"), Hot = Mats.Hex("#FFE0A0"), Smoke = new Color(0.25f, 0.18f, 0.16f, 0.45f);

    public bool Moves(ArrivalKind kind) => true;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        var fx = stage.Fx;
        yield return stage.Each(ArrivalShows.Shuffled(items, 1), 0.17f, (a, i) => Fall(stage, a, fx.Rnd.Range(0.85f, 1.15f)));
    }

    IEnumerator Fall(ArrivalStage stage, Arrival a, float reach)
    {
        var fx = stage.Fx;
        var from = a.Home + new Vector3(-2.2f, 7.5f, 2.6f) * reach;
        var trailColor = a.Kind == ArrivalKind.Pickup ? stage.Look.Sparkle : Fire;
        yield return ArrivalMoves.Arc(a, from, 0f, 0.55f, t => t * t, p =>
        {
            for (int k = 0; k < 3; k++) fx.Trail(p, fx.Rnd.Chance(0.5f) ? trailColor : Hot, 0.16f);
            if (a.Kind != ArrivalKind.Pickup) fx.Smoke(p, Smoke, 0.22f);
        }, Vector3.right, 520f);
        Sound.Thud();
        fx.Sparks(a.Home + Vector3.up * 0.08f, Fire, 18, 2.2f);
        fx.DustRing(a.Home, stage.Look.Dust, 14, 1.4f);
        stage.Host.StartCoroutine(fx.Ring(a.Home, Fire));
        yield return ArrivalMoves.Squash(a, 0.35f);
    }
}

// Bolge 2 (kutup): soguk bir dalga alani capraz gecer; gectigi karede yer catlar gibi isik halkasi, kar tozu kalkar, buz
// yerden yukselerek buyur (engel buz bloklari daha derinden)
public sealed class FrostGrowth : IArrivalShow
{
    static readonly Color Frost = Mats.Hex("#CFF6FF"), Snow = new Color(0.92f, 0.96f, 1f, 0.6f);

    public bool Moves(ArrivalKind kind) => true;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        var wave = items.OrderBy(a => a.Home.x + a.Home.z).ToList();   // sol onden sag arkaya capraz dalga
        Sound.Shimmer();
        yield return stage.Each(wave, 0.13f, (a, i) => Grow(stage, a));
    }

    IEnumerator Grow(ArrivalStage stage, Arrival a)
    {
        var fx = stage.Fx;
        stage.Host.StartCoroutine(fx.Ring(a.Home, Frost));
        fx.Sparks(a.Home + Vector3.up * 0.05f, Frost, 10, 0.9f);
        fx.Puff(a.Home, Snow, 10, 0.25f, 0.8f);
        yield return ArrivalMoves.Grow(a, 0.5f, a.Kind == ArrivalKind.Obstacle ? 0.6f : 0.25f, 40f);
    }
}

// Bolge 3 (kraterli duzluk): batidan bir toz ruzgari gecer; panel parcalari onunla savrulup takla atarak gelir, sekerek yerine duser.
// Agir bazalt kayalar yerinde durur.
public sealed class DustGust : IArrivalShow
{
    const float BandSpeed = 4.5f;

    public bool Moves(ArrivalKind kind) => kind == ArrivalKind.Pickup;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        var fx = stage.Fx;
        float from = -stage.AreaHalf.x - 2.5f, to = stage.AreaHalf.x + 2.5f;
        stage.Host.StartCoroutine(Band(stage, from, to));
        yield return stage.Each(items, 0f, (a, i) => Tumble(stage, a, (a.Home.x - 2.6f - from) / BandSpeed));
    }

    IEnumerator Band(ArrivalStage stage, float from, float to)
    {
        var fx = stage.Fx;
        yield return Tween.Run((to - from) / BandSpeed, t =>
        {
            float x = Mathf.Lerp(from, to, t);
            for (int k = 0; k < 4; k++)
                fx.Puff(new Vector3(x + fx.Rnd.Range(-0.6f, 0.2f), 0.1f, fx.Rnd.Range(-stage.AreaHalf.y - 0.5f, stage.AreaHalf.y + 0.5f)),
                    stage.Look.Dust, 1, 0.6f, 0.5f);
        });
    }

    IEnumerator Tumble(ArrivalStage stage, Arrival a, float delay)
    {
        var fx = stage.Fx;
        if (delay > 0f) yield return Tween.Wait(delay);
        var from = a.Home + new Vector3(-2.6f, 0.9f, fx.Rnd.Range(-0.3f, 0.3f));
        yield return ArrivalMoves.Arc(a, from, 0.5f, 0.6f, Tween.OutCubic, null, Vector3.forward, -540f);
        Sound.Thud();
        fx.DustRing(a.Home, stage.Look.Dust, 8, 0.8f);
        yield return ArrivalMoves.Bounce(a, 0.18f, 2, p => fx.DustRing(p, stage.Look.Dust, 5, 0.5f));
    }
}

// Bolge 4 (kum tepeleri): parcanin yerinde kum girdap yapar, parca kumun altindan donerek firlar, kum fiskiyesi sacilir
public sealed class SandRise : IArrivalShow
{
    public bool Moves(ArrivalKind kind) => kind == ArrivalKind.Pickup;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        yield return stage.Each(ArrivalShows.Shuffled(items, 4), 0.2f, (a, i) => Rise(stage, a));
    }

    IEnumerator Rise(ArrivalStage stage, Arrival a)
    {
        var fx = stage.Fx;
        var sand = stage.Look.Dust;
        // girdap: kum, yerin uzerinde daralan bir halka cizerek doner
        yield return Tween.Run(0.45f, t =>
        {
            float r = Mathf.Lerp(0.45f, 0.1f, t), ang = t * 14f;
            for (int k = 0; k < 2; k++)
            {
                float a2 = ang + k * Mathf.PI;
                fx.Puff(a.Home + new Vector3(Mathf.Cos(a2) * r, 0.04f, Mathf.Sin(a2) * r), sand, 1, 0.05f, 0.2f);
            }
        });
        Sound.Thud();
        fx.Puff(a.Home, sand, 16, 0.35f, 1.6f);
        yield return ArrivalMoves.Grow(a, 0.45f, 0.5f, 360f);
        yield return ArrivalMoves.Squash(a, 0.2f);
    }
}

// Bolge 5 (anten tepesi): kablo makaralari tepenin ustunden (alanin arkasindan) yuvarlanarak, kucuk sekmelerle gelir
public sealed class RollIn : IArrivalShow
{
    const float Speed = 3.2f, Radius = 0.18f;

    public bool Moves(ArrivalKind kind) => kind == ArrivalKind.Pickup;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        yield return stage.Each(items.OrderByDescending(a => a.Home.z).ToList(), 0.18f, (a, i) => Roll(stage, a));
    }

    IEnumerator Roll(ArrivalStage stage, Arrival a)
    {
        var fx = stage.Fx;
        var from = new Vector3(a.Home.x, a.Home.y, stage.AreaHalf.y + 2.2f);
        float distance = from.z - a.Home.z;
        a.Item.localScale = a.HomeScale;
        float lastHop = 0f;
        yield return Tween.Run(distance / Speed, t =>
        {
            float e = Tween.OutCubic(t);
            float travelled = distance * e;
            float hop = Mathf.Abs(Mathf.Sin(travelled * 2.2f)) * 0.12f * (1f - e);
            a.Item.localPosition = Vector3.Lerp(from, a.Home, e) + Vector3.up * hop;
            a.Item.localRotation = Quaternion.AngleAxis(-(distance - travelled) / Radius * Mathf.Rad2Deg, Vector3.right) * a.HomeRotation;
            if (hop < 0.01f && travelled - lastHop > 0.5f) { lastHop = travelled; fx.Puff(a.Item.localPosition, stage.Look.Dust, 2, 0.15f, 0.3f); }
        });
        a.Settle();
        Sound.Thud();
        fx.DustRing(a.Home, stage.Look.Dust, 8, 0.7f);
        yield return ArrivalMoves.Squash(a, 0.18f);
    }
}

// Bolge 6 (kanyon girisi): parcalar kanyonun yukarisindan duser, arkasinda kirintilar dokulur, sekerek yerine oturur
public sealed class CliffDrop : IArrivalShow
{
    static readonly Color Grit = new Color(0.42f, 0.27f, 0.2f, 0.55f);

    public bool Moves(ArrivalKind kind) => kind == ArrivalKind.Pickup;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        yield return stage.Each(ArrivalShows.Shuffled(items, 6), 0.2f, (a, i) => Drop(stage, a));
    }

    IEnumerator Drop(ArrivalStage stage, Arrival a)
    {
        var fx = stage.Fx;
        var from = new Vector3(a.Home.x + fx.Rnd.Range(-0.6f, 0.6f), 4.2f, stage.AreaHalf.y + 2.5f);
        var axis = new Vector3(fx.Rnd.Range(-1f, 1f), fx.Rnd.Range(-1f, 1f), fx.Rnd.Range(-1f, 1f)).normalized;
        yield return ArrivalMoves.Arc(a, from, 1.0f, 0.75f, t => t * t, p => { if (fx.Rnd.Chance(0.6f)) fx.Smoke(p, Grit, 0.12f); }, axis, 420f);
        Sound.Thud();
        fx.DustRing(a.Home, stage.Look.Dust, 12, 1.1f);
        yield return ArrivalMoves.Bounce(a, 0.2f, 2, p => fx.DustRing(p, stage.Look.Dust, 5, 0.5f));
    }
}

// Bolge 7 (eski nehir yatagi): kurumus nehrin bir anlik hayali akar (arkadan one gecen mavi-beyaz su dalgasi, kopuk, parilti);
// dalga gectigi yerde cakillari birakir (cakil islak parlar)
public sealed class GhostRiver : IArrivalShow
{
    const float WaveSeconds = 1.7f;
    static readonly Color Foam = new Color(0.78f, 0.9f, 1f, 0.5f), Water = new Color(0.45f, 0.7f, 0.95f, 0.35f), Shine = Mats.Hex("#B8F0FF");

    public bool Moves(ArrivalKind kind) => kind == ArrivalKind.Pickup;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        float from = stage.AreaHalf.y + 1.5f, to = -stage.AreaHalf.y - 1.2f;
        Sound.Shimmer();
        stage.Host.StartCoroutine(Wave(stage, from, to));
        yield return stage.Each(items, 0f, (a, i) => Leave(stage, a, (from - a.Home.z) / (from - to) * WaveSeconds));
    }

    IEnumerator Wave(ArrivalStage stage, float from, float to)
    {
        var fx = stage.Fx;
        float halfX = stage.AreaHalf.x + 0.8f;
        yield return Tween.Run(WaveSeconds, t =>
        {
            float z = Mathf.Lerp(from, to, t);
            for (int k = 0; k < 5; k++)
            {
                float x = fx.Rnd.Range(-halfX, halfX);
                fx.Puff(new Vector3(x, 0.05f, z + fx.Rnd.Range(-0.15f, 0.25f)), fx.Rnd.Chance(0.5f) ? Foam : Water, 1, 0.3f, 0.25f);
                if (fx.Rnd.Chance(0.4f)) fx.Trail(new Vector3(fx.Rnd.Range(-halfX, halfX), 0.06f, z), Shine, 0.09f, 0.5f);
            }
        });
    }

    IEnumerator Leave(ArrivalStage stage, Arrival a, float delay)
    {
        var fx = stage.Fx;
        if (delay > 0f) yield return Tween.Wait(delay);
        stage.Host.StartCoroutine(fx.Ring(a.Home, Shine));
        fx.Sparks(a.Home + Vector3.up * 0.05f, Shine, 6, 0.6f);
        yield return ArrivalMoves.Grow(a, 0.35f, 0.05f);
    }
}

// Bolge 8 (istasyon): veri kartlari tavandan inen mavi bir isik sutununda belirir (isinlanir gibi titreyerek)
public sealed class TeleportBeam : IArrivalShow
{
    static readonly Color Beam = Mats.Hex("#7FD8FF");

    public bool Moves(ArrivalKind kind) => kind == ArrivalKind.Pickup;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        yield return stage.Each(ArrivalShows.Shuffled(items, 8), 0.25f, (a, i) => Materialize(stage, a));
    }

    IEnumerator Materialize(ArrivalStage stage, Arrival a)
    {
        var fx = stage.Fx;
        // isik sutunu yukaridan asagi tarar
        yield return Tween.Run(0.45f, t =>
        {
            float y = Mathf.Lerp(2.2f, 0.05f, t);
            for (int k = 0; k < 4; k++)
            {
                float ang = fx.Rnd.Range(0f, Mathf.PI * 2f);
                fx.Trail(a.Home + new Vector3(Mathf.Cos(ang) * 0.18f, y, Mathf.Sin(ang) * 0.18f), Beam, 0.07f, 0.45f);
            }
        });
        Sound.Shimmer();
        stage.Host.StartCoroutine(fx.Ring(a.Home, Beam));
        for (int k = 0; k < 4; k++)   // titreyerek belirir
        {
            a.Item.localScale = k % 2 == 0 ? a.HomeScale : Vector3.zero;
            yield return Tween.Wait(0.06f);
        }
        a.Settle();
        fx.Sparks(a.Home + Vector3.up * 0.15f, Beam, 8, 0.6f);
    }
}

// Bolge 9 (tuz golu): altin bir halka dalgalanir, tuz kristali parildayarak yerden buyur; ortadan disari yayilan sirayla
public sealed class SaltBloom : IArrivalShow
{
    static readonly Color Gold = Mats.Hex("#F4D35E"), Brine = new Color(0.95f, 0.92f, 0.82f, 0.55f);

    public bool Moves(ArrivalKind kind) => kind == ArrivalKind.Pickup;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        Sound.Shimmer();
        yield return stage.Each(items.OrderBy(a => new Vector2(a.Home.x, a.Home.z).sqrMagnitude).ToList(), 0.12f, (a, i) => Bloom(stage, a));
    }

    IEnumerator Bloom(ArrivalStage stage, Arrival a)
    {
        var fx = stage.Fx;
        stage.Host.StartCoroutine(fx.Ring(a.Home, Gold));
        fx.Sparks(a.Home + Vector3.up * 0.05f, Gold, 8, 0.6f);
        fx.Puff(a.Home, Brine, 6, 0.2f, 0.4f);
        yield return ArrivalMoves.Grow(a, 0.55f, 0.1f, 90f);
    }
}

// Bolge 10 (gece ovasi): ufuktaki volkan patlar (NightSky.hlsl); agzindan firlayan lav toplari ates ve duman iziyle yay cizip
// tahtaya duser (goktaslari, kayalar); dustugu yerde kivilcim, koyu toz, isik halkasi
public sealed class VolcanoBurst : IArrivalShow
{
    const float EruptionSeconds = 3.2f, FirstBomb = 0.55f, FlightSeconds = 1.1f;
    static readonly Color Lava = Mats.Hex("#FF7A2A"), Hot = Mats.Hex("#FFD27A"), Smoke = new Color(0.22f, 0.16f, 0.24f, 0.55f);
    static readonly Color Ash = new Color(0.26f, 0.2f, 0.28f, 0.6f);

    public bool Moves(ArrivalKind kind) => true;

    public IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items)
    {
        var mouth = VolcanoSky.Mouth(stage.Cam);
        float end = Time.time + EruptionSeconds;
        Sound.Eruption();
        stage.Host.StartCoroutine(Eruption());
        yield return Tween.Wait(FirstBomb);
        yield return stage.Each(ArrivalShows.Shuffled(items, 10), 0.14f, (a, i) => Bomb(stage, a, mouth));
        while (Time.time < end) yield return null;   // patlama sonuna kadar soner (gosteri bitince Finish onu keser)
    }

    static IEnumerator Eruption()
    {
        yield return Tween.Run(EruptionSeconds, t => VolcanoSky.Burst(Mathf.Max(0.001f, t)));
        VolcanoSky.Burst(0f);
    }

    IEnumerator Bomb(ArrivalStage stage, Arrival a, Vector3 mouth)
    {
        var fx = stage.Fx;
        float height = Mathf.Max(4f, Vector3.Distance(mouth, a.Home) * 0.35f);
        yield return ArrivalMoves.Arc(a, mouth, height, FlightSeconds, t => t, p =>
        {
            for (int k = 0; k < 3; k++) fx.Trail(p, fx.Rnd.Chance(0.6f) ? Lava : Hot, 0.18f, 0.4f);
            fx.Smoke(p, Smoke, 0.25f);
        }, Vector3.right, 720f, 0.4f);
        Sound.Thud();
        fx.Sparks(a.Home + Vector3.up * 0.08f, Lava, 22, 2.4f);
        fx.DustRing(a.Home, Ash, 14, 1.3f);
        stage.Host.StartCoroutine(fx.Ring(a.Home, Lava));
        yield return ArrivalMoves.Squash(a, 0.35f);
    }
}
