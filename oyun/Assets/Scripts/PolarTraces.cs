using UnityEngine;

// Bolge 2 (kutup buzulu, Bolum 11-20) hikaye izleri: docs/tasarim/senaryo-bolge-02.md.
// Hepsi alanin arkasinda (kuzey kenarin otesinde) durur; bulmacayi etkilemez.
public static class PolarTraces
{
    // KT-2 araci Bolum 14'ten itibaren alanin arkasinda; o zamana kadar yalnizca ufukta (PolarSky.hlsl) gorunur.
    public const int FirstLevel = 11, TankLevel = 17, RoverFirstLevel = 14, RoverDoorLevel = 18, RoverStandLevel = 19, LastLevel = 20;

    // Bolge 2 acilisi: kara ekranda iki satir (docs/tasarim/senaryo-bolge-02.md, Bolum 11 basi)
    public static readonly string[] TransitionLines = { "Yeni bölge: Kutup buzulu", "Hedef: su" };

    // Su tanki: alanin arkasinda, sol tarafta
    public static Vector3 TankPosition(Vector2 areaHalf) => new Vector3(-1.0f, 0f, areaHalf.y + 0.7f);

    public static bool HasRover(int levelNumber) => levelNumber >= RoverFirstLevel && levelNumber <= LastLevel;

    // Bolumun izlerini kurar; arac varsa onu dondurur.
    public static ResearchRover Build(int levelNumber, Transform parent, Vector2 areaHalf)
    {
        float back = areaHalf.y + 0.55f;   // alanin arka kenarinin hemen otesi
        var roverPos = new Vector3(1.6f, 0f, areaHalf.y + 0.8f);
        switch (levelNumber)
        {
            case 11: BlownTarp(parent, new Vector3(-1.6f, 0f, back)); break;
            case 12: PanelCover(parent, new Vector3(0.9f, 0f, back)); break;
            case 14: Footprints(parent, roverPos); break;
            case 15: Footprints(parent, roverPos); WheelTracks(parent, FootprintsEnd(roverPos)); break;
            case 16: IceAxe(parent, new Vector3(-1.3f, 0f, back)); break;
            case 18: Thermos(parent, roverPos + new Vector3(-0.3f, 0f, -0.55f)); break;
        }
        if (!HasRover(levelNumber)) return null;
        return ResearchRover.Create(parent, roverPos, upright: levelNumber > RoverStandLevel, doorOpen: levelNumber >= RoverDoorLevel);
    }

    static Material IceMark => Mats.Lit(Mats.Hex("#6E7C99"), 0.1f);

    // Bolum 11: firtinanin izi: bir yana yigilmis kar ve savrulmus turuncu branda
    static void BlownTarp(Transform parent, Vector3 p)
    {
        Parts.Add("Iz-Kar", parent, MeshFactory.RoundedBox(new Vector3(0.9f, 0.16f, 0.45f), 0.14f), Mats.Lit(Mats.Hex("#C9D3E6"), 0.25f), p, outline: false);
        var tarp = Parts.Add("Iz-Branda", parent, MeshFactory.RoundedBox(new Vector3(0.5f, 0.015f, 0.34f), 0.01f), Mats.Lit(Mats.Hex("#D9732F"), 0.2f),
            p + new Vector3(0.35f, 0.12f, -0.05f));
        tarp.localRotation = Quaternion.Euler(18f, 30f, -24f);   // yariya kadar kara gomulmus
    }

    // Bolum 12: aractan sokulmus panel kapagi (sensor oradan alindi): gri plaka, iki vida yuvasi
    static void PanelCover(Transform parent, Vector3 p)
    {
        var plate = Parts.Empty("Iz-PanelKapagi", parent);
        plate.localPosition = p;
        plate.localRotation = Quaternion.Euler(6f, -25f, 4f);
        Parts.Add("Plaka", plate, MeshFactory.RoundedBox(new Vector3(0.34f, 0.02f, 0.24f), 0.02f), Mats.Lit(Mats.Hex("#9EA3AE"), 0.5f), new Vector3(0f, 0.02f, 0f));
        var screw = Mats.Lit(Mats.Hex("#3A3C44"), 0.3f);
        foreach (float x in new[] { -0.13f, 0.13f })
            Parts.Add("Vida", plate, MeshFactory.RoundedCylinder(0.015f, 0.01f, 0.003f, 8), screw, new Vector3(x, 0.035f, 0.08f), outline: false);
    }

    // Bolum 14-15: aracin yanindan baslayan iki kisilik ayak izi (bati yonune, birkac adim)
    const float FootprintsLength = 1.3f;
    static Vector3 FootprintsStart(Vector3 roverPos) => new Vector3(roverPos.x - 0.55f, 0f, roverPos.z - 0.41f);
    static Vector3 FootprintsEnd(Vector3 roverPos) => FootprintsStart(roverPos) - new Vector3(FootprintsLength, 0f, 0f);

    static void Footprints(Transform parent, Vector3 roverPos)
    {
        var mat = IceMark;
        var mesh = MeshFactory.RoundedBox(new Vector3(0.07f, 0.008f, 0.04f), 0.015f);
        var start = FootprintsStart(roverPos);
        // iki insan yan yana; adimlari birbirinden biraz farkli
        foreach (var (oz, step) in new[] { (0.09f, 0.17f), (-0.09f, 0.2f) })
            for (float d = 0f, side = 1f; d < FootprintsLength; d += step, side = -side)
                Parts.Add("Iz-Ayak", parent, mesh, mat, new Vector3(start.x - d, 0.005f, start.z + oz + side * 0.035f), outline: false, castShadow: false);
    }

    // Bolum 15: ayak izlerinin bittigi yerde baslayan, batiya uzaklasan genis tekerlek izi (biri gelip onlari almis)
    static void WheelTracks(Transform parent, Vector3 start)
    {
        var mat = IceMark;
        const float length = 6f;   // alanin bati kenarinin otesine
        float startX = start.x, z = start.z;
        foreach (float oz in new[] { -0.17f, 0.17f })
            Parts.Add("Iz-Tekerlek", parent, MeshFactory.RoundedBox(new Vector3(length, 0.01f, 0.1f), 0.02f), mat,
                new Vector3(startX - length * 0.5f, 0.005f, z + oz), outline: false, castShadow: false);
    }

    // Bolum 16: kara saplanmis buz kazmasi; sapinda kucuk bir bayrak
    static void IceAxe(Transform parent, Vector3 p)
    {
        var root = Parts.Empty("Iz-BuzKazmasi", parent);
        root.localPosition = p;
        root.localRotation = Quaternion.Euler(0f, 20f, -12f);
        root.localScale = Vector3.one * 1.7f;   // uzaktan da secilsin
        var metal = Mats.Lit(Mats.Hex("#5C6070"), 0.5f);
        Parts.Add("Sap", root, MeshFactory.RoundedCylinder(0.018f, 0.42f, 0.006f, 8), Mats.Lit(Mats.Hex("#2E3038"), 0.3f), new Vector3(0f, 0.21f, 0f));
        Parts.Add("Bas", root, MeshFactory.RoundedBox(new Vector3(0.2f, 0.035f, 0.03f), 0.012f), metal, new Vector3(0.02f, 0.42f, 0f));
        Parts.Add("Bayrak", root, MeshFactory.RoundedBox(new Vector3(0.13f, 0.08f, 0.008f), 0.004f), Mats.Lit(Mats.Hex("#E07B3C"), 0.2f),
            new Vector3(0.075f, 0.32f, 0f), outline: false);
    }

    // Bolum 18: acik kapinin onunde, karda duran bir termos (sakince inmisler)
    static void Thermos(Transform parent, Vector3 p)
    {
        var t = Parts.Add("Iz-Termos", parent, MeshFactory.RoundedCylinder(0.035f, 0.13f, 0.012f, 12), Mats.Lit(Mats.Hex("#3F7FA8"), 0.5f),
            p + new Vector3(0f, 0.065f, 0f));
        Parts.Add("Kapak", t, MeshFactory.RoundedCylinder(0.037f, 0.035f, 0.01f, 12), Mats.Lit(Mats.Hex("#2A2C33"), 0.3f), new Vector3(0f, 0.08f, 0f), outline: false);
    }
}
