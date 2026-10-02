# Hikâye, bölgeler ve araçlar

> Başlangıç: 2026-10-01 (Ragıp + Claude). Durum: **kararlar** verildi; **öneriler** Ragıp + Claude birlikte karara bağlar (karar 10-01: hikâye Ragıp'ın alanı; YouTube dizisi olacak).
> İlgili: tasarım belgesi §3 (`docs/superpowers/specs/2026-09-25-marskod-design.md`).

## Neden

Ekibin kaygısı: oyuncu hep aynı şeyleri görürse sıkılır. Arada bir gelen belirgin değişim (yeni yer, yeni araç, yeni toplanacak şey) oyuncuyu oyunda tutar. Kimi bölgeyi beğenir, kimini beğenmez; ama "sıradakini görmek" isteği oynatır.

## Kararlar (Ragıp, 2026-10-01)

1. **Gezegen yerine Mars'ın farklı bölgeleri.** Oyun Mars'ta kalır; değişen bölgedir (ova, buzul, kanyon, volkan, mağara…).
2. **Değişim çok sık olmaz ama belirgin olur.** Bölüm bölüm değil; birkaç bölümde bir küçük, daha seyrek büyük değişim.
3. **Toplanan şey bölgeye göre değişir.** Kurak ovada buz kristali toplamak oyuncuya mantıksız görünüyor; ovada başka bir şey toplanmalı.
4. **Araç ilerleyen bölümlerde değişebilir.** Örnek: roketle başka bölgeye gitmek, astronotun uzay kapsülünden dışarı çıkması.
5. **Hikâye çok önemli.** Oyuna farklı bir hava katar. Gerekirse şimdiye kadarki tasarım baştan yapılır, bu engel değil.

## Öneriler (Claude, 2026-10-01 — Ragıp "hepsini yapalım" dedi; Enes ile netleşecek)

### 1. Bölge, Python konusu, toplanan şey ve araç birlikte değişir

Her yeni bölge mantıklı bir yenilik getirir: oyuncu yeni yere varınca yeni şey öğrenir. Örnek sıra (taslak):

| Bölümler | Bölge | Yeni Python konusu | Toplanan | Araç |
|---|---|---|---|---|
| 1-10 | İniş ovası (kızıl çöl) | `move`, `for` | Kaya numunesi / meteor parçası | Gezgin robot |
| 11-20 | Kutup buzulu | `if` | **Buz** (burada mantıklı) | Aynı robot, kar paletli |
| 21-30 | Kanyon | `while` | Maden | Drone (uçar, kayaların üstünden geçer) |
| 31-40 | Volkan eteği | Fonksiyonlar | Isı kristali | Ağır iş robotu |
| 41-50 | Lav tüneli (mağara, karanlık) | Listeler | Kayıp parçalar | Astronot (fenerle) |

Not: Mars'ta gerçekten buz var (toprağın altında ve kutuplarda), yani buz bilimsel olarak yanlış değil. Ama kızıl çölde parlayan kristal oyuncuya tuhaf görünüyor; buz kutup buzuluna taşınır.

### 2. Bölge geçişi bir ödül anı

Bölgenin son bölümü bitince kısa bir geçiş sahnesi: roket kalkar, yeni bölgeye iner; ya da astronot kapsülden çıkar. Oyuncu "bir sonrakine kadar oynayayım" der.

### 3. Araç değişince öğrenilen kod bozulmaz

`move`, `collect` gibi komutlar her araçta aynı çalışır. Yeni araç yalnızca **yeni komut ekler** (drone'da `fly`, astronotta `light` gibi). Hangi aracın kullanıldığı bölüm dosyasına yazılır; oyun kurallarının çoğu değişmez.

### 4. Bölge içinde ucuz çeşitlilik

Aynı bölgede 10 bölüm boyunca aynı görüntü olmasın: saat ve hava değişir (gün batımı, gece, toz fırtınası). Görüntü kodla çizildiği için paket boyutuna neredeyse hiç eklemez. Büyük değişim ~10 bölümde bir, küçük değişim 3-4 bölümde bir.

### 5. Koloni büyür

Oyuncu bölüm çözdükçe arkadaki koloniye yeni bina, sera, anten eklenir; toplanan şey bir işe yarar ("bu buzla seraya su gitti"). Oyuncu emeğini ekranda görür. Sıkılmaya karşı en güçlü araç bu olabilir.

### Boyut

Ölçüldü (10-01): oyunun kendi dosyaları ~2 MB; Mars görüntüsünün tamamı kodla çiziliyor (çizim kodları 90 KB). Kodla çizilen yeni bir bölge birkaç yüz KB – 1-2 MB tutar; 20 bölge bile paketi ~55-75 MB yapar. **Koşul:** yeni bölgeler de kodla çizilir; hazır büyük model/resim eklenirse boyut hızla büyür (önce sorulur). Asıl maliyet boyut değil **emek**: her bölge bugünkü Mars görünümü kadar uğraş ister.

## Hikâye

Tam hikâye sonra yazılabilir; ama bölge, araç ve toplanan şeyler hikâyesiz seçilirse sonra uymayabilir. Bu yüzden **önce yarım sayfalık bir iskelet**: kim, neden, nereye gidiyor. Bölge/araç seçimleri ona göre yapılır.

Ton: tasarım belgesindeki gibi sıcak ve sade, çocuksu değil (13+). Yeni Python konusu hikâyeyle açılır (tasarım belgesi §3: "tarama sensörü geliştirildi" → `if`).

**KARAR (Ragıp, 10-01): C — Sessiz koloni**, film kalitesinde anlatım. Taslak hikâye: `hikaye.md`.
**KARAR (Ragıp, 10-02): Taslak 3** — oyuncu, Dünya'daki "Mars Kod Okulu" öğrencisi (ödev sandığı şeyin gerçek olduğunu Bölüm 50'de anlar); güneş kavuşumu gerilimi; Kor'un fedakârlığı ve yeniden kurulması. Ayrıntı `hikaye.md`.

Konuşmaya başlamak için yazılan üç iskelet önerisi:

- **A. Fırtına sonrası:** Dev bir toz fırtınası koloninin malzemelerini ve araçlarını Mars'ın dört bir yanına savurdu. Oyuncu robotları kodlayıp bölge bölge parçaları geri topluyor; her bölgede bir araç daha bulunup onarılıyor (drone, ağır iş robotu…), koloni yeniden ayağa kalkıyor. *Artısı:* farklı bölgelerde farklı şeyler toplamayı ve araç değişimini doğal açıklar; koloni büyümesiyle tam uyar.
- **B. Yeni mühendis:** Oyuncu koloniye yeni gelen mühendis. Koloni büyüyor; her görev yeni bir bölgeye uzanmak (buz için kutba, maden için kanyona). Sonunda kendisi kapsülden çıkıp lav tüneline iner. *Artısı:* sıcak, umutlu; oyuncu kendini hikâyenin içinde görür.
- **C. Sessiz koloni:** Robot uyanıyor; koloni boş ve sessiz. Oyuncu uzaktan (yörüngeden) robotu kodlayarak koloniyi yeniden çalıştırıyor ve bölge bölge ilerledikçe insanlara ne olduğunu öğreniyor. *Artısı:* merak en güçlü bağlayıcı; "sonra ne olacak" oynatır. *Eksisi:* ton biraz karanlık; "sıcak, sade" ile dengelenmeli.

## Sıra

1. **İlk insan testi** ve telefonda deneme (zaten sıradaki iş).
2. **Hikâye iskeleti + bölge listesi + toplanan şeyler** — Enes ile (içerik/sanat kararı). Hikâye önemli olduğu için insan testini beklemeden konuşulabilir; yalnızca kod işi testten sonra.
3. Toplanan şey değişirse **Bölüm 1-10'daki `ice_here()` komutunun adı** ve bölüm/sözlük metinleri düzeltilir (komut adları gerçek kod olarak öğreniliyor; ne kadar erken o kadar ucuz).
4. Sonra: 2. bölge, roket geçişi, koloni büyümesi.

## Açık sorular

- İniş ovasında ne toplanır? (`ice_here()` yerine ne gelir?)
- Bölge başına 10 bölüm mü? İlk sürümde kaç bölge? (Tasarım belgesi: ilk sürüm 2 dünya, ~20 bölüm.)
- Geçiş sahnesi ne kadar uzun olsun, atlanabilir mi?
