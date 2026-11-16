# Sezon 1 denetimi — Bölüm 6-10

Yöntem: yalnızca dosya okuma (bolum-06..11.json, sinav-10.json, bolum-dosyasi.md, senaryo-bolge-01.md, sozluk.json, World.cs `Collect()`). Haritalar elle sayıldı (satır 0 = en üst/kuzey), çözümler ve tipik hata kodları kare kare izlendi. Çalıştırma yapılmadı. Başlık/giriş/bitiş metinleri senaryo belgesiyle birebir aynı.
Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila. YÜKSEK bulgu yok.

## Genel

**G1 (ORTA) `dongu_sarti` alması gereken bölümler: 6, 7, 8, 9, 10** (ve bu bölgenin dışında ama aynı ders olarak 11).
Hepsi `for` dersi; hiçbiri `for` olmadan da geçilebilir (komutlar alt alta: B6 ≈ 12 satır, B7 ≈ 17, B8 ≈ 11, B9 ≈ 8, B10 ≈ 9 komut). B9'da başlangıç kodunda `for` var ama oyuncu silip 8 komut yazabilir. Şart eklenirse B9 başlangıç kodu ve çözümü zaten `for` içerdiği için sorun çıkmaz.

**G2 (ORTA) Boş karede `collect()` zararsız: "her adımda topla" kestirmesi.**
`World.Collect()` boş karede hata vermez, sözlük `collect` sayfası da "Karede toplanacak bir şey yoksa hiçbir şey olmaz" diyor (tutarlı). Sonuç: B6 ve B10'da "iki adım at, bir kez topla" dersi, tur sayısını artırıp her adımda `collect()` yazarak atlatılabilir (aşağıda). Kod tarafında çözmek zor (boş collect'i yasaklamak sözlükle çelişir); metin tarafında yanlış iddiaları düzeltmek yeter (B6 #2, B10 #2).

**G3 (DÜŞÜK) Tekrar ayrışması.** "Döngünün içine birden çok komut" B3'ten beri var (B3 döngüsü `move` + `collect`), B6 girişi, B10 ipucu 2 ve B11 ipucu 2 bunu yine "yeni bilgi" gibi söylüyor. Gerçek yeni şey: B6 aynı komutu iki kez yazmak, B7 döngüler arasında tek satır, B8 dört kenar. Emin değilim: B4/B5'in içeriğine bakmadım (talimat gereği); B7'nin B4 "İki yönlü" ile ne kadar örtüştüğünü sen bil.

## Bölüm 6 — Atlamalı

Sayım: enerji hücreleri (sütun 4, satır 1), (4,3), (2,5), (4,5) = 4 ✓ ("4 hücre topla"). Robot (0,5). Kaya (2,2) yoldan uzak, süs. Çözüm: doğuya 2+2, topla (2,5) ve (4,5); kuzeye 2+2, topla (4,3) ve (4,1) ✓. Tipik hatalar koda uyuyor: #1 tek move (yalnız (2,5) toplanır), #2 girintisiz collect (yalnız (4,5)), #3 ikinci döngü doğuya → alan dışı ✓. İpuçları 3 adet, kademeli.

1. (ORTA) `tipik_hatalar[0].aciklama` yanlış iddia içeriyor: "Her turda iki adım gerek." Oysa `for i in range(4): move(East); collect()` + `for i in range(4): move(North); collect()` görevi bitirir (boş collect zararsız, robot (4,5)'ten kuzeye 4 adım: (4,3) ve (4,1) toplanır). Dersin amacı (iki adım atlama) böylece atlatılır ve açıklama bu çözümü "yanlış" ilan etmiyor ama "gerek" diyerek yanlış söylüyor. Önerilen `aciklama`: "Tur sayısı yetmedi: tek move() ile 2 tur robotu yalnızca 2 kare yürütür, hücreler ise ikişer kare arayla dizili. Robot yarı yolda kalıyor." (G2: kestirme kodla engellenemiyor; en azından metin doğru olsun.)
2. (DÜŞÜK) `ipuclari[0]` ilk ipucunda esas fikri (iki adım atıp bir kez topla) söylüyor; ipucu 1 yalnızca yön göstermeli. Önerilen: "Enerji hücreleri ikişer kare arayla dizilmiş. Önce doğudakileri, sonra kuzeydekileri topla." ve `ipuclari[1]` başına: "Robot her turda iki adım atıp bir kez toplayabilir." eklenmesi (ipucu 2 zaten "aynı komutu iki kez yazabilirsin" diyor; birleştirilebilir).
3. (DÜŞÜK) `baslangic_kodu` (`range(2)`, tek `move(East)`, `collect()`) tipik hata #1'in aynısı yarım; oyuncuyu tuzağa iter. B7/B8/B10 başlangıç kodu çözümün ilk döngüsünü veriyor; B6 tutarsız. Önerilen: `["for i in range(2):", "    move(East)", "    move(East)", "    collect()"]`.
4. (DÜŞÜK) `giris` "Döngünün içine birden çok komut koyabilirsin." (G3). Senaryo belgesi de aynı satırı taşıyor; değişirse birlikte güncellenmeli. Önerilen: "Bir turda iki adım da atabilirsin: komutları döngünün içine art arda yaz."

Metin ve ton temiz.

## Bölüm 7 — Gidiş dönüş

Sayım: alt satır (satır 5): sütun 1-5 = 5 hücre; satır 4: sütun 0-5 = 6 hücre; toplam 11 ✓. Kayalar (2,1) ve (0,2) yolda değil. Çözüm: doğuya 5 topla; kuzeye 1 topla (köşe); batıya 5 topla = 11 ✓. Tipik hatalar uyuyor (#3: merdiven gibi yürür, sonunda hepsini toplamaz ✓). Başlangıç kodu çözümün ilk döngüsü, yardımcı.

1. (DÜŞÜK) `ipuclari[0]`: "Köşede de bir hücre var." Üst sıra zaten 6 hücre; "köşe" neresi belli değil, sayarken 5+5+1 sanılabilir. Önerilen: "Hücreler iki sıra: alttakini doğuya giderek, üsttekini batıya dönerek topla. Üst sıra bir hücre daha uzun; sıranın başı doğu ucunda."
2. (DÜŞÜK) `giris` "Uzun yol! Döngüleri art arda kullan." art arda döngü B4'te de var (G3, emin değilim). Değişiklik gerekmiyor, yalnızca not.

Zorluk makul (bölümün yeni şeyi: döngüler arası tek satır). Metin temiz.

## Bölüm 8 — Kayanın çevresi

Sayım: üst satır (satır 1) sütun 1-4 = 4; satır 2 ve 3: sütun 1 ve 4 = 4; alt satır (satır 4): sütun 2-4 = 3; toplam 11 ✓. Robot (1,4). Çözüm: kuzeye 3, doğuya 3, güneye 3, batıya 2 = 11 ✓. Tipik hatalar uyuyor (#1 range(4): üst kenarın bir üstüne çıkar, batıda kayaya çarpar; #2 alt iki hücre kalır ✓; #3 yalnız köşe toplanır ✓).

1. (ORTA) `ipuclari[1]` "sonuncusu diğerlerinden kısa" ve hata mantığı: dört kenarın hepsi `range(3)` ile de geçer (batıda üçüncü adım robotun boş başlangıç karesine (1,4) basar, hücre toplanmış olur). Yani "sonuncusu kısa olmalı" doğru değil, oyuncu sayım yanlışıyla da geçer ve neden `range(2)` olduğunu öğrenmez. Haritayla zorlamak zor; en azından ipucu yanlış yönlendirmesin. `ipuclari[1]` önerilen: "Her kenar için bir `for`: dört döngü art arda. Her kenarda kaç hücre var, tek tek say."
2. (DÜŞÜK) `parcalar` içindeki `for i in range(2):` çözümdeki sonuncu döngüyle aynı sayı; bolum-dosyasi.md "sayıları bilerek çözümdekinden farklı yaz" diyor (B6: 3, B7: 3, B9: 2, B10: 4, B11: 3 hepsi farklı). Önerilen: `"for i in range(4):"`.
3. (DÜŞÜK) `gorev` B7 ile aynı ("11 hücre topla"). Zararsız, yalnızca not.

Metin temiz.

## Bölüm 9 — Bozuk rutin

Sayım: robot (0,5); hücreler (1,5)-(4,5) = 4 ✓; kaya (5,5) bitişik, (5,4) ve (1,2) süs. Çözüm `range(4)` + girintili `collect()` ✓. Tipik hatalar koda uyuyor: #1 yalnız (4,5) toplanır ✓; #2 üç toplar ✓; #3 beşinci adımda kayaya çarpar ✓ (hücreler toplanmış olsa da kural durması görevi bitirmez). İpucu 1 "iki yanlışı var" başlangıç koduyla (range(3) + dışarıdaki collect) uyuyor; ipucu 3 tam doğru düzeltmeyi veriyor. Senaryo (D.A.) ile uyumlu.

1. (ORTA) `baslangic_kodu[0]` `# sabah turu - D.A.` yorum satırı hiçbir yerde anlatılmıyor: sözlükte `#` için sayfa yok (senaryo belgesi "sözlüğe küçük bir not" demişti, yapılmamış), ipuçlarında da yok. Yeni başlayan bunu da "bozuk" satır sanıp takılabilir ya da silmeye çalışırken kafası karışır. Sözlük sayfası kelime tabanlı olduğu için (emin değilim `#` bir sayfaya bağlanabilir mi) en güvenli çözüm ipucu: `ipuclari[0]` önerilen: "Bu eski bir bakım rutini; neredeyse doğru ama iki yanlışı var. İlk satır (`#` ile başlayan) yalnızca bir nottur, robot onu okumaz. Önce Çalıştır'a bas ve robotun ne yaptığını izle."
2. (DÜŞÜK) `gorev` "Rutini onar" ve `giris` "rutin": Bölge 6-8'de "rutin" `def` ile yazılan fonksiyon anlamına geliyor (sözlük `def`, `rutinler` alanı). Burada hiç `def` yok, `for` döngüsü. Terim sonradan başka anlam kazanıyor. Hikâye gereği ("bakım rutini") bırakılabilir; öğretici açıdan "Kodu onar" daha az karıştırır. Emin değilim, senaryo kararı olabilir.
3. (DÜŞÜK) Bölüm tipi yeni: hazır bozuk kodu düzeltmek (önceki bölümlerde sıfırdan yazılıyordu). `bilgi` kartı yok; `giris` yeterince açık, karar sizde.

Metin ve ton temiz.

## Bölüm 10 — Telsiz

Sayım: sütun 0'da satır 2, 3, 4 = 3 hücre; (2,2) ve (4,2) = 2; toplam 5 ✓. Hedef H (4,0). Kayalar (2,0), (1,4), (3,4) yolda değil. Çözüm: kuzeye 3 topla; doğuya 2+2 topla (2,2), (4,2); kuzeye 2 → (4,0) hedef ✓. Tipik hatalar: #1 hedefe çıkılmadı ✓ (açıklama doğru), #3 `range(4)` hücre sırasının üstüne çıkar, doğudakileri ıskalar, hedefe yine de varır ✓ (açıklama doğru).

1. (ORTA) `tipik_hatalar[1].aciklama` ("iki kez move(East) gerek") yanlış iddia: `for i in range(4): move(East); collect()` (tek move, 4 tur) de geçer (G2). Üstelik gösterilen kodda (`range(2)`, tek move) olan şey başka: robot yalnızca (2,2)'ye varır, hücreyi toplar, kuzeye giderken (2,0)'daki kayaya çarpar. Önerilen `aciklama`: "range(2) ve tek move(East) robotu yalnızca ilk hücreye götürür; sonra kuzeye giderken kayaya çarpar, doğudaki hücre kalır."
2. (DÜŞÜK) Bölüm 6'nın birebir uzantısı (sütun topla + iki adımlı hücreler + hedef); yeni bir şey öğretmiyor, bölge finali olarak "tekrar" işlevi görüyor. Bilinçli olabilir; zorluk eğrisi için aşağıya bak.
Giriş/bitiş metni ("Tebrikler!" dahil) senaryodaki neşeli program tonuyla uyumlu, değişiklik önerilmiyor.

## sinav-10.json (Bölüm 10 sınavı)

Cevaplar doğru (1, 0, 1). Dersle uyumlu (girintisiz satır, döngüde çok komut, range sayısı).
1. (ORTA) Soru 2 yanlış kurulmuş: "Aynı satırı (örn. iki move) bir turda tekrar tekrar yazmak yerine ne yapılır?" Cevap seçeneği "Döngünün içine iki satır birden yazılır" soruya cevap olmuyor (soru satırı tekrar etmeyi kasteder, cevap döngünün içine yazmayı). Önerilen `soru`: "Bir turda iki komutun da çalışmasını istiyorsun (örn. önce move, sonra collect). Ne yaparsın?" Seçenek 0 aynı kalabilir.
2. (DÜŞÜK) Yanlış şıklar "İki ayrı dosya açılır", "Satırlar yarıya bölünür" şaka gibi, 13+ için ciddi şıklar daha öğretici. Önerilen: "Her komut için ayrı bir for yazılır" ya da "Komutlar arasına noktalı virgül konur".

## 10 → 11 geçişi

B11 (Merdiven, buz) yeni bölge/yeni nesne; teknik olarak B10'dan kolay (tek döngü, 3 komut). Sıçrama yok. Kontrol: 5 buz ✓, kaya (1,5) robotun hemen sağında ✓, hata açıklamaları uyuyor. (Not: B11 de `for` dersi, `dongu_sarti` listesinde.)

## Sözlük kontrolü

`move`, `Yönler`, `collect`, `for`, `range` sayfaları bu bölümlerin anlatımıyla çelişmiyor. `collect` "yoksa hiçbir şey olmaz" bölüm açıklamalarıyla uyumlu. `for` sayfası bu düzeyin üstünde (metin/liste/iç içe) — bu bulgu Bölüm 3 raporunda var, tekrarlanmadı. `#` yorum için sayfa yok (B9 #1).

## Özet — zorluk eğrisi

6 (orta) → 7 (kolay-orta, uzun) → 8 (zirve: dört kenar, sayma) → 9 (çok kolay, hata bulma) → 10 (6'nın biraz uzatılmışı); final 8'den kolay ama bölge sonu rahatlaması olarak kabul edilebilir, 10→11 sıçraması yok.
