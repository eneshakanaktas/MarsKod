# Sezon 1 denetimi — Bölüm 16-20

Yöntem: yalnızca dosya okuma (bolum-15..21.json, sinav-20.json, bolum-dosyasi.md, senaryo-bolge-02.md, sozluk.json `if`/`ice_here`/`else`/`rock_ahead` sayfaları, World.cs `Collect()`/`RockAhead()`/`Move()`, ProgramRun.cs bitiş denetimi). Haritalar elle sayıldı (satır 0 = en üst/kuzey; (sütun, satır)); çözümler ve her tipik hata kodu kare kare izlendi. Çalıştırma yapılmadı.
Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: bulmacalar, sayımlar ve çözümler tutarlı; her çözüm görevi bitiriyor, her tipik hata açıklaması gerçekten olanı anlatıyor, ipuçları 3'er adet, YÜKSEK bulgu yok. Asıl sorunlar: (1) yeni gelen `else` + `rock_ahead` ilk iki bölümünde (16, 17) zorunlu değil; (2) başlangıç kodları çözümün neredeyse tamamını veriyor (18, 19, 20) ve 20'de "tur sayısını say" ipucunu yalancı çıkarıyor; (3) Bölüm 18 ipucunda çelişkili "girintisiz collect()" ifadesi.

## Genel

**G1 (ORTA) `else` ve `rock_ahead` ilk iki bölümünde (16, 17) zorunlu değil; ezber yürüyüşle geçiliyor.**
Bu partide yeni gelen özellikler: `rock_ahead(yön)` (komut, B16), `else` (B16). `for`, `if`, `ice_here`, hedef kare daha önce geldi.
- B16 `if`/`else`/`rock_ahead` olmadan: `move(North)`, `collect()`, `move(East)`, `move(East)`, `collect()` → görev biter.
- B17 olmadan: `move(North)`, `collect()`, `move(East)`, `move(North)`, `collect()` → görev biter.
Öneri (zorunlu yapı listesine): B16 ve B17 → `else` (ve `rock_ahead` çağrısı). Yani "kodda en az bir `if ... else` olmalı ve `rock_ahead` çağrılmalı". B18-B20'de ekleme gerekmez (üçüncü ve sonrası serbest). Hata mesajı önerisi: "Kod bitti, else yok" + "Görev tamam, ama kayaya bakmadan ezberden yürüdün. Bu bölümde `if rock_ahead(East): ... else: ...` ile dene." Şart eklenince B16 ve B17 `tipik_hatalar`'a "komutları alt alta yazma" örneği konur: B16 için `["move(North)", "collect()", "move(East)", "move(East)", "collect()"]` ("Harita hep aynı olduğu için bu da çalışır, ama bu bölümde kayaya bakarak karar vermeyi öğreniyoruz."), B17 için ilgili 5 satırlık ezber çözüm.

**G2 (bilgi) Koşul (`if`/`else`) gerçekten gerekli mi?** Hiçbir bölümde gerekli değil: harita her denemede aynı, oyuncu ezber yürür. Tek istisna denemek gerekirse bile değil; 16, 17, 18, 19, 20'nin hepsi sensörsüz, komutları alt alta yazarak geçiliyor (19'da kristalde `collect()` yazmamak, 20'de 7 satırlık sabit yol yeterli). Yani koşulun tek koruması G1'deki zorunlu yapı listesi. Bundan sonrası için (B21+) aynı durum geçerli olabilir; bu parti dışı.

**G3 (DÜŞÜK) Bilgi kartı (`bilgi`) yok.** B16 iki yeni şey getiriyor (`else`, `rock_ahead`); kılavuz "yeni komut gelince `bilgi` yazılır" diyor. Giriş satırı ve ipuçları açıklıyor, bu yüzden sorun değil. Emin değilim: erken bölümlerde kart yok (B12 de yok), Enes'le konuşulacaklar listesine koy.

**G4 (DÜŞÜK) Senaryo belgesi eskimiş.** `senaryo-bolge-02.md` 17-18 bitişlerini hâlâ "Tank doluyor: %30 / %60" diye yazıyor (satırlar ~79 ve ~85); oyundaki dosyalar "Tank dolu! Su yolu açık." (17) ve "Sera için su yolda." (18) diyor, tank toplanan buz oranında dolduğu için 17'de tamamen doluyor (Traces.cs). Oyun tutarlı; belge güncellenmeli.

**G5 (bilgi) Zorluk eğrisi.** B15 (for + if, 4 satır) → B16 yeni kavram ama `for` yok, tek if/else, kolay → B17 aynı kalıp iki kez, kolay → B18 `for` + if/else birleşir, orta → B19 iç içe üç kat (for > else > if), ilk kez dört girintiye yakın, zor sıçrama → B20 aynı kod yalnız `range(5)` yerine `range(7)` ve hedef: sıçrama çok küçük, bölge finali kendi başına yeni bir şey sormuyor.

## Bölüm 16 — Çatal yol

Sayım: buzlar (0,4) ve (2,4) = 2 ✓ ("2 buz topla"). Robot (0,5), kaya (1,5). Çözüm: `rock_ahead(East)` True → kuzey (0,4) → `collect` (1) → doğu, doğu → (2,4) → `collect` (2) ✓. Tipik hatalar: #1 doğuya yürümek kayaya çarpar ✓; #2 `if` içi ters (kaya varken doğuya) → çarpar ✓; #3 `collect` yok → buz kalır ✓. `parcalar` çözümü kuruyor (`else:`, `if rock_ahead(East):`, dört yön, `collect()`) ✓. Başlangıç kodu yardımcı, bitirmiyor ✓.

Yeni şeyler: `else` + `rock_ahead(yön)` birden; ikisi de gerçekten ilk kez geliyor (B15'in `komutlar`'ında yok, sözlük sayfaları ayrı). Sıçrama kabul edilebilir: B15'in iç içe `for`+`if`'inden sonra bu bölümde `for` yok, 5 satırlık tek dallanma. `python_kelimeleri: ["else"]` ✓.

1. (ORTA) Zorunlu değil, bkz. G1. Koşul gerekli mi: hayır, ezberle geçilir.
2. (ORTA) `ipuclari[2]` karışık ve eksik: "Her iki dalın altına da (girintisiz) `collect()` ve yola devam et." `collect()` dalların altına değil, `if/else` bittikten sonra yazılıyor; "yola devam et" kaç adım olduğunu söylemiyor. Önerilen `ipuclari[2]`: "`if rock_ahead(East):` altına girintili `move(North)`, girintisiz `else:` altına girintili `move(East)`. `if/else` bittikten sonra, girintisiz `collect()` yaz. Sonra iki kez `move(East)` ve yine `collect()`."
3. (DÜŞÜK) `bitis` "İki yoldan doğrusunu seçtin!" iddiası: oyuncu ezberle geçtiyse seçim yapmadı. G1 uygulanırsa doğru olur; uygulanmazsa değiştir: "Kayaya bakıp yolunu seçtin!".
4. (DÜŞÜK) `tipik_hatalar[2]` (collect yok) kodu ikinci buza hiç gitmiyor da, açıklama yalnızca collect'ten söz ediyor. Doğru ama kısa; olası düzeltme yok, not olarak.

## Bölüm 17 — İki çatal

Sayım: buzlar (1,3) ve (0,4) = 2 ✓. Kayalar (2,4) ve (1,5). Çözüm: (0,5)'te kaya var → kuzey (0,4), `collect` (1) → doğu (1,4) → `rock_ahead(East)` (2,4) kaya True → kuzey (1,3), `collect` (2) ✓. Tipik hatalar: #1 ikinci çatala bakmadan doğu, doğu → (2,4) kayaya çarpar ✓; #2 son `collect()` yok → (1,3) toplanmaz ✓. Bitiş "Tank dolu!" tankla uyumlu (tank oranla doluyor). `parcalar` ✓. Başlangıç kodu ilk çatalın tamamı (yardımcı, bitirmiyor).

1. (ORTA) Zorunlu değil, bkz. G1. Koşul gerekli mi: hayır, ezberle geçilir.
2. (DÜŞÜK) `giris`: "Buzları tanka taşı." Robot taşımıyor, yalnızca topluyor; "taşı" yanıltıcı. Önerilen `giris`: "Aracın su tankı bulundu! Buzları topla, tankı doldur."
3. (DÜŞÜK) En olası acemi hatası eksik: iki bloğun arasındaki `move(East)` unutulması. Kod `["if rock_ahead(East):", "    move(North)", "else:", "    move(East)", "collect()", "if rock_ahead(East):", "    move(North)", "else:", "    move(East)", "collect()"]` izlendi: robot (0,4)'te kalır, doğusu boş, doğuya yürür, (1,4)'te `collect` boşa gider, ikinci buz kalır. Önerilen `tipik_hatalar` ekle, `aciklama`: "İki çatal arasında `move(East)` yok: robot hâlâ ilk sütunda, ikinci kayayı göremiyor ve kuzeye çıkmadan doğuya yürüyor. Önce ikinci kayaya yaklaş."
4. (DÜŞÜK) `ipuclari[1]` ("Aynı kalıbı iki kez yaz... aralarında `move(East)`") çözümün iskeletini ikinci basamakta veriyor; kademe biraz erken. Emin değilim, bölüm kolay olduğu için kabul edilebilir.

## Bölüm 18 — Köşeyi dön

Sayım: buzlar (1,1), (1,2), (1,3), (0,4) = 4 ✓ ("4 buz topla"). Kaya (0,3). Çözüm 5 tur: T1 kuzey (0,4) topla (1); T2 kaya var → doğu (1,4), `collect` boşa (hata vermez, `Collect()` buz yoksa yalnız False döner); T3 (1,3) topla (2); T4 (1,2) (3); T5 (1,1) (4) ✓; `range(5)` tam gerekli. `parcalar` `range(3)` ✓. Tipik hatalar: #1 hep kuzey → T2'de kayaya çarpar ✓; #3 `collect` yok ✓; #2 (aşağıda).

Yeni şey yok (`for` + `if/else` + `rock_ahead` birleşimi). Koşul gerekli mi: hayır, ezberle geçilir.

1. (ORTA) `ipuclari[2]` çelişkili: "en altta (if/else'in dışında, for'un içinde) girintisiz `collect()`". `collect()` `for`'un içinde olduğu için tek girinti ister; "girintisiz" harfiyen yapılırsa `collect()` döngünün dışında kalır, yalnızca bir kez çalışır ve görev bitmez. Önerilen `ipuclari[2]`: "`for i in range(5):` altına girintili `if rock_ahead(North):` / `    move(East)` / `else:` / `    move(North)`; en altta, `if` ile aynı hizada (döngünün içinde, `if/else`'in dışında) `collect()`."
2. (ORTA) `tipik_hatalar[1]` acemi paletinde kurulamaz: `move(South)` `parcalar`'da yok (palet `move(North)`, `move(East)` veriyor). Açıklama doğru (T1'de güneye çıkamaz, alan sınırı) ama paletle çalışan oyuncu bu hatayı hiç yapamaz. Daha olası hata: dalların ters yazılması. Kod `["for i in range(5):", "    if rock_ahead(North):", "        move(North)", "    else:", "        move(East)", "    collect()"]` izlendi: T1 kaya yok → doğu (1,5), kaya yok yine doğu... robot hep doğuya gider, (5,5)'e varır, hiçbir buz toplanmaz. Önerilen yeni örnek, `aciklama`: "Yönler ters: kaya yokken doğuya, kaya varken kuzeye yazılmış. Robot hiç kayaya gelmeden doğuya gider ve buzlara uğramaz." (Eski güney örneği silinebilir ya da kalabilir.)
3. (ORTA) Başlangıç kodu çözümün `collect()` hariç tamamı; çalıştırınca doğrudan `tipik_hatalar[2]`'deki hataya düşer ve bulmacanın düşünülen kısmı (kayayı hangi yönde dolaşmak, kaç tur) hazır geliyor. Önerilen `baslangic_kodu`: `["for i in range(5):", "    move(North)"]` (B15'in kalıbıyla aynı, dallanmayı oyuncu kurar).
4. (DÜŞÜK) `ipuclari[0]` "onu bir kare doğudan dolaşıp devam etmen gerekiyor": robot sonra sütununa geri dönmüyor (sol sütuna dönmez, 1. sütundan çıkar). "Dolaşıp" geri dönmeyi çağrıştırıyor; `ipuclari[0]` önerisi: "Kuzeye giderken yolda bir kaya var; bir kare doğuya kayıp oradan devam etmen gerekiyor."
5. (DÜŞÜK) `range(4)` hatası eksik (son buz kalır): `["for i in range(4):", "    if rock_ahead(North):", "        move(East)", "    else:", "        move(North)", "    collect()"]` → (1,1) toplanmaz ✓. Önerilen `aciklama`: "range(4) bir tur eksik: kayayı dolaşmak da bir tur harcıyor, en üstteki buz kalıyor." (19'da benzeri var.)
6. (DÜŞÜK) `giris` amacı söylemiyor ("Kayayı dolaş, yoluna devam et"); 17'de tank vardı, burada neden toplandığı yok. Senaryo (araç kapısı açık) yalnızca sahnede. Kabul edilebilir, not.

## Bölüm 19 — İki duyu bir arada

Sayım: buzlar (1,1), (1,3), (0,4) = 3 ✓. Kristal (1,2), kaya (0,3). Çözüm 5 tur: T1 (0,4) topla; T2 kaya → doğu (1,4); T3 (1,3) topla; T4 kristale (1,2) çıkar, `ice_here` False; T5 (1,1) topla ✓. Tipik hatalar: #1 koruma yok, T4'te `collect()` kristalde robotu durdurur ✓; #2 `range(4)` (1,1) kalır ✓. `parcalar` ✓. Yeni şey yok (B15+B18 birleşimi, ilk kez `else` içinde `if`).

1. (ORTA) Başlangıç kodu çözümün `collect()` hariç tamamı (yalnız `if ice_here():` gövdesiz kalmış). Bölümün asıl işi (iki sensörü birleştirmek ve içe içe girintiyi kurmak) hazır geliyor, gövdesiz `if` da çalıştırılınca girinti hatası verir. Önerilen `baslangic_kodu`: `["for i in range(5):", "    if rock_ahead(North):", "        move(East)"]` (20'nin mevcut başlangıcıyla aynı biçim).
2. (DÜŞÜK) `ipuclari[2]` kodu tek satırda yazıyor (`if rock_ahead(North): move(East) else: move(North)`); bu geçerli Python değil ve girinti bu bölümün asıl konusu. Önerilen: B18 gibi " / " ile ayrılmış satırlar yaz: "`for i in range(5):` altına girintili `if rock_ahead(North):` / `    move(East)` / `else:` / `    move(North)`, ve onun altında bir girinti daha içeride `if ice_here():` / `    collect()`".
3. (DÜŞÜK) Olası acemi hatası eksik: `ice_here()`'e hamleden önce bakmak. Kod `["for i in range(5):", "    if ice_here():", "        collect()", "    if rock_ahead(North):", "        move(East)", "    else:", "        move(North)"]` izlendi: son turda yeni kareye bakılmıyor, (1,1) toplanmaz (2 buz). Önerilen `aciklama`: "Önce bakıp sonra yürüyorsun: son turda vardığın karede bakmadan döngü bitiyor. Önce yürü, sonra bak."

## Bölüm 20 — Su yolu (bölge finali)

Sayım: buzlar (2,1), (1,3), (0,4) = 3 ✓; hedef H (2,0); kayalar (1,1), (0,3); kristal (1,2). Çözüm 7 tur: T1 (0,4) topla; T2 kaya → doğu (1,4); T3 (1,3) topla; T4 kristal (1,2) geç; T5 kaya (1,1) → doğu (2,2); T6 (2,1) topla; T7 (2,0) hedefte bitiyor ✓ (5 kuzey + 2 doğu = 7, `range(7)` tam). Görev bitişi program sonunda denetleniyor (ProgramRun.cs), robot hedefte durmalı. Tipik hatalar: #1 `range(5)` (2,2)'de kalır, 2 buz ✓; #2 koruma yok, T4 kristalde durur ✓. `parcalar` `range(3)` ✓.

Sınav: sinav-20.json 3 soru, cevaplar doğru (Q1: şık 1 ✓, Q2: şık 0 ✓, Q3: şık 1 ✓). Sözlük `else`/`rock_ahead` sayfalarıyla çelişki yok.

Yeni şey yok. Koşul gerekli mi: hayır, 7 satırlık sabit yol geçer.

1. (ORTA) Başlangıç kodu `range(7)` yazıyor ve dallanmanın yarısını veriyor; `ipuclari[1]` ise "Kaç tur gerektiğini say" diyor. Sayı zaten verilmiş, ipucu ile bulmaca çelişiyor ve `tipik_hatalar[0]` (`range(5)`) başlangıç koduyla hiç yapılamaz. Önerilen `baslangic_kodu`: `["for i in range(3):", "    if rock_ahead(North):", "        move(East)"]` (paletteki sayıyla aynı, `range(3)`).
2. (ORTA) En olası fazla-tur hatası eksik: `range(8)`. İzlendi: yedinci turda robot (2,0) hedefte; sekizinci turda önünde alanın sınırı var, `rock_ahead(North)` True döner, robot doğuya kayıp (3,0)'da bitirir, hedefte değil → görev bitmez. "Fazlası zarar vermez" diye düşünen oyuncuyu yanıltır. Önerilen `tipik_hatalar` ekle: kod `range(8)` ile çözüm; `aciklama`: "range(8) bir tur fazla: yedinci turda hedefe varıyorsun, sekizincide önünde alanın sınırı var (rock_ahead True), doğuya kayıp hedefin dışında kalıyorsun. Tur sayısı tam yetmeli."
3. (DÜŞÜK) `ipuclari[2]` B19 gibi tek satır kod (`if rock_ahead(North): move(East) else: move(North)`), geçersiz Python; " / " ile ayrılmış satırlara çevir (B19 madde 2'deki metin, `range(7)` ile).
4. (DÜŞÜK) `sinav-20` Q3 sorusu "if'in içinde başka bir if" diyor ama Bölüm 19-20'deki iç içe `if`, `else` dalının içinde, `if`'in değil. Açıklamadaki "(Bölüm 19'daki gibi)" tam tutmuyor. Önerilen `soru`: "Bir for döngüsünün içindeki if/else'in else bölümüne başka bir if yazılabilir mi?"; şıklar aynı kalabilir ("Hayır, ... yalnızca bir if konabilir" / "Evet; her iç içe geçişte bir girinti daha eklenir" ...). Önerilen `aciklama`: "Evet; else'in içine de if konabilir, her iç katman bir girinti daha içeri yazılır (Bölüm 19 ve 20'deki gibi)."
5. (DÜŞÜK) `giris` "Aracı koloniye hazırla" ve görev (buz topla, hedefe var) ilişkisi bulmaca düzeyinde yok, ama kapanış sahnesi bunu karşılıyor; sorun değil.

## Geçiş sıçramaları

- B15 → B16: `komutlar`'a `rock_ahead` ve palette `else:` ekleniyor, yön argümanı ilk kez; `for` bir bölümlüğüne çıkıyor (iyi, sıçramayı yumuşatır). Tek riskli yer G1 (zorunlu değil) ve G3 (bilgi kartı yok).
- B20 → B21: B21 `panel_power`, `repair`, `<` karşılaştırması, panel 0-9 sayıları ve ilk kez sayı döndüren komut getiriyor; `rock_ahead` komutlar listesinde kalıyor ama kullanılmıyor. Bu sıçrama büyük (tamamen yeni dünya nesnesi + karşılaştırma) ve B20'den sonra bölge değişimi olduğu için kabul edilebilir; bu partinin kapsamı dışı.
