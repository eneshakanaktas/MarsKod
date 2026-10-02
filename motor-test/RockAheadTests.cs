using MarsKod.Dunya;

namespace MotorTest;

// rock_ahead(yon): "o yonde bir adim ilerlersem dururum mu" (kaya ya da alanin siniri); ilerletmez, yalnizca bakar.
// if/else'in gercek iki dala ayrildigi ilk sensor (Bolum 16-20).
public class RockAheadTests
{
    // Robot (0,0). Doguda (1,0) kaya; kuzey (0,1) bos.
    static World Board(string[] commands = null) => new World(6, 6, new Cell(0, 0),
        System.Array.Empty<Cell>(), new[] { new Cell(1, 0) }, null, commands);

    [Fact]
    public void Onunde_kaya_varsa_True_doner()
    {
        var report = ProgramRun.Execute("print(rock_ahead(East))\n", Board());
        Assert.Equal("True\n", report.Output);
    }

    [Fact]
    public void Onu_aciksa_False_doner()
    {
        var report = ProgramRun.Execute("print(rock_ahead(North))\n", Board());
        Assert.Equal("False\n", report.Output);
    }

    [Fact]
    public void Alanin_siniri_da_engel_sayilir()
    {
        var report = ProgramRun.Execute("print(rock_ahead(South))\n", Board());
        Assert.Equal("True\n", report.Output);
    }

    [Fact]
    public void Robotu_ilerletmez()
    {
        var world = Board();
        ProgramRun.Execute("rock_ahead(East)\nrock_ahead(North)\n", world);
        Assert.Equal(new Cell(0, 0), world.Robot);
    }

    [Fact]
    public void Tarama_olayi_bakilan_karede_yazar()
    {
        var world = Board();
        var report = ProgramRun.Execute("rock_ahead(East)\n", world);
        var scan = Assert.Single(report.Trace.SelectMany(t => t.Events).OfType<Scanned>());
        Assert.Equal(new Cell(1, 0), scan.At);
        Assert.True(scan.Found);
    }

    [Fact]
    public void Arguman_sayisi_yanlissa_TypeError()
    {
        Assert.Equal("TypeError", ProgramRun.Execute("rock_ahead()\n", Board()).Error.Type);
        Assert.Equal("TypeError", ProgramRun.Execute("rock_ahead(East, North)\n", Board()).Error.Type);
    }

    [Fact]
    public void Acilmadiysa_oyun_kurali_olarak_durur()
    {
        var report = ProgramRun.Execute("rock_ahead(East)\n", Board(new[] { "move", "collect" }));
        Assert.Equal("Bu komut henüz açılmadı", report.Rule.Title);
    }

    [Fact]
    public void If_else_ile_farkli_dal_secer()
    {
        // Dogu kapali -> else: kuzeye git. Robot (0,0) -> (0,1).
        var world = Board();
        var report = ProgramRun.Execute(
            "if rock_ahead(East):\n    move(North)\nelse:\n    move(East)\n", world);
        Assert.False(report.Stopped);
        Assert.Equal(new Cell(0, 1), world.Robot);
    }
}
