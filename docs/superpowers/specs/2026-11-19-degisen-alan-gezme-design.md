# Değişen alanda düzenleri gezme (A1) — kısa tasarım

Enes, 2026-11-19. Açık iş A1 (`ILERLEME.md`).

## Neden
Değişen alanlı bölümlerde (`duzenler`: 15, 20, 91, 92, 93, 94, 96, 100) oyuncu bugün yalnız 1. düzeni görüyor; öteki düzeni ancak kodu takılınca, ani bir sahne değişimiyle görüyor. Enes 2026-11-16: "öteki düzenler önceden görünsün". Oyuncu kodun her düzende çalışması gerektiğini görerek anlamalı; değişim de hikâyeye uygun görünmeli (Bölge 2 tipi, Bölge 10 meteor).

## Seçilen yaklaşım: A — gezme göstergesi (Enes seçti)
- Başlığın altında (buz noktalarının altında) `◀  ALAN 1/2  ▶`. Yalnız `duzenler` olan bölümlerde görünür.
- Oklar: kod çalışmıyorken sahneyi öteki düzene geçirir (bitmiş koşu varsa önce sahne sıfırlanır). Çalışırken oklar sönük ve basılmaz; gösterge oynatılan düzeni yazar.
- "Çalıştır"da oynatılacak düzen sahnedekinden farklıysa aynı geçiş önce oynar, sonra robot yürür (bugünkü ani değişim yerine).
- Geçiş (~1 sn): **Bölge 2** — tipi: beyaz kar topakları alanı bir yandan öbür yana süpürür; eski buz/kristal/engel küçülüp kaybolur, yeniler büyüyerek çıkar. **Bölge 10** — eski kraterler söner/küçülür; yeni her krater için gökten akkor bir göktaşı düşer, değdiği yerde krater açılır.
- Elenenler: B (kendiliğinden dönen sahne: oyuncu düşünürken değişir), C (küçük harita kartları: telefonda küçük, 3B görünüşe uymaz).

## Kapsam dışı
Bölge 7 izleri (`izler`, `scout()`) ve 76/80 notları (`notlar`): yolu keşfetmek bulmacanın kendisi; önceden gösterilmez.

## Kod yerleşimi (Claude'un kararı)
- `LayoutSwitcher` (yeni arayüz parçası): gösterge ve oklar; Hud yalnız ekler ve olayı iletir.
- `LayoutGust` (yeni sahne parçası): iki geçiş hareketi; dünya kurallarına dokunmaz.
- `Oyun`: oklara basılınca / koşu başında geçişi çağırır, `BuildLayout` + `MeteorMarks.Show`.
- Görsel denetim: `-duzenler <klasör>` her değişen alanlı bölümde geçiş karelerini çeker.

## Görsel tutarlılık
Yeni nesneler yere oturur (`GroundPlacement`), kar topakları zeminden taşmaz, göktaşı gölgesi/ışığı bölgenin gece ışığıyla tutarlı; geçiş sonunda sahne bugünkü kurulumla birebir aynı.

## Sıra
Önce Bölüm 15'te taslak + ekran görüntüleri → Enes onayı → Bölüm 20 ve Bölge 10 → paket, `-izle` ile değişen bölümler.
