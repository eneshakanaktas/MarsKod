# İlerleme Notu — MarsKod (geçici isim)

## Şu an nerede
- **Aşama 0 ve 1 bitti:** Expo iskeleti ve mini-Python motoru hazır. Motor 216 örnekte gerçek Python 3.12 ile birebir aynı sonucu veriyor (919 test). Türkçe hata açıklamaları da hazır.
- **Görsel yön takıldı:** 4 deneme yapıldı (2 boyut → GPU doku → Kenney çizimleri → 3 boyut). Ragıp hiçbirini The Farmer Was Replaced seviyesinde bulmadı. **Oyun motoru değişikliği (Unity/Godot) gündemde.**
- Ayrıntılı gün raporu: `docs/raporlar/2026-09-25-ragip-gun-sonu.md`. GitHub: https://github.com/eneshakanaktas/MarsKod (private).

## Sıradaki adım
1. **Ekip kararı: oyun motoru.** Seçenekler: Unity, Godot ya da Expo'da devam. Claude'un önerisi Godot (her şey metin dosyası, Claude doğrudan çalışabiliyor). Karşılaştırma raporda.
2. Karar verilince seçilen motorda **tek bir görsel deneme sahnesi** (ada + yürüyen robot + parlayan buz + efektler), telefonda denenir.
3. Beğenilirse Python motoru yeni motora taşınır ve aynı 216 örnekle doğrulanır; sonra plandaki Aşama 2 (Mars dünyası kuralları).

## Alınan kararlar
- 13 yaş ve altı hedef kitle değil; oyun yaş sormaz (Google Play Aileler politikası + KVKK veli izni). Tasarım belgesi §1.
- Anlatım tek ton (sıcak, sade); "Eğlenceli / Sade" seçeneği ileride ayarlara eklenecek.
- Ekran testleri Maestro ile yapılacak (Playwright web içindir).
- Önce Android.

## Açık sorular
- **Oyun motoru** (yukarıda, en önemli karar).
- **PC ve mobil (Ragıp):** PC sürümü de olsun mu? Claude'un görüşü: görseller aynı kalsın, sadece ekran düzeni değişsin. Önce mobil.
- **Boyut:** Ragıp oyunun az yer kaplamasını istiyor. Unity/Godot'da boyut 60–120 MB olur (Expo ile 30–45 MB).
- Oyunun kesin ismi (şimdilik "MarsKod").
- Supabase eklentisi: Aşama 8'de SkillSpector taraması + onayla kurulacak.
- ⏰ HATIRLAT: Oyuncu çözümlerini toplama fikri ertelendi (tasarım belgesi §4). İzin yöntemi + KVKK kararı verilmedi.

## Günlük
- **2026-09-25 — Enes:** Fikir netleşti (The Farmer Was Replaced mantığında, Mars temalı mobil Python oyunu), temel kararlar alındı, tasarım belgesi yazılıp onaylandı, GitHub deposu açıldı.
- **2026-09-25 — Ragıp:** Kurulum (Python 3.12, GitHub CLI, EAS, SkillSpector, MarsKod_Telefon emülatörü; açılmazsa `.android/avd/MarsKod_Telefon.avd` içindeki `.lock` dosyalarını sil). Yapım planı yazıldı. Aşama 0 ve 1 bitti (Python motoru, 919 test, Türkçe hata açıklamaları). Kararlar: yaş 13+, tek ton, Maestro. 4 görsel deneme yapıldı, hiçbiri beğenilmedi; motor değişikliği konuşulacak. Ayrıntı: `docs/raporlar/2026-09-25-ragip-gun-sonu.md`.
