using MarsKod.Dunya;
using MarsKod.Motor;

namespace MotorTest;

// Toz bulutu (Bölge 4, Bölüm 31-40, while): robot bulutun içindeyken yürüyemez; dust_here() bakar, wait() bir tur bekletir.
// Bulutun kaç beklemede dağılacağını oyuncu bilmez: doğru kalıp "while dust_here(): wait()".
public class DustTests
{
    // Robot (0,0). Doğuda (1,0) 3 beklemelik toz bulutu.
    static World Board(int duration = 3, string[] commands = null) => new World(6, 6, new Cell(0, 0),
        System.Array.Empty<Cell>(), null, new Cell(3, 0), commands, null, null,
        new System.Collections.Generic.Dictionary<Cell, int> { [new Cell(1, 0)] = duration });

    const string WhileSolution = "move(East)\nwhile dust_here():\n    wait()\nmove(East)\nmove(East)\n";

    [Fact]
    public void While_ile_toz_gecene_kadar_beklenir()
    {
        var world = Board();
        var report = ProgramRun.Execute(WhileSolution, world);
        Assert.False(report.Stopped);
        Assert.True(world.Complete);
        Assert.Equal(3, report.Trace.SelectMany(t => t.Events).OfType<Waited>().Count());
    }

    [Fact]
    public void Ayni_kod_baska_toz_suresinde_de_calisir()
    {
        var world = Board(duration: 6);
        Assert.False(ProgramRun.Execute(WhileSolution, world).Stopped);
        Assert.True(world.Complete);
    }

    [Fact]
    public void Tozun_icinde_yurumek_oyun_kuralidir()
    {
        var report = ProgramRun.Execute("move(East)\nmove(East)\n", Board());
        Assert.Equal("Tozda yol görünmüyor", report.Rule.Title);
        Assert.Equal(2, report.StopLine);
        var blocked = report.Trace.SelectMany(t => t.Events).OfType<Blocked>().Single();
        Assert.True(blocked.ByDust);
    }

    [Fact]
    public void Az_beklemek_yetmez()
    {
        var report = ProgramRun.Execute("move(East)\nwait()\nwait()\nmove(East)\n", Board());
        Assert.Equal("Tozda yol görünmüyor", report.Rule.Title);
    }

    [Fact]
    public void Toz_yokken_beklemek_oyun_kuralidir()
    {
        Assert.Equal("Beklenecek toz yok", ProgramRun.Execute("wait()\n", Board()).Rule.Title);
        var report = ProgramRun.Execute("move(East)\nfor i in range(4):\n    wait()\n", Board());
        Assert.Equal("Beklenecek toz yok", report.Rule.Title);
    }

    [Fact]
    public void Dust_here_yalnizca_robotun_karesine_bakar()
    {
        var report = ProgramRun.Execute("print(dust_here())\nmove(East)\nprint(dust_here())\n", Board());
        Assert.Equal("False\nTrue\n", report.Output);
    }

    [Fact]
    public void Bekleme_olayi_kalan_sureyi_yazar()
    {
        var report = ProgramRun.Execute("move(East)\nwait()\n", Board());
        var waited = report.Trace.SelectMany(t => t.Events).OfType<Waited>().Single();
        Assert.Equal(new Cell(1, 0), waited.At);
        Assert.Equal(2, waited.Left);
        Assert.Equal(3, waited.Total);
    }

    [Fact]
    public void Arguman_verilirse_TypeError()
    {
        Assert.Equal("TypeError", ProgramRun.Execute("wait(3)\n", Board()).Error.Type);
        Assert.Equal("TypeError", ProgramRun.Execute("dust_here(East)\n", Board()).Error.Type);
    }

    [Fact]
    public void Acilmadiysa_oyun_kurali_olarak_durur()
    {
        var report = ProgramRun.Execute("move(East)\nwait()\n", Board(commands: new[] { "move" }));
        Assert.Equal("Bu komut henüz açılmadı", report.Rule.Title);
    }

    // ---- bölüm dosyası ----

    const string Mini = @"{
  ""numara"": 1, ""baslik"": ""a"", ""gorev"": ""b"",
  ""harita"": [""Z . ."", ""R Z H""],
  ""toz_suresi"": [4, 2],
  ""komutlar"": [""move"", ""dust_here"", ""wait""], ""konular"": [""while""], ""ipuclari"": [""c""],
  ""cozum"": [""move(East)""]
}";

    [Fact]
    public void Toz_sureleri_haritadaki_okuma_sirasiyla_baglanir()
    {
        var level = Level.Parse(Mini);
        Assert.Equal(4, level.Dust[new Cell(0, 1)]); // üst satır önce
        Assert.Equal(2, level.Dust[new Cell(1, 0)]);
    }

    [Theory]
    [InlineData("[4, 2]", "[4]", "2 toz bulutu")]
    [InlineData("[4, 2]", "[4, 0]", "1 ya da daha büyük")]
    [InlineData("[4, 2]", "[\"4\", 2]", "tam sayılardan")]
    public void Toz_suresi_yanlissa_anlasilir_hata(string from, string to, string message)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace(from, to)));
        Assert.Contains(message, e.Message);
    }

    [Fact]
    public void Pusula_parcasi_P_ile_yazilir()
    {
        var level = Level.Parse(Mini.Replace("Z . .", "P . .").Replace("[4, 2]", "[2]").Replace("\"wait\"]", "\"wait\", \"collect\"]"));
        Assert.Equal(Collectible.CompassPart, level.Item);
        Assert.Equal(Collectible.CompassPart, Regions.Item(Region.Dunes));
        Assert.Equal(Region.Dunes, Regions.Of(31));
        Assert.Equal(Region.Dunes, Regions.Of(40));
    }
}
