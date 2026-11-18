# Sezon 1 denetimi — Bölüm 91-95 (Bölge 10, gece ovası, ilk yarı)

Yöntem: dosya okuma + her çözüm, başlangıç kodu ve her tipik hata motorda (iki meteor düzeninin ikisinde) çalıştırıldı; 15 ezber/kestirme denemesi yapıldı (geçici test silindi). Görsel taraf bakılmadı (görsel tur ayrı). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: 5 çözüm her iki düzende geçiyor, başlangıç kodlarının hiçbiri tek başına geçmiyor, 17 tipik hatanın hiçbiri tüm düzenlerde geçmiyor (91'in 2. hatası yalnız 1. düzende tesadüfen geçiyor; bölüm yine geçilmez). **YÜKSEK bulgu yok.** 91 ve 95'te bilgi kartı var; 92-94'te yok; hiçbirinde `zorunlu_yapilar` yok.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 93 | `and` ilk kez geliyor, kart yok | ORTA | Yapıldı: kart "İki şart birden: and" |
| 2 | 94 | `or` ilk kez geliyor, kart yok | ORTA | Yapıldı: kart "Biri yeter: or" |
| 3 | 95 | Kestirme: döngüsüz, `if`siz, `smoke_here`siz; dört `collect()` doğru adımlara elle yazılınca geçiyor (harita sabit) | ORTA | Yapıldı: `zorunlu_yapilar: ["for", "if"]` |
| 4 | 92 | `not` kullanmadan koşulu ters çevirmek (`if meteor_ahead(North): doğu else: kuzey`) geçiyor; geçerli ikinci yol. `not` 32'de öğretilmişti | DÜŞÜK | Dokunulmadı |
| 5 | 93 | `or` ile ters kurulum ve iç içe iki `if` geçiyor; geçerli ikinci yol (`and` zorunlu yapılamaz, yalnız `for`/`if` var) | DÜŞÜK | Dokunulmadı |
| 6 | 94 | Rutinsiz yazmak geçmiyor (rutin şartı çalışıyor); `if`li uzun rutin geçiyor, geçerli | DÜŞÜK | Dokunulmadı |
| 7 | 94 | `donus_sarti` zorunlu | karar | Dokunulmadı |

Sağlam çıkanlar: 91'de ezber yol, ifsiz yürüyüş ve ters koşul bütün düzenlerde elenir; 94'ün 3. hatası (`print`) açıklamasıyla doğru; 93'ün 3 hatası (yalnız krater, yalnız kaya, `or`) hepsi doğru yakalanıyor.

Enes "önerilerini uygula" dedi; 1-3 uygulandı. `dotnet test` 1869 geçti, paket yeniden üretildi, `-izle 91 95` TAMAM (24 deneme).
