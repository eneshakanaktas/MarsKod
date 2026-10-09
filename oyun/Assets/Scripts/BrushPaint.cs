using System.Collections.Generic;
using UnityEngine;

// Elle boyanmis firca darbeleri ve yazi (Bolge 6). Bilgisayar yazisi gibi kusursuz durmasin diye her darbe hafif titrek,
// kalinligi degisken, uclari yuvarlak. Yuzeyin yerel XY duzleminde cizilir, -Z'ye (kameraya) bakar; golge vermez.
// Bir cagrinin butun darbeleri tek sekilde birlesir (telefonda parca sayisi az kalsin).
public static class BrushPaint
{
    const float Jitter = 0.025f, ThicknessJitter = 0.15f, LetterAdvance = 0.68f, LetterWidth = 0.6f;
    const int CapSegments = 8;

    // lines: 0..1 kutuda (x saga, y yukari) cizgi dizileri; kutunun ortasi localPos'a gelir. size: kutunun dunyadaki boyu;
    // angleDeg: kutunun donusu (0: x saga); seed: titremeyi sabitler; thickness: kutu birimiyle darbe kalinligi.
    public static Transform Strokes(Transform surface, Vector2[][] lines, Vector3 localPos, float size, float angleDeg,
        Material paint, int seed, float thickness = 0.09f)
    {
        var centered = new List<Vector2[]>();
        foreach (var line in lines) centered.Add(Shift(line, new Vector2(-0.5f, -0.5f)));
        return Paint(surface, "Boya", centered, localPos, size, angleDeg, paint, seed, thickness, Jitter);
    }

    // lines: dogrudan yuzeyin biriminde (olceksiz, donmesiz); z: yuzeyden uzaklik. Titreme kalinlikla orantili.
    public static Transform Lines(Transform surface, Vector2[][] lines, float z, Material paint, int seed, float thickness) =>
        Paint(surface, "Boya", new List<Vector2[]>(lines), new Vector3(0f, 0f, z), 1f, 0f, paint, seed, thickness, thickness * 0.25f);

    // Yalnizca gereken harfler cizilir: 1 6 7 8 - A B Ç E I K M P R S (bilinmeyen harf atlanir, bosluk ilerler).
    // localPos: yazinin ortasi; height: harf boyu.
    public static Transform Write(Transform surface, string text, Vector3 localPos, float height, Material paint, int seed,
        float angleDeg = 0f, float thickness = 0.1f)
    {
        float total = (text.Length - 1) * LetterAdvance + LetterWidth;
        var lines = new List<Vector2[]>();
        for (int i = 0; i < text.Length; i++)
        {
            if (!Glyphs.TryGetValue(char.ToUpperInvariant(text[i]), out var glyph)) continue;
            var offset = new Vector2(-total * 0.5f + i * LetterAdvance, -0.5f);
            foreach (var line in glyph) lines.Add(Shift(line, offset));
        }
        return Paint(surface, "Yazi", lines, localPos, height, angleDeg, paint, seed, thickness, Jitter);
    }

    static Vector2[] Shift(Vector2[] line, Vector2 offset)
    {
        var p = new Vector2[line.Length];
        for (int i = 0; i < line.Length; i++) p[i] = line[i] + offset;
        return p;
    }

    // Darbeleri tek sekle cevirir: her parca icin ince dortgen, her noktada yuvarlak uc (disk). Kok olceklenir (size).
    static Transform Paint(Transform surface, string name, List<Vector2[]> lines, Vector3 localPos, float size, float angleDeg,
        Material paint, int seed, float thickness, float jitter)
    {
        var rng = new System.Random(seed);
        var verts = new List<Vector3>();
        var tris = new List<int>();
        foreach (var line in lines)
        {
            var pts = new Vector2[line.Length];
            for (int i = 0; i < line.Length; i++) pts[i] = line[i] + new Vector2(Rand(rng) * jitter, Rand(rng) * jitter);
            float half = thickness * 0.5f * (1f + Rand(rng) * ThicknessJitter);
            for (int i = 0; i < pts.Length; i++)
            {
                AddCap(verts, tris, pts[i], half);
                if (i > 0) AddSegment(verts, tris, pts[i - 1], pts[i], half);
            }
        }

        var mesh = new Mesh { name = name };
        mesh.SetVertices(verts);
        var normals = new Vector3[verts.Count];
        for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.back;
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();

        var root = Parts.Add(name, surface, mesh, paint, localPos, outline: false, castShadow: false);
        root.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
        root.localScale = Vector3.one * size;
        return root;
    }

    static float Rand(System.Random rng) => (float)rng.NextDouble() * 2f - 1f;

    // -Z'den bakan kameraya gore saat yonunde siralanir (Unity'de on yuz)
    static void AddSegment(List<Vector3> verts, List<int> tris, Vector2 a, Vector2 b, float half)
    {
        var d = b - a;
        if (d.sqrMagnitude < 1e-8f) return;
        var n = new Vector2(-d.y, d.x).normalized * half;
        int s = verts.Count;
        verts.Add(a + n); verts.Add(b + n); verts.Add(b - n); verts.Add(a - n);
        tris.AddRange(new[] { s, s + 1, s + 2, s, s + 2, s + 3 });
    }

    static void AddCap(List<Vector3> verts, List<int> tris, Vector2 c, float half)
    {
        int s = verts.Count;
        verts.Add(c);
        for (int k = 0; k < CapSegments; k++)
        {
            float a = -k * Mathf.PI * 2f / CapSegments;
            verts.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * half);
        }
        for (int k = 0; k < CapSegments; k++)
            tris.AddRange(new[] { s, s + 1 + k, s + 1 + (k + 1) % CapSegments });
    }

    // Harfler: 0..1 kutu (genislik ~0.6), cocuk/acele el yazisina yakin sade cizgiler
    static readonly Dictionary<char, Vector2[][]> Glyphs = new Dictionary<char, Vector2[][]>
    {
        ['1'] = L(P(0.30f, 0.80f, 0.45f, 1f, 0.45f, 0f)),
        ['6'] = L(P(0.50f, 0.95f, 0.25f, 0.70f, 0.10f, 0.35f, 0.15f, 0.10f, 0.35f, 0f, 0.50f, 0.10f, 0.52f, 0.30f, 0.38f, 0.45f, 0.18f, 0.40f, 0.10f, 0.30f)),
        ['7'] = L(P(0.05f, 1f, 0.55f, 1f, 0.25f, 0f)),
        ['8'] = L(P(0.30f, 0.55f, 0.12f, 0.70f, 0.15f, 0.92f, 0.30f, 1f, 0.45f, 0.92f, 0.48f, 0.70f, 0.30f, 0.55f, 0.10f, 0.35f, 0.10f, 0.12f, 0.30f, 0f, 0.50f, 0.12f, 0.50f, 0.35f, 0.30f, 0.55f)),
        ['-'] = L(P(0.10f, 0.50f, 0.45f, 0.50f)),
        ['A'] = L(P(0.05f, 0f, 0.30f, 1f, 0.55f, 0f), P(0.15f, 0.38f, 0.45f, 0.38f)),
        ['B'] = L(P(0.08f, 0f, 0.08f, 1f, 0.35f, 1f, 0.48f, 0.88f, 0.45f, 0.66f, 0.30f, 0.55f, 0.08f, 0.55f), P(0.30f, 0.55f, 0.50f, 0.42f, 0.52f, 0.18f, 0.38f, 0f, 0.08f, 0f)),
        ['Ç'] = L(P(0.52f, 0.85f, 0.38f, 1f, 0.18f, 0.95f, 0.06f, 0.70f, 0.06f, 0.30f, 0.18f, 0.05f, 0.38f, 0f, 0.52f, 0.15f), P(0.30f, 0f, 0.33f, -0.12f, 0.24f, -0.20f)),
        ['E'] = L(P(0.50f, 1f, 0.08f, 1f, 0.08f, 0f, 0.50f, 0f), P(0.08f, 0.52f, 0.40f, 0.52f)),
        ['I'] = L(P(0.25f, 0f, 0.25f, 1f)),
        ['K'] = L(P(0.08f, 0f, 0.08f, 1f), P(0.50f, 1f, 0.10f, 0.45f), P(0.22f, 0.58f, 0.52f, 0f)),
        ['M'] = L(P(0.05f, 0f, 0.08f, 1f, 0.30f, 0.45f, 0.52f, 1f, 0.55f, 0f)),
        ['P'] = L(P(0.08f, 0f, 0.08f, 1f, 0.35f, 1f, 0.50f, 0.88f, 0.50f, 0.68f, 0.35f, 0.55f, 0.08f, 0.55f)),
        ['R'] = L(P(0.08f, 0f, 0.08f, 1f, 0.35f, 1f, 0.50f, 0.88f, 0.50f, 0.68f, 0.35f, 0.55f, 0.08f, 0.55f), P(0.28f, 0.55f, 0.52f, 0f)),
        ['S'] = L(P(0.50f, 0.90f, 0.35f, 1f, 0.15f, 0.95f, 0.08f, 0.78f, 0.20f, 0.58f, 0.42f, 0.45f, 0.52f, 0.25f, 0.42f, 0.05f, 0.20f, 0f, 0.05f, 0.12f)),
    };

    static Vector2[][] L(params Vector2[][] lines) => lines;
    static Vector2[] P(params float[] xy)
    {
        var p = new Vector2[xy.Length / 2];
        for (int i = 0; i < p.Length; i++) p[i] = new Vector2(xy[2 * i], xy[2 * i + 1]);
        return p;
    }
}
