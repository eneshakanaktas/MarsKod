using System.Numerics;
using MarsKod.Dunya;

namespace MotorTest;

// Ekran bulmacası (Bölge 8, tür B): robot yürümez; oyun oyuncunun fonksiyonunu her satırın girdisiyle çağırır, cevabı kaydeder.
public class ScreenPuzzleTests
{
    static ScreenPuzzle Double() => ScreenPuzzle.Parse(Json.Parse(
        "{\"fonksiyon\": \"iki_kat\", \"satirlar\": [{\"girdi\": 3, \"cevap\": 6, \"gorunur\": true}, {\"girdi\": 7, \"cevap\": 14}, {\"girdi\": 0, \"cevap\": 0}]}"));
    static ScreenPuzzle Contains() => ScreenPuzzle.Parse(Json.Parse(
        "{\"fonksiyon\": \"aras_mi\", \"satirlar\": [{\"girdi\": \"Defne Aras\", \"cevap\": true, \"gorunur\": true}, {\"girdi\": \"Kerem Yalın\", \"cevap\": false}, {\"girdi\": \"Ece Aras\", \"cevap\": true}]}"));
    static World Still() => new World(6, 6, new Cell(2, 1), new Cell[0], commands: new string[0]);
    static RunReport Run(string code, ScreenPuzzle screen = null) => ProgramRun.Execute(code, Still(), screen: screen ?? Double());

    [Fact]
    public void Dogru_fonksiyon_her_satiri_dogru_yanitlar()
    {
        var report = Run("def iki_kat(sayi):\n    return sayi * 2\n");
        Assert.True(report.Complete);
        Assert.Equal(3, report.ScreenAnswers.Count);
        Assert.All(report.ScreenAnswers, a => Assert.True(a.Correct));
        Assert.Equal(new BigInteger(14), report.ScreenAnswers[1].Actual);
        Assert.Equal(3, report.RoutineRuns["iki_kat"]);   // ekranın çağrıları rutin sayımına girer
    }

    [Fact]
    public void Print_eden_fonksiyon_None_mesaji_alir_ve_butun_satirlar_sorulur()
    {
        var report = Run("def iki_kat(sayi):\n    print(sayi * 2)\n");
        Assert.False(report.Complete);
        Assert.Equal(3, report.ScreenAnswers.Count);
        Assert.Contains("`None` geldi", report.ScreenProblem.Text);
        Assert.Contains("`return`", report.ScreenProblem.Text);
        Assert.Equal("6\n14\n0\n", report.Output);
    }

    [Fact]
    public void Ezber_gizli_satirda_takilir_ve_bunu_soyler()
    {
        var report = Run("def iki_kat(sayi):\n    if sayi == 3:\n        return 6\n");
        Assert.True(report.ScreenAnswers[0].Correct);
        Assert.False(report.ScreenAnswers[1].Correct);
        Assert.Contains("görev kartında yoktu", report.ScreenProblem.Text);
    }

    [Fact]
    public void Fonksiyon_yoksa_anlasilir_mesaj()
    {
        var report = Run("iki_kat = 5\n");
        Assert.False(report.Complete);
        Assert.Empty(report.ScreenAnswers);
        Assert.Equal("Ekran: `iki_kat` fonksiyonu yok", report.ScreenProblem.Title);
    }

    [Fact]
    public void Tur_farki_ayri_soylenir()
    {
        var report = Run("def iki_kat(sayi):\n    return str(sayi * 2)\n");
        Assert.Contains("Tırnaklı", report.ScreenProblem.Text);
    }

    [Fact]
    public void Cevap_turuyle_karsilastirilir()
    {
        Assert.True(ScreenPuzzle.SameAnswer(true, true));
        Assert.False(ScreenPuzzle.SameAnswer(true, BigInteger.One));
        Assert.False(ScreenPuzzle.SameAnswer(new BigInteger(6), "6"));
        Assert.False(ScreenPuzzle.SameAnswer(false, null));
        Assert.True(ScreenPuzzle.SameAnswer("Aras", "Aras"));
    }

    [Fact]
    public void Metin_girdisiyle_in_calisir()
    {
        var report = Run("def aras_mi(satir):\n    return \"Aras\" in satir\n", Contains());
        Assert.True(report.Complete);
        Assert.Equal(new object[] { true, false, true }, report.ScreenAnswers.Select(a => a.Actual).ToArray());
    }

    [Fact]
    public void Cagri_hatasi_def_satirinda_gorunur()
    {
        var report = Run("x = 1\n\ndef iki_kat():\n    return 6\n");
        Assert.Equal("TypeError", report.Error.Type);
        Assert.Equal(3, report.StopLine);
    }

    [Fact]
    public void Fonksiyon_icindeki_hata_kendi_satirinda()
    {
        var report = Run("def iki_kat(sayi):\n    return sayi * 2 + y\n");
        Assert.Equal("NameError", report.Error.Type);
        Assert.Equal(2, report.StopLine);
    }

    [Fact]
    public void Bitmeyen_dongu_durdurulur()
    {
        var report = Run("def iki_kat(sayi):\n    while True:\n        pass\n");
        Assert.Equal("steps", report.Halt.Reason);
    }

    [Fact]
    public void Fonksiyonun_satirlari_kayda_girer_degiskenler_gizli_ad_tasimaz()
    {
        var report = Run("def iki_kat(sayi):\n    return sayi * 2\n");
        Assert.Equal(3, report.Trace.Count(t => t.Line == 2));
        Assert.DoesNotContain(report.Trace.SelectMany(t => t.Vars), v => v.Key.StartsWith("__"));
    }

    [Fact]
    public void Komut_acik_degilse_ekran_mesaji()
    {
        var report = Run("move(North)\n");
        Assert.Equal("Bu bölümde robotun komutu yok", report.Rule.Title);
    }

    [Theory]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 1, \"cevap\": 2, \"gorunur\": true}, {\"girdi\": 2, \"cevap\": 4}]}", "en az 3 satır")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 1, \"cevap\": 2}, {\"girdi\": 2, \"cevap\": 4}, {\"girdi\": 3, \"cevap\": 6}]}", "görünür")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 1, \"cevap\": 2, \"gorunur\": true}, {\"girdi\": 2, \"cevap\": 4, \"gorunur\": true}, {\"girdi\": 3, \"cevap\": 6, \"gorunur\": true}]}", "gizli")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 1, \"cevap\": true, \"gorunur\": true}, {\"girdi\": 2, \"cevap\": true}, {\"girdi\": 3, \"cevap\": true}]}", "hepsi aynı")]
    [InlineData("{\"fonksiyon\": \"move\", \"satirlar\": []}", "oyun komutu")]
    [InlineData("{\"fonksiyon\": \"return\", \"satirlar\": []}", "geçerli bir fonksiyon adı")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [{\"girdi\": 2.5, \"cevap\": 5, \"gorunur\": true}, {\"girdi\": 2, \"cevap\": 4}, {\"girdi\": 3, \"cevap\": 6}]}", "tam sayı")]
    [InlineData("{\"fonksiyon\": \"f\", \"satirlar\": [], \"renk\": 1}", "Bilinmeyen alan")]
    public void Bozuk_ekran_alani_anlasilir_mesaj_verir(string json, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => ScreenPuzzle.Parse(Json.Parse(json)));
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Ekranli_bolum_okunur_haritada_gorev_aranmaz()
    {
        var level = Level.Parse(LevelJson.Screen());
        Assert.Equal("iki_kat", level.Screen.Function);
        Assert.Single(level.Screen.VisibleRows);
        Assert.True(level.Run(level.Solution).Complete);
        Assert.False(level.Run("def iki_kat(sayi):\n    return 6\n").Complete);
    }
}
