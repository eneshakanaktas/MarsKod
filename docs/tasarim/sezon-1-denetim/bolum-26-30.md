# Sezon 1 denetimi — Bölüm 26-30

Yöntem: dosya okuma (bolum-26..30.json, sinav-30.json, senaryo-bolge-03.md, World.cs kuralları). Kare yazımı (sütun, y); y = 0 robotun bulunduğu alt sıra, yukarı doğru artar. Haritalar elle sayıldı; çözümler ve her tipik hata kare kare izlendi. Çalıştırma yapılmadı (oyunda `-izle 26 30` ile denenecek).
Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: bulmacalar, sayımlar ve çözümler tutarlı; her çözüm görevi bitiriyor, her tipik hata açıklaması gerçekten olanı anlatıyor, ipuçları 3'er adet, bitiş metinleri senaryo belgesiyle ve sol sayılarıyla uyumlu (1.070-1.072). **YÜKSEK bulgu yok.** Konular: (1) Bölüm 30'un başlangıç kodu tur sayısını (`range(7)`) veriyor, ipucu "say" diyor; (2) `elif` 26'da hâlâ zorunlu değil (21-25 raporu G1); (3) ezberle geçiş (21-25 raporu G2).

## Genel

**G1 (ORTA) Bölüm 30: başlangıç kodu cevabı veriyor, ipucu yalan söylüyor.** `baslangic_kodu` `for i in range(7):` ile başlıyor; 2. ipucu "Kaç tur gerektiğini say" diyor. Oyuncu sayacak bir şey bırakılmamış; ayrıca doğru tur sayısı (5 kuzey + 2 doğu = 7) hiçbir bölümde oyuncuya bulduruluyor. Bölüm 20'de bunu `range(3)` ile düzelttik (aynı bulgu). Öneri: 30'un başlangıç kodunda `range(3)` yaz; tipik hata `range(6)` zaten var.

**G2 (ORTA) `elif` hâlâ zorunlu değil (26).** Yeni özellik kuralına göre 24 ve 26'da zorunlu olmalı (21-25 raporu G1). 26 `elif` olmadan da çözülür (ikinci `if` + `else` başka biçimde, ya da koşulsuz yürüyüş).

**G3 (ORTA) Ezber:** 26-30'un hepsi komutlar alt alta yazılarak geçilir (harita sabit). Bkz. 21-25 raporu G2. Bölge 3 sınavı için değişen alan düşünülebilir (ayrı karar; önce 15 ve 20 oyunda hissedilsin).

## Bölüm 26 — Dönüş
Robot (0,0). Parçalar (2,0), (4,1), (0,1) = 3 ✓. Çözüm: `i<4` doğu ×4 → (4,0); `i==4` kuzey → (4,1); `i>4` batı ×4 → (0,1); her turun sonunda `collect()` → üç parça ✓, yolda çalışan panel yok (80, 70, 90 yolda değil) ✓. Tipik hatalar: #1 ikinci `if` + `else`: ilk dört turda doğu-batı gidip gelir, yerinde sayar; i=4'te kuzey, i=5'te batıda alanın dışı → durur ✓ (açıklama doğru); #2 `i==5`: bir tur geç, köşe kayar, (4,1) kalır ✓; #3 `range(8)` son parça kalır ✓. Başlangıç kodu yardımcı, bitirmiyor ✓.
1. (ORTA) G2.

## Bölüm 27 — Köşede ölç
Robot (0,0). Yol: doğu ×3, kuzey ×3. Paneller yolda: (1,0)=80 sağlam, (2,0)=20 onar, (3,0)=0 topla, (3,1) boş, (3,2)=30 onar, (3,3)=0 topla → 2 onar, 2 topla ✓. Boş karede güç 0 olduğu için `collect()` zararsız çalışır ✓. Tipik hatalar: #1 ilk panel 80 → "Çalışan panel sökülmez" ✓; #2 kontrol döngü dışında → yalnız son kare ✓ (görev bitmez); #3 ters sıra → (3,0)'da "Kırık panel onarılmaz" ✓. İpuçları ✓.
Sorun yok.

## Bölüm 28 — Kayayı dolaş
Robot (0,0), kaya (0,3) ve (3,3). Yol: (0,1)=0 topla, (0,2)=30 onar, kaya → doğu (1,2)=70 geç, (1,3)=0 topla, (1,4) boş, (1,5)=20 onar → 2 onar, 2 topla ✓, `range(6)` ✓. Öteki kaya (3,3) ve paneller (60, 80) yolda değil, süs. Tipik hatalar: #1 hep kuzey → 3. turda kayaya çarpar ✓; #2 `range(5)` en kuzeydeki çatlak kalır ✓; #3 iki `if` → toplanınca karede panel kalmaz, "Burada panel yok" ✓. Bitiş metni senaryodaki oyuncak robot satırı ✓.
Sorun yok.

## Bölüm 29 — Geri dönüş
Robot (0,0), kaya (5,0) (robotun yolunda değil: en doğu (4,0)). Yol: doğu ×4, kuzey, batı ×4. Paneller: (1,0)=60 geç, (2,0)=0 topla, (3,0)=10 onar, (4,0)=90 geç, (4,1)=0 topla, (3,1)=40 onar, (2,1) boş, (1,1)=80 geç, (0,1)=20 onar → 3 onar, 2 topla ✓. Tipik hatalar: #1 `i>5`: 5. turda `else` doğuya götürür, yol kayar, (0,1) kalır ✓ (açıklama doğru); #2 `range(8)` ✓; #3 ters sıra → (2,0)'da "Kırık panel onarılmaz" ✓.
1. (DÜŞÜK) İpucu 1 "bu sefer her karede panel var": (2,1) boş. "Çoğu karede" daha doğru.

## Bölüm 30 — Gün doğumu
Robot (0,0), hedef (2,5), kayalar (0,2) ve (1,4). Çözüm yolu: (0,1)=30 onar; kaya → doğu (1,1)=0 topla; (1,2)=80 geç; (1,3)=10 onar; kaya → doğu (2,3)=20 onar; (2,4)=0 topla; (2,5) hedef (boş, `collect()` zararsız) → 3 onar (30, 10, 20), 2 topla, hedefte ✓, 7 tur ✓ (5 kuzey + 2 doğu). Tipik hatalar: #1 `range(6)` hedefin bir kare gerisinde ✓; #2 korumasız `collect()` ilk panelde (gücü 30) "Çalışan panel sökülmez" ✓; #3 ters sıra → (1,1)'de "Kırık panel onarılmaz" ✓.
1. (ORTA) G1: başlangıç kodu `range(7)` veriyor.

## sinav-30
3 soru, cevaplar doğru (Q1 şık 1 ✓, Q2 şık 1 ✓, Q3 şık 1 ✓). `if/elif/else`, `elif` ile ikinci `if` farkı, `>` ile eşitlik: Bölge 3'ün dersleriyle örtüşüyor ✓.

## Enes'e sorular
1. Bölüm 30'un başlangıç kodu `range(3)` olsun mu? (Öneri: evet.)
2. 29'un 1. ipucunda "her karede" yerine "çoğu karede" yazılsın mı? (Öneri: evet.)
3. 21-25 raporundaki üç soru (elif zorunlu, bilgi kartları, panel komutları) bu rapor için de geçerli.
