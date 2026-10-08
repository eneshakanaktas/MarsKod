# Senaryo — Bölge 6: Kanyon girişi (Bölüm 51-60)

> **Taslak 1 — 2026-10-08 (Enes + Claude, Opus 5.5; Enes onayladı).** Dayandığı belgeler: `docs/superpowers/specs/2026-10-08-bolge-6-design.md` (kararlar 4-8), `senaryo-perde-2.md` (Bölge 6), `hikaye-kitabi.md`.
> Bulmacalar 2026-10-08'de yapıldı (`bolum-51..60.json`); bu belge yalnızca hikâye katmanını anlatır. Görünüş (roket, depo, raflar, Serçe, boya oklar, kanyon) Bölge 6'nın 3. parçasında.

## Bu bölgenin görevi

- Perde 2'nin ilk bölgesi: program kapandı, **robotlar konuşmaz, metin yalnızca kayıt damgası** ("az söz, çok iz").
- Defne'nin "rutinleri" öğretilir (fonksiyon). Oyuncunun ilk rutini `sabah_turu()`: Bölüm 9'daki `# sabah turu - D.A.` notunu hatırlayana küçük bir "aha".
- Drone **Serçe** bulunur, onarılır, uçar. Adını Ece vermiş (robota verdiği gibi).
- Oklar ve görev çizelgesi gizemi büyütür (tarihler: acele; "BKM-7 — KAPI": Bölge 8'de açılır).

## Perde 2 biçimi (bütün Perde 2 için geçerli; Ragıp'a haber)

| Ne | Nasıl |
|---|---|
| Üst etiket | Bölgenin adı: **"BÖLGE 6 · KANYON GİRİŞİ"** (bölüm dosyasında `"etiket"`). "ÖDEV" ve bölüm numarası yazmaz; Bölüm 50 finalindeki "BÖLGE 5 · ANTEN TEPESİ" başlığının devamı. Büyük başlıkta görev (en çok 22 harf, `dotnet test` denetler). |
| Giriş / bitiş | Cümle değil, **kayıt damgası**: `Sol 1.084 · Kanyon deposu · Raflar`, `Drone parçası: 6/9`. |
| Sınav | Başlık **"KIVILCIM · KENDİNİ DENETLİYOR"** (sınav dosyasında `"baslik"`; 50 ve sonrası). Sorular aynı biçimde. 45 ve öncesi "Mini sınav". |
| Kayıtlar | Var olan kayıt ekranı (`RecordingScene`): kararan ekran, dosya adı, ses dalgası, satır satır. Bir kez gösterilir; "Kayıtlar" sayfası Bölge 7'de. |

## Bölüm 50 → 51 geçişi

Bölüm 50 finalinin son karesi ("Ödev modu kapatıldı." + Kıvılcım gökyüzüne bakıyor) kalır. Final sürerken alttaki düğme gizlidir; final bitince **2,5 sn** sonra **"Devam"** belirir → `sinav-50` ("KIVILCIM · KENDİNİ DENETLİYOR") → Bölüm 51. Bölüm 51 açılırken kara ekranda: **"Yeni bölge: Kanyon girişi" / "Hedef: kanyon deposu"**. (Roketin kalkıp kanyona indiği açılış sahnesi 3. parçada; bu iki satırın yerini almaz, önüne gelir.)

`senaryo-bolge-05.md` öneri 7'deki "Sonraki bölüm düğmesi çıkmaz" kararı böylece güncellendi: son kare yine yalnız kalır, sonra yalnızca "Devam".

## Kayıt 2 (Bölüm 52 başı)

Dosya adı `rutinler.ses`. Bölüm 52 ilk kez açılınca, bölüm kurulduktan hemen sonra; bitince ekran açılır ve bölümün giriş damgası gelir. Telefonda bir kez (`kayit2` kaydı).

> Ona ezber değil, beceri öğret.
> Bir kez öğrettiğini kendisi tekrar edebilir.
> Bunlara biz 'rutin' derdik.
> İlki sabah turuydu; her sabah yapardı.

Bölüm 52'nin başlangıç kodu `# sabah turu - D.A.` ile başlar (Bölüm 9'daki notun aynısı).

## Bölüm bölüm

Sol sayısı iki bölümde bir artar (Bölge 5 sol 1.082'de bitti). Drone parçaları 52-54'te toplanır: 3 + 3 + 3 = 9. 56-58'deki parçalar Serçe'nin **yedek parçaları**.

| Bölüm | Görev (başlık) | Giriş damgası | Bitiş damgası | Görünüşte (3. parça) |
|---|---|---|---|---|
| 51 Kanyon kapısı | Hedefe var | Sol 1.083 · Otomatik rota: Kanyon deposu · Hazırlayan: D. Aras | İz: turuncu boya ok · Yön: depo | Roket arkada; kaya duvarında ilk turuncu ok |
| 52 Sabah turu | Rutini 3 kez çağır | Sol 1.083 · Fırtına izi: dağılmış drone parçaları | Drone parçası: 3/9 · Rutin kaydedildi: sabah_turu | Kayıt 2 başta; parçalar kuma saçılmış |
| 53 Depo | raftan_al ile 3 parça | Sol 1.084 · Kanyon deposu · Raflar | Drone parçası: 6/9 | Raf sırası; depo içinde kayalar **sandık** olarak çizilir |
| 54 Serçe | toz_gec ile 3 parça | Sol 1.084 · Depo çatısı yırtık · İçerisi tozlu | Drone parçası: 9/9 · Gövdede el yazısı: SERÇE | Rafta gövde, turkuaz (`#2FC4B8`) çocuk eliyle **SERÇE** |
| 55 İlk uçuş | panel_bak ile onar | Sol 1.085 · Depo paneli: güç zayıf | Serçe: çevrimiçi · İlk uçuş | Kapanış: Serçe'nin ışıkları yanar, havalanır, kamera bir an yukarıdan bakar |
| 56 Görev çizelgesi | Döngüde koridor çağır | Sol 1.085 · Duvarda görev çizelgesi | Çizelge: BKM-7 · Son görev: KAPI | Duvarda çizelge; BKM-7 satırının sonu "KAPI" |
| 57 Rutin içinde rutin | adim ve tur rutinleri | Sol 1.086 · Yedek parça taraması | Yedek parça: 2 | — |
| 58 Boya oklar | Kendi rutinini yaz | Sol 1.086 · Oklar: sol 852 … sol 861 | Boya akmış · Oklar acele çizilmiş | Okların yanında küçük tarihler; boya akmış |
| 59 Ortak iş | Topla, bildir, var | Sol 1.087 · Serçe yukarıda: yol açık | İlk ortak iş · Rapor alındı | Serçe hedef karenin üstünde süzülür |
| 60 Kanyona iniş | İki rutinle aşağı in | Sol 1.087 · Hedef: kanyon dibi | Sonraki: kanyon dibi | Final: Kıvılcım dibe iner, Serçe üstünde, kamera derinliğe iner |

**"Raf" sözü (Enes'in geri bildirimi):** Bölüm 53'te parçalar bir sırada, aralarında kayalar (`K D K D D K`). Görünüş gelince bu sıra **raf**, kayalar **sandık** olarak çizilir. Şimdilik yalnızca tipik hata açıklamaları "kaya" yerine "sandık" der; raf 3. parçaya kadar görünmez.

## Çelişki kontrolü

- Ses kaydı yalnızca Defne; öğrenciye konuşur. ✔
- Robotlar konuşmaz: damgalar kayıt satırı, cümle değil. ✔
- Kıvılcım uçmaz: roket otomatik (D. Aras'ın rotası). Serçe uçar (drone). ✔
- Kimse ölmez. ✔
- Öğretilmeyen yapılmaz: rutin Bölüm 52'de öğretilir, 51'de rutin yok. ✔

## Oyuna konanlar (2026-10-08, Enes)

- `bolum-51..60.json`: `etiket`, `giris`, `bitis`; görevler kısaltıldı (≤ 22 harf); 53'te "sandık".
- `sinav-50/55/60.json`: `"baslik": "KIVILCIM · KENDİNİ DENETLİYOR"` (yeni isteğe bağlı alan, `Quiz.Title`).
- `CanyonTraces.cs` (yeni): bölge açılış satırları + Kayıt 2. Görünüş eklenince yerleşim de buraya.
- `Hud`: "BÖLGE …" etiketinde numara/ad yazılmaz; Bölüm 50 finalinde düğme gizli, sonra "Devam".
- Denetim: `-shots <klasör> -bolum 50 -final` artık Kayıt 2'yi, Bölüm 52 girişini, "Devam"lı son kareyi ve sınav başlığını da çeker (`FINAL DENETIMI:` satırı).
