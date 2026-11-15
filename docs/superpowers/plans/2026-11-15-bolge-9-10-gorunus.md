# Bölge 9-10 Görünüşü — 3a (ortam, nesneler) + 3b (sahneler) · Yapım Planı

> 2026-11-15 · Claude (Opus 5.5); Ragıp "durmadan çalış, kararları sen ver, sonra söyle" dedi. Kod yazılmadı (bu oturumda Unity yoktu: görüntüye bakılamadan görünüş kodlanmaz). Kararların hepsi **Claude'un**; Ragıp görüntüde ya da okurken değiştirebilir.
> **Model · efor: Opus 5.5 · high** (görsel/3B iş). Süre tahmini: Bölge 9 · 3a ~4 sa, Bölge 9 · 3b ~2 sa, Bölge 10 · 3a ~4-5 sa, Bölge 10 · 3b (100 finali dahil) ~3-4 sa. Her biri ayrı oturum.
> **Ara görüntü kuralı (Bölge 8'deki gibi):** her görevin sonunda Ragıp'a görüntü; Ragıp onaylamadan sonraki göreve geçilmez.

**Dayandığı belgeler:** spec `docs/superpowers/specs/2026-11-09-bolge-8-10-ders-bulmaca-design.md` (karar 7: "9 bembeyaz, gökyüzünü yansıtan göl; 10 gece + meteorlar" ve **"Görsel tutarlılık" 1-7**), `docs/tasarim/senaryo-bolge-09.md`, `docs/tasarim/senaryo-bolge-10.md`, `docs/tasarim/senaryo-perde-2.md` (Bölge 9-10, Bölüm 100), bulmaca kayıtları `2026-11-15-bolge-9-bulmacalar.md`, `2026-11-15-bolge-10-bulmacalar.md`.

**Şu anki geçici hâl (bulmaca parçasında kuruldu):** 81-100 iniş ovasının gökyüzü ve ışığında; tuz kristali ve göktaşı buz modelinde, sayaçta buz simgesi; engel Bölge 1 kayası; krater ve kızgın taş `MeteorMarks.cs`'teki basit modeller; liste notu kâğıt kartta satır satır (`NoteView`); 99'un ekranı istasyon terminali, başlığı "ANTEN".

**Yapı (önceki bölgeler gibi):** Bölge 9 `_Region = 7`, Bölge 10 `_Region = 8`. Bölge 1-8'in kodu değişmez. Her bölge: arka plan `SaltSky.hlsl` / `NightSky.hlsl`, zemin `Ground.shader`'da `saltFlat` / `nightPlain` (masaüstü ve hafif telefon dalı ikisi de), ışık `RegionLook.SaltLake` (+ `SaltLakeDusk`) / `RegionLook.NightPlain`, izler `SaltLakeTraces.Build` / `NightPlainTraces.Build` (bugün yalnız geçiş satırları var). Oynarken olanlar ayrı sınıflarda; `Oyun.cs`'e yalnız çağrı satırları.

---

## Renk paleti (Ragıp, 2026-11-15: "daha önce kullanılmamış, benzeri de olmayan renkler; hikâyeye uydur")

**Önceki bölgelerde kullanılanlar (Bölge 9-10'da kullanılmaz, benzeri de):** lavanta/periwinkle gök + turuncu-şeftali şafak (1, 3), mavi (2), sıcak şeftali-bej (4-6, 71), gri-leylak (7), soğuk beyaz + kehribar (8 iç). **Hikâyeye ayrılmış renkler:** turkuaz yalnız Ece'nin kalemi; turuncu yalnız Defne'nin boya okları; kırmızı tehlikeli kristal; 76'daki gizli yıldızın mavisi (`#5B8FE0`) Dünya'nın rengi.

### Bölge 9 — Tuz gölü: sedef beyazı, kükürt sarısı, şampanya altını

| Ne | Renk | Neden (hikâye) |
|---|---|---|
| Tuz kabuğu (zemin) | sedef `#F4F1E8`, çatlak çizgileri `#D9D2B8` | Kurumuş gölün tuzu. |
| Kabuktaki mineral bantları | kükürt-limon `#E8D24A`, koyusu `#C9B23A` | Göl kururken dibe çöken kükürtlü tuzlar (Mars'ta gerçekten var: sarı sülfat tuzları, jarosit). Önceki bölgelerde hiç sarı zemin yok. |
| Gökyüzü ve zemindeki yansıması | şampanya-altın `#F2E2A8`, ufukta `#FFF6D6` | Ufuk kaybolur: gök ile yer aynı açık altına karışır (82). Kum tepelerinin şeftalisinden ayrı: sarıya çalar, pembeye değil. |
| Gölge ve ortam ışığı | zeytin-gri `#7E7D5C`, yer `#34331F` | Önceki bölgelerin gölgesi mor ya da kahve; burada yeşilimsi zeytin. |
| Tuz kristali | sedef, içinde altın parıltı `#FFF3B0` | Ölçülen kristal ışığı tutar. |
| 90 gün batımı | altın `#F5C842` → fuşya alacakaranlık `#C2307A` → mürdüm | Bölge 9'un altını Bölge 10'un gecesine bağlanır; Mars'ın mavi gün batımı (Bölge 2) tekrarlanmaz. |

Kayıt 5'in tonu ("Acele etme, ama durma da"): sıcak, umutlu bir gündüz; altın bunu taşır.

### Bölge 10 — Gece ovası: mürdüm gece, zümrüt yeşili ışık, akkor fuşya kor, tek mavi nokta

| Ne | Renk | Neden (hikâye) |
|---|---|---|
| Gece gökyüzü | mürdüm-siyah `#1E0F24`, ufukta `#3A1840` | Bölge 2'nin mavisinden ve 1-3'ün lavantasından ayrı: kırmızımsı koyu mor. |
| Kutup ışığı perdesi | zümrüt `#2EE86A`, soluk `#18B850` | Mars'ta gerçekten yeşil kutup ışığı görülür (2024'te Perseverance yüzeyden görünür ışıkta çekti). Oyundaki ilk yeşil. |
| Meteor izleri | baş `#B8FFD0`, kuyruk `#2EE86A` | Meteorlar magnezyum/nikel yanarken yeşil parlar; aynı gökyüzünün parçası. |
| Zemin (bazalt) | koyu mürdüm-gri `#2A2030` | Yıldız ve aurora ışığında. |
| Göktaşı | grafit `#2B2B2E`, yüzeyinde zümrüt parıltı | Meteorun yeşili taşta kalır: oyuncu taşı gökyüzündeki izle bağlar. |
| Krater koru ve kızgın taş | akkor çekirdek `#FFF0F6`, fuşya hale `#FF3FA4` | Turuncu Defne'nin oklarına ayrıldığı için kor turuncu değil: beyaz-sıcak çekirdek, fuşya hale. Tehlike rengi kırmızı kristalden de ayrı. |
| Mavi nokta (Dünya) | `#5B8FE0` | **Bütün bölgede gökteki tek mavi şey.** 76'da katlanmış çizimin köşesindeki gizli yıldızla aynı mavi; finalde Ece'nin çizdiği yıldız. Oyuncunun gözü ona gider. |
| Kıvılcım'ın lambası | sıcak beyaz | Robotun kendi ışığı; renkleri bozmaz. |

## Yapılanlar: ortamın ilk hâli (2026-11-15, Ragıp "renkler arka planda ve temada olsun" dedi)

Görev A1 ve C1'in ilk hâli kodlandı, Windows paketinde görüntüyle bakıldı (`docs/tasarim/bolge-9-10-renk-2026-11-15/`):
- **Bölge 9:** `SaltSky.hlsl` (şampanya-altın gök, kaybolan ufuk, gölde gökyüzü ve volkanın ters yansıması, volkan 81→90 yaklaşır: `_SaltScene`, `SaltLakeTraces.ResetBackdrop`), `Ground.shader` `saltFlat` (sedef kabuk, çokgen çatlak sırtları, kükürt sarısı şeritler; telefon dalı sade), `RegionLook.SaltLake` (altın-beyaz ışık, güneş solda yüksek, zeytin ortam), uzak kayalar tuz kabuklu, engel sedef tuz yığını, kristal sedef + altın parıltı, sayaçta altın simge.
- **Bölge 10:** `NightSky.hlsl` (mürdüm gece, sık yıldız ve Samanyolu şeridi, zümrüt kutup ışığı perdesi, aynı yönden düşen yeşil meteorlar, mavi nokta Dünya 93'ten parlak, volkan silueti ve 97'den itibaren eteğinde kapı: `_NightScene`), `nightPlain` (mürdüm bazalt, yeşil meteor tozu), `RegionLook.NightPlain` (zümrüt üst ışık, mürdüm ortam, güneş yok), engel mürdüm bazalt, krater koyu çanak + küçük akkor-fuşya kor, göktaşı grafit + zümrüt parıltı, sayaçta zümrüt simge.
- **Kalan (bu planın görevleri):** asıl kristal/göktaşı modelleri, 84 tuz halkalı ok, ayak izleri, yansıma lekeleri, 86 astronot, kraterin dumanı ve iniş çizgisi, Kıvılcım'ın lambasının gece güçlenmesi, 3b sahneleri, `-bolge9`/`-bolge10` denetimleri.

## Kararlar (Claude, 2026-11-15)

### Bölge 9 — tuz gölü

| # | Konu | Karar | Neden |
|---|---|---|---|
| T1 | Zemin | Sedef beyazı tuz kabuğu, büyük altıgen-çokgen çatlak deseni (gerçek tuz göllerindeki gibi), kenarları hafif kabarık; aralarda kükürt sarısı mineral bantları (paletteki renkler). Alanın dışında ufka doğru renk gökyüzüne karışır: **ufuk kaybolur** (82). | Spec: "bembeyaz, gökyüzünü yansıtan". Kanyondan ve istasyondan bilerek farklı. |
| T2 | Yansıma | **Gerçek yansıma yok** (telefona pahalı). Zemin hafif parlak; her nesnenin (robot, kristal, engel) altında **ters, soluk, kısa bir yansıma lekesi** (decal, nesneyle aynı yerde). | Spec görsel tutarlılık 1: "yansıma nesneyle aynı yerde, ters". Ucuz ve okunur. |
| T3 | Gökyüzü | Şampanya-altın gündüz (pembe-bej değil: o kum tepelerinin rengine yakın); ufuk çizgisi silik. Ufukta **volkan silueti** (81'de küçük, 88'de belirgin, 90'da büyük ve gün batımında). | Senaryo: oklar volkanı gösterir; 88 "Volkan belirginleşir". |
| T4 | Işık | Gündüz, yüksek, altın-beyaz; gölgeler kısa, zeytin tonunda. 89-90 öğleden sonra; **90 gün batımı: altın → fuşya → mürdüm** (`SaltLakeDusk`). Gölgeler güneşin yönüyle aynı (spec 3). | Spec: "Bölge 9 gündüzden gün batımına (90)". |
| T5 | Tuz kristali | Sedef, yarı saydam, içinde altın parıltı; birkaç kübik kristalin kümesi; **boyu `kristal_boyu`ya göre** (3 cm küçük, 12 cm iri): oyuncu ölçtüğü boyu gözüyle de görür. Sayaç simgesi: küçük sedef küp, altın kenarlı. | Ölçümle görüntü aynı şeyi söylesin (spec 6). |
| T6 | Kırmızı kristal (88) | Bölge 2'nin kırmızı kristali aynen (`Ice.CreateHazard`); tuz beyazı üstünde zaten çok okunur. | Kural aynı, görünüş aynı. |
| T7 | Engel | Tuz yığını (beyaz, kabarık, yanında küçük parça). | "Buradan geçilmez" okunsun; renk bölgenin. |
| T8 | Oklar | Bölge 6'nın turuncu boya okları (`PaintMarks.Arrow`) tuz üstünde; **84'te ilk ok tuzdan bir halkayla çevrili**. Okların yanında rüzgârın silmediği birkaç ayak izi (kırık kabuk gibi, kum gibi değil: spec 4). | Senaryo-perde-2 "biri okları korumuş". |
| T9 | Oyuncak astronot (86) | Taranan bloğun boş karelerinden birinde (2,1) yarı gömülü; robot o kareden geçince tuzdan çıkar (3b mini sahne). Beyaz, kask camı Dünya mavisi (`#5B8FE0`). Sırt çantasında solmuş **"K.A."** (Ragıp onayladı, 2026-11-15): Ece'nin babası Kaan Aras'ın hediyesi; oyunda açıklanmaz. | Çanta sırası: 4. eşya. |
| T10 | Sırttaki eşyalar | 86'dan sonra astronot da sırtta (kurdele, kalem, katlanmış çizim, astronot); `BackSlot` ortak. | Spec 6. |

### Bölge 10 — gece ovası

| # | Konu | Karar | Neden |
|---|---|---|---|
| G1 | Gece | **Mürdüm** gökyüzü (lacivert değil), çok yıldız, ufukta **zümrüt yeşili kutup ışığı perdesi**; **yeşil meteor çizgileri** aynı genel yönden düşer (spec 4); performans modunda az. | Spec: "10 gece + meteorlar". |
| G2 | Işık | Güneş yok (gündüz gölgesi yok: spec 3). Soluk zümrüt kutup ışığı (yukarıdan, gölgesiz) + mürdüm ortam + **Kıvılcım'ın lambası** (robotun önüne düşen sıcak ışık) + kraterlerin fuşya kor ışığı. Telefonda okunurluk için ortam ışığı düşük ama sıfır değil. | Spec 3; telefonda karanlık sahne okunmazsa oyun oynanmaz. |
| G3 | Mavi nokta | Gökyüzünde sabit yerde, parlak tek nokta: **Dünya** (`#5B8FE0`; bölgede gökteki tek mavi). 93'ten itibaren hafifçe parlar (fark edilir). | Senaryo: 93 "mavi bir nokta", 94 bakış. |
| G4 | Volkan + kapı | Ufukta volkan (Bölge 9'dakiyle aynı biçim, daha yakın). **97'de volkanın eteğinde küçük, dikdörtgen metal kapı silueti** (ışıksız, yalnız biçim). | Senaryo 97; Perde 3'ün hedefi. |
| G5 | Krater | Koyu çanak + kenarında atılmış toprak halkası + dibinde **akkor-fuşya** kor + **ince duman** (rüzgârla aynı yöne eğik; spec 4). Her "Çalıştır"da düzen değişince yeni kraterlere **yeşil bir meteor çizgisi iner** (kısa, 0,4 sn): krater oraya o anda düşmüş olur, önceden orada değildi (spec 4). | `meteorlar`: her denemede başka yere düşer. |
| G6 | Göktaşı | Grafit, yuvarlakça taş, yüzeyinde zümrüt parıltı (meteorun yeşili); sayaçta küçük grafit taş, yeşil noktalı. | Bölge nesnesi. |
| G7 | Kızgın taş | Göktaşının aynısı + akkor-fuşya çatlaklar + yukarı yükselen ince duman (krater dumanıyla aynı rüzgâr). `smoke_here()` taraması dumanı biraz büyütür. | Kural: toplanmaz; görünüş "dokunma" der. |
| G8 | Engel | Koyu bazalt kayası (gece rengi). | — |
| G9 | 99 ANTEN | Ekran düzeni aynı (`ScreenView`), başlık "ANTEN · gercek_mi(...)"; gerçek kodlar (True) kehribar zeminde vurgulu (77-78 gibi). Sahnede Kıvılcım küçük bir anten direğinin önünde. | Spec: "99 aynı ekran düzeni, çerçevesi anten/telsiz". |

### 3b sahneleri (ikisi de "Atla" kuralıyla: sağ üstte düğme + kalan saniye; son hâl yöntemiyle)

| Bölüm | Sahne | Karar |
|---|---|---|
| 86 | Astronotun alınışı | Kıvılcım astronotun karesine döner, eğilir, tuzdan çıkarır, bir an elinde tutar (kask camında gökyüzü), sırtına koyar (`PickUp` + `StowOnBack`, 68/76 ile ortak). ~5 sn. |
| 90 | Bölge 9 kapanışı | Güneş batar: göl altından fuşyaya, sonra mürdüme döner; ilk yıldızlar ve ufukta ilk soluk yeşil kutup ışığı; volkanın siluetinin üstünde **küçük bir parıltı** (Bölge 10'un mavi noktasına hazırlık; aynı yerde değil, volkanın üstünde). Kamera yavaş geri çekilir. ~6 sn. |
| 94 | Mavi nokta | Bölüm bitince Kıvılcım mavi noktaya döner, başını kaldırır, **uzun süre** bakar (dans yok, 78 gibi sessiz bitiş). Kamera arkasından, nokta ekranın üst üçte birinde. ~5 sn. |
| 99 | Sinyal | Doğru kod çalışınca ekranda gerçek üç kod vurgulanır, kutu kapanır, anten direğinin ışığı yeşil bir kez yanar. Kısa (~2 sn), düğmesiz. |
| **100** | **Sezon 1 finali: iki saat** | (1) Kırmızı makine yazısı: `SIĞINAK — KAPSÜL ENERJİSİ: %31` / `KRİTİK EŞİK: 101 SOL SONRA`. (2) Sarı: `GÜNEŞ KAVUŞUMU: 94 SOL SONRA` / `DÜNYA BAĞLANTISI 14 GÜN KESİLECEK`. (3) Başlıkta ilk kez **sinyal göstergesi** (dolu; kalıcı, Sezon 2'de zayıflayacak). (4) Kıvılcım mavi noktaya, sonra volkana bakar; anten ışığını **bir kez** yakar. (5) **"SEZON 1 — SON" kartı (Ragıp onayladı, 2026-11-15):** oyuncunun 100 bölümde öğrendiği konular kısa bir liste (komutlar, `for`, `if`, `while`, değişken, rutin, `return`, metin, liste, iç içe döngü, `and/or/not`) ve "Kıvılcım seni bekliyor. Sezon 2 yakında." ~12 sn + kart. |

---

## Görevler

### Bölge 9 · 3a (bir oturum)

**Görev A1 — Ortam:** `_Region = 7`; `SaltSky.hlsl` (gündüz gökyüzü, silik ufuk, volkan silueti boyu bölüme göre: global `_VolcanoNear` 81→0,2 · 88→0,6 · 90→0,8); `Ground.shader` `saltFlat` (çokgen çatlaklar, ufka karışma; telefon dalı sade); `RegionLook.SaltLake` ve `SaltLakeDusk`; `RegionLook.ForLevel` 89-90 öğleden sonra. **Denetim:** 81, 85, 88, 90'dan görüntü (masaüstü + `-performans ac`).
**Görev A2 — Nesneler:** `SaltCrystal` (boyla ölçekli, `Pickups.Create`'e bir satır), sayaç simgesi (`Hud.DrawSaltDot`), `Obstacles.SaltMound`, yansıma lekesi (`ReflectionDecal`: robot, kristal, engel, kırmızı kristal altına), 84 tuz halkalı ok + ayak izleri (`SaltLakeTraces.Build`), 86 gömülü astronot (kurulur, görünmez değil: yarısı tuzda). **Denetim:** her nesne yere basıyor, alandan taşmıyor (spec 1-2), 84'te halka ilk okun çevresinde.
**Görev A3 — Denetim ve belgeler:** `-sahneler -bolge9` (yeni denetim dosyası `Oyun.TuzGoluDenetimi.cs`: kristal boyu ölçüsü, yansıma nesnenin altında, oklar alan dışında, astronot 86'da var 85'te yok), `senaryo-bolge-09.md` "Oyuna konanlar: 3a", `CLAUDE.md`'ye `-bolge9` satırı.

### Bölge 9 · 3b (bir oturum)

**Görev B1:** 86 astronotun alınışı (`SaltLakeScenes.TakeAstronaut` + `FinishTakeAstronaut`, `SceneSkip.Run`), sırtta 4 eşya. **Görev B2:** 90 kapanışı (`RegionLook.BlendTo(SaltLakeDusk → gece)`, yıldızlar, volkan üstünde parıltı). **Görev B3:** `-sahneler -bolge9` sahne kareleri + "Atla" denetimi + belgeler.

### Bölge 10 · 3a (bir oturum)

**Görev C1 — Ortam:** `_Region = 8`; `NightSky.hlsl` (yıldızlar, Samanyolu, meteor çizgileri — global `_MeteorRate`, performans modunda düşük; mavi nokta sabit yönde; volkan; 97'de kapı silueti `_DoorSilhouette`); `Ground.shader` `nightPlain`; `RegionLook.NightPlain` (güneş 0); Kıvılcım'ın lambası (`SparkBot` üstünde, yalnız Bölge 10'da açık). **Denetim:** telefon boyunda okunurluk (91, 95, 98 görüntüsü; parlaklık ölçüsü: robot ve göktaşı arka plandan ayrılıyor mu).
**Görev C2 — Nesneler:** `MeteorMarks`'ın asıl hâli (krater çanağı, toprak halkası, kor, duman; düzen değişince iniş çizgisi), `MeteorStone` pickup + sayaç simgesi, kızgın taşın dumanı, bazalt engel. **Denetim:** düzen değişince kraterler yer değiştiriyor ve iniş çizgisi yeni kratere düşüyor; duman hepsinde aynı yöne eğik (spec 4).
**Görev C3 — 99 ANTEN** (direk + ekran vurgusu), denetim `-sahneler -bolge10` (`Oyun.GeceOvasiDenetimi.cs`), belgeler.

### Bölge 10 · 3b (bir oturum)

**Görev D1:** 94 mavi noktaya bakış. **Görev D2:** 100 finali (iki uyarı `Hud.ShowAlert(kırmızı/sarı)`, sinyal göstergesi `Hud.SignalMeter` kalıcı, bakış, tek ışık; `SceneSkip`). **Görev D3:** "SEZON 1 — SON" kartı (`SeasonEndCard`; konular sözlükten değil sabit liste). **Görev D4:** `-sahneler -bolge10` + `-bolum 100 -final` benzeri tek bölüm turu; belgeler; telefon denemesi listesine Bölge 9-10.

---

## Görsel tutarlılık denetimi (her görevde; spec 1-7)

1. Kristal, göktaşı, kızgın taş, engel, astronot yere basar; tuzda yansıma lekesi nesnenin tam altında.
2. Liste notu kartı ve ANTEN ekranı taşmaz (99: 9 satır kayar).
3. Bölge 9 gündüz → 90 gün batımı, gölgeler güneşle aynı yönde; Bölge 10'da gündüz gölgesi yok.
4. Meteorlar aynı genel yönden; krater düzen değişince "yeni düşmüş" görünür; duman yukarı ve rüzgârla aynı yöne.
5. Astronot oyuncak boyunda (Kıvılcım'ın elinde); kristal boyu ölçüyle orantılı.
6. Damgalarla görünüş aynı: 88'de 4 kırmızı kristal, 95'te 3 kızgın taş, 96'da 2+2.
7. Robot, kraterin ya da kızgın taşın içinden "geçiyor" görünmez (krater kuralı zaten girmez; kızgın taşın üstünden geçerken robot taşın içine gömülmez).

## Kapsam dışı

- Ses (meteor vınlaması, rüzgâr) — ileride.
- Bölge 11+ (Sezon 2).
- Bilgi kartlarının metni (bulmaca parçasında yazıldı).

## Ragıp'ın cevapları (2026-11-15)

- 86 astronotta "K.A.": **olsun.** 100'den sonra "SEZON 1 — SON" kartı: **olsun.** 99'daki `BKM7`, `ARAS-ECE` gürültü kodları: **fazla açık değil, kalsın.**
- **Renkler:** Bölge 9-10 önceki bölgelerde kullanılmamış, benzeri de olmayan renklerle; hikâyeye uydurulmuş (yukarıda "Renk paleti").

## Açık soru

- Bölge 10'da karanlık: telefonda okunurluk için ova biraz aydınlık mı olsun (önerilen: mürdüm ama zümrüt kutup ışığı zemini okunur kılar), yoksa gerçekçi koyu gece mi? Görüntüde karar verilir.
