using System.Collections.Generic;
using UnityEngine;

// Kanyondaki turuncu boya oklar ve yanlarindaki kucuk tarihler (Bolge 6): D. Aras'in isaretledigi yol.
// Yuzeye (kaya, duvar) yapisik cizilir: yuzeyin yerel XY duzleminde, -Z'ye (kameraya) bakar. Gerceklik: boya ince, golge vermez.
public static class PaintMarks
{
    public static readonly Color Paint = Mats.Hex("#FF8436");
    static Mesh triangle;

    // angleDeg: okun gosterdigi yon (0 saga, 90 yukari). drips: altindan akan boya (acele cizilmis).
    public static Transform Arrow(Transform surface, Vector3 localPos, float angleDeg, float size, bool drips)
    {
        var mat = Mats.Emissive(Paint, Paint * 0.22f, 0.25f);   // hafif parlak: kizil kayada secilsin
        var arrow = Parts.Empty("Ok", surface);
        arrow.localPosition = localPos;
        arrow.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
        Parts.Add("Govde", arrow, MeshFactory.RoundedBox(new Vector3(size * 0.62f, size * 0.2f, 0.004f), 0.002f), mat,
            new Vector3(-size * 0.19f, 0f, 0f), outline: false, castShadow: false);
        var head = Parts.Add("Uc", arrow, Triangle(), mat, new Vector3(size * 0.1f, 0f, -0.001f), outline: false, castShadow: false);
        head.localScale = new Vector3(size * 0.42f, size * 0.55f, 1f);
        if (!drips) return arrow;
        // akintilar ok dondurulse de asagi akar
        for (int k = 0; k < 3; k++)
        {
            float len = size * (0.18f + 0.12f * k);
            var p = arrow.TransformPoint(new Vector3(-size * (0.4f - 0.2f * k), -size * 0.06f, 0f));
            var drip = Parts.Add("Akinti", surface, MeshFactory.RoundedBox(new Vector3(size * 0.035f, len, 0.004f), 0.002f), mat,
                surface.InverseTransformPoint(p) - new Vector3(0f, len * 0.5f, 0f), outline: false, castShadow: false);
            Parts.Add("Damla", drip, MeshFactory.Sphere(size * 0.03f, 4, 8), mat, new Vector3(0f, -len * 0.5f, 0f), outline: false, castShadow: false);
        }
        return arrow;
    }

    public static TextMesh Date(Transform surface, Vector3 localPos, string text, float height) =>
        WorldText.Create(surface, text, localPos, height, Paint);

    // Okun ucu: tabani x=0'da, sivri ucu x=1'de; -Z'ye bakan tek yuzlu ucgen
    static Mesh Triangle()
    {
        if (triangle != null) return triangle;
        triangle = new Mesh { name = "Ucgen" };
        triangle.SetVertices(new List<Vector3> { new Vector3(0f, 0.5f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, -0.5f, 0f) });
        triangle.SetNormals(new List<Vector3> { Vector3.back, Vector3.back, Vector3.back });
        triangle.SetTriangles(new[] { 0, 1, 2 }, 0);
        triangle.RecalculateBounds();
        return triangle;
    }
}
