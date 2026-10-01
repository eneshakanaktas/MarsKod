# Hikâye — "Sessiz Koloni"

> Taslak 2 — 2026-10-01. Seçim: iskelet **C** (Ragıp). Yazan: Claude, Ragıp'ın isteğiyle ("dünyanın en iyi film yönetmenleri havasında").
> **Kararlar (Ragıp, 10-01):** her 10 bölümde yeni bölge; oyun en az 200 bölüm, ileride 2000'e büyüyebilir; Bölüm 6-10'un kutup buzuluna taşınması önerisi kabul.
> Durum: taslak — Enes ile okunacak; adlar ve bölge sırası değişebilir. Bölge kuralları: `hikaye-ve-bolgeler.md`.

## Tek cümlede

Koloninin küçük bakım robotu bir sabah uyanır: ışıklar sönük, kapılar açık, kimse yok. Ona ulaşan tek şey senin yazdığın kod. Robot, Mars'ı bölge bölge geçip koloniyi yeniden ayağa kaldırırken insanların izini sürer ve sonunda onlara neden kendisinin gerektiğini anlar.

## Ana fikir (filmin "neden"i)

**Öğrenmek, bir başkasına güvenmektir.** Oyuncu robota kod öğretirken kendisi de öğreniyor. Koloninin insanları ise robotun bir gün öğreneceğine güvenip yolu ona hazırlamış. Son sahnede oyuncu, baştan beri bir zincirin halkası olduğunu fark eder.

## Uzun oyun yapısı: sezonlar

Tek hikâyeyi 2000 bölüme yaymak sulandırır. Diziler gibi **sezonlar**:

- **Sezon 1 — Sessiz Koloni: 200 bölüm = 20 bölge × 10 bölüm.** Kendi içinde başlar ve biter (temel Python'un tamamı).
- Final, bir sonraki sezonun kapısını açar (aşağıda "Sezon 2'ye kapı"). Her sezon ~200 bölüm → 10 sezon = 2000 bölüm.
- Oyuncu her sezonda bir hikâyeyi bitirmenin tadını alır; yeni sezon "geri dönme" sebebidir.

Sezon 1, filmler gibi **dört perde** (her perde 5 bölge = 50 bölüm). Her perde bir büyük soruyla başlar, büyük bir cevap ya da dönüşle biter; böylece 200 bölüm boyunca merak hiç düşmez.

## Ton kuralları

- **Sıcak, sade, çocuksu değil (13+).** Gizem var, korku yok. **Kimse ölmez.**
- **Az söz, çok iz.** Hikâyeyi haritadaki izler anlatır: terk edilmiş araç, yarım sera, kumda ayak izleri.
- **Robot konuşmaz.** Işığı, sesi ve hareketiyle anlaşır (ruhu R2-D2 gibi sadık ve sevimli). **Görünüşü bize özgü olmalı:** R2-D2 Disney/Lucasfilm'in tescilli karakteri; siluet benzememeli.
- **Hikâye oyunu hiç bekletmez.** Bölüm arası en fazla 2-3 satır, her sahne atlanabilir; uzun sahne yalnızca bölge ve perde geçişinde.

## Filmlerden alınan teknikler (taklit değil, yöntem)

| Teknik | Bizde nasıl |
|---|---|
| Sözsüz açılış | Bölüm 1'de hiç hikâye metni yok. Karanlık koloni, uyanan robot, ilk `move`. |
| Önce yerleştir, sonra karşılığını ver | Robotun gövdesinde silik bir çocuk çizimi. Anlamı Bölüm 200'de çıkar. |
| Zamanın ötesinden mesaj | Komutan Defne'nin ses kayıtları; oyuncu "geçmişten yardım" sanar, aslında robot için bırakılmıştır. |
| Saat işliyor (gerilim) | Bölüm 100'de kapsüllerin enerjisinin azaldığı anlaşılır; acele etme sebebi doğar. |
| Yol arkadaşı | Bölüm 121-130'da yaşlı, huysuz bir ikinci robot (Kor) katılır; sonra ikisi birlikte kodlanır. |
| Her şeyi yeniden anlamlandıran son | Son kayıt: "Bunu duyuyorsan, biri sana öğretmiş demektir." |
| Tekrarlayan motif | Seradaki saksı bitkisi: Bölüm 1'de kuru, su geldikçe yeşerir, Bölüm 200'de çiçek açar. |

## Karakterler (adlar taslak)

- **Kıvılcım (robot):** Küçük bakım robotu, belleği silinmiş. Oyuncunun öğrettiği her komut onun bir becerisi olur. Gövdesinde bir çocuk çizimi.
- **Komutan Defne Aras:** Koloni komutanı. 150. bölüme kadar yalnızca ses kayıtlarıyla vardır: sakin, esprili, kararlı. Bölüm 150'de kapsülden çıkar, oyuncu onu da kodlar.
- **Ece:** Koloninin tek çocuğu. Sözü yok, izleri var: küçük ayak izleri, saksının üstündeki adı, robotun üstündeki çizim.
- **Kor:** Fırtınadan önce hizmet dışı bırakılmış eski, iri bir maden robotu. Huysuz ama güçlü. Kıvılcım'ın aksine her şeyi hatırlıyor; insanların gittiği geceye dair ilk görgü tanığı.
- **Oyuncu:** Kim olduğu hiç söylenmez. Ekrandaki kod onun sesidir.

## Gizem

Dev bir **güneş fırtınası** yaklaşıyordu; yüzey haftalarca güvenli olmayacaktı. Koloni volkanın altındaki **lav tüneli sığınağına** inip uyku kapsüllerine girdi. Fırtına koloninin enerjisini ve robotun belleğini sildi. Fırtına çoktan geçti, ama sığınağın kapısını açacak enerji yok; kapsüllerin enerjisi de azalıyor. Defne gitmeden önce yol boyunca robot için kayıtlar ve işaretler bıraktı.

## Sezon 1: 20 bölge

Python konusu sırası, kolaydan zora genel bir müfredat taslağıdır; motorun desteklediği konulara göre Enes ile netleşir. Toplanan şey ve araç bölgeyle birlikte değişir.

### Perde 1 — Uyanış (Bölüm 1-50) · Soru: "Herkes nerede?"

| # | Bölümler | Bölge | Ne olur | Toplanan | Yeni Python |
|---|---|---|---|---|---|
| 1 | 1-10 | İniş ovası | Robot uyanır; fırtınada saçılan enerji hücreleri toplandıkça koloninin ışıkları tek tek yanar. Bölüm 10: telsiz çalışır, kutuptan zayıf bir sinyal. | Enerji hücresi | `move`, `collect`, `for` |
| 2 | 11-20 | Kutup buzulu | Sinyal, terk edilmiş bir aracın yardım çağrısı. Sera için su toplanır; bitki ilk kez yeşerir. | Buz (temiz / kirli = kırmızı kristal) | `if` (tarama sensörü) |
| 3 | 21-30 | Kraterli düzlük (güneş paneli tarlası) | Kırık paneller onarılır, koloni gündüz enerjisine kavuşur. | Panel parçası | `else`, `elif`, karşılaştırma |
| 4 | 31-40 | Kum tepeleri | Toz bulutları gelip geçer; robot "fırtına dinene kadar" bekler, "yol bitene kadar" ilerler. | Pusula parçaları | `while` |
| 5 | 41-50 | Anten tepesi | Büyük anten onarılır. **Perde sonu:** Defne'nin ilk tam kaydı açılır: "Güneş fırtınası geliyor. Herkes sığınağa." Cevap: gitmediler, **saklandılar**. | Kablo makarası | Değişkenler (sayaç, toplam) |

### Perde 2 — İz (Bölüm 51-100) · Soru: "Nereye gittiler?"

| # | Bölümler | Bölge | Ne olur | Toplanan | Yeni Python |
|---|---|---|---|---|---|
| 6 | 51-60 | Kanyon girişi | Geçiş: roketle kanyona. Depoda bir drone bulunup onarılır; kanyon duvarlarında boya oklar. | Drone parçası | Fonksiyonlar (Defne'nin "rutinleri") |
| 7 | 61-70 | Kanyon dibi (eski nehir yatağı) | Drone ile yukarıdan, robotla aşağıdan iş birliği. Kumda küçük ayak izleri: Ece. | Maden | Parametreli fonksiyonlar |
| 8 | 71-80 | Terk edilmiş araştırma istasyonu | Ece'nin çizimleri duvarlarda; Defne'nin kayıtları kişiselleşir: "Ece korkuyor ama belli etmiyor." | Veri kartı | `return` (fonksiyonun cevap vermesi) |
| 9 | 81-90 | Tuz gölü (kurumuş göl) | Geniş, düz, aynasız beyaz alan; oklar volkanı gösterir. | Tuz kristali | Listeler |
| 10 | 91-100 | Gece ovası (meteor yağmuru) | **Orta nokta dönüşü (Bölüm 100):** sığınaktan gelen otomatik uyarı: kapsüllerin enerjisi azalıyor. Artık acele var. | Meteor taşı | Listede dolaşma (`for x in liste`) |

### Perde 3 — Yolculuk (Bölüm 101-150) · Soru: "Neden geri dönmediler?"

| # | Bölümler | Bölge | Ne olur | Toplanan | Yeni Python |
|---|---|---|---|---|---|
| 11 | 101-110 | Volkan eteği | Geçiş: roketle volkana. Defne'nin şifreli notları çözülür. | Isı kristali | Metinler (string) |
| 12 | 111-120 | Lav ovası | Soğumuş lav ızgara gibi çatlamış; alan satır satır, sütun sütun taranır. | Obsidyen | İç içe döngüler (2 boyutlu alan) |
| 13 | 121-130 | Eski hangar | **Kor bulunur.** Huysuz yaşlı robot, insanların gittiği geceyi hatırlıyor. Parça envanteri tutulur. | Yedek parça | Sözlükler (dict) |
| 14 | 131-140 | Jeotermal santral | Santral çalıştırılır; birden çok koşul aynı anda tutmalı. | Vana | `and`, `or`, `not` |
| 15 | 141-150 | Sığınak kapısı | Kapının kodu aranır. **Perde sonu (Bölüm 150):** kapı açılır, ilk kapsül açılır, **Komutan Defne dışarı çıkar.** | Kapı anahtarı | Arama (bir listede bulmak) |

### Perde 4 — Dönüş (Bölüm 151-200) · Soru: "Herkesi kurtarabilecek miyiz?"

| # | Bölümler | Bölge | Ne olur | Toplanan | Yeni Python |
|---|---|---|---|---|---|
| 16 | 151-160 | Lav tüneli girişi | Defne kapsülleri enerjisi en az olandan başlayarak sıralar. | Enerji hücresi (motif dönüşü) | Sıralama |
| 17 | 161-170 | Derin tüneller (labirent) | Fener ışığında, karanlık dallanan tüneller. | Fener pili | Yol bulma |
| 18 | 171-180 | Buz mağarası (yeraltı gölü) | Tünel içinde tünel; aynı işi her dalda tekrar. | Saf buz | Kendini çağıran fonksiyon (özyineleme) |
| 19 | 181-190 | Sığınak çekirdeği | Kıvılcım, Kor ve Defne birlikte çalışır; her biri kendi becerileriyle. | Kapsül anahtarı | Sınıflar ve nesneler |
| 20 | 191-200 | Yüzeye dönüş | Herkes uyandırılır, koloni yeniden yaşar. Final. | — (hepsi) | Hepsi birlikte (büyük final görevleri) |

## Final (Bölüm 200)

Son kapsül Ece'nin. Ece uyanır, robotu görür, gövdesindeki kendi çizimine dokunur. Seradaki bitki çiçek açar; koloninin bütün ışıkları yanar. Oyunun başından beri kilitli duran son kayıt açılır:

> "Bunu duyuyorsan, biri sana öğretmiş demektir. Ona teşekkür et. Biz seni bekledik; o seni buraya getirdi."

Robot kameraya, yani oyuncuya döner, ışığını iki kez yakıp söndürür.

## Sezon 2'ye kapı

Final yazılarından sonra kısa bir sahne: koloni uyurken telsiz kendiliğinden açılır. Sinyal Dünya'dan değil, **gökyüzünden**: Phobos'tan, tekrar eden bir desen. Kor uzun uzun bakar ve ışığını bir kez yakar: o deseni tanıyor.

Sezon 2'nin konusu açık bırakılır (Phobos, ikinci koloni, buzun altındaki şey…). Her sezon yeni bir ileri Python alanı açabilir (veri işleme, algoritmalar, çoklu robot, oyun yapay zekası…).

## Oyunda nasıl anlatılır (telefon için)

- **Ses kaydı = kısa yazı + simge.** Bulununca başlığın altında 1-2 satır; dokunulmazsa kendiliğinden kapanır. Hepsi "Kayıtlar" sayfasında saklanır. Son kayıt ilk günden kilitli görünür (merak).
- **İzler haritada:** hikâye nesneleri (terk edilmiş araç, ok, ayak izi) engel ya da süs olarak bölüm haritasında durur.
- **Koloni büyümesi = hikâye ilerlemesi:** arka plandaki koloni bölümler bitince değişir.
- **Bölge geçişi:** 10-20 sn, atlanabilir, sözsüz. **Perde geçişi** (50, 100, 150, 200): biraz daha uzun, hikâyenin büyük anları.
- **Yeni komut hikâyeyle gelir** ("Tarama sensörü takıldı: artık `if` kullanabilirsin"); sözlük sayfası aynı anda açılır.
- **Bölge içinde çeşitlilik:** saat ve hava (gün batımı, gece, toz) 3-4 bölümde bir değişir.

## Emek gerçekçiliği

200 bölüm + 20 bölge büyük iş. Öneriler:
- Bölgeler kodla çizilir (boyut küçük kalır) ve **ortak parçalardan** kurulur: aynı zemin sistemi, farklı renk/ışık/biçim + bölgeye özgü 3-5 nesne.
- Bölümler metin dosyası olduğu için hızlı yazılır; asıl zaman görsel ve test.
- İlk sürüm (Play Store) Perde 1'in tamamı (50 bölüm) ya da ilk 2-3 bölge ile çıkabilir; kalan bölgeler güncellemelerle gelir (oyuncuyu geri çağırır).

## Bugünkü oyuna uygulanacaklar (karar: Ragıp, 10-01)

Bölge 1 = Bölüm 1-10 (enerji hücresi, `for`), Bölge 2 = Bölüm 11-20 (buz, `if`):
1. ✅ (Ragıp, 10-01) Bugünkü **Bölüm 6-10** (`if` + buz + kırmızı kristal + `ice_here()`) → **Bölüm 11-15** olur (kutup buzulu).
2. ✅ (Ragıp, 10-01; görünüş madde 5'te) Bölüm 1-5 içerik olarak kalır; toplanan şey **enerji hücresi** olur (görünüş + görev metinleri, örn. "2 enerji hücresi topla").
3. ✅ (Ragıp, 10-01: Atlamalı, Gidiş dönüş, Kayanın çevresi, Bozuk rutin, Telsiz; telsiz sahnesinin hikâye metni hikâye sistemi gelince) **Yeni Bölüm 6-10** yazılır: `for` derinleşir, enerji hücresi; Bölüm 10 = telsiz/sinyal anı.
4. Bölüm 16-20 ileride (`if` derinleşir).
5. Görsel: ✅ (Ragıp, 10-01: pil kapsülü + kolonide yanan güç lambaları) enerji hücresi nesnesi; ✅ (Ragıp, 10-01: mavi gün batımı, buz uçurumları + yardım sinyali, buz bloğu engeller) kutup buzulu bölgesi (ilk ikinci bölge).
6. ✅ (Ragıp, 10-01: Kıvılcım, yarı insansı süzülen bakım robotu, göğsünde çiçek + el ele kız ve robot çizimi) Robotun kendine özgü görünüşü (R2-D2'ye benzememeli) ve gövdedeki çizim.

## Açık sorular

- Adlar: Kıvılcım, Defne Aras, Ece, Kor? (İngilizce sürümde de okunabilir olmalı.)
- Python konu sırası (Bölge 3-20) motorun desteklediklerine ve Enes'in müfredatına göre netleşmeli.
- İlk Play Store sürümü kaç bölümle çıkar?
- Hikâye metinlerinin "Eğlenceli / Sade" anlatım seçeneğiyle ilişkisi.
