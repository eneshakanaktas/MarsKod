# Sezon 1 denetimi — Bölüm 51-55

Yöntem: dosya okuma + her çözüm ve her tipik hata motorda çalıştırıldı (geçici test silindi). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: çözümler ve tipik hata açıklamaları gerçek davranışla tutuyor (51: ikinci turda kaya, ikinci kapıda toz; 52: tanımsız ad / çağrılmayan rutin; 53: ikinci rafta sandık; 54: tozda duruş; 55: sağlam panel onarılmaz). Sol sayıları senaryoyla uyumlu. **YÜKSEK bulgu yok.** Enes "önerilerini uygula" dedi; 1-4 uygulandı.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 52 | `def` (rutin) için bilgi kartı yoktu | ORTA | Yapıldı: "Rutin: adımlara ad ver" |
| 2 | 53 | Döngü–rutin farkı yalnız ipucundaydı | ORTA | Yapıldı: "Döngü mü, rutin mi?" |
| 3 | 54 | Tipik hata 1 açıklaması yanlıştı ("tozsuz karede wait() durdurur") | ORTA | Yapıldı: düzeltildi |
| 4 | sinav-55 | 53-55 konuları sorulmuyordu | DÜŞÜK | Yapıldı: 2 soru eklendi (döngü mü rutin mi; rutinde if) |
| 5 | 51 | "Toz ne kadar sürer bilemezsin" ama süre sabit; sabit `wait()` ile geçiliyor | bilgi | Ertelendi: "değişen toz süresi" işiyle birlikte |
| 6 | 54, 55 | Rutin 52-55'in dördünde zorunlu (yeni özellik kuralı 2 bölüm) | karar | Dokunulmadı (54/55 yeni ders: rutinde while/if; görev adı rutini istiyor) |
| 7 | 52 | Görev "3 kez çağır", kural "en az 2" | DÜŞÜK | Dokunulmadı (belgelenmiş davranış) |
