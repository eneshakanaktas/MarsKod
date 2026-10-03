using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Cizim deneme secenekleri (oyuncu gormez). Zayif telefonda neyin agir geldigini bulmak icin. Iki yoldan verilir:
//  - baslatma secenegi: adb shell am start -S -n com.marskod.oyun/com.unity3d.player.UnityPlayerGameActivity -e unity "-arkaplan yok"
//  - dosya (baslatma secenegi telefona ulasmazsa): oyunun klasorundeki secenekler.txt (OptionsFile), ayni yazim.
// Ayrinti ve hazir komutlar: docs/tasarim/telefon-denemesi.md. Hangi secenekler acik, log'a "GRAFIK:" satiriyla yazilir.
public static class GraphicsOptions
{
    const string OptionsFile = "secenekler.txt";

    public static bool BackdropFlat { get; private set; }               // -arkaplan yok
    public static float BackdropScale { get; private set; } = 1f;  // -arkaplan-olcek 0.25..1 (telefonda varsayilan 0,5)
    public static bool NoShadows { get; private set; }                  // -golgesiz
    public static bool NoAnimations { get; private set; }               // -animasyonsuz (ayari degistirmeden kapali baslar)

    // Oyun basinda bir kez: komut satirini okur, cizim ayarlarina uygular
    public static void Apply()
    {
        BackdropScale = Application.isMobilePlatform ? 0.5f : 1f;
        var a = Arguments();
        var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        var used = new System.Text.StringBuilder();
        for (int i = 0; i < a.Length; i++)
        {
            string next = i + 1 < a.Length ? a[i + 1] : "";
            switch (a[i])
            {
                case "-kalite":
                    if (int.TryParse(next, out int level)) QualitySettings.SetQualityLevel(level, true);
                    urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                    break;
                case "-cizgisiz": Parts.NoOutline = true; break;
                case "-arkaplan": BackdropFlat = next == "yok"; break;
                case "-arkaplan-olcek": if (TryNumber(next, out float s)) BackdropScale = s; break;
                case "-zemin": Shader.SetGlobalFloat("_GroundSimple", next == "sade" ? 1f : 0f); break;
                case "-golgesiz": NoShadows = true; break;
                case "-animasyonsuz": NoAnimations = true; break;
                case "-olcek": if (urp != null && TryNumber(next, out float r)) urp.renderScale = Mathf.Clamp(r, 0.25f, 1f); break;
                case "-msaa": if (urp != null && int.TryParse(next, out int m)) urp.msaaSampleCount = m; break;
                case "-hdr": if (urp != null) urp.supportsHDR = next != "0"; break;
                case "-kare": if (int.TryParse(next, out int fps)) Application.targetFrameRate = fps; break;
                default: continue;
            }
            used.Append(a[i]).Append(' ');
        }
        Debug.Log("GRAFIK: " + (used.Length > 0 ? used.ToString() : "varsayilan") +
                  $"| arka plan olcegi {BackdropScale} | ekran {Screen.width}x{Screen.height} | {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType})" +
                  (urp != null ? $" | olcek {urp.renderScale} msaa {urp.msaaSampleCount} hdr {urp.supportsHDR}" : ""));
    }

    // Baslatma secenekleri + (varsa) dosyadaki secenekler
    static string[] Arguments()
    {
        var list = new System.Collections.Generic.List<string>(System.Environment.GetCommandLineArgs());
        string path = System.IO.Path.Combine(Application.persistentDataPath, OptionsFile);
        try
        {
            if (System.IO.File.Exists(path))
                list.AddRange(System.IO.File.ReadAllText(path).Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("GRAFIK: " + OptionsFile + " okunamadi: " + e.Message);
        }
        return list.ToArray();
    }

    static bool TryNumber(string s, out float value) =>
        float.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
