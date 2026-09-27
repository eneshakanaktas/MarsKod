# MarsKod — oyun (Unity 6)

Oyunun asıl evi. 2026-09-27'de (Ragıp) unitytaslak1'den kuruldu; taslak `prototipler/unity/` içinde olduğu gibi duruyor.

## Açmak
- **En kolayı:** `calistir.bat` dosyasına çift tıkla. Telefon boyutunda bir pencere açılır. (Paket yoksa önce üretir; ilk seferde birkaç dakika sürer.) Kod değiştiyse önce `Build` klasörünü sil.
- **Unity içinde:** Unity Hub → *Add* → bu klasörü (`oyun`) seç → `Assets/Scenes/Oyun` sahnesini aç → ▶ (Play). Game penceresinde çözünürlüğü 1080×2340 seç.
- **Telefon paketi (APK):** Unity'de üst menü *MarsKod → Android paketi (APK)*. Çıktı: `Build/Android/marskod.apk`.

## unitytaslak1'den farkı
- **Oyun alanı büyük:** 6×6 alan, daha dik bakış (arka sıralar ezilmez). Kamera, alanı üstteki başlık ile alttaki kod kartı arasında kalan boşluğa kendisi sığdırır: telefon boyu ya da kod uzunluğu değişse de alan hep olabildiğince büyük ve ortada durur. Alanın üstünde ufuk, tepeler ve koloni için ince bir şerit bırakılır.
- Adlar kalıcı hâle getirildi: `Oyun.cs`, `OyunBuild.cs`, `Oyun.unity`; uygulama adı "MarsKod", kimliği `com.marskod.oyun` (geçici — Play Store'a ilk yüklemeden önce kesinleşmeli, sonra değiştirilemez).
- Görünüm (şafak öncesi Mars, koloni, robot, buzlar, kod kartı) taslaktakiyle aynı.

## Dosyalar
- `Assets/Scripts/Oyun.cs` — sahneyi kurar (kamera, ışık, zemin, alan), gösteri programını oynatır. Python motoru henüz bağlı değil.
- `Assets/Scripts/Hud.cs` — arayüz ve simgeler; alan için boş kalan bandı kameraya bildirir.
- `Assets/Scripts/Robot.cs`, `Ice.cs`, `MeshFactory.cs`, `Mats.cs` — robot, buz, şekil üretici, malzemeler.
- `Assets/Shaders/` — gökyüzü/tepeler (Backdrop), zemin (Ground), dış çizgi, halka.
- `Assets/Editor/OyunBuild.cs` — kurulum ve paket üretimi (menü: *MarsKod*).
- Yazı tipleri: Poppins (OFL lisansı) ve JetBrains Mono.
