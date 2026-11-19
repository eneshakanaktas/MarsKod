using UnityEngine;

// Gece ovasinin (Bolge 10) gokyuzundeki volkan (NightSky.hlsl). Bolum girisinde patlar: _VolcanoBurst 0-1 patlamanin ilerleyisi,
// 0 = sakin. Volkan resimdedir (3B degil); lav toplari Mouth noktasindan (resimdeki agzin 3B karsiligi, SkyPicture) cikar.
public static class VolcanoSky
{
    static readonly int BurstId = Shader.PropertyToID("_VolcanoBurst");
    const float MouthX = 0.06f, MouthAboveHorizon = 0.084f;   // NightSky.hlsl: NIGHT_VOLCANO_X, NIGHT_VOLCANO_TOP

    public static void Burst(float progress) => Shader.SetGlobalFloat(BurstId, progress);

    public static Vector3 Mouth(Camera cam) => SkyPicture.WorldPoint(cam, MouthX, MouthAboveHorizon);
}
