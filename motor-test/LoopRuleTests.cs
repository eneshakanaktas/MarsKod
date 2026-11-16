using MarsKod.Dunya;

namespace MotorTest;

// Zorunlu yapı şartı ("zorunlu_yapilar"): görev bitse bile kodda istenen yapı (for, if) yoksa bölüm geçilmez.
public class LoopRuleTests
{
    // Robot (0,0); doğuda (1,0), (2,0), (3,0) toplanacak.
    static World Row() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(2, 0), new Cell(3, 0) },
        null, null, new[] { "move", "collect", "ice_here" });

    const string Unrolled = "move(East)\ncollect()\nmove(East)\ncollect()\nmove(East)\ncollect()\n";
    const string Looped = "for i in range(3):\n    move(East)\n    collect()\n";
    const string WithIf = "move(East)\nif ice_here():\n    collect()\nmove(East)\nif ice_here():\n    collect()\nmove(East)\nif ice_here():\n    collect()\n";
    static readonly string[] ForOnly = { "for" };
    static readonly string[] IfOnly = { "if" };

    static string LevelFile(int n) => File.ReadAllText(Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Bolumler", $"bolum-{n:00}.json"));

    [Fact]
    public void For_istenirken_donguesuz_cozum_gecmez_ve_mesaj_dogru()
    {
        var report = ProgramRun.Execute(Unrolled, Row(), required: ForOnly);
        Assert.False(report.Complete);
        Assert.Equal("Kod bitti, for döngüsü yok", report.RoutineProblem.Title);
        Assert.Contains("`for` döngüsüyle dene", report.RoutineProblem.Text);
    }

    [Fact]
    public void For_istenirken_donguyle_cozum_gecer()
    {
        var report = ProgramRun.Execute(Looped, Row(), required: ForOnly);
        Assert.True(report.Complete);
        Assert.Null(report.RoutineProblem);
    }

    [Fact]
    public void If_istenirken_ifsiz_cozum_gecmez_ve_mesaj_dogru()
    {
        var report = ProgramRun.Execute(Unrolled, Row(), required: IfOnly);
        Assert.False(report.Complete);
        Assert.Equal("Kod bitti, if yok", report.RoutineProblem.Title);
        Assert.Contains("`if` ile önce kontrol et", report.RoutineProblem.Text);
    }

    [Fact]
    public void If_istenirken_ifli_cozum_gecer_ve_for_icindeki_if_sayilir()
    {
        Assert.True(ProgramRun.Execute(WithIf, Row(), required: IfOnly).Complete);
        string nested = "for i in range(3):\n    move(East)\n    if ice_here():\n        collect()\n";
        Assert.True(ProgramRun.Execute(nested, Row(), required: IfOnly).Complete);
        Assert.True(ProgramRun.Execute(nested, Row(), required: new[] { "for", "if" }).Complete);
    }

    [Fact]
    public void If_istenirken_yalniz_for_yetmez_ve_ilk_eksik_soylenir()
    {
        var report = ProgramRun.Execute(Looped, Row(), required: new[] { "for", "if" });
        Assert.False(report.Complete);
        Assert.Equal("Kod bitti, if yok", report.RoutineProblem.Title);
    }

    const string WithElse = "move(East)\nif ice_here():\n    collect()\nelse:\n    move(East)\nmove(East)\nif ice_here():\n    collect()\nmove(East)\ncollect()\n";
    static readonly string[] ElseOnly = { "else" };

    [Fact]
    public void Else_istenirken_elsesiz_cozum_gecmez_ve_mesaj_dogru()
    {
        var report = ProgramRun.Execute(Unrolled, Row(), required: ElseOnly);
        Assert.False(report.Complete);
        Assert.Equal("Kod bitti, else yok", report.RoutineProblem.Title);
        Assert.Contains("`else` ile öteki durumu da yaz", report.RoutineProblem.Text);
    }

    [Fact]
    public void Else_istenirken_yalniz_if_yetmez()
    {
        var report = ProgramRun.Execute(WithIf, Row(), required: ElseOnly);
        Assert.False(report.Complete);
        Assert.Equal("Kod bitti, else yok", report.RoutineProblem.Title);
    }

    [Fact]
    public void Else_istenirken_elseli_cozum_gecer_ve_icteki_else_sayilir()
    {
        Assert.True(ProgramRun.Execute(WithElse, Row(), required: ElseOnly).Complete);
        string nested = "for i in range(3):\n    move(East)\n    if ice_here():\n        collect()\n    else:\n        move(East)\n";
        var report = ProgramRun.Execute(nested, Row(), required: ElseOnly);
        Assert.Null(report.RoutineProblem);
    }

    [Fact]
    public void Sart_yokken_yapisiz_cozum_eskisi_gibi_gecer()
    {
        var report = ProgramRun.Execute(Unrolled, Row());
        Assert.True(report.Complete);
        Assert.Null(report.RoutineProblem);
    }

    [Fact]
    public void Kod_durduysa_yapi_mesaji_yok()
    {
        var report = ProgramRun.Execute("move(East)\nmove(", Row(), required: ForOnly);
        Assert.Null(report.RoutineProblem);
        Assert.False(report.Complete);
    }

    [Fact]
    public void Bilinmeyen_yapi_adi_bolum_dosyasinda_hata_verir()
    {
        string text = LevelFile(3).Replace("\"zorunlu_yapilar\": [\"for\"]", "\"zorunlu_yapilar\": [\"while\"]");
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(text));
        Assert.Contains("while", e.Message);
        Assert.Contains("for, if, else", e.Message);
    }

    [Fact]
    public void Bolum_dosyalarinda_zorunlu_yapilar_beklenen_yerde()
    {
        var expected = new Dictionary<int, string[]>
        {
            [3] = new[] { "for" }, [4] = new[] { "for" }, [12] = new[] { "if" }, [13] = new[] { "if" },
            [16] = new[] { "else" }, [17] = new[] { "else" },
        };
        foreach (int n in new[] { 3, 4, 5, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 })
        {
            var level = Level.Parse(LevelFile(n));
            Assert.Equal(expected.TryGetValue(n, out var names) ? names : new string[0], level.RequiredConstructs);
            Assert.True(level.Run(level.Solution).Complete);
        }
    }
}
