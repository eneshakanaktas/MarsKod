# Bölge 7 Görünüşü — 3a (ortam, nesneler, izler, Serçe ışığı) · Uygulama Planı

> 2026-11-02 · Ragıp + Claude (Opus 5.5). Kod yazılmadı; bu plan yeni sohbette görev görev uygulanır.
> **Model · efor: Opus 5.5 · high** (görsel/3B iş: ölçü, yerleşim, "doğru görünüyor mu" kararı). Süre: ~5-6 sa (iki oturuma bölünebilir: Görev 1-2 / Görev 3-5).
> **Ara görüntü kuralı:** her görevin sonunda Ragıp'a görüntü gösterilir; **Ragıp onaylamadan sonraki göreve geçilmez.** Beğenmezse aynı görevde düzeltilir. Yardımcı ajan yok, Claude işi kendisi yapar.

**Amaç:** Bölüm 61-70'in geçici görünüşünü (Bölge 6'nın kanyon gökyüzü/zemini/kayası, drone parçası gibi çakıl, `scout()` halkası + turkuaz yazı, hedefsiz iz bölümleri) tasarım belgesindeki gerçek görünüşle değiştirmek: sabah, gölgedeki kuru nehir yatağı; nehir çakılı; Serçe'nin ışığı; yürüdükçe beliren izler; hikâye izleri (göç izi, ayrılan küçük iz, dönüş izi, zıplayan iz); kalem, el izleri, duvar çizimi/yazısı.

**Tasarım:** `docs/superpowers/specs/2026-11-02-bolge-7-design.md` → "3. Görünüş ve sahneler" (3a.1-3a.4, 3c). Hikâye: `docs/tasarim/senaryo-bolge-07.md` ("İzler ve gizli ayrıntılar"). 3b (63 kuş bakışı, 68 kalem, 70 final) **bu planda yok**, ayrı plan; ama 3a onların ihtiyacını hazır bırakır (en alttaki "3b için hazır bırakılanlar").

**Yapı:** Önceki bölgelerin düzeni korunur. Bölge 7 kendi bölge numarasını alır (`_Region = 5`); Bölge 6'nın kanyon kodu (`CanyonSky.hlsl`, `canyonFloor`, `RegionLook.Canyon`, `LayeredRock`) **değişmez**. Arka plan yeni `RiverSky.hlsl` (`BackdropCache` ile saklanır), zemin `Ground.shader` yeni `riverFloor` (tam + `MARSKOD_SADE`), ışık `RegionLook.RiverBed`. Nesneler ayrı dosyalarda kodla çizilir; yerleşimi yalnızca `RiverBedTraces.Build` seçer. Oynarken olanları gösteren iş (izlerin belirmesi, Serçe'nin ışığı, hedefin belirmesi) yeni `TrailView` sınıfında; `Oyun.cs`'e yalnızca çağrı satırları girer.

**Teknik:** Unity 6 (6000.6.3f1, URP), C# 9, HLSL. `Motor/` ve `Dunya/` değişmez (gerekirse yalnızca okuma için küçük bir özellik; aşağıda yok, gerekmiyor).

---

## Kararlar (Ragıp, 2026-11-02: K1-K5'in hepsi **önerildiği gibi**)

Uygulanacak olan "Öneri" sütunu. Hikâye görüntüsü Ragıp'ın alanı; görüntüde beğenilmezse değişir.

| # | Soru | Öneri | Diğer seçenek |
|---|---|---|---|
| K1 | Serçe'nin yol ayrımındaki ışığı ve `scout()`'un yön yazısı hangi renkte? Şu an yazı turkuaz, ama turkuaz yalnızca Ece'nin rengi. | **Güneş rengi** (sıcak beyaz-sarı `#FFE3A8`): Serçe yukarıda güneşte, ışığı aşağıdaki gölgeye yansıtıyor gibi. Turkuaz Ece'ye kalır. | Serçe'nin göz rengi |
| K2 | Yön yazısı (`East` gibi) yol ayrımında görünsün mü? | **Kalsın, küçük:** oyuncu `scout()`'un verdiği değeri görür (parametre dersi). Panel gücü, makara uzunluğu gibi ölçümler de böyle gösteriliyor; dünyaya yazılmış yazı sayılmaz. | Kaldır, yalnız ışık |
| K3 | 69'daki el izleri nasıl? | **Kalemle çizilmiş el çevresi:** çocuklar elini duvara koyup etrafını çizer. Gerçekçi, kalemle bağlanıyor, turkuaz. | Dolu boya avuç izi |
| K4 | Büyük el izi nasıl? | **O da turkuaz çevre:** Ece annesinin elini de çizmiş (Defne onu orada buldu). 70'teki "küçük el, büyük el, robot eli yan yana" ile birebir bağlanır. | Boyasız toz izi (Defne'nin eli) |
| K5 | 70'te duvarda el izi var mı? | **Yazının yanında 1 küçük + 1 büyük el çevresi** (69'daki dizinin sonu; aynı duvar devam ediyor) ve yanlarında robot eli için boş yer (3b'de Kıvılcım oraya elini koyar). 69'un sayısı (7 küçük + 1 büyük) bundan etkilenmez: o bölümün duvarı. | Yalnızca 3b'deki sahnede belirsin |

Ragıp'a sorulmayan, plan içinde verilmiş kararlar (beğenmezse görüntüde değişir):
- **Göç izi (63-64) = 23 büyük iz dizisi + 1 küçük iz dizisi.** Her dizi bir kişi; küçük dizi bir büyüğün hemen yanında (el ele). Diziler aile aile gevşek gruplar hâlinde, böylece 3b'nin kuş bakışında sayılabilir. 61-62'de göç izi yok (izler henüz bulunmadı).
- **İz bölümlerinde (62, 65, 67, 68, 69, 70) Serçe'nin izi** başlangıç karesinde görünür, sonra kumla örtülüdür; Kıvılcım bir kareye bastıkça o karenin izleri belirir. İki kol aynı görünür. Hedef görünmez; izin sonuna varılınca belirir (`Target`).
- **Hangi izi takip ediyoruz:** 62 göç izinin kenarı (büyük + küçük birlikte), 65 yalnızca küçük iz (Ece ayrılıyor), 67 küçük iz taştan taşa zıplıyor (zıplama yerinde iki ayak yan yana, aralık uzun) + büyük koşan iz, 68-70 küçük + büyük koşan iz.
- **Dönüş izi (66-69):** süs olarak alanın içinden ya da yanından geçen, adımları kısa ve **daha derin** (koyu) tek çift büyük iz; yanında küçük iz yok. 66'da gidiş (büyük, aralıklı) ile dönüş yan yana.
- **Kalem 68'den sonra sırtta** (69 ve sonrası, kurdelenin yanında). 68'de yerde durur; alınma sahnesi 3b'de. 3a'da 68 bitince kalem yerinde kalır (sahne gelince değişir).

---

## Genel kurallar

- Her şey kodla çizilir; hazır model / büyük doku / yazı tipi dosyası yok (paket boyutu). Kodla üretilen küçük doku (≤ 64×64, iz için) olur.
- Unity C# 9. Yeni her `.cs`/`.hlsl` için `.meta` (Unity üretir); silinen dosyanın `.meta`'sı da silinir; hepsi git'e girer.
- **Bölge 6 kuralları geçerli:** sis yok (derinlik gölgeyle), nesneler zemine basar (`TerrainHeight`), dünyada bilgisayar yazısı (`WorldText`) yok (yazı `BrushPaint` ile), bir sahnede en fazla tek okunur kelime (**tek istisna 70'teki duvar yazısı**), robotlar konuşmaz. Duvarlar/kayalıklar üst yazılara/düğmelere (ekranın üst ~%20'si) uzanmaz.
- **Renkler:** turkuaz `#2FC4B8` (`CraterTraces.EceColor`) yalnızca Ece'nin şeyleri (kalem, el izleri, duvar çizimi/yazısı). Serçe'nin ışığı K1'e göre.
- **Saat sabah, güneş doğuda (sağda, +x), alçakta.** Uzak duvarların yalnız **üst şeridi** turuncu aydınlık; dip (alan) serin morumsu-kahve gölgede: doğrudan güneş ışığı çok zayıf, gölgeler yumuşak. **Ama oynanış okunur kalmalı:** robot, kaya, çakıl, toz, panel karanlıkta kaybolmaz (Review Focus 2).
- `Oyun.cs` büyütülmez: yalnızca çağrı satırları (en fazla ~15 satır). İş `TrailView` ve `RiverBedTraces`'te.
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıflar, tekrar yok, anlamlı adlar, ölçülü soyutlama. Yorumlar Türkçe ASCII (ç/ş yerine c/s), dosya başında ne olduğu.
- Denetim komutları (Bash; **`-shots` için mutlak yol ver**, göreli yol görüntü yazmıyor):
  - Paket: `rm -rf oyun/Build && "/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log`; `grep "error CS" build.log` boş, `grep -i "shader error" build.log` boş.
  - Görüntü: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -shots <scratch mutlak yol>/<klasör> -bolum 61` (61-70 + sınavlar; birkaç dakika). Performans modu: sona `-performans ac`. Her bölümde `bNN-0-baslangic-kodu`, `-1-bekleme`, `-2-yolda`, `-3-bitti` kareleri çıkar.
  - Sahne denetimi: `... -sahneler -shots <klasör>` → log'da `SAHNE DENETIMI: TAMAM` (Görev 5'te Bölge 7 sayı denetimi buna eklenir).
  - `cd motor-test && dotnet test` → hepsi geçer (şu an 1463).
- Arayüz/klavye denetimlerinde oyun penceresi **ön planda** olmalı (değilse sahte hata).

## Review Focus (her görevde akılda)

1. **Başka bölgeye sızma:** `_Region = 5` ve `_RiverScene` yalnız 61-70'te. Bölge 6 (51, 57, 60) ve Bölge 5 (41) görünüşü değişmemeli. → Görev 1 ve 5'te `-bolum 51` ve `-bolum 41` ilk kareleri eski görüntülerle karşılaştırılır (`docs/tasarim/bolge-6-sahneler-2026-10-11/`, `docs/tasarim/bolge-5-bulmacalar-2026-10-07/b41-1-bekleme.png`; yoksa değişiklikten önce aynı komutla "önce" karesi alınır).
2. **Gölgede okunurluk:** sabah gölgesi oyunu karartmamalı; çakıl, kaya, toz, panel ve robot ilk bakışta seçilmeli. Telefon ekranı bilgisayardan koyu görünür: şüphede bir tık aydınlat.
3. **Geniş görünüm (klavye + ipucu açık, kamera geri çekilir):** arka duvar (67-70) ve yan kanyon ağzı (65-66) üst düğmelere taşmamalı; 70'in yazısı yine de okunmalı.
4. **Yeniden dene / bölüm değişimi:** "↻", "Tekrar" ve bölüm değişince izler yeniden gizlenir, hedef kaybolur, Serçe yerine döner, tek kopya kalır (statik önbellekte nesne tutulmaz). Değişen yolda (68-70) her "Çalıştır" başka iz gösterir: önceki koşunun izleri kalmamalı.
5. **Performans modu (`MARSKOD_SADE`, `-performans ac`):** zemin hafif sürümde de nehir yatağı; iz ağları tek parça (göç izi yüzlerce iz: tek ağ, tek çizim).
6. **Sayı eşleşmeleri:** 63'te 23 büyük + 1 küçük iz dizisi; 69'da 7 küçük + 1 büyük el izi; 64'te 2 çakıl (haritada zaten 2 `C`). Görev 5'te otomatik sayılır.

---

### Görev 1: Ortam — sabah, gölgedeki nehir yatağı (arka plan, ışık, zemin, kaya, duvarlar)

**Dosyalar:**
- Yeni: `oyun/Assets/Shaders/RiverSky.hlsl`
- Değişir: `oyun/Assets/Shaders/MarsSky.hlsl` (`MarsRiverBed()` + kanyon dallarının yanına nehir dalları)
- Değişir: `oyun/Assets/Shaders/Ground.shader` (yeni `riverFloor`, iki sürüm)
- Değişir: `oyun/Assets/Scripts/RegionLook.cs` (`RiverBed` tablosu; `case Region.RiverBed` artık kanyona düşmez)
- Değişir: `oyun/Assets/Scripts/Obstacles.cs` (`case Region.RiverBed` → `RiverBoulder`)
- Yeni: `oyun/Assets/Scripts/RiverWall.cs` (yakındaki kaya duvar parçası; 65-66 yan kanyon ağzı, 67-70 arka duvar + yan kanatlar)
- Değişir: `oyun/Assets/Scripts/RiverBedTraces.cs` (`Build`, `ResetBackdrop`, `_RiverScene`)
- Değişir: `oyun/Assets/Scripts/Traces.cs` (`RiverBedTraces.Build` / `ResetBackdrop` çağrısı)
- Değişir: `oyun/Assets/Scripts/BackdropCache.cs` (izlenen değerlere `_RiverScene`)

**Arayüzler:**

```hlsl
// MarsSky.hlsl
float _Region;   // ... 4 kanyon, 5 nehir yatagi (Bolge 7)
bool MarsRiverBed() { return abs(_Region - 5.0) < 0.5; }
float _RiverScene;   // RiverBedTraces: 0 = 61-62 yarik patikasinin dibi, 1 = 63-64 nehir kivrimi, 2 = 65-66 yan kanyon agzi, 3 = 67-70 yakin arka duvar

// RiverSky.hlsl (MARSKOD_RIVER_INCLUDED korumasi; CanyonSky.hlsl iskeleti ve ayni yardimcilar)
static const float3 RIVER_PLAIN = ...;   // ufuk dibindeki golgeli yatak (zemin buna solar)
float3 RiverBackdrop(float2 suv);
```

```csharp
// RiverBedTraces.cs
public static void Build(int levelNumber, Transform parent, Vector2 areaHalf, Func<float, float, float> ground)  // Gorev 1'de yalniz duvarlar; Gorev 3-4'te izler/kalem/duvar resmi eklenir
public static void ResetBackdrop(int levelNumber)   // _RiverScene; Bolge 7 disinda 0

// RiverWall.cs — kodla cizilmis katmanli, asinmis kaya duvar parcasi (su asindirmis: alt kisimda yatay oyuklar, yumusak kenar).
// Zemine oturur; yuksekligi ust yazilara uzanmayacak kadar alcak. Doner: yuzunun kameraya bakan yerel duzlemi (el izi/resim buraya).
public static Transform Build(Transform parent, Vector3 baseCenter, float length, float height, float yawDeg, int seed, Func<float, float, float> ground)
```

- [x] **Adım 1: "Önce" kareleri** — değişiklikten önce paket üret, `-shots .../once -bolum 61` (yalnız `b61-1`, `b67-1` gerekli; tur bitmeden kapatılabilir) ve `-bolum 51`, `-bolum 41` ilk kareleri (Review Focus 1).
- [x] **Adım 2: `RiverSky.hlsl`** — gökyüzü sabah: ufukta soluk şeftali-krem, tepede açık solgun mavi-gri (Mars sabahı; kanyon gün batımından belirgin farklı), güneş sağda alçakta, küçük beyaz disk + soğuk mavi hale (Mars'ın gerçek güneşi). Uzak duvarlar **alçak**, ufkun hemen üstünde (en yüksek tepe `_Horizon + 0.06`'yı geçmez), iki yanda, ortada açık. **Işık:** duvarların batıya bakan (güneşe dönük) yüzlerinde yalnızca **üst şerit** turuncu `float3(0.88,0.56,0.36)` civarı; altı ve dip serin morumsu-kahve `float3(0.30,0.24,0.27)` civarı. Tortu katmanları (yatay bantlar) Bölge 6'dakinden daha düzgün/ince (su çökeltisi). `_RiverScene` dalları:
  - **0 (61-62):** ortadaki uzak duvarda yarığın ağzı (koyu dikey çentik) ve oradan dibe inen ince zikzak patika (60'tan gelinen yer).
  - **1 (63-64):** duvarlar biraz geri; ufukta eski nehrin kıvrımının açık renkli kumlu yatağı uzaklaşır (zemindeki kıvrımla birleşir, Adım 4).
  - **2 (65-66):** 0 gibi ama çentik yok (yan kanyon ağzı 3B, Adım 6).
  - **3 (67-70):** yakın arka duvar 3B olduğu için arka plan yalnız duvarların üstündeki gök şeridi + iki yanda uzak duvar.
  Sonuç diğer dallarla aynı biçimde döner (`CanyonBackdrop`'a bak, aynısını yap: `MARS_EXPOSURE` vb.).
- [x] **Adım 3: `MarsSky.hlsl` nehir dalları** — `MarsCanyon()` geçen her yerin yanına `MarsRiverBed()` dalı: arka plan (`RiverBackdrop`), ufuk ovası (`RIVER_PLAIN`), zemin sıcaklığı (sabah gölgesi: zayıf, serin `float3(0.10,0.08,0.10)` civarı), pus (kanyondaki gibi yok), ufuk bandı, yüzey rengi (`MarsRegionSurface`: açık kum/kil). `#include "RiverSky.hlsl"`. Kanyon satırlarına dokunma.
- [x] **Adım 4: `Ground.shader` `riverFloor` (iki sürüm)** — kuru nehir yatağı, Bölge 6'nın koyu pas toprağından açıkça farklı:
  - Ana renk açık kil-krem `float3(0.62,0.50,0.42)` civarı (gölgede olduğu için ekranda daha koyu görünür; görüntüde ayarla).
  - **Çatlak çamur:** Voronoi hücreleri (çokgen plakalar, 0,25-0,4 boyunda), kenarları ince koyu çatlak; hücre içi hafif kıvrık (kurumuş çamur). Alan içinde de var ama kare çizgilerini bastırmayacak kadar soluk.
  - **Akıntı izleri:** alanın dışında, yatay kıvrımlı açık şeritler (eski su akışı). `_RiverScene == 1`'de alanın arkasından dolanan geniş bir **kıvrım** (açık kumlu kanal, kenarı hafif koyu); 3b'nin kuş bakışında harita gibi görünecek, bu yüzden alanın çevresinde en az ±8 birim sürsün.
  - **Küçük yuvarlak çakıl lekeleri:** seyrek, açık renkli noktalar (toplanan çakıllarla karışmayacak kadar küçük).
  - `SADE` sürümü: Voronoi yerine ucuz desen (tek `vnoise` çatlak bandı) + aynı ana renk + kıvrım.
  - Dal: `else if (MarsRiverBed()) a = riverFloor(p, far, inArea);`
- [x] **Adım 5: `RegionLook.RiverBed`** — `shaderRegion = 5f`. Başlangıç değerleri (görüntüde ayarlanır): `skyLight` serin `#C9C4D6` yüksek açıdan, yumuşak (`skyIntensity ~0.75`); `sunLight` turuncu `#FFB27A`, çok zayıf (`~0.2`), sağdan alçak (`sunDir` y ≈ 270°, x ≈ 10°); ambient morumsu (`ambientSky "#7A7488"`, `ambientEquator "#6A5A5C"`, `ambientGround "#3A2E2C"`). Yorum: "Nehir yatagi: sabah; gunes sagda alcakta, dibe inmemis. Alan golgede: isik yumusak, serin; dogrudan gunes cok zayif." `For`: `case Region.RiverBed: return RiverBed;`
- [x] **Adım 6: `RiverWall.cs` + `RiverBedTraces.Build` (duvarlar)** — `FlatMesh` ile katmanlı kaya: yatay basamaklı katmanlar, altta suyun oyduğu yatay girinti, üst kenar hafif dalgalı. Renk gölgeli kahve-mor; güneşe bakan üst kenar şeridi turuncu (köşe rengiyle). Gölge verir ama alanı karartmaz.
  - **65-66:** sağ arkada (izin gittiği yön, doğu) iki kısa alçak duvar parçası arasında yan kanyonun ağzı (boşluk); yükseklik ≤ 0,9.
  - **67-70:** alanın arkasında tek duvar (`z = areaHalf.y + 0.9` civarı, boyu alanın genişliği + 2), yükseklik normal bakışta ufkun biraz üstüne çıkar ama üst ~%20'ye girmez (görüntüde ayarla; 70'in resmi ve yazısı bu duvarın yüzüne sığmalı). İki yanda ekranın kenarında kesilen alçak kanatlar ("dar, kıvrımlı" yan kanyon).
  - **Arka plan kayaları (`Scenery`)** duvarın içinden çıkıyorsa: Bölge 6'daki `RiftZoneRocks` yöntemiyle **yalnız 67-70'te** gizlenen ayrı grup (`WallZoneRocks`). Önce görüntüde bak; sorun yoksa ekleme. Bölge 6'nın davranışı değişmez.
- [x] **Adım 7: `Obstacles` → `RiverBoulder`** — engel kaya: suyun yuvarlattığı iri, düzgün yüzeyli, açık gri-kahve kaya (`MeshFactory.Rock` düşük pürüzle ya da basık küre) + yanında küçük yuvarlak taş. "Buradan geçilmez" okunur kalsın (Bölge 6'nın katmanlı kayasından farklı ama aynı iri).
- [x] **Adım 8: `_RiverScene` + önbellek** — `RiverBedTraces.ResetBackdrop`: 61-62 → 0, 63-64 → 1, 65-66 → 2, 67-70 → 3, Bölge 7 dışı 0. `Traces.ResetBackdrop`'tan çağrılır. `BackdropCache` izlenen değerlere `_RiverScene` (`_CanyonDescent`'in yanına).
- [x] **Adım 9: Paket + görüntü + kendi kontrolün** — `-shots .../g1 -bolum 61`. Bak: `b61-1`, `b63-1`, `b65-1`, `b67-1`, `b70-1` (+ klavye açık kare `b61-0`/`-2`): sabah mı, dip gölgede mi, duvarların üst şeridi turuncu mu, oyun okunuyor mu (Review Focus 2), duvarlar üst düğmelere değmiyor mu (3), kaya duvarın içinden çıkıyor mu. `-bolum 51` ve `-bolum 41` ilk kareleri "önce" ile aynı (1). Sorun varsa düzelt, yeniden üret.
- [x] **Adım 10: Ragıp'a göster** — 61 ve 67 (spec 3c.1), gerekirse 63/65 de; kısaca ne değiştiğini anlat. **Onay gelmeden Görev 2'ye geçme.**

### Görev 2: Nehir çakılı, Serçe, `scout()` ışığı, yürüdükçe beliren izler, hedefin belirmesi

**Dosyalar:**
- Yeni: `oyun/Assets/Scripts/RiverPebble.cs` (`IPickup`)
- Değişir: `oyun/Assets/Scripts/Pickup.cs` (`Collectible.Pebble` → `RiverPebble.Create`; geçici yorum silinir)
- Değişir: `oyun/Assets/Scripts/Hud.cs:430` (çakıl simgesi: pervane noktası yerine yuvarlak çakıl)
- Yeni: `oyun/Assets/Scripts/Footprints.cs` (iz ağları: küçük/büyük, normal/derin, koşan/yürüyen/zıplayan)
- Yeni: `oyun/Assets/Scripts/ScoutLight.cs` (yol ayrımındaki yumuşak ışık lekesi)
- Yeni: `oyun/Assets/Scripts/TrailView.cs` (oynarken: iz belirmesi, Serçe'nin gidip dönmesi, hedefin belirmesi)
- Değişir: `oyun/Assets/Scripts/Sparrow.cs` (`SetSunlit`: güneşteki Serçe'ye sıcak kenar ışığı)
- Değişir: `oyun/Assets/Scripts/RiverBedTraces.cs` (Serçe'yi kurar: `ActiveSparrow`; iz türü tablosu)
- Değişir: `oyun/Assets/Scripts/Oyun.cs` (yalnızca çağrı satırları: `LoadLevel`, `RunProgram` başı, `Moved`, `Scouted`, tamamlanma, `ResetRobot`/geri alma)

**Arayüzler:**

```csharp
// Footprints.cs — zemine islenmis hafif cukur izler. Kodla uretilen kucuk tek doku (koyu ic, acik kenar: cukur gibi),
// zemine yatik dortgenler; bir cagrinin butun izleri tek agda birlesir (telefonda parca sayisi az).
public enum Walker { Big, Small }
public enum Gait { Walk, Run, Hop, Carry }   // Carry: donus izi; kisa adim, daha derin (koyu)
public static class Footprints
{
    // path: dunya noktalari (izin gectigi yol, zemin yuksekligi ground ile); side: yolun ortasindan yana kayma (el ele yuruyen ikinci iz icin).
    // Doner: tek parca agli nesne. reveal icin hucre hucre ayri parca gerekiyorsa Cells kullanilir.
    public static Transform Track(Transform parent, IReadOnlyList<Vector3> path, Walker who, Gait gait, float side, int seed, Func<float, float, float> ground);
}

// ScoutLight.cs
public static class ScoutLight
{
    public static IEnumerator Play(Transform parent, Vector3 cellCenter, Vector3 towards);   // yumusak leke kola dogru uzanir, ~0,5 sn parlar, soner
}

// TrailView.cs — Bolge 7 iz bolumlerinde oynarken gorunenler. Dunya kurallarini bilmez: Oyun ona hucre konumu ve olaylari verir.
public class TrailView
{
    public TrailView(Transform levelRoot, Func<Cell, Vector3> pos, Func<float, float, float> ground, Sparrow sparrow, TrailLook look);
    public void Begin(Trail shown);                 // her Calistir'da: onceki izleri siler, bu izin izlerini gizli kurar, baslangic karesini gosterir
    public void Stepped(Cell at);                   // Kivilcim bu kareye basti: karenin izleri belirir (yumusakca)
    public IEnumerator Scouted(Cell at, Direction d); // Serce kareye suzulur, kolu isikla gosterir; donus arkada (beklenmez)
    public void Arrived(Vector3 targetPos);         // izin sonu: hedef belirir (Target.Create + Reach)
    public void Restore();                          // Tekrar/bolum degisimi: izler gizli, hedef yok, Serce yerinde
}
public readonly struct TrailLook { public readonly bool Small, Big; public readonly Gait SmallGait, BigGait; }  // RiverBedTraces.LookFor(level)
```

- [x] **Adım 1: `RiverPebble`** — 3-5 yuvarlak, düzgün, açık renkli (krem/gri/soluk pembe) basık çakıl kümesi; suyun yuvarlattığı belli olsun (yumuşak yansıma, `smoothness` biraz yüksek). Boyu kareyi doldurmaz ama uzaktan seçilir (Review Focus 2). `Pop` (toplanınca küçülüp kaybolur) ve `Restore` diğer toplananlar gibi (`DronePart`'a bak). Hud simgesi: içi dolu yuvarlak çakıl (toplanan/toplanmayan farkı aynı kuralla).
- [x] **Adım 2: Serçe Bölge 7'de** — `RiverBedTraces.Build` her bölümde Serçe'yi kurar (`SparrowState.Flying`), alanın arka kenarının üstünde, Kıvılcım'dan belirgin yüksekte (güneşli şerit hissi), üst yazılara girmeyecek yükseklikte. `Sparrow.SetSunlit(true)`: kabuğa sıcak turuncu kenar ışığı (malzeme rengi/parlaması; sahnedeki ışık değişmez). Bölge 6'da çağrılmaz, görünüşü değişmez.
- [x] **Adım 3: `Footprints`** — tek doku (kodla, ≤ 32×64, alfa): ayak tabanı biçimi, içi koyu, kenarı açık (çukur). Büyük iz: uzunluk ~0,13, adım aralığı Walk 0,32 / Run 0,5 / Carry 0,24 (Carry daha koyu: "daha derin"). Küçük iz: ~0,08, Walk 0,22, Hop: iki ayak yan yana, aralık 0,45. Sağ/sol ayak sırayla, yolun kıvrımını izler, her ize küçük rastgele dönüş. Zemin yüksekliğine oturur (`ground(x, z) + 0.004`), gölge vermez. Gölgede de seçilsin ama bağırmasın.
- [x] **Adım 4: `ScoutLight`** — K1 renginde yumuşak, kenarı dağınık ışık lekesi: yol ayrımı karesinden seçilen kola doğru uzanan elips, ~0,15 sn'de belirir, ~0,35 sn durur, ~0,3 sn'de söner (toplam ≈ eski 0,5 sn bekleme). Zemine yatık, saydam, ekleyici (zemini aydınlatır). K2'ye göre yön yazısı `PowerLabel` ile, K1 renginde (turkuaz değil).
- [x] **Adım 5: `TrailView` + `Oyun.cs` bağlantısı**
  - `LoadLevel`: iz bölümündeyse `trailView = new TrailView(levelRoot, Pos, TerrainHeight, RiverBedTraces.ActiveSparrow, RiverBedTraces.LookFor(level.Number))`, değilse `null`. Hedef zaten kurulmuyor (`level.Target` iz bölümünde yok) — öyle kalır.
  - `RunProgram` başı: `var shown = TrailRuns.Pick(runs, ...)` satırından sonra `trailView?.Begin(level.Trails[runs.IndexOf(shown)])`.
  - Robot bir kareye vardığında (mevcut `Moved` olayının sonu): `trailView?.Stepped(cell)`.
  - `case Scouted s:` geçici satırlar silinir → `if (trailView != null) yield return trailView.Scouted(s.At, s.Direction);` (yorum: "Serce yol ayrimina suzulur, kolu isikla gosterir").
  - Tamamlanınca (`if (target != null) target.Reach();` satırının yanında): `trailView?.Arrived(Pos(world.Target.Value))` (`world.Target` iz bölümünde izin son karesi).
  - Geri alma (`ResetRobot`/"Tekrar" yolunda `target.Restore()` satırının yanında): `trailView?.Restore()`.
  - Serçe `Scouted`'da: kareye `0,35 sn` süzülür (Kıvılcım'dan ~1,2 yüksekte, kareye hafif önden bakarak), ışık oynar, dönüşü arkada başlar; peş peşe `scout()`'ta (67'de 8 kez) bulunduğu yerden sonrakine gider (geri dönmeyi beklemez). Toplam bekleme ≤ 0,7 sn (oyun ağırlaşmasın).
- [x] **Adım 6: Paket + görüntü + kendi kontrolün** — `-shots .../g2 -bolum 61`. Bak: `b62-*`, `b64-*`, `b65-*` (`-2-yolda` karesinde ışık/beliren iz görünüyor mu; görünmüyorsa `Shots`'a dokunma, Görev 5'teki denetim kareleri yeter, şimdilik `calistir.bat` ile elle bak), `b68-3`/`b69-3`/`b70-3` (hedef belirdi mi). 64'te 2 çakıl seçiliyor mu. Değişen yolda iki kez "Çalıştır": ikincide eski izler kalmadı mı (Review Focus 4; elle). Bölge 6'daki Serçe aynı mı (`-bolum 54` ilk kare).
- [x] **Adım 7: Ragıp'a göster** — 62 ve 65'in yolda kareleri (spec 3c.2), 64'ün çakılları. **Onay gelmeden Görev 3'e geçme.**

### Görev 3: Hikâye izleri (63-67): göç izi, ayrılan küçük iz, gidiş/dönüş, zıplayan iz

**Dosyalar:**
- Değişir: `oyun/Assets/Scripts/RiverBedTraces.cs` (bölüm bölüm süs izleri; sayılar sabit)
- (Gerekirse) `oyun/Assets/Scripts/Footprints.cs` (grup halinde dizi kurma yardımcısı)

**Arayüzler:**

```csharp
// RiverBedTraces.cs
public const int MigrationBig = 23, MigrationSmall = 1;   // 63 damgasi "Izler: 23 buyuk, 1 kucuk" (koloni 24 kisi)
public static Transform Migration { get; private set; }    // 63-64'te goc izi (3b kus bakisi ve sayi denetimi icin); diger bolumlerde null
```

- [x] **Adım 1: Göç izi (63-64)** — alanın içinden çapraz geçen geniş şerit (alanın dışında her iki yönde en az 8 birim sürer, nehrin kıvrımını izler: 3b'nin kuş bakışı). **23 büyük dizi + 1 küçük dizi**, her dizi bir kişi (`Footprints.Track`, `Gait.Walk`); diziler 4-6'lık gevşek gruplar (aileler), aralar boş; küçük dizi bir büyük dizinin hemen yanında (`side` küçük; el ele). Hepsi tek ağda birleşir (Review Focus 5). Kaya/çakıl/toz karelerinin altından geçebilir (zemine işli). Dizi sayısı `MigrationBig`/`MigrationSmall` sabitlerinden; nesne adlarında `Iz-Buyuk` / `Iz-Kucuk` (sayı denetimi ad sayar; birleşik ağ kullanılıyorsa sayı nesnenin üstünde bir alanda tutulur: `MigrationTrack` bileşeni `Big`, `Small` alanlarıyla).
- [x] **Adım 2: 65 — küçük iz ayrılıyor** — alanın arka kenarı boyunca göç izinin ince bir parçası (birkaç büyük dizi) ve ondan ayrılıp yan kanyon ağzına (sağ arka) sapan tek küçük iz. Oynanan iz (Görev 2) de küçük iz; süs izi alan dışında kalır, oynanan izle çakışmaz.
- [x] **Adım 3: 66 — gidiş ve dönüş** — alanın bir yanından geçen üç iz: küçük (Walk), büyük **Run** (aralıklı: koşuyor), büyük **Carry** (dönüş: kısa adım, daha derin, yanında küçük yok). Gidiş ikisi yan kanyona doğru, dönüş ters yönde. Söylenmez, yalnız görünür.
- [x] **Adım 4: 67 — zıplayan iz** — oynanan izde küçük iz `Gait.Hop`; ayrıca alanın dışında, yan kanyonun dibinde birkaç düz taşın üstünden geçen zıplama izi (taşın önünde ve arkasında iki ayak yan yana). Dönüş izi (Carry) de duvar dibinden geçer.
- [x] **Adım 5: 68-69 dönüş izi** — 68 ve 69'da Carry izi alanın yanından geçer (68'de kalem bunun yanına konacak, Görev 4).
- [x] **Adım 6: Paket + görüntü + kendi kontrolün** — `b63-1`, `b64-1`, `b65-1`, `b66-1`, `b67-1/-2`: izler gölgede seçiliyor mu, kalabalık değil mi, oynanan izle karışıyor mu (karışıyorsa süs izini alan dışına it).
- [x] **Adım 7: Ragıp'a göster** — 63, 66, 67. **Onay gelmeden Görev 4'e geçme.**

### Görev 4: Turkuaz kalem, el izleri, duvar çizimi ve yazısı (68-70)

**Dosyalar:**
- Yeni: `oyun/Assets/Scripts/Crayon.cs` (kalem: yerde ve sırtta aynı model)
- Değişir: `oyun/Assets/Scripts/Robot.cs` (`virtual void SetCrayon(bool on)`), `oyun/Assets/Scripts/SparkBot.cs` (kurdelenin yanına kalem)
- Değişir: `oyun/Assets/Scripts/Oyun.cs` (yalnız `robot.SetRibbon(...)` satırının yanına `robot.SetCrayon(level.Number > RiverBedTraces.CrayonLevel)`)
- Yeni: `oyun/Assets/Scripts/HandTrace.cs` (el çevresi çizgileri; küçük/büyük/robot)
- Yeni: `oyun/Assets/Scripts/WallMural.cs` (70: robot çizimi + yazı + çiçek + dokuz çizgili pasta)
- Değişir: `oyun/Assets/Scripts/BrushPaint.cs` (eksik harfler: `V L C N U İ`; çocuk eli yazısı seçeneği)
- Değişir: `oyun/Assets/Scripts/ChildDrawing.cs` (çiçeğin ölçüleri ortak sabit: duvardaki çiçek göğüstekiyle aynı)
- Değişir: `oyun/Assets/Scripts/RiverBedTraces.cs` (68 kalem, 69 el izleri, 70 duvar resmi; `Crayon`, `Mural` özellikleri)

**Arayüzler:**

```csharp
public const int CrayonLevel = 68, HandsLevel = 69, MuralLevel = 70;
public const int HandsSmall = 7, HandsBig = 1;   // 69 damgasi "El izi: 7 kucuk, 1 buyuk"

public static class Crayon { public static Transform Create(Transform parent, Vector3 localPos, float yawDeg); }   // kisa, ucu cok asinmis turkuaz boya kalemi
public static Transform Crayon { get; private set; }     // 68'de yerdeki kalem (3b sahnesi alir); diger bolumlerde null

public enum Hand { Small, Big, Robot }
public static class HandTrace { public static Transform Draw(Transform wallFace, Vector2 at, Hand hand, float angleDeg, int seed); }   // K3/K4'e gore

public static class WallMural
{
    // wallFace: RiverWall.Build'in dondurdugu yuz. Doner: Kivilcim'in elini koyacagi bos yer (3b) ve yazinin ortasi (kamera icin).
    public readonly struct Spots { public readonly Vector3 RobotHand, TextCenter; ... }
    public static Spots Paint(Transform wallFace, int seed);
}

// BrushPaint.cs — mevcut Write'a ek, ayni darbe kodunu kullanir
// childish: her harf farkli boyda (0,85-1,2), satir hafif egri yukari kayar, harfler hafif egik (cocuk eli). Varsayilan false: eski cagrilar degismez.
public static Transform Write(Transform surface, string text, Vector3 localPos, float height, Material paint, int seed,
    float angleDeg = 0f, float thickness = 0.1f, bool childish = false)
```

- [x] **Adım 1: Kalem (68)** — kısa (Kıvılcım'ın elinden küçük), altıgen gövde, turkuaz, kâğıt sargısı soluk; **ucu çok aşınmış** (yuvarlak küt uç: çok çizmiş). Dönüş izinin (Carry) hemen yanında yerde, oynanan izlerin hiçbirinin üstünde değil; gölgede seçilsin (turkuaz zaten parlak; gerekirse hafif `Emissive` değil, açık renk yeter). `RiverBedTraces.Crayon` olarak tutulur.
- [x] **Adım 2: Sırtta kalem (69+)** — `SparkBot`: kurdele düğümünün yanında çapraz duran aynı kalem (`Crayon.Create`, küçültülmüş). `SetCrayon(level > 68)`. **Kalıcı karakter görünüşü:** Ragıp'a ayrıca göster (spec 3a.4).
- [x] **Adım 3: El izleri (69)** — arka duvarın (`RiverWall` yüzü) alt kısmında, çocuk boyunda (Kıvılcım'ın başı hizası civarı; görüntüde ayarla), duvar boyunca **7 küçük** turkuaz el çevresi, aralıkları düzensiz (yürürken koymuş), hafif farklı açılarla; **sonuncusunun yanında 1 büyük** (K4). `HandTrace` `BrushPaint.Strokes` kullanır (kalem çizgisi: ince, titrek, yer yer kesik). Sayılar `HandsSmall`/`HandsBig` sabitlerinden; nesne adları `ElIzi-Kucuk` / `ElIzi-Buyuk`.
- [x] **Adım 4: Duvar resmi (70)** — arka duvarın ortasında, **telefonda okunacak boyda** (450×975 karede yazı rahat okunmalı):
  - **Robot çizimi:** çocuk çizimi, Kıvılcım'a benzer (kare baş, gözler, anten), turkuaz çizgi.
  - **"KIVILCIM BENİ BUL":** `BrushPaint.Write(..., childish: true)`, iki satır olabilir ("KIVILCIM" / "BENİ BUL"), harfler farklı boyda, eğri; aşağı doğru hafif akmış boya (birkaç harfin altında kısa ince akıntı).
  - **Çiçek:** göğüsteki çizimin **aynı çiçeği** (5 taç yaprak, iki yaprak, aynı oranlar). Tekrar olmasın: `ChildDrawing`'teki çiçek ölçüleri (`FlowerPetals = 5`, açı kayması, yarıçap oranları) ortak sabite alınır, iki çizim de onu kullanır; göğüsteki çizim **piksel piksel aynı** kalmalı (önce/sonra karşılaştır).
  - **Dokuz çizgili küçük pasta:** pasta + üstünde 9 kısa dik çizgi (mum).
  - K5: yazının yanında 1 küçük + 1 büyük el çevresi ve robot eli için boş yer (`Spots.RobotHand`).
  - Hepsi turkuaz, tek ağda birleşebildiği kadar az parça.
- [x] **Adım 5: `BrushPaint` harfleri** — `V L C N U İ` (İ: noktalı I) mevcut harflerin biçemiyle; eski çağrıların görüntüsü değişmez (`childish` varsayılan `false`). Bölge 6'daki yazılar (`-bolum 51` turundaki pano, Serçe adı) önce/sonra aynı.
- [x] **Adım 6: Paket + görüntü + kendi kontrolün** — `b68-1`, `b69-1`, `b70-1`, `b70-3` + klavye açık kare (Review Focus 3); 70'te yazı okunuyor mu; 69'da el izleri tek tek sayılabiliyor mu (7 + 1); 69'da sırtta kalem; göğüsteki çizim aynı.
- [x] **Adım 7: Ragıp'a göster** — 68, 69, 70 (spec 3c.3) + sırttaki kalem yakın görüntüsü (gerekirse `CameraShot` ile tek kare). **Onay gelmeden Görev 5'e geçme.**

### Görev 5: 3a son denetimi, sayı denetimi, belgeler

**Dosyalar:**
- Değişir: `oyun/Assets/Scripts/Oyun.SahneDenetimi.cs` (`RiverBedChecks`: sayılar + Bölge 7 kareleri; `SceneShots` içine bir satır)
- Değişir: `docs/tasarim/senaryo-bolge-07.md` ("Oyuna konanlar: 3a")
- Değişir: `docs/tasarim/telefon-denemesi.md` (61 ve 70 için `KARE:` satırı)
- Değişir: `ILERLEME.md`
- Yeni klasör: `docs/tasarim/bolge-7-gorunus-<tarih>/` (son görüntüler)

- [x] **Adım 1: Sayı denetimi (`-sahneler`)** — `RiverBedChecks(dir, problems)`: 63'ü yükle → `RiverBedTraces.Migration`'da 23 büyük + 1 küçük dizi; 64'ü yükle → toplanacak 2 çakıl (ve bitiş damgası "Örnek çakıl: 2" ile eşleşiyor); 69'u yükle → 7 `ElIzi-Kucuk` + 1 `ElIzi-Buyuk`; 70'i yükle → duvar resmi var, `Spots.RobotHand` duvarın önünde; 68 → `RiverBedTraces.Crayon` var. Her biri için birer kare (`s63-izler.png`, `s69-eller.png`, `s70-duvar.png`). Uymazsa `problems`'a yazılır. Ayrıca 62'de bir `scout()` anı karesi (`s62-scout.png`): kod çalıştırılır, ilk `Scouted` olayında çekilir (gerekirse `TrailView`'e salt-okunur `Scouting` bayrağı).
- [x] **Adım 2: Tam tur** — `dotnet test` (1463 ya da daha çok) hepsi geçer; Windows paketi hatasız (`error CS`, shader hatası yok); `-shots -bolum 61`: 61-70'in hepsi "Tamamlandı" ile bitiyor, görüntüler tek tek gözden geçirilir; `-performans ac` ile en az 62 ve 70; `-sahneler` → `SAHNE DENETIMI: TAMAM` (Bölge 6 sahneleri dahil); `-bolum 50 -final` TAMAM; klavye denetimi TAMAM (pencere ön planda); arayüz denetimleri TAMAM. Bölge 6 (51, 57, 60) ve Bölge 5 (41) kareleri değişmedi (Review Focus 1).
- [x] **Adım 3: Belgeler** — `senaryo-bolge-07.md` "Oyuna konanlar: 3a" (dosyalar, kararlar K1-K5'in sonucu); `telefon-denemesi.md` 61 ve 70; görüntüler `docs/tasarim/bolge-7-gorunus-<tarih>/` (yalnız son hâl; ara denemeler silinir); `ILERLEME.md` (ne yapıldı, açık sorular, sıradaki: 3b planı).
- [x] **Adım 4: Ragıp'a son tur** — 61-70 ilk kareleri yan yana + Performans modu iki kare. Onay → 3a biter; sıradaki iş **3b planı** (Opus 5.5 · high, yeni sohbet).

---

## 3b için hazır bırakılanlar (bu planda yapılmaz, bozulmasın)

| 3b sahnesi | 3a'nın bıraktığı |
|---|---|
| 63 kuş bakışı (Serçe'nin gözünden, tam yukarıdan) | `RiverBedTraces.Migration` (alanın dışında ±8 birim süren göç izi), zemindeki nehir kıvrımı (`_RiverScene == 1`, Ground.shader), `Sparrow` 63'te kurulu |
| 68 kalem (Kıvılcım eğilir, alır, çantaya) | `RiverBedTraces.Crayon` (yerdeki kalem), `Robot.SetCrayon` (sırttaki kalem) |
| 70 final (duvara dönüş, sessizlik, üç el yan yana, turkuaz gözler) | `WallMural.Spots` (`RobotHand`, `TextCenter`), `RiverWall` yüzü; turkuaz gözler 3b'de `SparkBot`'a eklenir |

## Kapsam dışı

3b sahneleri (63, 68, 70), kayıt ekranının ses dalgası rengi, telefon testleri (ILERLEME.md listesi), `TestAllLevelsOpen`/`TestSolutionsFilled` (Play Store öncesi), Bölge 5'in kendi görünüşü, Bölge 8-10 ders planı, panellerin (66, 69, 70'teki `2`/`3`) görünüşü (değişmez).
