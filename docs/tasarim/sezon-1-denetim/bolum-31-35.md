# Sezon 1 denetimi — Bölüm 31-35

Yöntem: dosya okuma (bolum-31..35.json, sinav-35.json, senaryo-bolge-04.md, World.cs kuralları). Haritalar elle sayıldı, çözümler ve her tipik hata kare kare izlendi. Önem: YÜKSEK = oyuncuyu yanıltır/takar, ORTA = kaliteyi düşürür, DÜŞÜK = cila.

Özet: bulmacalar ve çözümler tutarlı, bitiş metinleri ve sol sayıları senaryoyla uyumlu. **YÜKSEK bulgu yok.** Enes "önerilerinden devam edelim" dedi; 1-5 uygulandı, 6 ertelendi.

| # | Bölüm | Bulgu | Önem | Durum |
|---|---|---|---|---|
| 1 | 31, 32 | `while` zorunlu değildi: 31 `wait` sayarak, 32 `for i in range(4)` ile geçiliyordu | ORTA | Yapıldı: `RequiredConstructs`'a `while`; 31 ve 32 `zorunlu_yapilar: ["while"]`; `WhileZorunluTests.cs` |
| 2 | 31, 32 | Bilgi kartı yoktu (while + toz + wait; not) | ORTA | Yapıldı: 31 "Toz bulutu: while", 32 "Tersine çevir: not" (35'e kart konmadı) |
| 3 | 33 | Tipik hata #2 açıklaması "kayaya çarpar" diyordu; gerçekte 2. turda `(2,2)`'de döngü biter, robot ortada kalır | ORTA | Yapıldı: açıklama düzeltildi |
| 4 | 34 | Tipik hata #2 (`move; wait`) açıklamasını (tozsuz karede boşuna bekleme) göstermiyordu; gerçek durma #1 ile aynıydı | ORTA | Yapıldı: kod iki `wait()` oldu; 2. karede tozsuz `wait()` ile durur, açıklama doğru |
| 5 | 35 | sinav-35'te iç içe while sorusu yoktu | DÜŞÜK | Yapıldı: 4. soru eklendi |
| 6 | 31, 34, 35 | Ezber: toz süreleri sabit, sayarak yazılabilir (31 madde 1 ile çözüldü; 34-35 serbest kuralı gereği zorunlu değil) | ORTA (karar) | Ertelendi: "her denemede değişen toz süresi" ayrı ve büyük iş (Layout'a toz); Bölge 4 sınavı için sonra konuşulur |
