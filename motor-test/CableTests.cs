using System.Collections.Generic;
using System.Numerics;
using MarsKod.Dunya;
using MarsKod.Motor;

namespace MotorTest;

// Kablo makarası (Bölge 5, Bölüm 41-50, değişkenler): makaralar collect() ile toplanır; uzunluklarını oyuncu
// görmez, cable_length() ölçer. report(sayı) antene bir sayı gönderir; görev doğru sayıyı istiyorsa ancak
// bir değişkenle (sayaç, toplam) çözülür.
public class CableTests
{
    // Robot (0,0). Doğuda (1,0) 40 m, (2,0) 70 m, (3,0) 30 m makara; anten "expected" bekler.
    static World Row(int? expected = 140, string[] commands = null)
    {
        var reels = new Dictionary<Cell, int> { [new Cell(1, 0)] = 40, [new Cell(2, 0)] = 70, [new Cell(3, 0)] = 30 };
        return new World(6, 6, new Cell(0, 0), reels.Keys, null, null, commands, null, null, null,
            reels, expected == null ? (BigInteger?)null : expected.Value);
    }

    const string TotalSolution =
        "toplam = 0\nfor i in range(3):\n    move(East)\n    toplam += cable_length()\n    collect()\nreport(toplam)\n";

    [Fact]
    public void Toplam_degiskenle_dogru_rapor_gorevi_bitirir()
    {
        var world = Row();
        var report = ProgramRun.Execute(TotalSolution, world);
        Assert.False(report.Stopped);
        Assert.True(world.Complete);
        var sent = report.Trace.SelectMany(t => t.Events).OfType<ReportSent>().Single();
        Assert.True(sent.Correct);
        Assert.Equal(new BigInteger(140), sent.Value);
    }

    [Fact]
    public void Olcum_makara_uzunlugunu_verir_toplaninca_sifir()
    {
        var world = Row();
        world.Move(Direction.East);
        Assert.Equal(40, world.CableLength());
        world.Collect();
        Assert.Equal(0, world.CableLength());
        world.Move(Direction.West);
        Assert.Equal(0, world.CableLength());
        Assert.Equal(new[] { 40, 0, 0 }, world.Events.OfType<CableMeasured>().Select(e => e.Length));
    }

    [Fact]
    public void Rapor_gonderilmeden_gorev_bitmez()
    {
        var world = Row();
        var report = ProgramRun.Execute("for i in range(3):\n    move(East)\n    collect()\n", world);
        Assert.False(report.Stopped);
        Assert.Equal(0, world.IceLeft);
        Assert.False(world.Complete);
    }

    [Fact]
    public void Yanlis_sayi_oyun_kuralidir()
    {
        var report = ProgramRun.Execute("for i in range(3):\n    move(East)\n    collect()\nreport(100)\n", Row());
        Assert.Equal("Anten bu sayıyı beklemiyordu", report.Rule.Title);
        Assert.Equal(4, report.StopLine);
        Assert.False(report.Trace.SelectMany(t => t.Events).OfType<ReportSent>().Single().Correct);
    }

    [Fact]
    public void Sayi_olmayan_rapor_oyun_kuralidir()
    {
        Assert.Equal("Antene yalnızca sayı gönderilir", ProgramRun.Execute("report(\"140\")\n", Row()).Rule.Title);
        Assert.Equal("Antene yalnızca sayı gönderilir", ProgramRun.Execute("report(True)\n", Row()).Rule.Title);
        Assert.Equal("TypeError", ProgramRun.Execute("report()\n", Row()).Error.Type);
    }

    [Fact]
    public void Rapor_istenmeyen_bolumde_rapor_oyun_kuralidir()
    {
        Assert.Equal("Rapor istenmiyor", ProgramRun.Execute("report(3)\n", Row(expected: null)).Rule.Title);
    }

    [Fact]
    public void Komutlar_bolumde_acik_degilse_kullanilamaz()
    {
        var report = ProgramRun.Execute("cable_length()\n", Row(commands: new[] { "move", "collect" }));
        Assert.Equal("Bu komut henüz açılmadı", report.Rule.Title);
    }

    // Bölüm dosyasında makaralar M, uzunlukları "makara_uzunlugu" (okuma sırasıyla), beklenen sayı "rapor"
    const string Mini = "{ \"numara\": 41, \"baslik\": \"x\", \"gorev\": \"y\", \"ipuclari\": [\"z\"], \"cozum\": [\"report(2)\"]," +
        " \"harita\": [\"R M M\"], \"makara_uzunlugu\": [40, 70], \"rapor\": 2, \"konular\": [], \"komutlar\": [\"move\", \"collect\", \"cable_length\", \"report\"] }";

    [Fact]
    public void Makaralar_ve_rapor_dosyadan_okunur()
    {
        var level = Level.Parse(Mini);
        Assert.Equal(Collectible.CableReel, level.Item);
        Assert.Equal(70, level.Reels[new Cell(2, 0)]);
        Assert.Equal(2, level.ExpectedReport);
        Assert.Equal(Collectible.CableReel, Regions.Item(Region.AntennaHill));
        Assert.Equal(Region.AntennaHill, Regions.Of(41));
        Assert.Equal(Region.AntennaHill, Regions.Of(50));
    }

    [Theory]
    [InlineData("[40, 70]", "[40]", "2 kablo makarası")]
    [InlineData("[40, 70]", "[40, 0]", "1 ya da daha büyük")]
    [InlineData("\"rapor\": 2, ", "", "\"rapor\" yok")]
    [InlineData(", \"report\"]", "]", "\"report\" yok")]
    public void Makara_ve_rapor_yanlissa_anlasilir_hata(string from, string to, string message)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace(from, to)));
        Assert.Contains(message, e.Message);
    }
}
