using MarsKod.Dunya;
using MarsKod.Motor;

namespace MotorTest;

// Dünya kuralları ve oyuncu kodunun dünyada çalıştırılması (sahnedeki 1. bölüm düzeni).
public class DunyaTests
{
    const string Level1 = "# Buzları topla\nmove(East)\nfor i in range(3):\n    move(East)\n    collect()\n";

    static World Board() => new World(6, 6, new Cell(0, 2), new[] { new Cell(2, 2), new Cell(3, 2), new Cell(4, 2) });

    [Fact]
    public void Kayit_her_satirdan_sonraki_degiskenleri_tutar()
    {
        var report = ProgramRun.Execute("adim = 'uzun bir metin burada'\nfor i in range(2):\n    move(East)\ndef f():\n    pass\n", Board());
        // satır 1: adim atandı; for satırından sonra i = 0; fonksiyon (f) gösterilmez
        Assert.Equal("adim = 'uzun bir met…", report.Trace[0].VarsText());
        Assert.Equal(new[] { "adim", "i" }, report.Trace[1].Vars.Select(v => v.Key));
        Assert.Equal("0", report.Trace[1].Vars[1].Value);
        Assert.Equal("1", report.Trace[3].Vars[1].Value);
        Assert.DoesNotContain(report.Trace.Last().Vars, v => v.Key == "f");
        Assert.Equal("", ProgramRun.Execute("move(East)\n", Board()).Trace[0].VarsText());
    }

    [Fact]
    public void Bolum_1_cozumu_tum_buzlari_toplar()
    {
        var world = Board();
        var report = ProgramRun.Execute(Level1, world);
        Assert.False(report.Stopped);
        Assert.True(report.Complete);
        Assert.Equal(new Cell(4, 2), world.Robot);
        // satırlar çalıştığı sırayla: 2, sonra döngü 3 kez (3, 4, 5)
        Assert.Equal(new[] { 2, 3, 4, 5, 3, 4, 5, 3, 4, 5, 3 }, report.Trace.Select(t => t.Line));
        Assert.Equal(3, report.Trace.SelectMany(t => t.Events).OfType<Collected>().Count(c => c.IceIndex >= 0));
    }

    [Fact]
    public void Olaylar_calistiklari_satira_yazilir()
    {
        var report = ProgramRun.Execute(Level1, Board());
        Assert.IsType<Moved>(Assert.Single(report.Trace[0].Events));
        Assert.Empty(report.Trace[1].Events); // for satırı
        var collected = Assert.IsType<Collected>(Assert.Single(report.Trace[3].Events));
        Assert.Equal((0, 1), (collected.IceIndex, collected.Total));
    }

    [Fact]
    public void Eksik_toplama_gorevi_bitirmez()
    {
        var report = ProgramRun.Execute("move(East)\nmove(East)\ncollect()\n", Board());
        Assert.False(report.Stopped);
        Assert.False(report.Complete);
    }

    [Fact]
    public void Bos_karede_collect_False_doner()
    {
        var report = ProgramRun.Execute("print(collect())\nmove(East)\nmove(East)\nprint(collect())\nprint(collect())\n", Board());
        Assert.Equal("False\nTrue\nFalse\n", report.Output);
    }

    [Fact]
    public void Alanin_disina_cikmak_oyun_kuralidir_Python_hatasi_degil()
    {
        var world = Board();
        var report = ProgramRun.Execute("move(East)\nwhile True:\n    move(West)\n", world);
        Assert.NotNull(report.Rule);
        Assert.Null(report.Error);
        Assert.Equal(3, report.StopLine);
        Assert.Equal(new Cell(0, 2), world.Robot);
        Assert.IsType<Blocked>(report.Trace.Last().Events.Last());
    }

    [Fact]
    public void Python_hatasi_satiriyla_ve_oneriyle_gelir()
    {
        var report = ProgramRun.Execute("move(East)\ncolect()\n", Board());
        Assert.Equal("NameError", report.Error.Type);
        Assert.Equal("name 'colect' is not defined. Did you mean: 'collect'?", report.Error.Message);
        Assert.Equal(2, report.StopLine);
        Assert.Single(report.Trace[0].Events); // hatadan önceki hareket kayıtta
    }

    [Fact]
    public void Yazim_hatasinda_hic_calismaz()
    {
        var world = Board();
        var report = ProgramRun.Execute("move(East)\nfor i in range(3)\n    collect()\n", world);
        Assert.Equal("SyntaxError", report.Error.Type);
        Assert.Equal(2, report.StopLine);
        Assert.Empty(report.Trace);
        Assert.Equal(new Cell(0, 2), world.Robot);
    }

    [Fact]
    public void Bitmeyen_dongu_adim_sinirinda_durur()
    {
        var report = ProgramRun.Execute("while True:\n    collect()\n", Board(), 500);
        Assert.Equal(("limit", "steps"), (report.Halt.Kind, report.Halt.Reason));
        Assert.NotNull(report.StopLine);
    }

    [Theory]
    [InlineData("move()", "TypeError", "move() takes exactly one argument (0 given)")]
    [InlineData("move(East, 2)", "TypeError", "move() takes exactly one argument (2 given)")]
    [InlineData("collect(1)", "TypeError", "collect() takes no arguments (1 given)")]
    public void Komutlar_yanlis_sayida_degerle_Python_gibi_hata_verir(string code, string type, string message)
    {
        var report = ProgramRun.Execute(code + "\n", Board());
        Assert.Equal((type, message), (report.Error.Type, report.Error.Message));
    }

    [Theory]
    [InlineData("move(5)")]
    [InlineData("move(\"doğu\")")]
    [InlineData("move(\"0\")")]
    public void Bilinmeyen_yon_oyun_kuralidir(string code)
    {
        var report = ProgramRun.Execute(code + "\n", Board());
        Assert.Equal("Bilinmeyen yön", report.Rule.Title);
    }

    [Fact]
    public void Yonler_isim_olarak_kullanilabilir()
    {
        var world = Board();
        var report = ProgramRun.Execute("yonler = [North, East, South, West]\nfor y in yonler:\n    move(y)\nprint(East)\n", world);
        Assert.False(report.Stopped);
        Assert.Equal(new Cell(0, 2), world.Robot);
        Assert.Equal("East\n", report.Output);
    }
}
