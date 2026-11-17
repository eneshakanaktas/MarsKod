# Sezon 1 denetimi — Bölüm 46-50

Yöntem: dosya okuma + her çözüm ve her tipik hata motorda çalıştırıldı (durma nedeni ve gönderilen sayı okundu; geçici test silindi). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: çözümler ve tipik hata açıklamaları gerçek davranışla tutuyor (46: 140 ve 540; 47: eksik toplama ve alan sınırı; 48: 60 ve 0; 49: ilk tozda durma ve 70; 50: 145 ve dönüş tozu). Sol sayıları ve bitiş metinleri senaryoyla uyumlu, 46 "eksik 60 m", 50 "340 m". **YÜKSEK bulgu yok.** Enes "önerilerini uygula" dedi; 1-3 uygulandı.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 48 | "En büyüğü bul" kalıbının bilgi kartı yoktu | ORTA | Yapıldı: "En büyüğü aklında tut" |
| 2 | 46 | `-=` ve "eksiği bul" için kart yoktu | ORTA | Yapıldı: "Eksilt: -=" |
| 3 | sinav-50 | Toz/`report` döngü içi tuzağı sorulmuyordu | DÜŞÜK | Yapıldı: 4. soru eklendi |
| 4 | 47 | Palette kullanılmayan `rock_ahead` | DÜŞÜK | Dokunulmadı (31-45'te de aynı) |
| 5 | 46-50 | Sabit sayıyla geçilebiliyor | bilgi | Kural gereği serbest |
