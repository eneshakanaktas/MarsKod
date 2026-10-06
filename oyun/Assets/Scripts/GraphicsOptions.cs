using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Cizim deneme secenekleri (oyuncu gormez). Zayif telefonda neyin agir geldigini bulmak icin; oyuncunun grafik ayarinin
// (GraphicsQuality, performans modu) ustune yazar. Iki yoldan verilir:
//  - baslatma secenegi: adb shell am start -S -n com.marskod.oyun/com.unity3d.player.UnityPlayerGameActivity -e unity "-arkaplan yok"
//  - dosya (baslatma secenegi telefona ulasmazsa): oyunun klasorundeki secenekler.txt (OptionsFile), ayni yazim.
// Ayrinti ve hazir komutlar: docs/tasarim/telefon-denemesi.md. Hangi secenekler acik, log'a "GRAFIK:" satiriyla yazilir.
public static class GraphicsOptions
{
    const string OptionsFile = "secenekler.txt";

    public static GraphicsMode? ForcedMode { get; private set; }    // -performans ac|kapali
    public static bool BackdropFlat { get; private set; }            // -arkaplan yok
    public static float? BackdropScale { get; private set; }         // -arkaplan-olcek 0.1..1
    public static bool? SimpleGround { get; private set; }           // -zemin sade|tam
    public static bool NoShadows { get; private set; }               // -golgesiz
    public static bool NoAnimations { get; private set; }            // -animasyonsuz (ayari degistirmeden kapali baslar)
    public static bool NoHud { get; private set; }                   // -arayuz yok (yalniz olcum: arayuz gorunmez)
    public static bool NoScenery { get; private set; }               // -cevre yok (alan cevresindeki kayalar/esyalar kurulmaz)
    public static string Used { get; private set; } = "varsayilan";

    static float? renderScale;
    static int? msaa;
    static bool? hdr;

    // Editorde oynatma her basladiginda (alan yeniden yuklenmeden de): onceki oynatmanin secenekleri kalmasin
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ForcedMode = null; BackdropFlat = false; BackdropScale = null; SimpleGround = null;
        NoShadows = NoAnimations = NoHud = NoScenery = false;
        Used = "varsayilan";
        renderScale = null; msaa = null; hdr = null;
    }

    // Oyun basinda bir kez, GraphicsQuality.Init'ten once
    public static void Parse()
    {
        var a = Arguments();
        var used = new System.Text.StringBuilder();
        for (int i = 0; i < a.Length; i++)
        {
            string next = i + 1 < a.Length ? a[i + 1] : "";
            switch (a[i])
            {
                case "-kalite": if (int.TryParse(next, out int level)) QualitySettings.SetQualityLevel(level, true); break;
                case "-cizgisiz": Parts.NoOutline = true; break;
                case "-performans": ForcedMode = next == "kapali" ? GraphicsMode.Full : GraphicsMode.Performance; break;
                case "-arkaplan": BackdropFlat = next == "yok"; break;
                case "-arkaplan-olcek": if (TryNumber(next, out float s)) BackdropScale = s; break;
                case "-zemin": SimpleGround = next == "sade"; break;
                case "-golgesiz": NoShadows = true; break;
                case "-animasyonsuz": NoAnimations = true; break;
                case "-arayuz": NoHud = next == "yok"; break;
                case "-cevre": NoScenery = next == "yok"; break;
                case "-olcek": if (TryNumber(next, out float r)) renderScale = Mathf.Clamp(r, 0.25f, 1f); break;
                case "-msaa": if (int.TryParse(next, out int m)) msaa = m; break;
                case "-hdr": hdr = next != "0"; break;
                case "-kare": if (int.TryParse(next, out int fps)) Application.targetFrameRate = fps; break;
                default: continue;
            }
            used.Append(a[i]).Append(' ');
        }
        if (used.Length > 0) Used = used.ToString().TrimEnd();
    }

    // Oyuncu ayari uygulandiktan sonra: deneme secenekleri ustune yazar
    public static void ApplyOverrides(UniversalRenderPipelineAsset urp)
    {
        if (renderScale.HasValue) urp.renderScale = renderScale.Value;
        if (msaa.HasValue) urp.msaaSampleCount = msaa.Value;
        if (hdr.HasValue) urp.supportsHDR = hdr.Value;
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
