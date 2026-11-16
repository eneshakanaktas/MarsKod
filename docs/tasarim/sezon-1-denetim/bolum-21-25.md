# Sezon 1 denetimi — Bölüm 21-25

Yöntem: dosya okuma (bolum-21..25.json, sinav-25.json, senaryo-bolge-03.md, World.cs `Collect()`/`Repair()`/`PanelPower()`, sozluk başlıkları). Haritalar elle sayıldı (satır sırası üstten alta; kare yazımı (sütun, satır; satır 0 = robotun bulunduğu alt sıra değil, her bölümde ayrıca belirtildi)); çözümler ve her tipik hata kare kare izlendi. Çalıştırma yapılmadı (oyunda `-izle 21 25` ile denenecek).
Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: bulmacalar, sayımlar ve çözümler tutarlı; her çözüm görevi bitiriyor, her tipik hata açıklaması gerçekten olanı anlatıyor, ipuçları 3'er adet, **YÜKSEK bulgu yok**. Asıl konular: (1) `elif` 24'te zorunlu değil, (2) yeni mekanikler (güç ölçer, onarım kolu, `elif`, sayaç `i`) bilgi kartsız, (3) panel komutları ilk bölümlerde zorunlu değil, ezber yürüyüşle geçiliyor.

## Genel

**G1 (ORTA) `elif` Bölüm 24'te zorunlu değil.** Ekip kuralı: yeni özellik geldiği bölümde ve sonrakinde zorunlu. `zorunlu_yapilar` tablosunda `elif` ayrı bir yapı olarak yok (`else` sayılıyor). 24 `elif` olmadan da geçilebilir (ör. iki `if`, ikincisinde `panel_power() > 0` ile birlikte) ve ezber yol da geçer. Öneri: `RequiredConstructs` tablosuna `elif` satırı ekle; 24 ve 25'te zorunlu yap (25'te kullanılmıyor: o zaman yalnız 24).

**G2 (ORTA) Panel komutları ezberle geçiliyor.** Harita sabit olduğu için 21-25'in hepsi komutları alt alta yazarak geçilir (ör. 21: `move(East)` ×2, `repair()`, `move(East)` ×2, `repair()`). Kural gereği yeni özellik (`panel_power`, `repair`, `<`, `==`) 21-22'de zorunlu olmalı; tabloda komut/işaret yok, yalnız yapı (`for`, `if`, `else`). Öneri: değişen alan kalıbını (`duzenler`, Bölüm 15/20 gibi) 25 ya da 30 sınavına uygulamak (ayrı karar); 21-22 için "kodda `panel_power` çağrılmalı" türü bir zorunluluk (kod yazısına bakan basit şart) değerlendirilsin.

**G3 (DÜŞÜK) Bilgi kartı yok:** 21 (güç ölçer + onarım kolu: iki yeni komut, `<`), 24 (`elif`), 25 (sayaç `i`). Ekip kuralı: yeni mekaniğe bilgi kartı. Hangi bölümlere konacağı Enes'le konuşulacak.

## Bölüm 21 — Panel tarlası
Robot (0,0), panel sırası doğuya: 80, 30, 70, 20, 90. Çatlak (0'dan çok, 50'den az): 30 ve 20 = 2 ✓ ("2 çatlak paneli onar"). Üst sıradaki üç panel (90, 60, 80) yolda değil, süs. Çözüm: `range(5)`, her karede ölç, çatlaksa onar → 2 onarım ✓. Tipik hatalar: #1 ilk panel 80 → "Panel zaten sağlam" ✓ (açıklama "gücü 80" doğru); #2 `>` sağlamı onarmaya çalışır ✓; #3 önce ölç sonra yürü: ilk karede panel yok, güç 0 < 50 → "Burada panel yok" ✓. Başlangıç kodu yardımcı, bitirmiyor ✓. İpuçları ✓ ("bu sırada beş panel").
1. (ORTA) G2/G3.

## Bölüm 22 — Kırık parçalar
Robot (0,0), kuzeye: (0,1)=0 kırık, (0,2)=70, (0,3) boş, (0,4)=0 kırık, (0,5)=60. 2 parça ✓. Çözüm: `==0` ise `collect()`; boş karede güç 0, `collect()` bir şey bulamaz ama durmaz (ipucu 2 bunu söylüyor) ✓. Tipik hatalar: `=` söz dizimi hatası ✓; ölçmeden toplamak (ikinci panel 70) → "Çalışan panel sökülmez" ✓; `repair()` kırıkta → "Kırık panel onarılmaz" ✓.
1. (DÜŞÜK) İpucu 2 "tek `=` if'in içinde hata verir" doğru; yeni olan `==` için bilgi kartı yok (G3 listesine eklenebilir, ama sözlükte sayfa var).

## Bölüm 23 — Ya onar ya topla
Robot (0,2), doğuya: 20, 0, 30, 0, 10. Çatlak 3 (onar), kırık 2 (topla) ✓ ("3 onar, 2 topla"). Çözüm `> 0` ise onar, değilse topla ✓. Tipik hatalar: ters dal → "Çalışan panel sökülmez" ✓; `< 0` hiç doğru olmaz, ilk panelde durur ✓; `else` unutulmuş → kırık parçalar kalır ✓.
Sorun yok.

## Bölüm 24 — Üç yol
Robot (0,0), doğuya: 80, 20, 0, 70, 30. 2 onar (20, 30), 1 topla (0) ✓. Çözüm `== 0` önce, sonra `elif < 50` ✓ (sıra önemli). Tipik hatalar: ters sıra → kırık panelde "Kırık panel onarılmaz" ✓; iki ayrı `if` → toplandıktan sonra karede panel kalmaz, güç 0 < 50 → "Burada panel yok" ✓ (açıklama doğru); `else` her şeyi kapsar → ilk panelde (80) durur ✓. Başlangıç kodu çözümün yarısı: `elif`i oyuncu yazar ✓.
1. (ORTA) G1: `elif` zorunlu değil.
2. (DÜŞÜK) İpucu 3 neredeyse bütün çözümü yazıyor (3 ipucu kademeli olsa da); 21-23 ile aynı kalıp, kabul edilebilir.

## Bölüm 25 — Sayaç
Robot (0,0). Yol: 3 doğu, 3 kuzey; her adımdan sonra `collect()`. Parçalar (2,0), (3,1), (3,3) = 3 ✓; yolda çalışan panel yok (ipucu 2 doğru: 80 ve 70 yolda değil). `range(6)` = 3+3 ✓. Tipik hatalar: `i < 4` bir kare fazla doğu, köşeyi kaçırır ✓ (parça kalır, robot durmaz); `i > 3` ters yol ✓; `collect()` yok ✓.
1. (DÜŞÜK) Yeni olan `i` sayacı için bilgi kartı yok (G3).

## sinav-25
3 soru, cevaplar doğru (Q1: şık 1 ✓, Q2: şık 2 ✓, Q3: şık 1 ✓). Bölge 3'ün dersleriyle (`==`, `elif` sırası, sayaç) örtüşüyor ✓.

## Enes'e sorular
1. `elif` yapısı 24'te zorunlu olsun mu (tabloya `elif` satırı)? Öneri: evet.
2. 21, 24, 25 için bilgi kartı yazılsın mı? Öneri: 21 ve 24 evet; 25 (sayaç) kısa kart.
3. Panel komutları (21-22) zorunlu olsun mu, yoksa Bölge 3 sınavlarına (25/30) değişen alan mı konsun? Öneri: önce 15 ve 20'nin oyunda nasıl hissettirdiğine bak.
