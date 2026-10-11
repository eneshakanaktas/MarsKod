# Bölge 6 Görünüşü — 3b üç sahne · Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bölüm 60 (kanyona iniş, final), 55 (Serçe'nin ilk uçuşu) ve 51 (roketin inişi, ilk açılışta bir kez) sahnelerini oyuna koymak.

**Architecture:** Kamera sahne sürerken yeni `CameraShot`'a geçer (Oyun.FitCamera durur; ekrandaki gökyüzü resminin ufku kameraya göre yeniden hesaplanır), sahne bitince ya da "Tekrar"da geri verilir. Sahnelerin akışı `CanyonScenes.cs`'te; nesneler kendi dosyalarında yalnızca komut alır (roket `Land/OpenDoor`, Serçe `WakeUp/TakeOff/FlyTo`, yarık `Trail`). Robotun boş karelerden yolu saf C# `GridPath` (Dunya, `dotnet test`). 60'ta zemin yarığın üstünde çizim sırasında kesilir (`clip`), yarık ayrı 3B ağdır.

**Tech Stack:** Unity 6 (6000.6.3f1, URP), C# 9, HLSL; `Dunya/` için .NET Standard 2.1 + xUnit (`motor-test`).

**Spec:** `docs/superpowers/specs/2026-10-08-bolge-6-design.md` → §3b (3b.1-3b.5). Hikâye: `docs/tasarim/senaryo-bolge-06.md`.

## Global Constraints

- Sis yok; derinlik **gölgeyle**. Nesneler zemine basar (`TerrainHeight`). Dünyada bilgisayar yazısı yok. Robotlar konuşmaz. **Kıvılcım uçmaz**: yerden kalkmaz, engel (kaya, panel, kristal, makara, toz) üstünden geçmez. Roket otomatik.
- Her şey kodla çizilir; hazır model/doku/ses dosyası yok (paket boyutu, Ragıp).
- Unity C# 9; `Dunya/` UnityEngine kullanmaz. Yeni her `.cs`'in `.meta`'sı Unity'de üretilir ve git'e girer.
- `Oyun.cs` büyütülmez: yalnızca çağrı/bağlantı satırları. Sahne görüntü denetimi yeni `Oyun.SahneDenetimi.cs`'te (`partial class Oyun`; `Oyun.cs`'teki sınıf bildirimi `public partial class Oyun` olur).
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıflar, tekrar yok, anlamlı adlar, ölçülü soyutlama. Yorumlar Türkçe ASCII (ç/ş yerine c/s), dosya başında ne olduğu.
- **Ara görüntü kuralı (spec 3b.5):** Task 1, 2, 3, 4 sonunda Enes'e görüntü gösterilir; **Enes onaylamadan sonraki göreve geçilmez.** Beğenmezse aynı görevde düzeltilir.
- **Commit:** ekip döngüsü gereği görev sonlarında commit **yok**; her şey oturum kapanışında tek commit + push (ILERLEME.md ile birlikte).
- Denetim komutları:
  - Paket: `rm -rf oyun/Build && "/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log`; `grep "error CS" build.log` ve `grep -i "shader error" build.log` boş olmalı. Unity Hub/Editor işi bitince kapatılır.
  - Sahne görüntüleri: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -shots <scratch>/<klasör> -sahneler` (yalnızca sahne kareleri, birkaç dakika; log'da `SAHNE DENETIMI:` satırı).
  - Normal bölüm görüntüleri: aynı komut `-sahneler` yerine `-bolum 51` (51-60).
  - `cd motor-test && dotnet test` → hepsi geçer (şu an 1395).

## Review Focus

1. **Sahne ortasında "Tekrar" (↻) ya da bölüm seçme:** kamera serbest kalmalı, robot görünür ve başlangıçta, Serçe evinde (55: rafta sönük; 60: köşesinde), roket yerde. → Task 2 `SAHNE DENETIMI` sahnenin ortasında `ResetLevel` çağırıp bunları denetler; Task 3 ve 4 aynı denetime kendi sahnesini ekler.
2. **Başka bölgeye/bölüme sızma:** yarık, zemin kesmesi ve gökyüzü çizgisi yalnızca 60'ta. → Task 1 sonunda 41, 59 ve 51 normal görüntüleri `docs/tasarim/bolge-6-gorunus-2026-10-09/` ve `bolge-5-bulmacalar-2026-10-07/b41-1-bekleme.png` ile karşılaştırılır.
3. **Kamera hareketinde gökyüzü kayması:** kamera dönerken arka plan resmi ufukla birlikte gitmeli (ufuk zemin kenarından kopmamalı). → Task 1 aşağı bakış karesi + Task 2 hareket ortası karesi.
4. **Yol bulunamaz:** engeller yolu kapatırsa sahne takılmaz; robot doğrudan son karesine konur (kayaların üstünden yürümez). → Task 2 `GridPath` testi (`Yol_yoksa_null`) + `CanyonScenes.Walk` geri dönüşü.
5. **Performans modu ve klavye açık bitiriş:** 60 yarığı `MARSKOD_SADE` zemininde de kesik; klavye açıkken bölüm bitince (fotoğraf küçülmüş) sahne kamerası yine doğru yerden başlar. → Task 1 `-performans ac` ile 60 karesi; Task 5'te klavye açık 60 bitirişi.

---

### Task 1: Sahne kamerası + 60'ın yarığı (en riskli parça; görüntü onayı)

**Files:**
- Create: `oyun/Assets/Scripts/CameraShot.cs`
- Create: `oyun/Assets/Scripts/CanyonRift.cs`
- Create: `oyun/Assets/Scripts/Oyun.SahneDenetimi.cs`
- Modify: `oyun/Assets/Scripts/Oyun.cs` (sınıf `partial`; `CameraShot shot` alanı; `LateUpdate`; `-sahneler` argümanı)
- Modify: `oyun/Assets/Shaders/Ground.shader` (`canyonDescent`: patika kenarda biter; yeni `riftClip`, iki sürümde de)
- Modify: `oyun/Assets/Shaders/CanyonSky.hlsl` (`canyonPathToMouth` → koyu yarık çizgisi)
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs` (60'ta `CanyonRift.Build`; `public static CanyonRift Rift`)
- Modify: `oyun/Assets/Scripts/CanyonLedge.cs` (kenar kayaları yarık ağzını açık bırakır; `RimRock` → `internal static`)

**Interfaces:**
- Produces:
  - `public class CameraShot` — `CameraShot(Camera cam)`, `bool Active`, `void Begin()`, `IEnumerator MoveTo(Vector3 position, Vector3 lookAt, float seconds)`, `void Cut(Vector3 position, Quaternion rotation)`, `float Sway`, `void Apply()`, `void Release()`.
  - `public class CanyonRift : MonoBehaviour` — `static CanyonRift Build(Transform parent, float startZ, float pathX, Func<float,float,float> ground)`, `IReadOnlyList<Vector3> Trail` (yerel, kenardan dibe), `int BendIndex` (Trail'de dönemeç kayasının arkasına geçilen nokta), `Vector3 ViewPoint`, `Vector3 ViewTarget`, `static float CenterX(float t, float pathX)`, `static float HalfWidth(float t)`, `static float Depth(float t)`; `const float Length = 9f`.
  - `CanyonTraces.Rift` (60'ta dolu, diğerlerinde null).

- [ ] **Step 1: `CameraShot.cs`** — kamera ve gökyüzü resmini birlikte yönetir:

```csharp
using System.Collections;
using UnityEngine;

// Sahne kamerasi (Bolge 6 sahneleri, spec 3b.1): sahne surerken kamerayi Oyun.FitCamera yerine bu yonetir.
// Gokyuzu resmi (MarsSky.hlsl) ekranda _Horizon/_Picture ile yerlesir; kamera donunce resmin ufku da gercek ufukla
// birlikte kaysin diye her karede yeniden hesaplanir. Begin normal kameranin halini saklar, Release geri verir.
public class CameraShot
{
    static readonly int HorizonId = Shader.PropertyToID("_Horizon");
    static readonly int PictureId = Shader.PropertyToID("_Picture");

    readonly Camera cam;
    Vector3 position;
    Quaternion rotation;
    float fov, shift, horizonOffset, swayTime;

    public bool Active { get; private set; }
    // Yukaridan bakista (55) ucan bir seyden bakiyormus gibi hafif salinim (derece)
    public float Sway { get; set; }

    public CameraShot(Camera cam) { this.cam = cam; }

    // Normal kameranin o anki halini alir: sahne buradan baslar, ufuk farki burada olculur.
    public void Begin()
    {
        if (Active) return;
        position = cam.transform.position;
        rotation = cam.transform.rotation;
        fov = cam.fieldOfView;
        var picture = Shader.GetGlobalVector(PictureId);
        shift = picture.y;
        float screenHorizon = ((2f * Shader.GetGlobalFloat(HorizonId) - 1f) * picture.x + picture.y + 1f) * 0.5f;
        horizonOffset = screenHorizon - GeometricHorizon();
        Sway = 0f;
        Active = true;
    }

    public IEnumerator MoveTo(Vector3 to, Vector3 lookAt, float seconds)
    {
        Vector3 fromPos = position;
        Quaternion fromRot = rotation, toRot = Quaternion.LookRotation(lookAt - to);
        float fromShift = shift;
        yield return Tween.Run(seconds, t =>
        {
            float e = Tween.InOutCubic(t);
            position = Vector3.Lerp(fromPos, to, e);
            rotation = Quaternion.Slerp(fromRot, toRot, e);
            shift = Mathf.Lerp(fromShift, 0f, e);
        });
    }

    public void Cut(Vector3 to, Quaternion rot) { position = to; rotation = rot; shift = 0f; }

    // Oyun.LateUpdate her karede cagirir (Active iken FitCamera yerine)
    public void Apply()
    {
        swayTime += Time.deltaTime;
        var sway = Quaternion.Euler(Sway * Mathf.Sin(swayTime * 1.7f), 0f, Sway * 0.7f * Mathf.Sin(swayTime * 1.1f + 1f));
        cam.transform.SetPositionAndRotation(position, rotation * sway);
        cam.fieldOfView = fov;
        cam.ResetProjectionMatrix();
        var p = cam.projectionMatrix;
        p[1, 2] = -shift;
        cam.projectionMatrix = p;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 80f;
        float screenHorizon = GeometricHorizon() + horizonOffset;
        Shader.SetGlobalVector(PictureId, new Vector4(1f, shift, 0f, 0f));
        Shader.SetGlobalFloat(HorizonId, ((2f * screenHorizon - 1f - shift) + 1f) * 0.5f);
    }

    public void Release() { Active = false; Sway = 0f; }

    // Kameranin yuksekliginde, tam onunde cok uzaktaki noktanin ekrandaki yuksekligi: duz zeminin gercek ufku
    float GeometricHorizon()
    {
        var flat = cam.transform.forward; flat.y = 0f;
        if (flat.sqrMagnitude < 1e-4f) flat = Vector3.forward;   // tam asagi bakis (55): ufuk ekranin disinda
        return cam.WorldToViewportPoint(cam.transform.position + flat.normalized * 1000f).y;
    }
}
```

- [ ] **Step 2: `Oyun.cs` bağlantısı** — sınıf bildirimini `public partial class Oyun : MonoBehaviour` yap; alan `CameraShot shot;` (Camera alanının yanına, yorum: `// Bolge 6 sahneleri: sahne surerken kamerayi bu yonetir`); kamera kurulduktan hemen sonra `shot = new CameraShot(cam);`; `LateUpdate` içinde `FitCamera();` satırını `if (shot.Active) shot.Apply(); else FitCamera();` yap. `LoadLevel` başında ve `ResetLevel` içinde `shot.Release();`. `Start`taki argüman satırına `-sahneler`: 

```csharp
        bool scenes = System.Array.IndexOf(a, "-sahneler") >= 0;   // yalnizca Bolge 6 sahnelerinin kareleri
        ...
            if (a[i] == "-shots") { shotsMode = true; LoadLevel(levelIndex); StartCoroutine(scenes ? SceneShots(a[i + 1]) : finale ? FinaleShots(a[i + 1]) : Shots(a[i + 1])); }
```

- [ ] **Step 3: `CanyonRift.cs`** — yarığın ölçüleri (zemin çiziminde aynı formüller; ikisi birlikte değişir) ve 3B ağ:

```csharp
// Bolum 60: direklerin hemen otesinden baslayip ufuktaki kanyon agzina giden dar yarik (spec 3b.2). Zemin bu seridin
// ustunde cizilmez (Ground.shader riftClip, ayni formuller). Duvarlar yatay katman katman, asagi indikce ortaya basamaklanir;
// sag duvara oyulmus patika kenardan dibe iner, dipte doneme kayasinin arkasina gecer (robot orada gozden kaybolur).
// Isik gercek gunesten: gunes solda alcak oldugundan sol duvar ve diplerin cogu golgede kalir (sis yok).
public class CanyonRift : MonoBehaviour
{
    public const float Length = 9f;                 // kenardan (t = 0) yarigin kapandigi uca
    public static float CenterX(float t, float pathX) => Mathf.Lerp(pathX, 0f, Mathf.SmoothStep(0f, 1f, t / 7f));
    public static float HalfWidth(float t) => (0.5f + 0.06f * t) * (1f - Mathf.SmoothStep(0f, 1f, (t - (Length - 2f)) / 2f));
    public static float Depth(float t) => Mathf.Min(1.4f + 0.45f * t, 4.5f);
    ...
}
```

Ağ: `t` 0'dan `Length`'e 0,25 adımla; her iki yan (`s = -1, +1`) için kenardan (`y = ground(x, z)`) dibe (`y = ground(cx, z) - Depth(t)`) inen şerit, profil (derinlik oranı → içe kayma): `(0,0) (0.3,0.04) (0.33,0.12) (0.62,0.15) (0.65,0.22) (1,0.24)`; her katman ayrı alt ağ, iki renk sırayla (`#7A4530` / `CanyonLedge.RimRock`), dip `#3A2018` düz şerit. Hepsi gölge düşürür ve alır. Patika: sağ duvarda 0,35 genişlikte raf, `t = 0.3`'te kenardan başlar, `t = 5.5`'te dibe varır, dipte `t = 7`'ye gider; rafın üst yüzü + ön yüzü, renk `#BD8561` (zemindeki patika rengi). `Trail`: raf ortasından her 0,5 `t`'de bir nokta (+ zemindeki patikadan rafa giriş noktası başta). `t ≈ 6.4`'te dipte iri bir çıkıntı kayası (`MeshFactory.Rock(0.45f, 700)`, `RimRock`); `BendIndex` = Trail'de kayayı geçen ilk nokta. `ViewPoint = (pathX, ground(pathX, startZ) + 2.4f, startZ - 0.6f)`, `ViewTarget = Trail'in t≈4 noktası`; Step 7'de görüntüye bakılarak ayarlanır.

- [ ] **Step 4: Zemin kesmesi (`Ground.shader`)** — `canyonDescent`'in yanına:

```hlsl
            // Bolum 60: yarigin ustunde zemin cizilmez (CanyonRift.cs ile ayni olculer: CenterX, HalfWidth, Length)
            void riftClip(float2 q)
            {
                if (_CanyonDescent.y < 0.5) return;
                float t = q.y - _CanyonDescent.x;
                if (t <= 0.0 || t >= 9.0) return;
                float cx = lerp(_CanyonDescent.z, 0.0, smoothstep(0.0, 7.0, t));
                float hw = (0.5 + 0.06 * t) * (1.0 - smoothstep(0.0, 1.0, (t - 7.0) / 2.0));
                clip(hw - abs(q.x - cx));
            }
```

Her iki `frag`'ın (tam + `MARSKOD_SADE`) başında `riftClip(i.posWS.xz);`. Not: HLSL `smoothstep(0,1,x)` = C# `Mathf.SmoothStep(0,1,x)` (aynı eğri); `smoothstep(0,7,t)` = `SmoothStep(0,1,t/7)`. Shader'da `ShadowCaster`/`DepthOnly` geçişi varsa onlara da eklenir (zemin gölge düşürmüyor, yine de kontrol et). `canyonDescent`'te patika kenarda biter: `float along = smoothstep(startZ - 0.1, startZ + 0.2, q.y) * (1.0 - smoothstep(edgeZ - 0.1, edgeZ + 0.15, q.y));` ve `toMouth` kıvrılması silinir (`cx = pathX + 0.15 * sin(...)`).

- [ ] **Step 5: Gökyüzü çizgisi (`CanyonSky.hlsl`)** — `canyonPathToMouth` artık açık patika değil, kanyon ağzına giden ince koyu yarık: `halfWidth = 0.12 * d`, renk `lerp(CANYON_PLAIN, float3(0.22, 0.13, 0.11), 0.85 * saturate(d / 0.04))`; adı `canyonRiftToMouth`, yorum güncellenir. `CANYON_PATH` başka yerde kullanılmıyorsa silinir.

- [ ] **Step 6: Yerleşim** — `CanyonTraces.Build` `case LastLevel:` içinde `Rift = CanyonRift.Build(parent, areaHalf.y + DescentGap, DescentPathX, ground);` (`Build` başında `Rift = null;`). `CanyonLedge.Edge` kaya atlama koşulu `Mathf.Abs(x - pathX) < Mathf.Max(PathMouth, CanyonRift.HalfWidth(0f) + 0.15f)` (iki yerde). Kenardaki zemin oku (`PaintMarks.Arrow(pathHead…)`) yerinde kalır.

- [ ] **Step 7: `Oyun.SahneDenetimi.cs` ilk hâli + görüntü**

```csharp
using System.Collections;
using System.IO;
using UnityEngine;

// Bolge 6 sahnelerinin goruntu denetimi (-shots <klasor> -sahneler): sahnelerin ara kareleri + SAHNE DENETIMI satiri.
public partial class Oyun
{
    IEnumerator SceneShots(string dir)
    {
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2.5f);
        var problems = new System.Collections.Generic.List<string>();
        yield return RiftShots(dir, problems);
        Debug.Log("SAHNE DENETIMI: " + (problems.Count == 0 ? "TAMAM" : "HATA " + string.Join("; ", problems)));
        Application.Quit();
    }

    IEnumerator RiftShots(string dir, System.Collections.Generic.List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "s60-0-normal.png"));
        if (CanyonTraces.Rift == null) { problems.Add("60'ta yarik yok"); yield break; }
        shot.Begin();
        var rift = CanyonTraces.Rift;
        shot.Cut(rift.ViewPoint, Quaternion.LookRotation(rift.ViewTarget - rift.ViewPoint));
        yield return new WaitForSeconds(1.5f);   // arka plan yeniden cizilsin
        Cap(Path.Combine(dir, "s60-1-asagi.png"));
        shot.Release();
    }
}
```

(`Cap` ve `Application.Quit` kullanımı `FinaleShots` ile aynı biçimde; `FinaleShots`'ın sonunu oku ve aynı kapanışı kullan.)

- [ ] **Step 8: Derle, görüntü al, bak** — paket; `-sahneler` ve `-sahneler -performans ac` (ayrı klasör); `-bolum 59` (59 ve 60 normal kareleri); `-bolum 41`in ilk karesi için `-bolum 41` turu 41-1'den sonra kapatılabilir. Bak: (a) normal bakışta yarık direklerin hemen ötesinden başlıyor, kanyon ağzına inceliyor, gökyüzündeki koyu çizgiyle dikişsiz birleşiyor, kenar kayaları ağzı kapatmıyor; (b) aşağı bakışta katmanlı duvarlar, sağ duvarda patika, sol duvar ve dip gölgede, dipte zemin "deliği"nden siyah/boşluk görünmüyor, gökyüzü resmi ufukla birlikte doğru yerde; (c) 41, 51, 59 öncekiyle aynı; (d) Performans modunda yarık kesik. Gerekirse `ViewPoint/ViewTarget`, derinlik ve renkler ayarlanır.
- [ ] **Step 9: ENES ONAYI** — `s60-0-normal`, `s60-1-asagi` (+ performans karesi) Enes'e gösterilir. Onaylamadan Task 2'ye geçilmez.

---

### Task 2: Boş karelerden yol + Bölüm 60 sahnesi (görüntü onayı)

**Files:**
- Create: `oyun/Assets/Dunya/GridPath.cs`
- Create: `motor-test/GridPathTests.cs`
- Create: `oyun/Assets/Scripts/CanyonScenes.cs`
- Modify: `oyun/Assets/Scripts/Sparrow.cs` (`FlyTo`, `ResetHome`)
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs` (`ResetBackdrop` Serçe'yi evine döndürür)
- Modify: `oyun/Assets/Scripts/Oyun.cs` (`CanyonScenes scenes` alanı; kapanış çağrısı; `LoadLevel`/`ResetLevel`'da `scenes.Restore()`)
- Modify: `oyun/Assets/Scripts/Oyun.SahneDenetimi.cs` (60 sahnesi kareleri + geri alma denetimi)

**Interfaces:**
- Consumes: `CameraShot` (Task 1), `CanyonTraces.Rift`, `CanyonRift.Trail/BendIndex/ViewPoint/ViewTarget`.
- Produces:
  - `MarsKod.Dunya.GridPath` — `static List<Cell> Find(int cols, int rows, Cell from, Cell to, ICollection<Cell> blocked)` (from ve to dahil; yoksa `null`; `to` engelliyse `null`), `static Cell? OpenInRow(int cols, int row, int preferCol, ICollection<Cell> blocked)` (satırda `preferCol`'a en yakın boş kare), `static HashSet<Cell> Obstacles(Level level, bool itemsTaken)` (kaya, panel, kristal, makara, toz; `itemsTaken` false ise toplanacaklar da).
  - `Sparrow` — `IEnumerator FlyTo(Vector3 localPos, float seconds)`, `void ResetHome()` (Create'teki yer ve hâl; koşan hareketleri durdurur).
  - `public class CanyonScenes` — `CanyonScenes(Robot robot, CameraShot shot, Hud hud, Func<Cell, Vector3> cellPos, int cols, int rows, float startYaw)` (son hâli baştan; `hud` Task 3, `startYaw` Task 4 kullanır), `string Phase`, `IEnumerator Descent(Level level)`, `void Restore()`; içte `IEnumerator Walk(IReadOnlyList<Vector3> points, float secondsPerUnit)`.

- [ ] **Step 1: Başarısız testleri yaz** (`motor-test/GridPathTests.cs`):

```csharp
using System.Collections.Generic;
using MarsKod.Dunya;

namespace MotorTest;

// Sahnelerde robotun yalnizca bos karelerden yurudugu yol (Bolge 6, 51 ve 60): en kisa, engelin ustunden gecmez.
public class GridPathTests
{
    static readonly HashSet<Cell> None = new HashSet<Cell>();

    [Fact]
    public void Duz_yol_en_kisadir()
    {
        var path = GridPath.Find(6, 6, new Cell(0, 0), new Cell(3, 0), None);
        Assert.Equal(new[] { new Cell(0, 0), new Cell(1, 0), new Cell(2, 0), new Cell(3, 0) }, path);
    }

    [Fact]
    public void Kayanin_etrafindan_dolasir()
    {
        var path = GridPath.Find(6, 6, new Cell(0, 0), new Cell(2, 0), new HashSet<Cell> { new Cell(1, 0) });
        Assert.Equal(5, path.Count);
        Assert.DoesNotContain(new Cell(1, 0), path);
    }

    [Fact]
    public void Yol_yoksa_null()
    {
        var wall = new HashSet<Cell> { new Cell(1, 0), new Cell(1, 1), new Cell(0, 1) };
        Assert.Null(GridPath.Find(6, 6, new Cell(0, 0), new Cell(5, 5), wall));
    }

    [Fact]
    public void Ayni_karede_tek_kare()
    {
        Assert.Equal(new[] { new Cell(2, 2) }, GridPath.Find(6, 6, new Cell(2, 2), new Cell(2, 2), None));
    }

    [Fact]
    public void Satirda_tercihe_en_yakin_bos_kare()
    {
        var blocked = new HashSet<Cell> { new Cell(4, 5) };
        Assert.Equal(new Cell(3, 5), GridPath.OpenInRow(6, 5, 4, blocked));   // esitlikte sola
        Assert.Null(GridPath.OpenInRow(2, 0, 0, new HashSet<Cell> { new Cell(0, 0), new Cell(1, 0) }));
    }

    [Fact]
    public void Bolum_60_bitince_hedeften_arka_siraya_yol_var()
    {
        var level = Bolumler.Yukle(60);   // BolumTests'teki bolum okuma yardimcisini kullan (adi farkliysa onu)
        var blocked = GridPath.Obstacles(level, itemsTaken: true);
        var exit = GridPath.OpenInRow(level.Cols, level.Rows - 1, 4, blocked);
        Assert.NotNull(exit);
        Assert.NotNull(GridPath.Find(level.Cols, level.Rows, level.Target.Value, exit.Value, blocked));
    }

    [Fact]
    public void Bolum_51_baslangica_sol_arka_siradan_yol_var()
    {
        var level = Bolumler.Yukle(51);
        var blocked = GridPath.Obstacles(level, itemsTaken: false);
        var entry = GridPath.OpenInRow(level.Cols, level.Rows - 1, 0, blocked);
        Assert.NotNull(entry);
        Assert.NotNull(GridPath.Find(level.Cols, level.Rows, entry.Value, level.Robot, blocked));
    }
}
```

(`Bolumler.Yukle`: `BolumTests.cs`'te bölüm dosyasını okuyan yardımcı neyse onu kullan; yoksa `Level.Parse(File.ReadAllText(".../bolum-60.json"))` biçimindeki mevcut kalıbı kopyalama, ortak yardımcıyı çağır.)

- [ ] **Step 2:** `cd motor-test && dotnet test --filter GridPathTests` → derleme hatası (GridPath yok).
- [ ] **Step 3: `GridPath.cs`** (namespace `MarsKod.Dunya`, saf C#): 4 yönlü genişlik öncelikli arama (BFS), komşu sırası Kuzey, Doğu, Güney, Batı (sonuç her koşuda aynı); `Obstacles` = `level.Rocks ∪ level.Panels.Keys ∪ level.Crystals ∪ level.Reels.Keys ∪ level.Dust.Keys` (+ `!itemsTaken` ise `level.Ices`). Dosya başı yorumu: ne için (sahne yürüyüşü, oyun kuralı değil).
- [ ] **Step 4:** `dotnet test` → hepsi geçer (1395 + 7).
- [ ] **Step 5: `Sparrow`** — `Vector3 home; SparrowState homeState;` Create'te saklanır. `FlyTo`: `Tween.Run(seconds, t => transform.localPosition = Vector3.Lerp(from, to, Tween.InOutCubic(t)))`. `ResetHome`: `StopAllCoroutines(); gameObject.SetActive(true); transform.localPosition = home; SetState(homeState);`. `CanyonTraces.ResetBackdrop` içinde `if (ActiveSparrow != null) ActiveSparrow.ResetHome();`.
- [ ] **Step 6: `CanyonScenes.cs` — ortak + 60**

```csharp
// Bolge 6'nin uc sahnesi (spec 3b): 60 kanyona inis, 55 Serce'nin ilk ucusu, 51 roketin inisi. Akis burada; nesneler
// (yarik, Serce, roket) yalnizca komut alir. Kamera sahne surerken CameraShot'ta; Restore her seyi geri verir.
// Kivilcim ucmaz: yalnizca bos karelerden (GridPath) ve patikadan yurur.
public class CanyonScenes
{
    readonly Robot robot;
    readonly CameraShot shot;
    readonly Hud hud;
    readonly Func<Cell, Vector3> cellPos;
    readonly int cols, rows;
    readonly float startYaw;   // Oyun.StartYaw: robotun baslangic yonu
    public string Phase { get; private set; } = "";   // goruntu denetimi hangi anda oldugunu buradan okur

    public CanyonScenes(Robot robot, CameraShot shot, Hud hud, Func<Cell, Vector3> cellPos, int cols, int rows, float startYaw) { ... }

    // 60: hedeften bos karelerle arka siraya, patikadan direklerin arasina, yariga inis; kamera yariga bakar
    public IEnumerator Descent(Level level)
    {
        var rift = CanyonTraces.Rift;
        var sparrow = CanyonTraces.ActiveSparrow;
        if (rift == null) yield break;
        Phase = "kalkis";
        if (sparrow != null) yield return sparrow.FlyTo(robot.transform.localPosition + Vector3.up * 1.2f, 1f);
        Phase = "yol";
        var cells = CellsTo(level, level.Target ?? level.Robot, ExitCell(level), itemsTaken: true);
        var walk = new List<Vector3>(cells.ConvertAll(c => cellPos(c)));
        walk.Add(rift.Trail[0]);
        if (sparrow != null) sparrow.StartCoroutine(Follow(sparrow, 0.5f));
        yield return Walk(walk, 0.45f);
        Phase = "bakis";
        shot.Begin();
        yield return shot.MoveTo(rift.ViewPoint, rift.ViewTarget, 3f);
        Phase = "inis";
        yield return Walk(rift.Trail.Take(rift.BendIndex + 1).ToList(), 0.5f);
        robot.gameObject.SetActive(false);
        if (sparrow != null) { sparrow.StopAllCoroutines(); yield return sparrow.FlyTo(rift.Trail[rift.BendIndex] + Vector3.up * 0.3f, 1.2f); sparrow.gameObject.SetActive(false); }
        Phase = "son";
    }

    public void Restore()
    {
        robot.gameObject.SetActive(true);
        shot.Release();
        Phase = "";
    }

    // Serce robotun biraz onunde ve ustunde kalir (yol gosterir gibi)
    IEnumerator Follow(Sparrow sparrow, float lead) { while (true) { var target = robot.transform.localPosition + robot.transform.forward * lead + Vector3.up * 1.1f; sparrow.transform.localPosition = Vector3.Lerp(sparrow.transform.localPosition, target, 1f - Mathf.Exp(-Time.deltaTime * 3f)); yield return null; } }

    // Noktadan noktaya: once doner, sonra yurur (hiz birim basina)
    IEnumerator Walk(IReadOnlyList<Vector3> points, float secondsPerUnit)
    {
        foreach (var p in points)
        {
            var from = robot.transform.localPosition;
            if ((p - from).sqrMagnitude < 1e-4f) continue;
            yield return robot.TurnTo(DuneFinale.YawTowards(from, p), 0.2f);
            yield return robot.MoveTo(p, Vector3.Distance(from, p) * secondsPerUnit);
        }
    }

    // Bos karelerden yol; yoksa (engeller kapatmis) robot dogrudan son kareye konur: sahne takilmaz
    List<Cell> CellsTo(Level level, Cell from, Cell to, bool itemsTaken)
    {
        var path = GridPath.Find(cols, rows, from, to, GridPath.Obstacles(level, itemsTaken));
        return path ?? new List<Cell> { to };
    }

    Cell ExitCell(Level level) =>
        GridPath.OpenInRow(cols, rows - 1, ColNear(CanyonTraces.DescentPathX), GridPath.Obstacles(level, itemsTaken: true)) ?? level.Target ?? level.Robot;

    int ColNear(float x) => Mathf.Clamp(Mathf.RoundToInt(x + (cols - 1) * 0.5f), 0, cols - 1);
}
```

(Son satırdaki yürüyüş `Robot.MoveTo` tekerlek animasyonunu kullanır. `Follow` Serçe'nin kendi coroutine'i: `ResetHome` durdurur.)

- [ ] **Step 7: `Oyun.cs` bağlantısı** — alan `CanyonScenes scenes;` robot ve shot kurulduktan sonra `scenes = new CanyonScenes(robot, shot, hud, c => Pos(c), Cols, Rows, StartYaw);` (hud kurulduktan sonra). `RunProgram`'da `if (level.Number == Bolge5SonBolum) …` satırının altına `if (level.Number == CanyonTraces.LastLevel) yield return scenes.Descent(level);`. `LoadLevel` başında ve `ResetLevel`'da `scenes.Restore();` (`shot.Release()` satırlarının yerine; Restore zaten bırakır).
- [ ] **Step 8: Görüntü denetimi** — `Oyun.SahneDenetimi.cs`'e `DescentShots`: 60'ı yükle, `SetCode(level.Solution)`, `OnRun()`; `scenes.Phase` "yol", "bakis" (+1,5 sn), "inis" (+1,5 sn), "son" (+1 sn) olunca `s60-2-yol.png`, `s60-3-bakis.png`, `s60-4-inis.png`, `s60-5-son.png`; her bekleme en çok 40 sn (aşılırsa `problems.Add("60: <evre> gelmedi")`). Sonra **geri alma**: yeniden çalıştır, `Phase == "inis"` olunca `ResetLevel()`; bir kare bekle; denetle: `shot.Active == false`, `robot.gameObject.activeSelf`, robot `Pos(level.Robot)`'ta, Serçe `ResetHome` yerinde (`CanyonTraces.ActiveSparrow.transform.localPosition`), aksi `problems`'a. `SceneShots` sırası: `RiftShots`, `DescentShots`.
- [ ] **Step 9:** `dotnet test`; paket; `-sahneler` turu; `SAHNE DENETIMI: TAMAM`; kareler: Kıvılcım kayaların/panellerin üstünden geçmiyor, direklerin arasından giriyor, kamera hareketinde gökyüzü ufukla birlikte, inişte gölgeler doğru, son karede ikisi de görünmüyor ve "Sonraki: kanyon dibi" damgası okunuyor.
- [ ] **Step 10: ENES ONAYI** — `s60-2..5` Enes'e gösterilir; istenirse `calistir.bat` ile oyunda izletilir (`oyun/Build/Win` silinip yeniden üretilmiş paketle). Onaysız Task 3'e geçilmez.

---

### Task 3: Bölüm 55 — Serçe'nin ilk uçuşu (görüntü onayı)

**Files:**
- Modify: `oyun/Assets/Scripts/Sparrow.cs` (gözler Unlit; `WakeUp`, `TakeOff`; pervane hızı ayrı)
- Create: `oyun/Assets/Scripts/ScreenBlink.cs` (UI Toolkit kara kırpışma katmanı)
- Modify: `oyun/Assets/Scripts/Hud.cs` (yalnızca `ScreenBlink` eklenir + `public void Blink(float seconds)`)
- Modify: `oyun/Assets/Scripts/Sound.cs` (`Rotor()`)
- Modify: `oyun/Assets/Scripts/CanyonScenes.cs` (`FirstFlight`), `CanyonTraces.cs` (`public static Vector3 SparrowHover` — 55'te `HoverSpot`), `Oyun.cs` (kapanış satırı), `Oyun.SahneDenetimi.cs`

**Interfaces:**
- Consumes: `CameraShot.Begin/Cut/Release/Sway`, `Sparrow.FlyTo/ResetHome`, `CanyonScenes` (Task 2).
- Produces: `Sparrow.WakeUp()` (IEnumerator), `Sparrow.TakeOff(Vector3 localTarget, float seconds)` (IEnumerator), `Hud.Blink(float seconds)`, `Sound.Rotor()`, `CanyonScenes.FirstFlight(Level level)`, `CanyonTraces.SparrowHover`.

- [ ] **Step 1: Serçe gözleri** — `eyeMat` yerine iki malzeme: `eyeOff = Mats.Lit(Mats.Hex("#20262C"), 0.8f)`, `eyeOn = Mats.Unlit(Mats.Hex("#8FF0FF"))` (yorum: pakette Emissive parlamıyor, ILERLEME.md açık soru; Unlit her ışıkta net). Mercek `Renderer`'ları dizide; `SetEyes(int litCount)` ilk `litCount` merceği `eyeOn`, kalanı `eyeOff` yapar. `SetState`: `Flying` → `SetEyes(2)`, değilse `SetEyes(0)`. Pervane dönüşü `rotorSpeed` alanına bağlanır (`Update`'te `state == Flying || rotorSpeed > 0` ise döner; `Flying`'de 1).
- [ ] **Step 2: `WakeUp` / `TakeOff`**

```csharp
    // 55: gozler sirayla yanar (iki kisa bip), pervaneler yavastan hizliya (vizilti)
    public IEnumerator WakeUp()
    {
        SetEyes(1); Sound.Blip();
        yield return Tween.Wait(0.35f);
        SetEyes(2); Sound.Blip();
        Sound.Rotor();
        yield return Tween.Run(1.2f, t => rotorSpeed = t * t);
    }

    // Ilk ucus: once yalpalar, sonra dengesini bulup yukselir
    public IEnumerator TakeOff(Vector3 to, float seconds)
    {
        var from = transform.localPosition;
        yield return Tween.Run(seconds, t =>
        {
            float wobble = (1f - t) * (1f - t);
            transform.localPosition = Vector3.Lerp(from, to, Tween.InOutCubic(t)) + new Vector3(0.06f * wobble * Mathf.Sin(t * 31f), 0f, 0f);
            body.localRotation = Quaternion.Euler(12f * wobble * Mathf.Sin(t * 23f), 0f, 14f * wobble * Mathf.Sin(t * 19f + 1f));
        });
        SetState(SparrowState.Flying);
    }
```

- [ ] **Step 3: `Sound.Rotor()`** — `public static void Rotor() => Play(Get("rotor", () => Whirr(1.6f)), 0.35f);`; `Whirr`: frekansı 90'dan 240 Hz'e çıkan testere dalgası + az gürültü, yükselen sonra hafif sönen zarf (`Hiss`/`Buzz` kalıbında, `SampleRate`, `MakeClip`). Dosya başı yorumundaki ses listesine "pervane vızıltısı" eklenir.
- [ ] **Step 4: `ScreenBlink.cs` + `Hud.Blink`** — tam ekran siyah `VisualElement`, `pickingMode = Ignore`, `opacity` 0; `Blink(seconds)`: opaklık yarı sürede 1'e, yarı sürede 0'a (`schedule.Execute`). Hud'da tek satır `Add(blink)` + `public void Blink(float s) => blink.Play(s);`.
- [ ] **Step 5: `CanyonScenes.FirstFlight`**

```csharp
    // 55: Serce uyanir, cati boslugundan yukselir; bir an Serce'nin gozunden tam yukaridan bakis; sonra kosesine iner
    public IEnumerator FirstFlight(Level level)
    {
        var sparrow = CanyonTraces.ActiveSparrow;
        if (sparrow == null) yield break;
        Phase = "uyanis";
        yield return robot.TurnTo(DuneFinale.YawTowards(robot.transform.position, sparrow.transform.position), 0.5f);
        yield return sparrow.WakeUp();
        Phase = "kalkis";
        robot.StartCoroutine(robot.LookUp());
        var above = sparrow.transform.localPosition + Vector3.up * 3.2f;
        yield return sparrow.TakeOff(above, 3f);
        Phase = "kus";
        shot.Begin();
        hud.Blink(0.3f);
        yield return Tween.Wait(0.15f);
        var eye = new Vector3(sparrow.transform.localPosition.x * 0.5f, 6.5f, 0.6f);   // alanin ve sundurmanin ustu
        shot.Cut(eye, Quaternion.Euler(90f, 0f, 0f));
        shot.Sway = 1.5f;
        yield return Tween.Wait(2.5f);
        hud.Blink(0.3f);
        yield return Tween.Wait(0.15f);
        shot.Release();
        Phase = "inis";
        yield return sparrow.FlyTo(CanyonTraces.SparrowHover, 1.6f);
        Phase = "son";
    }
```

(Robot `LookUp` robotun kendi coroutine'i; `ResetTo` durdurur. `Tween.Wait` ölçeklenmiş zaman kullanıyorsa sorun yok; `hud` Task 2'den beri yapıcıda.) Not: Serçe'nin rafının üstünde çatıda boşluk zaten var (`DepotShed.Roof` raf üstünü açık bırakır); kalkış o boşluktan geçer — kare `s55-2`'de çatıya çarpmadığı görülür, çarparsa `above` boşluğun ortasına (raf `x`'i) hizalanır.
- [ ] **Step 6: `Oyun.cs`** — `if (level.Number == CanyonTraces.SparrowRepairLevel) yield return scenes.FirstFlight(level);` (`SparrowRepairLevel` `public const` yapılır). `CanyonTraces.SparrowHover` 55'te `HoverSpot(areaHalf, ground)` ile doldurulur.
- [ ] **Step 7: Denetim** — `FlightShots`: 55'i çöz; `uyanis`(+1 sn) `s55-1-uyanis`, `kalkis`(+1,5 sn) `s55-2-kalkis`, `kus`(+1 sn) `s55-3-yukaridan`, `son`(+0,5 sn) `s55-4-son`. Geri alma: `kus` evresinde `ResetLevel` → kamera serbest, Serçe rafta (`ResetHome`), gözleri sönük (`SetState(Dormant)`). `SceneShots`'a eklenir.
- [ ] **Step 8:** paket + `-sahneler` → `SAHNE DENETIMI: TAMAM`; bak: gözler net turkuaz-beyaz, yukarıdan bakışta çatı + alan + küçük Kıvılcım, ekranda yazı yok (üst arayüz yerinde kalıyor; kabul — spec "ekrana yazı gelmez" dünyadaki yazı içindir), Serçe sonunda 56'daki yerinde.
- [ ] **Step 9: ENES ONAYI** — `s55-*` gösterilir; onaysız Task 4'e geçilmez.

---

### Task 4: Bölüm 51 — Roketin inişi (ilk açılışta bir kez; görüntü onayı)

**Files:**
- Modify: `oyun/Assets/Scripts/ColonyRocket.cs` (`static class` → `MonoBehaviour`; kapak/merdiven/alev/yanık hâlleri)
- Create: `oyun/Assets/Scripts/LandingDust.cs`
- Create: `oyun/Assets/Scripts/SkipCatcher.cs` (dokununca atlatan saydam katman)
- Modify: `oyun/Assets/Scripts/Hud.cs` (yalnızca `SkipCatcher` eklenir + `ShowSkip(Action)`/`HideSkip()`)
- Modify: `oyun/Assets/Scripts/Sound.cs` (`Rocket()`)
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs` (`public static ColonyRocket ActiveRocket`; `ResetBackdrop` roketi yere oturtur)
- Modify: `oyun/Assets/Scripts/CanyonScenes.cs` (`Landing`, `FinishLanding`), `Oyun.cs` (`LoadLevel`'da tetik), `Oyun.SahneDenetimi.cs`

**Interfaces:**
- Consumes: `GridPath`, `CanyonScenes.Walk/CellsTo`, `hud`, `startYaw` (Task 2 yapıcısı).
- Produces: `ColonyRocket` — `static ColonyRocket Create(Transform parent, Vector3 localPos, float scale)`, `void SetLanded()`, `void SetInFlight(float height)`, `IEnumerator Land(float seconds)`, `IEnumerator OpenDoor()`, `Vector3 DoorPoint`, `Vector3 LadderFoot` (ikisi parent'ın yerel uzayında); `LandingDust.Play(Transform parent, Vector3 localPos)`; `Sound.Rocket()`; `Hud.ShowSkip(Action onTap)`, `Hud.HideSkip()`; `CanyonScenes.Landing(Level)`, `CanyonScenes.FinishLanding(Level)`.

- [ ] **Step 1: Ölçü kontrolü (gerçekçilik)** — Kıvılcım'ın boyunu/genişliğini (`SparkBot`, `ModelScale 2.1`) ve 51'deki roketin (ölçek 0,7: boy ~1,5, gövde çapı ~0,28, kapı ~0,1×0,19) ölçüsünü karşılaştır. Kıvılcım kapıdan sığmıyorsa (beklenen: sığmıyor) 51-52 roketi **boyu aynı, gövdesi kalın** (kapı Kıvılcım'ın ~%115'i) iniş aracına dönüşür: `Create`'e `float girth` (gövde ve kapı yatay ölçeği; 51-52'de gerekli değer, varsayılan 1). Bekleme karesi (`-bolum 51` 51-1) alınıp **Enes'e sorulur**, onaylarsa devam (roketin üst yazılara değmediği ve 52'de yazılara değmediği de kontrol edilir).
- [ ] **Step 2: `ColonyRocket` MonoBehaviour** — var olan çizim aynen kalır; kapak (`Kapak`), merdiven (`Merdiven`), yanık izler ve yeni alev (`Alev`: motorun altında aşağı bakan koni, `Mats.Unlit(Mats.Hex("#FFB25A"))`, gölge düşürmez) alanlarda tutulur. `home` = Create'teki yer. `SetLanded`: yerinde, kapak açık (Y 70°), merdiven görünür, yanık görünür, alev kapalı. `SetInFlight(h)`: `home + up*h`, kapak kapalı (0°), merdiven/yanık gizli, alev açık. `Land(seconds)`: `OutCubic` ile `home+up*h`'den `home`'a; son %15'te alev küçülür; değince 0,04 seker; alev kapanır, yanık açılır. `OpenDoor`: kapak 0→70° (0,5 sn), merdiven `localScale.y` 0→1 (0,5 sn, üstten aşağı açılır gibi: merdiven pivotu kapı altında). `DoorPoint`/`LadderFoot`: kapı önü ve merdiven ucunun `parent` yerel konumu (`transform.parent.InverseTransformPoint(child.position)`). `CanyonTraces` 51'de `ActiveRocket = ColonyRocket.Create(...)`, 52'de dönen değer kullanılmaz; `ResetBackdrop`'ta `ActiveRocket?.SetLanded()`.
- [ ] **Step 3: `LandingDust`** — roketin altında 12 küçük toz yumağı (`MeshFactory.Sphere`, `Mats.Lit(DuneTraces.Sand* koyu)` ya da `DustCloud`'un rengi), 2,5 sn'de dışa doğru 0,4→1,4 birim yayılır, yükselip alçalır, ölçeği 0'a iner ve kendini siler (`Destroy`). Saydamlık yok (sis değil, katı yumaklar).
- [ ] **Step 4: `Sound.Rocket()`** — `Rumble(3.2f)`: alçak süzülmüş gürültü + 45 Hz sinüs, sonda 0,5 sn'de kesilir; ses `0.45f`. Dosya başı listesine eklenir.
- [ ] **Step 5: `SkipCatcher` + Hud** — tam ekran saydam `VisualElement` (`pickingMode = Position`, arka plan saydam), `ClickEvent` → bir kez `onTap`, sonra gizlenir. Açıkken kod kartına ve düğmelere dokunulamaz ("Çalıştır" kapalı).
- [ ] **Step 6: `CanyonScenes.Landing` / `FinishLanding`**

```csharp
    // 51 (ilk acilis, bir kez): roket kanyona iner, kapi acilir, Kivilcim merdivenden inip bos karelerden baslangica gider.
    public IEnumerator Landing(Level level)
    {
        var rocket = CanyonTraces.ActiveRocket;
        if (rocket == null) yield break;
        Phase = "ucus";
        robot.gameObject.SetActive(false);
        rocket.SetInFlight(LandingHeight);
        Sound.Rocket();
        yield return rocket.Land(4f);
        LandingDust.Play(rocket.transform.parent, rocket.transform.localPosition);
        Phase = "indi";
        yield return Tween.Wait(0.6f);
        yield return rocket.OpenDoor();
        Phase = "kapi";
        robot.ResetTo(rocket.DoorPoint, 180f);
        robot.gameObject.SetActive(true);
        yield return robot.MoveTo(rocket.LadderFoot, 0.8f);
        var entry = GridPath.OpenInRow(cols, rows - 1, 0, GridPath.Obstacles(level, itemsTaken: false)) ?? level.Robot;
        var points = CellsTo(level, entry, level.Robot, itemsTaken: false).ConvertAll(c => cellPos(c));
        yield return Walk(points, 0.4f);
        yield return robot.TurnTo(startYaw, 0.3f);
        Phase = "son";
    }

    // Dokununca: her sey son haline (roket yerde, kapi acik, robot baslangicta)
    public void FinishLanding(Level level)
    {
        CanyonTraces.ActiveRocket?.SetLanded();
        robot.gameObject.SetActive(true);
        robot.ResetTo(cellPos(level.Robot), startYaw);
        Phase = "son";
    }

    const float LandingHeight = 6f;   // roketin gokyuzunde belirdigi yukseklik
```
- [ ] **Step 7: `Oyun.cs` tetik** — `LoadLevel`'daki geçiş satırı: 51 ilk kez (`PlayerPrefs` `"roket_inisi"` = 0) ve `!shotsMode` ise `hud.ShowTransition(transition, () => StartCoroutine(PlayLanding()))`, yoksa bugünkü gibi. `PlayLanding` (Oyun.cs'te 10 satırı geçmez; `PlayCanyonRecording`'in yanına): `hud.ShowSkip(() => { StopCoroutine(landing); scenes.FinishLanding(level); Finish(); })`; sahne biter ya da atlanırsa: `PlayerPrefs.SetInt("roket_inisi", 1)`, `hud.HideSkip()`, `hud.ShowIntro(level.Intro)`. 51'in `SetLevel` içindeki ilk `ShowIntro`'su bu durumda boş verilir (damga sahneden sonra gelsin). Sahne sürerken bölüm değişirse `LoadLevel` → `scenes.Restore()` + `hud.HideSkip()`.
- [ ] **Step 8: Denetim** — `LandingShots`: 51'i yükle, `StartCoroutine(PlayLanding())`'i doğrudan çağır (shotsMode bayrağına bakmadan); `ucus`(+1,5 sn) `s51-1-ucus`, `indi`(+0,3) `s51-2-indi`, `kapi`(+0,5) `s51-3-kapi`, `son` `s51-4-son`. Atlatma: yeniden başlat, `ucus`ta `SkipCatcher`'ın `onTap`'ini çağır → roket yerde, robot `Pos(level.Robot)`'ta, `Phase == "son"`. `PlayerPrefs` denetimden sonra eski değerine döndürülür.
- [ ] **Step 9:** paket + `-sahneler` → `SAHNE DENETIMI: TAMAM`; bak: alev ve toz, iniş yeri bugünkü roketin yeri, kapak ve merdiven, Kıvılcım'ın merdivenden inişi ölçüye uygun, boş karelerden yürüyüş, sonunda damga.
- [ ] **Step 10: ENES ONAYI** — `s51-*` gösterilir; onaysız Task 5'e geçilmez.

---

### Task 5: Son denetim + belgeler

**Files:**
- Modify: `docs/tasarim/senaryo-bolge-06.md` (Bölüm 50 → 51 paragrafı: önce yazılar, sonra sahne; "Oyuna konanlar: 3b" bölümü)
- Modify: `docs/tasarim/telefon-denemesi.md` (Hamza: 51 inişi, 55 ve 60 sahnelerinde `KARE:` satırları)
- Modify: `CLAUDE.md` (Çalıştırma: `-sahneler` seçeneği bir satır)
- Modify: `ILERLEME.md`

- [ ] **Step 1:** `cd motor-test && dotnet test` → hepsi geçer.
- [ ] **Step 2:** paket (hatasız); `-sahneler` → `SAHNE DENETIMI: TAMAM`; `-sahneler -performans ac` → TAMAM ve 60 yarığı kesik; `-bolum 51` (51-60 normal) — 51-59 görünüşü `bolge-6-gorunus-2026-10-09/` ile aynı (60 dışında), 55 ve 60'ın `3-bitti` karesi sahnenin son karesi; `-bolum 41` ilk karesi önceki ile aynı; `-bolum 50 -final` → `FINAL DENETIMI: TAMAM`; `scripts/klavye-denetimi.ps1` → `TAMAM`.
- [ ] **Step 3:** Klavye açıkken 60 bitirişi: `calistir.bat` ile elle ya da `DescentShots`'a klavye açık bir koşu eklenerek; sahne kamerası küçülmüş fotoğraftan sıçramadan başlıyor mu.
- [ ] **Step 4:** Görüntüler `docs/tasarim/bolge-6-sahneler-2026-10-11/`'e (`s51-*`, `s55-*`, `s60-*`, performans 60).
- [ ] **Step 5:** Belgeler + ILERLEME.md (Enes adıyla: ne yapıldı, denenmeyenler — telefonda sahne akıcılığı, ses düzeyi; sıradaki). Commit/push yalnızca oturum kapanışında.
