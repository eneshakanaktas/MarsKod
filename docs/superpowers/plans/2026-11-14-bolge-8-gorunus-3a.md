# Bölge 8 Görünüşü — 3a (istasyon kapısı, iç mekân, ekran, not, duvar çizimleri, nesneler) · Uygulama Planı

> 2026-11-14 · Ragıp + Claude (Opus 5.5). Kod yazılmadı; bu plan yeni sohbette görev görev uygulanır.
> **Model · efor: Opus 5.5 · high** (görsel/3B iş: ölçü, yerleşim, ışık, "doğru görünüyor mu" kararı). Süre: ~6-7 sa (iki-üç oturum: Görev 1-2 / Görev 3-4 / Görev 5).
> **Ara görüntü kuralı:** her görevin sonunda Ragıp'a görüntü gösterilir; **Ragıp onaylamadan sonraki göreve geçilmez.** Beğenmezse aynı görevde düzeltilir. Yardımcı ajan yok, Claude işi kendisi yapar.

**Amaç:** Bölüm 71-80'in geçici görünüşünü gerçek görünüşle değiştirmek. Şu anki geçici hâl: kanyonun gökyüzü ve ışığı, engel yerine kasalar, enerji hücresi kılığında veri kartı, kod kartının üstünde sade bir ekran kutusu, robotun üstünde düz not yazısı, panellerin Bölge 3 güneş paneli görünüşü. Yerine gelecekler:
- 71: kanyonun ucunda istasyonun kapısı ve koloninin arması.
- 72-80: kapalı, terk edilmiş iç mekân. Işık lambalardan ve ekranlardan gelir.
- İstasyon terminali ve uyanan bilgisayarlar.
- Kâğıt not ve okunan harfin vurgusu.
- Ece'nin turuncu duvar çizimleri, Ece ile Defne'nin odası ve katlanmış çizim.
- Kor'un kırık bacağı, istasyon haritası.
- Veri kartı ve güç kutusu.

**Dayandığı belgeler:**
- Tasarım: `docs/superpowers/specs/2026-11-09-bolge-8-10-ders-bulmaca-design.md`, karar 7 ("8 kapalı mekân, ekranlar") ve **"Görsel tutarlılık" 1-7**. Bu planın her görevi bu yedi maddeye göre ölçülür.
- Hikâye: `docs/tasarim/senaryo-bolge-08.md` ("İzler ve gizli ayrıntılar", damgalar), `docs/tasarim/senaryo-perde-2.md` (Bölge 8), `docs/tasarim/hikaye-kitabi.md` (Kor, 24 isim).
- Geçici görünüşün yazıldığı yer: `docs/superpowers/plans/2026-11-09-bolge-8-bulmacalar.md` Görev 5, karar 12.

Bölge 8'in ayrı bir görünüş tasarım belgesi yok (Bölge 7'de vardı); görünüş kararları bu planın başında (K1-K4 Ragıp'ın, ötekiler planın önerisi).

**3b bu planda yok, ayrı plan.** 3b'nin sahneleri: 76 çizimin alınıp çantaya konması, 78 açıklama anı, 80 kapanış. 3a onların ihtiyacını hazır bırakır (en alttaki "3b için hazır bırakılanlar").

**Yapı:** Önceki bölgelerin düzeni korunur:
- Bölge 8 kendi bölge numarasını alır (`_Region = 6`). Bölge 6-7'nin kodu (`CanyonSky`, `RiverSky`, `canyonFloor`, `riverFloor`, `RegionLook.Canyon/RiverBed`) **değişmez**.
- İki manzara var: `_StationScene` 0 = kapı önü, dışarısı (71); 1 = iç mekân (72-80).
- Arka plan yeni `StationSky.hlsl`, zemin `Ground.shader` içinde yeni `stationFloor`, ışık `RegionLook.StationDoor` ve `RegionLook.Station`.
- Nesneler ayrı dosyalarda kodla çizilir; yerleşimi yalnız `StationTraces.Build` seçer.
- Oynarken olanlar iki yeni sınıfta: bilgisayarın uyanması `StationScreens`, notun okunması `NoteView`. `Oyun.cs`'e yalnız çağrı satırları girer.

**Teknik:** Unity 6 (6000.6.3f1, URP), C# 9, HLSL. `Motor/` değişmez. `Dunya/`'da yalnız 72, 77 ve 78'in haritasında robotun yeri değişir (K2), kural kodu değişmez.

---

## Kararlar

### Ragıp'ın seçtikleri (2026-11-14, dördü de önerildiği gibi)

| # | Soru | Karar |
|---|---|---|
| K1 | İç mekân (72-80) nasıl görünsün? | **Tam kapalı oda.** Arka duvar ekranın tepesine kadar uzanır, gökyüzü görünmez. Işık yalnız lambalardan ve ekranlardan gelir. 72-73 loş (acil ışık + ekranlar); **73 bitince** onarılan panellerle tavan lambaları kırpışarak yanar; 74-80 aydınlık. 71 dışarıda: kanyonun ucunda istasyonun kapısı ve arması. |
| K2 | Ekran bulmacası (72, 77, 78) nasıl görünsün? | **Terminal + sahnede bilgisayar.** Ekran kutusu istasyon terminaline benzer: koyu zemin, kehribar yazı, ince çerçeve, ince tarama çizgileri. Sahnede Kıvılcım'ın önünde gerçek bilgisayarlar durur. 72'de 6 bilgisayar var; her doğru cevapta biri uyanır, ekranı yanar. |
| K3 | Not (76, 80) nasıl okunsun? | **Kâğıt + harf vurgusu.** Kıvılcım küçük bir kâğıdı kaldırır. Harfler ekranın üstünde kâğıt renkli bir kartta görünür. Kıvılcım yürürken o an okuduğu harf vurgulanır: döngünün harf harf ilerlediği görünür. |
| K4 | 79'da Kor'dan iz olsun mu? | **Kor'un kırık bacağı.** Atölyede Kıvılcım'dan büyük, eski, ezik bir maden robotu bacağı durur (`hikaye-kitabi.md`: Kor'un bir bacağı aksak). Kimse söylemez; Kor'un ne kadar iri olduğunu önceden ima eder. Kor'un kendisi istasyonda **yok** (koloninin eski hangarında; Perde 3). |

### Planın verdiği kararlar (Ragıp'a sorulmadı; beğenmezse görüntüde değişir)

- **Arma (71):** daire içinde kubbe, kubbenin üstünde tek küçük yıldız (Dünya). Kapının üstünde soluk boyalı, yer yer kumla aşınmış. Renk krem + koloni turuncusu. Bölge 2'deki bayrağın "okunmayan amblemi" (Bölüm 16) **bu planda değişmez** (Bölge 2'ye dokunulmaz). İleride bayrağa ve sığınak kapısına aynı arma konabilir; açık soru (`senaryo-bolge-02.md` önerisiyle aynı).
- **Duvar çizimlerinin rengi:** Bölge 6'daki boya okların turuncusu (`PaintMarks`'taki renk; aynı sabit kullanılır, yeni renk yazılmaz). Ece annesinin yol işaretlediği kalemle çizdi. Çizgi Bölge 7'nin kalem çizgisi gibidir (ince, titrek, yer yer kesik: `HandTrace`/`BrushPaint` darbe kodu); Bölge 6'nın kalın, akmış boya okundan böyle ayrılır. Turkuaz yok (turkuaz kalem 68'de düştü).
- **Çizimlerin yüksekliği:** duvarın alt kısmı, çocuk boyu (Bölge 7'deki el izleri gibi: Kıvılcım'ın başı hizası civarı). Görsel tutarlılık 5.
- **73'teki üçüncü çizim (gizli iz):** yeraltına inen insanlar **24 çöp adam**; en önde, en derinde olan küçük (Ece ilk iner). Sayılabilir boyda; uzaktan "insan dizisi" okunur. Kimse söylemez. Sayı denetimde ölçülür.
- **73 ve 75 aynı koridor duvarı:** 73'te duvarda 3 çizim (koloni, fırtına bulutu, yeraltına inen insanlar). 75'te aynı duvarda 5 çizim: ilk üçü aynı, sonra dışarıda tek robot ve robot ile kız el ele. Bitiş damgalarıyla aynı: 73 "Çizim: 3", 75 "Çizim: 5".
- **75'in son çizimi:** göğüsteki çizimin kız + robot kısmı, **aynı ölçülerden**. Bölge 7'deki çiçek gibi: `ChildDrawing`'teki kız/robot ölçüleri ortak sabite alınır, göğüsteki çizim piksel piksel aynı kalır. Çiçek bu duvarda yok (çiçek 70'teki duvardaydı).
- **75'in rota yazısı:** çizimin altında çocuk eliyle `KKDDDKKB` (başlangıç kodundaki `rota` ile harfi harfine aynı). Görsel tutarlılık 6. `BrushPaint`'e `D` eklenir.
- **76'nın odası:** Ece ile Defne'nin küçük odası; kuzey duvarı boyunca **iki** kamp yatağı (küçük: Ece, battaniyesi renkli; büyük: Defne). 24 kişi istasyonun başka odalarında kaldı. Bu odada 2 yatak olduğu için sayı çelişkisi doğmaz (görsel tutarlılık 5).
- **Katlanmış çizim:** oynanan notun sonunda yerde belirir (Bölge 7'de izin sonunda hedefin belirmesi gibi). Üç notun sonu üst sıradaki üç farklı kare: hepsi yatakların önü. 3a'da 76 bitince çizim yerinde kalır (alınma sahnesi 3b). **77'den itibaren Kıvılcım'ın sırtında**, kalemin yanında, katlanmış küçük kâğıt. Çanta sırası: kurdele, kalem, çizim (görsel tutarlılık 6).
- **Ekran bulmacasında robotun yeri (72, 77, 78):** Kıvılcım ekranlara dönük, **alanın üst kenarında** (kuzey; bilgisayarların hemen önü) durur. Ekran kutusu alanın alt yarısına yaslanır. Böylece kutu açıkken de üstte Kıvılcım ve uyanan bilgisayarlar görünür. Bunun için üç bölüm dosyasında `R` üst sıraya taşınır; robot yürümediği için bulmaca değişmez.
- **Sahnedeki ekranlarda okunur yazı yok** (dünyada bilgisayar yazısı yok kuralı): uyanan ekranda kehribar renkli soyut satır çizgileri kayar. Okunan yazı yalnız terminal kutusunda (arayüz).
- **Engeller (`K`):** 71'de dışarıda kanyon kayası ve kapının önünde bırakılmış kasalar; 72-80'de iç mekân eşyası: dolap, masa (sandalyesiyle), raf, kasa yığını. Her biri tek kareye sığar, "buradan geçilmez" okunur.
- **Paneller (73-74, rakamlar):** istasyonun **zemindeki güç kutusu**. Alçak kapaklı kutu; Kıvılcım üstüne çıkabilir (yükseklik ≤ 0,1). Önünde 10 küçük ışık çubuğu var, yanan çubuk sayısı = gücün onda biri (7 → 7 çubuk; ölçüm görünür olur). Çatlak (1-4): kapak kırık, çubuklar kırpışır, ara sıra kıvılcım. Onarılınca çubuklar sabit yanar. Davranış (ölçme, onarma, geri alma) Bölge 3'teki panelin aynısı; yalnız görünüş farklı.
- **Veri kartı (`V`):** kalın, avuç içi boyunda bilgi kartuşu (yaklaşık 0,18 × 0,12 × 0,03), yere yatık. Üstünde koloni turuncusu etiket, küçük yanıp sönen yeşil ışık. Loş odada da seçilir. Arayüz simgesi: köşesi kesik küçük kart.
- **80'in haritası:** arka duvarda büyük, basılı bir istasyon haritası. Üzerinde istasyon (küçük kare), kanyon (kıvrık çizgi), tuz gölü (beyaz oval), gölün ötesinde volkan (üçgen). Defne'nin turuncu işaret kalemiyle çizdiği **oklar** gölün ötesini gösterir. Haritada yazı yok (resim). 80'de `read_note()` rotayı bu haritadan okur: Kıvılcım haritaya bakar, kâğıt kartı yine aynı (K3).
- **79'un odası:** atölye (tezgâh, alet rafı, yedek parça kasaları) + Kor'un bacağı (K4). Bacak duvar dibinde yatar, alanın dışında, kapakları ezik, eklemi kırık, sarı-turuncu maden boyası soyulmuş. Boyu Kıvılcım'ın ~1,5 katı.
- **Pencere yok** (K1: kapalı oda). Ses bu planda değişmez.

---

## Genel kurallar

- Her şey kodla çizilir; hazır model / büyük doku / yazı tipi dosyası yok (paket boyutu). Kodla üretilen küçük doku (≤ 64×64) olur.
- Unity C# 9. Yeni her `.cs`/`.hlsl` için `.meta` (Unity üretir); silinen dosyanın `.meta`'sı da silinir; hepsi git'e girer.
- **Önceki bölgelerin kuralları geçerli:**
  - Nesneler zemine basar.
  - Dünyada bilgisayar yazısı (`WorldText`) yok; dünyadaki yazı `BrushPaint` ile.
  - Bir sahnede en fazla tek okunur kelime. **Bu bölgede istisna 75'teki rota yazısı:** kelime değil, harf dizisi; bulmacanın kendisi.
  - Robotlar konuşmaz.
- **İç mekânda ekranın üstü:** arka duvarın ve tavan kirişlerinin yalnız koyu, sade kısmı üst yazıların/düğmelerin (ekranın üst ~%20'si) arkasına denk gelir. Orada çizim, ekran, lamba olmaz (okunurluk).
- **Görsel tutarlılık 1-7 (spec)** her görevde ölçülür. Mümkün olan sahne denetimine (`-sahneler -bolge8`, Görev 5) girer: yere basma, yüzeyden taşmama, sayılar, çakışma.
- **Işık tek kaynaktan:** iç mekânda tavan lambaları (yukarıdan, hafif serin beyaz) + ekranların kehribar ışığı; gölgeler yukarıdan, yumuşak. Telefonda nokta ışık pahalıdır: **gerçek nokta ışık en fazla 1-2**. Lambaların ve ekranların ışığı parlayan malzeme (`Emissive`) + zemine yatık sahte ışık lekesi (Bölge 7'deki `ScoutLight` gibi ekleyici leke) ile verilir.
- **Oynanış okunur kalmalı:** 72-73'ün loşluğunda bile robot, eşya, veri kartı, güç kutusu ilk bakışta seçilir. Telefon ekranı bilgisayardan koyu görünür: şüphede bir tık aydınlat.
- `Oyun.cs` büyütülmez: yalnız çağrı satırları (en fazla ~20). İş `StationTraces`, `StationScreens`, `NoteView`'da.
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıflar, tekrar yok (renkler, çizim ölçüleri ortak sabitten), anlamlı adlar, ölçülü soyutlama. Yorumlar Türkçe ASCII (ç/ş yerine c/s), dosya başında ne olduğu.
- **Denetimi ölçülü yap (CLAUDE.md):** ara görevlerde yalnız ilgili dar denetim; tam tur yalnız Görev 5'te bir kez.
- Denetim komutları (Bash; **`-shots` için mutlak yol ver**, göreli yol görüntü yazmıyor; paket için Unity Hub açık ve giriş yapılmış olmalı):
  - Paket: `rm -rf oyun/Build && "/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log`. Sonra `grep "error CS" build.log` ve `grep -i "shader error" build.log` boş çıkmalı.
  - Görüntü: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -shots <scratch mutlak yol>/<klasör> -bolum 71`. Bu 71-80 + sınavları çeker (birkaç dakika). Her bölümde `bNN-0-baslangic-kodu`, `-1-bekleme`, `-2-yolda`, `-3-bitti` kareleri çıkar. Performans modu için sona `-performans ac`.
  - Motor/dünya: `cd motor-test && dotnet test` (şu an 1711).
- Arayüz/klavye denetimlerinde oyun penceresi **ön planda** olmalı (değilse sahte hata).

## Review Focus (her görevde akılda)

1. **Başka bölgeye sızma:** `_Region = 6` ve `_StationScene` yalnız 71-80'de olmalı. Bölge 7 (61, 67, 70) ve Bölge 6 (51) görünüşü değişmemeli. Görev 1 ve 5'te ilk kareleri eski görüntülerle karşılaştır: `docs/tasarim/bolge-7-sahneler-2026-11-04/` ya da `bolge-7-gorunus-2026-11-03/`. Yoksa değişiklikten önce aynı komutla "önce" karesi al.
2. **Loşlukta okunurluk (72-73):** acil ışık atmosfer verir ama oyunu karartmaz.
3. **Geniş görünüm (klavye + ipucu açık, kamera geri çekilir):** arka duvarın tepesinde gökyüzü açılmamalı. `StationSky` iç mekânı koyu çizer, tavan kirişi görünür. Yan duvarlar ekran kenarında kalmalı; duvar çizimleri ve harita yine okunmalı.
4. **Ekran kutusu ile sahne (72, 77, 78):** kutu açıkken Kıvılcım ve bilgisayarlar üstte görünür. 26 satır kutudan taşmaz, kayar (görsel tutarlılık 2). Klavye açıkken kutu ile kod kartı çakışmaz.
5. **Yeniden dene / bölüm değişimi:** "↻", "Tekrar" ve bölüm değişince şunlar başa döner:
   - uyanan bilgisayarlar söner,
   - kâğıt kartı kapanır, harf vurgusu baştan başlar,
   - katlanmış çizim yine gizlenir,
   - 73'ün lambaları loşa döner.
   Tek kopya kalır; statik önbellekte nesne tutulmaz. Değişen notta (76, 80) her "Çalıştır" yeni notun kartını gösterir.
6. **Performans modu (`MARSKOD_SADE`, `-performans ac`):** zeminin hafif sürümü de istasyon zemini olmalı. Eşyalar az parçalı, gerçek nokta ışık sayısı sınırda.
7. **Sayı eşleşmeleri (damgalarla):** 71'de 2 veri kartı; 72'de 6 bilgisayar; 73'te 3 çizim ve 2 onarılacak güç kutusu; 75'te 5 çizim; 73'ün üçüncü çiziminde 24 çöp adam; 76'da 2 yatak; 79'da 1 bacak. Görev 5'te otomatik sayılır.

---

### Görev 1: Ortam — 71 kapı önü (dış) ve 72-80 iç mekân (arka plan, ışık, zemin, duvarlar, eşya engelleri)

**Dosyalar:**
- Yeni: `oyun/Assets/Shaders/StationSky.hlsl`
- Değişir: `oyun/Assets/Shaders/MarsSky.hlsl` (`MarsStation()` + nehir dallarının yanına istasyon dalları)
- Değişir: `oyun/Assets/Shaders/Ground.shader` (yeni `stationFloor`, iki sürüm)
- Değişir: `oyun/Assets/Scripts/RegionLook.cs` (`StationDoor`, `Station`, `StationDim` tabloları; `case Region.Station` artık kanyona düşmez; bölüm numarasıyla seçim)
- Değişir: `oyun/Assets/Scripts/Obstacles.cs` (`case Region.Station` → dışarıda kaya + kasa, içeride eşya)
- Yeni: `oyun/Assets/Scripts/StationRoom.cs` (iç mekânın kabuğu: arka duvar, yan duvarlar, tavan kirişleri, lambalar)
- Yeni: `oyun/Assets/Scripts/StationFacade.cs` (71: istasyonun dış cephesi, kapı, arma yeri)
- Yeni: `oyun/Assets/Scripts/StationFurniture.cs` (dolap, masa + sandalye, raf, kasa yığını; engel ve süs ortak)
- Değişir: `oyun/Assets/Scripts/StationTraces.cs` (`Build`, `ResetBackdrop`, `_StationScene`, `IsIndoor`)
- Değişir: `oyun/Assets/Scripts/Traces.cs` (`StationTraces.Build` / `ResetBackdrop` çağrısı)
- Değişir: `oyun/Assets/Scripts/BackdropCache.cs` (izlenen değerlere `_StationScene`)
- Değişir: `oyun/Assets/Scripts/Oyun.cs` (yalnız çağrı satırları: `RegionLook.ForLevel`, iç mekânda alan dışı manzaranın gizlenmesi)

**Arayüzler:**

```hlsl
// MarsSky.hlsl
float _Region;   // ... 5 nehir yatagi, 6 istasyon (Bolge 8)
bool MarsStation() { return abs(_Region - 6.0) < 0.5; }
float _StationScene;   // StationTraces: 0 = 71 kapi onu (disari), 1 = 72-80 ic mekan

// StationSky.hlsl (MARSKOD_STATION_INCLUDED korumasi; RiverSky.hlsl iskeleti ve ayni yardimcilar)
static const float3 STATION_PLAIN = ...;   // 0: kanyon ucunun golgeli dibi; 1: ic mekanin koyu zemini (zemin buna solar)
float3 StationBackdrop(float2 suv);
```

```csharp
// StationTraces.cs
public const int FirstLevel = 71, LastLevel = 80, DoorLevel = 71, LightsOnLevel = 73;   // 73 bitince tavan lambalari yanar
public static bool IsIndoor(int levelNumber);    // 72-80
public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Func<float, float, float> ground)  // Gorev 1: kabuk + cephe; Gorev 2-4: nesneler
public static void ResetBackdrop(int levelNumber);   // _StationScene; Bolge 8 disinda 0

// RegionLook.cs: bolge yerine bolum numarasiyla secim (Bolge 8'de kapi onu disarida, ic mekan iceride; 72-73 los)
public static RegionLook ForLevel(int levelNumber);   // Station disinda For(Regions.Of(levelNumber)) ile ayni
public IEnumerator Blend(RegionLook to, Light sky, Light sun, float seconds);   // 73 bitince los -> aydinlik (Sunrise'in benzeri; ortak kod varsa onu kullan)

// StationRoom.cs — ic mekanin kabugu. Arka duvar alanin arkasinda, ekranin tepesine kadar; yan duvarlar ekranin kenarinda.
// Doner: arka duvarin on yuzu (cizimler, harita, bilgisayarlar buraya) ve lamba nesneleri (73'te yanar).
public readonly struct Room { public readonly Transform BackWallFace; public readonly IReadOnlyList<Renderer> Lamps; ... }
public static Room Build(Transform parent, Vector2 areaHalf, bool lightsOn, int seed, Func<float, float, float> ground)
```

- [ ] **Adım 1: "Önce" kareleri.** Değişiklikten önce paket üret. Şunların ilk karelerini al:
  - `-shots .../once -bolum 71`: yalnız `b71-1`, `b72-1`, `b73-1` gerekli; tur bitmeden kapatılabilir.
  - `-bolum 61`, `-bolum 67`: Review Focus 1.
- [ ] **Adım 2: `StationSky.hlsl`.** İki dal:
  - **0 (71, dışarısı):** kanyonun ucu. İki yanda yükselen kanyon duvarları ufukta birleşir. Saat **öğleden sonra**: Bölge 7'nin sabahından sonraki gün, istasyon cephesi güneşte. Güneş solda yüksekçe (kapının yüzü aydınlık). Gökyüzü Mars öğleden sonrası: tepede soluk bej-pembe, ufukta açık kum rengi. Kanyon gün batımından (Bölge 6) ve nehir sabahından (Bölge 7) ayırt edilmeli.
  - **1 (72-80, içerisi):** arka plan iç mekânın en uzak kısmı. Koyu tavan, birkaç kalın kiriş, yer yer kopmuş kablo, uzak duvarda tek tük sönük gösterge ışığı. Gökyüzü **yok**. Renk koyu serin gri-mavi + çok hafif kehribar yansıma. Bu dal yalnız 3B duvarın kapatamadığı yerde görünür (geniş görünüm, Review Focus 3): "boşluk" değil "tavan" okunmalı.
  - Sonuç öteki dallarla aynı biçimde döner (`RiverBackdrop`'a bak, aynısını yap: `MARS_EXPOSURE` vb.).
- [ ] **Adım 3: `MarsSky.hlsl` istasyon dalları.** `MarsRiverBed()` geçen her yerin yanına `MarsStation()` dalı: arka plan (`StationBackdrop`), ufuk ovası (`STATION_PLAIN`), zemin sıcaklığı, pus, ufuk bandı, yüzey rengi (`MarsRegionSurface`).
  - Zemin sıcaklığı: dışarıda öğleden sonra ılık, içeride çok zayıf.
  - Pus: dışarıda kanyondaki gibi yok; içeride **hafif toz pusu**. Uzak köşeler kararır, havada toz var gibi. Ama alan net kalmalı.
  - `#include "StationSky.hlsl"`. Kanyon ve nehir satırlarına dokunma.
- [ ] **Adım 4: `Ground.shader` `stationFloor` (iki sürüm).**
  - **İçeride:** metal zemin plakaları. Karelerden **farklı** boyda (~0,5'lik plakalar, kare çizgileriyle karışmasın). Plaka kenarında ince koyu derz, köşelerde perçin noktası. Her yerde ince bir **toz/kum tabakası**: kapıdan içeri savrulmuş, duvar diplerinde ve köşelerde kalın, ortada ince. Yer yer eski ayak izi lekesi (koloni bir gece kaldı); seyrek, belli belirsiz. Kare çizgileri (oyun ızgarası) açıkça okunur kalır.
  - **Dışarıda (71):** kanyon ucunun kumu (Bölge 7'den farklı: çatlak çamur yok) + kapının önünde dökme beton zemin. Beton, alanın arka kenarından cepheye kadar uzanır, kenarları kumla örtülü.
  - `SADE` sürümü: derz/perçin yerine ucuz çizgi deseni + aynı ana renk + toz tabakası.
  - Dal: `else if (MarsStation()) a = stationFloor(p, far, inArea);`
- [ ] **Adım 5: `RegionLook` + bölüme göre seçim.** Başlangıç değerleri görüntüde ayarlanır:
  - **`StationDoor` (71):** `shaderRegion = 6f`, güneş solda, öğleden sonra, ılık `#FFD3A1`, orta güçte; gök ışığı soluk bej.
  - **`StationDim` (72-73, acil ışık):** yukarıdan zayıf serin beyaz (`skyIntensity ~0.45`), ikinci ışık kehribar, çok zayıf (ekranlardan geliyormuş gibi, önden). Ortam ışığı koyu mavi-gri.
  - **`Station` (74-80 ve 73 bitince):** yukarıdan serin beyaz `#E6ECF2` (`~0.85`), yumuşak gölge; kehribar ikinci ışık zayıf.
  - `RegionLook.ForLevel(int)` ekle. `Oyun.cs`'te üç `RegionLook.For(...)` çağrısını (satır ~503, ~820, ~1003) `ForLevel(level.Number)` ile değiştir; bölge dışı davranış aynı kalır.
  - Yorum: "Istasyon: 71 kapi onu ogleden sonra; icerisi yalniz lambalar ve ekranlarla aydinlanir (pencere yok)."
- [ ] **Adım 6: `StationRoom` (iç mekânın kabuğu).** `FlatMesh` ile:
  - **Arka duvar:** alanın arkasında (`z = areaHalf.y + 0.8` civarı), boyu alanın genişliği + 3. Yüksekliği normal bakışta ekranın tepesine kadar; geniş görünümde de tavan kirişiyle birleşir.
    - Alt bant: çocuk boyuna kadar düz, açık gri-krem boyalı panel. Çizimler, harita, bilgisayarlar buraya.
    - Üstü: koyu, sade, yatay panel derzleri, havalandırma ızgarası. Ekranın üst %20'sine denk gelen kısımda nesne yok.
  - **Yan duvarlar:** ekran kenarında, alanın iki yanında; içe bakan yüzleri hafif aydınlık.
  - **Tavan kirişleri ve lambalar:** arka duvarın üstünden öne uzanan 2-3 kiriş. Kirişlerde dikdörtgen tavan lambaları (parlayan malzeme). `lightsOn` false iken yalnız biri kırmızı-turuncu acil ışık, ötekiler sönük. Zemine lamba altında sahte ışık lekesi.
  - **Gerçek nokta ışık:** en fazla 1 (`lightsOn` iken). `MARSKOD_SADE`'de yok.
  - **Terk edilmişlik:** duvar diplerinde kum birikintisi, yere düşmüş birkaç kâğıt ve bir kablo (alan dışında, zemine basar).
- [ ] **Adım 7: `StationFacade` (71).** Alanın arkasında istasyonun cephesi: prefabrik, köşeleri yuvarlak, alçak modüller (iki modül + ortada kapı modülü), koloni turuncusu şeritler, kumla aşınmış.
  - **Kapı:** tek kanatlı, yarı açık sürgülü kapı. İçerisi karanlık; 72'de içeri girilecek.
  - Kapı üstünde arma için boş yer (Görev 4). Yükseklik üst %20'ye girmez.
  - İki yanda kanyon duvarına dayanan alçak kayalar.
  - Kapının önünde yere bırakılmış iki kasa (alan dışı, süs).
- [ ] **Adım 8: `StationFurniture` + `Obstacles`.** `case Region.Station:` dışarıda (`!StationTraces.IsIndoor`) kanyon kayası (Bölge 6'nın `LayeredRock`'ı; yeni kaya yazılmaz); içeride `StationFurniture.Obstacle(parent, pos, index)`.
  - Eşya türü index'e göre sırayla: dolap (uzun, kapağı aralık), masa + devrilmiş sandalye, raf (üstünde kutular), kasa yığını.
  - Her biri tek karenin içinde (kenarlara 0,05 pay), yere basar. Boyu Kıvılcım'ın 1-1,6 katı (görsel tutarlılık 5: insan eşyası, robottan büyük).
  - Engelin hangi bölümde olduğunu `Obstacles` bilmez: `StationTraces.IsIndoor`'u `Oyun`'un verdiği bölüm numarasıyla sorar. Hangi yol en sadeyse o (çağrıya bak; ek parametre ya da `Build`'in önce çağrılması).
- [ ] **Adım 9: Alan dışı manzara içeride gizlenir.** `Scenery` kayaları ve uzak taşlar iç mekânda görünmemeli. `Oyun.cs`'te manzara kökü `IsIndoor` iken kapatılır (1-2 satır). Bölge 7'deki `riftZoneRocks` gibi; Bölge 6-7 davranışı değişmez. Dışarıda (71) manzara açık.
- [ ] **Adım 10: `_StationScene` + önbellek.** `StationTraces.ResetBackdrop`: 71 → 0, 72-80 → 1, Bölge 8 dışı 0. `Traces.ResetBackdrop`'tan çağrılır. `BackdropCache` izlenen değerlere `_StationScene` eklenir.
- [ ] **Adım 11: Paket + görüntü + kendi kontrolün.** `-shots .../g1 -bolum 71`. Bak: `b71-1`, `b72-1`, `b73-1`, `b74-1`, `b79-1` + klavye açık kare (`b71-0` / `b72-0`).
  - 71 dışarı mı, cephe ve kapı okunuyor mu?
  - 72-80'de gökyüzü hiç görünmüyor mu (geniş görünüm dahil, Review Focus 3)?
  - 72-73 loş ama okunur mu (2)? 74 aydınlık mı?
  - Eşya engelleri tek karede mi, yere basıyor mu?
  - Zemin plakası ızgarayla karışıyor mu?
  - `-bolum 61` / `-bolum 67` / `-bolum 51` ilk kareleri "önce" ile aynı mı (1)?
  Sorun varsa düzelt, yeniden üret.
- [ ] **Adım 12: Ragıp'a göster.** 71, 72, 74 (gerekirse 79). Kısaca ne değiştiğini anlat. **Onay gelmeden Görev 2'ye geçme.**

### Görev 2: Nesneler — veri kartı, güç kutusu, 76'nın odası, 79'un atölyesi ve Kor'un bacağı, 80'in haritası

**Dosyalar:**
- Yeni: `oyun/Assets/Scripts/DataCard.cs` (`IPickup`)
- Değişir: `oyun/Assets/Scripts/Pickup.cs` (`Collectible.DataCard` → `DataCard.Create`; geçici yorum silinir)
- Değişir: `oyun/Assets/Scripts/Hud.cs` (veri kartı simgesi: hücre noktası yerine küçük kart; geçici yorum silinir)
- Yeni ya da değişir: güç kutusu görünüşü. Önce `SolarPanel.cs`'i oku:
  - Davranış (ölçme, onarma, `Pop`/`Restore`) görünüşten ayrılabiliyorsa `SolarPanel`'a görünüş seçimi eklenir: yeni `PowerUnitLook.cs`, Bölge 8'de seçilir.
  - Ayrılamıyorsa ortak davranış tek yere alınır, iki görünüş onu kullanır.
  - **Davranış kodu iki kez yazılmaz.**
- Yeni: `oyun/Assets/Scripts/StationProps.cs` (yatak, atölye tezgâhı ve rafı, Kor'un bacağı)
- Yeni: `oyun/Assets/Scripts/StationMap.cs` (80: duvar haritası)
- Değişir: `oyun/Assets/Scripts/StationTraces.cs` (bölüm bölüm yerleşim)

**Arayüzler:**

```csharp
public static class DataCard { public static IPickup Create(Transform parent, Vector3 localPos, int seed); }   // kalin kartus, turuncu etiket, yanip sonen yesil isik
public static class StationProps
{
    public static Transform Beds(Transform parent, Vector3 wallBase, float yawDeg, Func<float, float, float> ground);   // 76: iki kamp yatagi (kucuk Ece, buyuk Defne)
    public static Transform Workshop(Transform parent, Vector3 wallBase, Func<float, float, float> ground);            // 79: tezgah, alet rafi, yedek parca kasalari
    public static Transform KorLeg(Transform parent, Vector3 floorPos, float yawDeg, Func<float, float, float> ground); // 79: K4
}
public static class StationMap { public static Transform Paint(Transform wallFace, Vector2 at, float width, int seed); }   // 80: harita + Defne'nin oklari (yazisiz)

// StationTraces.cs
public const int ScreensLevel = 72, DrawingsFirstLevel = 73, DrawingsSecondLevel = 75, FoldedLevel = 76, ListLevel = 77, ListEndLevel = 78, WorkshopLevel = 79, MapLevel = 80;
public static Transform Beds { get; private set; }      // 76 (3b sahnesi icin); diger bolumlerde null
public static Transform WallMap { get; private set; }   // 80 (3b kapanisi icin); diger bolumlerde null
```

- [ ] **Adım 1: `DataCard`.** Ölçü ve renk "Planın verdiği kararlar"daki gibi. Toplanınca küçülüp kaybolur (`Pop`), `Restore` öteki toplananlar gibi (`RiverPebble`'a bak). Işığı yavaşça yanıp söner (canlılık; `Scenery.Attach` benzeri). Hud simgesi: toplanan dolu, toplanmayan boş, aynı kural.
- [ ] **Adım 2: Güç kutusu (73, 74, 79'daki paneller).** Önce `SolarPanel`'ı oku, yukarıdaki kurala göre ayır. Görünüş: alçak metal kutu, üstünde menteşeli kapak, önünde 10 ışık çubuğu (yanan = güç/10).
  - Çatlak: kapak kırık, çubuklar düzensiz kırpışır, ara sıra kıvılcım.
  - Onarılınca: kapak kapanır, çubuklar sabit yanar.
  - Kırık (`0`, parçası toplanır): kapak yok, çubuklar sönük.
  - Robot üstünde durabilir (≤ 0,1 yükseklik). Bölge 3'teki paneller **aynı kalır** (Review Focus 1: `-bolum 21` ilk karesi).
- [ ] **Adım 3: 76 — oda ve yataklar.** Kuzey duvarı boyunca iki kamp yatağı, ikisi de alanın dışında (üst sıranın arkası), oynanan karelerin üstünde değil.
  - Küçük yatak: renkli battaniye, yastığın yanında küçük bez bir şey. Büyük yatak: düzgün katlanmış gri battaniye, yanında küçük çanta.
  - Yatakların boyu insan boyunda (görsel tutarlılık 5): Ece'ninki Kıvılcım'ın ~2 katı uzunlukta, Defne'ninki ~3 katı.
  - `StationTraces.Beds` tutulur.
- [ ] **Adım 4: 79 — atölye ve Kor'un bacağı.**
  - Arka duvar dibinde tezgâh (üstünde aletler), alet rafı, yedek parça kasaları.
  - Duvar dibinde yatık iri bacak: K4'teki görünüş. Eklemi kırık, kapakları ezik, sarı-turuncu maden boyası soyulmuş, ayak tabanı geniş ve kalın tırtıllı. Boyu Kıvılcım'ın ~1,5 katı, alan dışında, yere basar.
  - Nesne adı `KorBacak`. Hiçbir yazı yok.
- [ ] **Adım 5: 80 — istasyon haritası.** Arka duvarın alt-orta bandında, telefonda resim olarak okunacak boyda: çerçeveli büyük levha, soluk krem zemin. Üzerinde:
  - istasyon (küçük koyu kare),
  - kanyon (kıvrık kahve şerit),
  - tuz gölü (büyük beyaz oval),
  - gölün ötesinde volkan (koyu üçgen, tepesi kırık),
  - Defne'nin turuncu işaret kalemiyle çizdiği 2-3 ok: istasyondan gölü geçip volkana doğru, sonuncusu gölün ötesini gösterir. Oklar Bölge 6'nın boya ok rengi, ama ince kalem çizgisi.
  Yazı yok. Harita duvar yüzünden taşmaz (görsel tutarlılık 2). `StationTraces.WallMap` tutulur.
- [ ] **Adım 6: Paket + görüntü + kendi kontrolün.** `-shots .../g2 -bolum 71`. Bak:
  - `b71-1`: 2 veri kartı seçiliyor mu?
  - `b73-1`, `b74-1`: güç kutularının çubukları sayılabiliyor mu? `b73-3`: onarılınca değişiyor mu?
  - `b76-1`: yatak ölçeği.
  - `b79-1`: atölye, bacak okunuyor mu, Kıvılcım'a göre iri mi?
  - `b80-1`: harita okunuyor mu?
  - `-bolum 21` ilk karesi önceki gibi mi?
- [ ] **Adım 7: Ragıp'a göster.** 71 (veri kartları), 73 (güç kutuları), 76 (yataklar), 79 (Kor'un bacağı), 80 (harita). **Onay gelmeden Görev 3'e geçme.**

### Görev 3: İstasyon terminali + uyanan bilgisayarlar (72, 77, 78), kâğıt not + harf vurgusu (76, 80), notun sonunda beliren nesne, 73'te lambaların yanması

**Dosyalar:**
- Değişir: `oyun/Assets/Scripts/ScreenView.cs` (terminal görünüşü; konumu alanın alt yarısı; geçici yorum silinir)
- Yeni: `oyun/Assets/Scripts/StationScreens.cs` (sahnedeki bilgisayarlar: kur, cevap gelince uyandır, sıfırla)
- Yeni: `oyun/Assets/Scripts/NoteView.cs` (kâğıt kartı + harf vurgusu; arayüz)
- Değişir: `oyun/Assets/Scripts/Hud.cs` (`NoteView`'u kurar; `ShowNote`, `MarkNoteLetter`, `HideNote`)
- Değişir: `oyun/Assets/Scripts/Robot.cs` / `SparkBot.cs` (`virtual IEnumerator HoldNote(bool up)`: kolunu kaldırıp küçük kâğıdı göğsünün önünde tutar, indirir)
- Değişir: `oyun/Assets/Scripts/Oyun.cs` (yalnız çağrı satırları: `NoteRead`'in geçici satırları silinir; ekran cevabı → `StationScreens.Wake`; notlu bölümde varış → beliren nesne; 73 bitince `LightsOn`)
- Değişir: `oyun/Assets/Resources/Bolumler/bolum-72.json`, `-77.json`, `-78.json` (yalnız `harita`: `R` üst sıranın ortasına)

**Arayüzler:**

```csharp
// StationScreens.cs — ekran bulmacasinda arka duvardaki bilgisayarlar. Dunya kurallarini bilmez: Oyun cevabi ve dogrulugunu verir.
public class StationScreens
{
    public StationScreens(Transform wallFace, int count, int seed);   // 72: 6 masa + 6 bilgisayar; 77-78: tek buyuk duvar terminali (count = 1)
    public void Wake(int rowIndex, bool correct);   // 72: dogruysa o bilgisayar uyanir (sonuk -> kehribar, soyut satirlar kayar); 77-78: terminal her satirda kisa kirpisir
    public void Highlight();                         // 78: dogru (True) satir geldiginde terminal bir an turuncuya doner (3b aciklama aninin isareti)
    public void Restore();                           // hepsi soner
    public int Awake { get; }                         // denetim icin
}

// NoteView.cs — notun kagit karti (arayuz). Dunyada yazi yok kurali: harfler arayuzde, kagit renginde.
public class NoteView : VisualElement
{
    public void Show(string note);     // kart acilir, harfler sirayla (KKDGDD), hicbiri vurgulu degil
    public void Advance();             // siradaki harfi vurgular (yurunen ya da toplanan her adimda bir)
    public void Hide();
}
```

- [ ] **Adım 1: Terminal görünüşü (`ScreenView`).** Davranış aynı (`Show`, `Clear`, `Add`, `Hide`; kayma; 26 satır). Görünüş değişir:
  - Zemin koyu yeşilimsi siyah `#0B0F0C` (%95). Yazı kehribar `#FFB347` civarı, mono.
  - Çerçeve: koyu metal kenar (2 px açık + 6 px koyu), köşeler az yuvarlak.
  - Üstten alta ince tarama çizgileri: kodla üretilen 1×4 doku, `backgroundImage` + tekrar, çok soluk.
  - Başlık `"İSTASYON EKRANI · " + fonksiyon + "(...)"` aynı; başlıkta yanıp sönen küçük imleç (`_`).
  - `✓` yeşil ve `✗` kırmızı kalır (oyunun ortak dili). `True` dönen satır kehribar zeminle ters yazılır (eskiden accent zemin).
  - **Konum:** `free` alanının alt yarısına yaslı. Yükseklik en çok `free`'nin ~%55'i; üstte Kıvılcım ve bilgisayarlar görünür (Review Focus 4). Klavye açıkken kod kartıyla çakışmaz.
- [ ] **Adım 2: Robotun yeri (72, 77, 78).** Üç bölüm dosyasında `R` üst sıranın ortasına taşınır (yalnız `harita`). `dotnet test` geçmeli; geçmezse sebebe bak, haritayı zorla değiştirme, Ragıp'a sor. Kıvılcım ekrana dönük (yüzü kuzeye). Gerekirse `LoadLevel`'de ekranlı bölümde robotun başlangıç yönü kuzey yapılır (1 satır).
- [ ] **Adım 3: `StationScreens`.**
  - **72:** arka duvar önünde, alanın arka kenarının hemen arkasında, yan yana 6 masa ve her birinde eski, kalın kasalı bir bilgisayar. Ekranlar sönük koyu cam.
    - `Wake(i, true)`: o ekran kırpışarak kehribar yanar, içinde soyut satır çizgileri kayar (yazı yok), masanın önüne küçük kehribar ışık lekesi düşer.
    - Yanlış cevapta ekran bir an kırmızı kırpışır, sönük kalır.
    - Satırlar 6 bilgisayarla sırayla eşleşir (soldan sağa). "Uyanan bilgisayar: 6" ile tutar.
  - **77-78:** arka duvarda tek, geniş duvar terminali (kalın çerçeve, ekran alanın genişliğinin ~yarısı). Kod çalışınca satır başına kısa kırpışma. 78'de `True` satırı (BKM-7) gelince `Highlight`: bir an turuncu.
  - Hepsi duvar yüzünden taşmaz, masalar yere basar.
- [ ] **Adım 4: `Oyun.cs` bağlantısı (ekran).**
  - `LoadLevel`: ekranlı bölümde `stationScreens = new StationScreens(StationTraces.BackWallFace, level.Number == StationTraces.ScreensLevel ? 6 : 1, ...)`, değilse `null`.
  - Ekran cevap döngüsünde (`hud.AddScreenAnswer(answer);` satırının yanı): `stationScreens?.Wake(i, answer.Correct)`, `True` cevapta `Highlight()`. Cevap nesnesinin alan adı `ScreenAnswer`'da neyse o.
  - Geri alma (`hud.ShowScreen` / `trailView?.Restore()` yanı): `stationScreens?.Restore()`.
- [ ] **Adım 5: `NoteView` + `HoldNote` (K3).**
  - **Kart:** kâğıt rengi `#EFE6CF`, hafif eğik (~-2°), kenarı yırtık gibi düzensiz değil ama köşeleri hafif kıvrık. Harfler koyu mürekkep `#2B2622`, büyük (telefonda rahat okunur), aralarında boşluk.
  - **Vurgu:** okunan harfin altı turuncu çizgi + harf koyu turuncu. Okunmuşlar soluk, okunmayanlar normal.
  - **Yer:** başlığın hemen altında, ortada; tek satır. 12 harf sığmazsa harf boyu küçülür, kart taşmaz.
  - **`HoldNote(true)`:** sağ kol kalkar, elde küçük krem dörtgen (3B kâğıt, yazısız), ~0,3 sn. Yürürken kol iner, kâğıt elde kalmaz (yürüyüşü bozmasın): kâğıt arayüzdeki karta "geçmiş" sayılır. Kart, not bitene kadar açık kalır.
  - **80:** Kıvılcım okumadan önce haritaya döner, kısa bakar (0,3 sn), sonra `HoldNote` (rota haritadan).
- [ ] **Adım 6: `Oyun.cs` bağlantısı (not).**
  - `case NoteRead n:` geçici satırlar (`PowerLabel.Show(...)`) silinir. Yerine: `yield return robot.HoldNote(true); hud.ShowNote(n.Text);` (80'de önce haritaya bakış, `StationTraces`'in verdiği yönle).
  - Not okunduktan sonraki her `Moved` ve `Collected` olayında `hud.MarkNoteLetter()` çağrılır: `V` harfi `Collected`'a karşılık gelir.
  - Oyuncunun kodu notu bozuk yürürse vurgu olaylara göre ilerler ve sonda durur; hata vermez.
  - Geri alma ve bölüm değişimi: `hud.HideNote()`.
- [ ] **Adım 7: Notun sonunda beliren nesne.** Notlu bölümde `level.Target` yok; varışta `Oyun` yolun son karesini bilir (`world.Target` ya da notun son karesi; 1. parçada nasıl tutulduğuna bak):
  - **76:** o karenin yatağa bakan kenarında, yerde **katlanmış çizim** (Görev 4, Adım 4) kumdan açılır gibi yavaşça belirir: `DecalFade` / `LandingDust` küçük toz (Bölge 7'deki `TrailView.Arrived` gibi). Ayrıca hedef işareti (`Target.Create` + `Reach`).
  - **80:** yalnız hedef işareti.
  Tekrar yok: Bölge 7'deki "izin sonunda beliren hedef" kodu kullanılabiliyorsa onu ortak yere al, ikinci kopya yazma.
- [ ] **Adım 8: 73 bitince lambalar.** Görev tamamlanınca (`target.Reach()` satırının yanı) `if (level.Number == StationTraces.LightsOnLevel) StartCoroutine(StationTraces.LightsOn(...))`:
  - tavan lambaları sırayla kırpışarak yanar (~1,2 sn),
  - `RegionLook.Blend(StationDim → Station)`,
  - acil ışık söner.
  Geri alma ile loşa döner. 74 zaten aydınlık başlar.
- [ ] **Adım 9: Paket + görüntü + kendi kontrolün.** `-shots .../g3 -bolum 71`. Bak:
  - `b72-1`: kutu altta mı, üstte Kıvılcım + sönük bilgisayarlar mı?
  - `b72-3`: 6 bilgisayar yanık mı?
  - `b77-3`, `b78-3`: terminal ve kutu, vurgulu BKM-7 satırı.
  - `b76-2`: kart ve vurgu görünüyor mu? Görünmüyorsa `Shots`'a dokunma, Görev 5'teki denetim karesi yeter; şimdilik `calistir.bat` ile elle bak.
  - `b76-3`: katlanmış çizim belirdi mi? `b80-3`.
  - `b73-3`: lambalar yandı mı?
  - Klavye açıkken 72 ve 77 (Review Focus 4).
  - Elle: 76'da iki kez "Çalıştır": ikincide kart yeni notu gösteriyor, eski vurgu kalmıyor (Review Focus 5).
  Arayüz kodu (`Hud`, `ScreenView`) değişti ama klavye ve palet değişmedi: arayüz denetim turu burada **gerekmez**, Görev 5'te bir kez.
- [ ] **Adım 10: Ragıp'a göster.** 72 (başında ve bitince), 77-78 (kutu + terminal), 76 (kart + vurgu yolda + çizimin belirmesi), 73'ün bitişi. **Onay gelmeden Görev 4'e geçme.**

### Görev 4: Arma (71), Ece'nin turuncu duvar çizimleri (73, 75), rota yazısı, katlanmış çizim (yerde ve sırtta)

**Dosyalar:**
- Yeni: `oyun/Assets/Scripts/ColonyCrest.cs` (arma; 71'de kapı üstünde)
- Yeni: `oyun/Assets/Scripts/WallDrawings.cs` (73/75: beş çizim, sırayla)
- Değişir: `oyun/Assets/Scripts/ChildDrawing.cs` (kız + robot ölçüleri ortak sabit; göğüsteki çizim piksel piksel aynı)
- Değişir: `oyun/Assets/Scripts/BrushPaint.cs` (eksik harf `D`; eski harfler değişmez)
- Yeni: `oyun/Assets/Scripts/FoldedDrawing.cs` (katlanmış kâğıt: yerde ve sırtta aynı model)
- Değişir: `oyun/Assets/Scripts/Robot.cs` (`virtual void SetFoldedDrawing(bool on)`), `oyun/Assets/Scripts/SparkBot.cs` (kalemin yanına katlanmış kâğıt)
- Değişir: `oyun/Assets/Scripts/Oyun.cs` (yalnız `robot.SetCrayon(...)` satırının yanına `robot.SetFoldedDrawing(level.Number > StationTraces.FoldedLevel)`)
- Değişir: `oyun/Assets/Scripts/StationTraces.cs` (arma, çizimler, rota yazısı, katlanmış çizim; `Drawings`, `FoldedDrawing` özellikleri)

**Arayüzler:**

```csharp
public static class ColonyCrest { public static Transform Paint(Transform surface, Vector3 localPos, float size, int seed); }   // daire + kubbe + tek yildiz; soluk, asinmis

public enum Drawing { Colony, Storm, Descent, AloneOutside, RobotAndGirl }   // hikaye sirasi (senaryo-perde-2.md)
public static class WallDrawings
{
    public const int DescentPeople = 24;   // yeraltina inen insanlar: 24 cop adam, en ondeki kucuk (Ece ilk iner)
    // wallFace: StationRoom'un arka duvar yuzu. count: 73'te 3, 75'te 5. Doner: her cizimin ortasi (3b kamerasi icin).
    public static IReadOnlyList<Vector3> Paint(Transform wallFace, int count, Material crayon, int seed);
}

public static class FoldedDrawing { public static Transform Create(Transform parent, Vector3 localPos, float yawDeg); }   // dorde katlanmis kagit, kenarindan turuncu cizgi ucu gorunur
public static IReadOnlyList<Vector3> Drawings { get; private set; }   // 73, 75; diger bolumlerde null
public static Transform FoldedDrawing { get; private set; }            // 76'da yerdeki kagit (3b sahnesi alir); diger bolumlerde null
```

- [ ] **Adım 1: Arma (71).** Kapının üstünde, cephe yüzünde: krem daire, içinde koloni turuncusu kubbe, kubbenin üstünde küçük beş köşeli yıldız. Boya soluk, alt kenarı kumla aşınmış (yer yer eksik). Yüzeyden taşmaz. Telefonda "arma" olarak seçilir ama bağırmaz.
- [ ] **Adım 2: Ortak ölçüler (`ChildDrawing`).** Göğüsteki çizimin kız ve robot kısmının ölçüleri (baş, gövde, kol, el ele tutuşan çizgi, robotun kare başı/anteni) Bölge 7'deki çiçek gibi `public const`'lara alınır. `ChildDrawing.Create` onları kullanır. Göğüsteki doku **piksel piksel aynı** kalmalı: önce/sonra dokuyu dosyaya yazıp karşılaştır ya da aynı sayılar olduğunu kodla doğrula.
- [ ] **Adım 3: `WallDrawings` (73, 75).** Arka duvarın alt bandında, çocuk boyunda, soldan sağa hikâye sırasıyla. Hepsi turuncu kalem çizgisi, ince ve titrek, `BrushPaint`/`HandTrace` darbe kodu (yeni çizgi kodu yazılmaz):
  1. **Koloni:** üç yarım daire kubbe, yanında anten, üstte güneş.
  2. **Fırtına bulutu:** kubbelerin üstüne gelen büyük, karalanmış bulut; altında toz çizgileri.
  3. **Yeraltına inen insanlar:** yerde bir kapak/ağız, ondan aşağı inen merdiven gibi bir çizgi, üstünde sıra hâlinde **24 çöp adam**; en aşağıdaki (en önde inen) küçük. Sayılabilir (nesne adlarında ya da bir bileşende sayı tutulur, denetim sayar).
  4. **Dışarıda tek robot** (75): yer çizgisinin üstünde kapalı kapak, yanında tek küçük robot (kare baş, anten), üstünde yıldızlı gece.
  5. **Robot ve kız el ele** (75): Adım 2'deki ortak ölçülerle, aynı biçim, turuncu. Altında rota `KKDDDKKB` (`BrushPaint.Write(..., childish: true)`, `D` yeni harf).

  73'te 1-3, 75'te 1-5 (aynı duvar, aynı yerler; 75'te sağa doğru devam). Çizimler duvar yüzünden ve alt banttan taşmaz (görsel tutarlılık 2; Bölge 7'deki `RiverWall.Guard` dersi: duvarın kenarı çizimin dışında kalmalı). Gerekirse 75'te çizimler biraz küçülür ama telefonda her biri seçilmeli.
- [ ] **Adım 4: `FoldedDrawing`.** Dörde katlanmış küçük kâğıt, krem. Kenarından turuncu bir çizgi ucu görünür; içi görünmez (Perde 4'e saklı). 76'da Görev 3'teki notun sonunda belirir, `StationTraces.FoldedDrawing` olarak tutulur. Sırtta (77+): `SparkBot`'ta kalemin yanına, küçültülmüş, kayışın altına sıkıştırılmış. `SetFoldedDrawing(level > 76)`. **Kalıcı karakter görünüşü:** Ragıp'a ayrıca yakın görüntüyle göster.
- [ ] **Adım 5: Paket + görüntü + kendi kontrolün.** Bak:
  - `b71-1`: arma.
  - `b73-1`: 3 çizim okunuyor mu, 24 çöp adam seçiliyor mu?
  - `b75-1`: 5 çizim; son çizim göğüsteki ile aynı biçim mi; rota yazısı başlangıç kodundakiyle aynı mı?
  - `b76-3`: katlanmış çizim.
  - `b77-1`: sırtta kâğıt.
  - Klavye açık kare: çizimler yine görünüyor mu (Review Focus 3)?
  - Göğüsteki çizim önce/sonra aynı mı? `-bolum 51` ve `-bolum 70` fırça yazıları önce/sonra aynı mı (`BrushPaint` değişti)?
- [ ] **Adım 6: Ragıp'a göster.** 71 arma, 73, 75 (yakın kare: son çizim + rota), sırttaki kâğıt yakın görüntüsü (gerekirse `CameraShot` ile tek kare). **Onay gelmeden Görev 5'e geçme.**

### Görev 5: 3a son denetimi, sayı ve tutarlılık denetimi (`-bolge8`), belgeler

**Dosyalar:**
- Değişir: `oyun/Assets/Scripts/Oyun.SahneDenetimi.cs` (`StationChecks`; yeni `-bolge8` bayrağı: yalnız Bölge 8, `-bolge7` gibi)
- Değişir: `CLAUDE.md` (`-bolge8` komutu bir satır)
- Değişir: `docs/tasarim/senaryo-bolge-08.md` ("Oyuna konanlar: görünüş 3a")
- Değişir: `docs/tasarim/telefon-denemesi.md` (71, 72, 76 için `KARE:` satırı)
- Değişir: `ILERLEME.md`
- Yeni klasör: `docs/tasarim/bolge-8-gorunus-<tarih>/` (son görüntüler)

- [ ] **Adım 1: Sahne denetimi (`-sahneler -bolge8`).** `StationChecks(dir, problems)`. Her biri için kare çekilir (`s71-kapi.png`, `s72-ekranlar.png`, `s73-cizimler.png`, `s75-cizimler.png`, `s76-not.png`, `s79-atolye.png`, `s80-harita.png`), uymazsa `problems`'a evre adı + konumla yazılır:
  - **71:** 2 veri kartı; arma cephenin içinde.
  - **72:** çözüm kodu çalıştırılır → `StationScreens.Awake == 6`; geri alınca 0.
  - **73:** 3 çizim, üçüncüsünde 24 çöp adam; çizimlerin hepsi duvar yüzünün sınırları içinde. Çözümden sonra lambalar açık; geri alınca kapalı.
  - **75:** 5 çizim.
  - **76:** her not sürümünde katlanmış çizim notun son karesinde ve yere basıyor. Notun sonunda kart vurgusu son harfte. Ayrıca kod çalışırken ilk adımın karesi.
  - **77:** sırtta kâğıt görünür.
  - **78:** `Highlight` bir kez.
  - **79:** `KorBacak` var, alan dışında, yere basıyor.
  - **80:** `WallMap` duvar yüzünde.
  - **Genel (görsel tutarlılık 1, 2, 7):** 71-80'in her birinde alan dışındaki istasyon nesneleri zemine basıyor (alt sınır zemin yüksekliğinin ±2 cm'inde). Hiçbiri oynanan karelere girmiyor. Engel eşyası kendi karesinde.
- [ ] **Adım 2: Son tur (bölge sonu, bir kez).**
  - `dotnet test` (1711 ya da daha çok) hepsi geçer.
  - Windows paketi hatasız (`error CS` ve shader hatası yok).
  - `-shots -bolum 71`: 71-80'in hepsi "Tamamlandı" ile bitiyor, görüntüler tek tek gözden geçirilir.
  - `-performans ac` ile en az 72, 76 ve 80.
  - `-sahneler -bolge8` → TAMAM.
  - Arayüz kodu değiştiği için (`Hud`, `ScreenView`, `NoteView`): klavye denetimi ve arayüz denetimleri (`-bolum 71` turu sonunda) TAMAM; pencere ön planda.
  - Ortak sahne kodu (`Robot`, `SparkBot`) değiştiği için (`HoldNote`, `SetFoldedDrawing`): `-sahneler -bolge7` bir kez TAMAM (Bölge 7 sahneleri kolları kullanıyor). Tam `-sahneler` (Bölge 6+7) ve `-bolum 50 -final` yalnız `CameraShot`/`SceneWalker`'a dokunulduysa; dokunulmadıysa çalıştırma (CLAUDE.md "Denetimi ölçülü yap").
  - Bölge 7 (61, 67, 70), Bölge 6 (51) ve Bölge 3 (21) ilk kareleri değişmedi (Review Focus 1).
- [ ] **Adım 3: Belgeler.**
  - `senaryo-bolge-08.md` "Oyuna konanlar: görünüş 3a": dosyalar, K1-K4'ün sonucu, planın verdiği kararlar ve görüntüde değişenler.
  - `telefon-denemesi.md` 71, 72, 76.
  - `CLAUDE.md`: `-bolge8` satırı.
  - Görüntüler `docs/tasarim/bolge-8-gorunus-<tarih>/` (yalnız son hâl; ara denemeler silinir).
  - `ILERLEME.md`: ne yapıldı, açık sorular, sıradaki iş = 3b planı.
- [ ] **Adım 4: Ragıp'a son tur.** 71-80 ilk kareleri yan yana + performans modu iki kare. Onay gelince 3a biter; sıradaki iş **Bölge 8 · 3b planı** (Opus 5.5 · high, yeni sohbet). Bölge bitince **telefon testi hatırlatması** (Hamza, `telefon-denemesi.md`).

---

## 3b için hazır bırakılanlar (bu planda yapılmaz, bozulmasın)

| 3b sahnesi | 3a'nın bıraktığı |
|---|---|
| 76 çizimin alınması (Kıvılcım eğilir, alır, kalemin yanına koyar) | `StationTraces.FoldedDrawing` (yerdeki kâğıt), `Robot.SetFoldedDrawing` (sırttaki), `StationTraces.Beds`. `Robot.TakeToBack` Bölge 7'de kalem için yazıldı; 3b onu genelleştirir. |
| 78 açıklama anı (BKM-7 satırı vurgulanınca Kıvılcım'ın tepkisi, kamera) | `StationScreens.Highlight`, duvar terminali (`StationTraces.BackWallFace` üstünde), ekran kutusunun vurgulu satırı |
| 80 kapanış (son çizimin önü → listedeki "görev: KAPI" → Kıvılcım başını kaldırır) | `StationTraces.Drawings` (75'teki çizimlerin yerleri), `StationTraces.WallMap`. Kapanış 80'in odasında geçeceği için 3b, son çizimin nasıl görüneceğine karar verir (örn. 80'e kısa geçiş ya da 80'in duvarında çizimin kopyası değil, kamera geçişi; **3b planında Ragıp'a sorulur**). |

## Kapsam dışı

- 3b sahneleri (76, 78, 80).
- Ses (bilgisayar uğultusu, lamba kırpışma sesi): istenirse 3b'de.
- Bölge 2 bayrağına arma eklenmesi (açık soru).
- Bilgi kartının 73-77/80'e eklenmesi (açık soru, Enes dönünce).
- Bölge 9-10 görünüşü.
- Telefon testleri (ILERLEME.md listesi).
- `TestAllLevelsOpen`/`TestSolutionsFilled` (Play Store öncesi).

## Ragıp'a açık sorular (plan dışı, acelesi yok)

1. Bölge 2'deki bayraktaki "okunmayan amblem" ileride bu arma mı olsun (küçük gizli bağ)? Olursa ayrı, küçük iş.
2. 80 kapanışında "son çizimin önü": çizimler 73-75'in koridorunda, harita 80'in odasında. 3b'de Kıvılcım koridora mı yürüsün, yoksa kapanış harita odasında mı geçsin? (3b planında yeniden sorulur.)
