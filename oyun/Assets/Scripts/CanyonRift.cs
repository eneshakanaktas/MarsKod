using System;
using System.Collections.Generic;
using UnityEngine;

// Bolum 60: direklerin hemen otesinden baslayip ufuktaki kanyon agzina giden dar yarik (spec 3b.2). Zemin bu seridin
// ustunde cizilmez (Ground.shader riftClip, ayni formuller). Duvarlar yatay katman katman, asagi indikce ortaya basamaklanir;
// sag duvara oyulmus patika kenardan dibe iner, dipte doneme kayasinin arkasina gecer (robot orada gozden kaybolur).
// Isik gercek gunesten: gunes solda alcak oldugundan sol duvar ve diplerin cogu golgede kalir (sis yok). Malzeme FarRock:
// zeminle ayni kuralla arka plana karisir; boylece yarik zeminin gokyuzune donustugu yerde, gokyuzundeki koyu cizgiye
// dikissiz baglanir (yoksa ufkun ustunde havada asili kalirdi).
public class CanyonRift : MonoBehaviour
{
    public const float ViewFov = 50f;               // "asagi bakis" sahnesinin gorus acisi (derece)
    public const float Length = 9f;                 // kenardan (t = 0) yarigin kapandigi uca
    public static float CenterX(float t, float pathX) => Mathf.Lerp(pathX, 0f, Mathf.SmoothStep(0f, 1f, t / 7f));
    public static float HalfWidth(float t) => (0.5f + 0.06f * t) * (1f - Mathf.SmoothStep(0f, 1f, (t - (Length - 2f)) / 2f));
    public static float Depth(float t) => Mathf.Min(1.4f + 0.45f * t, 4.5f);

    // Patika: kenardan t = 0.3'te baslar, t = 5.5'te dibe varir, dipte t = 7'ye gider
    const float PathStart = 0.3f, PathReach = 5.5f, PathEnd = 7f, PathWidth = 0.35f, PathThick = 0.1f;
    const float BendRockT = 6.4f, Step = 0.25f;
    // Duvar profili: (derinlik orani, iceri kayma) -- dik duvar, dar raf, dik duvar, dar raf, dik duvar
    static readonly Vector2[] Profile =
    {
        new Vector2(0f, 0f), new Vector2(0.3f, 0.04f), new Vector2(0.33f, 0.12f), new Vector2(0.62f, 0.15f),
        new Vector2(0.65f, 0.22f), new Vector2(1f, 0.24f),
    };

    readonly List<Vector3> trail = new List<Vector3>();
    public IReadOnlyList<Vector3> Trail => trail;       // yerel, kenardan dibe
    public int BendIndex { get; private set; }          // Trail'de donemec kayasinin arkasina gecilen ilk nokta
    public Vector3 ViewPoint { get; private set; }      // "asagi bakis" sahnesinin kamerasi
    public Vector3 ViewTarget { get; private set; }

    float startZ, pathX;
    Func<float, float, float> ground;

    public static CanyonRift Build(Transform parent, float startZ, float pathX, Func<float, float, float> ground)
    {
        var rift = Parts.Empty("Iz-Yarik", parent).gameObject.AddComponent<CanyonRift>();
        rift.startZ = startZ;
        rift.pathX = pathX;
        rift.ground = ground;
        rift.Make();
        return rift;
    }

    void Make()
    {
        var wallA = new MeshBuilder();
        var wallB = new MeshBuilder();
        var floor = new MeshBuilder();
        var path = new MeshBuilder();
        int steps = Mathf.RoundToInt(Length / Step);
        var bottom = Profile[Profile.Length - 1];
        for (int i = 0; i < steps; i++)
        {
            float t0 = i * Step, t1 = (i + 1) * Step;
            foreach (float s in new[] { -1f, 1f })
                for (int k = 0; k < Profile.Length - 1; k++)
                    (k % 2 == 0 ? wallA : wallB).Quad(WallPoint(t0, s, Profile[k]), WallPoint(t0, s, Profile[k + 1]),
                        WallPoint(t1, s, Profile[k + 1]), WallPoint(t1, s, Profile[k]), new Vector3(-s * 0.5f, 0.5f, 0f));
            floor.Quad(WallPoint(t0, -1f, bottom), WallPoint(t0, 1f, bottom), WallPoint(t1, 1f, bottom), WallPoint(t1, -1f, bottom), Vector3.up);
        }
        BuildPath(path);

        AddPart("Duvar-A", wallA, "#7A4530");
        AddPart("Duvar-B", wallB, "#5E3424");
        AddPart("Dip", floor, "#3A2018");
        AddPart("Patika", path, "#BD8561");
        var rock = Parts.Add("DonemecKayasi", transform, MeshFactory.Rock(0.45f, 700), RiftMat("#5E3424"), BendRockPosition(), outline: false);
        rock.localScale = new Vector3(1.3f, 1f, 1.1f);

        float viewZ = startZ - 1.2f;   // direklerin biraz gerisinden, yukaridan: sol duvar, sag duvar ve raf birlikte gorunur
        ViewPoint = new Vector3(pathX, ground(pathX, viewZ) + 3.0f, viewZ);
        ViewTarget = trail[Mathf.Min(trail.Count - 1, 11)];
    }

    // Yarigin t'deki duvar noktasi: s (-1 sol, +1 sag), p.x = derinlik orani, p.y = iceri kayma
    Vector3 WallPoint(float t, float s, Vector2 p)
    {
        float z = startZ + t, hw = HalfWidth(t), cx = CenterX(t, pathX);
        float edgeY = ground(cx + s * hw, z);
        float bottomY = ground(cx, z) - Depth(t);
        float inward = Mathf.Min(p.y, hw * 0.9f);
        return new Vector3(cx + s * (hw - inward), Mathf.Lerp(edgeY, bottomY, p.x), z);
    }

    // Sag duvarin derinlik oranindaki iceri kaymasi (profilden)
    static float WallInward(float depthFraction)
    {
        for (int k = 0; k < Profile.Length - 1; k++)
            if (depthFraction <= Profile[k + 1].x)
                return Mathf.Lerp(Profile[k].y, Profile[k + 1].y, Mathf.InverseLerp(Profile[k].x, Profile[k + 1].x, depthFraction));
        return Profile[Profile.Length - 1].y;
    }

    // Patika rafinin t'deki duvar tarafi noktasi: kenardan dibe inis orani t'ye gore yumusak artar
    Vector3 ShelfPoint(float t)
    {
        float z = startZ + t, hw = HalfWidth(t), cx = CenterX(t, pathX);
        float edgeY = ground(cx + hw, z), bottomY = ground(cx, z) - Depth(t);
        float fraction = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(PathStart, PathReach, t));
        float inward = Mathf.Min(WallInward(fraction), hw * 0.9f);
        return new Vector3(cx + hw - inward, Mathf.Lerp(edgeY, bottomY, fraction), z);
    }

    void BuildPath(MeshBuilder mesh)
    {
        trail.Add(new Vector3(pathX, ground(pathX, startZ - 0.3f), startZ - 0.3f));   // zemindeki patikadan rafa giris
        const float Dt = 0.25f;
        for (float t = PathStart; t < PathReach - 1e-3f; t += Dt)
        {
            var a = ShelfPoint(t);
            var b = ShelfPoint(Mathf.Min(t + Dt, PathReach));
            var ai = a + Vector3.left * PathWidth;
            var bi = b + Vector3.left * PathWidth;
            mesh.Quad(ai, a, b, bi, Vector3.up);                                                  // ust yuz
            mesh.Quad(ai, bi, bi + Vector3.down * PathThick, ai + Vector3.down * PathThick, Vector3.left);   // on yuz
        }
        for (float t = PathStart; t <= PathReach + 1e-3f; t += 0.5f)
            trail.Add(ShelfPoint(t) + Vector3.left * (PathWidth * 0.5f));
        BendIndex = -1;
        for (float t = PathReach + 0.5f; t <= PathEnd + 1e-3f; t += 0.5f)
        {
            trail.Add(FloorPoint(t));
            if (BendIndex < 0 && t > BendRockT) BendIndex = trail.Count - 1;   // kayayi gectik: robot burada gozden kaybolur
        }
        if (BendIndex < 0) BendIndex = trail.Count - 1;
    }

    // Yarik dibinde t'deki nokta (merkezin biraz sagi: patikanin devami)
    Vector3 FloorPoint(float t)
    {
        float z = startZ + t, cx = CenterX(t, pathX);
        return new Vector3(cx + HalfWidth(t) * 0.35f, ground(cx, z) - Depth(t) + 0.01f, z);
    }

    Vector3 BendRockPosition()
    {
        var p = FloorPoint(BendRockT);
        p.y += 0.11f;
        return p;
    }

    // FarRock malzemesi, ama yarik ufka yaklasana kadar gorunur kalir (zemin gibi erken solmaz): son 0,05'te kademeli gokyuzune karisir (boyaniyla ayni cizgiye dikissiz baglanir)
    static Material RiftMat(string hex)
    {
        var m = Oyun.FarRockMat(hex);
        m.SetVector("_FadeBand", new Vector4(0.05f, 0f, 0f, 0f));
        m.SetVector("_FogRange", new Vector4(60f, 80f, 0f, 0f));   // uzaklik karismasi yok: yalnizca ufukta
        return m;
    }

    // Duvarlar tek yuzlu ag: gunes soldayken sol duvarin yuzu isiga arkasini doner; iki yuzlu golge, yarigin icini gercekten karartir
    void AddPart(string name, MeshBuilder mesh, string hex)
    {
        var part = Parts.Add(name, transform, mesh.ToMesh(name), RiftMat(hex), Vector3.zero, outline: false);
        part.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    }

    // Duz yuzeyli (her ucgenin kendi normali) basit ag kurucu
    class MeshBuilder
    {
        readonly List<Vector3> v = new List<Vector3>();
        readonly List<Vector3> n = new List<Vector3>();
        readonly List<int> tri = new List<int>();

        // a-b-c-d dortgeni; outward: yuzun bakmasi gereken yon (sarmal buna gore duzeltilir)
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 outward)
        {
            Triangle(a, b, c, outward);
            Triangle(a, c, d, outward);
        }

        void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 outward)
        {
            var normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-10f) return;
            if (Vector3.Dot(normal, outward) < 0f) { var swap = b; b = c; c = swap; normal = -normal; }
            normal.Normalize();
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            n.Add(normal); n.Add(normal); n.Add(normal);
            tri.Add(i); tri.Add(i + 1); tri.Add(i + 2);
        }

        public Mesh ToMesh(string name)
        {
            var m = new Mesh { name = name };
            m.SetVertices(v);
            m.SetNormals(n);
            m.SetTriangles(tri, 0);
            m.RecalculateBounds();
            return m;
        }
    }
}
