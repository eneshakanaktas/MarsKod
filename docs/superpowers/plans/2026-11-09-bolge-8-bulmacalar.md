# Bölge 8 Bulmacaları (Bölüm 71-80) Uygulama Planı

> **Yürütme:** Görevleri sırayla yap; adımlar onay kutusu (`- [ ]`) biçiminde. Yardımcı ajan (subagent) yok, işi aynı sohbet yapar (Enes'in kuralı: "ajansız çalış").
> **Model · efor:** Sonnet 5.5 · high (~5 sa). Bölge 8'de iki yeni bulmaca türü ilk kez kuruluyor (spec "İş sırası" 1). Görev 5 (sahne) zorlaşırsa Opus 5.5'e geçmeyi öner.

**Goal:** Bölüm 71-80'i (`return` + metin) oynanır hâle getirmek: dönüş şartı, ekran bulmacası (B), okuyan robot (C, `read_note()`), istasyon bölgesi, veri kartı `V`, 10 bölüm dosyası, 2 sınav, sözlükte 5 sayfa.

**Architecture:** Bütün kurallar saf C# dünya tarafında (`oyun/Assets/Dunya/`). Dönüş şartı `RoutineRule`'a eklenir; `ProgramRun` kodun ağacından `return` eden rutinleri okur. Ekran bulmacası `ScreenPuzzle` sınıfıdır: `ProgramRun` oyuncunun kodunu çalıştırdıktan sonra aynı yorumlayıcıda her satır için `__cevap__ = fonksiyon(__girdi__)` çalıştırır, cevapları kaydeder. Okuyan robot için notlar Bölge 7'nin değişen yol düzenine girer: `Level.RunVersions` kodu her notta yeni bir dünyada çalıştırır (`TrailRuns.Pick` aynı kalır), notun harfleri (`NoteLetters`) bir `Trail` yoluna çevrilir, hedef yolun sonudur. Sahne (`Oyun.cs`, `Hud.cs`) yalnızca kayıtları gösterir: ekran = sade bir kutu (`ScreenView`), not = robotun üstünde geçici yazı; asıl görünüş 3. parçada.

**Tech Stack:** Unity 6 (C# 9, .NET Standard 2.1), kendi mini-Python motoru (`oyun/Assets/Motor/`), xUnit (`motor-test/`, `dotnet test`; başlangıçta 1463 test).

**Spec:** `docs/superpowers/specs/2026-11-09-bolge-8-10-ders-bulmaca-design.md` (Bölge 8 bölümü, "Bulmaca türleri", "Tutarlılık kuralları", "Yeni kurallar ve kod").

## Global Constraints

- Motor ve dünya kodu saf C#: `UnityEngine` yok, C# 9, .NET Standard 2.1 (motor-test derlerken yakalar).
- Kullanıcıya görünen bütün metinler sade Türkçe; kod parçaları ters tırnak içinde.
- Haritalar 6×6. Bölge 8'de toplanacak varsa o `V` (veri kartı); bir bölümde tek tür.
- **Komutlar değişmez:** robot her zaman `move(North)`, `move(East)`... ile yürür. Notlardaki yön harfleri Türkçe: `K` kuzey → `North`, `G` güney → `South`, `D` doğu → `East`, `B` batı → `West` (spec, Enes onayı).
- Bir bölümde en çok bir yeni şey; yeni komut (`read_note`) yeni Python konusuyla aynı bölümde açılmaz (spec "Zorluk eğrisi").
- Yalnızca öğretilmiş şeyle çözülür (spec tutarlılık 1): 71-80'in çözümü ve başlangıç kodu yalnızca o bölüme kadar açılmış Python kelimelerini kullanır (Görev 4 testi).
- Oyuncunun yazacağı her işaret oyun klavyesinde var (`CodeKeyboard.cs`: Türkçe harfler, `"`, `==`, `+=`). Veri metinlerindeki `—` işaretini oyuncu yazmak zorunda kalmaz.
- Acemi kademesi: her çözüm satırı ya başlangıç kodunda ya `parcalar`'da (sayıları farklı olabilir) bulunur; tırnak içindeki harfler düğmede değiştirilemediği için doğru yazılır (Görev 4 testi).
- Bölüm 71-80'de `etiket`, `giris`, `bitis` yazılmaz (hikâye 2. parçada). Kayıt 4, 24 ismin son hâli, çanta eşyası (76 katlanmış çizim) 2./3. parçada.
- Yeni `.cs` dosyasının `.meta`'sı Unity paketi üretilince oluşur, git'e onunla girer.
- **Commit kuralı (CLAUDE.md, ekip döngüsü):** görev başına commit yok; oturum kapanışında ILERLEME.md ile birlikte tek commit + push. (Aşağıdaki görevlerde "commit" adımı bu yüzden yok.)
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıf/fonksiyon, tekrar yok, okunur adlar, gereksiz katman yok.
- Denetimi ölçülü yap (CLAUDE.md): görev aralarında yalnızca `dotnet test`; paket ve görüntü turu yalnızca Görev 8'de.

## Plan sırasında verilen kararlar (spec'e ek / düzeltme)

1. **77'de aranan kelime `"hazır"` değil `"Aras"`.** Spec'e göre 24 satırın hepsi `kapsül: hazır` (`senaryo-perde-2.md`); o zaman her satıra `True` diyen kod (`return True`) da geçerdi, bulmaca bir şey sınamazdı. Ders aynı (`in` ile metinde aramak); Kıvılcım listede Defne'yi ve Ece'yi arar, ekranda ikisinin satırı vurgulanır. **Enes onayladı (2026-11-09): Ece'nin soyadı Aras** (`hikaye-kitabi.md`'ye işlendi).
2. **24 isim geçici (Claude'un önerisi).** Bulmaca satırları isim istiyor ama isimler spec'e göre 2. parçada yazılacaktı. Bu plan 24 ismi şimdi yazar (Defne Aras ilk, Ece Aras ikinci satır; içinde "Aras" ya da "dış" geçen başka isim yok). 2. parça isimleri değiştirebilir; Görev 4'teki test 77'nin 24 satırının 78'in ilk 24 satırıyla harfi harfine aynı olduğunu, 78'in son iki satırının `senaryo-perde-2.md`'dekiyle aynı olduğunu denetler.
3. **Yeni kural `donus_sarti` (dönüş şartı).** Bölge 7'nin `parametre_sarti`'na karşılık: şarta giren rutinin içinde değer döndüren bir `return` olmalı. Yoksa oyuncu `return` yazmadan da (ör. `range(3)` ile) bölümü geçerdi. Değer döndüren rutin doğal olarak bir kez çağrılır (`report(kac_kez(...))`), bu yüzden dönüş şartında rutinin **bir kez** çalışması yeter (öteki rutinlerde 2 kez kuralı aynen kalır). Denetim kodun ağacına bakar (ifade içindeki `return` sayılır, iç içe tanımlı rutinin `return`'ü dıştakine sayılmaz, boş `return` sayılmaz).
4. **Ekran bulmacası alanı `ekran`:** `{"fonksiyon": "...", "satirlar": [{"girdi": ..., "cevap": ..., "gorunur": true}, ...]}`. Satırlar ekranda göründüğü sırayla tek listede durur (77-78'de liste sırası önemli); `gorunur: true` olanlar görev kartında baştan görünür, ötekiler gizli (yalnızca çalıştırınca). Denetleyici: en az 3 satır, en az 1 görünür, en az 1 gizli, cevapların hepsi aynı olamaz (yoksa `return True` gibi tek cevap geçer). Girdi ve cevap: metin, tam sayı ya da `true/false`. Cevap karşılaştırması türüyle birlikte yapılır: `True` ile `1`, `"6"` ile `6` aynı sayılmaz.
5. **`None` açıklaması `ScreenPuzzle`'da, `ExplanationsTr`'de değil.** `None` dönmesi Python hatası değil, yanlış cevaptır; mesajı ekran verir ("`None` geldi: `return` unutulmuş olabilir"). Spec'in `ExplanationsTr` için istediği öteki açıklamalar zaten var ve doğrulandı: `None` ile işlem (`TipNone`, satır 41/476), `"4" + 1` (satır 452), listenin dışı (satır 735). Bu planda `ExplanationsTr` değişmez.
6. **Notlar (`notlar`) Bölge 8'de yalnızca yön notudur:** harfleri `K G D B` (yürü) ve `V` ("veri kartı burada", yalnızca 80'de; robot yürümez). Hedef notun yolunun sonudur, haritaya `H` yazılmaz (Bölge 7'deki `izler` gibi). Notların sonları birbirinden farklı olmalı (yoksa ezber yol da geçer). Metin listesi veren notlar (84, 87) Bölge 9 planında eklenir.
7. **76'nın notları aynı yere çıkmaz ama hikâyeyle çelişmez:** üç notun sonu üst sıradaki üç farklı kare (1,5), (2,5), (5,5); hepsi kuzey duvarının önünde (yatağın yanı). 3. parçada katlanmış çizim, oynanan notun sonunda belirir; yatak kuzey duvarı boyunca durur.
8. **`upper` Bölge 8'de yok.** Doğal bir yeri çıkmadı; büyük-küçük harf farkı 77'de (`"aras"` tipik hatası) öğretiliyor. İstenirse 99'da kullanılabilir.
9. **`not` Bölüm 32'den beri açık** (`bolum-32.json`: `while not rock_ahead(East):`). Spec 73 için "`not` henüz yok", 92 için "`not` yeni" diyor; ikisi de eski bilgiye dayanıyor. Bölge 8'i etkilemez (çözümler `not` kullanmıyor); **Bölge 10 planında** 92 "`not` hatırlatma" olarak yeniden düşünülmeli. `and`/`or` hiçbir bölümde kullanılmadı (doğrulandı).
10. **75'in kod ipucu `move(North)` kalıbını gösterir, 76'nınki `return North`.** 75'te henüz fonksiyon yok (rota başlangıç kodunda, harf doğrudan `move`'a çevrilir); `return North` kalıbı 76'da gelir. K/G/D/B ipucu (Türkçe harf → İngilizce komut) ikisinde de **ilk ipucu**, spec'teki cümleyle.
11. **Ekran bulmacasında `komutlar` boş** (robot yürümez). Komut çağıran oyuncuya "Bu bölümde robotun komutu yok: iş ekranda" denir (`World.Unlocked`).
12. **Geçici görünüşler:** istasyon = kanyonun ışığı + engel olarak kasalar (`Obstacles.Crates`); veri kartı = enerji hücresinin görünüşü; not = robotun üstünde açık renk yazı (`PowerLabel`; dünyada bilgisayar yazısı kuralına aykırı, 3a'da kâğıt not); ekran = kod kartının üstünde sade kutu (`ScreenView`). 73-74'teki paneller istasyonun enerji panoları (3a'da duvar panosu görünüşü).
13. Haritalar, çözümler ve tipik hatalar bu plan yazılırken ayrı bir benzetimle (gerçek Python 3.12) denendi: her çözüm her notta biter; 46 tipik hatanın hepsi en az bir notta ya da satırda, açıklamasındaki sebeple takılır; her çözüm acemi düğmeleriyle kurulabilir. Son söz yine `dotnet test`'tir.

## Review Focus

- Ekran çağrısının kendi hatası (fonksiyonun bilgi kutusu yok: `TypeError`) **`def` satırında** görünmeli, oyuncunun kodunun 1. satırında değil → Görev 2 testi `Cagri_hatasi_def_satirinda_gorunur`.
- Ekran fonksiyonu içinde bitmeyen döngü oyunu dondurmamalı: adım sınırı çağrılar boyunca da sayılır → Görev 2 testi `Bitmeyen_dongu_durdurulur`.
- `True` ile `1`, `"6"` ile `6` aynı cevap sayılmamalı ve mesaj tür farkını söylemeli → Görev 2 testleri `Tur_farki_ayri_soylenir`, `Cevap_turuyle_karsilastirilir`.
- `move("K")` ve `move(None)` "Bilinmeyen yön" yerine kendi açıklamasını vermeli (en sık iki 76 hatası) → Görev 3 testi `Harf_ve_None_yon_kendi_mesajini_verir`.
- Notlu bölümde toplanacak bir şey yokken "Haritada görev yok" hatası verilmemeli (76) ve ekranlı bölümde de (72, 77, 78) → Görev 2 ve 3'teki bölüm okuma testleri.

---

### Görev 1: Dönüş şartı (`donus_sarti`)

**Files:**
- Modify: `oyun/Assets/Dunya/RoutineRule.cs` (yapıcıya `needsReturn`, `RequiredRuns`, `ReturnProblem`)
- Modify: `oyun/Assets/Dunya/ProgramRun.cs` (`RunReport.ReturningRoutines`, kodun ağacından okunur)
- Modify: `oyun/Assets/Dunya/Level.cs` (`"donus_sarti"` alanı, `ReadRoutines`)
- Modify: `docs/tasarim/bolum-dosyasi.md` (alan tablosuna `donus_sarti`)
- Test: `motor-test/ReturnTests.cs` (yeni), `motor-test/BolumTests.cs`

**Interfaces:**
- Produces:
  - `RoutineRule(IEnumerable<string> names, int minCount, bool needsParameters = false, bool needsReturn = false)`, `public readonly bool NeedsReturn`, `public int RequiredRuns => NeedsReturn ? 1 : MinRuns`
  - `RunReport.ReturningRoutines : HashSet<string>` (modül düzeyindeki `def`'lerden içinde değerli `return` olanlar)

- [ ] **Step 1: Failing test yaz** — `motor-test/ReturnTests.cs`:

```csharp
using MarsKod.Dunya;

namespace MotorTest;

// Dönüş şartı (Bölge 8, return): şarta giren rutin cevabını return ile vermeli; değer döndüren rutin bir kez çağrılsa yeter.
public class ReturnTests
{
    // Robot (0,0); toplanacak (0,3): kuzeye 3 adım
    static World Corridor() => new World(6, 6, new Cell(0, 0), new[] { new Cell(0, 3) }, null, null, new[] { "move", "collect" });
    static readonly RoutineRule Koridor = new RoutineRule(new[] { "koridor" }, 0, needsReturn: true);
    const string Walk = "for i in range(koridor()):\n    move(North)\ncollect()\n";

    static RunReport Run(string code, RoutineRule rule = null) => ProgramRun.Execute(code, Corridor(), routines: rule ?? Koridor);

    [Fact]
    public void Return_eden_rutin_bir_kez_calissa_yeter()
    {
        var report = Run("def koridor():\n    return 3\n" + Walk);
        Assert.True(report.Complete);
        Assert.Contains("koridor", report.ReturningRoutines);
    }

    [Fact]
    public void Return_olmayan_rutin_sayilmaz()
    {
        var report = Run("def koridor():\n    move(North)\nfor i in range(3):\n    koridor()\ncollect()\n");
        Assert.False(report.Complete);
        Assert.Equal("Kod bitti, koridor cevap vermiyor", report.RoutineProblem.Title);
        Assert.Contains("`return`", report.RoutineProblem.Text);
    }

    [Fact]
    public void Kosul_icindeki_return_sayilir()
    {
        Assert.True(Run("def koridor():\n    if True:\n        return 3\n    return 0\n" + Walk).Complete);
    }

    [Fact]
    public void Ic_rutinin_return_u_distakine_sayilmaz()
    {
        var report = Run("def koridor():\n    def ic():\n        return 3\n    for i in range(ic()):\n        move(North)\nkoridor()\ncollect()\n");
        Assert.DoesNotContain("koridor", report.ReturningRoutines);
        Assert.Equal("Kod bitti, koridor cevap vermiyor", report.RoutineProblem.Title);
    }

    [Fact]
    public void Bos_return_sayilmaz()
    {
        var report = Run("def koridor():\n    for i in range(3):\n        move(North)\n    return\nkoridor()\ncollect()\n");
        Assert.Equal("Kod bitti, koridor cevap vermiyor", report.RoutineProblem.Title);
    }

    [Fact]
    public void Donus_sarti_yoksa_iki_kez_kurali_aynen_kalir()
    {
        var plain = new RoutineRule(new[] { "koridor" }, 0);
        Assert.Equal(2, plain.RequiredRuns);
        Assert.Contains("yalnızca 1 kez", Run("def koridor():\n    return 3\n" + Walk, plain).RoutineProblem.Title);
        Assert.Equal(1, Koridor.RequiredRuns);
    }
}
```

`BolumTests.cs`'e (`Parametre_sarti_okunur`'un altına, aynı `Mini` kalıbıyla):

```csharp
    [Fact]
    public void Donus_sarti_okunur()
    {
        var level = Level.Parse(Mini.Replace("\"komutlar\"", "\"rutinler\": [\"git\"], \"donus_sarti\": true, \"komutlar\""));
        Assert.True(level.Routines.NeedsReturn);
        Assert.False(Level.Parse(Mini).Routines.NeedsReturn);
    }
```

ve `Bozuk_parametre_sarti_anlasilir_mesaj_verir` Theory'sine iki satır:

```csharp
    [InlineData("\"donus_sarti\": true, ", "rutin şartıyla birlikte")]
    [InlineData("\"rutinler\": [\"git\"], \"donus_sarti\": 1, ", "true ya da false")]
```

- [ ] **Step 2: Çalıştır, başarısız olduğunu gör**

Run: `cd motor-test && dotnet test --filter "ReturnTests|Donus_sarti"`
Expected: derleme hatası (`needsReturn`, `ReturningRoutines`, `RequiredRuns` yok).

- [ ] **Step 3: `RoutineRule.cs`**

Dosya başı yorumuna: "Bölge 8'den itibaren dönüş şartı da olabilir ("donus_sarti"): rutin cevabını `return` ile vermeli; o zaman bir kez çalışması yeter."
- Yapıcıya `bool needsReturn = false`, alan `NeedsReturn`, özellik `RequiredRuns`.
- `Check` içinde `MinRuns` geçen iki yer (`n < MinRuns` ve `repeated` süzgeci) → `RequiredRuns`. "yalnızca n kez çalıştı" mesajı aynı kalır.
- `ParameterProblem(report, name)` çağrılan iki yer → yeni `RuleProblem(report, name) => ParameterProblem(report, name) ?? ReturnProblem(report, name)`.
- `ReturnProblem`: `NeedsReturn` kapalıysa ya da `report.ReturningRoutines` adı içeriyorsa `null`; değilse:
  - Title: `"Kod bitti, " + name + " cevap vermiyor"`
  - Text: `"`" + name + "` içinde değer veren bir `return` yok: rutin iş yapıyor ama cevabını vermiyor. Cevabı `return` ile ver (örn. `return 3`); `print` ekrana yazar, cevap vermez."`

- [ ] **Step 4: `ProgramRun.cs`** — `RunReport`'a `public readonly HashSet<string> ReturningRoutines = new HashSet<string>();` (özet: "def ile yazılan rutinlerden içinde değer veren return olanlar (kodun yazısından okunur)"). `Execute` içinde `var module = Parser.Parse(source);` satırının altına `ReadReturningRoutines(report, module);`.

`static void ReadReturningRoutines(RunReport report, Module module)`: `module.Body` içindeki her `FunctionDef` için gövdeyi gez; `Return { Value: not null }` bulunursa adı ekle. Gezerken `If`, `For`, `While` gövdelerine ve `OrElse`'lerine in, iç `FunctionDef`'e inme (iç rutinin cevabı dıştakinin değildir). Küçük özyinelemeli yardımcı `static bool Returns(IEnumerable<Stmt> body)` yeter.

- [ ] **Step 5: `Level.cs`** — `known`'a `"donus_sarti"`. `ReadRoutines` içinde `needsParameters`'ın yanına `bool needsReturn = d.ContainsKey("donus_sarti") && Flag(d, "donus_sarti");`; rutin şartı yokken yazılmışsa `"\"donus_sarti\" yalnızca rutin şartıyla birlikte yazılır: \"rutinler\" ya da \"rutin_sayisi\" de ekle."`. `new RoutineRule(names, count, needsParameters, needsReturn)`. Dosya başı yorumuna bir satır: `"donus_sarti": rutinler cevabını return ile vermeli; bir kez çalışması yeter (Bölge 8).`

- [ ] **Step 6: `bolum-dosyasi.md`** — alan tablosunda `parametre_sarti` satırının altına:

| `donus_sarti` | İsteğe bağlı (Bölge 8'den itibaren, `return`). `true` ise rutin şartına giren her rutinin içinde cevap veren bir `return` olmalı (`return 3`, `return toplam`; boş `return` sayılmaz). Böyle rutin **bir kez** çalışsa yeter (cevabı kullanılır). Yalnızca rutin şartıyla birlikte yazılır. |

- [ ] **Step 7: Çalıştır** — `cd motor-test && dotnet test` → hepsi geçer (1463 + yeniler).

---

### Görev 2: Ekran bulmacası (`ekran`, tür B)

**Files:**
- Create: `oyun/Assets/Dunya/ScreenPuzzle.cs`
- Modify: `oyun/Assets/Dunya/ProgramRun.cs` (`screen` parametresi, adım izleme yerel fonksiyona, `ValueText` açılır)
- Modify: `oyun/Assets/Dunya/Level.cs` (`"ekran"` alanı, `Level.Screen`, "görev yok" denetimi, `RunVersions`'a geçer)
- Modify: `oyun/Assets/Dunya/World.cs` (`Unlocked`: hiç komut açık değilse ayrı mesaj)
- Modify: `docs/tasarim/bolum-dosyasi.md`
- Test: `motor-test/ScreenPuzzleTests.cs` (yeni), `motor-test/LevelJson.cs` (yeni)

**Interfaces:**
- Consumes: Görev 1'deki `RoutineRule` (değişmeden geçer).
- Produces:
  - `public sealed class ScreenRow { public object Input; public object Expected; public bool Visible; }` (değerler motorun türleri: `string`, `BigInteger`, `bool`)
  - `public sealed class ScreenAnswer { public ScreenRow Row; public object Actual; public bool Correct; }`
  - `public sealed class ScreenPuzzle`: `public readonly string Function; public readonly IReadOnlyList<ScreenRow> Rows; public IEnumerable<ScreenRow> VisibleRows`; `public static ScreenPuzzle Parse(object value)` (`"ekran"` alanının değeri; yanlışta `DataFormatError`); `public const string InputName = "__girdi__", AnswerName = "__cevap__"`; `public static bool SameAnswer(object expected, object actual)`; `public RoutineProblem MissingFunction()`; `public RoutineProblem ProblemOf(IReadOnlyList<ScreenAnswer> answers)` (hepsi doğruysa null)
  - `ProgramRun.Execute(string source, World world, int maxSteps = DefaultMaxSteps, RoutineRule routines = null, ScreenPuzzle screen = null)`; `RunReport.ScreenAnswers : List<ScreenAnswer>`, `RunReport.ScreenProblem : RoutineProblem`; `Complete` artık `ScreenProblem == null` da ister
  - `public static string ProgramRun.ValueText(object value)` (eski özel `ArgumentText`; yön `East`, gerisi Python görünüşü `'abc'`, `True`, `None`)
  - `Level.Screen : ScreenPuzzle` (yoksa null)

> `RoutineProblem` yalnızca başlık + metin taşıyan küçük sınıf; ekran sorunları için de kullanılır (aynı iki alan için ikinci sınıf açılmaz).

- [ ] **Step 1: Failing test yaz** — `motor-test/ScreenPuzzleTests.cs`:

```csharp
using System.Numerics;
using MarsKod.Dunya;

namespace MotorTest;

// Ekran bulmacası (Bölge 8, tür B): robot yürümez; oyun oyuncunun fonksiyonunu her satırın girdisiyle çağırır, cevabı kaydeder.
public class ScreenPuzzleTests
{
    static ScreenPuzzle Double() => ScreenPuzzle.Parse(Json.Parse(
        "{\"fonksiyon\": \"iki_kat\", \"satirlar\": [{\"girdi\": 3, \"cevap\": 6, \"gorunur\": true}, {\"girdi\": 7, \"cevap\": 14}, {\"girdi\": 0, \"cevap\": 0}]}"));
    static ScreenPuzzle Contains() => ScreenPuzzle.Parse(Json.Parse(
        "{\"fonksiyon\": \"aras_mi\", \"satirlar\": [{\"girdi\": \"Defne Aras\", \"cevap\": true, \"gorunur\": true}, {\"girdi\": \"Kerem Yalın\", \"cevap\": false}, {\"girdi\": \"Ece Aras\", \"cevap\": true}]}"));
    static World Still() => new World(6, 6, new Cell(2, 1), new Cell[0], commands: new string[0]);
    static RunReport Run(string code, ScreenPuzzle screen = null) => ProgramRun.Execute(code, Still(), screen: screen ?? Double());

    [Fact]
    public void Dogru_fonksiyon_her_satiri_dogru_yanitlar()
    {
        var report = Run("def iki_kat(sayi):\n    return sayi * 2\n");
        Assert.True(report.Complete);
        Assert.Equal(3, report.ScreenAnswers.Count);
        Assert.All(report.ScreenAnswers, a => Assert.True(a.Correct));
        Assert.Equal(new BigInteger(14), report.ScreenAnswers[1].Actual);
        Assert.Equal(3, report.RoutineRuns["iki_kat"]);   // ekranın çağrıları rutin sayımına girer
    }

    [Fact]
    public void Print_eden_fonksiyon_None_mesaji_alir_ve_butun_satirlar_sorulur()
    {
        var report = Run("def iki_kat(sayi):\n    print(sayi * 2)\n");
        Assert.False(report.Complete);
        Assert.Equal(3, report.ScreenAnswers.Count);
        Assert.Contains("`None` geldi", report.ScreenProblem.Text);
        Assert.Contains("`return`", report.ScreenProblem.Text);
        Assert.Equal("6\n14\n0\n", report.Output);
    }

    [Fact]
    public void Ezber_gizli_satirda_takilir_ve_bunu_soyler()
    {
        var report = Run("def iki_kat(sayi):\n    if sayi == 3:\n        return 6\n");
        Assert.True(report.ScreenAnswers[0].Correct);
        Assert.False(report.ScreenAnswers[1].Correct);
        Assert.Contains("görev kartında yoktu", report.ScreenProblem.Text);
    }

    [Fact]
    public void Fonksiyon_yoksa_anlasilir_mesaj()
    {
        var report = Run("iki_kat = 5\n");
        Assert.False(report.Complete);
        Assert.Empty(report.ScreenAnswers);
        Assert.Equal("Ekran: `iki_kat` fonksiyonu yok", report.ScreenProblem.Title);
    }

    [Fact]
    public void Tur_farki_ayri_soylenir()
    {
        var report = Run("def iki_kat(sayi):\n    return str(sayi * 2)\n");
        Assert.Contains("Tırnaklı", report.ScreenProblem.Text);
    }

    [Fact]
    public void Cevap_turuyle_karsilastirilir()
    {
        Assert.True(ScreenPuzzle.SameAnswer(true, true));
        Assert.False(ScreenPuzzle.SameAnswer(true, BigInteger.One));
        Assert.False(ScreenPuzzle.SameAnswer(new BigInteger(6), "6"));
        Assert.False(ScreenPuzzle.SameAnswer(false, null));
        Assert.True(ScreenPuzzle.SameAnswer("Aras", "Aras"));
    }

    [Fact]
    public void Metin_girdisiyle_in_calisir()
    {
        var report = Run("def aras_mi(satir):\n    return \"Aras\" in satir\n", Contains());
        Assert.True(report.Complete);
        Assert.Equal(new object[] { true, false, true }, report.ScreenAnswers.Select(a => a.Actual).ToArray());
    }

    [Fact]
    public void Cagri_hatasi_def_satirinda_gorunur()
    {
        var report = Run("x = 1\n\ndef iki_kat():\n    return 6\n");
        Assert.Equal("TypeError", report.Error.Type);
        Assert.Equal(3, report.StopLine);
    }

    [Fact]
    public void Fonksiyon_icindeki_hata_kendi_satirinda()
    {
        var report = Run("def iki_kat(sayi):\n    return sayi * 2 + y\n");
        Assert.Equal("NameError", report.Error.Type);
        Assert.Equal(2, report.StopLine);
    }

    [Fact]
    public void Bitmeyen_dongu_durdurulur()
    {
        var report = Run("def iki_kat(sayi):\n    while True:\n        pass\n");
        Assert.Equal("steps", report.Halt.Reason);
    }

    [Fact]
    public void Fonksiyonun_satirlari_kayda_girer_degiskenler_gizli_ad_tasimaz()
    {
        var report = Run("def iki_kat(sayi):\n    return sayi * 2\n");
        Assert.Equal(3, report.Trace.Count(t => t.Line == 2));
        Assert.DoesNotContain(report.Trace.SelectMany(t => t.Vars), v => v.Key.StartsWith("__"));
    }

    [Fact]
    public void Komut_acik_degilse_ekran_mesaji()
    {
        var report = Run("move(North)\n");
        Assert.Equal("Bu bölümde robotun komutu yok", report.Rule.Title);
    }

    [Theory]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 1, \"cevap\": 2, \"gorunur\": true}, {\"girdi\": 2, \"cevap\": 4}]}", "en az 3 satır")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 1, \"cevap\": 2}, {\"girdi\": 2, \"cevap\": 4}, {\"girdi\": 3, \"cevap\": 6}]}", "görünür")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 1, \"cevap\": 2, \"gorunur\": true}, {\"girdi\": 2, \"cevap\": 4, \"gorunur\": true}, {\"girdi\": 3, \"cevap\": 6, \"gorunur\": true}]}", "gizli")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 1, \"cevap\": true, \"gorunur\": true}, {\"girdi\": 2, \"cevap\": true}, {\"girdi\": 3, \"cevap\": true}]}", "hepsi aynı")]
    [InlineData("{\"fonksiyon\": \"move\", \"satirlar\": []}", "oyun komutu")]
    [InlineData("{\"fonksiyon\": \"return\", \"satirlar\": []}", "geçerli bir fonksiyon adı")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 2.5, \"cevap\": 5, \"gorunur\": true}, {\"girdi\": 2, \"cevap\": 4}, {\"girdi\": 3, \"cevap\": 6}]}", "tam sayı")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [], \"renk\": 1}", "bilinmeyen")]
    public void Bozuk_ekran_alani_anlasilir_mesaj_verir(string json, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => ScreenPuzzle.Parse(Json.Parse(json)));
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Ekranli_bolum_okunur_haritada_gorev_aranmaz()
    {
        var level = Level.Parse(LevelJson.Screen());
        Assert.Equal("iki_kat", level.Screen.Function);
        Assert.Single(level.Screen.VisibleRows);
        Assert.True(level.Run(level.Solution).Complete);
        Assert.False(level.Run("def iki_kat(sayi):\n    return 6\n").Complete);
    }
}
```

Ekranlı küçük bölüm metni iki test dosyasında (burada ve Görev 4'te) kullanılır; tek yerde dursun — `motor-test/LevelJson.cs` (yeni):

```csharp
namespace MotorTest;

// Testlerde kullanılan küçük bölüm metinleri (tek yerde)
static class LevelJson
{
    // 72 gibi ekranlı bölüm: robot yürümez, iki_kat(sayi) fonksiyonu sorulur. extra: sona eklenecek alanlar (", \"parcalar\": [...]")
    public static string Screen(string extra = "") =>
        "{\"numara\": 72, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . R . . .\", \". . . . . .\"], "
        + "\"ekran\": {\"fonksiyon\": \"iki_kat\", \"satirlar\": [{\"girdi\": 3, \"cevap\": 6, \"gorunur\": true}, {\"girdi\": 7, \"cevap\": 14}, {\"girdi\": 0, \"cevap\": 0}]}, "
        + "\"komutlar\": [], \"konular\": [\"return\"], \"ipuclari\": [\"x\"], \"cozum\": [\"def iki_kat(sayi):\", \"    return sayi * 2\"]" + extra + "}";
}
```

> `ScreenPuzzleTests` dosyasının başına `using System.Linq;` gerekirse ekle (proje `ImplicitUsings` açıksa gerekmez; `BolumTests.cs` `using` yazmadan `Select` kullanıyor).

- [ ] **Step 2: Çalıştır, başarısız olduğunu gör**

Run: `cd motor-test && dotnet test --filter ScreenPuzzleTests`
Expected: derleme hatası (`ScreenPuzzle` yok).

- [ ] **Step 3: `ScreenPuzzle.cs`** — dosya başı yorumu: "Ekran bulmacası (Bölge 8'den itibaren, tür B): robot yürümez. Oyuncu adı belli bir fonksiyon yazar; oyun onu her satırın girdisiyle çağırır, istasyon ekranına `girdi → cevap → ✓/✗` yazar. Görünen satırlar görev kartında baştan görünür, gizliler yalnızca çalıştırınca: görünenleri ezberleyen kod gizlide takılır. Bölüm dosyasında: "ekran"."

`Parse` kuralları (mesajlar Türkçe, `DataFields.CheckKeys` ile bilinmeyen alan):
- değer nesne değilse: `"\"ekran\" bir nesne olmalı: { \"fonksiyon\": \"...\", \"satirlar\": [...] }."`
- `fonksiyon`: `Level`'daki rutin adı kuralıyla aynı (harf/`_` ile başlar, Python kelimesi değil → `"... geçerli bir fonksiyon adı değil ..."`; oyun komutu → `"... bir oyun komutunun adı ..."`). Bunun için `Level.RoutineName` regex'ini ve iki denetimi `Level`'da `internal static string RoutineNameProblem(string name, string field)` olarak tek yere al; `ReadRoutines` de onu kullansın (tekrar olmasın).
- `satirlar`: liste; her öğe `{girdi, cevap, gorunur?}`; değer metin, `true/false` ya da tam sayı (`double` tam ise `BigInteger`; değilse `"... tam sayı olmalı ..."`).
- en az 3 satır (`"\"ekran\" en az 3 satır içermeli ..."`), en az 1 `gorunur: true` (`"... en az bir satır görünür olmalı (\"gorunur\": true): oyuncu ne beklendiğini görsün."`), en az 1 gizli (`"... en az bir satır gizli olmalı: yalnızca örnekleri ezberleyen kod takılsın."`), cevapların hepsi aynı olamaz (`"... cevapların hepsi aynı: tek bir cevabı ezberleyen kod (return True gibi) geçerdi."`).

`SameAnswer`: ikisi de `null` → true; `bool` ↔ yalnızca `bool`; `BigInteger` ↔ yalnızca `BigInteger` (motor `bool`'u ayrı tür tutar); `string` ↔ sıralı (ordinal) eşitlik; gerisi false.

`MissingFunction()`: Title `"Ekran: `" + Function + "` fonksiyonu yok"`, Text `"Ekran her satır için `" + Function + "(...)` diye soruyor ama kodda bu adla bir fonksiyon yok. `def " + Function + "(...):` ile yaz; ad harfi harfine aynı olmalı."`

`ProblemOf(answers)`: ilk yanlış cevap için (girdi `ProgramRun.ValueText(Input)` ile):
- Title: `"Ekran: " + input + " için yanlış cevap"`
- Text, başta gizli satırsa: `"Bu satır görev kartında yoktu: kod yalnız örneklere göre yazılmış olabilir. "`; sonra
  - cevap `null`: `"`" + Function + "(" + input + ")` için `" + expected + "` bekleniyordu, `None` geldi: fonksiyon cevabını `return` ile vermeli. `print` ekrana yazar ama cevap vermez; `return` yazılmayan yol da `None` döndürür."`
  - tür farklı (ikisi de null değil, `SameAnswer` türden dolayı false): `"... `" + expected + "` bekleniyordu, `" + actual + "` geldi: biri metin, öteki değil. Tırnaklı `\"6\"` metindir, tırnaksız `6` sayıdır, `True` tırnaksız yazılır."`
  - değer farklı: `"... `" + expected + "` bekleniyordu, `" + actual + "` geldi."`
- hepsi doğruysa `null`.

- [ ] **Step 4: `ProgramRun.cs`**

- `ArgumentText` → `public static string ValueText(object value)` (aynı gövde; `RecordArguments` onu çağırır).
- `RunReport`'a: `public readonly List<ScreenAnswer> ScreenAnswers = new List<ScreenAnswer>();` ve `public RoutineProblem ScreenProblem;` (özet: "Ekran bulmacasında yanlış/eksik cevap; ekran yoksa, hepsi doğruysa ya da kod durduysa null"). `Complete` yorumuna "ekran varsa hepsi doğru" ekle.
- `Execute`'a `ScreenPuzzle screen = null` parametresi. `foreach (var step in interpreter.Run(module))` döngüsünün gövdesini yerel fonksiyona al: `void Follow(IEnumerable<Step> steps)` (Flush, yeni `TraceEntry`, `CountCall`, `Trace.Add`); ana kod `Follow(interpreter.Run(module))`, ardından aynı `try` içinde `if (screen != null) AskScreen(screen, interpreter, report, Follow);`. Böylece ekran çağrılarındaki hatalar, adım sınırı ve rutin sayımı ana kodla aynı yoldan geçer.
- `Complete` satırı: `!report.Stopped && world.Complete && report.RoutineProblem == null && report.ScreenProblem == null`.

```csharp
        // Ekran bulmacasi: oyuncunun fonksiyonu her satirin girdisiyle ayni yorumlayicida cagrilir. Cagri satiri,
        // fonksiyonun def satirina denk gelsin diye o kadar bos satirla baslar: cagrinin kendi hatasi (ornegin bilgi
        // kutusu eksik: TypeError) oyuncuya def satirinda gorunur; fonksiyonun icindeki hatalar zaten kendi satirinda.
        static void AskScreen(ScreenPuzzle screen, Interpreter interpreter, RunReport report, Action<IEnumerable<Step>> follow)
        {
            if (!(interpreter.Globals.Vars.TryGetValue(screen.Function, out var value) && value is PyFunction function))
            {
                report.ScreenProblem = screen.MissingFunction();
                return;
            }
            string call = new string('\n', function.Def.Line - 1) + ScreenPuzzle.AnswerName + " = " + screen.Function + "(" + ScreenPuzzle.InputName + ")\n";
            var vars = interpreter.Globals.Vars;
            foreach (var row in screen.Rows)
            {
                vars[ScreenPuzzle.InputName] = row.Input;
                follow(interpreter.Run(Parser.Parse(call)));
                vars.TryGetValue(ScreenPuzzle.AnswerName, out var actual);
                vars.Remove(ScreenPuzzle.InputName);
                vars.Remove(ScreenPuzzle.AnswerName);
                report.ScreenAnswers.Add(new ScreenAnswer { Row = row, Actual = actual, Correct = ScreenPuzzle.SameAnswer(row.Expected, actual) });
            }
            report.ScreenProblem = screen.ProblemOf(report.ScreenAnswers);
        }
```

> Adım sayacı `Interpreter`'ın alanıdır, `Run` onu sıfırlamaz (Interpreter.cs:114, 141): çağrılar ortak sınırı paylaşır. `ReadDefinedRoutines` `Execute`'ın sonunda kalır.

- [ ] **Step 5: `World.cs` `Unlocked`** — `open` listesi boşsa: `throw new GameRuleStop("Bu bölümde robotun komutu yok", name + "() gerçek bir oyun komutu ama bu bölümde robot yürümüyor: iş ekranda. Ekranın sorduğu fonksiyonu yaz; onu ekran çağırır.");` Liste doluysa eski mesaj.

- [ ] **Step 6: `Level.cs`**

- `known`'a `"ekran"`; `public ScreenPuzzle Screen;` (özet: "Ekran bulmacası (Bölge 8'den, tür B); yoksa null").
- `Parse` içinde `ReadMap`'ten **önce** `if (d.ContainsKey("ekran")) level.Screen = ScreenPuzzle.Parse(d["ekran"]);` ve `ReadMap(level, rows, hasTrails: d.ContainsKey("izler") || d.ContainsKey("notlar") || level.Screen != null)` (parametrenin adını `hasOtherTask` yap; "görev yok" mesajına `bir ekran bulmacası ("ekran")` ekle).
- `izler` ile `ekran` birlikte: `"\"ekran\" olan bölümde \"izler\" yazılmaz: robot yürümez."`.
- `RunTrails` → `RunVersions` (Görev 3 notları da bu döngüye bağlar), `ProgramRun.Execute(code, world, routines: Routines, screen: Screen)`. `Run` yorumu: "her sürümde (iz/not; yoksa bir kez) tamamsa tamam". Çağıranlar: `Oyun.cs:824`, `motor-test/TrailTests.cs` (3 yer) → `RunVersions`.
- Dosya başı yorumuna: `"ekran": ekran bulmacası; oyun oyuncunun fonksiyonunu satırların girdisiyle çağırır (Bölge 8, robot yürümez).`

- [ ] **Step 7: `bolum-dosyasi.md`** — alan tablosuna:

| `ekran` | İsteğe bağlı (Bölge 8'den itibaren, **ekran bulmacası**: robot yürümez). `{"fonksiyon": "uyanma_kodu", "satirlar": [{"girdi": 3, "cevap": 6, "gorunur": true}, {"girdi": 7, "cevap": 14}, ...]}`. Oyuncu `fonksiyon` adıyla bir fonksiyon yazar; oyun onu her satırın `girdi`siyle çağırır, ekrana satır satır `girdi → cevap → ✓/✗` yazar; hepsi `cevap`la aynıysa görev biter. `gorunur: true` olan satırlar görev kartında baştan görünür, ötekiler yalnızca çalıştırınca (gizli). Girdi ve cevap: metin (`"Aras"`), tam sayı ya da `true`/`false`; tür de tutmalı (`"6"` ile `6` farklı). En az 3 satır, en az 1 görünür, en az 1 gizli; cevapların hepsi aynı olamaz. Satırlar ekranda yazıldığı sırayla görünür. Haritada yine robot olur (ekranın önünde durur); `komutlar` boş olabilir (`[]`). `True` dönen satırlar ekranda vurgulanır. |

- [ ] **Step 8: Çalıştır** — `cd motor-test && dotnet test` → hepsi geçer.

---

### Görev 3: Okuyan robot (`read_note`, `notlar`, tür C)

**Files:**
- Create: `oyun/Assets/Dunya/NoteLetters.cs`
- Modify: `oyun/Assets/Dunya/World.cs` (`read_note` komutu, `note` yapıcı parametresi, `NoteRead` olayı, `DirectionOf` mesajları)
- Modify: `oyun/Assets/Dunya/Level.cs` (`"notlar"`, `Notes`, `NoteRoutes`, `VersionCount`, `CreateWorld(int version)`)
- Modify: `oyun/Assets/Dunya/Trail.cs` (yalnızca yorum: yol sınıfı artık not yolunu da taşır)
- Modify: `docs/tasarim/bolum-dosyasi.md`
- Test: `motor-test/NoteTests.cs` (yeni)

**Interfaces:**
- Consumes: Görev 2'deki `Level.RunVersions`.
- Produces:
  - `public static class NoteLetters { public const char Collect = 'V'; public static readonly IReadOnlyDictionary<char, Direction> Directions; public static bool IsDirectionLetter(object value); public static List<Direction> Steps(string note); }` (`Steps`: `V` atlanır; bilinmeyen harf için `null` döner, mesajı `Level` yazar)
  - `World(..., Trail trail = null, string note = null)`; `public string ReadNote()`; `public sealed class NoteRead : WorldEvent { public Cell At; public string Text; }`; `AllCommands` sonuna `"read_note"`
  - `Level.Notes : List<string>`, `Level.NoteRoutes : List<Trail>`, `public int VersionCount => Math.Max(1, Math.Max(Trails.Count, Notes.Count))`, `CreateWorld(int version = 0)` (eski `trail` parametresinin adı `version`)

- [ ] **Step 1: Failing test yaz** — `motor-test/NoteTests.cs`:

```csharp
using MarsKod.Dunya;

namespace MotorTest;

// Okuyan robot (Bölge 8, tür C): read_note() notun yazısını verir; notlardaki yön harfleri Türkçe (K G D B), V veri kartı.
// 2-3 not = değişen not: kod her notta ayrı denenir, hedef notun yolunun sonudur.
public class NoteTests
{
    const string Map = "\"harita\": [\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". K . . . .\", \"R . . . . .\"]";
    const string Yon = "def yon(harf):\n    if harf == \"K\":\n        return North\n    elif harf == \"G\":\n        return South\n    elif harf == \"D\":\n        return East\n    elif harf == \"B\":\n        return West\n";
    const string Walk = "for harf in read_note():\n    move(yon(harf))\n";

    static Level Notes(string notes, string extra = "") => Level.Parse("{\"numara\": 76, \"baslik\": \"a\", \"gorev\": \"b\", " + Map + ", \"notlar\": " + notes
        + ", \"komutlar\": [\"move\", \"read_note\"]" + extra + ", \"konular\": [\"metin\"], \"ipuclari\": [\"x\"], \"cozum\": [\"for harf in read_note():\", \"    move(harf)\"]}");

    [Fact]
    public void Harfler_turkce_yonlerdir()
    {
        Assert.Equal(Direction.North, NoteLetters.Directions['K']);
        Assert.Equal(Direction.South, NoteLetters.Directions['G']);
        Assert.Equal(Direction.East, NoteLetters.Directions['D']);
        Assert.Equal(Direction.West, NoteLetters.Directions['B']);
        Assert.Equal(new[] { Direction.North, Direction.East }, NoteLetters.Steps("KVD"));
        Assert.Null(NoteLetters.Steps("KX"));
        Assert.True(NoteLetters.IsDirectionLetter("K"));
        Assert.False(NoteLetters.IsDirectionLetter("North"));
    }

    [Fact]
    public void Not_okunur_ve_olay_birakir()
    {
        var world = new World(6, 6, new Cell(0, 0), new Cell[0], commands: new[] { "move", "read_note" }, note: "KD");
        var report = ProgramRun.Execute("yazi = read_note()\n", world);
        Assert.False(report.Stopped);
        Assert.Contains(report.Trace.SelectMany(t => t.Events), e => e is NoteRead n && n.Text == "KD");
        Assert.Contains(report.Trace.SelectMany(t => t.Vars), v => v.Key == "yazi" && v.Value == "'KD'");
    }

    [Fact]
    public void Harf_ve_None_yon_kendi_mesajini_verir()
    {
        var harf = ProgramRun.Execute("move(\"K\")\n", new World(6, 6, new Cell(0, 0), new Cell[0]));
        Assert.Equal("move yön ister, harf değil", harf.Rule.Title);
        Assert.Contains("move(yon(harf))", harf.Rule.Text);
        var none = ProgramRun.Execute("move(None)\n", new World(6, 6, new Cell(0, 0), new Cell[0]));
        Assert.Equal("Yön yerine None geldi", none.Rule.Title);
        Assert.Contains("return", none.Rule.Text);
        Assert.Equal("Bilinmeyen yön", ProgramRun.Execute("move(5)\n", new World(6, 6, new Cell(0, 0), new Cell[0])).Rule.Title);
    }

    [Fact]
    public void Notun_sonu_hedeftir_ve_her_notta_denenir()
    {
        var level = Notes("[\"KKD\", \"DDK\"]");
        Assert.Equal(2, level.VersionCount);
        Assert.Equal(new Cell(1, 2), level.CreateWorld(0).Target);
        Assert.Equal(new Cell(2, 1), level.CreateWorld(1).Target);
        Assert.Null(level.Target);
        Assert.True(level.Run(Yon + Walk).Complete);
        var memorized = level.RunVersions(Yon + "for harf in \"KKD\":\n    move(yon(harf))\n");
        Assert.True(memorized[0].Report.Complete);
        Assert.False(memorized[1].Report.Complete);
        Assert.True(TrailRuns.Mixed(memorized));
    }

    [Fact]
    public void V_harfi_yurumez()
    {
        Assert.Equal(new Cell(0, 2), Notes("[\"KVK\", \"DDK\"]").CreateWorld(0).Target);
    }

    [Theory]
    [InlineData("[\"KX\"]", "", "bilinmeyen harf")]
    [InlineData("[\"BB\"]", "", "alanın dışına")]
    [InlineData("[\"DK\"]", "", "kayanın üstünden")]
    [InlineData("[\"KKDD\", \"DDKK\"]", "", "aynı yere")]
    [InlineData("[\"KK\", \"KK\"]", "", "iki kez")]
    [InlineData("[]", "", "1-3 not")]
    [InlineData("[\"KK\"]", ", \"izler\": [[\"North\"]]", "birlikte")]
    public void Bozuk_notlar_anlasilir_mesaj_verir(string notes, string extra, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Notes(notes, extra));
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Read_note_ile_notlar_birlikte_yazilir()
    {
        var noCommand = Assert.Throws<DataFormatError>(() => Level.Parse("{\"numara\": 76, \"baslik\": \"a\", \"gorev\": \"b\", " + Map + ", \"notlar\": [\"KD\"], \"komutlar\": [\"move\"], \"konular\": [\"x\"], \"ipuclari\": [\"x\"], \"cozum\": [\"move(North)\"]}"));
        Assert.Contains("read_note", noCommand.Message);
        var noNotes = Assert.Throws<DataFormatError>(() => Level.Parse("{\"numara\": 76, \"baslik\": \"a\", \"gorev\": \"b\", " + Map.Replace("R . .", "R H .") + ", \"komutlar\": [\"move\", \"read_note\"], \"konular\": [\"x\"], \"ipuclari\": [\"x\"], \"cozum\": [\"move(East)\"]}"));
        Assert.Contains("\"notlar\" yok", noNotes.Message);
    }
}
```

> Haritada (1,1) kaya: yukarıdaki notlar onu bilerek atlar (`KKDD`/`DDKK` aynı kareye çıkar, kayaya değmez). `Notes()` içindeki `"izler"` örneği için `scout` açık değil; mesaj "birlikte" sözcüğünü içermeli: `"\"notlar\" ile \"izler\" birlikte yazılmaz: bir bölümde ya Serçe'nin izi ya not değişir."` Bu denetim `ReadTrails`'ın scout denetiminden **önce** yapılmalı.

- [ ] **Step 2: Çalıştır, başarısız olduğunu gör**

Run: `cd motor-test && dotnet test --filter NoteTests`
Expected: derleme hatası (`NoteLetters`, `note:`, `NoteRead`, `VersionCount` yok).

- [ ] **Step 3: `NoteLetters.cs`** — dosya başı yorumu: "Notlardaki yön harfleri (Bölge 8'den): koloni Türkçe konuştuğu için Ece ve Defne yönleri Türkçe kısaltmayla yazmış: K kuzey, G güney, D doğu, B batı. Komutlar değişmez (move(North)); oyuncu harfi yöne çeviren rutini kendisi yazar. V: veri kartı burada (robot yürümez)." Arayüz yukarıda.

- [ ] **Step 4: `World.cs`**

- `NoteRead` olayı (`Scouted`'ın altına; özet: "Robot read_note() ile durduğu yerdeki notu okudu.").
- Alan `readonly string note;` (özet: "Robotun okuyacağı not (Bölge 8); yoksa null"), yapıcının sonuna `string note = null`, `this.note = note;`.
- `public string ReadNote()`: `note == null` ise `GameRuleStop("Burada okunacak not yok", "Kıvılcım'ın okuyabileceği bir not yok, robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil.")`; değilse `Events.Add(new NoteRead { At = Robot, Text = note }); return note;`.
- `AllCommands` sonuna `"read_note"`; `Commands()` içine `scout`'un altına aynı kalıpla `["read_note"]` (argüman yok, metin döner).
- `DirectionOf`: eski mesajdan önce iki dal:
  - `NoteLetters.IsDirectionLetter(value)` → `GameRuleStop("move yön ister, harf değil", "move(\"" + value + "\") çalışmaz: \"" + value + "\" nottaki bir harf, yön değil. Komutlar İngilizce yön ister: North, East, South, West. Harfi önce yöne çeviren rutinine ver: move(yon(harf)).")`
  - `value == null` → `GameRuleStop("Yön yerine None geldi", "move() bir yön bekliyordu ama None (hiçbir şey) geldi. Çoğu zaman bir rutinin return etmediği yoldan gelir: harfi yöne çeviren rutin bu harf için bir şey döndürmüyor olabilir (bir harf unutulmuş ya da print yazılmış).")`

- [ ] **Step 5: `Level.cs`**

- `known`'a `"notlar"`; alanlar `public List<string> Notes = new List<string>();` ("Robotun okuyacağı notlar (Bölge 8); birden çoksa değişen not: kod her notta denenir"), `public List<Trail> NoteRoutes = new List<Trail>();` ("Her notun yolu; hedef yolun sonu"), `VersionCount`.
- `CreateWorld(int version = 0)`: `target: NoteRoutes.Count > 0 ? NoteRoutes[version].End : Target`, `trail: Trails.Count > 0 ? Trails[version] : null`, `note: Notes.Count > 0 ? Notes[version] : null`.
- `RunVersions`: `for (int i = 0; i < VersionCount; i++)`.
- `Parse`'ta `ReadTrails`'ın altına `ReadNotes(level, d)`. `ReadNotes` kuralları (sıra ve mesajlar):
  - `notlar` ile `izler` birlikte → yukarıdaki "birlikte" mesajı (ReadTrails'tan önce bak).
  - `read_note` açık ↔ `notlar` var (`izler`/`scout` kalıbıyla aynı iki mesaj: `"\"komutlar\" içinde \"read_note\" var ama \"notlar\" yok; ..."`, `"\"notlar\" yazılmış ama \"komutlar\" içinde \"read_note\" yok; robot notu okuyamaz."`).
  - haritada `H` varsa: `"\"notlar\" olan bölümde haritaya hedef (H) yazılmaz: hedef notun yolunun sonudur."`
  - 1-3 boş olmayan metin (`"\"notlar\" 1-3 not içeren bir liste olmalı; her not bir metin, örn. [\"KKDD\"]."`).
  - her not: `NoteLetters.Steps(note) == null` → `"\"notlar\" içindeki N. not bilinmeyen harf içeriyor: \"X\". Harfler: K kuzey, G güney, D doğu, B batı, V veri kartı."`; yol `new Trail(level.Robot, steps)`; her karesi alanın içinde (`"... N. not J. adımda alanın dışına çıkıyor."`) ve kaya değil (`"... J. adımda kayanın üstünden geçiyor."`). İz kendini kesebilir (izden farklı: notta `scout` yok, kendini kesmesi sorun değil).
  - aynı not iki kez → `"\"notlar\" içinde aynı not iki kez yazılmış; değişen notta notlar farklı olmalı."`; birden çok notun sonu aynı kare → `"\"notlar\" içindeki notlar aynı yere çıkıyor: değişen notta her not başka bir yere götürmeli, yoksa ezberlenen yol da geçer."`
  - `ReadTrail`'deki alan/kaya denetim döngüsünü `static void CheckPath(Level level, Trail path, string where)` olarak ortak yere al; `ReadTrail` ve `ReadNotes` onu kullansın.
- Dosya başı yorumuna: `"notlar": read_note() ile okunan yön notları (K G D B, V veri kartı); hedef notun yolunun sonu, H yazılmaz. 2-3 not = değişen not.`

- [ ] **Step 6: `Trail.cs`** — dosya başı yorumu: "Kareden kareye yön listesi: Bölge 7'de Serçe'nin gördüğü göç izi (scout() izin sonraki yönünü söyler), Bölge 8'de notun yolu (Level.NoteRoutes). Hedef yolun son karesidir."

- [ ] **Step 7: `bolum-dosyasi.md`** — `komutlar` hücresine `read_note` (robotun okuduğu notun yazısını metin olarak verir, örn. `"KKDB"`; yalnızca `notlar` olan bölümde açılır) ve tabloya:

| `notlar` | İsteğe bağlı (Bölge 8'den itibaren, **okuyan robot**). Robotun `read_note()` ile okuyacağı notlar: `["KKKKKD", "DDDKKKKKB"]`. `komutlar` içinde `read_note` varsa zorunlu, yoksa yazılmaz. Harfler Türkçe: `K` kuzey (`North`), `G` güney (`South`), `D` doğu (`East`), `B` batı (`West`); `V` "veri kartı burada" (robot yürümez, oyuncu `collect()` der). Haritaya `H` yazılmaz: hedef notun yolunun sonudur. Yol kaya üstünden ve alanın dışından geçmez. **2-3 not = değişen not:** oyuncunun kodu her notta ayrı denenir, görev hepsinde bitince biter; notlar farklı olmalı ve **farklı yerlere** çıkmalı. Değişen not bölümünde `tipik_hatalar` içinde ilk notun yazısını koda kopyalayan bir "ezber" çözüm olsun. |

Harita tablosuna `V` satırı Görev 4'te eklenir.

- [ ] **Step 8: Çalıştır** — `cd motor-test && dotnet test` → hepsi geçer.

---

### Görev 4: İstasyon bölgesi, veri kartı `V`, bölüm denetimleri, Python örnekleri

**Files:**
- Modify: `oyun/Assets/Dunya/Collectible.cs`, `oyun/Assets/Dunya/Region.cs`
- Modify: `oyun/Assets/Dunya/Level.cs` (`PieceProblems`: `return` ile başlayan parça)
- Create: `tests/python-cases/03-dongu/metin-turkce.py`, `tests/python-cases/04-fonksiyon/return-dogru-yanlis.py` (+ `.json`'ları betik üretir)
- Modify: `docs/tasarim/bolum-dosyasi.md` (harita tablosuna `V`)
- Test: `motor-test/BolumTests.cs`

**Interfaces:**
- Produces: `Collectible.DataCard` (işaret `'V'`, ad "veri kartı" / "veri kartları"), `Region.Station` (Bölüm 71-80), `Regions.Item(Region.Station) == Collectible.DataCard`

> `V` hem harita işareti (veri kartı) hem nottaki "veri kartı burada" harfi: ikisi aynı şeyi söyler, çakışma yok.

- [ ] **Step 1: Failing testler** — `BolumTests.cs`:

`Her_on_bolum_yeni_bolge` sonuna:

```csharp
        Assert.Equal(Region.Station, Regions.Of(71));
        Assert.Equal(Region.Station, Regions.Of(80));
        Assert.Equal(Collectible.DataCard, Regions.Item(Region.Station));
        Assert.Equal(Collectible.DataCard, Collectibles.Marks['V']);
        Assert.Equal("veri kartı", Collectibles.Name(Collectible.DataCard));
```

Yeni testler (Bölge 8 bölümleri Görev 6-7'de gelince sınarlar; o zamana kadar `n` aralığında dosya yoksa atlarlar):

```csharp
    // Spec tutarlılık 1: Bölge 8'den itibaren çözüm ve başlangıç kodu yalnızca o bölüme kadar açılmış Python kelimelerini kullanır
    [Theory]
    [MemberData(nameof(Files))]
    public void Cozum_yalnizca_acilmis_kelimeleri_kullanir(string file)
    {
        var level = Load(file);
        if (level.Number < 71) return;
        var all = Directory.GetFiles(Dir, "*.json").Select(f => Level.Parse(File.ReadAllText(f))).ToList();
        var open = new HashSet<string>(Suggestions.OpenWords(all, level.Number));
        foreach (var word in PythonWordsIn(level.Solution + "\n" + level.StartCode))
            Assert.True(open.Contains(word), level.Number + ": \"" + word + "\" henüz açılmadı (python_kelimeleri)");
    }

    // Metinleri ve yorumları çıkarıp kalan adlardan Python kelimesi olanlar
    static IEnumerable<string> PythonWordsIn(string code)
    {
        var bare = Regex.Replace(code, "\"[^\"\\n]*\"|'[^'\\n]*'|#.*", "");
        return Regex.Matches(bare, @"[A-Za-z_]\w*").Select(m => m.Value).Where(Suggestions.IsPythonWord).Distinct();
    }

    // Spec tutarlılık 3: acemi düğmeleri çözümü kurabilmeli. Her çözüm satırı ya başlangıç kodunda ya parcalar'da
    // (sayılar farklı olabilir: oyuncu sayıyı düğmede değiştirir; metin değiştirilemez)
    [Theory]
    [MemberData(nameof(Files))]
    public void Parcalar_cozumu_kurar(string file)
    {
        var level = Load(file);
        if (level.Number < 71) return;
        string Norm(string line) => Regex.Replace(line.Trim(), @"\d+", "#");
        var known = new HashSet<string>(level.StartCode.Split('\n').Concat(level.Pieces).Select(Norm));
        foreach (var line in level.Solution.Split('\n').Where(l => l.Trim().Length > 0))
            Assert.True(known.Contains(Norm(line)), level.Number + ": \"" + line.Trim() + "\" düğmelerle kurulamıyor");
    }

    // Spec tutarlılık 4 ve 6: 77'nin 24 kişisi 78'in ilk 24 satırı; 78'in son iki satırı senaryodaki gibi, harfi harfine
    [Fact]
    public void Bolum_77_ve_78_listesi_tutarli()
    {
        var people = Load("bolum-77.json").Screen.Rows.Select(r => (string)r.Input).ToList();
        var all = Load("bolum-78.json").Screen.Rows.Select(r => (string)r.Input).ToList();
        Assert.Equal(24, people.Count);
        Assert.Equal(26, all.Count);
        Assert.Equal(people, all.Take(24));
        Assert.All(people, p => Assert.EndsWith(" — kapsül: hazır", p));
        Assert.Equal("BKM-7   — dışarıda — görev: KAPI", all[24]);
        Assert.Equal("KOR     — hizmet dışı — hangar", all[25]);
        var senaryo = File.ReadAllText(Path.Combine(Cases.Root, "docs", "tasarim", "senaryo-perde-2.md"));
        Assert.Contains(all[24], senaryo);
        Assert.Contains(all[25], senaryo);
    }
```

> Gerekli `using`'ler: `System.Text.RegularExpressions`. `Bolum_77_ve_78_listesi_tutarli` Görev 7'ye kadar dosya bulamaz; bu testi Görev 7'nin Step 1'inde ekle (burada değil). Öteki iki test şimdi eklenir ve 71+ dosya olmadığı için hemen geçer.

`LevelCheck` için (`Denetleyici_...` testlerinin yanına; Görev 2'deki `LevelJson.Screen` ile):

```csharp
    // "return sayi * 3" tek başına fonksiyon dışında geçersizdir; düğme olarak geçerli sayılmalı
    [Fact]
    public void Return_parcasi_gecerli_sayilir()
    {
        var level = Level.Parse(LevelJson.Screen(", \"parcalar\": [\"def iki_kat(sayi):\", \"return sayi * 3\"]"));
        Assert.Empty(LevelCheck.Problems(level));
    }
```

Run: `cd motor-test && dotnet test --filter "Her_on_bolum_yeni_bolge|Return_parcasi"` → derleme hatası / başarısız.

- [ ] **Step 2: Dünya** — `Collectible.cs`: enum'a `DataCard`; `Marks`'a `['V'] = Collectible.DataCard,`; `Names`'e `[Collectible.DataCard] = ("veri kartı", "veri kartları"),`. `Region.cs`: enum'a ve `Order` sonuna `Station`; `Item`'a `case Region.Station: return Collectible.DataCard;`; özet yorumuna "terk edilmiş araştırma istasyonu: veri kartı". `Level.cs` dosya başındaki işaret satırına `V veri kartı`.

`Level.PieceProblems`: `else:`/`elif` dalının yanına `return` ile başlayan parça için `"def _():\n    " + piece + "\n"` (yorum: "return tek başına fonksiyon dışında geçersiz; sınamak için sahte bir def içine konur, parçanın kendisi değişmez").

- [ ] **Step 3: Python örnekleri** (spec tutarlılık 2: motor gerçek Python'la aynı mı)

`tests/python-cases/03-dongu/metin-turkce.py`:

```python
satir = "KOR     — hizmet dışı — hangar"
print("dışarıda" in satir)
print("dış" in satir)
print(len("KOR"))
sayac = 0
for h in "MMTMAAÇ":
    if h == "A":
        sayac += 1
print(sayac)
print("4" == 4)
print("Aras" in "Defne Aras — kapsül: hazır", "aras" in "Defne Aras")
```

`tests/python-cases/04-fonksiyon/return-dogru-yanlis.py`:

```python
def catlak_mi(guc):
    return guc < 50

def aras_mi(satir):
    if "Aras" in satir:
        return True

print(catlak_mi(30), catlak_mi(70))
print(aras_mi("Ece Aras"), aras_mi("Kerem"))
x = aras_mi("Kerem")
print(x is None)
```

Run: `py -3.12 scripts/python_referans.py` → iki `.json` yazılır. Sonra `cd motor-test && dotnet test --filter PythonCases` → geçer (motor aynı sonucu verir; vermiyorsa motor hatasıdır, dur ve kullanıcıya söyle).

- [ ] **Step 4: `bolum-dosyasi.md`** harita tablosuna `C` satırının altına: `| V | Veri kartı (toplanacak; Bölge 8: Bölüm 71-80) |`.

- [ ] **Step 5: Çalıştır** — `cd motor-test && dotnet test` → hepsi geçer.

---

### Görev 5: Sahne — istasyon, ekran kutusu, not, mesajlar (geçici görünüşler)

**Files:**
- Create: `oyun/Assets/Scripts/ScreenView.cs`
- Modify: `oyun/Assets/Scripts/Hud.cs` (ekran kutusu `free` alanına; üç yeni yöntem; veri kartı noktası)
- Modify: `oyun/Assets/Scripts/Oyun.cs` (`LoadLevel`, `ResetLevel`'deki geri alma, `RunProgram`, `Play`, `ShowFailure`)
- Modify: `oyun/Assets/Scripts/Pickup.cs`, `RegionLook.cs`, `Obstacles.cs`

**Interfaces:**
- Consumes: `Level.Screen`, `ScreenPuzzle.VisibleRows`, `RunReport.ScreenAnswers/ScreenProblem`, `NoteRead`, `Level.Notes`, `Level.RunVersions`, `ProgramRun.ValueText`.
- Produces: `public sealed class ScreenView : VisualElement` — `ScreenView(Font mono, Font bold, Color ink, Color accent, Color error, Color hairline)`, `void Show(ScreenPuzzle puzzle)` (başlık `"İSTASYON EKRANI · " + Function + "(...)"`, görünen satırlar `girdi → ?`), `void Clear()` (satırları siler, başlık kalır), `void Add(ScreenAnswer answer)` (satır: girdi · `→` · cevap · ✓/✗; `True` dönen satır accent zeminle vurgulu; gizli satır soluk "gizli" etiketi; eklenince en alta kaydırır), `void Hide()`. Hud: `ShowScreen(ScreenPuzzle p)`, `AddScreenAnswer(ScreenAnswer a)`, `ClearScreen()`, `HideScreen()`.

- [ ] **Step 1: Geçici görünüşler (asıl görünüş Bölge 8 · 3a)**

- `Pickup.cs`: `case Collectible.DataCard: return EnergyCell.Create(parent, localPos, seed); // gecici: veri kartinin kendi gorunusu Bolge 8 gorunus isinde`
- `Hud.cs:423` `BuildDots`: `item == Collectible.EnergyCell || item == Collectible.DataCard ? ... DrawCellDot ...` (yorum `// veri karti: gecici`)
- `RegionLook.cs` `For`: `case Region.Station: // istasyonun ic mekan gorunusu gelene kadar kanyon` `case Region.Canyon: return Canyon;`
- `Obstacles.cs`: `case Region.Station: Crates(parent, pos, index); break; // gecici: istasyonda engeller kasa/dolap`

- [ ] **Step 2: `ScreenView.cs`** — `FullScreenPanel`'deki `Ui.Text`, `Ui.Radius`, `Ui.Border` yardımcılarıyla; koyu yarı saydam kutu (`#0E0D12` %92), kenar `hairline`, yazılar `mono` 34 px; satırlar bir `ScrollView` içinde (spec görsel tutarlılık 2: 26 satır kutudan taşmaz, kayar); kutu `free` alanının içinde, kenarlardan 36 px boşlukla, yüksekliği `free`'nin en çok %90'ı. Uzun girdi (77-78 satırları) tek satırda `…` ile kısalmaz, kutunun genişliğinde alt satıra kayar. `✓` yeşil `#7CF07A`, `✗` `error`. Yazı tipinde olmayan işaret kullanma (sözlük kuralı): `✓ ✗ →` oyunun mono yazı tipinde var mı Unity'de bak; yoksa `OK` / `X` / `->`.

- [ ] **Step 3: `Hud.cs`** — `free` öğesi alan olur (şu an yerel değişken: `var free`), `screenView` onun içine eklenir, başlangıçta gizli. Dört yöntem `screenView`'a geçer.

- [ ] **Step 4: `Oyun.cs`**

- `LoadLevel` ve sıfırlamada (`ResetLevel`'deki `trailView?.Restore();` satırının yanına): `if (level.Screen != null) hud.ShowScreen(level.Screen); else hud.HideScreen();`
- `RunProgram`: `level.RunTrails(code)` → `level.RunVersions(code)`. Kayıt oynatma döngüsünden (`for (int i = 0; i < count; i++)`) sonra, `report.Stopped` dalından önce:

```csharp
        if (level.Screen != null)   // ekran bulmacasi: cevaplar satir satir ekrana dusur
        {
            hud.ClearScreen();
            foreach (var answer in report.ScreenAnswers)
            {
                hud.AddScreenAnswer(answer);
                yield return Tween.Wait(0.08f);
            }
        }
```

- "Görev bitmedi" dalının (`else { hud.SetActiveLine(-1); ...`) ilk koşulu olarak: `if (report.ScreenProblem != null) ShowFailure("EKRAN", report.ScreenProblem.Title, report.ScreenProblem.Text, null, error: false); else if (world.Complete && report.RoutineProblem != null) ...` (eski zincir aynen devam eder).
- Aynı zincirde `level.Trails.Count > 0` dalının altına: `else if (level.Notes.Count > 0) ShowFailure("GÖREV", "Kod bitti, notun sonuna varmadı", "Kodun sonuna kadar çalıştı ama Kıvılcım notun gösterdiği yere varmadı. Notun her harfi bir adım: harfleri sırayla yürü.", null, error: false);`
- `TrailChangedNote` sabiti → özellik: `string VersionChangedNote => level.Notes.Count > 0 ? "Bu denemede not başka bir yol yazıyor." : "Serçe bu sefer başka bir kol gördü.";` (`ShowFailure` onu kullanır; `trailChanged` alanının adı `versionChanged`, yorum "iz ya da not").
- `Play`'e (`Scouted`'ın altına) geçici not görünüşü:

```csharp
            case NoteRead n: // gecici: Bolge 8 · 3a'da robot kagit notu kaldirip okur
                StartCoroutine(PowerLabel.Show(levelRoot, Pos(n.At), n.Text, Mats.Hex("#E8E0C8")));
                yield return Tween.Wait(0.6f);
                break;
```

> Hedef: notlu bölümde `level.Target` null olduğu için hedef görünmez (Bölge 7'deki izler gibi; varınca belirmesi 3a'da). Ekranlı bölümde robot yürümez; kayıtta olay yok, oynatma yalnızca satırları vurgular.

- [ ] **Step 5: Derleme denetimi** — `cd motor-test && dotnet test` (dünya). Unity derlemesi Görev 8'de paketle denetlenir.

---

### Görev 6: Bölüm 71-75 + `sinav-75` + sözlük (`return`, `True / False`, `Metin`)

**Files:**
- Create: `oyun/Assets/Resources/Bolumler/bolum-71.json` … `bolum-75.json`
- Create: `oyun/Assets/Resources/Sinavlar/sinav-75.json`
- Modify: `oyun/Assets/Resources/Sozluk/sozluk.json`

Dosyaları aşağıdaki gibi yaz (benzetimle denendi; elle değiştirirsen `dotnet test` yeniden sınar). Haritada en üst satır kuzey.

- [ ] **Step 1: `bolum-71.json` (İstasyon kapısı · ilk `return`, tür A)**

```json
{
  "numara": 71,
  "baslik": "İstasyon kapısı",
  "gorev": "2 veri kartı topla",
  "harita": [
    ". . . . . .",
    "K . . . . .",
    "V . . V K .",
    ". . . . . .",
    ". K . . . .",
    "R . . . . ."
  ],
  "komutlar": ["move", "collect"],
  "rutinler": ["koridor"],
  "donus_sarti": true,
  "parcalar": ["def koridor():", "return 2", "for i in range(koridor()):", "move(North)", "move(East)", "collect()"],
  "python_kelimeleri": ["return"],
  "konular": ["return", "değer döndüren rutin"],
  "ipuclari": [
    "Koridorlar hep aynı uzunlukta. `koridor()` bu uzunluğu biliyor: `panel_power()` gibi, sorunca cevap veren bir rutin.",
    "`return 3` rutinin cevabıdır: `koridor()` yazılan yere 3 gelir. Bu yüzden `range(koridor())` 3 tur döner.",
    "Kuzey koridorundan sonra `collect()`; sonra `for i in range(koridor()):` altına `    move(East)`, en sonda yine `collect()`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def koridor():", "    print(3)", "for i in range(koridor()):", "    move(North)", "collect()", "for i in range(koridor()):", "    move(East)", "collect()"],
      "aciklama": "print 3'ü ekrana yazar ama cevap vermez: koridor() None (hiçbir şey) olur, range(None) çalışmaz. Cevabı return ile ver."
    },
    {
      "kod": ["def koridor():", "    return 3", "for i in range(koridor):", "    move(North)", "collect()"],
      "aciklama": "Parantezsiz koridor rutinin kendisidir, cevabı değil; range bir sayı ister. Cevabı almak için koridor()."
    },
    {
      "kod": ["def koridor():", "    return 4", "for i in range(koridor()):", "    move(North)", "collect()", "for i in range(koridor()):", "    move(East)", "collect()"],
      "aciklama": "Koridor 3 adım: dördüncü adımda robot kayaya çarpar. return'ün verdiği sayı her çağrıda kullanılır; yanlışsa hepsi yanlış olur."
    },
    {
      "kod": ["def koridor():", "    return 3", "for i in range(koridor()):", "    move(North)", "collect()", "for i in range(koridor()):", "    move(East)"],
      "aciklama": "Robot ikinci kartın üstünde ama collect() unutuldu."
    }
  ],
  "baslangic_kodu": [
    "def koridor():",
    "    return 3",
    "for i in range(koridor()):",
    "    move(North)"
  ],
  "cozum": [
    "def koridor():",
    "    return 3",
    "for i in range(koridor()):",
    "    move(North)",
    "collect()",
    "for i in range(koridor()):",
    "    move(East)",
    "collect()"
  ]
}
```

- [ ] **Step 2: `bolum-72.json` (Bilgisayarlar uyanıyor · ilk ekran bulmacası, tür B)** — 6 satır = 6 bilgisayar (3a'da 6 ekran yanar).

```json
{
  "numara": 72,
  "baslik": "Bilgisayarlar uyanıyor",
  "gorev": "6 bilgisayarı uyandır",
  "harita": [
    ". . . . . .",
    ". . . . . .",
    ". . . . . .",
    ". . . . . .",
    ". . R . . .",
    ". . . . . ."
  ],
  "ekran": {
    "fonksiyon": "uyanma_kodu",
    "satirlar": [
      {"girdi": 3, "cevap": 6, "gorunur": true},
      {"girdi": 10, "cevap": 20, "gorunur": true},
      {"girdi": 7, "cevap": 14},
      {"girdi": 0, "cevap": 0},
      {"girdi": 25, "cevap": 50},
      {"girdi": 1, "cevap": 2}
    ]
  },
  "komutlar": [],
  "parcalar": ["def uyanma_kodu(sayi):", "return sayi * 3"],
  "konular": ["return", "parametre", "ekran bulmacası"],
  "ipuclari": [
    "Robot yürümüyor, işi ekran yapar: her bilgisayar bir sayı söyler, ekran `uyanma_kodu(sayı)` diye sorar. Cevap sayının iki katı (3 için 6).",
    "Parametre girdiyi alır, `return` cevabı geri verir. `print` ekrana yazar ama cevap vermez: ekran o zaman `None` (hiçbir şey) alır.",
    "`def uyanma_kodu(sayi):` altına `    return sayi * 2`. Fonksiyonu sen çağırmazsın, ekran her bilgisayar için çağırır."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def uyanma_kodu(sayi):", "    if sayi == 3:", "        return 6", "    if sayi == 10:", "        return 20"],
      "aciklama": "Görev kartındaki iki örnek doğru ama ekran başka sayılar da soruyor (7, 25...): ezberlenen cevap yetmez, kuralı yaz."
    },
    {
      "kod": ["def uyanma_kodu(sayi):", "    print(sayi * 2)"],
      "aciklama": "Doğru sayı ekrana yazılır ama ekran cevap olarak None alır: print yazar, return cevap verir."
    },
    {
      "kod": ["def uyanma_kodu(sayi):", "    return sayi + 2"],
      "aciklama": "3 için 5 verir: iki katı, 2 eklemek değil. sayi * 2."
    },
    {
      "kod": ["def uyandir(sayi):", "    return sayi * 2"],
      "aciklama": "Ekran uyanma_kodu adlı fonksiyonu arıyor; ad harfi harfine aynı olmalı."
    },
    {
      "kod": ["def uyanma_kodu():", "    return 6"],
      "aciklama": "Fonksiyonun bilgi kutusu (parametre) yok ama ekran ona bir sayı veriyor: Python hata verir (TypeError). def uyanma_kodu(sayi):"
    }
  ],
  "baslangic_kodu": [
    "def uyanma_kodu(sayi):",
    "    print(sayi * 2)"
  ],
  "cozum": [
    "def uyanma_kodu(sayi):",
    "    return sayi * 2"
  ]
}
```

- [ ] **Step 3: `bolum-73.json` (Duvardaki çizimler · `True/False` döndüren rutin, tür A)** — iki çatlak panel (30, 20) çizim duvarının ışığını besler.

```json
{
  "numara": 73,
  "baslik": "Duvardaki çizimler",
  "gorev": "Çatlak panelleri onar",
  "harita": [
    ". . . . . .",
    "R 7 3 8 2 6",
    ". . . . . .",
    ". . K . . .",
    ". . . . K .",
    ". . . . . ."
  ],
  "komutlar": ["move", "panel_power", "repair"],
  "rutinler": ["catlak_mi"],
  "donus_sarti": true,
  "parcalar": ["def catlak_mi():", "return panel_power() < 30", "for i in range(5):", "move(East)", "if catlak_mi():", "repair()"],
  "python_kelimeleri": ["True", "False"],
  "konular": ["return", "True/False döndüren rutin"],
  "ipuclari": [
    "Duvar boyunca 5 panel var, ikisi çatlak (gücü 50'den az). Ana kod hazır; ona soruyu soran `catlak_mi()` rutini eksik.",
    "Karşılaştırma kendisi bir cevaptır: `panel_power() < 50` ya `True` ya `False` olur. Rutin bu cevabı `return` ile verir, `if` onu kullanır.",
    "Ana kodun üstüne: `def catlak_mi():` ve altına `    return panel_power() < 50`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def catlak_mi():", "    print(panel_power() < 50)", "for i in range(5):", "    move(East)", "    if catlak_mi():", "        repair()"],
      "aciklama": "catlak_mi() True/False'u ekrana yazar ama cevap vermez: if None'a bakar, None yanlış sayılır ve hiçbir panel onarılmaz."
    },
    {
      "kod": ["def catlak_mi():", "    return panel_power() > 50", "for i in range(5):", "    move(East)", "    if catlak_mi():", "        repair()"],
      "aciklama": "Soru ters: rutin sağlam panel için True der, robot sağlam paneli onarmaya kalkıp durur."
    },
    {
      "kod": ["def catlak_mi():", "    return panel_power() < 50", "for i in range(5):", "    move(East)", "    if catlak_mi:", "        repair()"],
      "aciklama": "Parantezsiz catlak_mi rutinin kendisidir, cevabı değil; if onu hep doğru sayar ve robot sağlam paneli de onarmaya kalkar."
    },
    {
      "kod": ["for i in range(5):", "    move(East)", "    if panel_power() < 50:", "        repair()"],
      "aciklama": "Paneller onarılır ama bu bölüm soruyu bir rutine koymanı istiyor: catlak_mi()."
    }
  ],
  "baslangic_kodu": [
    "for i in range(5):",
    "    move(East)",
    "    if catlak_mi():",
    "        repair()"
  ],
  "cozum": [
    "def catlak_mi():",
    "    return panel_power() < 50",
    "for i in range(5):",
    "    move(East)",
    "    if catlak_mi():",
    "        repair()"
  ]
}
```

- [ ] **Step 4: `bolum-74.json` (Kayıt 4 · dönen değeri değişkene almak, tür A)** — alt sıra 50+60+70+80 = 260, üst sıra (batıya) 70+60+50+90 = 270; anten 530 bekler.

```json
{
  "numara": 74,
  "baslik": "Kayıt 4",
  "gorev": "Toplam gücü gönder",
  "harita": [
    ". . . . . .",
    ". . K . . .",
    ". . . . . K",
    ". . . . . .",
    "9 5 6 7 . .",
    "R 5 6 7 8 ."
  ],
  "komutlar": ["move", "panel_power", "report"],
  "rapor": 530,
  "rutinler": ["tara"],
  "donus_sarti": true,
  "parcalar": ["def tara(yon):", "toplam = 0", "for i in range(4):", "move(yon)", "toplam += panel_power()", "return toplam", "alt = tara(East)", "ust = tara(West)", "move(North)", "report(alt + ust)"],
  "konular": ["return", "dönen değeri değişkene almak"],
  "ipuclari": [
    "Robot iki sırayı tarar: alttakini doğuya, bir kuzeye çıkıp üsttekini batıya. Anten iki sıranın toplam gücünü bekliyor.",
    "`tara` toplamı hesaplıyor ama kimseye vermiyor. Döngü bitince `return toplam` yaz: `for` ile aynı hizada, döngünün içinde değil. Cevabı bir değişkene al: `alt = tara(East)`.",
    "Rutinden sonra: `alt = tara(East)`, `move(North)`, `ust = tara(West)`, en sonda `report(alt + ust)`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def tara(yon):", "    toplam = 0", "    for i in range(4):", "        move(yon)", "        toplam += panel_power()", "        return toplam", "alt = tara(East)", "move(North)", "ust = tara(West)", "report(alt + ust)"],
      "aciklama": "return döngünün içinde: rutin ilk panelde cevap verip biter, sıranın gerisi taranmaz. return'ü for ile aynı hizaya al."
    },
    {
      "kod": ["def tara(yon):", "    for i in range(4):", "        toplam = 0", "        move(yon)", "        toplam += panel_power()", "    return toplam", "alt = tara(East)", "move(North)", "ust = tara(West)", "report(alt + ust)"],
      "aciklama": "toplam her turda sıfırlanıyor: rutin yalnızca son panelin gücünü verir. toplam = 0 döngüden önce."
    },
    {
      "kod": ["def tara(yon):", "    toplam = 0", "    for i in range(4):", "        move(yon)", "        toplam += panel_power()", "    print(toplam)", "alt = tara(East)", "move(North)", "ust = tara(West)", "report(alt + ust)"],
      "aciklama": "tara toplamı ekrana yazıyor ama cevap vermiyor: alt ve ust None olur, None'lar toplanamaz (TypeError). return toplam."
    },
    {
      "kod": ["def tara(yon):", "    toplam = 0", "    for i in range(4):", "        move(yon)", "        toplam += panel_power()", "    return toplam", "report(tara(East))", "move(North)", "report(tara(West))"],
      "aciklama": "Anten tek sayı bekliyor: iki sıranın toplamı. Önce iki cevabı topla, sonra bir kez gönder."
    }
  ],
  "baslangic_kodu": [
    "def tara(yon):",
    "    toplam = 0",
    "    for i in range(4):",
    "        move(yon)",
    "        toplam += panel_power()"
  ],
  "cozum": [
    "def tara(yon):",
    "    toplam = 0",
    "    for i in range(4):",
    "        move(yon)",
    "        toplam += panel_power()",
    "    return toplam",
    "alt = tara(East)",
    "move(North)",
    "ust = tara(West)",
    "report(alt + ust)"
  ]
}
```

- [ ] **Step 5: `bolum-75.json` (Dışarıda tek robot · metne giriş, tür A)** — rota `KKDDDKKB`; D/B ya da K/G karıştıran robot alanın sınırına çarpar.

```json
{
  "numara": 75,
  "baslik": "Dışarıda tek robot",
  "gorev": "Çizimdeki yolu yürü",
  "harita": [
    ". . . . . .",
    ". K H . K .",
    ". . K . . .",
    ". . . . . .",
    ". . . . . .",
    "R K . . . ."
  ],
  "komutlar": ["move"],
  "parcalar": ["for harf in rota:", "if harf == \"K\":", "elif harf == \"D\":", "elif harf == \"B\":", "move(North)", "move(East)", "move(West)"],
  "python_kelimeleri": ["str"],
  "konular": ["metin", "metinde dolaşma", "=="],
  "ipuclari": [
    "Nottaki harfler Türkçe: K kuzey → `North`, G güney → `South`, D doğu → `East`, B batı → `West`.",
    "Tırnak içindeki yazı bir metindir. `for harf in rota:` metnin harflerini sırayla verir; `harf == \"K\"` harfin K olup olmadığına bakar (`=` değil, `==`).",
    "Döngünün içinde: `if harf == \"K\":` / `    move(North)`, sonra `elif harf == \"D\":` / `    move(East)`; B için de aynısı."
  ],
  "tipik_hatalar": [
    {
      "kod": ["rota = \"KKDDDKKB\"   # çizimin altındaki yazı", "for harf in rota:", "    if harf == \"K\":", "        move(North)", "    elif harf == \"D\":", "        move(West)", "    elif harf == \"B\":", "        move(East)"],
      "aciklama": "Doğu ile batı karışmış: D doğu, yani East; B batı, yani West. Robot alanın sınırına çarpar."
    },
    {
      "kod": ["rota = \"KKDDDKKB\"   # çizimin altındaki yazı", "for harf in rota:", "    if harf == K:", "        move(North)"],
      "aciklama": "Tırnaksız K bir değişken adıdır, harf değil: Python onu tanımaz (NameError). Harf tırnak içinde yazılır: \"K\"."
    },
    {
      "kod": ["rota = \"KKDDDKKB\"   # çizimin altındaki yazı", "for harf in rota:", "    move(harf)"],
      "aciklama": "move bir yön ister (North, East...), harf değil. Harfi önce yöne çevir."
    },
    {
      "kod": ["rota = \"KKDDDKKB\"   # çizimin altındaki yazı", "for harf in \"rota\":", "    if harf == \"K\":", "        move(North)", "    elif harf == \"D\":", "        move(East)", "    elif harf == \"B\":", "        move(West)"],
      "aciklama": "\"rota\" tırnak içinde: değişken değil, r-o-t-a harflerinden oluşan yeni bir metin. Hiçbir harf K, D ya da B değil; robot kıpırdamaz."
    },
    {
      "kod": ["rota = \"KKDDDKKB\"   # çizimin altındaki yazı", "for harf in rota:", "    if harf = \"K\":", "        move(North)"],
      "aciklama": "Tek = bir değere ad verir; karşılaştırma iki eşittir ister: harf == \"K\" (SyntaxError)."
    }
  ],
  "baslangic_kodu": [
    "rota = \"KKDDDKKB\"   # çizimin altındaki yazı",
    "for harf in rota:",
    "    if harf == \"K\":",
    "        move(North)"
  ],
  "cozum": [
    "rota = \"KKDDDKKB\"   # çizimin altındaki yazı",
    "for harf in rota:",
    "    if harf == \"K\":",
    "        move(North)",
    "    elif harf == \"D\":",
    "        move(East)",
    "    elif harf == \"B\":",
    "        move(West)"
  ]
}
```

- [ ] **Step 6: `sinav-75.json`** (çerçeve önceki sınavlar gibi; spec: `return` ile `print` farkı, `None`, dönen değeri değişkene almak)

```json
{
  "bolum": 75,
  "baslik": "KIVILCIM · KENDİNİ DENETLİYOR",
  "sorular": [
    {
      "soru": "def iki_kat(s): altında return s * 2 yazıyor. sonuc = iki_kat(4) satırından sonra sonuc kaçtır?",
      "secenekler": ["4", "8", "None", "s * 2"],
      "dogru": 1,
      "aciklama": "return'ün verdiği cevap çağrının yerine gelir: iki_kat(4) 8 olur, değişken onu tutar."
    },
    {
      "soru": "def f(): altında yalnızca print(5) var. x = f() satırından sonra x nedir?",
      "secenekler": ["5", "\"5\"", "None", "Hata verir"],
      "dogru": 2,
      "aciklama": "print ekrana 5 yazar ama cevap vermez. return'süz fonksiyon None (hiçbir şey) döndürür."
    },
    {
      "soru": "Ekran bir fonksiyonun cevabını bekliyor. Hangisi cevap verir?",
      "secenekler": ["print(6)", "return 6", "6 yazıp bırakmak", "report(6)"],
      "dogru": 1,
      "aciklama": "Cevabı yalnızca return verir. print yazar, report antene gönderir."
    },
    {
      "soru": "def buyuk_mu(x): altında if x > 5: ve onun altında return True var, başka satır yok. buyuk_mu(3) nedir?",
      "secenekler": ["False", "True", "None", "3"],
      "dogru": 2,
      "aciklama": "x 5'ten büyük değil, return'e hiç gelinmez: fonksiyon None döndürür. Kalan yol için de return False yaz."
    }
  ]
}
```

- [ ] **Step 7: `sozluk.json`** — `scout` sayfasından sonra üç sayfa (kelimelerin açıldığı bölümler: `return` 71, `True`/`False` 73, `str` 75; `Glossary.OpensAt` bulur):

```json
    {
      "baslik": "return",
      "kelimeler": ["return"],
      "aciklama": "Rutinin cevabını verir: return 3 yazan rutin çağrıldığı yere 3 getirir. return çalışınca rutin orada biter. print ekrana yazar ama cevap vermez; return'süz rutin None (hiçbir şey) döndürür.",
      "ornek": [
        "def koridor():",
        "    return 3",
        "for i in range(koridor()):   # 3 tur",
        "    move(North)",
        "def iki_kat(sayi):",
        "    return sayi * 2",
        "x = iki_kat(4)   # x = 8"
      ]
    },
    {
      "baslik": "True / False",
      "kelimeler": ["True", "False"],
      "aciklama": "Doğru ve yanlış. Karşılaştırmalar (<, ==) ve ice_here() gibi sorular bunlardan birini verir. Bir rutin de return ile True ya da False verebilir; if ona bakar. Büyük harfle ve tırnaksız yazılır.",
      "ornek": [
        "def catlak_mi():",
        "    return panel_power() < 50",
        "if catlak_mi():",
        "    repair()"
      ]
    },
    {
      "baslik": "Metin (str)",
      "kelimeler": ["str"],
      "aciklama": "Tırnak içindeki yazı bir metindir (Python'da adı str). for harf in metin: harflerini sırayla verir; == iki metni karşılaştırır; in içinde arar ve True/False verir. Büyük-küçük harf önemlidir. Tırnaklı \"4\" metindir, tırnaksız 4 sayı: ikisi eşit değildir.",
      "ornek": [
        "rota = \"KKDD\"",
        "for harf in rota:",
        "    if harf == \"K\":",
        "        move(North)",
        "\"Aras\" in \"Defne Aras\"   # True",
        "\"4\" == 4   # False"
      ]
    }
```

`for` sayfasının `aciklama`sının sonuna: " Bir metnin harflerinde de dolaşır: for harf in \"KKD\":". `ornek`ine iki satır: `"for harf in \"KKD\":"`, `"    print(harf)   # K, K, D"`.

- [ ] **Step 8: Çalıştır** — `cd motor-test && dotnet test` → hepsi geçer (bölüm denetimi, tipik hatalar, açılmış kelimeler, düğmeler, sözlük uyumu, sınav).

---

### Görev 7: Bölüm 76-80 + `sinav-80` + sözlük (`read_note`, `len`) + belgeler

**Files:**
- Create: `oyun/Assets/Resources/Bolumler/bolum-76.json` … `bolum-80.json`
- Create: `oyun/Assets/Resources/Sinavlar/sinav-80.json`
- Modify: `oyun/Assets/Resources/Sozluk/sozluk.json`, `motor-test/BolumTests.cs`
- Modify: `docs/tasarim/bolum-cozumleri.md`, `docs/tasarim/bolum-dosyasi.md`

- [ ] **Step 1: `BolumTests.cs`** — Görev 4'teki `Bolum_77_ve_78_listesi_tutarli` testini ekle; ayrıca:

```csharp
    // Spec kod çözme bölümleri (Bölge 8): B 72, 77, 78 · C 76, 80; C'de 3 not
    [Fact]
    public void Bolge_8_kod_cozme_bolumleri()
    {
        foreach (var n in new[] { 72, 77, 78 }) Assert.NotNull(Load("bolum-" + n + ".json").Screen);
        foreach (var n in new[] { 76, 80 }) Assert.Equal(3, Load("bolum-" + n + ".json").Notes.Count);
        Assert.Contains("read_note", Load("bolum-76.json").Commands);
        Assert.DoesNotContain("read_note", Load("bolum-75.json").Commands);   // yeni komut yeni Python konusuyla aynı bölümde açılmaz
    }
```

Run: `cd motor-test && dotnet test --filter "Bolum_77|Bolge_8"` → başarısız (dosyalar yok).

- [ ] **Step 2: `bolum-76.json` (Katlanmış çizim · ilk okuyan robot, tür C)** — üç notun sonu (1,5), (2,5), (5,5): üçü de kuzey duvarının önünde (yatağın yanı). Not 3'teki `G`, (2,3)'teki kayanın etrafından dolaşmak için.

```json
{
  "numara": 76,
  "baslik": "Katlanmış çizim",
  "gorev": "Notu oku, yürü",
  "harita": [
    ". . . . . .",
    ". . . . K .",
    ". . K . . .",
    ". . . . . .",
    ". K . . . K",
    "R . . . . ."
  ],
  "notlar": ["KKKKKD", "DDDKKKKKB", "KKKDGDDDDKKK"],
  "komutlar": ["move", "read_note"],
  "rutinler": ["yon"],
  "donus_sarti": true,
  "parcalar": ["elif harf == \"G\":", "elif harf == \"D\":", "elif harf == \"B\":", "return South", "return East", "return West", "for harf in read_note():", "move(yon(harf))"],
  "konular": ["return", "metin", "read_note", "değişen not"],
  "ipuclari": [
    "Nottaki harfler Türkçe: K kuzey → `North`, G güney → `South`, D doğu → `East`, B batı → `West`. Not her denemede başka bir yol yazabilir: ezberleme, oku.",
    "`read_note()` notu metin olarak verir. Harfi yöne çeviren bir rutin yaz: harfi alır, yönü `return` ile verir. Sonra `move(yon(harf))`.",
    "`def yon(harf):` / `    if harf == \"K\":` / `        return North`; G, D, B için `elif`. Sonra `for harf in read_note():` / `    move(yon(harf))`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        return North", "    elif harf == \"G\":", "        return South", "    elif harf == \"D\":", "        return East", "    elif harf == \"B\":", "        return West", "for harf in \"KKKKKD\":", "    move(yon(harf))"],
      "aciklama": "İlk notun yazısı koda elle kopyalandı: bir kez çalışır ama not değişince robot yanlış yere gider. Her seferinde read_note() ile oku."
    },
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        return North", "    elif harf == \"G\":", "        return South", "    elif harf == \"D\":", "        return West", "    elif harf == \"B\":", "        return East", "for harf in read_note():", "    move(yon(harf))"],
      "aciklama": "Doğu ile batı karışmış: D doğu, yani East; B batı, yani West."
    },
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        return North", "    elif harf == \"G\":", "        return South", "    elif harf == \"D\":", "        return East", "    elif harf == \"B\":", "        return West", "for harf in read_note():", "    if harf == \"K\":", "        move(\"K\")"],
      "aciklama": "move bir yön ister (North, East...), nottaki harf değil: \"K\" bir harftir. Harfi yöne çeviren rutini kullan: move(yon(harf))."
    },
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        return North", "    elif harf == \"D\":", "        return East", "    elif harf == \"B\":", "        return West", "for harf in read_note():", "    move(yon(harf))"],
      "aciklama": "G (güney) unutulmuş: o harf için yon hiçbir şey döndürmez (None), move yön bulamaz. Dört harfin hepsini çevir."
    },
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        print(North)", "    elif harf == \"G\":", "        print(South)", "    elif harf == \"D\":", "        print(East)", "    elif harf == \"B\":", "        print(West)", "for harf in read_note():", "    move(yon(harf))"],
      "aciklama": "yon yönü ekrana yazıyor ama cevap vermiyor: move None alır. print değil return."
    }
  ],
  "baslangic_kodu": [
    "def yon(harf):",
    "    if harf == \"K\":",
    "        return North"
  ],
  "cozum": [
    "def yon(harf):",
    "    if harf == \"K\":",
    "        return North",
    "    elif harf == \"G\":",
    "        return South",
    "    elif harf == \"D\":",
    "        return East",
    "    elif harf == \"B\":",
    "        return West",
    "for harf in read_note():",
    "    move(yon(harf))"
  ]
}
```

- [ ] **Step 3: `bolum-77.json` (24 kişilik liste · `in`, tür B)** — karar 1 ve 2. Görünen: Defne (True), Kerem (False); Ece gizli (True).

```json
{
  "numara": 77,
  "baslik": "24 kişilik liste",
  "gorev": "Listede Aras'ları bul",
  "harita": [
    ". . . . . .",
    ". . . . . .",
    ". . . . . .",
    ". . . . . .",
    ". . R . . .",
    ". . . . . ."
  ],
  "ekran": {
    "fonksiyon": "aras_mi",
    "satirlar": [
      {"girdi": "Defne Aras — kapsül: hazır", "cevap": true, "gorunur": true},
      {"girdi": "Ece Aras — kapsül: hazır", "cevap": true},
      {"girdi": "Kerem Yalın — kapsül: hazır", "cevap": false, "gorunur": true},
      {"girdi": "Selin Toprak — kapsül: hazır", "cevap": false},
      {"girdi": "Murat Ekinci — kapsül: hazır", "cevap": false},
      {"girdi": "Leyla Ekinci — kapsül: hazır", "cevap": false},
      {"girdi": "Can Ekinci — kapsül: hazır", "cevap": false},
      {"girdi": "Zeynep Uçar — kapsül: hazır", "cevap": false},
      {"girdi": "Ali Rıza Demir — kapsül: hazır", "cevap": false},
      {"girdi": "Nur Demir — kapsül: hazır", "cevap": false},
      {"girdi": "Baran Kaya — kapsül: hazır", "cevap": false},
      {"girdi": "Elif Kaya — kapsül: hazır", "cevap": false},
      {"girdi": "Hakan Sezer — kapsül: hazır", "cevap": false},
      {"girdi": "Derya Öztürk — kapsül: hazır", "cevap": false},
      {"girdi": "Emre Bulut — kapsül: hazır", "cevap": false},
      {"girdi": "Aylin Bulut — kapsül: hazır", "cevap": false},
      {"girdi": "Deniz Bulut — kapsül: hazır", "cevap": false},
      {"girdi": "Tarık Erdem — kapsül: hazır", "cevap": false},
      {"girdi": "Gül Şahin — kapsül: hazır", "cevap": false},
      {"girdi": "Okan Akın — kapsül: hazır", "cevap": false},
      {"girdi": "Melis Aydın — kapsül: hazır", "cevap": false},
      {"girdi": "Burak Tan — kapsül: hazır", "cevap": false},
      {"girdi": "Seda Koç — kapsül: hazır", "cevap": false},
      {"girdi": "Orhan Usta — kapsül: hazır", "cevap": false}
    ]
  },
  "komutlar": [],
  "parcalar": ["def aras_mi(satir):", "return \"Aras\" in satir"],
  "konular": ["metin", "in", "True/False döndüren fonksiyon"],
  "ipuclari": [
    "İstasyonun bilgisayarı sığınaktaki 24 kişinin satırını tek tek soruyor. Satırında `Aras` geçen için `True`, diğerleri için `False`.",
    "`in` metnin içinde arar: `\"Aras\" in satir` satırda Aras geçiyorsa `True`, geçmiyorsa `False`. Büyük-küçük harf önemli: `aras` başka bir kelimedir.",
    "`def aras_mi(satir):` altına `    return \"Aras\" in satir`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def aras_mi(satir):", "    if satir == \"Defne Aras — kapsül: hazır\":", "        return True", "    return False"],
      "aciklama": "Görev kartındaki satır doğru ama listede başka bir Aras daha var: tek satırı ezberlemek yetmez, satırın içinde ara."
    },
    {
      "kod": ["def aras_mi(satir):", "    return \"aras\" in satir"],
      "aciklama": "Küçük harfle aras listede hiç geçmiyor: in harfleri birebir arar, A ile a farklıdır."
    },
    {
      "kod": ["def aras_mi(satir):", "    if \"Aras\" in satir:", "        return True"],
      "aciklama": "Aras olmayan satırlarda fonksiyon hiçbir şey döndürmez (None); ekran False bekliyordu. Kalan yol için de return yaz."
    },
    {
      "kod": ["def aras_mi(satir):", "    return True"],
      "aciklama": "Herkese True diyor: ekran yalnızca Aras'ları bekliyordu."
    },
    {
      "kod": ["def aras_mi(satir):", "    print(\"Aras\" in satir)"],
      "aciklama": "Cevap ekrana yazılır ama verilmez: ekran None alır. print değil return."
    }
  ],
  "baslangic_kodu": [
    "def aras_mi(satir):",
    "    return False"
  ],
  "cozum": [
    "def aras_mi(satir):",
    "    return \"Aras\" in satir"
  ]
}
```

- [ ] **Step 4: `bolum-78.json` (Listenin altı · BKM-7, tür B)** — ekran 26 satır; tek `True` BKM-7 (gizli), ekranda vurgulanır. KOR satırı `dış` içerir ama `dışarıda` içermez.

```json
{
  "numara": 78,
  "baslik": "Listenin altı",
  "gorev": "Dışarıda kalanı bul",
  "harita": [
    ". . . . . .",
    ". . . . . .",
    ". . . . . .",
    ". . . . . .",
    ". . R . . .",
    ". . . . . ."
  ],
  "ekran": {
    "fonksiyon": "disarida_mi",
    "satirlar": [
      {"girdi": "Defne Aras — kapsül: hazır", "cevap": false, "gorunur": true},
      {"girdi": "Ece Aras — kapsül: hazır", "cevap": false, "gorunur": true},
      {"girdi": "Kerem Yalın — kapsül: hazır", "cevap": false},
      {"girdi": "Selin Toprak — kapsül: hazır", "cevap": false},
      {"girdi": "Murat Ekinci — kapsül: hazır", "cevap": false},
      {"girdi": "Leyla Ekinci — kapsül: hazır", "cevap": false},
      {"girdi": "Can Ekinci — kapsül: hazır", "cevap": false},
      {"girdi": "Zeynep Uçar — kapsül: hazır", "cevap": false},
      {"girdi": "Ali Rıza Demir — kapsül: hazır", "cevap": false},
      {"girdi": "Nur Demir — kapsül: hazır", "cevap": false},
      {"girdi": "Baran Kaya — kapsül: hazır", "cevap": false},
      {"girdi": "Elif Kaya — kapsül: hazır", "cevap": false},
      {"girdi": "Hakan Sezer — kapsül: hazır", "cevap": false},
      {"girdi": "Derya Öztürk — kapsül: hazır", "cevap": false},
      {"girdi": "Emre Bulut — kapsül: hazır", "cevap": false},
      {"girdi": "Aylin Bulut — kapsül: hazır", "cevap": false},
      {"girdi": "Deniz Bulut — kapsül: hazır", "cevap": false},
      {"girdi": "Tarık Erdem — kapsül: hazır", "cevap": false},
      {"girdi": "Gül Şahin — kapsül: hazır", "cevap": false},
      {"girdi": "Okan Akın — kapsül: hazır", "cevap": false},
      {"girdi": "Melis Aydın — kapsül: hazır", "cevap": false},
      {"girdi": "Burak Tan — kapsül: hazır", "cevap": false},
      {"girdi": "Seda Koç — kapsül: hazır", "cevap": false},
      {"girdi": "Orhan Usta — kapsül: hazır", "cevap": false},
      {"girdi": "BKM-7   — dışarıda — görev: KAPI", "cevap": true},
      {"girdi": "KOR     — hizmet dışı — hangar", "cevap": false}
    ]
  },
  "komutlar": [],
  "parcalar": ["def disarida_mi(satir):", "return \"dışarıda\" in satir"],
  "konular": ["metin", "in"],
  "ipuclari": [
    "Liste uzadı: 24 kişinin altında iki satır daha var. Satırında `dışarıda` geçen için `True`, diğerleri için `False`.",
    "Geçen bölümdeki kalıp: `\"dışarıda\" in satir`. Kelimenin tamamını yaz; yalnızca `dış` başka bir satırda da geçiyor.",
    "`def disarida_mi(satir):` altına `    return \"dışarıda\" in satir`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def disarida_mi(satir):", "    return \"dış\" in satir"],
      "aciklama": "dış kelimesi KOR'un satırında da geçiyor (hizmet dışı): aradığın kelimenin tamamını yaz, dışarıda."
    },
    {
      "kod": ["def disarida_mi(satir):", "    return satir == \"dışarıda\""],
      "aciklama": "== satırın tamamını karşılaştırır, hiçbir satır yalnızca dışarıda değil. İçinde aramak için in."
    },
    {
      "kod": ["def disarida_mi(satir):", "    return False"],
      "aciklama": "Görev kartındaki iki satır için doğru ama listede dışarıda olan biri var."
    },
    {
      "kod": ["def disarida_mi(satir):", "    if satir == \"Defne Aras — kapsül: hazır\":", "        return False", "    if satir == \"Ece Aras — kapsül: hazır\":", "        return False"],
      "aciklama": "Yalnızca görev kartındaki iki satır ezberlendi: öteki satırlarda fonksiyon None döndürür. Kuralı yaz."
    }
  ],
  "baslangic_kodu": [
    "def disarida_mi(satir):",
    "    return False"
  ],
  "cozum": [
    "def disarida_mi(satir):",
    "    return \"dışarıda\" in satir"
  ]
}
```

- [ ] **Step 5: `bolum-79.json` (Hizmet dışı · harf sayma + `len`, tür A)** — Kor'un kaydı 18 hafta, 6'sı arıza: anten 12 bekler. Kor koloninin ilk maden robotu (`hikaye-kitabi.md`): `M` maden, `T` taşıma.

```json
{
  "numara": 79,
  "baslik": "Hizmet dışı",
  "gorev": "Kor kaç hafta çalıştı?",
  "harita": [
    ". . . . . .",
    ". K . . K .",
    ". . . . . .",
    "V . . V . H",
    ". K . K . .",
    "R . . . . ."
  ],
  "komutlar": ["move", "collect", "report"],
  "rapor": 12,
  "rutinler": ["kac_kez"],
  "donus_sarti": true,
  "parcalar": ["def kac_kez(metin, harf):", "sayac = 0", "for h in metin:", "if h == harf:", "sayac += 1", "return sayac", "len(kayit)", "report(len(kayit) - kac_kez(kayit, \"A\"))"],
  "python_kelimeleri": ["len"],
  "konular": ["metin", "harf sayma", "len", "return"],
  "ipuclari": [
    "Kayıtta her harf bir hafta: M maden, T taşıma, A arıza. Kor arızasız haftalarda çalıştı; anten o sayıyı bekliyor.",
    "Harf saymak: sayaç kur, metnin harflerinde dolaş, harf eşleşince sayacı büyüt, sonunda sayacı `return` et. `len(kayit)` metnin kaç harf olduğunu verir.",
    "`def kac_kez(metin, harf):` içinde `sayac = 0`, `for h in metin:`, `if h == harf:`, `sayac += 1`, en sonda `return sayac`. Hangar kapısında: `report(len(kayit) - kac_kez(kayit, \"A\"))`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["kayit = \"MMTMMTMTMMTMAAAAAA\"   # KOR: M maden, T taşıma, A arıza", "def kac_kez(metin, harf):", "    for h in metin:", "        sayac = 0", "        if h == harf:", "            sayac += 1", "    return sayac", "move(North)", "move(North)", "collect()", "for i in range(3):", "    move(East)", "collect()", "move(East)", "move(East)", "report(len(kayit) - kac_kez(kayit, \"A\"))"],
      "aciklama": "sayac her harfte sıfırlanıyor: rutin en çok 1 sayar. sayac = 0 döngüden önce."
    },
    {
      "kod": ["kayit = \"MMTMMTMTMMTMAAAAAA\"   # KOR: M maden, T taşıma, A arıza", "def kac_kez(metin, harf):", "    sayac = 0", "    for h in metin:", "        if h == harf:", "            sayac += 1", "        return sayac", "move(North)", "move(North)", "collect()", "for i in range(3):", "    move(East)", "collect()", "move(East)", "move(East)", "report(len(kayit) - kac_kez(kayit, \"A\"))"],
      "aciklama": "return döngünün içinde: rutin ilk harfe bakıp cevap verir. return'ü for ile aynı hizaya al."
    },
    {
      "kod": ["kayit = \"MMTMMTMTMMTMAAAAAA\"   # KOR: M maden, T taşıma, A arıza", "def kac_kez(metin, harf):", "    sayac = 0", "    for h in metin:", "        if h == A:", "            sayac += 1", "    return sayac", "move(North)", "move(North)", "collect()", "for i in range(3):", "    move(East)", "collect()", "move(East)", "move(East)", "report(len(kayit) - kac_kez(kayit, \"A\"))"],
      "aciklama": "Tırnaksız A bir değişken adıdır (NameError); rutin zaten aranacak harfi harf kutusunda alıyor: h == harf."
    },
    {
      "kod": ["kayit = \"MMTMMTMTMMTMAAAAAA\"   # KOR: M maden, T taşıma, A arıza", "def kac_kez(metin, harf):", "    sayac = 0", "    for h in metin:", "        if h == harf:", "            sayac += 1", "    return sayac", "move(North)", "move(North)", "collect()", "for i in range(3):", "    move(East)", "collect()", "move(East)", "move(East)", "report(kac_kez(kayit, \"A\"))"],
      "aciklama": "Bu, Kor'un arızalı kaldığı hafta sayısı. Soru çalıştığı haftalar: toplam haftadan arızalıları çıkar."
    },
    {
      "kod": ["kayit = \"MMTMMTMTMMTMAAAAAA\"   # KOR: M maden, T taşıma, A arıza", "def kac_kez(metin, harf):", "    sayac = 0", "    for h in metin:", "        if h == harf:", "            sayac += 1", "    return sayac", "move(North)", "move(North)", "collect()", "for i in range(3):", "    move(East)", "collect()", "move(East)", "move(East)", "report(len(kayit))"],
      "aciklama": "len bütün haftaları sayar, arızalı haftalar da içinde."
    }
  ],
  "baslangic_kodu": [
    "kayit = \"MMTMMTMTMMTMAAAAAA\"   # KOR: M maden, T taşıma, A arıza",
    "move(North)",
    "move(North)",
    "collect()",
    "for i in range(3):",
    "    move(East)",
    "collect()",
    "move(East)",
    "move(East)"
  ],
  "cozum": [
    "kayit = \"MMTMMTMTMMTMAAAAAA\"   # KOR: M maden, T taşıma, A arıza",
    "def kac_kez(metin, harf):",
    "    sayac = 0",
    "    for h in metin:",
    "        if h == harf:",
    "            sayac += 1",
    "    return sayac",
    "move(North)",
    "move(North)",
    "collect()",
    "for i in range(3):",
    "    move(East)",
    "collect()",
    "move(East)",
    "move(East)",
    "report(len(kayit) - kac_kez(kayit, \"A\"))"
  ]
}
```

- [ ] **Step 6: `bolum-80.json` (İstasyon haritası · Bölge 8 finali, tür C)** — üç not aynı başlar (`KVKKV`: iki veri kartı, ortak yolda), sonra üç yöne ayrılır: (0,5), (4,5), (5,2).

```json
{
  "numara": 80,
  "baslik": "İstasyon haritası",
  "gorev": "Haritadaki yolu izle",
  "harita": [
    ". . . . . .",
    ". . . K . .",
    ". . V . K .",
    "K . . . . .",
    ". . V . . .",
    ". . R . . ."
  ],
  "notlar": ["KVKKVBBKK", "KVKKVKKDD", "KVKKVDGDD"],
  "komutlar": ["move", "collect", "read_note"],
  "rutinler": ["yon"],
  "donus_sarti": true,
  "parcalar": ["def yon(harf):", "if harf == \"K\":", "elif harf == \"G\":", "elif harf == \"D\":", "elif harf == \"B\":", "return North", "return South", "return East", "return West", "for harf in read_note():", "if harf == \"V\":", "else:", "collect()", "move(yon(harf))"],
  "konular": ["return", "metin", "read_note", "değişen not", "bölge finali"],
  "ipuclari": [
    "Harita her denemede başka bir yol çizebilir. Harfler: K kuzey, G güney, D doğu, B batı; V ise \"veri kartı burada\" demek.",
    "Harfi yöne çeviren rutin 76'daki gibi. V bir yön değil, `yon` onu bilmez: döngüde önce V'ye bak; V ise topla, değilse yürü.",
    "`for harf in read_note():` / `    if harf == \"V\":` / `        collect()` / `    else:` / `        move(yon(harf))`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        return North", "    elif harf == \"G\":", "        return South", "    elif harf == \"D\":", "        return East", "    elif harf == \"B\":", "        return West", "for harf in \"KVKKVBBKK\":", "    if harf == \"V\":", "        collect()", "    else:", "        move(yon(harf))"],
      "aciklama": "İlk haritanın yazısı koda elle kopyalandı: harita değişince robot yanlış yere gider. Her seferinde read_note() ile oku."
    },
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        return North", "    elif harf == \"G\":", "        return South", "    elif harf == \"D\":", "        return East", "    elif harf == \"B\":", "        return West", "for harf in read_note():", "    move(yon(harf))"],
      "aciklama": "V bir yön değil: yon onun için hiçbir şey döndürmez (None), move yön bulamaz. V'yi döngüde ayrıca sor."
    },
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        return North", "    elif harf == \"G\":", "        return South", "    elif harf == \"D\":", "        return West", "    elif harf == \"B\":", "        return East", "for harf in read_note():", "    if harf == \"V\":", "        collect()", "    else:", "        move(yon(harf))"],
      "aciklama": "Doğu ile batı karışmış: D doğu, yani East; B batı, yani West."
    },
    {
      "kod": ["def yon(harf):", "    if harf == \"K\":", "        return North", "    elif harf == \"G\":", "        return South", "    elif harf == \"D\":", "        return East", "    elif harf == \"B\":", "        return West", "for harf in read_note():", "    if harf == \"V\":", "        move(North)", "    else:", "        move(yon(harf))"],
      "aciklama": "V'de robot kartı toplamalı, yürümemeli: collect()."
    }
  ],
  "cozum": [
    "def yon(harf):",
    "    if harf == \"K\":",
    "        return North",
    "    elif harf == \"G\":",
    "        return South",
    "    elif harf == \"D\":",
    "        return East",
    "    elif harf == \"B\":",
    "        return West",
    "for harf in read_note():",
    "    if harf == \"V\":",
    "        collect()",
    "    else:",
    "        move(yon(harf))"
  ]
}
```

- [ ] **Step 7: `sinav-80.json`** (spec: `"4"` ile `4`; `==` ile `=`; `len("KOR")`; `in`)

```json
{
  "bolum": 80,
  "baslik": "KIVILCIM · KENDİNİ DENETLİYOR",
  "sorular": [
    {
      "soru": "\"4\" == 4 sonucu nedir?",
      "secenekler": ["True", "False", "Hata verir", "4"],
      "dogru": 1,
      "aciklama": "Tırnaklı \"4\" bir metin, tırnaksız 4 bir sayı: Python ikisini eşit saymaz."
    },
    {
      "soru": "harf = \"K\" ile harf == \"K\" arasındaki fark nedir?",
      "secenekler": ["Fark yok", "İkisi de karşılaştırır", "= ad verir, == karşılaştırıp True/False verir", "== ad verir, = karşılaştırır"],
      "dogru": 2,
      "aciklama": "Tek eşittir değeri bir ada bağlar; iki eşittir iki değer aynı mı diye sorar."
    },
    {
      "soru": "len(\"KOR\") kaçtır?",
      "secenekler": ["1", "2", "3", "KOR"],
      "dogru": 2,
      "aciklama": "len metnin kaç harf olduğunu verir: K, O, R."
    },
    {
      "soru": "\"dış\" in \"KOR — hizmet dışı — hangar\" sonucu nedir?",
      "secenekler": ["True", "False", "Hata verir", "dış"],
      "dogru": 0,
      "aciklama": "in metnin içinde arar: \"dışı\" kelimesinin başında \"dış\" var. Bu yüzden 78'de kelimenin tamamı aranır: \"dışarıda\"."
    }
  ]
}
```

- [ ] **Step 8: `sozluk.json`** — iki sayfa (`read_note` 76'da, `len` 79'da açılır):

```json
    {
      "baslik": "read_note",
      "kelimeler": ["read_note"],
      "aciklama": "Kıvılcım durduğu yerdeki notu okur, yazısını metin olarak verir (örn. \"KKDB\"). Notlardaki yön harfleri Türkçe: K kuzey, G güney, D doğu, B batı. move harf değil yön ister: harfi önce yöne çevir. Not her denemede değişebilir; yazıyı ezberleme, oku.",
      "ornek": [
        "for harf in read_note():",
        "    move(yon(harf))"
      ]
    },
    {
      "baslik": "len",
      "kelimeler": ["len"],
      "aciklama": "Bir metnin kaç harf olduğunu verir: len(\"KOR\") 3. Boşluklar ve işaretler de sayılır.",
      "ornek": [
        "len(\"KOR\")   # 3",
        "kayit = \"MMTA\"",
        "haftalar = len(kayit)   # 4"
      ]
    }
```

- [ ] **Step 9: Belgeler**
- `bolum-cozumleri.md`: başlık satırına "71-80: 2026-11-09"; Bölüm 70'in altına `## Bölüm 71` … `## Bölüm 80`, her biri `cozum` alanının aynısı (```python blokları).
- `bolum-dosyasi.md` "Denetim" listesine iki madde: "Bölge 8'den itibaren: çözüm yalnızca o bölüme kadar açılmış Python kelimelerini kullanıyor mu (`python_kelimeleri`)." ve "Bölge 8'den itibaren: her çözüm satırı başlangıç kodunda ya da `parcalar`'da var mı (acemi düğmeleriyle kurulabilir mi; sayılar farklı olabilir)."

- [ ] **Step 10: Çalıştır** — `cd motor-test && dotnet test` → hepsi geçer.

---

### Görev 8: Son denetim + Enes'e görüntü

- [ ] **Step 1:** `cd motor-test && dotnet test` → hepsi geçer; sayıyı not et.
- [ ] **Step 2: Paket** — `oyun/Build/` sil; Windows paketi üret (`CLAUDE.md` "Çalıştırma": `-executeMethod OyunBuild.BuildWindows -logFile build.log`; Unity Hub açık ve giriş yapılmış olmalı). `build.log`'da `error CS` yok.
- [ ] **Step 3: Görüntü turu (yalnız Bölge 8)** — `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -bolum 71 -shots <mutlak klasör>`. Bakılacaklar: 72/77/78'de ekran kutusu görev kartının üstünde, satırlar kutudan taşmıyor (78'de 26 satır kayıyor), 78'de BKM-7 satırı vurgulu; 76/80'de not yazısı robotun üstünde; 71/79/80'de veri kartları; 73-74 paneller. Tam sahne denetimleri (`-sahneler`), klavye denetimi ve `-bolum 50 -final` **koşturulmaz** (ortak sahne ve klavye koduna dokunulmadı; CLAUDE.md "Denetimi ölçülü yap").
- [ ] **Step 4: Enes'e göster** — görüntüleri `docs/tasarim/bolge-8-bulmacalar-<tarih>/`'e kopyala, Enes'e 72, 76, 78 ve 80'in karelerini göster; paketi aç (`calistir.bat` değil, paketi kendin üret-aç), Enes oyunda 72'yi (ekran) ve 76'yı (not) denesin. Onaysız 2. parçaya geçme.
- [ ] **Step 5: ILERLEME.md** — "Enes, <tarih> (commit edilmedi) — Bölge 8 · 1. parça bitti" girdisi: yapılanlar, test sayısı, açık sorular (karar 1: Ece'nin soyadı; karar 9: Bölge 10'da `not`), sıradaki (2. parça: hikâye metinleri, 24 ismin son hâli, Kayıt 4, `hikaye.md` düzeltmeleri; Sonnet 5.5 · medium, yeni sohbet). Kapanış adımları CLAUDE.md ekip döngüsündeki gibi (önce ILERLEME, sonra tek commit + push).
