# MarsKod (geçici isim)

**Telefonda oynanan, gerçek Python öğreten bulmaca oyunu.** Oyuncu, Mars'taki küçük bir robota (Kıvılcım) kod yazarak görev verir: robot yürür, buz toplar, engelleri aşar. Oyuncu Python öğrenir.


---

## 1. Oyunu hemen denemek (Unity kurmadan)

Oyunu denemek için Unity ya da başka bir program kurmak gerekmez.

**Windows bilgisayar (en kolayı):**
1. [Releases sayfasından](https://github.com/eneshakanaktas/MarsKod/releases/latest) **MarsKod-Windows.zip** dosyasını indirin (~40 MB).
2. Zip'e sağ tıklayıp **"Tümünü ayıkla"** deyin. Zip'in içinden doğrudan çalıştırmayın.
3. Açılan klasörde **MarsKod-Baslat.bat** dosyasına çift tıklayın. Oyun telefon boyutunda bir pencerede açılır.

Windows "Bilinmeyen yayıncı" uyarısı verirse **Daha fazla bilgi → Yine de çalıştır** deyin. Uyarı oyunun bozuk olduğundan değil, henüz imzalanmamış olmasından çıkar.

Gereken: Windows 10/11 (64 bit). Ayrı bir kurulum yoktur, klasörü silince oyun kalkar.

**Android telefon:** `paketler/` klasöründeki `.apk` dosyası telefona kurulur ([kurulum adımları](paketler/BENIOKU.md)). Oradaki paket eskidir (2 Ekim, 15 bölüm). Güncel Android paketi, Windows sürümü gibi Releases sayfasına konulacaktır.

**Oyunu geliştirmek isteyenler** için: `oyun/` klasörünü Unity Hub ile açmak gerekir (aşağıda "Geliştiriciler için").

---

## 2. Oyun ne anlatıyor?

- **Amaç:** Hiç kod yazmamış biri, dışarıdan yardım almadan oyunu bitirirken gerçek Python yazmayı öğrensin.
- **Nasıl oynanır:** Ekranda Mars'ın bir bölgesi var. Alt tarafta kod yazılan kart var. Oyuncu "▶ Çalıştır"a basar, robot yazılan kodu aynen yapar. Doğru yazarsa görev biter, yanlışsa robot takılır ve oyun hatayı sade Türkçe ile açıklar.
- **Öğretme yöntemi:**
  - Başta komutlar düğmelerle seçilir. Oyuncu ilerledikçe kodu kendisi yazmaya geçer.
  - ⏯ "Adım adım" modunda kod satır satır yavaş çalışır. Oyuncu hangi satırın ne yaptığını görür.
  - Hatalar hem Türkçe açıklanır hemde gerçek Python'un İngilizce mesajı gösterilir. Böylece oyuncu gerçek İngilizce hatalara da alışır.
  - Oyun içi bir **kod sözlüğü** her komutu örnekle anlatır.
  - 3 kademeli ipucu vardır.
- **Hikâye:** Oyuncu Dünya'daki bir kod okulunun öğrencisi. Sıradan bir ödev yaptığını sanır, ama kodlar Mars'taki bir koloninin tek uyanık robotuna ulaşmaktadır. Robot fırtınada belleğini yitirmiştir ve her şeyi yeniden öğrenmesi gerekir. Ana fikir: *öğrenmek, bir başkasına güvenmektir.* İnsanlar ölmez, gizem vardır ama korku yoktur; hikâye çoğunlukla haritadaki izlerle anlatılır.
- **Hedef kitle:** Yazılımla tanışmamış ya da yeni başlayan gençler ve yetişkinler. **13 yaş altı hedef değildir** (çocuk uygulamalarının mağaza ve veri koruma kuralları farklı olduğu için).
- **Para:** İlk aşamada reklam ya da satın alma yoktur. Önce oyunun sevilmesi hedeflenir.

## 3. Şu anki durum

| Konu | Durum |
|---|---|
| Bölüm sayısı | **60 bölüm hazır**, 6 Mars bölgesi (iniş ovası, kutup buzulu, kraterli düzlük, kum tepeleri, anten tepesi, kanyon girişi) |
| Hedef | İlk sezon 200 bölüm (20 bölge); ilerde daha çok sezon |
| Bölüm sonu sahneleri | 50. bölümde büyük final sahnesi, 60. bölümde kanyona iniş sahnesi |
| Platform | Önce **Android (Play Store)**, sonra **Steam (Windows)** |
| Dil | Türkçe arayüz (İngilizce sonraki sürümlerde) |
| Yayın | Henüz mağazada değil, deneme aşaması |
| Sıradaki işler | Bölge 6 sahneleri (Serçe'nin ilk uçuşu, roketin inişi), parlayan malzemelerin telefonda doğru görünmesi, telefonda gerçek denemeler |

Ayrıntılı günlük ilerleme: [ILERLEME.md](ILERLEME.md).

## 4. Nasıl geliştirildi?

- **Ekip:** Enes  Ragıp ve Hamza. Ekipte kodlama bilgisi az düzeyde; bu yüzden iş, **yapay zekâ kod asistanı (Claude Code)** ile birlikte yürütülür. Kodun büyük bölümünü yapay zekâ yazar, ekip kararları verir ve sonucu oyunda sınar.
- **Çalışma düzeni:** Her çalışma günü sonunda yapılanlar `ILERLEME.md` dosyasına yazılır ve GitHub'a yüklenir. Böylece kim başlarsa nerede kalındığını görür.
- **Kalite için alınan önlemler:**
  - Oyuncu kodunu okuyan "mini Python" motoru için **1400'den fazla otomatik test** vardır. Test örnekleri gerçek Python ile karşılaştırılarak hazırlanır, yani oyun "Python gibi" değil, Python'un verdiği sonucu verir.
  - Her bölüm dosyası otomatik denetlenir: çözümü var mı, ilgili sözlük metinleri eksik mi.
  - Oyunun görünümü, kayıtlı ekran görüntüleri ve otomatik denetimlerle kontrol edilir (`docs/tasarim/` içinde tarihli klasörler halinde).
  - Kod yazım kuralları: her parça tek bir iş yapsın, tekrar olmasın, yeni özellik eskiyi bozmasın (kurallar [CLAUDE.md](CLAUDE.md) içinde).
- **Teknoloji seçimi:** Önce telefon uygulaması yapım aracıyla (Expo) başlandı, sonra Godot ve Unity ile görsel taslaklar denendi. Görünüm ve performans için **Unity 6** seçildi. İlk deneme kodu `arsiv/` klasöründe saklanıyor, artık kullanılmıyor.
- **Görsel yön:** Koyu, sade bir görünüm. Mars yüzeyi, ışık ve gölge gerçekçi olsun diye özenle ayarlanır. Oyunun görüntüleri büyük oranda kodla çizilir, bu yüzden telefon paketi küçük kalır (hedef: Android paketi ~50-75 MB).

## 5. Klasörler (kısaca)

| Klasör | İçinde ne var |
|---|---|
| `oyun/` | Oyunun kendisi (Unity projesi) |
| `oyun/Assets/Motor/` | Oyuncunun yazdığı Python kodunu okuyup çalıştıran parça |
| `oyun/Assets/Dunya/` | Mars dünyasının kuralları: robot, buz, kaya, hedef |
| `oyun/Assets/Resources/Bolumler/` | 60 bölümün dosyaları |
| `motor-test/` | Otomatik testler |
| `docs/` | Tasarım belgeleri, hikâye, bölge senaryoları, ekran görüntüleri |
| `paketler/` | Telefona kurulan eski deneme paketleri |
| `prototipler/`, `arsiv/` | Eski denemeler (dokunulmaz) |

Önemli belgeler:
- Tasarım: [docs/superpowers/specs/2026-09-25-marskod-design.md](docs/superpowers/specs/2026-09-25-marskod-design.md)
- Hikâye özeti: [docs/tasarim/hikaye.md](docs/tasarim/hikaye.md)
- Bölge bölge senaryo: `docs/tasarim/senaryo-bolge-01.md` … `06.md`
- Yapım planı: [docs/superpowers/plans/2026-09-25-marskod-yapim-plani.md](docs/superpowers/plans/2026-09-25-marskod-yapim-plani.md)

## 6. Şirketin bilmesi gereken noktalar

- **Lisans:** Projede henüz bir lisans dosyası yoktur; lisans henüz belirlenmemiştir; kullanım izni için ekiple görüşülmelidir.
- **Üçüncü taraf içerik:** Yazı tipleri Poppins ve JetBrains Mono (açık lisanslı, ticari kullanıma uygun). Hazır model ya da doku eklenirken ticari kullanım lisansı kontrol edilir.
- **Kişisel veri:** Oyun şu an kullanıcıdan hesap, yaş ya da veri istemez; ilerleme telefonda saklanır. Oyuncu çözümlerini toplama fikri ertelenmiştir; yapılırsa önce izin ve gizlilik metni kararı gerekir.
- **Bilinen eksikler:** Oyun denemede. Bazı malzemeler telefonda olması gerektiği kadar parlamıyor, gerçek telefonda uzun süreli denemeler devam ediyor, mağaza adı ve paket kimliği (`com.marskod.oyun`) kesinleşmedi.
- **Sürdürülebilirlik:** Bir bölüm, tek bir metin dosyasıyla tanımlanır ([kılavuz](docs/tasarim/bolum-dosyasi.md)); sade bir bölüm eklemek için kod yazmak gerekmez (özel sahneler hariç).

---

## Geliştiriciler için (teknik)

- **Yapı:** Unity 6 (6000.6.x, URP). Python yorumlayıcısı ve dünya kuralları saf C#'tır (Unity'ye bağlı değil), Unity'siz `dotnet test` ile test edilir.
- **Unity'de açmak:** Unity Hub → Add → `oyun/` → `Assets/Scenes/Oyun` → ▶. Ya da `oyun/calistir.bat` (Unity 6000.6 kuruluysa paketi üretip açar).
- **Testler:** `motor-test` klasöründe `dotnet test`.
- **Windows dağıtım paketi:** `oyun/calistir.bat` ile paket üretildikten sonra `powershell -ExecutionPolicy Bypass -File scripts/dagitim-paketi.ps1` → `oyun/Build/MarsKod-Windows.zip` (Unity'siz açılan zip).
- **Android paketi:** `Unity.exe -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildAndroid` → `oyun/Build/Android/marskod.apk`.
- Ayrıntılı geliştirme kuralları: [CLAUDE.md](CLAUDE.md).
