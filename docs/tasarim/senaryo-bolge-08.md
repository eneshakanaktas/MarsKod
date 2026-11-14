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

## Oyuna konanlar: görünüş 3a (2026-11-14, Ragıp; plan `docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md`)

- **Ortam (Görev 1):** Bölge 8 kendi bölge numarası (`_Region = 6`), `_StationScene` 0 = 71 dışarısı, 1 = 72-80 içerisi. `StationSky.hlsl` (71: kanyonun ucu, öğleden sonra; içeride koyu tavan), `Ground.shader` `stationFloor` (metal plaka + toz; 71'de kum + beton; içeride düz zemin), `RegionLook.StationDoor/StationDim/Station` (`ForLevel`: 72-73 loş, 74-80 aydınlık), `StationRoom` (kapalı oda: bantlı arka duvar, yan duvarlar, tavan kirişleri, 3 lamba + acil ışık, `LightPool`), `StationFacade` (71: üç prefabrik modül, yarı açık kapı), `StationFurniture` (engeller: dolap, masa + devrik sandalye, raf, kasa yığını). İçeride dış manzara gizli.
- **Nesneler (Görev 2):** `DataCard` (kartuş + sayaç simgesi), güç kutusu (`SolarPanel` + `PowerUnitLook`; davranış tek yerde, 10 ışık çubuğu), `StationProps` (76 iki kamp yatağı, 79 atölye + Kor'un bacağı `KorBacak`), `StationMap` (80: yazısız harita, Defne'nin turuncu okları).
- **Terminal, bilgisayarlar, not, lambalar (Görev 3):** `ScreenView` istasyon terminali (koyu cam, kehribar yazı, tarama çizgileri; alanın alt yarısında; `True` satırı kehribar zemin). 72/77/78'de `R` üst sıranın ortasında, Kıvılcım bilgisayarlara dönük. `StationScreens` (72: 6 masa + eski bilgisayar, her doğru cevapta biri kehribar uyanır; 77-78: duvar terminali, 78'de BKM-7 satırında bir an turuncu). `NoteView` (kâğıt kartı, okunan harf turuncu ve altı çizili; başlığın altındaki koyu bantta, bölüm bitince kapanır), `Robot.HoldNote` (kâğıdı kaldırır; 80'de önce haritaya bakar). `NoteEnd` (notun sonunda hedef; 76'da katlanmış çizim Kıvılcım'ın ayağının dibinde kumdan yükselir), ortak `ArrivalMark` (Bölge 7 `TrailView` da kullanır). `StationLamps` (73 bitince lambalar kırpışarak yanar, ışık aydınlığa geçer, acil ışık söner; Tekrar'da loşa döner; `RegionLook.BlendTo`).
- **Arma, çizimler, sırttaki kâğıt (Görev 4):** `ColonyCrest` (turuncu kenarlı krem daire, kubbe, yıldız; soluk, kumla aşınmış; kapı modülünün çatısında). `WallDrawings` (73'te 3, 75'te 5: koloni, fırtına, yeraltına inen 24 çöp adam — yerde iki sıra 18 + merdivende 6, en alttaki küçük Ece —, dışarıda tek robot, el ele kız + robot ve altında `KKDDDKKB`); turuncu kalem `CrayonMarks.OrangePaint` (Bölge 6 boya turuncusu). Kız + robot ölçüleri `ChildDrawing`'te ortak sabit (göğüsteki doku piksel piksel aynı kaldı). `BrushPaint`'e `D`. `FoldedDrawing` + `Robot.SetFoldedDrawing` (77'den itibaren sırtta, kayışın altında).
- **Denetim (Görev 5):** `-sahneler -bolge8` (`Oyun.IstasyonDenetimi.cs`; tam `-sahneler`'in sonunda da çalışır): 71 iki veri kartı + arma çatının içinde; 72 altı bilgisayar uyanır, Tekrar'da söner; 73/75 çizim sayısı (3/5), 24 çöp adam, çizimler duvar bandında; 73 lambalar yanar, Tekrar'da loşa döner; 76 üç not sürümünde kart son harfe kadar ilerler, katlanmış çizim son karede ve yere basıyor, Tekrar'da kalkar; 77 sırtta kâğıt; 78 terminal bir kez turuncu; 79 Kor'un bacağı; 80 harita duvarda; 71-80'de alan dışı eşyalar yere basar ve karelere girmez, engel eşyası kendi karesinde. Bitiş damgalarındaki sayılar sahneyle eşleşir.
- **Görüntüde verilen kararlar (plan dışı):** not kartı planda başlığın hemen altındaydı, 76'da yatakları ve 80'de haritayı kapatıyordu → başlığın altındaki koyu banda; katlanmış çizim planda karenin yatak tarafındaydı, Kıvılcım'ın arkasında kalıyordu → ayağının dibine; arma planda kapının üstünde ön yüzdeydi (orada ~7 cm, okunmuyordu) → kapı modülünün çatısına (koloni iniş araçları görsün diye boyamış; kimse söylemez); arma yıldızı kremde krem görünmüyordu → turuncu; 24 çöp adam tek sırada zikzak gibi okunuyordu → iki sıra; sırttaki kâğıt çantanın altında kalemin arkasında kalıyordu → çantanın üst kenarı (soldan: çizim, kalem, kurdele).
- Görüntüler: `docs/tasarim/bolge-8-gorunus-2026-11-14/`.

## Oyuna konanlar: 3b sahneler (2026-11-14, Ragıp; plan `docs/superpowers/plans/2026-11-14-bolge-8-sahneler-3b.md`)

- **76 (K3, `StationScenes.TakeDrawing`, ~8 sn):** Kıvılcım ayağının dibindeki katlanmış çizime döner, eğilip alır, kâğıdı göğsünün önüne getirir (iki eli kenarlarında), üst kat menteşesinden biraz aralanır (`FoldedDrawing.SetOpen`; içi görünmez, yalnız turuncu çizgi ucu uzar), durur. Ece'nin yatağına döner (`StationTraces.EceBed`), bakar, kâğıdı katlar, omzunun üstünden sırtına koyar. Kimse söylemez: "Açılmadı" damgası anlam kazanır.
- **78 (K2, `StationScenes.Reveal`, ~7 sn):** Satırlar düşerken BKM-7 satırında akış durur, terminal turuncu kalır (`StationScreens.HoldHighlight`, önüne turuncu ışık), kamera Kıvılcım'ın yan profiline yaklaşır; Kıvılcım donar, gözleri kısılır, anteni söner, başını göğsündeki çizime eğer (`Robot.Realize`), sonra ekrana kaldırır. KOR düşer; sevinç dansı yok, yumuşak iki notalı bitiş sesi (`Sound.Quiet`). Yalnız çözüm tam doğruysa oynar.
- **80 (K1, `StationScenes.Closing`, ~14 sn):** Kıvılcım haritaya döner (kamera omzunun arkasından) → kırpışma → 75'in koridoru (`CorridorSet`, aynı oda ve 5 çizim; 80'in eşyaları gizli): son çizimin (kız + robot el ele) önünde, sırtında katlı çizim; terminal kutusunda tek satır `BKM-7   — dışarıda — görev: KAPI` (`Hud.ShowRecall`) → kırpışma → harita, Kıvılcım başını oklara (okların ötesine) kaldırır (`Robot.RaiseHeadTo`). Son kare kalır; dans önce oynar.
- **Atla (K4):** `SceneSkip` + `SkipButton`: sahne başladıktan 1 sn sonra sağ üstte (pusulanın altında) "Atla", üstünde kalan saniye; atlanınca son hâl kurulur (76: çizim sırtta; 78: kalan satırlar düşer, sessiz bitiş; 80: son kare). 70'in eski kuralı (ilk izleyişten sonra dokunma katmanı) aynı sınıfa taşındı; Görev 5'te 70 ve eski sahneler de düğmeye geçecek. **Plandan sapma:** düğme planda "altta"ydı; altta 78'in ekran kutusu ve 80'in hatıra kutusu olduğu için sağ üste alındı.
- **Denetim:** `-sahneler -bolge8` (76 üç not sürümü, kâğıt gövdeye girmez, yatağa ±25°, sonda sırtta; 78'de anlama anında 25 satır, sonda 26, bir kez turuncu, kutlama yok, yanlış kodda sahne yok, klavye açıkken kamera sıçramaz, 77'ye sızmaz; 80 üç not sürümünde sona varılır, koridorda 5 çizim + harita gizli + sırtta 3 eşya, hatıra satırı bolum-78.json ile aynı, sonda baş kalkık; üç sahnede atlama ve sahne ortasında Tekrar).
- Görüntüler: `docs/tasarim/bolge-8-sahneler-2026-11-14/`.
