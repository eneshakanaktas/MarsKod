// Bolge 10 (gece ovasi, Bolum 91-100; docs/tasarim/senaryo-bolge-10.md) izleri. Gorunus (gece, meteor yagmuru, kraterler,
// dumani tuten kizgin tas) 3a'da; gokyuzu (NightSky.hlsl) bolumun evresini buradan okur.
using UnityEngine;

public static class NightPlainTraces
{
    public const int FirstLevel = 91, LastLevel = 100;
    const int EarthNoticedLevel = 93, DoorLevel = 97;
    static readonly int SceneId = Shader.PropertyToID("_NightScene");

    // Arka plana bilgi (NightSky.hlsl): 0 = 91-92, 1 = 93-96 (mavi nokta fark edilir), 2 = 97-100 (volkanin eteginde kapi). Disinda 0.
    public static void ResetBackdrop(int levelNumber)
    {
        bool inRegion = levelNumber >= FirstLevel && levelNumber <= LastLevel;
        float scene = !inRegion ? 0f : levelNumber >= DoorLevel ? 2f : levelNumber >= EarthNoticedLevel ? 1f : 0f;
        Shader.SetGlobalFloat(SceneId, scene);
    }
    public static readonly string[] TransitionLines = { "Yeni bölge: Gece ovası", "Hedef: volkanın eteği" };
}
