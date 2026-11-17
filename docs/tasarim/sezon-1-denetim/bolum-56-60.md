# Sezon 1 denetimi — Bölüm 56-60

Yöntem: dosya okuma + her çözüm ve her tipik hata motorda çalıştırıldı (geçici test silindi). Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: çözümler ve tipik hata açıklamaları gerçek davranışla tutuyor (56: if ile koridor sonuna varılmıyor; 57: tanımsız adim; 58: 0'ı sormadan repair "panel yok", else "panel sağlam"; 59: rutinde toplam hatası, döngüde sıfırlama 160, erken rapor 130; 60: sağlam panel, ikinci rutin şartı). 60'ın ipucu sırası çözümle birebir. **YÜKSEK bulgu yok.** Enes "önerilerini uygula" dedi; 1-3 uygulandı. **Bölge 6 (51-60) bitti.**

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 59 | Rutinin kendi kutuları (kapsam) için kart yoktu | ORTA | Yapıldı: "Rutinin kendi kutuları var" |
| 2 | 57 | "Rutin içinde rutin" için kart yoktu | ORTA | Yapıldı: "Rutin içinde rutin" |
| 3 | sinav-60 | Kapsam ve elif sırası sorulmuyordu | DÜŞÜK | Yapıldı: 2 soru eklendi |
| 4 | 58 | Kopyalanmış if ile de geçiliyor (kural "en az 2 çalışma") | bilgi | Dokunulmadı |
| 5 | 56-60 | Rutin şartı beşinde de var | karar | Dokunulmadı (51-55 kararıyla birlikte) |
| 6 | 59 | `global` ve `return` ile de geçiliyor | bilgi | Serbest |
