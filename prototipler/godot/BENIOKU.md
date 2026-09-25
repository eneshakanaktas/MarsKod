# MarsKod — Godot görsel denemesi (godottaslak4, Godot'daki son taslak)

> Önceki taslaklar arşivde: `prototipler/arsiv/godottaslak1/` (yüzen ada), `godottaslak2/` (Mars yüzeyi), `godottaslak3/` (meteorlu ova). Her birinin kendi `calistir.bat` dosyası var.

Bu klasör, oyunun Godot oyun motoruyla nasıl görüneceğini gösteren **deneme sahnesidir**. Ana uygulamadan (Expo) tamamen ayrıdır.

## Nasıl açılır
- **Bilgisayarda:** `calistir.bat` dosyasına çift tıklayın (Godot 4.7 kurulu olmalı: `winget install GodotEngine.GodotEngine`).
- **Android telefonda:** `build/marskod-godottaslak4.apk` dosyasını telefona atıp açın ("bilinmeyen kaynaklara izin ver" sorarsa izin verin). Dosya büyük olduğu için GitHub'a yüklenmez.

## Sahnede ne var
- **Mars kanyonu:** katman katman yükselen kayalıklar, kanyonun ucunda batan güneş (Mars'ta gün batımı mavidir), zemine uzanan uzun gölgeler, kanyon tabanında alçak toz sisi, uzakta ışıkları yanan küçük üs, altın folyolu iniş aracı.
- **Canlı gökyüzü ve olaylar:** Phobos ve Deimos, yıldızlar, kayan yıldızlar, uzakta dönen toz hortumu, arada bir kanyonun ucuna düşen meteor (parlama, şok dalgası, toz bulutu, hafif sarsıntı).
- **Sinema görünümü:** oyuna girişte kamera kanyona süzülerek iner (üstte ve altta film şeritleri, "Melas Kanyonu" yazısı); film greni, kenar karartma, sinema renk ayarı; buz toplanırken kamera o ana yaklaşır.
- **Buz:** yere gömülü, tozlu, doğal buz parçaları. Toplanınca buhara dönüşür.
- **Robot:** NASA'nın Perseverance gezgini.
- **Animasyon ve efekt anahtarı:** giriş ekranında tek dokunuşla kapatılır; oyun içinde sağ üstteki "Efektler" düğmesiyle istenince açılıp kapanır. Kapalıyken meteor, toz, kamera hareketleri, gren ve bulanıklık durur; oyun sade ve hızlı çalışır.

## Dosyalar
| Dosya | Ne |
|---|---|
| `main.gd` | Sahnenin tamamı, giriş ekranı, efekt anahtarı |
| `canyon.gdshader` | Katmanlı kanyon kayası + kum zemin |
| `cinema.gdshader` | Film görünümü (renk ayarı, gren, film şeritleri) |
| `dust_devil.gdshader` | Toz hortumu |
| `ice.gdshader` | Doğal buz |
| `mars_twilight_sky.gdshader` | Gökyüzü |
| `hologrid.gdshader`, `glass.gdshader` | Kod ızgarası, buzlu cam panel |
| `export_presets.cfg` | Android telefon paketi ayarı |
| `textures/`, `models/nasa/`, `fonts/` | Dokular (CC0), NASA gezgini, yazı tipleri |

## Dikkat edilecekler
- **NASA modeli:** NASA logosu kullanılamaz, "NASA onayladı" izlenimi verilemez; satıştan önce lisans kontrolü gerekir. Ayrıca ~12 MB, telefonda ağır; gerçek oyunda sadeleştirilmeli.
- Telefon paketi deneme sürümüdür (~106 MB). Gerçek sürüm çok daha küçük olur.
- Gerçek Python motoru bağlı değil; robot sabit bir senaryoyu oynatıyor.
