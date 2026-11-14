# Bölge 8 Görünüşü — 3b üç sahne (76 katlanmış çizim, 78 açıklama anı, 80 kapanış) · Uygulama Planı

> 2026-11-14 · Ragıp + Claude (Opus 5.5). Kod yazılmadı; bu plan yeni sohbette görev görev uygulanır.
> **Model · efor: Opus 5.5 · high** (kamera, hareketli sahne, "doğru görünüyor mu" kararı; CLAUDE.md'ye göre görsel/3B iş). Süre: ~6 sa, iki oturum: **Görev 1-2** (~3 sa) / **Görev 3-4** (~3 sa).
> **Ara görüntü kuralı:** her görevin sonunda Ragıp'a görüntü gösterilir (kareler önce scratch klasöre; son hâl Görev 4'te `docs/tasarim/bolge-8-sahneler-<tarih>/`'e). **Ragıp onaylamadan (ya da "onaysız devam" demeden) sonraki göreve geçilmez.** Beğenmezse aynı görevde düzeltilir. Yardımcı ajan yok, Claude işi kendisi yapar.

**Amaç:** Senaryodaki üç anı oyuna koymak:
- **76** Kıvılcım ayağının dibinde beliren katlanmış çizimi alır, açmaya yeltenir, durur, Ece'nin yatağına bakar, geri katlar ve sırtına koyar (~6 sn).
- **78** Ekrana satırlar düşerken BKM-7 satırında akış durur. Terminal turuncu kalır, kamera Kıvılcım'a yaklaşır, Kıvılcım donar ve başını göğsündeki çizime eğer. Sonra akış devam eder ve bölüm **sevinç dansı olmadan** biter (~6 sn).
- **80** Kapanış: haritanın önü → kırpışma → koridorda son çizimin (robot ve kız el ele) önü + terminal kutusunda "BKM-7 — dışarıda — görev: KAPI" → kırpışma → harita odası, Kıvılcım başını haritadaki oklara kaldırır (~14 sn).

**Dayandığı belgeler:**
- Hikâye: `docs/tasarim/senaryo-bolge-08.md` ve `senaryo-perde-2.md` (Bölge 8 kapanışı). 80'in kapanışı için senaryoda şöyle yazıyor: "Kıvılcım duvardaki son çizimin, kendisini ve Ece'yi gösteren çizimin önünde. Sonra listedeki kendi satırına bakar: 'görev: KAPI'. Başını kaldırır; artık neden burada olduğunu biliyor."
- 3a planı: `docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md`; buradan "3b için hazır bırakılanlar" tablosu kullanılır.
- Örnek alınan plan: `docs/superpowers/plans/2026-11-03-bolge-7-sahneler-3b.md`.

**Yapı:** Bölge 6-7'nin sahne düzeni aynen kullanılır:
- Sahne sürerken kamera `CameraShot`'ta olur.
- Akış yeni `StationScenes.cs`'te yazılır.
- Nesneler yalnızca komut alır: robot için `TakeToBack`, `TryUnfold`, `Realize`, `RaiseHeadTo`; terminal için `StationScreens.HoldHighlight`; hatıra satırı için `Hud.ShowRecall`.
- 80'in koridor sahnesi (`CorridorSet`) yalnız sahne için kurulur ve kaldırılır. Yerleşimi `StationTraces` seçer.
- 70'in "ilk izleyişten sonra atlanır" mantığı ortak `SceneSkip`'e taşınır ve 80 de onu kullanır (tekrar yok).
- `Oyun.cs`'e yalnızca çağrı satırları eklenir.

**Teknik:** Unity 6 (6000.6.3f1, URP), C# 9. `Motor/` ve `Dunya/` değişmez.

---

## Kararlar

### Ragıp'ın seçtikleri (2026-11-14, dördü de önerildiği gibi)

| # | Soru | Karar |
|---|---|---|
| K1 | 80 kapanışı koridorda mı, harita odasında mı? | **Harita odası + tek kesme.** Kıvılcım haritanın önünde durur. Ekran kırpışır ve koridora geçilir (75'in duvarı, aynı 5 çizim). Kıvılcım son çizimin önündedir; alttaki terminal kutusunda tek satır turuncu yanar: `BKM-7   — dışarıda — görev: KAPI` (onun hatırası). Ekran yine kırpışır, harita odasına dönülür; Kıvılcım başını kaldırıp haritadaki turuncu oklara bakar. Böylece hem "neden burada" hem "nereye" aynı kapanışta söylenir. |
| K2 | 78'de BKM-7 satırı bulununca ne olsun? | **Akış durur, sessiz anlama.** Satırlar düşerken BKM-7'de durulur, terminal turuncu kalır, kamera yavaşça yaklaşır. Kıvılcım bir an donar, başını göğsündeki çizime eğer, sonra yine ekrana bakar. Son satır (KOR) düşer ve bölüm biter. **Sevinç dansı yok**, bitiş sesi yumuşak. |
| K3 | 76'da çizim nasıl alınsın? | **Açmaya yeltenir.** Kıvılcım eğilip çizimi alır, göğsünün önünde tutar ve üst katın köşesini biraz kaldırır. Sonra durur, Ece'nin (küçük) yatağına döner, kâğıdı geri katlayıp omzunun üstünden sırtına koyar. Kimse söylemeden "Açılmadı" damgasına anlam verir: çizimi Ece'ye götürüyor. |

### Planın verdiği kararlar (Ragıp'a sorulmadı; görüntüde beğenilmezse değişir)

- **Kutlama sırası:** 76 ve 80'de sahne `robot.Celebrate()`'ten sonra başlar (Bölge 6-7 gibi). 78'de kutlama yoktur (K2).
- **80'de dans kalır:** 70'te de dans vardı, sonra duygusal sahne geliyordu. Beğenilmezse 78 gibi sessiz biter (açık soru 1).
- **Atlama (Ragıp, 2026-11-14 — K4, bütün sahneler için kural):** Hikâyeye bakmadan oynamak isteyenler olacak; bu yüzden sahnelerin neredeyse hepsi **ilk izleyişten itibaren** atlanabilir.
  - Ekranın altında küçük bir **"Atla"** düğmesi olur; düğmenin üstünde sahnenin kaç saniye daha süreceği yazar (`12 sn`, her saniye azalır).
  - Düğme sahne başladıktan **1 sn sonra** belirir. Böylece "Çalıştır"a dokunan parmak sahneyi yanlışlıkla atlatmaz.
  - Atlanınca sahnenin **sonucu yine kurulur**: 76'da çizim sırtta olur, 80'de son kare, 70'te turkuaz gözler. Hikâyeyi izlemeyen oyuncu da izleri görür.
  - **3 sn'den kısa sahnelerde düğme yoktur** (çıkıp hemen kaybolması göz yorar).
  - Kalan süre, her sahnenin kendi bildirdiği tahmini uzunluktan (`Length`) geçen süre çıkarılarak hesaplanır, en az 1 sn gösterilir. Yürüyüşle uzayan sahnelerde tahmin sahnenin başında yoldan hesaplanır.
  - 70'teki "ilk izleyiş atlanamaz" kuralı ve tam ekran dokunma katmanı kalkar; yerine bu düğme gelir (Görev 5).
- **78'in koşulu:** Sahne yalnız **bütün çözüm doğruysa** oynar (`report.Complete`; rapor akış başlamadan önce hazırdır). O zaman BKM-7 satırının cevabı doğru ve `True` olur. Kod yanlışsa 3a'daki gibi terminal bir an turuncu yanar, akış durmaz, dans zaten yoktur.
- **Hatıra satırı (80):**
  - Sahnede okunur yazı yok kuralı korunur: satır dünyada değil, arayüzdeki terminal kutusunda görünür (78'deki `ScreenView` görünüşü).
  - Kutuda tek satır olur, 78'deki bulunan satırın görünüşünde: kehribar zemin, koyu yazı. Başlık `İSTASYON EKRANI` olur, fonksiyon adı yazmaz; "→ True ✓" kısmı da yoktur.
  - Metin `bolum-78.json`'daki satırla **harfi harfine aynıdır** ve denetimde ölçülür.
- **Koridor sahnesi nasıl kurulur:**
  - Kırpışmanın en karanlık anında 80'in odası ve alan eşyaları gizlenir.
  - Aynı yerde 75'in odası kurulur: aynı tohum (`875`), 5 çizim, aynı duvar. Oyuncu 75'te gördüğü duvarı tanır.
  - Dönüşte koridor silinir, 80 eski hâline gelir.
  - Koridor önceden kurulmaz: 80 açılırken telefonu yormasın.
- **Kıvılcım koridorda nerede durur:** Alanın üst sırasında, son çizimin x'ine en yakın sütunda, kuzeye (duvara) dönük. Çizim çocuk boyunda olduğu için Kıvılcım'ın başı hizasındadır. Kamera alçaktan ve yan-arkadan bakar: çizim büyük görünür, Kıvılcım'ın başı ve omzu önde, sırtındaki katlanmış çizim de görünür. Yani Ece'nin iki çizimi aynı karede olur: duvardaki açık, sırttaki katlı. Kimse söylemez.
- **Son kare (80):**
  - Kamera bırakılmaz (60 ve 70 gibi). Üstte haritada turuncu oklar, altta arkadan Kıvılcım başını kaldırmış hâlde görünür.
  - Kod kartı geri gelir, bitiş damgası okunur: "Oklar: tuz gölünün ötesi · Sonraki: tuz gölü".
  - Gözler sıcak renkte kalır: turkuaz yalnız 70'e özel.
- **Ses:**
  - Yeni ses dosyası yok; her ses kodla üretilir.
  - 78 bitişinde `Sound.Celebrate()` yerine yeni `Sound.Quiet()` çalar: tek, alçak, kısa iki notalı yumuşak tını.
  - 80'de rüzgâr yok (iç mekân). Kırpışmalar sessiz geçer.
- **Değişen yol:**
  - 76 ve 80'in üç not sürümü üç farklı karede biter; sahne robotun o anki karesinden başlar.
  - Denetim her bitiş karesini ayrı ayrı dener.
- **Kamera ve kart sırası:** Önce kamera sabitlenir (`shot.Begin()`), sonra kart gizlenir. Kart geri geldikten sonra kamera bırakılır ya da yerine döner (Bölge 6-7'den öğrenilen sıra). Sahnenin başında `hud.StopEditing()` çağrılır (klavye açıkken bitiş).

---

## Genel kurallar

- Sis yok; derinlik gölgeyle verilir. Nesneler zemine basar.
- Dünyada bilgisayar yazısı yok: okunan her yazı arayüzde görünür.
- Robotlar konuşmaz.
- **Kıvılcım uçmaz:** eşya, yatak, duvar ya da masanın içinden veya üstünden geçmez. 76 ve 80'de yürüme yok; koridorda Kıvılcım kırpışma sırasında yer değiştirir (kesme).
- Her şey kodla çizilir ve üretilir; ses ya da doku dosyası eklenmez (paket boyutu).
- Unity C# 9. Her yeni `.cs` dosyasının `.meta`'sı (Unity üretir) git'e girer.
- `Oyun.cs` büyütülmez: yalnızca çağrı satırları eklenir (en fazla ~12 satır). Görüntü denetimi `Oyun.IstasyonDenetimi.cs`'te.
- Kod kalitesi (CLAUDE.md): her sınıf tek iş yapar, tekrar yok, anlamlı adlar kullanılır, soyutlama ölçülü olur. Yorumlar Türkçe ve ASCII; her dosyanın başında ne olduğu yazar.
- **Commit:** Görev sonlarında commit yok. Oturum kapanışında tek commit + push yapılır (ILERLEME.md ile birlikte).
- **Denetim komutları** (Bash; `-shots` için mutlak yol kullanılır):
  - **Paket:** `rm -rf oyun/Build && "/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log`. Ardından `grep "error CS" build.log` ve `grep -i "shader error" build.log` boş çıkmalı.
  - **Bölge 8 sahneleri:** `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -sahneler -bolge8 -shots <scratch mutlak yol>/<klasör>` → log'da `SAHNE DENETIMI (yalniz Bolge 8): TAMAM` yazmalı (~3 dk + bu planın sahneleri).
  - **Bölge 7 sahneleri:** komuta `-sahneler -bolge7` eklenir (~7 dk). Yalnızca `SparkBot.TakeToBack` ve `SceneSkip` değişiminden sonra, yani Görev 1 ve Görev 3'te çalıştırılır (68 ve 70 bu kodu kullanır).
  - **Motor testleri:** `cd motor-test && dotnet test` → hepsi geçmeli (şu an 1711). Motor değişmez; yalnız Görev 4'te bir kez çalıştırılır.
- Arayüz ve klavye denetimlerinde oyun penceresi **ön planda** olmalı (değilse sahte hata çıkar).

## Review Focus (her görevde akılda)

1. **Sahnenin ortasında "↻ Tekrar" ya da bölüm değişimi:** Şunların hepsi denetlenir; her görevin denetimi sahne ortasında `ResetLevel` çağırıp bunlara bakar.
   - Kamera serbest, kod kartı görünür, kırpışma ve atlama katmanı kapalı.
   - Robot başlangıç yerinde ve normal duruşta: başı düz, kolları inik, gözleri sıcak renkte.
   - 76: çizim ne elde ne sırtta; yerdeki çizim de kalkmış (`NoteEnd.Restore`).
   - 78: terminal boşta, akış durmamış hâlde.
   - 80: koridor silinmiş, 80'in odası, haritası ve eşyaları görünür, hatıra kutusu kapalı.
2. **Bölge 7 bozulmadı:** `TakeToBack` genelleştirmesi ve `SceneSkip`'e taşıma davranışı değiştirmemeli. → Görev 1 ve 3'te `-sahneler -bolge7` TAMAM olmalı; `s68-*` ve `s70-*` kareleri `docs/tasarim/bolge-7-sahneler-*` ile aynı görünmeli.
3. **Değişen yol:** 76'nın ve 80'in üç not sürümünün her bitiş karesinden sahne düzgün başlar. Çizim yatağın, Kıvılcım eşyanın içinde kalmaz.
4. **Çanta sırası ve kalıcılık:** 76'nın sonunda sırtta soldan çizim, kalem, kurdele görünür; çizim 3a'daki yerinde durur (`s77-sirt-cizim` ile aynı). 77'den sonra hep sırttadır. 76'da "Tekrar" yapılınca sırttan kalkar.
5. **Okunurluk (telefon ekranı):**
   - 78'de kamera yaklaşırken Kıvılcım ve turuncu terminal, alt yarıdaki ekran kutusunun **üstünde** kalır.
   - 80'de son çizim ve hatıra satırı aynı anda okunur. Son karede oklar ve Kıvılcım birbirini kapatmaz; kart geri gelince damga okunur.
6. **Klavye açık bitiş + hafif mod (`-performans ac`):** Sahne kamerası küçülmüş görünümden sıçramadan başlar. Hafif modda koridor ışıkları ve çizimler görünür.

---

### Görev 1: `StationScenes` iskeleti + Bölüm 76 — katlanmış çizim sırta (~1,5 sa)

**Dosyalar:**
- Yeni: `oyun/Assets/Scripts/StationScenes.cs`
- Değişir:
  - `oyun/Assets/Scripts/Robot.cs`: `TakeToBack` hangi sırt yerine konacağını alır; yeni `TryUnfold`.
  - `oyun/Assets/Scripts/SparkBot.cs`: iki sırt yeri; kâğıdı göğsün önünde tutma; kâğıt sırttaki küçük boyuna kayar.
  - `oyun/Assets/Scripts/FoldedDrawing.cs`: üst kat bir kenarından menteşeli olur; `SetOpen(Transform paper, float amount)`.
  - `oyun/Assets/Scripts/StationProps.cs` ve `StationTraces.cs`: Ece'nin yatağının yeri `StationTraces.EceBed`.
  - `oyun/Assets/Scripts/RiverBedScenes.cs`: 68 çağrısı `TakeToBack(crayon, BackSlot.Crayon)` olur; davranış aynı kalır.
  - `oyun/Assets/Scripts/Oyun.cs`: alan, kurulum, 76 çağrı satırı, `Restore` çağrıları.
  - `oyun/Assets/Scripts/Oyun.IstasyonDenetimi.cs`: `DrawingTakeCheck`.

**Arayüzler:**

```csharp
// Robot.cs — sirttaki yerler (canta sirasi soldan: cizim, kalem, kurdele)
public enum BackSlot { Crayon, FoldedDrawing }
// yerdeki nesneyi alip sirtina takar (68 kalem, 76 cizim). Varsayilan: nesne kaybolur, sirttaki gorunur.
public virtual IEnumerator TakeToBack(Transform item, BackSlot slot);
// 76: elindeki katli kagidi gogsunun onune getirir, ust katin kosesini 'amount' kadar kaldirir, 'hold' sn durur, geri katlar
public virtual IEnumerator TryUnfold(Transform paper, float amount, float hold) { yield break; }

// FoldedDrawing.cs — ust kat bir kenarindan menteseli ("Kat-Ust-Mentese"); 0 katli, 1 tam acik (3b'de en fazla ~0,35)
public static void SetOpen(Transform paper, float amount);

// StationTraces.cs — 76: Ece'nin (kucuk) yataginin ortasi, alanin yerel konumu; baska bolumlerde null
public static Vector3? EceBed { get; }

// Bolge 8'in sahneleri (3b): 76 cizim, 78 aciklama ani, 80 kapanis. Akis burada; nesneler yalnizca komut alir.
public class StationScenes
{
    public StationScenes(MonoBehaviour host, Robot robot, CameraShot shot, Hud hud, Func<bool> saveProgress);
    public string Phase { get; }                     // goruntu denetimi hangi anda oldugunu buradan okur
    public IEnumerator TakeDrawing(Level level, NoteEnd end);   // 76 (bu gorev)
    public IEnumerator Reveal(int row);              // 78 (Gorev 2)
    public IEnumerator ClosingWithSkip(Level level); // 80 (Gorev 3)
    public void Restore();                           // "Tekrar" ve bolum degisimi
}
```

- [ ] **Adım 1: `TakeToBack` genelleştirmesi.**
  - `SparkBot`'ta hedef yer `slot`'a göre seçilir: `crayon` ya da `foldedDrawing` (iki alan zaten var).
  - Kayarken nesnenin **ölçeği** de hedefin dünya ölçeğine geçer (sırttaki kâğıt 0,36 ölçekli; kalemde ölçek zaten aynı, değişmez).
  - Sonunda `SetCrayon(true)` ya da `SetFoldedDrawing(true)` çağrılır.
  - Yerdeki nesne eski ebeveynine gizli olarak geri verilir (68'deki gibi; 76'da `NoteEnd.Restore` onu siler).
  - 68'in çağrısı `BackSlot.Crayon` olur. Bölge 7'nin davranışı **aynı kalmalı**.
- [ ] **Adım 2: Menteşeli üst kat.**
  - `FoldedDrawing.Create`'te üst kat (`Kat-Ust`) ile kat izi, kâğıdın uzun kenarındaki bir menteşe boşluğunun (`Kat-Ust-Mentese`) altına alınır. Görünüş piksel piksel aynı kalmalı (3a karesi `s76-*` ile karşılaştır).
  - `SetOpen` menteşeyi kenar ekseninde 0→~110° çevirir.
  - İçi görünmez: üst katın alt yüzü krem kalır, yalnız turuncu çizgi ucu biraz daha uzun görünür. Çizimin içi Perde 4'e saklı.
- [ ] **Adım 3: `SparkBot.TryUnfold`.**
  - `posing = true`. İki kol 0,4 sn'de `ArmHoldNote`'a yakın bir duruşa geçer (`ArmHoldPaper`, görüntüde ayarlanır); kâğıt sağ eldedir.
  - Baş kâğıda eğilir (`NoteNod`).
  - `FoldedDrawing.SetOpen` 0,5 sn'de 0→`amount`; sonra `hold` sn durur; sonra 0,4 sn'de yine 0'a iner.
  - Kollar inmez: hemen ardından `TakeToBack` gelir.
  - `ResetTo` (`OnReset`) menteşeyi kapatır ve duruşu sıfırlar.
- [ ] **Adım 4: `StationTraces.EceBed`.** `StationProps.Beds` küçük yatağın merkezini döndürür, `Build` bunu saklar. Kıvılcım'ın başını çevireceği nokta budur.
- [ ] **Adım 5: `StationScenes` iskeleti.**
  - İçinde alanlar, `Phase` ve `Restore` bulunur. `Restore` şunları yapar: `robot.gameObject.SetActive(true)`, `hud.SetCardVisible(true)`, `hud.ClearBlink()`, `hud.HideSkip()`, `shot.Release()`, `Phase = ""`.
  - Sonraki görevler kendi geri almalarını `Restore`'a ekler.
  - Yorum satırı: `// Bolge 8 sahneleri: 76 cizim, 78 aciklama ani, 80 kapanis`.
- [ ] **Adım 6: `TakeDrawing` (76, ~6 sn; K3).**
  1. `hud.StopEditing()`; `Phase = "bakis"`.
     - Robot kutlamadan sonra kameraya dönüktür; şimdi çizime döner (`TurnTo`; çizim ayağının dibinde, ön-sağda).
     - `shot.Begin()` → `hud.SetCardVisible(false)` → `shot.MoveTo(...)` 0,9 sn. Kamera ön-yan-yukarıdadır; tek kadrajda Kıvılcım, elindeki kâğıt ve arkada yataklar görünür (`DrawingShotOffset` ve yarı genişlik görüntüde ayarlanır).
  2. `Phase = "alis"`: Robot eğilir ve alır. Bu, `TakeToBack`'in ilk yarısıdır; ayrılması gerekirse `PickUp(item)` ve `StowOnBack(item, slot)` diye ikiye bölünür, `TakeToBack` ikisini sırayla çağırır (tekrar yok).
  3. `Phase = "acma"`: `robot.TryUnfold(paper, 0.3f, 0.6f)`, yalnız açılma ve durma kısmı.
  4. `Phase = "yatak"`: Kâğıt hâlâ yarı açıkken robot Ece'nin yatağına döner (`TurnTo`, 0,6 sn; sırtı kameraya gelir), 0,6 sn bakar; kâğıt kapanır.
  5. `Phase = "sirt"`: `StowOnBack(paper, BackSlot.FoldedDrawing)`. Sırt kameraya dönük olduğu için kâğıdın kayışın altına girişi görünür. 0,8 sn beklenir.
  6. `hud.SetCardVisible(true)` → 0,2 sn → `shot.Return(1f)`; `Phase = "son"`.
- [ ] **Adım 7: `Oyun.cs`.**
  - Alan `StationScenes stationScenes;` eklenir ve `riverScenes`'in yanında kurulur.
  - Kutlamadan sonraki satırlara bir satır eklenir: `if (level.Number == StationTraces.FoldedLevel) yield return stationScenes.TakeDrawing(level, noteEnd);`.
  - `riverScenes.Restore()` çağrılan her yere `stationScenes.Restore()` de eklenir.
  - `robot.SetFoldedDrawing(level.Number > FoldedLevel)` satırı "Tekrar" yolunda zaten çalışıyor (satır 818); yeni satır gerekmez.
- [ ] **Adım 8: Denetim (`DrawingTakeCheck`, `-bolge8` turuna).**
  - 76'nın **üç not sürümünün her biri** için: notu seç, çözümü çalıştır, `son`'a varılmasını bekle (40 sn sınır).
  - İlk sürümden kareler: `bakis`(+0,4) `s76-1-bakis`, `acma`(+0,5) `s76-2-acma`, `yatak`(+0,4) `s76-3-yatak`, `sirt`(+0,3) `s76-4-sirt`, `son`(+0,5) `s76-5-son`.
  - Her sürümde ölçülenler:
    - Sonda yerdeki çizim gizli, sırttaki görünür.
    - Kâğıt elde iken robotun gövde sınırlarının içine girmiyor.
    - `yatak` anında robotun ileri yönü `EceBed`'e ±25° içinde.
  - **Geri alma:** `acma`'da `ResetLevel()` → Review Focus 1 (76 maddeleri) + menteşe kapalı.
  - 3a'nın 76 denetimi (`NoteCheck`) çizim artık alındığı için güncellenir: "son karede yerde" yerine "sahne öncesinde yerde, sonra sırtta".
- [ ] **Adım 9: Paket + görüntü + kendi kontrolün.**
  - `-sahneler -bolge8` ve `-sahneler -bolge7` (68 değişti) çalıştırılır.
  - Bakılacaklar:
    - Kâğıt elin içinde duruyor, havada değil.
    - Açılan köşe telefonda seçiliyor, içi görünmüyor.
    - Yatağa dönüş "Ece'nin yatağı" diye okunuyor (küçük, renkli battaniye kadrajda).
    - Kol omzun üstünden geçerken gövdenin içinden geçmiyor.
    - Sırttaki kâğıt 3a'daki yerinde.
    - 68 kareleri öncekiyle aynı.
- [ ] **Adım 10: Ragıp'a göster.** `s76-1..5`. **Onay gelmeden Görev 2'ye geçme.**

---

### Görev 2: Bölüm 78 — açıklama anı (~1,5 sa)

**Dosyalar:**
- Değişir:
  - `oyun/Assets/Scripts/StationScreens.cs`: `HoldHighlight(bool on)`, turuncu kalır ya da bırakır.
  - `oyun/Assets/Scripts/Robot.cs`, `SparkBot.cs`: `Realize`.
  - `oyun/Assets/Scripts/Sound.cs`: `Quiet()`.
  - `oyun/Assets/Scripts/StationScenes.cs`: `RevealRow`, `Reveal`, `QuietEnding`.
  - `oyun/Assets/Scripts/Oyun.cs`: ekran akışında bir satır; kutlama satırlarında 78 için koşul.
  - `oyun/Assets/Scripts/Oyun.IstasyonDenetimi.cs`: `RevealCheck`.

**Arayüzler:**

```csharp
// StationScreens.cs — 78: True satirinda terminal turuncu kalir (on) ya da bosta haline doner (off). Highlight sayaci bir artar.
public void HoldHighlight(bool on);

// Robot.cs — 78: bir an donar (bosta suzulme durur, gozler kisilir), basini gogsundeki cizime eger, sonra yavasca ekrana kaldirir.
public virtual IEnumerator Realize() { yield break; }

// Sound.cs — sessiz bitis (78): alcak, yumusak iki notali tini
public static void Quiet() => Play(Get("quiet", () => Chime(new[] { 392f, 330f }, 0.22f)), 0.4f);

// StationScenes.cs
public static int RevealRow(Level level, RunReport report);  // 78 ve cozum tam dogruysa BKM-7 satirinin sirasi, yoksa -1
public static bool QuietEnding(int levelNumber);              // 78: sevinc dansi ve kutlama sesi yok
public IEnumerator Reveal(int row);
```

- [ ] **Adım 1: `StationScreens.HoldHighlight`.** `on` çağrısında var olan `Highlight` kırpışması ~0,4 sn'de turuncuya çıkar ve orada kalır; `off` çağrısında 0,7 sn'de kehribar boştaki parlaklığına iner. `Restore` her zaman boşta hâline döndürür.
- [ ] **Adım 2: `SparkBot.Realize`.**
  - `posing = true`; boşta süzülme ve kol sallanması durur.
  - 0,3 sn: gözler `EyeCalm`'ın sönük hâline iner (~%60).
  - 0,6 sn sonra baş göğse eğilir (`nod` ~+38°) ve 1,2 sn orada kalır.
  - 0,8 sn'de baş yine ekrana kalkar, gözler eski parlaklığına döner; `posing = false`.
  - Anten ışığı söner ve sonunda yine yanar. Görüntüde fazla gelirse bu kısım çıkarılır.
- [ ] **Adım 3: `Reveal` (~6 sn; K2).**
  1. `hud.StopEditing()`; `Phase = "durus"`; `StationTraces.Screens.HoldHighlight(true)`.
  2. `shot.Begin()` (kod kartı zaten ekran kutusunun altında kalır; kart gizlenmez, çünkü ekran kutusu satırı gösteriyor).
  3. `shot.MoveTo(...)` 2,2 sn yavaş yaklaşır: kamera Kıvılcım'ın yan profilinden, göğüs hizasının biraz üstünden bakar. Terminalin turuncu ışığı Kıvılcım'ın yüzüne vurur, baş eğilişi profilden okunur. Kıvılcım ve terminal ekranın **üst yarısında** durur; gerekirse `CameraShot` `Shift` ile kaydırılır, ekran kutusunun üstünde kalmalı.
  4. `Phase = "anlama"`: `robot.Realize()`.
  5. `Phase = "devam"`: 0,4 sn beklenir → `shot.Return(1.2f)`; terminal turuncu kalır (bölüm sonuna kadar); `Phase = "son"`.
- [ ] **Adım 4: `Oyun.cs` (en fazla 4 satır).**
  - Ekran akışı döngüsünden önce: `int reveal = StationScenes.RevealRow(level, report);`.
  - Döngüde `AddScreenAnswer` ve `Answer`'dan sonra: `if (i == reveal) yield return stationScenes.Reveal(i);`.
  - `StationScreens.Answer` bu satırda `Highlight` yerine hiçbir şey yapmaz; turunculuğu `Reveal` verir. Bunun için `Answer`'a `reveal` bilgisi gider ya da `Answer` çağrısı o satırda atlanır. Hangisi daha sade ise o seçilir.
  - Kutlama satırları: `Sound.Celebrate()` ve `robot.Celebrate()` yerine `QuietEnding` ise `Sound.Quiet()` çalar, robot ekrana dönük kalır. XP, damga, kart ve "Sonraki bölüm" **aynen** çalışır.
- [ ] **Adım 5: Denetim (`RevealCheck`).**
  - 78'i yükle, çözümü çalıştır. Kareler: `durus`(+0,6) `s78-1-durus`, `anlama`(+1,0) `s78-2-anlama`, `son`(+0,5) `s78-3-son`.
  - Ölçülenler:
    - Akış BKM-7 satırında durdu: `anlama` anında ekran kutusunda 25 satır var, KOR yok.
    - `son`'da 26 satır var, `Highlights == 1`, robot kutlama yapmadı (yaw kameraya dönmedi), bitiş damgası görünür.
  - **Yanlış kod:** Her satıra `True` döndüren kod yazılır → `Phase` hiç "durus" olmaz, terminal kırpışır, bölüm "EKRAN" hatasıyla biter (3a davranışı).
  - **Geri alma:** `anlama`'da `ResetLevel()` → Review Focus 1 (78 maddeleri) + robot gözleri normal parlaklıkta, `posing` kapalı (robot boşta süzülüyor).
  - **Klavye açık bitiş:** Çalıştırmadan 0,5 sn sonra `hud.FocusCode(0, 0)` çağrılır; kamera sıçramadan başlamalı.
  - 77 bir kez çalıştırılır: sahne oynamaz, kutlama var (yanlışlıkla 77'ye sızmadı).
- [ ] **Adım 6: Paket + görüntü + kendi kontrolün.**
  - Bakılacaklar:
    - Kıvılcım'ın profili ve terminal ekran kutusunun üstünde.
    - Turuncu ışık yüzde seçiliyor.
    - Baş eğilişi "göğsüne bakıyor" diye okunuyor (göğüsteki çizim profilden görünmüyorsa kamera biraz öne döner).
    - Bitiş sesi yumuşak (bir kez `calistir.bat` ile dinle).
    - Ritim: 6 sn uzun gelmiyor.
- [ ] **Adım 7: Ragıp'a göster.** `s78-1..3` (istenirse oyunda izletilir). **Onay gelmeden Görev 3'e geçme.**

---

### Görev 3: Bölüm 80 kapanışı — harita, koridor hatırası, baş kalkar + atlama (~2,5 sa)

**Dosyalar:**
- Yeni:
  - `oyun/Assets/Scripts/CorridorSet.cs`: 75'in koridorunu sahne için kurar ve kaldırır.
  - `oyun/Assets/Scripts/SceneSkip.cs`: "ilk izleyişten sonra atlanır" ortak mantığı (70 ve 80).
- Değişir:
  - `oyun/Assets/Scripts/StationTraces.cs`: `ShowCorridor` ve `HideCorridor`; `MapArrowsEnd`.
  - `oyun/Assets/Scripts/StationMap.cs`: son okun ucunun yeri (duvar yüzünün yerel konumu) döner.
  - `oyun/Assets/Scripts/ScreenView.cs` ve `Hud.cs`: `ShowRecall(string line)`.
  - `oyun/Assets/Scripts/Robot.cs` ve `SparkBot.cs`: `RaiseHeadTo`.
  - `oyun/Assets/Scripts/RiverBedScenes.cs`: `FinaleWithSkip` `SceneSkip`'i kullanır; davranış aynı.
  - `oyun/Assets/Scripts/StationScenes.cs`: `Closing`, `ClosingWithSkip`, `FinishClosing`.
  - `oyun/Assets/Scripts/Oyun.cs`: 80 çağrı satırı.
  - `oyun/Assets/Scripts/Oyun.IstasyonDenetimi.cs`: `ClosingCheck`.

**Arayüzler:**

```csharp
// Atlanabilir sahne (K4): 1 sn sonra altta kucuk "Atla" dugmesi, ustunde kalan saniye. Atlanirsa sahne durur, finish son hali kurar.
// 3 sn'den kisa sahnede dugme yok. Butun bolgelerin sahneleri kullanir (Gorev 5'te eskiler de). Eskiden RiverBedScenes'in icindeydi.
public class SceneSkip
{
    public SceneSkip(MonoBehaviour host, Hud hud);
    public IEnumerator Run(IEnumerator scene, float length, Action finish);
    public void Skip();                                         // atlama katmani (ve denetim) cagirir
    public void Stop();                                         // Restore: kosan sahneyi durdurur
}

// Bolum 80 kapanisinin hatira kesmesi: 75'in koridoru (ayni tohum, 5 cizim) ayni yerde kurulur; 80'in odasi ve alan esyalari gizlenir.
public class CorridorSet
{
    public static CorridorSet Show(Transform levelRoot, Vector2 areaHalf, Func<float, float, float> ground);
    public Vector3 LastDrawing { get; }   // son cizimin (robot ve kiz) ortasi, alanin yerel konumu
    public void Hide();                   // koridor silinir, gizlenenler eski haline (Restore da cagirir)
}

// Hud / ScreenView — 80: terminal kutusunda tek satir, 78'in bulunan satiri gorunusunde (hatira); HideScreen kapatir
public void ShowRecall(string line);

// Robot.cs — basini (gerekirse govdesini de) dunyadaki bir noktaya kaldirir ve orada tutar; ResetTo duz haline dondurur
public virtual IEnumerator RaiseHeadTo(Vector3 worldPoint, float seconds) { yield break; }

// StationTraces.cs
public static Vector3? MapArrowsEnd { get; }   // 80: haritadaki son okun ucu (tuz golunun otesi), dunya konumu
public const string KapiLine = "BKM-7   — dışarıda — görev: KAPI";   // denetim bolum-78.json ile karsilastirir
```

- [ ] **Adım 1: Keşif (10 dk, kod yazmadan).**
  - `levelRoot` altında neler var: oda, eşyalar, veri kartları, `ArrivalMark`. Zemin ve ızgara orada mı, değil mi? Robot `levelRoot`'un dışında mı?
  - `CorridorSet` buna göre "gizlenecekler" listesini seçer. Zemin global ise dokunulmaz.
  - İç mekânda `ground` düz mü (`_FlatFloor`)? Koridorun eşyaları zemine basmalı.
- [ ] **Adım 2: `SceneSkip`.**
  - `RiverBedScenes.FinaleWithSkip`, `SkipFinale`, `PlayFinale` buraya taşınır (`final_70` anahtarı aynı kalır).
  - `RiverBedScenes` ince bir sarmalayıcı olur: `FinaleWithSkip(level) => skip.Run(Finale(level), FinishFinale)`.
  - Atlama denetimi (`FinaleShots70`'in atlama kısmı) değişmeden geçmeli.
- [ ] **Adım 3: `CorridorSet`.**
  - Önce `levelRoot` altında gizlenecekler kapatılır (hangileri açıktı, saklanır).
  - Sonra kardeş bir kök altında `StationRoom.Build(..., lightsOn: true, backClutter: true, 875, ground)` ve `WallDrawings.Paint(room.BackWallFace, 5, 873)` kurulur. Bunlar 75'in `StationTraces.Build` satırlarıyla **aynı değerler**: değerler `StationTraces`'te ortak sabit/yardımcıya alınır, tekrar yazılmaz.
  - Işık 74-80'de aynı (aydınlık); `RegionLook` değişmez.
  - `Hide()` kökü siler, gizlenenleri açar.
  - Yerleşimi `StationTraces.ShowCorridor/HideCorridor` seçer (3a kuralı: yerleşimi yalnız `StationTraces` seçer).
- [ ] **Adım 4: `ShowRecall`.**
  - `ScreenView`'de başlık `İSTASYON EKRANI` olur; liste temizlenir ve tek satır eklenir. Satır `MakeRow`'un "bulundu" görünüşündedir (kehribar zemin, koyu yazı) ama "→ True ✓" kısmı yoktur. Bunun için satır görünüşü küçük bir yardımcıya ayrılır, `MakeRow` da onu kullanır.
  - Kutu 0,3 sn'de belirir (opaklık).
- [ ] **Adım 5: `SparkBot.RaiseHeadTo`.**
  - Başın noktaya olan yükseklik açısı `nod`'a çevrilir (−45° ile sınırlı). Gerekirse gövde de hafif geriye yaslanır (`lean` eksi, en fazla −8°).
  - Bölge 5'teki `LookUp` gibi kollar iner, gözler sakindir; orada kalır.
  - `LookUp` bunu sabit açıyla çağıracak şekilde sadeleşebilir. Ancak Bölge 5 kapanışı (`-bolum 50 -final`) bozulmasın diye **`LookUp`'a dokunulmaz**.
- [ ] **Adım 6: `Closing` (80, ~14 sn; K1).**
  1. `hud.StopEditing()`; `Phase = "harita"`.
     - Robot haritaya döner (`TurnTo(0)`).
     - `shot.Begin()` → kart gizlenir → `shot.MoveTo(...)` 1,5 sn: alçak, Kıvılcım'ın arkasından; harita ve Kıvılcım aynı karede (`MapShot`).
     - 1,2 sn beklenir.
  2. `Phase = "gecis"`: `hud.Blink(0.3f)` → en karanlık anda (0,15 sn):
     - `corridor = StationTraces.ShowCorridor(...)`.
     - Robot `ResetTo(üst sıradaki durma yeri, 0f)`; sırttaki eşyalar kalır: `ResetTo` çanta eşyalarını kaldırıyor mu bakılır, kaldırıyorsa sahneye özel `PlaceAt(pos, yaw)` kullanılır.
     - `shot.Cut(...)`: alçak, yan-arka; son çizim kadrajın üst-ortasında büyük, Kıvılcım'ın başı ve sırtı önde.
  3. `Phase = "cizim"`: kamera 2,5 sn'de çok az yaklaşır (`MoveTo`, küçük adım); 1,5 sn çizim yalnız kalır.
  4. `Phase = "satir"`: `hud.ShowRecall(StationTraces.KapiLine)`; 2,5 sn beklenir. Kutu alt yarıda olur, çizim ve Kıvılcım üstte kalır.
  5. `Phase = "donus"`: `hud.HideScreen()`; `hud.Blink(0.3f)` → en karanlık anda:
     - `corridor.Hide()`.
     - Robot 80'deki bitiş karesine haritaya dönük konur.
     - `shot.Cut(MapShot)`.
  6. `Phase = "bas"`:
     - `robot.RaiseHeadTo(MapArrowsEnd, 1.2f)`.
     - Aynı anda kamera 2,5 sn'de biraz yükselip geri çekilir ve son kadraja varır: üstte haritanın okları, altta arkadan Kıvılcım.
  7. `hud.SetCardVisible(true)`; `Phase = "son"`. Kamera bırakılmaz; "Tekrar" ya da bölüm değişimi `Restore` ile bırakır.
- [ ] **Adım 7: `ClosingWithSkip` + `FinishClosing`.**
  - `skip.Run(Closing(level), ClosingLength, FinishClosing)`; ilk izleyişte de atlanır (K4). 76 (`FinishTakeDrawing`: çizim sırtta, menteşe kapalı, kamera geri) ve 78 (`FinishReveal`: kalan satırlar düşer, terminal turuncu) da aynı yolla atlanır; bu iki "son hâl" yöntemi Görev 1-2'de yazılır, düğme bu adımda bağlanır.
  - `FinishClosing` şunları yapar: koridor varsa `Hide`, ekran kutusu kapalı, robot bitiş karesinde haritaya dönük ve başı kalkık (`RaiseHeadTo(..., 0f)`), kamera son kadrajda, kart görünür, `Phase = "son"`.
  - `Restore`'a eklenir: `skip.Stop()`, `corridor?.Hide()`, `hud.HideScreen()` (80'de ekran bulmacası yok; güvenli).
- [ ] **Adım 8: `Oyun.cs`.** Kutlama satırlarına bir satır: `if (level.Number == StationTraces.MapLevel) yield return stationScenes.ClosingWithSkip(level);`.
- [ ] **Adım 9: Denetim (`ClosingCheck`).**
  - **Üç not sürümünün hepsinden** `son`'a varılmalı (60 sn sınır).
  - İlk sürümden kareler: `harita`(+1) `s80-1-harita`, `cizim`(+1) `s80-2-cizim`, `satir`(+1,2) `s80-3-satir`, `bas`(+1,5) `s80-4-bas`, `son`(+0,5) `s80-5-son`.
  - Ölçülenler:
    - `cizim` anında: koridorda 5 çizim var, 80'in haritası gizli, robot hiçbir eşyanın sınırına girmiyor, sırtta üç eşya var.
    - `satir` anında: kutudaki metin `bolum-78.json`'daki BKM-7 satırıyla harfi harfine aynı.
    - `son` anında: koridor yok, harita görünür, robotun başı kalkık (`nod` < −15°), kart görünür.
  - **Atlama:** İlk izleyişte düğme 1 sn sonra görünür, üstündeki sayı azalır. `cizim`'de düğmeye dokunulur → `Phase == "son"`, koridor yok, kutu kapalı, düğme kapalı. Aynısı 76'da (`acma`'da → çizim sırtta) ve 78'de (`anlama`'da → 26 satır, sessiz bitiş) denenir.
  - **Geri alma:** `satir`'da `ResetLevel()` → Review Focus 1 (80 maddeleri). Aynı denetim `cizim`'de bir kez daha yapılır (koridor açıkken).
  - **Hafif mod:** `-performans ac` ile `s80-2-cizim` ve `s80-5-son` alınır.
- [ ] **Adım 10: Paket + görüntü + kendi kontrolün.**
  - `-sahneler -bolge8`, `-sahneler -bolge7` (SceneSkip, 70) ve hafif mod çalıştırılır.
  - Bakılacaklar:
    - Koridor 75'in kareleriyle aynı görünüyor (`docs/tasarim/bolge-8-gorunus-2026-11-14/` içindeki 75 karesi).
    - Son çizim ile hatıra satırı aynı anda okunuyor.
    - İki kırpışma "hatıraya gidip dönmek" gibi okunuyor, kopukluk yok.
    - Son karede oklar Kıvılcım'ın başının üstünde ve damga okunuyor.
    - Bölge 7'nin 70 atlaması aynı çalışıyor.
  - `calistir.bat` ile bir kez baştan sona oyunda izlenir (ritim ~14 sn).
- [ ] **Adım 11: Ragıp'a göster.** `s80-1..5` + 80'in tamamı oyunda (istenirse ekran kaydı: `docs/tasarim/taslaklar.md` kuralı). **Onay gelmeden Görev 4'e geçme.**

---

### Görev 4: 3b son denetimi, belgeler (~45 dk)

**Dosyalar:**
- Değişir:
  - `docs/tasarim/senaryo-bolge-08.md`: "Oyuna konanlar: 3b sahneler"; dosyalar ve K1-K3'ün sonucu.
  - `docs/tasarim/hikaye-kitabi.md`: Kıvılcım katlanmış çizimi açmaya yeltendi, açmadı; 80'de koridor hatırası.
  - `docs/tasarim/telefon-denemesi.md`: 76, 78 ve 80 sahneleri için `KARE:` satırları. 78'in yumuşak bitiş sesi ve hatıra satırının okunurluğu telefonda denenir.
  - `CLAUDE.md`: Çalıştırma bölümündeki `-bolge8` satırına 3b sahneleri (76 çizim alınışı, 78 açıklama anı, 80 kapanış ve atlaması).
  - `ILERLEME.md`.
- Yeni klasör: `docs/tasarim/bolge-8-sahneler-<tarih>/` (son görüntüler).

- [ ] **Adım 1:** `cd motor-test && dotnet test` → hepsi geçmeli.
- [ ] **Adım 2:** Windows paketi hatasız üretilmeli; ardından şu denetimler çalıştırılır:
  - `-sahneler -bolge8` → TAMAM.
  - `-sahneler -bolge7` → TAMAM.
  - `-sahneler -bolge8 -performans ac` → TAMAM.
  - `-bolum 71` turu (bu turda `Oyun.cs`'in akışı ve kutlama değişti): 71-80 "Tamamlandı" olmalı; 76, 78 ve 80'in `3-bitti` kareleri sahnenin son karesi olmalı; diğerleri 3a görüntüleriyle aynı (`b71-80-bitisler`); arayüz denetimleri TAMAM (pencere ön planda).
  - Klavye denetimi → TAMAM.
  - Tam `-sahneler` (Bölge 6+7) ve `-bolum 50 -final` **çalıştırılmaz**: `CameraShot`, `SceneWalker` ve `LookUp` değişmedi. Değiştiyse çalıştırılır.
- [ ] **Adım 3:** Klavye açıkken 78 ve 80 bitirişi (78 denetimde var; 80 bir kez elle): sahne kamerası sıçramadan başlamalı.
- [ ] **Adım 4: Belgeler.**
  - Yukarıdaki dosyalar güncellenir.
  - Görüntüler `docs/tasarim/bolge-8-sahneler-<tarih>/`'e konur: `s76-*`, `s78-*`, `s80-*`, hafif mod `s80-2`/`s80-5`. Ara denemeler silinir.
  - `ILERLEME.md`'ye Ragıp adıyla yazılır: ne yapıldı; denenmeyenler (telefonda sahne akıcılığı, yumuşak ses düzeyi, kırpışmalarda takılma); açık sorular; sıradaki iş: **Bölge 9 (tuz gölü) planı**, Opus 5.5 · high, yeni sohbet.
- [ ] **Adım 5: Ragıp'a son tur.** Üç sahnenin anahtar kareleri yan yana gösterilir (76 açma + sırt, 78 anlama, 80 çizim + satır + son). Onay gelince Bölge 8 görünüşü (3a + 3b) biter.

---

### Görev 5: Eski sahneler de "Atla" düğmesine (~1 sa; Görev 4'ten önce ya da sonra, ayrı oturum olabilir)

**Dosyalar:** `CanyonScenes.cs` (51 roket inişi, 55 ilk uçuş, 60 kanyona iniş), `RiverBedScenes.cs` (63, 68, 70), `Oyun.cs` (Bölge 1-5 kapanışları: `ClosingSceneBolge1..5`), `Hud.cs` (eski tam ekran dokunma katmanı `SkipCatcher` kalkar), `Oyun.SahneDenetimi.cs`.

- [ ] **Adım 1:** Her sahne için uzunluk (`Length`) ve "son hâl" yöntemi (`Finish…`) yazılır; yoksa eklenir (70'te `FinishFinale` var). 3 sn'den kısa olanlar düğmesiz kalır.
- [ ] **Adım 2:** Hepsi `SceneSkip.Run` ile çağrılır; `final_70` anahtarı ve "ilk izleyiş atlanamaz" kuralı kalkar.
- [ ] **Adım 3: Denetim:** tam `-sahneler` (Bölge 6+7+8) ve `-bolum 50 -final` (ortak sahne kodu değiştiği için gerekli, ~25 dk). Her sahnede ortada düğmeye dokunulur → son hâl doğru (68: kalem sırtta; 70: turkuaz gözler; 60: kamera son kadrajda).
- [ ] **Adım 4: Ragıp'a göster:** düğmenin yeri ve boyu (telefon ekranında kod kartına, "Sonraki bölüm"e değmiyor).

---

## Ragıp'a açık sorular (plan kararları önerildiği gibi verildi; görüntüde ya da sonra değişebilir)

1. **80'de sevinç dansı kalsın mı?** Şimdilik kalıyor (70 gibi: önce sevinç, sonra duygusal sahne). 78 gibi sessiz bitmesi de seçilebilir: tek satırlık değişiklik (`QuietEnding`'e 80 eklenir).
2. **Hatıra satırının kutusu:** 78'deki terminal kutusunun aynısı (başlık `İSTASYON EKRANI`). Başlıksız, daha "hatıra gibi" soluk bir kart da olabilir.
3. ~~78'in süresi~~ → K4 ile çözüldü: 78 de "Atla" düğmesiyle atlanır.
4. **76'da açılan köşe:** İçi görünmüyor, yalnız turuncu çizgi ucu biraz uzuyor. Daha fazla ipucu (renkli bir leke, Ece'nin turkuazı değil) istenirse Perde 4 finaliyle tutarlı olmalı: içerik orada karar verilecek.

## Kapsam dışı

- Bölge 9-10 görünüşü ve planı.
- Ses: bilgisayar uğultusu, lamba kırpışma sesi.
- Bölge 2 bayrağına arma (3a açık sorusu).
- Bilgi kartının 73-77/80'e eklenmesi (Enes dönünce).
- Telefon testleri (ILERLEME.md listesi).
- `TestAllLevelsOpen`/`TestSolutionsFilled` (Play Store öncesi).
- Katlanmış çizimin içeriği (Perde 4).
- Kıvılcım'ın gözlerinin kalıcı rengi (Bölge 7 açık sorusu).
