# İlerleme Notu — MarsKod (geçici isim)

## Şu an nerede
Tasarım onaylandı, yapım planı yazıldı (`docs/superpowers/plans/2026-09-25-marskod-yapim-plani.md`). **Aşama 0 (proje iskeleti) bitti:** Expo projesi depoda, Skia ile Mars ızgarası sanal telefonda çiziliyor, testler çalışıyor. GitHub: https://github.com/eneshakanaktas/MarsKod (private).

## Sıradaki adım
1. **Aşama 1 — Mini-Python motoru:** karşılaştırma düzeneği hazır (43 örnek, hepsi "atlandı" durumda bekliyor). **Aşama 1 (mini-Python motoru) tamamen bitti.** Sıradaki: Aşama 2, Mars dünyası (ızgara, robot, kaynaklar) ve bölüm dosyası biçimi. Ekip isterse `src/engine/explanations-tr.ts` içindeki Türkçe hata metinlerini okuyup dilini düzeltebilir; dosyanın başında düzenleme rehberi var.
2. Diğer ekip üyeleri: `git pull` → `npm install` → `npx expo start` ile kendi telefonlarında (Expo Go) açabildiklerini denesin.
3. Görsel tasarım adımı (Aşama 4) — ekibin referans görselleri o zaman gönderilecek.

**Görsel yön (ilk taslak, ekip onayı bekliyor):** `docs/tasarim/gorsel-yon.md` + ekran görüntüsü. Kodla çizim (Skia), tepeden 3/4 bakış, sıcak Mars toprağı + soğuk teknoloji ışıkları; robot, buz, kaya, depo, laboratuvar, sera çizildi. Oyun ekranının örnek düzeni (sayaçlar, görev, kod paneli, Çalıştır) telefonda çalışıyor.

## Alınan kararlar (bu hafta)
- 13 yaş ve altı hedef kitle değil; oyun yaş sormaz (Google Play Aileler politikası + KVKK veli izni yükünden kaçınmak için). Ayrıntı: tasarım belgesi §1.
- Anlatım tek ton (sıcak, sade). "Eğlenceli / Sade" seçeneği ileride ayarlara eklenecek (plan, Aşama 7).

## Açık sorular
- **Çizimler:** Ragıp ücretsiz paketle devam kararı verdi. Kenney Sci-Fi RTS (CC0) seçildi ve karma kullanılıyor (ayrıntı: `docs/tasarim/gorsel-yon.md`). Güncel görünüm: `docs/tasarim/ucuncu-taslak-oyun-ekrani.png`. Ekip görüşü bekleniyor. İleride bütçe olursa robot/maskot için çizer düşünülebilir.
- Oyunun kesin ismi (şimdilik "MarsKod").
- Supabase eklentisi: Aşama 8'de SkillSpector taraması + onayla kurulacak. (Expo eklentisi tarandı ve kuruldu.)
- ✅ Ekran testi: **Maestro kullanılacak** (Ragıp onayladı). Tasarımda Playwright yazıyordu ama o web içindir.
- Görsel: Şu anki dama görünümü geçici. Gerçek görünüm Aşama 4'te ekibin referans görselleriyle belirlenecek.
- ⏰ HATIRLAT: Oyuncu çözümlerini toplama fikri ertelendi (tasarım belgesi §4). İzin yöntemi + KVKK kararı verilmedi; ilk sürüm bitince veya bulut/üyelik işine gelince gündeme getir.

## Günlük
- **2026-09-25 — Enes:** Fikir netleşti (The Farmer Was Replaced mantığında, Mars temalı mobil Python oyunu), tüm temel kararlar alındı, tasarım belgesi yazılıp onaylandı, GitHub deposu açıldı. Oyuncu çözümlerini toplama fikri ertelendi.
- **2026-09-25 — Ragıp:**
  - Kurulum: Python 3.12, GitHub CLI, EAS CLI, SkillSpector; Expo eklentisi tarandı ve kuruldu. MarsKod'a özel "MarsKod_Telefon" emülatörü açıldı (açılmazsa `.android/avd/MarsKod_Telefon.avd` içindeki `.lock` dosyalarını sil).
  - Yapım planı yazıldı (önce 3 bölümlük oynanabilir ince dilim). Aşama 0 (Expo 57 + TypeScript + Skia + Jest) bitti.
  - **Aşama 1 bitti, mini-Python motoru:** kelime ayırıcı → cümle çözücü → derleyici + tek döngülü çalıştırıcı. 216 Python örneğinde çıktı, hata mesajı ve satır gerçek Python 3.12 ile birebir aynı; toplam 919 test. Adım adım modu, bitmeyen döngü sınırı, "bu oyunda henüz yok" bildirimi, oyun komutlarını dışarıdan bağlama ve Türkçe hata açıklamaları (15 hata türü) hazır.
  - Kararlar: Maestro (ekran testi), 13 yaş altı hedef değil, tek sade ton (+ ileride Eğlenceli/Sade seçeneği).
  - Görsel yön: 3 taslak yapıldı (kodla çizim → GPU doku + diorama → Kenney CC0 çizimleri + animasyon + yazı tipleri). Ragıp sonucu hâlâ yetersiz buluyor; hedef The Farmer Was Replaced düzeyi (o oyun Unity ile yapılmış, 3 boyutlu). Yön kararı açık.
  - Ara yükleme: GitHub'a yüklendi.
