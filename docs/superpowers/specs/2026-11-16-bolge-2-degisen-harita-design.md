# Bölge 2 — Değişen alan: Bölüm 15 ve 20 · Tasarım

> 2026-11-16 · Enes + Claude (Opus 5.5). **Taslak: Enes henüz onaylamadı.** "Öneri (Claude)" yazan kararları Claude verdi, Enes değiştirebilir.
> Dayandığı belgeler: `docs/tasarim/sezon-1-denetim/bolum-11-15.md`, `docs/tasarim/sezon-1-denetim/bolum-16-20.md`, `docs/tasarim/bolum-dosyasi.md`, `docs/tasarim/senaryo-bolge-02.md`, `docs/superpowers/specs/2026-11-02-bolge-7-design.md` (değişen yol), `docs/superpowers/plans/2026-11-15-bolge-10-bulmacalar.md` (değişen meteor), `docs/superpowers/plans/2026-11-15-bolge-9-10-gorunus.md` (G5: düzen değişince görsel).
> Konu: Bölge 2'nin iki sınav bölümünde (15 ve 20) buzun, kristalin ve (20'de) engelin yeri **her denemede değişir**. Yeni Python kavramı yok.

## Kararlar (özet)

| # | Karar | Neden |
|---|---|---|
| 1 | **Tek ortak "düzen" kalıbı:** bölüm dosyasına `duzenler` alanı. Bölge 10'un `meteorlar` alanı da aynı kalıba katılır (meteor = yalnızca krater içeren düzen). Öneri (Claude). | Aynı işi yapan iki alan olmasın. Bugün `meteorlar` yalnız krater taşıyabiliyor; Bölge 2 buz, kristal ve engel taşımak istiyor. Genişletmek, ikinci bir alan açmaktan temiz. |
| 2 | **Kod bütün düzenlerde denenir** (Bölge 7 ve 10 gibi); bölüm, kod hepsinde çalışınca biter. Rastgele seçim yok. | Rastgele seçimde ezber kod şans eseri geçebilir. Hepsinde denemek adil ve her seferinde aynı sonucu verir (testler de bunu ölçebilir). |
| 3 | **Bölüm 15: 2 düzen. Bölüm 20: 3 düzen.** Öneri (Claude). | Bölge 7 (68'de 2, sonra 3) ve Bölge 10 (91'de 2, sonra 3) ile aynı zorluk eğrisi. |
| 4 | **Bölüm 20 yeni bir şey sorar:** kaya robotu doğuya kaydırdığında da bir buzun üstüne gelebilir; bu yüzden buza bakmak `else`'in içinde değil, `if/else`'ten sonra, her turda yapılır. Öneri (Claude), Enes'e soru 1. | 19'un kodu artık 20'de yetmez. Yeni kavram değil: 18'in kalıbı (her turda dallanma, sonra iş) + 15'in koruması (`if ice_here():`). |
| 5 | **Hikâye: tipi.** Kutupta rüzgâr karı savurur; her denemede kar bazı buzları örter, bazılarını açar; 20'de kar yığınları da yer değiştirir. Enes'e soru 2. | Bölge 2 fırtına izleriyle başlıyor (Bölüm 11). Kutup ekibinin "kırmızı olana dokunma" notuyla da bağ kurar: kristaller karın altından çıkıyor. |
| 6 | **20'deki engeller "kar yığını" görünür** (rüzgârın yığdığı, robotun tekerleği saplanır, geçilmez). Enes'e soru 3. | Buz bloğunun her denemede yer değiştirmesi inandırıcı değil; kar yığını değişir. |
| 7 | **Düzen değişimi kısa bir kar savrulmasıyla olur** (~0,8 sn, "Çalıştır"a basınca, robot yürümeden önce); hafif grafik modunda anında. | Nesneler ekranda "ışınlanmaz"; oyuncu bir şeyin değiştiğini görür. |
| 8 | **İlk açılışta görünen düzen** 15'te bugünkü harita, 20'de yeni harita (A). Öneri (Claude). | 20'de oyuncu, "kaydığı karede buz var" durumunu daha ilk bakışta görsün; ilk denemede bilmediği bir şey yüzünden takılmasın. |
| 9 | **Bilgi kartı:** 15'te (yeni mekanik) ve 20'de (yığınlar da değişiyor). Öneri (Claude). | Ekip kuralı: yeni mekaniğe bilgi kartı. |

## İş sırası (her parça yeni sohbette)

1. **Bulmacalar + kurallar (saf C# + testler + 15/20 dosyaları + belgeler)** — Sonnet 5 · medium (~3-4 sa). Bölüm 1-6.
2. **Görünüş (düzenin sahnede kurulması, kar savrulması, kar yığını, izleme denetimi)** — Opus 5.5 · high (~3-4 sa). Bölüm 5. Ara görüntü **Enes'e erken gösterilir**; onaylanmadan ilerlenmez.

---

## 1. Amaç ve neden

Bölge 2 koşulu öğretiyor (`if`, `else`, `ice_here()`, `rock_ahead()`). Ama haritalar sabit olduğu için on bölümün hepsi koşul yazmadan, adımları ezberleyip alt alta yazarak geçilebiliyor (denetim raporları G1/G2). Ekip kuralına göre yalnız 12-13 (`if`) ve 16-17 (`else`) **kuralla** zorunlu; sonrasında oyuncu kendi yolunu bulmalı. O hâlde 15 ve 20'de koşulu zorlayan şey **kural değil bulmacanın kendisi** olmalı: alan her denemede değişirse ezber yol hiçbir zaman hepsinde çalışmaz, "bak ve karar ver" tek yol olur. Bu, gerçek programlamanın da özü: kod bugünkü haritaya değil, gelebilecek her haritaya göre yazılır.

### Hedefler
- 15 ve 20'de koşulsuz (yalnızca `move`/`collect`, döngü olsa da olmasa da) **hiçbir kod** bütün düzenlerde geçemesin; bunun kanıtı bu belgede ve otomatik testte olsun.
- 20, 19'un kopyası olmaktan çıksın ve bölgenin öğrettiklerini birleştirsin.
- Oyuncu takıldığında **neden** takıldığını anlasın: alanın değiştiğini görsün, mesajda okusun.
- Bölge 7 ve 10'daki düzen yapısı tek kalıba otursun (tekrar yok).

### Hedef olmayanlar
- Yeni Python kavramı (`while`, `elif`, değişken sayacı) yok.
- 11-14 ve 16-19 değişmez (bkz. 8. bölüm "Sonra bakılabilir").
- Her seferinde bilgisayarın ürettiği rastgele harita yok: düzenler elle yazılır, test edilir.
- Bölge 7'nin `izler`i ve Bölge 8'in `notlar`ı değişmez (onlar haritadaki nesne değil, gizli bilgi).
- Öteki düzenleri önceden gösteren yeni ekran yok (Enes'e soru 4).

### Şöyle olursa başarısızlık
- Oyuncu **şans eseri** geçebiliyorsa (ör. ezber kodu o denemede denk geldiği için).
- Koşulsuz bir kod bütün düzenlerde geçiyorsa.
- Düzen değişimi **haksızlık gibi** hissettiriyorsa: oyuncu alanın değiştiğini fark etmiyor, mesaj yanlış şey söylüyor ya da ilk gördüğü haritada hiç belli olmayan bir şey yüzünden takılıyor.
- Çözüm için tur sayısı düzenden düzene değişiyorsa (o zaman sabit `for` ile çözülemez, Bölge 2 dışı bir şey ister).
- 15, 13-14'ün; 20, 18-19'un öğrettiğinin **dışında** bir şey istiyorsa (batıya/güneye gitmek, sayaç tutmak, `while`).
- Kar savrulması Bölge 2'nin görünüşünü bozuyor, hikâye izlerini (15'teki ayak izi, tekerlek izi) siliyor ya da nesneler havada/iç içe görünüyorsa.

---

## 2. Mekanik

### 2.1 Var olan düzenler nasıl çalışıyor

- **Bölge 7 `izler`, Bölge 8 `notlar`, Bölge 10 `meteorlar`** aynı altyapıyı kullanıyor: bölüm kaç "sürüm" taşıyorsa (`VersionCount`), oyun oyuncunun kodunu her sürümde ayrı, yepyeni bir dünyada baştan çalıştırır (`Level.RunVersions`). Görev **hepsinde** bitince bölüm biter.
- Ekranda hangisinin oynatılacağını `TrailRuns.Pick` seçer: kodun takıldığı sürüm varsa ilki; hepsi başarılıysa her "Çalıştır"da sıradaki.
- Kod bazı sürümlerde geçip bazılarında takılırsa hata mesajının başına bir cümle eklenir: "Serçe bu sefer başka bir kol gördü." / "Bu denemede meteorlar başka yere düştü." (`Oyun.VersionChangedNote`).
- Bölge 10'da görsel: "Çalıştır"a basınca, oynatılacak denemenin kraterleri **anında** kurulur (`MeteorMarks.Show`, eskiler silinir, geçiş yok). Görünüş planı (G5) ileride "yeni kratere yeşil bir meteor çizgisi iner (0,4 sn)" diyor; henüz yapılmadı.
- `meteorlar` bugün yalnızca `U` (krater) taşıyabiliyor; buz, kristal ve kaya hep haritada sabit.

### 2.2 Öneri: `meteorlar` → genel `duzenler` (öneri, Claude)

Yeni bir "buz düzeni" alanı açmak yerine `meteorlar`ın kalıbı genişletilir ve adı `duzenler` olur:

- **`harita`**: hiç değişmeyen şeyler (robot, hedef, her denemede aynı kalan nesneler).
- **`duzenler`**: 1-3 düzen; her düzen haritanın boyunda satırlardır ve yalnızca **değişen** şeyleri taşır: bölümün toplanacağı (Bölge 2'de `B`), `T` kristal, `K` engel, `U` krater; `.` boş.
- Her denemede dünya = harita + o düzen.
- Bölge 10'un altı dosyasındaki `meteorlar` (91, 92, 93, 94, 96, 100) aynı işte `duzenler` adına çevrilir (yalnız ad değişir, içerik aynı; `dotnet test` doğrular). Eski ad okunmaz: tek kalıp kalır. *Alternatif:* Bölge 10 dosyalarına dokunmamak için eski ad bir süre daha okunabilir; ama o zaman iki ad yaşar.

**Neden yeni alan değil:** `meteorlar` zaten "her denemede haritaya eklenen şeyler"dir; buz da, kristal de, kaya da aynı işi görür. İki alan iki okuyucu, iki doğrulama, iki görsel kod demek (ekip kuralı: tekrar yok).

**Neden `izler`/`notlar` katılmıyor:** onlar haritada görünen nesne değil, oyuncunun bir komutla öğrendiği gizli bilgi (Serçe'nin gördüğü iz, notun yazısı). Ortak olan "her sürümde dene" altyapısı zaten paylaşılıyor.

### 2.3 Dosyada biçim (Bölüm 15 örneği)

```json
"harita": [
  ". . . . . .",
  ". . . . . .",
  ". . . . . .",
  ". . . . . .",
  ". . . . . .",
  "R . . . . ."
],
"duzenler": [
  [
    "T . . . . .",
    "B . . . . .",
    "B . . . . .",
    "T . . . . .",
    "B . . . . .",
    ". . . . . ."
  ],
  [
    "B . . . . .",
    "B . . . . .",
    "T . . . . .",
    "B . . . . .",
    "T . . . . .",
    ". . . . . ."
  ]
]
```

**Kurallar** (bölüm denetçisi yanlışta Türkçe mesaj verir; kılavuz `bolum-dosyasi.md`'ye yazılır):
- 1-3 düzen; **2-3 düzen = değişen alan**. Düzenler birbirinden farklı olmalı.
- Her düzen haritanın boyunda; içinde yalnızca bölümün toplanacağı, `T`, `K`, `U` ve `.` olur. `U` yalnızca `meteor_ahead` açıksa.
- Düzendeki işaret haritada dolu bir kareye düşmez (robot, hedef, haritadaki nesneler).
- **Her düzende toplanacak sayısı aynı** (görev yazısı tek: "3 buz"). Kristal ve engel sayısı değişebilir.
- `ice_here` açıksa toplanacak buz olmalı (bugünkü kural; buz düzenlerin içinde de sayılır). "Haritada görev yok" denetimi düzenlerdeki toplanacakları da sayar.
- `notlar` ile birlikte: sayıları aynı, notun yolu düzendeki `K`/`U`'dan geçmez (bugünkü `meteorlar` kuralı). `izler` ile birlikte yazılmaz.
- Değişen alan bölümünde `tipik_hatalar` içinde **ilk düzene göre koşulsuz yazılmış bir "ezber" çözüm** olur (Bölge 7 kuralıyla aynı).
- **Yeni otomatik "ezber denetimi"** (`dotnet test`): oyun çözümün her düzendeki yürüyüşünü kaydeder, koşulsuz koda çevirir (yalnızca `move(...)` ve `collect()` alt alta) ve öteki düzenlerde dener. Her biri **en az bir düzende takılmalı**; takılmazsa: "Bu düzenler ezberle geçiliyor: 2. düzenin yolu koşulsuz yazılınca hepsinde çalışıyor." Bölge 10 bölümleri de bu denetimden geçer; takılan çıkarsa bu işte düzeltilmez, ayrı bulgu olarak Enes'e söylenir.

### 2.4 Bir "Çalıştır"da ne olur

Oyun kodu her düzende ayrı çalıştırır, sonra ekranda birini oynatır:

| Durum | Ekranda | Mesaj |
|---|---|---|
| Kod, **ekrandaki** düzende takıldı | Alan değişmez, robot o düzende oynar | Bildik mesaj (kristal, kaya, "buz kaldı"…) |
| Ekrandakinde geçti, **başka** bir düzende takıldı | Kar savrulur, o düzen kurulur, robot orada oynar ve takılır | "**Bu denemede tipi alanı değiştirdi.** " + bildik mesaj |
| Hepsinde geçti | Sıradaki düzen (her başarılı "Çalıştır"da bir sonraki; değişiyorsa kar savrulur) | "Tamamlandı!" + bitiş metni |

İki küçük düzeltme (öneri, Claude; ortak kodda, yalnızca `duzenler` olan bölümlerde — yani Bölge 2 ve Bölge 10):
- **Ekrandaki düzen önce:** kod ekrandaki düzende de takılıyorsa ekran o düzende kalır. Bugün her zaman 1. düzene dönülüyor; oyuncu 2. düzeni düzeltmeye çalışırken alan gereksiz yere geri değişirdi.
- **Cümle yalnız alan gerçekten değiştiyse:** bugün "meteorlar başka yere düştü" cümlesi, ekrandaki düzen hiç değişmediğinde de çıkabiliyor (kod 1. düzende takılıp 2.'de geçince). Cümle yalnızca o denemede alan değiştiyse eklenir.
- Cümlenin metni bölgeye göre bir yerde durur: Bölge 2 "Bu denemede tipi alanı değiştirdi.", Bölge 10 bugünkü "Bu denemede meteorlar başka yere düştü." (yeni alan açılmaz).

### 2.5 İlk deneme ile ikinci deneme arasındaki fark nasıl anlaşılır

1. **Bilgi kartı** bölüm açılınca anlatır (4. bölüm).
2. **Görev yazısı** sürekli hatırlatır: "3 buz · alan değişir".
3. **Kar savrulması** "Çalıştır"a basınca, robot yürümeden önce, oyuncunun gözü alandayken olur.
4. **Mesajın başındaki cümle** takılmanın sebebini söyler.
5. **Alan, yeni düzende kalır:** oyuncu kodunu düzeltirken takıldığı düzene bakabilir (↻ ile robot başa döner, düzen değişmez; bölüme yeniden girince 1. düzen gelir — Bölge 10'daki gibi).
6. 1. ipucu da söyler.

Not: Kodu ilk seferde doğru yazan oyuncu değişimi hiç görmeyebilir (başarıda 1. düzen oynatılır). Bu Bölge 7 ve 10'da da böyle; bilgi kartı bunu karşılar. Başarı ekranına "Kodun iki düzende de çalıştı" yazmak ortak kodu (7 ve 10'u da) değiştirir; bu işe katılmadı.

---

## 3. Bölüm 15 ve Bölüm 20'nin yeni tasarımı

**Kare yazımı:** (sütun, satır). Sütun soldan 0-5, satır **üstten** 0-5 (satır 0 en üst = kuzey). Robot iki bölümde de (0,5), sol alt köşede. Denetim raporlarıyla aynı yazım.

**Dünyanın kuralları (bugünkü, değişmez):** `collect()` boş karede bir şey yapmaz; kristalde robotu durdurur. Engele ya da alanın dışına yürümek robotu durdurur. `rock_ahead(yön)` o yönde engel **ya da alanın sınırı** varsa `True` der. Görev: bütün buzlar toplanmış, (hedef varsa) robot hedefte ve kod durmadan bitmiş.

### 3.1 Neden koşulsuz hiçbir kod geçemez (genel kanıt)

Koşulsuz kod (yalnızca `move` ve `collect`, istersen `for` içinde) her düzende **aynı hareketleri aynı sırayla** yapar; robot her düzende aynı karelerden geçer ve aynı karelerde `collect()` der.
- Bir kare **bir düzende buz, öteki düzende kristal** ise: ilk düzende geçmek için orada `collect()` demek zorundasın (yoksa buz kalır); aynı kod öteki düzende aynı karede `collect()` der ve robot kristalde durur. Demek ki **böyle tek bir "değişen kare" yeterli**: koşulsuz kod iki düzenin ikisinde birden geçemez.
- **"Her karede `collect()`" kestirmesini kırmızı kristal engelliyor.** Bu dünyada boş karede `collect()` zararsız; kristal olmasaydı "her adımda topla" kodu, robotun yolu bütün buzlardan geçtiği sürece her düzende geçerdi. Bu yüzden ek kural (öneri, Claude): **her düzende, çözümün yolunda en az bir kristal** olur. (Tipik hatalardaki "korumasız collect" örneği de testte takılmak zorunda; bu da bunu denetler.)
- 20'de bunlara iki engel daha eklenir: **engellerin yeri değiştiği için** sabit bir yol en az bir düzende yığına çarpar; **kaydığın karede buz olduğu için** 19'un kodu (bakmak yalnızca kuzeye çıkınca) buz kaçırır.

### 3.2 Bölüm 15 — Karışık sıra (2 düzen)

Ders aynı kalır (13-14'ün devamı): `for` yürür, `if` her karede karar verir. Robot sütun 0'da 5 adım kuzeye gider. Her düzende 3 buz, 2 kristal.

**Düzen 1** (bugünkü harita, ilk açılışta görünen):
```
T . . . . .
B . . . . .
B . . . . .
T . . . . .
B . . . . .
R . . . . .
```
**Düzen 2**:
```
B . . . . .
B . . . . .
T . . . . .
B . . . . .
T . . . . .
R . . . . .
```
Değişen kareler: (0,4) B↔T, (0,3) T↔B, (0,2) B↔T, (0,0) T↔B. Yalnız (0,1) iki düzende de buz.

**Örnek çözüm** (bugünkü, değişmez):
```python
for i in range(5):
    move(North)
    if ice_here():
        collect()
```

**Kanıt — çözüm iki düzende de geçer:**

| Tur | Vardığı kare | Düzen 1: orada | toplanan | Düzen 2: orada | toplanan |
|---|---|---|---|---|---|
| 1 | (0,4) | buz → topla | 1 | kristal → bakar, toplamaz | 0 |
| 2 | (0,3) | kristal → toplamaz | 1 | buz → topla | 1 |
| 3 | (0,2) | buz → topla | 2 | kristal → toplamaz | 1 |
| 4 | (0,1) | buz → topla | 3 | buz → topla | 2 |
| 5 | (0,0) | kristal → toplamaz | 3 ✓ | buz → topla | 3 ✓ |

**Kanıt — ezber takılır:**
- Düzen 1'e göre ezber: `move(North)`, `collect()`, `move(North)`, `move(North)`, `collect()`, `move(North)`, `collect()`, `move(North)`. Düzen 1'de geçer. **Düzen 2'de** 1. adımda (0,4) kristal, `collect()` → robot durur ("Tehlikeli kristal"). Ekran düzen 2'ye geçer, mesaj "Bu denemede tipi alanı değiştirdi. …".
- Düzen 2'ye göre ezber: `move(North)`, `move(North)`, `collect()`, `move(North)`, `move(North)`, `collect()`, `move(North)`, `collect()`. **Düzen 1'de** 2. adımda (0,3) kristal → robot durur.
- "Her karede topla" (`for` içinde `move(North)` + `collect()`): düzen 1'de 2. adımda, düzen 2'de 1. adımda kristal → durur.

**Dosyada değişenler:**
- `harita` yalnızca robot; `duzenler` 2.3'teki gibi.
- `gorev`: "3 buz · alan değişir" (20 harf).
- `giris`: "Tipi başladı! Kar her denemede başka yeri açıyor. Her karede bak!" (program sesi: neşeli, habersiz).
- `bitis`: aynı ("Harika tarama! Sırada küçük bir sınav var.").
- `konular`: + "değişen alan".
- `ipuclari` (3, kademeli):
  1. "Buzlar ve kristaller robotun üstündeki sütunda; kuzeye doğru yürüyeceksin. Tipi her denemede hangisinin nerede olduğunu değiştiriyor."
  2. (bugünkü) "`for` yürümeyi tekrarlar, `if` her karede karar verir. `if` döngünün içinde olmalı, `collect()` de `if`'in içinde: iki kat girinti."
  3. (bugünkü) "`for i in range(5):` altında `move(North)` ve `if ice_here():`; onun da altında, bir girinti daha içeride `collect()`."
- `tipik_hatalar` (bugünkü dördü kalır, ikisinin metni düzene göre genelleşir; biri eklenir):
  1. Her karede `collect()` — "Her karede toplamaya çalışmak kristalde robotu durdurur; toplamadan önce if ice_here(): ile sor." (aynı)
  2. `if` döngünün dışında — "if döngünün dışında kaldı: robot sona kadar yürür ve yalnızca en üstte bir kez bakar; yoldaki buzlar kalır."
  3. `range(3)` — "range(3) üç kare yürür; üstteki kareler bakılmadan kalır."
  4. `collect()` `if` ile aynı hizada (girinti hatası) — aynı.
  5. **Yeni: ezber** (düzen 1'in 8 satırı, yukarıda) — "Bu sıra yalnızca ilk düzende çalışır. Tipi karı savurunca ilk kare kristal oluyor ve collect() robotu durduruyor. Yolu ezberleme: her karede ice_here() ile bak."
- `baslangic_kodu`, `parcalar`, `cozum`: aynı. `zorunlu_yapilar` yok (doğru: `if`'in üçüncü bölümünden sonrası serbest).
- `bilgi`: 4.2'deki kart.

### 3.3 Bölüm 20 — Su yolu, bölge finali (3 düzen)

**Ne soruyor (19'dan farkı):** 19'da robot doğuya kaydığı karede hiç buz yoktu; bu yüzden buza bakmak `else`'in içinde (yalnız kuzeye çıkınca) yetiyordu. 20'de **kaydığın karede de buz olabilir**: robot hangi yöne adım atarsa atsın, vardığı her karede bakmalı. Yani `if ice_here():` satırı `if/else`'ten **sonra**, onunla aynı hizada durur. Bu yeni kavram değil: 18'in kalıbı (her turda önce dallan, sonra işini yap; 18'de `collect()` tam bu yerdeydi) + 15'in koruması. Bölgenin her şeyini birleştirir: `for`, `if/else`, `rock_ahead`, `ice_here`, kristal, alan sınırı, hedef.

**Tur sayısı her düzende aynı:** robot yalnızca kuzeye ve doğuya gider; (0,5)'ten hedef (2,0)'a hangi yoldan giderse gitsin **5 kuzey + 2 doğu = 7 adım** atar. Robot üst sıraya erken varırsa önünde alanın sınırı vardır, `rock_ahead(North)` `True` der ve robot üst sırada doğuya, hedefe doğru yürür. Bu yüzden `range(7)` her düzende tam yeter (ve 2. ipucu "say" dediğinde yalan söylemez).

Her düzende 3 buz, 2 engel (kar yığını, `K`), 1-2 kristal. Hedef ve robot sabit: `harita`da yalnızca `R` (0,5) ve `H` (2,0).

**Düzen A** (yeni, ilk açılışta görünen — kayınca buz):
```
. . H . . .
. . T . . .
. K . . . .
. B B . . .
K T . . . .
R B . . . .
```
**Düzen B** (bugünkü Bölüm 20 haritası):
```
. . H . . .
. K B . . .
. T . . . .
K B . . . .
B . . . . .
R . . . . .
```
**Düzen C** (sütun boyunca tırmanış, üst sırada kenar):
```
. B H . . .
B K . . . .
T . . . . .
B K . . . .
T . . . . .
R . . . . .
```

**Örnek çözüm:**
```python
for i in range(7):
    if rock_ahead(North):
        move(East)
    else:
        move(North)
    if ice_here():
        collect()
```

**Kanıt — çözüm üç düzende de geçer** ("önü" = robotun kuzeyindeki kare):

*Düzen A*

| Tur | Robot | Önü | Hamle | Vardığı kare | Orada | Toplanan |
|---|---|---|---|---|---|---|
| 1 | (0,5) | (0,4) yığın | doğu | (1,5) | buz → topla | 1 |
| 2 | (1,5) | (1,4) kristal (engel değil) | kuzey | (1,4) | kristal → toplamaz | 1 |
| 3 | (1,4) | (1,3) | kuzey | (1,3) | buz → topla | 2 |
| 4 | (1,3) | (1,2) yığın | doğu | (2,3) | buz → topla | 3 |
| 5 | (2,3) | (2,2) | kuzey | (2,2) | boş | 3 |
| 6 | (2,2) | (2,1) | kuzey | (2,1) | kristal → toplamaz | 3 |
| 7 | (2,1) | (2,0) | kuzey | (2,0) hedef | boş | 3 ✓ hedefte |

*Düzen B*

| Tur | Robot | Önü | Hamle | Vardığı kare | Orada | Toplanan |
|---|---|---|---|---|---|---|
| 1 | (0,5) | (0,4) | kuzey | (0,4) | buz → topla | 1 |
| 2 | (0,4) | (0,3) yığın | doğu | (1,4) | boş | 1 |
| 3 | (1,4) | (1,3) | kuzey | (1,3) | buz → topla | 2 |
| 4 | (1,3) | (1,2) | kuzey | (1,2) | kristal → toplamaz | 2 |
| 5 | (1,2) | (1,1) yığın | doğu | (2,2) | boş | 2 |
| 6 | (2,2) | (2,1) | kuzey | (2,1) | buz → topla | 3 |
| 7 | (2,1) | (2,0) | kuzey | (2,0) hedef | boş | 3 ✓ hedefte |

*Düzen C*

| Tur | Robot | Önü | Hamle | Vardığı kare | Orada | Toplanan |
|---|---|---|---|---|---|---|
| 1 | (0,5) | (0,4) | kuzey | (0,4) | kristal → toplamaz | 0 |
| 2 | (0,4) | (0,3) | kuzey | (0,3) | buz → topla | 1 |
| 3 | (0,3) | (0,2) | kuzey | (0,2) | kristal → toplamaz | 1 |
| 4 | (0,2) | (0,1) | kuzey | (0,1) | buz → topla | 2 |
| 5 | (0,1) | (0,0) | kuzey | (0,0) | boş | 2 |
| 6 | (0,0) | alanın sınırı | doğu | (1,0) | buz → topla | 3 |
| 7 | (1,0) | alanın sınırı | doğu | (2,0) hedef | boş | 3 ✓ hedefte |

**Kanıt — ezber ve eski kodlar takılır:**
- **Değişen kareler (genel kanıt):** (2,1) B'de buz, A'da kristal; (0,4) B'de buz, C'de kristal. 3.1'e göre koşulsuz kod ne A+B'de ne B+C'de birden geçebilir.
- **A'ya göre ezber** (`move(East)`, `collect()`, `move(North)`, `move(North)`, `collect()`, `move(East)`, `collect()`, `move(North)`, `move(North)`, `move(North)`): A'da geçer. **B'de** robot hedefe varır ama (0,4) ve (2,1)'deki buzlar kalır → görev bitmez. **C'de** 4. hamlede (1,3) yığın → robot durur.
- **B'ye göre ezber** (bugünkü yolun alt alta yazılışı: kuzey, topla, doğu, kuzey, topla, kuzey, doğu, kuzey, topla, kuzey): **A'da** ilk hamlede (0,4) yığın → durur. **C'de** ilk karedeki kristalde `collect()` → durur.
- **C'ye göre ezber** (kuzey, kuzey, topla, kuzey, kuzey, topla, kuzey, doğu, topla, doğu): **A'da** ilk hamlede yığın → durur. **B'de** 2. hamlede (0,3) yığın → durur.
- **19'un kodu** (bakmak `else`'in içinde, `range(7)`): B'de geçer. **A'da** 1. ve 4. turdaki kaymalarda (1,5) ve (2,3)'e bakmaz → 1 buz, görev bitmez. **C'de** 6. turdaki kaymada (1,0)'a bakmaz → 2 buz. A ilk açılışta görünen düzen olduğu için oyuncu bunu **değişim olmadan, ilk gördüğü alanda** fark eder (karar 8).
- **18'in kodu** (her turda dallan + korumasız `collect()`): A'da 2. turda, B'de 4. turda, C'de 1. turda kristal → durur.
- **15'in kodu** (`move(North)` + `if ice_here()`, kayaya bakmadan): A'da 1. hamlede, B'de 2. hamlede yığın; C'de 6. turda alanın dışına çıkmaya çalışır → durur.

**Dosyada değişenler:**
- `harita`: yalnızca `R` ve `H`; `duzenler`: A, B, C (yalnız `B`, `T`, `K` işaretleri).
- `gorev`: "3 buz, hedef · değişir" (22 harf).
- `giris`: "Son ödev! Tipi geri döndü. Aracı koloniye hazırla."
- `bitis`: "Tipide bile yolunu buldun. Bölge 2 tamamlandı!"
- `konular`: + "değişen alan", "her adımdan sonra bakma".
- `baslangic_kodu`: `["for i in range(3):", "    if rock_ahead(North):", "        move(East)"]` (denetim raporunun önerisi: sayıyı oyuncu bulsun; üç düzende de görevi bitirmez).
- `parcalar`, `komutlar`: aynı.
- `ipuclari` (3, kademeli):
  1. "Hedef üst sırada. Tipi her denemede kar yığınlarını, buzları ve kristalleri taşıyor; yolu ezberleme. Robot yalnızca kuzeye ve doğuya gidecek."
  2. "Bölüm 18'deki gibi her turda önce `rock_ahead(North)` ile bak: yığın ya da alanın kenarı varsa doğuya kay, yoksa kuzeye çık. Kaydığın karede de buz olabilir: bakmayı yalnızca kuzeye çıkınca değil, her adımdan sonra yap. Tur sayısı her düzende aynı: 5 kuzey + 2 doğu."
  3. "`for i in range(7):` / `    if rock_ahead(North):` / `        move(East)` / `    else:` / `        move(North)` ve en altta, `if rock_ahead` ile aynı hizada (döngünün içinde, if/else'in dışında) `    if ice_here():` / `        collect()`."
- `tipik_hatalar`:
  1. `range(5)` — "range(5) yetmez: robot hedefe hangi yoldan giderse gitsin 5 kuzey + 2 doğu = 7 adım atar; 5 turda hedefin gerisinde kalır."
  2. Korumasız `collect()` — "collect() ice_here() ile korunmamış: robot kristal karesinde durur."
  3. **Yeni: 19'un kodu** — "Buza yalnızca kuzeye çıkınca bakıyorsun (Bölüm 19'daki gibi). Burada yığın seni doğuya kaydırdığında da bir buzun üstüne gelebilirsin: if ice_here():'i if/else'in dışına, döngünün içine al; robot nereye adım atarsa atsın baksın."
  4. **Yeni: `range(8)`** (denetim raporu) — "range(8) bir tur fazla: yedinci turda hedefe varıyorsun, sekizincide önünde alanın sınırı var (rock_ahead True), doğuya kayıp hedefin dışında kalıyorsun. Tur sayısı tam yetmeli."
  5. **Yeni: ezber** (A'ya göre 10 satır, yukarıda) — "Yol elle yazıldı: ilk düzende çalışır, ama tipi yığınları ve buzları taşıyınca robot ya yığına çarpar ya buz kaçırır. Her turda rock_ahead(North) ile bak, vardığın her karede ice_here() ile sor."
- `bilgi`: 4.2'deki kart.

**Kapanış sahnesi:** değişmez. Robot üç düzende de hedefte (2,0) bitirir; sahne buza bağlı değil. Hangi düzende başarılı olduysa o düzen ekranda kalır, sahne onun üstünde oynar.

### 3.4 Sınav dosyaları

- **sinav-15:** değişmez (soruları `ice_here`/`if`/kristal; hâlâ doğru).
- **sinav-20:** denetim raporundaki 3. soru düzeltmesi (iç içe `if`, `else`'in içinde) yapılır ve 20'nin dersine bir soru eklenir (öneri, Claude):
  `{ "soru": "for döngüsünde, if/else'ten sonra aynı hizada yazılan if ice_here(): ne zaman çalışır?", "secenekler": ["Her turda, robot hangi yola girerse girsin", "Yalnızca if'in altındaki yol çalıştığında", "Yalnızca else'in altındaki yol çalıştığında", "Döngü bitince, yalnızca bir kez"], "dogru": 0, "aciklama": "if/else ile aynı hizadaki satır iki yolun da dışındadır; robot kuzeye de çıksa doğuya da kaysa her turda çalışır (Bölüm 20)." }`

---

## 4. Hikâye gerekçesi ve oyuncuya anlatım

### 4.1 Neden buz yer değiştiriyor: tipi

Bölge 2 bir fırtınanın izleriyle açılıyor (Bölüm 11: bir yana yığılmış kar, savrulmuş branda). Kutupta rüzgâr yerdeki ince karı savurur (tipi). Buzlar ve kristaller yerinden **kımıldamaz**; kar bazılarını **örter**, bazılarını **açar**. Robot karın altındakine ulaşamaz, bu yüzden o denemede orası "yok" sayılır. 20'de rüzgâr karı yer yer **yığar**: kar yığını robotun tekerleğini saplar, geçilmez (kaya gibi; `rock_ahead()` onu da görür). Her "Çalıştır", tipiden sonraki yeni bir deneme.

- **Gerçek bilim:** Mars'ta kar gerçekten yağar. Phoenix aracı 2008'de kuzey kutbu yakınında bulutlardan kar düştüğünü ölçtü; güney kutbunda karbondioksit karı yağdığı da biliniyor. İnce kar ve toz Mars rüzgârıyla savrulur.
- **Hikâyeye bağ (program söylemez):** Bölüm 13'te kutup ekibinin notu "`# kirmizi olana dokunma!`" idi. 15'te kristaller karın altından çıkmaya başlayınca not anlam kazanır: ekip, tehlikenin karın altında saklandığını biliyordu. `senaryo-bolge-02.md`'deki "Ekip neden aceleyle 'kırmızı olana dokunma' yazmış?" sorusuna yarım cevap.
- **Süreklilik:** tipi 15'te başlar, 16-19'da diner (bulmacalar sabit), 20'de geri döner ("Tipi geri döndü"). Bölüm 20'nin kapanışında savrulma yok (araç sakin bir anda uzaklaşır).

### 4.2 Bilgi kartları

**Bölüm 15:**
```json
"bilgi": {
  "baslik": "Tipi alanı her denemede değiştiriyor",
  "metin": [
    "Kutupta rüzgâr karı savuruyor. Her denemede kar bazı buzları örtüyor, bazılarını açıyor; kırmızı kristaller de başka karelerde çıkabiliyor. Robotun yeri değişmez.",
    "Kodun her düzende ayrı denenir; bölüm, kod hepsinde çalışınca biter. Bir düzende takılırsa ekranda o düzen oynatılır.",
    "Bu yüzden yolu ezberleme: her karede `ice_here()` ile bak, buz varsa topla."
  ]
}
```

**Bölüm 20:**
```json
"bilgi": {
  "baslik": "Tipi kar yığınlarını da taşıyor",
  "metin": [
    "Bu sefer rüzgâr karı yer yer yığıyor. Kar yığınının içinden robot geçemez, kaya gibidir; `rock_ahead()` onu da görür.",
    "Buzlar, kristaller ve yığınlar her denemede başka yerde; robotun yeri ve hedef aynı. Kodun üç düzende de çalışmalı."
  ]
}
```
(Kartta `✓ ✗ →` işaretleri yok.)

### 4.3 Belgelere yansıması
`senaryo-bolge-02.md`: 15 ve 20'nin görev/giriş/bitiş satırları, "Sahnede: tipi" notu, açık sorulara 13 notuyla bağ. 15'in "İz 2"si (ayak izinin bittiği yerde tekerlek izi) aynen kalır.

---

## 5. Görsel tutarlılık

**Ne zaman değişir:** yalnızca "Çalıştır"a basınca, oynatılacak düzen ekrandakinden farklıysa, robot yürümeden önce. Bölüm açılınca 1. düzen kurulu gelir. ↻ düzeni değiştirmez. Ekrandaki düzen aynı kalacaksa savrulma olmaz.

**Nasıl değişir (~0,8 sn):**
- Rüzgâr yönünden alçak bir **kar savrulması** alanın üstünden geçer: yere yakın, robot boyunu aşmayan beyaz topaklar (sis değil, bütün ekranı kaplamaz). Rüzgâr yönü Bölüm 11'deki kar yığını ve branda ile aynı.
- Savrulma en yoğunken eski nesneler kara **gömülür** (aşağı doğru kısa bir küçülme), yenileri **karın altından çıkar** (yerden yukarı kısa bir büyüme). Hiçbir şey havadan düşmez, havada asılı kalmaz, yana kaymaz.
- 20'de kar yığınları yerden kabarır, eskileri yere yayılıp kaybolur.
- **Buzun altındaki zemin izi** (`_Frost` kar/buz lekesi) buzla birlikte taşınır; eski yerinde iz kalmaz.
- Hafif grafik modunda (`-performans ac`) savrulma yok: nesneler anında yer değiştirir (bugünkü Bölge 10 gibi).

**Yere basma, ışık, çakışma:**
- Her yeni nesne kendi karesinin ortasında ve zeminde; buz, kristal, kar yığını bir kareden taşmaz (kar yığını buz bloğu boyunda, ~0,56 kare).
- Gölgeler güneşle aynı yönde; savrulma topakları gölge düşürmez (yerde kayan koyu bant olmasın), ışık değişmez (bölgenin `RegionLook`'u aynı).
- Hiçbir şey robotun, hedefin ya da başka bir nesnenin karesine çıkmaz (bölüm denetçisi dosyada zaten engeller).
- Kar yığınının rengi Bölge 2'deki buz bloklarının üstündeki karla aynı; buz bloğundan ayırt edilsin ama yabancı durmasın.

**Bölge 2'nin var olan görünüşü bozulmaz:**
- 11-14 ve 16-19'a dokunulmaz. 15'in ilk görünüşü bugünküyle aynı. 20'nin ilk görünüşü yalnız nesnelerin yeriyle farklı; araç, zemin, gök aynı.
- **Hikâye izleri silinmez:** 15'teki ayak izleri ve tekerlek izi, 20'deki araç savrulmadan etkilenmez (savrulma hafif bir yer savrulması; izler donmuş ve derin). Savrulma alanın arkasına, ufuk şeridine taşmaz (ufuk kuralı).
- 20'nin kapanış sahnesi ve "Atla"daki son hâli (araç uzakta) değişmez.

**Var olandan yeniden kullanılan:**
- Bölge 10'un "Çalıştır'da o denemenin düzenini kur" anı (`MeteorMarks.Show`): genel bir **düzen görünüşü**ne dönüşür (buz, kristal, engel, krater). Krater çizimi aynen kalır.
- **Geçiş anı ortak, görünüşü bölgeye göre:** Bölge 2'de kar savrulması; Bölge 10'da görünüş planındaki (G5) meteor çizgisi ileride aynı yere takılır.
- Topak çizimi Bölge 4'ün toz bulutundan (`DustCloud`: saydam olmayan, telefonda ucuz topaklar) kar rengine boyanarak.
- Buz (`Pickups`), kristal (`Ice.CreateHazard`), buz bloğu (`Obstacles`) aynen.
- Yeni olan yalnızca: kar savrulması ve kar yığını görünüşü.

**Enes'e gösterilecek ara görüntüler:** 15 düzen 1 → savrulmanın ortası → düzen 2; 20 A → B; hafif mod; kontrastı artırıp nesnelerin zemine oturduğu ve ufuk şeridinin temiz kaldığı yakın kare.

---

## 6. Etki ve risk

### 6.1 Değişecek dosyalar (tahmini)

| Dosya | Ne değişir |
|---|---|
| `oyun/Assets/Dunya/Level.cs` (+ belki yeni küçük `Layout.cs`) | `duzenler` okuma ve kuralları (bugünkü `ReadMeteors`/`ReadCraterLayout` genelleşir); her denemenin dünyası harita + düzen; `meteorlar` adı kalkar. Level.cs büyümesin diye düzen okuma ayrı dosyaya alınabilir. |
| `oyun/Assets/Dunya/TrailRuns.cs` | "Ekrandaki düzen önce" seçimi ve "alan değişti mi" bilgisi (yalnız düzenli bölümler). |
| `oyun/Assets/Scripts/Oyun.cs` | Bölüm kurulurken değişen nesneleri düzen görünüşü kurar; "Çalıştır"da geçiş; mesaj cümlesi bölgeye göre. Buz listesinin sırası dünyanınkiyle aynı kalmalı (toplanınca doğru buz kaybolsun). |
| `oyun/Assets/Scripts/MeteorMarks.cs` → düzen görünüşü | Krater + buz + kristal + engel; geçiş anı. |
| Yeni: kar savrulması; `Obstacles.cs` | Kar yığını görünüşü (yalnız düzenli Bölge 2 bölümünde). |
| `oyun/Assets/Scripts/Oyun.IzlemeDenetimi.cs` | Düzenli bölümde çözüm her düzende bir kez oynatılır. |
| `bolum-15.json`, `bolum-20.json` | 3. bölümdeki her şey. |
| `bolum-91/92/93/94/96/100.json` | Yalnızca `meteorlar` → `duzenler` (ad). |
| `sinav-20.json` | Soru 3 düzeltmesi + yeni soru. |
| `motor-test/` | Düzen okuma hataları, her denemenin dünyası, 15/20 çözümleri, ezber denetimi, Bölge 10'un hâlâ geçtiği. |
| `docs/tasarim/bolum-dosyasi.md`, `senaryo-bolge-02.md` | Alan açıklaması (`meteorlar` satırı `duzenler` olur), hikâye metinleri. |

### 6.2 Test
- **`dotnet test`** (saniyeler): bütün bölümler; 15 ve 20'nin çözümü her düzende geçer, her tipik hata ve başlangıç kodu en az bir düzende takılır, ezber denetimi, Bölge 10 değişmeden geçer.
- **Oyunda izleme denetimi** (paket üretildikten sonra): `-izle 15` ve `-izle 20`. Çözüm her düzende bir kez oynar (her düzen görülür, hepsi "Tamamlandı"); ezber tipik hatası öteki düzende takılır ve mesajda "Bu denemede tipi alanı değiştirdi." görünür; 20'de kapanış "Atla" ile geçilir.
- **Bölge 10 için** yalnız `-izle 91` (ad değişikliğinin oyunda da çalıştığını görmek; tam Bölge 10 turu gerekmez).
- Görünüş: ara görüntüler + hafif mod karesi. Tam tur, sahne denetimleri ve arayüz/klavye denetimleri **gerekmez** (ortak sahne kodu ve arayüz değişmiyor).

### 6.3 Kayıtlı ilerlemesi olan oyuncu
- Oyun henüz yayımlanmadı (Sezon 1 çıkış öncesi); etkilenen yalnızca ekibin cihazları.
- Bitmiş bölümler bitmiş kalır. 15 ya da 20'ye geri dönen birinin kaydedilmiş kodu (ör. ezber) artık bir düzende takılabilir; bu doğru davranış.
- Sınav sonuçları ve sera yaprağı gibi kalıcı kayıtlar etkilenmez.

### 6.4 Riskler
- **Bölge 10'a dokunmak:** ad değişikliği ve iki küçük mesaj düzeltmesi Bölge 10'u da etkiler (Ragıp'ın bölgesi). `dotnet test` + `-izle 91` ile ölçülür; ezber denetiminde takılan bir Bölge 10 bölümü çıkarsa ayrı karar.
- **Buz sırası:** sahnedeki buzlar dünyanınkiyle aynı sırada kurulmazsa toplanınca yanlış buz kaybolur. Testlerle ve 20'de izleyerek bakılır.
- **20 zorlaşıyor:** üç düzen ve yeni yerleşim. İpuçları kademeli; ilk düzen (A) gerekeni baştan gösteriyor.

### 6.5 İş büyüklüğü
~1 iş günü (6-8 saat), iki oturum: bulmacalar + kurallar + testler + belgeler (Sonnet 5 · medium, ~3-4 sa); görünüş + izleme denetimi (Opus 5.5 · high, ~3-4 sa, Enes'in onayıyla).

---

## 7. Açık sorular (Enes)

1. **Bölüm 20'nin yeni sorusu uygun mu?** Öneri: 20'de kaya robotu doğuya kaydırdığında da buzun üstüne gelinebilir, bu yüzden buza bakmak `else`'in içinden çıkıp her turun sonuna geçer (19'un kodu yetmez). Alternatif: 20 yalnız 19'un değişen hâli olur (bakmak `else`'in içinde kalır, sadece yerler değişir); daha kolay, ama 20 yine 19'a çok benzer.
2. **Hikâye gerekçesi tipi olsun mu?** Öneri: rüzgâr karı savurur, her denemede başka buzlar ve kristaller açılır, 20'de kar yığınları da yer değiştirir. Alternatif: program "Bu kodu buzulun öteki şeritlerinde de çalıştıracağım" der (hava olayı yok; ama alanın arkası her denemede aynı görüneceği için daha az inandırıcı).
3. **20'deki engeller kar yığını mı olsun?** Öneri: evet (yeni görünüş, yer değiştirmesi inandırıcı). Alternatif: 16-19'daki buz blokları kalır (görünüş aynı, ama buz bloğunun her denemede başka yerde olmasının gerekçesi zayıf).
4. **Oyuncu öteki düzenleri önceden görsün mü?** Öneri: hayır; Bölge 7 ve 10'daki gibi bilgi kartı söyler, oyuncu takılınca görür. İstersen üstte küçük bir "Alan 1/2" yazısı eklenebilir (yeni arayüz işi, Bölge 7 ve 10'a da yayılır).

Claude'un kendi verdiği kararlar (değiştirilebilir): tek `duzenler` alanı ve Bölge 10'un ona çevrilmesi; 15'te 2, 20'de 3 düzen; 20'nin ilk düzeninin yeni harita (A) olması; iki bilgi kartı; sinav-20'ye yeni soru; "ekrandaki düzen önce" ve "cümle yalnız alan değiştiyse" düzeltmeleri.

---

## 8. Sonra bakılabilir

Aynı düzenler 14, 18 ve 19'a da az sayıda (2 düzen) konabilir: 14'te son karede buz/kristal, 18'de kayanın yeri, 19'da buz/kristal değişirse bu bölümler de ezberle geçilmez; ama her bölüm değişirse mekanik özelliğini kaybeder ve bölge yorucu olur, bu yüzden önce 15 ve 20'nin oyuncuda nasıl hissettirdiğine bakmak iyi. Sonraki bölgelerin sınav bölümleri (25/30, 35/40, 45/50…) için de aynı fikir işler: düzen içine o bölgenin değişen şeyi konur (ör. Bölge 3'te panellerin gücü, Bölge 4'te toz bulutunun yeri). Bu belge yalnız 15 ve 20'yi kapsıyor; ötekiler 15 ve 20 oynandıktan sonra ayrı karar.
