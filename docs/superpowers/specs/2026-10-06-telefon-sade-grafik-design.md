# Telefon için "Sade" grafik + ölçüm — tasarım

**Tarih:** 2026-10-06 · **Yazan:** Enes + Claude (Opus 5.5) · **Görsel onay:** Ragıp

> **İsim değişikliği (Enes, 2026-10-06):** Oyuncuya görünen ad "Sade/Güzel" değil **"Performans modu"** (anahtar açık = eski "Sade"; telefonda açık, bilgisayarda kapalı başlar; not: "Açıkken oyun daha akıcı, görüntü biraz sadeleşir"). Kodda `enum GraphicsMode { Performance, Full }`, `GraphicsQuality.Mode`, `PlayerPrefs("performans")` 1 = açık, deneme seçeneği `-performans ac|kapali`, log `GRAFIK: Performance/Full`, `KARE: ... | performans ac/kapali`. Aşağıda "Sade" = Performans modu açık, "Güzel" = kapalı.

## Amaç
Oyun orta seviye telefonlarda (test: Oppo A74, Adreno 610, 1080×2400) akıcı oynansın: **≥30 kare/sn, hiçbir deneme seçeneği vermeden.** Şimdi 1 kare/sn; Hamza'nın ölçümünde her şey kapalıyken bile 18 (bkz. ILERLEME.md, Hamza 10-04).
Bilgisayar görünümü değişmez. Paket büyümez. Telefon bugün elimizde değil: tek pakette hem sadeleştirme hem de "neyin ağır olduğunu" söyleyen ölçüm gider; Hamza bir denemeyle sonucu getirir.

## Kararlar (Enes, 2026-10-06)
- Yol A: ölçüm + sadeleştirme aynı pakette.
- Telefonda varsayılan **Sade**; Bölümler ekranında **"Grafik: Sade / Güzel"** ayarıyla değişir, telefonda hatırlanır. Bilgisayar varsayılanı **Güzel**.
- Kapsam dışı: robot dururken çizimi yavaşlatmak (pil/ısı), telefona göre otomatik seçim.

## 1. Sade görünüm
| Ayar | Güzel (bugünkü) | Sade |
|---|---|---|
| Çözünürlük ölçeği (yalnız 3B sahne; arayüz tam keskin) | 1 | 0,6 |
| Kenar yumuşatma (MSAA) | 4 | kapalı |
| HDR | açık | kapalı |
| Gölge | yumuşak, harita 2048 | sert, harita 1024 |
| Çizim yolu (URP) | Forward+ | ~~Forward~~ **Forward+ kalır** (uygulamada çıkarıldı: `OyunBuild.cs` notu — telefonda Forward'da kodla üretilen Lit malzemeler simsiyah çıkmıştı) |
| Zemin | tam ayrıntı | ayrı hafif çeşit (aşağıda) |
| Gökyüzü dokusu | ekranın 0,5'i, animasyonda 20/sn | ekranın 0,35'i, animasyonda 10/sn |

**Hafif zemin:** `Ground.shader`'a `MARSKOD_SADE` anahtar kelimesiyle ayrı bir çeşit (`multi_compile`). Sade çeşitte pahalı kod hiç derlenmez (bugünkü `_GroundSimple` sayısı ağır kodu yine taşıyor; ekran kartında bu bile yavaşlatabilir). Kalan: ana renk lekesi (tek `vnoise2`), kare çizgileri, alan kenarı izi, ışık/gölge/şafak/lambalar/robot ışığı, ufka karışma. Giden: kraterler, kum dalgaları, ince taneler, benekler, buz levhası çatlakları. Buz lekeleri yalnızca yuvarlak yumuşak leke (fbm'siz). Kutup zemini aynı mantıkla. `-zemin sade` deneme seçeneği bu anahtar kelimeyi açar; `_GroundSimple` kaldırılır.
Uzak kayalar (`FarRock.shader`) zaten hafif; değişmez.

## 2. Ölçüm
- **Gösterge** (`FrameRateMeter`, üç parmakla açılır): kare/sn + en uzun kare + **işlemci ms + ekran kartı ms** (`FrameTimingManager`; Player ayarında `enableFrameTimingStats: 1`). Ekran kartı süresi alınamazsa "—".
- **Kayıt satırı:** gösterge kapalı olsa bile 5 sn'de bir log'a: `KARE: 24 kare/sn | en uzun 61 ms | islemci 12 ms | ekran karti 38 ms | grafik sade`. Hamza: `adb logcat -d -s Unity | findstr KARE`.
- **Yeni deneme seçenekleri** (`GraphicsOptions`): `-grafik sade|guzel` (ayarı ezer), `-arayuz yok` (arayüz gizlenir; yalnız ölçüm), `-cevre yok` (alan çevresindeki kayalar/eşyalar kurulmaz).
- `docs/tasarim/telefon-denemesi.md` yeni sırayla güncellenir.

## 3. Yapı
- **`GraphicsQuality.cs` (yeni):** iki seviye (Sade/Güzel); varsayılan `Application.isMobilePlatform ? Sade : Güzel`; `PlayerPrefs("grafik")`. `Apply(level)`: URP ayarları, renderer seçimi, `MARSKOD_SADE` genel anahtar kelimesi, gökyüzü ölçeği/hızı, gölge türü. Seviye değişince `Changed` olayı (Oyun ışığı ve gökyüzü önbelleğini günceller). Oyun açıkken anında uygulanır.
- **URP ayar dosyası kopyası:** oyun başında etkin URP ayar dosyasının çalışma anı kopyası kullanılır (`QualitySettings.renderPipeline = Instantiate(...)`); böylece Unity editöründe oynarken ayar dosyaları diskte değişmez.
- **Forward renderer:** yeni `Mobile_Renderer_Forward.asset` (Mobile_Renderer'ın kopyası, `m_RenderingMode: 0`), hem Mobile hem PC URP ayarında listenin 2. sırasında. Sade → kamera 1 numaralı renderer'ı kullanır.
- **`GraphicsOptions`** deneme seçenekleri `GraphicsQuality`'den sonra uygulanır, üstüne yazar (oyuncu ayarı ile test düğmeleri ayrı).
- **`BackdropCache`:** ölçek ve animasyon hızı çalışırken değiştirilebilir (`SetQuality(scale, rate)`).
- **`LevelSelect`:** "Arka plan animasyonları" satırının çizimi ortak `SwitchRow(başlık, not, açık mı, tıklanınca)` parçasına alınır; altına "Grafik" satırı (açık = Güzel; not: "Sade: telefon daha akıcı").

## 4. Doğrulama
- `dotnet test` (motora dokunulmaz, sayı aynı kalmalı: 1285).
- Windows paketi hatasız; `-grafik sade` ve varsayılan ile Bölüm 1, 11, 21 ekran görüntüleri yan yana → `docs/tasarim/telefon-sade-2026-10-06/` (Ragıp onayı için). Mevcut otomatik denetimler (yedi fare + klavye) geçer.
- Android paketi `paketler/marskod-oyun.apk`; sanal telefonda açılıyor, `GRAFIK:` ve `KARE:` satırları log'da (hız sanal telefonda anlamsız).
- **Hamza:** ayarsız deneme → `KARE:` satırları. <30 ise rehberdeki sırayla `-arayuz yok`, `-cevre yok`, `-golgesiz`; işlemci/ekran kartı süreleri bir sonraki adımı belirler.

## Riskler
- 18'lik tavanın sebebi bu listede olmayabilir → ölçüm kısmı tam bunun için.
- Forward'a geçişte görünüm farkı çıkarsa ekran görüntüleriyle yakalanır; çıkarsa Sade'de Forward+ kalır.
- Sade görünüm Ragıp'ın onayına bağlı; zemin ayrıntısı seviyesi görüntülere göre ayarlanabilir.
