# Bölge 6 — Kanyon girişi (Bölüm 51-60) · Tasarım

> 2026-10-08 · Enes + Claude (Opus 5.5). Enes her bölümü onayladı.
> Dayandığı belgeler: `docs/tasarim/senaryo-perde-2.md` (Bölge 6), `docs/tasarim/hikaye-kitabi.md`, `docs/tasarim/bolum-dosyasi.md`.
> Konu: **fonksiyonlar** (`def`, parametresiz). Parametre Bölge 7'de, `return` Bölge 8'de.

## Kararlar (özet)

| # | Karar | Neden |
|---|---|---|
| 1 | Serçe (drone) Bölge 6'da **yalnızca hikâyede**; oyuncu yine yalnızca Kıvılcım'ı kodlar. | Fonksiyon soyut bir konu; aynı anda ikinci araç öğretmek dikkati böler. Oynanır Serçe Bölge 7'ye ("yukarıdan / aşağıdan", parametreler) daha uygun. |
| 2 | **Rutin şartı:** bölüm dosyası hangi rutinlerin yazılması gerektiğini söyler; yoksa görev bitmez. | Yoksa oyuncu kopyala-yapıştırla geçer, fonksiyonu öğrenmez. Testler kesin denetleyebilir. |
| 3 | Kanyonda **eski beceriler + tek yeni nesne (drone parçası `D`)**. Boya oklar yalnızca görünüş. | Rutin = bildiğin adımları tek ada bağlamak; eski becerileri sarmak bunu en iyi gösterir ve tekrar dersi olur. Ok okuyan komut Bölge 7'ye saklandı. |
| 4 | Perde 2'de sınavın çerçevesi: **"KIVILCIM · KENDİNİ DENETLİYOR"**. Bölüm 50 finali son karesinden sonra "Devam" düğmesi → sınav → Bölüm 51. | "Ödev modu kapatıldı"dan sonra "ödev sınavı" tuhaf; kalan 150 bölüm için tek çözüm. Sınavın öğreticiliği korunur. |
| 5 | Perde 2'de giriş/bitiş yazıları **cümle değil, kayıt damgası** ("Sol 1.083 · Kanyon girişi", "Drone parçası: 2/5"). | Program kapandı, robotlar konuşmaz; "az söz, çok iz". |
| 6 | Üst etiket "ÖDEV N" yerine **"BÖLGE 6 · KANYON GİRİŞİ"**, büyük başlıkta görev adı. | Bölüm 50 finalindeki başlığın devamı. |
| 7 | Roket **kendi kendine** uçar: "OTOMATİK ROTA: KANYON DEPOSU · Hazırlayan: D. Aras". | Kıvılcım'a uçmak öğretilmedi (kural: öğretilmeyeni yapamaz); Defne'nin "yolu işaretledik" sözüne bağlanır. |
| 8 | "Kayıtlar" sayfası **Bölge 7'ye** kalır. Kayıt 2, Bölüm 52 başında var olan kayıt ekranıyla (`RecordingScene`) gösterilir. | Bölge 6'da tek, kısa kayıt var. |

**Ragıp'a haber:** 4, 5, 6 numaralı kararlar Perde 2'nin bütün bölgelerinin biçimini belirler; büyük hikâye kararı (ölüm/dönüş/ana gizem) yok.

## İş sırası (her parça yeni sohbette)

1. **Bulmacalar** — Sonnet 5.5 · medium (~3 sa). Bu belgenin "1. Bulmacalar" bölümü.
2. **Hikâye metinleri + sınav çerçevesi + 50→51 geçişi** — Sonnet 5.5 · medium (~2 sa). "2. Hikâye".
3. **Görünüş, nesneler, sahneler** — Opus 5.5 · high (~4-5 sa, gerekirse ikiye bölünür). "3. Görünüş".

---

## 1. Bulmacalar

### 1.1 Rutin şartı (yeni oyun kuralı)

Bölüm dosyasına iki isteğe bağlı alan:

| Alan | Anlamı |
|---|---|
| `rutinler` | Yazılması gereken rutinlerin adları, örn. `["sabah_turu"]`. Her biri `def` ile tanımlanmış olmalı **ve** kod çalışırken **en az 2 kez çalışmış** olmalı (tek kez kullanılan şey rutin sayılmaz). |
| `rutin_sayisi` | Adı serbest rutin şartı: oyuncunun `def` ile yazdığı, en az 2 kez çalışan **en az bu kadar** farklı rutin olmalı. Örn. `1` ("rutinine sen ad ver"), `2`. |

- İkisi birlikte de yazılabilir (adlı rutinler sayıya dahildir).
- "Çalışmış" sayımı kodun gerçekten yürütülmesinden gelir (döngü içinden 3 çağrı = 3). Dünya kuralları saf C#'ta kalır (`ProgramRun` / `World` tarafı); arayüz yalnızca sonucu gösterir.
- Görev tamam şartına eklenir: diğer şartlar sağlandı ama rutin şartı sağlanmadıysa görev bitmez.
- Mesajlar (Türkçe, sade):
  - Rutin hiç tanımlanmamış: "Kod bitti ama `sabah_turu` rutini yok. `def sabah_turu():` ile yaz."
  - Tanımlı ama < 2 kez çalıştı: "`sabah_turu` rutini yalnızca 1 kez çalıştı. Rutin, tekrar kullanmak içindir."
  - `rutin_sayisi` eksik: "Kod bitti ama rutin yok. Tekrar eden adımları `def` ile bir rutine koy."
- Kılavuz `bolum-dosyasi.md` güncellenir; `Level.Parse` bilinen alanlara ekler; denetim: `rutinler` adları geçerli Python adı olmalı, `cozum` şartı sağlamalı.

### 1.2 Yeni nesne: drone parçası

- Haritada `D`, `collect()` ile toplanır (`Collectible.DronePart`, Türkçe ad "drone parçası").
- Yeni bölge `Region.Canyon` (51-60); `Regions.Item(Canyon) = DronePart`. Görünüşü 3. parçaya kadar en yakın var olan bölgeninki.
- Bir bölümde yine tek tür toplanacak kuralı geçerli (örn. `D` ile `M` aynı haritada olmaz). Panel onarma (`1`-`4`), toz (`Z`), kaya, hedef, makara ölçümü + `report` gibi eski öğeler serbestçe kullanılır.

### 1.3 Bilerek uzak durulan

**Rutinin içinden dışarıdaki bir değişkeni değiştirmek** (`toplam += 1` rutin içinde → Python'da hata). Bölge 6'da sayaçlar ve toplamlar hep rutinin **dışında** (ana kodda). Oyuncu yine de yaparsa motor kendi Türkçe açıklamasını verir; bölümler buna dayanmaz. Konu Bölge 8'de (`return`).

### 1.4 Bölümler

Haritalar 6×6. Ad ve ayrıntılar plan sırasında netleşir; aşağıdaki öğretim sırası değişmez.

| Bölüm | Ad (taslak) | Öğrettiği | Şart | Öğeler |
|---|---|---|---|---|
| 51 | Kanyon kapısı | Tekrar bölümü (`while`, `if`). Yolda aynı 3 satırlık desen üç kez, araya farklı adımlar girerek geçer; döngüyle sarılamaz, oyuncu aynı satırları üç kez yazar. 52'deki rutin buna zemin hazırlar (bitişte cümle yok, karar 5). | yok | toz, kaya, hedef |
| 52 | Sabah turu | İlk `def`: bir kez tanımla, 3 kez çağır. Tipik hatalar: tanımdan önce çağırmak (`NameError`), tanımlayıp çağırmamak, gövdeyi girintisiz yazmak. Başlangıç kodu `# sabah turu - D.A.` ile başlar. | `sabah_turu` | `D` |
| 53 | Depo | Rutinin **döngüden farkı**: aynı rutin farklı yerlerde, araya başka adımlar girerek çağrılır (raftan al: uzan, topla, geri çekil). | `raftan_al` | `D`, kaya |
| 54 | Serçe | İçinde `while` olan rutin: tozda bekleyip ilerle. | `toz_gec` | `D`, toz |
| 55 | İlk uçuş | İçinde `if` olan rutin: paneli ölç, zayıfsa onar. | `panel_bak` | paneller (`1`-`9`), hedef |
| 56 | Görev çizelgesi | Döngünün içinden rutin çağırmak (depo koridorları). | `koridor` | `D` ya da panel, kaya |
| 57 | Rutin içinde rutin | Bir rutin başka bir rutini çağırır; önce küçüğü yaz. | `adim` + `tur` | toz ya da `D` |
| 58 | Boya oklar | Rutine oyuncu ad verir. | `rutin_sayisi: 1` | karışık |
| 59 | Ortak iş | Rutinler + ana kodda toplam + `report()` (yol üstündeki panellerin gücü `panel_power()` ile toplanır; makara değil: testler her bölgede tek toplanan nesne ister, kanyonda o `D`). | `rutin_sayisi: 1` | panel, toz, hedef |
| 60 | Kanyona iniş (final) | Hepsi birlikte: iki rutin, toz, panel, hedef. | `rutin_sayisi: 2` | toz, panel, hedef |

- Her bölümde `tipik_hatalar` içinde **kopyala-yapıştır çözümü** (rutinsiz) bulunur (52-57'de); test onun görevi bitirmediğini denetler.
- `python_kelimeleri`: 52'de `["def"]`. `parcalar` (acemi düğmeleri): `def sabah_turu():`, `sabah_turu()` gibi.
- Zorluk eğrisi: 52 en kolay (başlangıç kodu tanım satırını verebilir), 58'den sonra ad ve yapı oyuncuda.

### 1.5 Sınavlar ve sözlük

- `sinav-55` (tanımlamak çalıştırmaz; önce tanım sonra çağrı; girinti), `sinav-60` (rutin içinde rutin; rutin mi döngü mü; rutin adı kuralları). Testler her 5 bölümde sınav istiyor → 1. parçada yazılır; başlık değişikliği 2. parçada.
- Sözlük: tek sayfa `def` ("rutin (fonksiyon)" açıklamasıyla; sözlükte her sayfa bir kelimeyle açılır, ikinci sayfanın açılacağı kelime yok).

### 1.6 Denetim

- `dotnet test`: yeni `motor-test/RoutineTests.cs` (rutin şartı: tanımsız, 1 kez, 2 kez, döngü içinden, rutin içinde rutin, `rutin_sayisi`, mesajlar) + bütün bölüm dosyaları (var olan denetim).
- Windows paketi hatasız; Bölüm 51-60 `-shots -bolum 51` görüntüleriyle "Tamamlandı"; yedi fare denetimi TAMAM.

---

## 2. Hikâye

Sol 1.083'ten başlar (Bölge 5 sol 1.082'de bitti). Bütün metinler yeni `docs/tasarim/senaryo-bolge-06.md`'ye yazılır; `hikaye-kitabi.md` (Serçe, motifler, Perde 2 biçimi) ve `senaryo-perde-2.md` güncellenir.

| Bölüm | Olan |
|---|---|
| 50 → 51 | Final son karesi kalır → birkaç saniye sonra **"Devam"** düğmesi → sınav (`sinav-50`, "KIVILCIM · KENDİNİ DENETLİYOR") → Bölüm 51 açılışı. |
| 51 | Açılış sahnesi: roket kalkar (OTOMATİK ROTA: KANYON DEPOSU · Hazırlayan: D. Aras), kanyona iner. Kaya duvarında ilk turuncu boya ok. |
| 52 | Bölüm başında **Kayıt 2** (`RecordingScene`): "Ona ezber değil, beceri öğret. Bir kez öğrettiğini kendisi tekrar edebilir. Bunlara biz 'rutin' derdik. İlki sabah turuydu; her sabah yapardı." Başlangıç kodu `# sabah turu - D.A.` (Bölüm 9'daki notun aynısı). |
| 53 | Depo; raflarda drone parçaları. |
| 54 | Rafta drone gövdesi; turkuaz (`#2FC4B8`), çocuk eliyle **SERÇE**. |
| 55 | Kapanış: Serçe onarılır, ışıkları yanar, havalanır; kamera bir an yukarıdan bakar. |
| 56 | Depo duvarında görev çizelgesi; "BKM-7" satırında son görev **KAPI** (açıklanmaz; Bölge 8). |
| 57 | Hikâye yok. |
| 58 | Okların yanında küçük tarihler sol 852 … sol 861; boya akmış (acele). |
| 59 | Serçe hedef karenin üstünde süzülür: ilk ortak iş. |
| 60 | Final: Kıvılcım kanyonun dibine iner, Serçe üstünde süzülür, kamera derinliğe iner. Son damga: "Sonraki: kanyon dibi". |

- Giriş/bitiş: yalnızca damga (karar 5). Örnek giriş "Sol 1.084 · Depo", bitiş "Drone parçası: 3/5".
- Etiket: "BÖLGE 6 · KANYON GİRİŞİ" (karar 6); Bölüm 51-60'ta "ÖDEV" yok.
- Çelişki kontrolü (`hikaye-kitabi.md` listesi): ses kaydı yalnızca Defne; robot konuşmaz (damgalar konuşma değil); Kıvılcım uçmaz (roket yolcusu, otomatik); Serçe uçabilir (drone); kimse ölmez; kayıt öğrenciye ("sen") konuşur.

---

## 3. Görünüş ve sahneler

Her şey kodla çizilir; hazır model / büyük doku yok (paket boyutu, Ragıp'ın kuralı).

- **Gökyüzü/arka plan** (`Shaders/CanyonSky.hlsl`, `MarsSky.hlsl`'e bölge olarak): iki yanda katman katman dev kanyon duvarları (Valles Marineris esinli), dar gökyüzü şeridi, uzakta kıvrılarak derinleşen kanyon, dipte sis. İkindi; duvar gölgesi oyun alanına düşmez.
- **Zemin** (`Ground.shader` yeni çeşit): kızıl, sert, çatlaklı kaya + çakıl; Performans modu için hafif sürüm aynı işte.
- **Işık** (`RegionLook.Canyon`): güneş yönü gökyüzündeki güneşle tutarlı.
- **Nesneler** (her biri ayrı dosya): `ColonyRocket` (51-52 arka plan), `CanyonDepot` (hangar + raflar, 53-56), `DronePart` (`D`; pervane / kol / gövde parçası), `Sparrow` (Serçe; 54 rafta + "SERÇE" `WorldText`, 55'ten sonra uçar), boya oklar + tarihler (51+, 58), görev çizelgesi (56). Yerleşim `CanyonTraces.cs` (önceki `AntennaTraces` gibi).
- **Sahneler** yeni dosyada (`CanyonScenes.cs`; `Oyun.cs` ~1.700 satır, büyütülmez): 51 açılışı (roket), 55 kapanışı (Serçe ilk uçuş, üstten bakış), 60 finali (iniş, kamera derinliğe).
- **Performans riski:** Hamza'nın telefon ölçümü ~10-11 Ekim. Görünüş hafif tutulur, Performans modu birlikte yapılır, `telefon-denemesi.md`'ye Bölüm 51 kare hızı satırı eklenir.

## Kapsam dışı

Oynanır Serçe (Bölge 7), parametreli fonksiyon (Bölge 7), `return` (Bölge 8), "Kayıtlar" sayfası (Bölge 7), Bölge 5'in kendi görünüşü (ayrı iş, Hamza'nın ölçümünden sonra), `TestAllLevelsOpen` (Play Store öncesi).
