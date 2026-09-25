# MarsKod — Godot görsel denemesi (taslak 5, gerçekçi)

> Önceki taslaklar arşivde: `prototipler/arsiv/godot-taslak-3/` (yüzen ada) ve `prototipler/arsiv/godot-taslak-4/` (Mars yüzeyi). Her birinin kendi `calistir.bat` dosyası var.

Bu klasör, oyunun Godot oyun motoruyla nasıl görüneceğini gösteren **deneme sahnesidir**. Ana uygulamadan (Expo) tamamen ayrıdır; ona dokunmaz.

## Nasıl açılır
1. Godot 4.7 kurulu değilse: `winget install GodotEngine.GodotEngine` (ücretsiz, ~100 MB).
2. Bu klasördeki **`calistir.bat`** dosyasına çift tıklayın. Telefon oranında bir pencere açılır.
3. Düzenlemek isterseniz: Godot'yu açın → "Import" → bu klasördeki `project.godot`.

## Sahnede ne var
- **Gerçekçi Mars zemini:** gerçek fotoğraflardan yapılmış toprak, kum ve kaya dokuları Mars kırmızısına boyanır; dik yamaçlar kendiliğinden kaya görünür. Ufukta dev mesalar, yerde yüzlerce kaya.
- **Gökyüzü:** gerçek Mars'taki gibi güneşin çevresi mavi (Mars'ta gün batımı mavidir), uzak ufuk tozlu. Yıldızlar, Samanyolu, Dünya parlak mavi bir nokta olarak görünür. Phobos ve Deimos uyduları. Kayan yıldızlar.
- **Meteorlar:** birkaç saniyede bir arkadaki araziye alevli bir meteor düşer. Ateş ve duman kuyruğu, çarpınca parlama, genişleyen şok dalgası halkası, kıvılcımlar, yükselen toz bulutu ve hafif ekran sarsıntısı.
- **Robot:** NASA'nın Perseverance gezgini (farıyla), giderken toz kaldırır.
- **Kod bölgesi:** yere yansıyan hologram ızgara (tarama dalgası), köşelerde projektör kuleleri.
- **Buz kristalleri:** arkasındaki görüntüyü hafifçe kıran, içten mavi parlayan kristal kümeleri. Toplanınca parçalanır ve buhara dönüşür (Mars'ın ince havasında buz doğrudan buhar olur).
- **Koloni:** kubbeli yaşam alanı, bağlantı tüneli, içinde bitkiler olan cam sera, dönen çanak antenli laboratuvar kulesi, güneş paneli tarlası, beton rampada çelik roket (buhar çıkarır).
- **Arayüz:** taslak 4'teki buzlu cam paneller, editör gibi kod paneli, satır satır çalışma, ekranda sayaca uçan buz simgesi.

## Dosyalar
| Dosya | Ne |
|---|---|
| `main.gd` | Sahnenin tamamı |
| `terrain.gdshader` | Fotoğraf dokulu Mars zemini |
| `mars_twilight_sky.gdshader` | Mavi gün batımı, yıldızlar, kayan yıldızlar |
| `hologrid.gdshader` | Hologram kod ızgarası |
| `ice.gdshader` | Işığı kıran buz |
| `glass.gdshader` | Buzlu cam panel |
| `textures/` | ambientCG dokuları (CC0, serbest kullanım) |
| `models/nasa/` | Perseverance modeli (NASA/JPL-Caltech) — lisans notu aşağıda |
| `fonts/` | Chakra Petch + JetBrains Mono (Open Font License) |

## Dikkat edilecekler
- **NASA modeli:** NASA modelleri genelde serbesttir ama NASA logosu kullanılamaz ve "NASA onayladı" izlenimi verilemez. Satışa çıkmadan önce lisans kontrol edilmeli; gerekirse kendi gezginimizi yaparız.
- **Boyut:** Perseverance modeli tek başına ~12 MB ve çok ayrıntılı; telefonda ağır gelebilir. Gerçek oyunda sadeleştirilmiş hâli kullanılmalı. Dokular ~7 MB.
- Gerçek Python motoru henüz bağlı değil; robot sabit bir senaryoyu oynatıyor.
