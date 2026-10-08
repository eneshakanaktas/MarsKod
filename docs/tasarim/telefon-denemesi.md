# Telefon denemesi: oyun açılmıyorsa ne yapılır

**Kimin için:** Hamza (test telefonu Oppo A74) ve telefonda deneme yapacak herkes.
**Neden:** 2026-10-03'te Oppo A74'te oyun siyah ekranda kaldı. Telefon ekran kartını (GPU) durduruyordu: bir çizim zamanında bitmedi.
Güçlü tahmin: gökyüzü/tepeler/koloni çizimi çok ağır. 2026-10-03'te bu çizim hafifletildi (aşağıda). Bu sayfa,
hafifletmenin yetip yetmediğini ve yetmezse **neyin** ağır geldiğini adım adım bulmak için.

## Ne değişti (2026-10-06, Enes) — ÖNCE BUNU OKU
- **Performans modu** geldi: telefonda oyun bununla açılır (Bölümler ekranında "Performans modu" anahtarı, açık).
  Ekran çözünürlüğü %60, kenar yumuşatma kapalı, gölge sert, zemin telefona özel hafif çizimle, gökyüzü resmi daha küçük.
  Yazılar ve düğmeler keskin kalır. Anahtarı kapatınca eski (tam ayrıntılı) görünüm gelir, oyun yeniden başlamadan.
- **Ölçüm:** üç parmakla açılan göstergede artık iki satır var: kare/sn ve **"işlemci X ms · ekran kartı Y ms"**.
  Oyun ayrıca 5 saniyede bir log'a `KARE:` satırı yazar (gösterge kapalıyken de).
- Bilgisayarda (Intel HD 4000, eski bir dizüstü ekran kartı) ölçüldü: tam görünümde ekran kartı 32 ms/kare,
  Performans modunda **8 ms** (4 kat hafif).

### Hamza: yapılacak (tek seferde)
1. Yeni paketi kur: `adb install -r paketler\marskod-oyun.apk`
2. Aşağıdaki adımları sırayla dene. Her adımda oyunu başlat, **1 dakika bekle**, sonra:
   `adb logcat -d -s Unity | findstr "GRAFIK KARE"` → çıkan **son 3 `KARE:` satırını** ve `GRAFIK:` satırını gönder.
   (Her adımdan önce eski log'u temizle: `adb logcat -c`)

| Adım | Ne dener | Seçenekler |
|---|---|---|
| 1 | Yeni hâli (Performans modu açık) | *(boş)* |
| 2 | Arayüzün (yazılar, düğmeler) payı | `-arayuz yok` |
| 3 | Alan çevresindeki kaya/eşyaların payı | `-cevre yok` |
| 4 | Gölgenin payı | `-golgesiz` |
| 5 | Tam görünümle karşılaştırma | `-performans kapali` |

Adım 1'de 30 kare/sn ve üstü çıkarsa 2-4'e gerek yok, yalnızca 5'i yap.

**Yeni bölgeler için ek ölçüm (Bölge 3-5 geldi, 2026-10-08):** Bölge 3 ve 4'ün kendi zemini/gökyüzü var; kare hızına etkisi hiç ölçülmedi.
Adım 1'in ayarıyla (Performans modu açık) şu bölümleri sırayla aç (`-bolum N` ya da bölüm listesinden), her birinde **30 sn bekle**
ve son 3 `KARE:` satırını gönder: **Bölüm 1** (karşılaştırma için), **21** (Bölge 3), **31** (Bölge 4), **41** (Bölge 5, şimdilik Bölge 4'ün görünüşü),
**51** (Bölge 6 kanyonu: yeni gökyüzü + zemin, roket) ve **53** (kanyon deposu: duvarlar, raflar, sandıklar; en çok nesne bu bölümde).
Ayrıca gözle bak: Bölüm 31'de toz bulutları ve turuncu hedef halkası seçiliyor mu; Bölüm 50 kapanışı (`-bolum 50`, kod çözülünce) takılmadan akıyor mu.

**Nasıl okunur:** 30 kare/sn için kare başına 33 ms'den az gerekir. `ekran karti` sayısı büyükse çizim ağır
(çözüm: daha da sadeleştirmek); `islemci` sayısı büyükse oyunun kodu ya da çizim komutlarının sayısı ağır
(çözüm başka). `—` yazıyorsa telefon o süreyi vermiyor demektir.

## Ne değişti (2026-10-03, Ragıp)
- Gökyüzü artık her karede baştan hesaplanmıyor: bir kez bir resme çiziliyor, oyun o resmi gösteriyor.
  Telefonda bu resim ekranın yarı çözünürlüğünde (piksel sayısı dörtte bir).
- Resim yalnızca gerektiğinde yenileniyor: bir şey değişince (klavye açılınca alan küçülür, bölge değişir, lamba yanar...)
  ve **arka plan animasyonları** açıksa saniyede 20 kez.
- Yeni ayar: **Bölümler ekranının en üstünde "Arka plan animasyonları"** aç/kapa. Kapalıyken yıldızlar, dronlar,
  yanıp sönen ışıklar durur; telefon daha az yorulur. Seçim telefonda kalır.

## Hazırlık (bir kez)
1. Telefonda kablosuz hata ayıklama açık olsun (Geliştirici seçenekleri → Kablosuz hata ayıklama).
2. adb, Unity'nin içinde: `<Editor>\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe`.
   Aşağıdaki komutlarda `adb` yazan yere bu yolu koy (ya da o klasörde komut penceresi aç).
3. Bağlan: `adb mdns services` adresi gösterir → `adb connect <adres>`.
4. Yeni paketi kur: `adb install -r paketler\marskod-oyun.apk`

## Deneme sırası
Her adımda oyunu başlat, **1-2 dakika bekle**, sonucu not et (açıldı mı, akıcı mı, kare hızı kaç).
Kare hızı göstergesi: oyun açılınca ekrana **üç parmakla dokun** (sol altta "kare/sn" çıkar; tekrar dokununca kapanır).
İlk açılan adımda dur, sonraki adımlara gerek yok. Sonucu bana (Claude'a) ya da Ragıp'a ilet.

| Adım | Ne dener | Seçenekler |
|---|---|---|
| 1 | Yeni hâli, hiçbir ayar yok | *(boş)* |
| 2 | Arka planı tamamen kapatır (düz renk). Açılırsa tahmin doğru: suçlu gökyüzü çizimi | `-arkaplan yok` |
| 3 | Üstüne zemini de sadeleştirir | `-arkaplan yok -zemin sade` |
| 4 | Ekranı daha düşük çözünürlükte çizer, kenar yumuşatma/HDR/gölge kapalı | `-arkaplan yok -zemin sade -olcek 0.5 -msaa 1 -hdr 0 -golgesiz` |

*(Aşağıdaki tablo 2026-10-03'ün; güncel sıra yukarıda.)*

Adım 1 açılırsa ayrıca şunları dene (akıcılığı karşılaştırmak için):
- `-animasyonsuz` (arka plan animasyonları kapalı başlar)
- `-arkaplan-olcek 0.25` (gökyüzü resmi daha da küçük)
- `-kare 30` (saniyede 30 kare; pil dostu)

### Seçenekleri telefona iki yoldan verebilirsin
**Yol A — başlatma komutu** (sanal telefonda denendi, çalışıyor):
```
adb shell "am start -S -n com.marskod.oyun/com.unity3d.player.UnityPlayerGameActivity -e unity '-arkaplan yok'"
```
Tırnaklara dikkat: dışta çift tırnak, seçeneklerin etrafında tek tırnak. Yoksa telefon `-arkaplan` ile `yok`u ayırır.
Tek tırnağın içine tablodaki seçenekleri yaz. Adım 1 için `-e unity '...'` kısmını hiç yazma.

**Yol B — dosya** (Yol A'da seçenekler oyuna ulaşmazsa; aşağıdaki "Seçenek ulaştı mı?" bölümüne bak):
```
echo -arkaplan yok > secenekler.txt
adb push secenekler.txt /sdcard/Android/data/com.marskod.oyun/files/secenekler.txt
adb shell am start -S -n com.marskod.oyun/com.unity3d.player.UnityPlayerGameActivity
```
Dosya telefonda kaldıkça oyun her açılışta onu okur. Bitince sil:
`adb shell rm /sdcard/Android/data/com.marskod.oyun/files/secenekler.txt`

### Bütün deneme seçenekleri
`-performans ac|kapali` (oyuncu ayarını ezer, kaydedilmez) · `-arayuz yok` · `-cevre yok` · `-golgesiz` · `-zemin sade|tam` ·
`-arkaplan yok` · `-arkaplan-olcek 0.25` · `-olcek 0.5` · `-msaa 1` · `-hdr 0` · `-animasyonsuz` · `-kare 30` · `-kalite 0` · `-cizgisiz`.
Birden fazlası birlikte verilebilir: `-e unity '-performans ac -golgesiz'`.

### Seçenek ulaştı mı? (log)
Oyunu başlattıktan sonra:
```
adb logcat -d -s Unity | findstr GRAFIK
```
Şuna benzer bir satır çıkar:
`GRAFIK: Performance | secenekler: -arayuz | arka plan olcegi 0,35 | ekran 1080x2400 | Adreno (TM) 610 (Vulkan) | olcek 0,6 msaa 1 hdr False`
- `secenekler:` kısmında verdiğin seçeneklerin adları yazıyorsa (`-arayuz`, `-golgesiz`...) ulaşmış.
- `secenekler: varsayilan` yazıyorsa ulaşmamış → Yol B'yi kullan.
- Hiç satır yoksa oyun o noktaya gelmeden takılmış demektir; log'un tamamını kaydet:
  `adb logcat -d > log.txt` ve gönder.

### Ekran kartı takıldı mı? (log)
Takılmada log'da `OplusGpuMinidump` / `PREEMPTFAULT` geçer:
```
adb logcat -d | findstr /i "PREEMPT GpuMinidump"
```
Boş çıkıyorsa ekran kartı takılmamış.
