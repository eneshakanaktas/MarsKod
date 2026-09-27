# Tasarım Taslakları

> İsimlendirme (Ragıp, 2026-09-26): taslaklar motora göre adlandırılır: `expotaslak1`, `godottaslak1`, `unitytaslak1`…
>
> Kural (Ragıp, 2026-09-26): Her tasarım denemesi silinmez, **taslak** olarak saklanır ve istenince geri dönülebilir.
> Ragıp'ın beğendiği taslak ekiple paylaşılır ve onunla devam edilir. Kabul görmezse başka bir taslaktan devam edilir.

## Nasıl saklanıyor
- **Git etiketi (tag):** Her taslak, projenin o anki hâline kalıcı bir isimle işaretlenir (ör. `godottaslak3`). O hâle dönmek için:
  `git checkout expotaslak2` (geri gelmek için: `git checkout main`).
- **Ekran görüntüsü:** `docs/tasarim/` klasöründe.
- **Video (10-18 sn):** `docs/tasarim/gelisim/video/` klasöründe (Ragıp, 2026-09-27). Expo taslakları hareketsiz ekran olduğu için videosuz.
- **Oyun motoru denemeleri:** Birbirine karışmasın diye her biri kendi klasöründe, yan yana durur:
  `prototipler/godot/`, ileride `prototipler/unity/`. Ana uygulamaya (Expo) dokunmazlar.

## Taslaklar

**Gelişim süreci (hepsi tarih sırasıyla, videolu):** `gelisim/index.html` dosyasına çift tıkla (tarayıcıda açılır). Ekip bağlantısı: https://claude.ai/artifact/NLbqv8cZmZfn8TXWpWDofs

| Taslak | Klasör | Motor | Ne var | Görüntü | Durum |
|---|---|---|---|---|---|
| **expotaslak1** | — (sadece etiket) | Expo (2D, Skia) | GPU'da hesaplanan Mars dokusu, havada yüzen ada, Kenney Sci-Fi RTS çizimleri, canlılık animasyonları, Chakra Petch + JetBrains Mono yazı tipleri | [ilk](expotaslak1-a.png) · [ikinci](expotaslak1-b.png) · [üçüncü](expotaslak1-c.png) | Beğenilmedi |
| **expotaslak2** | — (sadece etiket) | Expo (3D, Three.js) | Kenney Space Kit 3D modelleri, gerçek ışık ve gölge, pürüzlü kaya katmanlı ada | [görüntü](expotaslak2.png) | Beğenilmedi |
| **godottaslak1** | `prototipler/arsiv/godottaslak1/` | Godot 4.7 (Mobile renderer) | Kenney Space Kit modelleri, yüzen ada, nebula + yıldızlı gökyüzü, üç ışık + gölge + bloom + kenar yumuşatma, robotun kodu satır satır çalıştırma animasyonu, toz ve kıvılcım parçacıkları, "+1 buz" yazısı, canlı sayaç. Açmak için: `prototipler/arsiv/godottaslak1/calistir.bat` | [yürüme](godottaslak1-yurume.png) · [toplama](godottaslak1-toplama.png) · [bitiş](godottaslak1-bitis.png) | Saklandı; godottaslak2 ile geliştirildi |
| **godottaslak2** | `prototipler/arsiv/godottaslak2/` | Godot 4.7 (Mobile renderer) | Ufka uzanan Mars yüzeyi (kumullar, mesalar, toz sisi, altın saat gökyüzü), ışıklı kod bölgesi platformu, arkada yaşayan koloni + roket + geçen uzay aracı, minyatür bulanıklığı. Animasyon: yol okları, tarayıcı ışın, ekranda sayaca uçan buz simgesi, dolan görev çubuğu. Buzlu cam arayüz, editör gibi kod paneli. Açmak için: `prototipler/arsiv/godottaslak2/calistir.bat` | [yürüme](godottaslak2-yurume.png) · [toplama](godottaslak2-toplama.png) · [bitiş](godottaslak2-bitis.png) | Saklandı; beğenildi ama godottaslak1'e benzediği için godottaslak3 yapıldı |
| **godottaslak3** | `prototipler/arsiv/godottaslak3/` | Godot 4.7 (Mobile renderer) | Gerçekçi yön: gerçek fotoğraflardan yapılmış kaya/toprak dokuları (ambientCG, CC0), NASA'nın Perseverance gezgini modeli, gerçek Mars'taki gibi mavi gün batımı, yıldızlar, Samanyolu, Dünya'nın parlak noktası, Phobos ve Deimos uyduları, kayan yıldızlar. **Arkada düşen alevli meteorlar:** ateş kuyruğu, çarpışma parlaması, şok dalgası halkası, kıvılcımlar, toz bulutu, hafif ekran sarsıntısı. Yere yansıyan hologram kod ızgarası, projektör kuleleri, ışığı kıran buz kristalleri (toplanınca buhara dönüşür), kubbeli koloni + cam sera + iletişim kulesi + güneş paneli tarlası + çelik roket. Açmak için: `prototipler/arsiv/godottaslak3/calistir.bat` | [genel](godottaslak3-genel.png) · [meteor](godottaslak3-meteor.png) · [toplama](godottaslak3-toplama.png) · [telefon](godottaslak3-telefon.png) | Saklandı; buzlar fazla parlak ve godottaslak2'ye çok benzediği için godottaslak4 yapıldı |
| **godottaslak4** | `prototipler/godot/` | Godot 4.7 (Mobile renderer) | **Godot'daki son taslak.** Tamamen yeni mekân: katman katman yükselen kayalıklarla çevrili Mars kanyonu (Melas Kanyonu), kanyonun ucunda batan güneş, zemine uzanan uzun gölgeler, alçak toz sisi, uzakta dönen toz hortumu, ufka düşen meteor. Doğal, tozlu buz yatakları. Sinema görünümü: girişte kameranın kanyona süzülmesi + film şeritleri, film greni, renk ayarı, buz toplanırken kameranın yaklaşması. **Giriş ekranında "Animasyon ve efektler" anahtarı**; oyun içinde sağ üstteki düğmeyle de tek tuşla kapanıp açılır. Telefonda (sanal) denendi. Açmak için: `prototipler/godot/calistir.bat` · Telefon paketi: `build/marskod-godottaslak4.apk` | [giriş ekranı](godottaslak4-giris-ekrani.png) · [giriş sahnesi](godottaslak4-giris-sahnesi.png) · [meteor](godottaslak4-meteor.png) · [efektler kapalı](godottaslak4-efektler-kapali.png) · [telefonda toplama](godottaslak4-telefon-toplama.png) | Ragıp'ın değerlendirmesinde; sıradaki taslak Unity ile (unitytaslak1) |
| **unitytaslak1** | `prototipler/unity/` | Unity 6 (URP, Mobile) | **İlk Unity taslağı, "premium sade" yön** (The Farmer Was Replaced + MyRisale esinli). **Koyu tema, Mars'ta şafaktan hemen önce:** tepelerin ardından taşan görünmez güneşin ışıltısı (tahtaya doğru yayılır, yakındaki yıldızları söndürür), **rastgele parlayan yıldızlar** (✦ ile aç/kapa), geçen uydu, sabit küçük kayalar, hafif kum fırtınası sisi, ufukta gelişmiş koloni ve yumuşak gölgeleri (su depoları, kubbeler, kule, radar, roket, paneller, pist ışıkları). Ortada **Mars yüzeyine gömülü oyun alanı** (tek parça zemin, ince kare çizgileri, köşe direkleri); aynı zemin kayalar ve kraterlerle ufka kadar uzanıp arka plana karışır; kodla şekillendirilmiş tombul oyuncak robot (ince dış çizgi, göz kırpar, sürerken başını bize çevirir) ve buz kümeleri. Altta koyu kod kartı (satır yanar), ipucu / Çalıştır / baştan al. Ölçülü animasyon: kayarak ilerleme, buzda "pop" + halka, sonda kısa kutlama. Açmak için: `prototipler/unity/calistir.bat` | [bekleme](unitytaslak1-bekleme.png) · [toplama](unitytaslak1-toplama.png) · [bitiş](unitytaslak1-bitti.png) · [yıldızsız](unitytaslak1-yildizsiz.png) | Enes'in değerlendirmesinde |

## Expo taslaklarını canlı açmak (Ragıp, 2026-09-27)
Ana projeye dokunmadan, taslak ayrı bir klasöre açılır: `git worktree add ..\MarsKod-expotaslak1 expotaslak1` → o klasörde `npm install` → `npx expo start --android`. Sanal telefonda ekran siyah kalırsa: `adb reverse tcp:8081 tcp:8081` (port neyse) ve Expo Go'da `exp://127.0.0.1:<port>` açılır. İş bitince klasör silinir (`git worktree remove ..\MarsKod-expotaslak1`), ~500 MB yer açılır. Kısaca: Claude'a "expotaslak1'i aç" demek yeterli.
## Yeni taslak eklerken
1. Deneme bitince ekran görüntüsünü `docs/tasarim/` içine koy.
2. `git tag -a <motor>taslak<no> -m "<açıklama>"` ile etiketle (ör. `godottaslak4`, `unitytaslak1`). Numara her motorda 1'den başlar.
3. Bu tabloya bir satır ekle.
4. **Video çek** ve `gelisim/index.html` sayfasına tarih sırasıyla ekle (küçük resim `gelisim/img/`, video `gelisim/video/`; 450 px genişlik, H.264, ~1-2 MB). Sayfa yenilenince ekip bağlantısı da güncellenir. Video yolları (videoyu Claude çeker, Ragıp'ın çekmesi gerekmez):
   - **Godot:** kendi film modu, taslağa dokunmadan: `godot --path <klasör> --write-movie x.avi --fixed-fps 30 --quit-after 450 --resolution 540x1200` (arşivdeki taslakta önce `godot --headless --path <klasör> --import`).
   - **Unity:** `scripts/video-kaydet.ps1` (oyunu gösteri modunda açıp pencereyi kaydeder).
   - Çeviri/kesme: `imageio-ffmpeg` paketindeki ffmpeg (`py -3.12 -m pip install --user imageio-ffmpeg`).
