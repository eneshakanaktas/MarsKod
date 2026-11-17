# Sezon 1 denetimi — Bölüm 66-70

Yöntem: dosya okuma + her çözüm, başlangıç kodu ve her tipik hata motorda çalıştırıldı; 9 ezber denemesi yapıldı (geçici test silindi). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: 5 çözüm geçiyor, 16 tipik hatanın hiçbiri geçmiyor, açıklamalar gerçek davranışla tutuyor. **YÜKSEK bulgu yok.** Asıl bulgu: 67 tek izle geldiği için yönler elle yazılınca geçiliyordu (62/65 ile aynı açık). Enes "önerileri uygula" dedi; 1-3 uygulandı.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 67 | Tek iz: `git(North)`, `git(East)`... elle yazılınca geçiyordu | ORTA | Yapıldı: 2. iz (K,K,D,D,K,D,K,B; farklı yerde biter), ezber tipik hatası, ipucu |
| 2 | 66 | Varsayılan değer (`kac=1`) ilk kez geliyor, kart yoktu | ORTA | Yapıldı: "Sayıyı yazmazsan 1 olsun" |
| 3 | 68 | Rutin başka rutini çağırıyor (ilk kez), kart yoktu | ORTA | Yapıldı: "Rutin başka rutini çağırır" |
| 4 | 67, 69, 70 | Yeni mekanik yok | DÜŞÜK | Kart konmadı |
| 5 | 66-70 | Parametre şartı hepsinde var | karar | Dokunulmadı (51-65 kararıyla aynı) |

Notlar: 69'da `repair()` döngü içinde `if i == 1:` ile de yazılabilir ve geçer; geçerli ikinci yol, sorun değil. 67'de sabit sayıda `wait()` zaten geçmiyor (toz süresi değişiyor). Dersler: ikinci iz önceki izle aynı yerde bitince hedef aynı olduğu için ezber yine geçiyordu; iz sonları farklı olmalı.
