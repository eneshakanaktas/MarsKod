# Tasarım Taslakları

> İsimlendirme (Ragıp, 2026-09-26): taslaklar motora göre adlandırılır: `expotaslak1`, `godottaslak1`, `unitytaslak1`…
>
> Kural (Ragıp, 2026-09-26): Her tasarım denemesi silinmez, **taslak** olarak saklanır ve istenince geri dönülebilir.
> Ragıp'ın beğendiği taslak ekiple paylaşılır ve onunla devam edilir. Kabul görmezse başka bir taslaktan devam edilir.

## Nasıl saklanıyor
- **Git etiketi (tag):** Her taslak, projenin o anki hâline kalıcı bir isimle işaretlenir (ör. `godottaslak3`). O hâle dönmek için:
  `git checkout expotaslak2` (geri gelmek için: `git checkout main`).
- **Ekran görüntüsü:** `docs/tasarim/` klasöründe.
- **Oyun motoru denemeleri:** Birbirine karışmasın diye her biri kendi klasöründe, yan yana durur:
  `prototipler/godot/`, ileride `prototipler/unity/`. Ana uygulamaya (Expo) dokunmazlar.

## Taslaklar

| Taslak | Klasör | Motor | Ne var | Görüntü | Durum |
|---|---|---|---|---|---|
| **expotaslak1** | — (sadece etiket) | Expo (2D, Skia) | GPU'da hesaplanan Mars dokusu, havada yüzen ada, Kenney Sci-Fi RTS çizimleri, canlılık animasyonları, Chakra Petch + JetBrains Mono yazı tipleri | [ilk](expotaslak1-a.png) · [ikinci](expotaslak1-b.png) · [üçüncü](expotaslak1-c.png) | Beğenilmedi |
| **expotaslak2** | — (sadece etiket) | Expo (3D, Three.js) | Kenney Space Kit 3D modelleri, gerçek ışık ve gölge, pürüzlü kaya katmanlı ada | [görüntü](expotaslak2.png) | Beğenilmedi |
| **godottaslak1** | `prototipler/arsiv/godottaslak1/` | Godot 4.7 (Mobile renderer) | Kenney Space Kit modelleri, yüzen ada, nebula + yıldızlı gökyüzü, üç ışık + gölge + bloom + kenar yumuşatma, robotun kodu satır satır çalıştırma animasyonu, toz ve kıvılcım parçacıkları, "+1 buz" yazısı, canlı sayaç. Açmak için: `prototipler/arsiv/godottaslak1/calistir.bat` | [yürüme](godottaslak1-yurume.png) · [toplama](godottaslak1-toplama.png) · [bitiş](godottaslak1-bitis.png) | Saklandı; godottaslak2 ile geliştirildi |
| **godottaslak2** | `prototipler/arsiv/godottaslak2/` | Godot 4.7 (Mobile renderer) | Ufka uzanan Mars yüzeyi (kumullar, mesalar, toz sisi, altın saat gökyüzü), ışıklı kod bölgesi platformu, arkada yaşayan koloni + roket + geçen uzay aracı, minyatür bulanıklığı. Animasyon: yol okları, tarayıcı ışın, ekranda sayaca uçan buz simgesi, dolan görev çubuğu. Buzlu cam arayüz, editör gibi kod paneli. Açmak için: `prototipler/arsiv/godottaslak2/calistir.bat` | [yürüme](godottaslak2-yurume.png) · [toplama](godottaslak2-toplama.png) · [bitiş](godottaslak2-bitis.png) | Saklandı; beğenildi ama godottaslak1'e benzediği için godottaslak3 yapıldı |
| **godottaslak3** | `prototipler/godot/` | Godot 4.7 (Mobile renderer) | Gerçekçi yön: gerçek fotoğraflardan yapılmış kaya/toprak dokuları (ambientCG, CC0), NASA'nın Perseverance gezgini modeli, gerçek Mars'taki gibi mavi gün batımı, yıldızlar, Samanyolu, Dünya'nın parlak noktası, Phobos ve Deimos uyduları, kayan yıldızlar. **Arkada düşen alevli meteorlar:** ateş kuyruğu, çarpışma parlaması, şok dalgası halkası, kıvılcımlar, toz bulutu, hafif ekran sarsıntısı. Yere yansıyan hologram kod ızgarası, projektör kuleleri, ışığı kıran buz kristalleri (toplanınca buhara dönüşür), kubbeli koloni + cam sera + iletişim kulesi + güneş paneli tarlası + çelik roket. Açmak için: `prototipler/godot/calistir.bat` | [genel](godottaslak3-genel.png) · [meteor](godottaslak3-meteor.png) · [toplama](godottaslak3-toplama.png) | Ragıp'ın değerlendirmesinde |

## Yeni taslak eklerken
1. Deneme bitince ekran görüntüsünü `docs/tasarim/` içine koy.
2. `git tag -a <motor>taslak<no> -m "<açıklama>"` ile etiketle (ör. `godottaslak4`, `unitytaslak1`). Numara her motorda 1'den başlar.
3. Bu tabloya bir satır ekle.
