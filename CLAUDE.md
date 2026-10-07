# MarsKod (geçici isim)

Telefonda oynanan, gerçek Python öğreten bulmaca oyunu (Mars kolonisi teması). Tasarım: `docs/superpowers/specs/2026-09-25-marskod-design.md`.
Teknik yol: **Unity 6** (6000.6.x, URP) + kendi mini-Python motorumuz (C#, `oyun/Assets/Motor/`). Önce Android (Play Store), sonra Steam. Görsel hedef: `docs/referanslar/BENIOKU.md`.
Yapım planı: `docs/superpowers/plans/2026-09-25-marskod-yapim-plani.md`.

## Klasörler
- `oyun/` — **oyunun asıl evi** (Unity projesi). Sahne tamamen kodla kurulur (`Assets/Scripts/Oyun.cs`); sahne dosyası boştur.
- `oyun/Assets/Motor/` — mini-Python motoru (C#). Giriş: `PythonEngine.RunPython`, adım adım: `Interpreter.Run`, Türkçe açıklama: `Explain`. Metinler: `ExplanationsTr.cs`.
- `oyun/Assets/Dunya/` — Mars dünyası kuralları (saf C#): ızgara, robot, buz, kaya, hedef, oyun komutları (`move`, `collect`...). Oyuncu kodunu çalıştırıp olanları kaydeden: `ProgramRun.Execute`. Bölüm dosyası okuma + denetleyici: `Level.cs`.
- `oyun/Assets/Resources/Bolumler/` — bölüm dosyaları (`bolum-NN.json`); kılavuz `docs/tasarim/bolum-dosyasi.md`. `dotnet test` her bölümü denetler.
- `oyun/Assets/Resources/Sozluk/sozluk.json` — kod sözlüğü metinleri; kılavuz `docs/tasarim/sozluk-dosyasi.md`. `dotnet test` bölümlerle uyumunu denetler.
- `motor-test/` — motor ve dünya testleri (.NET). Motoru Unity'nin sınırlarıyla derler (C# 9, .NET Standard 2.1).
- `prototipler/` — görsel taslaklar (unitytaslak1, godot…). Dokunma; kural: `docs/tasarim/taslaklar.md`.
- `arsiv/expo/` — eski Expo/TypeScript kodu (TypeScript motoru dahil); motor C#'a taşındı. Dokunulmaz, yeni özellik eklenmez; son hâli git etiketi `expo-son`. `tests/python-cases/` örnekleri dilden bağımsız; C# motoru bunlarla sınanır.
- `docs/arsiv/expo-kurallari.md` — eski Expo kuralları (yalnızca `arsiv/expo/`'ya dokunulursa).

## Çalıştırma (Unity)
- Bilgisayarda açmak: `oyun/calistir.bat` (paket yoksa önce üretir). Kod değişince `oyun/Build/` silinip yeniden üretilmeli.
- Komut satırından paket: `Unity.exe -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log` (Unity: `C:/Program Files/Unity/Hub/Editor/<sürüm>/Editor/`). Android: `OyunBuild.BuildAndroid` → `oyun/Build/Android/marskod.apk`. Derleme hatası `build.log` içinde `error CS` diye aranır.
- Görsel kontrol: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -shots <klasör>` ekran görüntülerini alıp kapanır (hepsi ~12 dk; `-bolum 31` eklenirse bölüm görüntüleri 31'den başlar).
- Unity içinde: Unity Hub → Add → `oyun/` → `Assets/Scenes/Oyun` → ▶. Menü: *MarsKod*.
- Bilgisayar klavyesi denetimi (Windows paketi üretildikten sonra): `powershell -ExecutionPolicy Bypass -File scripts/klavye-denetimi.ps1` → `KLAVYE DENETIMI: TAMAM`. Unity klavye harflerini Windows tuş mesajlarından okur; oyunun girdi sistemine sahte tuş vermek (`InputSystem.QueueTextEvent`) arayüze ulaşmaz, bu yüzden betik mesaj gönderir.
- Motor testleri: `motor-test` klasöründe `dotnet test` (Unity gerekmez, birkaç saniye). Unity'nin kendi ortamında: menü *MarsKod > Motor denetimi* ya da `-executeMethod MotorDenetimi.Calistir` (log'da `MOTOR DENETIMI:` satırı).
- Yeni Python örneği eklenirse `py -3.12 scripts/python_referans.py` beklenen sonucu üretir. Python sürümü değişirse `py -3.12 scripts/python_isimler.py` isim listelerini (`PythonNames.cs`) yeniler.
- Eski TypeScript testleri (referans): `arsiv/expo` klasöründe `npm install` + `npm test`.

## Unity kuralları
- Motor ve dünya kuralları saf C#'tır (UnityEngine kullanmaz); Unity açmadan `dotnet test` ile test edilir. Unity C# 9 kullanır: motorda daha yeni C# özellikleri ve .NET Standard 2.1'de olmayan kitaplıklar kullanılmaz (`motor-test` bunu derlerken yakalar).
- `Library/`, `Build/`, `Temp/`, `Logs/`, `UserSettings/` üretilen klasörlerdir; git'e girmez.
- Her dosyanın yanındaki `.meta` dosyası onunla birlikte taşınır/silinir (Unity bağlantıları onunla kurar).
- Boyut önemli (Ragıp): telefon paketini küçük tut; büyük doku/model eklemeden önce sor. Hazır model/doku eklerken ticari kullanım lisansını kontrol et.

## Genel kurallar
- Ekipte kodlama bilgisi yok; her zaman sade Türkçe açıkla, jargon kullanma.
- Token verimliliği önemli: iç işlerde kısa ol; kullanıcıya açıklamayı kısma.
- Yeni skill kurmadan önce SkillSpector ile tara; aynı işi yapan iki skill kurma.
- Fikir/karar değerlendirmede yeri geldiğinde: önce karşı çık, varsayımları sorgula, büyük resmi gör, görülmeyen fırsatları bul, tarafsız dış göz gibi bak. Gerçekçi ol.

## Kod kalitesi (kritik — Ragıp, herkes için)
Yazılan her kod SOLID ve temiz olmalı; yeni kod yazarken ve var olanı değiştirirken buna uy:
- Her sınıf/fonksiyon tek bir iş yapar; iş büyüyünce parçalara bölünür.
- Tekrar yok: aynı mantık iki yerde yazılmaz, ortak yere alınır.
- Yeni özellik eklemek var olan kodu bozmadan yapılabilmeli (genişletilebilir yapı).
- Parçalar birbirine gevşek bağlı: dünya kuralları arayüze, arayüz dünya kurallarının içine bağımlı olmaz.
- Okunurluk kısalıktan önce gelir: anlamlı adlar, sıkıştırılmış "zekice" kod yok.
- Ölçülü ol: SOLID uğruna gereksiz katman/soyutlama ekleme; küçük iş için sade çözüm yeterli.

## Model ve efor (kritik — Enes, herkes için)
Her işin başında önerilen "model · efor" ikilisini tek satırla ve gerekçesiyle söyle; iş türü değişince yeniden öner (gerekirse yeni sohbet).
- **Sonnet 5 · medium** (zorlaşırsa high): planı yazılmış, net tarifli kod işleri (plan görevleri, saf C# + testler). Opus'un yarı fiyatı → kota iki kat dayanır.
- **Opus 5.5 · medium/high**: planlama, tasarım/mimari kararı, plan yazma, zor hata (Unity görüntü sorunları gibi).
- **Fable 5.1**: yalnızca Opus'un takıldığı en zor işler (Opus'un ~2,5 katı pahalı).
- Yardımcı ajanlar: arama/okuma → Haiku 4.5; kod yazan → Sonnet 5. Aynı sohbette sık model değiştirme.

## İlerleme notu (kritik)
Kökteki `ILERLEME.md`'yi oturum başında oku; her önemli adımdan sonra ve oturum sonunda güncelle (ne yapıldı, nerede kalındı, sıradaki adım, açık sorular). Kısa tut, eski girdileri özetle.

## Ekip döngüsü (projede birden fazla kişi çalışır: kullanıcı, Hamza, Ragıp)
1. Açılış: Kişi kendini tanıtır ("Merhaba, ben Ragıp"). Tanıtmazsa ilk iş adını sor.
2. "GitHub'dan güncel hâli çekiyorum" de → `git pull`.
3. `ILERLEME.md`'yi oku → "En son [kim], [ne zaman], [ne yaptı]; sıradaki iş; açık sorular" özetini 2-4 satırda ver.
4. Çalışırken ILERLEME.md'yi yerelde güncel tut (girdilere kişinin adını yaz). Push etme.
5. Kapanış: Kişi "bugünlük bitti" (veya benzeri) deyince → ILERLEME.md'ye günün özetini adıyla yaz; üstteki "En son (özet)" bölümünü güncelle, başlıklardaki "commit edilmedi" etiketlerini "GitHub'a yüklendi (commit no)" yap → commit + push → "GitHub'a yüklendi" diye onayla. Push hata verirse (başkası önce yüklediyse) pull + birleştir, sonra tekrar push; çözemezsen kullanıcıya açıkla.
Kişi "bitti" demeden oturumu bırakacak gibiyse (vedalaşma vb.) push'u hatırlat.
