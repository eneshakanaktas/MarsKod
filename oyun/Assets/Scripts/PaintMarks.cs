using UnityEngine;

// Kanyondaki turuncu boya oklar ve tek okunur kelimeler (Bolge 6): D. Aras'in isaretledigi yol. Elle, firca ile boyanmis
// (BrushPaint): titrek, uclari yuvarlak. Yuzeye (kaya, branda, zemin) yapisik: yuzeyin yerel XY duzleminde, -Z'ye bakar.
public static class PaintMarks
{
    public static readonly Color Paint = Mats.Hex("#FF8436");
    static readonly Color SunBleached = Mats.Hex("#A8705A");   // gunes yemis boya kayanin rengine karisir

    // Ok: govde + iki kanatli uc; 0..1 kutuda saga bakar
    static readonly Vector2[][] ArrowShape =
    {
        new[] { new Vector2(0f, 0.5f), new Vector2(1f, 0.5f) },
        new[] { new Vector2(0.7f, 0.78f), new Vector2(1f, 0.5f), new Vector2(0.7f, 0.22f) },
    };

    // fade: 0 taze (parlak: golgedeki kizil kayada da secilsin) .. 1 gunes yemis, kayaya karismis
    public static Material PaintMat(float fade) =>
        Mats.Emissive(Color.Lerp(Paint, SunBleached, fade * 0.75f), Paint * 0.6f * (1f - fade), 0.25f);

    // angleDeg: okun gosterdigi yon (0 saga, 90 yukari). drips: altindan akan boya (acele boyanmis).
    public static Transform Arrow(Transform surface, Vector3 localPos, float angleDeg, float size, float fade, bool drips, int seed)
    {
        var mat = PaintMat(fade);
        var arrow = BrushPaint.Strokes(surface, ArrowShape, localPos, size, angleDeg, mat, seed, thickness: 0.16f);
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
