# MarsKod — Yapım Planı (İlk Sürüm)

> Tarih: 2026-09-25 · Yazan: Ragıp (Claude ile) · Dayandığı belge: `docs/superpowers/specs/2026-09-25-marskod-design.md`
> Bu plan yaşayan bir belgedir: her aşama bitince işaretlenir, sonraki aşamanın ayrıntısı o aşamaya gelince yazılır.

## Ana fikir: önce "oynanabilir ince dilim"

Her parçayı sırayla bitirmek yerine, önce **baştan sona çalışan küçük bir oyun** yapıyoruz: 3 bölüm, çirkin ama çalışan ekran, kod yazılıyor, robot Mars'ta yürüyor. Sonra bunu genişletip güzelleştiriyoruz.

Neden: En büyük iki risk (motorun yazılması ve telefonda kod yazmanın zorluğu) erken ortaya çıkar. 20 bölüm yazdıktan sonra "bu oynanmıyor" demek çok pahalı olur.

## Klasör düzeni (hedef)

```
src/
  engine/     Mini-Python motoru. Oyundan habersiz; telefon koduna hiç dokunmaz.
  world/      Mars dünyası kuralları (ızgara, robot, kaynak, bina, görev denetimi).
  levels/     Bölüm dosyaları (JSON). Kod bilmeyen ekip üyeleri de düzenleyebilir.
  screens/    Ekranlar (oyun, eğitim, profil...).
  components/ Ekran parçaları (harita çizimi, kod klavyesi...).
tests/
  python-cases/  Motoru gerçek Python'la karşılaştıran küçük kod örnekleri.
```

Kural: `engine` ve `world` saf mantıktır; telefon olmadan bilgisayarda test edilir. Ekranlar bunları sadece kullanır.

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

## Aşama 2 — Mars dünyası + bölüm dosyası
- [ ] Izgara, robot (konum/yön), kaynak (buz, kaya), depo, bina kuralları.
- [ ] Robot duvara/harita dışına çıkarsa: dur, satırı işaretle, "bu oyun kuralı, Python hatası değil" de.
- [ ] Bölüm dosyası biçimi (JSON): harita, görev, açık komutlar, konular, ipuçları, tipik hatalar, doğru çözüm.
- [ ] Bölüm denetleyici: her bölümün doğru çözümü otomatik çalışır; geçemeyen bölüm test hatası verir.
- [ ] İlk 3 bölüm (Dünya 1'in başı).

## Aşama 3 — İnce dilim: ilk oynanabilir sürüm 🎯
- [ ] Oyun ekranı: üstte Skia ile Mars haritası, altta kod alanı (şimdilik normal klavye), ▶ Çalıştır.
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
- [ ] Ekran testleri **Maestro** ile (temel akış: aç, oyna, ipucu al, kaydet).
- [ ] İnsan testi: ilk 5 bölüm, kod bilmeyen 2–3 kişi.
- [ ] Google Play iç test sürümü (EAS ile paketleme) → kapalı test → yayın.

---

## Açık konular
- **Ekran testi aracı — ✅ KARAR VERİLDİ (2026-09-25, Ragıp): Maestro kullanılacak.** Tasarımda Playwright yazıyordu, ama Playwright web sayfası test eder, telefon uygulamasını değil. Maestro ücretsiz ve telefon için yapılmış. İlk ekran testleri Aşama 3 bitince yazılabilir.
- **Expo eklentisi — ✅ KURULDU (2026-09-25, Ragıp):** SkillSpector ile tarandı. Ciddi görünen bulgular yanlış alarm çıktı (belgelerdeki şifre ayarı örnekleri). Kullanım istatistiği kodu var ama varsayılan olarak kapalı.
- **Supabase eklentisi:** Aşama 8'e gelince aynı şekilde SkillSpector ile taranıp onayla kurulacak.
- **Görsel:** Şu anki dama görünümlü ızgara geçicidir; gerçek görünüm Aşama 4'te belirlenecek.
- Oyunun kesin ismi.
