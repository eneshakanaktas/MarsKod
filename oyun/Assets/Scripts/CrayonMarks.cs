using System.Collections.Generic;
using UnityEngine;

// Ece'nin kalemle duvara cizdikleri (Bolge 7: el izleri, 70'teki duvar resmi; Bolge 8: istasyon duvar cizimleri). Cizgiler ince,
// hafif titrek ve yer yer kesik (kalem purtuklu yuzeyde atlar). Bolge 7'de turkuaz kalem; Bolge 8'de turuncu (annesinin yol isaretledigi
// kalem, Bolge 6'nin boya oklariyla ayni renk). Golgedeki duvarda da secilsin diye boya hafif parlar.
public static class CrayonMarks
{
    // Yuzeyin biriminde cizer (BrushPaint.Lines): lines dogrudan yuzeyin yerel XY'sinde
    public static Transform Draw(Transform surface, string name, IEnumerable<Vector2[]> lines, int seed, float thickness) =>
        Draw(surface, name, lines, seed, thickness, Paint, Lift);

    // paint: kalemin rengi; lift: yuzeyden one uzaklik (duz duvarda kucuk)
    public static Transform Draw(Transform surface, string name, IEnumerable<Vector2[]> lines, int seed, float thickness, Material paint, float lift)
    {
        var drawn = BrushPaint.Lines(surface, Broken(lines, seed, thickness * 6f).ToArray(), lift, paint, seed, thickness);
        drawn.name = name;
        return drawn;
    }

    public static Material Paint => Mats.Emissive(CraterTraces.EceColor, CraterTraces.EceColor * 0.3f, 0.2f);
    // Bolge 8: turuncu kalem (PaintMarks'in boya rengi)
    public static Material OrangePaint => Mats.Emissive(PaintMarks.FreshInShade, PaintMarks.FreshInShade * 0.3f, 0.2f);

    // Duvar yuzunun onu: RiverWall katmanlari yuzden en fazla ~8 cm one cikar (Setback dalgasi); cizim hepsinin onunde kalir
    public const float Lift = -0.085f;

    // Uzun cizgileri yer yer keser: her ~gap*6 uzunlukta bir kisa bosluk (rastgele yerde)
    static List<Vector2[]> Broken(IEnumerable<Vector2[]> lines, int seed, float gap)
    {
        var rng = new System.Random(seed + 101);
        var result = new List<Vector2[]>();
        foreach (var sparse in lines)
        {
            var line = Densify(sparse, gap * 0.5f);
            var piece = new List<Vector2> { line[0] };
            float run = 0f, nextBreak = gap * (5f + 4f * (float)rng.NextDouble());
            for (int i = 1; i < line.Length; i++)
            {
                run += Vector2.Distance(line[i - 1], line[i]);
                piece.Add(line[i]);
                if (run < nextBreak || i == line.Length - 1 || piece.Count < 2) continue;
                result.Add(piece.ToArray());
                piece = new List<Vector2>();   // bosluk: siradaki nokta yeni parcanin basi
                run = 0f;
                nextBreak = gap * (5f + 4f * (float)rng.NextDouble());
            }
            if (piece.Count >= 2) result.Add(piece.ToArray());
        }
        return result;
    }

    // Noktalari en fazla step arayla sikilastirir (kesikler her yerde olabilsin)
    static Vector2[] Densify(Vector2[] line, float step)
    {
        var dense = new List<Vector2> { line[0] };
        for (int i = 1; i < line.Length; i++)
        {
            int n = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(line[i - 1], line[i]) / step));
            for (int k = 1; k <= n; k++) dense.Add(Vector2.Lerp(line[i - 1], line[i], (float)k / n));
        }
        return dense.ToArray();
    }
}
