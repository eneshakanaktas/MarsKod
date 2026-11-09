using MarsKod.Dunya;

namespace MotorTest;

// Okuyan robot (Bölge 8, tür C): read_note() notun yazısını verir; notlardaki yön harfleri Türkçe (K G D B), V veri kartı.
// 2-3 not = değişen not: kod her notta ayrı denenir, hedef notun yolunun sonudur.
public class NoteTests
{
    const string Map = "\"harita\": [\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". K . . . .\", \"R . . . . .\"]";
    const string Yon = "def yon(harf):\n    if harf == \"K\":\n        return North\n    elif harf == \"G\":\n        return South\n    elif harf == \"D\":\n        return East\n    elif harf == \"B\":\n        return West\n";
    const string Walk = "for harf in read_note():\n    move(yon(harf))\n";

    static Level Notes(string notes, string extra = "") => Level.Parse("{\"numara\": 76, \"baslik\": \"a\", \"gorev\": \"b\", " + Map + ", \"notlar\": " + notes
        + ", \"komutlar\": [\"move\", \"read_note\"]" + extra + ", \"konular\": [\"metin\"], \"ipuclari\": [\"x\"], \"cozum\": [\"for harf in read_note():\", \"    move(harf)\"]}");

    [Fact]
    public void Harfler_turkce_yonlerdir()
    {
        Assert.Equal(Direction.North, NoteLetters.Directions['K']);
        Assert.Equal(Direction.South, NoteLetters.Directions['G']);
        Assert.Equal(Direction.East, NoteLetters.Directions['D']);
        Assert.Equal(Direction.West, NoteLetters.Directions['B']);
        Assert.Equal(new[] { Direction.North, Direction.East }, NoteLetters.Steps("KVD"));
        Assert.Null(NoteLetters.Steps("KX"));
        Assert.True(NoteLetters.IsDirectionLetter("K"));
        Assert.False(NoteLetters.IsDirectionLetter("North"));
    }

    [Fact]
    public void Not_okunur_ve_olay_birakir()
    {
        var world = new World(6, 6, new Cell(0, 0), new Cell[0], commands: new[] { "move", "read_note" }, note: "KD");
        var report = ProgramRun.Execute("yazi = read_note()\n", world);
        Assert.False(report.Stopped);
        Assert.Contains(report.Trace.SelectMany(t => t.Events), e => e is NoteRead n && n.Text == "KD");
        Assert.Contains(report.Trace.SelectMany(t => t.Vars), v => v.Key == "yazi" && v.Value == "'KD'");
    }

    [Fact]
    public void Harf_ve_None_yon_kendi_mesajini_verir()
    {
        var harf = ProgramRun.Execute("move(\"K\")\n", new World(6, 6, new Cell(0, 0), new Cell[0]));
        Assert.Equal("move yön ister, harf değil", harf.Rule.Title);
        Assert.Contains("move(yon(harf))", harf.Rule.Text);
        var none = ProgramRun.Execute("move(None)\n", new World(6, 6, new Cell(0, 0), new Cell[0]));
        Assert.Equal("Yön yerine None geldi", none.Rule.Title);
        Assert.Contains("return", none.Rule.Text);
        Assert.Equal("Bilinmeyen yön", ProgramRun.Execute("move(5)\n", new World(6, 6, new Cell(0, 0), new Cell[0])).Rule.Title);
    }

    [Fact]
    public void Notun_sonu_hedeftir_ve_her_notta_denenir()
    {
        var level = Notes("[\"KKD\", \"DDK\"]");
        Assert.Equal(2, level.VersionCount);
        Assert.Equal(new Cell(1, 2), level.CreateWorld(0).Target);
        Assert.Equal(new Cell(2, 1), level.CreateWorld(1).Target);
        Assert.Null(level.Target);
        Assert.True(level.Run(Yon + Walk).Complete);
        var memorized = level.RunVersions(Yon + "for harf in \"KKD\":\n    move(yon(harf))\n");
        Assert.True(memorized[0].Report.Complete);
        Assert.False(memorized[1].Report.Complete);
        Assert.True(TrailRuns.Mixed(memorized));
    }

    [Fact]
    public void V_harfi_yurumez()
    {
        Assert.Equal(new Cell(0, 2), Notes("[\"KVK\", \"DDK\"]").CreateWorld(0).Target);
    }

    [Theory]
    [InlineData("[\"KX\"]", "", "bilinmeyen harf")]
    [InlineData("[\"BB\"]", "", "alanın dışına")]
    [InlineData("[\"DK\"]", "", "kayanın üstünden")]
    [InlineData("[\"KKDD\", \"DDKK\"]", "", "aynı yere")]
    [InlineData("[\"KK\", \"KK\"]", "", "iki kez")]
    [InlineData("[]", "", "1-3 not")]
    [InlineData("[\"KK\"]", ", \"izler\": [[\"North\"]]", "birlikte")]
    public void Bozuk_notlar_anlasilir_mesaj_verir(string notes, string extra, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Notes(notes, extra));
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Read_note_ile_notlar_birlikte_yazilir()
    {
        var noCommand = Assert.Throws<DataFormatError>(() => Level.Parse("{\"numara\": 76, \"baslik\": \"a\", \"gorev\": \"b\", " + Map + ", \"notlar\": [\"KD\"], \"komutlar\": [\"move\"], \"konular\": [\"x\"], \"ipuclari\": [\"x\"], \"cozum\": [\"move(North)\"]}"));
        Assert.Contains("read_note", noCommand.Message);
        var noNotes = Assert.Throws<DataFormatError>(() => Level.Parse("{\"numara\": 76, \"baslik\": \"a\", \"gorev\": \"b\", " + Map.Replace("R . .", "R H .") + ", \"komutlar\": [\"move\", \"read_note\"], \"konular\": [\"x\"], \"ipuclari\": [\"x\"], \"cozum\": [\"move(East)\"]}"));
        Assert.Contains("\"notlar\" yok", noNotes.Message);
    }
}
