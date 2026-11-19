# Senaryo — Bölge 9: Tuz gölü (Bölüm 81-90)

> **Taslak 1 — 2026-11-15 (Claude; Ragıp "durmadan çalış, kararları sen ver, sonra söyle" dedi).** Ragıp bakınca değişebilir.
> Dayandığı belgeler: tasarım belgesi (`docs/superpowers/specs/2026-11-09-bolge-8-10-ders-bulmaca-design.md`, Bölge 9), `senaryo-perde-2.md` (Bölge 9), `hikaye-kitabi.md`.
> Bulmacalar ve hikâye aynı gün yapıldı (plan `docs/superpowers/plans/2026-11-15-bolge-9-bulmacalar.md`). Görünüş (3a/3b) henüz yok: bölüm oynanır ama göl şimdilik iniş ovası gibi görünür, kristal buz gibi.

## Bu bölgenin görevi

- Ders: liste (81-85: `[...]`, `for x in liste`, `append`, `max`, `liste[0]`, `len`, `in`) + iç içe döngü (86-90).
- Hikâye: bembeyaz, gökyüzünü yansıtan kurumuş göl → Kayıt 5 → tuzla çevrili oklar → Defne'nin yarım listesi → tuza gömülü oyuncak astronot (çantaya) → ufukta volkan → gölün ortası (her yön aynı, yalnız oklar) → karşı kıyı, gece.
- Oyuncu ne öğrenir: okların hepsi aynı yeri, **volkanı** gösteriyor; koloni "dağın altında".

## Bölüm 80 → 81 geçişi

80'in finali → `sinav-80` → 81 açılırken kara ekranda iki satır: **"Yeni bölge: Tuz gölü" / "Hedef: okların gösterdiği dağ"**. Kod: `SaltLakeTraces.TransitionLines`.

## Kayıt 5 (Bölüm 83 başı)

Dosya adı `oklar.ses`. Kayıt 3-4 gibi: Bölüm 83 ilk kez açılınca oynar; yeniden dinlemek Kayıtlar sayfasından.

> Okları takip et.
> Biz oradayız; dağın altında.
> Acele etme, ama durma da.

Yeri: 81-82'de göl uçsuz bucaksız; oyuncu nereye gittiğini bilmiyor. Kayıt yönü verir (dağ = volkan), 84'teki tuzla çevrili oklar bunu gösterir, 88'de volkan ufukta belirginleşir. "Acele etme" Bölüm 35'teki D.A. notunu (`# acele etme. - D.A.`) yankılar; kimse söylemez.

## Bölüm bölüm

Sol sayısı iki bölümde bir artar (Bölge 8 sol 1.097'de bitti). Etiket hepsinde **"BÖLGE 9 · TUZ GÖLÜ"**.

| Bölüm | Giriş damgası | Bitiş damgası | Olan · ders |
|---|---|---|---|
| 81 Tuz gölünün kıyısı | Sol 1.098 · Tuz gölünün kıyısı | Tuz kristali: 5 · Ufuk: bembeyaz | Kıyı. İlk liste: `yol = [North, ...]`, `for y in yol:`. |
| 82 Ufuk kayboluyor | Sol 1.098 · Ufuk kayboluyor | Toplam boy: {rapor} cm · Gökyüzü yerde (27 ya da 24; boylar her denemede değişir, A3) | `crystal_size()` açılır (yeni komut; bilgi kartı). Listedeki yolu yürü, boyları topla, `report`. |
| 83 Kayıt 5 | Sol 1.099 · Kayıt 5 | En büyük kristal: {rapor} cm (12 ya da 14; A3) | Bölüm başında Kayıt 5. `append` + `max`. |
| 84 Tuzla çevrili oklar | Sol 1.099 · Tuzla çevrili oklar | İlk ok tuzla çevrili · Biri okları korumuş | `read_note()` artık liste verir (bilgi kartı). İlk ok `oklar[0]` yön, okların sayısı `len(oklar)` adım. 2 not. |
| 85 Yarım kalmış liste | Sol 1.100 · Yarım kalmış liste | Farklı boy: {rapor} · Liste yarım kalmış (4 ya da 3; A3) | Başlangıç kodunda Defne'nin yarım listesi. `in` / `not in` + `append` + `len`: kaç farklı boy (bilgi kartı: sondaki virgül hata değil). |
| 86 Tuza gömülü | Sol 1.100 · Tuza gömülü | Bulundu: oyuncak astronot · Çantaya | İç içe döngüye giriş: gölü satır satır tara. Oyuncak astronot taranan karelerden birinde (3a). |
| 87 Kristaller sıraya | Sol 1.101 · Kristaller sıraya | Not: gölün haritası · Sayıldı | Not gölün küçük haritası (satır listesi); iç içe döngüyle `T`'leri say, `report`. 3 not, her birinde anten başka sayı bekler. |
| 88 Volkan belirginleşir | Sol 1.101 · Volkan belirginleşir | Kırmızı kristal: 4 · Dokunulmadı | Tarama + `if crystal_size() > 0:`; kırmızı kristaller toplanmaz. |
| 89 Gölün ortası | Sol 1.102 · Gölün ortası | Her yön aynı · Yalnız oklar | Not yol parçalarının listesi; dış döngü parçalar, iç döngü harfler. 3 not. |
| 90 Karşı kıyı | Sol 1.102 · Karşı kıyı | En büyük kristal: {rapor} cm · Sonraki: gece ovası (11, 12 ya da 10; A3) | Final: parça listesi + ölç + listeye ekle + `max`; bir notta yol kırmızı kristalden geçer. 3 not. |

Sınavlar: `sinav-85` (`liste[0]`, `len`, `append`, sondaki virgül, `in`), `sinav-90` (iç içe döngüde kaç tur: 3 × 4 = 12; iç ile dış döngünün sırası; `parcalar[0][1]`).

## İzler ve gizli ayrıntılar

| Bölüm | İz |
|---|---|
| 84 | Tuzla çevrili ok: biri okları rüzgârdan korumuş (senaryo-perde-2). Rüzgâr ayak izlerinin çoğunu silmiş, okların yanındakiler kalmış. Kimse söylemez. |
| 85 | Başlangıç kodunun ilk satırı Defne'nin yarım listesi: `yanimiza_al = ["su", "ilac", "battaniye", "Ece'nin defteri", ]`. Sondaki virgülden sonrası boş: aceleyle kesilmiş. Oyuncu listeyi silmeden çözer. |
| 86 | Tuza gömülü **oyuncak astronot** → çantaya (sırada dördüncü: kurdele, kalem, katlanmış çizim, astronot). **Ragıp onayladı (2026-11-15):** astronotun sırt çantasında küçük, solmuş iki harf: **K.A.** Ece'nin babası Kaan Aras'ın hediyesi (`hikaye-kitabi.md`); oyunda hiç söylenmez. İkinci izleyişte anlaşılır. |
| 87 | Defne'nin gölün haritasında kristalleri `T` (tuz) diye işaretlemesi. |
| 83 | Kayıt 5'teki "Acele etme" Bölüm 35'in D.A. notuyla aynı söz. |
| 90 | Kapanış (senaryo-perde-2'den renkleri değişti, Ragıp 2026-11-15: önceki bölgelerde kullanılmamış renkler): güneş batar; göl altından fuşyaya, sonra mürdüme döner; ilk yıldızlar; volkanın siluetinin üstünde bir şey parıldıyor (Bölge 10'un mavi yıldızına hazırlık). |

## Renkler (Ragıp, 2026-11-15)

Bölge 9 önceki bölgelerde kullanılmamış renklerle: **sedef beyazı tuz, kükürt sarısı mineral bantları (kuruyan gölün dibine çöken sülfat tuzları), şampanya-altın gök ve yansıması, zeytin tonu gölge.** Kesin renkler: `docs/superpowers/plans/2026-11-15-bolge-9-10-gorunus.md` "Renk paleti".

## Çelişki kontrolü

- Ses kaydı yalnızca Defne; öğrenciye konuşur ("Okları takip et"). ✔
- Robotlar konuşmaz; damgalar makine yazısı. ✔
- Kimse ölmez. ✔
- Çanta sırası: kurdele (4), kalem (7, 68), katlanmış çizim (8, 76), oyuncak astronot (9, 86). ✔
- Notlardaki yön harfleri Bölge 8'deki gibi Türkçe (K G D B); `T` yalnız 87'nin haritasında kristal demek, yön harfi değil. ✔
- Kırmızı kristal (Bölge 2'den beri "tehlikeli kristal", toplanmaz) gölde de aynı kural. ✔
- Bölge 10'un sayıları (`%31`, `101 SOL`, `94 SOL`, `14 GÜN`) Bölge 9'da kullanılmadı. ✔
