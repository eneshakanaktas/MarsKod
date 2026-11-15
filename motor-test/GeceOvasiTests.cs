using MarsKod.Dunya;
using MarsKod.Motor;

namespace MotorTest;

// Gece ovası (Bölge 10, Bölüm 91-100, and/or/not): göktaşı G toplanır; meteorun yeni düştüğü krater U'ya girilmez
// (meteor_ahead bakar); kızgın göktaşı X'in üstünden geçilir ama toplanmaz (smoke_here bakar). "meteorlar": her denemede
// başka yere düşen kraterler; kod her düzende ayrı denenir.
public class GeceOvasiTests
{
    const string Empty = "\". . . . . .\"";

    // Robot (0,0); (2,0) göktaşı, (3,0) kızgın taş; meteorlar: 1. düzende (1,1), 2. düzende (1,0)
    static Level Plain(string meteors = "[[" + "\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". U . . . .\", \". . . . . .\"], ["
        + "\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". U . . . .\"]]", string commands = "\"move\", \"collect\", \"meteor_ahead\", \"smoke_here\"")
        => Level.Parse("{\"numara\": 91, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [" + Empty + ", " + Empty + ", " + Empty + ", " + Empty + ", " + Empty + ", \"R . G X . .\"],"
            + (meteors == null ? "" : " \"meteorlar\": " + meteors + ",") + " \"komutlar\": [" + commands + "], \"konular\": [], \"ipuclari\": [\"x\"], \"cozum\": [\"move(East)\"]}");

    [Fact]
    public void Goktasi_G_ile_yazilir_bolge_10_gece_ovasi()
    {
        Assert.Equal(Collectible.MeteorStone, Collectibles.Marks['G']);
        Assert.Equal("göktaşı", Collectibles.Name(Collectible.MeteorStone));
        Assert.Equal(Region.NightPlain, Regions.Of(91));
        var level = Plain();
        Assert.Equal(Collectible.MeteorStone, level.Item);
        Assert.Equal(new[] { new Cell(3, 0) }, level.HotStones);
        Assert.Equal(2, level.VersionCount);
        Assert.Equal(new[] { new Cell(1, 1) }, level.CratersOf(0));
        Assert.Equal(new[] { new Cell(1, 0) }, level.CratersOf(1));
    }

    [Fact]
    public void Kratere_girilmez_meteor_ahead_bakar()
    {
        var world = Plain().CreateWorld(1);
        Assert.True(world.MeteorAhead(Direction.East));
        Assert.False(world.MeteorAhead(Direction.North));
        Assert.False(world.MeteorAhead(Direction.West));   // alanın dışı krater değil
        var stop = Assert.Throws<GameRuleStop>(() => world.Move(Direction.East));
        Assert.Equal("Önünde yeni bir krater var", stop.Title);
        Assert.Equal(new Cell(0, 0), world.Robot);
        Assert.Contains(world.Events, e => e is Blocked b && b.ByCrater);
    }

    [Fact]
    public void Kizgin_tas_toplanmaz_smoke_here_bakar()
    {
        var world = Plain().CreateWorld(0);
        world.Move(Direction.East);
        world.Move(Direction.East);
        Assert.False(world.SmokeHere());
        Assert.True(world.Collect());
        world.Move(Direction.East);
        Assert.True(world.SmokeHere());
        Assert.Equal("Taş hâlâ kızgın", Assert.Throws<GameRuleStop>(() => world.Collect()).Title);
    }

    [Fact]
    public void Kod_her_meteor_duzeninde_denenir()
    {
        var level = Plain();
        // 1. düzende doğu açık, 2. düzende (1,0) krater: bakmadan yürüyen kod ikincide takılır
        var blind = "move(East)\nmove(East)\ncollect()\n";
        var runs = level.RunVersions(blind);
        Assert.True(runs[0].Report.Complete);
        Assert.False(runs[1].Report.Complete);
        Assert.False(level.Run(blind).Complete);
        var careful = "if meteor_ahead(East):\n    move(North)\n    move(East)\n    move(East)\n    move(South)\nelse:\n    move(East)\n    move(East)\nif not smoke_here():\n    collect()\n";
        Assert.All(level.RunVersions(careful), r => Assert.True(r.Report.Complete));
    }

    [Fact]
    public void And_or_not_motorda_python_gibi()
    {
        var report = ProgramRun.Execute("print(True and False)\nprint(True or False)\nprint(not True)\nprint(False or 0 == 0)\n", new World(6, 6, new Cell(0, 0), new Cell[0]));
        Assert.Equal("False\nTrue\nFalse\nTrue\n", report.Output);
    }

    [Theory]
    [InlineData("[[\". . . . . .\"]]", "haritanın boyunda")]
    [InlineData("[[\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\"]]", "en az bir krater")]
    [InlineData("[[\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . U . . .\"]]", "dolu bir kareye")]
    [InlineData("[[\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". K . . . .\"]]", "yalnızca U")]
    [InlineData("[[\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". U . . . .\"], [\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". U . . . .\"]]", "aynı düzen")]
    public void Yanlis_meteor_duzeni_anlasilir_mesaj_verir(string meteors, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Plain(meteors));
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Meteorlar_meteor_ahead_olmadan_yazilmaz()
    {
        var e = Assert.Throws<DataFormatError>(() => Plain(commands: "\"move\", \"collect\""));
        Assert.Contains("\"meteor_ahead\" yok", e.Message);
    }
}
