# MarsKod — Godot görsel denemesi (godottaslak2)

> godottaslak1 (yüzen ada) arşivde: `prototipler/arsiv/godottaslak1/` (kendi `calistir.bat` dosyasıyla açılır).

Bu klasör, oyunun Godot oyun motoruyla nasıl görüneceğini gösteren **deneme sahnesidir**. Ana uygulamadan (Expo) tamamen ayrıdır; ona dokunmaz.

## Nasıl açılır
1. Godot 4.7 kurulu değilse: `winget install GodotEngine.GodotEngine` (ücretsiz, ~100 MB).
2. Bu klasördeki **`calistir.bat`** dosyasına çift tıklayın. Telefon oranında bir pencere açılır.
3. Düzenlemek isterseniz: Godot'yu açın → "Import" → bu klasördeki `project.godot`.

## Sahnede ne var
- **Mars yüzeyi:** ufka kadar uzanan kumullar, uzakta dev kaya kütleleri (mesalar), toz sisi, altın saat gökyüzü (Mars'taki gibi güneş çevresi mavimsi).
- **Kod bölgesi:** metal platform, turuncu uyarı şeridi, nabız gibi atan ışık çizgileri, köşelerde yanıp sönen işaret direkleri.
- **Yaşayan koloni:** hangarlar, içi yeşil parlayan sera, çanak antenli laboratuvar, rampada roket (buhar çıkarır), astronotlar; gökyüzünden ara sıra bir uzay aracı geçer.
- **Işık ve görüntü:** alçak güneşten uzun gölgeler, soğuk kenar ışığı, parlama (bloom), kenar yumuşatma, uzağı hafif bulanıklaştıran minyatür etkisi.
- **Robotun kodu çalıştırması:** o anki satır parlak şeritle işaretlenir; robot gitmeden önce yolunu parlayan oklarla gösterir, kayarak ilerler, toz kaldırır; buzu tarayıcı ışınla toplar, kristal parçalanır, "+1 buz" yükselir, buz simgesi ekranda uçarak sayaca gider, sayaç ve görev çubuğu dolar. Sonra sahne başa döner.
- **Arayüz:** arkasındaki sahneyi bulanık gösteren buzlu cam paneller, editör penceresi gibi kod paneli (başlık çubuğu, satır numaraları), simgeli düğmeler.

## Dosyalar
| Dosya | Ne |
|---|---|
| `main.gd` | Sahnenin tamamı (yüzey, koloni, ışık, animasyon, arayüz) |
| `mars_sky.gdshader` | Mars gökyüzü |
| `zone.gdshader` | Işıklı kod bölgesi zemini |
| `glass.gdshader` | Buzlu cam panel |
| `models/` | Kenney Space Kit 3D modelleri (CC0) |
| `fonts/` | Chakra Petch + JetBrains Mono (Open Font License) |

## Henüz yok
- Gerçek Python motoru bağlı değil; robot sabit bir senaryoyu oynatıyor. Bu sahne yalnızca görünümü denemek için.
- Telefon paketi (APK) henüz alınmadı; bilgisayarda telefon oranında çalışıyor.
