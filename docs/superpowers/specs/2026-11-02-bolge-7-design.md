# Bölge 7 — Kanyon dibi, eski nehir yatağı (Bölüm 61-70) · Tasarım

> 2026-11-02 · Ragıp + Claude (Opus 5.5). Ragıp her kararı tek tek seçti.
> Dayandığı belgeler: `docs/tasarim/senaryo-perde-2.md` (Bölge 7), `docs/tasarim/hikaye-kitabi.md`, `docs/superpowers/specs/2026-10-08-bolge-6-design.md`, `docs/tasarim/bolum-dosyasi.md`.
> Konu: **parametreli fonksiyonlar** (`def git(yon, kac=1):`). `return` Bölge 8'de.

## Kararlar (özet)

| # | Karar | Neden |
|---|---|---|
| 1 | **Serçe "göz" olur:** oyuncu yine yalnızca Kıvılcım'ı kodlar; Serçe yukarıdan bakıp yeni komut **`scout()`** ile izin yönünü söyler (örn. `East`). | Senaryodaki "Serçe yukarıdan, Kıvılcım aşağıdan" oyuna girer; parametre "dışarıdan gelen bilgiyi rutine vermek" olarak doğal bir anlam kazanır. Tam oynanır Serçe (iki kod, `serce.move`) seçilmedi: noktalı yazım henüz öğretilmedi, telefonda iki kod sığmaz, parametre dersini gölgeler. |
| 2 | **Yol ayrımları:** haritada iki kol da görünür, izlerin hangi koldan gittiğini yalnızca Serçe görür. Hedef izin bittiği yerdir, varılınca belirir. | Harita her şeyi gösterirse Serçe'ye sormak anlamsızlaşır (toz süresi / makara uzunluğu gibi gizli bilgi). |
| 3 | **Değişen yol (68-70):** bölüm dosyasında 2-3 iz; oyun kodu hepsinde dener, görev hepsinde hedefe varınca biter. Kod bir yolda takılırsa ekranda **o yol** oynatılır. | Ezber ve deneme-yanılma işe yaramaz; kod gerçekten "ne derse onu yap" olmak zorunda (gerçek programlamanın özü). Yalnızca son üç bölümde: zorluk eğrisi. |
| 4 | **Parametre şartı:** rutinin parametresi olmalı ve **en az iki farklı değerle** çağrılmalı. | Yoksa oyuncu `git(East)`'i hep aynı değerle çağırır, parametre anlamsız kalır. Bölge 6'nın rutin şartının devamı. |
| 5 | Toplanan nesne: **yuvarlak nehir çakılı** (`C`). Mavi (turkuaz) kalem yalnızca 68'de hikâye eşyası (kapanış sahnesinde alınır, `collect()` ile değil). | Gerçek bilim: Curiosity 2012'de suyun yuvarlattığı çakıllarla Mars'ta eski dere yatağını kanıtladı. Bölümde tek tür toplanır kuralı bozulmaz. |
| 6 | Kalem, duvar yazısı ve finalde Kıvılcım'ın gözleri **turkuaz** (senaryodaki "mavi" düzeltilir). | Turkuaz Ece'nin rengi (kapıdaki adı, direk 5, kurdele, Serçe yazısı). "Mavi" Bölge 10'daki mavi yıldıza (Dünya) kalır, iki motif karışmaz. |
| 7 | Kayıt 3'ün **uzun metni** + **dönüş izi**. | Uzun metin kalemi ve duvarı önceden sezdirir. Çelişki ("kalem elindeydi ama yolda bulundu") dönüş iziyle çözülür: kalem Defne Ece'yi taşıyarak dönerken düştü (bkz. 2.3). |
| 8 | "KIVILCIM BENİ BUL" Bölge 6'nın "bir sahnede en fazla tek okunur kelime" kuralının **bilinçli tek istisnası**. | Perde 2'nin en güçlü anı. Kural geri kalan her yerde geçerli. |
| 9 | **"Kayıtlar" sayfası** Bölge 7'de yapılır (Bölge 6'dan kalmıştı). | Kayıt 3 ile kayıtlar birikmeye başlıyor. |
| 10 | Ortam: **sabah, dip gölgede**; geniş nehir yatağı, duvarlar uzakta (67-70'te yalnızca arkada tek duvar yaklaşır). | "Serçe yukarıda güneşte, Kıvılcım aşağıda gölgede" görüntüye çevrilir. Bölge 6'da ekranın tepesine yükselen dev duvarlar beğenilmemişti. |

## İş sırası (her parça yeni sohbette)

1. **Bulmacalar + yeni kurallar** — Sonnet 5.5 · medium (~4 sa). "1. Bulmacalar". Değişen yol denetimi zorlaşırsa high.
2. **Hikâye metinleri + Kayıtlar sayfası + belgeler** — Sonnet 5.5 · medium (~2-3 sa). "2. Hikâye".
3. **Görünüş** — Opus 5.5 · high, ikiye bölünür: **3a** ortam + nesneler + iz/Serçe ışığı (~4-5 sa), **3b** üç sahne (~3-4 sa). "3. Görünüş". Ara görüntüler **Ragıp'a** gösterilir (hikâye görüntüleri Ragıp'ın alanı); onaysız sonraki adıma geçilmez.

---

## 1. Bulmacalar

### 1.1 Parametre şartı (rutin şartına ek)

Bölüm dosyasına isteğe bağlı alan:

| Alan | Anlamı |
|---|---|
| `parametre_sarti` | `true` ise rutin şartına giren her rutin (`rutinler` içindekiler ya da `rutin_sayisi` ile sayılanlar) **en az bir parametre** almalı ve kod çalışırken **en az iki farklı değerle** çağrılmış olmalı (herhangi bir parametrenin değeri farklıysa sayılır; `git(East)` ile `git(East, 3)` farklıdır). |

- Bölge 6'nın kuralları aynen geçerli (tanımlı olmalı, en az 2 kez çalışmalı). Sayım kodun gerçek yürütülmesinden gelir; dünya kuralları saf C#'ta (`RoutineRule` / `ProgramRun`).
- Mesajlar (Türkçe, sade):
  - Parametresiz: "`git` rutininin parametresi yok. Rutine nereye gideceğini söylemek için `def git(yon):` gibi bir bilgi kutusu ekle."
  - Hep aynı değer: "`git` hep aynı bilgiyle çağrıldı (`East`). Parametre, rutine her seferinde farklı bilgi vermek içindir."
- Varsayılan değerli parametre (`kac=1`) parametre sayılır.

### 1.2 Serçe ve iz: `scout()`

- Yeni komut **`scout()`** (World.AllCommands'a eklenir). Türkçe açıklaması: "Serçe yukarıdan bakar, izin bu kareden hangi yöne gittiğini söyler." Döndürdüğü değer bir yön: `North`, `East`, `South`, `West` (yönler zaten metin olarak tutuluyor, `move(scout())` doğrudan çalışır).
- Bölüm dosyasına alan:

| Alan | Anlamı |
|---|---|
| `izler` | Göç izinin yolu: robotun başlangıç karesinden adım adım yön listesi, örn. `[["South","South","East","East"]]`. **Tek iz** = sabit yol. **2-3 iz** = değişen yol (1.3). Alan varsa `scout()` açılır, haritada `H` yazılmaz: hedef izin son karesidir. |

- `scout()` robotun bulunduğu karede izin bir sonraki adımının yönünü verir.
  - Robot izin üstünde değilse oyun kuralıyla durur: "Serçe izi göremiyor: Kıvılcım izin üstünde değil." (Python hatası değil.)
  - İzin son karesinde çağrılırsa: "İz burada bitiyor; Serçe gösterecek yön bulamadı."
- Görev tamam: izin son karesine varmak + bölümün öteki şartları (toplama, onarma, rutin/parametre şartı).
- Denetleyici (`Level` doğrulaması): iz yalnızca boş/geçilebilir karelerden gider (kaya, panel üstünden değil; toz olabilir), alanın dışına çıkmaz, kendini kesmez; izler aynı uzunlukta (1.3); `cozum` her izde görevi bitirir.

### 1.3 Değişen yol (68-70)

- `izler` 2-3 yol içerirse oyun oyuncunun kodunu **her yolda ayrı ayrı** çalıştırır (`ProgramRun.Execute` yol başına; dünya her seferinde baştan kurulur).
- Hepsi başarılıysa görev biter; ekranda yollardan biri oynatılır (her "Çalıştır"da sırayla bir sonraki: oyuncu yolların değiştiğini görür).
- Biri başarısızsa ekranda **ilk başarısız yol** oynatılır ve damga: "Serçe bu sefer başka bir kol gördü." Hata/takılma açıklaması o yolun koşusundan gelir.
- **Bütün izler aynı uzunlukta** olur: oyuncu `for i in range(7):` gibi sabit sayılı döngü yazabilir (döngüyü bitirmek için yeni bir kavram, örn. `None`, gerekmez). Görev metni sayıyı söyler ("Serçe 7 adımlık iz görüyor").
- Toplanacak çakıl ve onarılacak panel **bütün yolların ortak parçasında** durur (yoksa bir yolda eksik kalır). Toz kollarda olabilir (`while dust_here(): wait()` her yolda çalışır).
- Her değişen yol bölümünde `tipik_hatalar` içinde **ezber çözüm** (ilk yolun yönleri elle yazılmış) bulunur; test onun en az bir yolda takıldığını denetler.

### 1.4 Yeni nesne: nehir çakılı

- Haritada `C`, `collect()` ile toplanır (`Collectible.Pebble`, Türkçe ad "çakıl"). Yeni bölge `Region.RiverBed` (61-70); `Regions.Item(RiverBed) = Pebble`.
- Görünüşü 3a'ya kadar en yakın var olan bölgeninki (Bölge 6'daki gibi).
- Bir bölümde tek tür toplanır kuralı geçerli. Toz (`Z`), panel (`1`-`9`), kaya, makara + `report` serbestçe kullanılır.

### 1.5 Bölümler

Haritalar 6×6. Ad ve ayrıntılar plan sırasında netleşir; öğretim sırası değişmez.

| Bölüm | Ad (taslak) | Öğrettiği | Şart | Öğeler |
|---|---|---|---|---|
| 61 | Nehir yatağı | **İlk parametre (yön).** 60'taki iki benzer iş tek rutinde: `def git(yon): move(yon)` + toz beklemesi; `git(South)`, `git(East)`. Tipik hatalar: parametreyi tanımda unutup çağrıda vermek (`TypeError`), tanımda yazıp çağrıda unutmak. | `git` + parametre şartı | toz, çakıl, hedef |
| 62 | Serçe yukarıdan | **`scout()` ilk kez:** bir komutun sonucu rutine verilir: `git(scout())`. Tek yol ayrımı. Başlangıç kodu Ece'nin notuyla başlar (2.3). | parametre şartı | tek iz |
| 63 | Göç izleri | **Sayı parametresi:** `def ilerle(kac): for i in range(kac): ...` | `ilerle` + parametre şartı | çakıl, hedef |
| 64 | Kayıt 3 | **İki parametre:** `git(yon, kac)`. Tipik hatalar: sırayı karıştırmak (`git(3, East)`), birini unutmak. | parametre şartı | çakıl, toz, hedef |
| 65 | İzler ayrılıyor | Serçe'nin cevabı **bir değişkene** konur, sonra verilir: `yon = scout()` → `git(yon, 2)`. Değişken adı ile parametre adı farklı olabilir. | parametre şartı | tek iz |
| 66 | İkinci iz | **Varsayılan değer:** `def git(yon, kac=1):`; `git(East)` ve `git(South, 3)`. | parametre şartı | panel, çakıl, hedef |
| 67 | Yan kanyon | **Döngü içinde Serçe'ye sorma:** `for i in range(n): git(scout())`, çok sayıda yol ayrımı. | parametre şartı | tek iz, toz |
| 68 | Boya kalemi | **Değişen yol başlar.** Rutin içinden parametreli rutin çağrılır (`def kol(yon): git(yon, 2) ...`). | `rutin_sayisi: 2` + parametre şartı | 2 iz |
| 69 | El izleri | Değişen yol + toz + panel; rutin adları oyuncuda. | `rutin_sayisi: 1` + parametre şartı | 2-3 iz, toz, panel |
| 70 | KIVILCIM BENİ BUL (final) | Hepsi birlikte, değişen yol. | `rutin_sayisi: 2` + parametre şartı | 3 iz, toz, panel |

- Her bölümde `tipik_hatalar` içinde **parametresiz çözüm** (iki ayrı rutin ya da kopyala-yapıştır) bulunur (61-67); test onun görevi bitirmediğini denetler.
- `parcalar` (acemi düğmeleri): `def git(yon):`, `git(East)`, `scout()`, `yon = scout()`, `def git(yon, kac=1):` gibi.
- Zorluk eğrisi: 61 en kolay (başlangıç kodu tanım satırını verebilir); 69'dan sonra ad ve yapı oyuncuda.

### 1.6 Sınavlar ve sözlük

- `sinav-65`: parametre nedir; çağrıda değerlerin sırası; eksik değerle çağırınca ne olur (`TypeError`); bir komutun sonucunu rutine vermek.
- `sinav-70`: varsayılan değer; değişken adı ile parametre adı farklı olabilir mi; neden ezber yön listesi değişen yolda çalışmaz.
- Çerçeve Bölge 6'daki gibi "KIVILCIM · KENDİNİ DENETLİYOR".
- Sözlük: yeni Python kelimesi yok. `def` sayfasına parametre ve varsayılan değer açıklaması eklenir; `scout` komut açıklaması (komut sözlüğü/ipucu nerede tutuluyorsa).

### 1.7 Denetim

- `dotnet test`: yeni testler: parametre şartı (parametresiz, hep aynı değer, iki farklı değer, varsayılan değer, mesajlar); `scout()` (iz üstünde, izin dışında, iz sonunda); değişen yol (hepsi başarılı, biri başarısız → o yol seçilir, ezber çözüm takılır); bölüm denetleyicisi (iz duvardan geçmez, izler eşit uzunlukta, çözüm her izde biter) + bütün bölüm dosyaları.
- Windows paketi hatasız; Bölüm 61-70 `-shots -bolum 61` görüntüleriyle "Tamamlandı"; yedi fare denetimi TAMAM.

---

## 2. Hikâye

Sol 1.088'den başlar (Bölge 6 sol 1.087'de bitti). Bütün metinler yeni `docs/tasarim/senaryo-bolge-07.md`'ye yazılır. Güncellenecekler: `senaryo-perde-2.md` (Bölge 7: "mavi" → turkuaz, gözler turkuaz, dönüş izi, Serçe'nin rolü), `hikaye-kitabi.md` (Kayıt 3'ün uzun metni, çanta: kalem turkuaz, motif tablosu, `scout`).

### 2.1 Bölüm bölüm

| Bölüm | Giriş damgası | Bitiş damgası | Olan |
|---|---|---|---|
| 61 | Sol 1.088 · Kanyon dibi | Zemin: kurumuş çamur · eski su izi | Yarıktaki patikanın bittiği yer arkada; kuru nehir yatağı. |
| 62 | Sol 1.088 · Serçe yukarıda | Yol ayrımı: 1 · Serçe gördü | İlk `scout()`; başlangıç kodunda Ece'nin notu. |
| 63 | Sol 1.089 · Kumda izler | İzler: 23 büyük, 1 küçük | Kapanış: Serçe'nin gözünden kuş bakışı (3b). |
| 64 | Sol 1.089 · Kayıt 3 | Örnek çakıl: n | Bölüm başında **Kayıt 3** (`RecordingScene`). |
| 65 | Sol 1.090 · İzler ayrılıyor | Küçük iz: yan kanyona | Küçük iz gruptan ayrılır (nedeni söylenmez; 70'te anlaşılır). |
| 66 | Sol 1.090 · İkinci iz | Adım aralığı: 1,6 m | Büyük iz peşinden, koşarak. Dönüş izi ilk kez görünür (2.3). |
| 67 | Sol 1.091 · Yan kanyon | Kanyon genişliği: 4 m | Dar, kıvrımlı; küçük iz taştan taşa zıplıyor. |
| 68 | Sol 1.091 · İz: tek kişi, derin | Bulundu: boya kalemi | Kapanış: kalem çantaya (3b). |
| 69 | Sol 1.092 · Duvarda el izleri | El izi: 7 küçük, 1 büyük | Duvar boyunca çocuk boyunda el izleri. |
| 70 | Sol 1.092 · İzin sonu | Sonraki: araştırma istasyonu | Final: "KIVILCIM BENİ BUL" (3b). |

- Etiket: "BÖLGE 7 · KANYON DİBİ"; damgalar Bölge 6'nın biçiminde (konuşma değil, ölçüm/kayıt). Kesin sayılar (çakıl, el izi) plan sırasında haritalarla eşleşir.

### 2.2 Kayıt 3 (Bölüm 64 başı)

> "Ece yine önden koştu. Bir anlığına onu kaybettim. Sonra buldum; bir duvarın önünde, elinde boya kalemi. Ona kızamadım."

`hikaye-kitabi.md`'deki kısa metin ("Ece yine önden koştu. Ona kızamıyorum.") bununla değiştirilir.

### 2.3 İzler ve gizli ayrıntılar

| Bölüm | İz |
|---|---|
| 62 | Başlangıç kodunun ilk satırı Ece'nin notu: `# serçe yukarıdan bakar, sen yürü :) - E.` Annesi "D.A." diye imzalardı; kızı "E.". Ece'nin oyundaki ilk "sesi". |
| 63 | Kuş bakışında geniş göç izi; bir çift küçük iz büyük bir izin hemen yanında yürüyor (el ele). Damga "İzler: 23 büyük, 1 küçük" (koloni 24 kişi). |
| 65 | Küçük iz gruptan ayrılır. Neden: robota mesaj bırakmak (70'te anlaşılır, kimse söylemez). |
| 66-69 | **Dönüş izi:** iki çift büyük iz. Gidiş: aralıklı (koşuyor). Dönüş: yavaş, **daha derin**, yanında küçük iz **yok** — Defne Ece'yi kucağında taşıdı. Söylenmez. |
| 67 | Küçük iz taştan taşa zıplıyor: kaçarken bile oyun oynayan çocuk. |
| 68 | Turkuaz kalem dönüş izinin yanında; ucu çok aşınmış (çok çizmiş). Kıvılcım'ın çantasına, kurdelenin yanına; 68'den sonra sırtında görünür. |
| 69 | Duvarda çocuk boyunda turkuaz el izleri; sonuncusunun yanında bir **büyük el izi** (Defne'nin onu bulduğu yer, Kayıt 3). |
| 70 | Turkuaz robot çizimi + **"KIVILCIM BENİ BUL"** (çocuk eli, eğri, harfler farklı boyda). Çizimin yanında göğüsteki çizimin **aynı çiçeği** ve **dokuz çizgili küçük bir pasta**: o gün (sol 861) Ece'nin 9. doğum günüydü (Bölge 3'ten bilinir, kimse hatırlatmaz). |

- Nehir yatağının kıvrımları yukarıdan bir harita gibi (63 kuş bakışı).
- **Ragıp'a/Hamza'ya not (dizi):** dönüş izi, el izleri ve pasta sözsüz anlatım; videoda da yazısız çalışır.

### 2.4 "Kayıtlar" sayfası

- Menüde "Kayıtlar": dinlenmiş Defne kayıtlarının listesi (numara, bölge, ilk birkaç kelime); dokununca kayıt ekranı (`RecordingScene`) yeniden oynar. Dinlenmemiş kayıtlar listede görünmez (sürpriz bozulmaz).
- Arayüz dünya kurallarına bağımlı olmaz: hangi kaydın dinlendiği bilgisi bugünkü kayıt saklama yerinden okunur.

### 2.5 Çelişki kontrolü (`hikaye-kitabi.md` listesi)

Ses kaydı yalnızca Defne; robotlar konuşmaz (damgalar ve Serçe'nin ışığı konuşma değil; `scout()` bir yön döndürür); Kıvılcım uçmaz; Serçe uçar; kimse ölmez; kayıt öğrenciye ("sen") konuşur; Ece'nin rengi turkuaz; sol 861 = Ece'nin 9. doğum günü = göç günü.

---

## 3. Görünüş ve sahneler

Her şey kodla çizilir; hazır model / büyük doku yok (paket boyutu). Bölge 6'nın kuralları geçerli: sis yok (derinlik gölgeyle), nesneler zemine basar (`TerrainHeight`), dünyada bilgisayar yazısı yok, bir sahnede en fazla tek okunur kelime (**tek istisna: 70'teki duvar yazısı**), robotlar konuşmaz.

### 3a.1 Ortam

- **Saat: sabah.** Güneş henüz dibe inmemiş: duvarların yalnızca üst şeridi turuncu aydınlık, dip serin morumsu-kahve gölgede. Gökyüzü duvarların arasında. Görüntünün fikri: **Serçe yukarıda güneşte, Kıvılcım aşağıda gölgede.**
- **Duvarlar:** 61-66 geniş nehir yatağı; duvarlar uzakta, arka planda alçak, ufkun hemen üstünde (üstteki yazılara/düğmelere uzanmaz). 67-70 yan kanyon: **yalnızca arkada tek duvar** yaklaşır (69-70'in konusu); yanlar ekranın kenarında kesilir, tepeye yükselmez.
- **Zemin:** kuru nehir yatağı; açık renkli, çatlak çamur tabakaları (Curiosity'nin Mars'ta bulduğu gibi), yuvarlak çakıllar, kıvrımlı eski akıntı izleri. Bölge 6'nın koyu pas toprağından açıkça farklı.
- **Yapım:** arka plan Bölge 6 gibi kodla boyanmış tek resim (`BackdropCache`); `RegionLook.RiverBed`; zemin `Ground.shader` yeni dip zemini + Performans modu sürümü.

### 3a.2 İzler, yol ayrımı, Serçe

- **Ayak izleri** zemine işlenmiş hafif çukurlar (küçük/büyük, gidiş/dönüş derinliği farklı). Göç izi (63) geniş bir şerit.
- **Yol ayrımında izler kumla örtülü** (iki kol aynı görünür). Gerçekte de eski izler yerden zor, yukarıdan kolay seçilir. `scout()` çağrılınca Serçe yol ayrımının üstüne süzülür, seçtiği kolu **kısa bir an aydınlatır** (yumuşak ışık lekesi); Kıvılcım yürüdükçe o koldaki izler belirir.
- **Hedef** yol ayrımlı bölümlerde görünmez; iki kolun sonu birbirine benzer. İzin sonuna varılınca belirir.
- **Serçe** alanın arka kenarının üstünde, Kıvılcım'dan yüksekte süzülür (güneşli bölgede); `scout()`'ta yol ayrımına gidip döner. Havada nesne yok kuralı Serçe için geçerli değil (uçar), ama alanın üst yazılarına girmez.
- `scout()` dünyada bir olay olarak kaydedilir (`ProgramRun`); arayüz yalnızca bu olayı gösterir (dünya kuralları arayüze bağımlı değil).

### 3a.3 Bölüm bölüm yerleşim

| Bölüm | Görünüş |
|---|---|
| 61-62 | Geniş nehir yatağı; arkada Bölge 6 finalindeki yarık patikasının dibe vardığı yer. |
| 63-64 | Göç izinin geniş şeridi alanın içinden geçer, arkada nehir kıvrımı. |
| 65-66 | Alanın bir kenarında yan kanyonun ağzı; küçük iz o yana sapar. 66'da gidiş ve dönüş izleri. |
| 67 | Yan kanyon: dar, kıvrımlı; arkada tek duvar; taşlar arasında zıplayan küçük iz. |
| 68 | Dönüş izinin yanında yerde turkuaz kalem. |
| 69 | Arka duvar boyunca çocuk boyunda turkuaz el izleri; sonuncusunun yanında büyük el izi. |
| 70 | Arka duvarda robot çizimi + "KIVILCIM BENİ BUL" + çiçek + dokuz çizgili pasta; telefonda okunacak boyda. |

### 3a.4 Nesneler

| Nesne | Durum | Görünüş |
|---|---|---|
| Nehir çakılı (`C`) | yeni | Yuvarlak, düzgün, açık renkli çakıl kümesi (suyun yuvarlattığı). |
| Turkuaz kalem | yeni | Kısa, ucu aşınmış boya kalemi; 68'den sonra Kıvılcım'ın çantasında kurdelenin yanında. **Ragıp'a haber:** kalıcı karakter görünüşü. |
| Ayak izleri, el izleri, duvar çizimi | yeni | 2.3 ve 3a.2'deki gibi; duvar yazısı elle boyanmış gibi (eğri, akmış boya). |
| Serçe | kalır | Bölge 6'daki model; yeni: yol ayrımında ışık lekesi. |

### 3b. Sahneler

Bölge 6'nın `CameraShot` ve `CanyonScenes` yapısı kullanılır; yeni sahneler ayrı bir dosyada (`RiverBedScenes.cs` gibi). `Oyun.cs`'e yalnızca çağrı satırları. Geri alma: "Tekrar" ve bölüm değişince her şey eski hâline.

#### 3b.1 Bölüm 63 — Kuş bakışı (her bitirişte, ~6 sn)

Kutlama → kırpışma → **Serçe'nin gözünden** tam yukarıdan bakış (55'teki yapı): eski nehrin kıvrımları bir harita gibi, geniş göç izi, büyük bir izin yanında yürüyen küçük iz; görüntü hafifçe salınır, yazı yok → kırpışma, kamera yerine döner. Bitiş damgası "İzler: 23 büyük, 1 küçük".

#### 3b.2 Bölüm 68 — Kalem (her bitirişte, ~3 sn)

Kıvılcım kaleme döner, eğilir, alır, çantasında kurdelenin yanına takar. Bitiş damgası "Bulundu: boya kalemi".

#### 3b.3 Bölüm 70 — KIVILCIM BENİ BUL (final, her bitirişte, ~18 sn)

Kutlama → Kıvılcım duvara yaklaşır, kamera duvara döner, yazı ve çizim okunur → **bütün sesler kesilir (~2 sn sessizlik)** → Kıvılcım elini duvara koyar: **küçük el, büyük el, robot eli yan yana** → kısa yakın çekim: göğsündeki çizim ile duvardaki çizim aynı elden → gözleri yumuşakça **turkuaz** yanar; Serçe yukarıda süzülür → son damga "Sonraki: araştırma istasyonu".

### 3c. Çalışma sırası ve denetim

Her adımın sonunda Ragıp'a görüntü gösterilir; **onaylamadan sonraki adıma geçilmez.** İlk adımda kaba çizimlerle (tarayıcı) seçenekler gösterilebilir (Bölge 6 3a'daki gibi).

1. Ortam: arka plan, ışık, zemin → Bölüm 61 ve 67 görüntüsü.
2. İzler, yol ayrımı, Serçe ışığı, hedefin belirmesi → Bölüm 62 ve 65 (oynarken ara kareler).
3. Kalem, el izleri, duvar çizimi/yazısı → Bölüm 68, 69, 70.
4. Sahneler 63, 68, 70 (3b) → ara kareler; `-sahneler` denetimine eklenir.
5. Son denetim: `dotnet test`; Windows paketi hatasız; 61-70 görüntüleri tek tek, Performans modunda en az iki bölüm; Bölge 6'nın görünüşü değişmedi; `SAHNE DENETIMI: TAMAM`; final ve klavye denetimleri TAMAM; `telefon-denemesi.md`'ye 61 ve 70 için `KARE:` satırı; görüntüler `docs/tasarim/bolge-7-gorunus-<tarih>/`.

## Kapsam dışı

Tam oynanır Serçe (iki robot kodu), `return` ve değer döndüren rutinler (Bölge 8), `None` ile döngü bitirme, Bölge 5'in kendi görünüşü, telefon testleri (ILERLEME.md listesi), `TestAllLevelsOpen` (Play Store öncesi).
