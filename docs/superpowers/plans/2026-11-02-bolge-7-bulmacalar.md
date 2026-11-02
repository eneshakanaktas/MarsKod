# Bölge 7 Bulmacaları (Bölüm 61-70) Uygulama Planı

> **Yürütme:** Görevleri sırayla yap, adımlar onay kutusu (`- [ ]`) biçiminde. Yardımcı ajan (subagent) yok; işi aynı sohbet yapar (Enes'in kararı, ILERLEME.md).
> **Model · efor:** Sonnet 5.5 · medium (~4 sa). Görev 4 (değişen yol) zorlaşırsa high.

**Goal:** Bölüm 61-70'i (parametreli fonksiyonlar) oynanır hâle getirmek: parametre şartı, `scout()` + iz (`izler`), değişen yol (2-3 iz), nehir çakılı `C`, nehir yatağı bölgesi, 10 bölüm dosyası, 2 sınav, sözlükte `def` + `scout`.

**Architecture:** Bütün kurallar saf C# dünya tarafında. Parametre şartı `RoutineRule`'a eklenir; `ProgramRun` her rutin çağrısının bilgilerini (argümanlarını) motorun adım çerçevesinden (`Step.Frame`, ilk adımda parametreler zaten bağlı) okur. İz `Trail` sınıfıdır; `World.Scout()` robotun karesinde izin sonraki yönünü verir, hedef izin son karesidir. Değişen yolda `Level.RunTrails` kodu her izde yeni bir dünyada ayrı çalıştırır, `TrailRuns.Pick` ekranda hangisinin oynatılacağını seçer; `Level.Run` (denetleyici + testler) "hepsi tamamsa tamam" sonucunu verir. Sahne (`Oyun.cs`) yalnızca seçilen koşuyu oynatır; çakıl, nehir yatağı ve Serçe'nin ışığı bu planda **geçici** görünüşlere bağlanır (asıl görünüş 3. parçada).

**Tech Stack:** Unity 6 (C# 9, .NET Standard 2.1), kendi mini-Python motoru (`oyun/Assets/Motor/`), xUnit (`motor-test/`, `dotnet test`; başlangıçta 1403 test).

**Spec:** `docs/superpowers/specs/2026-11-02-bolge-7-design.md` (bölüm "1. Bulmacalar").

## Global Constraints

- Motor ve dünya kodu saf C#: `UnityEngine` yok, C# 9, .NET Standard 2.1 (motor-test derlerken yakalar).
- Kullanıcıya görünen bütün metinler sade Türkçe; kod parçaları ters tırnak içinde.
- Haritalar 6×6. Bir bölümde tek tür toplanır; nehir yatağında (61-70) toplanacak varsa o `C` (nehir çakılı).
- Parametre şartı: rutin şartına giren her rutin en az **1 parametre** almalı ve kod çalışırken en az **2 farklı bilgiyle** çağrılmalı. Varsayılan değer de sayılır: `git(East)` ile `git(East, 3)` farklıdır, `git(East)` ile `git(East, 1)` aynıdır (`kac=1` iken).
- `izler` olan bölümde haritada `H` yazılmaz (hedef izin son karesi). Bütün izler aynı uzunlukta; iz kaya üstünden ve alanın dışından geçmez, kendini kesmez. Panel, toz, çakıl karesinden geçebilir (robot onların üstünden yürüyebiliyor).
- Bölüm 61-70'te `etiket`, `giris`, `bitis` yazılmaz (hikâye 2. parçada). Kayıt 3 sahnesi, kalem, duvar yazısı da 2./3. parçada.
- Yeni `.cs` dosyasının `.meta`'sı Unity paketi üretilince oluşur, git'e onunla girer.
- **Commit kuralı (CLAUDE.md, ekip döngüsü):** görev başına commit yok; oturum kapanışında ILERLEME.md ile birlikte tek commit + push.
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıf/fonksiyon, tekrar yok, okunur adlar, gereksiz katman yok.

## Plan sırasında verilen kararlar (spec'e ek / düzeltme)

1. **İz panelin üstünden geçebilir.** Spec 1.2 "panel üstünden değil" diyor ama 1.3 "onarılacak panel bütün yolların ortak parçasında durur" diyor; ikisi birlikte olamaz. Robot panellerin üstünde yürüyebildiği için iz de geçebilir (69 ve 70'te panel izin ortak başında).
2. **62, 65, 67'de de `rutinler: ["git"]` var.** Parametre şartı yalnızca rutin şartına giren rutinlere uygulanır; tablodaki "parametre şartı" ancak bir rutin istenince anlam taşır.
3. **"Serçe bu sefer başka bir kol gördü." yalnızca kod bir izde başarılı, başka bir izde başarısızsa** mesajın başına eklenir. Kod hiçbir izde çalışmıyorsa bu cümle yanıltır (sorun yol değil, kod).
4. **70'in başlığı "İzin sonu"** (spec taslağı "KIVILCIM BENİ BUL"). Başlık bölüm açılırken görünür; duvardaki yazıyı önceden söylerse finalin sürprizi bozulur. Ragıp'a sor; o adı isterse yalnızca `baslik` değişir.
5. **Geçici görünüşler:** çakıl = drone parçasının görünüşü, nehir yatağı = kanyonun görünüşü, `scout()` = hedef karede tarama halkası + turkuaz yön yazısı (ör. "East"). Dünyada bilgisayar yazısı kuralına aykırı; 3a'da Serçe'nin ışık lekesiyle değişir. Hedef `izler` olan bölümlerde hiç görünmez (3a'da izin sonunda belirir).
6. Haritalar, çözümler ve tipik hatalar bu plan yazılırken ayrı bir benzetimle (Python) denendi: her çözüm her izde biter, her tipik hata en az bir izde takılır. Yine de son söz `dotnet test`'tir.

## Review Focus

- Rutin çağrısının bilgileri **ilk adımda** okunmalı (parametreler o anda bağlı; sonra rutin içinde değişebilir) → Görev 1 testi.
- Varsayılan değerli çağrı: `git(East)` (kac=1) ile `git(East, 1)` aynı sayılmalı → Görev 1 testi.
- `rutin_sayisi` + parametre şartı: parametresiz ama 2 kez çalışan rutin sayılmamalı ve mesaj **parametre** mesajı olmalı ("rutin yok" değil) → Görev 1 testi.
- `izler` olan bölümde toplanacak/onarılacak bir şey yokken "Haritada görev yok" hatası **verilmemeli** (62, 65, 67, 68) → Görev 4.
- Değişen yolda `Level.Run` hepsi tamamsa tamam; ezber çözüm (ilk izin yönleri elle) en az bir izde takılmalı → Görev 4 testi + bölüm dosyaları.
- Sahnede seçilen koşunun **kendi dünyası** mesajlarda kullanılmalı (`world.IceLeft` vb.) → Görev 5.

---

### Görev 1: Parametre şartı (dünya kuralı) + çağrı bilgilerinin kaydı

**Files:**
- Modify: `oyun/Assets/Dunya/RoutineRule.cs` (yapıcıya `needsParameters`, `Check(RunReport)`, `ParameterProblem`)
- Modify: `oyun/Assets/Dunya/ProgramRun.cs` (`RunReport` iki yeni alan; `CountCall` bilgileri de yazar; `Check` çağrısı)
- Test: `motor-test/ParameterTests.cs` (yeni)

**Interfaces:**
- Produces:
  - `RoutineRule(IEnumerable<string> names, int minCount, bool needsParameters = false)`, `public readonly bool NeedsParameters`, `public const int MinDistinctArguments = 2`, `public RoutineProblem Check(RunReport report)` (eski `Check(defined, runs)` kalkar; tek çağıran `ProgramRun`)
  - `RunReport.RoutineParameters : Dictionary<string,int>` (çalışan her rutinin parametre sayısı), `RunReport.RoutineArguments : Dictionary<string, List<string>>` (her rutinin aldığı farklı bilgiler, ilk görülme sırasıyla; örn. `"East"`, `"South, 3"`)

- [ ] **Step 1: Failing test yaz** — `motor-test/ParameterTests.cs`:

```csharp
using MarsKod.Dunya;

namespace MotorTest;

// Parametre şartı (Bölge 7, Bölüm 61-70, parametreli fonksiyonlar): rutin bir bilgi kutusu (parametre) almalı ve
// en az iki farklı bilgiyle çağrılmalı; yoksa robot işi bitirse bile görev tamam sayılmaz.
public class ParameterTests
{
    // Robot (0,0); toplanacaklar (1,0) ve (1,1): önce doğu, sonra kuzey
    static World Corner() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(1, 1) }, null, null, new[] { "move", "collect" });
    // Robot (0,0); toplanacaklar (1,0) ve (2,0): iki kez doğu
    static World Row() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(2, 0) }, null, null, new[] { "move", "collect" });
    // Robot (0,0); toplanacaklar (1,0) ve (3,0): bir, sonra iki adım doğu
    static World Gap() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(3, 0) }, null, null, new[] { "move", "collect" });

    static readonly RoutineRule Git = new RoutineRule(new[] { "git" }, 0, needsParameters: true);

    const string GitYon = "def git(yon):\n    move(yon)\n    collect()\n";
    const string GitKac = "def git(yon, kac=1):\n    for i in range(kac):\n        move(yon)\n    collect()\n";

    [Fact]
    public void Iki_farkli_yonle_cagrilan_rutin_sarti_saglar()
    {
        var report = ProgramRun.Execute(GitYon + "git(East)\ngit(North)\n", Corner(), routines: Git);
        Assert.True(report.Complete);
        Assert.Null(report.RoutineProblem);
        Assert.Equal(1, report.RoutineParameters["git"]);
        Assert.Equal(new[] { "East", "North" }, report.RoutineArguments["git"]);
    }

    [Fact]
    public void Parametresiz_rutin_sayilmaz()
    {
        var report = ProgramRun.Execute("def git():\n    collect()\nmove(East)\ngit()\nmove(North)\ngit()\n", Corner(), routines: Git);
        Assert.False(report.Complete);
        Assert.Contains("parametresiz", report.RoutineProblem.Title);
        Assert.Contains("def git(yon):", report.RoutineProblem.Text);
    }

    [Fact]
    public void Hep_ayni_bilgiyle_cagri_sayilmaz()
    {
        var world = Row();
        var report = ProgramRun.Execute(GitYon + "git(East)\ngit(East)\n", world, routines: Git);
        Assert.True(world.Complete);
        Assert.False(report.Complete);
        Assert.Contains("hep aynı", report.RoutineProblem.Title);
        Assert.Contains("`East`", report.RoutineProblem.Text);
    }

    [Fact]
    public void Varsayilan_deger_bilgiye_dahildir()
    {
        var same = ProgramRun.Execute(GitKac + "git(East)\ngit(East, 1)\n", Row(), routines: Git);
        Assert.Contains("hep aynı", same.RoutineProblem.Title); // kac=1 yazılmasa da 1
        var different = ProgramRun.Execute(GitKac + "git(East)\ngit(East, 2)\n", Gap(), routines: Git);
        Assert.True(different.Complete);
        Assert.Equal(new[] { "East, 1", "East, 2" }, different.RoutineArguments["git"]);
    }

    [Fact]
    public void Bilgi_ilk_adimda_okunur()
    {
        // rutin parametreyi içeride değiştirse de çağrıdaki bilgi sayılır
        var report = ProgramRun.Execute("def git(yon):\n    yon = East\n    move(yon)\n    collect()\ngit(North)\ngit(South)\n", Row(), routines: Git);
        Assert.Equal(new[] { "North", "South" }, report.RoutineArguments["git"]);
    }

    [Fact]
    public void Rutin_sayisinda_parametresiz_rutin_sayilmaz_ve_parametre_mesaji_gelir()
    {
        var one = new RoutineRule(new string[0], 1, needsParameters: true);
        var report = ProgramRun.Execute("def adim():\n    move(East)\n    collect()\nadim()\nadim()\n", Row(), routines: one);
        Assert.False(report.Complete);
        Assert.Contains("adim parametresiz", report.RoutineProblem.Title);
        Assert.True(ProgramRun.Execute(GitYon + "git(East)\ngit(North)\n", Corner(), routines: one).Complete);
    }

    [Fact]
    public void Parametre_sarti_yoksa_eskisi_gibi()
    {
        var plain = new RoutineRule(new[] { "git" }, 0);
        Assert.True(ProgramRun.Execute(GitYon + "git(East)\ngit(East)\n", Row(), routines: plain).Complete);
        Assert.False(plain.NeedsParameters);
    }
}
```

> `World(cols, rows, robot, iceCells, rockCells = null, target = null, commands = null, ...)` (World.cs:196).

- [ ] **Step 2: Çalıştır, başarısız olduğunu gör**

Run: `cd motor-test && dotnet test --filter ParameterTests`
Expected: derleme hatası (`needsParameters`, `RoutineArguments` yok).

- [ ] **Step 3: `ProgramRun.cs`** — `RunReport`'a (`RoutineProblem` alanının üstüne):

```csharp
        /// <summary>Çalışan her rutinin kaç parametresi (bilgi kutusu) var</summary>
        public readonly Dictionary<string, int> RoutineParameters = new Dictionary<string, int>();
        /// <summary>Her rutinin çağrılırken aldığı farklı bilgiler, ilk görülme sırasıyla ("East", "South, 3"); varsayılan değerler dahil</summary>
        public readonly Dictionary<string, List<string>> RoutineArguments = new Dictionary<string, List<string>>();
```

`Execute` içinde `routines.Check(report.DefinedRoutines, report.RoutineRuns)` → `routines.Check(report)`.

`CountCall`'u şöyle değiştir (sayım aynı, ardından bilgiler):

```csharp
        // Rutin cagrisinin ilk adimi: cerceve yeniyse o rutinin calisma sayisini bir artirir ve aldigi bilgileri yazar
        // (ilk adimda parametreler cagridaki degerlerle bagli; rutin sonra onlari degistirebilir)
        static void CountCall(RunReport report, HashSet<Frame> calls, Frame frame)
        {
            if (frame?.Fn == null || !calls.Add(frame)) return;
            string name = frame.Fn.Def.Name;
            report.RoutineRuns[name] = report.RoutineRuns.TryGetValue(name, out var n) ? n + 1 : 1;
            RecordArguments(report, name, frame);
        }

        static void RecordArguments(RunReport report, string name, Frame frame)
        {
            var parameters = frame.Fn.Def.Params;
            report.RoutineParameters[name] = parameters.Count;
            var values = new List<string>(parameters.Count);
            foreach (var p in parameters)
                values.Add(frame.Vars.TryGetValue(p.Name, out var v) ? ArgumentText(v) : "?");
            string text = string.Join(", ", values);
            if (!report.RoutineArguments.TryGetValue(name, out var seen)) report.RoutineArguments[name] = seen = new List<string>();
            if (!seen.Contains(text)) seen.Add(text);
        }

        // Yonler (metin olarak tutulur) oyuncunun yazdigi gibi: East; gerisi Python'daki gorunusu: 3, 'abc'
        static string ArgumentText(object value) =>
            value is string s && Enum.TryParse(s, out Direction d) && s == d.ToString() ? s : Values.Repr(value);
```

- [ ] **Step 4: `RoutineRule.cs`** — dosya başı yorumuna ekle: "Bölge 7'den itibaren parametre şartı da olabilir ("parametre_sarti"): rutin bilgi almalı ve en az iki farklı bilgiyle çağrılmalı."

Alanlar ve yapıcı:

```csharp
        /// <summary>Parametre şartında bir rutinin en az kaç farklı bilgiyle çağrılması gerekir</summary>
        public const int MinDistinctArguments = 2;

        /// <summary>Parametre şartı: şarta giren her rutin parametre almalı ve farklı bilgilerle çağrılmalı</summary>
        public readonly bool NeedsParameters;

        public RoutineRule(IEnumerable<string> names, int minCount, bool needsParameters = false)
        {
            Names = names.ToList();
            MinCount = minCount;
            NeedsParameters = needsParameters;
        }
```

`Check`'i `RunReport` alacak şekilde değiştir; mesajlar aynı kalır, yalnızca iki yere parametre denetimi girer:

```csharp
        /// <summary>Kodun çalışma kaydına bakar: rutinler tanımlı mı, kaç kez çalıştı, (istendiyse) bilgi aldı mı. Şart sağlandıysa null.</summary>
        public RoutineProblem Check(RunReport report)
        {
            var defined = report.DefinedRoutines;
            foreach (var name in Names)
            {
                int n = Runs(report, name);
                if (!defined.Contains(name)) return /* eskisi gibi: "Kod bitti, NAME rutini yok" */;
                if (n == 0) return /* eskisi gibi */;
                if (n < MinRuns) return /* eskisi gibi */;
                var parameterProblem = ParameterProblem(report, name);
                if (parameterProblem != null) return parameterProblem;
            }
            var repeated = defined.Where(name => Runs(report, name) >= MinRuns).OrderBy(name => name, StringComparer.Ordinal).ToList();
            int used = repeated.Count(name => ParameterProblem(report, name) == null);
            if (used < MinCount)
            {
                // tekrar eden rutin var ama bilgi almıyor: oyuncuya asıl eksiği söyle
                var parameterProblem = repeated.Select(name => ParameterProblem(report, name)).FirstOrDefault(p => p != null);
                if (parameterProblem != null) return parameterProblem;
                return /* eskisi gibi: MinCount == 1 ? "Kod bitti, rutin yok" : "Kod bitti, N rutin gerekiyor" */;
            }
            return null;
        }

        /// <summary>Parametre şartı açıksa: rutin en az bir parametre almalı ve en az iki farklı bilgiyle çağrılmalı.</summary>
        RoutineProblem ParameterProblem(RunReport report, string name)
        {
            if (!NeedsParameters) return null;
            if (!report.RoutineParameters.TryGetValue(name, out int count) || count == 0)
                return new RoutineProblem
                {
                    Title = "Kod bitti, " + name + " parametresiz",
                    Text = "`" + name + "` rutininin parametresi yok. Rutine her çağrıda ne yapacağını söylemek için `def " + name + "(yon):` gibi bir bilgi kutusu ekle.",
                };
            var seen = report.RoutineArguments[name];
            if (seen.Count < MinDistinctArguments)
                return new RoutineProblem
                {
                    Title = "Kod bitti, " + name + " hep aynı bilgiyle",
                    Text = "`" + name + "` hep aynı bilgiyle çağrıldı (`" + seen[0] + "`). Parametre, rutine her seferinde farklı bilgi vermek içindir.",
                };
            return null;
        }

        static int Runs(RunReport report, string name) => report.RoutineRuns.TryGetValue(name, out var n) ? n : 0;
```

> `/* eskisi gibi */` yerlerine bugünkü `RoutineProblem` nesnelerini aynen taşı (metinler değişmez; `RoutineTests` onları denetliyor). `using System;` (StringComparer) ekle.

- [ ] **Step 5: Testleri çalıştır**

Run: `cd motor-test && dotnet test`
Expected: hepsi geçer (1403 + 7).

---

### Görev 2: Bölüm dosyasında `parametre_sarti`

**Files:**
- Modify: `oyun/Assets/Dunya/DataFile.cs` (`DataFields.Flag`)
- Modify: `oyun/Assets/Dunya/Level.cs` (`known`, `ReadRoutines`)
- Modify: `docs/tasarim/bolum-dosyasi.md`
- Test: `motor-test/BolumTests.cs`

- [ ] **Step 1: Failing test** — `BolumTests.cs`'te `Rutin_sayisi_okunur`'un altına:

```csharp
    [Fact]
    public void Parametre_sarti_okunur()
    {
        var level = Level.Parse(Mini.Replace("\"komutlar\"", "\"rutinler\": [\"git\"], \"parametre_sarti\": true, \"komutlar\""));
        Assert.True(level.Routines.NeedsParameters);
        Assert.False(Level.Parse(Mini).Routines.NeedsParameters);
    }

    [Theory]
    [InlineData("\"parametre_sarti\": true, ", "rutin şartıyla birlikte")]
    [InlineData("\"rutinler\": [\"git\"], \"parametre_sarti\": 1, ", "true ya da false")]
    public void Bozuk_parametre_sarti_anlasilir_mesaj_verir(string field, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace("\"komutlar\"", field + "\"komutlar\"")));
        Assert.Contains(expected, e.Message);
    }
```

Run: `cd motor-test && dotnet test --filter BolumTests` → derleme hatası.

- [ ] **Step 2: `DataFields`** (`Text`'in altına):

```csharp
        public static bool Flag(Dictionary<string, object> d, string key)
        {
            if (Field(d, key) is bool b) return b;
            throw new DataFormatError("\"" + key + "\" true ya da false olmalı (tırnaksız).");
        }
```

- [ ] **Step 3: `Level.cs`** — `known` kümesine `"parametre_sarti"`; `ReadRoutines`'in sonu:

```csharp
            bool needsParameters = d.ContainsKey("parametre_sarti") && Flag(d, "parametre_sarti");
            if (needsParameters && names.Count == 0 && count == 0)
                throw new DataFormatError("\"parametre_sarti\" yalnızca rutin şartıyla birlikte yazılır: \"rutinler\" ya da \"rutin_sayisi\" de ekle.");
            level.Routines = new RoutineRule(names, count, needsParameters);
```

Dosya başı yorumuna (rutin satırı yoksa harita açıklamasının altına) bir satır: `// "parametre_sarti": rutinler bilgi (parametre) almalı ve farklı bilgilerle çağrılmalı (Bölge 7).`

- [ ] **Step 4: Testleri çalıştır** — `cd motor-test && dotnet test` → hepsi geçer.

- [ ] **Step 5: Kılavuz** — `docs/tasarim/bolum-dosyasi.md` alan tablosunda `rutin_sayisi` satırının altına:

```markdown
| `parametre_sarti` | İsteğe bağlı (Bölge 7'den itibaren, parametreli fonksiyonlar). `true` ise rutin şartına giren her rutin (`rutinler` ya da `rutin_sayisi`) en az bir **parametre** almalı (`def git(yon):`) ve kod çalışırken **en az iki farklı bilgiyle** çağrılmalı (`git(South)` ve `git(East)`). Varsayılan değer de bilgiden sayılır: `git(East)` ile `git(East, 3)` farklıdır. Yalnızca rutin şartıyla birlikte yazılır. |
```

---

### Görev 3: İz (`Trail`) ve `scout()`

**Files:**
- Create: `oyun/Assets/Dunya/Trail.cs`
- Modify: `oyun/Assets/Dunya/World.cs` (`Scouted` olayı, `trail` alanı, yapıcı, `Scout()`, `AllCommands`, `Commands()`)
- Test: `motor-test/ScoutTests.cs` (yeni)

**Interfaces:**
- Produces:
  - `public sealed class Trail { IReadOnlyList<Direction> Steps; IReadOnlyList<Cell> Cells; Cell End; int IndexOf(Cell); bool CrossesItself; Trail(Cell start, IEnumerable<Direction> steps) }` (`Cells[0]` başlangıç, `Cells.Count == Steps.Count + 1`)
  - `public sealed class Scouted : WorldEvent { public Cell At; public Direction Direction; }`
  - `World(..., BigInteger? expectedReport = null, Trail trail = null)`: iz varsa `Target = trail.End` (target parametresi null verilir)
  - `World.Scout() : Direction`; komut `scout()` yönü metin olarak döndürür (`"East"`), `move(scout())` doğrudan çalışır
  - `World.AllCommands` sonuna `"scout"`

- [ ] **Step 1: Failing test** — `motor-test/ScoutTests.cs`:

```csharp
using MarsKod.Dunya;

namespace MotorTest;

// Serçe ve iz (Bölge 7): scout() robotun durduğu karede izin sonraki yönünü söyler; hedef izin son karesidir.
public class ScoutTests
{
    // Robot (0,0); iz: kuzey, kuzey, doğu -> son kare (1,2)
    static readonly Direction[] Steps = { Direction.North, Direction.North, Direction.East };
    static World WithTrail() => new World(6, 6, new Cell(0, 0), new Cell[0], null, null, new[] { "move", "scout" },
        trail: new Trail(new Cell(0, 0), Steps));

    [Fact]
    public void Iz_kareleri_ve_sonu()
    {
        var trail = new Trail(new Cell(0, 0), Steps);
        Assert.Equal(new[] { new Cell(0, 0), new Cell(0, 1), new Cell(0, 2), new Cell(1, 2) }, trail.Cells);
        Assert.Equal(new Cell(1, 2), trail.End);
        Assert.Equal(2, trail.IndexOf(new Cell(0, 2)));
        Assert.Equal(-1, trail.IndexOf(new Cell(3, 3)));
        Assert.False(trail.CrossesItself);
        Assert.True(new Trail(new Cell(0, 0), new[] { Direction.North, Direction.East, Direction.South, Direction.West }).CrossesItself);
    }

    [Fact]
    public void Scout_ile_iz_sonuna_varilir_hedef_izin_sonu()
    {
        var world = WithTrail();
        var report = ProgramRun.Execute("for i in range(3):\n    move(scout())\n", world);
        Assert.True(report.Complete);
        Assert.Equal(new Cell(1, 2), world.Target.Value);
        Assert.Contains(report.Trace, e => e.Events.Exists(ev => ev is Scouted s && s.Direction == Direction.East && s.At.Equals(new Cell(0, 2))));
    }

    [Fact]
    public void Scout_yonu_metin_olarak_dondurur()
    {
        var report = ProgramRun.Execute("yon = scout()\nprint(yon)\n", WithTrail());
        Assert.Equal("North\n", report.Output);
    }

    [Fact]
    public void Iz_disinda_scout_oyun_kuralidir()
    {
        var report = ProgramRun.Execute("move(East)\nscout()\n", WithTrail());
        Assert.Equal("Serçe izi göremiyor", report.Rule.Title);
        Assert.Equal(2, report.StopLine);
    }

    [Fact]
    public void Iz_sonunda_scout_oyun_kuralidir()
    {
        var report = ProgramRun.Execute("for i in range(4):\n    move(scout())\n", WithTrail());
        Assert.Equal("İz burada bitiyor", report.Rule.Title);
    }

    [Fact]
    public void Izin_sonuna_varmayan_kod_tamam_degil()
    {
        var report = ProgramRun.Execute("for i in range(2):\n    move(scout())\n", WithTrail());
        Assert.False(report.Stopped);
        Assert.False(report.Complete);
    }

    [Fact]
    public void Scout_bilgi_almaz()
    {
        var report = ProgramRun.Execute("scout(East)\n", WithTrail());
        Assert.Equal("TypeError", report.Error.Type);
    }
}
```

Run: `cd motor-test && dotnet test --filter ScoutTests` → derleme hatası.

- [ ] **Step 2: `Trail.cs`**

```csharp
// Göç izi (Bölge 7): robotun başlangıç karesinden adım adım yönler. İzi yalnızca Serçe yukarıdan görür; scout()
// robotun durduğu karede izin bir sonraki yönünü söyler. Hedef izin son karesidir. Bölüm dosyasında: "izler".

using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    public sealed class Trail
    {
        public readonly IReadOnlyList<Direction> Steps;
        /// <summary>İzin kareleri: Cells[0] başlangıç, Cells[i] i. adımdan sonra varılan kare</summary>
        public readonly IReadOnlyList<Cell> Cells;

        public Trail(Cell start, IEnumerable<Direction> steps)
        {
            Steps = steps.ToList();
            var cells = new List<Cell> { start };
            foreach (var step in Steps) cells.Add(cells[cells.Count - 1].Step(step));
            Cells = cells;
        }

        public Cell End => Cells[Cells.Count - 1];

        /// <summary>Karenin izdeki sırası (0 başlangıç); izde değilse -1</summary>
        public int IndexOf(Cell c)
        {
            for (int i = 0; i < Cells.Count; i++)
                if (Cells[i].Equals(c)) return i;
            return -1;
        }

        /// <summary>İz aynı kareden iki kez geçiyor mu (o zaman scout() o karede iki yön arasında kalırdı)</summary>
        public bool CrossesItself => Cells.Distinct().Count() != Cells.Count;
    }
}
```

- [ ] **Step 3: `World.cs`**

`ReportSent`'in altına olay:

```csharp
    /// <summary>Serçe yukarıdan baktı ve izin bu kareden gittiği yönü söyledi (scout()).</summary>
    public sealed class Scouted : WorldEvent
    {
        public Cell At;
        public Direction Direction;
    }
```

Alan (`allowed`'ın üstüne): `/// <summary>Serçe'nin gördüğü iz; yoksa null (scout() çalışmaz)</summary> readonly Trail trail;`

Yapıcıya son parametre `Trail trail = null`; gövdede `Target = target;` → `Target = target ?? trail?.End;` ve `this.trail = trail;`. Yapıcının üstündeki yoruma ekle: "İz verilirse hedef izin son karesidir."

`AllCommands` sonuna `"scout"`.

`Report`'un altına:

```csharp
        /// <summary>Serçe'ye sorar: izin robotun durduğu kareden gittiği yön. Robot izde değilse ya da iz bittiyse robot durur (oyun kuralı).</summary>
        public Direction Scout()
        {
            int i = trail == null ? -1 : trail.IndexOf(Robot);
            if (i < 0)
                throw new GameRuleStop("Serçe izi göremiyor",
                    "Kıvılcım izin üstünde değil, Serçe ona yön gösteremez; robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil: izden ayrılma, her yol ayrımında scout() ile sor ve cevabı kullan.");
            if (i == trail.Steps.Count)
                throw new GameRuleStop("İz burada bitiyor",
                    "Kıvılcım izin son karesinde; Serçe gösterecek yön bulamadı, robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil: iz kaç adımsa Serçe'ye o kadar sor.");
            var direction = trail.Steps[i];
            Events.Add(new Scouted { At = Robot, Direction = direction });
            return direction;
        }
```

`Commands()` sözlüğüne (`report`'un altına):

```csharp
                ["scout"] = new PyBuiltin("scout", (args, kwargs) =>
                {
                    Unlocked("scout");
                    NoKeywords("scout", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "scout() takes no arguments (" + args.Count + " given)");
                    return Scout().ToString(); // yönler metin olarak tutulur: move(scout()) doğrudan çalışır
                }),
```

- [ ] **Step 4: Testleri çalıştır** — `cd motor-test && dotnet test` → hepsi geçer. (`Level.DerivePieces` `scout` için `scout()` üretir; değişiklik gerekmez.)

---

### Görev 4: Bölüm dosyasında `izler` + değişen yol

**Files:**
- Create: `oyun/Assets/Dunya/TrailRuns.cs`
- Modify: `oyun/Assets/Dunya/Level.cs` (`Trails`, `CreateWorld(int)`, `RunTrails`, `Run`, `ReadMap` imzası, yeni `ReadTrails`, dosya başı yorumu)
- Modify: `docs/tasarim/bolum-dosyasi.md`
- Test: `motor-test/TrailTests.cs` (yeni)

**Interfaces:**
- Consumes: `Trail`, `World(..., trail:)` (Görev 3), `RoutineRule` (Görev 1)
- Produces:
  - `Level.Trails : List<Trail>` (boş = iz yok), `Level.CreateWorld(int trail = 0)`, `Level.RunTrails(string code) : List<TrailRun>` (iz yoksa tek koşu), `Level.Run(string code) : RunReport` = `TrailRuns.Pick(RunTrails(code), 0).Report` (hepsi tamamsa `Complete`, değilse ilk başarısız izin raporu)
  - `public sealed class TrailRun { public readonly World World; public readonly RunReport Report; }`
  - `public static class TrailRuns { TrailRun Pick(IReadOnlyList<TrailRun> runs, int turn); bool Mixed(IReadOnlyList<TrailRun> runs); }`

- [ ] **Step 1: Failing test** — `motor-test/TrailTests.cs`:

```csharp
using MarsKod.Dunya;

namespace MotorTest;

// Bölüm dosyasında "izler" (Bölge 7): tek iz sabit yol, 2-3 iz değişen yol. Kod her izde ayrı denenir;
// görev hepsinde bitince biter. Ekranda başarısız iz ya da (hepsi tamamsa) sıradaki iz oynatılır.
public class TrailTests
{
    // İki iz: kuzey-kuzey ya da doğu-doğu
    const string Fork = """
        {
          "numara": 99, "baslik": "Çatal", "gorev": "Serçe'ye sor",
          "harita": [
            ". . . . . .",
            ". . . . . .",
            ". . . . . .",
            ". . . . . .",
            ". . . . . .",
            "R . . . . ."
          ],
          "izler": [["North", "North"], ["East", "East"]],
          "komutlar": ["move", "scout"],
          "ipuclari": ["Serçe'ye sor."],
          "tipik_hatalar": [ { "kod": ["move(North)", "move(North)"], "aciklama": "Ezber: ikinci izde yanlış." } ],
          "cozum": ["for i in range(2):", "    move(scout())"]
        }
        """;

    static DataFormatError Broken(string from, string to) => Assert.Throws<DataFormatError>(() => Level.Parse(Fork.Replace(from, to)));

    [Fact]
    public void Izler_okunur_hedef_izin_sonu()
    {
        var level = Level.Parse(Fork);
        Assert.Equal(2, level.Trails.Count);
        Assert.Null(level.Target);
        Assert.Equal(new Cell(0, 2), level.CreateWorld(0).Target.Value);
        Assert.Equal(new Cell(2, 0), level.CreateWorld(1).Target.Value);
        Assert.Empty(LevelCheck.Problems(level)); // görev yok hatası vermez, çözüm iki izde de biter
    }

    [Fact]
    public void Ezber_cozum_bir_izde_takilir()
    {
        var level = Level.Parse(Fork);
        var runs = level.RunTrails("move(North)\nmove(North)\n");
        Assert.True(runs[0].Report.Complete);
        Assert.False(runs[1].Report.Complete);
        Assert.False(level.Run("move(North)\nmove(North)\n").Complete);
        Assert.True(TrailRuns.Mixed(runs));
    }

    [Fact]
    public void Basarisiz_iz_secilir_hepsi_tamamsa_sirayla()
    {
        var level = Level.Parse(Fork);
        var bad = level.RunTrails("move(North)\nmove(North)\n");
        Assert.Same(bad[1], TrailRuns.Pick(bad, 0));
        Assert.Same(bad[1], TrailRuns.Pick(bad, 7));
        var good = level.RunTrails(level.Solution);
        Assert.Same(good[0], TrailRuns.Pick(good, 0));
        Assert.Same(good[1], TrailRuns.Pick(good, 1));
        Assert.Same(good[0], TrailRuns.Pick(good, 2));
        Assert.False(TrailRuns.Mixed(good));
    }

    [Fact]
    public void Izsiz_bolum_tek_kosu()
    {
        var level = Level.Parse(Fork.Replace("\"izler\": [[\"North\", \"North\"], [\"East\", \"East\"]],", "").Replace("\"scout\"", "\"collect\"")
            .Replace("\"R . . . . .\"", "\"R . . . . H\""));
        Assert.Single(level.RunTrails("move(East)\n"));
    }

    [Theory]
    [InlineData("\"izler\": [[\"North\", \"North\"], [\"East\", \"East\"]],", "", "\"izler\" yok")]
    [InlineData("\"komutlar\": [\"move\", \"scout\"]", "\"komutlar\": [\"move\"]", "\"scout\" yok")]
    [InlineData("[\"East\", \"East\"]", "[\"East\", \"East\", \"East\"]", "aynı uzunlukta")]
    [InlineData("[\"East\", \"East\"]", "[\"West\", \"West\"]", "alanın dışına")]
    [InlineData("[\"East\", \"East\"]", "[\"East\", \"West\"]", "iki kez geçiyor")]
    [InlineData("[\"East\", \"East\"]", "[\"North\", \"North\"]", "aynı iz iki kez")]
    [InlineData("[\"East\", \"East\"]", "[\"East\", \"Doğu\"]", "bilinmeyen yön")]
    [InlineData("\"R . . . . .\"", "\"R K . . . .\"", "kayanın")]
    [InlineData("\"R . . . . .\"", "\"R . . . . H\"", "hedef (H) yazılmaz")]
    [InlineData("[[\"North\", \"North\"], [\"East\", \"East\"]]", "[]", "1-3 iz")]
    public void Bozuk_iz_anlasilir_mesaj_verir(string from, string to, string expected)
    {
        Assert.Contains(expected, Broken(from, to).Message);
    }
}
```

> Ham metin (`"""`) yalnızca testte (motor-test .NET 10; `BolumTests.Mini` de böyle). "kayanın": (1,0)'a kaya → ikinci iz (doğu-doğu) ilk adımda kayaya basar. "hedef": H (5,0)'da, izlerin dışında.

Run: `cd motor-test && dotnet test --filter TrailTests` → derleme hatası.

- [ ] **Step 2: `TrailRuns.cs`**

```csharp
// Değişen yol (Bölge 7, Bölüm 68-70): oyuncunun kodu her izde yeni bir dünyada ayrı çalışır. Görev hepsinde bitince biter.
// Ekranda hangi koşunun oynatılacağı: başarısız olan varsa ilki (oyuncu kodun nerede takıldığını görür), hepsi tamamsa
// her "Çalıştır"da sıradaki iz (oyuncu yolun değiştiğini görür).

using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    /// <summary>Kodun bir izdeki koşusu: o izin dünyası ve olanların kaydı</summary>
    public sealed class TrailRun
    {
        public readonly World World;
        public readonly RunReport Report;

        public TrailRun(World world, RunReport report)
        {
            World = world;
            Report = report;
        }
    }

    public static class TrailRuns
    {
        /// <summary>Gösterilecek koşu. turn: kaçıncı "Çalıştır" (0'dan başlar).</summary>
        public static TrailRun Pick(IReadOnlyList<TrailRun> runs, int turn)
        {
            var failed = runs.FirstOrDefault(r => !r.Report.Complete);
            return failed ?? runs[turn % runs.Count];
        }

        /// <summary>Kod bazı izlerde başarılı, bazılarında değil mi (yol değişince takılan kod)</summary>
        public static bool Mixed(IReadOnlyList<TrailRun> runs) =>
            runs.Any(r => r.Report.Complete) && runs.Any(r => !r.Report.Complete);
    }
}
```

- [ ] **Step 3: `Level.cs`**

Dosya başı yorumuna: `//   İz ("izler"): Serçe'nin gördüğü yön listeleri; varsa scout() açılır, H yazılmaz (hedef izin son karesi). 2-3 iz = değişen yol.`

Alan (`Target`'ın altına): `/// <summary>Serçe'nin gördüğü izler (Bölge 7); boşsa iz yok. Birden çoksa değişen yol: kod her izde denenir.</summary> public List<Trail> Trails = new List<Trail>();` ve `/// <summary>Bir bölümde en fazla kaç iz</summary> public const int MaxTrails = 3;`

`CreateWorld` ve `Run`:

```csharp
        /// <summary>Bölümün yeni dünyası; değişen yolda trail kaçıncı izin dünyası olduğu</summary>
        public World CreateWorld(int trail = 0) => new World(Cols, Rows, Robot, Ices, Rocks, Target, Commands, Crystals, Panels, Dust, Reels, ExpectedReport,
            Trails.Count > 0 ? Trails[trail] : null);

        /// <summary>Kodu her izde (iz yoksa bir kez) bölümün yeni bir dünyasında, bölümün rutin şartıyla çalıştırır</summary>
        public List<TrailRun> RunTrails(string code)
        {
            var runs = new List<TrailRun>();
            for (int i = 0; i < Math.Max(1, Trails.Count); i++)
            {
                var world = CreateWorld(i);
                runs.Add(new TrailRun(world, ProgramRun.Execute(code, world, routines: Routines)));
            }
            return runs;
        }

        /// <summary>Denetleyici ve testler için tek sonuç: her izde tamamsa tamam; değilse ilk başarısız izin raporu</summary>
        public RunReport Run(string code) => TrailRuns.Pick(RunTrails(code), 0).Report;
```

`Parse`: `known` kümesine `"izler"`; `var dustCells = ReadMap(level, TextList(d, "harita"));` → `ReadMap(level, TextList(d, "harita"), d.ContainsKey("izler"))`; `ReadReport(level, d);` satırının altına `ReadTrails(level, d);`.

`ReadMap(Level level, List<string> rows, bool hasTrails)`: "Haritada görev yok" koşuluna `&& !hasTrails` ekle, mesajın sonuna `", ya da Serçe'nin izi (\"izler\")"` ekle (iz varsa görev izin sonuna varmaktır).

Yeni yardımcı (`ReadReport`'un altına):

```csharp
        /// <summary>"izler": Serçe'nin gördüğü izler. scout komutu açıksa zorunlu, kapalıysa yazılmaz. Her iz robotun
        /// karesinden başlar; kaya üstünden ve alanın dışından geçmez, kendini kesmez; izler aynı uzunlukta ve farklı olur.</summary>
        static void ReadTrails(Level level, Dictionary<string, object> d)
        {
            bool open = level.Commands.Contains("scout");
            if (d.ContainsKey("izler") != open)
                throw new DataFormatError(open
                    ? "\"komutlar\" içinde \"scout\" var ama \"izler\" yok; Serçe'nin göreceği izi yaz (örn. \"izler\": [[\"North\", \"East\"]])."
                    : "\"izler\" yazılmış ama \"komutlar\" içinde \"scout\" yok; oyuncu Serçe'ye soramaz.");
            if (!open) return;
            if (level.Target != null)
                throw new DataFormatError("\"izler\" olan bölümde haritaya hedef (H) yazılmaz: hedef izin son karesidir.");
            if (!(d["izler"] is List<object> list) || list.Count < 1 || list.Count > MaxTrails)
                throw new DataFormatError("\"izler\" 1-" + MaxTrails + " iz içeren bir liste olmalı; her iz yönlerden oluşan bir liste, örn. [[\"North\", \"East\"]].");
            for (int i = 0; i < list.Count; i++)
                level.Trails.Add(ReadTrail(level, list[i], i + 1));
            var lengths = level.Trails.Select(t => t.Steps.Count).ToList();
            if (lengths.Distinct().Count() > 1)
                throw new DataFormatError("\"izler\" içindeki izler aynı uzunlukta olmalı (oyuncu sabit sayılı döngü yazabilsin); şimdi: " + string.Join(", ", lengths) + ".");
            if (level.Trails.Select(t => string.Join(",", t.Steps)).Distinct().Count() < level.Trails.Count)
                throw new DataFormatError("\"izler\" içinde aynı iz iki kez yazılmış; değişen yolda izler farklı olmalı.");
        }

        static Trail ReadTrail(Level level, object value, int number)
        {
            string where = "\"izler\" içindeki " + number + ". iz";
            if (!(value is List<object> steps) || steps.Count == 0)
                throw new DataFormatError(where + " boş olmayan bir yön listesi olmalı, örn. [\"North\", \"East\"].");
            var directions = new List<Direction>();
            foreach (var step in steps)
            {
                if (!(step is string s && Enum.TryParse(s, out Direction dir) && s == dir.ToString()))
                    throw new DataFormatError(where + " içinde bilinmeyen yön: \"" + step + "\". Yönler: North, East, South, West.");
                directions.Add(dir);
            }
            var trail = new Trail(level.Robot, directions);
            for (int j = 1; j < trail.Cells.Count; j++)
            {
                var cell = trail.Cells[j];
                if (cell.Col < 0 || cell.Col >= level.Cols || cell.Row < 0 || cell.Row >= level.Rows)
                    throw new DataFormatError(where + " " + j + ". adımda alanın dışına çıkıyor.");
                if (level.Rocks.Contains(cell))
                    throw new DataFormatError(where + " " + j + ". adımda kayanın üstünden geçiyor.");
            }
            if (trail.CrossesItself)
                throw new DataFormatError(where + " aynı kareden iki kez geçiyor; iz kendini kesmemeli (Serçe o karede iki yön arasında kalır).");
            return trail;
        }
```

> `dir`, `World.DirectionOf`'taki desenle aynı: `throw`'dan sonra derleyici onu atanmış sayar.

- [ ] **Step 4: Testleri çalıştır** — `cd motor-test && dotnet test` → hepsi geçer (eski bölümler değişmeden).

- [ ] **Step 5: Kılavuz** — `docs/tasarim/bolum-dosyasi.md`:
  - Harita tablosuna `| `C` | Nehir çakılı (toplanacak; Bölge 7: Bölüm 61-70) |` (Görev 5 ile birlikte).
  - `komutlar` açıklamasına: `scout` ("Serçe yukarıdan bakar, izin robotun durduğu kareden hangi yöne gittiğini söyler: North/East/South/West; robot izde değilse ya da iz bittiyse robot durur. Yalnızca `izler` olan bölümde açılır.").
  - Alan tablosuna `parametre_sarti`'nın altına:

```markdown
| `izler` | İsteğe bağlı (Bölge 7). Serçe'nin gördüğü iz: robotun karesinden başlayan yön listesi, örn. `[["South", "South", "East"]]`. `komutlar` içinde `scout` varsa zorunlu, yoksa yazılmaz. Haritaya `H` yazılmaz: hedef izin son karesidir (oyuncu görmez, varınca belirir). İz kaya üstünden ve alanın dışından geçmez, kendini kesmez; panel, toz, toplanacak karelerden geçebilir. **2-3 iz = değişen yol:** oyuncunun kodu her izde ayrı denenir, görev hepsinde bitince biter. İzler aynı uzunlukta olur (görev yazısında sayıyı söyle: "7 adım"). Toplanacak ve onarılacaklar bütün izlerin ortak parçasına (aynı adımda) konur. Değişen yol bölümünde `tipik_hatalar` içinde ilk izin yönleri elle yazılmış bir "ezber" çözüm olsun. |
```

  - "Görev:" paragrafına: `(iz varsa) robot izin son karesinde` ekle.

---

### Görev 5: Nehir yatağı, çakıl `C`, sahnede iz ve geçici görünüşler

**Files:**
- Modify: `oyun/Assets/Dunya/Collectible.cs`, `oyun/Assets/Dunya/Region.cs`
- Modify: `oyun/Assets/Scripts/Pickup.cs:22`, `Hud.cs:423`, `RegionLook.cs:94`, `Obstacles.cs:17`, `Oyun.cs` (`RunProgram`, `Play`, `ShowStop`, `LoadLevel`)
- Test: `motor-test/BolumTests.cs` (`Her_on_bolum_yeni_bolge`)

**Interfaces:**
- Produces: `Collectible.Pebble` (işaret `'C'`, ad "çakıl" / "çakıllar"), `Region.RiverBed` (Bölüm 61-70), `Regions.Item(Region.RiverBed) == Collectible.Pebble`

- [ ] **Step 1: Failing test** — `Her_on_bolum_yeni_bolge` sonuna:

```csharp
        Assert.Equal(Region.RiverBed, Regions.Of(61));
        Assert.Equal(Region.RiverBed, Regions.Of(70));
        Assert.Equal(Collectible.Pebble, Regions.Item(Region.RiverBed));
        Assert.Equal(Collectible.Pebble, Collectibles.Marks['C']);
        Assert.Equal("çakıl", Collectibles.Name(Collectible.Pebble));
```

Run: `cd motor-test && dotnet test --filter Her_on_bolum_yeni_bolge` → derleme hatası.

- [ ] **Step 2: Dünya tarafı** — `Collectible.cs`: enum'a `Pebble`; `Marks`'a `['C'] = Collectible.Pebble,`; `Names`'e `[Collectible.Pebble] = ("çakıl", "çakıllar"),`. `Region.cs`: enum'a ve `Order`'ın sonuna `RiverBed`; `Item`'a `case Region.RiverBed: return Collectible.Pebble;`; özet yorumuna "kanyon dibi (eski nehir yatağı): nehir çakılı". `Level.cs` dosya başındaki işaret satırına `C nehir çakılı` (Collectibles'tan zaten okunur; yalnızca yorum).

Run: `cd motor-test && dotnet test` → hepsi geçer.

- [ ] **Step 3: Sahne — geçici görünüşler (asıl görünüş Bölge 7 · 3a)**

- `Pickup.cs`: `case Collectible.Pebble: return DronePart.Create(parent, localPos, seed); // gecici: cakilin kendi gorunusu Bolge 7 gorunus isinde`
- `Hud.cs:423`: `item == Collectible.DronePart || item == Collectible.Pebble ? ... DrawPropellerDot ...` (yorum: `// cakil: gecici`)
- `RegionLook.cs` `For`: `case Region.Canyon:` üstüne `case Region.RiverBed: // nehir yatagi kendi gorunusu gelene kadar kanyon`
- `Obstacles.cs`: aynı şekilde `case Region.RiverBed:` `case Region.Canyon:`'ın üstüne.

- [ ] **Step 4: Sahne — iz ve değişen yol (`Oyun.cs`)**

Alan (diğer oyun durumu alanlarının yanına): `int trailTurn; // degisen yolda her Calistir'da siradaki iz oynatilir (TrailRuns.Pick)`. `LoadLevel`'da `stepMode = false; ...` satırının altına `trailTurn = 0;`.

`RunProgram` başı:

```csharp
        var runs = level.RunTrails(code);
        var shown = TrailRuns.Pick(runs, trailTurn++);
        var world = shown.World;
        var report = shown.Report;
        trailChanged = TrailRuns.Mixed(runs);
```

`trailChanged` bir alan: `bool trailChanged; // son kosuda kod bir izde calisip digerinde takildi: mesajin basinda soylenir`.

Başarısızlık mesajları tek yerden geçsin (`ShowStop`'un altına):

```csharp
    const string TrailChangedNote = "Serçe bu sefer başka bir kol gördü.";

    // Kod bir izde calisip bu izde takildiysa oyuncu yolun degistigini bilsin
    void ShowFailure(string header, string title, string text, string detail, bool error)
    {
        hud.ShowMessage(header, title, trailChanged ? TrailChangedNote + " " + text : text, detail, error);
    }
```

`ShowStop` içindeki üç `hud.ShowMessage` ve `RunProgram`'ın "görev bitmedi" dalındaki beş `hud.ShowMessage` → `ShowFailure` (parametreler aynı). Son dal ("robot hedefte değil") iz varsa farklı metin:

```csharp
            else if (level.Trails.Count > 0)
                ShowFailure("GÖREV", "Kod bitti, iz bitmedi",
                    "Kodun sonuna kadar çalıştı ama Kıvılcım izin sonuna varmadı. Serçe izin kaç adım olduğunu görevde söylüyor: ona o kadar sor.",
                    null, error: false);
            else
                ShowFailure("GÖREV", "Kod bitti, robot hedefte değil", /* eskisi gibi */);
```

`Play`'e (`ReportSent`'in altına) geçici Serçe cevabı:

```csharp
            case Scouted s: // gecici: Bolge 7 · 3a'da Serce yol ayrimina suzulup kolu isikla gosterir
                StartCoroutine(ScanPulse.Play(levelRoot, Pos(s.At.Step(s.Direction)), true));
                StartCoroutine(PowerLabel.Show(levelRoot, Pos(s.At), s.Direction.ToString(), Mats.Hex("#5FD4C4")));
                yield return Tween.Wait(0.5f);
                break;
```

> Hedef: `izler` olan bölümde `level.Target` null olduğu için `Target.Create` çağrılmaz, hedef görünmez (spec: varınca belirir → 3a). Başka değişiklik gerekmez.

- [ ] **Step 5: Derleme denetimi** — `cd motor-test && dotnet test` (dünya). Unity derlemesi Görev 8'de paketle denetlenir.

---

### Görev 6: Bölüm 61-65 + `sinav-65` + sözlük

**Files:**
- Create: `oyun/Assets/Resources/Bolumler/bolum-61.json` … `bolum-65.json`
- Create: `oyun/Assets/Resources/Sinavlar/sinav-65.json`
- Modify: `oyun/Assets/Resources/Sozluk/sozluk.json` (`def` sayfası + yeni `scout` sayfası)

Haritalarda en üst satır kuzey (row 5), sol batı (col 0). Her bölümde 3 ipucu (yön, konu, kod parçası) ve parametresiz bir tipik hata var. Bir bölüm `dotnet test`'te düşerse mesajdaki satıra göre haritayı/çözümü düzelt; tablodaki "Öğrettiği" değişmesin.

- [ ] **Step 1: `bolum-61.json` (Nehir yatağı)**

```json
{
  "numara": 61,
  "baslik": "Nehir yatağı",
  "gorev": "git rutinine yön ver",
  "harita": [
    "R . . K . .",
    "Z K . . . .",
    "C . Z . K .",
    ". K . . . .",
    ". . C Z H .",
    ". . . . . K"
  ],
  "toz_suresi": [2, 3, 2],
  "komutlar": ["move", "collect", "dust_here", "wait"],
  "rutinler": ["git"],
  "parametre_sarti": true,
  "parcalar": ["def git(yon):", "git(South)", "git(East)", "move(yon)", "while dust_here():", "wait()", "collect()"],
  "konular": ["fonksiyon", "parametre"],
  "ipuclari": [
    "Bölge 6'nın sonunda iki rutin yazmıştın: biri güneye, biri doğuya bir adım + toz beklemesi. İkisi aynı iş, yalnızca yön farklı.",
    "Rutine bir bilgi kutusu (parametre) ekle: `def git(yon):`. Çağırırken kutuya yönü koyarsın: `git(South)`, `git(East)`. Rutinin içinde `move(yon)`.",
    "`def git(yon):` altına `    move(yon)`, `    while dust_here():`, `        wait()`. Sonra: `git(South)`, `git(South)`, `collect()`, `git(East)`..."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def git():", "    move(South)", "    while dust_here():", "        wait()", "git(South)", "git(South)"],
      "aciklama": "Tanımda bilgi kutusu yok ama çağrıda yön veriliyor: Python git'in bilgi almadığını söyler (TypeError). Tanıma kutuyu ekle: def git(yon):"
    },
    {
      "kod": ["def git(yon):", "    move(yon)", "    while dust_here():", "        wait()", "git()", "git()"],
      "aciklama": "Tanımda kutu var ama çağrıda boş bırakılmış: Python yon'un eksik olduğunu söyler (TypeError). git(South) gibi yönü ver."
    },
    {
      "kod": ["def asagi():", "    move(South)", "    while dust_here():", "        wait()", "def doguya():", "    move(East)", "    while dust_here():", "        wait()", "asagi()", "asagi()", "collect()", "doguya()", "doguya()", "asagi()", "asagi()", "collect()", "doguya()", "doguya()"],
      "aciklama": "Robot hedefe varır ama iki ayrı rutin aynı işi yapıyor: bu bölüm yönü parametre olarak alan tek bir git rutini istiyor."
    }
  ],
  "baslangic_kodu": [
    "def git(yon):",
    "    move(yon)"
  ],
  "cozum": [
    "def git(yon):",
    "    move(yon)",
    "    while dust_here():",
    "        wait()",
    "git(South)",
    "git(South)",
    "collect()",
    "git(East)",
    "git(East)",
    "git(South)",
    "git(South)",
    "collect()",
    "git(East)",
    "git(East)"
  ]
}
```

- [ ] **Step 2: `bolum-62.json` (Serçe yukarıdan)**

```json
{
  "numara": 62,
  "baslik": "Serçe yukarıdan",
  "gorev": "Serçe'ye sor: 5 adım",
  "harita": [
    ". . K . . .",
    ". K . . . .",
    ". K . . K .",
    ". . . . K .",
    ". K K K . .",
    "R K . . . ."
  ],
  "izler": [["North", "North", "East", "East", "East"]],
  "komutlar": ["move", "scout"],
  "rutinler": ["git"],
  "parametre_sarti": true,
  "parcalar": ["def git(yon):", "git(scout())", "move(yon)", "scout()", "for i in range(3):"],
  "konular": ["fonksiyon", "parametre", "komutun sonucunu kullanmak"],
  "ipuclari": [
    "Aşağıdan iki kol aynı görünüyor; izleri yalnızca Serçe yukarıdan görür. Serçe 5 adımlık iz görüyor.",
    "`scout()` Serçe'ye sorar ve izin yönünü verir (örn. `East`). Bu cevabı doğrudan rutine verebilirsin: `git(scout())`.",
    "`def git(yon):` altına `    move(yon)`. Sonra `for i in range(5):` altına `    git(scout())`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def git(yon):", "    move(yon)", "for i in range(5):", "    git(scout)"],
      "aciklama": "Parantezsiz scout komutun kendisidir, cevabı değil; robot ona yön diye bakar ve anlamaz. Serçe'ye sormak için scout()."
    },
    {
      "kod": ["def git(yon):", "    move(yon)", "for i in range(6):", "    git(scout())"],
      "aciklama": "İz 5 adım: altıncı soruda Serçe gösterecek yön bulamaz."
    },
    {
      "kod": ["def git():", "    move(scout())", "for i in range(5):", "    git()"],
      "aciklama": "Robot izi yürür ama git'in bilgi kutusu yok: bu bölüm Serçe'nin cevabını rutine parametre olarak vermeni istiyor."
    }
  ],
  "baslangic_kodu": [
    "# serçe yukarıdan bakar, sen yürü :) - E.",
    "def git(yon):",
    "    move(yon)"
  ],
  "cozum": [
    "def git(yon):",
    "    move(yon)",
    "for i in range(5):",
    "    git(scout())"
  ]
}
```

- [ ] **Step 3: `bolum-63.json` (Göç izleri)**

```json
{
  "numara": 63,
  "baslik": "Göç izleri",
  "gorev": "ilerle'ye sayı ver",
  "harita": [
    ". . . . K .",
    ". . . . . H",
    "K . . K . .",
    ". . . . C .",
    ". . K . . .",
    "R C . . . K"
  ],
  "komutlar": ["move", "collect"],
  "rutinler": ["ilerle"],
  "parametre_sarti": true,
  "parcalar": ["def ilerle(kac):", "for i in range(kac):", "move(East)", "move(North)", "ilerle(2)", "collect()"],
  "konular": ["fonksiyon", "parametre", "sayı parametresi"],
  "ipuclari": [
    "İz doğuya hep farklı uzunlukta gidiyor: önce 1 kare, sonra 3, sonra yine 1. Aradaki kuzey adımları ana kodda kalsın.",
    "Parametre sayı da olabilir: `def ilerle(kac):` içinde `for i in range(kac):` ile kaç adım gidileceğini kutudan alırsın.",
    "`def ilerle(kac):`, `    for i in range(kac):`, `        move(East)`. Sonra: `ilerle(1)`, `collect()`, `move(North)`, `move(North)`, `ilerle(3)`..."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def ilerle(kac):", "    for i in range(kac):", "        move(East)", "ilerle(1)", "collect()", "move(North)", "move(North)", "ilerle(1)", "ilerle(1)", "ilerle(1)", "collect()", "move(North)", "move(North)", "ilerle(1)"],
      "aciklama": "Robot hedefe varır ama ilerle hep aynı sayıyla (1) çağrıldı: kutuya her seferinde gereken sayıyı koy, ilerle(3) gibi."
    },
    {
      "kod": ["def ilerle(kac):", "    for i in range(kac):", "        move(East)", "ilerle(kac)"],
      "aciklama": "kac yalnızca rutinin içinde var; dışarıda Python onu tanımaz (NameError). Çağırırken sayıyı yaz: ilerle(1)."
    },
    {
      "kod": ["def ilerle(kac):", "    move(East)", "ilerle(1)", "collect()", "move(North)", "move(North)", "ilerle(3)", "collect()", "move(North)", "move(North)", "ilerle(1)"],
      "aciklama": "Rutin kac'ı hiç kullanmıyor: ilerle(3) de tek adım gider. range(kac) ile kutudaki sayı kadar yürü."
    },
    {
      "kod": ["def ilerle():", "    move(East)", "ilerle()", "collect()", "move(North)", "move(North)", "ilerle()", "ilerle()", "ilerle()", "collect()", "move(North)", "move(North)", "ilerle()"],
      "aciklama": "Robot hedefe varır ama ilerle'nin bilgi kutusu yok: kaç adım gideceğini parametreyle söyle."
    }
  ],
  "cozum": [
    "def ilerle(kac):",
    "    for i in range(kac):",
    "        move(East)",
    "ilerle(1)",
    "collect()",
    "move(North)",
    "move(North)",
    "ilerle(3)",
    "collect()",
    "move(North)",
    "move(North)",
    "ilerle(1)"
  ]
}
```

- [ ] **Step 4: `bolum-64.json` (Kayıt 3)**

```json
{
  "numara": 64,
  "baslik": "Kayıt 3",
  "gorev": "git'e yön ve sayı ver",
  "harita": [
    "R . K . . .",
    "Z . . . K .",
    "C . Z C . .",
    ". K . . . .",
    ". . K Z . K",
    "K . . . . H"
  ],
  "toz_suresi": [3, 2, 2],
  "komutlar": ["move", "collect", "dust_here", "wait"],
  "rutinler": ["git"],
  "parametre_sarti": true,
  "parcalar": [
    "def git(yon, kac):",
    "for i in range(kac):",
    "move(yon)",
    "while dust_here():",
    "wait()",
    "git(South, 2)",
    "collect()"
  ],
  "konular": ["fonksiyon", "iki parametre", "parametre sırası"],
  "ipuclari": [
    "Yol dört düz parçadan oluşuyor: güneye 2, doğuya 3, güneye 3, doğuya 2. Her parçada hem yön hem uzunluk değişiyor.",
    "Rutin iki bilgi alabilir: `def git(yon, kac):`. Çağırırken sıra aynı olmalı: önce yön, sonra sayı: `git(South, 2)`.",
    "`def git(yon, kac):`, `    for i in range(kac):`, `        move(yon)`, `        while dust_here():`, `            wait()`. Sonra `git(South, 2)`, `collect()`, `git(East, 3)`..."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "        while dust_here():", "            wait()", "git(2, South)"],
      "aciklama": "Sıra karıştı: yon'a 2, kac'a South gitti; range bir sayı ister (TypeError). Tanımdaki sırayla ver: git(South, 2)."
    },
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "        while dust_here():", "            wait()", "git(South)"],
      "aciklama": "İki kutu var, biri boş kaldı: Python kac'ın eksik olduğunu söyler (TypeError)."
    },
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "    while dust_here():", "        wait()", "git(South, 2)", "collect()", "git(East, 3)", "collect()", "git(South, 3)", "git(East, 2)"],
      "aciklama": "Toz beklemesi döngünün dışında kaldı: robot tozlu karede durmadan yürümeye çalışır. Beklemeyi her adımın içine al."
    },
    {
      "kod": ["move(South)", "while dust_here():", "    wait()", "move(South)", "collect()", "move(East)", "move(East)", "while dust_here():", "    wait()", "move(East)", "collect()", "move(South)", "move(South)", "while dust_here():", "    wait()", "move(South)", "move(East)", "move(East)"],
      "aciklama": "Robot hedefe varır ama git rutini yok: yön ve uzunluğu parametre olarak alan bir rutin yaz."
    }
  ],
  "cozum": [
    "def git(yon, kac):",
    "    for i in range(kac):",
    "        move(yon)",
    "        while dust_here():",
    "            wait()",
    "git(South, 2)",
    "collect()",
    "git(East, 3)",
    "collect()",
    "git(South, 3)",
    "git(East, 2)"
  ]
}
```

- [ ] **Step 5: `bolum-65.json` (İzler ayrılıyor)**

```json
{
  "numara": 65,
  "baslik": "İzler ayrılıyor",
  "gorev": "3 kez sor, 2'şer yürü",
  "harita": [
    ". . . . . .",
    ". . . K . .",
    "K . . . . K",
    ". K . . . .",
    "K . . K . .",
    "R . . . . ."
  ],
  "izler": [["East", "East", "North", "North", "East", "East"]],
  "komutlar": ["move", "scout"],
  "rutinler": ["git"],
  "parametre_sarti": true,
  "parcalar": [
    "def git(yon, kac):",
    "for i in range(kac):",
    "move(yon)",
    "yon = scout()",
    "git(yon, 2)",
    "for i in range(3):"
  ],
  "konular": ["fonksiyon", "parametre", "değişken"],
  "ipuclari": [
    "İz her yol ayrımından sonra 2 kare düz gidiyor; 3 yol ayrımı var. Her ayrımda Serçe'ye bir kez sor.",
    "Serçe'nin cevabını bir değişkene koyabilirsin: `yon = scout()`. Sonra o değişkeni rutine ver: `git(yon, 2)`. Değişkenin adı parametrenin adıyla aynı olmak zorunda değil.",
    "`def git(yon, kac):` + `for` ile `move(yon)`. Sonra `for i in range(3):` altına `    yon = scout()` ve `    git(yon, 2)`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "for i in range(3):", "    git(yon, 2)", "    yon = scout()"],
      "aciklama": "Önce sor, sonra yürü: ilk turda yon daha boş (NameError)."
    },
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "yon = scout()", "for i in range(3):", "    git(yon, 2)"],
      "aciklama": "Serçe'ye yalnızca bir kez soruldu: robot hep ilk yöne gider ve alanın dışına çıkar. Her yol ayrımında yeniden sor."
    },
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "for i in range(3):", "    yon = scout", "    git(yon, 2)"],
      "aciklama": "Parantezsiz scout Serçe'ye sormaz; değişkene komutun kendisi girer, robot onu yön diye anlamaz."
    },
    {
      "kod": ["def git():", "    for i in range(2):", "        move(scout())", "for i in range(3):", "    git()"],
      "aciklama": "Robot izi yürür ama git'in bilgi kutusu yok: yönü ve adım sayısını parametreyle ver."
    }
  ],
  "cozum": [
    "def git(yon, kac):",
    "    for i in range(kac):",
    "        move(yon)",
    "for i in range(3):",
    "    yon = scout()",
    "    git(yon, 2)"
  ]
}
```


- [ ] **Step 6: `sinav-65.json`**

```json
{
  "bolum": 65,
  "baslik": "KIVILCIM · KENDİNİ DENETLİYOR",
  "sorular": [
    {
      "soru": "def git(yon): satırında yon nedir?",
      "secenekler": ["Bir oyun komutu", "Rutine çağrılırken verilen bilgiyi tutan kutu (parametre)", "Her zaman East", "Bir hata"],
      "dogru": 1,
      "aciklama": "yon bir parametredir: git(South) çağrılınca rutinin içinde yon South olur, git(East) çağrılınca East."
    },
    {
      "soru": "def git(yon, kac): tanımlıyken git(3, East) yazılırsa ne olur?",
      "secenekler": ["Robot doğuya 3 adım gider", "Python sırayı kendisi düzeltir", "Bilgiler sırayla dağıtılır: yon 3, kac East olur; range(kac) hata verir", "Robot 3 kez döner"],
      "dogru": 2,
      "aciklama": "Bilgiler tanımdaki sırayla kutulara girer. Önce yön, sonra sayı: git(East, 3)."
    },
    {
      "soru": "def git(yon): tanımlıyken git() yazarsan ne olur?",
      "secenekler": ["TypeError: yon için bilgi verilmedi", "Robot yerinde durur, hata yok", "Robot rastgele bir yöne gider", "git iki kez çalışır"],
      "dogru": 0,
      "aciklama": "Kutusu olan rutin bilgi bekler; boş çağrılırsa Python eksik bilgiyi söyler (missing 1 required positional argument)."
    },
    {
      "soru": "git(scout()) satırında önce ne olur?",
      "secenekler": ["git çalışır, sonra scout", "İkisi aynı anda", "scout git'in içinde çalışır", "scout() Serçe'ye sorar; gelen yön git'e verilir"],
      "dogru": 3,
      "aciklama": "Parantezin içi önce çalışır: Serçe'nin cevabı (örn. East) git'in kutusuna girer."
    }
  ]
}
```

- [ ] **Step 7: Sözlük** — `sozluk.json`'da `def` sayfasının `aciklama`sının sonuna: ` Parantezin içine bilgi kutuları (parametre) yazılabilir: def git(yon, kac=1): Çağırırken bilgiler aynı sırayla verilir: git(South, 3). kac=1 varsayılan değerdir: yazılmazsa 1 olur.` `ornek` listesinin sonuna:

```json
        "def git(yon, kac=1):",
        "    for i in range(kac):",
        "        move(yon)",
        "git(East)       # kac = 1",
        "git(South, 3)   # kac = 3"
```

`sayfalar` listesinin sonuna:

```json
    {
      "baslik": "scout",
      "kelimeler": ["scout"],
      "aciklama": "Serçe yukarıdan bakar, izin Kıvılcım'ın durduğu kareden hangi yöne gittiğini söyler: North, East, South ya da West. Cevabı doğrudan bir rutine ya da move'a verebilir, bir değişkende de tutabilirsin. Kıvılcım izin üstünde değilse ya da iz bittiyse robot durur.",
      "ornek": [
        "move(scout())",
        "yon = scout()",
        "move(yon)"
      ]
    }
```

- [ ] **Step 8: Denetim** — `cd motor-test && dotnet test` → hepsi geçer. `Her_bes_bolumde_bir_sinav_var` en büyük bölüm 65 iken geçer. Sözlük testi `scout` sayfasının 62'de açıldığını kendiliğinden bulur.

---

### Görev 7: Bölüm 66-70 + `sinav-70` + çözümler belgesi

**Files:**
- Create: `oyun/Assets/Resources/Bolumler/bolum-66.json` … `bolum-70.json`
- Create: `oyun/Assets/Resources/Sinavlar/sinav-70.json`
- Modify: `docs/tasarim/bolum-cozumleri.md` (61-70 çözümleri, var olan biçimde)

- [ ] **Step 1: `bolum-66.json` (İkinci iz)**

```json
{
  "numara": 66,
  "baslik": "İkinci iz",
  "gorev": "kac=1 ile kısa çağır",
  "harita": [
    "R K . . . .",
    ". . . K . .",
    ". K . . . .",
    "C 3 . . K .",
    ". K . C . H",
    ". . . . . K"
  ],
  "komutlar": ["move", "collect", "panel_power", "repair"],
  "rutinler": ["git"],
  "parametre_sarti": true,
  "parcalar": [
    "def git(yon, kac=1):",
    "for i in range(kac):",
    "move(yon)",
    "git(East)",
    "git(South, 3)",
    "collect()",
    "repair()"
  ],
  "konular": ["fonksiyon", "varsayılan değer"],
  "ipuclari": [
    "Bu yolda çoğu parça tek adım, bazıları uzun. Her seferinde `git(East, 1)` yazmak yerine 1'i rutine öğretebilirsin.",
    "Varsayılan değer: `def git(yon, kac=1):`. Sayıyı yazmazsan kac 1 olur: `git(East)`. Yazarsan onu kullanır: `git(South, 3)`.",
    "`git(South, 3)`, `collect()`, `git(East)`, `repair()`, `git(East, 2)`, `git(South)`, `collect()`, `git(East, 2)`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def git(yon, kac=1):", "    move(yon)", "git(South, 3)", "collect()", "git(East)", "repair()", "git(East, 2)", "git(South)", "collect()", "git(East, 2)"],
      "aciklama": "Rutin kac'ı kullanmıyor: git(South, 3) de tek adım gider. for i in range(kac): ile kutudaki sayı kadar yürü."
    },
    {
      "kod": ["def git(yon, kac=1):", "    for i in range(kac):", "        move(yon)", "git(South, 3)", "collect()", "git(East)", "git(East, 2)", "git(South)", "collect()", "git(East, 2)"],
      "aciklama": "Robot hedefe varır ama yoldaki çatlak panel onarılmadı."
    },
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "git(South, 3)", "collect()", "git(East)"],
      "aciklama": "kac'ın varsayılan değeri yok: git(East) eksik bilgiyle çağrıldı (TypeError). Tanımda kac=1 yaz."
    },
    {
      "kod": ["move(South)", "move(South)", "move(South)", "collect()", "move(East)", "repair()", "move(East)", "move(East)", "move(South)", "collect()", "move(East)", "move(East)"],
      "aciklama": "Robot hedefe varır ama git rutini yok: yönü ve (gerekirse) sayıyı alan bir rutin yaz."
    }
  ],
  "cozum": [
    "def git(yon, kac=1):",
    "    for i in range(kac):",
    "        move(yon)",
    "git(South, 3)",
    "collect()",
    "git(East)",
    "repair()",
    "git(East, 2)",
    "git(South)",
    "collect()",
    "git(East, 2)"
  ]
}
```

- [ ] **Step 2: `bolum-67.json` (Yan kanyon)**

```json
{
  "numara": 67,
  "baslik": "Yan kanyon",
  "gorev": "8 adım: hep Serçe'ye",
  "harita": [
    ". . . K . .",
    "K . . . . .",
    ". K . Z . K",
    ". . . . K .",
    ". Z K . . .",
    "R . . . . K"
  ],
  "toz_suresi": [3, 2],
  "izler": [["North", "East", "North", "East", "East", "North", "North", "East"]],
  "komutlar": ["move", "scout", "dust_here", "wait"],
  "rutinler": ["git"],
  "parametre_sarti": true,
  "parcalar": ["def git(yon):", "move(yon)", "while dust_here():", "wait()", "git(scout())", "for i in range(4):"],
  "konular": ["fonksiyon", "parametre", "döngü içinde fonksiyon"],
  "ipuclari": [
    "Yan kanyon kıvrım kıvrım: neredeyse her adımda yol ayrılıyor. Serçe 8 adımlık iz görüyor; her adımda ona sor.",
    "Soruyu döngüye koy: her turda `git(scout())`. Yolda toz da var; beklemeyi rutinin içine al, her adımda kendiliğinden çalışsın.",
    "`def git(yon):`, `    move(yon)`, `    while dust_here():`, `        wait()`. Sonra `for i in range(8):` altına `    git(scout())`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def git(yon):", "    move(yon)", "for i in range(8):", "    git(scout())"],
      "aciklama": "Yolda toz var: rutin beklemeden yürümeye çalışır. Tozda yol görünmez; while dust_here(): ile bekle."
    },
    {
      "kod": ["def git(yon):", "    move(yon)", "    while dust_here():", "        wait()", "for i in range(7):", "    git(scout())"],
      "aciklama": "İz 8 adım: robot izin sonuna bir adım kala durur."
    },
    {
      "kod": ["def git():", "    move(scout())", "    while dust_here():", "        wait()", "for i in range(8):", "    git()"],
      "aciklama": "Robot izi yürür ama git'in bilgi kutusu yok: Serçe'nin cevabını rutine parametre olarak ver."
    }
  ],
  "cozum": [
    "def git(yon):",
    "    move(yon)",
    "    while dust_here():",
    "        wait()",
    "for i in range(8):",
    "    git(scout())"
  ]
}
```

- [ ] **Step 3: `bolum-68.json` (Boya kalemi)**

```json
{
  "numara": 68,
  "baslik": "Boya kalemi",
  "gorev": "9 adım, yol değişir",
  "harita": [
    "K . . . . K",
    ". . K . . .",
    ". . . K . .",
    ". . . . . K",
    ". . . . . .",
    ". . R . . K"
  ],
  "izler": [
    ["East", "East", "North", "West", "West", "North", "East", "East", "North"],
    ["West", "West", "North", "East", "East", "North", "West", "West", "North"]
  ],
  "komutlar": ["move", "scout"],
  "rutin_sayisi": 2,
  "parametre_sarti": true,
  "parcalar": [
    "def git(yon, kac):",
    "def kol(yon):",
    "for i in range(kac):",
    "move(yon)",
    "git(yon, 2)",
    "git(North, 1)",
    "kol(scout())",
    "for i in range(3):"
  ],
  "konular": ["fonksiyon", "parametre", "fonksiyon içinde fonksiyon", "değişen yol"],
  "ipuclari": [
    "Bu iz her denemede değişebilir: kod ezberle değil, Serçe'nin cevabıyla yürümeli. İz 3 koldan oluşuyor: her kol 2 kare yana, 1 kare kuzeye.",
    "Bir rutin, aldığı bilgiyi başka bir rutine aktarabilir: `def kol(yon):` içinde `git(yon, 2)`. Böylece iki rutin yazarsın: git ve kol.",
    "`def kol(yon):`, `    git(yon, 2)`, `    git(North, 1)`. Sonra `for i in range(3):` altına `    kol(scout())`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "def kol(yon):", "    git(yon, 2)", "    git(North, 1)", "kol(East)", "kol(West)", "kol(East)"],
      "aciklama": "Yönler elle yazıldı: bir kez çalışır ama iz değişince robot yanlış kola sapar. Her kolda Serçe'ye sor."
    },
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "def kol(yon):", "    git(yon, 2)", "    git(North, 1)", "yon = scout()", "for i in range(3):", "    kol(yon)"],
      "aciklama": "Serçe'ye yalnızca bir kez soruldu: robot hep aynı yana gider ve alanın dışına çıkar."
    },
    {
      "kod": ["def git(yon, kac):", "    for i in range(kac):", "        move(yon)", "for i in range(3):", "    git(scout(), 2)", "    git(North, 1)"],
      "aciklama": "Robot izi yürür ama bu bölüm iki rutin istiyor: kolu yürüme işini de bir rutine koy."
    }
  ],
  "cozum": [
    "def git(yon, kac):",
    "    for i in range(kac):",
    "        move(yon)",
    "def kol(yon):",
    "    git(yon, 2)",
    "    git(North, 1)",
    "for i in range(3):",
    "    kol(scout())"
  ]
}
```

- [ ] **Step 4: `bolum-69.json` (El izleri)**

```json
{
  "numara": 69,
  "baslik": "El izleri",
  "gorev": "7 adım, yol değişir",
  "harita": [
    "K . K . . K",
    ". Z . . K .",
    ". K Z . . K",
    "2 . . K . .",
    "Z . . . . K",
    "R K . . . ."
  ],
  "toz_suresi": [2, 3, 2],
  "izler": [
    ["North", "North", "East", "East", "North", "East", "East"],
    ["North", "North", "North", "North", "East", "East", "East"],
    ["North", "North", "East", "South", "East", "East", "East"]
  ],
  "komutlar": ["move", "scout", "dust_here", "wait", "panel_power", "repair"],
  "rutin_sayisi": 1,
  "parametre_sarti": true,
  "parcalar": [
    "def adim(yon):",
    "move(yon)",
    "while dust_here():",
    "wait()",
    "adim(scout())",
    "repair()",
    "for i in range(5):"
  ],
  "konular": ["fonksiyon", "parametre", "değişen yol"],
  "ipuclari": [
    "İz her denemede değişebilir ama hep 7 adım. İlk iki adım her izde aynı yerden geçer: orada çatlak bir panel var.",
    "Rutinin adını sen seç; yönü parametre olarak alsın, tozu kendisi beklesin. Paneli rutinin içinde değil, ikinci adımdan sonra bir kez onar.",
    "Rutin: `move(yon)` + `while dust_here(): wait()`. Sonra iki kez `adim(scout())`, `repair()`, `for i in range(5):` altına `adim(scout())`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def adim(yon):", "    move(yon)", "    while dust_here():", "        wait()", "adim(North)", "adim(North)", "repair()", "adim(East)", "adim(East)", "adim(North)", "adim(East)", "adim(East)"],
      "aciklama": "Yönler elle yazıldı: bir kez çalışır ama iz değişince robot yanlış kola sapar. Her adımda Serçe'ye sor."
    },
    {
      "kod": ["def adim(yon):", "    move(yon)", "    while dust_here():", "        wait()", "    repair()", "for i in range(7):", "    adim(scout())"],
      "aciklama": "repair() rutinin içinde her adımda çalışır: panel olmayan karede robot durur. Paneli yalnızca bulunduğu yerde onar."
    },
    {
      "kod": ["def adim(yon):", "    move(yon)", "adim(scout())", "adim(scout())", "repair()", "for i in range(5):", "    adim(scout())"],
      "aciklama": "İlk adımda toz var: rutin beklemeden yürümeye çalışır."
    },
    {
      "kod": ["def adim():", "    move(scout())", "    while dust_here():", "        wait()", "adim()", "adim()", "repair()", "for i in range(5):", "    adim()"],
      "aciklama": "Robot izi yürür ama rutinin bilgi kutusu yok: Serçe'nin cevabını parametre olarak ver."
    }
  ],
  "cozum": [
    "def adim(yon):",
    "    move(yon)",
    "    while dust_here():",
    "        wait()",
    "adim(scout())",
    "adim(scout())",
    "repair()",
    "for i in range(5):",
    "    adim(scout())"
  ]
}
```

- [ ] **Step 5: `bolum-70.json` (İzin sonu)**

```json
{
  "numara": 70,
  "baslik": "İzin sonu",
  "gorev": "7 adım, iki rutin",
  "harita": [
    ". K . . . .",
    ". K . K . .",
    ". . Z K . K",
    "Z K . . Z .",
    ". . 3 . . .",
    "K . R . . K"
  ],
  "toz_suresi": [2, 3, 2],
  "izler": [
    ["North", "West", "West", "North", "North", "North", "North"],
    ["North", "East", "East", "North", "North", "North", "North"],
    ["North", "North", "North", "North", "North", "East", "East"]
  ],
  "komutlar": ["move", "scout", "dust_here", "wait", "panel_power", "repair"],
  "rutin_sayisi": 2,
  "parametre_sarti": true,
  "parcalar": [
    "def adim(yon):",
    "def kol(yon):",
    "move(yon)",
    "while dust_here():",
    "wait()",
    "adim(yon)",
    "kol(scout())",
    "repair()",
    "for i in range(2):"
  ],
  "konular": ["fonksiyon", "parametre", "fonksiyon içinde fonksiyon", "değişen yol"],
  "ipuclari": [
    "İzin başı hep aynı: bir adım kuzey, orada çatlak panel. Sonra 3 kol, her biri 2 kare; hangi yöne gittiğini yalnızca Serçe görür.",
    "Küçük rutin bir adım atıp tozu beklesin; büyük rutin bir kolu (iki adımı) yürüsün ve yönü küçüğe aktarsın.",
    "`def kol(yon):`, `    adim(yon)`, `    adim(yon)`. Başta `adim(North)`, `repair()`; sonra `for i in range(3):` altına `    kol(scout())`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def adim(yon):", "    move(yon)", "    while dust_here():", "        wait()", "def kol(yon):", "    adim(yon)", "    adim(yon)", "adim(North)", "repair()", "kol(West)", "kol(North)", "kol(North)"],
      "aciklama": "Yönler elle yazıldı: bir kez çalışır ama iz değişince robot yanlış kola sapar. Her kolda Serçe'ye sor."
    },
    {
      "kod": ["def adim(yon):", "    move(yon)", "    while dust_here():", "        wait()", "def kol(yon):", "    adim(yon)", "    adim(yon)", "adim(North)", "for i in range(3):", "    kol(scout())"],
      "aciklama": "Robot izin sonuna varır ama çatlak panel onarılmadı."
    },
    {
      "kod": ["def adim(yon):", "    move(yon)", "    while dust_here():", "        wait()", "adim(North)", "repair()", "for i in range(3):", "    yon = scout()", "    adim(yon)", "    adim(yon)"],
      "aciklama": "Robot izin sonuna varır ama bu bölüm iki rutin istiyor: bir kolu yürüme işini de rutine koy."
    }
  ],
  "cozum": [
    "def adim(yon):",
    "    move(yon)",
    "    while dust_here():",
    "        wait()",
    "def kol(yon):",
    "    adim(yon)",
    "    adim(yon)",
    "adim(North)",
    "repair()",
    "for i in range(3):",
    "    kol(scout())"
  ]
}
```


- [ ] **Step 6: `sinav-70.json`**

```json
{
  "bolum": 70,
  "baslik": "KIVILCIM · KENDİNİ DENETLİYOR",
  "sorular": [
    {
      "soru": "def git(yon, kac=1): tanımlıyken git(North) çağrılırsa kac kaçtır?",
      "secenekler": ["0", "Hata verir", "1", "North"],
      "dogru": 2,
      "aciklama": "kac=1 varsayılan değerdir: çağrıda sayı yazılmazsa 1 olur."
    },
    {
      "soru": "def git(yon): tanımlı. Kod: isaret = scout() ve sonra git(isaret). Çalışır mı?",
      "secenekler": ["Evet: değer kutuya girer, değişkenin adı önemli değil", "Hayır, değişkenin adı da yon olmalı", "NameError verir", "Yalnızca isaret East ise"],
      "dogru": 0,
      "aciklama": "Rutine değişkenin adı değil değeri gider. Rutinin içinde o değerin adı yon'dur."
    },
    {
      "soru": "Yol her denemede değişiyorsa move(North), move(East)... diye yönleri ezberlemek neden işe yaramaz?",
      "secenekler": ["move yavaş çalışır", "İşe yarar, yollar hep aynıdır", "move yön almaz", "Bir yolda doğru, öteki yolda yanlış olur; yönü her seferinde Serçe'ye sormak gerekir"],
      "dogru": 3,
      "aciklama": "Kod ne yazıyorsa onu yapar. Değişen dünyada doğru kod soru sorar ve cevaba göre davranır."
    }
  ]
}
```

- [ ] **Step 7: `bolum-cozumleri.md`** — sona `## Bölüm 61` … `## Bölüm 70` başlıklarıyla her bölümün `cozum` satırları (```python bloğu), var olan biçimde. Başlıktaki "oluşturuldu" tarihini değiştirme; altına "(61-70: 2026-11-02)" ekle.

- [ ] **Step 8: Denetim** — `cd motor-test && dotnet test` → hepsi geçer (sınav testi 70'i bekler).

---

### Görev 8: Son denetim

- [ ] **Step 1:** `cd motor-test && dotnet test` → hepsi geçer; sayıyı not et.
- [ ] **Step 2:** Windows paketi: `oyun/Build/` sil → `Unity.exe -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log` → `build.log`'da `error CS` yok. Yeni `.cs` dosyalarının `.meta`'ları oluştu mu bak (`Trail.cs.meta`, `TrailRuns.cs.meta`). İş bitince Unity süreçlerini kapat.
- [ ] **Step 3:** Görüntü: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -bolum 61 -shots <klasör>` → 61-70'in her biri "Tamamlandı" ile bitiyor; 62'de geçici turkuaz yön yazısı görünüyor; 68-70'te hedef görünmüyor. Görüntüleri `docs/tasarim/bolge-7-bulmacalar-2026-11-02/`'ye koy (yalnızca her bölümün bitiş karesi + 62'nin bir `scout` karesi).
- [ ] **Step 4:** Elle (Unity'de ya da pakette, `TestSolutionsFilled` açık): 68'de "Çalıştır"a iki kez bas → robot iki farklı kolu yürür. Çözümün ilk satırlarını `kol(East)`, `kol(West)`, `kol(East)` yap → ikinci izde takılır ve mesaj "Serçe bu sefer başka bir kol gördü." ile başlar. 61'de `git(East)`'leri `move(East)` yapıp `git`'i yalnızca `git(South)` ile çağır → "hep aynı bilgiyle" mesajı.
- [ ] **Step 5:** Yedi arayüz denetimi + klavye denetimi (`scripts/klavye-denetimi.ps1`) → TAMAM. Bölge 6'ya sızma yok: 51 ve 60'ın görüntüsü önceki gibi.
- [ ] **Step 6:** ILERLEME.md'ye (Ragıp/kim yaptıysa adıyla) özet; açık sorular: 70'in başlığı (karar 4), geçici görünüşler (karar 5). Oturum kapanışında tek commit + push (CLAUDE.md ekip döngüsü).
