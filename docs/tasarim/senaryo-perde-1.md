# Senaryo — Perde 1: Ödev (Bölüm 1-50)

> Taslak 1 — 2026-10-02 (Ragıp + Claude). Dayandığı belgeler: `hikaye.md`, `hikaye-kitabi.md`.
> Bölge 1 (Bölüm 1-10) ayrıntılı senaryo: `senaryo-bolge-01.md`. Bu belge Bölge 2-5'i anlatır; Bölge 2-4'ün ayrıntılı senaryoları `senaryo-bolge-02.md`, `-03.md`, `-04.md`.
> Ayrıntı düzeyi: her bölge için olay, izler, program metinleri, kapanış; her bölüm için bir satır. Bölüm bulmacaları yapıldıkça satırlar ayrıntılanır (bulmacaya göre değişebilir).

## Perde 1'in kuralları

- Bölüm 50'ye kadar **ses kaydı yok, insan sesi yok.** Makine yazıları (araç kayıt ekranı, etiketler, koddaki yorumlar) serbest.
- Program (Mars Kod Okulu) hep **neşeli ve habersiz**. Bölüm 50'ye kadar başlıklar "Ödev N · ad".
- Robot arayüzde **BKM-7**. "Kıvılcım" adı Bölüm 50'de.
- Hiçbir soru bu perdede tam cevaplanmaz; yalnızca Bölüm 50 büyük cevabı verir.

## Tuhaflık merdiveni (her bölge bir basamak)

Oyuncunun şüphesi bölge bölge büyür; her bölge bir öncekinden biraz daha fazlasını söyler.

| Bölge | Oyuncu ne öğrenir (sezer) |
|---|---|
| 1 | Koloni boş; bir şey ovayı savurmuş; ödevi yalnızca ben alıyorum; "D.A." diye biri var; kutuptan yardım çağrısı. |
| 2 | İnsanlar **kurtarılmış** ("Ekip alındı. 2 kişi."), yani bir yere gitmişler; ölüm yok. |
| 3 | Kolonide bir **çocuk** var: Ece. Son boy çizgisinin tarihi, herkesin gittiği gün (ve Ece'nin doğum günü). |
| 4 | Biri bu yolu **benim için hazırlamış** (işaret direkleri, D.A.'nın ikinci notu); ödevler okuldan değil kolonidan geliyor. |
| 5 | D.A. = **D. Aras**. Ödev 50 kilitli, gönderen: D. Aras. → Kayıt. |

---

## Bölge 2 — Kutup buzulu (Bölüm 11-20) · `if`

**Olay:** Kıvılcım, Bölüm 10'daki yardım sinyalini izleyip kutba gelir. Sinyal, yan yatmış bir araştırma aracından geliyor (ufukta kırmızı ışık; oyunda zaten var). Aracın tarama sensörü sökülüp robota takılır: robot artık "bu karede ne var?" diye bakabilir (`if`). Buz toplanır; araç onarılıp otomatik sürüşle koloniye su taşır; seradaki kuru saksı yeşerir.

**Program:** "Yeni modül takıldı: tarama sensörü! Artık `if` kullanabilirsin." (Sensörün bir araçtan söküldüğünü söyler ama nedenini sormaz.)

**İzler ve gizli ayrıntılar:**
- İki kişilik ayak izi aracın yanından başlayıp birkaç adım sonra kayboluyor: orada başka bir aracın tekerlek izleri başlıyor (biri gelip onları almış).
- Kara saplanmış bir buz kazması, üstünde küçük bir bayrak.
- **Koda gizlenmiş:** kırmızı kristal bölümünün başlangıç kodunda araç ekibinin bıraktığı yorum: `# kirmizi olana dokunma! - kutup ekibi`
- **Aracın kayıt ekranı (Bölüm 20):** makine yazısı: `SON KAYIT — ACİL ALIM — 2 KİŞİ — HEDEF: [VERİ BOZUK]`. İnsanlar kurtarılmış, ama nereye gittikleri okunamıyor.

**Bölümler:**
- 11 · Merdiven — Kar fırtınasının izleri; yan yatmış araç ufukta, kırmızı ışık.
- 12 · Tarayıcı — Sensör takılır; program sevinçle `if`'i tanıtır.
- 13 · Kırmızı kristal — Başlangıç kodunda kutup ekibinin uyarı yorumu.
- 14 · Önce yürü sonra bak — Aracın yanında iki kişilik ayak izi.
- 15 · Karışık sıra — İzler birkaç adım sonra bitiyor; başka bir aracın tekerlek izleri başlıyor.
- 16 — Buz kazması ve bayrak; robot bayrağa bir an bakar.
- 17 — Aracın su tankı bulunur; buz tanka taşınacak.
- 18 — Kirli ve temiz buz karışık; dikkatli tarama.
- 19 — Araç doğrultulur; ışıkları yanar.
- 20 · Bölge finali — Aracın ekranında `SON KAYIT — ACİL ALIM — 2 KİŞİ`. Araç otomatik sürüşle koloniye su götürür.

**Kapanış sahnesi:** Kesme → kolonide sera. Su borusundan damla; kuru saksıdaki bitki ilk yeşil yaprağını açar. Program: "Sera sulandı! Bitki sağlığı: %12." Araç ekranında son bir şey: bir harita ve kraterli düzlükte yanıp sönen bir nokta (sonraki bölge).

---

## Bölge 3 — Kraterli düzlük, güneş paneli tarlası (Bölüm 21-30) · karşılaştırma (`<`, `==`, `>`), `elif`, döngü sayacı

> 2026-10-03 düzeltmesi: `else` artık Bölge 2'de (Bölüm 16) öğretiliyor; bu bölge karşılaştırma + `elif` + döngü sayacı `i`. Ayrıntılı senaryo: `senaryo-bolge-03.md`.

**Olay:** Fırtına güneş paneli tarlasını darmadağın etmiş. Robota bir modül daha takılır: **güç ölçer** (`panel_power()`) ve onarım kolu (`repair()`). Kıvılcım panelleri tek tek ölçer: gücü düşük (çatlak) olanı onarır, gücü sıfır (kırık) olanın parçasını toplar, sağlam olanı bırakır (üç yol = `if / elif`). Tarlanın kenarında bir **bakım kulübesi** var. Tarla çalışınca koloni ilk kez gündüz enerjisine kavuşur; bölgenin sonunda oyundaki ilk **gün doğumu**.

**Program:** "Sayıları karşılaştır: `<` küçüktür, `>` büyüktür, `==` eşittir." · "Bazen iki değil üç yol vardır: `elif`!" · Her bölümün girişinde küçük, sıkıcı bir bilgi: `Sol 1.068` (Mars günü).

**İzler ve gizli ayrıntılar:**
- **Bakım kulübesinin kapısında çocuk eliyle yazılmış "ECE"** (turkuaz boya). Tuhaflık basamağı: kolonide bir çocuk var.
- **Kapı pervazında boy çizgileri:** `ECE 7 · sol 150`, `ECE 8 · sol 506`, `ECE 9 · sol 861`. Program bunları kulübe taramasında sıradan bir liste gibi okur.
- **Kulübedeki takvim:** son işaretli gün `sol 861`; sonrası boş. Son boy çizgisi ile aynı gün: Ece'nin 9. doğum günü, aynı zamanda herkesin gittiği gün.
- **Sol sayacı:** oyuncu `1.068 − 861 = 207` hesabını yaparsa fırtınanın üstünden ~7 ay geçtiğini bulur (dikkatli oyuncuya ödül; karşılaştırma dersine de uyar).
- Kulübenin rafında kırık bir oyuncak robot: küçük, el yapımı; göğsünde Kıvılcım'ınkine benzer bir çizim.

**Bölümler:**
- 21 · Panel tarlası — Güç ölçer takılır; `<` ile çatlak paneller bulunur. Tarla darmadağın.
- 22 · Kırık parçalar — `==`: gücü tam 0 olan kırık panellerin parçaları toplanır.
- 23 · Ya onar ya topla — `>` + `else`; ufukta bakım kulübesi görünür.
- 24 · Üç yol — `elif` tanışma; kulübe yaklaşır.
- 25 · Sayaç — Döngü sayacı `i`; kulübenin kapısında "ECE". (Ardından mini sınav.)
- 26 · Dönüş — Kapı pervazında boy çizgileri.
- 27 · Köşede ölç — Kapı aralık; içeride takvim, son işaretli gün `sol 861`.
- 28 · Kayayı dolaş — Rafta el yapımı oyuncak robot; Kıvılcım ona bakar.
- 29 · Geri dönüş — Son paneller; ufukta gün ağarıyor.
- 30 · Gün doğumu (bölge finali) — Tarla tamamlanır.

**Kapanış sahnesi:** Güneş ufuktan yükselir (oyundaki ilk gün doğumu). Panellerin güç ışıkları birer birer yeşil dolar. Kıvılcım güneşe döner. Program: "Koloni gündüz enerjisi: %40." Sonra rüzgâr çıkar: "Hava uyarısı: toz. Sonraki ödev: kum tepeleri."

---

## Bölge 4 — Kum tepeleri (Bölüm 31-40) · `while`, `not`

> 2026-10-07 düzeltmesi: bulmacalara uyduruldu (pusula parçaları 38-40'a yayıldı, direk 40 kapanışında çalışır; gömülü araç yerine direk arabası; kurdele son direkte; direkler 9'dan 1'e geri sayar). Ayrıntılı senaryo: `senaryo-bolge-04.md`.

**Olay:** Rüzgârlı kum tepeleri. Toz bulutları gelip geçer: robot "toz geçene kadar bekle", "yol bitene kadar ilerle" der (`while`). Robota **toz algılayıcı** takılır (`dust_here()`, `wait()`). Kum tepelerinin arasında **yarı gömülü, geri sayan işaret direkleri** (9 → 1) var: biri bu yolu önceden işaretlemiş. Kıvılcım fırtınada dağılmış bir **yön bulma direğinin** (pusula) parçalarını toplar; direk çalışınca ibresi tepedeki anteni gösterir.

**Program:** "Ne zaman duracağını bilmiyorsan: `while`." · İlk kez programda **tuhaf bir satır:** `Görev kaynağı: KOLONİ (öncelikli)` (oyuncu: "ödevler okuldan değil mi?").

**İzler ve gizli ayrıntılar:**
- Direk 5'in numarası çocuk eliyle, **turkuaz** (`#2FC4B8`, Bölge 3'teki "ECE" ile aynı renk).
- Son direkte (1, tepede) **bir saç kurdelesi** bağlı (Ece'nin, turkuaz).
- **Koda gizlenmiş, ikinci D.A. notu:** Bölüm 35'in (adı "Acele etme") başlangıç kodunda: `# toz gecene kadar bekle.` + `# acele etme. - D.A.` (iki satır) ("acele etme": D.A.'nın oyuncuya ilk "sesi").
- Program sıkıcı bilgi: `Bu ödevi alan öğrenci sayısı: 1. Sınıf ortalaması: sen.`
- Kuma yarı gömülü bir **direk arabası**: kasası boş (direkler dikilmiş), sürücü yerinde bir battaniye.

**Bölümler:**
- 31 · İlk toz — İlk toz bulutu; robot bekler (`while` tanışma).
- 32 · Yol bitene kadar — İlk işaret direği: 9.
- 33 · İki duvar — Direk dizisi 8, 7, 6; biri yolu işaretlemiş.
- 34 · Bulut dizisi — Direk 5: numara turkuaz, çocuk eliyle.
- 35 · Acele etme — Başlangıç kodunda `# toz gecene kadar bekle.` + `# acele etme. - D.A.` (iki satır) (Ardından mini sınav.)
- 36 · Köşe — Programda `Görev kaynağı: KOLONİ (öncelikli)`.
- 37 · Merdiven — Direk arabası; sürücü yerinde battaniye.
- 38 · Pusula parçaları — Yön bulma direğinin parçaları; direğin boş ayağı.
- 39 · Basamaklar — Parçalar toplanmaya devam; "son parçalar tepede".
- 40 · Son tepe (bölge finali) — Son direk (1) ve kurdele; uzakta devrik dev anten.

**Kapanış sahnesi:** Parçalar direğin ayağına oturur, ibre döner ve anteni gösterir. Kıvılcım antene bakar. Rüzgâr kurdeleyi direkten koparır, Kıvılcım uzanıp yakalar, sırtındaki pil çantasına takılı kalır (kurdele bundan sonra robotun üstünde görünür; finalde Ece onu tanır). Program: "Sonraki ödev: anten tepesi."

---

## Bölge 5 — Anten tepesi (Bölüm 41-50) · değişkenler (sayaç, toplam)

**Olay:** Dev anten yeniden dikilir. Kablo makaraları toplanır, uzunlukları toplanır, kaç bağlantı kaldığı sayılır (değişkenler: sayaç, toplam). Bölüm bölüm anten ayağa kalkar. Bölüm 50: anten çalışır, ilk büyük veri gelir: **Defne'nin kaydı.**

**Program:** "Bir şeyi aklında tutmak için: değişken." · `Toplam kablo: 340 m. Kalan: 60 m.`

**İzler ve gizli ayrıntılar:**
- Antenin dibinde bir **alet çantası**, üstünde isim etiketi: **"D. ARAS"**. (Oyuncu D.A.'nın kim olduğunu yarı yarıya anlar.)
- Alet çantasının içinde, kapağın arkasına bantlanmış küçük bir çocuk çizimi: bir anten, yanında el sallayan iki çöp adam (büyük ve küçük).
- **Ödev listesinde kilitli bir satır** (Bölüm 45'ten itibaren): `Ödev 50 — KİLİTLİ — gönderen: D. Aras`. Program bunu sıradan bir şeymiş gibi listeler.
- Bölüm 49 sonunda program: `Büyük veri bekleniyor: 1 dosya (ses). Tahmini süre: 4 dk.` (İlk kez "ses" kelimesi.)

**Bölümler:**
- 41 — Tepenin eteği; dağınık kablo makaraları (sayaç).
- 42 — Kablolar toplanır, toplam uzunluk (toplam).
- 43 — Antenin dibi; alet çantası "D. ARAS".
- 44 — Çantanın kapağında çocuk çizimi.
- 45 — Ödev listesinde `Ödev 50 — KİLİTLİ — gönderen: D. Aras`.
- 46 — Antenin ayakları doğrultulur.
- 47 — Çanak yerine oturur.
- 48 — Son bağlantılar; sayaç sıfıra iner.
- 49 — Anten açılır; `Büyük veri bekleniyor: 1 dosya (ses).`
- 50 · Perde finali — Anten tam güçte. **Kayıt 1.**

### Bölüm 50 — Perde 1 finali (en önemli sahne)

1. Anten tam güce gelir; koloninin bütün ışıkları bir an titrer.
2. Program, son kez neşeyle: "Ödev 50 tamamlandı! Bir dosya geldi." Dosyanın adı: `ogretmene.ses`.
3. Ekran kararır. Hışırtı. Sonra **ilk insan sesi** (oyunda yazı + ses dalgası simgesi; dizide gerçek ses):

> "Bu bir ödev değil.
> Adım Defne Aras. Bu koloninin komutanıyım.
> Bunu duyuyorsan, onu yeniden öğretecek kişi sensin.
> Bir güneş fırtınası geliyor. Biz yeraltına iniyoruz, hepimiz. Uyuyacağız.
> Fırtına robotumuzun belleğini silecek. Ona her şeyi baştan öğretmen gerekecek.
> Kızım Ece ona Kıvılcım der. Ona iyi bak.
> Yolu işaretledik. Okları takip et.
> Seni tanımıyorum. Ama sana güveniyorum. Lütfen… ona öğret."

4. Sessizlik. Kıvılcım antenin dibinde, gökyüzüne bakıyor.
5. **Arayüz değişir (oyunun en güçlü anı):**
   - Robotun etiketi `BKM-7` silinir, harf harf **`KIVILCIM`** yazılır.
   - Başlıktaki "Ödev 51" yazısı silinir; yerine bölge ve görev adı gelir.
   - Program'ın neşeli kutusu bir daha görünmez; son satırı: `Ödev modu kapatıldı.`
6. Kıvılcım gökyüzüne, Dünya'ya bakar ve anten ışığını iki kez yakıp söndürür (ilk kez oyuncuya doğrudan "selam").

**Dizide:** Bu sahne Yıldız'ın odasında da oynar: gece, ekrandaki ses; Yıldız telefonundan haberleri açar: "Mars kolonisiyle bağlantı 7 aydır yok; ajans koloniyi kayıp sayıyor." Yıldız ekrana bakar.

---

## Perde 1 sonunda oyuncunun bildiği

- İnsanlar yaşıyor, yeraltında uyuyorlar.
- Defne Aras komutan; Ece onun kızı; robotun adı Kıvılcım.
- Ödev diye yaptığı her şey gerçekti; ona güvenilmiş.
- Hâlâ bilinmeyen: Nereye indiler? (Perde 2: "Nereye gittiler?")
