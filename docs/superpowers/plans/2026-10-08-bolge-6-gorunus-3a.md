# Bölge 6 Görünüşü (3a: ortam + nesneler) Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bölüm 51-60'a kanyonun kendi görünüşünü (ikindi gökyüzü + dev duvarlar, kanyon tabanı / depo zemini, ışık) ve bütün hikâye nesnelerini (drone parçası, Serçe, roket, depo, raf, sandık, boya oklar, görev panosu) koymak.

**Architecture:** Önceki bölgelerin düzeni: arka plan `MarsSky.hlsl`'e yeni bölge dalı (`_Region` 4, `CanyonSky.hlsl`), zemin `Ground.shader`'a yeni çeşit (+ Performans modu sürümü), ışık `RegionLook.Canyon`. Her nesne kendi dosyasında, kodla çizilir; bölüm bölüm yerleşimi `CanyonTraces.Build` seçer, `Traces.Build` onu çağırır. `Oyun.cs` yalnızca birkaç satır değişir.

**Tech Stack:** Unity 6 (6000.6.3f1, URP), C# 9, HLSL. Saf C# motor/dünya kodu değişmez (`dotnet test` yalnızca bozulmadığını denetler).

**Spec:** `docs/superpowers/specs/2026-10-08-bolge-6-design.md` — "3. Görünüş ve sahneler", 3a.1-3a.4. Hikâye: `docs/tasarim/senaryo-bolge-06.md`.

## Global Constraints

- Her şey kodla çizilir; hazır model / büyük doku yok (paket boyutu, Ragıp'ın kuralı).
- Unity C# 9; `UnityEngine` kullanan kod yalnızca `oyun/Assets/Scripts/`'te. `Motor/` ve `Dunya/` değişmez.
- Yeni her `.cs`/`.hlsl` dosyası için Unity `.meta` üretir; `.meta` dosyaları da git'e girer.
- Turkuaz `#2FC4B8` (Ece'nin rengi; `CraterTraces.EceColor`). "SERÇE" çocuk eliyle, turkuaz.
- Güneş batıda = ekranda solda, sol duvarın kenarının hemen üstünde; gölgeler sağa-öne; duvar gölgesi alana düşmez.
- Hikâye izleri alanın arkasında/yanlarında; bulmaca karelerini kapatmaz, sağ/sol üst düğmelerin altına girmez.
- `Oyun.cs` (~1.750 satır) büyütülmez: yalnızca bağlantı satırları.
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıflar, tekrar yok, anlamlı adlar, ölçülü soyutlama. Yorumlar Türkçe ASCII (ç/ş yerine c/s), dosyanın başında ne olduğu.
- Denetim komutları:
  - Paket: `"/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log` (önce `oyun/Build/` silinir), hata: `grep "error CS" build.log`.
  - Görüntü: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -shots <klasör> -bolum 51` (51-60, birkaç dakika). Performans modu: sona `-performans ac`.
  - `cd motor-test && dotnet test` (1395 geçmeli).

## Review Focus

1. **Bölge 1-5'e sızma:** `_Region` 4 eklenince `MarsDunes()` (`_Region > 2.5`) kanyonu da kum tepesi sanar; Bölge 4 ve 5 aynı kalmalı, kanyon yalnızca 51-60'ta. → Task 1'de Bölüm 31/41 görüntüsü eskisiyle karşılaştırılır.
2. **Işık yönü geri dönmeli:** kanyonda değişen güneş yönü 51-60'tan başka bölüme (bölüm listesinden 1'e dönünce) geçince eski hâline dönmeli. → Task 1'de 60 → 1 geçişi görüntüyle denetlenir.
3. **Depo zemini anahtarı kalmamalı:** `_CanyonIndoor` 53-56 dışında 0 olmalı (57'ye, ya da 53'ten Bölüm 1'e geçince beton kalmamalı). → Task 3.
4. **Yeniden dene / toplama:** drone parçası toplanınca kaybolur, "↻" ile geri gelir (`IPickup.Restore`); Serçe ve dekorlar yeniden denemede bozulmaz. → Task 4, 5.
5. **Performans modu:** kanyon zemini hafif sürümde de doğru renk/desende, arka plan Performans modunda doğru. → Task 8.

---

### Task 1: Kanyon gökyüzü + ışık (`_Region` 4)

**Files:**
- Create: `oyun/Assets/Shaders/CanyonSky.hlsl`
- Modify: `oyun/Assets/Shaders/MarsSky.hlsl` (bölge seçiciler satır 7-10; `MarsBackdrop`, `MarsHorizonPlain`, `MarsGroundWarmth`, `MarsRegionSurface`)
- Modify: `oyun/Assets/Scripts/RegionLook.cs` (Canyon tablosu + ışık yönleri)
- Modify: `oyun/Assets/Scripts/Oyun.cs:365,371` (ışık yönleri RegionLook'a taşınır)

**Interfaces:**
- Produces: `MarsCanyon()` (HLSL), `CANYON_PLAIN` (float3), `CanyonBackdrop(float2 suv)`; `RegionLook.For(Region.Canyon)` artık kendi tablosu; `RegionLook` ışık yönlerini de uygular.

- [ ] **Step 1: Bölge seçicileri düzelt** — `MarsSky.hlsl`:

```hlsl
float _Region;    // bolge (RegionLook.cs): 0 inis ovasi, 1 kutup buzulu, 2 kraterli duzluk, 3 kum tepeleri, 4 kanyon
bool MarsPolar() { return abs(_Region - 1.0) < 0.5; }
bool MarsCrater() { return abs(_Region - 2.0) < 0.5; }
bool MarsDunes() { return abs(_Region - 3.0) < 0.5; }
bool MarsCanyon() { return _Region > 3.5; }
```

- [ ] **Step 2: `CanyonSky.hlsl` yaz** — `DuneSky.hlsl` iskeletiyle (aynı yardımcılar: `MarsPictureUV`, `_Horizon`, `_SkyScreen`, `_SkyTime`, `vnoise`, `fbm`, `hash11`). İçerik (spec 3a.1):
  - `CANYON_PLAIN` (ufuk dibindeki puslu kanyon tabanı rengi), `CANYON_SUN = float2(-0.30, 0.16)` (solda, sol duvarın kenarının hemen üstünde; görüntüde sol üst düğmelerle çakışmayacak yere ayarlanır).
  - Gökyüzü: ufukta karamela → tepede soluk gri-mor; güneş küçük, beyazımsı, yakın halesi mavimsi (DuneSky'deki gibi), ikindi olduğu için geniş hale daha sıcak/altın.
  - **Sol duvar** ekranın sol kenarından, **sağ duvar** sağ kenarından yükselir; profil: duvar kenarı x'e göre basamaklı (birkaç `step` + `vnoise`), içe doğru alçalır. Duvarda yatay katmanlar: `frac(y * N + vnoise(x)*k)` ile açık/koyu bantlar; eteklerde moloz yelpazesi (aşağı doğru genişleyen, açık renkli üçgen bölge).
  - Işık: sağ duvarın içe bakan yüzü güneşe bakar → altın (`float3(0.85,0.55,0.32)` civarı), sol duvarın içe bakan yüzü gölgede → morumsu-mavi (`float3(0.25,0.20,0.28)` civarı); duvarın ön kenarı (silüet) ince açık çizgi değil, yumuşak geçiş.
  - Orta: kanyon uzaklara kıvrılarak alçalır: 2-3 kat uzak duvar silüeti (her biri bir öncekinden daha soluk/mavimsi, `kDuneHaze` gibi pus katsayısı), ortadaki boşluğun dibinde toz sisi bandı (ufkun hemen altında açık, hafif hareketli `fbm`, `_SkyTime` ile çok yavaş).
  - Yıldız yok. Sonuç `* MARS_EXPOSURE`.
- [ ] **Step 3: `MarsSky.hlsl` dallarına bağla:**

```hlsl
#include "CanyonSky.hlsl"   // DuneSky.hlsl satirinin altina
// MarsBackdrop:
    else if (MarsDunes()) col = DuneBackdrop(suv);
    else if (MarsCanyon()) col = CanyonBackdrop(suv);
// MarsHorizonPlain:
    if (MarsCanyon()) return CANYON_PLAIN;
// MarsGroundWarmth (gunese dogru, altin):
    if (MarsCanyon()) return float3(0.30, 0.16, 0.07);
// MarsRegionSurface (uzak kayalar: katmanli tortu kayasi, ustu hafif acik):
    if (MarsCanyon()) return lerp(albedo * float3(0.95, 0.85, 0.80), float3(0.42, 0.22, 0.14), smoothstep(0.6, 0.9, n.y));
```

- [ ] **Step 4: Işık yönlerini RegionLook'a taşı** — `RegionLook.cs`:

```csharp
    // Isiklarin yonu (Euler). Bolge 1-5 hep ayni; kanyonda ikindi gunesi batidan (solda) gelir.
    static readonly Vector3 DefaultSkyDir = new Vector3(52f, -35f, 0f), DefaultSunDir = new Vector3(14f, 168f, 0f);
    Vector3 skyDir = DefaultSkyDir, sunDir = DefaultSunDir;

    // Kanyon (Bolge 6): ikindi; gunes solda sol duvarin ustunde, sicak altin isik, golgeler saga-one; golgedeki yuzler serin
    static readonly RegionLook Canyon = new RegionLook
    {
        shaderRegion = 4f,
        skyLight = "#FFD9AE", skyIntensity = 0.9f,
        sunLight = "#FFB070", sunIntensity = 0.6f,
        ambientSky = "#7C6E80", ambientEquator = "#7A5A4E", ambientGround = "#33241E",
        probeBase = "#6E5658", probeTop = "#7A6A7A",
        skyDir = new Vector3(38f, 112f, 0f),   // batidan (sol arka) gelir, golge doguya (saga) ve hafif one duser
        sunDir = new Vector3(12f, 100f, 0f),
    };
```
  `For`: `case Region.Canyon: return Canyon;` (AntennaHill hâlâ `Dunes`). `Blend` içine:

```csharp
        sky.transform.rotation = Quaternion.Slerp(Quaternion.Euler(a.skyDir), Quaternion.Euler(b.skyDir), t);
        sun.transform.rotation = Quaternion.Slerp(Quaternion.Euler(a.sunDir), Quaternion.Euler(b.sunDir), t);
```
  (`_DawnDir` satırından ÖNCE, çünkü o `sun.transform.forward` okur.) `Oyun.cs:365` ve `:371`'deki sabit `rotation` satırları silinir (375'teki `Apply` artık kuruyor).
- [ ] **Step 5: Paket + görüntü** — `oyun/Build` sil, paketi üret, `grep "error CS" build.log` boş. `-shots <scratch>/t1 -bolum 51` → `b51-0`, `b55-0`, `b60-0`'a bak: iki duvar, solda güneş, ortada derinleşen kanyon, gölgeler sağa. Ayrıca `-shots <scratch>/t1b -bolum 31` ilk iki görüntüsü `docs/tasarim/bolge-3-4-gorunus-2026-10-07/b31-0-baslangic-kodu.png` ile aynı (Review Focus 1). İlk deneme beklendiği gibi değilse düzelt, yeniden üret.

### Task 2: Kanyon zemini (+ depo betonu, + Performans modu)

**Files:**
- Modify: `oyun/Assets/Shaders/Ground.shader` (hafif dallar ~satır 73-140, tam dallar ~144-295)

**Interfaces:**
- Consumes: `MarsCanyon()` (Task 1).
- Produces: genel float `_CanyonIndoor` (0 dışarısı, 1 depo içi); Task 3 kurar.

- [ ] **Step 1: Tam sürüm `canyonFloor(p, far, inArea)`** — `duneSand`'ın yanına: koyu pas sıkışmış toprak (temel `float3(0.36,0.18,0.12)` civarı), büyük ölçekli `fbm` ile düz katmanlı kaya plakaları (keskin kenarlı açık-koyu yamalar, kenarında ince koyu çizgi), seyrek çakıl (`hash` noktaları), alanın içinde (`inArea`) desen soluk. Kum dalgası yok.
- [ ] **Step 2: Depo betonu** — `canyonFloor` içinde `_CanyonIndoor > 0.5` iken: alanın içi ve 1 kare çevresi toz kaplı beton plakalar (soluk gri-bej `float3(0.42,0.36,0.32)`, büyük kare levha çizgileri ~2 karede bir, kenarlara doğru toz birikmesi = pas rengine geçiş); daha uzak zemin kanyon tabanı kalır (depo duvarlarının dışı).
- [ ] **Step 3: Seçiciye ekle** (tam ve hafif dallar):

```hlsl
                else if (MarsDunes()) a = duneSand(p, far, inArea);
                else if (MarsCanyon()) a = canyonFloor(p, far, inArea);
```
  `float _CanyonIndoor;` genel değişken olarak (Properties'e değil; `Shader.SetGlobalFloat`).
- [ ] **Step 4: Hafif sürüm** (`MARSKOD_SADE` dalı): yalnızca renk + tek `vnoise` plakası + kare çizgileri; depo içi düz beton rengi. `duneSand`'ın hafif sürümü kadar ucuz.
- [ ] **Step 5: Paket + görüntü** — Task 1'deki gibi; 51 ve 58'de kanyon tabanı (henüz 53'te de taban; beton Task 3'te açılır). `-performans ac` ile 51'e bak.

### Task 3: CanyonTraces yerleşim iskeleti + depo anahtarı + sandıklar + katmanlı kayalar

**Files:**
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs`
- Modify: `oyun/Assets/Scripts/Traces.cs` (`Build`, `ResetBackdrop`)
- Modify: `oyun/Assets/Scripts/Obstacles.cs`
- Modify: `oyun/Assets/Scripts/Oyun.cs:508,510` (iki çağrı)

**Interfaces:**
- Produces:
  - `CanyonTraces.IsDepot(int levelNumber)` → 53-56 true.
  - `CanyonTraces.Build(int levelNumber, Transform parent, Vector2 areaHalf, Vector3? targetPos, IReadOnlyList<Vector3> itemPositions)` → Bölge 6 dışında hiçbir şey yapmaz; içi Task 4-7'de dolar.
  - `CanyonTraces.ResetBackdrop(int levelNumber)` → `_CanyonIndoor` kurar.
  - `Traces.Build(..., Vector3? targetPos, IReadOnlyList<Vector3> itemPositions)` (yeni son parametre).
  - `Obstacles.Create(Region region, Transform parent, Vector3 pos, int index, bool crate)`.

- [ ] **Step 1: CanyonTraces'e ekle:**

```csharp
    public const int DepotFirst = 53, DepotLast = 56;
    static readonly int IndoorId = Shader.PropertyToID("_CanyonIndoor");

    // Bolum 53-56 yari acik deponun icinde gecer (zemin beton, engeller sandik)
    public static bool IsDepot(int levelNumber) => levelNumber >= DepotFirst && levelNumber <= DepotLast;

    // Zemin cizimine "depo ici" bilgisi; Bolge 6 disinda hep 0
    public static void ResetBackdrop(int levelNumber) => Shader.SetGlobalFloat(IndoorId, IsDepot(levelNumber) ? 1f : 0f);

    // Bolumun izlerini kurar (Bolge 6 disinda hicbir sey). itemPositions: toplanacak drone parcalarinin yerleri (53'te raf o siraya).
    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Vector3? targetPos, IReadOnlyList<Vector3> itemPositions)
    {
        if (levelNumber < FirstLevel || levelNumber > LastLevel) return;
        // Task 4-7: bolum bolum nesneler
    }
```
  (`using System.Collections.Generic; using UnityEngine;` eklenir; dosya başındaki yorum "Görünüş 3a: yerleşim" diye güncellenir.)
- [ ] **Step 2: Traces'e bağla** — `Build` imzasına `IReadOnlyList<Vector3> itemPositions` ekle, `AntennaTraces.Build` satırının altına `CanyonTraces.Build(levelNumber, parent, areaHalf, targetPos, itemPositions);`. `ResetBackdrop` içine `CanyonTraces.ResetBackdrop(levelNumber);`.
- [ ] **Step 3: Oyun.cs çağrıları:**

```csharp
        for (int i = 0; i < level.Rocks.Count; i++)
            Obstacles.Create(region, levelRoot, Pos(level.Rocks[i]), i, CanyonTraces.IsDepot(level.Number));
        target = ...;
        Traces.Build(level.Number, levelRoot, new Vector2(AreaHalfX, AreaHalfZ), level.Target.HasValue ? Pos(level.Target.Value) : (Vector3?)null,
            level.Ices.ConvertAll(c => Pos(c)));
```
  (`level.Ices` türü `List<Cell>` değilse `.Select(Pos).ToList()`; derleyici söyler.)
- [ ] **Step 4: Obstacles** — `crate` parametresi; `Region.Canyon` kendi dalı:

```csharp
            case Region.Canyon:
                if (crate) Crate(parent, pos, index); else LayeredRock(parent, pos, index);
                break;
```
  `LayeredRock`: kanyon duvarından kopmuş tortu kayası = 2-3 üst üste yassı `MeshFactory.Rock` (y ölçeği ~0.45, her kat biraz kaymış ve dönmüş; renkler `#8E5A44` / `#7A4A38` / açık bant `#A8705A`) + yanında küçük parça. Kare dolu görünmeli ("buradan geçilmez").
  `Crate`: kareyi dolduran 2 sandık (biri büyük `RoundedBox(0.5,0.4,0.45)`, üstünde ya da yanında küçük), tahta-metal: soluk haki gövde `#7C7458`, koyu metal köşe şeritleri `#3E3C40`, birinde açık kapak (üstte eğik levha), ön yüzde soluk stencil şerit. Köşeler ezik: hafif `localRotation` eğikliği.
- [ ] **Step 5: Paket + görüntü** — 53-56'da beton zemin + sandıklar, 51/52/57-60'ta katmanlı kayalar ve taban; Bölüm 31/41 kayaları değişmedi.

### Task 4: Drone parçası (`D`) + üst sayaç simgesi + 52 kırıntıları

**Files:**
- Create: `oyun/Assets/Scripts/DronePart.cs`
- Modify: `oyun/Assets/Scripts/Pickup.cs:22`
- Modify: `oyun/Assets/Scripts/Hud.cs:416` (+ yeni `DrawPropellerDot`)
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs` (52 kırıntıları)

**Interfaces:**
- Produces:
  - `public enum DronePiece { Propeller, Arm, Shell }`
  - `DronePart : MonoBehaviour, IPickup` — `static DronePart Create(Transform parent, Vector3 localPos, int seed)` (çeşit `(DronePiece)((seed - 1) % 3)`).
  - `static Transform DronePart.BuildPiece(Transform parent, DronePiece piece)` — yalnızca görünüş (Serçe renkleri); kırıntılar ve ileride Serçe'nin eksik hâli de kullanır.
  - `DroneColors` (static): `Body = #E8E1D2` (krem), `Accent = #E8743B` (turuncu), `Dark = #34313A`, `Prop = #4A4750`. Task 5 de kullanır.

- [ ] **Step 1: `DronePart.cs`** — `CompassPart`'ın düzeniyle: `Build(seed)` → `BuildPiece`, kuma hafif eğik yatmış; `Update`'te çok hafif sallanma + turuncu ayrıntıda yavaş parıltı (`Mats.Emissive` + `_EmissionColor`), `Pop()` → `PickupFx.RingPulse` + `PopAway`, `Restore()` aynı. Parçalar:
  - **Propeller:** halka koruyucu (`Lathe` ya da ince `RoundedCylinder` halka: dış + iç yarıçap farkı için iki silindir yerine 12 kısa kutudan halka) + ortada göbek + 2 pal (ince yassı kutu, hafif kıvrık).
  - **Arm:** ince kol kutusu, ucunda motor silindiri (turuncu bant), kopuk ucunda iki ince kablo.
  - **Shell:** gövde kabuğundan yarım küre parçası (`Sphere` y ölçek 0.5, krem), kenarında turuncu şerit.
  Boyut: karede `CompassPart` kadar yer kaplar (`localScale` ~1.8).
- [ ] **Step 2: Pickup** — `case Collectible.DronePart: return DronePart.Create(parent, localPos, seed);` (geçici yorum silinir).
- [ ] **Step 3: Hud simgesi** — `DrawPropellerDot(Painter2D p, Rect r, bool filled)`: dış çember (halka koruyucu) + ortada nokta + çapraz iki pal; dolu hâlde `CompassFill` rengi yerine Serçe turuncusu `new Color(0.91f,0.45f,0.23f)`. 416. satır: `item == Collectible.CompassPart ? ...DrawCompassDot... : item == Collectible.DronePart ? new Icon(36, (p, r) => DrawPropellerDot(p, r, idx < collected)) : ...`.
- [ ] **Step 4: 52 kırıntıları** — `CanyonTraces.Build` içinde `if (levelNumber == StormLevel /*52*/) StormScatter(parent, areaHalf);`: alanın sol ve sağ dışında 3-4 `DronePart.BuildPiece` (küçük ölçek 1.2, devrik, yarı kuma gömük) + birkaç sürüklenme izi (koyu ince uzun yassı kutular, `Traces.WheelTracks` gibi ama kısa ve yönü rüzgârla).
- [ ] **Step 5: Paket + görüntü** — 52-58'de üç çeşit parça seçiliyor mu, üst sayaçta pervane simgesi; `b52-3-bitti` hepsi toplanmış. "↻" sonrası geri geliyor (görüntü turundaki yeniden dene karesi varsa ona, yoksa elle bir kez).

### Task 5: Serçe

**Files:**
- Create: `oyun/Assets/Scripts/Sparrow.cs`
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs`

**Interfaces:**
- Consumes: `DroneColors`, `DronePart.BuildPiece` (Task 4); `WorldText.Create`; `CraterTraces.EceColor`.
- Produces:
  - `public enum SparrowState { Incomplete, Dormant, Flying }`
  - `Sparrow : MonoBehaviour` — `static Sparrow Create(Transform parent, Vector3 localPos, SparrowState state)`; `void SetState(SparrowState s)` (3b'deki ilk uçuş sahnesi bunu ve `Rotors` dönüşünü kullanacak); `Transform Body` (3b için).
  - `CanyonTraces.Sparrow { get; }` (bölümde yoksa null) — 3b sahneleri için.

- [ ] **Step 1: Gövde** — Kıvılcım'ın yarı boyu (SparkBot ölçüsüne bakılır): tombul `Sphere` gövde (x 1.0, y 0.8, z 1.15), krem; önde iki kamera "göz" (koyu yuvarlak + içte küçük parlak nokta, `Flying`'de ışıklı, diğerlerinde sönük); arkada kısa yassı kuyruk; altta iki kısa kızak ayak; dört çapraz kol (`DroneColors.Dark`), uçlarında halkalı pervane (Task 4'teki propeller ile aynı yapı, kod tekrarı yok: ortak `BuildRotor` `DronePart`'tan çağrılır ya da `DronePart.BuildPiece(Propeller)`); turuncu ayrıntılar (kol uçları, kuyruk ucu).
- [ ] **Step 2: "SERÇE" yazısı** — gövdenin üst-yan yüzünde, kameradan okunur: `WorldText.Create(body, "SERÇE", ..., height, CraterTraces.EceColor)`, hafif eğik (çocuk eli). "Ç" doğru çıkıyor mu görüntüde bak; çıkmazsa font kontrolü.
- [ ] **Step 3: Hâller** — `Incomplete`: bir kol ve iki pervane yok (gizli), kabukta açık delik (koyu yama); `Dormant`: hepsi tam, ışıklar sönük, pervaneler duruyor; `Flying`: ışıklar yanık (`Blinker` ya da emissive), `Update`'te pervaneler hızla döner, gövde `sin` ile 3-4 cm süzülür ve hafifçe yalpalar.
- [ ] **Step 4: Yerleşim** — `CanyonTraces.Build`:
  - 54: arka duvar rafının üstünde `Incomplete` (raf Task 7'de; şimdilik alanın arkasında `z = areaHalf.y + 0.7`, `y ~0.5`).
  - 55: aynı yerde `Dormant`.
  - 56-58, 60: `Flying`, alanın sağ arka köşesinin üstünde (`x = areaHalf.x + 0.2, y = 1.3, z = areaHalf.y - 0.3`); sağ üst düğmelerin/pusulanın altına girmez (görüntüde ayarla).
  - 59: `Flying`, `targetPos` karesinin ~1.2 üstünde.
- [ ] **Step 5: Paket + görüntü** — 54 eksik, 55 sönük, 56-60 uçuyor; "SERÇE" okunuyor; 59'da hedef halkasını kapatmıyor.

### Task 6: Roket + boya oklar + yakın kanyon duvarı / iniş rampası

**Files:**
- Create: `oyun/Assets/Scripts/ColonyRocket.cs`
- Create: `oyun/Assets/Scripts/PaintMarks.cs`
- Create: `oyun/Assets/Scripts/CanyonCliff.cs`
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs`

**Interfaces:**
- Produces:
  - `ColonyRocket.Create(Transform parent, Vector3 localPos, float scale)` → `Transform` (3b iniş sahnesi bunu hareket ettirecek; kapı `Transform Door` alt adıyla bulunur: `root.Find("Kapi")`).
  - `PaintMarks.Arrow(Transform surface, Vector3 localPos, float angleDeg, float size, bool drips)` (yüzeye yapışık turuncu ok `#E8742E`, boya dokusu: kenarları hafif düzensiz, `drips` → altında 2-3 ince akıntı); `PaintMarks.Date(Transform surface, Vector3 localPos, string text, float height)` (`WorldText`, aynı turuncu).
  - `CanyonCliff.Wall(Transform parent, float z, float width, float height)` → `Transform` (alanın arkasında yakın, katmanlı kaya duvarı; ön yüzü kameraya bakar, oklar buna yapışır); `CanyonCliff.Ramp(Transform parent, Vector2 areaHalf)` (60: alanın arkasından aşağı inen kayalık yol + kenarında karanlık derinlik).

- [ ] **Step 1: ColonyRocket** — ince uzun gövde (`Lathe` profili: burun konisi + silindir + motor eteği), soluk beyaz `#D9D4CA` + turuncu bant + koloni işareti yok (hikâye yok); 3 eğik ayak + ayak tabanları; yan kapı açık (koyu dikdörtgen + dışa açılmış kapak), kapıdan yere inen ince merdiven; altında yere yanık iz (koyu, yumuşak kenarlı yassı disk, `castShadow: false`). Yükseklik ~2.2 birim (ölçekle).
- [ ] **Step 2: PaintMarks** — ok = gövde (yassı kutu) + üçgen uç (`Mesh` 3 köşeli ya da döndürülmüş kutu); yüzeyden 2 mm önde, `outline: false`, gölge vermez. Akıntı: ince dikey kutular, uçları damla (`Sphere`).
- [ ] **Step 3: CanyonCliff** — Wall: yan yana 4-6 dev `MeshFactory.Rock` (yassı, üst üste katman bantlı renkler, `MarsRegionSurface` uzak kayalarla uyumlu) ya da katmanlı kutular; ön yüzde okların durduğu düz bir yüz (açık renkli düz kaya levhası). Ramp: alanın kuzey kenarından başlayan, aşağı eğimli 3-4 kaya plakası + iki yanında iri kayalar; rampanın ötesi koyu (derinlik: koyu, gölgeli düşük yüzey).
- [ ] **Step 4: Yerleşim** (`CanyonTraces.Build`):
  - 51: roket sol arkada (`x = -areaHalf.x - 0.4, z = areaHalf.y + 1.4`, ölçek 1.0; sol üst düğmelerin altına girmez → görüntüde ayarla); sağ arkada büyük kaya bloğu (`MeshFactory.Rock` 0.8) + üstünde ok (sağa-ileri, depoyu gösterir).
  - 52: roket uzakta (`z = areaHalf.y + 3.5`, ölçek 0.6) + bir ok.
  - 57, 59: arka kenarda bir kayada ok.
  - 58: `CanyonCliff.Wall(z = areaHalf.y + 0.6, width ~6.5, height ~1.4)`; üstünde soldan sağa 5 ok + tarihler `sol 852`, `sol 855`, `sol 857`, `sol 859`, `sol 861` (son ikisi `drips: true`, oklar sağa doğru daha eğri/acele). "sol 861" yazısı en büyük, telefonda okunur (`height` ~0.16).
  - 60: `CanyonCliff.Ramp` + rampanın başında bir ok aşağıyı gösterir.
- [ ] **Step 5: Paket + görüntü** — 51 roket + ok, 58 tarihler okunuyor (450×975 pencerede "sol 861" seçiliyor), 60 rampa ve derinlik.

### Task 7: Depo (duvarlar, yırtık çatı, raflar) + görev panosu

**Files:**
- Create: `oyun/Assets/Scripts/CanyonDepot.cs`
- Create: `oyun/Assets/Scripts/StorageShelf.cs`
- Create: `oyun/Assets/Scripts/TaskBoard.cs`
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs`

**Interfaces:**
- Produces:
  - `CanyonDepot.Inside(Transform parent, Vector2 areaHalf, bool wideTear)` → `Transform BackShelfTop` (arka duvar rafının üst yüzeyinin konumu; Serçe 54-55'te oraya konur).
  - `CanyonDepot.Outside(Transform parent, Vector3 localPos)` (57: dışarıdan, alanın arkasında).
  - `StorageShelf.Create(Transform parent, Vector3 center, float length)` (53: alanın içindeki uzun raf; D'ler alt gözde, iskeleti karelerin arasına/arkasına düşer, robot karelere girebilir).
  - `TaskBoard.Create(Transform surface, Vector3 localPos)`.

- [ ] **Step 1: CanyonDepot.Inside** — arka duvar `z = areaHalf.y + 0.5`, yan duvarlar `x = ±(areaHalf.x + 0.5)`, yükseklik ~0.8 (alçak: kanyon duvarları üstünden görünür); oluklu metal (dikey ince çizgili kutular), yıpranmış soluk gri-mavi `#6E7480` + pas lekeleri; birkaç yerde eğik/kırık levha. Köşelerde dikmeler; dikmelerden kalkan **yırtık çatı iskeleti**: alanın kenarları boyunca 2-3 kiriş (ön kenar yok), bir kiriş kırık sarkıyor; kirişlerden sarkan branda parçaları (soluk haki, hafif dalgalı yassı kutular). `wideTear` (55): orta kiriş yok, branda yırtığı geniş. Arka duvar boyunca dekor raf (`StorageShelf` kısa) + kutular; `BackShelfTop` döndürülür.
- [ ] **Step 2: StorageShelf** — uzun metal raf: dikmeler kare sınırlarında (alanın içinde robotun yürüdüğü karelerin ortasını kapatmaz), 2 göz; alt göz yerden ~5 cm (D'ler orada görünür), üst göz ~0.55; arka panel yok (D'ler görünür). 53'te `center.z = itemPositions[0].z + 0.35` (rafın gövdesi karenin arka yarısında), `length = 6`. Sandıklar Task 3'ten raf önünde.
- [ ] **Step 3: TaskBoard** — arka duvara asılı pano (koyu yeşil-gri levha, çerçeve), 5 satır: üst 4 satır soluk, tozlu, okunmaz (kısa koyu çizgiler + yarım harfler; `WorldText` ile rastgele soluk "▬▬▬ ···" değil, düz çizgi blokları), en alttaki satır net: `BKM-7 ········ KAPI` (`WorldText`, beyazımsı tebeşir rengi, "KAPI" biraz daha kalın/büyük). Telefonda okunur büyüklük.
- [ ] **Step 4: Yerleşim** — 53: `Inside(wideTear:false)` + `StorageShelf` (itemPositions sırasına); 54: `Inside` + Serçe `BackShelfTop`'ta (Task 5 yerleşimini buraya taşı); 55: `Inside(wideTear:true)` + Serçe; 56: `Inside` + `TaskBoard` arka duvarın ortasında; 57: `Outside` alanın arkasında (`z = areaHalf.y + 2`).
- [ ] **Step 5: Paket + görüntü** — 53 raf satırı "raf" diye okunuyor, D'ler alt gözde, sandıklar önde; çatı alanın üstünü kapatmıyor; duvar üstünden kanyon görünüyor; 56 "BKM-7 … KAPI" okunuyor; 57 depo dışarıdan.

### Task 8: Performans modu, tam denetim, belgeler

**Files:**
- Modify: `docs/tasarim/telefon-denemesi.md`
- Modify: `docs/tasarim/senaryo-bolge-06.md` ("Oyuna konanlar")
- Modify: `docs/tasarim/hikaye-kitabi.md` (Serçe görünüşü satırı)
- Create: `docs/tasarim/bolge-6-gorunus-2026-10-08/` (seçilmiş görüntüler)
- Modify: `ILERLEME.md`

- [ ] **Step 1:** `-performans ac` ile `-shots -bolum 51`; 51, 53, 58'e bak (zemin hafif sürümde doğru, arka plan doğru).
- [ ] **Step 2:** `cd motor-test && dotnet test` → 1395 geçti.
- [ ] **Step 3:** Tam görüntü turu `-bolum 51` (normal mod) + `-shots … -bolum 50 -final` (`FINAL DENETIMI: TAMAM`, 51 girişi kanyonla) + `powershell -ExecutionPolicy Bypass -File scripts/klavye-denetimi.ps1` → `KLAVYE DENETIMI: TAMAM`; log'da yedi fare denetimi TAMAM.
- [ ] **Step 4:** Görüntülerden seçilenleri (51, 52, 53, 54, 55, 56, 58, 59, 60, performans-51, performans-53) `docs/tasarim/bolge-6-gorunus-2026-10-08/`'e kopyala.
- [ ] **Step 5:** `telefon-denemesi.md`'ye Hamza için "Bölüm 51 ve 53'te `KARE:` satırları (Bölüm 1 ile karşılaştır)"; `senaryo-bolge-06.md` "Oyuna konanlar"a 3a listesi; `hikaye-kitabi.md` Serçe satırına görünüş (tombul, halkalı 4 pervane, turkuaz "SERÇE"; "Ragıp'a haber"); ILERLEME.md girdisi (Enes, ne yapıldı, denenmeyenler, sıradaki 3b).
