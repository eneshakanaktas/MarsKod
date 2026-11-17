# Sezon 1 denetimi — Bölüm 71-75

Yöntem: dosya okuma + her çözüm, başlangıç kodu ve her tipik hata motorda çalıştırıldı; 14 ezber/kestirme denemesi yapıldı (geçici test silindi). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: 5 çözüm geçiyor, 21 tipik hatanın hiçbiri geçmiyor. **YÜKSEK bulgu yok.** Enes "önerileri uygula" dedi; 1-3 uygulandı.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 71 | `return` ilk kez geliyor, kart yoktu | ORTA | Yapıldı: "Rutin cevap verir: return" |
| 2 | 75 | Kestirme: `rota` okunmadan 8 `move` ya da yön listesi geçiyordu | ORTA | Yapıldı: `zorunlu_yapilar: ["for", "if"]` (if yerine elif/ayrı if serbest) |
| 3 | 75 | Metin ve `for harf in rota` ilk kez geliyor, kart yoktu | ORTA | Yapıldı: "Yazıyı harf harf oku" |
| 4 | 73, 74 | Yeni mekanik küçük, ipuçları yetiyor | DÜŞÜK | Kart konmadı |
| 5 | 74 | Harita sabit: `def tara(yon): return 260` gibi rakam gömen kod geçiyor | DÜŞÜK | Dokunulmadı (rakamlar haritada görünüyor; düzen sistemi panel rakamlarını değiştirmiyor, ayrı büyük iş) |
| 6 | 71, 73, 74 | `donus_sarti` zorunlu | karar | Dokunulmadı |

Notlar: 73'te `<= 30` ve `if ...: return True / return False` biçimi de geçer (geçerli ikinci yol). 73'teki `< 30` parçası bilerek konmuş tuzak parça. 72'de ezber geçmiyor (ekran gizli satırları da soruyor).
