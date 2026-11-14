using System.Collections.Generic;
using UnityEngine;

// Bolge 8 · 80: arka duvarda buyuk, basili istasyon haritasi (yazisiz, resim). Cerceveli levha, soluk krem zemin. Uzerinde:
// istasyon (kucuk koyu kare), kanyon (kivrik kahve serit, haritanin ustunden istasyona iner), tuz golu (buyuk beyaz oval), golun
// otesinde volkan (koyu ucgen, tepesi kirik). Defne'nin turuncu isaret kalemiyle cizdigi uc ok: istasyondan gole, golun ustunden,
// sonuncusu golun otesini (volkani) gosterir. Oklar Bolge 6'nin boya oklariyla ayni renk (PaintMarks), ama ince kalem cizgisi.
// Hersey levhanin icinde kalir (yuzeyden tasmaz). 80'de read_note() rotayi bu haritadan okur (Gorev 3).
public static class StationMap
{
    static readonly Material Frame = Mats.Lit(Mats.Hex("#3E4048"), 0.4f);
    static readonly Material Paper = Mats.Lit(Mats.Hex("#E6DCC4"), 0.15f);
    static readonly Material Ground = Mats.Lit(Mats.Hex("#D7C2A0"), 0.15f);     // haritadaki arazi (krem ustunde soluk kum)
    static readonly Material Canyon = Mats.Lit(Mats.Hex("#8A5A3E"), 0.1f);   // basili renkler kagitla ayni isikta
    static readonly Material Station = Mats.Lit(Mats.Hex("#2C2E34"), 0.1f);
    static readonly Material Lake = Mats.Lit(Mats.Hex("#F6F4EE"), 0.1f);
    static readonly Material LakeRim = Mats.Lit(Mats.Hex("#A9B4BC"), 0.1f);
    static readonly Material Volcano = Mats.Lit(Mats.Hex("#4A3A36"), 0.1f);

    // wallFace: arka duvarin on yuzu (yerel -Z kameraya bakar, y = 0 zemin). at: levhanin ortasi (x, y); width: eni.
    public static Transform Paint(Transform wallFace, Vector2 at, float width, int seed)
    {
        float height = width * 0.44f;
        var map = Parts.Empty("Harita", wallFace);
        map.localPosition = new Vector3(at.x, at.y, 0f);
        Parts.Add("Cerceve", map, MeshFactory.RoundedBox(new Vector3(width + 0.08f, height + 0.08f, 0.03f), 0.01f, 2), Frame,
            new Vector3(0f, 0f, -0.015f), outline: false, castShadow: false);
        Parts.Add("Kagit", map, MeshFactory.RoundedBox(new Vector3(width, height, 0.01f), 0.004f, 1), Paper,
            new Vector3(0f, 0f, -0.034f), outline: false, castShadow: false);
        var face = Parts.Empty("Yuz", map);
        face.localPosition = new Vector3(0f, 0f, -0.04f);

        // harita birimi: u, v -1..1 -> levhanin ici (kenardan pay)
        Vector2 M(float u, float v) => new Vector2(u * width * 0.46f, v * height * 0.42f);

        Shape(face, "Arazi", Ellipse(M(0f, 0f), new Vector2(width * 0.47f, height * 0.44f), 40), Ground, 0f);
        Shape(face, "Gol-Kiyi", Ellipse(M(0.08f, -0.05f), new Vector2(width * 0.2f, height * 0.25f), 40), LakeRim, -0.002f);
        Shape(face, "Gol", Ellipse(M(0.08f, -0.05f), new Vector2(width * 0.19f, height * 0.225f), 40), Lake, -0.004f);
        // volkan: kesik tepeli ucgen (yamuk) + tepesinde kucuk centik
        Shape(face, "Volkan", new[] { M(0.62f, -0.2f), M(0.9f, -0.2f), M(0.8f, 0.42f), M(0.72f, 0.42f) }, Volcano, -0.002f);
        Shape(face, "Istasyon", Rect(M(-0.72f, -0.5f), 0.07f), Station, -0.004f);
        // kanyon: haritanin ust solundan kivrilarak istasyona iner (kalin kahve cizgi)
        var canyon = new List<Vector2>();
        for (int i = 0; i <= 16; i++)
        {
            float t = i / 16f;
            canyon.Add(M(-0.92f + 0.22f * t + 0.07f * Mathf.Sin(t * 9f), 0.92f - 1.3f * t));
        }
        BrushPaint.Lines(face, new[] { canyon.ToArray() }, -0.003f, Canyon, seed, 0.045f).name = "Kanyon";

        // Defne'nin oklari (turuncu isaret kalemi, ince): istasyon -> golun kiyisi, golun ustunden, golun otesine (volkana)
        var arrows = new List<Vector2[]>();
        AddArrow(arrows, M(-0.62f, -0.42f), M(-0.36f, -0.12f));
        AddArrow(arrows, M(-0.28f, -0.02f), M(0.36f, 0.04f));
        AddArrow(arrows, M(0.44f, 0.08f), M(0.64f, 0.2f));
        BrushPaint.Lines(face, arrows.ToArray(), -0.006f, PaintMarks.PaintMat(0f), seed + 1, 0.022f).name = "Oklar";
        var end = Parts.Empty(ArrowsEndName, face);   // son okun ucu (3b kapanisinda Kivilcim basini buraya kaldirir)
        end.localPosition = new Vector3(M(0.64f, 0.2f).x, M(0.64f, 0.2f).y, -0.006f);
        return map;
    }

    const string ArrowsEndName = "Ok-Ucu";

    // Haritadaki son okun ucu (tuz golunun otesi), dunya konumu. map: Paint'in dondurdugu levha
    public static Vector3 ArrowsEnd(Transform map) => map.Find("Yuz/" + ArrowsEndName).position;

    // Govde + iki kanatli uc (kalemle cizilmis)
    static void AddArrow(List<Vector2[]> lines, Vector2 from, Vector2 to)
    {
        var dir = (to - from).normalized;
        var side = new Vector2(-dir.y, dir.x);
        float head = Mathf.Min(0.07f, (to - from).magnitude * 0.35f);
        lines.Add(new[] { from, Vector2.Lerp(from, to, 0.5f) + side * 0.008f, to });   // hafif egri govde (elle cizilmis)
        lines.Add(new[] { to - dir * head + side * head * 0.6f, to, to - dir * head - side * head * 0.6f });
    }

    static Vector2[] Ellipse(Vector2 c, Vector2 r, int n)
    {
        var pts = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            float a = i * Mathf.PI * 2f / n;
            pts[i] = c + new Vector2(Mathf.Cos(a) * r.x, Mathf.Sin(a) * r.y);
        }
        return pts;
    }

    static Vector2[] Rect(Vector2 c, float half) =>
        new[] { c + new Vector2(-half, -half), c + new Vector2(half, -half), c + new Vector2(half, half), c + new Vector2(-half, half) };

    // Dis bukey cokgen: yuzeyin XY duzleminde, -Z'ye (kameraya) bakan duz sekil
    static void Shape(Transform face, string name, Vector2[] pts, Material mat, float z)
    {
        var mesh = new FlatMesh();
        var center = Vector2.zero;
        foreach (var p in pts) center += p;
        center /= pts.Length;
        for (int i = 0; i < pts.Length; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % pts.Length];
            mesh.Triangle(new Vector3(center.x, center.y, z), new Vector3(a.x, a.y, z), new Vector3(b.x, b.y, z), Vector3.back, 1f, 1f, 1f);
        }
        Parts.Add(name, face, mesh.ToMesh(name), mat, Vector3.zero, outline: false, castShadow: false);
    }
}
