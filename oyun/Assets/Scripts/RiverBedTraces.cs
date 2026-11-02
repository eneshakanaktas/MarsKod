using System;
using UnityEngine;

// Bolge 7 · Kanyon dibi, eski nehir yatagi (Bolum 61-70, parametreli fonksiyonlar): docs/tasarim/senaryo-bolge-07.md, gorunus:
// tasarim belgesi 3a (docs/superpowers/specs/2026-11-02-bolge-7-design.md). Bolge acilisi, manzara secimi ve bolum bolum
// nesneler: 61-62 yarik patikasinin dibi, 63-64 nehir kivrimi, 65-66 yan kanyonun agzi (sag arkada), 67-70 yan kanyonun
// icinde yakin arka duvar. Alan disindaki her nesne zeminin gercek yuksekligine oturur.
public static class RiverBedTraces
{
    public const int FirstLevel = 61, LastLevel = 70;
    public const int CrayonLevel = 68, HandsLevel = 69, MuralLevel = 70;
    public const int HandsSmall = 7, HandsBig = 1;   // 69 damgasi "El izi: 7 kucuk, 1 buyuk"
    static readonly int SceneId = Shader.PropertyToID("_RiverScene");

    // Bolge 7 acilisi: kara ekranda iki satir
    public static readonly string[] TransitionLines = { "Yeni bölge: Kanyon dibi", "Hedef: göç izleri" };

    // Arka planin ve zeminin manzarasi (RiverSky.hlsl, Ground.shader _RiverScene)
    enum Scene { Descent = 0, Meander = 1, SideMouth = 2, BackWall = 3 }

    static bool InRegion(int levelNumber) => levelNumber >= FirstLevel && levelNumber <= LastLevel;

    static Scene SceneOf(int levelNumber) =>
        levelNumber <= 62 ? Scene.Descent : levelNumber <= 64 ? Scene.Meander : levelNumber <= 66 ? Scene.SideMouth : Scene.BackWall;

    // 68: yerdeki kalem (3b sahnesi alir); 70: duvar resminin yerleri; baska bolumlerde null
    public static Transform Crayon { get; private set; }
    public static WallMural.Spots? Mural { get; private set; }

    // 63-64: goc izi (3b kus bakisi ve sayi denetimi icin); baska bolumlerde null
    public static Transform Migration { get; private set; }

    // 67-70: arka duvarin on yuzu (el izleri ve duvar resmi buraya); baska bolumlerde null
    public static Transform BackWallFace { get; private set; }

    // Serce: Bolge 7'nin her bolumunde yukarida, gunesli seritte; bolum disinda null. scout() aninda TrailView kullanir.
    public static Sparrow ActiveSparrow { get; private set; }

    // Zemine ve arka plana manzara bilgisi; Bolge 7 disinda 0. Serce yerine doner (scout() onu goturmus olabilir).
    public static void ResetBackdrop(int levelNumber)
    {
        Shader.SetGlobalFloat(SceneId, InRegion(levelNumber) ? (float)SceneOf(levelNumber) : 0f);
        if (ActiveSparrow != null) ActiveSparrow.ResetHome();
    }

    // Oynanan izde kimin izleri var (docs: plan "Hangi izi takip ediyoruz"): 62 goc izinin kenari (buyuk + kucuk), 65 yalniz
    // kucuk (Ece ayriliyor), 67 kucuk zipliyor + buyuk kosuyor, 68-70 kucuk + buyuk kosan.
    public static TrailLook LookFor(int levelNumber)
    {
        if (levelNumber == 65) return new TrailLook(true, false, Gait.Walk, Gait.Walk);
        if (levelNumber == 67) return new TrailLook(true, true, Gait.Hop, Gait.Run);
        if (levelNumber >= 68) return new TrailLook(true, true, Gait.Walk, Gait.Run);
        return new TrailLook(true, true, Gait.Walk, Gait.Walk);
    }

    // Bolumun nesnelerini kurar (Bolge 7 disinda hicbir sey). ground: zemin yuksekligi (x, z).
    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        BackWallFace = null;
        ActiveSparrow = null;
        Migration = null;
        Crayon = null;
        Mural = null;
        if (!InRegion(levelNumber)) return;
        ActiveSparrow = Sparrow.Create(parent, SparrowSpot(levelNumber, areaHalf, ground), SparrowState.Flying);
        ActiveSparrow.SetSunlit(true);
        switch (SceneOf(levelNumber))
        {
            case Scene.SideMouth: SideCanyonMouth(parent, areaHalf, ground); break;
            case Scene.BackWall: BackWallFace = SideCanyon(parent, areaHalf, ground); break;
        }
        Migration = RiverStoryTracks.Build(levelNumber, parent, areaHalf, ground);
        if (levelNumber == CrayonLevel) Crayon = GroundCrayon(parent, areaHalf, ground);
        if (levelNumber == HandsLevel) HandRow(BackWallFace);
        if (levelNumber == MuralLevel) Mural = WallMural.Paint(BackWallFace, 700);
    }

    // 65-66: yan kanyonun agzi sag arkada (izin gittigi yon, dogu), arka planda (RiverSky.hlsl RIVER_MOUTH). Agzin iki yaninda
    // duvardan kopmus alcak, golgedeki kaya cikintilari goz agza gitsin diye onu cerceveler.
    static void SideCanyonMouth(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        RiverWall.Build(parent, new Vector3(0.4f, 0f, areaHalf.y + 1.4f), 2.2f, 0.6f, 6f, false, 651, ground);
        RiverWall.Build(parent, new Vector3(areaHalf.x + 1.1f, 0f, areaHalf.y + 0.8f), 2.2f, 0.75f, -40f, false, 652, ground);
    }

    // 67-70: yan kanyonun ici. Alanin arkasinda tek duvar; iki yanda ekranin kenarinda kesilen alcak kanatlar (dar, kivrimli kanyon).
    // Doner: arka duvarin on yuzu.
    static Transform SideCanyon(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        var face = RiverWall.Build(parent, new Vector3(0f, 0f, areaHalf.y + BackWallGap), areaHalf.x * 2f + 2f, BackWallHeight, 0f, true, 670, ground);
        foreach (float side in new[] { -1f, 1f })
            RiverWall.Build(parent, new Vector3(side * (areaHalf.x + 1.7f), 0f, areaHalf.y - 0.4f), 3.2f, 0.7f, side * 68f, false, 671 + (int)side, ground);
        return face;
    }

    // 68: Ece'nin kalemi yerde, alanin solundan gecen donus izinin hemen yaninda (izlerin ustunde degil)
    static Transform GroundCrayon(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        float x = -(areaHalf.x + 0.22f), z = 0.4f;
        var crayon = global::Crayon.Create(parent, new Vector3(x, ground(x, z) + CrayonLength * 0.12f, z), CrayonLength);
        crayon.localRotation = Quaternion.Euler(0f, 62f, 0f);
        return crayon;
    }

    const float CrayonLength = 0.22f;

    // 69: arka duvarda cocuk boyunda yedi kucuk el cevresi (yururken koymus: araliklar ve acilar duzensiz), sonuncusunun yaninda bir buyuk
    static void HandRow(Transform face)
    {
        var rng = new System.Random(69);
        float x = -2.7f;
        for (int i = 0; i < HandsSmall; i++)
        {
            float y = HandHeight + 0.06f * ((float)rng.NextDouble() - 0.5f);
            HandTrace.Draw(face, new Vector2(x, y), Hand.Small, ((float)rng.NextDouble() - 0.5f) * 30f, 690 + i);
            x += 0.5f + 0.35f * (float)rng.NextDouble();
        }
        for (int i = 0; i < HandsBig; i++)
            HandTrace.Draw(face, new Vector2(x - 0.12f, HandHeight - 0.03f), Hand.Big, 5f, 698 + i);
    }

    const float HandHeight = 0.5f;   // bilek; Kivilcim'in basi hizasi civari

    // Serce'nin bekledigi yer: alanin sag arka kosesinin ustu, Kivilcim'dan belirgin yuksekte (gunesli serit); ust yazilara girmez.
    // 67-70'te arka duvarin (el izleri, resim) onunu kapatmasin diye alanin sag yaninda, daha alcakta.
    static Vector3 SparrowSpot(int levelNumber, Vector2 areaHalf, Func<float, float, float> ground)
    {
        bool wall = SceneOf(levelNumber) == Scene.BackWall;
        float x = wall ? areaHalf.x + 0.15f : areaHalf.x - 0.5f, z = wall ? areaHalf.y - 1.6f : areaHalf.y - 0.4f;
        return new Vector3(x, ground(x, z) + (wall ? 1.3f : SparrowHeight), z);
    }

    const float SparrowHeight = 2.0f;

    const float BackWallGap = 0.9f;      // alanin arka kenari ile arka duvar arasi
    const float BackWallHeight = 1.3f;   // normal bakista ufkun biraz ustune cikar, ust yazilara girmez (goruntude ayarlandi)
}
