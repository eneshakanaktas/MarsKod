using System.Collections.Generic;
using UnityEngine;

// Koloninin armasi (Bolge 8, 71; docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md, Gorev 4): turuncu kenarli krem daire, icinde
// koloni turuncusu kubbe, kubbenin ustunde kucuk bes koseli turuncu yildiz (Dunya; kremde krem secilmiyordu). Boya soluk; alt kenari (yuzeyin -Y'si) kumla asinmis,
// yer yer eksik. Duz boyali katmanlar; yuzeyin yerel XY'sinde, -Z'ye (disari) bakar. Golge vermez.
public static class ColonyCrest
{
    static readonly Color Cream = Mats.Hex("#ECE3CE");
    static readonly Color FadedOrange = Mats.Hex("#C9774C");      // koloni turuncusu, gunes yemis
    static readonly Color HullUnder = Mats.Hex("#CDC2AE");        // boyanin altindaki govde rengi (StationFacade.Hull): asinan yerler
    static readonly Color SandSpeck = Mats.Hex("#B39E86");
    const float Layer = 0.0015f;
    const int RimPoints = 40;

    // surface: boyanin yuzeyi (71'de kapi modulunun catisi: StationFacade); localPos: dairenin ortasi; size: dairenin capi
    public static Transform Paint(Transform surface, Vector3 localPos, float size, int seed)
    {
        var rng = new System.Random(seed);
        var root = Parts.Empty("Arma", surface);
        root.localPosition = localPos;
        float r = size * 0.5f;
        Material cream = Mats.Lit(Cream, 0.15f), orange = Mats.Lit(FadedOrange, 0.15f), hull = Mats.Lit(HullUnder, 0.15f), sand = Mats.Lit(SandSpeck, 0.1f);

        Fill(root, "Arma-Kenar", Eroded(r, rng), orange, 1);
        Fill(root, "Arma-Zemin", Eroded(r * 0.86f, rng), cream, 2);
        Fill(root, "Arma-Kubbe", DomeShape(new Vector2(0f, -r * 0.36f), r * 0.5f), orange, 3);
        Fill(root, "Arma-Yildiz", Star(new Vector2(0f, r * 0.47f), r * 0.18f, r * 0.075f), orange, 3);

        // asinma: alt yarida kumla kazinmis lekeler (govde rengi) ve birkac kum tanesi
        for (int i = 0; i < 14; i++)
        {
            float x = Range(rng, -0.85f, 0.85f) * r, y = Range(rng, -1.0f, -0.55f) * r;   // kubbenin altinda: arma okunur kalir
            Fill(root, "Arma-Asinma", Blob(new Vector2(x, y), Range(rng, 0.04f, 0.09f) * r, rng), hull, 5);
        }
        for (int i = 0; i < 8; i++)
        {
            float x = Range(rng, -0.7f, 0.7f) * r, y = Range(rng, -0.9f, 0.2f) * r;
            Fill(root, "Arma-Kum", Blob(new Vector2(x, y), Range(rng, 0.02f, 0.04f) * r, rng), sand, 6);
        }
        return root;
    }

    // Daire; alt yarida kenari kumla kemirilmis (yaricap yer yer iceri girer)
    static List<Vector2> Eroded(float r, System.Random rng)
    {
        var rim = new List<Vector2>();
        for (int k = 0; k < RimPoints; k++)
        {
            float a = -k * Mathf.PI * 2f / RimPoints;
            float bite = Mathf.Sin(a) < -0.3f ? Range(rng, 0f, 0.12f) : 0f;
            rim.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * (1f - bite));
        }
        return rim;
    }

    static List<Vector2> DomeShape(Vector2 baseCenter, float r)
    {
        var rim = new List<Vector2> { baseCenter + new Vector2(-r, 0f) };
        for (int k = 0; k <= 16; k++)
        {
            float a = Mathf.PI * (1f - k / 16f);   // soldan saga ustten: saat yonu
            rim.Add(baseCenter + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r);
        }
        rim.Add(baseCenter + new Vector2(r, 0f));
        return rim;
    }

    static List<Vector2> Star(Vector2 c, float outer, float inner)
    {
        var rim = new List<Vector2>();
        for (int k = 0; k < 10; k++)
        {
            float a = Mathf.PI * 0.5f - k * Mathf.PI / 5f;   // tepeden baslar, saat yonunde
            rim.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (k % 2 == 0 ? outer : inner));
        }
        return rim;
    }

    static List<Vector2> Blob(Vector2 c, float r, System.Random rng)
    {
        var rim = new List<Vector2>();
        for (int k = 0; k < 7; k++)
        {
            float a = -k * Mathf.PI * 2f / 7f;
            rim.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * Range(rng, 0.6f, 1.2f));
        }
        return rim;
    }

    // Ortasindan yelpaze: rim saat yonunde (-Z'den bakan kameraya gore on yuz); layer: yuzeyden one kac kat
    static void Fill(Transform root, string name, List<Vector2> rim, Material paint, int layer)
    {
        var center = Vector2.zero;
        foreach (var p in rim) center += p;
        center /= rim.Count;
        var verts = new List<Vector3> { center };
        foreach (var p in rim) verts.Add(p);
        var tris = new List<int>();
        for (int k = 0; k < rim.Count; k++) tris.AddRange(new[] { 0, 1 + k, 1 + (k + 1) % rim.Count });
        var normals = new Vector3[verts.Count];
        for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.back;
        var mesh = new Mesh { name = name };
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        Parts.Add(name, root, mesh, paint, new Vector3(0f, 0f, -Layer * layer), outline: false, castShadow: false);
    }

    static float Range(System.Random rng, float a, float b) => a + (float)rng.NextDouble() * (b - a);
}
