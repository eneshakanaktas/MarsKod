# Senaryo — Perde 1: Ödev (Bölüm 1-50)

> Taslak 1 — 2026-10-02 (Ragıp + Claude). Dayandığı belgeler: `hikaye.md`, `hikaye-kitabi.md`.
> Bölge 1 (Bölüm 1-10) ayrıntılı senaryo: `senaryo-bolge-01.md`. Bu belge Bölge 2-5'i anlatır.
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
| 3 | Kolonide bir **çocuk** var: Ece. Son boy çizgisinin tarihi bir şeyin olduğu gün. |
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

## Bölge 3 — Kraterli düzlük, güneş paneli tarlası (Bölüm 21-30) · `else`, `elif`, karşılaştırma

**Olay:** Fırtına güneş paneli tarlasını darmadağın etmiş. Kıvılcım panelleri tek tek tarar: sağlam olanı bağlar, çatlak olanı onarır, kırık olanın parçasını toplar (üç yol = `if / elif / else`). Tarla çalışınca koloni ilk kez gündüz enerjisine kavuşur. Bölgede ilk kez **gün doğumu** olur.

**Program:** "Bazen iki değil üç yol vardır: `elif`!" · "Koloni gündüz enerjisi: %40."

**İzler ve gizli ayrıntılar:**
- **Bakım kulübesinin kapısında çocuk eliyle yazılmış "ECE".** (Tuhaflık basamağı: kolonide bir çocuk var.)
- **Kapı pervazında boy çizgileri**, yanlarında tarih: "Ece 7", "Ece 8", "Ece 9". Son çizginin tarihi, kulübedeki takvimde işaretlenmiş son günle aynı. O günden sonra takvimde hiçbir gün işaretlenmemiş.
- Program her bölümün altında küçük, sıkıcı bir bilgi verir: `Mars günü (sol): 1.247`. Oyuncu takvimdeki son günle karşılaştırırsa fırtınanın üstünden ~200 sol (~7 ay) geçtiğini hesaplayabilir (dikkatli oyuncuya ödül; karşılaştırma dersine de uyar).
- Kulübenin rafında kırık bir oyuncak robot: küçük, el yapımı; göğsünde Kıvılcım'ınkine benzer bir çizim.

**Bölümler:**
- 21 — Tarla darmadağın; paneller ters dönmüş, kuma gömülmüş.
- 22 — Sağlam / kırık ayrımı (`else`).
- 23 — Sağlam / çatlak / kırık (`elif`).
- 24 — Panel gücü karşılaştırılır (`>`, `<`); zayıf olanlar ayrılır.
- 25 — Bakım kulübesi; kapıda "ECE".
- 26 — Kapı pervazında boy çizgileri.
- 27 — Kulübede takvim; son işaretli gün.
- 28 — Rafta el yapımı oyuncak robot; Kıvılcım ona uzun uzun bakar.
- 29 — Son paneller; ufukta gün ağarıyor.
- 30 · Bölge finali — Gün doğumu; tarla ışıl ışıl, koloninin kubbeleri ilk kez gündüz ışığında.

**Kapanış sahnesi:** Güneş ufuktan yükselir (oyundaki ilk gün doğumu). Panellerin yüzeyi birer birer parlar, koloniye doğru akan bir enerji çizgisi. Kıvılcım güneşe döner; gözleri bir an kısılır. Sonra rüzgâr çıkar: uzakta kum tepeleri tozlanmaya başlar. Program: "Hava uyarısı: toz. Sonraki ödev: kum tepeleri."

---

## Bölge 4 — Kum tepeleri (Bölüm 31-40) · `while`

**Olay:** Rüzgârlı kum tepeleri. Toz bulutları gelip geçer: robot "toz geçene kadar bekle", "yol bitene kadar ilerle" der (`while`). Kum tepelerinin arasında **yarı gömülü işaret direkleri** dizisi var: biri bu yolu önceden işaretlemiş. Kıvılcım fırtınada dağılmış bir **yön bulma direğinin** (pusula) parçalarını toplar; direk çalışınca yolu, tepedeki anteni gösterir.

**Program:** "Ne zaman duracağını bilmiyorsan: `while`." · İlk kez programda **tuhaf bir satır:** `Görev kaynağı: KOLONİ (öncelikli)` (oyuncu: "ödevler okuldan değil mi?").

**İzler ve gizli ayrıntılar:**
- İşaret direklerinin birinde **bir saç kurdelesi** bağlı (Ece'nin; renk: Bölge 3'teki "ECE" yazısının rengiyle aynı).
- **Koda gizlenmiş, ikinci D.A. notu:** bir `while` bölümünün başlangıç kodunda: `# toz gecene kadar bekle. acele etme. - D.A.` ("acele etme": D.A.'nın oyuncuya ilk "sesi").
- Program sıkıcı bilgi: `Bu ödevi alan öğrenci sayısı: 1. Sınıf ortalaması: sen.`
- Kum tepesinin arkasında kuma yarı gömülü bir **koloni aracı**: içi boş, kapısı açık, koltukta bir battaniye.

**Bölümler:**
- 31 — İlk toz bulutu; robot bekler (`while` tanışma).
- 32 — Kum tepeleri arasında ilk işaret direği.
- 33 — Direk dizisi; biri yolu işaretlemiş.
- 34 — Direkte saç kurdelesi.
- 35 — Başlangıç kodunda `# toz gecene kadar bekle. acele etme. - D.A.`
- 36 — Programda `Görev kaynağı: KOLONİ (öncelikli)`.
- 37 — Kuma gömülü boş araç; koltukta battaniye.
- 38 — Yön bulma direğinin parçaları toplanır.
- 39 — Direk çalışır; ok tepeyi gösterir.
- 40 · Bölge finali — Son kum tepesinin üstü: uzakta bir tepede devrik dev anten.

**Kapanış sahnesi:** Toz dağılır. Kıvılcım son tepenin üstünde; aşağıda geniş bir vadi ve karşı tepede yere yatmış dev bir anten. Rüzgâr kurdeleyi direkten koparır, Kıvılcım havada yakalar, sırt çantasına takar (kurdele bundan sonra robotun üstünde görünür; finalde Ece onu tanır).

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
