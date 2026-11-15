# Bölge 10 Bulmacaları + Hikâye damgaları (Bölüm 91-100) — yapılanların kaydı

> **2026-11-15, Claude (Opus 5.5) — Ragıp "durmadan çalış, kararları sen ver, sonra söyle" dedi.** Plan ayrı yazılmadı; iş aynı oturumda yapıldı (Bölge 9'dan hemen sonra). Bu belge kararların ve yapılanların kaydıdır.
> **Spec:** `docs/superpowers/specs/2026-11-09-bolge-8-10-ders-bulmaca-design.md` (Bölge 10). **Senaryo:** `docs/tasarim/senaryo-bolge-10.md`.

## Yapılanlar

1. **Göktaşı `G`** (`Collectible.MeteorStone`, toplanır), **gece ovası** (`Region.NightPlain`, 91-100).
2. **Krater `U`** (girilmez: "Önünde yeni bir krater var") ve **`meteor_ahead(yon)`** (91'de açılır; alanın dışı krater sayılmaz). Kraterler haritada sabit olabilir ya da **`meteorlar`** alanında her denemede başka yerde (2-3 düzen; düzen haritanın boyunda satırlar, `U` krater). Kod her düzende ayrı denenir (değişen yol/not ile aynı altyapı: `Level.VersionCount`, `CratersOf(version)`). Not ile birlikte yazılırsa sayıları aynı, not yolu kraterden geçmez.
3. **Kızgın göktaşı `X`** (üstünden geçilir, toplanmaz: "Taş hâlâ kızgın") ve **`smoke_here()`** (95'te açılır).
4. **Sahne (geçici, derlendi, Unity'de görülmedi):** `MeteorMarks` (krater: koyu halka + kor gibi parlayan çukur; kızgın taş: koyu taş + turuncu ışık); her "Çalıştır"da o denemenin kraterleri kurulur. Ekran bulmacasının adı bölgeye göre (`ScreenView.NameFor`: istasyonda "İSTASYON EKRANI", 99'da "ANTEN"). Göktaşı şimdilik buz görünüşünde, ova gündüz ışığında (3a).
5. **Bölüm 91-100**, **`sinav-95`**, **`sinav-100`** (Sezon 1 büyük sınavı, 9 soru), sözlükte **`and`**, **`or`**, **`meteor_ahead`**, **`smoke_here`**.
6. **Hikâye:** etiket "BÖLGE 10 · GECE OVASI", damgalar (sol 1.103-1.107), **Kayıt 6** (taslaklarda 5b; `yildiz.ses`, 93 başı), bölge geçişi ("Yeni bölge: Gece ovası / Hedef: volkanın eteği").
7. **Testler:** `GeceOvasiTests.cs`, Bölge 10 damga testi (bitiş sayıları bulmacayla aynı), Kayıt 6 testi; bölüm sırası testi `bolum-100` için düzeltildi (dosya adı sırası sayı sırası değil). **1827 test geçti.**

## Plan sırasında verilen kararlar (spec'e ek / düzeltme)

1. **Meteorlar her denemede başka yere düşer (`meteorlar`).** Spec "değişen yol 2 sürüm" yalnız 94 ve 96'da diyordu; sabit kraterle `meteor_ahead` ezber yolla atlanırdı. 91'den itibaren 2-3 düzen: komut gerçekten gerekli olur, ezber yol takılır.
2. **Merdiven bulmacası:** 91-94, 96, 100'de robot köşeden köşeye 10 adım; her adımda "o yön tehlikeli mi" diye bakar, öyleyse öbür yöne gider. Düzenler, kodun her adımda doğru kararla hedefe varacağı (ve alanın dışına çıkmayacağı) biçimde kuruldu; bulmacayı her denemede 10 turluk aynı döngü çözer.
3. **`smoke_here` = kızgın taş, `wait` değil.** Spec'te "dumanlı yeni krater"; toz bulutu (`wait`) Bölge 4'te vardı, tekrar olmasın diye duman taşın kendisinde: kızgın taş toplanmaz. Bu, 95-98'de `not smoke_here()` ile "topla ya da geç" kararını verir.
4. **95'te `while` + `and` yok:** yeni komut (`smoke_here`) açıldığı için bölüm sade (yol hazır). Spec'teki "while + and + fonksiyon" birleşimi 96-98'e dağıldı.
5. **96'da liste yok, 97'de `and/or` yok:** iki bölümde de merdiven/okuyan robot + duman birleşimi yeterince yeni; liste sayma (sabit sayı çıkacağı için ezberlenirdi) eklenmedi. Liste ve `and/or` 99 ve `sinav-100`'de birlikte kullanılıyor.
6. **98 sakin bölüm:** tek ipucu, rutin şartı (`satir_tara`), krater yok ("yağmur diner").
7. **99:** gürültü listesi başlangıç kodunda (`gurultu = [...]`), oyuncu `not in` ile kullanır; rakam için `harf in "0123456789"` (spec: `isdigit` zorunlu değil). Tam 8 karakter de geçerli (`>=`; tipik hata `>`).
8. **Kayıt numarası:** oyunda "KAYIT 6" (Kayıtlar sayfası sayı ister); belgelerde "taslakta 5b" notu. `hikaye-kitabi.md` tablosunda Bölüm 100 satırı "—" (kayıt değil).

## Kalanlar (sonraki parçalar)

- **3a görünüş:** gece (yıldızlar, meteor çizgileri; spec "ışık yıldızlardan, meteorlardan ve Kıvılcım'ın lambasından", gündüz gölgesi yok), krater ve kızgın taşın asıl hâli (duman rüzgârla aynı yöne eğilir), göktaşı modeli + sayaç simgesi, gökyüzünde mavi nokta, 97'de volkanın eteğinde kapının silueti, ova engeli.
- **3b sahneler:** 94 Kıvılcım mavi noktaya bakar; 99 antene kodların akışı; **100 final** (iki uyarı, sinyal göstergesi, bir kez yanan ışık); "Atla" düğmesi kuralı.
- Unity'de paket + görüntü denetimi (bu oturumda Unity yoktu).
