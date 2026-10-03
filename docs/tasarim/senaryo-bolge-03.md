# Senaryo — Bölge 3: Kraterli düzlük, güneş paneli tarlası (Bölüm 21-30)

> Taslak 1 — 2026-10-03 (Ragıp + Claude). Dayandığı belgeler: `senaryo-perde-1.md` (Bölge 3 bölümü, bugün bulmacalara göre düzeltildi), `hikaye-kitabi.md` (zaman çizelgesi, Ece), `senaryo-bolge-02.md` (biçim).
> Bulmacalar değişmez (bugünkü `bolum-21..30.json`); bu belge yalnızca hikâye katmanını anlatır.

## Perde 1 taslağından farklar (bulmacalara uydurmak için)

- **`else` artık Bölge 2'de** (Bölüm 16). Bu bölgenin konuları: karşılaştırma (`<` 21, `==` 22, `>` 23), `elif` (24), döngü sayacı `i` (25-26), hepsi bir arada (27-30).
- **Robotun yeni becerisi bir modül:** güç ölçer (`panel_power()`) ve onarım kolu (`repair()`). Bölge 2'deki sensörler gibi program bunu "takıldı" diye duyurur, nereden geldiğini söylemez.
- **Sağlam panel "bağlanmaz":** bulmacada sağlam panele dokunulmaz. Üç yol: kırık (güç 0) → parçasını topla, çatlak (güç 50'den az) → onar, sağlam → bırak.
- **Tarih ayrıntısı netleşti:** sol sayıları hikâye kitabındaki zaman çizelgesine göre hesaplandı (aşağıda "Sayılar").

## Bu bölgenin görevi

- Tuhaflık merdiveninin üçüncü basamağı: **kolonide bir çocuk var: Ece.** Kapıdaki adı, boy çizgileri, oyuncak robot.
- **Zaman ilk kez sayıyla görünür:** son boy çizgisi ile takvimin son işaretli günü aynı gün. Program bugünün solünü her bölüm söylüyor; dikkatli oyuncu aradaki farkı hesaplar (~7 ay). Karşılaştırma dersiyle aynı beceri.
- Oyuncunun emeği koloniye bir şey daha verir: **gündüz enerjisi.** Bölge 1'de lambalar, Bölge 2'de su, Bölge 3'te güneş.
- Oyunda ilk **gün doğumu**: şimdiye kadar hep alacakaranlıktı.

## Sayılar (yazarlar için; hikâye kitabıyla uyumlu)

| Ne | Sol | Neden |
|---|---|---|
| Koloninin kuruluşu | 0 | ~3 yıl önce (hikâye kitabı) |
| Ece 7 yaşında | 150 | Bir Dünya yılı ≈ 356 sol |
| Ece 8 yaşında | 506 | |
| Ece 9 yaşında = takvimin son günü = sığınağa iniş | 861 | Kulübe Defne'nin çalıştığı yer (mühendis); Ece'nin boyu her doğum gününde burada ölçülürdü. Son gün Defne panelleri fırtınaya karşı sabitlemeye geldi, Ece yanındaydı; aceleyle de olsa boyunu ölçtü. Aynı akşam sığınağın kapıları kapandı. |
| Bölüm 21 (bugün) | 1.068 | 1.068 − 861 = 207 sol ≈ 7 ay (hikâye kitabı: fırtınadan bu yana 7 ay); 1.068 sol ≈ 3 yıl |
| Bölüm 30 | 1.072 | Tarla birkaç günde onarılır (iki bölümde bir sol artar) |

Oyuncuya hiçbir yerde "7 ay" denmez; yalnızca sayılar verilir. Doğum günü de söylenmez: "ECE 9" yazısıyla takvimin son günü aynı sayıyı taşır, gerisini oyuncu bağlar.

## Anlatım araçları

Bölge 1-2 ile aynı (ses kaydı yok, insan sesi yok): program metni (neşeli, habersiz), haritadaki izler (süs, bulmacayı etkilemez), robotun tepkileri. Yeni: **program kulübeyi tarar** ve bulduklarını sıradan bir liste gibi okur (boy çizgileri, takvim, oyuncak). Program duygusuz; oyuncu duygulanır.

## Açılış (Bölüm 21'den önce, ~5 sn)

Bölge 2 senaryosunun kapanışında araç ekranı kraterli düzlüğü gösteriyordu (oyunda bu harita henüz yok). Bölüm 21 açılırken kara ekranda:
1. `Yeni bölge: Kraterli düzlük`
2. `Hedef: güneş enerjisi`

## Bölüm bölüm

Biçim: **Başlık** · görev · **Program (giriş / bitiş)** · **Sahnede**. Girişler ~3 sn görünür; kısa tutuldu.

### Bölüm 21 — Ödev 21 · Panel tarlası
- Görev: 2 çatlak paneli onar.
- Program giriş: "Sol 1.068 · Yeni modül: güç ölçer! Gücü 50'den az olan panel çatlaktır."
- Program bitiş: "2 panel onarıldı. Tarla gücü: %8."
- Sahnede: Tarla darmadağın: kuma yarı gömülmüş devrik bir panel ve kopmuş bir kablo makarası alanın arkasında.

### Bölüm 22 — Ödev 22 · Kırık parçalar
- Görev: 2 panel parçası topla.
- Program giriş: "Sol 1.068 · Gücü tam 0 olan panel kırıktır: parçasını topla."
- Program bitiş: "Parçalar ayrıldı. Tarla gücü: %12."
- Sahnede: Devrik panel yerinde; toplanan parçalar için alanın arkasında boş bir yedek parça sandığı.

### Bölüm 23 — Ödev 23 · Ya onar ya topla
- Görev: 3 onar, 2 topla.
- Program giriş: "Sol 1.069 · Gücü varsa onar, yoksa parçasını topla."
- Program bitiş: "Tarla gücü: %18. Yakında bir bakım kulübesi var."
- Sahnede: **Ufukta küçük bir kulübe** (uzakta, alanın çok gerisinde).

### Bölüm 24 — Ödev 24 · Üç yol
- Görev: 2 onar, 1 topla.
- Program giriş: "Sol 1.069 · Bazen iki değil üç yol vardır: elif!"
- Program bitiş: "Üç yolu da doğru seçtin! Tarla gücü: %22."
- Sahnede: Kulübe hâlâ uzakta, biraz daha belirgin.

### Bölüm 25 — Ödev 25 · Sayaç
- Görev: 3 panel parçası topla.
- Program giriş: "Sol 1.070 · Döngü turlarını sayar: i önce 0, sonra 1, 2…"
- Program bitiş: "Sayaç ustası! Sırada küçük bir sınav var."
- Sahnede: Kulübe artık alanın hemen arkasında. **İz 1:** kapısında çocuk eliyle, turkuaz boyayla, kocaman ve eğri büğrü: **"ECE"**. (Program hiçbir şey söylemez.)

### Bölüm 26 — Ödev 26 · Dönüş
- Görev: 3 panel parçası topla.
- Program giriş: "Sol 1.070 · Doğuya git, köşede dön, batıya gel."
- Program bitiş: "Kulübe tarandı. Kapı pervazında 3 işaret: ECE 7 · sol 150 — ECE 8 · sol 506 — ECE 9 · sol 861."
- Sahnede: **İz 2:** kapı pervazında, farklı yüksekliklerde üç kısa çizgi (boy çizgileri). Program bunları "işaret" diye okur; ne olduklarını bilmez.

### Bölüm 27 — Ödev 27 · Köşede ölç
- Görev: 2 onar, 2 topla.
- Program giriş: "Sol 1.071 · Önce yolu say, sonra paneli ölç."
- Program bitiş: "Kulübedeki takvim tarandı. Son işaretli gün: sol 861. Sonrası boş."
- Sahnede: **İz 3:** kulübenin kapısı aralık; içeride, arka duvarda beyaz bir takvim, son birkaç günün üstü kırmızıyla çizilmiş. (Kapı bu bölümden sonra açık kalır.)

### Bölüm 28 — Ödev 28 · Kayayı dolaş
- Görev: 2 onar, 2 topla.
- Program giriş: "Sol 1.071 · Önüne bak, sonra paneli ölç."
- Program bitiş: "Rafta bir nesne: oyuncak robot (el yapımı). Ödevle ilgisi yok."
- Sahnede: **İz 4:** kulübenin önündeki rafta küçük, el yapımı bir oyuncak robot; göğsünde turuncu küçük bir çizim (Kıvılcım'ınkine benzer). Programın "ödevle ilgisi yok" demesi bu bölgenin en acı satırı.

### Bölüm 29 — Ödev 29 · Geri dönüş
- Görev: 3 onar, 2 topla.
- Program giriş: "Sol 1.072 · Son paneller! Ufuk ağarıyor."
- Program bitiş: "Tarla gücü: %36. Gün doğumuna az kaldı."
- Sahnede: Kulübe, açık kapı, oyuncak robot yerinde.

### Bölüm 30 — Ödev 30 · Gün doğumu (bölge finali)
- Görev: Onar, topla, hedef.
- Program giriş: "Sol 1.072 · Son ödev! Tarlayı tamamla, hedefe ulaş."
- Program bitiş: "Bölge 3 tamamlandı! Tebrikler!"
- **Kapanış sahnesi (~10 sn):**
  1. Gün doğar: ışık yavaşça sıcaklaşır, her şey aydınlanır (oyundaki ilk gün doğumu).
  2. Panellerin güç ışıkları birer birer parlar. Program: "Gün doğumu. Tarla gücü: %100."
  3. Kıvılcım doğuya, güneşe döner.
  4. Program: "Koloni gündüz enerjisi: %40."
  5. Son satır: "Hava uyarısı: toz. Sonraki ödev: kum tepeleri."

## Oyuncuda bırakılması gereken sorular (Bölüm 30 sonunda)

- Ece kim? Kolonide bir çocuk mu vardı? Şimdi nerede?
- Sol 861'de ne oldu? Neden takvim orada bitiyor?
- Oyuncak robot neden Kıvılcım'a benziyor? (Göğüsteki çizim aynı elden.)
- Bugün sol 1.072: 211 sol, yani ~7 ay boyunca kimse dönmedi mi?

## Oyuna konanlar (Ragıp + Claude, 2026-10-03)

- **Yapıldı:** 1-4 hepsi. Bölüm 21-30 `giris`/`bitis`; Bölüm 21 açılış geçişi (`Traces.TransitionLines`); izler `CraterTraces.cs` (kulübe 25-30'da alanın sağ arkasında, kapıda turkuaz "ECE" telefonda okunuyor; 27'den sonra kapı aralık, içeride takvim; 28'den sonra rafta oyuncak robot, bir kolu kopuk); Bölüm 30 kapanışı `Oyun.ClosingSceneBolge3` (ışık 3 sn'de gün doğumuna geçer: `RegionLook.Sunrise`; yeniden denemede eski ışık geri gelir).
- **Eksik / sonra:** gökyüzünün kendisi gün doğumunda değişmiyor (yalnızca sahnenin ışığı sıcaklaşıyor; ufukta güneş diski için `MarsSky.hlsl`'e iş gerekir). Boy çizgileri ve takvim küçük; ayrıntıyı program metni taşıyor. Kraterli düzlüğün kendi görünüşü yok (ova). Telefonda denenmedi.

## Oyuna yapılacaklar (ilk liste)

1. **Bölüm dosyaları 21-30:** `giris`/`bitis` metinleri (yukarıdaki gibi).
2. **Açılış geçişi** (Bölüm 21 başı): "Yeni bölge: Kraterli düzlük / Hedef: güneş enerjisi".
3. **İzler (`CraterTraces.cs`):** 21-22 devrik panel + kablo makarası, 22 yedek parça sandığı; bakım kulübesi 23-24 uzakta, 25-30 yakında; kapıda "ECE" (25+), pervazda üç boy çizgisi (25+), kapı aralık + takvim (27+), rafta oyuncak robot (28+).
4. **Kapanış (Bölüm 30):** gün doğumu ışığı, panellerin birer birer parlaması, robotun güneşe dönmesi, üç program satırı.

## Hikâye kitabıyla çelişki kontrolü (2026-10-03)

Kitaptaki "Çelişki kontrol listesi" ve zaman çizelgesiyle karşılaştırıldı:

- Ses kaydı / insan sesi yok: ✅ yalnızca program metni ve makine yazısı (kulübe taraması).
- Robotlar konuşmaz: ✅ Kıvılcım yalnızca döner, bakar.
- Öğrencinin adı geçmez: ✅
- Kıvılcım uçmaz: ✅ kaya dolaşılıyor (28, 30).
- İnsanlar ölmez: ✅ hiçbir iz ölüm sezdirmez; takvim "gittiler" der, "öldüler" demez.
- Öğretilmeden yapamaz: ✅ güç ölçme ve onarma yeni modül + yeni Python konusu (karşılaştırma) ile gelir.
- Zaman çizelgesi: ✅ koloni ~3 yıl (1.068 sol ≈ 3,0 yıl), fırtınadan bu yana 7 ay (207 sol ≈ 7,0 ay), Ece ~9 (sol 861'de 9 oldu). Fırtına "güneş panelleri kırılır": ✅
- Motifler: ✅ "Ece yazısı: Bölge 3 kapı" (kitapta zaten var); göğüsteki çizim oyuncak robotta tekrar eder (aynı el, Ece).
- Işık: ✅ Bölge 1 ve Bölüm 21-29 "şafaktan hemen önce" (ova ışığı), Bölüm 30 kapanışı oyundaki ilk gün doğumu.
- **Kitaba eklenen yeni bilgi:** sol sayıları ve "sol 861 = sığınağa iniş = Ece'nin 9. doğum günü" (zaman çizelgesine not düşüldü).
- Küçük fark: `senaryo-bolge-02.md` kapanışındaki harita (kraterli düzlükte yanıp sönen nokta) oyuna konmadı; Bölge 3 açılışı ona dayanmıyor, sorun yok.

## Alınan kararlar (Ragıp, 2026-10-03)

- **Doğum günü ayrıntısı kalır:** sol 861 = Ece'nin 9. doğum günü = sığınağa iniş. Hiçbir yerde söylenmez, yalnızca sayılar aynı. Dizide Defne'nin kızının boyunu aceleyle ölçtüğü kısa bir geri dönüş sahnesi olabilir.
- **Oyuncak robot çantaya girmez:** Kıvılcım'ın çantası Bölge 4'te kurdeleyle başlar (Perde 2 sırası bozulmaz). Oyuncak robot kulübede kalır; finalde kulübeye dönülürse orada durur.
- **Turkuaz:** "ECE" yazısı turkuaz (`#2FC4B8`); Bölge 4'teki kurdele de aynı renk.
- **Kulübe yalnızca iz:** güç ölçer modülünün nereden geldiğini program söylemez.
