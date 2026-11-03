using System.Collections.Generic;
using UnityEngine;

// Bolum 70: yan kanyonun arka duvarinda Ece'nin turkuaz kalemle cizdigi resim (spec 3a.4, K5). Soldan saga: Kivilcim'a benzeyen
// cocuk cizimi robot (kare bas, iki goz, anten, el sallar); iki satir cocuk eli yazi "KIVILCIM / BENİ BUL" (oyundaki tek okunur
// yazi istisnasi); gogusteki cizimin ayni cicegi; dokuz mumlu kucuk pasta; 69'daki dizinin sonu gibi bir kucuk ve bir buyuk el
// cevresi, yanlarinda robot eli icin bos yer (3b'de Kivilcim elini oraya koyar). Yazi telefonda okunacak boyda.
public static class WallMural
{
    // Yuzeyin yerel XY'sinde (x saga, y yukari; 0 = duvarin ortasinin zemini)
    // Kamera duvari yukaridan, yatik gorur (dikeyde ~yarisi kalir): yazi ve cizimler duvarin neredeyse tum yuksekligini kullanir.
    static readonly Vector2 RobotAt = new Vector2(-2.05f, 0.68f);
    const float RobotSize = 0.85f;
    static readonly Vector2 TopLineAt = new Vector2(-0.1f, 0.98f), BottomLineAt = new Vector2(-0.1f, 0.5f);
    const float LetterHeight = 0.4f;
    static readonly Vector2 FlowerAt = new Vector2(1.55f, 0.18f);   // sapin dibi
    const float FlowerHeight = 0.9f;
    static readonly Vector2 CakeAt = new Vector2(2.2f, 0.22f);      // pastanin alt ortasi
    const float CakeWidth = 0.46f;
    static readonly Vector2 SmallHandAt = new Vector2(2.75f, 0.5f), BigHandAt = new Vector2(3.08f, 0.46f), RobotHandAt = new Vector2(3.45f, 0.5f);
    const float Line = 0.026f;   // cizimlerin kalinligi (yazi kendi kalinliginda)

    // Resmin yerleri (duvarin kuruldugu bolum kokunun yerel konumu, duvarin biraz onu); 3b kamerasi ve sahnesi icin: Kivilcim'in elini
    // koyacagi bos yer, yazinin ortasi, robot ciziminin ortasi, kucuk ve buyuk el izi, cicegin tac yapraklari
    public readonly struct Spots
    {
        public readonly Vector3 RobotHand, TextCenter, Drawing, SmallHand, BigHand, Flower;
        public Spots(Vector3 robotHand, Vector3 textCenter, Vector3 drawing, Vector3 smallHand, Vector3 bigHand, Vector3 flower)
        {
            RobotHand = robotHand; TextCenter = textCenter; Drawing = drawing; SmallHand = smallHand; BigHand = bigHand; Flower = flower;
        }
    }

    public static Spots Paint(Transform wallFace, int seed)
    {
        var paint = CrayonMarks.Paint;
        BrushPaint.Write(wallFace, "KIVILCIM", Lifted(TopLineAt), LetterHeight, paint, seed, angleDeg: 2f, thickness: 0.12f, childish: true).name = "DuvarYazisi-1";
        BrushPaint.Write(wallFace, "BENİ BUL", Lifted(BottomLineAt), LetterHeight, paint, seed + 1, angleDeg: 1f, thickness: 0.12f, childish: true).name = "DuvarYazisi-2";

        var lines = new List<Vector2[]>();
        lines.AddRange(RobotDrawing(RobotAt, RobotSize));
        lines.AddRange(Flower(FlowerAt, FlowerHeight));
        lines.AddRange(Cake(CakeAt, CakeWidth));
        CrayonMarks.Draw(wallFace, "DuvarResmi", lines, seed + 2, Line);
        HandTrace.Draw(wallFace, SmallHandAt, Hand.Small, -6f, seed + 3);
        HandTrace.Draw(wallFace, BigHandAt, Hand.Big, 4f, seed + 4);

        var center = (TopLineAt + BottomLineAt) * 0.5f;
        var bloom = FlowerAt + new Vector2(0f, FlowerHeight * 0.75f);
        return new Spots(ToParent(wallFace, RobotHandAt), ToParent(wallFace, center), ToParent(wallFace, RobotAt),
            ToParent(wallFace, SmallHandAt), ToParent(wallFace, BigHandAt), ToParent(wallFace, bloom));
    }

    static Vector3 Lifted(Vector2 at) => new Vector3(at.x, at.y, CrayonMarks.Lift);

    // Yuzeydeki nokta, duvarin kuruldugu ebeveynin (bolumun koku) yerel konumunda; biraz onde (elin duracagi yer)
    static Vector3 ToParent(Transform face, Vector2 at)
    {
        var world = face.TransformPoint(new Vector3(at.x, at.y, CrayonMarks.Lift - 0.05f));
        var level = face.parent != null ? face.parent.parent : null;   // yuz -> duvar -> bolum
        return level != null ? level.InverseTransformPoint(world) : world;
    }

    // Cocuk cizimi robot: kare bas, iki yuvarlak goz, anten ve topu, kare govde, el sallayan kol, altta itici cizgileri (ayak yok)
    static IEnumerable<Vector2[]> RobotDrawing(Vector2 center, float size)
    {
        Vector2 P(float x, float y) => center + new Vector2(x - 0.5f, y - 0.5f) * size;
        yield return new[] { P(0.32f, 0.58f), P(0.68f, 0.6f), P(0.67f, 0.9f), P(0.31f, 0.88f), P(0.32f, 0.58f) };   // bas
        yield return Circle(P(0.42f, 0.75f), 0.035f * size);
        yield return Circle(P(0.58f, 0.76f), 0.035f * size);
        yield return new[] { P(0.5f, 0.89f), P(0.51f, 1.0f) };                                                         // anten
        yield return Circle(P(0.51f, 1.03f), 0.03f * size);
        yield return new[] { P(0.28f, 0.2f), P(0.72f, 0.22f), P(0.7f, 0.56f), P(0.3f, 0.55f), P(0.28f, 0.2f) };        // govde
        yield return new[] { P(0.3f, 0.45f), P(0.12f, 0.3f) };                                                          // sol kol
        yield return new[] { P(0.7f, 0.46f), P(0.86f, 0.62f), P(0.9f, 0.75f) };                                         // el sallayan sag kol
        yield return new[] { P(0.42f, 0.18f), P(0.4f, 0.06f) };                                                         // itici
        yield return new[] { P(0.58f, 0.18f), P(0.6f, 0.06f) };
    }

    // Gogusteki cicek (ChildDrawing olculeri): sap, iki yaprak, bes tac yaprak, orta. height: sapin dibinden tac yapraklarin tepesine.
    static IEnumerable<Vector2[]> Flower(Vector2 stemBase, float height)
    {
        float top = ChildDrawing.FlowerY + ChildDrawing.PetalDistance + ChildDrawing.PetalRadius;
        float k = height / (top - ChildDrawing.StemBottom);
        Vector2 P(float x, float y) => stemBase + new Vector2(x - ChildDrawing.FlowerX, y - ChildDrawing.StemBottom) * k;
        yield return new[] { P(ChildDrawing.FlowerX, ChildDrawing.StemBottom), P(ChildDrawing.FlowerX, ChildDrawing.StemTop) };
        foreach (var leaf in ChildDrawing.Leaves) yield return new[] { P(leaf.x, leaf.y), P(leaf.z, leaf.w) };
        for (int i = 0; i < ChildDrawing.Petals; i++)
        {
            float a = ChildDrawing.PetalAngle(i);
            var c = P(ChildDrawing.FlowerX + Mathf.Cos(a) * ChildDrawing.PetalDistance, ChildDrawing.FlowerY + Mathf.Sin(a) * ChildDrawing.PetalDistance);
            yield return Circle(c, ChildDrawing.PetalRadius * k);
        }
        yield return Circle(P(ChildDrawing.FlowerX, ChildDrawing.FlowerY), ChildDrawing.CenterRadius * k);
    }

    // Kucuk pasta: govde, ust kenar, ustunde dokuz kisa dik cizgi (mum)
    static IEnumerable<Vector2[]> Cake(Vector2 bottom, float width)
    {
        float h = width * 0.55f, half = width * 0.5f;
        yield return new[] { bottom + new Vector2(-half, 0f), bottom + new Vector2(half, 0f), bottom + new Vector2(half * 0.95f, h),
            bottom + new Vector2(-half * 0.95f, h), bottom + new Vector2(-half, 0f) };
        yield return new[] { bottom + new Vector2(-half * 0.95f, h * 0.55f), bottom + new Vector2(half * 0.95f, h * 0.6f) };   // krema
        const int candles = 9;
        for (int i = 0; i < candles; i++)
        {
            float x = -half * 0.8f + width * 0.8f * i / (candles - 1);
            yield return new[] { bottom + new Vector2(x, h + 0.02f), bottom + new Vector2(x + 0.004f, h + 0.11f) };
        }
    }

    static Vector2[] Circle(Vector2 c, float r)
    {
        const int n = 14;
        var p = new Vector2[n + 1];
        for (int i = 0; i <= n; i++)
        {
            float a = i * Mathf.PI * 2f / n;
            p[i] = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
        }
        return p;
    }
}
