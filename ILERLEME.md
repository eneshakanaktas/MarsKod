# İlerleme Notu — MarsKod (geçici isim)

## Şu an nerede
- **Aşama 0 ve 1 bitti:** Expo iskeleti ve mini-Python motoru hazır. Motor 216 örnekte gerçek Python 3.12 ile birebir aynı sonucu veriyor (919 test). Türkçe hata açıklamaları da hazır.
- **Görsel yön takıldı:** 4 deneme yapıldı (2 boyut → GPU doku → Kenney çizimleri → 3 boyut). Ragıp hiçbirini The Farmer Was Replaced seviyesinde bulmadı. **Oyun motoru değişikliği (Unity/Godot) gündemde.**
- Ayrıntılı gün raporu: `docs/raporlar/2026-09-25-ragip-gun-sonu.md`. GitHub: https://github.com/eneshakanaktas/MarsKod (private).

## Sıradaki adım
- **(Ragıp, 09-26)** Godot taslakları bitti (godottaslak1-4, en sonuncusu godottaslak4). Sıradaki: aynı sahneyi **Unity** ile denemek (unitytaslak1), sonra ekip hangi motorla devam edeceğine karar verir.
1. **Ekip kararı: oyun motoru.** Seçenekler: Unity, Godot ya da Expo'da devam. Claude'un önerisi Godot (her şey metin dosyası, Claude doğrudan çalışabiliyor). Karşılaştırma raporda.
2. Karar verilince seçilen motorda **tek bir görsel deneme sahnesi** (ada + yürüyen robot + parlayan buz + efektler), telefonda denenir.
3. Beğenilirse Python motoru yeni motora taşınır ve aynı 216 örnekle doğrulanır; sonra plandaki Aşama 2 (Mars dünyası kuralları).

## Alınan kararlar
- 13 yaş ve altı hedef kitle değil; oyun yaş sormaz (Google Play Aileler politikası + KVKK veli izni). Tasarım belgesi §1.
- Anlatım tek ton (sıcak, sade); "Eğlenceli / Sade" seçeneği ileride ayarlara eklenecek.
- Ekran testleri Maestro ile yapılacak (Playwright web içindir).
- Önce Android.

## Açık sorular
- **Telefon paketleri (Ragıp, 09-26):** Arkadaşların kurabilmesi için godottaslak4 küçültülüp (106 MB → 57 MB; gereksiz sanal-telefon parçası ve hata ayıklama kısmı çıkarıldı, dokular telefon boyutuna indirildi) GitHub'a eklendi: `paketler/marskod-godottaslak4.apk` (kurulum `paketler/BENIOKU.md`). GitHub'a **yüklenmeyen**: `build/` klasöründeki büyük deneme paketleri (godottaslak3 ve 4'ün sanal telefon sürümleri, ~106 MB; GitHub 100 MB üstünü kabul etmez). Gerekirse Claude'a "telefon paketini üret" demek yeter.
- **Oyun motoru** (yukarıda, en önemli karar).
- **PC ve mobil (Ragıp):** PC sürümü de olsun mu? Claude'un görüşü: görseller aynı kalsın, sadece ekran düzeni değişsin. Önce mobil.
- **Boyut:** Ragıp oyunun az yer kaplamasını istiyor. Unity/Godot'da boyut 60–120 MB olur (Expo ile 30–45 MB).
- Oyunun kesin ismi (şimdilik "MarsKod").
- Supabase eklentisi: Aşama 8'de SkillSpector taraması + onayla kurulacak.
- ⏰ HATIRLAT: Oyuncu çözümlerini toplama fikri ertelendi (tasarım belgesi §4). İzin yöntemi + KVKK kararı verilmedi.

## Günlük
- **2026-09-25 — Enes:** Fikir netleşti (The Farmer Was Replaced mantığında, Mars temalı mobil Python oyunu), temel kararlar alındı, tasarım belgesi yazılıp onaylandı, GitHub deposu açıldı.
- **2026-09-25 — Ragıp:** Kurulum (Python 3.12, GitHub CLI, EAS, SkillSpector, MarsKod_Telefon emülatörü; açılmazsa `.android/avd/MarsKod_Telefon.avd` içindeki `.lock` dosyalarını sil). Yapım planı yazıldı. Aşama 0 ve 1 bitti (Python motoru, 919 test, Türkçe hata açıklamaları). Kararlar: yaş 13+, tek ton, Maestro. 4 görsel deneme yapıldı, hiçbiri beğenilmedi; motor değişikliği konuşulacak. Ayrıntı: `docs/raporlar/2026-09-25-ragip-gun-sonu.md`.
- **2026-09-26 — Ragıp:** Tasarımlar geri dönülebilir taslak olarak saklanıyor (motora göre adlandırılır: `expotaslak1-2`, `godottaslak1-3`; git etiketi + `docs/tasarim/taslaklar.md`). Godot 4.7 kuruldu; Godot ile iki görsel taslak yapıldı: godottaslak1 (yüzen ada) ve godottaslak2 (Mars yüzeyi, ışıklı kod bölgesi, yaşayan koloni, buzlu cam arayüz). godottaslak2 de beğenildi ama godottaslak1'e benzediği için arşivlendi. **godottaslak3 (gerçekçi):** fotoğraf dokulu zemin, NASA Perseverance gezgini, mavi Mars gün batımı, Phobos/Deimos, arkada düşen alevli meteorlar (parlama, şok dalgası, toz bulutu), hologram kod ızgarası, gerçekçi koloni. Açmak için `prototipler/godot/calistir.bat`. Açık konu: NASA modelinin ticari kullanım lisansı + telefonda ağırlığı (~12 MB). Ragıp beğenirse ekiple paylaşılacak; kabul görmezse başka motorla (Unity) denenecek. godottaslak3 telefon paketi (APK) yapıldı ve sanal telefonda çalıştı (`docs/tasarim/godottaslak3-telefon.png`). APK: `build/marskod-godottaslak3.apk` (106 MB, GitHub'a yüklenmez; deneme sürümü). Sanal telefonda ilk açılış ~75 sn sürdü; meteor dumanı telefonda beyaz noktalar gibi görünüyor, düzeltilecek. **godottaslak4 (Godot'daki son taslak):** godottaslak3'te buzlar fazla parlak ve görüntü öncekine benziyordu; baştan farklı bir sahne yapıldı: katmanlı kayalıklı Mars kanyonu, sinema görünümü (giriş kamera süzülmesi, film şeritleri, gren, yakınlaşma), toz hortumu, doğal buz. Girişte ve oyun içinde tek tuşla "Animasyon ve efektler" kapatma. Sanal telefonda denendi, düğmeler çalışıyor. **Sıradaki: aynı sahneyi Unity ile unitytaslak1 olarak denemek.**
- **2026-09-26 — Hamza:** Kendi bilgisayarında kurulum: proje paketleri (`npm install`) ve Python 3.12 (`py install 3.12`). 919 test geçti, tip denetimi temiz.
