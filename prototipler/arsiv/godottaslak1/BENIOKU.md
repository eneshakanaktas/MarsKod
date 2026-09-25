# MarsKod — Godot görsel denemesi (godottaslak1)

Bu klasör, oyunun Godot oyun motoruyla nasıl görüneceğini gösteren **deneme sahnesidir**. Ana uygulamadan (Expo) tamamen ayrıdır; ona dokunmaz.

## Nasıl açılır
1. Godot 4.7 kurulu değilse: `winget install GodotEngine.GodotEngine` (ücretsiz, ~100 MB).
2. Bu klasördeki **`calistir.bat`** dosyasına çift tıklayın. Telefon oranında bir pencere açılır.
3. Düzenlemek isterseniz: Godot'yu açın → "Import" → bu klasördeki `project.godot`.

## Sahnede ne var
- Uzayda yüzen Mars adası: kaya katmanları, kraterler, gökyüzünde nebula ve yanıp sönen yıldızlar.
- Işık: sol önden sıcak güneş, arkadan mor kenar ışığı, adanın altından turuncu yansıma, yumuşak gölgeler, parlama (bloom), kenar yumuşatma.
- Robot, kod panelindeki programı **satır satır çalıştırır**: o anki satır yanar, robot kayarak ilerler, tekerleklerinden toz kalkar, buzu toplarken kristal parçalara ayrılıp kaybolur, "+1 buz" yazısı yükselir, sayaç artar. Sonra sahne başa döner.
- Canlılık: buz nefes alır gibi parlar, anten ışıkları yanıp söner, çanak anten döner, havada toz zerreleri uçuşur, kamera çok yavaş salınır.
- Oyun alanının çevresinde süsler: roket, çanak anten, jeneratör, variller, astronotlar.

## Dosyalar
| Dosya | Ne |
|---|---|
| `main.gd` | Sahnenin tamamı (harita, ışık, animasyon, arayüz) |
| `space_sky.gdshader` | Uzay gökyüzü |
| `models/` | Kenney Space Kit 3D modelleri (CC0) |
| `fonts/` | Chakra Petch + JetBrains Mono (Open Font License) |

## Henüz yok
- Gerçek Python motoru bağlı değil; robot sabit bir senaryoyu oynatıyor. Bu sahne yalnızca görünümü denemek için.
- Telefon paketi (APK) henüz alınmadı; bilgisayarda telefon oranında çalışıyor.
