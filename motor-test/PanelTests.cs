using MarsKod.Dunya;

namespace MotorTest;

// Güneş panelleri (Bölge 3, Bölüm 21-30): panel_power() gücü ölçer, repair() çatlak paneli onarır,
// kırık panelin parçası collect() ile toplanır. Karşılaştırma (<, >, ==) ve elif için ihtiyaç.
public class PanelTests
{
    // Alt sıra: robot (0,0), sağlam 80 (1,0), çatlak 20 (2,0), kırık 0 (3,0, parçası toplanır), boş (4,0)
    static World Row(string[] commands = null) => new World(6, 6, new Cell(0, 0),
        new[] { new Cell(3, 0) }, null, null, commands, null,
        new Dictionary<Cell, int> { [new Cell(1, 0)] = 80, [new Cell(2, 0)] = 20, [new Cell(3, 0)] = 0 });

    [Fact]
    public void Panel_power_gucu_sayi_olarak_verir_panel_yoksa_0()
    {
        var report = ProgramRun.Execute(
            "print(panel_power())\nfor i in range(4):\n    move(East)\n    print(panel_power())\n", Row());
        Assert.Equal("0\n80\n20\n0\n0\n", report.Output);
    }

    [Fact]
    public void Panel_power_olcme_olayi_yazar_ve_bir_sey_degistirmez()
    {
        var world = Row();
        var report = ProgramRun.Execute("move(East)\npanel_power()\n", world);
        var m = Assert.Single(report.Trace.SelectMany(t => t.Events).OfType<Measured>());
        Assert.Equal(80, m.Power);
        Assert.Equal(1, world.CrackedLeft);
    }

    [Fact]
    public void Repair_catlak_paneli_onarir_guc_100_olur()
    {
        var world = Row();
        var report = ProgramRun.Execute("move(East)\nmove(East)\nrepair()\nprint(panel_power())\n", world);
        Assert.False(report.Stopped);
        Assert.Equal("100\n", report.Output);
        Assert.Equal(0, world.CrackedLeft);
        Assert.Single(report.Trace.SelectMany(t => t.Events).OfType<Repaired>());
    }

    [Theory]
    [InlineData("move(East)\nrepair()\n", "Panel zaten sağlam")]
    [InlineData("for i in range(3):\n    move(East)\nrepair()\n", "Kırık panel onarılmaz")]
    [InlineData("repair()\n", "Burada panel yok")]
    [InlineData("move(East)\ncollect()\n", "Çalışan panel sökülmez")]
    [InlineData("move(East)\nmove(East)\ncollect()\n", "Çalışan panel sökülmez")]
    public void Yanlis_panele_yanlis_is_oyun_kuralidir(string code, string title)
    {
        var report = ProgramRun.Execute(code, Row());
        Assert.Equal(title, report.Rule.Title);
        Assert.Single(report.Trace.SelectMany(t => t.Events).OfType<Rejected>());
    }

    [Fact]
    public void Kirik_panelin_parcasi_toplaninca_karede_panel_kalmaz()
    {
        var world = Row();
        var report = ProgramRun.Execute("for i in range(3):\n    move(East)\ncollect()\nprint(panel_power())\nrepair()\n", world);
        Assert.Equal(1, world.CollectedCount);
        Assert.Equal("0\n", report.Output);
        Assert.Equal("Burada panel yok", report.Rule.Title);
    }

    [Fact]
    public void Gorev_catlak_panel_kalinca_bitmez()
    {
        var report = ProgramRun.Execute("for i in range(3):\n    move(East)\ncollect()\n", Row());
        Assert.False(report.Complete);
        report = ProgramRun.Execute(
            "for i in range(4):\n    move(East)\n    if panel_power() == 0:\n        collect()\n    elif panel_power() < 50:\n        repair()\n", Row());
        Assert.True(report.Complete);
    }

    [Fact]
    public void Komutlar_acilmadiysa_oyun_kurali_olarak_durur()
    {
        var closed = new[] { "move", "collect" };
        Assert.Equal("Bu komut henüz açılmadı", ProgramRun.Execute("panel_power()\n", Row(closed)).Rule.Title);
        Assert.Equal("Bu komut henüz açılmadı", ProgramRun.Execute("repair()\n", Row(closed)).Rule.Title);
    }

    [Fact]
    public void Arguman_verilirse_TypeError()
    {
        Assert.Equal("TypeError", ProgramRun.Execute("panel_power(East)\n", Row()).Error.Type);
        Assert.Equal("TypeError", ProgramRun.Execute("repair(1)\n", Row()).Error.Type);
    }

    static string Mini(string row, string solution) => """
        {
          "numara": 21,
          "baslik": "Deneme",
          "gorev": "Onar",
          "harita": ["ROW"],
          "komutlar": ["move", "collect", "panel_power", "repair"],
          "konular": [],
          "ipuclari": ["bir"],
          "cozum": [SOLUTION]
        }
        """.Replace("ROW", row).Replace("SOLUTION", solution);

    [Fact]
    public void Haritada_rakam_panel_gucunun_onda_biri_0_kirik_parca()
    {
        var level = Level.Parse(Mini("R 0 3 8", "\"move(East)\", \"collect()\", \"move(East)\", \"repair()\""));
        Assert.Equal(Collectible.PanelPart, level.Item);
        Assert.Equal(new[] { new Cell(1, 0) }, level.Ices);
        Assert.Equal(0, level.Panels[new Cell(1, 0)]);
        Assert.Equal(30, level.Panels[new Cell(2, 0)]);
        Assert.Equal(80, level.Panels[new Cell(3, 0)]);
        Assert.Empty(LevelCheck.Problems(level));
    }

    [Fact]
    public void Yalnizca_catlak_panel_de_gorev_sayilir()
    {
        var level = Level.Parse(Mini("R . 3", "\"move(East)\", \"move(East)\", \"repair()\""));
        Assert.Empty(level.Ices);
        Assert.Equal(1, level.CreateWorld().CrackedLeft);
        Assert.Empty(LevelCheck.Problems(level));
    }
}
