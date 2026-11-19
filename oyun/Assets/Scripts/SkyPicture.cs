using UnityEngine;

// Gokyuzu resmi (MarsSky.hlsl) kameraya yapisik duz bir "fotograf"tir; ustundeki bir nokta (koloninin lambasi, Bolge 10'un
// volkani) 3B sahnede yoktur. Buradaki yardimci, resimdeki noktanin ekrandaki yerine bakan, sahnenin cok gerisinde bir 3B nokta
// verir (MarsPictureUV'nin tersi: arka planla ayni olcek ve kayma). O noktadan cikan/oraya giden bir sey resimdekiyle ust uste gorunur.
public static class SkyPicture
{
    /// <summary>x: fotograf yuksekligi biriminde yatay (0 = orta), aboveHorizon: ufka gore yukseklik (fotograf biriminde)</summary>
    public static Vector3 WorldPoint(Camera cam, float x, float aboveHorizon)
    {
        Vector4 picture = Shader.GetGlobalVector("_Picture");
        float horizon = Shader.GetGlobalFloat("_Horizon");
        float aspect = cam.pixelWidth / (float)cam.pixelHeight;
        var uv = new Vector2(x / aspect + 0.5f, horizon + aboveHorizon);
        Vector2 n = (uv * 2f - Vector2.one) * Mathf.Max(picture.x, 1e-3f) + new Vector2(0f, picture.y);
        Vector2 screen = (n * 0.5f + Vector2.one * 0.5f) * new Vector2(cam.pixelWidth, cam.pixelHeight);
        var ray = cam.ScreenPointToRay(screen);
        // gercek ufuk ekranin disinda: bu bakis bir yerde zemine degir; nokta zeminin icine girmesin diye ondan once durur
        float distance = cam.farClipPlane * 0.8f;
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float ground)) distance = Mathf.Min(distance, ground * 0.85f);
        return ray.GetPoint(distance);
    }
}
