using System;
using System.Collections.Generic;
using UnityEngine;

// Kumdaki ayak izleri: bir yol boyunca sag-sol sirayla basilmis izler. Her iz kuma basilmis bir cukur: koyu taban (golgede kalan ic)
// ve dogu (sag) kenarinda acik bir serit (alcak bati gunesini alan ic duvar). Butun izler iki ag halinde kurulur: yuzlerce iz
// telefonu yormasin. Malzeme zemini carpar (GroundMark): iz alttaki zeminin rengini alir, ufka yaklasinca zeminle birlikte soner.
// Yalnizca gorunus; hangi izin nerede oldugunu cagiran (orn. RiverbedTraces) secer.
public class Footprints
{
    public struct Walker
    {
        public float Offset;          // yolun ortasindan yana uzaklik (+ yurume yonune gore sag)
        public float Step;            // art arda iki iz arasi (sol-sag)
        public float Length, Width;   // izin boyu ve eni
        public float Start, End;      // yol boyunca nereden nereye (0..1)
        public float Phase;           // ilk izin kaymasi: yan yana yuruyenlerin adimlari ayni hizaya dusmesin
    }

    const int Segments = 10;
    // Lift: zemin agi seyrek (0,4 birim); iz agin altinda kalmasin. Sonra yuruyenin izi oncekinin ustunde (ust uste binen izler titremesin).
    const float Lift = 0.012f, LayerLift = 0.0003f;
    const float SunSide = 0.16f;                     // acik seridin dogu yonune kaymasi (izin enine gore)
    // Zemini carpanlar (dogrusal): taban ekranda kumun ~0,6'si, acik serit ~1,2'si. Taban acik seridin ustune biner ve carpanlar
    // birbirini de carpar: tabanin kendi carpani buna gore kucuk.
    const float LitMark = 1.5f, HollowMark = 0.32f / LitMark;

    readonly Func<float, float, float> ground;
    readonly List<Vector3> hollowVerts = new List<Vector3>(), litVerts = new List<Vector3>();
    readonly List<int> hollowTris = new List<int>(), litTris = new List<int>();
    int layer;

    public Footprints(Func<float, float, float> ground) { this.ground = ground; }

    // Yol: zemindeki noktalar (x, z), yurume yonunde sirali
    public void Walk(IReadOnlyList<Vector2> path, Walker w)
    {
        float total = PathLength(path);
        float footGap = w.Width * 1.2f;   // sag ve sol ayagin arasi
        float side = 1f;
        for (float d = total * w.Start + w.Phase; d < total * w.End; d += w.Step, side = -side)
        {
            PointAt(path, d, out var p, out var dir);
            var right = new Vector2(dir.y, -dir.x);
            Print(p + right * (w.Offset + side * footGap * 0.5f), dir, w.Length, w.Width);
        }
        layer++;
    }

    // markMaterial: zemini verilen carpanla carpan malzeme (Oyun.GroundMarkMat)
    public void Build(Transform parent, Func<float, Material> markMaterial)
    {
        if (hollowVerts.Count == 0) return;
        Parts.Add("Iz-AyakIzleri-Isik", parent, ToMesh(litVerts, litTris), markMaterial(LitMark), Vector3.zero, outline: false, castShadow: false);
        Parts.Add("Iz-AyakIzleri", parent, ToMesh(hollowVerts, hollowTris), markMaterial(HollowMark), Vector3.zero, outline: false, castShadow: false);
    }

    // Tek iz: one dogru genis (parmak tarafi), topukta dar oval. Acik serit ayni bicimde, biraz buyuk ve doguya kaymis; koyu taban ustunde.
    void Print(Vector2 c, Vector2 dir, float length, float width)
    {
        float y = ground(c.x, c.y) + Lift + layer * LayerLift;
        AddSole(litVerts, litTris, c + new Vector2(width * SunSide, 0f), dir, length * 1.06f, width * 1.15f, y);
        AddSole(hollowVerts, hollowTris, c, dir, length, width, y + LayerLift * 0.5f);
    }

    static void AddSole(List<Vector3> verts, List<int> tris, Vector2 c, Vector2 dir, float length, float width, float y)
    {
        var right = new Vector2(dir.y, -dir.x);
        int centre = verts.Count;
        verts.Add(new Vector3(c.x, y, c.y));
        for (int i = 0; i < Segments; i++)
        {
            float a = i * Mathf.PI * 2f / Segments;
            float along = Mathf.Cos(a), across = Mathf.Sin(a);
            float halfWidth = width * 0.5f * (0.78f + 0.27f * (along + 1f) * 0.5f);
            var p = c + dir * (along * length * 0.5f) + right * (across * halfWidth);
            verts.Add(new Vector3(p.x, y, p.y));
            tris.Add(centre); tris.Add(centre + 1 + i); tris.Add(centre + 1 + (i + 1) % Segments);
        }
    }

    static Mesh ToMesh(List<Vector3> verts, List<int> tris)
    {
        var m = new Mesh { name = "AyakIzleri" };
        if (verts.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.SetVertices(verts);
        var up = new List<Vector3>(verts.Count);
        for (int i = 0; i < verts.Count; i++) up.Add(Vector3.up);
        m.SetNormals(up);
        m.SetTriangles(tris, 0);
        m.RecalculateBounds();
        return m;
    }

    static float PathLength(IReadOnlyList<Vector2> path)
    {
        float total = 0f;
        for (int i = 1; i < path.Count; i++) total += Vector2.Distance(path[i - 1], path[i]);
        return total;
    }

    // Yol boyunca d uzakliktaki nokta ve yurume yonu
    static void PointAt(IReadOnlyList<Vector2> path, float d, out Vector2 point, out Vector2 dir)
    {
        for (int i = 1; i < path.Count; i++)
        {
            float seg = Vector2.Distance(path[i - 1], path[i]);
            if (d <= seg || i == path.Count - 1)
            {
                dir = (path[i] - path[i - 1]).normalized;
                point = path[i - 1] + dir * Mathf.Min(d, seg);
                return;
            }
            d -= seg;
        }
        point = path[0];
        dir = Vector2.up;
    }

    // Kontrol noktalarindan gecen yumusak yol (Catmull-Rom), sik ornekli
    public static List<Vector2> Curve(params Vector2[] points)
    {
        var result = new List<Vector2>();
        const int PerSegment = 8;
        for (int i = 0; i < points.Length - 1; i++)
        {
            var p0 = points[Mathf.Max(i - 1, 0)];
            var p1 = points[i];
            var p2 = points[i + 1];
            var p3 = points[Mathf.Min(i + 2, points.Length - 1)];
            for (int k = 0; k < PerSegment; k++)
            {
                float t = k / (float)PerSegment;
                result.Add(0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (3f * p1 - p0 - 3f * p2 + p3) * t * t * t));
            }
        }
        result.Add(points[points.Length - 1]);
        return result;
    }
}
