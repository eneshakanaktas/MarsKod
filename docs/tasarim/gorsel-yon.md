# MarsKod — Görsel Yön

> Tarih: 2026-09-25 · Yazan: Ragıp (Claude ile) · Durum: İlk taslak, ekip onayı bekliyor
> Esin kaynağı: *The Farmer Was Replaced* (oyun mantığı ve sadelik). Görseller tamamen özgün; tema çiftlik değil uzay.

Telefondaki görüntüler (kod paneli ve sayaçlar şimdilik örnek; sağ üstteki dişli Expo'nun geliştirici düğmesi, oyunda görünmez):
- [expotaslak1-a.png](expotaslak1-a.png): düz renkli şekiller. Yetersiz bulundu.
- [expotaslak1-b.png](expotaslak1-b.png): ekran kartında hesaplanan arazi dokusu (kumullar, ışık-gölge), diorama yamacı, üst ve ön yüzlü 3 boyut hissi veren nesneler, parıltılar, toz.
- [expotaslak1-c.png](expotaslak1-c.png) (**güncel**): karma yaklaşım. Depo, laboratuvar ve kayalar Kenney çizimi; robot, buz ve sera kendi çizimimiz. Yeni yazı tipleri ve canlılık animasyonları var (parıltılar yanıp söner, robot yaylanır, anten yanıp söner, toz sürüklenir).

## Kaynaklar ve lisanslar
| Kaynak | Ne için | Lisans |
|---|---|---|
| [Kenney — Sci-Fi RTS](https://kenney.nl/assets/sci-fi-rts) | Depo, laboratuvar, kayalar (+ ileride: su tankları, anten, hangar, fabrika, sondaj, buz zemini) · `assets/sprites/kenney-scifi-rts/` | CC0 (her amaçla serbest; isim vermek zorunlu değil ama jeneriğe "Kenney.nl" yazacağız) |
| [Chakra Petch](https://fonts.google.com/specimen/Chakra+Petch) | Başlıklar, düğmeler | SIL Open Font License |
| [JetBrains Mono](https://fonts.google.com/specimen/JetBrains+Mono) | Kod | SIL Open Font License |

Neden karma: Her nesnede daha iyi olan seçildi. Kenney'nin binaları bizimkilerden iyi. Bizim robotumuzun maskot karakteri var; bizim kristalimiz tek bakışta "buz" olarak okunuyor (Kenney'nin mavi kayaları taş gibi duruyor).

**Kalite hedefi ve gerçekçi yol:** Sadece kodla çizim belirli bir seviyeye kadar gider. The Farmer Was Replaced düzeyinde bir cila için robot, binalar ve kaynaklarda profesyonel çizimler (sprite) gerekecek. Arazi dokusu, ışık, parıltı ve animasyonlar kodla kalır. Çizimlerin kaynağı ekip kararı (bkz. ILERLEME.md açık sorular).

## İlkeler
1. **Okunaklılık önce gelir.** Oyuncu kareleri sayabilmeli, robotun yönünü ve kaynakları tek bakışta görmeli. Süs, bunu bozmamalı.
2. **Sıcak Mars, soğuk teknoloji.** Zemin kiremit/pas tonlarında; insan yapımı her şey (robot, yapılar, arayüz) krem-beyaz gövde + turuncu şerit + camgöbeği ışık.
3. **Sevimli ama çocuksu değil.** Yuvarlak hatlar, yumuşak gölgeler; abartılı yüz ifadeleri yok. (Hedef kitle 14+.)
4. **Özgün.** Orijinal oyunun görselleri, drone + tarla ikilisi kullanılmaz (tasarım belgesi §9).

## Teknik kararlar
| Konu | Karar | Neden |
|---|---|---|
| Çizim yöntemi | Kodla çizim (React Native Skia) | Özgün, lisans derdi yok, küçük boyut, her ekranda keskin, kolay animasyon. İleride çizer/hazır paketle zenginleştirilebilir. |
| Bakış açısı | Tepeden, hafif derinlikli (3/4) | İzometrik daha şık olabilirdi ama "kuzey = yukarı" yeni başlayanlar için şart; izometrikte yönler çapraz görünür. |
| Işık | Sol üstten | Gölgeler sağ alta düşer; tüm çizimlerde aynı. |
| Derinlik | Satır satır çizim | Alttaki satırdaki nesne üsttekinin önünde görünür. |
| Izgara | İnce koyu derzler + kare başına hafif renk farkı | Kareler sayılabilir ama dama tahtası görünmez. |

## Renk paleti
Kaynak: `src/theme/colors.ts` (tek doğru kaynak).

| Rol | Renk |
|---|---|
| Uzay arka planı | `#0B0C1D` → `#1C1236` (gradyan) + yıldızlar |
| Mars toprağı | `#C2562F`, `#BA502C`, `#C75C33`, `#BE5530` (kare başına) · derz `#7E2F16` |
| Buz | `#EFFCFF` / `#BDEFFC` / kenar `#6FC6E0` + camgöbeği parıltı |
| Kaya | `#4B2F2B` / üst yüz `#6E4842` / maden benekleri `#F0A04B` |
| Gövde (robot, yapılar) | `#F4EEE6` → `#D6CCBF` |
| Vurgu (turuncu) | `#FF7A3D` — robot şeridi, Çalıştır düğmesi |
| Işık (camgöbeği) | `#39D5FF` — vizör, pencere ışıkları, ikincil düğme |
| Panel | `#141A2E` · kenar `#26304A` |

Kod renkleri: anahtar kelime turuncu `#FF9E64`, fonksiyon camgöbeği `#7DCFFF`, metin yeşil `#9ECE6A`, sayı sarı `#E0AF68`.

## Nesneler
- **Robot (gezgin):** Krem gövde, iki palet, turuncu şerit, yuvarlak baş, camgöbeği parlayan vizör, ucu turuncu yanan anten.
- **Buz:** Üç kristal parçası; sol yüz açık, sağ yüz koyu; etrafında camgöbeği parıltı.
- **Kaya:** Düzensiz bazalt yığını, üst yüzü açık; turuncu maden benekleri.
- **Depo:** Beyaz kargo modülü, turuncu uyarı şeritleri, koyu kapı.
- **Araştırma Laboratuvarı:** Altıgen modül, ışıklı pencere bandı, çanak anten.
- **Sera:** Cam kubbe, içinde yeşil bitkiler.
- **Zemin süsleri:** Çakıllar, ara sıra krater (yerleri kareye göre sabit).

## Ekran düzeni (oyun ekranı)
Yukarıdan aşağı: dünya/bölüm adı + kaynak sayaçları → harita → görev kutusu → kod paneli → ⏯ Adım adım / ▶ Çalıştır.

## Sıradaki görsel işler
- Robotun yöne göre dönmesi, yürüme ve toplama animasyonları (Reanimated).
- Buz parıltısı, anten ışığı gibi küçük canlılık animasyonları.
- Yazı tipleri (başlık + kod) ve simgeler.
- Bölüm seçimi, bölüm sonu, eğitim ekranlarının tasarımı.
