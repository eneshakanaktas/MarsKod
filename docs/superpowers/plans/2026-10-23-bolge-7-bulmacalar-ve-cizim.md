# Bölge 7 · 1. parça: bulmacalar ve çizim düzeneği — Yapım Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (Enes'in kararı: yardımcı/subagent yok, Claude işi kendisi yapar). Adımlar `- [ ]` ile işaretlenir.

**Goal:** Kıvılcım'ın çizimi (kalem, boyanacak kareler), parametre şartı ve Bölüm 61-70 (+ sınavlar, sözlük) çalışır ve `dotnet test` ile denetlenir halde olsun.

**Architecture:** Çizim kuralı ayrı bir saf C# sınıfında (`PaintBoard`); `World` onu kullanır ve olay üretir (dünya kuralları arayüze bağımlı olmaz). Parametre şartı var olan `RoutineRule`'ün genişlemesi: `ProgramRun`, rutin çağrılarının argümanlarını kaydeder. Bölüm dosyası iki yeni alan ve iki yeni harita işareti kazanır. Boyanmış karelerin **görünüşü bu parçada yoktur** (3. parça); bu parçanın sonunda 61-70 oyunda açılır ama boya görünmez.

**Tech Stack:** C# 9 / .NET Standard 2.1 (Unity 6 ile aynı sınırlar), xUnit (`motor-test`), JSON bölüm dosyaları.

**Spec:** `docs/superpowers/specs/2026-10-23-bolge-7-design.md` (§1; §2, §3 sonraki parçalar)

## Global Constraints

- `oyun/Assets/Dunya/` saf C#: `UnityEngine` kullanılmaz; C# 9, .NET Standard 2.1 dışı kitaplık yok (`motor-test` derlerken yakalar).
- Kod kalitesi (CLAUDE.md): sınıf/fonksiyon tek iş; tekrar yok; `World` şişirilmez (çizim `PaintBoard`'da); gereksiz katman yok.
- Komut adları: `pen_down`, `pen_up` (İngilizce, `move`/`collect` gibi). Harita işaretleri: `O` boyanacak kare, `Q` mavi boya kalemi (`X` ve `Y` testlerde "bilinmeyen işaret" olarak kullanılıyor, dokunma).
- Alan 6×6 kalır (`BolumTests` zorunlu kılıyor).
- Parametre şartı: rutin **en az bir parametreyle** tanımlı ve **en az iki farklı argüman değeriyle** çağrılmış olmalı; sayma kodun gerçek yürütülmesinden gelir.
- Çizimli görev tamam: boyanacak karelerin hepsi boyanmış **ve** fazladan hiçbir kare boyanmamış; diğer şartlar eskisi gibi. Kalem başta kalkık; `pen_down()` bulunduğu kareyi boyar; boyanmış kareden yeniden geçmek "fazla" değildir; iki kez aynı komut hata değildir.
- Her bölümde `tipik_hatalar` içinde parametresiz çözüm; `dotnet test` her bölümü denetler (çözüm görevi bitirir, hatalar bitirmez).
- Mesajlar sade Türkçe, jargon yok; hedef kitle 13+ (çocuksu değil).
- Her görev sonunda commit; ILERLEME.md'ye yalnızca kapanışta yazılır (ekip döngüsü).

## Review Focus

1. Kod kural yüzünden (kaya, sınır) ya da Python hatasıyla çizimin ortasında durursa görev tamam sayılmaz (boya tam görünse bile).
2. `def ok(boy=3)` ile `ok()` iki kez çağrılırsa argüman değeri aynıdır ("hep aynı sayıyla"); `ok(boy)` değişkenle ve döngü değişkeniyle (`for i in range(...)`) çağrılırsa gerçek değerler sayılır.
3. Parametresiz `def ok():` tanımı parametre şartında "bir sayı almıyor" mesajı verir (tanımsız mesajından ayrı).
4. Kalem hiç inmeden biten kod "Kalem hiç inmedi" der; eksik ve fazla mesajları kare sayısını doğru yazar.
5. Çizim komutları çizimsiz bölümde açık değildir (mevcut "bu komut henüz açılmadı" kuralı); çizimsiz eski bölümler ve eski testler değişmez.

---

## File Structure

| Dosya | Sorumluluk |
|---|---|
| `oyun/Assets/Dunya/PaintBoard.cs` (yeni) | Kalem durumu, boyanacak ve boyanmış kareler, eksik/fazla sayımı, oyuncuya mesaj. Saf kural. |
| `oyun/Assets/Dunya/World.cs` | `PaintBoard`'u kullanır: `pen_down`/`pen_up` komutları, `Move` sonrası boyama, olaylar (`Painted`, `PenChanged`), `Complete`'e boya şartı. |
| `oyun/Assets/Dunya/RoutineRule.cs` | Parametre şartı (`ParametricNames`); `Check` argüman kayıtlarını alır. |
| `oyun/Assets/Dunya/ProgramRun.cs` | Rutinlerin parametre sayısı ve çağrı argümanlarını `RunReport`'a yazar; boya sorununu rapora ekler. |
| `oyun/Assets/Dunya/Level.cs` | `O`/`Q` işaretleri, `Paint` listesi, `parametreli_rutinler` alanı, çizim komutu denetimi. |
| `oyun/Assets/Dunya/Region.cs`, `Collectible.cs` | `Region.Riverbed` (61-70), `Collectible.BluePencil`. |
| `oyun/Assets/Scripts/RegionLook.cs`, `Obstacles.cs`, `Oyun.cs` | Yalnızca derleme/davranış uyumu: Riverbed şimdilik Canyon görünüşünde; boya sorunu mesajı gösterilir. |
| `oyun/Assets/Resources/Bolumler/bolum-61..70.json`, `Sinavlar/sinav-65/70.json`, `Sozluk/sozluk.json` | İçerik. |
| `docs/tasarim/bolum-dosyasi.md` | Kılavuz güncellenir. |
| `motor-test/PaintTests.cs`, `ParametricRoutineTests.cs`, `RiverbedTests.cs` (yeni) | Testler. |

---

### Task 1: PaintBoard (çizim kuralı, saf mantık)

**Files:**
- Create: `oyun/Assets/Dunya/PaintBoard.cs` (+ Unity `.meta` dosyası: aynı klasördeki bir dosyanın `.meta`'sı örnek alınarak yeni GUID ile)
- Test: `motor-test/PaintTests.cs` (`motor-test/Dunya/Dunya.csproj` `oyun/Assets/Dunya/**/*.cs`'i joker ile alır: yeni dosya için proje değişikliği gerekmez; uyarılar hata sayılır, derleme uyarısız olmalı)

**Interfaces:**
- Produces:
  - `public sealed class PaintProblem { public string Title; public string Text; }`
  - `public sealed class PaintBoard`
    - `public PaintBoard(IEnumerable<Cell> target)`
    - `public bool Active` (boyanacak kare var mı)
    - `public bool PenDown { get; }`
    - `public bool Lower(Cell at)` — kalemi indirir, `at` karesini boyar; ilk kez boyandıysa true
    - `public void Raise()`
    - `public bool Enter(Cell at)` — kalem inikse `at`'i boyar; yeni boyandıysa true, kalem kalkıksa false
    - `public bool WantedAt(Cell c)`, `public bool PaintedAt(Cell c)`
    - `public int Missing` (boyanacak ama boyanmamış), `public int Extra` (boyanmış ama boyanacak değil)
    - `public bool Done` (`Missing == 0 && Extra == 0`)
    - `public bool EverLowered { get; }`
    - `public PaintProblem Problem()` — `Done` ise null; yoksa mesaj

- [ ] **Step 1: Failing test'leri yaz** (`PaintTests.cs`): her biri yalnızca `PaintBoard` ile.
  - `Kalem_kalkikken_gezmek_boyamaz`: `Enter(c)` false, `PaintedAt(c)` false.
  - `Kalemi_indirmek_bulundugu_kareyi_boyar`: hedef {(1,0)}; `Lower(new Cell(1,0))` true, `Missing == 0`, `Done`.
  - `Hedef_disi_kare_fazla_sayilir`: `Lower((0,0))` → `Extra == 1`, `Done == false`.
  - `Ayni_kareyi_iki_kez_boyamak_fazla_degildir`: hedef karede `Enter` iki kez; `Extra == 0`, ikinci çağrı false.
  - `Iki_kez_ayni_komut_hata_degildir`: `Lower` iki kez, `Raise` iki kez, özel durum yok.
  - `Mesajlar`: eksik → `Title` "İşaret yarım kaldı", `Text` "3 kare boyanmadı" içerir; fazla → `Text` "Fazladan 2 kare boyadın" ve "pen_up()" içerir; hiç inmediyse (`EverLowered == false`, `Missing > 0`) → `Title` "Kalem hiç inmedi", `Text` "pen_down()" içerir; `Done` iken `Problem()` null. Öncelik: hiç inmediyse "Kalem hiç inmedi", yoksa fazla varsa fazla mesajı, yoksa eksik mesajı.
- [ ] **Step 2:** `cd motor-test && dotnet test --filter PaintTests` → derleme hatası/FAIL ("PaintBoard yok").
- [ ] **Step 3: `PaintBoard` yaz** (imzalar yukarıda). `HashSet<Cell>` ile iki küme tutar; dosya başına diğer Dunya dosyaları gibi Türkçe 2-3 satır açıklama yorumu.
- [ ] **Step 4:** `dotnet test --filter PaintTests` → PASS.
- [ ] **Step 5: Commit** `git add oyun/Assets/Dunya/PaintBoard.cs* motor-test/PaintTests.cs && git commit` ("Bolge 7: PaintBoard (cizim kurali)").

### Task 2: World'e çizim komutları, olaylar ve görev şartı

**Files:**
- Modify: `oyun/Assets/Dunya/World.cs` (olay sınıfları ~satır 100-130; `AllCommands` satır 191; kurucu 196-215; `Complete` 188; `Move` 237-260; `Commands()` 383-461)
- Test: `motor-test/PaintTests.cs` (World ile kısımlar)

**Interfaces:**
- Consumes: `PaintBoard` (Task 1).
- Produces:
  - `public sealed class Painted : WorldEvent { public Cell At; public bool Wanted; }` (Wanted: boyanacak kareydi)
  - `public sealed class PenChanged : WorldEvent { public Cell At; public bool Down; }`
  - `World(..., BigInteger? expectedReport = null, IEnumerable<Cell> paintCells = null)` (son parametre, varsayılan boş: eski çağrılar bozulmaz)
  - `public PaintBoard Paint { get; }`
  - `AllCommands`'a `"pen_down"`, `"pen_up"` eklenir
  - `Complete` artık `&& Paint.Done`
  - `Level.cs` bu görevden sonra `CreateWorld()`'da `paintCells`'i geçirecek (Task 4).

- [ ] **Step 1: Failing test'ler** (`PaintTests.cs`, `World` ile; örnek dünya: 6×6, robot (0,0), hedef `paintCells` {(1,0),(2,0),(3,0)}, komutlar `move`, `pen_down`, `pen_up`):
  - `Kalemle_yurumek_kareleri_boyar_ve_gorevi_bitirir`: kod `move(East)\npen_down()\nmove(East)\nmove(East)\n` → `report.Complete`; (1,0) `pen_down` ile boyanmıştır → kod `move(East)\npen_down()\nmove(East)\nmove(East)` hedefi (1,2,3)'ü tam boyar.
  - `Fazla_kare_boyamak_gorevi_bitirmez`: önce `pen_down()` (başlangıç karesi hedef değil) → `Complete` false, `world.Paint.Extra == 1`.
  - `Yarim_kalan_cizim_bitirmez`: üçünden ikisi → `Complete` false, `Paint.Missing == 1`.
  - `Kural_ile_duran_kod_tamam_sayilmaz`: boyamayı tamamladıktan sonra alan dışına `move` → `report.Rule != null`, `report.Complete` false (Review Focus 1).
  - `Cizim_komutlari_cizimsiz_bolumde_kapali`: `commands = {"move"}` ile `pen_down()` → `report.Rule.Title` "Bu komut henüz açılmadı".
  - `Olaylar_kaydedilir`: `report.Trace` içinde `PenChanged{Down=true}` ve `Painted{Wanted=true}` olayları sırayla bulunur.
  - `Cizimsiz_eski_dunya_degismez`: `paintCells` verilmeden kurulan dünyada `Complete` eskisi gibi (mevcut testlerin tümü geçer).
- [ ] **Step 2:** `dotnet test --filter PaintTests` → FAIL.
- [ ] **Step 3: Uygula.** `pen_down`/`pen_up` komutları diğer komutlar gibi (`Unlocked`, `NoKeywords`, argüman sayısı 0). `LowerPen()`: `Paint.Lower(Robot)`; `PenChanged` + (yeni boyandıysa) `Painted`. `RaisePen()`: `Paint.Raise()`; `PenChanged`. `Move` içinde `Robot = to` satırından sonra `if (Paint.Enter(to)) Events.Add(new Painted…)`. Kod başı yorumundaki komut listesi güncellenir.
- [ ] **Step 4:** `dotnet test` (tüm paket) → PASS (1403 + yeni).
- [ ] **Step 5: Commit** ("Bolge 7: World cizim komutlari ve gorev sarti").

### Task 3: Parametre şartı (`RoutineRule` + `ProgramRun`)

**Files:**
- Modify: `oyun/Assets/Dunya/RoutineRule.cs`, `oyun/Assets/Dunya/ProgramRun.cs` (`RunReport` ~34-58; `CountCall` 107-112; `ReadDefinedRoutines` 115-119; `Execute` 65-104)
- Test: `motor-test/ParametricRoutineTests.cs`

**Interfaces:**
- Produces:
  - `RunReport.RoutineParams : Dictionary<string,int>` — modül düzeyinde tanımlı her rutinin parametre sayısı.
  - `RunReport.RoutineArgs : Dictionary<string,List<string>>` — her çağrının argüman değerleri, `Values.Repr` ile, parametre sırasıyla `|` ile birleştirilmiş tek metin (örn. `"3"`, `"East|2"`); parametresiz rutinde `""`. Çerçevenin ilk adımında (varsayılanlar doldurulmuş hâliyle) okunur; `CountCall` içinde.
  - `RoutineRule(IEnumerable<string> names, int minCount, IEnumerable<string> parametricNames = null)` ve `public readonly IReadOnlyList<string> ParametricNames`; `IsEmpty` bunları da sayar.
  - `RoutineProblem Check(ICollection<string> defined, IReadOnlyDictionary<string,int> runs, IReadOnlyDictionary<string,int> paramCounts = null, IReadOnlyDictionary<string,List<string>> args = null)`. `ParametricNames` aynı zamanda adlı rutin sayılır (tanımsız / hiç çalışmadı / 1 kez mesajları aynen); ardından her biri için sırayla:
    - parametre sayısı 0 → `Title` "Kod bitti, `ad` bir sayı almıyor", `Text` "`def ad(boy):` diye yaz: parantezin içindeki ad, rutine verilen sayıyı taşır."
    - çağrılarında tek bir farklı argüman değeri → `Title` "Kod bitti, `ad` hep aynı sayıyla çağrıldı", `Text` "Farklı boylar için farklı sayı ver: `ad(3)`, `ad(5)`."

- [ ] **Step 1: Failing test'ler** (`ParametricRoutineTests.cs`; dünya `RoutineTests.Row()` gibi, kural `new RoutineRule(new string[0], 0, new[]{"git"})`):
  - `Iki_farkli_sayiyla_cagrilan_parametreli_rutin_sartı_saglar`: `def git(n):\n    for i in range(n):\n        move(East)\n        collect()\ngit(1)\ngit(2)\n` → `Complete`, `RoutineProblem == null`.
  - `Parametresiz_tanim_bir_sayi_almiyor_der`: `def git():` ... iki kez çağrı → `Title` "bir sayı almıyor".
  - `Hep_ayni_sayi_yetmez`: `git(1)` iki kez (üç buz için üçüncü `move/collect` ekle ya da kural testi yalnızca `RoutineProblem` üzerinden) → `Title` "hep aynı sayıyla".
  - `Varsayilan_parametre_hep_ayni_sayilir`: `def git(n=1):`, `git()` iki kez → "hep aynı sayıyla" (Review Focus 2).
  - `Dongu_degiskeni_ile_cagri_gercek_degerleri_sayar`: `for i in range(1, 3):\n    git(i)` → şart sağlanır.
  - `Eski_kurallar_degismez`: `RoutineTests` tamamı geçer (dokunulmaz).
  - `Rapor_parametre_sayisini_ve_argumanlari_yazar`: `report.RoutineParams["git"] == 1`, `report.RoutineArgs["git"]` = `["1","2"]`.
- [ ] **Step 2:** `dotnet test --filter ParametricRoutineTests` → FAIL.
- [ ] **Step 3: Uygula.** `ProgramRun.CountCall` imzasına `report` zaten var: aynı yerde, çerçeve yeniyse `frame.Fn.Def.Params` sırasıyla `frame.Vars[p.Name]` değerlerini `Values.Repr` ile alıp ekle. `ReadDefinedRoutines`: `PyFunction` için `Def.Params.Count`. `Execute`: `routines.Check(...)` çağrısına iki yeni sözlüğü ver.
- [ ] **Step 4:** `dotnet test` (tüm paket) → PASS.
- [ ] **Step 5: Commit** ("Bolge 7: parametre sarti").

### Task 4: Bölüm dosyası: işaretler, alanlar, denetim; bölge ve toplanabilir

**Files:**
- Modify: `oyun/Assets/Dunya/Level.cs` (`known` küme satır 89; `ReadMap` 144-209; `CreateWorld` 75; `ReadRoutines` 252-269; `LevelCheck` 293+; yorum başlığı 5-10), `oyun/Assets/Dunya/Region.cs`, `oyun/Assets/Dunya/Collectible.cs`, `oyun/Assets/Scripts/RegionLook.cs` (`For`, satır ~94), `oyun/Assets/Scripts/Obstacles.cs` (`Create`, satır ~17), `oyun/Assets/Scripts/Oyun.cs` (~858-875 mesaj basamakları), `docs/tasarim/bolum-dosyasi.md`
- Test: `motor-test/RiverbedTests.cs` (+ var olan `BolumTests` içindeki `Her_on_bolum_yeni_bolge`'ye Bölge 7 satırları)

**Interfaces:**
- Produces:
  - `Level.Paint : List<Cell>` (harita `O` işaretleri), `Level.PaintMark = 'O'`.
  - Bölüm JSON alanı `"parametreli_rutinler": ["ok"]` → `RoutineRule.ParametricNames` (ad kuralları `rutinler` ile aynı; Python kelimesi/oyun komutu olamaz).
  - `Collectible.BluePencil`, işaret `'Q'`, adı ("mavi boya kalemi", "mavi boya kalemleri").
  - `Region.Riverbed` (`Order` sonuna), `Regions.Item(Riverbed) == Collectible.BluePencil`; `RegionLook.For(Riverbed)` ve `Obstacles.Create` şimdilik **Canyon** ile aynı dalı kullanır (3. parça kendi görünüşünü verir; yoruma yaz).
  - `Oyun.cs`'te boya sorunu: `world.Complete == false` ve kod durmadıysa ve `!world.Paint.Done` ise `hud.ShowMessage("GÖREV", problem.Title, problem.Text, null, error: false)` (mevcut rutin sorunu basamağının hemen önüne; `Paint.Problem()` kullanılır).

- [ ] **Step 1: Failing test'ler** (`RiverbedTests.cs`, `Mini`-benzeri küçük JSON ile):
  - `O_haritada_boyanacak_kare_olarak_okunur`: `Level.Paint` beklenen hücreler; `CreateWorld().Paint.WantedAt(...)` true.
  - `Cizim_olan_bolumde_kalem_komutlari_acik_olmali`: `O` var ama `komutlar` içinde `pen_down`/`pen_up` yok → `DataFormatError` ("pen_down" ve "pen_up" geçen mesaj).
  - `Cizimsiz_bolumde_kalem_komutu_acilamaz`: `O` yok, `komutlar` içinde `pen_down` var → `DataFormatError`.
  - `Q_mavi_kalemdir`: `Collectibles.Marks['Q'] == Collectible.BluePencil`, `Level.Item` buna eşit, `Collectibles.Name` doğru.
  - `Parametreli_rutinler_okunur_ve_adi_denetlenir`: geçerli ad okunur (`level.Routines.ParametricNames`); `"def"` ya da `"move"` adı `DataFormatError`.
  - `Bolge_7_61_den_70e`: `Regions.Of(61) == Region.Riverbed`, `Regions.Of(70) == Region.Riverbed`, `Regions.Of(71)` hâlâ son bölge (`Riverbed`), `Regions.Item(Riverbed) == BluePencil`.
  - Mevcut `BolumTests.Yanlis_dosya_anlasilir_mesaj_verir` (X bilinmeyen işaret) değişmeden geçer.
- [ ] **Step 2:** `dotnet test --filter RiverbedTests` → FAIL.
- [ ] **Step 3: Uygula.** `ReadMap` switch'ine `PaintMark` dalı (`level.Paint.Add(cell)`; "görev yok" denetimine boya da sayılır: yalnızca `O` olan harita görev sayılır); `MarkList()`/yanlış işaret mesajı `O boyanacak kare`'yi de sayar; `known`'a `parametreli_rutinler`; `ReadRoutines`'ta ad kuralları ortak yardımcıya alınır (iki alan aynı kuralı kullanır, kod tekrarı yok); `ReadMap` sonunda çizim komutu uyumu (yukarıdaki iki hata). `CreateWorld()` son parametre `Paint`. `bolum-dosyasi.md`: `O`, `Q`, `parametreli_rutinler`, `pen_down`/`pen_up` ve çizim şartı anlatılır.
- [ ] **Step 4:** `dotnet test` (tüm paket) → PASS. Unity tarafı derleme riski için Task 7'de paket üretilir.
- [ ] **Step 5: Commit** ("Bolge 7: bolum dosyasi cizim ve parametre alanlari, Riverbed bolgesi").

### Task 5: Bölüm 61-65

**Files:**
- Create: `oyun/Assets/Resources/Bolumler/bolum-61.json` … `bolum-65.json` (+ `.meta`), `oyun/Assets/Resources/Sinavlar/sinav-65.json` (+ `.meta`)
- Modify: `oyun/Assets/Resources/Sozluk/sozluk.json`
- Test: `motor-test/RiverbedTests.cs` (Bölge 7 biçim testi), var olan `BolumTests`/`SozlukTests`/`QuizTests` (kendiliğinden denetler)

**Her bölümde ortak (61-70):** `"etiket": "BÖLGE 7 · NEHİR YATAĞI"`; `giris`/`bitis` kısa kayıt damgası (örn. `"Sol 1.0xx · Nehir yatağı: ..."`, `"İşaret: n/m"`; sayılar `hikaye-kitabi.md` zaman çizelgesine uygun, 2. parçada cilalanır); `gorev` ≤ 22 harf; `komutlar` yalnızca bölümün ihtiyacı (çizimli olanlarda `pen_down`, `pen_up`, `move`; 68'de `collect`); `parametreli_rutinler`; `parcalar` (acemi düğmeleri) `def ok(boy):`, `ok(3)`, `pen_down()`, `pen_up()`, `move(...)`; 3 ipucu (yön / konu / kodun bir kısmı); `tipik_hatalar`: parametresiz çözüm (iki ayrı rutin ya da kopyala-yapıştır), çizimli bölümde fazla boyayan, kalemi hiç indirmeyen; `cozum` doğru ve spec tablosundaki şartları sağlar. Haritalar 6×6; çizilecek şekil `O`'larla yazılır, robot `R`; robot başlangıç karesi hiçbir zaman `O` değildir (tek harf kuralı; oyuncu önce yürür sonra `pen_down()` der).

| Bölüm | Ad (taslak) | Şekil / harita | Parametreli rutin | Başlangıç kodu |
|---|---|---|---|---|
| 61 | İniş pisti | farklı boylarda 3 ayrı yatay çizgi (örn. 2, 3, 4 kare) | `cizgi(boy)` | rutin tanımı hazır, çağrılar oyuncuda |
| 62 | İlk işaret | küçük ok (boy 3) + büyük ok (boy 5), ikisi alanın farklı yerlerinde; ok başı basamaklı | `ok(boy)` | yok |
| 63 | Göç izleri | **çizim yok**; robot izlerin sonundaki `H` hedefe varır, yol kayalı | `yuru(yon, adim)` (iki parametre; `yon` olarak `East` gibi yön geçirmek motorda çalışmalı, çalışmazsa `yuru(adim)`'a in ve ikinci parametreyi 64'e kaydır) | yok |
| 64 | Kareler | kayalar arasında iki kare (boy 2 ve boy 3) | `kare(boy)` içinde `cizgi(boy)` çağrısı | `cizgi` hazır |
| 65 | Ayrılan izler | ortasında kaya olan çarpı; kalemi kaldırıp dolaşmak gerekir | parametreli, `rock_ahead` ve `if` kullanılabilir (`komutlar`'a `rock_ahead`) | yok |

- [ ] **Step 1: Bölge 7 biçim testi** (`RiverbedTests.cs`): 61-65 için `Label == "BÖLGE 7 · NEHİR YATAĞI"`, `Intro`/`Outro` dolu, `Goal.Length <= 22`, `Routines.ParametricNames` boş değil, `tipik_hatalar` içinde en az bir **parametresiz** çözüm (adı geçen rutini `def ad():` ile tanımlayan ya da hiç rutin yazmayan) ve `Run(...)`'ı `Complete` değil. (61-70 testi tek Theory; 66-70 dosyaları gelene kadar yalnızca var olanları dolaş: `File.Exists` ile, Task 6'da tam aralığa genişlet.)
- [ ] **Step 2:** `dotnet test --filter RiverbedTests` → FAIL (dosyalar yok).
- [ ] **Step 3: Dosyaları yaz.** Önce çözümü yaz, sonra haritayı çözümün çizeceği kareler olacak şekilde kur (`O` sayısı = çözümün boyadığı kare sayısı, fazlası yok). `sinav-65.json`: `bolum: 65`, `baslik: "KIVILCIM · KENDİNİ DENETLİYOR"`, 3 soru (parametre nedir; tanımdaki ad ile çağrıdaki değer; parametre sayısı uymazsa hata: `ok(3, 4)` → `TypeError`), biçim `sinav-60.json` gibi. `sozluk.json`: yeni sayfa `pen_down / pen_up` (`kelimeler: ["pen_down", "pen_up"]`, kısa açıklama, geçerli Python örnek); `def` sayfasının açıklama ve örneğine parametre eklenir (örn. `def ok(boy):` ve `ok(3)` / `ok(5)`).
- [ ] **Step 4:** `dotnet test` (tüm paket) → PASS (bölüm denetimi, sınav denetimi, sözlük uyumu dahil).
- [ ] **Step 5: Commit** ("Bolge 7: Bolum 61-65, sinav-65, sozluk").

### Task 6: Bölüm 66-70

**Files:**
- Create: `bolum-66.json` … `bolum-70.json` (+ `.meta`), `sinav-70.json` (+ `.meta`)
- Test: `motor-test/RiverbedTests.cs` (Task 5'teki Theory tam 61-70'e genişler)

| Bölüm | Ad (taslak) | Şekil / harita | Şart |
|---|---|---|---|
| 66 | Büyüyen oklar | boyları 1, 2, 3 olan üç ok (ya da çizgi), döngüyle (`for i in range(1, 4): ok(i)`) | `ok` parametreli; çözüm döngü değişkenini verir |
| 67 | Dar kanyon | **çizim yok**; toz (`Z`, `toz_suresi`) ve kaya; hedef `H` | parametreli yürüme/bekleme rutini (örn. `yuru(adim)` içinde `while dust_here(): wait()`); `komutlar`: `move`, `dust_here`, `wait` |
| 68 | Mavi kalem | `Q` toplanır (`collect`), ardından ilk çizim | `ok` ya da `cizgi` parametreli; `Region.Riverbed` kalem kuralı (haritada yalnızca `Q` toplanır) |
| 69 | El izleri | iki ayrı rutin, farklı sayılarla; toz | `parametreli_rutinler` iki ad (örn. `cizgi`, `iz`) |
| 70 | Final | Ece'nin yazısının altına ok; kaya + toz + hedef birlikte | iki parametreli rutin, `rutin_sayisi` gerekmez (`parametreli_rutinler` yeter) |

- [ ] **Step 1:** Biçim testini 61-70'e genişlet (`File.Exists` gevşemesi kalkar); ek: 63 ve 67'de `Level.Paint` boş, 68'de `Level.Item == Collectible.BluePencil`, 70'te `parametreli_rutinler` en az iki ad; sınav sayısı: `QuizTests.Her_bes_bolumde_bir_sinav_var` 65 ve 70'i de ister.
- [ ] **Step 2:** `dotnet test --filter RiverbedTests` → FAIL.
- [ ] **Step 3: Dosyaları yaz** (Task 5'teki ortak kurallar aynen). `sinav-70.json`: iki parametre, rutin içinden parametreli rutin, döngü değişkenini rutine vermek; 3 soru.
- [ ] **Step 4:** `dotnet test` (tüm paket) → PASS.
- [ ] **Step 5: Commit** ("Bolge 7: Bolum 66-70, sinav-70").

### Task 7: Son denetim

**Files:** yok (denetim; sorun çıkarsa ilgili görevin dosyaları düzeltilir)

- [ ] **Step 1:** `cd motor-test && dotnet test` → hepsi başarılı (toplam ≈ 1403 + yeni; sayıyı ILERLEME'ye yaz).
- [ ] **Step 2: Windows paketi.** Eski `oyun/Build/` silinir; `Unity.exe -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log` (CLAUDE.md'deki komut; Unity sürümü `C:/Program Files/Unity/Hub/Editor/` altında). `build.log` içinde `error CS` yok. Unity Hub/Editor işlemleri iş bitince kapatılır.
- [ ] **Step 3: Eski bölümler değişmedi.** `-sahneler` denetimi ve `-bolum 50 -final` çalıştırılır (CLAUDE.md'deki komutlar): `SAHNE DENETIMI: TAMAM`, `FINAL DENETIMI: TAMAM`.
- [ ] **Step 4: Oyunda kısa bakış** (`oyun/calistir.bat`): 61 açılır (Canyon görünüşünde), kod kartı çalışır, `pen_down()` kodu hata vermeden çalışır; boya görünmediği bilinen eksiktir (3. parça).
- [ ] **Step 5:** Sorun yoksa Enes'e özet: yapılanlar, bilinen eksik (boyanın görünüşü yok, mavi kalem şimdilik buz gibi görünür, Riverbed Canyon görünüşünde), sıradaki parça (2. parça hikâye, sonra 3. parça Opus 5.5). Kapanışta ILERLEME.md güncellenir ve tek commit + push yapılır (ekip döngüsü).

---

## Self-review (yazarken)

- **Spec kapsamı:** §1.1 çizim → Görev 1-2; §1.2 parametre şartı → 3; §1.3 bölge/nesne → 4; §1.4 bölümler → 5-6; §1.5 sınav/sözlük → 5-6; §1.6 denetim → 7. §1.1'deki "mavi/koyu boya renk alanı" ve "Tekrar kum temizler" yalnızca görünüş/arayüzdür → 3. parça (Tekrar zaten yeni dünya kurar, ek iş yok).
- **Bilinçli sapma:** spec "toz ve hedef boyanacak karede olabilir" diyordu; harita karesi tek harf olduğundan bu parçada boyanacak kare başka işaretle çakışamaz (kural Global Constraints'te). Çizim kalem robotun başlangıç karesinde başlamaz.
- **Açık risk:** 63'teki `yuru(yon, adim)`: yön değeri (`East`) rutine geçirilince `move(yon)` çalışıyor mu? `World.DirectionOf` yönleri metin olarak ister; `Commands()` yönleri metin tutuyor, çalışması beklenir; Görev 5'te ilk iş küçük bir testle doğrulanır.
