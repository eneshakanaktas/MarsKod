using System;
using System.Collections.Generic;
using UnityEngine;

// Bolum 60: direklerin hemen otesinden baslayip ufuktaki kanyon agzina inen yarik (spec 3b.2). Uzaklastikca genisler ve
// derinlesir; ucu kapali degil, zeminin kivrilip bittigi ufkun otesine (kanyon agzina) uzanir. Sag duvarina oyulmus patika
// kenardan dibe iner; dipte bir donemec kayasinin arkasina gecilir (robot orada gozden kaybolur). Bicim RiftShape'te, ag
// RiftMeshes'te; burada olculer (zemin kesmesiyle ikiz), patika noktalari ve sahnenin kamera yeri.
// Isik gercek gunesten: gunes solda alcak oldugundan sol duvar, sag duvarin alt kismi ve dip golgede kalir (sis yok).
public class CanyonRift : MonoBehaviour
{
    // ---- Olculer. IKIZ: Ground.shader riftClip ayni formulleri kullanir; biri degisirse oteki de degisir. ----
    public const float Length = 16f;               // kenardan (t = 0) yarigin bittigi yere; uc, her bakista ufkun otesinde kalir
    public const float BendLength = 7f;            // yarik bu uzunlukta patikanin hizasindan kanyon agzinin hizasina (x = 0) kivrilir
    public const float HeadShift = 0.35f;          // yarigin basi patikanin solunda: zemindeki patika dumduz raftan devam eder
    public const float LedgeRecess = 0.3f;         // patika rafinin duvara oyuldugu derinlik
    public const float SetbackFrom = 2.7f, SetbackTo = 3.3f;   // bas tarafta raf ustu acik (duvar raf boyunca geride); burada kapanir

    public static float CenterX(float t, float pathX) => Mathf.Lerp(pathX - HeadShift, 0f, Mathf.SmoothStep(0f, 1f, t / BendLength));
    // Bas (t = 0) yuvarlak bir uc: genislik, kenar duzensizligi ve raf acikligi sifirdan karekokle acilir (zeminde duz kesik kalmaz)
    public const float HeadRound = 0.5f;
    static float Head(float t) => Mathf.Sqrt(Mathf.Clamp01(t / HeadRound));
    public static float HalfWidth(float t) => (0.5f + 0.06f * t) * (0.6f + 0.4f * Mathf.SmoothStep(0f, 1f, t / 1.2f)) * Head(t);
    // Kenarin duzensizligi (her zaman disari) + sag duvarda bas taraftaki raf acikligi. s: -1 sol, +1 sag.
    public static float RimOffset(float t, float s) => RimJitter(t, s) + (s > 0f ? RimSetback(t) : 0f);
    public static float RimJitter(float t, float s) => Head(t) *
        (0.03f * (1f + Mathf.Sin(t * 2.3f + s * 1.7f)) + 0.018f * (1f + Mathf.Sin(t * 5.9f + s * 4.1f)) + 0.008f * (1f + Mathf.Sin(t * 13.1f + s * 2.9f)));
    public static float RimSetback(float t) => Head(t) * LedgeRecess * (1f - Mathf.SmoothStep(0f, 1f, (t - SetbackFrom) / (SetbackTo - SetbackFrom)));
    // ---- (ikiz sonu) ----

    // (x, z) yarigin agzinin margin kadar yakininda mi (kenara konan kayalar yarigin ustunde havada kalmasin)
    public static bool Covers(float x, float z, float startZ, float pathX, float margin)
    {
        float t = z - startZ;
        if (t < -margin || t > Length) return false;
        t = Mathf.Max(t, 0f);
        float cx = CenterX(t, pathX), s = x >= cx ? 1f : -1f;
        return Mathf.Abs(x - cx) < HalfWidth(t) + RimOffset(t, s) + margin;
    }

    // Ortadaki zeminden dibe derinlik: bas tarafi sig, uzaklastikca derinlesir; DropT'de dip kanyona dogru dikce iner (kenar).
    // Yukaridan bakan kamera kenarin otesini goremez: Kivilcim oradan inince gozden kaybolur.
    public const float DropT = 7.4f, DropLength = 1.2f, DropDepth = 5f;
    public static float Depth(float t) =>
        (0.8f + 0.3f * t) * (0.35f + 0.65f * Mathf.SmoothStep(0f, 1f, t)) + DropDepth * Mathf.SmoothStep(0f, 1f, (t - DropT) / DropLength);

    // "Yariga bakis" sahnesinin kamerasi: direklerin ve Kivilcim'in arkasindan, yukaridan, yarik boyunca kanyon agzina bakar
    // (Enes). Kamera yarigin ufka vardigi yere (ViewAimT) doner: orasi ekranin ortasina gelir, gokyuzundeki kanyon agzi da orada
    // (gokyuzu resmi ekrana sabit, kameranin yonuyle donmez).
    public const float ViewFov = 50f;
    const float ViewBack = 2.5f, ViewHeight = 3.0f, ViewPitch = 40f, ViewAimT = 9f;

    readonly List<Vector3> trail = new List<Vector3>();
    public IReadOnlyList<Vector3> Trail => trail;       // yerel; zemindeki patikadan rafa, raftan dibe, donemecin arkasina
    public int BendIndex { get; private set; }          // Trail'de dibin kenarindan inip gozden kaybolunan ilk nokta
    public Vector3 ViewPoint { get; private set; }      // "yariga bakis" sahnesinin kamerasi (dunya)
    public Vector3 ViewTarget { get; private set; }     // kameranin baktigi nokta (dunya)

    RiftShape shape;

    public static CanyonRift Build(Transform parent, float startZ, float pathX, Func<float, float, float> ground)
    {
        var rift = Parts.Empty("Iz-Yarik", parent).gameObject.AddComponent<CanyonRift>();
        rift.shape = new RiftShape(startZ, pathX, ground);
        RiftMeshes.Build(rift.transform, rift.shape);
        rift.BuildTrail(pathX, startZ, ground);
        rift.SetView();
        return rift;
    }

    // Kamera: patikanin basinin ViewBack kadar gerisinde, zeminden ViewHeight yukarida; yarigin ufka vardigi yere doner, ViewPitch
    // kadar egilir
    void SetView()
    {
        float z = shape.Z(0f) - ViewBack, x = shape.PathX, pitch = ViewPitch;
        var eye = new Vector3(x, shape.Ground(x, z) + ViewHeight, z);
        var aim = new Vector3(shape.CenterX(ViewAimT), 0f, shape.Z(ViewAimT)) - eye;
        aim.y = 0f;
        var dir = aim.normalized * Mathf.Cos(pitch * Mathf.Deg2Rad) + Vector3.down * Mathf.Sin(pitch * Mathf.Deg2Rad);
        ViewPoint = transform.TransformPoint(eye);
        ViewTarget = transform.TransformPoint(eye + dir * 6f);
    }

    // Patika: zemindeki patikadan (direklerin arasi) rafa, raf boyunca dibe, dipte donemec kayasini gecip kenardan asagi
    void BuildTrail(float pathX, float startZ, Func<float, float, float> ground)
    {
        float z0 = startZ - 0.3f;
        trail.Add(new Vector3(pathX, ground(pathX, z0), z0));
        for (float t = 0.25f; t < RiftShape.LedgeReach - 1e-3f; t += 0.5f) trail.Add(shape.LedgeCenter(t));
        foreach (float t in RiftShape.FloorTrail)
        {
            if (BendIndex == 0 && t > DropT + 0.5f) BendIndex = trail.Count;   // kenardan indik: robot burada gozden kaybolur
            trail.Add(shape.FloorTrailPoint(t));
        }
    }
}
