using System;
using System.Collections.Generic;
using UnityEngine;

// Bolge 7 · Nehir yatagi (Bolum 61-70, parametreli fonksiyonlar): docs/tasarim/senaryo-perde-2.md, gorunus: tasarim belgesi §3
// (docs/superpowers/specs/2026-10-23-bolge-7-design.md). 63-66'da kumda koloninin goc izleri: 63 genis bir grubun yolu (alanin
// solundan kanyonun ortasina), 64 grubun yaninda kosan kucuk izler (Kayit 3: "Ece yine onden kostu"), 65 kucuk izler gruptan
// ayrilip alanin arkasindan doguya (yan kanyona) sapar, 66 buyuk izler kucuklerin pesinden (alanin saginda). Izler alanin disinda.
public static class RiverbedTraces
{
    const int GroupLevel = 63, RunAheadLevel = 64, SplitLevel = 65, FollowLevel = 66;
    const int SideCanyonFirst = 67, LastLevel = 70;
    static readonly int SideCanyonId = Shader.PropertyToID("_SideCanyon");

    // 67-70 dar yan kanyonda gecer (kucuk izlerin saptigi yer; Ece'nin yazisi buradaki duvarda)
    public static bool IsSideCanyon(int levelNumber) => levelNumber >= SideCanyonFirst && levelNumber <= LastLevel;

    // Arka plan ve zemin cizimine "yan kanyondayiz" bilgisi; baska bolumlerde 0
    public static void ResetBackdrop(int levelNumber) => Shader.SetGlobalFloat(SideCanyonId, IsSideCanyon(levelNumber) ? 1f : 0f);

    // Insan olcusu (robot ~0,5 birim, yetiskin ~1): yetiskin izi 0,16 boyunda, cocugunki 0,1
    const float AdultLength = 0.16f, AdultWidth = 0.065f, ChildLength = 0.1f, ChildWidth = 0.045f;

    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        if (levelNumber < GroupLevel || levelNumber > FollowLevel) return;
        var prints = new Footprints(ground);
        switch (levelNumber)
        {
            case GroupLevel:
                Group(prints, areaHalf);
                break;
            case RunAheadLevel:
                Group(prints, areaHalf);
                prints.Walk(GroupPath(areaHalf), Child(-0.66f, 0.4f, 0f, 0.07f));   // grubun kenarinda kosarak, uzun adim
                break;
            case SplitLevel:
                Group(prints, areaHalf);
                prints.Walk(SplitPath(areaHalf), Child(0f, 0.26f, 0f, 0.05f));
                break;
            case FollowLevel:
                prints.Walk(FollowPath(areaHalf), Child(-0.1f, 0.26f, 0f, 0.03f));
                // buyuk izler sonra gecmis: kucuklerin yaninda ikinci bir sira, aceleyle (uzun adim)
                prints.Walk(FollowPath(areaHalf), new Footprints.Walker
                    { Offset = 0.16f, Step = 0.46f, Length = AdultLength, Width = AdultWidth, Start = 0f, End = 1f, Phase = 0.15f });
                break;
        }
        prints.Build(parent, Oyun.GroundMarkMat);
    }

    // Grup: alanin solundan kuzeye yuruyen dokuz kisi; alanin arkasinda kanyonun ortasina (uzaktaki tek kola) doner
    static List<Vector2> GroupPath(Vector2 a) => Footprints.Curve(
        new Vector2(-a.x - 0.75f, -a.y - 4f), new Vector2(-a.x - 0.8f, -a.y), new Vector2(-a.x - 0.7f, 0f),
        new Vector2(-a.x - 0.75f, a.y), new Vector2(-a.x - 0.3f, a.y + 1.6f), new Vector2(-a.x + 0.6f, a.y + 3.2f));

    static void Group(Footprints prints, Vector2 areaHalf)
    {
        var path = GroupPath(areaHalf);
        const int People = 9;
        for (int i = 0; i < People; i++)
        {
            float offset = -0.42f + 0.84f * i / (People - 1) + 0.05f * Mathf.Sin(i * 2.7f);
            prints.Walk(path, new Footprints.Walker
            {
                Offset = offset, Step = 0.34f + 0.05f * Mathf.Sin(i * 1.9f + 0.4f), Length = AdultLength, Width = AdultWidth,
                Start = 0f, End = 1f, Phase = 0.29f * ((i * 0.37f) % 1f),
            });
        }
    }

    // 65: kucuk izler grubun icinden ayrilir, alanin arkasindan gecip doguya, yan kanyona dogru gider
    static List<Vector2> SplitPath(Vector2 a) => Footprints.Curve(
        new Vector2(-a.x - 0.6f, a.y - 2.2f), new Vector2(-a.x - 0.35f, a.y - 0.2f), new Vector2(-a.x + 0.9f, a.y + 0.75f),
        new Vector2(0f, a.y + 0.85f), new Vector2(a.x - 0.6f, a.y + 0.9f), new Vector2(a.x + 0.6f, a.y + 1.3f), new Vector2(a.x + 1.5f, a.y + 2.4f));

    // 66: kucuk izler alanin saginda kuzeye, sonra doguya (yan kanyonun agzina) sapar; buyuk izler ayni yoldan
    static List<Vector2> FollowPath(Vector2 a) => Footprints.Curve(
        new Vector2(a.x + 0.55f, -a.y - 4f), new Vector2(a.x + 0.6f, -a.y), new Vector2(a.x + 0.5f, 0f),
        new Vector2(a.x + 0.6f, a.y), new Vector2(a.x + 1.1f, a.y + 1.5f), new Vector2(a.x + 2.1f, a.y + 3f));

    static Footprints.Walker Child(float offset, float step, float start, float phase) => new Footprints.Walker
        { Offset = offset, Step = step, Length = ChildLength, Width = ChildWidth, Start = start, End = 1f, Phase = phase };
}
