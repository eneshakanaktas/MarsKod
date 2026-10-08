# Bölge 6 Bulmacaları (Bölüm 51-60) Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bölüm 51-60'ı (fonksiyonlar / "rutinler") oynanır hâle getirmek: rutin şartı oyun kuralı, drone parçası `D`, kanyon bölgesi, 10 bölüm dosyası, 2 sınav, sözlükte `def`.

**Architecture:** Rutin şartı saf C# dünya tarafında (`Dunya/RoutineRule.cs`); `ProgramRun` kod çalışırken her rutinin kaç kez çalıştığını motorun adım çerçevelerinden (`Step.Frame.Fn`) sayar, tanımlı rutinleri `Interpreter.Globals`'tan okur. `Level` şartı dosyadan okur ve `Level.Run` ile her yerde aynı şekilde uygular. Sahne (`Oyun.cs`) yalnızca sonucu mesaj olarak gösterir; drone parçası ve kanyonun görünüşü bu planda geçici olarak var olan görünüşlere bağlanır (asıl görünüş 3. parçada).

**Tech Stack:** Unity 6 (C# 9, .NET Standard 2.1), kendi mini-Python motoru (`oyun/Assets/Motor/`), xUnit (`motor-test/`, `dotnet test`).

**Spec:** `docs/superpowers/specs/2026-10-08-bolge-6-design.md` (bölüm "1. Bulmacalar").

## Global Constraints

- Motor ve dünya kodu saf C#: `UnityEngine` yok, C# 9, .NET Standard 2.1 (motor-test bunu derlerken yakalar).
- Kullanıcıya görünen bütün metinler sade Türkçe; kod parçaları ters tırnak içinde (`CodeColors.Inline` renklendirir).
- Haritalar 6×6. Bir bölümde tek tür toplanacak; kanyonda (51-60) toplanacak varsa o `D` (drone parçası).
- Rutin şartı: adlı rutin `def` ile **modül düzeyinde** tanımlı ve en az **2** kez çalışmış olmalı (`RoutineRule.MinRuns = 2`). `rutin_sayisi`: en az 2 kez çalışan en az N farklı rutin.
- Rutinin içinden dışarıdaki değişkeni değiştirmeye dayanan bölüm yok (sayaç/toplam ana kodda).
- Bölüm 51-60'ta `etiket`, `giris`, `bitis` yazılmaz (hikâye parçası 2'de gelir).
- Her `.cs` dosyası Unity'de yeni eklenirse yanında `.meta` oluşur; Unity paketi üretilince kendiliğinden oluşur, git'e onunla girer.
- **Commit kuralı (ekip döngüsü, CLAUDE.md):** görev başına commit atılmaz; oturum kapanışında ILERLEME.md ile birlikte tek commit + push.
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıf/fonksiyon, tekrar yok, okunur adlar, gereksiz katman yok.

## Review Focus

- Rutini **döngü içinden** çağırmak (`for i in range(3): sabah_turu()`): 3 çalışma sayılmalı → Task 1 testi.
- Rutini tanımlayıp **hiç çağırmamak** ve **tanımdan önce çağırmak** (NameError): ilkinde "hiç çalışmadı" mesajı, ikincisinde Python hatası → Task 1 testi + 52 tipik hataları.
- Görev yerine gelmiş ama rutin yok: ekranda dünyanın değil **rutinin** mesajı çıkmalı; dünya eksikse önce dünyanın mesajı → Task 3 (Oyun.cs dalı) + Task 6 görsel kontrol.
- Rutinin **içinde başka rutin** (57): iç rutinin çalışmaları da sayılmalı → Task 1 testi.
- Rutinin içinde `toplam +=` (59 tipik hatası): motor UnboundLocalError verir, görev tamam sayılmaz → 59 tipik hatası (`BolumTests` denetler).

---

### Task 1: Rutin şartı (dünya kuralı) + çalışma sayımı

**Files:**
- Create: `oyun/Assets/Dunya/RoutineRule.cs`
- Modify: `oyun/Assets/Dunya/ProgramRun.cs` (`RunReport` alanları, `Execute` imzası ve sayım)
- Test: `motor-test/RoutineTests.cs`

**Interfaces:**
- Produces:
  - `public sealed class RoutineProblem { public string Title; public string Text; }`
  - `public sealed class RoutineRule { public const int MinRuns = 2; public static readonly RoutineRule None; public IReadOnlyList<string> Names; public int MinCount; public RoutineRule(IEnumerable<string> names, int minCount); public bool IsEmpty; public RoutineProblem Check(ICollection<string> defined, IReadOnlyDictionary<string, int> runs); }`
  - `RunReport.RoutineRuns : Dictionary<string,int>`, `RunReport.DefinedRoutines : HashSet<string>`, `RunReport.RoutineProblem : RoutineProblem` (null = şart yok ya da sağlandı)
  - `ProgramRun.Execute(string source, World world, int maxSteps = DefaultMaxSteps, RoutineRule routines = null)`; `report.Complete = !Stopped && world.Complete && RoutineProblem == null`

- [ ] **Step 1: Failing test yaz** — `motor-test/RoutineTests.cs`:

```csharp
using System.Collections.Generic;
using MarsKod.Dunya;

namespace MotorTest;

// Rutin şartı (Bölge 6, Bölüm 51-60, fonksiyonlar): bölüm bir rutin istiyorsa oyuncu onu def ile yazmalı ve
// rutin en az iki kez çalışmalı; yoksa robot işi bitirse bile görev tamam sayılmaz.
public class RoutineTests
{
    // Robot (0,0); doğuda (1,0), (2,0), (3,0) toplanacak; görev üçünü toplamak.
    static World Row() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(2, 0), new Cell(3, 0) },
        null, null, new[] { "move", "collect" });

    static readonly RoutineRule Adim = new RoutineRule(new[] { "adim" }, 0);

    const string Routine = "def adim():\n    move(East)\n    collect()\n";

    [Fact]
    public void Uc_kez_cagrilan_rutin_gorevi_bitirir_ve_sayilir()
    {
        var report = ProgramRun.Execute(Routine + "adim()\nadim()\nadim()\n", Row(), routines: Adim);
        Assert.True(report.Complete);
        Assert.Null(report.RoutineProblem);
        Assert.Equal(3, report.RoutineRuns["adim"]);
        Assert.Contains("adim", report.DefinedRoutines);
    }

    [Fact]
    public void Donguden_cagri_her_turda_sayilir()
    {
        var report = ProgramRun.Execute(Routine + "for i in range(3):\n    adim()\n", Row(), routines: Adim);
        Assert.True(report.Complete);
        Assert.Equal(3, report.RoutineRuns["adim"]);
    }

    [Fact]
    public void Rutinsiz_kopyala_yapistir_gorevi_bitirmez()
    {
        var world = Row();
        var report = ProgramRun.Execute("move(East)\ncollect()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", world, routines: Adim);
        Assert.True(world.Complete);   // dünya tamam...
        Assert.False(report.Complete); // ...ama rutin yok
        Assert.Contains("adim rutini yok", report.RoutineProblem.Title);
        Assert.Contains("def adim():", report.RoutineProblem.Text);
    }

    [Fact]
    public void Tanimlanip_cagrilmayan_rutin_hic_calismadi_der()
    {
        var report = ProgramRun.Execute(Routine + "move(East)\ncollect()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", Row(), routines: Adim);
        Assert.False(report.Complete);
        Assert.Contains("hiç çalışmadı", report.RoutineProblem.Title);
    }

    [Fact]
    public void Bir_kez_calisan_rutin_yetmez()
    {
        var report = ProgramRun.Execute(Routine + "adim()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", Row(), routines: Adim);
        Assert.False(report.Complete);
        Assert.Contains("yalnızca 1 kez", report.RoutineProblem.Title);
    }

    [Fact]
    public void Rutin_icinde_rutin_ikisi_de_sayilir()
    {
        var both = new RoutineRule(new[] { "adim", "tur" }, 0);
        var report = ProgramRun.Execute(Routine + "def tur():\n    adim()\n    adim()\ntur()\nadim()\n", Row(), routines: both);
        Assert.Equal(3, report.RoutineRuns["adim"]);
        Assert.Equal(1, report.RoutineRuns["tur"]);
        Assert.Contains("tur", report.RoutineProblem.Title); // tur yalnızca 1 kez
    }

    [Fact]
    public void Serbest_adli_rutin_sayisi()
    {
        var one = new RoutineRule(new string[0], 1);
        Assert.True(ProgramRun.Execute("def git():\n    move(East)\n    collect()\ngit()\ngit()\ngit()\n", Row(), routines: one).Complete);
        var two = new RoutineRule(new string[0], 2);
        var report = ProgramRun.Execute("def git():\n    move(East)\n    collect()\ngit()\ngit()\ngit()\n", Row(), routines: two);
        Assert.False(report.Complete);
        Assert.Contains("2 rutin", report.RoutineProblem.Title);
    }

    [Fact]
    public void Tanimdan_once_cagri_python_hatasidir()
    {
        var report = ProgramRun.Execute("adim()\n" + Routine + "adim()\nadim()\n", Row(), routines: Adim);
        Assert.Equal("NameError", report.Error.Type);
        Assert.False(report.Complete);
        Assert.Null(report.RoutineProblem); // kod durduysa rutin mesajı yok, hata mesajı yeter
    }

    [Fact]
    public void Sart_yoksa_eskisi_gibi()
    {
        Assert.True(ProgramRun.Execute("move(East)\ncollect()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", Row()).Complete);
        Assert.True(RoutineRule.None.IsEmpty);
    }
}
```

> `World(cols, rows, robot, iceCells, rockCells = null, Cell? target = null, commands = null, ...)` (World.cs:196); kalan parametreler isteğe bağlı.

- [ ] **Step 2: Çalıştır, başarısız olduğunu gör**

Run: `cd motor-test && dotnet test --filter RoutineTests`
Expected: derleme hatası (`RoutineRule` yok).

- [ ] **Step 3: `RoutineRule.cs` yaz**

```csharp
// Rutin şartı (Bölge 6'dan itibaren, fonksiyonlar): bölüm, oyuncunun belli adımları def ile bir rutine koymasını
// isteyebilir. Şart yalnızca dünyaya bakmaz, koda bakar: rutin tanımlı mı, kaç kez çalıştı. Tek kez çalışan şey
// rutin sayılmaz (rutin tekrar kullanmak içindir). Bölüm dosyasında: "rutinler" (adlar), "rutin_sayisi" (adı serbest).

using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    /// <summary>Rutin şartı karşılanmadığında oyuncuya gösterilecek açıklama</summary>
    public sealed class RoutineProblem
    {
        public string Title;
        public string Text;
    }

    public sealed class RoutineRule
    {
        /// <summary>Bir rutinin sayılması için en az kaç kez çalışması gerekir</summary>
        public const int MinRuns = 2;

        public static readonly RoutineRule None = new RoutineRule(new string[0], 0);

        /// <summary>Adıyla istenen rutinler</summary>
        public readonly IReadOnlyList<string> Names;
        /// <summary>Adı serbest: en az kaç farklı rutin (adlı olanlar dahil)</summary>
        public readonly int MinCount;

        public RoutineRule(IEnumerable<string> names, int minCount)
        {
            Names = names.ToList();
            MinCount = minCount;
        }

        public bool IsEmpty => Names.Count == 0 && MinCount == 0;

        /// <summary>defined: oyuncunun def ile tanımladığı rutinler; runs: her rutinin kaç kez çalıştığı.
        /// Şart sağlandıysa null.</summary>
        public RoutineProblem Check(ICollection<string> defined, IReadOnlyDictionary<string, int> runs)
        {
            foreach (var name in Names)
            {
                int n = Runs(runs, name);
                if (!defined.Contains(name))
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " rutini yok",
                        Text = "Robot işi yaptı ama bu bölümde adımları bir rutine koyman isteniyor. `def " + name + "():` ile yaz, sonra `" + name + "()` diye çağır.",
                    };
                if (n == 0)
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " hiç çalışmadı",
                        Text = "Rutini tanımlamak onu çalıştırmaz. Tanımdan sonra `" + name + "()` yazarak çağır.",
                    };
                if (n < MinRuns)
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " yalnızca " + n + " kez çalıştı",
                        Text = "Rutin tekrar kullanmak içindir: aynı işi her gerektiğinde `" + name + "()` yazarak çağır.",
                    };
            }
            int used = defined.Count(name => Runs(runs, name) >= MinRuns);
            if (used < MinCount)
                return MinCount == 1
                    ? new RoutineProblem
                    {
                        Title = "Kod bitti, rutin yok",
                        Text = "Tekrar eden adımları `def` ile bir rutine koy, adını sen seç ve en az iki kez çağır.",
                    }
                    : new RoutineProblem
                    {
                        Title = "Kod bitti, " + MinCount + " rutin gerekiyor",
                        Text = "Bu bölümde en az " + MinCount + " ayrı rutin isteniyor; her biri en az iki kez çalışmalı. Tekrar eden adım gruplarını bul, her birine bir ad ver.",
                    };
            return null;
        }

        static int Runs(IReadOnlyDictionary<string, int> runs, string name) => runs.TryGetValue(name, out var n) ? n : 0;
    }
}
```

- [ ] **Step 4: `ProgramRun.cs`'i değiştir**

`RunReport`'a (Complete'in üstüne):

```csharp
        /// <summary>Oyuncunun def ile yazdığı (modül düzeyindeki) rutinler</summary>
        public readonly HashSet<string> DefinedRoutines = new HashSet<string>();
        /// <summary>Her rutinin kaç kez çalıştığı (döngüden ya da başka rutinden çağrılar dahil)</summary>
        public readonly Dictionary<string, int> RoutineRuns = new Dictionary<string, int>();
        /// <summary>Bölümün rutin şartı karşılanmadıysa açıklaması; şart yoksa, sağlandıysa ya da kod durduysa null</summary>
        public RoutineProblem RoutineProblem;
```

`Execute`'u şöyle değiştir (imza + sayım + sonuç):

```csharp
        public static RunReport Execute(string source, World world, int maxSteps = DefaultMaxSteps, RoutineRule routines = null)
        {
            var report = new RunReport();
            Interpreter interpreter = null;
            TraceEntry current = null;
            Frame frame = null; // current satirinin calistigi cerceve (degiskenleri satir bitince okunur)
            var calls = new HashSet<Frame>(); // her rutin cagrisi yeni bir cerceve: ilk adiminda bir kez sayilir
            world.Events.Clear();
            try
            {
                var module = Parser.Parse(source);
                interpreter = new Interpreter(new InterpreterOptions { MaxSteps = maxSteps, Externals = world.Commands() });
                foreach (var step in interpreter.Run(module))
                {
                    Flush(world, current, frame);
                    current = new TraceEntry { Line = step.Line };
                    frame = step.Frame;
                    CountCall(report, calls, step.Frame);
                    report.Trace.Add(current);
                }
            }
            // ... iki catch bloğu değişmeden kalır ...
            Flush(world, current, frame);
            report.Output = interpreter?.Output ?? "";
            if (interpreter != null) ReadDefinedRoutines(report, interpreter.Globals);
            if (!report.Stopped && routines != null && !routines.IsEmpty)
                report.RoutineProblem = routines.Check(report.DefinedRoutines, report.RoutineRuns);
            report.Complete = !report.Stopped && world.Complete && report.RoutineProblem == null;
            return report;
        }

        // Rutin cagrisinin ilk adimi: cerceve yeniyse o rutinin calisma sayisini bir artirir
        static void CountCall(RunReport report, HashSet<Frame> calls, Frame frame)
        {
            if (frame?.Fn == null || !calls.Add(frame)) return;
            string name = frame.Fn.Def.Name;
            report.RoutineRuns[name] = report.RoutineRuns.TryGetValue(name, out var n) ? n + 1 : 1;
        }

        // Kod bitince modul duzeyinde fonksiyon olarak duran adlar = oyuncunun def ile yazdigi rutinler
        static void ReadDefinedRoutines(RunReport report, Frame globals)
        {
            foreach (var pair in globals.Vars)
                if (pair.Value is PyFunction) report.DefinedRoutines.Add(pair.Key);
        }
```

> `Interpreter.Globals` `public readonly Frame Globals` (Interpreter.cs:83); `Frame.Fn` modül çerçevesinde null. `PyFunction.Def` bir `FunctionDef` (`Name` alanı).

- [ ] **Step 5: Testleri çalıştır**

Run: `cd motor-test && dotnet test`
Expected: RoutineTests dahil hepsi geçer (önceki sayı 1355 + 9).

---

### Task 2: Bölüm dosyasında `rutinler` / `rutin_sayisi` + `Level.Run`

**Files:**
- Modify: `oyun/Assets/Dunya/Level.cs` (alan, `Parse`, yeni `ReadRoutines`, `Run`, `LevelCheck`)
- Modify: `motor-test/BolumTests.cs:36` (tipik hatalar `level.Run` ile)
- Modify: `docs/tasarim/bolum-dosyasi.md` (alan tablosu)
- Test: `motor-test/BolumTests.cs` (yeni testler)

**Interfaces:**
- Consumes: `RoutineRule`, `ProgramRun.Execute(..., routines:)` (Task 1)
- Produces: `Level.Routines : RoutineRule` (varsayılan `RoutineRule.None`), `Level.Run(string code) : RunReport` (her çalıştırmada yeni dünya + bölümün rutin şartı)

- [ ] **Step 1: Failing test yaz** — `BolumTests.cs` sonuna (sınıfın içindeki `Mini` sabitini kullanır; `Mini`'nin içeriğine bakıp `"cozum"` satırını ona göre değiştir):

```csharp
    // Mini bölüme rutin şartı eklenmiş hâli: cozum rutinsiz olduğu için denetleyici yakalamalı
    [Fact]
    public void Rutin_sarti_okunur_ve_denetlenir()
    {
        var level = Level.Parse(Mini.Replace("\"komutlar\"", "\"rutinler\": [\"git\"], \"komutlar\""));
        Assert.Equal(new[] { "git" }, level.Routines.Names);
        Assert.Contains(LevelCheck.Problems(level), p => p.Contains("görevi bitirmedi"));
    }

    [Fact]
    public void Rutin_sayisi_okunur()
    {
        var level = Level.Parse(Mini.Replace("\"komutlar\"", "\"rutin_sayisi\": 2, \"komutlar\""));
        Assert.Equal(2, level.Routines.MinCount);
        Assert.True(Level.Parse(Mini).Routines.IsEmpty);
    }

    [Theory]
    [InlineData("\"rutinler\": [\"sabah turu\"], ", "geçerli bir rutin adı değil")]
    [InlineData("\"rutinler\": [\"move\"], ", "oyun komutunun adı")]
    [InlineData("\"rutinler\": [], ", "boş olamaz")]
    [InlineData("\"rutin_sayisi\": 0, ", "en az 1")]
    public void Bozuk_rutin_alani_anlasilir_mesaj_verir(string field, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace("\"komutlar\"", field + "\"komutlar\"")));
        Assert.Contains(expected, e.Message);
    }
```

- [ ] **Step 2: Çalıştır, başarısız olduğunu gör**

Run: `cd motor-test && dotnet test --filter BolumTests`
Expected: derleme hatası (`Level.Routines` yok).

- [ ] **Step 3: `Level.cs`'i değiştir**

Alanlar (`ExpectedReport` alanının altına):

```csharp
        /// <summary>Rutin şartı ("rutinler", "rutin_sayisi"); yazılmadıysa şart yok</summary>
        public RoutineRule Routines = RoutineRule.None;
```

`CreateWorld`'ün altına:

```csharp
        /// <summary>Kodu bu bölümün yeni bir dünyasında, bölümün rutin şartıyla çalıştırır (denetleyici ve testler için)</summary>
        public RunReport Run(string code) => ProgramRun.Execute(code, CreateWorld(), routines: Routines);
```

`Parse` içinde `known` kümesine `"rutinler", "rutin_sayisi"` ekle; `ReadReport(level, d)` çağrısının hemen altına `ReadRoutines(level, d);` ekle. Yeni yardımcı (`ReadReport`'un altına):

```csharp
        static readonly Regex RoutineName = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

        /// <summary>"rutinler": def ile yazılması istenen rutin adları; "rutin_sayisi": adı serbest en az kaç rutin.</summary>
        static void ReadRoutines(Level level, Dictionary<string, object> d)
        {
            var names = d.ContainsKey("rutinler") ? NonEmptyList(d, "rutinler") : new List<string>();
            foreach (var name in names)
            {
                if (!RoutineName.IsMatch(name) || Suggestions.IsPythonWord(name))
                    throw new DataFormatError("\"rutinler\" içindeki \"" + name + "\" geçerli bir rutin adı değil: harf ya da _ ile başlamalı, boşluk içermemeli, Python kelimesi olmamalı (örn. \"sabah_turu\").");
                if (Array.IndexOf(World.AllCommands, name) >= 0)
                    throw new DataFormatError("\"rutinler\" içindeki \"" + name + "\" bir oyun komutunun adı; rutine başka bir ad ver.");
            }
            int count = 0;
            if (d.ContainsKey("rutin_sayisi"))
            {
                count = Int(d, "rutin_sayisi");
                if (count < 1) throw new DataFormatError("\"rutin_sayisi\" en az 1 olmalı (rutin istemiyorsan alanı hiç yazma).");
            }
            level.Routines = new RoutineRule(names, count);
        }
```

> `NonEmptyList` zaten var ("boş olamaz" mesajıyla). `Regex` için `using System.Text.RegularExpressions;` dosyada var (`LevelCheck.Identifier`); `Array` için `using System;` var. Yoksa ekle.

`LevelCheck.Problems` içindeki üç `ProgramRun.Execute(X, level.CreateWorld())` çağrısını `level.Run(X)` yap (doğru çözüm, başlangıç kodu, tipik hatalar).

`BolumTests.cs:36`: `var report = ProgramRun.Execute(m.Code, level.CreateWorld());` → `var report = level.Run(m.Code);`

- [ ] **Step 4: Testleri çalıştır**

Run: `cd motor-test && dotnet test`
Expected: hepsi geçer.

- [ ] **Step 5: Kılavuzu güncelle** — `docs/tasarim/bolum-dosyasi.md` alan tablosunda `rapor` satırının altına:

```markdown
| `rutinler` | İsteğe bağlı (Bölge 6'dan itibaren, fonksiyonlar). Oyuncunun `def` ile yazması gereken rutin adları, örn. `["sabah_turu"]`. Her biri tanımlı olmalı ve kod çalışırken **en az 2 kez** çalışmalı; yoksa robot işi bitirse de görev bitmez ("Kod bitti, sabah_turu rutini yok"). Ad: harf ya da `_` ile başlar, boşluk yok, Python kelimesi ya da oyun komutu değil. Görev yazısında adı söyle. |
| `rutin_sayisi` | İsteğe bağlı. Adı oyuncuya bırakılmış rutin şartı: en az 2 kez çalışan en az bu kadar farklı rutin olmalı (örn. `1`, `2`). `rutinler` ile birlikte yazılabilir. |
```

"Görev:" paragrafına ekle: `…, (istendiyse) rutin şartı sağlanmış **ve** …`. Harita tablosuna `| `D` | Drone parçası (toplanacak; Bölge 6: Bölüm 51-60) |` satırı (Task 3 ile birlikte).

---

### Task 3: Drone parçası `D`, `Region.Canyon`, sahnede geçici görünüş + rutin mesajı

**Files:**
- Modify: `oyun/Assets/Dunya/Collectible.cs`, `oyun/Assets/Dunya/Region.cs`
- Modify: `oyun/Assets/Scripts/Pickup.cs:17-24`, `oyun/Assets/Scripts/Hud.cs:416`, `oyun/Assets/Scripts/RegionLook.cs:76-77`, `oyun/Assets/Scripts/Obstacles.cs:14-15`, `oyun/Assets/Scripts/Oyun.cs:739` ve `:807-826`
- Test: `motor-test/BolumTests.cs` (`Her_on_bolum_yeni_bolge`)

**Interfaces:**
- Consumes: `RunReport.RoutineProblem`, `Level.Routines` (Task 1-2)
- Produces: `Collectible.DronePart` (harita işareti `'D'`, ad "drone parçası" / "drone parçaları"), `Region.Canyon` (Bölüm 51-60), `Regions.Item(Region.Canyon) == Collectible.DronePart`

- [ ] **Step 1: Failing test** — `Her_on_bolum_yeni_bolge` testinin sonuna:

```csharp
        Assert.Equal(Region.AntennaHill, Regions.Of(50));
        Assert.Equal(Region.Canyon, Regions.Of(51));
        Assert.Equal(Region.Canyon, Regions.Of(60));
        Assert.Equal(Collectible.DronePart, Regions.Item(Region.Canyon));
        Assert.Equal(Collectible.DronePart, Collectibles.Marks['D']);
        Assert.Equal("drone parçası", Collectibles.Name(Collectible.DronePart));
```

Run: `cd motor-test && dotnet test --filter Her_on_bolum_yeni_bolge` → derleme hatası.

- [ ] **Step 2: Dünya tarafı**

`Collectible.cs`: enum'a `DronePart`; `Marks`'a `['D'] = Collectible.DronePart,`; `Names`'e `[Collectible.DronePart] = ("drone parçası", "drone parçaları"),`.

`Region.cs`: enum `{ Plain, PolarIce, CraterField, Dunes, AntennaHill, Canyon }`; `Order`'ın sonuna `Region.Canyon`; `Item`'a `case Region.Canyon: return Collectible.DronePart;`; özet yorumuna "kanyon girişi: drone parçası" ekle.

Run: `cd motor-test && dotnet test` → hepsi geçer.

- [ ] **Step 3: Sahne — geçici görünüş (asıl görünüş Bölge 6 3. parçada)**

`Pickup.cs` switch'ine:

```csharp
            case Collectible.DronePart: return CompassPart.Create(parent, localPos, seed); // gecici: drone parcasinin kendi gorunusu Bolge 6 gorunus isinde
```

`Hud.cs:416` satırını:

```csharp
                : item == Collectible.CompassPart || item == Collectible.DronePart ? new Icon(36, (p, r) => DrawCompassDot(p, r, idx < collected)) // drone: gecici
```

`RegionLook.cs` `For`: `case Region.AntennaHill:` altına `case Region.Canyon:` (yorum: "anten tepesi ve kanyonun kendi gorunusu gelene kadar kum tepeleri"). `Obstacles.cs`: aynı şekilde `case Region.AntennaHill:` altına `case Region.Canyon:`.

- [ ] **Step 4: Sahne — rutin şartı**

`Oyun.cs:739`: `var report = ProgramRun.Execute(code, world, routines: level.Routines);`

`Oyun.cs` "görev bitmedi" dalının başına (şu anki `if (world.IceLeft > 0)` zincirinin önüne; zincir `else if` ile devam eder):

```csharp
            if (world.Complete && report.RoutineProblem != null) // robot isi yapti ama bolumun istedigi rutin yok
                hud.ShowMessage("GÖREV", report.RoutineProblem.Title, report.RoutineProblem.Text, null, error: false);
            else if (world.IceLeft > 0)
```

- [ ] **Step 5: Derleme denetimi**

Run: `cd motor-test && dotnet test` (dünya tarafı). Unity derlemesi Task 6'da paketle denetlenir.

---

### Task 4: Bölüm 51-55 + `sinav-55` + sözlükte `def`

**Files:**
- Create: `oyun/Assets/Resources/Bolumler/bolum-51.json` … `bolum-55.json`
- Create: `oyun/Assets/Resources/Sinavlar/sinav-55.json`
- Modify: `oyun/Assets/Resources/Sozluk/sozluk.json` (sonuna `def` sayfası)

**Interfaces:**
- Consumes: `rutinler`, `D`, kanyon (Task 1-3). Var olan komutlar: `move`, `collect`, `dust_here`, `wait`, `panel_power`, `repair`, `rock_ahead`, `report`.

Haritalarda en üst satır kuzey (row 5), sol batı (col 0). `.meta` dosyaları Unity'de oluşur. Her bölümde **en az 3 ipucu** (1. yön, 2. konu, 3. kod parçası) ve en az 1 tipik hata.

- [ ] **Step 1: `bolum-51.json` (Kanyon kapısı, rutin yok)** — aynı 4 satırlık "toz kapısı" deseni araya farklı adımlar girerek 3 kez; döngüyle sarılamaz.

```json
{
  "numara": 51,
  "baslik": "Kanyon kapısı",
  "gorev": "Hedefe var",
  "harita": [
    ". . . . . H",
    ". . . K . .",
    ". . . . Z K",
    ". . K . K .",
    "K . . Z . .",
    "R Z K . . ."
  ],
  "toz_suresi": [2, 3, 2],
  "komutlar": ["move", "dust_here", "wait"],
  "konular": ["tekrar", "while", "komut sırası"],
  "ipuclari": [
    "Yolda üç toz kapısı var. Her kapıda aynı şeyi yaparsın: doğuya gir, toz geçene kadar bekle, kuzeye çık.",
    "Toz ne kadar sürer bilemezsin: `while dust_here():` altında `wait()`. Kapıların arasındaki adımlar her seferinde farklı.",
    "İlk kapı: `move(East)`, `while dust_here():`, `    wait()`, `move(North)`. Sonra `move(East)` ve ikinci kapı..."
  ],
  "tipik_hatalar": [
    {
      "kod": ["for i in range(3):", "    move(East)", "    while dust_here():", "        wait()", "    move(North)"],
      "aciklama": "Kapıların arasındaki adımlar farklı olduğu için aynı döngü üç kez dönemez: ikinci turda robot kayaya çarpar."
    },
    {
      "kod": ["move(East)", "while dust_here():", "    wait()", "move(North)", "move(East)", "move(East)", "move(North)"],
      "aciklama": "İkinci kapıda beklemeden kuzeye çıkmak olmaz: tozda yol görünmez."
    }
  ],
  "cozum": [
    "move(East)",
    "while dust_here():",
    "    wait()",
    "move(North)",
    "move(East)",
    "move(East)",
    "while dust_here():",
    "    wait()",
    "move(North)",
    "move(North)",
    "move(East)",
    "while dust_here():",
    "    wait()",
    "move(North)",
    "move(East)",
    "move(North)"
  ]
}
```

- [ ] **Step 2: `bolum-52.json` (Sabah turu, ilk `def`)**

```json
{
  "numara": 52,
  "baslik": "Sabah turu",
  "gorev": "sabah_turu rutinini yaz, 3 kez çağır",
  "harita": [
    ". . . . . .",
    "K . . . . .",
    ". . . D . .",
    ". . D . . .",
    ". D . . . .",
    "R . . . . K"
  ],
  "komutlar": ["move", "collect"],
  "rutinler": ["sabah_turu"],
  "python_kelimeleri": ["def"],
  "parcalar": ["def sabah_turu():", "sabah_turu()", "move(East)", "move(North)", "collect()"],
  "konular": ["fonksiyon", "def", "fonksiyon çağırma", "girinti"],
  "ipuclari": [
    "Her parça bir öncekinin bir sağında ve bir üstünde: aynı üç adım üç kez. Bu üç adımı bir kez öğret, sonra adıyla çağır.",
    "`def sabah_turu():` bir rutin tanımlar; altındaki girintili satırlar rutinin adımlarıdır. Tanımlamak çalıştırmaz: `sabah_turu()` yazınca çalışır.",
    "`def sabah_turu():` altına `    move(East)`, `    move(North)`, `    collect()`; sonra üç kez `sabah_turu()`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["sabah_turu()", "def sabah_turu():", "    move(East)", "    move(North)", "    collect()", "sabah_turu()", "sabah_turu()"],
      "aciklama": "Rutin tanımlanmadan çağrılamaz: Python o adı henüz bilmiyor (NameError). Önce def, sonra çağrı."
    },
    {
      "kod": ["def sabah_turu():", "    move(East)", "    move(North)", "    collect()"],
      "aciklama": "Rutin tanımlandı ama hiç çağrılmadı: robot yerinden kıpırdamaz."
    },
    {
      "kod": ["move(East)", "move(North)", "collect()", "move(East)", "move(North)", "collect()", "move(East)", "move(North)", "collect()"],
      "aciklama": "Robot parçaları toplar ama bu bölüm rutini istiyor: aynı satırları kopyalamak yerine sabah_turu yaz."
    }
  ],
  "baslangic_kodu": ["# sabah turu - D.A.", "def sabah_turu():", "    move(East)"],
  "cozum": [
    "def sabah_turu():",
    "    move(East)",
    "    move(North)",
    "    collect()",
    "sabah_turu()",
    "sabah_turu()",
    "sabah_turu()"
  ]
}
```

- [ ] **Step 3: `bolum-53.json` (Depo, rutin ≠ döngü)** — raflar (kaya) arasında üç parça; rafa uzan-topla-geri çekil rutini, aradaki yürüyüş 1, 2, 1 kare.

```json
{
  "numara": 53,
  "baslik": "Depo",
  "gorev": "raftan_al ile 3 parça al",
  "harita": [
    ". . . . . .",
    ". . . . . .",
    ". . . . . .",
    "K D K D D K",
    "R . . . . .",
    ". . . . . ."
  ],
  "komutlar": ["move", "collect"],
  "rutinler": ["raftan_al"],
  "parcalar": ["def raftan_al():", "raftan_al()", "move(East)", "move(North)", "move(South)", "collect()"],
  "konular": ["fonksiyon", "fonksiyon mu döngü mü"],
  "ipuclari": [
    "Raftan parça almak hep aynı: kuzeye uzan, topla, güneye geri çekil. Ama raflar arası yürüyüş her seferinde farklı.",
    "Döngü aynı şeyi arka arkaya yapar; rutin ise gereken her yerde çağrılır, araya başka adımlar girebilir.",
    "`def raftan_al():` altına `    move(North)`, `    collect()`, `    move(South)`. Sonra: `move(East)`, `raftan_al()`, `move(East)`, `move(East)`, `raftan_al()`..."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def raftan_al():", "    move(North)", "    collect()", "    move(South)", "for i in range(3):", "    move(East)", "    raftan_al()"],
      "aciklama": "Raflar arası yürüyüş eşit değil: ikinci turda robot rafın kayasına uzanır."
    },
    {
      "kod": ["def raftan_al():", "    move(North)", "    collect()", "move(East)", "raftan_al()", "move(East)", "move(East)", "raftan_al()", "move(East)", "raftan_al()"],
      "aciklama": "Rutin geri çekilmiyor: robot raf sırasında kalır ve yandaki kayaya çarpar."
    },
    {
      "kod": ["move(East)", "move(North)", "collect()", "move(South)", "move(East)", "move(East)", "move(North)", "collect()", "move(South)", "move(East)", "move(North)", "collect()", "move(South)"],
      "aciklama": "Parçalar toplanır ama raftan_al rutini yok: tekrar eden üç adımı rutine koy."
    }
  ],
  "cozum": [
    "def raftan_al():",
    "    move(North)",
    "    collect()",
    "    move(South)",
    "move(East)",
    "raftan_al()",
    "move(East)",
    "move(East)",
    "raftan_al()",
    "move(East)",
    "raftan_al()"
  ]
}
```

- [ ] **Step 4: `bolum-54.json` (Serçe, içinde `while` olan rutin)** — `toz_gec` = kuzeye bir adım + toz varsa bekle; tozsuz karede de güvenle çalışır.

```json
{
  "numara": 54,
  "baslik": "Serçe",
  "gorev": "toz_gec ile 3 parça topla",
  "harita": [
    ". . . Z D .",
    ". . D . K .",
    ". K Z K . .",
    "D . . . . .",
    "Z K . . . .",
    "R K . . . ."
  ],
  "toz_suresi": [2, 4, 3],
  "komutlar": ["move", "collect", "dust_here", "wait"],
  "rutinler": ["toz_gec"],
  "parcalar": ["def toz_gec():", "toz_gec()", "move(North)", "move(East)", "while dust_here():", "wait()", "collect()"],
  "konular": ["fonksiyon", "rutin içinde while"],
  "ipuclari": [
    "Kuzeye her adımda toz olabilir de olmayabilir de. Bir adım + 'toz varsa bekle' rutini her kuzey adımında güvenle çalışır.",
    "Rutinin içine `while` da yazılabilir; rutin çağrılınca içindeki döngü de çalışır. Toz yoksa `while` hiç dönmez.",
    "`def toz_gec():` altına `    move(North)`, `    while dust_here():`, `        wait()`. Sonra `toz_gec()`, `toz_gec()`, `collect()`..."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def toz_gec():", "    move(North)", "    wait()", "toz_gec()", "toz_gec()", "collect()", "move(East)", "move(East)", "toz_gec()", "toz_gec()", "collect()", "move(East)", "toz_gec()", "move(East)", "collect()"],
      "aciklama": "Tek wait() yetmez: toz birkaç tur sürer; tozsuz karede de wait() robotu durdurur. Sorarak bekle: while dust_here()."
    },
    {
      "kod": ["move(North)", "while dust_here():", "    wait()", "move(North)", "collect()", "move(East)", "move(East)", "move(North)", "while dust_here():", "    wait()", "move(North)", "collect()", "move(East)", "move(North)", "while dust_here():", "    wait()", "move(East)", "collect()"],
      "aciklama": "Parçalar toplanır ama toz_gec rutini yok: tekrar eden adımları rutine koy."
    }
  ],
  "cozum": [
    "def toz_gec():",
    "    move(North)",
    "    while dust_here():",
    "        wait()",
    "toz_gec()",
    "toz_gec()",
    "collect()",
    "move(East)",
    "move(East)",
    "toz_gec()",
    "toz_gec()",
    "collect()",
    "move(East)",
    "toz_gec()",
    "move(East)",
    "collect()"
  ]
}
```

- [ ] **Step 5: `bolum-55.json` (İlk uçuş, içinde `if` olan rutin)** — panellerin hepsi yolda; çatlakları onar, drone pistine (H) var.

```json
{
  "numara": 55,
  "baslik": "İlk uçuş",
  "gorev": "panel_bak ile onar, piste var",
  "harita": [
    ". . . . . .",
    ". . . . . .",
    ". . . . . .",
    ". . . 4 6 H",
    ". . . 7 . .",
    "R 3 8 2 . ."
  ],
  "komutlar": ["move", "panel_power", "repair"],
  "rutinler": ["panel_bak"],
  "parcalar": ["def panel_bak():", "panel_bak()", "move(East)", "move(North)", "if panel_power() < 50:", "repair()", "for i in range(3):"],
  "konular": ["fonksiyon", "rutin içinde if"],
  "ipuclari": [
    "Yoldaki her panelde aynı soru: zayıf mı? Zayıfsa onar, sağlamsa dokunma. Bu soruyu bir rutine koy.",
    "Rutinin içine `if` yazılabilir. Rutin her çağrıldığında robotun o an durduğu panele bakar.",
    "`def panel_bak():` altına `    if panel_power() < 50:`, `        repair()`. Her adımdan sonra `panel_bak()`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def panel_bak():", "    repair()", "move(East)", "panel_bak()", "move(East)", "panel_bak()"],
      "aciklama": "Sağlam panel onarılmaz: önce panel_power() ile ölç, yalnızca zayıfsa repair()."
    },
    {
      "kod": ["move(East)", "repair()", "move(East)", "move(East)", "repair()", "move(North)", "move(North)", "repair()", "move(East)", "move(East)"],
      "aciklama": "Paneller onarılır ama panel_bak rutini yok: bu bölüm ölç-onar işini bir rutine koymanı istiyor."
    }
  ],
  "cozum": [
    "def panel_bak():",
    "    if panel_power() < 50:",
    "        repair()",
    "move(East)",
    "panel_bak()",
    "move(East)",
    "panel_bak()",
    "move(East)",
    "panel_bak()",
    "move(North)",
    "panel_bak()",
    "move(North)",
    "panel_bak()",
    "move(East)",
    "panel_bak()",
    "move(East)"
  ]
}
```

- [ ] **Step 6: `sinav-55.json`**

```json
{
  "bolum": 55,
  "sorular": [
    {
      "soru": "def sabah_turu(): ve altındaki girintili satırlar çalışınca robot ne yapar?",
      "secenekler": ["Hemen yürür", "Hiçbir şey yapmaz; rutin yalnızca öğrenilir", "Hata verir", "Rutini iki kez çalıştırır"],
      "dogru": 1,
      "aciklama": "def rutini tanımlar, çalıştırmaz. Çalışması için adıyla çağrılır: sabah_turu()."
    },
    {
      "soru": "Rutini tanımından önce çağırırsan ne olur?",
      "secenekler": ["Normal çalışır", "NameError: Python o adı henüz bilmiyor", "Rutin iki kez çalışır", "Robot durur ama hata yok"],
      "dogru": 1,
      "aciklama": "Kod yukarıdan aşağı çalışır; def satırına gelmeden ad tanımlı değildir. Önce def, sonra çağrı."
    },
    {
      "soru": "Rutinin adımları nasıl yazılır?",
      "secenekler": ["def satırıyla aynı hizada", "def satırından 4 boşluk içeride", "Hepsi tek satırda", "Tırnak içinde"],
      "dogru": 1,
      "aciklama": "Rutinin adımları, döngüdeki gibi girintiyle (4 boşluk) yazılır; girinti bitince rutin de biter."
    }
  ]
}
```

- [ ] **Step 7: Sözlükte `def` sayfası** — `sozluk.json` `"sayfalar"` listesinin sonuna (son öğeden sonra virgül):

```json
    {
      "baslik": "def",
      "kelimeler": ["def"],
      "aciklama": "Rutin (fonksiyon) tanımlar: birkaç adımı bir ada bağlarsın, sonra o adı yazdıkça adımlar çalışır. Tanımlamak çalıştırmaz, çağırmak çalıştırır. Adımlar 4 boşluk içeride yazılır; rutin, çağrılmadan önce tanımlanmalı.",
      "ornek": [
        "def sabah_turu():",
        "    move(East)",
        "    collect()",
        "sabah_turu()   # bir kez",
        "sabah_turu()   # bir kez daha"
      ]
    }
```

- [ ] **Step 8: Denetim**

Run: `cd motor-test && dotnet test`
Expected: hepsi geçer. `Her_bes_bolumde_bir_sinav_var` 60'ı bekleyebilir (en büyük bölüm 55 iken geçmeli). Bir bölüm düşerse mesajdaki satıra göre haritayı/çözümü düzelt (mesaj Türkçe, hangi satırda durduğunu söyler); tasarımın öğrettiği şey (tablodaki "Öğrettiği") değişmesin.

---

### Task 5: Bölüm 56-60 + `sinav-60`

**Files:**
- Create: `oyun/Assets/Resources/Bolumler/bolum-56.json` … `bolum-60.json`
- Create: `oyun/Assets/Resources/Sinavlar/sinav-60.json`

- [ ] **Step 1: `bolum-56.json` (Görev çizelgesi, döngüden rutin)** — `koridor` = kayaya/sınıra kadar doğuya; her koridor farklı uzunlukta.

```json
{
  "numara": 56,
  "baslik": "Görev çizelgesi",
  "gorev": "koridor rutinini döngüde kullan",
  "harita": [
    ". . . . . .",
    "K . . . . .",
    ". . . . . H",
    ". . . . . D",
    ". . . D K .",
    "R D K . . ."
  ],
  "komutlar": ["move", "collect", "rock_ahead"],
  "rutinler": ["koridor"],
  "parcalar": ["def koridor():", "koridor()", "while not rock_ahead(East):", "move(East)", "move(North)", "collect()", "for i in range(2):"],
  "konular": ["fonksiyon", "döngüden fonksiyon çağırma", "while"],
  "ipuclari": [
    "Üç koridor var, uzunlukları farklı. Her birinde: duvara kadar doğuya git, parçayı al, bir kat yukarı çık.",
    "Uzunluğu bilmediğin yolu `while not rock_ahead(East):` yürür. Bunu bir rutine koyarsan döngünün içinden her turda çağırabilirsin.",
    "`def koridor():` altına `    while not rock_ahead(East):`, `        move(East)`. Sonra `for i in range(3):` içinde `koridor()`, `collect()`, `move(North)`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def koridor():", "    if not rock_ahead(East):", "        move(East)", "for i in range(3):", "    koridor()", "    collect()", "    move(North)"],
      "aciklama": "if yalnızca bir kez bakar: robot koridorun sonuna varmadan toplar ve parçalar kalır. Duvara kadar gitmek için while."
    },
    {
      "kod": ["for i in range(3):", "    while not rock_ahead(East):", "        move(East)", "    collect()", "    move(North)"],
      "aciklama": "Robot işi yapar ama koridor rutini yok: koridoru yürüme işini rutine koy."
    }
  ],
  "cozum": [
    "def koridor():",
    "    while not rock_ahead(East):",
    "        move(East)",
    "for i in range(3):",
    "    koridor()",
    "    collect()",
    "    move(North)"
  ]
}
```

- [ ] **Step 2: `bolum-57.json` (Rutin içinde rutin)**

```json
{
  "numara": 57,
  "baslik": "Rutin içinde rutin",
  "gorev": "adim ve tur rutinlerini yaz",
  "harita": [
    ". . . . . .",
    ". . . . . .",
    ". . . . K H",
    ". . K K . Z",
    "K K . Z D .",
    "R Z D . . ."
  ],
  "toz_suresi": [2, 3, 2],
  "komutlar": ["move", "collect", "dust_here", "wait"],
  "rutinler": ["adim", "tur"],
  "parcalar": ["def adim():", "def tur():", "adim()", "tur()", "move(East)", "move(North)", "while dust_here():", "wait()", "collect()"],
  "konular": ["fonksiyon", "fonksiyon içinde fonksiyon çağırma"],
  "ipuclari": [
    "Küçük iş: doğuya bir adım, toz varsa bekle (adim). Büyük iş: iki adım, topla, kuzeye çık (tur). Büyük iş küçüğü kullanır.",
    "Bir rutin başka bir rutini çağırabilir. Önce ikisini de tanımla, sonra çağır.",
    "`def tur():` altına `    adim()`, `    adim()`, `    collect()`, `    move(North)`. Sonunda: `tur()`, `tur()`, `adim()`, `move(North)`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def tur():", "    adim()", "    adim()", "    collect()", "    move(North)", "tur()", "tur()", "def adim():", "    move(East)", "    while dust_here():", "        wait()", "adim()", "move(North)"],
      "aciklama": "tur() çalışınca adim() henüz tanımlı değil (NameError). Hepsini başta tanımla, sonra çağır."
    },
    {
      "kod": ["def tur():", "    move(East)", "    while dust_here():", "        wait()", "    move(East)", "    collect()", "    move(North)", "tur()", "tur()", "move(East)", "while dust_here():", "    wait()", "move(North)"],
      "aciklama": "Robot hedefe varır ama adim rutini yok: tur'un içindeki tekrar eden adımı da rutine koy."
    }
  ],
  "cozum": [
    "def adim():",
    "    move(East)",
    "    while dust_here():",
    "        wait()",
    "def tur():",
    "    adim()",
    "    adim()",
    "    collect()",
    "    move(North)",
    "tur()",
    "tur()",
    "adim()",
    "move(North)"
  ]
}
```

- [ ] **Step 3: `bolum-58.json` (Boya oklar, adı serbest rutin)** — her karede aynı soru: parça mı (güç 0 → topla), çatlak mı (onar), sağlam mı (dokunma). Rutin iki ayrı döngüde paylaşılır.

```json
{
  "numara": 58,
  "baslik": "Boya oklar",
  "gorev": "Kendi rutinini yaz: topla ve onar",
  "harita": [
    ". . . . . D",
    ". . . . . 1",
    ". K . . . 9",
    ". . . K . D",
    ". . . . . 2",
    "R D 7 3 . D"
  ],
  "komutlar": ["move", "collect", "panel_power", "repair"],
  "rutin_sayisi": 1,
  "parcalar": ["def bak():", "bak()", "move(East)", "move(North)", "if panel_power() == 0:", "elif panel_power() < 50:", "collect()", "repair()", "for i in range(5):"],
  "konular": ["fonksiyon", "rutine ad verme", "elif"],
  "ipuclari": [
    "Oklar önce doğuya, sonra kuzeye gösteriyor. Yoldaki her karede aynı soruyu sor; bu soruyu bir rutine koy ve ona sen ad ver.",
    "Panel yoksa `panel_power()` 0 verir: orada toplanacak parça olabilir. Çatlak panel 50'nin altındadır. Sıra önemli: önce 0'ı sor.",
    "`if panel_power() == 0:` → `collect()`, `elif panel_power() < 50:` → `repair()`. Rutini iki döngüde de kullan: `for i in range(5):` + `move(East)`, sonra `move(North)` ile."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def bak():", "    if panel_power() < 50:", "        repair()", "    elif panel_power() == 0:", "        collect()", "for i in range(5):", "    move(East)", "    bak()", "for i in range(5):", "    move(North)", "    bak()"],
      "aciklama": "0 da 50'den küçük: parçanın olduğu karede repair() çalışır ama orada panel yok. Önce 0'ı sor."
    },
    {
      "kod": ["def bak():", "    if panel_power() == 0:", "        collect()", "    else:", "        repair()", "for i in range(5):", "    move(East)", "    bak()", "for i in range(5):", "    move(North)", "    bak()"],
      "aciklama": "else her panele repair() der: sağlam panel onarılmaz. Zayıf olanı ayrıca sor: elif panel_power() < 50."
    },
    {
      "kod": ["for i in range(5):", "    move(East)", "    if panel_power() == 0:", "        collect()", "    elif panel_power() < 50:", "        repair()", "for i in range(5):", "    move(North)", "    if panel_power() == 0:", "        collect()", "    elif panel_power() < 50:", "        repair()"],
      "aciklama": "Robot işi yapar ama aynı soru iki kez yazıldı: bir rutine koy ve iki döngüde de çağır."
    }
  ],
  "cozum": [
    "def bak():",
    "    if panel_power() == 0:",
    "        collect()",
    "    elif panel_power() < 50:",
    "        repair()",
    "for i in range(5):",
    "    move(East)",
    "    bak()",
    "for i in range(5):",
    "    move(North)",
    "    bak()"
  ]
}
```

- [ ] **Step 4: `bolum-59.json` (Ortak iş, rutin + ana kodda toplam + report)** — panellerin gücü toplanır: 60 + 0 + 70 + 0 + 90 = 220.

```json
{
  "numara": 59,
  "baslik": "Ortak iş",
  "gorev": "Panel gücünü topla, raporla, piste var",
  "harita": [
    ". . . . . .",
    ". . K . . .",
    "K . . . . H",
    ". . . . Z 9",
    ". . . . . .",
    "R 6 Z 7 K ."
  ],
  "toz_suresi": [3, 2],
  "komutlar": ["move", "dust_here", "wait", "panel_power", "report"],
  "rapor": 220,
  "rutin_sayisi": 1,
  "parcalar": ["def ilerle():", "ilerle()", "move(East)", "move(North)", "while dust_here():", "wait()", "toplam = 0", "toplam += panel_power()", "report(toplam)", "for i in range(2):"],
  "konular": ["fonksiyon", "değişken", "toplam"],
  "ipuclari": [
    "İki doğu yolu var, ikisinde de toz olabilir. Doğuya ilerle-bekle işini rutine koy; iki yolda da kullan. Her adımdan sonra panelin gücünü toplama ekle.",
    "Toplamı rutinin dışında, ana kodda tut: rutinin içindeki `toplam`, dışarıdaki `toplam` ile aynı kutu değil.",
    "`toplam = 0` en başta; `for i in range(3):` içinde `ilerle()` ve `toplam += panel_power()`; iki `move(North)`; sonra `for i in range(2):` aynısı; `report(toplam)`, `move(North)`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["toplam = 0", "def ilerle():", "    move(East)", "    while dust_here():", "        wait()", "    toplam += panel_power()", "for i in range(3):", "    ilerle()", "move(North)", "move(North)", "for i in range(2):", "    ilerle()", "report(toplam)", "move(North)"],
      "aciklama": "Rutinin içinde toplam'a eklemek hata verir: rutin kendi içinde ayrı bir toplam arar. Toplamı ana kodda tut."
    },
    {
      "kod": ["def ilerle():", "    move(East)", "    while dust_here():", "        wait()", "for i in range(3):", "    toplam = 0", "    ilerle()", "    toplam += panel_power()", "move(North)", "move(North)", "for i in range(2):", "    ilerle()", "    toplam += panel_power()", "report(toplam)", "move(North)"],
      "aciklama": "toplam = 0 döngünün içinde: her turda sıfırlanır, anten yanlış sayıyı alır."
    },
    {
      "kod": ["def ilerle():", "    move(East)", "    while dust_here():", "        wait()", "toplam = 0", "for i in range(3):", "    ilerle()", "    toplam += panel_power()", "report(toplam)", "move(North)", "move(North)", "for i in range(2):", "    ilerle()", "move(North)"],
      "aciklama": "Rapor erken gönderildi: ikinci yoldaki panel henüz ölçülmedi."
    }
  ],
  "cozum": [
    "def ilerle():",
    "    move(East)",
    "    while dust_here():",
    "        wait()",
    "toplam = 0",
    "for i in range(3):",
    "    ilerle()",
    "    toplam += panel_power()",
    "move(North)",
    "move(North)",
    "for i in range(2):",
    "    ilerle()",
    "    toplam += panel_power()",
    "report(toplam)",
    "move(North)"
  ]
}
```

- [ ] **Step 5: `bolum-60.json` (Kanyona iniş, final, iki rutin)** — kuzeybatıdan güneydoğudaki kanyon dibine iniş.

```json
{
  "numara": 60,
  "baslik": "Kanyona iniş",
  "gorev": "İki rutinle kanyonun dibine in",
  "harita": [
    "R . . . . .",
    "Z K . . . .",
    ". 4 8 K . .",
    ". . . . K .",
    ". K Z 7 1 3",
    ". . . . . H"
  ],
  "toz_suresi": [2, 4],
  "komutlar": ["move", "dust_here", "wait", "panel_power", "repair"],
  "rutin_sayisi": 2,
  "parcalar": ["def asagi():", "def doguya():", "asagi()", "doguya()", "move(South)", "move(East)", "while dust_here():", "wait()", "if panel_power() < 50:", "repair()", "for i in range(2):"],
  "konular": ["fonksiyon", "birden çok fonksiyon", "tekrar"],
  "ipuclari": [
    "İki tür adım var: güneye inerken toz çıkabilir, doğuya giderken panel çıkar. Her tür için bir rutin yaz.",
    "Güney rutini: bir adım + toz varsa bekle. Doğu rutini: bir adım + zayıfsa onar. Doğu yolundaki her kare bir panel.",
    "Sıra: `asagi()` ×2, `doguya()` ×2, `asagi()` ×2, `doguya()` ×3, `asagi()`."
  ],
  "tipik_hatalar": [
    {
      "kod": ["def asagi():", "    move(South)", "    while dust_here():", "        wait()", "def doguya():", "    move(East)", "    repair()", "asagi()", "asagi()", "doguya()", "doguya()"],
      "aciklama": "Sağlam panel onarılmaz: doğu rutini önce ölçmeli."
    },
    {
      "kod": ["def asagi():", "    move(South)", "    while dust_here():", "        wait()", "asagi()", "asagi()", "move(East)", "if panel_power() < 50:", "    repair()", "move(East)", "asagi()", "asagi()", "move(East)", "move(East)", "if panel_power() < 50:", "    repair()", "move(East)", "if panel_power() < 50:", "    repair()", "asagi()"],
      "aciklama": "Robot iner ama bu bölüm iki rutin istiyor: doğuya gidip panele bakma işi de bir rutin olmalı."
    }
  ],
  "cozum": [
    "def asagi():",
    "    move(South)",
    "    while dust_here():",
    "        wait()",
    "def doguya():",
    "    move(East)",
    "    if panel_power() < 50:",
    "        repair()",
    "asagi()",
    "asagi()",
    "doguya()",
    "doguya()",
    "asagi()",
    "asagi()",
    "for i in range(3):",
    "    doguya()",
    "asagi()"
  ]
}
```

- [ ] **Step 6: `sinav-60.json`**

```json
{
  "bolum": 60,
  "sorular": [
    {
      "soru": "tur() rutini içinde adim() iki kez çağrılıyor. tur() üç kez çağrılırsa adim() kaç kez çalışır?",
      "secenekler": ["2", "3", "5", "6"],
      "dogru": 3,
      "aciklama": "Her tur() içinde 2 adim(): 3 × 2 = 6."
    },
    {
      "soru": "Hangisi rutin adı olabilir?",
      "secenekler": ["sabah turu", "2tur", "toz_gec", "def"],
      "dogru": 2,
      "aciklama": "Ad boşluk içermez, rakamla başlamaz, Python kelimesi olamaz. Kelimeleri _ ile bağla: toz_gec."
    },
    {
      "soru": "Aynı adımlar, araya başka işler girerek farklı yerlerde tekrar ediyorsa en iyisi hangisi?",
      "secenekler": ["Satırları kopyalamak", "Bir rutin yazıp gereken her yerde çağırmak", "Hepsini tek döngüye koymak", "Hiçbiri, olmaz"],
      "dogru": 1,
      "aciklama": "Döngü aynı şeyi arka arkaya yapar; rutin ise gereken her yerde, araya başka adımlar girerek çağrılabilir."
    }
  ]
}
```

- [ ] **Step 7: Denetim**

Run: `cd motor-test && dotnet test`
Expected: hepsi geçer. Düşen bölüm varsa Task 4 Step 8'deki gibi düzelt.

---

### Task 6: Unity paketi, görüntü denetimi, belgeler

**Files:**
- Modify: `ILERLEME.md` (Enes girdisi), `docs/tasarim/bolum-dosyasi.md` (Task 2-3'te yazıldıysa yalnızca kontrol)

- [ ] **Step 1: Windows paketi** — `oyun/Build/` sil, sonra:

Run: `"C:/Program Files/Unity/Hub/Editor/<sürüm>/Editor/Unity.exe" -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log`
Expected: `build.log` içinde `error CS` yok; `oyun/Build/Win/MarsKod.exe` var. Yeni `.cs` dosyalarının `.meta`'ları oluştu (`RoutineRule.cs.meta`).

- [ ] **Step 2: Bölüm görüntüleri**

Run: `oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -shots <scratchpad>/b6 -bolum 51` (~5 dk; bu sırada fareye/klavyeye dokunulmaz)
Expected: 51-60'ın hepsi "Tamamlandı"; drone parçaları (geçici pusula görünüşü) ve paneller/toz yerinde. Görüntülerden birkaçını `docs/tasarim/bolge-6-bulmacalar-2026-10-XX/` altına kopyala.

- [ ] **Step 3: Rutin mesajı elle** — `calistir.bat` ile oyunu aç, Bölüm 52'de rutinsiz (kopyala-yapıştır) çözümü yaz → "Kod bitti, sabah_turu rutini yok" mesajı çıkmalı; Bölüm 52 tanımlayıp çağırmadan → robot kıpırdamaz, "Kod bitti, … bitmedi" (dünya mesajı önce gelir).

- [ ] **Step 4: Bölüm 50 → 51 geçişini gözle** — Bölüm 51 artık var: Bölüm 50 finali sonrası "Sonraki bölüm" düğmesi / `sinav-50` nasıl davranıyor, not et (düzeltmesi Bölge 6 2. parçada: "Devam" + "KIVILCIM · KENDİNİ DENETLİYOR"). Bozuk görünüyorsa ILERLEME.md'ye açık soru olarak yaz; bu planda düzeltme yok.

- [ ] **Step 5: Yedi fare denetimi + klavye denetimi** (CLAUDE.md): `powershell -ExecutionPolicy Bypass -File scripts/klavye-denetimi.ps1` → `KLAVYE DENETIMI: TAMAM`; tam `-shots` turundaki denetim satırları TAMAM.

- [ ] **Step 6: ILERLEME.md** — "En son (özet)"e Enes girdisi: ne yapıldı (rutin şartı, D, Canyon, 51-60, sınavlar, `def` sayfası), `dotnet test` sayısı, geçici görünüşler (drone = pusula parçası, kanyon = kum tepeleri), 50→51 geçişi gözlemi, sıradaki iş (Bölge 6 2. parça: hikâye; Sonnet 5.5 · medium). Commit + push oturum kapanışında (ekip döngüsü adım 5).
