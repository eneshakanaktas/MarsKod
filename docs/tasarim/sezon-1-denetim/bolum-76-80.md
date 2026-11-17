# Sezon 1 denetimi — Bölüm 76-80

Yöntem: dosya okuma + her çözüm, başlangıç kodu ve her tipik hata motorda çalıştırıldı; 17 ezber/kestirme denemesi yapıldı (geçici test silindi). Sahne tarafı (76 çizimin alınışı, 78, 80 kapanış) taranmadı; Ragıp'ın Bölge 8 · 3b denetimi TAMAM. Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: 5 çözüm geçiyor (76 ve 80 üç notun hepsinde), 24 tipik hatanın hiçbiri geçmiyor. **YÜKSEK bulgu yok.** Enes "önerileri uygula" dedi; 1-2 uygulandı.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 76 | `read_note()` ilk kez geliyor, not her denemede değişiyor, kart yoktu | ORTA | Yapıldı: "Not her seferinde başka" |
| 2 | 77 | `in` ilk kez geliyor, kart yoktu | ORTA | Yapıldı: "Metnin içinde ara: in" |
| 3 | 79 | `len` ilk kez, ipucu anlatıyor | DÜŞÜK | Kart konmadı |
| 4 | 77, 79 | Sabit liste/kayıt: ad ya da rakam gömen kod geçiyor (`"Ece" in satir or "Defne" in satir`, `return 6`); 74 ile aynı | DÜŞÜK | Dokunulmadı (ayrı büyük iş) |
| 5 | 71-80 | `donus_sarti` zorunlu | karar | Dokunulmadı |

Notlar: sinav-75 ve sinav-80 Bölge 8'i (`return`/`None`, `==`, `in`, `len`) zaten kapsıyor. 80'de `if harf != "V": ... else: collect()` ve ayrı `if` biçimleri de geçer (geçerli ikinci yol).
