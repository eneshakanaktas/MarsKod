# Her denemede değişen toz süresi (A2) — kısa tasarım

Enes, 2026-11-19. Açık iş A2 (`ILERLEME.md`); ilk bulgu `docs/tasarim/sezon-1-denetim/bolum-31-35.md` madde 6.

## Neden
Toz bulutunun kaç `wait()` süreceği bölüm dosyasında sabit (31'de 3). Oyuncu `wait()` sayısını ezberleyip geçebiliyor; bölümün dersi (bilmediğin şey için `while dust_here():` sor) atlanıyor. Hikâye ve bilgi kartı zaten "kaç tur süreceğini bilemezsin" diyor.

## Seçilen yaklaşım: A — her denemede başka süre (Enes seçti)
- `toz_suresi` iki biçimde yazılabilir: bugünkü gibi düz liste (`[3]`, tek deneme) ya da deneme başına liste (`[[3], [5]]`). Kod her denemede ayrı dünyada denenir (değişen alan ve izlerle aynı altyapı: `Level.VersionCount`, `RunVersions`).
- Kural: her deneme listesi bulut sayısı kadar sayı içerir; iki deneme aynı olamaz; `izler`/`notlar`/`duzenler` ile birlikte yazılırsa deneme sayıları eşit olmalı.
- Ezber kod bir denemede takılır (tozlu karede yürümek de, tozsuz karede beklemek de robotu durdurur); mesajın başında "Bu denemede toz başka bir süre sürdü." yazar.
- Gösterge yok: bulutlar iki denemede aynı yerde ve aynı görünüşte; süre görünmez (bulmacanın kendisi).
- Elenenler: B (bulut boyu süreyi göstersin: yine sayılıp ezberlenir), C (her Çalıştır'da rastgele: adil değil, test edilemez).

## Kapsam
Bölüm 31-40 ve 51. İkinci denemede her bölümde en az bir bulutun süresi farklı (ezber kod mutlaka takılır). 43, 49, 50, 54-70 kapsam dışı (ders başka; 43'te bekleme sayısı `report` ile bildiriliyor).

## Denetim
`dotnet test`: yeni okuma kuralları, her toz bölümünde ezber kodun (bir denemenin süreleri kadar `wait()`) öteki denemede takıldığı, çözümlerin hepsinde geçtiği. Oyunda: paket + `-izle 31 40` ve `-izle 51 51`.
