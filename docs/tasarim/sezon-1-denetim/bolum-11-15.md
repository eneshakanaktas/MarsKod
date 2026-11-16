# Sezon 1 denetimi — Bölüm 11-15

Yöntem: yalnızca dosya okuma (bolum-10..16.json, sinav-10/15.json, bolum-dosyasi.md, senaryo-bolge-02.md, sozluk.json `if`/`ice_here`/`collect` sayfaları, World.cs `Collect()`/`IceHere()`, Level.cs bilinen alanlar). Haritalar elle sayıldı (satır 0 = en üst/kuzey); çözümler ve her tipik hata kodu kare kare izlendi. Çalıştırma yapılmadı. `for` sözlük sayfası denetlenmedi (başkası sadeleştiriyor).
Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: bulmacalar ve çözümler tutarlı, sayımlar doğru, ipuçları 3'er adet. Asıl sorun yeni özellik zorunluluğu: `if` ilk iki bölümde (12, 13) zorunlu değil. Ayrıca 14'te yanlış kare sayısı, 15'te yanıltıcı görev yazısı, 15'in en olası acemi hatası eksik.

## Genel

**G1 (ORTA) `if` ilk iki bölümünde (12, 13) zorunlu değil; kestirme geçiyor.**
Ekip kuralına göre `if` 12 ve 13'te zorunlu olmalı. Motorda `if` için şart alanı yok (Level.cs bilinen alanlar: `dongu_sarti`, `parametre_sarti`, `donus_sarti`, rutin alanları; `if` için karşılığı yok). Sonuç:
- B12: `move(East)`, `collect()`, `move(North)`, `move(East)`, `collect()` görevi bitirir (if'siz).
- B13: `move(East)`, `collect()`, `move(East)`, `move(East)`, `collect()` görevi bitirir (if'siz; kristalin üstünden `collect()`siz geçilir). Bölümün "dersi" (kristalde bakmadan toplama) bu yüzden atlatılır.
Öneri: `dongu_sarti` gibi yeni bir alan (örn. `kosul_sarti`, `true` ise kodda en az bir `if` olmalı; yalnızca B12 ve B13'te yazılır). Hata mesajı önerisi: "Kod bitti, if yok" + "Görev tamam, ama bakmadan tek tek yazdın. Bu bölümde `if ice_here():` ile dene." Şart eklenince B12 ve B13 `tipik_hatalar`'a "if'siz alt alta komut" örneği konur (aşağıda her bölümde yazılı). Not: Level.cs şu an başkasında değişiyor (git durumu); çakışmamak için o kişiye sor. B14-B15'te şart gerekmez (üçüncü ve sonrası serbest); gereksiz zorunluluk yok, B11-15 hiçbirinde şart alanı yazılı değil (doğru).

**G2 (DÜŞÜK) Bilgi kartı (`bilgi`) yok.** B12 yeni komut + yeni kelime (`ice_here`, `if`), B13 yeni nesne ve kural (kırmızı kristal: `collect()` robotu durdurur). Şimdilik bilgi kartı yalnızca B72 ve sonrasında var. Giriş satırları ve ipuçları açıklıyor, bu yüzden sorun değil; ama kılavuz "yeni komut gelince `bilgi` yazılır" diyor. Emin değilim: bölgenin erken bölümleri için kartı Enes'le konuşulacaklar listesine koy.

**G3 (DÜŞÜK) Sözlük `collect` sayfasında kırmızı kristal istisnası yok.** Sayfa "Karede toplanacak bir şey yoksa hiçbir şey olmaz" diyor (World.Collect ile uyumlu); kristal uyarısı yalnızca `ice_here` sayfasında. 13'te robot kristalde durunca oyuncu `collect` sayfasına bakarsa bulamaz. Önerilen ek cümle `collect` sayfasının `aciklama`sına: "Kırmızı kristalin üstünde çağırırsan robot durur; önce ice_here() ile bak."

**G4 (bilgi) Zorluk eğrisi:** B10 (3 döngü) → B11 daha kolay (tek döngü, 3 komut) → B12-B13 kolay → B14 orta (for + if) → B15 orta (iç içe girinti) → B16 iki yeni şey birden (`else` + `rock_ahead`). 12-15'te harita sabit olduğu için `if` hiçbir yerde gerçekten gerekmiyor (15 bile komutları alt alta yazarak geçilir); bu yüzden G1'deki zorunluluk 12-13 için tek koruma.

## Bölüm 11 — Merdiven

Sayım: B hücreleri (5,0), (4,1), (3,2), (2,3), (1,4) = 5 ✓ ("5 buz topla"). Robot (0,5), kaya (1,5). Çözüm: kuzey → (0,4), doğu → (1,4) buz; 5 tur ile (5,0)'a kadar 5 buz ✓. Tipik hatalar koda uyuyor: #1 ilk adım doğuya = kayaya çarpar ✓; #2 collect yok ✓; #3 range(4) = 4 buz, (5,0) kalır ✓; #4 collect döngü dışı = yalnızca son kare (5,0) toplanır ✓. Başlangıç kodu yardımcı; `parcalar` `range(3)` (çözüm 5) ✓. Giriş/bitiş senaryoyla aynı.

1. (DÜŞÜK) `ipuclari[1]` "Döngünün içine birden fazla komut yazabilirsin; her turda hepsi sırayla çalışır." bunu B3'ten beri biliyorlar (B10 sınavı da soruyor); yeni bilgi gibi sunuluyor. `konular`'da "döngüde birden çok komut" da tekrar. Önerilen `ipuclari[1]`: "Her turda sıra önemli: robotun hemen sağında kaya var, o yüzden turun ilk adımı kuzey olmalı."
2. (DÜŞÜK) En olası acemi hatası eksik: `collect()`'i `move(East)`'ten önce yazmak (`move(North)`, `collect()`, `move(East)`; hiçbir buz toplanmaz çünkü robot buzdan bir kare önce toplar). Önerilen `tipik_hatalar` ekle: kod `["for i in range(5):", "    move(North)", "    collect()", "    move(East)"]`, `aciklama`: "collect() erken yazıldı: robot buzun bir kare soluna gelip topluyor, orada buz yok. Önce iki adımı at, sonra topla."

Metin ve ton temiz.

## Bölüm 12 — Tarayıcı

Sayım: B (1,5) ve (2,4) = 2 ✓. Çözüm doğru: doğu → buz, topla; kuzey, doğu → buz, topla ✓. Tipik hatalar: #1 girintisiz collect (Python hatası) ✓; #2 ikinci buz doğuda değil (robot (2,5)'te, buz (2,4)'te) ✓; #3 `if` ilk karede, orada buz yok ✓. İpuçları kademeli (yön → kural → kod). `parcalar` ✓ (`if ice_here():` var). `python_kelimeleri: ["if"]` ✓. Giriş/bitiş senaryoyla aynı. Sözlük `if` ve `ice_here` sayfaları anlatımla çelişmiyor.

1. (ORTA) `if` zorunlu değil, kestirme geçiyor (G1). Şart eklenirse önerilen `tipik_hatalar` örneği: kod `["move(East)", "collect()", "move(North)", "move(East)", "collect()"]`, `aciklama`: "Bu sıra görevi bitirir ama bu bölümde `if ice_here():` ile bakmayı öğreniyorsun: her `collect()`'ten önce `if` ile sor."
2. (DÜŞÜK) Eksik acemi hatası: `if` satırında iki nokta (`:`) unutmak ya da `ice_here`'i parantezsiz yazmak (`if ice_here:`; Python'da hata vermeden hep doğru sayılır, mantık hatası). Önerilen `tipik_hatalar` ekle: kod `["move(East)", "if ice_here", "    collect()"]`, `aciklama`: "if satırının sonunda iki nokta yok (ve ice_here() parantezli yazılır); Python bu satırı anlamaz."
3. (DÜŞÜK) Bu bölümde iki yeni şey birden: `ice_here` ve `if`. Çözüm yolu ipuçlarıyla kapalı olduğu için sorun değil; yalnızca not.

## Bölüm 13 — Kırmızı kristal

Sayım: harita alt satır `R B T B . .`: buz (1,5), (3,5), kristal (2,5) ✓. Çözüm doğru (kristalde `ice_here()` False; kristalin üstünden geçmek serbest ✓ World.Collect ile). Tipik hatalar: #1 ikinci `collect()` kristalde durdurur ✓; #2 `if` ile ama robot kristalin üstünde durdu, üçüncü kareye gitmedi ✓. `parcalar` ✓. Başlangıç kodundaki `#` yorumu B9'da öğretilmişti ✓ (B9 ipucu 1). Başlangıç kodu (3×`collect()`) bilerek tuzaklı: oyuncu `if` ve girinti eklemeli; B9 modeliyle tutarlı. Sözlük `ice_here` sayfası ("kırmızı kristal buz değildir; ice_here() False der") bölümle uyumlu.

1. (ORTA) `if` zorunlu değil (G1): kestirme `move(East)`, `collect()`, `move(East)`, `move(East)`, `collect()` görevi bitirir. Önerilen `tipik_hatalar` örneği (şart eklenirse): kod bu beş satır, `aciklama`: "Bu sıra görevi bitirir ama bu bölümün dersi: kristal ya da buz fark etmeksizin her karede `if ice_here():` ile bak."
2. (ORTA) Ton: `bitis` "Aferin, kristale dokunmadın!" öğretmen-çocuk tonu; hedef kitle 13+ ("çocuk diliyle hitap yok"). Senaryo belgesindeki satır da aynı, birlikte değişmeli. Önerilen `bitis`: "Temiz iş, kristale dokunmadın!"
3. (DÜŞÜK) `gorev` "Kristale dokunma" ne toplanacağını söylemiyor (B12'de "2 buz topla"). Önerilen `gorev`: "Yalnız 2 buz topla" (18 harf).
4. (DÜŞÜK) Başlangıç kodu `collect()`'leri çözümde olduğu gibi girintisiz bırakıyor; `if` satırlarını eklemek ve girintiyi vermek iki ayrı iş. Çözüm yolu zaten ipucu 3'te; sorun değil, yalnızca not. İpucu 3 başlangıç koduna açık gönderme yapmıyor: önerilen son cümle: "Başlangıçtaki her `collect()`'in üstüne `if ice_here():` ekle ve `collect()`'i içeri al."

## Bölüm 14 — Önce yürü, sonra bak

Sayım: harita `R . T . B H`: kristal (2,5), buz (4,5), hedef (5,5) ✓ ("1 buz ve hedef"). Çözüm: 4 adım doğu (buz karesinde durur), `if ice_here(): collect()`, bir adım doğu = hedef ✓. Tipik hatalar: #2 hedefte değil ✓; #3 range(5) hedefe varır ama buz toplanmaz ✓. `parcalar` `range(3)` (çözüm 4) ✓. `for`/`if` şartı yok (üçüncü ve sonrası serbest); `for`'suz `move(East)`×4, `collect()`, `move(East)` de geçer, kuralın izin verdiği durum, bulgu değil.

1. (ORTA) `tipik_hatalar[0].aciklama` kare sayısı yanlış: kod `for i in range(4): move(East); collect()` iken robot ilk turda (1,5)'e gelir ve `collect()` boş karede zararsız; kristal 2. turda (2,5)'tedir, "üçüncü karede" değil (B13 #2 adımları sayıyor, burada sayım tutarsız). Önerilen `aciklama`: "İlk tur zararsız (boş karede collect() bir şey yapmaz). İkinci turda robot kristalin üstüne gelir, collect() onu toplamaya çalışır ve robot durur."
2. (DÜŞÜK) `ipuclari[0]` yönü göstermekle kalmıyor, planın tamamını veriyor ("yürüt... bak, topla ve hedefe geç"). Önerilen `ipuclari[0]`: "Hedef buzun hemen ötesinde. Önce buzun üstüne kadar yürü; yolda kristal var ama üstünden geçmek serbest."
3. (DÜŞÜK) `ipuclari[2]` başlangıç kodunun (`for i in range(4):` + `move(East)`) zaten verdiği ilk iki satırı tekrar ediyor; yeni bilgi yalnızca son üç satır. Önerilen: "Başlangıç kodundaki döngüden sonra, girintisiz `if ice_here():` ve altında `collect()` yaz; en sonda hedefe bir `move(East)` daha."

Metin temiz. Giriş/bitiş senaryoyla aynı.

## Bölüm 15 — Karışık sıra

Sayım: sütun 0, kuzeye doğru: (0,4) B, (0,3) T, (0,2) B, (0,1) B, (0,0) T = 3 buz, 2 kristal ✓. Robot (0,5). Çözüm: 5 tur, her turda bir adım kuzey + `if` ile topla: (0,4), (0,2), (0,1) ✓, son kare kristal ama `collect()` çağrılmaz ✓. Tipik hatalar: #1 ikinci karede kristal, robot durur ✓ ("ikinci kare" = 2. adım, tutarlı); #2 `if` döngü dışında: robot (0,0) kristalinde tek bakar ✓; #3 range(3): (0,4) ve (0,2) toplanır, (0,1) kalır, "en üstteki buz kalır" ✓. `parcalar` `range(3)` (çözüm 5) ✓. Başlangıç kodu yardımcı. Giriş/bitiş senaryoyla aynı. Sözlük `if` sayfası ("her karede bakmak istersen for'un içine koy") bölümle uyumlu.

1. (ORTA) `gorev` "3 buz, 2 kristal" belirsiz: oyuncu "3 buz ve 2 kristal topla" diye okuyabilir; oysa kristal toplanırsa robot durur. Önerilen `gorev`: "3 buz topla" (senaryo belgesi de güncellenmeli).
2. (ORTA) En olası acemi hatası eksik: ikinci girinti yeni şey olduğu için `collect()`'i `if` ile aynı hizada bırakmak. Önerilen `tipik_hatalar` ekle: kod `["for i in range(5):", "    move(North)", "    if ice_here():", "    collect()"]`, `aciklama`: "collect() if ile aynı hizada: if'in içi boş kaldı, Python hata verir. collect()'i 8 boşluk içeri al (if'in bir girinti içinde)."
3. (DÜŞÜK) `tipik_hatalar[0].aciklama` "Önce bakmak gerek." mutlak iddia: bakmadan, komutları tek tek yazarak da geçilir. Önerilen: "Her karede toplamaya çalışmak ikinci karedeki kristalde robotu durdurur; toplamadan önce `if ice_here():` ile sor."
4. (DÜŞÜK) `ipuclari[0]` yön göstermiyor ("yürüyüp bakmalı"): ilk ipucu yalnızca yönü söylemeli. Önerilen: "Buzlar ve kristaller robotun üstündeki sütunda; kuzeye doğru yürüyeceksin."

### Sınav (sinav-15.json)
3 soru, doğru şıklar soruyla örtüşüyor, yanlış şıklar makul, açıklamalar doğru.
- (ORTA) Bölüm 14-15'in asıl konusu (`if`'in `for`'un içinde mi dışında mı olduğu) sorulmuyor; sınav yalnızca B12-13'ü ölçüyor. Önerilen 4. soru:
  `{ "soru": "`if ice_here():` satırı `for` döngüsünün içine (girintili) yazılırsa ne olur?", "secenekler": ["Her turda bir kez çalışır: robot her yeni karede yeniden bakar", "Yalnızca bir kez, en başta çalışır", "Hiç çalışmaz", "Döngü bir tur eksik döner"], "dogru": 0, "aciklama": "Döngünün içindeki satırlar her turda çalışır; `if` içerideyse robot her karede yeniden karar verir. Dışında olsaydı yalnızca bir kez, döngü bitince bakardı." }`
- (DÜŞÜK) Üç sorunun da doğru şıkkı en uzun şık (ipucu verir). Önerilen: yanlış şıkları biraz uzat (örn. Soru 1 şık 2: "Buzu toplar ve sayacı bir artırır"; Soru 3 şık 1: "Kristal toplanır ve robot yoluna devam eder").
