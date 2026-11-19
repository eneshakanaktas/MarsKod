using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Degisen alanda ("duzenler") bir duzenden otekine gecis hareketi (A1, docs/superpowers/specs/2026-11-19-degisen-alan-gezme-design.md).
// Eski duzenin nesneleri (oldRoots'un cocuklari) ile yeni duzenin nesneleri (newRoots'un cocuklari) gecis boyunca birlikte durur:
// - Snow (Bolge 2, tipi): ruzgarla savrulan kar taneleri ve yere yakin kar sisi (Unity parcacik sistemi, ParticleFx) alani
//   batidan doguya supurur; bant bir nesnenin ustunden gecerken eskisi kuculup kaybolur, yenisi buyuyerek cikar.
// - Meteors (Bolge 10): eski kraterler kuculup soner; her yeni kraterin ustune gokten akkor bir goktasi duser (arkasinda isikli iz),
//   degdigi an kivilcim sicrar, koyu toz kalkar ve krater acilir.
// Yalniz gorunus: dunya kurallarina dokunmaz. Finish (gecis bitince ya da yarida kesilince) eski nesneleri ve topaklari siler,
// yeni nesneleri kendi boylarina dondurur: sahne BuildLayout'un kurdugu haliyle birebir ayni kalir.
public sealed class LayoutGust
{
    public const float SnowSeconds = 1.3f;
    const float FlakesPerSecond = 650f, DriftPerSecond = 150f;
    const float BandWidth = 0.8f;   // kar bandinin bir nesneyi ortmesi icin gecmesi gereken yol
    const float FallSeconds = 0.42f, MeteorGap = 0.12f, ShrinkSeconds = 0.25f, OpenSeconds = 0.3f;
    const float TrailPerSecond = 700f;
    const int ImpactSparks = 34, ImpactDust = 12;
    static readonly Color MeteorDust = new Color(0.23f, 0.2f, 0.25f, 0.55f);   // gece ovasinin koyu tozu
    static readonly Color SnowLow = Mats.Hex("#DCE6EF");
    static readonly Color SnowHigh = Mats.Hex("#F7FAFD");
    static readonly Vector3 FallFrom = new Vector3(-0.45f, 1f, 0.3f).normalized * 7f;   // goktasi gokyuzunun bir yaninden egik gelir

    readonly Transform fx;
    readonly Transform[] oldRoots;
    readonly List<Transform> leaving, arriving;
    readonly List<Vector3> leavingScale, arrivingScale;
    readonly List<ParticleSystem> particles = new List<ParticleSystem>();   // yarida kesilirse de durdurulur

    /// <summary>changed: icerigi degisen karelerin yeri (LayoutDiff). Yalniz onlardaki nesneler gecis oynar; yerinde kalan
    /// nesnenin eskisi hemen gizlenir, yenisi hemen tam boyunda gorunur (kipirdamaz).</summary>
    public LayoutGust(Transform parent, IEnumerable<Transform> oldRoots, IEnumerable<Transform> newRoots, IEnumerable<Vector3> changed)
    {
        fx = Parts.Empty("Duzen-Gecisi", parent);
        this.oldRoots = oldRoots.Where(r => r != null).ToArray();
        var changedCells = changed.ToList();
        bool Changing(Transform t) => changedCells.Any(c => Mathf.Abs(c.x - t.localPosition.x) < 0.3f && Mathf.Abs(c.z - t.localPosition.z) < 0.3f);
        var oldObjects = this.oldRoots.SelectMany(Children).ToList();
        var newObjects = newRoots.Where(r => r != null).SelectMany(Children).ToList();
        foreach (var t in oldObjects.Where(t => !Changing(t))) t.gameObject.SetActive(false);   // yenisi ayni yerde, ayni sey
        leaving = oldObjects.Where(Changing).ToList();
        arriving = newObjects.Where(Changing).ToList();
        leavingScale = leaving.Select(t => t.localScale).ToList();
        arrivingScale = arriving.Select(t => t.localScale).ToList();
        for (int i = 0; i < arriving.Count; i++) arriving[i].localScale = Vector3.zero;
    }

    static IEnumerable<Transform> Children(Transform root)
    {
        for (int i = 0; i < root.childCount; i++) yield return root.GetChild(i);
    }

    /// <summary>Tipi: kar bandi alanin batisindan dogusuna gecer (half: alanin yari genisligi ve yari derinligi)</summary>
    public IEnumerator Snow(Vector2 half)
    {
        var rnd = new System.Random(11);   // her seferinde ayni tipi: goruntu turlari tekrarlanabilir
        // ruzgarla savrulan kar taneleri + yere yakin yumusak kar sisi (beyaz perde); ikisi de bantla birlikte doguya akar
        var flakes = ParticleFx.Create("Tipi-Tane", fx.parent, 900, 0.45f);
        var drift = ParticleFx.Create("Tipi-Sis", fx.parent, 240, 0f);
        particles.Add(flakes);
        particles.Add(drift);
        float from = -half.x - 1.4f, to = half.x + 1.4f;
        float flakeDue = 0f, driftDue = 0f;
        yield return Tween.Run(SnowSeconds, t =>
        {
            float band = Mathf.Lerp(from, to, t);
            float dt = Time.deltaTime;
            for (flakeDue += FlakesPerSecond * dt; flakeDue >= 1f; flakeDue--)
                Emit(flakes, rnd, new Vector3(band + Range(rnd, -0.7f, 0.2f), Range(rnd, 0.02f, 0.75f), Range(rnd, -half.y - 0.3f, half.y + 0.3f)),
                    new Vector3(Range(rnd, 5f, 8.5f), Range(rnd, -0.4f, 0.5f), Range(rnd, -0.5f, 0.5f)),
                    Range(rnd, 0.03f, 0.075f), Range(rnd, 0.3f, 0.6f), Color.Lerp(SnowLow, SnowHigh, (float)rnd.NextDouble()));
            for (driftDue += DriftPerSecond * dt; driftDue >= 1f; driftDue--)
                Emit(drift, rnd, new Vector3(band + Range(rnd, -0.9f, 0.1f), Range(rnd, 0.05f, 0.4f), Range(rnd, -half.y - 0.2f, half.y + 0.2f)),
                    new Vector3(Range(rnd, 5f, 7f), Range(rnd, 0f, 0.3f), Range(rnd, -0.2f, 0.2f)),
                    Range(rnd, 0.45f, 0.85f), Range(rnd, 0.35f, 0.6f), new Color(SnowLow.r, SnowLow.g, SnowLow.b, 0.32f));
            for (int i = 0; i < leaving.Count; i++)
                leaving[i].localScale = leavingScale[i] * (1f - Covered(band, leaving[i].localPosition.x));
            for (int i = 0; i < arriving.Count; i++)
                arriving[i].localScale = arrivingScale[i] * Tween.OutBack(Covered(band - BandWidth * 0.6f, arriving[i].localPosition.x));
        });
    }

    static float Range(System.Random rnd, float min, float max) => min + (max - min) * (float)rnd.NextDouble();

    static void Emit(ParticleSystem ps, System.Random rnd, Vector3 position, Vector3 velocity, float size, float life, Color color) =>
        ParticleFx.Emit(ps, position, velocity, size, life, color, Range(rnd, 0f, 360f));

    // Kar bandi bu x'teki nesnenin ustunden ne kadar gecti (0 = henuz gelmedi, 1 = gecti)
    static float Covered(float band, float x) => Mathf.SmoothStep(0f, 1f, (band - x) / BandWidth + 0.5f);

    /// <summary>Meteor yagmuru: eski kraterler kuculur, yeni her kratere sirayla bir goktasi duser</summary>
    public IEnumerator Meteors(Color ember)
    {
        yield return Tween.Run(ShrinkSeconds, t =>
        {
            for (int i = 0; i < leaving.Count; i++) leaving[i].localScale = leavingScale[i] * (1f - Tween.InCubic(t));
        });

        var rnd = new System.Random(23);   // her seferinde ayni yagmur: goruntu turlari tekrarlanabilir
        var trail = ParticleFx.Create("Meteor-Iz", fx.parent, 1500, 0.3f);
        var sparks = ParticleFx.Create("Meteor-Kivilcim", fx.parent, 500, 0.55f, gravity: 1.2f);
        var dust = ParticleFx.Create("Meteor-Toz", fx.parent, 200, 0f);
        particles.Add(trail);
        particles.Add(sparks);
        particles.Add(dust);
        var hot = Color.Lerp(ember, Color.white, 0.55f);   // izin ve kivilcimin en sicak (acik) tonu

        // dusen tas: akkor bir kaya; arkasindaki isikli iz parcaciklarla birakilir
        var rockMesh = MeshFactory.Rock(0.12f, 1201);
        var rockMat = Mats.Emissive(ember * 0.5f, ember * 1.6f);
        var falling = new Transform[arriving.Count];
        var landed = new bool[arriving.Count];
        var trailDue = new float[arriving.Count];
        var seen = new bool[arriving.Count];   // tas bir onceki karede de havadaydi (izin baslangici bilinir)
        for (int i = 0; i < arriving.Count; i++)
        {
            falling[i] = Parts.Add("Goktasi", fx, rockMesh, rockMat, Vector3.zero, outline: false, castShadow: false);
            falling[i].gameObject.SetActive(false);
        }

        float total = (arriving.Count - 1) * MeteorGap + FallSeconds + OpenSeconds;
        yield return Tween.Run(Mathf.Max(total, 0.01f), t =>
        {
            float now = t * total;
            for (int i = 0; i < arriving.Count; i++)
            {
                float fall = (now - i * MeteorGap) / FallSeconds;
                var ground = arriving[i].localPosition;
                if (!landed[i])
                {
                    falling[i].gameObject.SetActive(fall > 0f && fall < 1f);
                    if (fall > 0f && fall < 1f)
                    {
                        var at = ground + FallFrom * (1f - Tween.InCubic(fall));
                        var from = falling[i].gameObject.activeSelf && seen[i] ? falling[i].localPosition : at;
                        falling[i].localPosition = at;
                        seen[i] = true;
                        // taneler bu karede alinan yolun uzerine yayilir: iz kesik noktalar degil, surekli bir cizgi olur
                        trailDue[i] += TrailPerSecond * Time.deltaTime;
                        int n = (int)trailDue[i];
                        trailDue[i] -= n;
                        for (int k = 0; k < n; k++)
                            Emit(trail, rnd, Vector3.Lerp(from, at, (k + 1f) / n) + Jitter(rnd, 0.03f), Jitter(rnd, 0.1f), Range(rnd, 0.08f, 0.15f),
                                Range(rnd, 0.22f, 0.38f), Color.Lerp(ember, hot, (float)rnd.NextDouble()));
                    }
                    if (fall >= 1f)
                    {
                        landed[i] = true;
                        falling[i].gameObject.SetActive(false);
                        Impact(sparks, dust, rnd, ground, ember, hot);
                    }
                }
                float open = Mathf.Clamp01((now - i * MeteorGap - FallSeconds) / OpenSeconds);
                arriving[i].localScale = arrivingScale[i] * Tween.OutBack(open);
            }
        });
    }

    // Goktasi yere degdi: her yone sicrayan kivilcimlar ve yere yakin koyu bir toz halkasi
    static void Impact(ParticleSystem sparks, ParticleSystem dust, System.Random rnd, Vector3 ground, Color ember, Color hot)
    {
        for (int k = 0; k < ImpactSparks; k++)
        {
            float angle = Range(rnd, 0f, Mathf.PI * 2f), speed = Range(rnd, 1.2f, 2.8f);
            Emit(sparks, rnd, ground + new Vector3(0f, 0.06f, 0f), new Vector3(Mathf.Cos(angle) * speed, Range(rnd, 1f, 2.4f), Mathf.Sin(angle) * speed),
                Range(rnd, 0.04f, 0.08f), Range(rnd, 0.35f, 0.6f), Color.Lerp(ember, hot, (float)rnd.NextDouble()));
        }
        for (int k = 0; k < ImpactDust; k++)
        {
            float angle = Range(rnd, 0f, Mathf.PI * 2f), speed = Range(rnd, 0.3f, 0.8f);
            Emit(dust, rnd, ground + new Vector3(0f, 0.1f, 0f), new Vector3(Mathf.Cos(angle) * speed, Range(rnd, 0.1f, 0.3f), Mathf.Sin(angle) * speed),
                Range(rnd, 0.3f, 0.55f), Range(rnd, 0.6f, 0.9f), MeteorDust);
        }
    }

    static Vector3 Jitter(System.Random rnd, float r) => new Vector3(Range(rnd, -r, r), Range(rnd, -r, r), Range(rnd, -r, r));

    /// <summary>Gecis bitti ya da yarida kesildi: eski nesneler ve topaklar silinir, yeni nesneler kendi boyunda</summary>
    public void Finish()
    {
        for (int i = 0; i < arriving.Count; i++)
            if (arriving[i] != null) arriving[i].localScale = arrivingScale[i];
        foreach (var root in oldRoots)
            if (root != null) Object.Destroy(root.gameObject);
        if (fx != null) Object.Destroy(fx.gameObject);
        foreach (var ps in particles) ParticleFx.Finish(ps);   // kalan taneler solup biter, nesne kendini siler
    }
}
