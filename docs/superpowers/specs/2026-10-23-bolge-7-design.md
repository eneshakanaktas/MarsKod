# Bölge 7 — Kanyon dibi, eski nehir yatağı (Bölüm 61-70) · Tasarım

> 2026-10-23 · Enes + Claude (Opus 5.5). Enes akışı, çizim biçimini (A) ve bölüm tablosunu onayladı.
> Dayandığı belgeler: `docs/tasarim/senaryo-perde-2.md` (Bölge 7), `docs/tasarim/hikaye-kitabi.md`, `docs/superpowers/specs/2026-10-08-bolge-6-design.md` (biçim, rutin şartı), `docs/tasarim/bolum-dosyasi.md`.
> Konu: **parametreli fonksiyonlar** (`def ok(boy):`). `return` Bölge 8'de.

## Neden (Enes'in tespiti)

60 bölüm boyunca oyun **mekanik ve görsel olarak** kendini tekrar ediyor: her bölge yeni bir arka plan ve yeni bir Python kelimesi getiriyor ama oyuncunun eli hep aynı işi yapıyor (6×6 karede yürü, topla, hedefe var). Her 10 bölümde "başa sarıyor" hissi doğuyor. Sorun ders değil, oynanış. Bölge 7 bu yüzden **yeni bir mekanik** getirir: **Kıvılcım çizer.** Bölge 8 ikinci bir yeni mekanik getirir (aşağıda, §6). Hedef: üst üste iki bölge "başa sarma" hissini kırsın.

Hedef kitle: 13 yaş ve üstü (genç, öğrenci, yetişkin), yazılım bilmeyen ya da az bilen. Ton sıcak ve sade, çocuksu değil.

## Kararlar (özet)

| # | Karar | Kimin | Neden |
|---|---|---|---|
| 1 | Bölge 7'nin yeni mekaniği **çizim**: Kıvılcım kalem inikken üstünden geçtiği kareleri boyar. | Enes | Ekranda yol değil resim ortaya çıkar; parametrenin etkisi gözle görülür (`ok(3)` küçük, `ok(5)` büyük). |
| 2 | Çizim biçimi **A: kare boyama** (piksel). Çizgi biçimi (B) seçilmedi. | Enes | Kıvılcım çapraz gidemez; ok başı ancak basamaklı karelerle çizilebilir. Bölgenin işaretleri ve finali ok. |
| 3 | Çizimin hikâyedeki sebebi: **Serçe ile konuşmak.** Robotlar konuşmaz; Kıvılcım kuma büyük işaretler çizer, Serçe yukarıdan okur ve o yöne keşfe uçar (kumsala uçaktan okunsun diye yazılan "SOS" gibi). | Claude önerdi, Enes onayladı | Parametreye doğal sebep: Serçe yükseldikçe işaret büyümeli. |
| 4 | Eski öğeler **karışır** (kaya, toz, toplama, hedef); bazı bölümlerde çizim hiç yoktur. | Enes ("yalnızca ok çizmek için mi kod yazılacak?") | Yalnızca çizim 10 bölümde yeni bir tekrar olur; kodu zenginleştiren eski öğeler. |
| 5 | **Parametre şartı:** bölüm, belirtilen rutinin bir sayı almasını ve **en az iki farklı değerle** çağrılmasını ister. | Claude önerdi, Enes onayladı | Yoksa oyuncu `ok_kucuk()` + `ok_buyuk()` yazıp parametreyi atlar. |
| 6 | Oyuncu Serçe'yi **kodlamaz** (iki robotu birlikte kodlamak ileride, ayrı bir bölgede). | Enes | Enes ilgi duydu ama ileriye bıraktı. |
| 7 | Bölge 8'in ana fikri: **değişen harita** (kod üç haritada denenir). Bu belgede yalnızca not; tasarımı ayrı. | Enes | Oyuncu yolu tarif etmeyi bırakıp kural yazmayı öğrenir; `return` ile birlikte iyi oturur. |
| 8 | Alan **6×6'da kalır**; çizimler sığmazsa (planda ya da görüntüde görülürse) 7×7/8×8'e büyütme ayrıca Enes'e sorulur. | Claude'un kararı | Bütün bölümler 6×6; büyütmek kamera ve telefon ölçeğini etkiler. |
| 9 | Komut adları eski komutlar gibi İngilizce: `pen_down()`, `pen_up()`. | Claude'un kararı | `move`, `collect`, `report` ile aynı dil. |

## İş sırası (her parça yeni sohbette, Bölge 6 gibi)

1. **Bulmacalar + çizim düzeneği** — Sonnet 5.5 · medium (zorlaşırsa high). §1.
2. **Hikâye metinleri, kayıt 3, "Kayıtlar" sayfası** — Sonnet 5.5 · medium. §2.
3. **Görünüş ve sahneler** — Opus 5.5 · high; gerekirse 3a (ortam, nesneler) / 3b (sahneler) diye bölünür. §3. Görsel işte ara görüntü Enes'e gösterilir, onaysız ilerlenmez.

Her parçadan önce yapım planı yazılır (`docs/superpowers/plans/`). Yardımcı (subagent) yok: Claude işi kendisi yapar (Enes'in kararı).

---

## 1. Bulmacalar ve çizim düzeneği

### 1.1 Çizim (yeni dünya kuralı, saf C#)

- **Kalem durumu:** Kıvılcım'ın kalemi başta **kalkık**tır. `pen_down()` kalemi indirir **ve bulunduğu kareyi boyar**; kalem inikken her `move` girilen kareyi boyar. `pen_up()` kalemi kaldırır. Kalem inik/kalkık iken iki kez aynı komut hata değildir.
- **Boyanacak kareler:** bölüm haritasında yeni bir harf ile işaretlenir (harf plan sırasında seçilir; var olan harflerle çakışmaz: `R . K Z H 1-9 B E P M D` dolu). Bir kare hem boyanacak hem kaya olamaz; toz ve hedef olabilir (karar planda, denetleyici kuralı yazılır).
- **Görev tamamlanma şartı (çizim olan bölümde):** boyanacak karelerin **hepsi** boyanmış ve **fazladan hiçbir kare** boyanmamış olmalı. Haritada `H` varsa ayrıca hedefte bitirilmeli; diğer şartlar (toplama, rutin/parametre şartı) eskisi gibi eklenir.
- **Mesajlar (Türkçe, sade; ekte örnekler):**
  - Eksik: "İşaret yarım kaldı: 3 kare boyanmadı."
  - Fazla: "Fazladan 2 kare boyadın, işaret bozuldu. Yürürken boyamamak için `pen_up()`."
  - Hiç boyamadı: "Kalem hiç inmedi. Çizmek için `pen_down()`."
- **"Tekrar":** kum temizlenir (rüzgâr siler); boyalar sıfırlanır.
- **Kayda geçme:** `ProgramRun.Execute` olanları kaydederken boyama da bir olay olarak kaydedilir; arayüz bunu oynatır (adım adım çalıştırmada da). Dünya kuralları arayüze bağımlı olmaz.
- `pen_down`/`pen_up` yalnızca çizimli bölümlerin `komutlar` listesinde açılır (eski bölümlerde görünmez).
- 68'den sonra boya mavi (Kıvılcım kalemi buldu); 61-67'de koyu kum oluğu. Bu bir görünüş farkıdır; kural aynı. Renk bölüme bağlıdır, oyuncunun ilerlemesine değil: 61-67 sonradan yeniden oynansa da kum oluğu kalır (Claude'un kararı). Bölüm dosyası hangisinin kullanıldığını söyleyebilir (alan adı planda).
- Kalem inikken daha önce boyanmış kareden yeniden geçmek "fazla" sayılmaz. Başlangıç karesi de boyanacak kare olabilir (`pen_down()` onu boyar).
- 63'teki `yuru(yon, adim)` yönü (`East` gibi) değer olarak geçirir; motorun yönleri değer olarak taşıyabildiği planın ilk adımında denenir, taşıyamıyorsa 63 tek parametreye (`adim`) iner ve iki parametre 64'e kayar.

### 1.2 Parametre şartı (yeni oyun kuralı)

Bölüm dosyasına isteğe bağlı yeni alan (adı planda kesinleşir, örn. `parametreli_rutinler`): adları verilen rutinlerin her biri

1. `def` ile **en az bir parametreyle** tanımlanmış,
2. kod çalışırken **en az iki farklı argüman değeriyle** çağrılmış olmalı.

- Var olan `rutinler` / `rutin_sayisi` (`RoutineRule.cs`) ile birlikte kullanılabilir; aynı yerde genişletilir (tekrar eden mantık yazılmaz).
- Mesajlar: tanımsız → var olan mesaj; parametresiz tanım → "`ok` rutini bir sayı almıyor. `def ok(boy):` diye yaz."; hep aynı değer → "`ok` hep aynı sayıyla çağrıldı. Farklı boylar için farklı sayı ver: `ok(3)`, `ok(5)`."
- Motor parametreli fonksiyonları zaten destekliyor (`Interpreter`, `Parser`); yeni dil özelliği gerekmez. Çağrı değerleri dünya tarafında sayılır.

### 1.3 Yeni nesne ve bölge

- Yeni bölge (örn. `Region.Riverbed`, 61-70). Toplanan nesne: **mavi boya kalemi** (yalnızca 68'de; testler "her bölgede tek toplanan nesne" ister). Diğer bölümlerde toplama yoksa sorun değil; varsa kalem kuralına uyulur.
- Görünüş §3'e kadar en yakın var olan bölgeninki (Kanyon).

### 1.4 Bölümler

Haritalar 6×6. Adlar ve haritalar plan sırasında netleşir; **öğretim sırası değişmez**.

| Bölüm | Olay | Öğrettiği | Şart | Öğeler |
|---|---|---|---|---|
| 61 | Serçe'ye iniş pisti: farklı boylarda üç çizgi | İlk parametre: `cizgi(boy)`; aynı rutin, üç boy | `cizgi` parametreli | çizim |
| 62 | Serçe'ye ilk işaret; Serçe yükselir, daha büyüğü çizilir | `ok(boy)` iki boyda | `ok` parametreli | çizim |
| 63 | Göç izleri takip edilir (geniş bir grup), **çizim yok** | İki parametre: `yuru(yon, adim)` | `yuru` parametreli | kaya, hedef |
| 64 | Kayıt 3. Kayalar arasına kare çizilir | Rutin içinde parametreli rutin: `kare(boy)` → `cizgi(boy)` | `kare` + `cizgi` | çizim, kaya |
| 65 | Küçük izlerin ayrıldığı yere "çarpı"; ortada kaya | Kalemi kaldırıp dolaşmak (`pen_up`, `rock_ahead`, `if`) | parametreli rutin | çizim, kaya |
| 66 | Büyük izler boyunca büyüyen oklar | Döngü değişkenini rutine vermek: `for i in range(...): ok(i)` | `ok` parametreli | çizim |
| 67 | Yan kanyon, dar ve kıvrımlı; Serçe giremez. **Çizim yok** | Parametreli yürüme + bekleme (`while dust_here()`) | parametreli rutin | toz, kaya, hedef |
| 68 | Mavi kalem toplanır; ilk mavi çizim | Toplama + çizim birlikte | parametreli rutin | kalem, çizim |
| 69 | Duvar boyunca küçük el izleri | İki rutin, farklı sayılarla | 2 parametreli rutin | çizim, toz |
| 70 · final | Ece'nin "KIVILCIM BENİ BUL" yazısının altına mavi ok | Hepsi birlikte | 2 parametreli rutin | çizim, kaya, toz, hedef |

- Her bölümde `tipik_hatalar` içinde **parametresiz çözüm** (iki ayrı rutin ya da kopyala-yapıştır) bulunur; test onun görevi bitirmediğini denetler. Çizimli bölümlerde ayrıca "fazla kare boyayan" bir hata örneği.
- `python_kelimeleri`: yeni kelime yok (parametre bir kelime değil); acemi düğmelerine (`parcalar`) `def ok(boy):`, `ok(3)`, `pen_down()`, `pen_up()` gibi parçalar eklenir.
- Zorluk: 61 en kolay (başlangıç kodu rutini hazır verebilir, oyuncu yalnızca çağrıları yazar); 66'dan sonra ad ve yapı oyuncuda.
- **Bilerek uzak durulan:** varsayılan parametre değerleri (`def ok(boy=3)`), anahtar sözcüklü çağrı (`ok(boy=3)`), `return`. Oyuncu kullanırsa motorun desteklediği kadar çalışır; bölümler buna dayanmaz.

### 1.5 Sınavlar ve sözlük

- `sinav-65` (parametre nedir; tanımdaki ad ile çağrıdaki değer; parametre sayısı uymazsa hata), `sinav-70` (iki parametre; rutin içinden parametreli rutin; döngü değişkenini vermek). Çerçeve Bölge 6'daki gibi "KIVILCIM · KENDİNİ DENETLİYOR".
- Sözlük: `def` sayfasına parametre eklenir ya da yeni bir "parametre" sayfası (sözlük sayfaları bir kelimeyle açılır; hangi kelimeyle açılacağı planda: `def` sayfasının ikinci bölümü olarak eklemek daha doğal olabilir).

### 1.6 Denetim

- `dotnet test`: çizim kuralı (boya, kalem, eksik/fazla, Tekrar), parametre şartı (parametresiz, tek değer, iki değer, döngüden, rutin içinden, mesajlar), bütün bölüm dosyaları (var olan denetim: `cozum` görevi bitirir, `tipik_hatalar` bitirmez).
- Windows paketi hatasız; `-shots -bolum 61` görüntüleriyle 61-70 "Tamamlandı"; yedi arayüz denetimi TAMAM; Bölge 6 ve öncesine sızma yok (51/60 görüntüsü değişmedi).

---

## 2. Hikâye

- `senaryo-perde-2.md` Bölge 7 bölümü bu belgeye göre güncellenir (çizimin sebebi: Serçe ile işaretle konuşmak; Kıvılcım koloninin "iz bırakma" dilini sürdürür: Defne'nin turuncu okları → Ece'nin mavi yazısı → Kıvılcım'ın mavi oku). `hikaye-kitabi.md`'ye kısa not.
- Perde 2 biçimi aynen: üst etiket "BÖLGE 7 · NEHİR YATAĞI" (ad planda), büyük başlıkta görev adı, giriş/bitiş yazıları **kayıt damgası** (örn. "Sol 1.0xx · Nehir yatağı", "İşaret: 2/3").
- **Kayıt 3** (64 başında, var olan `RecordingScene` ile): "Ece yine önden koştu. Bir anlığına onu kaybettim. Sonra buldum; bir duvarın önünde, elinde boya kalemi. Ona kızamadım."
- **"Kayıtlar" sayfası** bu bölgede yapılır (Bölge 6 kararı 8'den kaldı): Defne'nin kayıtları (Kayıt 1-3) tek sayfada, sırayla, yeniden dinlenebilir. Yerleşim planda (bölüm seçme ekranından erişim önerilir).
- Kıvılcım'ın çantası: 68'den sonra mavi kalem robotun üstünde küçük bir ayrıntı olarak görünür (senaryodaki çanta motifi). Kıvılcım'ın görünüşü bir hikâye karakteri görünüşüdür: çanta ayrıntısı senaryoda var, başka değişiklik yapılmaz.
- Dizide (senaryodaki satır) değişmez.

## 3. Görünüş ve sahneler

- **Ortam:** kanyonun dibi, kurumuş eski nehir yatağı. İnce kum, kıvrımlı eski su izleri (Serçe'nin yukarıdan gördüğü kıvrımlar harita gibi), iki yanda yüksek kaya duvarları. 63-66'da kumda ayak izleri (geniş grup; sonra küçük izler ayrılır, büyük izler peşinden). 67'den sonra dar yan kanyon. Bölge 6'nın kuralları geçerli: gerçekçi ışık/gölge, nesneler yere basar, ufuk geçişinde sıçrama yok, alan arkasındaki nesneler yakın ve alçak.
- **Çizim görünüşü:** boyanacak kareler zeminde silik turuncu (kesik kenarlı); boyanmış kare 61-67'de koyu kum oluğu (kuma çizilmiş gibi), 68-70'de mavi boya. Boyama oynanırken kare yumuşakça belirir (adım adım çalıştırmada da).
- **Serçe'nin anı (çizimli bölümler):** görev bitince kamera ~1 sn Serçe'nin gözünden yukarı çıkar (55'teki kuşbakışı `BirdView` yeniden kullanılır), oyuncu işaretini kuşbakışı görür, Serçe işaretin gösterdiği yöne uçar. Kısa ve atlanabilir olmalı; her bölümde aynı uzunlukta. (Claude'un önerisi; görsel parçada Enes'e gösterilir.)
- **Final sahnesi (70):** alanın arka kenarı yan kanyonun duvarıdır; duvarda Ece'nin mavi robot çizimi ve çocuk eliyle "KIVILCIM BENİ BUL". Oyuncunun kodu oku duvarın önündeki zemine, yazının altına çizer (Claude'un kararı: Kıvılcım duvara tırmanmaz, alan zeminde kalır) → uzun sessizlik → Kıvılcım elini çizimin üstüne koyar → gözleri yumuşak mavi yanar. Söz yok.
  - **Dikkat:** "gözleri mavi yanar" parlama gerektirir; oyundaki parlama (Emissive) sorunu hâlâ açık. Serçe'nin gözleri gibi ışıktan etkilenmeyen malzemeyle çözülebilir ya da parlama kararı önce verilir (telefon gelince; bkz. ILERLEME).
- Görsel parçada telefon boyutunda görüntü (450×975) gösterilir; 6×6 alanda çizimler okunuyor mu bakılır (karar 8).

## 4. Hedefler / hedef olmayanlar

- **Başarı:** oyuncu Bölge 7'de "yeni bir şey yapıyorum" der; parametreyi, çizimin boyu değişince görerek kavrar; final duygusal olarak Bölge 6 finalinden güçlüdür.
- **Hedef değil:** oyuncunun Serçe'yi kodlaması; çapraz hareket; serbest çizim (oyuncunun kendi resmini çizmesi); alanı büyütmek (karar 8'e kadar).
- **Başarısızlık sayılır:** bölümler "yine yürü-topla" gibi hissettiriyorsa; çizim yalnızca ok olup tekrar ediyorsa; parametre şartı olmadan geçilebiliyorsa; eski bölümlerin görünüşü ya da kuralları bozulursa.

## 5. Kısıtlar

- Dünya ve motor saf C# (UnityEngine yok), C# 9 / .NET Standard 2.1; `motor-test` ile test.
- Kod kalitesi kuralları (CLAUDE.md): tek iş, tekrar yok, gevşek bağlılık, ölçülü soyutlama. Çizim kuralı ayrı bir parça olarak eklenir; `World`'ü şişirmez.
- Bölüm-özel istek diğer bölümlere yayılmaz (Bölge 6'daki kaya dersi).
- Telefon şu an yok: telefona bağlı denemeler ertelendi; Bölge 7 bitince Enes'e telefon testleri hatırlatılır.

## 6. Sonraki bölgeler için notlar (bu işin parçası değil)

- **Bölge 8 — değişen harita:** rüzgâr kumu ve kayaları savurur; "Çalıştır" kodu **üç farklı haritada** sırayla dener, görev üçünde de başarılınca biter; başarısız olan harita gösterilir ("2. haritada kayaya çarptın"). Bölge 7'den bağlantı: kumdaki çizimi rüzgâr siler. `return` ile birlikte: soru soran ve cevap veren rutinler. Tasarımı ayrı yapılır.
- **İleride — iki robotu birlikte kodlamak** (Serçe uçar, kayaların üstünden geçer; Kıvılcım yürür). Bölgesi sonra seçilir.
- **İleride — yıldızlar:** her bölümde isteğe bağlı meydan okuma (çalıştı ⭐, kısa yazdın ⭐⭐, en az adım ⭐⭐⭐; örn. "yalnızca 5 satırla"). Enes'in fikri; mekanik değil, bütün bölümlere eklenebilecek bir katman.

## 7. Yapacak kişiye bırakılanlar

Harita harfi ve bölüm dosyası alan adları; bölüm adları ve haritalar; mesajların son metni; sözlük sayfasının biçimi; "Kayıtlar" sayfasının yeri; Serçe anının tam süresi; 61-70 kayıt damgalarındaki sol sayıları (`hikaye-kitabi.md` zaman çizelgesine uygun).
