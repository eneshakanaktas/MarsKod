# Telefon denemesi: oyun açılmıyorsa ne yapılır

**Kimin için:** Hamza (test telefonu Oppo A74) ve telefonda deneme yapacak herkes.
**Neden:** 2026-10-03'te Oppo A74'te oyun siyah ekranda kaldı. Telefon ekran kartını (GPU) durduruyordu: bir çizim zamanında bitmedi.
Güçlü tahmin: gökyüzü/tepeler/koloni çizimi çok ağır. 2026-10-03'te bu çizim hafifletildi (aşağıda). Bu sayfa,
hafifletmenin yetip yetmediğini ve yetmezse **neyin** ağır geldiğini adım adım bulmak için.

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

### Seçenek ulaştı mı? (log)
Oyunu başlattıktan sonra:
```
adb logcat -d -s Unity | findstr GRAFIK
```
Şuna benzer bir satır çıkar:
`GRAFIK: -arkaplan | arka plan olcegi 0.5 | ekran 1080x2400 | Adreno (TM) 610 (Vulkan) | olcek 1 msaa 4 hdr True`
- Satırın başında verdiğin seçeneklerin adları yazıyorsa (`-arkaplan`, `-zemin`...) ulaşmış.
- `GRAFIK: varsayilan` yazıyorsa ulaşmamış → Yol B'yi kullan.
- Hiç satır yoksa oyun o noktaya gelmeden takılmış demektir; log'un tamamını kaydet:
  `adb logcat -d > log.txt` ve gönder.

### Ekran kartı takıldı mı? (log)
Takılmada log'da `OplusGpuMinidump` / `PREEMPTFAULT` geçer:
```
adb logcat -d | findstr /i "PREEMPT GpuMinidump"
```
Boş çıkıyorsa ekran kartı takılmamış.
