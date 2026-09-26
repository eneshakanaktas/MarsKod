# MarsKod — Tasarım Belgesi (İlk Sürüm)

> Tarih: 2026-09-25 · Durum: İnceleme bekliyor
> "MarsKod" geçici isimdir; kesin isim sonra verilecek.

## 1. Amaç ve hedef kitle

Telefonda oynanan, oyun oynarken **gerçek Python** öğreten bir bulmaca oyunu. İlham: Steam'deki *The Farmer Was Replaced* (oyuncu kod yazar, robot/drone o kodla işi yapar). Mantık benzer, ama tema, eğitim sistemi ve telefona uyarlama özgün.

Hedef kitle:
- Yazılım bilgisi olmayan, sıfırdan başlayanlar.
- Biraz bilip yazım kurallarında (syntax) kendini geliştirmek isteyenler.
- **Yaş: 13 yaş ve altı hedef kitle değildir** (karar: 2026-09-25, Ragıp). Oyun yaş sormaz ve yaşa göre farklı davranmaz. Sebep: Çocuklara yönelik uygulamalar Google Play Aileler politikasına ve KVKK'nın veli izni kurallarına girer; bu da reklam/satın alma, veri toplama ve geliştirmeyi zorlaştırır. Mağaza kaydında hedef yaş grubu buna göre seçilecek.
- Anlatım tonu: Tek ton; sıcak ve sade, çocuksu değil. İleride ayarlara oyuncunun kendi seçeceği "Anlatım: Eğlenceli / Sade" seçeneği eklenecek (yaşa göre değil, tercihe göre). Metin sistemi buna hazır kurulur.

İlk hedef para kazanmak değil; oyun sevilirse reklam/satın alma ve pazarlama sonradan düşünülecek.

**Başarı ölçütü:** Kodla hiç tanışmamış biri, dışarıdan yardım almadan 20 bölümü bitirebilmeli ve sonunda kısa bir `for` + `if` programını kendi başına yazabilmeli.

## 2. Alınan kararlar (özet)

| Konu | Karar |
|---|---|
| Programlama dili | Python (ilk sürümde tek dil) |
| Tema | Mars kolonisi: robot kaynak toplar → depolar → koloni binalarına dağıtır. Dünya yok; her şey koloni içinde. |
| Kod yazma | Kademeli geçiş: parça klavyesiyle başlanır, komutta ustalaştıkça o komut elle yazılır; sonunda tam klavye. |
| İpucu | 3 kademe; 1. ipucu bedava, 2.–3. oyunla kazanılan jetonla. |
| Para | İlk sürümde reklam ve gerçek para yok; sistem sonradan eklenebilecek şekilde kurulur. |
| İlk açılış | Tek soru ("Daha önce kod yazdın mı?"); ilk bölümler zorunlu antrenmandır ve seviye sessizce ölçülür. Deneyimliler kısa bir kod sınavıyla ileriden ve klavyeyle başlayabilir. |
| Eğitime yönlendirme | Öner + ödüllendir; aynı hata 3–4 kez üst üste → ders zorunlu. Her 5 bölümde mini sınav (zorunlu). |
| Kayıt | Telefonda; isteğe bağlı Google/Apple hesabıyla bulut yedeği. |
| Platform | Android + iPhone tek kodla; önce Google Play. |
| Teknik yol | ~~Expo (React Native)~~ → **Unity 6** (karar yönü 2026-09-26, Enes; Expo/Godot görsel taslakları beğenilmedi) + kendi mini-Python motorumuz (Unity'de C#'a taşınacak; mevcut 919 test kılavuz). |
| Arayüz dili | Türkçe (İngilizce sonraki sürümlerde). |
| Hata mesajı | Çift dilli: Türkçe açıklama + gerçek Python'un İngilizce hata mesajı. Başta Türkçe öne çıkar; oyuncu ilerledikçe İngilizce öne geçer, Türkçe dokununca açılır. |

## 3. Oyun döngüsü ve hikâye

Kod yaz → ▶ Çalıştır → robot Mars'ta çalışır → kaynak toplanır/depolanır/binalara gider → binalar yeni komut ve alan açar → yeni, daha zor görev.

Yeni kod konuları hikâyeyle açılır: örn. Araştırma Laboratuvarı yeterince malzeme alınca "tarama sensörü geliştirildi" der ve oyuncu `if` komutunu kazanır.

Uzun vadeli dünya planı:

| Dünya | Oyunda | Kodda | İlk sürümde? |
|---|---|---|---|
| 1. İniş bölgesi | Yürü, buz/kaya topla | Komut sırası, `for` | ✅ |
| 2. Depo | Cinsine göre rafa koy, depo dolunca karar ver | `if`, değişkenler, listeler | ✅ |
| 3. Dağıtım/lojistik | Binaların ihtiyacına göre paket hazırla, en kısa rotayı bul | Fonksiyonlar, sıralama, rota bulma | ❌ sonra |
| Sonrası | İkinci robot, konveyörler, fırtınalar | İleri konular | ❌ sonra |

## 4. İlk sürüm kapsamı

**Var:**
- 2 dünya, ~20 bölüm.
- 3 bina: Araştırma Laboratuvarı (yeni komut), Sera (yeni alan), Depo (kapasite).
- Kademeli kod klavyesi, ▶ Çalıştır, ⏯ Adım adım çalıştırma (kod satır satır ağır çekimde, değişkenler etiketli kutular olarak görünür), çift dilli hata mesajları.
- Eğitim bölümü: işlenen ~8–10 konunun her biri için kısa görsel ders + ardından mini oyun/soru.
- Oyuncu profili + "Bilgi haritası" ekranı, ders önerisi, her 5 bölümde mini sınav.
- 3 kademeli ipucu + jeton cüzdanı.
- Telefonda kayıt + isteğe bağlı hesapla bulut yedeği.

**Yok (sonraki sürümler):** Dünya 3, ikinci robot, konveyörler, sıralama tablosu, reklam/satın alma, İngilizce arayüz, başka programlama dilleri, yapay zeka özellikleri, oyuncu çözümlerini toplama (aşağıda).

**Ertelenen fikir — Oyuncu çözümlerini toplama (ileride tekrar gündeme getirilecek):**
- Oyuncu, desteklenen komutlarla bizimkinden farklı ama görevi tamamlayan bir çözüm yazarsa → çözüm "alternatif doğru çözüm" olarak kaydedilir.
- Oyuncu motorun bilmediği bir Python özelliği kullanırsa → kod ve eksik özellik "istenen özellik" olarak kaydedilir (doğruluğu telefonda bilinemez); en çok istenen özellik motora ilk eklenir.
- Yan fırsatlar: "oyunun bilmediği bir yol buldun" rozeti, "senin çözümün daha kısa" karşılaştırması, ipuçlarını gerçek veriyle iyileştirme, ileride "başkaları nasıl çözdü?" ekranı.
- Kodlar isimsiz, telefonda biriktirilip internet gelince buluta (Supabase) gönderilir.
- **Açık karar:** Oyuncudan izin nasıl alınacak (bildir + kapatılabilir / oyuncu açar / her seferinde sor) ve KVKK/gizlilik metni. Bu karar verilene kadar özellik yapılmaz.
- İlk sürüme etkisi: Motor, desteklemediği özelliği zaten türüyle tanıyıp "bu oyunda henüz yok" dediği için ileride bu özelliğe hazır olur; ek iş yok.

## 5. Sistemin parçaları

Her parça tek bir iş yapar; birbirinden bağımsız test edilebilir.

1. **Mini-Python motoru** — Oyuncunun kodunu okur, satır satır çalıştırır (adım adım modu için her adımda durabilir). Desteklenenler (ilk sürüm): değişkenler, sayılar/metin/mantık değerleri, aritmetik ve karşılaştırma, `if/elif/else`, `for ... in range(...)`, `while`, `def` ve `return`, listeler (temel işlemler), `print`. Hata mesajlarını **gerçek Python ile birebir aynı** İngilizce metinle ve ayrıca Türkçe açıklamayla üretir; her hatayı bir türe ayırır (yazım, girinti, döngü, koşul, isim...). Sonsuz döngüye karşı adım sınırı vardır. Oyundan habersizdir; oyun komutlarını (`move`, `collect`...) dışarıdan verilen fonksiyonlar olarak tanır.
2. **Mars dünyası** — Izgara harita, robot, kaynaklar, binalar. Motordan gelen komutları uygular, sorulara cevap verir ("burada buz var mı?"), görevin tamamlanıp tamamlanmadığını denetler.
3. **Kod ekranı** — Parça klavyesi, gerçek klavye, özel işaret şeridi, otomatik tamamlama. Hangi komutun parça, hangisinin elle yazılacağına oyuncu profiline göre karar verir.
4. **Oyuncu profili** — Konu konu puan (🟢 iyi / 🟡 orta / 🔴 zayıf); her bölümden sonra güncellenir.
5. **Eğitim bölümü** — Görsel dersler, mini oyunlar, mini sınavlar; profilden gelen önerileri sunar.
6. **İpucu ve jeton** — 3 kademe ipucu (1: yön, 2: ilgili derse götürür, 3: kodun bir kısmı), jeton cüzdanı. Jeton kazanma: bölüm bitirme, mini sınav, önerilen dersi bitirme, günlük giriş. İleride reklam/satın alma buraya bağlanır.
7. **Kayıt** — Önce telefona; hesap bağlıysa buluta kopya.

**Bölümler veri dosyasıdır, koda gömülmez.** Her bölüm dosyası şunları içerir: harita, görev, açık komutlar, öğretilen konular, ipuçları, tipik hatalar ve bir doğru çözüm. Böylece kod bilmeyen ekip üyeleri de bölüm tasarlayabilir.

## 6. Oyuncu analizi

- Bölüm sırasında kaydedilen: deneme sayısı, süre, hata türleri, alınan ipuçları ve kademeleri.
- Bölüm sonunda o bölümün konularının puanı güncellenir: hızlı ve ipucusuz çözüm puanı artırır; çok deneme, ipucu ve tekrarlanan hata düşürür.
- 🔴 konu → "2 dakikalık ders ister misin? (+jeton)" önerisi.
- Aynı hata türü 3–4 kez üst üste → ders zorunlu.
- Her 5 bölümde mini sınav: sorular ağırlıkla 🟡/🔴 konulardan; uzun süredir kullanılmayan 🟢 konular da ara ara eklenir (aralıklı tekrar).
- Tamamen kurallarla çalışır; yapay zeka yok (bedava, internetsiz, yanlış bilgi riski yok).

## 7. Ters giden durumlar

| Durum | Davranış |
|---|---|
| Bitmeyen döngü | Adım sınırında durdurulur, Türkçe + İngilizce açıklama; oyun donmaz. |
| Robot duvara/harita dışına | Robot durur, satır kırmızı yanar; bunun oyun kuralı olduğu (Python hatası olmadığı) belirtilir. |
| Desteklenmeyen Python özelliği | "Gerçek Python'da var ama bu oyunda henüz yok" mesajı. |
| Uygulama/telefon kapanır | Kod her değişiklikte, ilerleme her bölüm sonunda telefona kaydedilir. |
| İnternet yok / yedek başarısız | Oyun etkilenmez; yedek sonra yeniden denenir. |

## 8. Test

- **Motor:** Yüzlerce küçük Python kodu hem gerçek Python'da hem motorda çalıştırılıp sonuçlar ve hata mesajları otomatik karşılaştırılır.
- **Bölümler:** Her bölümün doğru çözümü otomatik çalıştırılır; geçilemeyen bölüm yayınlanamaz.
- **Ekranlar:** Maestro ile (2026-09-25 kararı; önceden Playwright yazıyordu, o web içindir) temel akışlar (aç, oyna, ipucu al, kaydet) otomatik denenir.
- **İnsan testi:** İlk 5 bölüm hazır olunca kod bilmeyen 2–3 kişiye oynatılıp takıldıkları yerler izlenir.

## 9. Riskler

- **Motorun yazımı en büyük teknik iş.** Adım adım, testle ilerlenecek; desteklenen Python alt kümesi bilerek dar tutuldu.
- **Telefonda kod yazma zorluğu.** Kademeli klavye bunun çözümü; ilk insan testlerinde özellikle izlenecek.
- **Taklit algısı.** Orijinal oyunun ismi, görselleri ve "drone + tarla" ikilisi kullanılmayacak.
- **Kapsam kayması.** Bölüm 4'teki "Yok" listesi ilk sürüm bitene kadar korunur.

## 10. Sonraki adımlar (bu belgeden sonra)

1. Bu belgenin ekip tarafından onaylanması.
2. Adım adım yapım planı (writing-plans).
3. Görsel tasarım adımı: ekibin elindeki referans görseller incelenecek; renk, çizim tarzı, robot ve bina görünümü belirlenecek (ui-ux-pro-max + impeccable).
4. Araç önerileri (onayla kurulur): Expo resmi skill'leri (mobil karar verildi), Supabase eklentisi (isteğe bağlı üyelik/bulut yedeği için).
5. GitHub'da private depo + Hamza ve Ragıp'ın davet edilmesi.
