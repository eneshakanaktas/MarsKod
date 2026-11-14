using System;
using System.Collections.Generic;
using UnityEngine;

// Bolge 8 · Terk edilmis arastirma istasyonu (Bolum 71-80, return + metin): docs/tasarim/senaryo-bolge-08.md, gorunus:
// docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md. Bolge acilisi, manzara secimi ve bolum bolum nesneler:
// 71 kanyonun ucunda istasyonun kapisi (disari), 72-80 kapali ic mekan (StationRoom). Alan disindaki her nesne zemine oturur.
public static class StationTraces
{
    public const int FirstLevel = 71, LastLevel = 80, DoorLevel = 71;
    public const int LightsOnLevel = 73;   // 73 bitince tavan lambalari yanar; 74-80 aydinlik baslar
    public const int ScreensLevel = 72, DrawingsFirstLevel = 73, DrawingsSecondLevel = 75, FoldedLevel = 76,
        ListLevel = 77, ListEndLevel = 78, WorkshopLevel = 79, MapLevel = 80;

    const float CrestSize = 0.62f;   // armanin capi (kapi modulunun catisi 1,5 x 1,2)

    static readonly int SceneId = Shader.PropertyToID("_StationScene");
    static readonly int FlatFloorId = Shader.PropertyToID("_FlatFloor");

    // Bolge 8 acilisi: kara ekranda iki satir (listeyi onceden soylemez)
    public static readonly string[] TransitionLines = { "Yeni bölge: Araştırma istasyonu", "Hedef: koloninin kayıtları" };

    static bool InRegion(int levelNumber) => levelNumber >= FirstLevel && levelNumber <= LastLevel;

    // 72-80: istasyonun ici (kapali oda, gokyuzu yok, zemin metal)
    public static bool IsIndoor(int levelNumber) => InRegion(levelNumber) && levelNumber != DoorLevel;

    // 72-80: odanin kabugu (arka duvarin on yuzu: cizimler, harita, bilgisayarlar buraya); baska bolumlerde null
    public static StationRoom.Room Room { get; private set; }
    public static Transform BackWallFace => Room?.BackWallFace;
    // 71: armanin yeri (kapinin ustu); baska bolumlerde null
    public static Transform CrestSpot { get; private set; }
    // 76: Ece ile Defne'nin yataklari (3b sahnesi icin); baska bolumlerde null
    public static Transform Beds { get; private set; }
    // 73, 75: Ece'nin duvar cizimlerinin ortalari (3b kapanisi icin); baska bolumlerde null
    public static IReadOnlyList<Vector3> Drawings { get; private set; }
    // 71: koloninin armasi (denetim icin); baska bolumlerde null
    public static Transform Crest { get; private set; }
    // 80: duvar haritasi (3b kapanisi icin); baska bolumlerde null
    public static Transform WallMap { get; private set; }
    // 79: Kor'un kirik bacagi (denetim icin); baska bolumlerde null
    public static Transform KorLeg { get; private set; }
    // 72, 77, 78: ekran bulmacasinin sahnedeki bilgisayarlari; baska bolumlerde null
    public static StationScreens Screens { get; private set; }

    // Arka plana ve zemine manzara bilgisi (StationSky.hlsl, Ground.shader); Bolge 8 disinda 0. Lambalar bolumun basindaki haline doner.
    public static void ResetBackdrop(int levelNumber)
    {
        bool indoor = IsIndoor(levelNumber);
        Shader.SetGlobalFloat(SceneId, indoor ? 1f : 0f);
        Shader.SetGlobalFloat(FlatFloorId, indoor ? 1f : 0f);
        Room?.SetLights(LightsOnAtStart(levelNumber));
    }

    static bool LightsOnAtStart(int levelNumber) => levelNumber > LightsOnLevel;

    // Bolumun nesnelerini kurar (Bolge 8 disinda hicbir sey). ground: zemin yuksekligi (x, z).
    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        Room = null;
        CrestSpot = null;
        Beds = null;
        WallMap = null;
        KorLeg = null;
        Screens = null;
        Drawings = null;
        Crest = null;
        if (!InRegion(levelNumber)) return;
        if (levelNumber == DoorLevel)
        {
            CrestSpot = StationFacade.Build(parent, areaHalf, ground);
            Crest = ColonyCrest.Paint(CrestSpot, Vector3.zero, CrestSize, 871);
            return;
        }
        // yatak, tezgah ve masalarin altina kum/kagit girmesin
        bool backClutter = levelNumber != FoldedLevel && levelNumber != WorkshopLevel && levelNumber != ScreensLevel;
        Room = StationRoom.Build(parent, areaHalf, LightsOnAtStart(levelNumber), backClutter, 800 + levelNumber, ground);
        float wallZ = areaHalf.y + StationRoom.BackGap;
        switch (levelNumber)
        {
            case FoldedLevel: Beds = StationProps.Beds(parent, wallZ, ground); break;
            case WorkshopLevel:
                StationProps.Workshop(parent, Room.BackWallFace, wallZ, ground);
                KorLeg = StationProps.KorLeg(parent, new Vector3(1.85f, 0f, wallZ - 0.4f), -10f, ground);
                break;
            case MapLevel: WallMap = StationMap.Paint(Room.BackWallFace, new Vector2(0f, 0.62f), 2.2f, 800); break;
            case DrawingsFirstLevel: Drawings = WallDrawings.Paint(Room.BackWallFace, 3, 873); break;
            case DrawingsSecondLevel: Drawings = WallDrawings.Paint(Room.BackWallFace, 5, 873); break;   // ayni duvar: ilk uc ayni
            case ScreensLevel: Screens = StationScreens.Create(Room.BackWallFace, 6, highlightTrue: false, 872, ground); break;
            case ListLevel: Screens = StationScreens.Create(Room.BackWallFace, 1, highlightTrue: false, 877, ground); break;
            case ListEndLevel: Screens = StationScreens.Create(Room.BackWallFace, 1, highlightTrue: true, 878, ground); break;   // BKM-7
        }
    }

    // 73 bitti: onarilan guc kutulariyla tavan lambalari kirpisarak yanar (Tekrar'da ResetBackdrop losa dondurur)
    public static void LightsOn(int levelNumber, Light sky, Light sun)
    {
        if (levelNumber != LightsOnLevel || Room == null) return;
        Room.TurnOn(sky, sun, RegionLook.ForLevel(levelNumber), RegionLook.StationLightsOn);
    }
}
