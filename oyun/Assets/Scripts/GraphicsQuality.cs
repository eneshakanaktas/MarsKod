using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GraphicsMode { Performance, Full }

// Oyuncunun grafik ayari (Bolumler ekrani, "Performans modu"): acikken telefonda akici, kapaliyken tam ayrinti.
// Cizim yolu iki modda da Forward+: telefonda Forward'da kodla uretilen Lit malzemeler simsiyah cikmisti (OyunBuild.cs).
// Telefonda acik, bilgisayarda kapali baslar; secim telefonda kalir. Oyun acikken degistirilebilir: URP ayarlari ve zemin
// cesidi burada, kamera/isik/gokyuzu dokusu Changed olayiyla Oyun.cs'te guncellenir. Deneme secenekleri (GraphicsOptions)
// bunun ustune yazar.
public static class GraphicsQuality
{
    const string SaveKey = "performans";   // 1 = performans modu acik
    static readonly GlobalKeyword SimpleGroundKeyword = GlobalKeyword.Create("MARSKOD_SADE");

    public static GraphicsMode Mode { get; private set; }
    public static event Action Changed;

    public static bool Performance => Mode == GraphicsMode.Performance;
    public static float BackdropScale => GraphicsOptions.BackdropScale ?? (Performance ? 0.35f : Application.isMobilePlatform ? 0.5f : 1f);
    public static float BackdropRate => Performance ? 10f : 20f;
    public static LightShadows Shadows => GraphicsOptions.NoShadows ? LightShadows.None : Performance ? LightShadows.Hard : LightShadows.Soft;

    static UniversalRenderPipelineAsset urp;
    static RenderPipelineAsset original;   // kopyadan onceki ayar: oyun bitince geri konur (editorde proje ayarina kopya kalmasin)

    // Editorde oynatma her basladiginda (alan yeniden yuklenmeden de): onceki oynatmadan kalan durum temizlenir
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Changed = null;
        urp = null;
        original = null;
    }

    // Oyun basinda bir kez (GraphicsOptions.Parse'tan sonra)
    public static void Init()
    {
        // URP ayar dosyasinin calisma ani kopyasi: editorde oynarken ayar dosyasi diskte degismesin
        original = QualitySettings.renderPipeline;
        var asset = original != null ? original : GraphicsSettings.defaultRenderPipeline;
        urp = asset != null ? UnityEngine.Object.Instantiate(asset) as UniversalRenderPipelineAsset : null;
        if (urp != null)
        {
            QualitySettings.renderPipeline = urp;
            Application.quitting += RestoreOriginal;
        }

        bool saved = PlayerPrefs.GetInt(SaveKey, Application.isMobilePlatform ? 1 : 0) == 1;
        Mode = GraphicsOptions.ForcedMode ?? (saved ? GraphicsMode.Performance : GraphicsMode.Full);
        Apply();
    }

    // Ayardan: secimi kaydeder ve hemen uygular
    public static void Set(GraphicsMode mode)
    {
        Mode = mode;
        PlayerPrefs.SetInt(SaveKey, mode == GraphicsMode.Performance ? 1 : 0);
        PlayerPrefs.Save();
        Apply();
    }

    static void RestoreOriginal()
    {
        Application.quitting -= RestoreOriginal;
        QualitySettings.renderPipeline = original;
    }

    static void Apply()
    {
        if (urp != null)
        {
            urp.renderScale = Performance ? 0.6f : 1f;
            urp.msaaSampleCount = Performance ? 1 : 4;
            urp.supportsHDR = !Performance;
            urp.mainLightShadowmapResolution = Performance ? 1024 : 2048;
            GraphicsOptions.ApplyOverrides(urp);
        }
        Shader.SetKeyword(SimpleGroundKeyword, GraphicsOptions.SimpleGround ?? Performance);
        Debug.Log($"GRAFIK: {Mode} | secenekler: {GraphicsOptions.Used} | arka plan olcegi {BackdropScale} | ekran {Screen.width}x{Screen.height}" +
                  $" | {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType})" +
                  (urp != null ? $" | olcek {urp.renderScale} msaa {urp.msaaSampleCount} hdr {urp.supportsHDR}" : ""));
        Changed?.Invoke();
    }
}
