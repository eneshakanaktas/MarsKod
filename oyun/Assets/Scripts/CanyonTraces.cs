using System.Collections.Generic;
using UnityEngine;

// Bolge 6 · Kanyon girisi (Bolum 51-60, fonksiyonlar): docs/tasarim/senaryo-bolge-06.md, gorunus: tasarim belgesi 3a
// (docs/superpowers/specs/2026-10-08-bolge-6-design.md). Bolge acilisi, Bolum 52 basindaki Kayit 2 ve bolum bolum izler:
// 51-52 kanyon agzi, 53-56 yari acik deponun ici, 57 deponun arka avlusu, 58 kanyon duvarinin dibi, 59-60 kanyon kenari.
public static class CanyonTraces
{
    public const int FirstLevel = 51, RecordingLevel = 52, LastLevel = 60;
    public const int StormLevel = 52, DepotFirst = 53, DepotLast = 56;
    static readonly int IndoorId = Shader.PropertyToID("_CanyonIndoor");

    // Bolum 53-56 yari acik deponun icinde gecer (zemin beton, engeller sandik)
    public static bool IsDepot(int levelNumber) => levelNumber >= DepotFirst && levelNumber <= DepotLast;

    // Zemin cizimine "depo ici" bilgisi; Bolge 6 disinda hep 0
    public static void ResetBackdrop(int levelNumber) => Shader.SetGlobalFloat(IndoorId, IsDepot(levelNumber) ? 1f : 0f);

    // Bolumun izlerini kurar (Bolge 6 disinda hicbir sey). itemPositions: toplanacak drone parcalarinin yerleri
    // (53'te raf o siraya kurulur); targetPos: hedef kare (59'da Serce onun ustunde bekler).
    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Vector3? targetPos, IReadOnlyList<Vector3> itemPositions)
    {
        ActiveSparrow = null;
        if (levelNumber < FirstLevel || levelNumber > LastLevel) return;
        switch (levelNumber)
        {
            case FirstLevel:
                ColonyRocket.Create(parent, new Vector3(-areaHalf.x + 0.8f, 0f, areaHalf.y + 1.4f), 0.7f);
                PaintMarks.Arrow(MarkedBoulder(parent, new Vector3(areaHalf.x - 0.3f, 0f, areaHalf.y + 1.0f), 1.1f), new Vector3(0f, 0.42f, -0.01f), 75f, 0.6f, false);
                break;
            case StormLevel:
                ColonyRocket.Create(parent, new Vector3(-areaHalf.x + 1.2f, 0f, areaHalf.y + 2.4f), 0.4f);
                PaintMarks.Arrow(MarkedBoulder(parent, new Vector3(areaHalf.x - 0.2f, 0f, areaHalf.y + 1.0f), 0.9f), new Vector3(0f, 0.34f, -0.01f), 70f, 0.5f, false);
                StormScatter(parent, areaHalf);
                break;
            case 53:
            case 54:
            case 55:
            case 56:
                BuildDepot(levelNumber, parent, areaHalf, itemPositions);
                break;
            case 57:
                CanyonDepot.Outside(parent, new Vector3(-0.6f, 0f, areaHalf.y + 2.0f));
                PaintMarks.Arrow(MarkedBoulder(parent, new Vector3(areaHalf.x - 0.6f, 0f, areaHalf.y + 0.9f), 0.9f), new Vector3(0f, 0.34f, -0.01f), 20f, 0.5f, false);
                ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf), SparrowState.Flying);
                break;
            case DatesLevel:
                DatedArrows(CanyonCliff.Wall(parent, areaHalf.y + 0.75f, areaHalf.x * 2f + 1.6f, 2.0f), areaHalf);
                ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf) + new Vector3(0f, 0.6f, -0.6f), SparrowState.Flying);
                break;
            case 59:
                PaintMarks.Arrow(MarkedBoulder(parent, new Vector3(-areaHalf.x + 0.6f, 0f, areaHalf.y + 0.9f), 0.9f), new Vector3(0f, 0.34f, -0.01f), 10f, 0.5f, false);
                ActiveSparrow = Sparrow.Create(parent, (targetPos ?? HoverSpot(areaHalf)) + Vector3.up * 1.0f, SparrowState.Flying);
                break;
            case LastLevel:
                // kanyonun kenari alanin arkasinda; asagi inen yol sagda (hedefin oldugu yanda)
                var surface = CanyonCliff.Rim(parent, new Vector3(0f, 0f, areaHalf.y + 0.6f), 0f, areaHalf.x * 2f, 1.5f);
                PaintMarks.Arrow(surface, Vector3.zero, -30f, 0.5f, false);
                ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf), SparrowState.Flying);
                break;
        }
    }

    // Serce (55'ten sonra yanimizda): bolumde yoksa null. 3b sahneleri kullanir.
    public static Sparrow ActiveSparrow { get; private set; }

    const int SparrowShelfLevel = 54, SparrowRepairLevel = 55, BoardLevel = 56, DatesLevel = 58;

    // Serce'nin bekledigi yer: alanin sag arka kosesinin ustu (sag ustteki pusula ve dugmelerin altina girmez)
    static Vector3 HoverSpot(Vector2 areaHalf) => new Vector3(areaHalf.x - 0.4f, 1.1f, areaHalf.y - 0.5f);

    // Depo ici (53-56): duvarlar, cati iskeleti, arka raf; 53'te parcalarin sirasi raf, 54-55'te Serce arka rafta,
    // 56'da gorev panosu. Her bolumde arka duvarda bir ok.
    static void BuildDepot(int levelNumber, Transform parent, Vector2 areaHalf, IReadOnlyList<Vector3> itemPositions)
    {
        var shelfTop = CanyonDepot.Inside(parent, areaHalf, wideTear: levelNumber == SparrowRepairLevel);
        if (levelNumber == DepotFirst && itemPositions.Count > 0)
            StorageShelf.Create(parent, new Vector3(0f, 0f, itemPositions[0].z), areaHalf.x * 2f, stocked: false);
        if (levelNumber == SparrowShelfLevel || levelNumber == SparrowRepairLevel)
            ActiveSparrow = Sparrow.Create(parent, shelfTop + Vector3.up * Sparrow.RestHeight,
                levelNumber == SparrowShelfLevel ? SparrowState.Incomplete : SparrowState.Dormant);
        if (levelNumber == BoardLevel)
        {
            TaskBoard.Create(parent, new Vector3(1.2f, CanyonDepot.WallHeight, CanyonDepot.BackWallZ(areaHalf) - 0.08f));
            ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf), SparrowState.Flying);
        }

        var wall = Parts.Empty("Yuzey", parent);
        wall.localPosition = new Vector3(levelNumber == BoardLevel ? -2.6f : 1.4f, 0.42f, CanyonDepot.BackWallZ(areaHalf) - 0.01f);
        PaintMarks.Arrow(wall, Vector3.zero, levelNumber % 2 == 0 ? 0f : 180f, 0.42f, false);
    }

    // Bolum 58: kanyon duvarinda soldan saga bes ok, altlarinda tarihler. Sona dogru oklar daha egri, boya akmis (acele).
    static void DatedArrows(Transform wall, Vector2 areaHalf)
    {
        const float PaintZ = CanyonCliff.FaceZ - 0.01f;
        string[] dates = { "sol 852", "sol 855", "sol 857", "sol 859", "sol 861" };
        float[] tilt = { 0f, -4f, 6f, -10f, 14f };
        for (int i = 0; i < dates.Length; i++)
        {
            float x = -areaHalf.x + 0.4f + i * (areaHalf.x * 2f - 0.8f) / (dates.Length - 1);
            PaintMarks.Arrow(wall, new Vector3(x, 1.05f, PaintZ), tilt[i], 0.8f, drips: i >= 3);
            PaintMarks.Date(wall, new Vector3(x, 0.5f, PaintZ - 0.005f), dates[i], i == dates.Length - 1 ? 0.32f : 0.24f);
        }
    }

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
    static void StormScatter(Transform parent, Vector2 areaHalf)
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
