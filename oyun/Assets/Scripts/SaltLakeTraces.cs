// Bolge 9 (tuz golu, Bolum 81-90; docs/tasarim/senaryo-bolge-09.md) izleri. Gorunus (bembeyaz gol, yansiyan gokyuzu, tuzla
// cevrili oklar, tuza gomulu oyuncak astronot) 3a'da; simdilik bolge gecisinin satirlari ve volkanin yakinligi.
using UnityEngine;

public static class SaltLakeTraces
{
    public const int FirstLevel = 81, LastLevel = 90;
    static readonly int SceneId = Shader.PropertyToID("_SaltScene");

    // Arka plana bilgi (SaltSky.hlsl): volkan bolum bolum yaklasir (81 uzakta, 88 belirgin, 90 yakin). Bolge 9 disinda 0.
    public static void ResetBackdrop(int levelNumber)
    {
        bool inRegion = levelNumber >= FirstLevel && levelNumber <= LastLevel;
        Shader.SetGlobalFloat(SceneId, inRegion ? Mathf.Lerp(0.2f, 0.85f, (levelNumber - FirstLevel) / (float)(LastLevel - FirstLevel)) : 0f);
    }
    public static readonly string[] TransitionLines = { "Yeni bölge: Tuz gölü", "Hedef: okların gösterdiği dağ" };
}
