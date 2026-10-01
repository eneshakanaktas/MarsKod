using MarsKod.Dunya;

namespace MotorTest;

// Kırmızı kristal (üstünden geçilir, toplanamaz) ve ice_here() ("bu karede buz var mı?"): if için ihtiyaç.
public class KristalTests
{
    // Alt sıra: robot (0,0), buz (1,0), kristal (2,0), buz (3,0)
    static World Row(string[] commands = null) => new World(6, 6, new Cell(0, 0),
        new[] { new Cell(1, 0), new Cell(3, 0) }, null, null, commands, new[] { new Cell(2, 0) });

    [Fact]
    public void Ice_here_buz_varken_True_yokken_False_doner()
    {
        var report = ProgramRun.Execute("print(ice_here())\nmove(East)\nprint(ice_here())\n", Row());
        Assert.Equal("False\nTrue\n", report.Output);
    }

    [Fact]
    public void Ice_here_toplamaz_ve_tarama_olayi_yazar()
    {
        var world = Row();
        var report = ProgramRun.Execute("move(East)\nice_here()\nice_here()\ncollect()\nice_here()\n", world);
        Assert.False(report.Stopped);
        var scans = report.Trace.SelectMany(t => t.Events).OfType<Scanned>().ToList();
        Assert.Equal(new[] { true, true, false }, scans.Select(s => s.Found));
        Assert.Equal(1, world.CollectedCount);
    }

    [Fact]
    public void Ice_here_arguman_almaz()
    {
        var report = ProgramRun.Execute("ice_here(1)\n", Row());
        Assert.Equal("TypeError", report.Error.Type);
    }

    [Fact]
    public void Ice_here_acilmadiysa_oyun_kurali_olarak_durur()
    {
        var report = ProgramRun.Execute("ice_here()\n", Row(new[] { "move", "collect" }));
        Assert.Equal("Bu komut henüz açılmadı", report.Rule.Title);
    }

    [Fact]
    public void Kristalin_ustunden_gecilir()
    {
        var world = Row();
        var report = ProgramRun.Execute("move(East)\nmove(East)\nmove(East)\n", world);
        Assert.False(report.Stopped);
        Assert.Equal(new Cell(3, 0), world.Robot);
    }

    [Fact]
    public void Kristali_toplamak_robotu_durdurur()
    {
        var world = Row();
        var report = ProgramRun.Execute("move(East)\ncollect()\nmove(East)\ncollect()\n", world);
        Assert.Equal("Tehlikeli kristal", report.Rule.Title);
        Assert.Equal(4, report.StopLine);
        Assert.Equal(1, world.CollectedCount);
        Assert.IsType<Rejected>(report.Trace.Last().Events.Last());
    }

    [Fact]
    public void Kristal_buz_sayilmaz()
    {
        Assert.Equal(2, Row().IceCount);
        Assert.False(Row().Complete);
    }

    [Fact]
    public void Kristal_bolum_dosyasinda_T_ile_yazilir()
    {
        var level = Level.Parse(@"{ ""numara"": 1, ""baslik"": ""x"", ""gorev"": ""y"",
            ""harita"": [ "". . . . . ."", "". . . . . ."", "". . . . . ."", "". . . . . ."", "". . . . . ."", ""R B T . . ."" ],
            ""komutlar"": [""move"", ""collect"", ""ice_here""], ""konular"": [""if""], ""ipuclari"": [""a""],
            ""cozum"": [""move(East)"", ""collect()""] }");
        Assert.Equal(new Cell(2, 0), Assert.Single(level.Crystals));
        Assert.Equal(new Cell(1, 0), Assert.Single(level.Ices));
        Assert.Empty(LevelCheck.Problems(level));
    }
}
