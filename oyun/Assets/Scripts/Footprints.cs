using System;
using System.Collections.Generic;
using UnityEngine;

// Kum/camurdaki ayak izleri (Bolge 7): Ece (kucuk), yetiskinler (buyuk). Her iz zemine yatik bir dortgen; doku kodla uretilir
// (ayakkabi tabani: ici koyu, kenari acik = hafif cukur) ve zemine "carpma" ile islenir (MarsKod/Decal): golgede de zeminle uyumlu.
// Bir yolun butun izleri tek agda birlesir (telefonda parca sayisi az). Izleri gruplara bolmek gerekirse (yurudukce belirme)
// once Prints ile izler alinir, sonra BuildMesh ile istenen grubun agi kurulur.
public enum Walker { Big, Small }
public enum Gait { Walk, Run, Hop, Carry }   // Carry: donus izi (yuk tasiyor): kisa adim, daha derin (koyu)

public static class Footprints
{
    // Tek bir ayak izi: yer (zemin duzleminde), ileri yonu, sol ayak mi, boyu, derinligi (0..1, koyuluk), yoldaki uzakligi
    public readonly struct Print
    {
        public readonly Vector3 Pos, Forward;
        public readonly bool Left;
        public readonly float Length, Depth, Distance;
        public Print(Vector3 pos, Vector3 forward, bool left, float length, float depth, float distance)
        {
            Pos = pos; Forward = forward; Left = left; Length = length; Depth = depth; Distance = distance;
        }

        public Print WithDepth(float depth) => new Print(Pos, Forward, Left, Length, depth, Distance);
    }

    const float Lift = 0.004f;          // zeminin hemen ustu (zemin ile kavga etmesin)
    const float WidthRatio = 0.5f;      // izin eni / boyu
    static Texture2D sole;

    // path boyunca izler. side: yolun ortasindan saga kayma (el ele yuruyen ikinci kisi icin); seed: kucuk rastgele donusler.
    public static List<Print> Prints(IReadOnlyList<Vector3> path, Walker who, Gait gait, float side, int seed)
    {
        var rng = new System.Random(seed);
        float length = LengthOf(who);
        float feetApart = ApartOf(who);
        float step = StepOf(who, gait);
        float depth = gait == Gait.Carry ? 1f : 0.7f;
        var prints = new List<Print>();
        float total = 0f;
        for (int i = 1; i < path.Count; i++) total += Flat(path[i] - path[i - 1]).magnitude;

        bool left = rng.Next(2) == 0;
        for (float s = step * 0.5f; s < total; s += step)
        {
            Along(path, s, out var at, out var forward);
            var right = new Vector3(forward.z, 0f, -forward.x);
            var center = at + right * side;
            if (gait == Gait.Hop)
            {
                prints.Add(Make(center - right * feetApart, forward, true, length, depth, s, rng));
                prints.Add(Make(center + right * feetApart, forward, false, length, depth, s, rng));
                continue;
            }
            prints.Add(Make(center + right * (left ? -feetApart : feetApart), forward, left, length, depth, s, rng));
            left = !left;
        }
        return prints;
    }

    // Ziplama yeri: iki ayak yan yana (bir tasin onunde ya da arkasinda)
    public static IEnumerable<Print> HopPair(Vector3 center, Vector3 forward, Walker who, int seed)
    {
        var rng = new System.Random(seed);
        var right = new Vector3(forward.z, 0f, -forward.x);
        float length = LengthOf(who), apart = ApartOf(who);
        yield return Make(center - right * apart, forward, true, length, 0.8f, 0f, rng);
        yield return Make(center + right * apart, forward, false, length, 0.8f, 0f, rng);
    }

    static float LengthOf(Walker who) => who == Walker.Big ? 0.2f : 0.13f;
    static float ApartOf(Walker who) => who == Walker.Big ? 0.07f : 0.05f;

    // Iki iz arasi (kisi basina degil, iz basina); Hop: iki ayak yan yana, siradaki ciftle arasi
    static float StepOf(Walker who, Gait gait)
    {
        if (who == Walker.Small) return gait == Gait.Hop ? 0.45f : gait == Gait.Run ? 0.34f : 0.22f;
        return gait == Gait.Run ? 0.5f : gait == Gait.Carry ? 0.24f : 0.32f;
    }

    static Print Make(Vector3 pos, Vector3 forward, bool left, float length, float depth, float distance, System.Random rng)
    {
        float turn = ((float)rng.NextDouble() - 0.5f) * 16f + (left ? -4f : 4f);   // ayak ucu hafif disa
        return new Print(pos, Quaternion.Euler(0f, turn, 0f) * forward, left, length, depth * (0.85f + 0.15f * (float)rng.NextDouble()), distance);
    }

    // Yolun s uzakligindaki nokta ve ileri yonu (zemin duzleminde)
    static void Along(IReadOnlyList<Vector3> path, float s, out Vector3 at, out Vector3 forward)
    {
        for (int i = 1; i < path.Count; i++)
        {
            var d = Flat(path[i] - path[i - 1]);
            float len = d.magnitude;
            if (s <= len || i == path.Count - 1)
            {
                forward = len > 1e-4f ? d / len : Vector3.forward;
                at = path[i - 1] + forward * Mathf.Min(s, len);
                return;
            }
            s -= len;
        }
        at = path[0];
        forward = Vector3.forward;
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // Izlerin tek agi: her iz zemin yuksekligine oturan bir dortgen; kose renginin alfasi izin derinligi
    public static Mesh BuildMesh(IEnumerable<Print> prints, Func<float, float, float> ground)
    {
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var colors = new List<Color>();
        var triangles = new List<int>();
        foreach (var p in prints)
        {
            var f = p.Forward * (p.Length * 0.5f);
            var r = new Vector3(p.Forward.z, 0f, -p.Forward.x) * (p.Length * WidthRatio * 0.5f);
            int b = vertices.Count;
            Vector3[] corners = { p.Pos - f - r, p.Pos - f + r, p.Pos + f + r, p.Pos + f - r };
            foreach (var c in corners) vertices.Add(new Vector3(c.x, ground(c.x, c.z) + Lift, c.z));
            float u0 = p.Left ? 1f : 0f, u1 = 1f - u0;   // sol ayak: doku aynalanir
            uvs.Add(new Vector2(u0, 0f)); uvs.Add(new Vector2(u1, 0f)); uvs.Add(new Vector2(u1, 1f)); uvs.Add(new Vector2(u0, 1f));
            for (int k = 0; k < 4; k++) colors.Add(new Color(1f, 1f, 1f, p.Depth));
            triangles.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
        }
        var mesh = new Mesh { name = "Izler" };
        mesh.indexFormat = vertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    // Izlerin malzemesi (her agin kendi kopyasi: belirme/sonme ayri ayri)
    public static Material Material(float visible = 1f)
    {
        var m = Mats.Custom("Decal", "MarsKod/Decal");
        m.SetTexture("_MainTex", Sole);
        m.SetFloat("_Mode", 0f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.DstColor);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.SrcColor);
        m.SetFloat("_A", visible);
        return m;
    }

    // Bir yolun butun izleri tek parca
    public static Transform Track(Transform parent, IReadOnlyList<Vector3> path, Walker who, Gait gait, float side, int seed,
        Func<float, float, float> ground, string name = "Izler") =>
        Parts.Add(name, parent, BuildMesh(Prints(path, who, gait, side, seed), ground), Material(), Vector3.zero, outline: false, castShadow: false);

    // Ayakkabi tabani dokusu (32x64, sag ayak; v=1 burun). 0.5 gri = zemin aynen; ic koyu (cukur), kenar acik (itilen kum),
    // tabanda ince enine cizgiler (tirtik).
    static Texture2D Sole
    {
        get
        {
            if (sole != null) return sole;
            const int w = 32, h = 64;
            sole = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Taban" };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                    float m = Mathf.Max(Ellipse(u, v, 0.5f, 0.22f, 0.27f, 0.18f),
                              Mathf.Max(Ellipse(u, v, 0.53f, 0.6f, 0.36f, 0.27f), Ellipse(u, v, 0.47f, 0.82f, 0.3f, 0.13f)));
                    float inner = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(m / 0.35f));
                    float rim = Mathf.Clamp01((m + 0.3f) / 0.3f) * (1f - Mathf.Clamp01(m / 0.2f));
                    float tread = inner * (Mathf.Repeat(v * 11f, 1f) < 0.45f ? 0.03f : 0f);
                    float g = 0.5f - 0.3f * inner + 0.12f * rim + tread;
                    px[y * w + x] = new Color(g, g, g, 1f);
                }
            sole.SetPixels(px);
            sole.Apply();
            return sole;
        }
    }

    // Elipsin icindeyse > 0 (merkezde 1), disinda < 0
    static float Ellipse(float u, float v, float cu, float cv, float ru, float rv)
    {
        float du = (u - cu) / ru, dv = (v - cv) / rv;
        return 1f - (du * du + dv * dv);
    }
}
