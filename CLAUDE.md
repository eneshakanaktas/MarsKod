# MarsKod (geçici isim)

Telefonda oynanan, gerçek Python öğreten bulmaca oyunu (Mars kolonisi teması). Tasarım: `docs/superpowers/specs/2026-09-25-marskod-design.md`.
Teknik yol: **Unity 6** (6000.6.x, URP) + kendi mini-Python motorumuz (C#, `oyun/Assets/Motor/`). Önce Android (Play Store), sonra Steam. Görsel hedef: `docs/referanslar/BENIOKU.md`.
Yapım planı: `docs/superpowers/plans/2026-09-25-marskod-yapim-plani.md`.

## Klasörler
- `oyun/` — **oyunun asıl evi** (Unity projesi). Sahne tamamen kodla kurulur (`Assets/Scripts/Oyun.cs`); sahne dosyası boştur.
- `oyun/Assets/Motor/` — mini-Python motoru (C#). Giriş: `PythonEngine.RunPython`, adım adım: `Interpreter.Run`, Türkçe açıklama: `Explain`. Metinler: `ExplanationsTr.cs`.
- `oyun/Assets/Dunya/` — Mars dünyası kuralları (saf C#): ızgara, robot, buz, oyun komutları (`move`, `collect`...). Oyuncu kodunu çalıştırıp olanları kaydeden: `ProgramRun.Execute`.
- `motor-test/` — motor ve dünya testleri (.NET). Motoru Unity'nin sınırlarıyla derler (C# 9, .NET Standard 2.1).
- `prototipler/` — görsel taslaklar (unitytaslak1, godot…). Dokunma; kural: `docs/tasarim/taslaklar.md`.
- `arsiv/expo/` — eski Expo/TypeScript kodu (TypeScript motoru dahil); motor C#'a taşındı. Dokunulmaz, yeni özellik eklenmez; son hâli git etiketi `expo-son`. `tests/python-cases/` örnekleri dilden bağımsız; C# motoru bunlarla sınanır.
- `docs/arsiv/expo-kurallari.md` — eski Expo kuralları (yalnızca `arsiv/expo/`'ya dokunulursa).

## Çalıştırma (Unity)
- Bilgisayarda açmak: `oyun/calistir.bat` (paket yoksa önce üretir). Kod değişince `oyun/Build/` silinip yeniden üretilmeli.
- Komut satırından paket: `Unity.exe -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log` (Unity: `C:/Program Files/Unity/Hub/Editor/<sürüm>/Editor/`). Android: `OyunBuild.BuildAndroid` → `oyun/Build/Android/marskod.apk`. Derleme hatası `build.log` içinde `error CS` diye aranır.
- Görsel kontrol: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -shots <klasör>` ekran görüntülerini alıp kapanır.
- Unity içinde: Unity Hub → Add → `oyun/` → `Assets/Scenes/Oyun` → ▶. Menü: *MarsKod*.
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

## İlerleme notu (kritik)
Kökteki `ILERLEME.md`'yi oturum başında oku; her önemli adımdan sonra ve oturum sonunda güncelle (ne yapıldı, nerede kalındı, sıradaki adım, açık sorular). Kısa tut, eski girdileri özetle.

## Ekip döngüsü (projede birden fazla kişi çalışır: kullanıcı, Hamza, Ragıp)
1. Açılış: Kişi kendini tanıtır ("Merhaba, ben Ragıp"). Tanıtmazsa ilk iş adını sor.
2. "GitHub'dan güncel hâli çekiyorum" de → `git pull`.
3. `ILERLEME.md`'yi oku → "En son [kim], [ne zaman], [ne yaptı]; sıradaki iş; açık sorular" özetini 2-4 satırda ver.
4. Çalışırken ILERLEME.md'yi yerelde güncel tut (girdilere kişinin adını yaz). Push etme.
5. Kapanış: Kişi "bugünlük bitti" (veya benzeri) deyince → ILERLEME.md'ye günün özetini adıyla yaz → commit + push → "GitHub'a yüklendi" diye onayla. Push hata verirse (başkası önce yüklediyse) pull + birleştir, sonra tekrar push; çözemezsen kullanıcıya açıkla.
Kişi "bitti" demeden oturumu bırakacak gibiyse (vedalaşma vb.) push'u hatırlat.
