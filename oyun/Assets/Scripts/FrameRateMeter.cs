using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Deneme icin kare hizi gostergesi (oyuncu gormez; kapali baslar).
// Acmak: telefonda uc parmakla ekrana dokunmak (ac/kapa, secim kalici) ya da "-fps" secenegi.
// Gosterir: son bir saniyedeki ortalama kare hizi, en uzun karenin suresi (takilmalari yakalar), islemci ve ekran karti
// suresi (hangisi buyukse yavaslik oradan). Gosterge kapaliyken de 5 sn'de bir log'a KARE satiri yazar
// (telefonda: adb logcat -d -s Unity | findstr KARE).
public class FrameRateMeter : MonoBehaviour
{
    const string SaveKey = "fps-gostergesi";
    const int Fingers = 3;
    const float LogEvery = 5f;

    Label label;
    float windowTime, slowest;
    int frames;
    bool wasTouching;
    float logTime;
    readonly FrameTiming[] timing = new FrameTiming[1];
    double cpuSum, gpuSum;
    int cpuCount, gpuCount;

    // Kendi bagimsiz nesnesinde durur: Hud'un arayuz belgesinin altina girerse onun panelini kullanmak zorunda kalir
    public static void Create(bool forceOn)
    {
        var host = new GameObject("FrameRateMeter");
        var meter = host.AddComponent<FrameRateMeter>();
        meter.Build();
        meter.SetVisible(forceOn || PlayerPrefs.GetInt(SaveKey, 0) == 1);
    }

    void Build()
    {
        var ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/MarsTheme");
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1080, 2340);
        ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match = 0f;
        ps.sortingOrder = 100; // arayuzun ustunde

        var doc = gameObject.AddComponent<UIDocument>();
        doc.panelSettings = ps;
        var root = doc.rootVisualElement;
        root.pickingMode = PickingMode.Ignore;

        label = Ui.Text("", Resources.Load<Font>("Fonts/JetBrainsMonoNL-Regular"), 30, Color.white);
        label.style.position = Position.Absolute;
        label.style.left = 20;
        label.style.bottom = Ui.SafeBottom() + 20;
        label.style.paddingLeft = 14; label.style.paddingRight = 14;
        label.style.paddingTop = 6; label.style.paddingBottom = 6;
        label.style.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        Ui.Radius(label, 10);
        root.Add(label);
    }

    void SetVisible(bool on)
    {
        label.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
    }

    bool Visible => label.style.display == DisplayStyle.Flex;

    void Update()
    {
        WatchToggleGesture();
        CollectTiming();

        float dt = Time.unscaledDeltaTime;
        windowTime += dt;
        logTime += dt;
        frames++;
        if (dt > slowest) slowest = dt;
        if (windowTime < 1f) return;

        float fps = frames / windowTime;
        string cpu = cpuCount > 0 ? $"{cpuSum / cpuCount:0}" : "—";
        string gpu = gpuCount > 0 ? $"{gpuSum / gpuCount:0}" : "—";
        if (Visible)
        {
            label.text = $"{fps:0} kare/sn  · en uzun {slowest * 1000f:0} ms\nişlemci {cpu} ms  · ekran kartı {gpu} ms";
            label.style.color = fps >= 50f ? new Color(0.6f, 1f, 0.6f) : fps >= 30f ? new Color(1f, 0.85f, 0.4f) : new Color(1f, 0.45f, 0.45f);
        }
        if (logTime >= LogEvery)
        {
            Debug.Log($"KARE: {fps:0} kare/sn | en uzun {slowest * 1000f:0} ms | islemci {cpu} ms | ekran karti {gpu} ms | performans {(GraphicsQuality.Performance ? "ac" : "kapali")}");
            logTime = 0f;
        }
        windowTime = 0f; frames = 0; slowest = 0f;
        cpuSum = gpuSum = 0; cpuCount = gpuCount = 0;
    }

    // Isletim sisteminden son karenin islemci ve ekran karti suresi (Player ayari: Frame Timing Stats). Desteklenmiyorsa 0 gelir, sayilmaz.
    void CollectTiming()
    {
        FrameTimingManager.CaptureFrameTimings();
        if (FrameTimingManager.GetLatestTimings(1, timing) < 1) return;
        if (timing[0].cpuFrameTime > 0) { cpuSum += timing[0].cpuFrameTime; cpuCount++; }
        if (timing[0].gpuFrameTime > 0) { gpuSum += timing[0].gpuFrameTime; gpuCount++; }
    }

    // Uc parmak ayni anda ekrana degince gostergeyi acip kapatir
    void WatchToggleGesture()
    {
        var screen = Touchscreen.current;
        if (screen == null) return;
        int pressed = 0;
        foreach (var touch in screen.touches)
            if (touch.press.isPressed) pressed++;
        bool touching = pressed >= Fingers;
        if (touching && !wasTouching)
        {
            bool on = !Visible;
            SetVisible(on);
            PlayerPrefs.SetInt(SaveKey, on ? 1 : 0);
            PlayerPrefs.Save();
        }
        wasTouching = touching;
    }
}
