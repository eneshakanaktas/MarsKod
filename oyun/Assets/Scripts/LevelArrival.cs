using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bolum girisi (Ragip, 2026-11-19; docs/tasarim/canli-cevre.md "Bolum girisi"): bolum acilinca toplanacak parcalar (ve bolgeye
// gore engeller) sahneye bolgenin temasina uygun bir gosteriyle gelir: Bolge 1'de gokten meteor gibi duser, Bolge 10'da
// volkan patlar ve lav toplari tahtaya yayilir... Yalniz gorunus: nesneler gosteri bitince tam olarak BuildLayout'un koydugu
// yerde, boyda ve donuste durur (Settle).
// - Bilgi karti, gecis yazisi, kayit gibi bir pencere aciksa gosteri onun kapanmasini bekler (oyuncu kacirmasin).
// - Calistir / Baştan al / duzen degisimi gosteriyi aninda bitirir (Finish): her sey yerine oturur.
// - Gosteriler bolgeye gore (IArrivalShow, ArrivalShows.cs); ortak hareketler ArrivalMoves, ortak efektler ArrivalFx.
public enum ArrivalKind { Pickup, Hazard, Obstacle }

// Gosteriye katilan bir nesne ve gosteri bitince donecegi hal
public sealed class Arrival
{
    public readonly Transform Item;
    public readonly ArrivalKind Kind;
    public readonly Vector3 Home, HomeScale;
    public readonly Quaternion HomeRotation;

    public Arrival(Transform item, ArrivalKind kind)
    {
        Item = item;
        Kind = kind;
        Home = item.localPosition;
        HomeScale = item.localScale;
        HomeRotation = item.localRotation;
    }

    public void Hide() => Item.localScale = Vector3.zero;

    public void Settle()
    {
        if (Item == null) return;
        Item.localPosition = Home;
        Item.localRotation = HomeRotation;
        Item.localScale = HomeScale;
    }
}

// Bir bolgenin giris gosterisi
public interface IArrivalShow
{
    /// <summary>Bu tur nesne gosteriye katilir mi (katilmayanlar bastan yerinde durur)</summary>
    bool Moves(ArrivalKind kind);

    IEnumerator Play(ArrivalStage stage, IReadOnlyList<Arrival> items);
}

// Gosterinin elindekiler: coroutine sahibi, efektler, alanin boyu, kamera, bolgenin renkleri
public sealed class ArrivalStage
{
    public LevelArrival Host;
    public ArrivalFx Fx;
    public Vector2 AreaHalf;
    public Camera Cam;
    public RegionFx Look;
    public Func<float, float, float> Height;

    /// <summary>Her nesne icin bir hareket baslatir (gap saniye arayla) ve hepsi bitene kadar bekler</summary>
    public IEnumerator Each(IReadOnlyList<Arrival> items, float gap, Func<Arrival, int, IEnumerator> move)
    {
        int left = items.Count;
        for (int i = 0; i < items.Count; i++)
        {
            Host.StartCoroutine(Counted(move(items[i], i), () => left--));
            if (gap > 0f && i < items.Count - 1) yield return Tween.Wait(gap);
        }
        while (left > 0) yield return null;
    }

    static IEnumerator Counted(IEnumerator move, Action done)
    {
        yield return move;
        done();
    }
}

public sealed class LevelArrival : MonoBehaviour
{
    readonly List<Arrival> all = new List<Arrival>();
    ArrivalFx fx;
    bool playing;

    public bool Playing => playing;

    public void Add(Transform item, ArrivalKind kind) => all.Add(new Arrival(item, kind));

    /// <summary>Gosteriye katilacak nesneler hemen gizlenir; covered dogru oldugu surece (pencere acik) beklenir, sonra gosteri oynar</summary>
    public void Begin(IArrivalShow show, ArrivalStage stage, Func<bool> covered)
    {
        if (show == null) return;
        var moving = all.FindAll(a => a.Item != null && show.Moves(a.Kind));
        if (moving.Count == 0) return;
        foreach (var a in moving) a.Hide();
        fx = ArrivalFx.Create(transform.parent);
        stage.Host = this;
        stage.Fx = fx;
        playing = true;
        StartCoroutine(Run(show, stage, moving, covered));
    }

    IEnumerator Run(IArrivalShow show, ArrivalStage stage, List<Arrival> moving, Func<bool> covered)
    {
        yield return Tween.Wait(0.25f);
        while (covered()) yield return null;
        yield return Tween.Wait(0.2f);
        yield return show.Play(stage, moving);
        Finish();
    }

    /// <summary>Gosteri aninda biter: her nesne yerine oturur, havadaki efektler soner</summary>
    public void Finish()
    {
        if (!playing) return;
        playing = false;
        StopAllCoroutines();
        foreach (var a in all) a.Settle();
        fx?.Release();
        fx = null;
        VolcanoSky.Burst(0f);
    }

    void OnDestroy()
    {
        if (playing) VolcanoSky.Burst(0f);
        fx?.Release();
    }
}

// Gosterilerin ortak efektleri: isikli iz, kivilcim, toz, duman, yerde yayilan halka
public sealed class ArrivalFx
{
    readonly Transform root;
    readonly ParticleSystem trail, sparks, dust, smoke;
    readonly Sprinkler rnd = new Sprinkler(77);

    ArrivalFx(Transform parent)
    {
        root = Parts.Empty("Giris-Efekt", parent);
        trail = ParticleFx.Create("Iz", root, 1400, 0.45f, glow: true);
        ParticleFx.SizeOverLife(trail, 1f, 0.2f);
        sparks = ParticleFx.Create("Kivilcim", root, 500, 0.6f, gravity: 0.6f, glow: true);
        dust = ParticleFx.Create("Toz", root, 500, 0.05f);
        ParticleFx.SizeOverLife(dust, 0.5f, 1.4f);
        smoke = ParticleFx.Create("Duman", root, 400, 0.05f);
        ParticleFx.SizeOverLife(smoke, 0.6f, 1.8f);
    }

    public static ArrivalFx Create(Transform parent) => new ArrivalFx(parent);

    public Sprinkler Rnd => rnd;

    static int Count(int full) => GraphicsQuality.Performance ? Mathf.Max(1, full / 2) : full;

    /// <summary>Kalkan taneler gecis bitince solup gider; kok sonra silinir</summary>
    public void Release()
    {
        if (root == null) return;
        foreach (var ps in new[] { trail, sparks, dust, smoke }) ParticleFx.Finish(ps);
        UnityEngine.Object.Destroy(root.gameObject, 3f);
    }

    /// <summary>Hareket eden nesnenin arkasinda kalan isikli iz (meteor, lav topu, isin)</summary>
    public void Trail(Vector3 at, Color color, float size, float life = 0.35f)
    {
        ParticleFx.Emit(trail, at + Jitter(size * 0.3f), Jitter(0.08f), size * rnd.Range(0.7f, 1.2f), life * rnd.Range(0.8f, 1.2f), color);
    }

    /// <summary>Her yone sicrayan kivilcim</summary>
    public void Sparks(Vector3 at, Color color, int count, float speed)
    {
        for (int i = 0, n = Count(count); i < n; i++)
        {
            float a = rnd.Range(0f, Mathf.PI * 2f), s = rnd.Range(0.4f, 1f) * speed;
            ParticleFx.Emit(sparks, at, new Vector3(Mathf.Cos(a) * s, rnd.Range(0.6f, 1.6f) * speed * 0.8f, Mathf.Sin(a) * s),
                rnd.Range(0.04f, 0.08f), rnd.Range(0.3f, 0.6f), rnd.Mix(color, Color.white));
        }
    }

    /// <summary>Yerde halka gibi disari yayilan toz (carpma, inis)</summary>
    public void DustRing(Vector3 at, Color color, int count, float speed)
    {
        for (int i = 0, n = Count(count); i < n; i++)
        {
            float a = (i + rnd.Range(-0.3f, 0.3f)) / n * Mathf.PI * 2f, s = rnd.Range(0.6f, 1f) * speed;
            ParticleFx.Emit(dust, at + new Vector3(Mathf.Cos(a) * 0.1f, 0.05f, Mathf.Sin(a) * 0.1f),
                new Vector3(Mathf.Cos(a) * s, rnd.Range(0.05f, 0.25f), Mathf.Sin(a) * s), rnd.Range(0.16f, 0.3f), rnd.Range(0.5f, 0.9f), color);
        }
    }

    /// <summary>Yukari kabaran toz/kar/kum (spread: yatay dagilma)</summary>
    public void Puff(Vector3 at, Color color, int count, float spread, float rise)
    {
        for (int i = 0, n = Count(count); i < n; i++)
            ParticleFx.Emit(dust, at + Jitter(spread), new Vector3(rnd.Range(-spread, spread), rnd.Range(0.3f, 1f) * rise, rnd.Range(-spread, spread)),
                rnd.Range(0.14f, 0.26f), rnd.Range(0.5f, 0.9f), color);
    }

    /// <summary>Koyu duman (lav topunun izi, volkan)</summary>
    public void Smoke(Vector3 at, Color color, float size)
    {
        ParticleFx.Emit(smoke, at + Jitter(0.05f), new Vector3(rnd.Range(-0.1f, 0.1f), rnd.Range(0.1f, 0.3f), rnd.Range(-0.1f, 0.1f)),
            size * rnd.Range(0.8f, 1.2f), rnd.Range(0.6f, 1.1f), color);
    }

    /// <summary>Yerde iceriden disari yayilan isik halkasi</summary>
    public IEnumerator Ring(Vector3 at, Color color)
    {
        var holder = Parts.Empty("Halka", root);
        holder.localPosition = at;
        var ring = PickupFx.AddRing(holder, color, out var mat);
        yield return PickupFx.RingPulse(ring, mat);
        UnityEngine.Object.Destroy(holder.gameObject);
    }

    Vector3 Jitter(float r) => new Vector3(rnd.Range(-r, r), rnd.Range(-r, r), rnd.Range(-r, r));
}

// Ortak hareketler: yay cizerek ucma, sekme, inis ezilmesi, yerden buyume
public static class ArrivalMoves
{
    /// <summary>from'dan nesnenin yerine yay cizerek gelir (height: yayin tepesi, ease: hiz egrisi). Her karede onStep(konum) cagrilir
    /// (iz birakmak icin). spin: yol boyunca kendi etrafinda donus (derece)</summary>
    public static IEnumerator Arc(Arrival a, Vector3 from, float height, float seconds, Func<float, float> ease, Action<Vector3> onStep,
        Vector3 spinAxis, float spin, float startScale = 1f)
    {
        var control = Vector3.Lerp(from, a.Home, 0.5f) + Vector3.up * height;
        yield return Tween.Run(seconds, t =>
        {
            float e = ease(t);
            var p = Vector3.Lerp(Vector3.Lerp(from, control, e), Vector3.Lerp(control, a.Home, e), e);
            a.Item.localPosition = p;
            a.Item.localScale = a.HomeScale * Mathf.Lerp(startScale, 1f, Mathf.Min(1f, t * 3f));
            a.Item.localRotation = a.HomeRotation * Quaternion.AngleAxis(spin * (1f - e), spinAxis);
            onStep?.Invoke(p);
        });
        a.Item.localPosition = a.Home;
        a.Item.localRotation = a.HomeRotation;
        a.Item.localScale = a.HomeScale;
    }

    /// <summary>Yerinde azalan sekmeler; her yere degiste onLand</summary>
    public static IEnumerator Bounce(Arrival a, float height, int times, Action<Vector3> onLand)
    {
        for (int k = 0; k < times; k++)
        {
            float h = height / (k + 1f), d = 0.16f + 0.1f * h;
            yield return Tween.Run(d * 2f, t => a.Item.localPosition = a.Home + Vector3.up * (h * 4f * t * (1f - t)));
            a.Item.localPosition = a.Home;
            onLand?.Invoke(a.Home);
        }
    }

    /// <summary>Yere inince bir an basik, sonra yerine esner</summary>
    public static IEnumerator Squash(Arrival a, float amount = 0.3f)
    {
        yield return Tween.Run(0.22f, t =>
        {
            float s = Mathf.Sin(t * Mathf.PI) * amount * (1f - t);
            a.Item.localScale = Vector3.Scale(a.HomeScale, new Vector3(1f + s, 1f - s, 1f + s));
        });
        a.Item.localScale = a.HomeScale;
    }

    /// <summary>Yerin altindan yukselerek ve buyuyerek cikar (depth: ne kadar asagidan)</summary>
    public static IEnumerator Grow(Arrival a, float seconds, float depth, float turn = 0f)
    {
        yield return Tween.Run(seconds, t =>
        {
            float e = Tween.OutBack(t);
            a.Item.localPosition = a.Home - Vector3.up * depth * (1f - Tween.OutCubic(t));
            a.Item.localScale = a.HomeScale * Mathf.Max(0f, e);
            a.Item.localRotation = a.HomeRotation * Quaternion.Euler(0f, turn * (1f - Tween.OutCubic(t)), 0f);
        });
        a.Settle();
    }
}
