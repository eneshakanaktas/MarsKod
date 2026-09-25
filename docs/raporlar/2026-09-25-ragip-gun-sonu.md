# Gün Sonu Raporu — 25 Eylül 2026 (Ragıp)

> Enes ve Hamza için: bugün neler yapıldı, hangi kararlar alındı, sizden hangi kararlar bekleniyor.
> Kısa özet `ILERLEME.md`'de; burası ayrıntılı hâli.

## Kısaca
1. **Oyunun kalbi hazır:** Oyuncunun yazdığı Python kodunu çalıştıran motor bitti. 216 farklı Python örneğinde, gerçek Python ile **harfi harfine aynı** sonucu ve aynı hata mesajlarını veriyor.
2. **Hatalar Türkçe anlatılıyor:** Her hataya sade bir Türkçe açıklama ve ipucu eklendi.
3. **Görsellikte bir sonuca varamadık:** Dört farklı görsel deneme yapıldı; Ragıp hiçbirini The Farmer Was Replaced seviyesinde bulmadı. **Oyun motoru değişikliği (Unity ya da Godot) konuşulmalı.** Bu, sizin de katılmanız gereken en önemli karar.

---

## 1. Hazırlık
- Ragıp'ın bilgisayarına gerekli araçlar kuruldu: Python, GitHub aracı, Android ayarları ve MarsKod için ayrı bir sanal telefon.
- Yeni bir eklenti kurmadan önce güvenlik taraması yapma kuralımız uygulandı. Expo eklentisi SkillSpector ile tarandı; ciddi bulgular yanlış alarm çıktı ve eklenti kuruldu.
- **Yapım planı** yazıldı: `docs/superpowers/plans/2026-09-25-marskod-yapim-plani.md`. Ana fikir şu: 20 bölümü sırayla yapmak yerine önce **3 bölümlük, baştan sona oynanabilir küçük bir oyun** yapmak. Böylece en büyük riskleri erkenden görürüz.

## 2. Python motoru (Aşama 1: bitti)
Oyuncu telefona kod yazıyor, robot o kodla hareket ediyor. Bu kodu okuyup çalıştıran parçaya "motor" diyoruz. Hazır bir motor kullanmak yerine kendimiz yazdık, çünkü:
- kodu **satır satır, ağır çekimde** gösterebilmek (⏯ Adım adım modu) ve
- hataları **Türkçe açıklayabilmek** istiyoruz.

**Nasıl emin olduk:** 216 küçük Python kodu yazdık. Her birini önce bilgisayardaki gerçek Python'da, sonra bizim motorumuzda çalıştırdık ve sonuçları otomatik karşılaştırdık. Hepsi birebir aynı çıkıyor. Toplam 919 otomatik test var ve hepsi geçiyor.

**Oyuncunun göreceği hata örnekleri:**

| Oyuncunun yazdığı | Python'un mesajı | Bizim Türkçe açıklamamız |
|---|---|---|
| `print("Enerji: " + enerji)` | can only concatenate str (not "int") to str | **Metne sayı eklenemez.** İpucu: `str(enerji)` kullan |
| `enerj` (yanlış yazım) | name 'enerj' is not defined. Did you mean: 'enerji'? | **Tanımsız isim.** İpucu: Belki `enerji` yazmak istedin? |
| `if x = 3:` | Maybe you meant '==' ... | **Karşılaştırmada tek eşittir** |
| Telefon klavyesinin koyduğu kıvrık tırnak “ ” | invalid character '“' | **Kıvrık tırnak:** Telefon klavyeleri bazen düz tırnağı buna çevirir |

**Başka hazır olanlar:**
- **Bitmeyen döngü koruması:** Oyun donmuyor, motor kodu durdurup açıklıyor.
- **Desteklenmeyen özellik bildirimi:** Motor `import`, `class` gibi ilk sürümde olmayacak özellikleri tanıyor ve "Gerçek Python'da var ama bu oyunda henüz yok" diyor.
- **Oyun komutlarını bağlama:** `move("north")` ve `collect()` gibi oyun komutları motora dışarıdan bağlanabiliyor.
- **Hata türleri:** Her hata 15 türden birine ayrılıyor. İleride "bu oyuncu hep girinti hatası yapıyor, girinti dersini öner" kararı buna göre verilecek.

**Sizin katkı verebileceğiniz yer:** Türkçe hata metinlerinin hepsi tek bir dosyada: `src/engine/explanations-tr.ts`. Dosyanın başında kod bilmeyenler için bir düzenleme rehberi var. Sadece tırnak içindeki cümleleri değiştirerek dili düzeltebilirsiniz.

## 3. Bugün alınan kararlar
| Karar | Neden |
|---|---|
| **13 yaş ve altı hedef kitle değil.** Oyun yaş sormayacak. | Çocuk uygulamaları Google Play'in Aileler kurallarına ve KVKK'nın veli izni şartına girer; reklam, satın alma ve veri toplama zorlaşır. |
| **Anlatım tek tonda:** sıcak ve sade, çocuksu değil. | İleride ayarlara oyuncunun seçeceği "Eğlenceli / Sade" seçeneği eklenecek. Metin sistemi buna hazır. |
| **Ekran testleri Maestro ile yapılacak** (Playwright değil). | Playwright web sayfalarını test eder; Maestro telefon uygulamaları için yapılmış. |
| Önce Android. | Tasarımdaki karar korunuyor. |

## 4. Görsel denemeler: buradayız, karar gerekiyor
Oyunun görünümü için dört yol denendi. Ekran görüntüleri `docs/tasarim/` klasöründe:

| # | Yöntem | Sonuç |
|---|---|---|
| 1 | Kodla çizilen düz şekiller | Dama tahtası gibi, yetersiz |
| 2 | Ekran kartında hesaplanan Mars dokusu + "havada yüzen ada" görünümü | Daha iyi ama yetersiz |
| 3 | Ücretsiz Kenney çizimleri + canlılık animasyonları + yeni yazı tipleri | Ragıp beğenmedi |
| 4 | **3 boyut** (Kenney Space Kit modelleri, gerçek ışık ve gölge) | Ragıp beğenmedi (`dorduncu-deneme-3d.png`) |

**Neden olmadı, dürüst değerlendirme:** The Farmer Was Replaced **Unity** oyun motoruyla yapılmış. Bizim kullandığımız yol (Expo) bir uygulama çatısı; 3 boyut, parlama efektleri, kenar yumuşatma ve hazır animasyon araçları gibi oyun motorlarının hazır verdiği şeyler burada ya yok ya da çok zahmetli. Ragıp'ın istediği seviye (bol animasyonlu, derinlikli, keskin) için **gerçek bir oyun motoru gerekiyor.**

### Sizden beklenen karar: oyun motoru
| | Unity | Godot |
|---|---|---|
| Görsel kalite | En yüksek | Çok iyi (telefonda Unity'nin biraz gerisinde) |
| Ücret | Yıllık 200 bin $ gelire kadar ücretsiz | Tamamen ücretsiz |
| Claude ile çalışma | Sahneler görsel editörde kuruluyor; Claude o editörü göremiyor, her şeyi koddan kurması gerekiyor. Yavaş. | Her şey metin dosyası; Claude doğrudan yazabiliyor. Hızlı. |
| Telefonda boyut | 60–120 MB | 40–80 MB |

**Claude'un önerisi Godot:** Ekipte kod yazan yok ve işin çoğunu Claude yapacak; Godot'da her şeye doğrudan erişebiliyor. Unity seçilirse de çalışılabilir, ama daha yavaş ilerlenir.

**Geçişte kaybolmayanlar:** Tasarım, plan, Türkçe metinler, 216 Python örneği ve doğru cevapları. Motor yeni dile çevrilirken aynı örneklerle doğrulanacak.

**Önerilen yol:**
1. Seçilen motorda **tek bir deneme sahnesi** yapılır ve telefonda denenir: Mars adası, robotun yürüme animasyonu, parlayan buz, toz ve parlama efektleri.
2. Beğenilirse Python motoru taşınır.
3. Beğenilmezse birkaç gün kaybedilmiş olur, daha fazlası değil.

## 5. Diğer açık sorular
- **PC ve mobil (Ragıp'ın sorusu):** PC sürümü de olsun mu? Claude'un görüşü: görseller aynı kalsın, sadece ekran düzeni değişsin (PC'de harita solda ve kod sağda, telefonda alt alta). Önce mobil, PC sonra.
- **Boyut:** Ragıp oyunun telefonda fazla yer kaplamamasını istiyor. Unity/Godot'ya geçilirse boyut 30–45 MB'tan 60–120 MB'a çıkar. Bu bir değiş tokuş.
- **Oyunun adı** hâlâ geçici ("MarsKod").
- **Oyuncu çözümlerini toplama** fikri hâlâ ertelenmiş durumda (KVKK/izin kararı bekliyor).

## 6. Denemek isterseniz
Bilgisayarınızda Node.js varsa: `git pull` → `npm install` → `npx expo start`, sonra telefondaki Expo Go uygulamasıyla ekrandaki QR kodu okutun. Şu an 4. deneme (3 boyut) görünüyor. Motor değişirse bu adımlar da değişecek.

## Dosyalar
- Yapım planı: `docs/superpowers/plans/2026-09-25-marskod-yapim-plani.md`
- Tasarım belgesi (güncellendi: yaş, ton, Maestro): `docs/superpowers/specs/2026-09-25-marskod-design.md`
- Görsel yön ve kaynak lisansları: `docs/tasarim/gorsel-yon.md`
- Türkçe hata metinleri: `src/engine/explanations-tr.ts`
