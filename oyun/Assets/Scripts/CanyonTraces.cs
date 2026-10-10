using System;
using System.Collections.Generic;
using UnityEngine;

// Bolge 6 · Kanyon girisi (Bolum 51-60, fonksiyonlar): docs/tasarim/senaryo-bolge-06.md, gorunus: tasarim belgesi 3a
// (docs/superpowers/specs/2026-10-08-bolge-6-design.md). Bolge acilisi, Bolum 52 basindaki Kayit 2 ve bolum bolum izler:
// 51-52 kanyon agzi, 53-56 acik sundurma deponun onu, 57 deponun arka avlusu, 58 alcak kaya basamagi, 59 kanyon kenarina
// dogru, 60 kanyona inen patika. Alan disindaki her nesne zeminin gercek yuksekligine oturur (havada nesne yok).
public static class CanyonTraces
{
    public const int FirstLevel = 51, RecordingLevel = 52, LastLevel = 60;
    public const int StormLevel = 52, DepotFirst = 53, DepotLast = 56;
    public const float DescentGap = 1.5f, DescentPathX = 0.9f;   // 60: kenarin alana uzakligi; patikanin x'i (hedef sagda)
    static readonly int IndoorId = Shader.PropertyToID("_CanyonIndoor");
    static readonly int DescentId = Shader.PropertyToID("_CanyonDescent");

    // 60'ta zemin cizimine kenar bilgisi (x: kenarin z'si, y: 1 acik, z: patikanin x'i, w: alanin arka kenari). Build hesaplar,
    // ResetBackdrop yazar: bolum yuklenince ve "yeniden dene"de ayni deger kalir; 60 disinda sifir.
    static Vector4 descent;

    // Bolum 60'in patikasinin gectigi serit (alanin arkasindan kanyonun agzina): cevre kayalari buraya konmaz (Scenery)
    public static Rect PathCorridor(Vector2 areaHalf) =>
        Rect.MinMaxRect(-0.4f, areaHalf.y, DescentPathX + 0.6f, areaHalf.y + DescentGap + 9f);

    // Bolum 53-56 acik sundurma deponun onunde gecer (zemin beton, engeller sandik)
    public static bool IsDepot(int levelNumber) => levelNumber >= DepotFirst && levelNumber <= DepotLast;

    // Zemin cizimine "depo" ve "kanyona inis" bilgisi; Bolge 6 disinda hep 0
    public static void ResetBackdrop(int levelNumber)
    {
        Shader.SetGlobalFloat(IndoorId, IsDepot(levelNumber) ? 1f : 0f);
        Shader.SetGlobalVector(DescentId, levelNumber == LastLevel ? descent : Vector4.zero);
    }

    // Bolumun izlerini kurar (Bolge 6 disinda hicbir sey). itemPositions: toplanacak drone parcalarinin yerleri
    // (53'te raf o siraya kurulur); targetPos: hedef kare (59'da Serce onun ustunde bekler); ground: zemin yuksekligi (x, z).
    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Vector3? targetPos, IReadOnlyList<Vector3> itemPositions,
        Func<float, float, float> ground)
    {
        ActiveSparrow = null;
        descent = Vector4.zero;
        if (levelNumber < FirstLevel || levelNumber > LastLevel) return;
        switch (levelNumber)
        {
            case FirstLevel:
                ColonyRocket.Create(parent, OnGround(ground, -areaHalf.x + 0.8f, areaHalf.y + 1.4f), 0.7f);
                MarkedRock(parent, OnGround(ground, areaHalf.x - 0.3f, areaHalf.y + 1.0f), 1.1f, 75f, levelNumber);
                break;
            case StormLevel:
                ColonyRocket.Create(parent, OnGround(ground, -areaHalf.x + 1.2f, areaHalf.y + 1.9f), 0.3f);   // kucuk: kisa kodda kamera yaklasinca ust yazilara degmesin
                MarkedRock(parent, OnGround(ground, areaHalf.x - 0.2f, areaHalf.y + 1.0f), 0.9f, 70f, levelNumber);
                StormScatter(parent, areaHalf, ground);
                break;
            case 53:
            case 54:
            case 55:
            case 56:
                BuildDepot(levelNumber, parent, areaHalf, itemPositions, ground);
                break;
            case 57:
                // sundurma sol arkada, alanin hemen gerisinde: uzaga konunca dibi ufuk pusuna karisip tepelerin ustunde duruyor gibi gorunuyordu
                DepotShed.Build(parent, new Vector3(-areaHalf.x * 0.5f, 0f, areaHalf.y + 0.6f), 2.4f, bigHole: false, ground);
                MarkedRock(parent, OnGround(ground, areaHalf.x - 0.6f, areaHalf.y + 0.9f), 0.9f, 20f, levelNumber);
                ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf, ground), SparrowState.Flying);
                break;
            case DatesLevel:
                FadedArrows(CanyonLedge.Step(parent, areaHalf.y + 0.5f, areaHalf.x * 2f + 1.4f, LedgeHeight, ground), areaHalf);
                ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf, ground), SparrowState.Flying);
                break;
            case 59:
                MarkedRock(parent, OnGround(ground, -areaHalf.x + 0.6f, areaHalf.y + 0.9f), 0.9f, 10f, levelNumber);
                ActiveSparrow = Sparrow.Create(parent, (targetPos ?? HoverSpot(areaHalf, ground)) + Vector3.up * 1.0f, SparrowState.Flying);
                break;
            case LastLevel:
                // kanyona inen patika: alanin arkasindan kenara gider, kenarda asagi doner (spec 3a.2)
                var pathHead = CanyonLedge.Edge(parent, areaHalf.y + DescentGap, areaHalf.x * 2f + 3f, DescentPathX, ground);
                PaintMarks.Arrow(pathHead, Vector3.zero, 90f, 0.5f, fade: 0f, drips: false, seed: levelNumber);
                ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf, ground), SparrowState.Flying);
                descent = new Vector4(areaHalf.y + DescentGap, 1f, DescentPathX, areaHalf.y);
                break;
        }
    }

    const float LedgeHeight = 0.38f;   // 58'deki kaya basamagi: ufku (kayaliklarin dibini) kesmeyecek kadar alcak

    static Vector3 OnGround(Func<float, float, float> ground, float x, float z) => new Vector3(x, ground(x, z), z);

    // Serce (55'ten sonra yanimizda): bolumde yoksa null. 3b sahneleri kullanir.
    public static Sparrow ActiveSparrow { get; private set; }

    const int SparrowShelfLevel = 54, SparrowRepairLevel = 55, BoardLevel = 56, DatesLevel = 58;

    // Serce'nin bekledigi yer: alanin sag arka kosesinin ustu (sag ustteki pusula ve dugmelerin altina girmez)
    static Vector3 HoverSpot(Vector2 areaHalf, Func<float, float, float> ground) =>
        OnGround(ground, areaHalf.x - 0.4f, areaHalf.y - 0.5f) + Vector3.up * 1.1f;

    // Depo (53-56): alanin arkasinda acik sundurma; alan onun onundeki beton. 53'te parcalarin sirasi raf (alanin icinde),
    // 54-55'te Serce sundurmanin altindaki rafta, 56'da pano sag on direkte. Brandada bir ok.
    static void BuildDepot(int levelNumber, Transform parent, Vector2 areaHalf, IReadOnlyList<Vector3> itemPositions, Func<float, float, float> ground)
    {
        var shed = DepotShed.Build(parent, new Vector3(0f, 0f, areaHalf.y + ShedGap), areaHalf.x * 2f + 1.2f,
            bigHole: levelNumber == SparrowRepairLevel, ground);
        if (levelNumber == DepotFirst && itemPositions.Count > 0)
            StorageShelf.Create(parent, new Vector3(0f, 0f, itemPositions[0].z), areaHalf.x * 2f, stocked: false);
        if (levelNumber == SparrowShelfLevel || levelNumber == SparrowRepairLevel)
            ActiveSparrow = Sparrow.Create(parent, shed.ShelfTop + Vector3.up * Sparrow.RestHeight,
                levelNumber == SparrowShelfLevel ? SparrowState.Incomplete : SparrowState.Dormant);
        if (levelNumber == BoardLevel)
        {
            TaskBoard.Create(parent, shed.PostFace);
            ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf, ground), SparrowState.Flying);
        }
        PaintMarks.Arrow(shed.TarpFace, Vector3.zero, levelNumber % 2 == 0 ? 0f : 180f, 0.36f, fade: 0.2f, drips: false, seed: levelNumber);
    }

    const float ShedGap = 0.55f;   // alanin arka kenari ile sundurmanin on direkleri arasi

    // Bolum 58: kaya basamaginda soldan saga oklar. Eskiler gunes yemis (soluk), sonuncusu taze, buyuk, boyasi akmis;
    // yaninda elle "861". Tarih dizisinin tamami ustteki damgada (spec 3a.4).
    static void FadedArrows(Transform face, Vector2 areaHalf)
    {
        const int Count = 5;
        for (int i = 0; i < Count; i++)
        {
            bool last = i == Count - 1;
            float x = -areaHalf.x + 0.3f + i * (areaHalf.x * 2f - 0.9f) / (Count - 1);
            PaintMarks.Arrow(face, new Vector3(x, last ? 0.2f : 0.18f, 0f), last ? -6f : -2f + i, last ? 0.36f : 0.26f,
                fade: last ? 0f : 0.85f - 0.1f * i, drips: last, seed: 580 + i);
        }
        PaintMarks.Word(face, "861", new Vector3(areaHalf.x - 1.3f, 0.17f, 0f), 0.2f, fade: 0f, seed: 861);   // son iki okun arasinda
    }

    // Alanin arkasinda taze boya oklu kaya (yolun devami). arrowDeg: okun yonu.
    static void MarkedRock(Transform parent, Vector3 pos, float size, float arrowDeg, int seed) =>
        PaintMarks.Arrow(MarkedBoulder(parent, pos, size), new Vector3(0f, size * 0.34f, -0.01f), arrowDeg, size * 0.65f,
            fade: 0f, drips: false, seed: seed);

    // Uzerine ok boyanmis kaya blogu: iki katli tortu kayasi, on yuzu duz. Doner: on yuz (ok buraya; -Z'ye bakar).
    static Transform MarkedBoulder(Transform parent, Vector3 pos, float size)
    {
        var root = Parts.Empty("Iz-OkluKaya", parent);
        root.localPosition = pos;
        root.localRotation = Quaternion.Euler(0f, -8f, 0f);
        Parts.Add("Alt", root, MeshFactory.RoundedBox(new Vector3(size, size * 0.42f, size * 0.6f), 0.06f), Mats.Lit(Mats.Hex("#8E5A44"), 0.12f), new Vector3(0f, size * 0.21f, 0f));
        Parts.Add("Ust", root, MeshFactory.RoundedBox(new Vector3(size * 0.86f, size * 0.36f, size * 0.52f), 0.06f), Mats.Lit(Mats.Hex("#A8705A"), 0.12f), new Vector3(0.04f, size * 0.6f, 0.03f));
        var cap = Parts.Add("Tepe", root, MeshFactory.Rock(size * 0.3f, 610), Mats.Lit(Mats.Hex("#7A4A38"), 0.12f), new Vector3(-0.1f, size * 0.8f, 0.05f));
        cap.localScale = new Vector3(1.3f, 0.6f, 1f);
        var face = Parts.Empty("Yuzey", root);
        face.localPosition = new Vector3(0f, 0f, -size * 0.3f);
        return face;
    }

    // Bolum 52: firtinada dagilmis drone'un kirintilari alanin iki yaninda; yarisi kuma gomulu, yaninda ruzgarin surukledigi iz.
    static void StormScatter(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        var accent = Mats.Lit(DroneColors.Accent, 0.4f);
        var drag = Mats.Lit(Mats.Hex("#5A3426"), 0.08f);
        var spots = new[]
        {
            new Vector3(-areaHalf.x - 0.45f, 0f, 1.1f), new Vector3(-areaHalf.x - 0.7f, 0f, -0.9f),
            new Vector3(areaHalf.x + 0.5f, 0f, 0.4f), new Vector3(areaHalf.x + 0.4f, 0f, areaHalf.y + 0.3f),
        };
        for (int i = 0; i < spots.Length; i++)
        {
            spots[i].y = ground(spots[i].x, spots[i].z);
            var bit = Parts.Empty("Iz-Kirinti", parent);
            bit.localPosition = spots[i] + new Vector3(0f, -0.015f, 0f);
            bit.localRotation = Quaternion.Euler(18f * Mathf.Sin(i * 2.3f), i * 77f, 24f * Mathf.Cos(i * 1.4f));
            bit.localScale = Vector3.one * 2.2f;
            DronePart.BuildPiece(bit, (DronePiece)(i % 3), accent);
            // ruzgar batidan esti: iz parcanin batisina (soluna) uzanir
            var trail = Parts.Add("Iz-Suruklenme", parent, MeshFactory.RoundedBox(new Vector3(0.6f, 0.01f, 0.05f), 0.02f), drag,
                spots[i] + new Vector3(-0.38f, 0.004f, 0.03f * (i - 1.5f)), outline: false, castShadow: false);
            trail.localRotation = Quaternion.Euler(0f, 8f * Mathf.Sin(i * 3.1f), 0f);
        }
    }

    // Bolge 6 acilisi: kara ekranda iki satir
    public static readonly string[] TransitionLines = { "Yeni bölge: Kanyon girişi", "Hedef: kanyon deposu" };

    // Kayit 2 (Defne'nin sesi, hikaye-kitabi.md "Kayitlar"): Bolum 52 basinda, bir kez
    public const string RecordingFile = "rutinler.ses";
    public static readonly string[] RecordingLines =
    {
        "Ona ezber değil, beceri öğret.",
        "Bir kez öğrettiğini kendisi tekrar edebilir.",
        "Bunlara biz 'rutin' derdik.",
        "İlki sabah turuydu; her sabah yapardı.",
    };
}
