# unitytaslak1 (Unity 6)

MarsKod'un Unity ile ilk görsel taslağı (Enes, 2026-09-26). Deneme amaçlıdır; kalıcı oyunun temeli sayılmaz.

## Açmak
- **En kolayı:** `calistir.bat` dosyasına çift tıkla. Telefon boyutunda bir pencere açılır. (İlk seferde paket üretildiği için birkaç dakika bekletir.)
- **Unity içinde:** Unity Hub → *Add* → bu klasörü (`prototipler/unity`) seç → proje açılınca `Assets/Scenes/Taslak` sahnesini aç → ▶ (Play). Oyun ekranını telefon oranına almak için Game penceresinde çözünürlüğü 1080×2340 seç.
- **Telefon paketi (APK):** Unity'de üst menü *MarsKod → Android paketi (APK)*. Çıktı: `Build/Android/`.

## Ekranda ne var
- **Koyu tema** (Enes'in isteği): Mars'ta şafaktan hemen önce. Güneş görünmez ama tepelerin ardından turuncu ışıltısı taşar; tepelerin ve binaların üst kenarlarına vurur, ovadan tahtaya doğru yumuşak ışık yayılır, güneşe yakın yıldızlar söner, yukarı çıktıkça sıklaşır. Tepelerin ve koloninin üzerinden yavaşça hafif bir kum fırtınası sisi geçer; koloni ışıkları sisin içinde hafif hale yapar. Rastgele parlayan yıldızlar (sağ üstteki ✦ ile aç/kapa), yavaşça geçen uydu, sabit duran küçük kayalar (Phobos + 4 parça). Ufukta gelişmiş koloni (arkadan vuran güneşle bize uzanan yumuşak gölgeleriyle): su işleme depoları, kubbeler ve tüpler, ışıklı kontrol kulesi, radar çanağı, roket ve rampa kulesi, güneş panelleri, sırayla yanan pist ışıkları, yanıp sönen işaret ışıkları.
- Ortada **Mars yüzeyine gömülü 5×4 oyun alanı**: tek parça tozlu Mars toprağı, kare sınırları ince soluk çizgiler, köşelerde ışıklı işaret direkleri; buzların çevresinde buzlanma, çakıllar, iri kayalar, küçük bir krater. Aynı zemin her yöne devam eder (kayalar, kraterler, hafif tepecikler) ve uzakta tozlanarak tepelere ve koloniye karışır. Üstünde tombul oyuncak robot (gözlerinden önündeki zemine hafif ışık düşer) ve 3 buz kümesi. Işık gerçekçi: güneş tepenin ardında, zemin ona doğru hafifçe aydınlanır, kameraya doğru kararır. Hepsi kodla şekillendirildi (hazır model yok, lisans derdi yok).
- Altta koyu kod kartı (gösteri kodu; Python burada gerçekten çalışmaz), ipucu 💡, Çalıştır, baştan al ↺.
- Animasyonlar ölçülü: çalışan satır yanar, robot kayarak ilerler ve başını bize çevirir, buz "pop" + halka, sonda tek kısa kutlama.

## Dosyalar
- `Assets/Scripts/Taslak.cs` — sahneyi kurar, gösteri programını oynatır.
- `Assets/Scripts/Robot.cs`, `Ice.cs`, `MeshFactory.cs` — robot, buz, şekil üretici.
- `Assets/Scripts/Hud.cs` — arayüz ve simgeler.
- `Assets/Shaders/Backdrop.shader` — gökyüzü, tepeler, yıldızlar.
- `Assets/Editor/TaslakBuild.cs` — kurulum ve paket üretimi (menü: *MarsKod*).
- Yazı tipleri: Poppins (OFL lisansı) ve JetBrains Mono.
