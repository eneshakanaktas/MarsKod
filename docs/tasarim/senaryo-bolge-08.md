# Senaryo — Bölge 8: Terk edilmiş araştırma istasyonu (Bölüm 71-80)

> **Taslak 1 — 2026-11-14 (Ragıp + Claude; Ragıp onayladı).** Dayandığı belgeler: tasarım belgesi (`docs/superpowers/specs/2026-11-09-bolge-8-10-ders-bulmaca-design.md`), `senaryo-perde-2.md` (Bölge 8), `hikaye-kitabi.md`.
> Bulmacalar 2026-11-09'da yapıldı (Enes; plan `docs/superpowers/plans/2026-11-09-bolge-8-bulmacalar.md`); bu belge hikâye katmanını anlatır. Görünüş (istasyon, ekran, duvar çizimleri, sahneler) 3a/3b'de.

## Bu bölgenin görevi

- Ders: `return` (71-74) + metin (75-80). İki yeni bulmaca türü: ekran bulmacası (72, 77, 78; robot yürümez) ve okuyan robot (`read_note()`, 76, 80).
- Hikâye: istasyon → bilgisayarlar uyanır → Ece'nin duvar çizimleri → Kayıt 4 → katlanmış çizim → 24 kişilik liste → **"BKM-7 — dışarıda — görev: KAPI"** → Kor'un satırı → istasyon haritası (oklar tuz gölünün ötesini gösteriyor).
- Oyuncu ne öğrenir: Kıvılcım **bilerek dışarıda bırakıldı** (bir gün kapıyı açacak olan o); Kor'un adı ilk kez geçer (Perde 3'ün tohumu).

## Bölüm 70 → 71 geçişi

70'in finali → `sinav-70` → 71 açılırken kara ekranda iki satır: **"Yeni bölge: Araştırma istasyonu" / "Hedef: koloninin kayıtları"** (listeyi önceden söylemez; sürpriz 77-78'de). Kod: `StationTraces.TransitionLines`.

## Kayıt 4 (Bölüm 74 başı)

Dosya adı `cizimler.ses` (Kayıt 1 `ogretmene.ses`, 2 `rutinler.ses`, 3 `izler.ses`: konuya göre ad). Kayıt 3 gibi: Bölüm 74 ilk kez açılınca oynar, "Tekrar" ve sonraki girişler yeniden oynatmaz; yeniden dinlemek Kayıtlar sayfasından.

> Ece korkuyor ama belli etmiyor.
> Bana benziyor.
> Bütün gece çizdi; 'Kıvılcım yalnız kalmasın diye' dedi.

Yeri: 73'te oyuncu çizimlerin ilk yarısını gördü; kayıt "bütün gece" çizimlerin nedenini söyler, 75'te çizimlerin sonu (dışarıda tek robot, robot ve kız el ele) bu cümleyle anlam kazanır.

## Bölüm bölüm

Sol sayısı iki bölümde bir artar (Bölge 7 sol 1.092'de bitti). Etiket hepsinde **"BÖLGE 8 · İSTASYON"** ("ARAŞTIRMA İSTASYONU" telefonda taşabilir; uzun ad açılış satırında).

| Bölüm | Giriş damgası | Bitiş damgası | Olan |
|---|---|---|---|
| 71 İstasyon kapısı | Sol 1.093 · Araştırma istasyonu | Kapıda: koloni arması · Veri kartı: 2 | Kanyonun ucunda istasyon; kapıda koloninin arması. İlk `return`. |
| 72 Bilgisayarlar uyanıyor | Sol 1.093 · Bilgisayarlar uyanıyor | Uyanan bilgisayar: 6 · Son açılış: sol 861 | İlk ekran bulmacası. |
| 73 Duvardaki çizimler | Sol 1.094 · Duvarda çizimler | Onarılan panel: 2 · Çizim: 3 | Çizimlerin ilk yarısı: koloni, fırtına bulutu, yeraltına inen insanlar. |
| 74 Kayıt 4 | Sol 1.094 · Kayıt 4 | Toplam güç: 530 | Bölüm başında Kayıt 4. |
| 75 Dışarıda tek robot | Sol 1.095 · Dışarıda tek robot | Çizim: 5 · Sonuncusu: robot ve kız | Çizimlerin ikinci yarısı: dışarıda tek başına robot; sonuncusu robot ve kız el ele (göğüsteki çizimin aynısı). Rota çizimin altındaki yazı. |
| 76 Katlanmış çizim | Sol 1.095 · Yatağın yanında | Bulundu: katlanmış çizim · Açılmadı | Ece'nin yatağının yanında katlanmış çizim → çantaya. İlk okuyan robot. |
| 77 24 kişilik liste | Sol 1.096 · Sığınak listesi | Kişi: 24 · Aras: 2 | Ekranda 24 kişi; oyuncu Aras'ları bulur. |
| 78 Listenin altı | Sol 1.096 · Listenin altı | BKM-7: dışarıda · Görev: KAPI | Liste 26 satıra uzar; oyuncunun kodu BKM-7 satırını vurgular. **Açıklama anı.** |
| 79 Hizmet dışı | Sol 1.097 · Hizmet dışı | KOR: 12 hafta çalıştı · Yer: hangar | Kor'un iş kaydı (M maden, T taşıma, A arıza). |
| 80 İstasyon haritası | Sol 1.097 · İstasyon haritası | Oklar: tuz gölünün ötesi · Sonraki: tuz gölü | Final: haritadaki rota; oklar tuz gölünün ötesini gösteriyor. |

Başlıklarda "BKM-7" yok: 78'in başlığı "Listenin altı", açıklama oyuncunun kendi koduyla gelsin.

## 24 kişilik liste (77-78)

Satır biçimi: `<Ad Soyad> — kapsül: hazır`; 78'de altına `BKM-7   — dışarıda — görev: KAPI` ve `KOR     — hizmet dışı — hangar`. Liste sığınağa **iniş sırası**: **Ece ilk, Defne en son.** Çocuk önce iner, komutan en son; ilk giren en derindeki kapsüle girer, en son o uyanır (Perde 4: "son uyanan Ece"). Kimse söylemez.

İsimler ve görevleri (görevler oyunda yazmaz; yazarlar için karakter tohumu, Perde 4'te uyananlar): `hikaye-kitabi.md` "Yan karakterler ve adlar". Dört evli çift (Ekinci, Demir, Kaya, Bulut); başka çocuk yok (Ece koloninin tek çocuğu).

**Bulmaca kuralı:** "Aras" (ve küçük harfle "aras") yalnız Defne ile Ece'nin satırında, "dışarıda" yalnız BKM-7 satırında, "dış" yalnız BKM-7 ile KOR satırında geçer. 77'nin görev kartında görünen satırlar Ece (Aras) ve Kerem Yalın (Aras değil); Defne listenin sonunda gizli kalır, ezberleyen oyuncu onu kaçırır.

## İzler ve gizli ayrıntılar

| Bölüm | İz |
|---|---|
| 72 | "Son açılış: sol 861": istasyon bilgisayarları en son göç günü açılmış. Sol 861 oyuncunun Bölge 3'ten (boy çizgisi, takvim), Bölge 6'dan (son boya ok) ve 70'teki dokuz çizgili pastadan bildiği gün. Kimse bağlamaz. |
| 73, 75 | Duvar çizimleri **turuncu**: Ece'nin turkuaz kalemi Bölge 7'de (dönüş yolunda) düştü; o gece annesinin yol işaretlediği turuncu kalemle çizdi. Kimse söylemez. Çizimler çocuk eliyle ince çizgi; Bölge 6'nın boya oklarından (kalın, akmış boya) ayırt edilir. Turkuaz kuralı bozulmaz: turkuaz yalnız Ece'nin kendi kalemi. |
| 75 | Sonuncu çizim (robot ve kız el ele) göğüsteki çizimin aynı biçimi; renk turuncu. |
| 76 | Katlanmış çizim açılmaz ("Açılmadı"): Kıvılcım onu Ece'ye götürüyor. İçi Perde 4 finalinde açılır. |
| 77-78 | Ece ilk, Defne en son (yukarıda). Listede başka "Aras" yok: Ece'nin babası kolonide değil; nedeni Sezon 1'de söylenmez (açık soru, Ragıp). |
| 79 | Kor'un kaydı `MMTMMTMTMMTMAAAAAA`: sondaki A'lar (arıza) hizmet dışı kalışı; Perde 3'teki "huysuz ama korumacı" Kor'un ilk izi. |

## Çelişki kontrolü

- Ses kaydı yalnızca Defne; öğrenciye ("sen") konuşur. Kayıt 4 Ece'den söz eder ama dinleyen öğrencidir. ✔
- Robotlar konuşmaz: damgalar ve istasyon ekranı (makine yazısı) konuşma değildir. ✔
- Kimse ölmez. ✔
- Ece'nin rengi turkuaz; istasyondaki turuncu çizimler kalemin düşmesiyle açıklanır (Bölge 7 dönüş izi). ✔
- **Sol 861 = Ece'nin 9. doğum günü = göçün başladığı gün** (Ragıp, 2026-11-14). Aynı gün: boy çizgisi (sabah), son boya ok, yan kanyon duvarı, gece istasyon. Sığınağa iniş birkaç sol sonra (kesin gün söylenmez). Eski "861 = sığınağa iniş" yazıları düzeltildi (`hikaye-kitabi.md`, `senaryo-bolge-03.md`, `senaryo-bolge-05.md`). Kapı kodu yine 861. ✔
- Koloni 24 kişi = 24 satır; BKM-7 ve KOR insan değil, listenin altında. 63'teki göç izi "23 büyük, 1 küçük" ile aynı. ✔
- Çanta eşyaları sırası: kurdele (4), kalem (7, Bölüm 68), katlanmış çizim (8, Bölüm 76). ✔

## Oyuna konanlar (2026-11-14, Ragıp)

- `bolum-71..80.json`: `etiket`, `giris`, `bitis`; 77-78'de 24 ismin son hâli ve iniş sırası (77'nin görev kartında Ece + Kerem, tipik hata Ece'nin satırıyla).
- `Recordings`: Kayıt 4 (`cizimler.ses`, Bölge 8, Bölüm 74 başı); `motor-test/RecordingTests.cs`.
- `StationTraces.cs` (yeni): Bölge 8 açılış satırları (görünüş 3a'da buraya eklenir); `Traces.TransitionLines`.
