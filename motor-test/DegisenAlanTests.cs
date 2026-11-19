using System.Text;
using MarsKod.Dunya;
using MarsKod.Motor;

namespace MotorTest;

// Değişen alan ("duzenler", Layout.cs): bölümün buzu, kristali, engeli ve kraterleri her denemede başka karelerde olabilir.
// Kod her düzende ayrı denenir; bölüm, kod hepsinde çalışınca biter. Bölge 2 sınavları (15, 20) ve Bölge 10 bu kalıbı kullanır.
public class DegisenAlanTests
{
    internal static Level Load(int number) => Level.Parse(File.ReadAllText(Path.Combine(BolumDizini, "bolum-" + number + ".json")));

    internal static string BolumDizini
    {
        get
        {
            var dir = AppContext.BaseDirectory;
            while (dir != null && !Directory.Exists(Path.Combine(dir, "oyun", "Assets", "Resources", "Bolumler"))) dir = Path.GetDirectoryName(dir);
            return Path.Combine(dir!, "oyun", "Assets", "Resources", "Bolumler");
        }
    }

    // ---- dosya okuma ----

    const string Empty = "\". . . .\"";
    const string Map = "\"R . . .\"";

    // 4x4 harita: robot (0,0); duzenler dışarıdan verilir
    static Level Small(string layouts, string commands = "\"move\", \"collect\", \"ice_here\"") =>
        Level.Parse("{\"numara\": 15, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [" + Empty + ", " + Empty + ", " + Empty + ", " + Map + "],"
            + " \"duzenler\": " + layouts + ", \"komutlar\": [" + commands + "], \"konular\": [], \"ipuclari\": [\"x\"], \"cozum\": [\"move(North)\"]}");

    static string Layout(params string[] rows) => "[" + string.Join(", ", rows.Select(r => "\"" + r + "\"")) + "]";

    [Fact]
    public void Duzende_buz_kristal_ve_engel_denemeye_gore_gelir()
    {
        var level = Small("[" + Layout(". . . .", "B . . .", "T . K .", ". . . .") + ", " + Layout(". . . .", "T . . .", "B . . K", ". . . .") + "]");
        Assert.Equal(2, level.VersionCount);
        Assert.Equal(new[] { new Cell(0, 2) }, level.IcesOf(0));
        Assert.Equal(new[] { new Cell(0, 1) }, level.CrystalsOf(0));
        Assert.Equal(new[] { new Cell(2, 1) }, level.RocksOf(0));
        Assert.Equal(new[] { new Cell(0, 1) }, level.IcesOf(1));
        Assert.Equal(new[] { new Cell(0, 2) }, level.CrystalsOf(1));
        Assert.Equal(new[] { new Cell(3, 1) }, level.RocksOf(1));
        Assert.Empty(level.Ices);   // harita hep aynı kalan şeyleri taşır
    }

    [Theory]
    [InlineData("[[\". . . .\"]]", "haritanın boyunda")]
    [InlineData("[[\". . . .\", \". . . .\", \". . . .\", \". . . .\"]]", "boş")]
    [InlineData("[[\". . . .\", \". . . .\", \". . . .\", \"B . . .\"]]", "dolu bir kareye")]   // (0,0) robotun karesi
    [InlineData("[[\". . . .\", \". . . .\", \". . . .\", \". Q . .\"]]", "yalnızca")]
    [InlineData("[[\". . . .\", \". . . .\", \". . . .\", \". U . .\"]]", "meteor_ahead")]
    public void Yanlis_duzen_anlasilir_mesaj_verir(string layouts, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Small(layouts));
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Ayni_duzen_iki_kez_yazilmaz()
    {
        var one = Layout("B . . .", ". . . .", ". . . .", ". . . .");
        var e = Assert.Throws<DataFormatError>(() => Small("[" + one + ", " + one + "]"));
        Assert.Contains("aynı düzen", e.Message);
    }

    [Fact]
    public void Her_duzende_toplanacak_sayisi_ayni_olur()
    {
        var e = Assert.Throws<DataFormatError>(() => Small("[" + Layout(". . . .", "B . . .", "B . . .", ". . . .") + ", " + Layout(". . . .", "B . . .", ". . . .", ". . . .") + "]"));
        Assert.Contains("toplanacak sayısı aynı olmalı", e.Message);
    }

    [Fact]
    public void Eski_meteorlar_adi_artik_okunmaz()
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse("{\"numara\": 91, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [\"R . . .\"], \"meteorlar\": [], \"komutlar\": [\"move\"], \"konular\": [], \"ipuclari\": [\"x\"], \"cozum\": [\"move(East)\"]}"));
        Assert.Contains("meteorlar", e.Message);
    }

    // ---- Bölüm 15 ve 20 ----

    [Theory]
    [InlineData(15, 2)]
    [InlineData(20, 3)]
    public void Bolge_2_sinavlari_degisen_alan(int number, int layouts)
    {
        var level = Load(number);
        Assert.Equal(layouts, level.VersionCount);
        Assert.Equal(Region.PolarIce, Regions.Of(number));
        Assert.All(level.RunVersions(level.Solution), run => Assert.True(run.Report.Complete));
        Assert.True(level.Run(level.Solution).Complete);
        Assert.Equal(3, level.IcesOf(0).Count);
        for (int v = 1; v < layouts; v++) Assert.Equal(3, level.IcesOf(v).Count);
    }

    [Fact]
    public void Ilk_acilan_duzen_15te_bugunku_20de_yeni_harita()
    {
        Assert.Equal(new HashSet<Cell> { new Cell(0, 4), new Cell(0, 3), new Cell(0, 1) }, Load(15).IcesOf(0).ToHashSet());
        Assert.Equal(new HashSet<Cell> { new Cell(1, 2), new Cell(2, 2), new Cell(1, 0) }, Load(20).IcesOf(0).ToHashSet());
    }

    [Fact]
    public void Bolum_15te_hicbir_kosulsuz_kod_gecmez()
    {
        var level = Load(15);
        foreach (var code in new[]
        {
            "for i in range(5):\n    move(North)\n    collect()\n",
            "for i in range(5):\n    move(North)\n",
            "move(North)\ncollect()\nmove(North)\nmove(North)\ncollect()\nmove(North)\ncollect()\nmove(North)\n",
            "move(North)\nmove(North)\ncollect()\nmove(North)\nmove(North)\ncollect()\nmove(North)\ncollect()\n",
        })
            Assert.False(level.Run(code).Complete, code);
    }

    [Fact]
    public void Bolum_20de_eski_bolumlerin_kodlari_yetmez()
    {
        var level = Load(20);
        string Loop(int turns, string tail) => "for i in range(" + turns + "):\n    if rock_ahead(North):\n        move(East)\n    else:\n        move(North)\n" + tail;
        Assert.True(level.Run(Loop(7, "    if ice_here():\n        collect()\n")).Complete);
        Assert.False(level.Run(Loop(7, "        if ice_here():\n            collect()\n")).Complete);   // 19'un kodu: yalnız kuzeye çıkınca bakar
        Assert.False(level.Run(Loop(7, "    collect()\n")).Complete);                                        // 18'in kodu: korumasız collect
        Assert.False(level.Run("for i in range(7):\n    move(North)\n    if ice_here():\n        collect()\n").Complete);   // 15'in kodu
        Assert.False(level.Run(Loop(5, "    if ice_here():\n        collect()\n")).Complete);
        Assert.False(level.Run(Loop(8, "    if ice_here():\n        collect()\n")).Complete);
    }

    // ---- ezber denetimi ----

    // Çözümün bir düzendeki yürüyüşü, koşulsuz yazılmış hâli: yalnızca move(...) ve collect() alt alta
    // Bir denemede çözümün yaptığı her şey koşulsuz yazılınca: yürüyüş, toplama, bekleme, rapor (ezber kod; DegisenTozTests ve
    // DegisenOlcuTests de kullanır)
    internal static string Memorized(Level level, int version)
    {
        var world = level.CreateWorld(version);
        var report = ProgramRun.Execute(level.Solution, world);   // bölümün rutin/ekran şartları ezber için aranmaz
        var code = new StringBuilder();
        foreach (var entry in report.Trace)
            foreach (var e in entry.Events)
            {
                if (e is Moved m) code.AppendLine("move(" + m.Direction + ")");
                else if (e is Collected c && c.IceIndex >= 0) code.AppendLine("collect()");
                else if (e is Waited) code.AppendLine("wait()");
                else if (e is ReportSent r) code.AppendLine("report(" + r.Value + ")");
            }
        return code.ToString();
    }

    internal static string Why(Level level, string code, int v)
    {
        var r = ProgramRun.Execute(code, level.CreateWorld(v));
        return "halt=" + r.Halt?.Reason + " rule=" + r.Rule?.Title + " err=" + r.Error?.Message;
    }

    internal static bool PlainRun(Level level, string code, int version) => ProgramRun.Execute(code, level.CreateWorld(version)).Complete;

    // Ezberle geçildiği bilinen ve henüz düzeltilmemiş bölümler (2026-11-18: 92, 93, 96, 100 düzeltildi, liste boş).
    // Düzeltilen bölüm listeden çıkarılır; Bilinen_ezber_acigi_listesi_guncel test'i listeyi güncel tutar.
    static readonly int[] KnownMemorizable = { };

    static IEnumerable<Level> ChangingLevelList() =>
        Directory.GetFiles(BolumDizini, "*.json").Select(f => Level.Parse(File.ReadAllText(f))).Where(l => l.Layouts.Count > 1).OrderBy(l => l.Number);

    public static IEnumerable<object[]> ChangingLevels() =>
        ChangingLevelList().Where(l => !KnownMemorizable.Contains(l.Number)).Select(l => new object[] { l.Number });

    static bool MemorizableInAllLayouts(Level level) =>
        Enumerable.Range(0, level.VersionCount).Any(v => { var code = Memorized(level, v); return Enumerable.Range(0, level.VersionCount).All(other => PlainRun(level, code, other)); });

    [Fact]
    public void Bilinen_ezber_acigi_listesi_guncel()
    {
        foreach (var level in ChangingLevelList())
            Assert.Equal(KnownMemorizable.Contains(level.Number), MemorizableInAllLayouts(level));
    }

    // Değişen alanın amacı: koşulsuz yazılmış yol (ezber) bütün düzenlerde geçemesin. Her düzenin kendi yürüyüşü
    // yazıldığında en az bir düzende takılmalı; takılmıyorsa bölüm ezberle geçiliyor demektir.
    [Theory]
    [MemberData(nameof(ChangingLevels))]
    public void Ezber_butun_duzenlerde_gecemez(int number)
    {
        var level = Load(number);
        for (int v = 0; v < level.VersionCount; v++)
        {
            var code = Memorized(level, v);
            Assert.True(PlainRun(level, code, v), number + ": " + (v + 1) + ". düzenin yürüyüşü kendi düzeninde geçmeli: " + code.Replace("\n", " ") + "| " + Why(level, code, v));
            Assert.False(Enumerable.Range(0, level.VersionCount).All(other => PlainRun(level, code, other)), number + ": bu düzenler ezberle geçiliyor: " + (v + 1) + ". düzenin yolu koşulsuz yazılınca hepsinde çalışıyor.");
        }
    }

    // ---- Bölge 10 değişmeden çalışır ----

    [Fact]
    public void Bolge_10_duzenleri_adi_degisse_de_ayni()
    {
        var level = Load(91);
        Assert.Equal(2, level.VersionCount);
        Assert.NotEqual(level.CratersOf(0), level.CratersOf(1));
        Assert.All(level.RunVersions(level.Solution), run => Assert.True(run.Report.Complete));
    }
}
