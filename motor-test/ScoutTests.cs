using MarsKod.Dunya;

namespace MotorTest;

// Serçe ve iz (Bölge 7): scout() robotun durduğu karede izin sonraki yönünü söyler; hedef izin son karesidir.
public class ScoutTests
{
    // Robot (0,0); iz: kuzey, kuzey, doğu -> son kare (1,2)
    static readonly Direction[] Steps = { Direction.North, Direction.North, Direction.East };
    static World WithTrail() => new World(6, 6, new Cell(0, 0), new Cell[0], null, null, new[] { "move", "scout" },
        trail: new Trail(new Cell(0, 0), Steps));

    [Fact]
    public void Iz_kareleri_ve_sonu()
    {
        var trail = new Trail(new Cell(0, 0), Steps);
        Assert.Equal(new[] { new Cell(0, 0), new Cell(0, 1), new Cell(0, 2), new Cell(1, 2) }, trail.Cells);
        Assert.Equal(new Cell(1, 2), trail.End);
        Assert.Equal(2, trail.IndexOf(new Cell(0, 2)));
        Assert.Equal(-1, trail.IndexOf(new Cell(3, 3)));
        Assert.False(trail.CrossesItself);
        Assert.True(new Trail(new Cell(0, 0), new[] { Direction.North, Direction.East, Direction.South, Direction.West }).CrossesItself);
    }

    [Fact]
    public void Scout_ile_iz_sonuna_varilir_hedef_izin_sonu()
    {
        var world = WithTrail();
        var report = ProgramRun.Execute("for i in range(3):\n    move(scout())\n", world);
        Assert.True(report.Complete);
        Assert.Equal(new Cell(1, 2), world.Target.Value);
        Assert.Contains(report.Trace, e => e.Events.Exists(ev => ev is Scouted s && s.Direction == Direction.East && s.At.Equals(new Cell(0, 2))));
    }

    [Fact]
    public void Scout_yonu_metin_olarak_dondurur()
    {
        var report = ProgramRun.Execute("yon = scout()\nprint(yon)\n", WithTrail());
        Assert.Equal("North\n", report.Output);
    }

    [Fact]
    public void Iz_disinda_scout_oyun_kuralidir()
    {
        var report = ProgramRun.Execute("move(East)\nscout()\n", WithTrail());
        Assert.Equal("Serçe izi göremiyor", report.Rule.Title);
        Assert.Equal(2, report.StopLine);
    }

    [Fact]
    public void Iz_sonunda_scout_oyun_kuralidir()
    {
        var report = ProgramRun.Execute("for i in range(4):\n    move(scout())\n", WithTrail());
        Assert.Equal("İz burada bitiyor", report.Rule.Title);
    }

    [Fact]
    public void Izin_sonuna_varmayan_kod_tamam_degil()
    {
        var report = ProgramRun.Execute("for i in range(2):\n    move(scout())\n", WithTrail());
        Assert.False(report.Stopped);
        Assert.False(report.Complete);
    }

    [Fact]
    public void Scout_bilgi_almaz()
    {
        var report = ProgramRun.Execute("scout(East)\n", WithTrail());
        Assert.Equal("TypeError", report.Error.Type);
    }
}
