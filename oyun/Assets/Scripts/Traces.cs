using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hikaye izleri: bazi bolumlerde alanin kenarinda duran kucuk sus nesneleri (docs/tasarim/senaryo-bolge-01.md, -02.md).
// Bulmacayi etkilemezler, yalnizca merak uyandirmak icin dururlar. Bolge 2'ninkiler PolarTraces'te, Bolge 3'unkuler CraterTraces'te,
// Bolge 4'unkuler DuneTraces'te, Bolge 5'inkiler AntennaTraces'te, Bolge 6'ninkiler CanyonTraces'te.
public static class Traces
{
    // Arka plandaki izler (MarsSky.hlsl, colonyTraces): kolonide acik kapi (Bolum 6), sera kubbesinde kuru saksi (Bolum 7).
    const int OpenDoorLevel = 6, GreenhouseLevel = 7;
    static readonly int DoorId = Shader.PropertyToID("_TraceDoor");
    static readonly int GreenhouseId = Shader.PropertyToID("_TraceGreenhouse");
    // Bolge 2: KT-2 araci alanin arkasina gelince ufuktaki uzak hali (PolarSky.hlsl) gizlenir.
    static readonly int RoverHereId = Shader.PropertyToID("_PolarRoverHere");

    // Bolge 2 sonunda seraya ilk yaprak acilir; kalici (PlayerPrefs). Sera ancak Bolum 7 bitince yandigi icin yaprak da orada gorunur.
    const string LeafKey = "sera_yaprak";
    static readonly int LeafId = Shader.PropertyToID("_TraceLeaf");

    static ResearchRover rover;
    static WaterTank tank;

    // Bolum 40 kapanisinin nesneleri (yon bulma diregi, kurdele, anten); baska bolumlerde null
    public static DuneFinale Dune { get; private set; }
    // Bolge 5'in dev anteni (Bolum 41-50); baska bolumlerde null
    public static BigAntenna Antenna { get; private set; }

    // targetPos: bolumde hedef kare varsa dunya konumu (10. bolumdeki telsiz diregi, 40. bolumdeki son isaret diregi icin).
    // itemPositions: toplanacak nesnelerin dunya konumlari (Bolum 53'teki raf icin).
    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Vector3? targetPos, IReadOnlyList<Vector3> itemPositions)
    {
        rover = PolarTraces.Build(levelNumber, parent, areaHalf);
        CraterTraces.Build(levelNumber, parent, areaHalf);
        Dune = DuneTraces.Build(levelNumber, parent, areaHalf, targetPos);
        Antenna = AntennaTraces.Build(levelNumber, parent, areaHalf);
        CanyonTraces.Build(levelNumber, parent, areaHalf, targetPos, itemPositions);
        tank = levelNumber == PolarTraces.TankLevel ? WaterTank.Create(parent, PolarTraces.TankPosition(areaHalf)) : null;
        ResetBackdrop(levelNumber);
        switch (levelNumber)
        {
            case 3: Helmet(parent, new Vector3(areaHalf.x + 0.26f, 0f, -areaHalf.y + 0.6f)); break;
            case 4: WheelTracks(parent, areaHalf); break;
            case 8: StormDebris(parent, new Vector3(-(areaHalf.x + 0.3f), 0f, areaHalf.y * 0.3f)); break;
            case 10: if (targetPos.HasValue) RadioMast(parent, targetPos.Value); break;
        }
    }

    // Bolum basi ya da yeniden deneme: kapi yalnizca kendi bolumunde acik, seranin isigi sonuk.
    public static void ResetBackdrop(int levelNumber)
    {
        Shader.SetGlobalFloat(DoorId, levelNumber == OpenDoorLevel ? 1f : 0f);
        Shader.SetGlobalFloat(GreenhouseId, 0f);   // 0 = sonuk (cizim tanimsiz degeri de 0 okur)
        Shader.SetGlobalFloat(RoverHereId, PolarTraces.HasRover(levelNumber) ? 1f : 0f);
        Shader.SetGlobalFloat(LeafId, PlayerPrefs.GetInt(LeafKey, 0));
        CanyonTraces.ResetBackdrop(levelNumber);
        if (rover != null) rover.Restore();
        if (tank != null) tank.Restore();
        if (Dune != null) Dune.Restore();
        if (Antenna != null) Antenna.Restore();
    }

    // Bolum 17: her toplanan buz tanki biraz daha doldurur
    public static void IceCollected(int collected, int total)
    {
        if (tank != null && total > 0) tank.Fill((float)collected / total);
    }

    // Bolge 2 finali: yaprak kalici olur
    public static void SeraYapragiAcildi()
    {
        PlayerPrefs.SetInt(LeafKey, 1);
        PlayerPrefs.Save();
    }

    // Bolum bitti: Bolum 7'de seraya enerji gelir, kubbenin isigi yanar ve camin ardinda kuru saksi gorunur.
    // Bolge 5'te anten bir sonraki asamaya kalkar (Bolum 50'de kapanis sahnesi tam guce getirir).
    public static void LevelDone(int levelNumber)
    {
        if (Antenna != null) Antenna.Advance(AntennaTraces.StageAfter(levelNumber));
        if (levelNumber == GreenhouseLevel) Shader.SetGlobalFloat(GreenhouseId, Time.timeSinceLevelLoad); // cizimdeki _Time.y ile ayni saat
        if (levelNumber == PolarTraces.RoverStandLevel && rover != null) rover.StandUp();
    }

    // Bolge 2 kapanisi: KT-2 koloniye dogru uzaklasir (bolumde arac yoksa hemen biter)
    public static IEnumerator RoverDriveAway()
    {
        if (rover != null) yield return rover.DriveAway();
    }

    // Bolum 10 (bolge finali): hedefteki telsiz diregi; tepesindeki isik yanip soner (kapanis sahnesinde anlam kazanir).
    static void RadioMast(Transform parent, Vector3 p)
    {
        var metal = Mats.Lit(Mats.Hex("#5A5660"), 0.4f);
        var mast = Parts.Empty("Telsiz", parent);
        mast.localPosition = p;
        Parts.Add("Direk", mast, MeshFactory.RoundedCylinder(0.025f, 0.5f, 0.008f, 10), metal, new Vector3(0f, 0.25f, 0f), outline: false);
        Parts.Add("Kol", mast, MeshFactory.RoundedBox(new Vector3(0.18f, 0.015f, 0.015f), 0.006f), metal, new Vector3(0f, 0.46f, 0f), outline: false);
        var lamp = Parts.Add("Isik", mast, MeshFactory.Sphere(0.028f, 8, 12), null, new Vector3(0f, 0.53f, 0f), outline: false, castShadow: false);
        Blinker.Attach(lamp, Mats.Hex("#FF4A3A"), 0.8f, 0f);
    }

    // Bolum 3: devrik kask. Yarim kure govde (yana devrilmis) + onundeki koyu siperlik seridi.
    static void Helmet(Transform parent, Vector3 p)
    {
        var shell = Mats.Lit(Mats.Hex("#C9C2B0"), 0.3f);
        var visor = Mats.Lit(Mats.Hex("#23222A"), 0.15f);
        var root = Parts.Empty("Iz-Kask", parent);
        root.localPosition = p;
        root.localRotation = Quaternion.Euler(78f, 35f, 0f); // yana devrilmis, acik agzi yana bakar
        var dome = Parts.Add("Govde", root, MeshFactory.Sphere(0.15f, 10, 18), shell, Vector3.zero);
        dome.localScale = new Vector3(1f, 1f, 0.78f);
        Parts.Add("Siperlik", root, MeshFactory.RoundedBox(new Vector3(0.22f, 0.05f, 0.08f), 0.02f), visor, new Vector3(0f, 0.02f, 0.13f));
    }

    // Bolum 4: koloniden (kuzey, +z) dagların oldugu yone uzanan cift tekerlek izi; donen iz yok.
    static void WheelTracks(Transform parent, Vector2 areaHalf)
    {
        var mat = Mats.Lit(Mats.Hex("#4A2A20"), 0.08f);
        float x = areaHalf.x + 0.3f, length = areaHalf.y * 2f + 3f, z = 1f;
        foreach (float ox in new[] { -0.09f, 0.09f })
            Parts.Add("Iz-Tekerlek", parent, MeshFactory.RoundedBox(new Vector3(0.07f, 0.012f, length), 0.02f), mat,
                new Vector3(x + ox, 0.006f, z), outline: false, castShadow: false);
    }

    // Bolge acilisi: kara ekranda gosterilecek satirlar (bolgenin ilk bolumu degilse null)
    public static string[] TransitionLines(int levelNumber) =>
        levelNumber == PolarTraces.FirstLevel ? PolarTraces.TransitionLines :
        levelNumber == CraterTraces.FirstLevel ? CraterTraces.TransitionLines :
        levelNumber == DuneTraces.FirstLevel ? DuneTraces.TransitionLines :
        levelNumber == AntennaTraces.FirstLevel ? AntennaTraces.TransitionLines :
        levelNumber == CanyonTraces.FirstLevel ? CanyonTraces.TransitionLines : null;

    // Bolum 8 (ve Bolum 21-22): firtinanin izi: devrilmis gunes paneli, kuma gomulu kablo, kayalarin yanina yigilmis kum.
    public static void StormDebris(Transform parent, Vector3 p)
    {
        var panelMat = Mats.Lit(Mats.Hex("#17232F"), 0.55f);
        var frameMat = Mats.Lit(Mats.Hex("#8A8A92"), 0.3f);
        var cableMat = Mats.Lit(Mats.Hex("#232323"), 0.2f);
        var sandMat = Mats.Lit(Mats.Hex("#8B5A42"), 0.15f);

        var panel = Parts.Empty("Iz-Panel", parent);
        panel.localPosition = p;
        panel.localRotation = Quaternion.Euler(58f, 20f, 8f); // devrilmis, yarisi kuma gommus
        Parts.Add("Panel", panel, MeshFactory.RoundedBox(new Vector3(0.5f, 0.02f, 0.32f), 0.015f), panelMat, Vector3.zero);
        Parts.Add("Cerceve", panel, MeshFactory.RoundedBox(new Vector3(0.52f, 0.015f, 0.04f), 0.01f), frameMat, new Vector3(0f, 0f, 0.15f));

        var cable = Parts.Add("Iz-Kablo", parent, MeshFactory.RoundedCylinder(0.018f, 0.7f, 0.006f, 10), cableMat,
            p + new Vector3(-0.1f, -0.008f, -0.35f), outline: false);
        cable.localRotation = Quaternion.Euler(0f, 24f, 90f);

        Parts.Add("Iz-Kum", parent, MeshFactory.RoundedBox(new Vector3(0.6f, 0.1f, 0.5f), 0.22f), sandMat,
            p + new Vector3(-0.25f, 0.02f, 0.35f), outline: false);
    }
}
