# Sezon 1 denetimi — Bölüm 86-90 (Bölge 9'un sonu)

Yöntem: dosya okuma + her çözüm, başlangıç kodu ve her tipik hata motorda (tüm not sürümlerinde) çalıştırıldı; 16 ezber/kestirme denemesi yapıldı (geçici test silindi). Görsel taraf bakılmadı (görsel tur ayrı). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: 5 çözüm tüm sürümlerde geçiyor, başlangıç kodlarının hiçbiri tek başına geçmiyor, 18 tipik hatanın hiçbiri tüm sürümlerde geçmiyor (87/89/90'da bazı hatalar yalnız bir not sürümünde tesadüfen geçiyor; bölüm yine geçilmez, asıl amaç bu). **YÜKSEK bulgu yok.** Bu bölümlerin hiçbirinde bilgi kartı ya da `zorunlu_yapilar` yok.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 86 | İç içe döngü ilk kez geliyor, kart yok | ORTA | Yapıldı: kart "Döngünün içinde döngü" |
| 2 | 86 | Kestirme: döngüsüz 12 `move`+`collect` alt alta geçiyor (81/82'deki sorunun aynısı) | ORTA | Yapıldı: 86 ve 87'de `zorunlu_yapilar: ["for"]` |
| 3 | 90 | Bölge finali: `report(11)` yazıp yolu doğru yürümek geçiyor (boylar sabit); `append`/`max` kullanmak zorunlu değil. Not: ölçmeden yazılan çalışan "en büyüğü tut" kodu (`if crystal_size() > b: b = ...`) da geçiyor, o geçerli ikinci yol | ORTA | Dokunulmadı |
| 4 | 89 | `"".join(read_note())` ile tek döngü geçiyor (iç içe döngü zorunlu değil); geçerli ikinci yol | DÜŞÜK | Dokunulmadı |
| 5 | 87 | `satir.count("T")` ile iç döngüsüz sayma geçiyor; geçerli ikinci yol | DÜŞÜK | Dokunulmadı |
| 6 | 88 | Haritada tuz/kırmızı sabit; ölçmeden elle rota yazmak teorik olarak geçer (denemem kırmızıya çarptı, kanıtlanmadı) | DÜŞÜK | Dokunulmadı (74/77/79 ile aynı) |
| 7 | 89, 90 | `donus_sarti` zorunlu | karar | Dokunulmadı |

Sağlam çıkanlar: 87 tipik hatalarının kuralı (satır sayma, `"t"` küçük harf, `report(5)` ezberi) hepsi doğru yakalanıyor; 89'da `read_note()` doğrudan harf döngüsü "Yön yerine None" ile açık hata veriyor; 90'da `len` ve harf-döngüsü hataları doğru yakalanıyor; 90'ın 1. hatası (her karede topla) yalnız kırmızıdan geçen notta duruyor ve açıklaması bunu söylüyor.

Enes "önerilerini uygula" dedi; 1 ve 2 uygulandı. `dotnet test` 1869 geçti, paket yeniden üretildi, `-izle 86 90` TAMAM (23 deneme).
