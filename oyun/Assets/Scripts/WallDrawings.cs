using System.Collections.Generic;
using UnityEngine;

public enum Drawing { Colony, Storm, Descent, AloneOutside, RobotAndGirl }   // hikaye sirasi (senaryo-perde-2.md)

// Bolge 8 · Ece'nin istasyon duvarina cizdikleri (73, 75; docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md, Gorev 4).
// Arka duvarin acik bandinda (cocuk boyu), soldan saga hikaye sirasiyla: koloni, firtina bulutu, yeraltina inen insanlar (24 cop adam,
// en asagidaki kucuk: Ece ilk iner), disarida tek robot, el ele robot ve kiz (gogusteki cizimin ayni olculeri) + altinda rota.
// 73'te ilk uc, 75'te bes (ayni duvar, ayni yerler; saga dogru devam). Turuncu kalem, ince ve titrek (CrayonMarks); kimse soylemez.
// Kamera duvari yukaridan, yatik gorur (dikeyde ~yarisi kalir): cizimler bandin neredeyse butun yuksekligini kullanir.
public static class WallDrawings
{
    public const int DescentPeople = 24;   // yeraltina inen insanlar (koloninin 24 kisisi)
    public const string Route = "KKDDDKKB";   // 75'in baslangic kodundaki rota ile harfi harfine ayni (bolum-75.json)

    const float Bottom = 0.14f, Top = 1.12f;          // StationRoom.PanelTop'un (1,2) altinda, supurgeligin ustunde
    const float Lift = -0.008f;                       // duvar yuzunden one: panel derzlerinin onunde
    const float Line = 0.022f, PersonLine = 0.013f;   // cizgi kalinligi
    const float PersonHeight = 0.26f, EceHeight = 0.15f;

    // Her cizimin duvardaki yatay araligi (yuzeyin x'i); 73 ve 75'te ayni yerler
    static readonly Vector2[] Slots =
    {
        new Vector2(-3.3f, -2.15f), new Vector2(-2.05f, -0.9f), new Vector2(-0.8f, 0.95f), new Vector2(1.05f, 2.0f), new Vector2(2.05f, 3.3f),
    };

    // wallFace: StationRoom'un arka duvar yuzu. count: 73'te 3, 75'te 5. Doner: her cizimin ortasi (bolumun yerel konumu; 3b kamerasi icin).
    public static IReadOnlyList<Vector3> Paint(Transform wallFace, int count, int seed)
    {
        var paint = CrayonMarks.OrangePaint;
        var centers = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            var drawing = (Drawing)i;
            var box = new Rect(Slots[i].x, Bottom, Slots[i].y - Slots[i].x, Top - Bottom);
            int people = 0;
            var lines = Lines(drawing, box, ref people);
            string name = "Cizim-" + (i + 1) + "-" + drawing + (people > 0 ? " (" + people + " kisi)" : "");
            CrayonMarks.Draw(wallFace, name, lines, seed + i, drawing == Drawing.Descent ? PersonLine : Line, paint, Lift);
            if (drawing == Drawing.RobotAndGirl)
                BrushPaint.Write(wallFace, Route, new Vector3(box.center.x, Bottom + 0.16f, Lift), 0.2f, paint, seed + 20, angleDeg: 1.5f,
                    thickness: 0.11f, childish: true).name = "Cizim-Rota";
            centers.Add(wallFace.localPosition + new Vector3(box.center.x, box.center.y, Lift));
        }
        return centers;
    }

    static List<Vector2[]> Lines(Drawing drawing, Rect box, ref int people)
    {
        var lines = new List<Vector2[]>();
        switch (drawing)
        {
            case Drawing.Colony: Colony(lines, box); break;
            case Drawing.Storm: Storm(lines, box); break;
            case Drawing.Descent: people = Descent(lines, box); break;
            case Drawing.AloneOutside: AloneOutside(lines, box); break;
            case Drawing.RobotAndGirl: RobotAndGirl(lines, box); break;
        }
        return lines;
    }

    // box icinde oranla nokta (u saga, v yukari; 0..1)
    static Vector2 At(Rect box, float u, float v) => new Vector2(box.x + u * box.width, box.y + v * box.height);

    // 1. Koloni: yerde uc yarim daire kubbe, yaninda anten, ustte gunes
    static void Colony(List<Vector2[]> lines, Rect box)
    {
        float ground = box.y + 0.08f;
        lines.Add(new[] { At(box, 0.02f, 0.08f), At(box, 0.5f, 0.1f), At(box, 0.98f, 0.075f) });
        lines.Add(Dome(new Vector2(At(box, 0.2f, 0f).x, ground), 0.14f));
        lines.Add(Dome(new Vector2(At(box, 0.47f, 0f).x, ground), 0.2f));
        lines.Add(Dome(new Vector2(At(box, 0.73f, 0f).x, ground), 0.13f));
        var poleTop = At(box, 0.92f, 0.62f);
        lines.Add(new[] { At(box, 0.92f, 0.08f), poleTop });
        lines.Add(new[] { poleTop + new Vector2(-0.08f, 0.12f), poleTop, poleTop + new Vector2(0.07f, 0.13f) });   // canak
        Sun(lines, At(box, 0.17f, 0.8f), 0.08f);
    }

    // 2. Firtina bulutu: kucuk kubbelerin ustune gelen buyuk, karalanmis bulut; altinda toz cizgileri
    static void Storm(List<Vector2[]> lines, Rect box)
    {
        lines.Add(new[] { At(box, 0.02f, 0.07f), At(box, 0.55f, 0.09f), At(box, 0.98f, 0.07f) });
        lines.Add(Dome(At(box, 0.25f, 0.08f), 0.09f));
        lines.Add(Dome(At(box, 0.5f, 0.08f), 0.12f));
        lines.Add(Dome(At(box, 0.74f, 0.08f), 0.08f));
        // bulut: tepede bes kabarti, alti duz
        var outline = new List<Vector2>();
        float[] bumps = { 0.12f, 0.31f, 0.5f, 0.69f, 0.88f };
        outline.Add(At(box, 0.04f, 0.6f));
        foreach (float u in bumps)
        {
            var c = At(box, u, 0.8f + (u == 0.5f ? 0.08f : 0f));
            float r = box.width * 0.11f;
            for (int k = 0; k <= 6; k++)
            {
                float a = Mathf.PI * (1f - k / 6f);
                outline.Add(c + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * 1.3f));
            }
        }
        outline.Add(At(box, 0.96f, 0.6f));
        outline.Add(At(box, 0.04f, 0.6f));
        lines.Add(outline.ToArray());
        // ici karalanmis: zikzak
        var scribble = new List<Vector2>();
        for (int k = 0; k <= 12; k++) scribble.Add(At(box, 0.1f + k * 0.065f, k % 2 == 0 ? 0.64f : 0.88f));
        lines.Add(scribble.ToArray());
        // toz: buluttan asagi egik kisa cizgiler
        foreach (float u in new[] { 0.18f, 0.38f, 0.58f, 0.78f })
            lines.Add(new[] { At(box, u, 0.52f), At(box, u - 0.07f, 0.36f) });
    }

    // 3. Yeraltina inen insanlar: yerde acik kapak, ondan asagi merdiven; yerde iki sira bekleyenler ve merdivende inenler: 24 cop adam
    // (tek sirada birbirine degip zikzak gibi okunuyordu). En asagidaki (en once inen) kucuk: Ece. Doner: cizilen insan sayisi.
    static int Descent(List<Vector2[]> lines, Rect box)
    {
        const float groundV = 0.56f, hatchFrom = 0.6f, hatchTo = 0.7f;
        lines.Add(new[] { At(box, 0.01f, groundV), At(box, hatchFrom, groundV + 0.005f) });
        lines.Add(new[] { At(box, hatchTo, groundV), At(box, 0.99f, groundV - 0.005f) });
        lines.Add(new[] { At(box, hatchTo, groundV), At(box, hatchTo + 0.05f, groundV + 0.2f) });   // kapagin kalkik kanadi
        // merdiven: kapaktan asagi saga basamak basamak
        const int steps = 7;
        Vector2 top = At(box, hatchFrom + 0.02f, groundV), bottom = At(box, 0.98f, 0.02f);
        var stair = new List<Vector2> { top };
        for (int k = 1; k <= steps; k++)
        {
            var p = Vector2.Lerp(top, bottom, k / (float)steps);
            stair.Add(new Vector2(stair[stair.Count - 1].x, p.y));
            stair.Add(p);
        }
        lines.Add(stair.ToArray());

        // merdivende inenler (asagidan yukari: ilk = en derindeki = Ece), sonra yerde siradakiler (kapaktan sola)
        const int onStairs = 6;
        int drawn = 0;
        for (int k = 0; k < onStairs; k++)
        {
            float t = 1f - (k + 0.5f) / onStairs;   // 1 = merdivenin dibi
            var feet = Vector2.Lerp(top, bottom, t) + new Vector2(-0.02f, 0.01f);
            Person(lines, feet, k == 0 ? EceHeight : PersonHeight);
            drawn++;
        }
        // yerde iki sira (arkadaki biraz yukarida, yarim aralik kaymis)
        int onGround = DescentPeople - onStairs, perRow = (onGround + 1) / 2;
        float spacing = (hatchFrom - 0.06f) / (perRow - 0.5f);
        for (int k = 0; k < onGround; k++)
        {
            int row = k % 2, place = k / 2;
            float u = hatchFrom - 0.035f - place * spacing - row * spacing * 0.5f;
            Person(lines, At(box, u, groundV + 0.01f + row * 0.17f), PersonHeight);
            drawn++;
        }
        return drawn;
    }

    // 4. Disarida tek robot: yerde kapali kapak, yaninda kucuk robot (kare bas, anten), ustte yildizli gece ve ay
    static void AloneOutside(List<Vector2[]> lines, Rect box)
    {
        lines.Add(new[] { At(box, 0.02f, 0.1f), At(box, 0.98f, 0.11f) });
        lines.Add(new[] { At(box, 0.08f, 0.1f), At(box, 0.1f, 0.16f), At(box, 0.46f, 0.16f), At(box, 0.48f, 0.1f) });   // kapali kapak
        lines.Add(new[] { At(box, 0.25f, 0.16f), At(box, 0.26f, 0.2f), At(box, 0.31f, 0.2f), At(box, 0.32f, 0.16f) });   // tutamak
        // kucuk robot: govde, bas, iki goz, anten
        lines.Add(Box(At(box, 0.6f, 0.12f), At(box, 0.82f, 0.36f)));
        lines.Add(Box(At(box, 0.63f, 0.38f), At(box, 0.79f, 0.55f)));
        lines.Add(Circle(At(box, 0.68f, 0.47f), 0.014f));
        lines.Add(Circle(At(box, 0.74f, 0.47f), 0.014f));
        lines.Add(new[] { At(box, 0.71f, 0.55f), At(box, 0.715f, 0.64f) });
        lines.Add(Circle(At(box, 0.715f, 0.67f), 0.018f));
        // gece: yildizlar (kucuk arti) ve hilal
        var stars = new[] { new Vector2(0.1f, 0.88f), new Vector2(0.28f, 0.72f), new Vector2(0.42f, 0.93f), new Vector2(0.56f, 0.8f),
            new Vector2(0.18f, 0.6f), new Vector2(0.92f, 0.62f) };
        foreach (var s in stars)
        {
            var c = At(box, s.x, s.y);
            lines.Add(new[] { c + new Vector2(-0.025f, 0f), c + new Vector2(0.025f, 0f) });
            lines.Add(new[] { c + new Vector2(0f, -0.035f), c + new Vector2(0f, 0.035f) });
        }
        var moon = At(box, 0.82f, 0.86f);
        var crescent = new List<Vector2>();
        for (int k = 0; k <= 10; k++) { float a = Mathf.PI * (0.35f + 1.3f * k / 10f); crescent.Add(moon + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 0.07f); }
        for (int k = 10; k >= 0; k--) { float a = Mathf.PI * (0.45f + 1.1f * k / 10f); crescent.Add(moon + new Vector2(0.025f, 0f) + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 0.055f); }
        lines.Add(crescent.ToArray());
    }

    // 5. El ele robot ve kiz: gogusteki cizimin kiz + robot kismi, ayni olculerden (ChildDrawing). Rota yazisi altinda (Paint).
    static void RobotAndGirl(List<Vector2[]> lines, Rect box)
    {
        const float pxLeft = 56f, pxBottom = 20f, pxSize = 60f;   // dokuda kiz + robotun kapladigi kare (x 56-116, y 20-80)
        float size = 0.62f;
        var origin = new Vector2(box.center.x - size * 0.5f, box.y + 0.36f);
        Vector2 P(float x, float y) => origin + new Vector2(x - pxLeft, y - pxBottom) / pxSize * size;
        Vector2[] Seg(Vector4 l) => new[] { P(l.x, l.y), P(l.z, l.w) };
        Vector2[] Rectangle(Vector4 r) => new[] { P(r.x, r.y), P(r.z, r.y), P(r.z, r.w), P(r.x, r.w), P(r.x, r.y) };

        lines.Add(Circle(P(ChildDrawing.GirlHead.x, ChildDrawing.GirlHead.y), ChildDrawing.GirlHead.z / pxSize * size));
        foreach (var l in ChildDrawing.GirlSkirt) lines.Add(Seg(l));
        foreach (var l in ChildDrawing.GirlLimbs) lines.Add(Seg(l));
        lines.Add(Rectangle(ChildDrawing.RobotHead));
        lines.Add(Rectangle(ChildDrawing.RobotBody));
        lines.Add(Seg(ChildDrawing.RobotAntenna));
        lines.Add(Circle(P(ChildDrawing.AntennaTip.x, ChildDrawing.AntennaTip.y), ChildDrawing.AntennaTip.z / pxSize * size));
        foreach (var e in ChildDrawing.RobotEyes) lines.Add(Circle(P(e.x, e.y), 1.2f / pxSize * size));
        foreach (var l in ChildDrawing.RobotLegs) lines.Add(Seg(l));
        lines.Add(Seg(ChildDrawing.HoldingHands));
    }

    // Cop adam: bas, govde, iki bacak, kollar. feet: ayaklarin ortasi.
    static void Person(List<Vector2[]> lines, Vector2 feet, float height)
    {
        float head = height * 0.14f, w = height * 0.1f;
        var hip = feet + new Vector2(0f, height * 0.42f);
        var neck = feet + new Vector2(0f, height - head * 2f);
        lines.Add(Circle(neck + new Vector2(0f, head), head));
        lines.Add(new[] { neck, hip });
        lines.Add(new[] { feet + new Vector2(-w, 0f), hip, feet + new Vector2(w, 0f) });
        var shoulder = neck + new Vector2(0f, -height * 0.1f);
        lines.Add(new[] { shoulder + new Vector2(-w * 1.3f, -height * 0.22f), shoulder, shoulder + new Vector2(w * 1.3f, -height * 0.22f) });   // kollar asagi
    }

    // Yerdeki yarim daire kubbe (base: tabaninin ortasi)
    static Vector2[] Dome(Vector2 baseCenter, float r)
    {
        var p = new Vector2[11];
        for (int k = 0; k <= 10; k++)
        {
            float a = Mathf.PI * k / 10f;
            p[k] = baseCenter + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * 1.2f);
        }
        return p;
    }

    static void Sun(List<Vector2[]> lines, Vector2 c, float r)
    {
        lines.Add(Circle(c, r));
        for (int k = 0; k < 8; k++)
        {
            var d = new Vector2(Mathf.Cos(k * Mathf.PI / 4f), Mathf.Sin(k * Mathf.PI / 4f));
            lines.Add(new[] { c + d * r * 1.35f, c + d * r * 1.9f });
        }
    }

    static Vector2[] Box(Vector2 a, Vector2 b) => new[] { a, new Vector2(b.x, a.y), b, new Vector2(a.x, b.y), a };

    static Vector2[] Circle(Vector2 c, float r)
    {
        int n = r < 0.03f ? 8 : 14;
        var p = new Vector2[n + 1];
        for (int i = 0; i <= n; i++)
        {
            float a = i * Mathf.PI * 2f / n;
            p[i] = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
        }
        return p;
    }
}
