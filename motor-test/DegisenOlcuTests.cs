using MarsKod.Dunya;

namespace MotorTest;

// Her denemede değişen kristal boyu ve rapor (AttemptFile, A3): "kristal_boyu" ve "rapor" deneme başına yazılınca kod her
// denemede ayrı denenir; ölçmeden yazılan cevap (report(27)) bir denemede tutmaz. Bitiş yazısındaki {rapor} o denemenin cevabı.
public class DegisenOlcuTests
{
    // 1 satır: robot, iki tuz kristali; boyların toplamı bildirilir
    static Level Small(string sizes, string report, string outro = "Toplam: {rapor} cm") =>
        Level.Parse("{\"numara\": 82, \"baslik\": \"a\", \"gorev\": \"b\", \"bitis\": \"" + outro + "\", \"harita\": [\"R S S .\"],"
            + " \"komutlar\": [\"move\", \"collect\", \"crystal_size\", \"report\"], \"konular\": [\"liste\"],"
            + " \"kristal_boyu\": " + sizes + ", \"rapor\": " + report + ", \"ipuclari\": [\"c\"],"
            + " \"cozum\": [\"toplam = 0\", \"for i in range(2):\", \"    move(East)\", \"    toplam += crystal_size()\", \"    collect()\", \"report(toplam)\"]}");

    [Fact]
    public void Deneme_basina_boy_ve_rapor_her_denemede_kendi_cevabini_bekler()
    {
        var level = Small("[[3, 4], [5, 1]]", "[7, 6]");
        Assert.Equal(2, level.VersionCount);
        Assert.Equal(5, level.CrystalSizesOf(1)[new Cell(1, 0)]);
        Assert.Equal(6, level.ReportOf(1));
        Assert.All(level.RunVersions(level.Solution), run => Assert.True(run.Report.Complete));
        Assert.False(level.Run("move(East)\ncollect()\nmove(East)\ncollect()\nreport(7)\n").Complete);   // ezber cevap 2. denemede tutmaz
    }

    [Fact]
    public void Bitis_yazisi_o_denemenin_cevabini_yazar()
    {
        var level = Small("[[3, 4], [5, 1]]", "[7, 6]");
        Assert.Equal("Toplam: 7 cm", level.OutroOf(0));
        Assert.Equal("Toplam: 6 cm", level.OutroOf(1));
    }

    [Fact]
    public void Duz_yazim_eskisi_gibi()
    {
        var level = Small("[3, 4]", "7");
        Assert.Equal(1, level.VersionCount);
        Assert.Equal("Toplam: 7 cm", level.OutroOf(0));
    }

    [Theory]
    [InlineData("[[3, 4], [5, 1]]", "7", "tek sayı")]                  // boy değişiyor, cevap tek
    [InlineData("[[3, 4], [5, 1]]", "[7, 6, 5]", "eşit olmalı")]       // deneme sayıları tutmuyor
    [InlineData("[3, 4]", "[7]", "2-3 deneme")]                       // tek denemelik rapor listesi
    [InlineData("[[3, 4], [3, 4]]", "[7, 7]", "iki deneme aynı")]
    [InlineData("[[3, 4], [5]]", "[7, 5]", "2 tuz kristali")]
    public void Yanlis_yazim_Turkce_hata_verir(string sizes, string report, string message)
    {
        var e = Assert.Throws<DataFormatError>(() => Small(sizes, report));
        Assert.Contains(message, e.Message);
    }

    static IEnumerable<Level> SizeLevels() =>
        Directory.GetFiles(DegisenAlanTests.BolumDizini, "*.json")
            .Select(f => Level.Parse(File.ReadAllText(f))).Where(l => l.CrystalSizeVersions.Count > 1).OrderBy(l => l.Number);

    public static IEnumerable<object[]> SizeLevelNumbers() => SizeLevels().Select(l => new object[] { l.Number });

    [Fact]
    public void Olcum_bolumlerinde_boy_degisir()
    {
        Assert.Equal(new[] { 82, 83, 85, 90 }, SizeLevels().Select(l => l.Number));
    }

    // Her denemede cevap farklı olmalı; o denemenin yürüyüşü ve cevabı koşulsuz yazılınca (ezber) başka bir denemede takılmalı
    [Theory]
    [MemberData(nameof(SizeLevelNumbers))]
    public void Ezber_cevap_butun_denemelerde_gecemez(int number)
    {
        var level = DegisenAlanTests.Load(number);
        var answers = Enumerable.Range(0, level.VersionCount).Select(v => level.ReportOf(v)).ToList();
        Assert.Equal(answers.Count, answers.Distinct().Count());
        for (int v = 0; v < level.VersionCount; v++)
        {
            var code = DegisenAlanTests.Memorized(level, v);
            Assert.Contains("report(" + level.ReportOf(v) + ")", code);
            Assert.True(DegisenAlanTests.PlainRun(level, code, v), number + ": " + (v + 1) + ". denemenin ezberi kendi denemesinde geçmeli | " + DegisenAlanTests.Why(level, code, v));
            Assert.False(Enumerable.Range(0, level.VersionCount).All(other => DegisenAlanTests.PlainRun(level, code, other)),
                number + ": " + (v + 1) + ". denemenin cevabı ezberlenince her denemede geçiyor.");
        }
    }
}
