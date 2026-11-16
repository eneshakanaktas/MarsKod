# Sezon 1 denetimi — Bölüm 1-5

Yöntem: yalnızca dosya okuma (bolum-01..06.json, bolum-dosyasi.md, senaryo-bolge-01.md, sozluk.json). Haritalar elle sayıldı, çözümler adım adım izlendi. Dört bölümde de çözüm ve tipik hata kodları haritayla tutarlı; başlık/giriş/bitiş metinleri senaryo belgesiyle aynı.
Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila. YÜKSEK bulgu yok.

## Genel

**G1 (ORTA) Döngü bölümlerinde dersi atlatan kestirme açık.**
Bölüm 3, 4, 5'te `for` kullanmadan komutları alt alta yazarak görev geçilir (B3 10 satır, B4 14 satır, B5 12 satır). Bölüm dosyasında buna karşı bir şart alanı yok (Level.cs'in bildiği alanlarda da yok). Bölüm 3 `giris` "döngü kullan" diyor ama zorunlu değil. Düzeltme kod tarafında: "bu bölümde `for` zorunlu" türü bir şart alanı (dosyada düzeltilecek metin yok).

**G2 (DÜŞÜK) Yön adlarının ekrandaki karşılığı geç söyleniyor.** Bölüm 1 ipucu 1 "doğu/kuzey" diyor; doğu = sağ, kuzey = yukarı bilgisi ilk ipucunda yok (sözlükte var). Ekranda pusula görünüyorsa sorun hafif; emin değilim.

## Bölüm 1 — İlk adımlar

Sayım: robot sütun 0 satır 3, hedef sütun 3 satır 2 → 3 doğu, 1 kuzey. İpucu 1 doğru, çözüm doğru, ipuçları kademeli. Öğretim tek şey (move + yön).

1. (DÜŞÜK) `ipuclari[0]`, yön karşılığı yok (G2). Önerilen: "Hedef kare robotun 3 kare doğusunda (sağında), 1 kare kuzeyinde (yukarısında)."
2. (DÜŞÜK) `bitis` "BKM-7 seni dinliyor." Robotun adı bu bölümde hiç verilmiyor (`giris` "Robotu" diyor). Senaryodaki açılış sahnesi ("Robot bulundu: BKM-7") oyunda varsa sorun yok; emin değilim. Yoksa `giris`: "Hoş geldin! Robotu (BKM-7) hedef kareye götür."
3. (DÜŞÜK) `tipik_hatalar` içinde ilk bölümün en olası ikinci hatası eksik: parantezsiz yazım. Eklenecek: kod `["move East"]`, açıklama "Yön parantezin içine yazılır: move(East)."

## Bölüm 2 — Kaya engeli

Sayım: enerji hücreleri (sütun 2, satır 3) ve (sütun 3, satır 2); kaya (3,3) ve (4,4). Çözüm doğru; üç tipik hata da koda uyuyor (ilki gerçekten kayaya çarpar, ikincisi ilk hücreyi toplamadan geçer, üçüncüsü parantezsiz).

1. (ORTA) Aynı bölümde iki yeni şey: `collect` komutu ve kayadan dolaşma. `giris` ("Etrafa saçılanları topla.") komut adını söylemiyor; `collect()` ilk kez ipucu 2'de geçiyor. Acemi paletinde düğme zaten görünüyorsa hafifler (`parcalar` yazılmamış, açık komutlardan çıkar). Önerilen `giris`: "Koloni enerji hücresi istiyor. Etrafa saçılanları collect() komutuyla topla." (Giriş metninde ters tırnak desteği olup olmadığından emin değilim, bu yüzden düz yazıldı.)
2. (DÜŞÜK) `ipuclari[0]` "aralarında bir kaya duruyor": kaya iki hücrenin arasında değil, çapraz iki hücrenin ortak köşesinde. Önerilen: "İki enerji hücresi var ve kaya ikisinin de yanında duruyor. Robot kayanın içinden geçemez."

Metin ve ton temiz.

## Bölüm 3 — Tekrar tekrar

Sayım: 5 hücre yan yana (satır 3, sütun 1-5), robot sütun 0. `range(5)` doğru; `range(4)` bir hücre bırakır; `range(6)` altıncı adımda alan dışına çıkar; girintisiz collect açıklaması doğru. Tipik hatalar acemi hatalarını iyi kapsıyor (iki nokta, girinti, girintisiz collect, sayı).

1. (ORTA) G1, `for` kullanmadan geçilir.
2. (ORTA) `i` hiçbir ipucunda anlatılmıyor (yalnızca sözlük `range` sayfasında); "bu i ne?" diye takılabilir. `ipuclari[1]` için önerilen tam metin: "`for i in range(5):` altındaki girintili (içeriden başlayan) satırları 5 kez tekrarlar. Satırın sonundaki iki noktayı unutma. (`i` tur sayacıdır; şimdilik olduğu gibi yaz.)"
3. (ORTA) Sözlük `for` sayfası bu bölümün düzeyini aşıyor: metin üzerinde `for`, liste üzerinde `for`, iç içe döngü, `print` ve "3 × 4 = 12 tur" örnekleri hep aynı sayfada. İlk kez `for` gören acemiyi bunaltır. Sayfalar bölüm bölüm kademeli açılıyor mu görmedim, emin değilim. Açılmıyorsa: Bölüm 3 için yalnız temel `for` anlatan kısa metin, geri kalanı sonraki bölümlerde eklenmeli.
4. (DÜŞÜK) `baslangic_kodu` (`move(East)`, `collect()`) oyuncuyu "bunu 5 kez kopyala" yoluna iter, G1 kestirmesini besler. Öneri: boş bırakmak ya da yarım döngü: `["for i in range(3):", "    move(East)"]`.
5. (DÜŞÜK) `bitis` "Döngüleri kaptın!" çoğul; tek döngü öğretildi. Önerilen: "Döngüyü kaptın!" (senaryo belgesi de aynı satırı taşıyor, birlikte güncellenmeli).

## Bölüm 4 — İki yönlü

Sayım: üst satırda 4 hücre (sütun 1-4), sütun 4'te aşağı 3 hücre (satır 1-3); toplam 7, görev doğru. Çözüm `range(4)` doğu + `range(3)` güney doğru. Dört tipik hata da koda uyuyor.

1. (ORTA) G1.
2. (ORTA) Köşedeki hücreyi iki kez sayma tuzağı: `ipuclari[1]` "Her sırada kaç hücre olduğunu say" diyor, acemi güney sırasını 4 sanabilir. İkinci döngüde `range(4)` yine görevi tamamlıyor (robot bir satır fazla iniyor, boş kareye `collect()` etkisiz), yani yanlış sayım fark edilmeden geçer. Önerilen `ipuclari[1]`: "Bir döngü bitince altına girintisiz yeni bir `for` yazabilirsin; Python önce birincisini, sonra ikincisini tekrarlar. Her sırada kaç hücre olduğunu say; köşedeki hücreyi yalnızca bir kez say."
3. (DÜŞÜK) `baslangic_kodu` birinci döngüyü doğru sayıyla hazır veriyor (ipucu 3'le aynı bilgi). Zararsız; yalnız bilgi.

Metin temiz. Öğretim: tek yeni şey (art arda iki döngü).

## Bölüm 5 — Küçük sınav

Sayım: alt satırda 2 hücre (sütun 1-2) ve kaya sütun 3; bir satır yukarıda 3 hücre (sütun 3-5); hedef sütun 5 bir satır daha yukarıda. Toplam 5 hücre, görev doğru. Çözüm robotu hedefte bırakıyor, doğru. Tipik hatalar (hedefe varmama, kayaya çarpma, range(2) üst sıra) koda uyuyor.

1. (ORTA) Tek ipucu var (biçim kılavuzu "ideali 3"). Sınav olsa da takılan oyuncuya ikinci/üçüncü basamak yok; öteki bölümlerde 3'er ipucu varken yardım aniden kesiliyor. Önerilen eklemeler:
   - `ipuclari[1]`: "Alt sırada kayaya kadar 2 hücre var. Kayaya çarpmadan önce `move(North)` ile bir sıra yukarı çık; üst sırada 3 hücre var. Hedef kare bir sıra daha yukarıda."
   - `ipuclari[2]`: "`for i in range(2):` ile başla (içine `move(East)` ve `collect()`), sonra `move(North)`; üst sıra için `range(3)`; en sonda yine `move(North)`."
2. (DÜŞÜK) `ipuclari[0]` "kayanın etrafından bir adım": kayanın etrafından dolaşılmıyor, bir sıra yukarı çıkılıyor. Önerilen: "Yolu parçalara böl: kayaya kadar bir sıra hücre, bir sıra yukarı çıkış, sonra üst sırada ikinci sıra hücre ve en sonda hedef."
3. (ORTA) G1, sınavda da döngü zorunlu değil.

## Bölüm 5 → 6 geçişi

Bölüm 6'nın (Atlamalı) zorluğu Bölüm 5'ten düşük; sıçrama yok. `giris` "Döngünün içine birden çok komut koyabilirsin" Bölüm 3-5'te zaten yapılanı (döngüde move + collect) yeni gibi sunuyor; asıl yeni şey "aynı komutu iki kez". (DÜŞÜK, Bölüm 6 denetiminde ele alınsın.)

## Sözlük

`move`, Yönler, `collect`, `range` sayfaları bölümlerle çelişmiyor. `for` sayfası için Bölüm 3 madde 3. `collect` sayfasındaki "karede toplanacak bir şey yoksa hiçbir şey olmaz" Bölüm 1-5 için doğru; Bölüm 21+ (sağlam panel) ve 91+ (kızgın göktaşı) gibi yerlerde `collect()` robotu durduruyor, oralarda sayfa eksik kalır (bu partinin dışı, bilgi).

## Zorluk eğrisi

1-2 kolay, asıl sıçrama Bölüm 3 (for + range + girinti birlikte), 4 ve 5 dengeli.
