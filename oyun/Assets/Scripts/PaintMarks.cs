using System.Collections.Generic;
using UnityEngine;

// Kanyondaki turuncu boya oklar ve tek okunur kelimeler (Bolge 6): D. Aras'in isaretledigi yol. Elle, firca ile boyanmis
// (BrushPaint): titrek, uclari yuvarlak. Yuzeye (kaya, branda, zemin) yapisik: yuzeyin yerel XY duzleminde, -Z'ye bakar.
public static class PaintMarks
{
    // Ok: govde + iki kanatli uc; 0..1 kutuda saga bakar
    static readonly Vector2[][] ArrowShape =
    {
        new[] { new Vector2(0f, 0.5f), new Vector2(1f, 0.5f) },
        new[] { new Vector2(0.7f, 0.78f), new Vector2(1f, 0.5f), new Vector2(0.7f, 0.22f) },
    };

    // Dolu, duzgun ok (60'in patikasinin basi): yuzeyin XY duzleminde yatar, saga bakar; length: ok boyu. Boya degil, net bir isaret.
    public static Transform FilledArrow(Transform surface, Vector3 localPos, float angleDeg, float length, Material mat)
    {
        var verts = new[]
        {
            new Vector3(0.5f, 0f), new Vector3(0.02f, -0.38f), new Vector3(0.02f, 0.38f),                                     // uc (ucgen)
            new Vector3(-0.5f, -0.14f), new Vector3(-0.5f, 0.14f), new Vector3(0.02f, 0.14f), new Vector3(0.02f, -0.14f),      // govde
        };
        var mesh = new Mesh { name = "Ok" };
        mesh.SetVertices(verts);
        var normals = new Vector3[verts.Length];
        for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.back;
        mesh.SetNormals(normals);
        mesh.SetTriangles(new[] { 0, 1, 2, 3, 4, 5, 3, 5, 6 }, 0);   // saat yonunde: -Z'den bakan kamera on yuzu gorur
        mesh.RecalculateBounds();
        var arrow = Parts.Add("Ok", surface, mesh, mat, localPos, outline: false, castShadow: false);
        arrow.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
        arrow.localScale = Vector3.one * length;
        return arrow;
    }

    // Duzgun ok: govde ve simetrik uc (iki kanat ayni aci ve boyda)
    static readonly Vector2[][] CleanArrowShape =
    {
        new[] { new Vector2(0.08f, 0.5f), new Vector2(0.9f, 0.5f) },
        new[] { new Vector2(0.58f, 0.84f), new Vector2(0.92f, 0.5f), new Vector2(0.58f, 0.16f) },
    };

    // fade: 0 taze (parlak: golgedeki kizil kayada da secilsin) .. 1 gunes yemis, kayaya karismis.
    // Isiktan etkilenmez (Unlit): kayalarin cogu gunesin karsisinda, golgede; isikli boya orada sonuk kahveye donuyordu.
    // (Parlayan malzeme pakette parlamiyor: Mats.Emissive'in _EMISSION'i pakete girmiyor, ILERLEME.md acik sorular.)
    static readonly Color FreshInShade = Mats.Hex("#E9783A");   // gunes almayan yuzde taze boyanin gorunen rengi
    static readonly Color BleachedInShade = Mats.Hex("#80503E");   // gunes yemis: kayanin rengine karismis
    public static Material PaintMat(float fade) =>
        Mats.Unlit(Color.Lerp(FreshInShade, BleachedInShade, fade * 0.75f));

    // angleDeg: okun gosterdigi yon (0 saga, 90 yukari). drips: altindan akan boya (acele boyanmis).
    // clean: titremesiz, duzgun ok (60'in patikasinin basi); degilse acele elle boyanmis
    public static Transform Arrow(Transform surface, Vector3 localPos, float angleDeg, float size, float fade, bool drips, int seed, bool clean = false)
    {
        var mat = PaintMat(fade);
        var arrow = clean
            ? BrushPaint.Strokes(surface, CleanArrowShape, localPos, size, angleDeg, mat, seed, thickness: 0.17f, jitter: 0f)
            : BrushPaint.Strokes(surface, ArrowShape, localPos, size, angleDeg, mat, seed, thickness: 0.2f);
        if (drips) Drips(surface, arrow, size, mat, seed);
        return arrow;
    }

    public static Transform Word(Transform surface, string text, Vector3 localPos, float height, float fade, int seed) =>
        BrushPaint.Write(surface, text, localPos, height, PaintMat(fade), seed);

    // Okun govdesinin altindan uc akinti; ok dondurulse de asagi (yuzeyin -Y'si) akar
    static void Drips(Transform surface, Transform arrow, float size, Material mat, int seed)
    {
        var lines = new Vector2[3][];
        for (int k = 0; k < lines.Length; k++)
        {
            Vector3 start = surface.InverseTransformPoint(arrow.TransformPoint(new Vector3(-0.3f + 0.2f * k, -0.03f, 0f)));
            float len = size * (0.18f + 0.12f * k);
            lines[k] = new[] { new Vector2(start.x, start.y), new Vector2(start.x, start.y - len) };
        }
        BrushPaint.Lines(surface, lines, arrow.localPosition.z, mat, seed + 7, thickness: size * 0.04f);
    }
}
