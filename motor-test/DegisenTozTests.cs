using MarsKod.Dunya;

namespace MotorTest;

// Her denemede değişen toz süresi (AttemptFile, A2): "toz_suresi" deneme başına liste olunca kod her denemede ayrı denenir;
// wait() sayısını ezberleyen kod bir denemede takılır (tozlu karede yürümek de tozsuz karede beklemek de robotu durdurur).
public class DegisenTozTests
{
    // 1 satır: robot, toz, boş, hedef
    static Level Small(string times) =>
        Level.Parse("{\"numara\": 31, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [\"R Z . H\"], \"komutlar\": [\"move\", \"dust_here\", \"wait\"], \"konular\": [\"while\"],"
            + " \"toz_suresi\": " + times + ", \"ipuclari\": [\"c\"], \"cozum\": [\"move(East)\", \"while dust_here():\", \"    wait()\", \"move(East)\", \"move(East)\"]}");

    [Fact]
    public void Deneme_basina_liste_her_denemede_kendi_suresini_verir()
    {
        var level = Small("[[3], [5]]");
        Assert.Equal(2, level.VersionCount);
        Assert.Equal(3, level.DustOf(0)[new Cell(1, 0)]);
        Assert.Equal(5, level.DustOf(1)[new Cell(1, 0)]);
        Assert.All(level.RunVersions(level.Solution), run => Assert.True(run.Report.Complete));
    }

    [Fact]
    public void Duz_liste_eskisi_gibi_tek_deneme()
    {
        var level = Small("[3]");
        Assert.Equal(1, level.VersionCount);
        Assert.Equal(3, level.DustOf(0)[new Cell(1, 0)]);
    }

    [Fact]
    public void Ezber_bekleme_obur_denemede_takilir()
    {
        var level = Small("[[3], [5]]");
        string three = "move(East)\nwait()\nwait()\nwait()\nmove(East)\nmove(East)\n";
        Assert.True(level.RunVersions(three)[0].Report.Complete);
        Assert.False(level.Run(three).Complete);
        string five = "move(East)\nwait()\nwait()\nwait()\nwait()\nwait()\nmove(East)\nmove(East)\n";
        Assert.False(level.Run(five).Complete);   // 1. denemede toz 3'te dağılır, 4. wait robotu durdurur
    }

    [Theory]
    [InlineData("[[3]]")]                 // deneme başına yazılmış ama tek deneme
    [InlineData("[[3], [3]]")]            // iki deneme aynı
    [InlineData("[[3], [5, 2]]")]         // bulut sayısı tutmuyor
    [InlineData("[[3], [0]]")]            // süre en az 1
    [InlineData("[[3], [\"5\"]]")]        // tırnaklı sayı
    public void Yanlis_yazim_Turkce_hata_verir(string times)
    {
        var e = Assert.Throws<DataFormatError>(() => Small(times));
        Assert.Contains("toz_suresi", e.Message);
    }

    static IEnumerable<Level> DustLevels() =>
        Directory.GetFiles(DegisenAlanTests.BolumDizini, "*.json")
            .Select(f => Level.Parse(File.ReadAllText(f))).Where(l => l.DustVersions.Count > 1).OrderBy(l => l.Number);

    public static IEnumerable<object[]> DustLevelNumbers() => DustLevels().Select(l => new object[] { l.Number });

    [Fact]
    public void Tozun_ogretildigi_bolumlerde_sure_degisir()
    {
        Assert.Equal(new[] { 31, 34, 35, 36, 37, 38, 39, 40, 51 }, DustLevels().Select(l => l.Number));
    }

    // Her denemenin beklemeleri koşulsuz yazılınca (ezber) kendi denemesinde geçmeli, en az bir öteki denemede takılmalı
    [Theory]
    [MemberData(nameof(DustLevelNumbers))]
    public void Ezber_butun_denemelerde_gecemez(int number)
    {
        var level = DegisenAlanTests.Load(number);
        for (int v = 0; v < level.VersionCount; v++)
        {
            var code = DegisenAlanTests.Memorized(level, v);
            Assert.Contains("wait()", code);
            Assert.True(DegisenAlanTests.PlainRun(level, code, v), number + ": " + (v + 1) + ". denemenin ezberi kendi denemesinde geçmeli | " + DegisenAlanTests.Why(level, code, v));
            Assert.False(Enumerable.Range(0, level.VersionCount).All(other => DegisenAlanTests.PlainRun(level, code, other)),
                number + ": " + (v + 1) + ". denemenin bekleme sayıları ezberlenince her denemede geçiyor.");
        }
    }
}
