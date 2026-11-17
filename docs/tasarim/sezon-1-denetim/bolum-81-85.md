# Sezon 1 denetimi — Bölüm 81-85

Yöntem: dosya okuma + her çözüm, başlangıç kodu ve her tipik hata motorda çalıştırıldı; 14 ezber/kestirme denemesi yapıldı (geçici test silindi). Görsel taraf bakılmadı (görsel tur ayrı). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: 5 çözüm geçiyor (84 iki notun ikisinde de), 19 tipik hatanın hiçbiri geçmiyor. **YÜKSEK bulgu yok.** Enes "önerilerini uygula" dedi; 1-4 uygulandı.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 81 | Kestirme: liste kullanılmadan 5 `move`+`collect` alt alta geçiyordu | ORTA | Yapıldı: `zorunlu_yapilar: ["for"]` |
| 2 | 82 | Aynı kestirme (yeni özellik kuralı: geldiği bölüm + sonraki) | ORTA | Yapıldı: `["for"]` |
| 3 | 81 | Liste ilk kez geliyor, kart yoktu | ORTA | Yapıldı: "Liste: öğeler virgülle" |
| 4 | 83 | `append` ve `max` ilk kez, kart yoktu | ORTA | Yapıldı: "Listeye ekle, en büyüğü bul" |
| 5 | 82, 83, 85 | Ölçmeden `report(27)` vb. yürüyüşle geçiyor (boylar sabit) | DÜŞÜK | Dokunulmadı (74/77/79 ile aynı) |
| 6 | 85 | Kart sondaki virgülle ilgili, `not in` için kart yok | DÜŞÜK | Dokunulmadı |
| 7 | 84 | `donus_sarti` zorunlu | karar | Dokunulmadı |

Not: 84'te `d = yon(oklar[0])` ile ilk yönü değişkene alıp yürümek de geçer (geçerli ikinci yol).
