using System;
using System.Collections.Generic;
using UnityEngine;

// Canli cevrenin ortak parcalari (AmbientLife.cs): her bolge bunlardan kendi karisimini kurar (RegionAmbience).
// Her parca kendi tanelerini/nesnelerini kurar ve her karede Tick ile oynar. gust: ortak ruzgar (Wind.Gust, 0-1).
// Yeni parca: IAmbientPiece'i uygulayan bir sinif; bolge sinifi onu Add ile ekler.
public interface IAmbientPiece
{
    void Tick(float time, float dt, float density, float gust);
}

// Bolge siniflarinin ortak iskeleti: parcalari sirayla oynatir, ruzgari bir kez hesaplar
public abstract class RegionAmbience : IAmbience
{
    protected readonly AmbienceSpace Space;
    readonly List<IAmbientPiece> pieces = new List<IAmbientPiece>();

    protected RegionAmbience(AmbienceSpace space) { Space = space; }

    protected T Add<T>(T piece) where T : IAmbientPiece
    {
        pieces.Add(piece);
        return piece;
    }

    public virtual void SetLevel(int levelNumber) { }

    public void Tick(float time, float dt, float density)
    {
        float gust = Wind.Gust(time);
        for (int i = 0; i < pieces.Count; i++) pieces[i].Tick(time, dt, density, gust);
    }

    /// <summary>Alanin disinda, zeminde rastgele bir nokta (box: x araligi ve z araligi)</summary>
    protected static Func<Sprinkler, Vector3> AroundArea(AmbienceSpace space, Vector2 xs, Vector2 zs, float lift, float margin = 0.25f) => rnd =>
    {
        for (int tries = 0; ; tries++)
        {
            float x = rnd.Range(xs.x, xs.y), z = rnd.Range(zs.x, zs.y);
            if (!space.InArea(x, z, margin) || tries > 6) return space.OnGround(x, z, lift);
        }
    };
}

// Gokten yavasca dusen taneler (kar). Bolum acilir acilmaz yagiyor olsun diye taneler kutunun her yuksekligine dagitilarak baslar.
public sealed class Snowfall : IAmbientPiece
{
    readonly ParticleSystem ps;
    readonly AmbienceSpace space;
    readonly Sprinkler rnd;
    readonly Vector2 xs, zs;
    readonly float top, perSecond, fall;
    readonly Color color;
    readonly Vector2 size;

    public Snowfall(AmbienceSpace space, int seed, Vector2 xs, Vector2 zs, float top, float perSecond, float fall, Color color, Vector2 size)
    {
        this.space = space; this.xs = xs; this.zs = zs; this.top = top; this.perSecond = perSecond; this.fall = fall; this.color = color; this.size = size;
        rnd = new Sprinkler(seed);
        ps = ParticleFx.Create("Kar", space.Root, 900, 0.5f);
        ParticleFx.Sway(ps, 0.18f, 0.6f);
        int count = Mathf.RoundToInt(perSecond * top / fall * (GraphicsQuality.Performance ? 0.45f : 1f));
        for (int i = 0; i < count; i++) Flake(rnd.Range(0.1f, top), 0f);
    }

    public void Tick(float time, float dt, float density, float gust)
    {
        for (int n = rnd.Due(perSecond * density, dt); n > 0; n--) Flake(top, gust);
    }

    void Flake(float y, float gust)
    {
        float x = rnd.Range(xs.x, xs.y), z = rnd.Range(zs.x, zs.y);
        float speed = rnd.Range(0.8f, 1.2f) * fall;
        float life = Mathf.Max(0.5f, (y - space.Height(x, z)) / speed);
        ParticleFx.Emit(ps, new Vector3(x, y, z), new Vector3(rnd.Range(0.08f, 0.22f) + 0.35f * gust, -speed, rnd.Range(-0.05f, 0.05f)),
            rnd.Range(size.x, size.y), life, color);
    }
}

// Yere yakin, ruzgarla doguya akan yumusak pus (savrulan kar, kum, tuz, toz). Ruzgar dalgasi gelince artar.
public sealed class GroundDrift : IAmbientPiece
{
    readonly ParticleSystem ps;
    readonly AmbienceSpace space;
    readonly Sprinkler rnd;
    readonly float calm, gustRate;
    readonly Color color;

    public GroundDrift(AmbienceSpace space, int seed, Color color, float calm, float gustRate)
    {
        this.space = space; this.color = color; this.calm = calm; this.gustRate = gustRate;
        rnd = new Sprinkler(seed);
        ps = ParticleFx.Create("SavrulanPus", space.Root, 260, 0f);
        ParticleFx.SizeOverLife(ps, 0.6f, 1.3f);
    }

    public void Tick(float time, float dt, float density, float gust)
    {
        for (int n = rnd.Due((calm + gustRate * gust) * density, dt); n > 0; n--)
        {
            float x = rnd.Range(-9f, 6f), z = rnd.Range(-4.5f, 8.5f);
            ParticleFx.Emit(ps, space.OnGround(x, z, rnd.Range(0.04f, 0.18f)),
                new Vector3(rnd.Range(0.9f, 1.7f) * (0.6f + gust), rnd.Range(0f, 0.05f), rnd.Range(-0.15f, 0.15f)),
                rnd.Range(0.45f, 0.9f), rnd.Range(3.5f, 6f), color);
        }
    }
}

// Havada suzulen zerreler (toz, polen gibi isikta parlayan nokta): yavasca salinir, ruzgarla hafifce kayar.
public sealed class Motes : IAmbientPiece
{
    readonly ParticleSystem ps;
    readonly Sprinkler rnd;
    readonly Func<Sprinkler, Vector3> where;
    readonly float perSecond;
    readonly Color a, b;
    readonly Vector2 size;

    public Motes(AmbienceSpace space, int seed, Func<Sprinkler, Vector3> where, float perSecond, Color a, Color b, Vector2 size, bool glow)
    {
        this.where = where; this.perSecond = perSecond; this.a = a; this.b = b; this.size = size;
        rnd = new Sprinkler(seed);
        ps = ParticleFx.Create("Zerre", space.Root, 260, glow ? 0.45f : 0.3f, glow: glow);
        ParticleFx.Sway(ps, 0.12f, 0.4f);
        for (int i = 0; i < perSecond * 3f; i++) Mote(0f, rnd.Range(0.2f, 1f));   // acilista havada zaten zerre var
    }

    public void Tick(float time, float dt, float density, float gust)
    {
        for (int n = rnd.Due(perSecond * density, dt); n > 0; n--) Mote(gust, 1f);
    }

    void Mote(float gust, float lifeShare)
    {
        ParticleFx.Emit(ps, where(rnd), new Vector3(0.05f + 0.25f * gust, rnd.Range(-0.02f, 0.05f), rnd.Range(-0.03f, 0.03f)),
            rnd.Range(size.x, size.y), rnd.Range(4f, 7f) * lifeShare, rnd.Mix(a, b));
    }
}

// Bir an yanip sonen parilti (buz, tuz, islak cakil, metal): yerinde, isik gibi
public sealed class Glints : IAmbientPiece
{
    readonly ParticleSystem ps;
    readonly Sprinkler rnd;
    readonly Func<Sprinkler, Vector3> where;
    readonly float perSecond;
    readonly Color a, b;
    readonly Vector2 size;

    /// <summary>glow: isik gibi parlar (koyu zeminde); acik zeminde (tuz) false: duz renkli belirgin nokta</summary>
    public Glints(AmbienceSpace space, int seed, Func<Sprinkler, Vector3> where, float perSecond, Color a, Color b, Vector2 size, bool glow = true)
    {
        this.where = where; this.perSecond = perSecond; this.a = a; this.b = b; this.size = size;
        rnd = new Sprinkler(seed);
        ps = ParticleFx.Create("Parilti", space.Root, 120, 0.55f, glow: glow);
    }

    public void Tick(float time, float dt, float density, float gust)
    {
        for (int n = rnd.Due(perSecond * density, dt); n > 0; n--)
            ParticleFx.Emit(ps, where(rnd), Vector3.zero, rnd.Range(size.x, size.y), rnd.Range(0.35f, 0.7f), rnd.Mix(a, b));
    }
}

// Yukari suzulen akkor kivilcim (kizgin goktasi, kor): dogarken parlak, yukseldikce soner
public sealed class Embers : IAmbientPiece
{
    readonly ParticleSystem ps;
    readonly Sprinkler rnd;
    readonly Func<Sprinkler, Vector3> where;
    readonly float perSecond;
    readonly Color a, b;

    public Embers(AmbienceSpace space, int seed, Func<Sprinkler, Vector3> where, float perSecond, Color a, Color b)
    {
        this.where = where; this.perSecond = perSecond; this.a = a; this.b = b;
        rnd = new Sprinkler(seed);
        ps = ParticleFx.Create("Kor", space.Root, 160, 0.5f, glow: true);
        ParticleFx.Sway(ps, 0.25f, 0.9f);
    }

    public void Tick(float time, float dt, float density, float gust)
    {
        for (int n = rnd.Due(perSecond * density, dt); n > 0; n--)
            ParticleFx.Emit(ps, where(rnd), new Vector3(0.1f * gust, rnd.Range(0.25f, 0.6f), 0f), rnd.Range(0.045f, 0.085f), rnd.Range(1.2f, 2.2f), rnd.Mix(a, b));
    }
}

// Toz hortumu: ovada yavasca gezen, kendi etrafinda donen toz sutunu. Bir yol boyunca gider, ucunda soner, bir sure sonra
// baska bir yerden yeniden dogar. Parcacik sisteminin kendisi yol boyunca tasinir; taneler onun etrafinda doner (orbital hiz).
public sealed class DustDevil : IAmbientPiece
{
    readonly ParticleSystem ps;
    readonly Transform column;
    readonly AmbienceSpace space;
    readonly Sprinkler rnd;
    readonly Color color;
    readonly Vector2 xs, zs;
    readonly float height, travelSeconds, restSeconds;
    float age;
    Vector3 from, to;

    public DustDevil(AmbienceSpace space, int seed, Color color, Vector2 xs, Vector2 zs, float height, float travelSeconds, float restSeconds)
    {
        this.space = space; this.color = color; this.xs = xs; this.zs = zs; this.height = height;
        this.travelSeconds = travelSeconds; this.restSeconds = restSeconds;
        rnd = new Sprinkler(seed);
        column = Parts.Empty("TozHortumu", space.Root);
        ps = ParticleFx.Create("Toz", column, 500, 0.08f);
        var spin = ps.velocityOverLifetime;
        spin.enabled = true;
        spin.space = ParticleSystemSimulationSpace.Local;
        spin.orbitalY = new ParticleSystem.MinMaxCurve(5.5f);
        spin.radial = new ParticleSystem.MinMaxCurve(0.05f);
        spin.x = spin.y = spin.z = new ParticleSystem.MinMaxCurve(0f);
        ParticleFx.SizeOverLife(ps, 0.7f, 1.4f);
        NewPath();
        age = travelSeconds * 0.35f;   // acilista yolun ortasinda
    }

    void NewPath()
    {
        float z = rnd.Range(zs.x, zs.y);
        bool east = rnd.Chance(0.5f);
        from = new Vector3(east ? xs.x : xs.y, 0f, z);
        to = new Vector3(east ? xs.y : xs.x, 0f, z + rnd.Range(-1.2f, 1.2f));
        age = 0f;
    }

    public void Tick(float time, float dt, float density, float gust)
    {
        age += dt;
        if (age > travelSeconds + restSeconds) NewPath();
        float k = Mathf.Clamp01(age / travelSeconds);
        var p = Vector3.Lerp(from, to, k);
        p.x += 0.4f * Mathf.Sin(time * 0.7f);
        column.localPosition = space.OnGround(p.x, p.z);
        if (age > travelSeconds) return;
        float strength = Mathf.Sin(k * Mathf.PI);   // dogar, buyur, soner
        for (int n = rnd.Due(60f * strength * density, dt); n > 0; n--)
        {
            float a = rnd.Range(0f, Mathf.PI * 2f);
            var c = color;
            if (rnd.Chance(0.25f))
            {
                // dipte yere yayilan toz etegi: hortumun yerden kalktigi belli olsun (uzakta pusa karisip havada asili gorunmesin)
                float rs = rnd.Range(0.12f, 0.4f);
                ParticleFx.Emit(ps, new Vector3(Mathf.Cos(a) * rs, rnd.Range(0.02f, 0.1f), Mathf.Sin(a) * rs), new Vector3(Mathf.Cos(a) * 0.15f, 0.03f, Mathf.Sin(a) * 0.15f),
                    rnd.Range(0.3f, 0.5f), rnd.Range(1f, 1.6f), c);
                continue;
            }
            float h = Mathf.Pow(rnd.Range(0f, 1f), 1.6f) * height;
            float r = 0.08f + h * 0.32f;   // yukari dogru genisleyen huni
            c.a *= 1f - h / height * 0.6f;
            ParticleFx.Emit(ps, new Vector3(Mathf.Cos(a) * r, h, Mathf.Sin(a) * r), new Vector3(0f, rnd.Range(0.08f, 0.25f), 0f),
                rnd.Range(0.18f, 0.32f) * (0.6f + h / height), rnd.Range(1f, 1.6f), c);
        }
    }
}

// Yanip sonen lamba (direk tepesi, anten, istasyon): kendi hizinda keskin bir parlama
public sealed class BlinkLamp : IAmbientPiece
{
    readonly Material mat;
    readonly Color color;
    readonly float speed, phase;

    public BlinkLamp(Transform parent, Vector3 localPos, float radius, Color color, float speed, float phase)
    {
        this.color = color; this.speed = speed; this.phase = phase;
        mat = Mats.Emissive(color, color, 0.5f);
        Parts.Add("Lamba", parent, MeshFactory.Sphere(radius, 6, 10), mat, localPos, outline: false, castShadow: false);
    }

    public void Tick(float time, float dt, float density, float gust)
    {
        float on = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(time * speed + phase), 10f);
        mat.SetColor("_EmissionColor", color * (0.2f + 2.6f * on));
    }
}

// Ruzgarla donen parca (ruzgar olcer kepceleri, pervane, ruzgar turbini): hiz ruzgarla artar
public sealed class Spinner : IAmbientPiece
{
    readonly Transform part;
    readonly Vector3 axis;
    readonly float calm, gustSpeed;
    float angle;

    public Spinner(Transform part, Vector3 axis, float calm, float gustSpeed)
    {
        this.part = part; this.axis = axis; this.calm = calm; this.gustSpeed = gustSpeed;
    }

    public void Tick(float time, float dt, float density, float gust)
    {
        angle += dt * (calm + gustSpeed * gust);
        part.localRotation = Quaternion.AngleAxis(angle, axis);
    }
}
