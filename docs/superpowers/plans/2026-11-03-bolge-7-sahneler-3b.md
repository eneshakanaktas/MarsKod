# Bölge 7 Görünüşü — 3b üç sahne (63 kuş bakışı, 68 kalem, 70 final) · Uygulama Planı

> 2026-11-03 · Ragıp + Claude (Opus 5.5). Kod yazılmadı; bu plan yeni sohbette görev görev uygulanır.
> **Model · efor: Opus 5.5 · high** (kamera, hareketli sahne, "doğru görünüyor mu" kararı; CLAUDE.md'ye göre görsel/3B iş). Süre: ~5 sa (iki oturuma bölünebilir: Görev 1-2 / Görev 3-5).
> **Ara görüntü kuralı:** her görevin sonunda Ragıp'a görüntü gösterilir (kareler `docs/tasarim/bolge-7-sahneler-<tarih>/`'e değil, önce scratch klasöre; son hâl Görev 5'te kopyalanır). **Ragıp onaylamadan (ya da "onaysız devam" demeden) sonraki göreve geçilmez.** Beğenmezse aynı görevde düzeltilir. Yardımcı ajan yok, Claude işi kendisi yapar.

**Amaç:** Tasarımdaki üç sahneyi oyuna koymak: **63** her bitirişte Serçe'nin gözünden kuş bakışı (nehir kıvrımı + göç izi, ~6 sn); **68** Kıvılcım kalemi yerden alıp sırtına, kurdelenin yanına takar (~6-8 sn); **70** final "KIVILCIM BENİ BUL": duvara yürüyüş, kamera duvara döner, sessizlik, üç el yan yana, göğüsteki çizim ile duvardaki çizim aynı karede, turkuaz gözler (~20 sn).

**Tasarım:** `docs/superpowers/specs/2026-11-02-bolge-7-design.md` → "3b. Sahneler" (3b.1-3b.3) ve "3c". Hikâye: `docs/tasarim/senaryo-bolge-07.md`. Örnek alınan plan: `docs/superpowers/plans/2026-10-11-bolge-6-sahneler-3b.md`. 3a'nın hazır bıraktıkları: `docs/superpowers/plans/2026-11-02-bolge-7-gorunus-3a.md` → "3b için hazır bırakılanlar".

**Yapı:** Bölge 6'nın sahne düzeni aynen: kamera sahne sürerken `CameraShot`'ta; akış yeni `RiverBedScenes.cs`'te; nesneler yalnızca komut alır (Serçe `FlyTo`, robot `TakeToBack` / `PalmOnWall` / `EceEyes`). `CanyonScenes`'in özel yürüme yardımcıları (`Walk`, `CellsTo`, `Follow`) yeni `SceneWalker`'a taşınır; iki sahne dosyası onu kullanır (tekrar yok). Kuş bakışının görüş açısı hesabı `CameraShot`'a ortak statik yardımcı olur. `Oyun.cs`'e yalnızca çağrı satırları.

**Teknik:** Unity 6 (6000.6.3f1, URP), C# 9. `Motor/` ve `Dunya/` değişmez (`GridPath` olduğu gibi kullanılır).

---

## Kararlar (Ragıp, 2026-11-03: "onay almadan önerdiğin şekilde bitir" — hepsi Claude'un önerisi)

Hikâye görüntüsü Ragıp'ın alanı; görüntüde beğenilmezse değişir. Sonradan sorulacaklar en altta "Ragıp'a açık sorular"da.

| # | Soru | Karar (uygulanacak) | Diğer seçenek |
|---|---|---|---|
| K1 | 63 kuş bakışı ne kadar yüksekten, ne gösterir? | **55'ten yüksek:** alan küçük bir kare olarak ortada-altta, üstte (kuzeyde) nehir kıvrımı, alanın iki yanında göç izinin ekran kenarına kadar uzanan şeridi; Kıvılcım hedefte minik. Kuzey ekranın üstünde (normal bakışla aynı yön, oyuncu şaşırmaz). Salınım 55'teki gibi hafif. Önce Serçe yerinden biraz yükselir (yükseldiği için görüyor), sonra kırpışma. | 55 kadar yakın (yalnız alan) |
| K2 | 68'de kalem alanın dışında, yol her koşuda başka yerde biter. Nasıl alınır? | **Kıvılcım boş karelerden sol kenara, oradan kalemin yanına yürür** (`GridPath`; kayaların üstünden geçmez), eğilir, alır, kolunu omzunun üstünden geçirip sırtına takar. Takarken kamera kısa bir yakın çekimle arkasına gelir (3a'daki sırt kalemi karesi gibi), sonra yerine döner. Yürüyüş yüzünden tasarımdaki ~3 sn yerine ~6-8 sn. | Kalem yolun sonuna taşınır (yürüme yok); kamera hiç yaklaşmaz |
| K3 | 70'te Kıvılcım duvara elini koyunca iz kalır mı? | **Kalmaz:** el duvarda dururken üç el yan yana görünür, el kalkınca boş yer yine boş. Tasarımdaki gibi; sade ve sözsüz. (İz bırakması sonradan eklenebilir: `Hand.Robot` 3a'da hazır.) | Kıvılcım kalemle kendi elinin çevresini çizer |
| K4 | "Göğsündeki çizim ile duvardaki çizim aynı elden" nasıl gösterilir? | **Tek kare:** Kıvılcım elini çekip duvardaki çiçeğin yanına geçer, **sırtını duvara verip kameraya döner**; alçak, yakın kamera önde göğüsteki çiçeği, omzunun üstünde duvardaki çiçeği aynı karede gösterir. Kare okunmazsa yedek: duvardaki çiçek yakın → kesme → göğüsteki çiçek yakın (aynı boy). | Doğrudan iki çekim (kesme) |
| K5 | Turkuaz gözler kalıcı mı? | **Yalnızca 70'in sonunda:** gözler yumuşakça turkuaza döner, son kare böyle kalır; "Tekrar"da ya da bölüm değişince eski sıcak rengine döner (71'den sonrası değişmez). Kalıcı karakter değişikliği büyük karar: Ragıp'a açık soru. | 71'den sonra hep turkuaz; duygusal anlarda turkuaz |
| K6 | "Bütün sesler kesilir" — oyunda arka plan sesi yok, sessizlik nasıl hissedilir? | **Sahneye özel hafif rüzgâr sesi:** Kıvılcım duvara yürürken kodla üretilmiş rüzgâr yavaşça yükselir; yazı okunduktan sonra **bir anda kesilir** (kutlama sesinin kuyruğu da) → ~2 sn tam sessizlik. Sonra ses yok; gözler sessiz yanar. | Yalnız kutlama sesi biter (fark edilmez) |
| K7 | 70'in sahnesi her bitirişte ~20 sn. XP için yeniden çözen oyuncu ne yapar? | **İlk izleyişte atlanamaz; sonraki bitirişlerde ekrana dokununca son kareye atlar** (51'in `SkipCatcher`'ı). 63 ve 68 kısa, atlanmaz. | Hep atlanamaz (60 gibi) |
| K8 | 70 nasıl biter? | **Son kare kalır** (60 gibi): Kıvılcım duvarın önünde, turkuaz gözler, Serçe duvarın üstünde güneşte süzülür; kod kartı geri gelir, bitiş damgası "Sonraki: araştırma istasyonu". Kamera normal görünüme dönmez. | Kamera normal görünüme döner |

Ragıp'a sorulmayan, plan içinde verilmiş kararlar:
- **Kutlama önce:** üç sahne de `robot.Celebrate()`'ten sonra başlar (Bölge 6 gibi).
- **Kod kartı:** kamera önce sabitlenir, sonra kart gizlenir; kart geri geldikten sonra kamera bırakılır/döner (Bölge 6'da öğrenilen sıra, `CanyonScenes` yorumları).
- **Klavye açıkken bitiş:** sahne başında `hud.StopEditing()` (Bölge 6 gibi).
- **Değişen yol:** 68 iki, 70 üç farklı yerde biter; sahne robotun **o anki karesinden** başlar. Denetim her bitiş karesini ayrı dener.

---

## Genel kurallar

- Sis yok, derinlik gölgeyle; nesneler zemine basar; dünyada bilgisayar yazısı yok (70'teki duvar yazısı tek istisna, 3a'da çizildi); robotlar konuşmaz; **Kıvılcım uçmaz:** kaya, panel, toz, duvar içinden/üstünden geçmez. Serçe uçar ama üst yazılara girmez.
- Her şey kodla çizilir ve üretilir (rüzgâr sesi dahil); ses/doku dosyası yok (paket boyutu).
- Unity C# 9. Yeni her `.cs` için `.meta` (Unity üretir) git'e girer.
- `Oyun.cs` büyütülmez: yalnızca çağrı satırları (en fazla ~10 satır). Görüntü denetimi `Oyun.SahneDenetimi.cs`'te.
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıflar, tekrar yok, anlamlı adlar, ölçülü soyutlama. Yorumlar Türkçe ASCII, dosya başında ne olduğu.
- **Commit:** görev sonlarında yok; oturum kapanışında tek commit + push (ILERLEME.md ile birlikte).
- Denetim komutları (Bash; **`-shots` için mutlak yol**):
  - Paket: `rm -rf oyun/Build && "/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log`; `grep "error CS" build.log` ve `grep -i "shader error" build.log` boş.
  - Sahneler: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -sahneler -shots <scratch mutlak yol>/<klasör>` → log'da `SAHNE DENETIMI: TAMAM` (Bölge 6 sahneleri dahil). Performans modu: sona `-performans ac`.
  - Bölümler: `-shots <klasör> -bolum 61` (61-70; `3-bitti` kareleri artık sahnenin son karesi).
  - `cd motor-test && dotnet test` → hepsi geçer (şu an 1463).
- Arayüz/klavye denetimlerinde oyun penceresi **ön planda** olmalı (değilse sahte hata).

## Review Focus (her görevde akılda)

1. **Sahne ortasında "↻ Tekrar" ya da bölüm değişimi:** kamera serbest, kod kartı görünür, kırpışma/atlama katmanı kapalı, robot başlangıçta ve görünür, gözleri sıcak renk, Serçe evinde; 68'de kalem **yerde yerinde**, sırtta yok; ses yeniden çalışıyor (`Hush` sonrası). → Her görevin denetiminde sahne ortasında `ResetLevel` + bu maddeler.
2. **Bölge 6 bozulmadı:** `SceneWalker`'a taşıma davranışı değiştirmemeli. → Görev 1'de `-sahneler` TAMAM + `s60-*`, `s55-*`, `s51-*` kareleri `docs/tasarim/bolge-6-sahneler-2026-10-11/` ile aynı.
3. **Kıvılcım engelden geçmez:** 68 ve 70'te yol `GridPath` ile; alan dışına çıkış noktası boş şeritte (70: alan ile duvar arasındaki 0,9'luk şerit). → Denetimde robotun yolu boyunca kaya/duvar çakışması (Bölge 6'daki yarık çakışma denetimi kalıbı).
4. **Değişen yol:** 68'in iki, 70'in üç bitiş karesinden sahne düzgün başlar. → Denetim sahneyi her bitiş karesinden ayrı çalıştırır.
5. **Klavye açık bitiş + performans modu:** sahne kamerası küçülmüş görünümden sıçramadan başlar; `MARSKOD_SADE`'de nehir kıvrımı ve izler kuş bakışında görünür.
6. **Okunurluk:** 70'te duvar yazısı sahne kamerasında telefon ekranında rahat okunur; üç el ayrı ayrı seçilir; göğüsteki ve duvardaki çiçek aynı karede tanınır.

---

### Görev 1: Ortak yürüyüş + `RiverBedScenes` iskeleti + Bölüm 63 kuş bakışı

**Dosyalar:**
- Yeni: `oyun/Assets/Scripts/SceneWalker.cs`
- Yeni: `oyun/Assets/Scripts/RiverBedScenes.cs`
- Değişir: `oyun/Assets/Scripts/CanyonScenes.cs` (yürüme yardımcıları `SceneWalker`'a; davranış aynı)
- Değişir: `oyun/Assets/Scripts/CameraShot.cs` (`static float FovToFit(float halfWidth, float distance)`)
- Değişir: `oyun/Assets/Scripts/Oyun.cs` (alan, kurulum, kapanış satırı, `Restore` çağrıları)
- Değişir: `oyun/Assets/Scripts/Oyun.SahneDenetimi.cs` (`BirdViewShots`)

**Arayüzler:**

```csharp
// Sahnelerde Kivilcim'in yurumesi (Bolge 6 ve 7): yalnizca bos karelerden (GridPath), noktadan noktaya; Serce'nin eslik etmesi.
// Oyun kurali degil, sahne hareketi. Eskiden CanyonScenes'in icindeydi.
public class SceneWalker
{
    public SceneWalker(Robot robot, Func<Cell, Vector3> cellPos, int cols, int rows);
    public IEnumerator Walk(IReadOnlyList<Vector3> points, float secondsPerUnit);           // once doner, sonra yurur
    public List<Vector3> CellsTo(Level level, Cell from, Cell to, bool itemsTaken);        // yol yoksa yalniz 'to'
    public IEnumerator Follow(Sparrow sparrow, float lead, float height);                  // robotun onunde ve ustunde
    public Cell? OpenInRow(Level level, int row, int preferCol, bool itemsTaken);
    public int ColNear(float x);
    public Cell CellAt(Vector3 localPos);                                                  // robotun su anki karesi (68, 70)
}

// CameraShot: ekran oranina gore yatayda halfWidth'i 'distance' uzakliktan sigdiran gorus acisi (55 ve 63 kus bakisi, 70 duvar)
public static float FovToFit(float halfWidth, float distance);

// Bolge 7'nin sahneleri (spec 3b): 63 kus bakisi, 68 kalem, 70 final. Akis burada; nesneler yalnizca komut alir.
public class RiverBedScenes
{
    public RiverBedScenes(MonoBehaviour host, Robot robot, CameraShot shot, Hud hud, SceneWalker walker);
    public string Phase { get; }                       // goruntu denetimi hangi anda oldugunu buradan okur
    public IEnumerator BirdView(Level level);          // 63 (bu gorev)
    public IEnumerator TakeCrayon(Level level);        // 68 (Gorev 2)
    public IEnumerator Finale(Level level);            // 70 (Gorev 3-4)
    public void Restore(int levelNumber);              // "Tekrar" ve bolum degisimi
}
```

- [ ] **Adım 1: `SceneWalker`** — `CanyonScenes`'teki `Walk`, `CellsTo`, `Follow`, `ColNear` aynen taşınır (`CellsTo` artık konum listesi döndürür; `ConvertAll` çağrıları sadeleşir). `CanyonScenes` yapıcısı `SceneWalker walker` alır; `cellPos/cols/rows` alanları yalnızca hâlâ kullanılıyorsa kalır. `Follow`'un yüksekliği parametre (60'ta 1,1). Davranış değişmemeli.
- [ ] **Adım 2: `CameraShot.FovToFit`** — `CanyonScenes.BirdView`'deki `2 * Atan(halfWidth / (height * aspect))` hesabı buraya; `BirdView` onu çağırır (sonuç aynı, kırpma 30-100 aynı).
- [ ] **Adım 3: `RiverBedScenes` iskeleti** — alanlar, `Phase`, `Restore(levelNumber)`: `robot.gameObject.SetActive(true)`, `hud.SetCardVisible(true)`, `hud.ClearBlink()`, `hud.HideSkip()`, `shot.Release()`, `Phase = ""` (sonraki görevler kendi geri almalarını buraya ekler). Serçe'yi `RiverBedTraces.ResetBackdrop` zaten evine döndürür.
- [ ] **Adım 4: `BirdView` (63, ~6 sn)**:
  1. `hud.StopEditing()`; `Phase = "kalkis"`: Serçe yerinden ~1,2 birim yükselir (`FlyTo`, 0,9 sn); robot `LookUp` (kendi coroutine'i).
  2. `Phase = "kus"`: `shot.Begin()` → `hud.SetCardVisible(false)` → `hud.Blink(0.3f)` → 0,15 sn → `shot.Cut(eye, Quaternion.Euler(90f, 0f, 0f), fov)`, `shot.Sway = 1.2f`, 3 sn bekler.
     - Kadraj (K1): yatay yarı genişlik = `areaHalf.x + BirdRiverMargin` (başlangıç 2,5; görüntüde ayarlanır), merkez x = 0; merkez z alanın ortası ile nehir kıvrımının ortası (`areaHalf.y + 1.15`) arasında, kıvrım ekranın üst üçte birinde, alt kenarda üst/alt arayüze yer kalsın. Yükseklik sabit `BirdRiverHeight` (başlangıç 11), `fov = CameraShot.FovToFit(...)`.
  3. `hud.Blink(0.3f)` → 0,15 sn → `hud.SetCardVisible(true)` → 0,2 sn → `shot.Release()`.
  4. `Phase = "inis"`: Serçe `FlyTo(HomePosition, 1.2f)`; `Phase = "son"`.
- [ ] **Adım 5: `Oyun.cs`** — alan `RiverBedScenes riverScenes;` (yorum: `// Bolge 7 sahneleri: 63 kus bakisi, 68 kalem, 70 final`); `canyonScenes` kurulduğu yerde önce `var walker = new SceneWalker(robot, c => Pos(c), Cols, Rows);`, sonra iki sahne sınıfı onu alır. `RunProgram`'da `CanyonTraces.LastLevel` satırının altına üç satır: `63 → BirdView`, `68 → TakeCrayon`, `70 → Finale` (sabitler `RiverBedTraces.BirdViewLevel = 63` (yeni), `CrayonLevel`, `MuralLevel`). `canyonScenes.Restore()` çağrılan her yere `riverScenes.Restore(level.Number)`. (Görev 2-4'e kadar `TakeCrayon`/`Finale` yalnız `yield break`.)
- [ ] **Adım 6: Denetim (`BirdViewShots`)** — 63'ü yükle, `SetCode(level.Solution)`, `OnRun()`, 0,5 sn sonra `hud.FocusCode(0, 0)` (klavye açık bitiş); `kalkis`(+0,5) `s63-1-kalkis`, `kus`(+1,2) `s63-2-kus`, `son`(+0,5) `s63-3-son`. Geri alma: yeniden çalıştır, `kus`'ta `ResetLevel()` → kamera serbest, kart görünür, Serçe `HomePosition`'da, robot başlangıçta (aksi `problems`). `SceneShots`'ta `ScoutShots`'tan sonra. `WaitForPhase` Bölge 6 için `canyonScenes.Phase` okuyorsa, hangi sahnenin evresini beklediğini parametre alır (`Func<string>`), tekrar yazılmaz.
- [ ] **Adım 7: Paket + görüntü + kendi kontrolün** — `-sahneler` ve `-sahneler -performans ac` (ayrı klasör). Bak: (a) kuş bakışında nehir kıvrımı harita gibi okunuyor, göç izi şeridi alanın iki yanında ekran kenarına uzanıyor, el ele yürüyen küçük iz seçilebiliyor; (b) Kıvılcım minik ama görünür; (c) alan dışındaki zemin "bitmiyor" (kenarda boşluk/gökyüzü yok); (d) performans modunda kıvrım ve izler var (yoksa `riverMeander` hafif sürüme eklenir); (e) Bölge 6 kareleri öncekiyle aynı, `SAHNE DENETIMI: TAMAM`.
- [ ] **Adım 8: Ragıp'a göster** — `s63-1..3` + performans `s63-2`. **Onay gelmeden Görev 2'ye geçme.**

---

### Görev 2: Bölüm 68 — Kalem çantaya

**Dosyalar:**
- Değişir: `oyun/Assets/Scripts/Robot.cs` (`virtual IEnumerator TakeToBack(Transform item)`)
- Değişir: `oyun/Assets/Scripts/SparkBot.cs` (eğilip alma, omuzdan sırta; tek kol duruşu `SetArm(side, pose)`)
- Değişir: `oyun/Assets/Scripts/RiverBedTraces.cs` (kalemin ilk yeri saklanır; `ResetBackdrop` kalemi yerine koyar)
- Değişir: `oyun/Assets/Scripts/RiverBedScenes.cs` (`TakeCrayon`, `Restore`'a kalem/sırt)
- Değişir: `oyun/Assets/Scripts/Oyun.SahneDenetimi.cs` (`CrayonShots`)

**Arayüzler:**

```csharp
// Robot.cs — yerdeki nesneyi alip sirtina takar (Bolum 68 kalemi). Varsayilan: nesne kaybolur, sirttaki kalem gorunur.
public virtual IEnumerator TakeToBack(Transform item) { item.gameObject.SetActive(false); SetCrayon(true); yield break; }

// SparkBot.cs
static readonly Vector3 ArmOverShoulder = new Vector3(165f, 10f, 70f);   // sag kol omzun ustunden sirta (goruntude ayarlanir)
void SetArm(int side, Vector3 pose);                                     // SetArms iki kol icin bunu cagirir
public override IEnumerator TakeToBack(Transform item);

// RiverBedTraces.cs
public static void ResetBackdrop(int levelNumber);   // var olan; 68'de kalem gorunur ve ilk yerinde/acisinda
```

- [ ] **Adım 1: `SparkBot.TakeToBack`** — `posing = true`; (1) 0,4 sn: `lean` 0→26, iki kol `ArmReach`'e (`Collect` gibi, daha yavaş); (2) kalem sağ elin (`elbows[1]` altındaki `Hand`) dünya konumuna 0,15 sn'de gelir, sonra **ele bağlanır** (`SetParent(hand, true)`); (3) 0,6 sn: `lean` 26→0, sol kol `ArmRest`, sağ kol `ArmOverShoulder`; (4) 0,25 sn: kalem elden sırttaki kalemin (`crayon` alanı) dünya konumu/dönüşüne kayar; (5) yerdeki kalem gizlenir ve eski ebeveynine geri verilir (geri almada yerine konabilsin), `SetCrayon(true)`; (6) 0,3 sn: sağ kol `ArmRest`; `posing = false`. Kurdele uçları savrulmaya devam eder (Animate). Kalemin elde ölçüsü: yerdeki kalem (0,22) sırttakinden (0,105 × 2,1 ölçek ≈ 0,22) aynı boyda görünür; değilse kayarken ölçek de geçer.
- [ ] **Adım 2: Kalemin geri alınması** — `RiverBedTraces.GroundCrayon` kalemin yerel konum/dönüşünü saklar (`crayonHome`); `ResetBackdrop` 68'de kalemi ebeveynine, yerine, görünür hâle döndürür. `RiverBedScenes.Restore`: `robot.SetCrayon(levelNumber > RiverBedTraces.CrayonLevel)` (68'de "Tekrar" sırttaki kalemi kaldırır). `Oyun.cs`'te `SetCrayon` satırı `ResetLevel` yolunda çalışıyor mu bakılır; çalışıyorsa bu satır gereksizdir, eklenmez.
- [ ] **Adım 3: `TakeCrayon` (68, ~6-8 sn; K2)**:
  1. `hud.StopEditing()`; `Phase = "yol"`; robotun karesi `walker.CellAt(robot.transform.localPosition)`.
  2. Hedef kare: sütun 0'da, satırı kalemin z'sine en yakın boş kare (`OpenInRow`, `itemsTaken: true`); yol `walker.CellsTo(...)` + alanın dışına tek nokta: kalemin alan tarafında `CrayonReach` (başlangıç 0,32) uzaklıkta. Yürüme `walker.Walk(..., 0.42f)`. Serçe izler (`walker.Follow(sparrow, 0.4f, 1.4f)`, Serçe'nin kendi coroutine'i).
  3. Robot kaleme döner (`TurnTo`). `Phase = "alis"`: `shot.Begin()` → `hud.SetCardVisible(false)` → `shot.MoveTo(...)` 0,9 sn: robotun arka-yan çaprazından, sırt ve kalem aynı karede (3a'daki `CrayonCloseup` kadrajı, `robot.RibbonPoint`'e bakar).
  4. `robot.TakeToBack(RiverBedTraces.Crayon)`; `Phase = "sirt"`; 0,8 sn bekle (kalem kurdelenin yanında görünür).
  5. `hud.SetCardVisible(true)` → 0,2 sn → `shot.Return(1f)`; Serçe evine (`FlyTo(HomePosition)`); `Phase = "son"`.
- [ ] **Adım 4: Denetim (`CrayonShots`)** — 68'i yükle; **iki iz sonunun her biri için** (bölüm dosyasındaki `izler`'in son kareleri, `Trail` verisinden hesaplanır): robotu o kareye koy (`robot.ResetTo(Pos(end), yaw)`), `StartCoroutine(riverScenes.TakeCrayon(level))`; ilki için kareler `yol`(+0,6) `s68-1-yol`, `alis`(+0,5) `s68-2-alis`, `sirt`(+0,3) `s68-3-sirt`, `son`(+0,5) `s68-4-son`; ikincisi yalnız `son`'a varıyor mu (40 sn sınır). Her ikisinde: yürüyüş boyunca robot kaya karesine girmedi (her karede robotun karesi `GridPath.Obstacles`'ta değil), sonunda yerdeki kalem gizli, sırttaki görünür. Normal akıştan da bir kez: `SetCode(level.Solution)`, `OnRun()`, `son` geliyor. Geri alma: `alis`'te `ResetLevel()` → kalem yerde ve ilk yerinde, sırtta kalem yok, kamera serbest, kart görünür.
- [ ] **Adım 5: Paket + görüntü + kendi kontrolün** — Bak: eğilme ve alma inandırıcı (kalem elin içinde, havada değil), kol omzun üstünden geçerken gövdenin içinden geçmiyor, sırttaki kalem 3a'daki yerinde (`s69-sirt-kalem.png` ile aynı), kamera yaklaşırken ve dönerken sıçrama yok, gölgede kalem seçiliyor.
- [ ] **Adım 6: Ragıp'a göster** — `s68-1..4`. **Onay gelmeden Görev 3'e geçme.**

---

### Görev 3: Bölüm 70 final — 1. kısım: duvara yürüyüş, yazı, sessizlik, üç el

**Dosyalar:**
- Değişir: `oyun/Assets/Scripts/Sound.cs` (`Wind()`, `Hush()`)
- Değişir: `oyun/Assets/Scripts/Robot.cs` (`virtual IEnumerator PalmOnWall(Vector3 worldPoint, bool on)`, `virtual Vector3 PalmReach`)
- Değişir: `oyun/Assets/Scripts/SparkBot.cs` (sağ kol duvara: `ArmToWall`)
- Değişir: `oyun/Assets/Scripts/WallMural.cs` (`Spots`'a `SmallHand`, `BigHand`, `Flower` eklenir: kamera kadrajları için)
- Değişir: `oyun/Assets/Scripts/RiverBedScenes.cs` (`Finale`'nin ilk yarısı)
- Değişir: `oyun/Assets/Scripts/Oyun.SahneDenetimi.cs` (`FinaleShots70`)

**Arayüzler:**

```csharp
// Sound.cs — sahneye ozel ruzgar (70, K6): yavasca yukselen, alcak suzulmus hisirti (~7 sn); Hush: calan her sesi aninda keser
public static void Wind() => Play(Get("wind", () => Breeze(7f)), 0.3f);
public static void Hush() { if (source != null) source.Stop(); }

// Robot.cs — sag elini duvara koyar (on) ya da indirir; worldPoint avucun degecegi nokta. Varsayilan bir sey yapmaz.
public virtual IEnumerator PalmOnWall(Vector3 worldPoint, bool on) { yield break; }
// Kol one uzanmisken avucun govdeye gore yeri (robotun durmasi gereken uzaklik buradan hesaplanir)
public virtual Vector3 PalmReach => new Vector3(0.1f, 0.6f, 0.35f);

// WallMural.Spots — eklenen alanlar (hepsi bolum kokunun yerel konumu, duvarin biraz onu)
public readonly Vector3 RobotHand, TextCenter, SmallHand, BigHand, Flower;
```

- [ ] **Adım 1: `Sound.Wind` / `Hush`** — `Breeze(seconds)`: süzülmüş gürültü (`Hiss` kalıbında, daha alçak ve yumuşak; ara ara hafif yükselip alçalan), zarf: 2 sn'de yükselir, sonra sabit. Dosya başı listesine "rüzgâr (70 finali)". `Hush` yalnız çalanları keser; sonraki `Play` çağrıları normal çalışır (denetimde `Hush` sonrası `Sound.Tap()` hata vermiyor).
- [ ] **Adım 2: `SparkBot.PalmOnWall`** — `SetArm(1, ArmToWall)` (sağ kol öne-yukarı, dirsek hafif bükük; başlangıç `(95f, 8f, 15f)`), 0,7 sn yumuşak; sol kol `ArmRest`; baş hafif duvara eğik (`nod`). `PalmReach` bu duruşta avucun gövdeye göre yeri (ölç: `Hand` nesnesinin dünya konumundan). `on = false` kolu `ArmRest`'e indirir. `posing` sırasında boşta sallanma durur.
- [ ] **Adım 3: Robotun duracağı yer** — `Spots.RobotHand` (duvarın 0,05 önü) ile `PalmReach`'ten: robotun duvara bakarken avucu tam o noktaya değsin diye duracağı yerel konum (`StandFor(RobotHand)`). Avucun yüksekliği `RobotHandAt.y` ile tutmuyorsa (Kıvılcım boyu sabit) **üç el birlikte** `WallMural`'da robotun avucunun yüksekliğine kaydırılır (`SmallHandAt`, `BigHandAt`, `RobotHandAt` aynı hizada kalır; 69'un dizisi değişmez).
- [ ] **Adım 4: `Finale` ilk yarısı** — 
  1. `hud.StopEditing()`; `Sound.Wind()`; `Phase = "yol"`: robotun karesinden üst (kuzey) sıraya, `StandFor` noktasının x'ine en yakın boş kareye (`GridPath`), oradan alan ile duvar arasındaki şeride, şerit boyunca `StandFor` noktasına (`walker.Walk`, 0,45). Serçe izler.
  2. `Phase = "duvar"`: `shot.Begin()` → kart gizli → `shot.MoveTo(...)` 3 sn: duvarın önünde, alçakta, `TextCenter`'a bakan kamera; robot çizimi, iki satır yazı, çiçek, pasta ve eller aynı karede (`FovToFit(duvar resminin yarı genişliği + pay, uzaklık)`); robot kadrajın sağ altında, sırtı kameraya. 2,5 sn okuma (`Phase = "okuma"`).
  3. `Phase = "sessizlik"`: `Sound.Hush()`; 2 sn hiçbir şey olmaz (yalnız robotun boşta süzülmesi, Serçe sessiz süzülür).
  4. `Phase = "el"`: robot duvara döner, `PalmOnWall(..., true)`; kamera 1,5 sn'de üç ele yaklaşır (`SmallHand`-`RobotHand` arası, eller ekranın ortasında yan yana, küçük-büyük-robot boy farkı okunur). `Phase = "eller"`, 2 sn bekle.
- [ ] **Adım 5: Denetim (`FinaleShots70`, ilk yarı)** — 70'i yükle; **üç iz sonunun her biri için** robotu oraya koyup `Finale`'yi başlat; ilki için `yol`(+0,8) `s70-1-yol`, `okuma`(+0,5) `s70-2-duvar`, `sessizlik`(+1) `s70-3-sessizlik`, `eller`(+0,5) `s70-4-eller`. Hepsinde: yürüyüşte kaya karesine/duvara girmedi (robotun dünya sınırları duvar ağının sınırlarıyla kesişmiyor; Bölge 6 yarık denetimi kalıbı), `eller` anında avuç ile `RobotHand` arası < 0,06. Görev 4 bitene kadar `Finale` `eller`'den sonra `Phase = "son"` ile biter (geçici).
- [ ] **Adım 6: Paket + görüntü + kendi kontrolün** — Bak: duvar karesinde yazı telefonda rahat okunuyor (gerekirse kamera yaklaşır, robot çizimi ya da pasta kadrajdan taşmasın); sessizlikte ses gerçekten kesiliyor (elle bir kez `calistir.bat` ile dinle); üç el yan yana, robotun eli duvarın içine girmiyor; gölgede eller seçiliyor.
- [ ] **Adım 7: Ragıp'a göster** — `s70-1..4` (+ istenirse oyunda izletilir). **Onay gelmeden Görev 4'e geçme.**

---

### Görev 4: Bölüm 70 final — 2. kısım: iki çiçek, turkuaz gözler, son kare, atlama

**Dosyalar:**
- Değişir: `oyun/Assets/Scripts/Robot.cs` (`virtual IEnumerator EceEyes(float seconds)`; kameraya dönüş için var olan `TurnTo` yeter)
- Değişir: `oyun/Assets/Scripts/SparkBot.cs` (`EyeEce` rengi `CraterTraces.EceColor` parlak hâli; `OnReset` zaten `EyeCalm`'a döndürür)
- Değişir: `oyun/Assets/Scripts/RiverBedScenes.cs` (`Finale`'nin ikinci yarısı, `FinaleWithSkip`, `FinishFinale`)
- Değişir: `oyun/Assets/Scripts/Oyun.cs` (70 satırı `FinaleWithSkip`'e döner; yeni satır yok)
- Değişir: `oyun/Assets/Scripts/Oyun.SahneDenetimi.cs` (`FinaleShots70` ikinci yarı + atlama)

**Arayüzler:**

```csharp
// Robot.cs — gozler yumusakca Ece'nin rengine (turkuaz) doner (70 sonu, K5). ResetTo eski renge dondurur.
public virtual IEnumerator EceEyes(float seconds) { yield break; }

// RiverBedScenes.cs
public IEnumerator FinaleWithSkip(Level level);   // ilk izleyiste Finale; sonra dokununca FinishFinale (K7)
void FinishFinale(Level level);                    // son kareye atlar: robot ciceklerin onunde kameraya donuk, gozler turkuaz,
                                                   // Serce duvarin ustunde, kamera son kadrajda, ses kesik, Phase "son"
const string FinaleSeenKey = "final_70";           // PlayerPrefs: ilk izleyis bitti mi
```

- [ ] **Adım 1: `Finale` ikinci yarısı (K4, K5, K8)** —
  1. `Phase = "donus"`: `PalmOnWall(..., false)`; robot şerit boyunca çiçeğin soluna yürür (`Spots.Flower`'ın x'inden ~0,45 sola, duvardan `FlowerStand` uzaklıkta), **kameraya doğru (güneye) döner**; sırtı duvarda, çiçek sağ omzunun üstünde.
  2. `Phase = "cicekler"`: kamera 1,5 sn'de alçak ve yakın önden: göğüsteki çizim kadrajın sol-ortasında, duvardaki çiçek sağ üstte, ikisi de seçilir boyda. 2,5 sn bekle. **Okunmuyorsa (görüntüde karar) yedek:** duvardaki çiçek yakın → `Cut` → göğüsteki çiçek yakın, aynı boy (iki çekim, 1,5'er sn).
  3. `Phase = "gozler"`: `robot.EceEyes(1.2f)`; aynı anda kamera 2,5 sn'de geri çekilir: son kadraj (duvar resmi üstte, Kıvılcım önde kameraya bakar); Serçe duvarın üstüne, güneşli şeride uçar (`FlyTo`, 2 sn) ve orada süzülür.
  4. `hud.SetCardVisible(true)`; `Phase = "son"`. Kamera bırakılmaz (K8); "Tekrar"/bölüm değişimi `Restore` ile bırakır. Gözler `ResetTo` → `OnReset` ile eski renge döner (denetlenir).
- [ ] **Adım 2: `SparkBot.EceEyes`** — `EyeEce = Mats.Hex(...EceColor) * 1.6f`; `eyeColor` 1,2 sn'de `EyeCalm`'dan `EyeEce`'ye `Color.Lerp`; kırpma (`wink`) sürer. Gözlerin malzemesi `Emissive`: 3a'daki parlama düzeltmesinden sonra pakette parlıyor mu bakılır; parlamıyorsa 55'teki Serçe gözleri gibi `Unlit` yedek.
- [ ] **Adım 3: Atlama (K7)** — `FinaleWithSkip`: `PlayerPrefs.GetInt(FinaleSeenKey, 0) == 1` ise `hud.ShowSkip(() => skip = true)`. `Finale` `host.StartCoroutine` ile koşar; her karede `skip` bakılır; atlanırsa `host.StopCoroutine`, Serçe ve robotun koşan hareketleri durur, `FinishFinale` son hâli kurar. Bitince ya da atlanınca `hud.HideSkip()`; ilk tam izleyişin sonunda (`!shotsMode` — yapıcıya `Func<bool> saveProgress` olarak verilir) `PlayerPrefs.SetInt(FinaleSeenKey, 1)`.
- [ ] **Adım 4: Denetim (`FinaleShots70` tamamı)** — üç iz sonunun hepsinden `son`'a varıyor; ilk koşudan `donus`(+0,8) `s70-5-donus`, `cicekler`(+0,8) `s70-6-cicekler`, `gozler`(+1,5) `s70-7-gozler`, `son`(+0,5) `s70-8-son`. `son`'da gözlerin rengi turkuaz (`SparkBot` salt-okunur `EyeColor` ya da malzemeden). **Atlama:** `FinaleSeenKey = 1`, yeniden başlat, `yol`'da atlama katmanının `onTap`'i → `Phase == "son"`, robot son yerinde, gözler turkuaz, katman kapalı. **Geri alma:** `cicekler`'de `ResetLevel()` → Review Focus 1'in hepsi + gözler sıcak renk + `Sound.Tap()` çalışıyor. `PlayerPrefs` denetimden sonra eski değerine döner. Normal akıştan bir kez (`SetCode(level.Solution)`, `OnRun()`): kutlama → sahne → `son`, bitiş damgası okunuyor.
- [ ] **Adım 5: Paket + görüntü + kendi kontrolün** — Bak: iki çiçek aynı karede tanınıyor (aynı biçim, ikisi de turkuaz), robot duvara/çiçeğin üstüne girmiyor, son kare dengeli (yazı, Kıvılcım, Serçe, damga birbirini kapatmıyor), turkuaz gözler gölgede net, kart geri gelince yazı kartın altında kalmıyor (gerekirse son kadraj yukarı kayar). `calistir.bat` ile bir kez baştan sona oyunda izle (sessizliğin süresi, ritim ~20 sn).
- [ ] **Adım 6: Ragıp'a göster** — `s70-5..8` + 70'in tamamı oyunda (istenirse ekran kaydı: `docs/tasarim/taslaklar.md` kuralı). **Onay gelmeden Görev 5'e geçme.**

---

### Görev 5: 3b son denetimi, belgeler

**Dosyalar:**
- Değişir: `docs/tasarim/senaryo-bolge-07.md` ("Oyuna konanlar: 3b sahneler": dosyalar, K1-K8'in sonucu)
- Değişir: `docs/tasarim/hikaye-kitabi.md` (Kıvılcım'ın turkuaz gözleri: yalnız 70 sonu; kalem 68'de sırta)
- Değişir: `docs/tasarim/telefon-denemesi.md` (63, 68, 70 sahneleri için `KARE:` satırları; 70'te ses: rüzgâr + sessizlik telefonda duyuluyor mu)
- Değişir: `CLAUDE.md` (Çalıştırma: `-sahneler` satırına Bölge 7 sahneleri: 63 kuş bakışı, 68 kalem, 70 final)
- Değişir: `ILERLEME.md`
- Yeni klasör: `docs/tasarim/bolge-7-sahneler-<tarih>/` (son görüntüler)

- [ ] **Adım 1:** `cd motor-test && dotnet test` → hepsi geçer.
- [ ] **Adım 2:** Windows paketi hatasız; `-sahneler` → `SAHNE DENETIMI: TAMAM` (Bölge 6 + 7); `-sahneler -performans ac` → TAMAM; `-bolum 61` turu: 61-70 "Tamamlandı", 63/68/70 `3-bitti` kareleri sahnenin son karesi, diğerleri 3a görüntüleriyle aynı (`docs/tasarim/bolge-7-gorunus-2026-11-03/61-70-ilk-kareler.png`); `-bolum 50 -final` TAMAM; klavye denetimi TAMAM; arayüz denetimleri TAMAM (pencere ön planda); `-bolum 51` ve `-bolum 41` ilk kareleri öncekiyle aynı.
- [ ] **Adım 3:** Klavye açıkken 63 ve 70 bitirişi (denetimde var; ayrıca bir kez elle): sahne kamerası sıçramadan başlıyor.
- [ ] **Adım 4: Belgeler** — yukarıdaki dosyalar; görüntüler `docs/tasarim/bolge-7-sahneler-<tarih>/` (`s63-*`, `s68-*`, `s70-*`, performans `s63-2`; ara denemeler silinir); `ILERLEME.md` (Ragıp adıyla: ne yapıldı, denenmeyenler — telefonda sahne akıcılığı, rüzgâr ses düzeyi, sessizliğin süresi; açık sorular; sıradaki: **Bölge 8-10 ders + bulmaca planı**, Opus 5.5 · high, yeni sohbet).
- [ ] **Adım 5: Ragıp'a son tur** — üç sahnenin anahtar kareleri yan yana (63 kuş, 68 sırt, 70 eller + çiçekler + son). Onay → Bölge 7 görünüşü (3a + 3b) biter.

---

## Ragıp'a açık sorular (plan kararları önerildiği gibi verildi; görüntüde ya da sonra değişebilir)

1. **K5 — Turkuaz gözler kalıcı mı?** Şimdilik yalnız 70'in sonunda. 71'den sonra hep turkuaz olursa Kıvılcım Ece'nin rengini taşır (büyük karakter kararı, dizi görünüşünü de etkiler).
2. **K3 — Kıvılcım duvara kendi el izini bıraksın mı?** Şimdilik bırakmaz. Bıraksa "robot konuşmaz ama cevap verdi" anı olur; kalemi sırttan alıp çizmesi gerekir (ek ~1 sa).
3. **K7 — 70'in atlanabilmesi:** ilk izleyişten sonra dokununca atlanır. 60 (Bölge 6 finali) hiç atlanmıyor; ikisi farklı kalabilir mi?
4. **K2 — 68'in süresi:** yürüyüş yüzünden ~6-8 sn (tasarımda ~3 sn). Uzun gelirse kalem yolun sonuna yakın taşınabilir.

## Kapsam dışı

Kıvılcım'ın duvara iz bırakması (soru 2), kalıcı turkuaz gözler (soru 1), Bölge 8-10 ders/bulmaca planı, kayıt ekranının ses dalgası rengi, telefon testleri (ILERLEME.md listesi), `TestAllLevelsOpen`/`TestSolutionsFilled` (Play Store öncesi), Bölge 5'in kendi görünüşü, 3a'nın açık maddeleri (68-70'te iki kez "Çalıştır" → eski izler, uzaktan izlerin görünüşü; ayrı küçük iş).
