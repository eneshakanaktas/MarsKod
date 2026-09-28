# MarsKod — Yapım Planı (İlk Sürüm)

> Tarih: 2026-09-25 · Yazan: Ragıp (Claude ile) · Dayandığı belge: `docs/superpowers/specs/2026-09-25-marskod-design.md`
> Bu plan yaşayan bir belgedir: her aşama bitince işaretlenir, sonraki aşamanın ayrıntısı o aşamaya gelince yazılır.
>
> **2026-09-27 güncellemesi (Ragıp): Oyun motoru Unity 6.** Aşama 0-1 Expo'da (TypeScript) yapıldı; `src/` klasörü, motor C#'a taşınana kadar **referans** olarak kalır, sonra arşivlenir. Oyunun asıl evi `oyun/` klasörü (Unity projesi). Hedef: önce Play Store (Android), sonra Steam.

## Ana fikir: önce "oynanabilir ince dilim"

Her parçayı sırayla bitirmek yerine, önce **baştan sona çalışan küçük bir oyun** yapıyoruz: 3 bölüm, çirkin ama çalışan ekran, kod yazılıyor, robot Mars'ta yürüyor. Sonra bunu genişletip güzelleştiriyoruz.

Neden: En büyük iki risk (motorun yazılması ve telefonda kod yazmanın zorluğu) erken ortaya çıkar. 20 bölüm yazdıktan sonra "bu oynanmıyor" demek çok pahalı olur.

## Klasör düzeni (hedef, Unity)

```
oyun/                  Unity projesi (oyunun asıl evi). Açmak: oyun/calistir.bat
  Assets/Scripts/      Sahne, robot, arayüz (Unity'ye bağlı kod).
  Assets/Motor/        Mini-Python motoru (C#). Unity'den habersiz; saf mantık.   ← Aşama 1b
  Assets/Dunya/        Mars dünyası kuralları (ızgara, robot, kaynak, görev). Saf mantık. ← Aşama 2
  Assets/Resources/Bolumler/  Bölüm dosyaları (JSON). Kod bilmeyen ekip üyeleri de düzenleyebilir (kılavuz: docs/tasarim/bolum-dosyasi.md).
motor-test/            Motor ve dünya için .NET test projesi (Unity açmadan, saniyeler içinde).
tests/python-cases/    Motoru gerçek Python'la karşılaştıran 194 örnek (dilden bağımsız; aynen kullanılır).
arsiv/expo/            Eski Expo kodu (TypeScript motoru dahil), arşivde; motorun C# hâli oyun/Assets/Motor.
prototipler/           Görsel taslaklar (unitytaslak1, godot...). Dokunulmaz.
```

Kural: Motor ve Dünya saf mantıktır (UnityEngine kullanmaz); bilgisayarda `dotnet test` ile test edilir. Sahne bunları sadece kullanır.

---

## Aşama 0 — Proje iskeleti ✅ (2026-09-25, Ragıp)
- [x] Expo projesi (TypeScript) oluştur, depo köküne yerleştir.
- [x] React Native Skia + Reanimated kur (harita çizimi ve akıcı hareket için).
- [x] Test aracı (Jest) kur; örnek bir test çalışsın.
- [x] Klasör düzenini oluştur.
- [x] Sanal telefonda (Android emülatörü) açılıp Skia ile basit bir ızgara çizdiğini gör.

**Bitti sayılır:** Her ekip üyesi `npm install` + `npx expo start` ile oyunu kendi telefonunda/emülatöründe açabiliyor; `npm test` yeşil.

## Aşama 1 — Mini-Python motoru (çekirdek) ✅ (2026-09-25, Ragıp — 216 örnek, 919 test)
En büyük iş. Küçük adımlarla ve her adım testle ilerler.

1. [x] **Karşılaştırma düzeneği:** ✅ (2026-09-25, Ragıp — 43 örnek, 7 grup; `npm run python-referans`) `tests/python-cases/` içindeki her `.py` örneği gerçek Python'da çalıştırılıp çıktısı/hatası bir "beklenen sonuç" dosyasına yazılır (bu dosyalar depoya eklenir; böylece Python'u olmayan ekip üyesi de testi çalıştırabilir). Motor aynı örneği çalıştırıp sonucu karşılaştırır.
2. [x] **Kelime ayırıcı:** ✅ (2026-09-25, Ragıp — kelime aşamasındaki 14 hata örneği Python ile birebir) Kodu parçalara böler; girintileri (Python'un blok yapısı) doğru tanır.
3. [x] **Cümle çözücü:** ✅ (2026-09-25, Ragıp — 08-cumle-hatasi grubundaki 54 hata örneği Python ile birebir) Parçalardan kodun yapısını (ağacını) çıkarır. Yazım hatalarını gerçek Python'un İngilizce mesajıyla birebir verir.
4. [x] **Çalıştırıcı — temel:** değişkenler, sayı/metin/mantık, aritmetik, karşılaştırma, `print`.
5. [x] **Akış:** `if/elif/else`, `for ... in range(...)`, `while`, `break/continue`.
6. [x] **Fonksiyon ve liste:** `def`, `return`, listeler (oluşturma, indeks, `append`, `len`, `for x in liste`).
7. [x] **Adım adım çalışma:** Motor her satırda durabilir ve o anki değişkenleri verir (⏯ modu için).
8. [x] **Güvenlik:** Adım sınırı (bitmeyen döngü), desteklenmeyen özellikte "gerçek Python'da var ama bu oyunda henüz yok".
9. [x] **Dış komutlar:** Oyun `move`, `collect` gibi komutları motora dışarıdan verebilir.
   ✅ 4–9 (2026-09-25, Ragıp): 216 Python örneğinin hepsi birebir; toplam 774 test. Çalıştırıcı "derle + tek döngüde işle" yöntemiyle yazıldı (Python'un 1000 derinlik sınırı birebir, JavaScript yığını büyümez).
10. [x] ✅ (2026-09-25, Ragıp — `src/engine/explanations-tr.ts`; bilinen her hatanın özel metni olduğu test ediliyor) **Türkçe açıklamalar + hata türleri:** Her hata bir türe ayrılır (yazım, girinti, döngü, koşul, isim...) ve Türkçe açıklaması eklenir.

**Bitti sayılır:** 200+ karşılaştırma örneği geçiyor.

## Aşama 0b — Unity iskeleti ✅ (2026-09-27, Ragıp)
- [x] `oyun/` Unity projesi, unitytaslak1'den kuruldu (taslak `prototipler/unity/` olarak aynen duruyor).
- [x] Oyun alanı büyütüldü: 6×6 alan, daha dik bakış; kamera alanı başlık ile kod kartı arasındaki boşluğa kendisi sığdırır (kod uzayıp kısalınca alan kendiliğinden ayarlanır).
- [x] Uygulama kimliği `com.marskod.oyun` (geçici; Play Store'a ilk yüklemeden önce kesinleşmeli, sonra değiştirilemez).

## Aşama 1b — Motoru C#'a taşı (taşıma ✅ 2026-09-27, sahneye bağlama ✅ 2026-09-27, Ragıp)
Aşama 1'deki TypeScript motoru (`src/engine`) C#'a birebir taşınır. Sıra: kelime ayırıcı → cümle çözücü → derleyici/çalıştırıcı → Türkçe açıklamalar.
- [x] Motor `oyun/Assets/Motor/` içinde (C#, `UnityEngine` kullanmaz; `MarsKod.Motor.asmdef` bunu zorunlu kılar). Dosyalar TypeScript'tekilerle birebir eşleşir (tokenizer → `Tokenizer.cs`, parser → `Parser.cs`...).
- [x] `motor-test/`: .NET test projesi; motoru Unity'nin sınırlarıyla (C# 9, .NET Standard 2.1) derler, `tests/python-cases/` örneklerini gerçek Python sonuçlarıyla karşılaştırır. `motor-test` klasöründe `dotnet test`: **988 test, hepsi geçiyor** (194 örnek birebir).
- [x] Unity içinde denetim: menü *MarsKod > Motor denetimi* (ya da `-executeMethod MotorDenetimi.Calistir`) aynı 194 örneği Unity'nin ortamında çalıştırır: hepsi birebir.
- [x] Motoru sahneye bağla: karttaki kod gerçekten çalışıyor. Kod önce motorda + dünya kurallarında anında çalıştırılır (`ProgramRun`), çıkan kayıt satır satır animasyonla oynatılır (bitmeyen döngü oyunu dondurmaz; adım adım modu aynı kaydı kullanacak). Hata olunca satır kırmızı yanar, Türkçe açıklama + Python'un kendi mesajı çıkar (Python hatası / oyun kuralı / eksik görev ayrı gösterilir). Kod renklendirme `CodeColors.cs`.
- [x] ✅ (2026-09-27, Ragıp — `arsiv/expo/`, etiket `expo-son`) TypeScript motorunu (`src/`, `tests/python-cases.test.ts`, Expo dosyaları) arşivle.

**Bitti sayılır:** 194 örneğin hepsi C# motorunda birebir; Unity'de sahnedeki kod gerçekten çalışıyor.

## Aşama 2 — Mars dünyası + bölüm dosyası
- [ ] Izgara, robot (konum/yön), kaynak (buz, kaya), depo, bina kuralları. *Başlandı (09-27):* `Assets/Dunya/` (ızgara, robot yeri, buz, `move`/`collect`, yönler `North/East/South/West`); testleri `motor-test/DunyaTests.cs`. ✅ (09-27, Ragıp) kaya engeli, hedef kare, bölümde açılmamış komut. Depo, bina yok (Dünya 2'de).
- [x] Robot harita dışına çıkarsa ya da kayaya çarparsa: dur, satırı işaretle, "bu oyun kuralı, Python hatası değil" de (09-27, kaya da).
- [x] Bölüm dosyası biçimi (JSON): harita, görev, açık komutlar, konular, ipuçları, tipik hatalar, doğru çözüm. ✅ (09-27, Ragıp) `Assets/Dunya/Level.cs`, harita "resim gibi" (R B K H .), yanlışta satır numaralı Türkçe mesaj.
- [x] Bölüm denetleyici: her bölümün doğru çözümü otomatik çalışır; geçemeyen bölüm test hatası verir. ✅ (09-27, Ragıp) `LevelCheck` + `motor-test/BolumTests.cs`; tipik hataların gerçekten hatalı olduğu da denetlenir.
- [x] İlk 3 bölüm (Dünya 1'in başı). ✅ (09-27, Ragıp) 1: yalnızca `move`, hedef kare; 2: `move` + `collect`, kaya; 3: 5 buz, `for`. Sahne bölümü dosyadan kurar, bitince "Sonraki bölüm".

## Aşama 3 — İnce dilim: ilk oynanabilir sürüm 🎯
- [x] Oyun ekranı: üstte Mars alanı (`oyun/` sahnesi hazır), altta kod alanı (şimdilik normal klavye; `CodeEditor.cs`), ▶ Çalıştır.
- [ ] Robot hareketleri animasyonlu; hata olunca satır kırmızı.
- [ ] ⏯ Adım adım modu: kod satır satır, değişkenler etiketli kutular.
- [ ] Çift dilli hata kutusu.
- [ ] Bölüm seçimi + bölüm sonu ekranı.
- [ ] **İnsan testi (erken):** Kod bilmeyen 1–2 kişiye 3 bölüm oynat; takıldıkları yeri not et.

**Bitti sayılır:** Telefonda 3 bölüm baştan sona oynanıyor.

## Aşama 4 — Görsel tasarım
- [ ] Ekibin referans görselleri incelenir; renk, çizim tarzı, robot ve bina görünümü belirlenir.
- [ ] İnce dilimin ekranları yeni görünüme geçirilir.

## Aşama 5 — Kademeli kod klavyesi
> **Öne alındı (2026-09-28, Enes):** Aşama 3'ün kalanından önce yapılıyor. Tasarım: `docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md` (Parça 1: klavye + satır takibi + XP; Parça 2: seviye sistemi sonra). Yapan: Ragıp.
- [ ] Parça klavyesi (komutlar dokunarak eklenir), özel işaret şeridi (`:`, `()`, girinti), otomatik tamamlama.
- [ ] Oyuncu profiline göre komutun "parça" mı "elle yazılır" mı olacağına karar veren kural.

## Aşama 6 — Bölümler (20 bölüm, 2 dünya)
- [ ] Dünya 1 (İniş bölgesi): komut sırası, `for`.
- [ ] Dünya 2 (Depo): `if`, değişkenler, listeler.
- [ ] 3 bina ve açtıkları şeyler (Laboratuvar → yeni komut, Sera → yeni alan, Depo → kapasite).
- Bölüm dosyaları kod bilmeyen ekip üyeleri tarafından da yazılabilir; denetleyici hatalıysa uyarır.

## Aşama 7 — Öğrenme sistemi
- [ ] Oyuncu profili (konu başına 🟢/🟡/🔴) ve "Bilgi haritası" ekranı.
- [ ] 3 kademe ipucu + jeton cüzdanı.
- [ ] Eğitim bölümü: ~8–10 konu için kısa görsel ders + mini oyun/soru.
- [ ] Ders önerisi, aynı hata 3–4 kez → zorunlu ders, her 5 bölümde mini sınav.
- [ ] İlk açılış sorusu ("Daha önce kod yazdın mı?") + deneyimliler için giriş sınavı.

- [ ] (İlk sürümden sonra da olabilir) Ayarlar: "Anlatım: Eğlenceli / Sade" seçeneği. Metin sistemi hazır; sadece eğlenceli tondaki metinler yazılacak.

## Aşama 8 — Kayıt
- [ ] Telefona kayıt: kod her değişiklikte, ilerleme her bölüm sonunda.
- [ ] İsteğe bağlı hesap (Google/Apple) + Supabase ile bulut yedeği; internet yoksa sonra dener.

## Aşama 9 — Son test ve yayın
- [ ] Ekran testleri (temel akış: aç, oyna, ipucu al, kaydet). Maestro Unity'nin içini göremez (sadece dokunma + ekran görüntüsü); Unity'nin kendi test aracı (PlayMode) daha uygun olabilir — Aşama 3'te karar.
- [ ] İnsan testi: ilk 5 bölüm, kod bilmeyen 2–3 kişi.
- [ ] Google Play iç test sürümü (Unity'den AAB paketi) → kapalı test → yayın. Sonra Steam (PC düzeni).

---

## Açık konular
- **Ekran testi aracı — (2026-09-25, Ragıp): Maestro seçilmişti; Unity'ye geçişle yeniden değerlendirilecek (Aşama 9 notu).** Tasarımda Playwright yazıyordu, ama Playwright web sayfası test eder, telefon uygulamasını değil. Maestro ücretsiz ve telefon için yapılmış. İlk ekran testleri Aşama 3 bitince yazılabilir.
- **Eklentiler:** Expo eklentisi 2026-09-26'da çıkarıldı; yerine Unity eklentileri kuruldu (ILERLEME.md, Enes 09-26).
- **Supabase eklentisi:** Aşama 8'e gelince aynı şekilde SkillSpector ile taranıp onayla kurulacak.
- **Görsel:** Yön unitytaslak1 ("premium sade", şafak öncesi Mars); `oyun/` sahnesi bunun üstüne kuruldu.
- Oyunun kesin ismi.
