# Görsel tur planı (metin denetimi bittikten sonra)

> Enes, 2026-11-18. Sezon 1 metin denetimi (Bölüm 1-100) bitince başlar. Bu belge o zamana kadar bekler; kurulum ve indirme **görsel turda** yapılır, şimdi değil.
> Model: **Opus 5.5 · high**, yeni sohbet (görsel/3B iş; bkz. CLAUDE.md "Model ve efor").

## Amaç
Oyunun "programcı işi" görünümünden kurtulması. Şu an oyunda hiç 3B model yok (`oyun/Assets` 4 MB), her şey koddan basit şekillerle kuruluyor; kodda post-processing (ışık parlaması, renk tonu) yok. Görsel hedef: `docs/referanslar/BENIOKU.md` (tombul, pürüzsüz, oyuncak gibi, ince dış çizgi, low-poly, sade ama premium).

## Boyut kararı (Enes)
Boyut artık ana sorun değil: kaliteli olacaksa telefon paketi 300-500 MB olabilir. **Ragıp'a haber verilmeli** (CLAUDE.md'deki "telefon paketini küçük tut" kuralı Ragıp'ın; kural birlikte güncellenir). Lisans kontrolü kuralı aynen geçerli.

## Seçilenler
| Araç / paket | Rol | Durum |
|---|---|---|
| `unity:craft-game-floors` | **Ana yol:** 3 satırlık sanat rehberi → sahne ışığı → dış çizgili (toon) görünüm → **6 eksenli görsel puan kartı** (önce/sonra) | Kurulu (Unity eklentisi) |
| `unity:urp-postprocessing` | Işık/renk ayarlarının teknik kurulumu (craft ne isteneceğini söyler) | Kurulu |
| `disciplines:create-game-assets` | Yalnız varlık listesi + lisans/kaynak kaydı (sanat rehberini craft yazar; çakışmasın) | Kurulu |
| `disciplines:game-feel` | Mekaniklerin hissi (dokunma tepkisi, yumuşak hareket), ortamdan sonra | Kurulu |
| **KayKit: Space Base Bits** (Kay Lousberg) | **Ana model paketi.** CC0; tombul oyuncak stili hedefe en yakın; zemin, kaya, iniş aracı/pisti, güneş paneli, matkap, sandık. İsteğe bağlı ücretli ek paketi (pay-what-you-want) Enes'e sorulur | İndirilecek (Enes onayı: dosya adı + boyut söylenerek) |
| **Kenney: Space Kit** | Yalnız KayKit'te olmayan parçalar için dolgu; KayKit paletine boyanır | Gerekirse |

**Elenenler:** Synty POLYGON Sci-Fi Space (boyut değil **stil** yüzünden: sivri, ayrıntılı, çok oyunda kullanılmış; oyuncak hissi yok). Quaternius Ultimate Space Kit (lisans kaynaklarda çelişkili; üstelik üçüncü bir stil yamalı görünüm riski).

**Kurallar:** Paketleri karıştırma (tek ana paket, dolgu ana pakete uydurulur). Kıvılcım, Ece ve öteki karakterler hikâyeye özel; ilk aşamada yalnız ortam (zemin, kaya, koloni, üs) değişir. Her modelin kaynağı ve lisansı `create-game-assets` varlık listesine yazılır.

## Sıra
1. **Sahiplen:** ILERLEME.md'nin en üstüne "Enes [tarih]: Görsel tur / Bölge 1 pilotu" → hemen commit + push.
2. **Bölge 1 pilotu** (oyuncunun ilk gördüğü yer; Bölge 10 da aynı arka planı kullanıyor):
   1. `BENIOKU.md`'den 3 satırlık sanat rehberi → Enes onaylar.
   2. Şimdiki görünümün ekran görüntüleri + puan kartı (önce).
   3. Işık + post-processing + dış çizgi.
   4. KayKit'ten zemin/kaya/koloni parçaları.
   5. Yeniden puan (sonra), önce/sonra yan yana → Enes karar verir: öteki bölgelere yay / yön değiştir.
3. **Öteki bölgeler** pilot kararına göre.
4. **Görsel hata turu** — yeni görünüm oturduktan **sonra** (önce yapılırsa eski şekiller değişince emek boşa gider):
   - Bölüm 70: Kıvılcım'ın duvara çizdiği el yandan bakınca havada. Sebep: `OwnHandPrint` / `CrayonMarks.Lift = -0.085` çizgiyi pürüzlü duvarın 8,5 cm önündeki düz bir düzleme çiziyor. Çözüm: yüzeye oturt (URP Decal ya da her noktayı duvar yüzeyine it + 1-2 mm).
   - Bölüm 76: kamera genişleyince siyah duvarların dışı görünüyor.
   - Bölüm 80: kapanışta koridor lambası "+30 XP" yazısına biniyor.
   - Genel: oyunda bunun gibi çok sorun var (Enes). Her bölüm ekran görüntüsüyle gerçekten bakılacak.
   - **Kalıcı önlem:** (a) yerleşim kuralı skill'i ("yüzeye yapışacak şeyi sabit sayıyla öne alma, yüzeye oturt"; "her yerleşimi önden, iki yandan, üstten kontrol et"); (b) sahne denetimine "yüzeye yapışık mı" ölçümü (her nokta duvara > 1 cm ise HATA).

## Görsel turdan bağımsız, bekleyen araç önerileri (2026-11-18)
İstenince kurulur (Sonnet 5.5 · medium), önce SkillSpector:
- **codebase-memory** (kod haritası; Serena değil — 213 .cs dosyası, toplu ad değiştirme nadir, Serena'nın hafızası claude-mem ile çakışır). Yalnız Claude Code'a elle bağlanır, otomatik kurucu kullanılmaz, ADR kapalı, CLAUDE.md'ye "kod yapısı sorularında önce bunu kullan".
- Skill `bolum-denetimi` (5'erli denetim yöntemi), skill `kapanis` (ILERLEME → tek commit → push; yalnız istenince çalışır).
- Kanca: tur sonunda motor/dünya/bölüm/sözlük değiştiyse `dotnet test` (~40 sn); kanca: `arsiv/expo/` ve `prototipler/` düzenlemesini engelle.
- Yeni ajan önerilmedi (2026-11-16 dersi).
