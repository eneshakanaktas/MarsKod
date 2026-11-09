# Bölge 8-10 — Ders ve bulmaca tasarımı (Bölüm 71-100, Sezon 1'in son üç bölgesi)

> 2026-11-09 · Enes + Claude (Opus 5.5). Kararlar "Enes" diye işaretli olanlar Enes'in; "Claude'un önerisi" olanlar Enes'e gösterildi, itiraz edilmedi ya da açıkça Claude'a bırakıldı.
> Dayandığı belgeler: `docs/tasarim/hikaye.md` (Sezonlar notu, satır 11), `docs/tasarim/senaryo-perde-2.md` (Bölge 8-10 bölüm listeleri), `docs/tasarim/hikaye-kitabi.md`, `docs/superpowers/specs/2026-11-02-bolge-7-design.md` (biçim ve "değişen yol" altyapısı), `docs/tasarim/bolum-dosyasi.md`.
> Kapsam: **yalnızca ders sırası ve bulmaca türleri** (bölüm bölüm ne öğretilir, hangi tür bulmaca, hangi yeni kural gerekir). Haritalar, çözümler, tipik hatalar her bölgenin "1. Bulmacalar" yapım planında yazılır. Hikâye metinleri ve görünüş ayrı parçalardır (aşağıda "İş sırası").

## Neden (amaç)

**Enes:** Bölüm 100'ü bitiren oyuncu, 100 bölümde öğrendiği her şeyi gerçekten oturtmuş olmalı ve "daha zor şeylere hazırım" diye hissetmeli.

**Başarı ölçüsü (Enes seçti):** Oyuncu oyunun dışındaki bir problemi kendi başına yazabilmeli. Örnek ödev: *"Bir şifreye bakıp güçlü mü zayıf mı diyen bir fonksiyon yaz: en az 8 harf, en az bir rakam, bilinen bir şifre (`"123456"` gibi) olmamalı."* Bu ödev metin, döngü, `and/or/not`, `if` ve `return`'ü birlikte kullanır. Sezon 1 bu ödevi yazdırabilecek kadar hazırlamalı.

**Ragıp'ın önceki kararı (2026-11-02, `hikaye.md`):** Sezon 1 = Bölüm 1-100; ders yarım kalmamalı. Eksik üç konu Bölge 8-10'a alınır: metin (string) → 8, iç içe döngü → 9, `and/or/not` → 10. Hikâye olayları ve bölüm numaraları yerinde kalır.

## Kararlar (özet)

| # | Karar | Kimin | Neden |
|---|---|---|---|
| 1 | Şifre ödevinin karşılığı **hikâyeye giydirilir**; oyuncu Mars'tan çıkmaz. | Enes | Oyunun dünyası bozulmasın. |
| 2 | **Kod çözme bölümleri** en az 3-5 tane: bazıları **yalnız ekranda** (robot yürümez), bazıları **ekran + robot birlikte**. | Enes | Görsel ve oynanış çeşitliliği: 70 bölümdür hep robot yürüyor, hep aynı görüntü. |
| 3 | Ana mekanik 100'e kadar **robotu yürütmek** kalır. Yeni mekanikler (yeraltı, maden, madenle robotu geliştirmek: hız vb.) **100'den sonra**, Ragıp'la birlikte düşünülür. | Enes | Bu belgenin dışında; aşağıda "İleride". |
| 4 | **Listede dolaşma Bölge 9'a, liste dersinin içine alınır.** Metinde `for harf in metin` öğrenilince `for x in liste` aynı kalıp; ayrı konu gibi yer tutmaz. | Claude'un önerisi | Altı yeni konu 30 bölüme sıkışınca pekiştirmeye yer kalmıyordu. |
| 5 | **95-100'de yeni konu yok**; eski konuların hepsini birlikte kullanan bulmacalar. | Claude'un önerisi (Enes itiraz etmedi; "içinde kod çözme de olsun" diye anlaşıldı) | "Hazırım" hissi yeni konuyla değil, bildiklerini birlikte kullanınca gelir. |
| 6 | **Tutarlılık birinci kural** (aşağıda "Tutarlılık kuralları"). | Enes | "Mantık hatası yaparsan olmamış derim." |
| 7 | Üç bölge **birbirinden ve kanyondan bilerek farklı görünür**: 8 kapalı mekân (istasyon içi, ekranlar), 9 bembeyaz, gökyüzünü yansıtan göl, 10 gece + meteorlar. | Enes'in isteğinden ("arka plan çok tekrar ediyor") | Görünüş işi ayrı (3a/3b); bu belge yalnızca kuralı koyar. |

## Ders sırası

Bölüm 1-70'te öğretilenler: `move`, `collect`, `for` + `range`, `if/elif/else`, karşılaştırma, `while`, değişkenler (sayaç, toplam), fonksiyonlar, parametreli fonksiyonlar (varsayılan değer dahil). Oyun komutları zaten değer veriyor (`ice_here`, `rock_ahead`, `panel_power`, `cable_length`, `scout`); oyuncu "cevap veren komut" kavramını tanıyor ama kendi fonksiyonundan değer döndürmedi.

| Bölge | Bölümler | Yeni konu |
|---|---|---|
| 8 · Terk edilmiş istasyon | 71-74 | `return` (sayı, yön, `True/False` döndüren fonksiyon) |
| | 75-80 | Metin: tırnak, `==`, `for harf in metin`, `len`, `in`, harf sayma; `upper` (bir kez, görünür sonuç için) |
| 9 · Tuz gölü | 81-85 | Liste: `[...]`, `for x in liste`, `liste[0]`, `len`, `append`, `in` |
| | 86-90 | İç içe döngü: satır satır tarama, metin listesinde harf harf dolaşma |
| 10 · Gece ovası | 91-94 | `not`, `and`, `or`; `True/False` döndüren "tehlike" fonksiyonu |
| | 95-100 | **Yeni konu yok** — pekiştirme ve Sezon 1 finali |

Bilerek **dışarıda bırakılan** (Sezon 2'ye): sözlük, `split/join`, dilimleme (`metin[1:3]`), `while True` + `break`, `elif` zincirinden sözlüğe geçiş, sıralama algoritması, özyineleme, sınıflar. `isdigit` yalnızca 99'da ipucu olarak önerilebilir; zorunlu değil (`h in "0123456789"` da çözer).

## Bulmaca türleri

Bölüm 1-70'teki tür **yürüme bulmacası** (A). Bölge 8-10 iki yeni tür ekler:

**A · Yürüme bulmacası (var olan):** harita, robot, toplama, hedef; değişen yol (2-3 iz) Bölge 7'den beri var.

**B · Ekran bulmacası (yeni, robot yürümez):** Oyuncu adı belli bir fonksiyon yazar. Oyun onu birkaç girdiyle çağırır ve istasyon ekranına her satırı yazar: `girdi → oyuncunun cevabı → ✓/✗`. Hepsi doğruysa görev biter.
- Bölüm dosyasında yeni alan (ad yapım planında kesinleşir, öneri `ekran_sinavi`): fonksiyon adı, **görünen örnekler** (girdi + beklenen cevap; görev kartında görünür) ve **gizli örnekler** (yalnızca çalıştırınca ekranda görünür). Gizli örnekler, Bölge 7'deki değişen yolun işini görür: görünen örnekleri `if kod == "...": return True` diye ezberleyen kod takılır.
- Kod hata verirse ya da fonksiyon yoksa mesaj Türkçe ve somut ("`kapi_kodu` fonksiyonu yok", "`"7A3"` için `True` bekleniyordu, `None` geldi: `return` unutulmuş olabilir").
- `None` dönüşü en sık hata olacak (fonksiyon `print` ediyor ya da `return` yok); bunun için ayrı açıklama metni (`ExplanationsTr`).
- Görünüş: istasyon ekranı (Bölge 8 ortamına ait). Ekran bulmacası Bölge 9-10'da da çıkar (99'da sığınaktan gelen mesaj aynı ekran düzenini kullanır; çerçevesi anten/telsiz). Ekranın tasarımı görünüş parçasında; ilk sürüm (1. parça) sade bir kutu olabilir.

**C · Okuyan robot (yeni, ekran + robot birlikte):** Robot, bölüm dosyasının verdiği bir **veriyi** (metin ya da liste) okuyup ona göre yürür. Veri her denemede değişebilir (2-3 sürüm; Bölge 7'deki değişen yol altyapısı genellenir: her sürüm ayrı denenir, hepsinde bitince görev biter). Ezber yön listesi bu yüzden çalışmaz.
- Veri oyuncuya nasıl gelir: **yeni oyun komutu** (öneri `read_note()` — "istasyondaki notu/haritayı okur", metin ya da liste verir). Neden değişken değil de komut: veri her denemede değişmeli; başlangıç koduna yazılan değişken değişmez. (Tek istisna 85: Defne'nin yarım `yanimiza_al` listesi başlangıç kodunda durur; hikâye izi.)
- **Komutlar değişmez:** robot her zaman `move(North)`, `move(East)`... ile yürür; `North/East/South/West` kalır. Değişen yalnızca **notların içindeki yazı**: Ece ve Defne notlarına yönleri Türkçe kısaltmayla yazmış: `K` kuzey, `G` güney, `D` doğu, `B` batı (örn. `"KKDD"` = iki kuzey, iki doğu). Oyuncu bu harfi komuta çeviren fonksiyonu kendisi yazar (76'dan itibaren): `if harf == "K": return North` — `return`'ün en doğal kullanımı. (Koloni Türkçe konuşuyor: liste satırları, "görev: KAPI" da Türkçe; notlar da öyle olmalı.) **Enes onayladı, şartı:** harf ile komut farklı olduğu için (`D` doğu ama komut `East`) 75 ve 76'da **ipucu bunu kod örneğiyle açıkça gösterir**: ilk ipucu "Nottaki harfler Türkçe: K kuzey → `North`, G güney → `South`, D doğu → `East`, B batı → `West`"; kod parçası veren ipucu `if harf == "K":` / `    return North` kalıbını gösterir. `tipik_hatalar`a `if harf == "D": return West` (doğu/batı karıştırma) ve `move("K")` (harfi doğrudan komuta vermek) eklenir; ikincisi için Türkçe açıklama ("`move` yön ister, harf değil") yazılır.

Kod çözme bölümleri (karar 2): **B:** 72, 77, 78, 99 · **C:** 76, 80, 84, 89, 97. Toplam 9 (Enes'in alt sınırı 3-5; fazlası "okuyan robot"un tekrarları, yürüme ile karışık).

B türünde robot yine haritadadır (bölüm dosyası haritasız olamaz): istasyon ekranının önünde durur, yürümez; harita küçük ve sade tutulabilir (builder'ın kararı).

## Bölüm bölüm

Sütunlar: hikâye olayı (`senaryo-perde-2.md`'den, değişmez) · ders · tür · yeni kural/komut. Toplanacak nesneler `hikaye.md` tablosundan: Bölge 8 **veri kartı**, 9 **tuz kristali**, 10 **meteor taşı** (harita harfleri yapım planında; var olan harflerle çakışmamalı: `E B K T H P Z M D C R` ve `0-9` dolu).

### Bölge 8 — Terk edilmiş araştırma istasyonu (71-80) · `return` + metin

| # | Hikâye | Ders | Tür |
|---|---|---|---|
| 71 | İstasyon; kapıda arma | İlk `return`: sayı döndüren rutin (`def kapi_mesafesi(): return 4` → `for i in range(kapi_mesafesi()):`). "Komutlar cevap veriyordu; artık senin rutinin de verir." | A |
| 72 | Bilgisayarlar uyanır | `return` + parametre: girdiden cevap hesaplayan fonksiyon (uyanma sırası: "her sayının iki katı" gibi tek kural). **İlk kod çözme.** | **B** |
| 73 | Çizimler (ilk yarı) | `True/False` döndüren fonksiyon, `if` içinde kullanılır (`def dolu_mu(): return panel_power() > 50` gibi; `not` henüz yok). | A |
| 74 | Kayıt 4 | `return` pekiştirme: bir döngüde hesaplanan toplamı döndür, `report()` ile gönder. | A |
| 75 | Çizimler (ikinci yarı): dışarıda tek robot | Metne giriş: tırnak, `for harf in rota`, `if harf == "K":`. Çizimin altındaki yön yazısını oyuncu başlangıç kodunda hazır bulur (`rota = "KKDD"`; tek sürüm, yeni komut yok). | A (metinli) |
| 76 | Katlanmış çizim (çantaya) | `read_note()` açılır (yeni komut; yeni Python yok): rota artık nottan okunur ve her denemede değişir. Harfi yöne çeviren fonksiyon: `def yon(harf): ... return North` → `move(yon(harf))` (bilinen `return`'ün uygulaması). **İlk okuyan robot.** | **C** (2-3 sürüm) |
| 77 | Bilgisayarda 24 kişilik liste | Metinde `in`: her satıra bakıp `"hazır"` geçiyor mu (`True/False`). Oyun **yalnız 24 kişinin satırını** tek tek verir; ekranda liste dolar. Liste **henüz öğretilmedi**: oyun satırları tek tek çağırır, oyuncu liste görmez. | **B** |
| 78 | Listenin altı: BKM-7 | Ekran 26 satıra uzar (24 + BKM-7 + KOR). Aynı kalıpla `"dışarıda"` geçen satırı bulmak; ekranda o satır vurgulanır: **BKM-7 — dışarıda — görev: KAPI.** (Açıklama anı; oyuncunun kendi kodu ortaya çıkarır.) KOR satırı görünür ama vurgulanmaz (79'un konusu). | **B** |
| 79 | KOR — hizmet dışı — hangar | Harf sayma: `len` ve sayaçla bir harfin kaç kez geçtiğini döndüren fonksiyon → robot o kadar adım ya da `report()`. | A |
| 80 | Final: istasyon haritası, oklar tuz gölünün ötesini gösteriyor | Bölge 8'in hepsi: `read_note()` haritadaki rotayı verir, harf→yön fonksiyonu, `return`, rutin şartı. | **C** (3 sürüm) |

Sınavlar: `sinav-75` (`return` ile `print` farkı; `None`; dönen değeri değişkene almak), `sinav-80` (tırnaklı/tırnaksız: `"4"` ile `4`; `==` ile `=`; `len("KOR")`; `in`).

### Bölge 9 — Tuz gölü (81-90) · liste + iç içe döngü

| # | Hikâye | Ders | Tür |
|---|---|---|---|
| 81 | Kıyı; ilk liste | Liste yazmak ve dolaşmak: `yol = [North, North, East]`, `for y in yol: move(y)`. "Metindeki harfler gibi." | A |
| 82 | Ufuk kayboluyor | `crystal_size()` açılır (yeni komut, `cable_length` gibi kristalin boyunu verir; yeni Python yok): listedeki yönlerle yürü, kristalleri ölç, toplamı `report()` ile gönder. | A |
| 83 | Kayıt 5 | `append`: ölçülen boyları boş bir listeye ekle, sonunda en büyüğünü `report()` et (`max` serbest, döngüyle de çözülür). | A |
| 84 | Tuzla çevrelenmiş oklar | `liste[0]`, `len(liste)`; sıranın 0'dan başlaması (ilk "aha"). `read_note()` artık bir **yön harfleri listesi** verir; ilk ok tuzla çevrili olandır (`oklar[0]`); harf→yön fonksiyonu yeniden kullanılır. | C (2 sürüm) |
| 85 | Başlangıç kodunda `yanimiza_al` listesi | Listede `in`; sondaki virgülün hata olmadığı (görev kartında küçük not). Hikâye izi başlangıç kodunda kalır, oyuncu silmeden çözebilmeli. | A |
| 86 | Tuza gömülü oyuncak astronot (çantaya) | **İç içe döngüye giriş:** gölü satır satır taramak (`for satir in range(...)` içinde `for adim in range(...)`), gömülü nesne taranınca bulunur. | A |
| 87 | Kristaller sıraya konur | İç içe döngü + metin listesi: `read_note()` gölün küçük haritasını satır satır verir (`["..K..", ".K..K", ...]`); kaç kristal var → `report()`. (**"Haritada kaya say" ödevinin hikâyedeki hâli.**) | A + veri |
| 88 | Volkan belirginleşir | İç içe döngü + `if`: tararken yalnızca sensörün gösterdiği karelerde topla. | A |
| 89 | Gölün ortası; her yön aynı, yalnız oklar | Rota parçaları listesi (`["KKD", "DG", ...]`): dış döngü parçalar, iç döngü harfler. | **C** (3 sürüm) |
| 90 | Final: karşı kıyı, gece çöküyor | Bölge 9'un hepsi + fonksiyon. | A/C |

Sınavlar: `sinav-85` (`liste[0]`, `len`, `append`, sondaki virgül), `sinav-90` (iç içe döngüde kaç kez çalışır: 3×4=12; iç ile dış döngünün sırası).

### Bölge 10 — Gece ovası, meteor yağmuru (91-100) · `and/or/not` + pekiştirme

Tehlikeyi ölçen 1-2 yeni komut gerekir (öneri: `smoke_here()` dumanlı krater, `meteor_ahead(yon)` o yönde yeni düşmüş taş/krater). Az komut, çok birleşim.

| # | Hikâye | Ders | Tür |
|---|---|---|---|
| 91 | İlk meteorlar | `meteor_ahead(yon)` açılır (yeni komut; yeni Python yok): `if/else` ile meteor düşen kareden kaçmak. | A |
| 92 | Meteor taşları toplanır | `not`: `if not meteor_ahead(East):`. | A |
| 93 | Kayıt 5b | `and`: iki şart birden (`not meteor_ahead(yon) and not rock_ahead(yon)`). | A |
| 94 | Kıvılcım mavi noktaya bakar | `or` + üçü bir fonksiyonda: `def guvenli_mi(yon): ... return ...`; fonksiyonla tehlike tartmak. | A (değişen yol, 2 sürüm) |
| 95 | Dumanlı yeni krater | **Pekiştirme başlar** (yeni Python yok). `smoke_here()` açılır (yeni komut); `while` + `and` + fonksiyon. | A |
| 96 | Yağmur yoğunlaşır | Değişen yol + `guvenli_mi` + liste (geçilen kareleri sayma). | A (3 sürüm) |
| 97 | Kapının silueti | Okuyan robot: rota listesi + harf→yön + tehlikeli kareyi atlama (`and/or`). | **C** |
| 98 | Yağmur diner; sessizlik | Bölge 8-10'un hepsi; büyük ama sakin bölüm (az ipucu). | A |
| 99 | Sığınaktan otomatik mesaj | **Şifre ödevinin hikâyedeki hâli:** antene birçok kod gelir (gürültü + gerçek sinyal). Oyuncu `gercek_mi(kod)` yazar: kural Defne'nin bıraktığı nottan (örn. en az 8 karakter **ve** en az bir rakam **ve** `"0000"`/bilinen gürültü değil). Ekranda kodlar akar; gerçek olan(lar) kalır → mesaj açılır (Bölüm 100'e geçiş). | **B** |
| 100 | Perde 2 / Sezon 1 finali: iki saat | Kısa ve duygusal bölüm: hepsi birlikte ama orta zorlukta (sınav tadı değil, son bir yürüyüş: antene varıp ışığı **bir kez** yakmak). Final sahnesi (`senaryo-perde-2.md` "Bölüm 100") bulmacadan sonra. | A |

Sınavlar: `sinav-95` (`and/or/not` doğruluk tablosu; `not` ile `!=`), **`sinav-100` = Sezon 1 büyük sınavı** (her konudan bir soru; şifre ödevine benzer bir kod okuma sorusu dahil). Çerçeve önceki bölgelerdeki gibi "KIVILCIM · KENDİNİ DENETLİYOR".

## Zorluk eğrisi

- Her yeni konunun **ilk bölümü** kolaydır (başlangıç kodu yapının yarısını verebilir: 71, 75, 81, 86, 92).
- Her bölgenin **ilk yarısında yalnız o bölgenin konusu**, ikinci yarısında eski konularla birleşir; bölge finali (80, 90) bölgenin hepsi.
- 95-100 eski konuların hepsini kullanır; ipucu sayısı azalabilir (98) ama **100 sert değil** (duygusal final; oyuncu orada takılıp bırakmasın).
- Bir bölümde **en çok bir yeni şey**. Yeni komut (`read_note`, `crystal_size`, `smoke_here`, `meteor_ahead`) yeni Python konusuyla aynı bölümde açılmaz; ya önce ya sonra.

## Tutarlılık kuralları (Enes: "mantık hatası olmasın")

Yapım planlarında her bölüm için ayrı ayrı denetlenir; mümkün olanı `dotnet test` denetler:

1. **Yalnızca öğretilmiş şeyle çözülür.** Her bölümün çözümü ve ipuçları o ana kadar açılmış Python kelimeleri ve komutlarıyla yazılır (`python_kelimeleri` alanı). 73'te `not`, 77'de liste, 86'dan önce iç içe döngü **yok**. Denetim: çözümde açılmamış kelime var mı (testle).
2. **Motor destekliyor mu:** kullanılan her metot/işlem `motor-test` ile gerçek Python'a karşı sınanır (`tests/python-cases/`, `scripts/python_referans.py`). Şu an var olduğu doğrulananlar: `len`, `in` (metin ve liste), `upper/lower`, `isdigit`, `append`, `liste[i]`, `max/min/sum`, `and/or/not`.
3. **Klavyeyle yazılabilir mi:** oyuncunun yazması gereken her işaret oyun klavyesinde olmalı (harfler Türkçe dahil, `" ' [ ] == != < >` var). Veri metinlerinde geçen `—` gibi işaretleri oyuncu **yazmak zorunda kalmaz** (`"dışarıda"` diye arar, satırın tamamını yazmaz). Acemi kademesi için `parcalar` düğmeleri her bölümde çözümü kurabilmeli.
4. **Hikâye sayıları:** 24 kişi (`hikaye-kitabi.md` 6. madde) + iki satır (BKM-7, KOR) = ekranda 26 satır. Satır biçimi `senaryo-perde-2.md`'deki gibi (`BKM-7   — dışarıda — görev: KAPI`, `KOR     — hizmet dışı — hangar`). 24 ismin hepsi yazılır (henüz yazılmadı: Bölge 8 hikâye parçasında; içinde Defne Aras ve Ece olmalı, "Ece Aras" mı başka soyadı mı `hikaye-kitabi.md`'ye bakılır).
5. **Görünen ile gizli örnekler** (B türü) ve **veri sürümleri** (C türü) aynı kurala uyar: görünen örneklere ezberlenmiş çözüm her bölümün `tipik_hatalar`ında bulunur ve test onun görevi **bitirmediğini** denetler.
6. **Çanta eşyaları** sırası `senaryo-perde-2.md` ile aynı: 76 katlanmış çizim, 86 oyuncak astronot.
7. **Bölüm 100 sahnesinin sayıları** (`%31`, `101 SOL`, `94 SOL`, `14 GÜN`) bulmacada kullanılırsa aynı kalır.
8. **Belgeler arası çelişki:** `hikaye.md` satır 30 hâlâ "Sezon 1 — 200 bölüm" diyor, satır 11'deki Sezonlar notu "Sezon 1 = 1-100". Ayrıca `hikaye.md` tablosundaki "Yeni Python" sütunu (Bölge 8-14) bu belgeyle uyumlu hâle getirilir (Bölge 11/12/14 Sezon 2'de yeniden dengelenecek diye işaretlenir). Bu düzeltme Bölge 8'in hikâye parçasında yapılır.

### Görsel tutarlılık (Enes: "görsel kısım da mantık çerçevesi içinde olmalı")

Yukarıdaki maddeler yalnızca kod için değil; ortam, nesneler, ışık ve animasyon da aynı kurala bağlıdır. Görünüş parçalarında (3a/3b) her görüntü Enes'e gösterilmeden önce bunlar ölçülür (göz kararı değil; mümkünse sahne denetimiyle):

1. **Her nesne yere basar.** Kaya, kristal, meteor taşı, masa, ekran, çizim kâğıdı havada durmaz, zemine gömülüp kaybolmaz. Tuz gölünde nesnenin altında yansıması ve gölgesi tutarlı olur (yansıma nesneyle aynı yerde, ters).
2. **Yazı ve çizim yüzeyin içinde kalır.** Duvar çizimleri, ekran yazıları, not kâğıtları kendi yüzeyinin dışına taşmaz (Bölge 7'deki "KIVILCIM BENİ BUL" taşması tekrar olmaz). Ekrandaki satırlar ekranın içine sığar; 26 satır sığmıyorsa kayar ya da küçülür, dışarı taşmaz.
3. **Işık tek kaynaktan ve saate uygun.** Bölge 8 iç mekân: ışık istasyonun lambalarından ve ekranlardan gelir, gölgeler ona göre; pencere varsa dışarıdaki saatle uyumlu. Bölge 9: gündüzden gün batımına (90), gölgeler güneşin yönüyle aynı. Bölge 10: gece; ışık yıldızlardan, meteorlardan ve Kıvılcım'ın lambasından. Gece sahnesinde gündüz gölgesi olmaz.
4. **Fizik mantıklı.** Meteorlar gökyüzünden aynı genel yönden düşer; düştüğü yerde krater ve taş kalır (krater önceden varsa meteor oraya yeni düşmüş olamaz); duman yukarı yükselir ve rüzgârla aynı yöne eğilir. Tuz kabuğu üstünde ayak izi ve robot izi kum gibi değil, kırılmış kabuk gibi görünür.
5. **Ölçek tutarlı.** İstasyonun kapısı, masaları, yatağı insan boyunda; Kıvılcım'a göre doğru büyüklükte. Ece'nin çizimleri çocuk boyunda bir yüksekliğe yapılmış (duvarın alt kısmı). 24 kapsül, 24 yatak gibi sayılar görünüşte de tutar.
6. **Hikâye ile görünüş aynı şeyi söyler.** Örneğin 78'de ekrandaki satır `senaryo-perde-2.md`'dekiyle harfi harfine aynı; çantadaki eşyalar (kurdele, kalem, 76'dan sonra katlanmış çizim, 86'dan sonra astronot) Kıvılcım'ın üstünde sırayla belirir ve kaybolmaz.
7. **Animasyonda çakışma yok.** Robot, Serçe ve nesneler birbirinin ya da duvarın içinden geçmez (Bölge 6-7'deki sahne denetimleri gibi ölçülür).

## Yeni kurallar ve kod (özet; ayrıntı yapım planlarında)

Hepsi saf C# (`oyun/Assets/Dunya/`), `dotnet test` ile sınanır; arayüz yalnızca çağırır.

- **Ekran bulmacası (B):** bölüm dosyasında yeni alan (fonksiyon adı + görünen/gizli örnekler; girdi ve beklenen cevap sayı, metin ya da `True/False`). `Level.cs` denetleyicisi: örnek sayısı ≥3, en az biri gizli, beklenenler çözümle tutuyor. `ProgramRun` oyuncunun kodunu çalıştırıp fonksiyonu her örnekle çağırır, sonucu kaydeder (arayüz ekrana basar).
- **Okuyan robot (C):** `read_note()` komutu + bölüm dosyasında veri sürümleri listesi (metin ya da metin listesi). Bölge 7'nin "değişen yol" denetimi (`TrailRuns`) genellenir: "sürüm" = iz ya da veri; tekrar yok, iki ayrı çok-denemeli sistem kurulmaz.
- **Yeni komutlar:** `read_note` (76; 84'ten itibaren liste de verebilir), `crystal_size` (82), `meteor_ahead` (91), `smoke_here` (95) — adlar plan sırasında kesinleşir, `PythonNames`/sözlük ile çakışmamalı.
- **Sözlük (`sozluk.json`):** yeni sayfalar `return`, metin (`str`), liste (`list`), `and`, `or`, `not`; `for` sayfasına "metinde/listede dolaşma" ve iç içe döngü eklenir. `dotnet test` sözlük-bölüm uyumunu zaten denetliyor.
- **Açıklamalar (`ExplanationsTr`):** `None` dönüşü, `TypeError` (metin + sayı toplamak: `"4" + 1`), `IndexError` (listenin dışı) için sade Türkçe açıklama.

## İş sırası (her parça yeni sohbette)

Bölge 7'deki düzen. Her bölge için:

1. **Bulmacalar + yeni kurallar** — Sonnet 5.5 · medium (Bölge 8'de ekran bulmacası ve okuyan robot ilk kez kurulduğu için **high** ya da önce Opus ile plan). Plan yazılırken her bölümün çözümü ve tipik hataları gerçek Python'la denenir (Bölge 7'de yapıldığı gibi).
2. **Hikâye metinleri + belgeler** — Sonnet 5.5 · medium. Bölge 8'de 24 isim, Kayıt 4, `hikaye.md` düzeltmeleri.
3. **Görünüş (3a ortam/nesneler + 3b sahneler)** — Opus 5.5 · high. Bölge 8'de **istasyon ekranı** (B türünün görünüşü) ilk kez tasarlanır.

Sıra: Bölge 8 (1→2→3) → Bölge 9 → Bölge 10. Bölge 10'un 1. parçası 99 ve `sinav-100`'ü de kapsar. Her bölge sonunda telefon testi hatırlatması (Enes'in kuralı).

## Builder'a bırakılanlar

Harita harfleri, komut adlarının son hâli, bölüm dosyasındaki yeni alanların adı ve biçimi, ekranın ilk (sade) görünüşü, her bölümün haritası/çözümü/ipuçları/tipik hataları, sınav soruları, kristal boyları ve tehlike yerleşimi. Bunlarda yukarıdaki tutarlılık kuralları bağlayıcıdır.

## İleride (bu belgenin dışında)

- **100'den sonra yeni mekanikler (Enes + Ragıp):** yeraltı, maden toplama, toplanan madenle robot geliştirme (hız vb.). Sezon 2 konularıyla birlikte düşünülür.
- Sezon 2'nin Bölge 11, 12, 14 konuları boşaldı; ileri konularla (sözlük, dilimleme, `split`, sıralama...) yeniden dengelenir.
- Enes'in rafa kaldırdığı çizim mekaniği (`bolge7-cizim` dalı, "kalem iner, kare boyanır") Bölge 8-10'da **kullanılmaz**; ileride yön tarif etmek için düşünülebilir.
