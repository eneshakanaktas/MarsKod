using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hikaye izleri: bazi bolumlerde alanin kenarinda duran kucuk sus nesneleri (docs/tasarim/senaryo-bolge-01.md, -02.md).
// Bulmacayi etkilemezler, yalnizca merak uyandirmak icin dururlar. Bolge 2'ninkiler PolarTraces'te, Bolge 3'unkuler CraterTraces'te,
// Bolge 4'unkuler DuneTraces'te, Bolge 5'inkiler AntennaTraces'te, Bolge 6'ninkiler CanyonTraces'te, Bolge 7'ninkiler RiverBedTraces'te, Bolge 8'inkiler StationTraces'te.
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
    // itemPositions: toplanacak nesnelerin dunya konumlari (Bolum 53'teki raf icin). groundHeight: zemin yuksekligi (x, z).
    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Vector3? targetPos, IReadOnlyList<Vector3> itemPositions,
        Func<float, float, float> groundHeight)
    {
        rover = PolarTraces.Build(levelNumber, parent, areaHalf);
        CraterTraces.Build(levelNumber, parent, areaHalf, groundHeight);
        Dune = DuneTraces.Build(levelNumber, parent, areaHalf, targetPos);
        Antenna = AntennaTraces.Build(levelNumber, parent, areaHalf);
        CanyonTraces.Build(levelNumber, parent, areaHalf, targetPos, itemPositions, groundHeight);
        RiverBedTraces.Build(levelNumber, parent, areaHalf, groundHeight);
        StationTraces.Build(levelNumber, parent, areaHalf, groundHeight);
        tank = levelNumber == PolarTraces.TankLevel ? WaterTank.Create(parent, PolarTraces.TankPosition(areaHalf)) : null;
        ResetBackdrop(levelNumber);
        switch (levelNumber)
        {
            case 3: Helmet(parent, new Vector3(areaHalf.x + 0.26f, 0f, -areaHalf.y + 0.6f), groundHeight); break;
            case 4: WheelTracks(parent, areaHalf, groundHeight); break;
            case 8: StormDebris(parent, new Vector3(-(areaHalf.x + 0.3f), 0f, areaHalf.y * 0.3f), groundHeight); break;
            case 10: if (targetPos.HasValue) RadioMast(parent, targetPos.Value, groundHeight); break;
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
        RiverBedTraces.ResetBackdrop(levelNumber);
        StationTraces.ResetBackdrop(levelNumber);
        SaltLakeTraces.ResetBackdrop(levelNumber);
        NightPlainTraces.ResetBackdrop(levelNumber);
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

    // Bolge 2 kapanisi atlandi: arac uzakta
    public static void RoverDrivenAway() => rover?.DrivenAway();

    // Bolum 10 (bolge finali): hedefteki telsiz diregi; tepesindeki isik yanip soner (kapanis sahnesinde anlam kazanir).
    // Bolum 10: koloninin telsiz diregi. Hedef kare "telsiz direginin dibi" (senaryo-bolge-01.md): direk hedefin hemen arkasinda
    // (kuzey, alanin kenarinin biraz disinda) durur ve robottan uzundur: kapanistaki SOS isigi robotun basinin ustunde gorunur.
    // (Once hedefin tam ortasindaydi: robot diregin icine giriyor, kapanistaki isik robotun govdesinde kaliyordu.)
    const float MastBehindTarget = 0.62f, MastHeight = 1.1f;

    static void RadioMast(Transform parent, Vector3 target, Func<float, float, float> groundHeight)
    {
        var metal = Mats.Lit(Mats.Hex("#5A5660"), 0.4f);
        var mast = Parts.Empty("Iz-Telsiz", parent);
        mast.localPosition = target + Vector3.forward * MastBehindTarget;
        Parts.Add("Taban", mast, MeshFactory.RoundedCylinder(0.07f, 0.04f, 0.012f, 16), metal, new Vector3(0f, 0.02f, 0f));
        Parts.Add("Direk", mast, MeshFactory.RoundedCylinder(0.025f, MastHeight, 0.008f, 10), metal, new Vector3(0f, MastHeight * 0.5f, 0f));
        Parts.Add("Kol", mast, MeshFactory.RoundedBox(new Vector3(0.22f, 0.015f, 0.015f), 0.006f), metal, new Vector3(0f, MastHeight - 0.05f, 0f), outline: false);
        var lamp = Parts.Add("Isik", mast, MeshFactory.Sphere(0.032f, 8, 12), null, new Vector3(0f, MastHeight + 0.03f, 0f), outline: false, castShadow: false);
        Blinker.Attach(lamp, Mats.Hex("#FF4A3A"), 0.8f, 0f);
        GroundPlacement.SitOnGround(mast, groundHeight, 0.01f);
    }

    // Bolum 3: devrik kask. Yarim kure govde (yana devrilmis) + onundeki koyu siperlik seridi.
    // Bolum 3: yere dusmus kask. Devrik yatar, boyun halkasi alandan disari, cam yuzu yukari ve kameraya doner (oyun acisindan
    // kask oldugu anlasilsin; once kure + kucuk siperlikti, ustten duz gri top gibi gorunuyordu). Zemine oturur (yarisi gomuluydu).
    static void Helmet(Transform parent, Vector3 p, Func<float, float, float> groundHeight)
    {
        var shell = Mats.Lit(Mats.Hex("#C9C2B0"), 0.3f);
        var glass = Mats.Lit(Mats.Hex("#1E1D26"), 0.9f);
        var ring = Mats.Lit(Mats.Hex("#5A5862"), 0.5f);
        var root = Parts.Empty("Iz-Kask", parent);
        root.localPosition = p;
        // dik kask: boyun -y, cam yuzu +z. Yana devrilir (boyun alanin disina, +x), cam yuzu yukari kalkar ve kameraya (-z) doner.
        root.localRotation = Quaternion.AngleAxis(200f, Vector3.up) * Quaternion.AngleAxis(-40f, Vector3.right) * Quaternion.AngleAxis(-80f, Vector3.forward);
        // iki kurenin kesistigi cam kenari puruzsuz olsun diye sik bolunmus
        Parts.Add("Govde", root, MeshFactory.Sphere(0.15f, 24, 40), shell, Vector3.zero);
        // cam kabuktan belirgin one cikar: iki yuzey neredeyse ust uste kesisince kenar titriyordu
        var visor = Parts.Add("Cam", root, MeshFactory.Sphere(0.15f, 24, 40), glass, new Vector3(0f, 0.012f, 0.085f), outline: false);
        visor.localScale = new Vector3(0.8f, 0.64f, 0.6f);
        Parts.Add("BoyunHalkasi", root, MeshFactory.RoundedCylinder(0.11f, 0.04f, 0.012f), ring, new Vector3(0f, -0.13f, 0f));
        GroundPlacement.SitOnGround(root, groundHeight, 0.02f);
    }

    // Bolum 4: koloniden cikan arac izleri; hepsi ayni yone, ufuktaki daglara gider (senaryo-bolge-01.md). Izler zemini izler ve
    // uzaklastikca zeminle ayni kuralla arka plana karisir (once duz cubuktular: zemin kivrilinca kalkip gokyuzunun ustune ciziliyordu).
    const float TrackStartZ = -6f, TrackEndZ = 21f, TrackStep = 0.3f, TrackGauge = 0.18f, TrackWidth = 0.07f, TrackLift = 0.01f;

    static void WheelTracks(Transform parent, Vector2 areaHalf, Func<float, float, float> groundHeight)
    {
        var mat = Oyun.FarRockMat("#4A2A20");
        foreach (var (x0, phase) in new[] { (areaHalf.x + 0.35f, 0f), (areaHalf.x + 1.15f, 1.7f), (areaHalf.x + 2.0f, 3.1f) })
        foreach (float wheel in new[] { -0.5f, 0.5f })
        {
            var path = new List<Vector2>();
            for (float z = TrackStartZ; z <= TrackEndZ; z += TrackStep)
                path.Add(new Vector2(x0 + wheel * TrackGauge + 0.2f * Mathf.Sin(z * 0.17f + phase), z));
            Parts.Add("Iz-Tekerlek", parent, GroundPlacement.Ribbon(path, TrackWidth, groundHeight, TrackLift), mat, Vector3.zero,
                outline: false, castShadow: false);
        }
    }

    // Bolge acilisi: kara ekranda gosterilecek satirlar (bolgenin ilk bolumu degilse null)
    public static string[] TransitionLines(int levelNumber) =>
        levelNumber == PolarTraces.FirstLevel ? PolarTraces.TransitionLines :
        levelNumber == CraterTraces.FirstLevel ? CraterTraces.TransitionLines :
        levelNumber == DuneTraces.FirstLevel ? DuneTraces.TransitionLines :
        levelNumber == AntennaTraces.FirstLevel ? AntennaTraces.TransitionLines :
        levelNumber == CanyonTraces.FirstLevel ? CanyonTraces.TransitionLines :
        levelNumber == RiverBedTraces.FirstLevel ? RiverBedTraces.TransitionLines :
        levelNumber == StationTraces.FirstLevel ? StationTraces.TransitionLines :
        levelNumber == SaltLakeTraces.FirstLevel ? SaltLakeTraces.TransitionLines :
        levelNumber == NightPlainTraces.FirstLevel ? NightPlainTraces.TransitionLines : null;

    // Bolum 8 (ve Bolum 21-22): firtinanin izi: devrilmis gunes paneli, kuma gomulu kablo, kayalarin yanina yigilmis kum.
    // Panel kameraya donuk devrik durur (yuzundeki hucre cizgileri ve cerceve okunsun), alt kenari kuma gomulu; kum yumusak tumsek;
    // kablo kivrilarak alana dogru uzanir, yer yer kuma girip cikar. Hepsi zemine oturur. (Once panelin dortte ucu topragin
    // altindaydi, kum yuvarlatilmis bir kutu, kablo duz bir cubuktu: oyun acisindan pembe-gri bir kare gibi gorunuyordu.)
    const float PanelWidth = 0.5f, PanelDepth = 0.32f, PanelTilt = 32f, PanelYaw = 25f, PanelSink = 0.04f;
    const float CableLength = 0.75f, CableRadius = 0.016f;
    const int CableSegments = 9;

    public static void StormDebris(Transform parent, Vector3 p, Func<float, float, float> groundHeight)
    {
        DebrisPanel(parent, p, Mats.Lit(Mats.Hex("#1B2C3E"), 0.75f), Mats.Lit(Mats.Hex("#9A9AA2"), 0.35f), groundHeight);
        DebrisSand(parent, p, Mats.Lit(Mats.Hex("#8A5440"), 0.05f), groundHeight);
        DebrisCable(parent, p, Mats.Lit(Mats.Hex("#2A2A2E"), 0.25f), groundHeight);
    }

    static void DebrisPanel(Transform parent, Vector3 p, Material face, Material frame, Func<float, float, float> groundHeight)
    {
        var panel = Parts.Empty("Iz-Panel", parent);
        panel.localPosition = p;
        // yuzu yukari ve kameraya (guneye) donuk devrik: guney kenari asagida, kuma gomulu
        panel.localRotation = Quaternion.AngleAxis(PanelYaw, Vector3.up) * Quaternion.AngleAxis(-PanelTilt, Vector3.right);
        float w = PanelWidth, d = PanelDepth;
        Parts.Add("Yuz", panel, MeshFactory.RoundedBox(new Vector3(w, 0.02f, d), 0.008f), face, Vector3.zero);
        foreach (float z in new[] { -d * 0.5f, d * 0.5f })
            Parts.Add("Cerceve", panel, MeshFactory.RoundedBox(new Vector3(w + 0.02f, 0.028f, 0.02f), 0.008f), frame, new Vector3(0f, 0.004f, z), outline: false);
        foreach (float x in new[] { -w * 0.5f, w * 0.5f })
            Parts.Add("Cerceve", panel, MeshFactory.RoundedBox(new Vector3(0.02f, 0.028f, d), 0.008f), frame, new Vector3(x, 0.004f, 0f), outline: false);
        // hucre cizgileri: panel oldugu buradan anlasilir
        foreach (float x in new[] { -w / 6f, w / 6f })
            Parts.Add("Cizgi", panel, MeshFactory.RoundedBox(new Vector3(0.008f, 0.024f, d), 0.003f), frame, new Vector3(x, 0.002f, 0f), outline: false);
        Parts.Add("Cizgi", panel, MeshFactory.RoundedBox(new Vector3(w, 0.024f, 0.008f), 0.003f), frame, new Vector3(0f, 0.002f, 0f), outline: false);
        GroundPlacement.SitOnGround(panel, groundHeight, PanelSink);
    }

    // Panelin gomulu (guney) kenarina yigilmis kum: basik, zemin renginde iki tumsek (tek duzgun tumsek yastik gibi duruyordu)
    static readonly (Vector3 offset, Vector3 scale)[] SandDrifts =
    {
        (new Vector3(-0.06f, 0f, -PanelDepth * 0.5f), new Vector3(1.9f, 0.26f, 0.75f)),
        (new Vector3(0.17f, 0f, -PanelDepth * 0.5f - 0.05f), new Vector3(1.1f, 0.2f, 0.6f)),
    };

    static void DebrisSand(Transform parent, Vector3 p, Material sand, Func<float, float, float> groundHeight)
    {
        var turn = Quaternion.AngleAxis(PanelYaw, Vector3.up);
        var drifts = Parts.Empty("Iz-Kum", parent);
        foreach (var (offset, scale) in SandDrifts)
        {
            var mound = Parts.Add("Tumsek", drifts, MeshFactory.Sphere(0.15f, 10, 18), sand, p + turn * offset, outline: false);
            mound.localRotation = turn;
            mound.localScale = scale;
            GroundPlacement.SitOnGround(mound, groundHeight, 0.025f);
        }
    }

    // Panelden alana dogru kivrilarak uzanan kablo; yer yer kuma girer, yer yer ustunde gorunur
    static void DebrisCable(Transform parent, Vector3 p, Material mat, Func<float, float, float> groundHeight)
    {
        var cable = Parts.Empty("Iz-Kablo", parent);
        var toArea = new Vector3(-p.x, 0f, -p.z).normalized;
        var side = new Vector3(toArea.z, 0f, -toArea.x);
        var start = p + toArea * (PanelWidth * 0.4f);
        var segment = MeshFactory.RoundedCylinder(CableRadius, 1f, CableRadius * 0.9f, 10);
        Vector3 Point(int i)
        {
            float t = (float)i / CableSegments;
            var q = start + toArea * (t * CableLength) + side * (0.08f * Mathf.Sin(t * Mathf.PI * 2f));
            q.y = groundHeight(q.x, q.z) + 0.006f + 0.014f * Mathf.Sin(t * Mathf.PI * 3f);
            return q;
        }
        for (int i = 0; i < CableSegments; i++)
        {
            Vector3 a = Point(i), b = Point(i + 1);
            var piece = Parts.Add("Parca", cable, segment, mat, (a + b) * 0.5f, outline: false);
            piece.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
            piece.localScale = new Vector3(1f, Vector3.Distance(a, b) + CableRadius, 1f);
        }
    }
}
