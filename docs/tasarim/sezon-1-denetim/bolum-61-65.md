# Sezon 1 denetimi — Bölüm 61-65

Yöntem: dosya okuma + her çözüm, başlangıç kodu ve her tipik hata motorda çalıştırıldı; ezber denemeleri yapıldı (geçici test silindi). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: çözümler geçiyor, 20 tipik hatanın hepsi geçmiyor ve açıklamaları gerçek davranışla tutuyor. **YÜKSEK bulgu yok.** Asıl bulgu: 62 ve 65 tek izle geldiği için `scout()` kullanmadan, yönleri elle yazarak geçiliyordu. Enes "önerilerini uygula" dedi; 1-5 uygulandı.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 62 | Tek iz: yönler elle yazılınca geçiliyordu (`scout()` gerekmiyordu) | ORTA | Yapıldı: 2. iz (K,K,D,D,K), ezber tipik hatası, ipucu |
| 2 | 65 | Tek iz: `yon = scout()` gerekmiyordu | ORTA | Yapıldı: 2. iz (D,D,K,K,K,K), ezber tipik hatası, ipucu |
| 3 | 61 | Parametre ilk kez geliyor, kart yoktu | ORTA | Yapıldı: "Rutine bilgi kutusu ver" |
| 4 | 62 | `scout()` yeni komut, kart yoktu | ORTA | Yapıldı: "Serçe yukarıdan bakar" |
| 5 | 65 | `yon = scout()` kartı yoktu | ORTA | Yapıldı: "Cevabı bir kutuda tut" |
| 6 | 63, 64 | `range(kac)` / iki parametre için kart | DÜŞÜK | Konmadı (ipuçları yetiyor) |
| 7 | 61, 63, 64 | Genel rutin uyarısı hep `def git():` / `def ilerle(yon):` diyor | DÜŞÜK | Dokunulmadı |
| 8 | 63-65 | Parametre şartı 61-62'den sonra da var | karar | Dokunulmadı (51-60 kararıyla aynı) |

Not (62 ikinci iz): denetçi her izde "iki farklı bilgi" şartını ayrı arar; ikinci iz tek yöne gidince çözüm o izde geçmiyordu, bu yüzden iki yönlü seçildi.

Sonraki parti için gözlem: 67-70 de tek izle geliyor; 66-70 taranırken aynı ezber sorusu bakılmalı.
