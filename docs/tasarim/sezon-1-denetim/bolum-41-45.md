# Sezon 1 denetimi — Bölüm 41-45

Yöntem: dosya okuma + her çözüm ve her tipik hata motorda çalıştırıldı (robotun durduğu kare, gönderilen rapor sayısı, durma nedeni okundu; geçici test silindi). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: bulmacalar, çözümler ve tipik hata açıklamaları gerçek davranışla tutuyor (41: antene 0 ve 1; 42: 0 ve 25; 43: 2 ve 4; 44: bir adım dönüp kalır / alan sınırı; 45: 2, 0, 5), sol sayıları (1.078-1.080) ve bitiş metinleri senaryoyla uyumlu, sinav-45 cevapları doğru. **YÜKSEK bulgu yok.** Enes "önerilerini uygula" dedi; 1-4 uygulandı, 44'e kart konmadı (önerim hayırdı).

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 41, 42 | Değişken kurmadan geçiliyordu: `report(4)` (41), `report(285)` (42) sabit sayıyla geçiyordu | ORTA | Yapıldı: `RequiredConstructs`'a `degisken` (atama ya da `+=`); 41 ve 42 `zorunlu_yapilar: ["degisken"]`; `DegiskenZorunluTests.cs` |
| 2 | 41, 42 | Bilgi kartı yoktu | ORTA | Yapıldı: 41 "Değişken: aklında tutmak", 42 "Ölç, sonra topla: cable_length" |
| 3 | 43 | 3. ipucu son `move(East)` ve `report(bekleme)`'yi söylemiyordu | DÜŞÜK | Yapıldı: ipuça eklendi |
| 4 | sinav-45 | `>=` ile `>` farkı sorulmuyordu | DÜŞÜK | Yapıldı: 4. soru eklendi |
| 5 | 43, 45 | Sabit sayıyla geçilebiliyor (zorunlu kural yalnızca 41-42'de; sonrası serbest) | bilgi | Kural gereği serbest |
