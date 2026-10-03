# Senaryo — Bölge 2: Kutup buzulu (Bölüm 11-20)

> Taslak 1 — 2026-10-03 (Ragıp + Claude). Dayandığı belgeler: `senaryo-perde-1.md` (Bölge 2 bölümü), `hikaye-kitabi.md` (madde 7: kutup ekibi; zaman çizelgesi), `senaryo-bolge-01.md` (biçim, anlatım araçları).
> Bulmacalar değişmez (bugünkü `bolum-11..20.json`); bu belge yalnızca hikâye katmanını anlatır.

## Perde 1 taslağından farklar (bulmacalara uydurmak için)

Bölüm 16-20 yapılırken bulmacalar değişti: `else` ve yeni duyu `rock_ahead()` artık Bölge 2'de. Bu yüzden:

- **Bölüm 18:** taslaktaki "kirli ve temiz buz karışık" olayı bulmacaya uymuyor (18'de buz taraması yok, kaya dolaşılıyor). Yerine: **aracın açık kapısı** (içeride iki boş koltuk, çözülmüş kemerler, bir termos). "İnsanlar sakince gitmiş" iz'ini güçlendirir.
- **Sensör ikiye çıktı:** araçtan iki modül sökülür: 12'de tarama sensörü (`ice_here`), 16'da ön engel sensörü (`rock_ahead`). Program ikisinin de "araştırma aracı KT-2"den geldiğini söyler, nedenini sormaz.
- **Bölge 3'e etkisi (sonra düzeltilecek):** `senaryo-perde-1.md` Bölge 3'ü "`else`, `elif`, karşılaştırma" diye anlatıyor; `else` artık burada öğretildiği için Bölge 3 = `elif` + karşılaştırma olur.
- **Toplanan şey buz:** Bölüm 16-20'nin görev yazıları "hücre topla" diyor; bölge buz bölgesi, metinler "buz" olmalı (oyuna koyarken düzeltilir).

## Bu bölgenin görevi

- Tuhaflık merdiveninin ikinci basamağı: oyuncu **insanların kurtarıldığını** sezmeli ("Ekip alındı. 2 kişi."). Ölüm yok; bir yere gitmişler.
- Bölge 1'deki "neden boş?" sorusuna yarım bir cevap: biri gelip onları almış. Ama **nereye** sorusu açık kalır (ekranda "VERİ BOZUK").
- Kuru saksı (Bölüm 7) bu bölgenin sonunda ilk yaprağını açar: oyuncunun emeği koloniye hayat veriyor.

## Anlatım araçları

Bölge 1 ile aynı (ses kaydı yok, insan sesi yok): program metni (neşeli, habersiz), haritadaki izler (süs, bulmacayı etkilemez), koloni (arka plan), robotun tepkileri (bakar, durur, konuşmaz). Yeni araç: **makine yazısı**: aracın kayıt ekranı, koddaki yorum satırı.

## Açılış (Bölüm 11'den önce, ~5 sn, atlanabilir)

Bölüm 10'un kapanışı zaten sinyali ve güneydeki kırmızı ışığı gösterdi. Bölüm 11 açılırken kısa bir geçiş:
1. Kara ekranda program yazısı: `Yeni bölge: Kutup buzulu` → `Hedef: su`.
2. Oyun başlar. Ufukta yan yatmış araç ve tepesindeki kırmızı ışık (oyunda zaten var).

## Bölüm bölüm

Biçim: **Başlık** · görev · **Program (giriş / bitiş)** · **Sahnede**.

### Bölüm 11 — Ödev 11 · Merdiven
- Görev: 5 buz topla.
- Program giriş: "Kutba hoş geldin! Koloninin suya ihtiyacı var: buz topla."
- Program bitiş: "5 buz! Su deposu doluyor."
- Sahnede: Kar fırtınasının izleri (bir yana yığılmış kar, savrulmuş bir branda). Ufukta yan yatmış araç, kırmızı ışığı yanıp sönüyor. Robot bölüm sonunda bir an ışığa bakar.

### Bölüm 12 — Ödev 12 · Tarayıcı
- Görev: 2 buz topla.
- Program giriş: "Yeni modül takıldı: tarama sensörü! Artık `if` kullanabilirsin."
- Program bitiş: "Sensör çalışıyor. Modülün kaynağı: araştırma aracı KT-2."
- Sahnede: Araç bu bölümde daha yakın; yanında sökülmüş bir panel kapağı karda duruyor (sensör oradan alındı). Program bunun garip olduğunu fark etmez.

### Bölüm 13 — Ödev 13 · Kırmızı kristal
- Görev: Kristale dokunma.
- Program giriş: "Kırmızı kristaller numune değil. Yalnızca buz topla."
- **Başlangıç kodunun ilk satırı:**
  ```python
  # kirmizi olana dokunma! - kutup ekibi
  ```
  Bu yorum, sensörün belleğinde kalmış: kutup ekibinin kendi notu. (Türkçe harfsiz yazılmış: aceleyle, araç klavyesinden.)
- Program bitiş: "Aferin, kristale dokunmadın!"
- Sahnede: İz yok; uyarıyı kod satırı taşıyor. Bölge 1'deki "# sabah turu - D.A." ile aynı yöntem: oyuncu "kodda başka insanların notları var" diye fark etmeye başlar.

### Bölüm 14 — Ödev 14 · Önce yürü, sonra bak
- Görev: 1 buz ve hedef.
- Program giriş: "Önce yürü, sonra bak. Araç çok yakında."
- Program bitiş: "Araca ulaştın!"
- Sahnede: Araç artık alanın hemen arkasında (yan yatmış, kırmızı ışığı yanıp sönüyor; ufuktaki uzak hâli kaybolur). **İz 1:** aracın yanından başlayan **iki kişilik ayak izi** (iki farklı adım boyu).

### Bölüm 15 — Ödev 15 · Karışık sıra
- Görev: 3 buz, 2 kristal.
- Program giriş: "Sütunda buz ve kristal karışık. Her karede bak!"
- Program bitiş: "Harika tarama! Sırada küçük bir sınav var."
- Sahnede: **İz 2:** ayak izleri birkaç adım sonra **bitiyor**; orada başka bir aracın geniş tekerlek izleri başlıyor ve uzaklaşıyor. Biri gelip onları almış. (Bu bölgenin asıl cevabı burada sezilir; program hiçbir şey söylemez.)

### Bölüm 16 — Ödev 16 · Çatal yol
- Görev: 2 buz topla.
- Program giriş: "Bir modül daha: ön engel sensörü! Yeni kelime: `else`. Ya bu yol ya öteki."
- Program bitiş: "İki yoldan doğrusunu seçtin!"
- Sahnede: **İz 3:** kara saplanmış bir **buz kazması**, sapında küçük bir bayrak (üstünde okunmayan bir amblem). Robot bölüm sonunda bayrağa bir an bakar, başını eğer.

### Bölüm 17 — Ödev 17 · İki çatal
- Görev: 2 buz topla.
- Program giriş: "Aracın su tankı bulundu! Buzları tanka taşı."
- Program bitiş: "Tank doluyor: %30."
- Sahnede: Yan yatmış araç. (Tank göstergesi fikri ertelendi: şimdilik yalnızca program metni "%30 / %60" diyor.)

### Bölüm 18 — Ödev 18 · Köşeyi dön
- Görev: 4 buz topla.
- Program giriş: "Kayayı dolaş, yoluna devam et."
- Program bitiş: "Tank doluyor: %60."
- Sahnede: **İz 4:** aracın **kapısı açık**, içi karanlık; kapının önünde karda bir **termos**. Kavga yok, dağınıklık yok: sakince inmişler. (Kapı bu bölümden sonra açık kalır.)

### Bölüm 19 — Ödev 19 · İki duyu bir arada
- Görev: 3 buz topla.
- Program giriş: "İki sensörü birlikte kullan!"
- Program bitiş: "Araç doğrultuldu! Işıkları yanıyor."
- Sahnede: Bölüm bitince araç doğrulur (yan yatmış hâlden dik hâle geçer), farları yanar. Kırmızı yardım ışığı söner: sinyal artık gerekmiyor.

### Bölüm 20 — Ödev 20 · Su yolu (bölge finali)
- Görev: 3 buz ve hedef. Araç dik, farları yanık, hedefin hemen arkasında.
- Program giriş: "Son ödev! Aracı koloniye hazırla."
- Program bitiş: "Bölge 2 tamamlandı! Tebrikler!"
- **Kapanış sahnesi (~10 sn, atlanabilir):**
  1. Aracın ekranı açılır. Makine yazısı, tek tek beliren harflerle:
     `SON KAYIT — ACİL ALIM — 2 KİŞİ — HEDEF: [VERİ BOZUK]`
  2. Ekran kısa bir hışırtıyla geçer: `OTOMATİK SÜRÜŞ: KOLONİ`. Araç tank dolu, yavaşça uzaklaşır.
  3. Kesme → kolonide sera. Su borusundan bir damla düşer; Bölüm 7'deki **kuru saksı ilk yeşil yaprağını açar.**
  4. Program, neşeyle ve habersiz: "Sera sulandı! Bitki sağlığı: %12."
  5. Aracın ekranında son bir görüntü: bir harita, kraterli düzlükte yanıp sönen bir nokta (Bölge 3).

## Oyuncuda bırakılması gereken sorular (Bölüm 20 sonunda)

- İki kişiyi kim aldı, nereye götürdü? (VERİ BOZUK)
- Ekip neden aceleyle "kırmızı olana dokunma" yazmış?
- Kolonidekiler de böyle mi alındı? Neden dönen yok?
- Haritadaki yanıp sönen nokta ne?

Bölge 1'in "insanlar nereye gitti?" sorusu yarım cevaplanır: **alındılar, sağlar.** Gerisi açık.

## Oyuna konanlar (Ragıp + Claude, 2026-10-03)

- **Yapıldı:** Bölüm 11-20 dosyalarına `etiket`/`giris`/`bitis`; 16-20 görevleri "buz"; Bölüm 20 adı "Su yolu"; Bölüm 13 başlangıç koduna kutup ekibinin yorumu. İzler (`PolarTraces.cs`): 11 kar yığını + branda, 12 panel kapağı, 14 ayak izleri, 15 ayak izleri + biten yerde tekerlek izi, 16 buz kazması + turuncu bayrak, 18 termos. Araç KT-2 (`ResearchRover.cs`): 14-20 alanın arkasında; 18'den sonra kapısı açık; 19 sonunda doğrulur, farları yanar, kırmızı ışık söner; 20 kapanışında (`Oyun.ClosingSceneBolge2`) ekranda son kayıt → "OTOMATİK SÜRÜŞ: KOLONİ" → robot batıya döner, araç uzaklaşır → "Sera sulandı! Bitki sağlığı: %12."
- **Ertelendi:** Bölüm 11 açılış geçişi (kara ekran yazıları), Bölüm 17 tank göstergesi, seradaki saksının ilk yaprağı (sera kubbesi kutup manzarasında görünmüyor; Bölge 3'ün ova manzarasında gösterilebilir), robotun izlere/bayrağa bakması, bayraktaki amblem.

## Oyuna yapılacaklar (ilk liste)

Bölge 1'de kurulan altyapı kullanılır (`etiket`/`giris`/`bitis` alanları, `Traces.cs`, kapanış sahnesi):

1. **Bölüm dosyaları 11-20:** `"etiket": "ÖDEV"`, `giris`/`bitis` metinleri; 16-20'nin görevinde "hücre" → "buz"; Bölüm 20'nin başlığı "Bölge finali" → "Su yolu".
2. **Bölüm 13 başlangıç kodu:** ilk satır `# kirmizi olana dokunma! - kutup ekibi` (`dotnet test` bölümün hâlâ çözüldüğünü denetler).
3. **İzler (`Traces.cs`):** 11 savrulmuş branda, 12 sökülmüş panel kapağı, 14 iki kişilik ayak izi, 15 izlerin bittiği yerde tekerlek izi, 16 buz kazması + bayrak, 17 tank göstergesi, 18 aracın açık kapısı. Bulmacayı etkilemez.
4. **Araç:** Bölüm 19 sonunda doğrulma + farlar + kırmızı ışığın sönmesi; Bölüm 20 kapanış sahnesi (ekran yazısı, uzaklaşma).
5. **Sera kapanışı:** Bölüm 7'nin saksısına (`_TraceGreenhouse`) ilk yeşil yaprak; "Bitki sağlığı: %12" satırı.
6. **Açılış geçişi** (Bölüm 11 başı, kısa): `OpeningScene` benzeri iki satır.

## Açık sorular

- Bayraktaki amblem: ileride bir bölgede anlamı çıksın mı (örn. sığınağın işareti)? Öneri: Perde 2'de sığınağın kapısında aynı amblem görünsün: küçük bir gizli bağ.
- Ayak izleri "biri büyük, biri küçük": ekibin biri genç mi olsun? Öneri: hayır, yalnızca iki farklı insan; anlam yüklemeyelim (çocuk motifi Ece'ye ait, karışmasın).
- Bölüm 15'ten sonraki mini sınavın girişine program bir satır söylesin mi ("Sınav zamanı!")? Öneri: evet, Bölüm 5'teki gibi.
