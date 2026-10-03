# Hikâye — "Sessiz Koloni"

> Taslak 3 — 2026-10-02 (Ragıp + Claude). Hedef (Ragıp): hikâye oyuncuyu en az The Walking Dead oyunu kadar etkilemeli.
> **Kararlar (Ragıp, 10-02):** oyuncu = Dünya'daki "Mars Kod Okulu" öğrencisi; ölüm yerine robotun paramparça olması (sabit diskinden yeniden kurulur, bedeli var); Taslak 3 onaylandı.
> **Kararlar (Ragıp, 10-01):** iskelet C; her 10 bölümde yeni bölge; oyun en az 200 bölüm, ileride 2000'e büyüyebilir. Hikâye yalnızca Ragıp'ın işi; ileride YouTube dizisi olacak.
> **Hikâye belgeleri (2026-10-02, hepsi `docs/tasarim/`):**
> - `hikaye.md` (bu belge): özet, 4 perde, 20 bölge tablosu, final.
> - `hikaye-kitabi.md`: başvuru belgesi: dünyanın kuralları, mantık kararları, zaman çizelgesi, karakter kartları, kayıtlar, motifler, çelişki listesi.
> - `senaryo-bolge-01.md`: Bölge 1 (Bölüm 1-10) bölüm bölüm ayrıntılı senaryo + oyuna yapım listesi.
> - `senaryo-perde-1.md` … `senaryo-perde-4.md`: Bölüm 11-200 ve final (her bölge: olay, izler, kayıtlar, kapanış; her bölüm bir satır). Perdelerin sonundaki "Hikâye kitabına işlenecekler" henüz kitaba aktarılmadı.
> Eski taslak (Taslak 2, robot merkezli): git geçmişinde (commit 78d555e). Bölge kuralları: `hikaye-ve-bolgeler.md`.

## Tek cümlede

Dünya'da bir genç, okulun Mars programında sıradan bir kod ödevi açtığını sanır; aslında insanları yeraltında uyuyan bir koloninin tek uyanık robotuna öğretmektedir. Güneş iki gezegenin arasına girip bağlantıyı kesmeden önce robota tek başına ayakta durmayı öğretmesi gerekir.

## Ana fikir (filmin "neden"i)

**Öğrenmek, bir başkasına güvenmektir.** Bir zincir: Defne, hiç görmediği bir öğretmene güvendi. Oyuncu Kıvılcım'a öğretti. Kıvılcım da belleğini yitiren Kor'a öğretir. Son sahnede oyuncu, baştan beri bu zincirin ilk halkası olduğunu fark eder.

## Neden etkiler (TWD'den alınanlar, oyunumuza uygun)

- **Bağ:** Oyuncu ile Kıvılcım arasında (Lee ile Clementine gibi): sen öğretirsin, o büyür.
- **Bedel:** İnsanlar ölmez ama kayıp gerçektir: Kor paramparça olur, geri döndüğünde hiçbir şey hatırlamaz; sonunda bağlantı kopar (ayrılık).
- **Oyuncunun duygusu = karakterin duygusu:** Oyuncu bir eğitim oyunu oynadığını sanır, oyundaki öğrenci de bir ödev yaptığını sanır. Bölüm 50'de ikisi aynı anda sarsılır.

## Uzun oyun yapısı: sezonlar

- **Sezon 1 — Sessiz Koloni: 200 bölüm = 20 bölge × 10 bölüm.** Kendi içinde başlar ve biter (temel Python'un tamamı).
- Final bir sonraki sezonun kapısını açar. Her sezon ~200 bölüm → 10 sezon = 2000 bölüm.
- Sezon 1 dört perde (her perde 5 bölge = 50 bölüm); her perde bir büyük soruyla başlar, büyük bir cevap ya da dönüşle biter.

## Ton kuralları

- **Sıcak, sade, çocuksu değil (13+).** Gizem var, korku yok.
- **İnsanlar ölmez.** Robotlar paramparça olabilir; sabit diskinden yeniden kurulabilir ama geri dönüşün **bir bedeli** olur (yoksa kayıp ucuzlar).
- **Az söz, çok iz.** Hikâyeyi haritadaki izler anlatır: terk edilmiş araç, yarım sera, kumda ayak izleri.
- **Robot konuşmaz.** Işığı, sesi ve hareketiyle anlaşır. Görünüşü bize özgü (R2-D2/EVE'ye benzemez).
- **Hikâye oyunu hiç bekletmez.** Bölüm arası en fazla 2-3 satır, her sahne atlanabilir; uzun sahne yalnızca bölge ve perde geçişinde.

## Filmlerden alınan teknikler (taklit değil, yöntem)

| Teknik | Bizde nasıl |
|---|---|
| Sözsüz açılış | Bölüm 1: "Mars Kod Okulu — Ödev 1". Karanlık koloni, uyanan robot, ilk `move`. Bir ödevden farkı yok. |
| Gerçeğin geç ortaya çıkması | Bölüm 50: "Bu bir ödev değil." |
| Önce yerleştir, sonra karşılığını ver | Robotun göğsündeki çocuk çizimi; finalde Ece ona bir yıldız ekler. |
| Zamanın ötesinden mesaj | Defne'nin kayıtları: robota değil, onu öğretecek kişiye, yani oyuncuya bırakılmış. |
| Saat işliyor | Bölüm 100: kapsüllerin enerjisi azalıyor + güneş kavuşumu yaklaşıyor (başlıkta zayıflayan sinyal). |
| Yol arkadaşı ve fedakârlık | Kor (Bölüm 121-130 katılır, 150'de Kıvılcım'ı kurtarırken paramparça olur). |
| Rollerin yer değiştirmesi | Başta belleksiz olan Kıvılcım'dı, hatırlayan Kor; sonunda Kor unutmuştur, Kıvılcım ona öğretir. |
| Ayrılık ve kavuşma | Bölüm 200'de bağlantı kopar; "iki hafta sonra" ilk görüntü. |
| Tekrarlayan motif | Seradaki saksı bitkisi: Bölüm 1'de kuru, su geldikçe yeşerir, finalde çiçek açmış. |

## Karakterler (adlar taslak)

- **Öğrenci (oyuncu):** Dünya'da, Mars Kod Okulu programına katılan bir genç. Oyunda adı ve yüzü yok; ekrandaki kod onun sesidir. **Dizide** adı ve yüzü olur: **Yıldız** (karar: Ragıp, 10-02). Sinyali ciddiye alan, kimsenin inanmadığı, gece gizlice bağlanan genç (dizinin konuşan, insan tarafı). Koloni onu yalnızca programdaki **kullanıcı adıyla** tanır (uzaylı tarzı bir ad; Mars'tan bakınca "uzaylı" olan odur). Gerçek adı Mars'ta kimse bilmez: Ece finalde çizime adını bilmeden bir yıldız ekler; izleyici bağı sonradan fark eder.
- **Kıvılcım (robot):** Küçük bakım robotu, belleği fırtınada silinmiş. Oyuncunun öğrettiği her komut onun bir becerisi olur. Göğsünde Ece'nin çizimi (kız, robot, çiçek). Yarı insansı, süzülür, konuşmaz.
- **Komutan Defne Aras:** Koloni komutanı. Perde 1-3'te yalnızca kayıtlarıyla vardır: sakin, esprili, kararlı. Bölüm 151'de kapsülden çıkar ve öğrenciyle ilk kez canlı (dakikalarca gecikmeyle) konuşur.
- **Ece:** Koloninin tek çocuğu. Sözü yok, izleri var: küçük ayak izleri, kapıdaki adı, duvarlardaki çizimler, robotun üstündeki çizim. Son uyanan odur.
- **Kor:** Fırtınadan önce hizmet dışı bırakılmış, eski, iri bir maden robotu. Huysuz ama güçlü; her şeyi hatırlar, insanların gittiği gecenin tanığıdır. Bölüm 150'de paramparça olur; Bölge 19'da yeniden kurulur ama hiçbir şey hatırlamaz.

## Gizem (arka plandaki gerçek)

Dev bir **güneş fırtınası** yaklaşıyordu; yüzey haftalarca güvenli olmayacaktı. Koloni volkanın altındaki **lav tüneli sığınağına** inip uyku kapsüllerine girdi. Defne fırtınanın robotun belleğini sileceğini biliyordu. Onu yeniden ancak **öğrenen biri** kodlayabilirdi. Bu yüzden Dünya'daki okulların katıldığı **Mars Kod Okulu** programına bir istek bıraktı ve yol boyunca robotu öğretecek kişi için kayıtlar ve işaretler hazırladı. Fırtına koloninin enerjisini ve robotun belleğini sildi. Fırtına çoktan geçti, ama sığınağın kapısını açacak enerji yok; kapsüllerin enerjisi de azalıyor.

**Mars Kod Okulu:** Dünya'daki öğrenciler eğitim için koloninin bakım robotlarına küçük kodlar yazar (gerçek dünyadaki örneği: Avrupa Uzay Ajansı'nın Astro Pi programı; öğrencilerin kodu Uzay İstasyonu'nda çalışır). Öğrenci ilk başta bunun da sıradan bir ödev olduğunu sanar; ciddiye alması için ikna edilmesi gerekmez, gerçeği öğrendiğinde zaten Kıvılcım'a bağlanmıştır.

**Güneş kavuşumu (gerçek bilim):** Mars ile Dünya arasında mesajlar dakikalarca gecikir; her iki yılda bir Güneş iki gezegenin arasına girer ve haberleşme yaklaşık iki hafta kesilir.

## Sezon 1: 20 bölge

Python konu sırası kolaydan zora genel bir taslaktır; motorun desteklediklerine göre netleşir. Toplanan şey ve araç bölgeyle birlikte değişir.

### Perde 1 — Ödev (Bölüm 1-50) · Soru: "Bu sadece bir ödev mi?"

| # | Bölümler | Bölge | Ne olur | Toplanan | Yeni Python |
|---|---|---|---|---|---|
| 1 | 1-10 | İniş ovası | "Ödev 1": robot uyanır; enerji hücreleri toplandıkça koloninin ışıkları tek tek yanar. Koloni boş. Bölüm 10: telsiz çalışır, kutuptan zayıf bir sinyal. | Enerji hücresi | `move`, `collect`, `for` |
| 2 | 11-20 | Kutup buzulu | Sinyal, terk edilmiş bir aracın yardım çağrısı. Sera için su toplanır; bitki ilk kez yeşerir. | Buz (temiz / kirli = kırmızı kristal) | `if` (tarama sensörü) |
| 3 | 21-30 | Kraterli düzlük (güneş paneli tarlası) | Kırık paneller onarılır, koloni gündüz enerjisine kavuşur. Bir kapıda çocuk eliyle "Ece". | Panel parçası | karşılaştırma, `elif`, döngü sayacı (`else` Bölge 2'de) |
| 4 | 31-40 | Kum tepeleri | Toz bulutları gelip geçer; robot "fırtına dinene kadar" bekler, "yol bitene kadar" ilerler. Tuhaflıklar birikir. | Pusula parçaları | `while` |
| 5 | 41-50 | Anten tepesi | Büyük anten onarılır. **Perde sonu (Bölüm 50):** ödevde olmaması gereken kayıt: "Bu bir ödev değil. Adım Defne Aras. Bunu duyan sensin, onu yeniden öğretecek kişi. Fırtına geliyor; biz yeraltına iniyoruz. Robotun belleği silinecek. Lütfen... ona öğret." | Kablo makarası | Değişkenler (sayaç, toplam) |

### Perde 2 — İz (Bölüm 51-100) · Soru: "Nereye gittiler?"

| # | Bölümler | Bölge | Ne olur | Toplanan | Yeni Python |
|---|---|---|---|---|---|
| 6 | 51-60 | Kanyon girişi | Geçiş: roketle kanyona. Depoda bir drone bulunup onarılır; kanyon duvarlarında boya oklar. Defne'nin kayıtları artık açıkça öğrenciye konuşur. | Drone parçası | Fonksiyonlar (Defne'nin "rutinleri": ezber değil beceri) |
| 7 | 61-70 | Kanyon dibi (eski nehir yatağı) | Drone ile yukarıdan, robotla aşağıdan iş birliği. Kumda küçük ayak izleri: Ece. | Maden | Parametreli fonksiyonlar |
| 8 | 71-80 | Terk edilmiş araştırma istasyonu | Ece'nin çizimleri duvarlarda; kayıtlar kişiselleşir: "Ece korkuyor ama belli etmiyor." | Veri kartı | `return` |
| 9 | 81-90 | Tuz gölü (kurumuş göl) | Geniş, düz, beyaz alan; oklar volkanı gösterir. | Tuz kristali | Listeler |
| 10 | 91-100 | Gece ovası (meteor yağmuru) | **Orta nokta (Bölüm 100):** iki saat birden: sığınaktan uyarı (kapsüllerin enerjisi azalıyor) + **güneş kavuşumu yaklaşıyor**; bağlantı, kapsüllerin dayanacağı süreden önce kopacak. Kıvılcım tek başına iş yapmayı öğrenmeli. | Meteor taşı | Listede dolaşma (`for x in liste`) |

### Perde 3 — Yol Arkadaşı (Bölüm 101-150) · Soru: "Kıvılcım bensiz yapabilir mi?"

| # | Bölümler | Bölge | Ne olur | Toplanan | Yeni Python |
|---|---|---|---|---|---|
| 11 | 101-110 | Volkan eteği | Geçiş: roketle volkana. Defne'nin şifreli notları çözülür. | Isı kristali | Metinler (string) |
| 12 | 111-120 | Lav ovası | Soğumuş lav ızgara gibi çatlamış; alan satır satır, sütun sütun taranır. | Obsidyen | İç içe döngüler (2 boyutlu alan) |
| 13 | 121-130 | Eski hangar | **Kor bulunur.** Huysuz yaşlı robot insanların gittiği geceyi anlatır; Kıvılcım bunları belleğine kaydeder (sonra önemli). Parça envanteri tutulur. | Yedek parça | Sözlükler (dict) |
| 14 | 131-140 | Jeotermal santral | Santral çalıştırılır; Kıvılcım öğrencinin yazdığı rutinleri kendi başına sırayla kullanmaya başlar (her yeri tek tek söylemek gerekmez). | Vana | `and`, `or`, `not` |
| 15 | 141-150 | Sığınak kapısı | Kapının kodu aranır. **Perde sonu (Bölüm 150, en karanlık an):** kapı açılırken tünel çöker; Kor, Kıvılcım'ı iterek kurtarır ve kayaların altında paramparça olur. Kapı açık, Kor yok. | Kapı anahtarı | Arama (bir listede bulmak) |

### Perde 4 — Dönüş (Bölüm 151-200) · Soru: "Herkesi kurtarabilecek miyiz?"

| # | Bölümler | Bölge | Ne olur | Toplanan | Yeni Python |
|---|---|---|---|---|---|
| 16 | 151-160 | Lav tüneli girişi | **Bölüm 151:** ilk kapsül açılır, Komutan Defne dışarı çıkar; öğrenciyle ilk kez canlı konuşur: "Demek sendin." Kapsüller enerjisi en azdan başlanarak sıralanır. | Enerji hücresi (motif dönüşü) | Sıralama |
| 17 | 161-170 | Derin tüneller (labirent) | Fener ışığında, karanlık dallanan tüneller. Sinyal belirgin biçimde zayıflar. | Fener pili | Yol bulma |
| 18 | 171-180 | Buz mağarası (yeraltı gölü) | Tünel içinde tünel; aynı işi her dalda tekrar. | Saf buz | Kendini çağıran fonksiyon (özyineleme) |
| 19 | 181-190 | Sığınak çekirdeği | **Kor'un sabit diski bulunur; oyuncu Kor'u kendi koduyla yeniden kurar.** Kor kalkar ama hiçbir şey hatırlamaz. Bu kez öğreten Kıvılcım: Kor'un anlattıklarını ona geri anlatır. | Kapsül anahtarı | Sınıflar ve nesneler (robotu "sınıf" olarak kurmak) |
| 20 | 191-200 | Son iletim | Bağlantı her bölümde biraz daha zayıflar. Bölüm 200: öğrenci son programını gönderir, **bağlantı kopar**, ekran kararır. | — (hepsi) | Hepsi birlikte (büyük final görevleri) |

## Final (Bölüm 200'den sonra)

"İki hafta sonra." Sinyal geri gelir. İlk görüntü: koloninin bütün ışıkları yanıyor, insanlar dışarıda, seradaki bitki çiçek açmış. Kıvılcım, Ece'nin son kapsülünü öğrenci yokken, onun öğrettikleriyle açmıştır.

Ece robotun göğsündeki çizime dokunur (kız, robot, çiçek) ve ona yeni bir şey ekler: **gökyüzünde küçük bir yıldız**, yanında "öğretmen". Oyunun başından beri kilitli duran son kayıt açılır:

> "Bunu duyuyorsan, ona öğretmeyi başardın. Biz seni hiç görmedik, ama hepimiz sana borçluyuz. Teşekkür ederim."

Kıvılcım gökyüzüne, Dünya'ya doğru, ışığını iki kez yakıp söndürür.

## Sezon 2'ye kapı

Final yazılarından sonra kısa bir sahne: koloni uyurken telsiz kendiliğinden açılır. Sinyal Dünya'dan değil, **gökyüzünden**: Phobos'tan, tekrar eden bir desen. Kıvılcım belleğindeki eski bir kaydı açar: Kor'un fırtınadan önce anlattığı bir anı; Kor bu deseni daha önce görmüştü. Yeni Kor ise hiçbir şey hatırlamadan bakar.

Sezon 2'nin konusu açık bırakılır (Phobos, ikinci koloni, buzun altındaki şey…). Her sezon yeni bir ileri Python alanı açabilir (veri işleme, algoritmalar, çoklu robot, oyun yapay zekası…).

## Oyunda nasıl anlatılır (telefon için)

- **Okul çerçevesi:** açılış ekranı ve bölüm adları Perde 1 boyunca bir okul programı gibi görünür ("Mars Kod Okulu — Ödev 1"). Bölüm 50'deki sarsıntı buna bağlı; ucuz ve çok etkili. Bölüm 50'den sonra çerçeve değişir (ödev numarası yerine bölge/görev).
- **Kayıt = kısa yazı + simge.** Bulununca başlığın altında 1-2 satır; dokunulmazsa kendiliğinden kapanır. Hepsi "Kayıtlar" sayfasında saklanır. Son kayıt ilk günden kilitli görünür (merak).
- **Sinyal göstergesi (Bölüm 100'den sonra):** başlıkta küçük bir sinyal simgesi, her bölgede biraz zayıflar; mesaj gecikmesi ("12 dk") yanında yazabilir.
- **İzler haritada:** hikâye nesneleri (terk edilmiş araç, ok, ayak izi) engel ya da süs olarak bölüm haritasında durur.
- **Koloni büyümesi = hikâye ilerlemesi:** arka plandaki koloni bölümler bitince değişir (şimdiden: güç lambaları).
- **Bölge geçişi:** 10-20 sn, atlanabilir, sözsüz. **Perde geçişi** (50, 100, 150, 200): biraz daha uzun, hikâyenin büyük anları.
- **Yeni komut hikâyeyle gelir** ("Tarama sensörü takıldı: artık `if` kullanabilirsin"); sözlük sayfası aynı anda açılır.
- **Bölge içinde çeşitlilik:** saat ve hava (gün batımı, gece, toz) 3-4 bölümde bir değişir.

## YouTube dizisi (Play Store'dan sonra)

- Dizi iki yerde akar: **sessiz Mars** (Kıvılcım, izler, kayıtlar) ve **konuşan Dünya** (öğrenci: ödevin gerçek olduğunu anlar, kimse inanmaz, öğretmeni "program hatası" der, gece gizlice bağlanır).
- Oyunla aynı hikâye; dizi öğrenciye ad ve yüz verir. Oyunda öğrenci adsız kalır (oyuncu kendini koyabilsin).

## Emek gerçekçiliği

- Bölgeler kodla çizilir (boyut küçük kalır) ve **ortak parçalardan** kurulur: aynı zemin sistemi, farklı renk/ışık/biçim + bölgeye özgü 3-5 nesne.
- Bölümler metin dosyası olduğu için hızlı yazılır; asıl zaman görsel ve test.
- İlk sürüm (Play Store) Perde 1'in tamamı (50 bölüm; Bölüm 50'deki dönüş dahil) ya da ilk 2-3 bölge ile çıkabilir; kalan bölgeler güncellemelerle gelir.

## Bugünkü oyuna uygulananlar

Bölge 1 = Bölüm 1-10 (enerji hücresi, `for`), Bölge 2 = Bölüm 11-20 (buz, `if`):
1. ✅ (Ragıp, 10-01) Eski Bölüm 6-10 → Bölüm 11-15 (kutup buzulu).
2. ✅ (Ragıp, 10-01) Bölüm 1-5'te toplanan şey enerji hücresi.
3. ✅ (Ragıp, 10-01) Yeni Bölüm 6-10 (`for` derinleşir); Bölüm 10 = telsiz/sinyal anı (hikâye metni hikâye sistemi gelince).
4. Bölüm 16-20 (`if` derinleşir; Enes'in iş listesinde, hikâyesiz).
5. ✅ (Ragıp, 10-01) Enerji hücresi + kolonide yanan güç lambaları; kutup buzulu bölgesi.
6. ✅ (Ragıp, 10-01) Kıvılcım ve göğsündeki çizim.
7. Yeni (Taslak 3): okul çerçevesi (açılış ekranı "Mars Kod Okulu — Ödev N"), kayıt sistemi, Bölüm 1'in sözsüz açılışı.

## Açık sorular

- Adlar: Kıvılcım, Defne Aras, Ece, Kor (İngilizce sürümde de okunabilir olmalı). Öğrenci: Yıldız ✅; programdaki kullanıcı adı? (örn. "Uzaylı42"). İngilizce sürümde "Yıldız"ın anlamı kaybolur ("Star"/"Stella" karşılığı düşünülmeli).
- Oyunun adı "MarsKod" okul çerçevesine uyuyor; "Mars Kod Okulu" adı oyunun içinde mi kalsın, oyunun adı mı olsun?
- Python konu sırası (Bölge 3-20) motorun desteklediklerine ve Enes'in müfredatına göre netleşmeli.
- İlk Play Store sürümü kaç bölümle çıkar?
- Hikâye metinlerinin "Eğlenceli / Sade" anlatım seçeneğiyle ilişkisi.
