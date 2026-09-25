# Tasarım Taslakları

> Kural (Ragıp, 2026-09-26): Her tasarım denemesi silinmez, **taslak** olarak saklanır ve istenince geri dönülebilir.
> Ragıp'ın beğendiği taslak ekiple paylaşılır ve onunla devam edilir. Kabul görmezse başka bir taslaktan devam edilir.

## Nasıl saklanıyor
- **Git etiketi (tag):** Her taslak, projenin o anki hâline kalıcı bir isimle işaretlenir (`taslak/...`). O hâle dönmek için:
  `git checkout taslak/2-expo-3d-threejs` (geri gelmek için: `git checkout main`).
- **Ekran görüntüsü:** `docs/tasarim/` klasöründe.
- **Oyun motoru denemeleri:** Birbirine karışmasın diye her biri kendi klasöründe, yan yana durur:
  `prototipler/godot/`, ileride `prototipler/unity/`. Ana uygulamaya (Expo) dokunmazlar.

## Taslaklar

| # | Etiket / klasör | Motor | Ne var | Görüntü | Durum |
|---|---|---|---|---|---|
| 1 | `taslak/1-expo-2d-kenney` | Expo (2D, Skia) | GPU'da hesaplanan Mars dokusu, havada yüzen ada, Kenney Sci-Fi RTS çizimleri, canlılık animasyonları, Chakra Petch + JetBrains Mono yazı tipleri | [ilk](ilk-taslak-oyun-ekrani.png) · [ikinci](ikinci-taslak-oyun-ekrani.png) · [üçüncü](ucuncu-taslak-oyun-ekrani.png) | Beğenilmedi |
| 2 | `taslak/2-expo-3d-threejs` | Expo (3D, Three.js) | Kenney Space Kit 3D modelleri, gerçek ışık ve gölge, pürüzlü kaya katmanlı ada | [görüntü](dorduncu-deneme-3d.png) | Beğenilmedi |
| 4 | `prototipler/godot/` · `taslak/4-godot-mars-yuzeyi` | Godot 4.7 (Mobile renderer) | Ufka uzanan Mars yüzeyi (kumullar, mesalar, toz sisi, altın saat gökyüzü), ışıklı kod bölgesi platformu, arkada yaşayan koloni + roket + geçen uzay aracı, minyatür bulanıklığı. Animasyon: yol okları, tarayıcı ışın, ekranda sayaca uçan buz simgesi, dolan görev çubuğu. Buzlu cam arayüz, editör gibi kod paneli. Açmak için: `prototipler/godot/calistir.bat` | [yürüme](godot-taslak4-yurume.png) · [toplama](godot-taslak4-toplama.png) · [bitiş](godot-taslak4-bitis.png) | Ragıp'ın değerlendirmesinde |
| 3 | `prototipler/arsiv/godot-taslak-3/` · `taslak/3-godot` | Godot 4.7 (Mobile renderer) | Kenney Space Kit modelleri, yüzen ada, nebula + yıldızlı gökyüzü, üç ışık + gölge + bloom + kenar yumuşatma, robotun kodu satır satır çalıştırma animasyonu, toz ve kıvılcım parçacıkları, "+1 buz" yazısı, canlı sayaç. Açmak için: `prototipler/arsiv/godot-taslak-3/calistir.bat` | [yürüme](godot-taslak-yurume.png) · [toplama](godot-taslak-toplama.png) · [bitiş](godot-taslak-bitis.png) | Ragıp'ın değerlendirmesinde |

## Yeni taslak eklerken
1. Deneme bitince ekran görüntüsünü `docs/tasarim/` içine koy.
2. `git tag -a taslak/<no>-<kısa-ad> -m "<açıklama>"` ile etiketle.
3. Bu tabloya bir satır ekle.
