# Bölge 6 Görünüşü — 3a yeniden yapım · Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bölüm 51-60'ın beğenilmeyen görünüşünü (dev duvarlı kanyon, kutu depo, bilgisayar yazıları, havada kaya, anlamsız kenar kayaları) Enes'in 2026-10-09'da seçtiği yönle baştan yapmak.

**Architecture:** Önceki bölgelerin düzeni korunur: arka plan `MarsSky.hlsl`'in kanyon dalı (`CanyonSky.hlsl`, baştan yazılır, `BackdropCache` ile saklanır), zemin `Ground.shader` `canyonFloor`, ışık `RegionLook.Canyon`. Nesneler ayrı dosyalarda kodla çizilir; yerleşimi yalnızca `CanyonTraces.Build` seçer. Yeni ortak parça `BrushPaint` (elle boyanmış darbe/yazı) okları, panoyu ve Serçe'nin adını çizer. 60'taki iniş, zemin çiziminde tek bir genel değerle (`_CanyonDescent`) yapılır: arazi ağı yeniden kurulmaz.

**Tech Stack:** Unity 6 (6000.6.3f1, URP), C# 9, HLSL. `Motor/` ve `Dunya/` değişmez.

**Spec:** `docs/superpowers/specs/2026-10-08-bolge-6-design.md` → "3. Görünüş ve sahneler", 3a.1-3a.6 (2026-10-09 hâli). Hikâye: `docs/tasarim/senaryo-bolge-06.md`.

## Global Constraints

- Her şey kodla çizilir; hazır model / büyük doku / yeni yazı tipi dosyası yok (paket boyutu, Ragıp).
- Unity C# 9. Yeni her `.cs`/`.hlsl` için Unity `.meta` üretir; silinen dosyanın `.meta`'sı da silinir; hepsi git'e girer.
- **Ara görüntü kuralı (spec 3a.5):** Task 1, 2 ve 4'ün sonunda Enes'e görüntü gösterilir; **Enes onaylamadan sonraki göreve geçilmez.** Beğenmezse aynı görevde düzeltilir.
- Kayalıklar ufkun hemen üstünde kalır; üst yazılara/düğmelere (ekranın üst ~%20'si) uzanmaz. Hiçbir nesne kayalıkları/ufku kesmez (sundurma dahil). Ekranın tepesine yükselen duvar yok.
- Gün batımı: güneş solda (batı), alçakta, küçük beyaz disk + soğuk mavi hale; gökyüzü üstte koyu, ufukta karamela; yıldız yok. Alanın ışığı aynı güneşten (solda alçak, gölgeler sağa uzun).
- Dünyada bilgisayar yazısı (`WorldText`) Bölge 6 nesnelerinde kullanılmaz; yazı `BrushPaint` ile. Bir sahnede en fazla tek okunur kelime.
- Alan dışındaki her nesne zeminin gerçek yüksekliğine oturur (`TerrainHeight`).
- Turkuaz `#2FC4B8` (`CraterTraces.EceColor`), boya turuncusu `PaintMarks.Paint`.
- `Oyun.cs` büyütülmez: yalnızca bağlantı satırı değişir.
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıflar, tekrar yok, anlamlı adlar, ölçülü soyutlama. Yorumlar Türkçe ASCII (ç/ş yerine c/s), dosya başında ne olduğu.
- Denetim komutları:
  - Paket: `rm -rf oyun/Build && "/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log`; hata: `grep "error CS" build.log` (boş olmalı) ve shader hatası `grep -i "shader error" build.log`.
  - Görüntü: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -shots <scratch>/<klasör> -bolum 51` (51'den sona kadar; birkaç dakika). Performans modu: sona `-performans ac`.
  - `cd motor-test && dotnet test` → hepsi geçer.

## Review Focus

1. **Başka bölgeye sızma:** kanyon gökyüzü/zemin/`_CanyonDescent` değişiklikleri Bölge 1-5'i değiştirmemeli. → Task 1 ve Task 4 sonunda `-bolum 41` ilk görüntüsü `docs/tasarim/bolge-5-bulmacalar-2026-10-07/b41-1-bekleme.png` ile karşılaştırılır.
2. **`_CanyonDescent` yalnızca 60'ta:** 60'tan 59'a ya da bölüm listesinden 1'e geçince iniş boyası kalmamalı. → Task 4'te `CanyonTraces.ResetBackdrop` her bölümde değeri yeniden yazar; görüntü turunda 59 (60'tan sonra değil önce gelir) + elle 60→59 geçişi `calistir.bat` ile.
3. **Geniş görünüm (klavye + ipucu açık, kamera geri çekilir):** kayalıklar üst düğmelere, sundurma çatısı ufka taşmamalı. → Task 1 ve 2'de görüntü turundaki klavye açık karesine (`*-2-*` / `*-klavye*` hangisi üretiliyorsa) bakılır.
4. **Yeniden dene:** "↻" sonrası Serçe, raf, sandık, boya ve pano yerinde ve tek kopya kalmalı (bölüm kökü silinip yeniden kurulur; statik önbellekte nesne tutulmaz). → Task 2 ve 3'te bir bölümü iki kez yükleyen görüntü (shots zaten başlangıç + bitiş kurar) sayılarak bakılır.
5. **Performans modu:** kanyon zemini, beton ve 60 iniş boyası hafif sürümde (`MARSKOD_SADE`) de var. → Task 5.

---

### Task 1: Ortam — gün batımı kanyon ağzı (arka plan, ışık, zemin)

**Files:**
- Rewrite: `oyun/Assets/Shaders/CanyonSky.hlsl` (eski içerik tamamen silinir)
- Modify: `oyun/Assets/Shaders/MarsSky.hlsl` (`MarsGroundWarmth` kanyon satırı; `MarsRegionSurface` kanyon satırı yalnızca renk ayarı gerekirse)
- Modify: `oyun/Assets/Scripts/RegionLook.cs` (`Canyon` tablosu + yorumu)
- Modify: `oyun/Assets/Shaders/Ground.shader` (`canyonFloor` iki sürümü: yalnızca renk/parlaklık ayarı)

**Interfaces:**
- Produces (HLSL, değişmeyen adlar): `CANYON_PLAIN`, `float3 CanyonBackdrop(float2 suv)`; yeni `CANYON_SUN` değeri. Diğer görevler bunlara dokunmaz.

- [ ] **Step 1: `CanyonSky.hlsl`'i baştan yaz** — `DuneSky.hlsl` iskeletiyle (aynı yardımcılar: `MarsPictureUV`, `_Horizon`, `_SkyScreen`, `_Picture`, `_SkyTime`, `vnoise`, `fbm`, `hash11`; `MARSKOD_CANYON_INCLUDED` koruması). Dosya başı yorumu yeni yönü anlatır (kanyon ağzı, gün batımı, alçak mesalar). İçerik:

```hlsl
static const float3 CANYON_PLAIN = float3(0.44, 0.30, 0.26);   // ufuk dibindeki puslu kanyon tabani (zemin buna solar)
static const float2 CANYON_SUN = float2(-0.20, 0.035);          // gunes: x, ufka gore yukseklik (alcak, solda; sol mesanin ustunde)

float3 canyonSkyColor(float t)   // t: 0 ufuk, 1 tepe. Gun batimi: ufukta karamela, tepede koyu
{
    float3 low = float3(0.76, 0.58, 0.47), mid = float3(0.42, 0.32, 0.31), top = float3(0.15, 0.12, 0.15);
    return t < 0.25 ? lerp(low, mid, t / 0.25) : lerp(mid, top, saturate((t - 0.25) / 0.75));
}

// Mesa (duz tepeli, basamakli kayalik) ust kenari. side: -1 sol, +1 sag; ekranin kenarinda en yuksek, ortaya dogru
// 2-3 basamakla alcalir, ortada kanyonun agzinda biter. step: hangi basamakta (katman bandi icin).
float canyonMesaTop(float x, float side, float heightScale, float seed, out float s)
```
  - **Katmanlar (uzaktan yakına), her biri `DUNE_LAYERS` gibi bir dizi:** (0) en uzak orta silüet: kanyonun içi, alçak, mavimsi-mor, güçlü pus; (1) sol mesa sırası: gölgede, `float3(0.36,0.25,0.25)` civarı; (2) sağ mesa sırası: gövdesi `float3(0.52,0.33,0.25)`, yalnızca **güneşe bakan ince kenarlar** (basamakların sol kenarı ve tepe çizgisi) `float3(0.86,0.58,0.40)`'a doğru aydınlanır.
  - **Yükseklik sınırı:** en yüksek mesa tepesi `_Horizon + 0.075`'i geçmez (dünkü duvarlar `0.16`'ya çıkıyordu); basamaklar arası düşüş `0.012-0.025`. Mesa tepesi düz (`vnoise` genliği küçük, `0.002`).
  - **Tortu katmanları:** mesa yüzünde yatay açık/koyu bantlar: `0.93 + 0.07 * sin((uv.y - _Horizon) * 260.0 + vnoise(x * 6.0) * 1.5)`; uzak katmanda bantlar soluk.
  - **Etek:** her mesanın dibinde aşağı doğru genişleyen moloz yelpazesi (açık, puslu), ufuk çizgisine yumuşakça karışır.
  - **Güneş:** `DuneBackdrop`'taki gibi küçük disk (`0.0065`), yakın hale **soğuk mavi** `float3(0.62,0.78,0.95)` (`exp(-d*30)*0.55`), geniş hale sıcak ama zayıf (`exp(-d*5)*0.25`). Güneş sol mesanın tepesinin hemen üstünde.
  - **Pus:** ortadaki kanyon ağzında, ufkun hemen üstünde yavaş hareketli ince toz (`fbm`, `_SkyTime*0.01`), mesaların dibinde `CANYON_PLAIN`'e karışma. Yıldız yok. Sonuç `MarsBackdrop`'taki diğer dallarla aynı biçimde döner (`DuneBackdrop` `MARS_EXPOSURE` uygulamıyorsa bu da uygulamaz; ona bak ve aynısını yap).
- [ ] **Step 2: `MarsSky.hlsl` kanyon renkleri** — `MarsGroundWarmth`: `if (MarsCanyon()) return float3(0.22, 0.12, 0.08);` (gün batımı: daha zayıf, sıcak). Diğer kanyon satırları (`MarsHorizonPlain`, `MarsRegionSurface`, `MarsBackdrop`) yerinde kalır.
- [ ] **Step 3: `RegionLook.Canyon`** — yorum "Kanyon: gun batimi; gunes solda, alcakta (CanyonSky.hlsl). Golge veren isik batidan, alcaktan: golgeler saga uzun." Başlangıç değerleri (görüntüde ayarlanır):

```csharp
    static readonly RegionLook Canyon = new RegionLook
    {
        shaderRegion = 4f,
        skyLight = "#FFCFA0", skyIntensity = 0.78f,
        sunLight = "#FF9E62", sunIntensity = 0.5f,
        ambientSky = "#6E6478", ambientEquator = "#6E5048", ambientGround = "#2C1F1B",
        probeBase = "#5E4A4E", probeTop = "#6C5E70",
        skyDir = new Vector3(24f, 80f, 0f),    // batidan, alcaktan: golge doguya (saga) uzun duser
        sunDir = new Vector3(8f, 115f, 0f),
    };
```
- [ ] **Step 4: Zemin rengi** — `Ground.shader` `canyonFloor` (iki sürüm: tam ve `MARSKOD_SADE`): gün batımına uygun biraz daha koyu/soğuk ana renk (`float3(0.35,0.21,0.16)`→`float3(0.43,0.26,0.19)`); desen aynı kalır.
- [ ] **Step 5: Paket + görüntü** — paketi üret, `error CS` / shader hatası yok. `-shots <scratch>/t1 -bolum 51`. Kendin bak: `b51-*` ve `b57-*` (alan + kayalıklar + güneş), klavye açık karesi (Review Focus 3), kayalıklar üst yazılara değmiyor, güneş solda ve mavi haleli, alan ışığı aynı yönden. Ayrıca `-shots <scratch>/t1b -bolum 41` ilk görüntüsü Bölge 5 referansıyla aynı (Review Focus 1). Sorun varsa düzelt, yeniden üret.
- [ ] **Step 6: Enes'e göster** — 51 ve 57 görüntüsünü (gerekirse klavye açık karesini) sohbette göster, kısaca ne değiştiğini anlat. **Onay gelmeden Task 2'ye geçme.**

### Task 2: Açık sundurma depo + raf (Bölüm 53-57)

**Files:**
- Create: `oyun/Assets/Scripts/DepotShed.cs`
- Delete: `oyun/Assets/Scripts/CanyonDepot.cs` + `.meta`
- Modify: `oyun/Assets/Scripts/Traces.cs:37` + `Traces.Build` imzası (yükseklik işlevi)
- Modify: `oyun/Assets/Scripts/Oyun.cs:508` (yalnızca çağrıya `TerrainHeight` eklenir)
- Rewrite: `oyun/Assets/Scripts/StorageShelf.cs`
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs` (`BuildDepot`, `case 57`)
- Modify: `oyun/Assets/Shaders/Ground.shader` (`depotFloor`: beton alanın arkasında sundurmanın altına kadar uzanır)

**Interfaces:**
- Produces:

```csharp
// Kanyon deposu (Bolum 53-57): alanin arkasinda alcak, uzun, acik bir sundurma. Ince direkler, yer yer yirtik oluklu cati,
// altinda raflar, bir yanda sarkan tek branda. Alani saran duvar yok; en yuksek noktasi ufku kesmeyecek kadar alcak.
public static class DepotShed
{
    public readonly struct Spots
    {
        public readonly Vector3 ShelfTop;   // arka rafin ust gozunun ortasi (Serce burada durur)
        public readonly Vector3 PostFace;   // sag on diregin kameraya bakan yuzu (pano buraya asilir)
        public Spots(Vector3 shelfTop, Vector3 postFace) { ShelfTop = shelfTop; PostFace = postFace; }
    }

    // frontCenter: on direk sirasinin ortasi (x, z; y kullanilmaz). width: boyu. bigHole: 55'teki cati deligi.
    // ground: zemin yuksekligi; her direk kendi yerine oturur.
    public static Spots Build(Transform parent, Vector3 frontCenter, float width, bool bigHole, Func<float, float, float> ground)
}

// Traces.cs / CanyonTraces.cs (son parametre yeni)
public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Vector3? targetPos,
    IReadOnlyList<Vector3> itemPositions, Func<float, float, float> groundHeight)

public static class StorageShelf
{
    public const float Height = 0.62f, Depth = 0.34f;
    // center: rafin tabaninin ortasi; length: boyu (x). stocked: gozlerde kutular. Doner: rafin koku.
    public static Transform Create(Transform parent, Vector3 center, float length, bool stocked)
    public static Vector3 TopCenter { get; }   // rafin ust gozunun ortasi (rafin kokune gore)
}
```

- [ ] **Step 0: Yükseklik işlevini geçir** — `Traces.Build`'e `Func<float, float, float> groundHeight` son parametre; yalnızca `CanyonTraces.Build`'e iletilir. `Oyun.cs:508`: `..., level.Ices.ConvertAll(c => Pos(c)), TerrainHeight);`. `CanyonTraces` işlevi statik alanda tutmaz, yardımcılara parametre olarak geçirir (Review Focus 4):

```csharp
    static Vector3 OnGround(Func<float, float, float> ground, float x, float z) => new Vector3(x, ground(x, z), z);
```
- [ ] **Step 1: `DepotShed.cs` yaz** — sade parçalar (`Parts.Add`, `MeshFactory.RoundedBox/RoundedCylinder`, `Mats.Lit`):
  - Ön direk sırası `frontCenter.z`'de, arka direk sırası `+0.9`; direkler `0.05` kalın, ön `1.0`, arka `0.85` yüksek (çatı arkaya eğik). Direk aralığı `~1.1`.
  - Çatı: oluklu saç şeritleri (her biri `RoundedBox(new Vector3(0.5f, 0.02f, 1.0f))`, hafif farklı eğim, renk `#6D625C`/`#5E5450`); 2-3 şerit eksik (yırtık), birinin ucu sarkık. `bigHole`: ortada 3 şerit yok + bir kiriş kırık sarkık.
  - Altında arka raf: `StorageShelf.Create(..., length: width * 0.7f, stocked: true)`, arka direklerin önünde.
  - Branda: bir uçtan sarkan tek ince levha, krem `#C9B8A0`, hafif eğik.
  - Renkler loş, metal: direk `#3A3230`, kiriş `#4A403C`.
  - Hiçbir parça `frontCenter.z - 0.1`'den öne (alana) taşmaz.
- [ ] **Step 2: `StorageShelf.cs`'i baştan yaz** — iki dik ayak çifti + 3 göz (ince levha), göz kenarında turuncu ince şerit yok (sade). `stocked`: gözlerde 2-3 küçük kutu (`#9A7A5A`, `#7E6A58`). Eski `TopCenter` adı korunur (Serçe yerleşimi kullanır).
- [ ] **Step 3: `CanyonDepot.cs` + `.meta` sil**; derleme hatası kalmaması için `CanyonTraces`'teki bütün `CanyonDepot.` çağrılarını Step 4'te değiştir.
- [ ] **Step 4: `CanyonTraces.BuildDepot` + `case 57`:**

```csharp
    // Depo (53-56): alanin arkasinda acik sundurma; alan onun onundeki beton. 53'te parcalarin sirasi raf (alanin icinde),
    // 54-55'te Serce sundurmanin altindaki rafta, 56'da pano sag on direkte.
    static void BuildDepot(int levelNumber, Transform parent, Vector2 areaHalf, IReadOnlyList<Vector3> itemPositions, Func<float, float, float> ground)
    {
        var shed = DepotShed.Build(parent, new Vector3(0f, 0f, areaHalf.y + ShedGap), areaHalf.x * 2f + 1.2f,
            bigHole: levelNumber == SparrowRepairLevel, ground);
        if (levelNumber == DepotFirst && itemPositions.Count > 0)
            StorageShelf.Create(parent, new Vector3(0f, 0f, itemPositions[0].z), areaHalf.x * 2f, stocked: false);
        if (levelNumber == SparrowShelfLevel || levelNumber == SparrowRepairLevel)
            ActiveSparrow = Sparrow.Create(parent, shed.ShelfTop + Vector3.up * Sparrow.RestHeight,
                levelNumber == SparrowShelfLevel ? SparrowState.Incomplete : SparrowState.Dormant);
        if (levelNumber == BoardLevel)
        {
            TaskBoard.Create(parent, shed.PostFace);   // Task 3'te TaskBoard yeni imzayla yazilir
            ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf), SparrowState.Flying);
        }
    }
    const float ShedGap = 0.55f;   // alanin arka kenari ile sundurmanin on direkleri arasi
```
  `case 57`: `DepotShed.Build(parent, new Vector3(-areaHalf.x * 0.55f, 0f, areaHalf.y + 2.2f), 2.6f, bigHole: false, ground);` (uzakta, sol arkada) + Serçe (değişmez). Boya oklar Task 3'te eklenir; bu görevde eski `PaintMarks.Arrow` çağrıları derlenmeye devam etsin (imza Task 3'te değişir).
  `TaskBoard.Create`'in eski imzası `(Transform, Vector3)` zaten uyumlu; Task 3'te içi yeniden yazılır.
- [ ] **Step 5: Beton sundurmanın altına uzansın** — `Ground.shader` `depotFloor` (iki sürüm): maske alanın çevresinde eşit değil, arkaya doğru uzun: `outside` yerine `float2 d = max(abs(q) - _Area.xy, 0.0); d.y *= (q.y > 0.0) ? 0.35 : 1.0; float outside = length(d);` (arkada ~1.5 birim beton, yanlarda ve önde dar).
- [ ] **Step 6: Paket + görüntü** — `-shots <scratch>/t2 -bolum 53`; `b53`, `b54`, `b55`, `b56`, `b57`'ye bak: sundurma ufku/kayalıkları kesmiyor, çatı alçak, Serçe rafta, 55'te delik + paneller, beton sundurmanın altına uzanıyor, klavye açık karesinde taşma yok, tek kopya (Review Focus 4).
- [ ] **Step 7: Enes'e göster** — 53 ve 55. **Onay gelmeden Task 3'e geçme.**

### Task 3: Elle boyanmış işaretler (BrushPaint, oklar, pano, Serçe adı, 58 kaya basamağı)

**Files:**
- Create: `oyun/Assets/Scripts/BrushPaint.cs`
- Rewrite: `oyun/Assets/Scripts/PaintMarks.cs`
- Rewrite: `oyun/Assets/Scripts/TaskBoard.cs`
- Create: `oyun/Assets/Scripts/CanyonLedge.cs`
- Delete: `oyun/Assets/Scripts/CanyonCliff.cs` + `.meta` (60'ın kenarı Task 4'te `CanyonLedge.Edge`)
- Modify: `oyun/Assets/Scripts/Sparrow.cs:89-91` (ad etiketi)
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs` (bütün ok çağrıları, `DatedArrows` → `FadedArrows`, `case 58`)

**Interfaces:**
- Produces:

```csharp
// Elle boyanmis firca darbeleri ve yazi (Bolge 6). Bilgisayar yazisi gibi kusursuz durmasin diye her darbe hafif titrek,
// kalinligi degisken, uclari yuvarlak. Yuzeyin yerel XY duzleminde cizilir, -Z'ye (kameraya) bakar; golge vermez.
public static class BrushPaint
{
    // lines: 0..1 kutuda (x saga, y yukari) cizgi dizileri; size: kutunun dunyadaki boyu; seed: titremeyi sabitler
    public static Transform Strokes(Transform surface, Vector2[][] lines, Vector3 localPos, float size, float angleDeg,
        Material paint, int seed, float thickness = 0.09f)
    // Yalnizca gereken harfler cizilir: 1 6 7 8 - A B Ç E I K M P R S (bilinmeyen harf atlanir, bosluk ilerler)
    public static Transform Write(Transform surface, string text, Vector3 localPos, float height, Material paint, int seed, float angleDeg = 0f)
}

// Kanyondaki turuncu boya oklar ve tek okunur kelimeler (Bolge 6): D. Aras'in isaretledigi yol.
public static class PaintMarks
{
    public static readonly Color Paint = Mats.Hex("#FF8436");
    // fade: 0 taze (hafif parlak) .. 1 gunes yemis, kayaya karismis. drips: altindan akan boya (acele).
    public static Material PaintMat(float fade)
    public static Transform Arrow(Transform surface, Vector3 localPos, float angleDeg, float size, float fade, bool drips, int seed)
    public static Transform Word(Transform surface, string text, Vector3 localPos, float height, float fade, int seed)
}

public static class TaskBoard
{
    public const string ReadableWord = "KAPI", FaintTag = "BKM-7";
    // postFace: diregin kameraya bakan yuzu; pano oraya asilir (kucuk, tebesir panosu)
    public static Transform Create(Transform parent, Vector3 postFace)
}

// Kanyondaki alcak kaya yapilari: 58'de alanin arkasindaki kaya basamagi, 60'ta zeminin bittigi kenar (Task 4).
public static class CanyonLedge
{
    // z: basamagin on yuzu; width: boyu; height: yuksekligi (ufku kesmeyecek kadar alcak). Doner: on yuz (boya buraya, -Z'ye bakar)
    public static Transform Step(Transform parent, float z, float width, float height, Func<float, float, float> ground)
}
```

- [ ] **Step 1: `BrushPaint.cs` yaz.** Harf tablosu (0..1 kutu, genişlik ~0.6, ilerleme 0.68):

```csharp
    static readonly Dictionary<char, Vector2[][]> Glyphs = new Dictionary<char, Vector2[][]>
    {
        ['1'] = L(P(0.30f, 0.80f, 0.45f, 1f, 0.45f, 0f)),
        ['6'] = L(P(0.50f, 0.95f, 0.25f, 0.70f, 0.10f, 0.35f, 0.15f, 0.10f, 0.35f, 0f, 0.50f, 0.10f, 0.52f, 0.30f, 0.38f, 0.45f, 0.18f, 0.40f, 0.10f, 0.30f)),
        ['7'] = L(P(0.05f, 1f, 0.55f, 1f, 0.25f, 0f)),
        ['8'] = L(P(0.30f, 0.55f, 0.12f, 0.70f, 0.15f, 0.92f, 0.30f, 1f, 0.45f, 0.92f, 0.48f, 0.70f, 0.30f, 0.55f, 0.10f, 0.35f, 0.10f, 0.12f, 0.30f, 0f, 0.50f, 0.12f, 0.50f, 0.35f, 0.30f, 0.55f)),
        ['-'] = L(P(0.10f, 0.50f, 0.45f, 0.50f)),
        ['A'] = L(P(0.05f, 0f, 0.30f, 1f, 0.55f, 0f), P(0.15f, 0.38f, 0.45f, 0.38f)),
        ['B'] = L(P(0.08f, 0f, 0.08f, 1f, 0.35f, 1f, 0.48f, 0.88f, 0.45f, 0.66f, 0.30f, 0.55f, 0.08f, 0.55f), P(0.30f, 0.55f, 0.50f, 0.42f, 0.52f, 0.18f, 0.38f, 0f, 0.08f, 0f)),
        ['Ç'] = L(P(0.52f, 0.85f, 0.38f, 1f, 0.18f, 0.95f, 0.06f, 0.70f, 0.06f, 0.30f, 0.18f, 0.05f, 0.38f, 0f, 0.52f, 0.15f), P(0.30f, 0f, 0.33f, -0.12f, 0.24f, -0.20f)),
        ['E'] = L(P(0.50f, 1f, 0.08f, 1f, 0.08f, 0f, 0.50f, 0f), P(0.08f, 0.52f, 0.40f, 0.52f)),
        ['I'] = L(P(0.25f, 0f, 0.25f, 1f)),
        ['K'] = L(P(0.08f, 0f, 0.08f, 1f), P(0.50f, 1f, 0.10f, 0.45f), P(0.22f, 0.58f, 0.52f, 0f)),
        ['M'] = L(P(0.05f, 0f, 0.08f, 1f, 0.30f, 0.45f, 0.52f, 1f, 0.55f, 0f)),
        ['P'] = L(P(0.08f, 0f, 0.08f, 1f, 0.35f, 1f, 0.50f, 0.88f, 0.50f, 0.68f, 0.35f, 0.55f, 0.08f, 0.55f)),
        ['R'] = L(P(0.08f, 0f, 0.08f, 1f, 0.35f, 1f, 0.50f, 0.88f, 0.50f, 0.68f, 0.35f, 0.55f, 0.08f, 0.55f), P(0.28f, 0.55f, 0.52f, 0f)),
        ['S'] = L(P(0.50f, 0.90f, 0.35f, 1f, 0.15f, 0.95f, 0.08f, 0.78f, 0.20f, 0.58f, 0.42f, 0.45f, 0.52f, 0.25f, 0.42f, 0.05f, 0.20f, 0f, 0.05f, 0.12f)),
    };
    static Vector2[][] L(params Vector2[][] lines) => lines;
    static Vector2[] P(params float[] xy) { var p = new Vector2[xy.Length / 2]; for (int i = 0; i < p.Length; i++) p[i] = new Vector2(xy[2 * i], xy[2 * i + 1]); return p; }
```
  `Strokes`: her çizgi parçası için seed'li titreme (`System.Random(seed)`; uç noktalara ±`0.025` kaydırma, kalınlığa ±%15), parça = `MeshFactory.RoundedBox(new Vector3(len + th, th, 0.004f), th * 0.5f)` (yuvarlak uçlu), `localRotation = Euler(0,0,atan2)`; `outline: false, castShadow: false`. Kök `Parts.Empty("Boya", surface)`, `localPosition`, `localRotation = Euler(0,0,angleDeg)`, `localScale = size`. `Write`: harfleri `0.68 * height` arayla dizer, her harfe `seed + i`, metin ortalanır (toplam genişliğin yarısı kadar sola).
- [ ] **Step 2: `PaintMarks.cs`'i baştan yaz.** Ok şekli darbe: `{ (0,0.5)->(1,0.5) }, { (0.7,0.78)->(1,0.5)->(0.7,0.22) }` → `BrushPaint.Strokes`. `PaintMat(fade)`: renk `Color.Lerp(Paint, Mats.Hex("#A8705A"), fade * 0.75f)`, ışıma `Paint * 0.22f * (1 - fade)`. `drips`: okun gövdesi altından 2-3 ince dikey darbe + uçta küçük küre (eski koddaki gibi aşağı akar, ok döndürülse de). `Word` = `BrushPaint.Write(..., PaintMat(fade), seed)`. Eski `Date` silinir.
- [ ] **Step 3: `TaskBoard.cs`'i baştan yaz.** Küçük tebeşir panosu (`0.62 x 0.44`, çerçeve `#4E4C52`, yüz `#33403A`), direğe iki küçük kelepçeyle asılı, hafif eğik (`-4°`). Üstte 3 sıra silik çizgi öbeği (eski koddaki gibi, `#56635C`); altta `BrushPaint.Write(..., "KAPI", height 0.12, tebeşir #E8E4D8)`; sağ üst köşede küçük, soluk `BrushPaint.Write(..., "BKM-7", height 0.05, #8A948C)`.
- [ ] **Step 4: Serçe'nin adı** — `Sparrow.cs:89-91`:

```csharp
        // ad: ust-on yuzde, cocuk eliyle boyanmis; yukaridan bakan kamera okur
        var label = BrushPaint.Write(body, "SERÇE", new Vector3(0f, 0.108f, -0.055f), 0.06f,
            Mats.Lit(CraterTraces.EceColor, 0.3f), seed: 11);
        label.localRotation = Quaternion.Euler(52f, 0f, -7f);
```
- [ ] **Step 5: `CanyonLedge.Step` (58) yaz, `CanyonCliff.cs` + `.meta` sil.** Alçak tortu basamağı: 3-4 yan yana `RoundedBox` blok (her biri farklı genişlik, `height` ± %15, üst kenarı düzensiz), ön yüzde 2 yatay ince koyu bant (tortu), renk `#7D4A34` / üst `#94593C`; her blok `ground(x, z)` yüksekliğine oturur. `height` başlangıcı `0.55` (görüntüde ufku kesmeyecek en yüksek değere ayarlanır). Döndürdüğü yüz transformu blokların önünde `z - 0.01`.
- [ ] **Step 6: `CanyonTraces` okları** — bütün `PaintMarks.Arrow(..., false)` çağrıları yeni imzaya: taze ok `fade 0, drips false, seed = levelNumber`. `case 58`:

```csharp
            case DatesLevel:
                FadedArrows(CanyonLedge.Step(parent, areaHalf.y + 0.7f, areaHalf.x * 2f + 1.4f, LedgeHeight, ground), areaHalf);
                ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf), SparrowState.Flying);
                break;

    // Bolum 58: kaya basamaginda soldan saga oklar. Eskiler gunes yemis (soluk), sonuncusu taze, buyuk, boyasi akmis;
    // yaninda elle "861". Tarih dizisinin tamami ustteki damgada (spec 3a.4).
    static void FadedArrows(Transform face, Vector2 areaHalf)
    {
        const int Count = 5;
        for (int i = 0; i < Count; i++)
        {
            bool last = i == Count - 1;
            float x = -areaHalf.x + 0.3f + i * (areaHalf.x * 2f - 0.9f) / (Count - 1);
            PaintMarks.Arrow(face, new Vector3(x, last ? 0.30f : 0.28f, 0f), last ? -6f : -2f + i, last ? 0.42f : 0.3f,
                fade: last ? 0f : 0.85f - 0.1f * i, drips: last, seed: 580 + i);
        }
        PaintMarks.Word(face, "861", new Vector3(areaHalf.x - 0.35f, 0.12f, 0f), 0.14f, fade: 0f, seed: 861);
    }
```
  `ground` = Task 2 Step 0'da `CanyonTraces.Build`'e eklenen yükseklik işlevi.
- [ ] **Step 7: Derleme** — paket + `error CS` yok (görüntü Task 4 sonunda birlikte).

### Task 4: Havada nesne düzeltmesi + Bölüm 60 patikası

**Files:**
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs` (`OnGround`, `case 51/52/57/59/60`, `ResetBackdrop`)
- Modify: `oyun/Assets/Scripts/CanyonLedge.cs` (`Edge`)
- Modify: `oyun/Assets/Shaders/MarsSky.hlsl` (`MarsDescentHaze` ortak işlev)
- Modify: `oyun/Assets/Shaders/Ground.shader` (patika + vadi boyası, iki sürüm)
- Modify: `oyun/Assets/Shaders/FarRock.shader` (kenarın ötesindeki kayalara aynı pus)

**Interfaces:**
- Consumes: `CanyonLedge` (Task 3), `PaintMarks` (Task 3).
- Produces:

```csharp
// CanyonTraces.cs
public static void ResetBackdrop(int levelNumber)   // _CanyonIndoor + _CanyonDescent (yukleme ve "yeniden dene"de cagrilir)
// CanyonLedge.cs
// z: kenarin cizgisi; pathX: patikanin kenari kestigi yer. Kenar boyunca duzensiz koyu kaya dudagi, patikada aciklik,
// patikanin iki yaninda isaret diregi (ustte turuncu bant). Doner: patikanin basindaki zemin yuzeyi (ok buraya, yere yatik).
public static Transform Edge(Transform parent, float z, float width, float pathX, Func<float, float, float> ground)
```
```hlsl
// MarsSky.hlsl — Bolum 60: kanyona inen kenarin otesi daha alcak vadi tabanidir; oradaki zemin ve kayalar daha puslu,
// serin ve soluk gorunur (uzaklik/derinlik). _CanyonDescent: x kenarin z'si, y 1 = acik (0: hicbir bolumde etkisi yok)
float4 _CanyonDescent;
float MarsBeyondDescent(float3 posWS) { return _CanyonDescent.y * smoothstep(_CanyonDescent.x, _CanyonDescent.x + 0.25, posWS.z); }
float3 MarsDescentHaze(float3 col, float3 posWS)
{
    float b = MarsBeyondDescent(posWS);
    return lerp(col, SRGBToLinear(MarsHorizonPlain()) * float3(0.92, 0.94, 1.05), b * 0.55);
}
```

- [ ] **Step 2: Bütün alan dışı nesneleri oturt** — `case 51/52` roket (ölçüsü: tepesi üst yazılara ve güneşe değmez; 51'de `0.7` fazla gelirse küçült) ve oklu kaya, `52` kırıntılar (`StormScatter`'a `ground` geçer, `spots[i].y = ground(...)`), `59` oklu kaya (`MarkedBoulder` konumu `OnGround(...)`). Serçe havada süzüldüğü için `HoverSpot(areaHalf, ground)` y'si `ground(x, z) + 1.1f` olur (bütün `HoverSpot` çağrıları yeni imzaya).
- [ ] **Step 3: `CanyonLedge.Edge` yaz** — kenar boyunca (x: `-width/2..width/2`) 6-8 düzensiz, basık, koyu kaya (`MeshFactory.Rock`, `#5E3424`, yükseklik `0.12-0.22`), `pathX ± 0.35` aralığı boş (patika ağzı). Patikanın iki yanında direk (`RoundedCylinder(0.035f, 0.42f, ...)`, `#2E2624`) ve üstünde turuncu bant (`PaintMarks.PaintMat(0)`); direkler telefonda seçilecek boyda (spec: "belirgin"). Döndürdüğü yüzey: patika başında, alanın arka kenarı ile kenar arasında, yere yatık (`localRotation = Euler(90,0,0)`, `y = ground + 0.005`).
- [ ] **Step 4: `case 60` + `ResetBackdrop`:**

```csharp
            case LastLevel:
                // kanyona inen patika: alanin arkasindan kenara gider, kenarda asagi doner (spec 3a.2)
                var pathHead = CanyonLedge.Edge(parent, areaHalf.y + DescentGap, areaHalf.x * 2f + 3f, DescentPathX, ground);
                PaintMarks.Arrow(pathHead, Vector3.zero, 90f, 0.5f, fade: 0f, drips: false, seed: 600);
                ActiveSparrow = Sparrow.Create(parent, HoverSpot(areaHalf, ground), SparrowState.Flying);
                break;

    public const float DescentGap = 1.5f, DescentPathX = 0.9f;   // kenarin alana uzakligi; patikanin x'i (hedef sagda)
    static readonly int DescentId = Shader.PropertyToID("_CanyonDescent");
    // 60'ta zemin cizimine kenar bilgisi (x: kenarin z'si, y: 1 acik, z: patikanin x'i, w: alanin arka kenari). Build hesaplar,
    // ResetBackdrop yazar: bolum yuklenince ve "yeniden dene"de (Oyun.cs:744) ayni deger kalir; 60 disinda sifir.
    static Vector4 descent;

    public static void ResetBackdrop(int levelNumber)
    {
        Shader.SetGlobalFloat(IndoorId, IsDepot(levelNumber) ? 1f : 0f);
        Shader.SetGlobalVector(DescentId, levelNumber == LastLevel ? descent : Vector4.zero);
    }
```
  `Build`'in başında `descent = Vector4.zero;`, `case LastLevel`'da `descent = new Vector4(areaHalf.y + DescentGap, 1f, DescentPathX, areaHalf.y);`. Sıra doğru: `Traces.Build` önce `CanyonTraces.Build`'i (satır 37), sonra `ResetBackdrop`'u (satır 39) çağırır (Review Focus 2).
- [ ] **Step 5: Zemin boyası (`Ground.shader`, iki sürüm)** — `canyonFloor` sonunda (`depotFloor`'dan sonra) `canyonDescent(q, a)`:
  - **Patika** (alanın arka kenarından `z = _CanyonDescent.w`'den kenara `z = _CanyonDescent.x`'e): merkez çizgisi `x = _CanyonDescent.z + 0.15 * sin((q.y - w) * 2.0)`; genişlik `0.55` (spec: geniş, açık renkli); renk `float3(0.62,0.42,0.31)` (zeminden belirgin açık), kenarları yumuşak; üstünde ayak/tekerlek izi gibi iki ince koyu çizgi.
  - **Kenarın ötesi (vadi tabanı):** `MarsBeyondDescent` ile zemin rengi serinleşip soluklaşır (`MarsDescentHaze`, `frag`'da `MarsFadeToBackdrop`'tan önce uygulanır); patikanın devamı ince (`0.12`), soluk açık çizgi olarak kenardan uzağa kıvrılır: `x = pathX * (1 - s) + 0.25 * sin(s * 5.0)`, `s = saturate((q.y - edge) / 9.0)`; `s → 1`'de söner (zemin arka plana karışmadan önce biter; kayalıkların üstüne çıkamaz, çünkü zemin boyasıdır).
  - `_CanyonDescent.y == 0` iken hiçbir şey değişmez (erken çıkış).
- [ ] **Step 6: Uzak kayalar** — `FarRock.shader` renk hesabında `MarsFadeToBackdrop`'tan önce `col = MarsDescentHaze(col, posWS);`.
- [ ] **Step 7: Paket + görüntü** — `-shots <scratch>/t4 -bolum 51` (51-60). Bak: hiçbir nesne havada değil (özellikle 59 oklu kaya, 52 kırıntılar, 57 direkler); 56 pano + "KAPI"; 58 solmuş oklar + taze ok + "861"; Serçe'nin adı boyalı; 60 patika belirgin, kenarda kayboluyor, devamı vadide ve **kayalıkların altında kalıyor**, direkler seçiliyor. `-bolum 41` ilk görüntüsü değişmemiş (Review Focus 1). `calistir.bat` ile 60'ı açıp bölüm listesinden 59'a geç: patika boyası kalmamalı (Review Focus 2).
- [ ] **Step 8: Enes'e göster** — 56, 58, 60. **Onay gelmeden Task 5'e geçme.**

### Task 5: Performans modu, tam denetim, belgeler

**Files:**
- Create: `docs/tasarim/bolge-6-gorunus-2026-10-09/` (seçilmiş görüntüler)
- Modify: `docs/tasarim/senaryo-bolge-06.md` ("Oyuna konanlar": yeni 3a listesi)
- Modify: `docs/tasarim/hikaye-kitabi.md` (Serçe satırı: ad gövdeye boyalı)
- Modify: `docs/tasarim/telefon-denemesi.md` (51 ve 53 kare hızı satırı hâlâ doğru mu)
- Modify: `ILERLEME.md`

- [ ] **Step 1:** `-performans ac` ile `-shots <scratch>/t5p -bolum 51`; 51, 53, 60'a bak (zemin hafif sürümde doğru, beton, patika var; Review Focus 5).
- [ ] **Step 2:** `cd motor-test && dotnet test` → hepsi geçti.
- [ ] **Step 3:** `-shots <scratch>/t5f -bolum 50 -final` (`FINAL DENETIMI: TAMAM`, 51 girişi yeni kanyonla) + `powershell -ExecutionPolicy Bypass -File scripts/klavye-denetimi.ps1` → `KLAVYE DENETIMI: TAMAM`; log'da yedi fare denetimi TAMAM.
- [ ] **Step 4:** Görüntüleri (51-60 + performans 51, 53) `docs/tasarim/bolge-6-gorunus-2026-10-09/`'a kopyala. Eski `bolge-6-gorunus-2026-10-08/` kayıt olarak kalır (BENIOKU satırı: "beğenilmeyen ilk deneme").
- [ ] **Step 5:** Belgeler + ILERLEME.md (Enes adıyla: ne yapıldı, denenmeyenler, sıradaki 3b). Commit/push yalnızca oturum kapanışında (ekip döngüsü).
