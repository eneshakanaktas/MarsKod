# Telefon "Sade" grafik + ölçüm — Yapım planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Telefonda varsayılan "Sade" grafik (Bölümler ekranından "Güzel"e geçilebilir) + Hamza'nın tek denemede "neyin ağır olduğunu" görmesini sağlayan ölçüm.

**Architecture:** Yeni `GraphicsQuality` (oyuncu ayarı: Sade/Güzel → URP ayarları, renderer, zemin anahtar kelimesi, gökyüzü dokusu). `GraphicsOptions` yalnızca deneme seçeneklerini okur ve üstüne yazar. Zemin shader'ı `MARSKOD_SADE` anahtar kelimesiyle iki çeşit derlenir. `FrameRateMeter` işlemci/ekran kartı süresini gösterir ve log'a `KARE:` yazar.

**Tech Stack:** Unity 6000.6.3f1, URP 17.6.0, UI Toolkit, HLSL. Unity kodu `dotnet test` kapsamında değil: doğrulama Windows paketi + `-shots` + log satırlarıyla.

**Spec:** `docs/superpowers/specs/2026-10-06-telefon-sade-grafik-design.md`

> **İsim değişikliği (Enes, 2026-10-06):** Oyuncuya görünen ad "Sade/Güzel" değil **"Performans modu"** (anahtar açık = eski "Sade"; telefonda açık, bilgisayarda kapalı başlar; not: "Açıkken oyun daha akıcı, görüntü biraz sadeleşir"). Kodda `enum GraphicsMode { Performance, Full }`, `GraphicsQuality.Mode`, `PlayerPrefs("performans")` 1 = açık, deneme seçeneği `-performans ac|kapali`, log `GRAFIK: Performance/Full`, `KARE: ... | performans ac/kapali`. Aşağıda "Sade" = Performans modu açık, "Güzel" = kapalı.

## Global Constraints
- Unity: `C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe`. Paket: `-batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log`; hata = `build.log` içinde `error CS` veya `Shader error`. Kod değişince önce `oyun/Build/` silinir.
- Bilgisayar varsayılanı **Güzel**, telefon varsayılanı **Sade**; `PlayerPrefs("grafik")`: 0 = Sade, 1 = Güzel.
- Sade: renderScale 0,6 · MSAA 1 · HDR kapalı · gölge sert, harita 1024 · renderer 1 (Forward) · `MARSKOD_SADE` açık · gökyüzü ölçeği 0,35, animasyon 10/sn.
- Güzel: renderScale 1 · MSAA 4 · HDR açık · gölge yumuşak, harita 2048 · renderer 0 · anahtar kelime kapalı · gökyüzü ölçeği telefonda 0,5 / bilgisayarda 1, animasyon 20/sn.
- Yorumlar Türkçe ve ASCII (dosyalardaki mevcut üslup: `// ...` Türkçe karaktersiz). Arayüz metinleri Türkçe karakterli.
- Her `.asset`/`.cs` yeni dosyanın yanında `.meta` (benzersiz guid).
- Motor (`Assets/Motor`, `Assets/Dunya`) değişmez; `dotnet test` 1285 kalır.
- Commit gün sonunda (ekip döngüsü), görev başına commit yok.

## Review Focus
1. Oyun açıkken Sade↔Güzel geçişi: gökyüzü dokusu yeni boyda yeniden çizilir, zemin, gölge, çözünürlük anında değişir; eski görünümden iz kalmaz. → Görev 4 Adım 4.
2. Unity editöründe ▶ ile oynayıp grafik değiştirmek `PC_RPAsset.asset`'i diskte değiştirmez (çalışma anı kopyası). → Görev 3 Adım 6.
3. `-grafik sade` kayıtlı tercihi ezer ama kendisi kaydedilmez; ayardan değişince yeni seçim kaydedilir. → Görev 4 Adım 4.
4. Ekran kartı süresi alınamayan cihazda gösterge "—" yazar, 0 ya da hata değil. → Görev 5 Adım 3.
5. `-arayuz yok` ile oyun çökmez, sahnenin kamera hesabı bozulmaz (arayüz yalnızca görünmez olur). → Görev 5 Adım 4.

---

### Task 1: Hazırlık — Forward renderer + kare süresi ölçüm ayarı

**Files:**
- Revert: `oyun/Assets/Settings/PC_RPAsset.asset`, `oyun/ProjectSettings/GraphicsSettings.asset`, `oyun/ProjectSettings/ProjectAuditorSettings.asset`, `oyun/ProjectSettings/ProjectSettings.asset` (yalnızca satır sonu farkı)
- Create: `oyun/Assets/Settings/Mobile_Renderer_Forward.asset` + `.meta`
- Modify: `oyun/Assets/Settings/Mobile_RPAsset.asset:19-21`, `oyun/Assets/Settings/PC_RPAsset.asset:19-21`, `oyun/ProjectSettings/ProjectSettings.asset:160`

**Interfaces:** Produces: renderer listesinde 1 numara = `Mobile_Renderer_Forward` (iki URP ayarında da).

- [ ] **Step 1:** `git checkout -- oyun/Assets/Settings/PC_RPAsset.asset oyun/ProjectSettings/GraphicsSettings.asset oyun/ProjectSettings/ProjectAuditorSettings.asset oyun/ProjectSettings/ProjectSettings.asset`; `git status -s` boş.
- [ ] **Step 2:** `Mobile_Renderer.asset`'i `Mobile_Renderer_Forward.asset` olarak kopyala; içinde `m_Name: Mobile_Renderer` → `m_Name: Mobile_Renderer_Forward`, `m_RenderingMode: 2` → `m_RenderingMode: 0`. `.meta`'yı `Mobile_Renderer.asset.meta`'dan kopyala, `guid:` satırına `py -c "import uuid;print(uuid.uuid4().hex)"` çıktısını yaz.
- [ ] **Step 3:** İki URP ayarında `m_RendererDataList` altına ikinci satır:
```yaml
  - {fileID: 11400000, guid: <yeni guid>, type: 2}
```
- [ ] **Step 4:** `ProjectSettings.asset`: `enableFrameTimingStats: 0` → `1`.
- [ ] **Step 5:** Windows paketi üret; `error CS`/`Shader error` yok, `Build/Win/MarsKod.exe` var. `git status -s` yalnızca bu görevin dosyalarını göstermeli (Unity başka ayar dosyasını yeniden yazdıysa ve fark yalnızca satır sonuysa geri al).

### Task 2: Hafif zemin (shader çeşidi)

**Files:**
- Modify: `oyun/Assets/Shaders/Ground.shader:40-52, 85-169`
- Modify: `oyun/Assets/Scripts/GraphicsOptions.cs:38` (geçici bağlantı; Görev 3'te yeniden yazılır)

**Interfaces:** Produces: genel anahtar kelime `MARSKOD_SADE` (açıkken hafif zemin). `_GroundSimple` kaldırılır.

- [ ] **Step 1:** ForwardLit geçişinin pragmalarına ekle: `#pragma multi_compile_fragment _ MARSKOD_SADE`. `float _GroundSimple;` satırını ve `regolith`/`iceSheet` içindeki `if (_GroundSimple > 0.5) return ...;` satırlarını sil.
- [ ] **Step 2:** `frostAt`, `regolith`, `iceSheet` (ve `craterShade`) tanımlarını `#if defined(MARSKOD_SADE) ... #else <mevcut kod> #endif` içine al. Sade dal:
```hlsl
#if defined(MARSKOD_SADE)
            // Telefon (Sade grafik): pahali gurultuler yok. Ana renk + genis lekeler + alandaki krater + yuvarlak buz lekeleri.
            float frostAt(float2 xz, float4 f)
            {
                if (f.w < 0.5) return 0.0;
                return 0.8 * (1.0 - smoothstep(f.z * 0.3, f.z, length(xz - f.xy)));
            }

            float3 regolith(float3 p, float far, float inArea)
            {
                float2 q = p.xz;
                float3 a = lerp(float3(0.42, 0.25, 0.20), float3(0.52, 0.32, 0.25), vnoise2(q * 2.2));
                a *= 0.9 + 0.2 * vnoise2(q * 0.35 + 4.0);   // genis lekeler
                if (_Crater.z > 0.0)
                {
                    float dc = length(q - _Crater.xy) / _Crater.z;
                    a *= lerp(0.72 + 0.2 * dc * dc, 1.0, smoothstep(0.85, 1.0, dc));
                    a *= 1.0 + 0.16 * exp(-pow((dc - 1.0) / 0.12, 2.0));
                }
                float fr = max(max(frostAt(q, _Frost0), frostAt(q, _Frost1)), max(frostAt(q, _Frost2), frostAt(q, _Frost3)));
                fr = max(fr, max(frostAt(q, _Frost4), frostAt(q, _Frost5)));
                return lerp(a, float3(0.66, 0.72, 0.78), fr * 0.7);
            }

            float3 iceSheet(float3 p, float far, float inArea)
            {
                float2 q = p.xz;
                float3 a = lerp(float3(0.70, 0.76, 0.85), float3(0.80, 0.85, 0.92), vnoise2(q * 2.2));
                return a * (0.94 + 0.1 * vnoise2(q * 0.35 + 4.0));
            }
#else
```
- [ ] **Step 3:** `GraphicsOptions.cs` `-zemin` satırı → `case "-zemin": Shader.SetKeyword(GlobalKeyword.Create("MARSKOD_SADE"), next == "sade"); break;`
- [ ] **Step 4:** Paketi üret (hatasız). `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -zemin sade` ile aç, 10 sn sonra kapat; log (`%USERPROFILE%/AppData/LocalLow/<şirket>/MarsKod/Player.log`) içinde `Shader` hatası yok. Görsel kontrol Görev 6'da.

### Task 3: GraphicsQuality (Sade/Güzel çekirdeği)

**Files:**
- Create: `oyun/Assets/Scripts/GraphicsQuality.cs` + `.meta`
- Rewrite: `oyun/Assets/Scripts/GraphicsOptions.cs`
- Modify: `oyun/Assets/Scripts/BackdropCache.cs` (AnimatedRate → alan, `SetQuality`)
- Modify: `oyun/Assets/Scripts/Oyun.cs:187, 221, 299, 308` + yeni `ApplyGraphics()`

**Interfaces:**
- Produces: `enum GraphicsLevel { Sade, Guzel }`; `GraphicsQuality.Init()`, `.Level`, `.Set(GraphicsLevel)`, `event Action Changed`, `.BackdropScale`, `.BackdropRate`, `.Shadows` (LightShadows), `.RendererIndex`.
- Produces: `GraphicsOptions.Parse()`, `.ApplyOverrides(UniversalRenderPipelineAsset)`, `.ForcedLevel` (GraphicsLevel?), `.BackdropFlat`, `.NoShadows`, `.NoAnimations`, `.NoHud`, `.NoScenery`, `.Used` (string).
- Produces: `BackdropCache.SetQuality(float scale, float rate)`.

- [ ] **Step 1: GraphicsOptions'ı yalnızca okuyan hâle getir.**
```csharp
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Cizim deneme secenekleri (oyuncu gormez). Zayif telefonda neyin agir geldigini bulmak icin; oyuncunun grafik ayarinin
// (GraphicsQuality) ustune yazar. Iki yoldan verilir:
//  - baslatma secenegi: adb shell am start -S -n com.marskod.oyun/com.unity3d.player.UnityPlayerGameActivity -e unity "-arkaplan yok"
//  - dosya (baslatma secenegi telefona ulasmazsa): oyunun klasorundeki secenekler.txt (OptionsFile), ayni yazim.
// Ayrinti ve hazir komutlar: docs/tasarim/telefon-denemesi.md. Hangi secenekler acik, log'a "GRAFIK:" satiriyla yazilir.
public static class GraphicsOptions
{
    const string OptionsFile = "secenekler.txt";

    public static GraphicsLevel? ForcedLevel { get; private set; }  // -grafik sade|guzel
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
                case "-grafik": ForcedLevel = next == "sade" ? GraphicsLevel.Sade : GraphicsLevel.Guzel; break;
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

    // Arguments() ve TryNumber() bugunku haliyle kalir.
}
```
(`Arguments()` ve `TryNumber()` gövdeleri mevcut dosyadan aynen taşınır.)

- [ ] **Step 2: GraphicsQuality.cs**
```csharp
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GraphicsLevel { Sade, Guzel }

// Oyuncunun grafik ayari (Bolumler ekrani): Sade = telefonda akici, Guzel = tam ayrinti. Telefonda Sade, bilgisayarda
// Guzel baslar; secim telefonda kalir. Oyun acikken degistirilebilir: URP ayarlari ve zemin cesidi burada, kamera/isik/
// gokyuzu dokusu Changed olayiyla Oyun.cs'te guncellenir. Deneme secenekleri (GraphicsOptions) bunun ustune yazar.
public static class GraphicsQuality
{
    const string SaveKey = "grafik";   // 0 = Sade, 1 = Guzel
    static readonly GlobalKeyword SimpleGroundKeyword = GlobalKeyword.Create("MARSKOD_SADE");

    public static GraphicsLevel Level { get; private set; }
    public static event Action Changed;

    static bool Sade => Level == GraphicsLevel.Sade;
    public static float BackdropScale => GraphicsOptions.BackdropScale ?? (Sade ? 0.35f : Application.isMobilePlatform ? 0.5f : 1f);
    public static float BackdropRate => Sade ? 10f : 20f;
    public static int RendererIndex => Sade ? 1 : 0;   // 1: Mobile_Renderer_Forward (iki URP ayarinda da)
    public static LightShadows Shadows => GraphicsOptions.NoShadows ? LightShadows.None : Sade ? LightShadows.Hard : LightShadows.Soft;

    static UniversalRenderPipelineAsset urp;

    // Oyun basinda bir kez (GraphicsOptions.Parse'tan sonra)
    public static void Init()
    {
        // URP ayar dosyasinin calisma ani kopyasi: editorde oynarken ayar dosyasi diskte degismesin
        var asset = QualitySettings.renderPipeline != null ? QualitySettings.renderPipeline : GraphicsSettings.defaultRenderPipeline;
        urp = UnityEngine.Object.Instantiate(asset) as UniversalRenderPipelineAsset;
        QualitySettings.renderPipeline = urp;

        var saved = (GraphicsLevel)PlayerPrefs.GetInt(SaveKey, Application.isMobilePlatform ? 0 : 1);
        Level = GraphicsOptions.ForcedLevel ?? saved;
        Apply();
    }

    // Ayardan: secimi kaydeder ve hemen uygular
    public static void Set(GraphicsLevel level)
    {
        Level = level;
        PlayerPrefs.SetInt(SaveKey, (int)level);
        PlayerPrefs.Save();
        Apply();
    }

    static void Apply()
    {
        if (urp != null)
        {
            urp.renderScale = Sade ? 0.6f : 1f;
            urp.msaaSampleCount = Sade ? 1 : 4;
            urp.supportsHDR = !Sade;
            urp.mainLightShadowmapResolution = Sade ? 1024 : 2048;
            GraphicsOptions.ApplyOverrides(urp);
        }
        Shader.SetKeyword(SimpleGroundKeyword, GraphicsOptions.SimpleGround ?? Sade);
        Debug.Log($"GRAFIK: {Level} | secenekler: {GraphicsOptions.Used} | arka plan olcegi {BackdropScale} | ekran {Screen.width}x{Screen.height}" +
                  $" | {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType})" +
                  (urp != null ? $" | olcek {urp.renderScale} msaa {urp.msaaSampleCount} hdr {urp.supportsHDR}" : ""));
        Changed?.Invoke();
    }
}
```
- [ ] **Step 3: BackdropCache.** `const float AnimatedRate = 20f;` → `float animatedRate = 20f;`; `LateUpdate`'te `1f / AnimatedRate` → `1f / animatedRate`. Ekle:
```csharp
    // Grafik ayari degisince: dokunun boyu (ekrana gore) ve animasyonda saniyede kac kez yenilenecegi
    public void SetQuality(float newScale, float rate)
    {
        scale = Mathf.Clamp(newScale, 0.1f, 1f);
        animatedRate = rate;
        settleUntil = 0f;   // EnsureTarget yeni boyu gorur, doku hemen yeniden cizilir
    }
```
- [ ] **Step 4: Oyun.cs.**
  - Satır 187: `GraphicsOptions.Apply();` → `GraphicsOptions.Parse();   // cizim deneme secenekleri (-grafik, -arkaplan yok ...)` + alt satırda `GraphicsQuality.Init();   // oyuncunun grafik ayari (Sade/Guzel)`.
  - Satır 299: `GraphicsOptions.BackdropScale` → `GraphicsQuality.BackdropScale`.
  - Satır 308: `skyLight.shadows = GraphicsOptions.NoShadows ? LightShadows.None : LightShadows.Soft;` satırı silinir (ApplyGraphics yapar).
  - `SetupLight();` çağrısının hemen ardından: `ApplyGraphics(); GraphicsQuality.Changed += ApplyGraphics;`
  - Yeni metod (SetAnimations'ın yanına):
```csharp
    // Grafik ayari (Sade/Guzel) uygulaninca: kameranin cizim yolu, gunes golgesi, gokyuzu dokusu
    void ApplyGraphics()
    {
        cam.GetUniversalAdditionalCameraData().SetRenderer(GraphicsQuality.RendererIndex);
        skyLight.shadows = GraphicsQuality.Shadows;
        backdropCache.SetQuality(GraphicsQuality.BackdropScale, GraphicsQuality.BackdropRate);
    }
```
  - Dosyanın başında `using UnityEngine.Rendering.Universal;` yoksa ekle.
- [ ] **Step 5:** Paketi üret (hatasız). `MarsKod.exe ... -grafik sade` ile aç, 10 sn sonra kapat; Player.log'da `GRAFIK: Sade | secenekler: -grafik sade | ... olcek 0.6 msaa 1 hdr False`. Varsayılanla açınca `GRAFIK: Guzel | secenekler: varsayilan ... olcek 1 msaa 4 hdr True`.
- [ ] **Step 6 (Review Focus 2):** `git status -s` → `PC_RPAsset.asset`/`Mobile_RPAsset.asset` değişmemiş olmalı (paket üretimi dahil). Değiştiyse farkı incele: yalnızca satır sonuysa geri al; içerik değiştiyse kopya mantığı hatalıdır, düzelt.

### Task 4: "Güzel grafik" ayar satırı

**Files:**
- Modify: `oyun/Assets/Scripts/LevelSelect.cs:20-94`
- Modify: `oyun/Assets/Scripts/Hud.cs:11, 115, 617`
- Modify: `oyun/Assets/Scripts/Oyun.cs` (Start'ta olay bağlantısı, ApplyGraphics)

**Interfaces:**
- Consumes: `GraphicsQuality.Level`, `.Set`, `.Changed`.
- Produces: `LevelSelect.GraphicsToggled`, `LevelSelect.SetGraphics(bool good)`, `Hud.GraphicsToggled`, `Hud.SetGraphics(bool good)`.

- [ ] **Step 1: LevelSelect — ortak anahtar satırı.** `animSwitch/animKnob/animationsOn/AnimationsRow/PaintSwitch` yerine:
```csharp
    public event Action AnimationsToggled, GraphicsToggled;
    readonly SwitchRow animations = new SwitchRow(true), graphics = new SwitchRow(true);

    public void Show(IReadOnlyList<Entry> entries)
    {
        Content.Clear();
        // Ayarlar: arka plandaki suregiden hareketler + grafik ayrintisi. Kapaliyken telefon daha az yorulur.
        Content.Add(animations.Build("Arka plan animasyonları", "Kapalıyken telefon daha az yorulur", fMed, fSemi, ink, accent,
                                     () => AnimationsToggled?.Invoke()));
        Content.Add(graphics.Build("Güzel grafik", "Kapalıyken (Sade) telefonda oyun daha akıcı", fMed, fSemi, ink, accent,
                                   () => GraphicsToggled?.Invoke()));
        foreach (var e in entries) Content.Add(Row(e));
        ShowPanel();
    }

    public void SetAnimations(bool on) => animations.Set(on);
    public void SetGraphics(bool good) => graphics.Set(good);

    // Ayar satiri: baslik + kucuk not + sagda ac/kapa anahtari. Satira dokununca haber verir; durumu disaridan Set ile gelir.
    class SwitchRow
    {
        bool on;
        Color accent;
        VisualElement track, knob;

        public SwitchRow(bool on) { this.on = on; }

        public VisualElement Build(string title, string note, Font fMed, Font fSemi, Color ink, Color accent, Action clicked)
        {
            this.accent = accent;
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 34;
            row.style.paddingTop = 22; row.style.paddingBottom = 22;
            row.style.paddingLeft = 36; row.style.paddingRight = 32;
            Ui.Radius(row, 36);
            Ui.Border(row, 2, new Color(1f, 1f, 1f, 0.07f));

            var texts = new VisualElement();
            texts.style.flexGrow = 1;
            texts.style.flexShrink = 1;
            texts.Add(Ui.Text(title, fSemi, 34, ink));
            var noteText = Ui.Text(note, fMed, 26, new Color(ink.r, ink.g, ink.b, 0.55f));
            noteText.style.marginTop = 4;
            noteText.style.whiteSpace = WhiteSpace.Normal;
            texts.Add(noteText);
            row.Add(texts);

            track = new VisualElement();
            track.style.width = 120; track.style.height = 68;
            track.style.flexShrink = 0;
            track.style.marginLeft = 24;
            Ui.Radius(track, 34);
            track.style.justifyContent = Justify.Center;
            knob = new VisualElement();
            knob.style.width = 52; knob.style.height = 52;
            Ui.Radius(knob, 26);
            knob.style.backgroundColor = Color.white;
            track.Add(knob);
            row.Add(track);
            Paint();

            row.RegisterCallback<ClickEvent>(_ => { Sound.Tap(); clicked(); });
            return row;
        }

        public void Set(bool value) { on = value; Paint(); }

        void Paint()
        {
            if (track == null) return;
            track.style.backgroundColor = on ? accent : new Color(1f, 1f, 1f, 0.16f);
            knob.style.marginLeft = on ? 60 : 8;
        }
    }
```
Dosya başı yorumundaki "ustte ayar satiri (arka plan animasyonlari)" → "ustte ayar satirlari (arka plan animasyonlari, grafik)". `RowCenter` yalnızca `int` userData'lı satırları aradığı için ayar satırlarından etkilenmez.
- [ ] **Step 2: Hud.** Olay satırına `GraphicsToggled` ekle; 115'in altına `levelSelect.GraphicsToggled += () => GraphicsToggled?.Invoke();`; 617'nin altına `public void SetGraphics(bool good) => levelSelect.SetGraphics(good);`.
- [ ] **Step 3: Oyun.** `hud.AnimationsToggled += ...` bloğunun altına:
```csharp
        hud.GraphicsToggled += () =>
            GraphicsQuality.Set(GraphicsQuality.Level == GraphicsLevel.Sade ? GraphicsLevel.Guzel : GraphicsLevel.Sade);
        hud.SetGraphics(GraphicsQuality.Level == GraphicsLevel.Guzel);
```
`ApplyGraphics()` sonuna: `if (hud != null) hud.SetGraphics(GraphicsQuality.Level == GraphicsLevel.Guzel);` (ilk çağrıda hud henüz yok).
- [ ] **Step 4 (Review Focus 1 ve 3): Elle deneme.** Paketi üret; `-grafik sade` ile aç → Bölümler → "Güzel grafik" anahtarına dokun: sahne anında keskinleşir, zeminde kraterler gelir, log'da yeni `GRAFIK: Guzel` satırı. Tekrar dokun → Sade. Oyunu kapat, `-grafik` olmadan aç → son seçim (Sade) hatırlanmış (`GRAFIK: Sade | secenekler: varsayilan`). Sonra Güzel'e geri al (bilgisayar varsayılanı bozulmasın). Geçişlerde gökyüzünde eski boyun bulanıklığı/kenar kayması kalmamalı.

### Task 5: Ölçüm — işlemci/ekran kartı süresi, KARE satırı, -arayuz yok, -cevre yok

**Files:**
- Modify: `oyun/Assets/Scripts/FrameRateMeter.cs:14-15, 60-75`
- Modify: `oyun/Assets/Scripts/Hud.cs` (yeni `HideForMeasurement()`)
- Modify: `oyun/Assets/Scripts/Oyun.cs:206, 394`

**Interfaces:** Consumes: `GraphicsQuality.Level`, `GraphicsOptions.NoHud`, `.NoScenery`.

- [ ] **Step 1: FrameRateMeter Update** (pencere her zaman ölçülür; gösterge kapalıyken de log yazılır):
```csharp
    const float LogEvery = 5f;   // log'a KARE satiri (telefonda: adb logcat -d -s Unity | findstr KARE)
    float logTime;
    readonly FrameTiming[] timing = new FrameTiming[1];
    double cpuSum, gpuSum;
    int cpuCount, gpuCount;

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
            Debug.Log($"KARE: {fps:0} kare/sn | en uzun {slowest * 1000f:0} ms | islemci {cpu} ms | ekran karti {gpu} ms | grafik {GraphicsQuality.Level}");
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
```
Sınıf başı yorumuna "islemci/ekran karti suresi + 5 sn'de bir log'a KARE satiri" ekle.
- [ ] **Step 2: Hud.**
```csharp
    // Olcum icin (-arayuz yok): arayuz cizilmez ama yerlesimi durur, sahnenin kamera hesabi degismez
    public void HideForMeasurement() => root.visible = false;
```
- [ ] **Step 3 (Review Focus 4):** `-fps` ile aç: göstergede iki satır; sayıların yerinde "—" çıkarsa çökme yok (Windows DX11'de genelde ikisi de dolu). Player.log'da 5 sn arayla `KARE:` satırları.
- [ ] **Step 4 (Review Focus 5): Oyun.** `hud.Build();` altına `if (GraphicsOptions.NoHud) hud.HideForMeasurement();`. Satır 394: `if (!GraphicsOptions.NoScenery) Scenery.Build(...);`. Paketi üret; `-arayuz yok -fps` ile aç: arayüz yok, Mars sahnesi normal kadrajda, gösterge görünür (kendi paneli), çökme yok. `-cevre yok` ile aç: alan çevresinde kaya yok.

### Task 6: Doğrulama, görüntüler, telefon paketi, belgeler

**Files:**
- Create: `docs/tasarim/telefon-sade-2026-10-06/` (görüntüler)
- Modify: `docs/tasarim/telefon-denemesi.md`, `ILERLEME.md`

- [ ] **Step 1:** `cd motor-test && dotnet test` → 1285 geçti.
- [ ] **Step 2:** Bilgisayara dokunmadan iki `-shots` koşusu (varsayılan = Güzel ve `-grafik sade`), ayrı klasörlere. Log'da yedi denetim `TAMAM`. `b1-1-bekleme.png`, `b11-1-bekleme.png`, `b21-1-bekleme.png` iki koşudan `guzel-` / `sade-` önekiyle `docs/tasarim/telefon-sade-2026-10-06/`'ya kopyalanır. Klavye denetimi: `powershell -ExecutionPolicy Bypass -File scripts/klavye-denetimi.ps1` → `TAMAM`.
- [ ] **Step 3:** Görüntüleri incele: Sade'de kare çizgileri, buz lekeleri, robot gölgesi, ufka karışma görünür; Güzel bugünkü görüntülerle aynı. Forward geçişi görünür bir fark yaptıysa (ışık, gölge) not et.
- [ ] **Step 4:** Android paketi: `-executeMethod OyunBuild.BuildAndroid` → `oyun/Build/Android/marskod.apk`, `paketler/marskod-oyun.apk`'ya kopyala. Sanal telefon (`MarsKod_Telefon`) varsa kur, aç; `adb logcat -d -s Unity | findstr "GRAFIK KARE"` → `GRAFIK: Sade` ve `KARE:` satırları.
- [ ] **Step 5:** `telefon-denemesi.md`: üstte "Ne değişti (2026-10-06, Enes)" — Sade varsayılan, ayar, ölçüm. Deneme sırası tablosu yenilenir:

| Adım | Ne dener | Seçenekler |
|---|---|---|
| 1 | Yeni hâli (telefonda Sade) | *(boş)* |
| 2 | Arayüzün payı | `-arayuz yok` |
| 3 | Çevredeki kaya/eşyaların payı | `-cevre yok` |
| 4 | Gölgenin payı | `-golgesiz` |
| 5 | Güzel ile karşılaştırma | `-grafik guzel` |

Her adımda: 1 dk bekle, `adb logcat -d -s Unity | findstr KARE` çıktısının son 3 satırını gönder. Okuma: "ekran kartı" süresi büyükse çizim ağır; "işlemci" büyükse kod/çizim komutu çok. 30 kare/sn ≈ kare başına 33 ms. Seçenek listesine `-grafik`, `-arayuz`, `-cevre` eklenir.
- [ ] **Step 6:** `ILERLEME.md`: en üste Enes girdisi (ne yapıldı, doğrulananlar, Hamza'nın yapacağı, Ragıp'ın görsel onayı, "commit edilmedi"); "En son (özet)" ve "SIRADAKİ İŞ" güncellenir.
