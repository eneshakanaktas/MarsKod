# Senaryo — Perde 2: İz (Bölüm 51-100)

> Taslak 1 — 2026-10-02 (Ragıp + Claude). Dayandığı belgeler: `hikaye.md`, `hikaye-kitabi.md`, `senaryo-perde-1.md`.
> Ayrıntı düzeyi: her bölge için olay, izler, kayıtlar, kapanış; her bölüm için bir satır (bulmacalar yapıldıkça ayrıntılanır).

## Perde 2'nin kuralları

- Soru: **"Nereye gittiler?"** Cevap perde sonunda değil, yol boyunca parça parça gelir (oklar volkanı gösterir).
- Artık **Defne'nin kayıtları** var (bölge başına 1, bazen 2). Hepsi öğrenciye ("sen") konuşur. Bölüm arası 1-2 satır; tamamı "Kayıtlar" sayfasında.
- Program kutusu yok (Bölüm 50'de kapandı). Başlıkta bölge ve görev adı.
- Duygusal görev: oyuncu **Defne'yi ve Ece'yi tanır, onlara bağlanır.** Perde sonunda (Bölüm 100) onları kaybetme korkusu doğar.

## Yeni motif: Kıvılcım'ın çantası

Bölge 4'te Kıvılcım, Ece'nin kurdelesini sırt çantasına taktı. Perde 2 boyunca yol üstünde bulduğu **Ece'ye ait küçük şeyleri** çantasına ekler (her biri robotun üstünde küçük bir ayrıntı olarak görünür):

| Bölge | Eşya |
|---|---|
| 4 | Saç kurdelesi (Perde 1) |
| 7 | Mavi bir boya kalemi |
| 8 | Katlanmış bir çizim |
| 9 | Tuza gömülü küçük bir oyuncak astronot |

Finalde Ece çantayı görür ve eşyalarını tek tek tanır. Kimse açıklamaz; oyuncu Kıvılcım'ın bunları **ona götürmek için** sakladığını anlar.

## Gizemin parçaları (Perde 2'de öğrenilenler)

| Bölge | Oyuncu ne öğrenir |
|---|---|
| 6 | Defne'nin rutinleri: Bölüm 9'daki "sabah turu"nun ne olduğu. Yolu Defne işaretlemiş (boya oklar). |
| 7 | Ece göç sırasında gruptan ayrılmış, Defne peşinden gitmiş; Ece kanyon duvarına bir mesaj bırakmış. |
| 8 | 24 kişinin listesi; Kıvılcım'ın **bilerek dışarıda bırakıldığı**: "BKM-7 — dışarıda — kapı". |
| 9 | Okların hepsi volkanı gösteriyor. |
| 10 | Dünya, Mars'tan bir yıldız gibi görünüyor. Bölüm 100: iki saat birden işlemeye başlıyor. |

---

## Bölge 6 — Kanyon girişi (Bölüm 51-60) · fonksiyonlar

> Ayrıntılı senaryo ve Perde 2 biçimi (etiket, kayıt damgaları, "KIVILCIM · KENDİNİ DENETLİYOR" sınavı): `senaryo-bolge-06.md` (2026-10-08). Kıvılcım roketi uçurmaz: otomatik rota (D. Aras).

**Olay:** Kıvılcım koloninin küçük roketiyle kanyona uçar (bölge geçişi sahnesi: ilk roket yolculuğu). Kanyon girişindeki depoda fırtınada parçalanmış bir **drone** bulunur ve onarılır. Drone'un gövdesinde Ece'nin el yazısıyla adı: **SERÇE**. Kanyon duvarlarında turuncu **boya oklar**. Defne'nin "rutinleri" öğretilir: bir kez yazılan iş adıyla tekrar çağrılır (fonksiyon).

**Kayıt 2 (bölge başı):**
> "Ona ezber değil, beceri öğret. Bir kez öğrettiğini kendisi tekrar edebilir. Bunlara biz 'rutin' derdik. İlki sabah turuydu; her sabah yapardı."

**Bağlantı (Perde 1'in karşılığı):** Oyuncunun yazdığı ilk fonksiyonun adı `sabah_turu()`. Bölüm 9'daki `# sabah turu - D.A.` yorumunu hatırlayan oyuncu için küçük bir "aha".

**İzler ve gizli ayrıntılar:**
- Drone'un adı SERÇE; Ece her şeye ad vermiş (robota, drone'a).
- Boya okların yanında küçük tarihler: oklar fırtınadan önceki 10 günde, aceleyle çizilmiş (boya akmış).
- Depoda duvara asılı bir görev çizelgesi; "BKM-7" satırında son görev: **"KAPI"** (anlamı Bölge 8'de).

**Bölümler:**
- 51 — Roketle kanyona iniş; ilk boya ok.
- 52 — Kayıt 2; ilk rutin: `sabah_turu()`.
- 53 — Depo; drone parçaları.
- 54 — Drone'un gövdesinde "SERÇE".
- 55 — Drone onarılır; ilk uçuş (yukarıdan harita görünür).
- 56 — Duvarda görev çizelgesi; "BKM-7 — KAPI".
- 57 — Rutinler birleşir (bir rutin içinde başka rutin).
- 58 — Boya okların yanındaki tarihler.
- 59 — Serçe ile Kıvılcım ilk ortak iş.
- 60 · Bölge finali — Kanyonun içine iniş.

**Kapanış:** Serçe kanyonun üstünde süzülür, Kıvılcım aşağıda; ilk kez iki robot birlikte. Kamera kanyonun derinliğine iner.

**Dizide:** Yıldız sabaha kadar uyumaz, kaydı defalarca dinler. Öğretmenine gösterir: "Bu bir program hatası, Yıldız."

---

## Bölge 7 — Kanyon dibi, eski nehir yatağı (Bölüm 61-70) · parametreli fonksiyonlar

**Olay:** Serçe yukarıdan yolu görür, Kıvılcım aşağıda yürür; rutinler artık "nereye", "kaç kez" gibi bilgi alır (parametre). Kumda koloninin göç izleri: geniş bir grup yürüyüşü. Sonra **küçük ayak izleri gruptan ayrılır**, yan bir kanyona sapar; bir süre sonra büyük ayak izleri onları izler.

**Kayıt 3:**
> "Ece yine önden koştu. Bir anlığına onu kaybettim. Sonra buldum; bir duvarın önünde, elinde boya kalemi. Ona kızamadım."

**İzler ve gizli ayrıntılar:**
- Yan kanyonda yere düşmüş **mavi boya kalemi** → Kıvılcım'ın çantasına.
- **Bölge finali, en güçlü iz:** yan kanyonun duvarında, mavi boyayla, çocuk eliyle: bir robot çizimi ve altında **"KIVILCIM BENİ BUL"**. Ece göç sırasında robota mesaj bırakmış.
- Nehir yatağında eski su izleri (Mars'ta gerçekten eski nehir yatakları var): Serçe'nin yukarıdan gördüğü kıvrımlar bir harita gibi.

**Bölümler:**
- 61 — Nehir yatağı; rutine bilgi verme (parametre).
- 62 — Serçe yukarıdan, Kıvılcım aşağıdan.
- 63 — Göç izleri: geniş bir grup.
- 64 — Kayıt 3.
- 65 — Küçük ayak izleri gruptan ayrılıyor.
- 66 — Büyük ayak izleri onları izliyor.
- 67 — Yan kanyon; dar ve kıvrımlı.
- 68 — Yerde mavi boya kalemi (çantaya).
- 69 — Duvar boyunca küçük el izleri.
- 70 · Bölge finali — "KIVILCIM BENİ BUL".

**Kapanış:** Kıvılcım duvardaki yazının önünde durur. Uzun bir sessizlik. Elini çizimin üstüne koyar. Göğsündeki çizim ile duvardaki çizim aynı elden çıkmış. Gözleri yumuşak, mavi bir ışıkla yanar.

**Dizide:** Yıldız kaydı internete koyar; "sahte" yorumları yağar. Tek bir kişi ciddiye alır (Sezon 1 boyunca Yıldız'ın Dünya'daki yardımcısı olabilir; karar sonra).

---

## Bölge 8 — Terk edilmiş araştırma istasyonu (Bölüm 71-80) · `return`

**Olay:** Kanyonun ucunda koloninin eski araştırma istasyonu. Koloni göç sırasında burada bir gece kalmış. İstasyonun bilgisayarları uyandırılır; sorulara cevap veren rutinler (`return`) ile kayıtlar okunur. Duvarlar Ece'nin çizimleriyle dolu (bir gece boyunca çizmiş).

**Kayıt 4:**
> "Ece korkuyor ama belli etmiyor. Bana benziyor. Bütün gece çizdi; 'Kıvılcım yalnız kalmasın diye' dedi."

**Büyük açıklama (makine yazısı, istasyon bilgisayarı):** Sığınağa inecek **24 kişinin listesi**, her ismin yanında `kapsül: hazır`. Listenin en altında iki satır daha:
```
BKM-7   — dışarıda — görev: KAPI
KOR     — hizmet dışı — hangar
```
Oyuncu Kıvılcım'ın **bilerek dışarıda bırakıldığını** öğrenir (bir gün kapıyı açacak olan o). Kor'un adı ilk kez geçer (Perde 3'ün tohumu).

**İzler ve gizli ayrıntılar:**
- Duvar çizimleri bir hikâye gibi sıralı: koloni, fırtına bulutu, yeraltına inen insanlar, dışarıda tek başına bir robot, sonuncusu: robot ve kız el ele (göğüsteki çizimin aynısı).
- Ece'nin yatağının yanında **katlanmış bir çizim** → çantaya (içini oyuncu görmez; finalde açılır: bkz. Perde 4).
- Listedeki 24 isim gerçek, tek tek yazılı (yazarlar için: her isim bir karakter tohumu; Perde 4'te uyananlar bunlar).

**Bölümler:**
- 71 — İstasyon; kapıda koloninin arması.
- 72 — Bilgisayarlar uyanır; ilk `return`.
- 73 — Duvarda Ece'nin çizimleri (ilk yarısı).
- 74 — Kayıt 4.
- 75 — Çizimlerin ikinci yarısı: dışarıda tek başına robot.
- 76 — Yatağın yanında katlanmış çizim (çantaya).
- 77 — Bilgisayarda 24 kişilik liste.
- 78 — Listenin altı: "BKM-7 — dışarıda — görev: KAPI".
- 79 — "KOR — hizmet dışı — hangar".
- 80 · Bölge finali — İstasyonun haritası: oklar tuz gölünün ötesini gösteriyor.

**Kapanış:** Kıvılcım duvardaki son çizimin, kendisini ve Ece'yi gösteren çizimin önünde. Sonra listedeki kendi satırına bakar: "görev: KAPI". Başını kaldırır; artık neden burada olduğunu biliyor.

---

## Bölge 9 — Tuz gölü (Bölüm 81-90) · listeler

**Olay:** Kurumuş, uçsuz bucaksız beyaz bir göl. Yer gökyüzünü yansıtıyor, ufuk kayboluyor. Tuz kristalleri toplanıp sıralı tutulur (liste). Okların hepsi aynı yeri gösteriyor: uzakta, tuzun ötesinde yükselen **volkan**.

**Kayıt 5:**
> "Okları takip et. Biz oradayız; dağın altında. Acele etme, ama durma da."

**İzler ve gizli ayrıntılar:**
- Tuza gömülü **küçük bir oyuncak astronot** → çantaya.
- Göl yüzeyinde kurumuş tuzda dizi dizi ayak izi; rüzgâr çoğunu silmiş, yalnızca okların yanındakiler kalmış (biri okları korumuş: tuzla çevrelenmiş).
- Liste dersine bağlı gizli ayrıntı: bir bölümün başlangıç kodunda Defne'nin yarım bıraktığı bir liste:
  ```python
  yanimiza_al = ["su", "ilac", "battaniye", "Ece'nin defteri", ]
  ```
  Sondaki virgülden sonrası boş: aceleyle kesilmiş. (Python'da sondaki virgül hata değildir; oyuncu bunu da öğrenir.)

**Bölümler:**
- 81 — Tuz gölünün kıyısı; ilk liste.
- 82 — Ufuk kayboluyor; yansıyan gökyüzü.
- 83 — Kayıt 5.
- 84 — Tuzla çevrelenmiş oklar.
- 85 — Başlangıç kodunda `yanimiza_al` listesi.
- 86 — Tuza gömülü oyuncak astronot (çantaya).
- 87 — Kristaller sıraya konur.
- 88 — Volkan ufukta belirginleşir.
- 89 — Gölün ortası; her yön aynı görünüyor, yalnızca oklar yol gösteriyor.
- 90 · Bölge finali — Karşı kıyı; gece çöküyor.

**Kapanış:** Güneş batar; tuz gölü mora, sonra laciverte döner. Gökyüzünde ilk yıldızlar. Volkanın siluetinin üstünde bir şey parıldıyor.

---

## Bölge 10 — Gece ovası, meteor yağmuru (Bölüm 91-100) · listede dolaşma (`for x in liste`)

**Olay:** Volkana giden gece ovası. Gökyüzünde meteor yağmuru; düşen meteor taşları toplanır, listede tek tek dolaşılır. Gökyüzünde parlak, mavimsi bir nokta: **Dünya** (Mars'tan Dünya gerçekten parlak bir yıldız gibi görünür).

**Kayıt 5b:**
> "Ece her gece o mavi yıldızı arardı. 'Orada bizi duyan biri var mı?' derdi. Ben de 'Belki bir gün' derdim."

**Motifin kurulduğu an:** Kıvılcım mavi noktaya, Dünya'ya bakar, uzun süre. Öğretmen orada. (Finalde Ece çizime bir yıldız ekleyecek: Mars'tan bakınca Dünya bir yıldızdır; öğrencinin adı Yıldız'dır. Üç anlam aynı yerde buluşur.)

**İzler ve gizli ayrıntılar:**
- Meteorların bıraktığı küçük kraterler; biri yakın zamanda düşmüş, hâlâ dumanlı (tehlike gerçek).
- Volkanın eteğinde, ok dizisinin bittiği yerde büyük, metal bir kapının silueti (sığınak; Perde 3 sonunda varılacak).

**Bölümler:**
- 91 — Gece ovası; ilk meteorlar.
- 92 — Meteor taşları toplanır (listede dolaşma).
- 93 — Kayıt 5b.
- 94 — Kıvılcım mavi noktaya bakar.
- 95 — Dumanlı yeni krater; dikkatli yol.
- 96 — Meteor yağmuru yoğunlaşır.
- 97 — Volkanın eteğinde kapının silueti.
- 98 — Yağmur diner; sessizlik.
- 99 — Koloniden (antenden) bir sinyal: sığınaktan otomatik bir mesaj geliyor.
- 100 · Perde finali — İki saat birden.

### Bölüm 100 — Perde 2 finali (orta nokta)

1. Sığınaktan otomatik sistem uyarısı (makine yazısı, kırmızı):
   ```
   SIĞINAK — KAPSÜL ENERJİSİ: %31
   KRİTİK EŞİK: 101 SOL SONRA
   ```
2. Hemen ardından, antenden ikinci bir uyarı (sarı):
   ```
   GÜNEŞ KAVUŞUMU: 94 SOL SONRA
   DÜNYA BAĞLANTISI 14 GÜN KESİLECEK
   ```
3. Oyuncu hesabı kendisi yapar (ya da ekran yapar): **bağlantı, kapsüllerin dayanabileceği son günden önce kopacak.** Son kapsüller açılırken öğretmen orada olmayacak.
4. Başlıkta ilk kez **sinyal göstergesi** belirir (dolu); bundan sonra her bölgede biraz zayıflar.
5. Kıvılcım mavi noktaya, sonra volkana bakar. Anten ışığını **bir kez** yakar (iki değil: sevinç değil, söz).

**Dizide:** Yıldız takvime bakar, günleri sayar. İlk kez korkar; bir şeyi sonuna kadar götürmesi gerekiyor ve zamanı belli.

---

## Perde 2 sonunda oyuncunun bildiği

- Koloni volkanın altındaki sığınakta; kapı orada.
- Kıvılcım kapıyı açmak için dışarıda bırakıldı; Kor diye bir robot var.
- Ece robota "beni bul" diye yazdı; Kıvılcım onun eşyalarını topluyor.
- Zaman daralıyor: bağlantı kopacak, kapsüller zayıflıyor.
- Yeni soru: **Kıvılcım bensiz yapabilir mi?** (Perde 3)

## Hikâye kitabına işlenecekler

- Kayıt 5b (Bölge 10, Dünya/mavi yıldız) kayıtlar listesine eklenecek.
- Yeni adlar: drone **Serçe**. Motif: Kıvılcım'ın çantası.
- Kayıt 0 (son kayıt) ile Ece'nin yıldızı arasındaki bağ: Dünya = mavi yıldız.
