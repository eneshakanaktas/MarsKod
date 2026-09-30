using MarsKod.Dunya;

namespace MotorTest;

// Oneri satiri (Orta) ve bolum dosyasinin "parcalar" / "python_kelimeleri" alanlari (tasarim belgesi §4, §5).
public class SuggestTests
{
    static readonly string[] Open = { "move", "collect", "North", "East", "South", "West", "for", "in", "range" };

    static List<string> Words(string partial, string code = "", string[] open = null)
        => Suggestions.Suggest(partial, code, open ?? Open).Select(s => s.Word).ToList();

    [Fact]
    public void Yarim_kelimeyle_baslayanlar()
    {
        Assert.Equal(new[] { "move" }, Words("mo"));
        Assert.Equal(new[] { "range" }, Words("ra"));
    }

    [Fact]
    public void Buyuk_kucuk_harf_farki_yok_oneri_dogru_yazimla_gelir()
        => Assert.Equal(new[] { "North" }, Words("no"));

    [Fact]
    public void Bos_yarim_kelime_oneri_vermez()
        => Assert.Empty(Words(""));

    [Fact]
    public void Acik_olmayan_kelime_onerilmez()
        => Assert.Empty(Words("wh"));

    [Fact]
    public void En_cok_3_oneri()
        => Assert.Equal(3, Words("a", "a1 = 1\na2 = 2\na3 = 3\na4 = 4").Count);

    [Fact]
    public void Sirasi_acik_kelimeler_sonra_koddaki_isimler()
        => Assert.Equal(new[] { "collect", "counter", "count" }, Words("co", "counter = 0\ncount = 1", new[] { "collect" }));

    [Fact]
    public void Fonksiyonsa_parantez_bilgisi()
    {
        var s = Suggestions.Suggest("mo", "", Open).Single();
        Assert.True(s.IsFunction);
        Assert.False(Suggestions.Suggest("no", "", Open).Single().IsFunction);
        Assert.True(Suggestions.Suggest("ra", "", Open).Single().IsFunction); // range(
        Assert.False(Suggestions.Suggest("fo", "", Open).Single().IsFunction); // for
    }

    [Fact]
    public void Koddaki_isimler_atama_for_ve_def()
    {
        const string code = "sayac = 0\nfor adim in range(3):\n    sayac += 1\ndef yürü():\n    pass";
        Assert.Equal(new[] { "sayac" }, Words("sa", code));
        Assert.Equal(new[] { "adim" }, Words("ad", code));
        var f = Suggestions.Suggest("yü", code, Open).Single();
        Assert.True(f.IsFunction);
    }

    [Fact]
    public void Esitlik_karsilastirmasi_isim_sayilmaz()
        => Assert.Empty(Words("x", "x == 1"));

    [Fact]
    public void Tamamlanmis_degisken_yeniden_onerilmez_ama_fonksiyon_onerilir()
    {
        Assert.Empty(Words("sayac", "sayac = 0"));
        Assert.Equal(new[] { "move" }, Words("move"));
    }

    [Fact]
    public void Acik_kelimeler_onceki_bolumlerle_birlesir()
    {
        var levels = new[]
        {
            Level.Parse(Mini("1", "[\"move\"]", "")),
            Level.Parse(Mini("2", "[\"move\", \"collect\"]", "")),
            Level.Parse(Mini("3", "[\"move\", \"collect\"]", "\"python_kelimeleri\": [\"for\", \"in\", \"range\"],")),
        };
        var one = Suggestions.OpenWords(levels, 1);
        Assert.Contains("move", one);
        Assert.Contains("East", one);
        Assert.DoesNotContain("collect", one);
        Assert.DoesNotContain("for", one);
        var three = Suggestions.OpenWords(levels, 3);
        Assert.Contains("collect", three);
        Assert.Contains("range", three);
        Assert.DoesNotContain("range", Suggestions.OpenWords(levels, 2));
    }

    // ---- bolum dosyasi alanlari ----

    static string Mini(string number, string commands, string extra) => @"{
  ""numara"": " + number + @", ""baslik"": ""x"", ""gorev"": ""y"",
  ""harita"": [""R H . . . ."", "". . . . . ."", "". . . . . ."", "". . . . . ."", "". . . . . ."", "". . . . . .""],
  ""komutlar"": " + commands + @", " + extra + @"
  ""konular"": [""a""], ""ipuclari"": [""i""],
  ""cozum"": [""move(East)""]
}";

    static Level Parse(string extra, string commands = "[\"move\", \"collect\"]") => Level.Parse(Mini("1", commands, extra));

    [Fact]
    public void Parcalar_yoksa_komutlardan_turetilir()
    {
        Assert.Equal(new[] { "move(North)", "move(East)", "move(South)", "move(West)", "collect()" }, Parse("").Pieces);
        Assert.Equal(new[] { "move(North)", "move(East)", "move(South)", "move(West)" }, Parse("", "[\"move\"]").Pieces
            .Where(p => p.StartsWith("move")).ToList());
    }

    [Fact]
    public void Parcalar_yazilmissa_onlar_kullanilir()
    {
        var level = Parse("\"parcalar\": [\"move(East)\", \"for i in range(3):\"], \"python_kelimeleri\": [\"for\", \"range\"],");
        Assert.Equal(new[] { "move(East)", "for i in range(3):" }, level.Pieces);
        Assert.Equal(new[] { "for", "range" }, level.PythonWords);
        Assert.Empty(LevelCheck.Problems(level));
    }

    [Fact]
    public void Bos_parcalar_listesi_hata()
    {
        var e = Assert.Throws<DataFormatError>(() => Parse("\"parcalar\": [],"));
        Assert.Contains("parcalar", e.Message);
    }

    [Theory]
    [InlineData("\"parcalar\": [\"move(East\"],", "geçerli bir Python satırı değil")]
    [InlineData("\"parcalar\": [\"for i in range(3)\"],", "geçerli bir Python satırı değil")]
    [InlineData("\"parcalar\": [\"move(East)\\ncollect()\"],", "tek satırlık")]
    [InlineData("\"python_kelimeleri\": [\"hop\"],", "tanıdığı bir Python kelimesi değil")]
    public void Denetleyici_bozuk_parcalari_yakalar(string extra, string expected)
        => Assert.Contains(expected, Assert.Single(LevelCheck.Problems(Parse(extra))));

    [Fact]
    public void Denetleyici_acik_olmayan_komutu_yakalar()
    {
        var level = Parse("\"parcalar\": [\"move(East)\", \"collect()\"],");
        Assert.Empty(LevelCheck.Problems(level));
        level.Commands.Remove("collect");
        Assert.Contains(LevelCheck.Problems(level), p => p.Contains("açık olmayan \"collect\""));
    }

    [Fact]
    public void Ilk_3_bolumun_parcalari_ve_kelimeleri()
    {
        var dir = Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Bolumler");
        var third = Level.Parse(File.ReadAllText(Path.Combine(dir, "bolum-03.json")));
        Assert.Contains("for i in range(3):", third.Pieces);
        Assert.Equal(new[] { "for", "in", "range" }, third.PythonWords);
        Assert.Empty(Level.Parse(File.ReadAllText(Path.Combine(dir, "bolum-01.json"))).PythonWords);
    }
}
