# Telefon paketleri (Android)

Bu klasördeki `.apk` dosyaları doğrudan Android telefona kurulur.

## Nasıl kurulur
1. GitHub'da dosyaya tıklayın → sağ üstteki **indir** (Download raw file) düğmesine basın. (Telefondan GitHub'a girerek de indirebilirsiniz.)
2. Dosyayı telefonda açın. "Bilinmeyen kaynaklardan yüklemeye izin ver" diye sorarsa izin verin.
3. Dosya adındaki taslak adıyla kurulur (ör. "MarsKod godottaslak4", "MarsKod unitytaslak1").

## Dosyalar
| Dosya | Ne | Boyut |
|---|---|---|
| `marskod-oyun.apk` | **Oyunun güncel hâli** (27 Eylül): karttaki Python kodu gerçekten çalışıyor, robot buzları topluyor. Telefonda "MarsKod" adıyla kurulur | ~30 MB |
| `marskod-godottaslak4.apk` | Godot'daki son taslak (Mars kanyonu, sinema görünümü, efekt anahtarı) | ~57 MB |
| `marskod-unitytaslak1.apk` | Unity'deki ilk taslak (koyu tema, Mars zeminine gömülü alan, koloni) | ~29 MB |

Not: Paket gerçek telefonlar içindir (ARM işlemcili; bugünkü telefonların neredeyse hepsi). Unity paketleri bilgisayardaki sanal telefonda (MarsKod_Telefon) da çalışır. Godot paketinin sanal telefon sürümü `build/` klasöründedir (GitHub'a yüklenmez).
