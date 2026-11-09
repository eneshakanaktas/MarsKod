using MarsKod.Dunya;

namespace MotorTest;

// Bölüm dosyasında "izler" (Bölge 7): tek iz sabit yol, 2-3 iz değişen yol. Kod her izde ayrı denenir;
// görev hepsinde bitince biter. Ekranda başarısız iz ya da (hepsi tamamsa) sıradaki iz oynatılır.
public class TrailTests
{
    // İki iz: kuzey-kuzey ya da doğu-doğu
    const string Fork = """
        {
          "numara": 99, "baslik": "Çatal", "gorev": "Serçe'ye sor",
          "harita": [
            ". . . . . .",
            ". . . . . .",
            ". . . . . .",
            ". . . . . .",
            ". . . . . .",
            "R . . . . ."
          ],
          "izler": [["North", "North"], ["East", "East"]],
          "komutlar": ["move", "scout"],
          "konular": ["döngü"],
          "ipuclari": ["Serçe'ye sor."],
          "tipik_hatalar": [ { "kod": ["move(North)", "move(North)"], "aciklama": "Ezber: ikinci izde yanlış." } ],
          "cozum": ["for i in range(2):", "    move(scout())"]
        }
        """;

    static DataFormatError Broken(string from, string to) => Assert.Throws<DataFormatError>(() => Level.Parse(Fork.Replace(from, to)));

    [Fact]
    public void Izler_okunur_hedef_izin_sonu()
    {
        var level = Level.Parse(Fork);
        Assert.Equal(2, level.Trails.Count);
        Assert.Null(level.Target);
        Assert.Equal(new Cell(0, 2), level.CreateWorld(0).Target.Value);
        Assert.Equal(new Cell(2, 0), level.CreateWorld(1).Target.Value);
        Assert.Empty(LevelCheck.Problems(level)); // görev yok hatası vermez, çözüm iki izde de biter
    }

    [Fact]
    public void Ezber_cozum_bir_izde_takilir()
    {
        var level = Level.Parse(Fork);
        var runs = level.RunVersions("move(North)\nmove(North)\n");
        Assert.True(runs[0].Report.Complete);
        Assert.False(runs[1].Report.Complete);
        Assert.False(level.Run("move(North)\nmove(North)\n").Complete);
        Assert.True(TrailRuns.Mixed(runs));
    }

    [Fact]
    public void Basarisiz_iz_secilir_hepsi_tamamsa_sirayla()
    {
        var level = Level.Parse(Fork);
        var bad = level.RunVersions("move(North)\nmove(North)\n");
        Assert.Same(bad[1], TrailRuns.Pick(bad, 0));
        Assert.Same(bad[1], TrailRuns.Pick(bad, 7));
        var good = level.RunVersions(level.Solution);
        Assert.Same(good[0], TrailRuns.Pick(good, 0));
        Assert.Same(good[1], TrailRuns.Pick(good, 1));
        Assert.Same(good[0], TrailRuns.Pick(good, 2));
        Assert.False(TrailRuns.Mixed(good));
    }

    [Fact]
    public void Izsiz_bolum_tek_kosu()
    {
        var level = Level.Parse(Fork.Replace("\"izler\": [[\"North\", \"North\"], [\"East\", \"East\"]],", "").Replace("\"scout\"", "\"collect\"")
            .Replace("\"R . . . . .\"", "\"R . . . . H\""));
        Assert.Single(level.RunVersions("move(East)\n"));
    }

    [Fact]
    public void Scout_acik_izler_yoksa_anlasilir_mesaj_verir()
    {
        // haritada hedef var ki harita denetimi "görev yok" demesin
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Fork.Replace("\"izler\": [[\"North\", \"North\"], [\"East\", \"East\"]],", "")
            .Replace("\"R . . . . .\"", "\"R . . . . H\"")));
        Assert.Contains("\"izler\" yok", e.Message);
    }

    [Theory]
    [InlineData("\"komutlar\": [\"move\", \"scout\"]", "\"komutlar\": [\"move\"]", "\"scout\" yok")]
    [InlineData("[\"East\", \"East\"]", "[\"East\", \"East\", \"East\"]", "aynı uzunlukta")]
    [InlineData("[\"East\", \"East\"]", "[\"West\", \"West\"]", "alanın dışına")]
    [InlineData("[\"East\", \"East\"]", "[\"East\", \"West\"]", "iki kez geçiyor")]
    [InlineData("[\"East\", \"East\"]", "[\"North\", \"North\"]", "aynı iz iki kez")]
    [InlineData("[\"East\", \"East\"]", "[\"East\", \"Doğu\"]", "bilinmeyen yön")]
    [InlineData("\"R . . . . .\"", "\"R K . . . .\"", "kayanın")]
    [InlineData("\"R . . . . .\"", "\"R . . . . H\"", "hedef (H) yazılmaz")]
    [InlineData("[[\"North\", \"North\"], [\"East\", \"East\"]]", "[]", "1-3 iz")]
    public void Bozuk_iz_anlasilir_mesaj_verir(string from, string to, string expected)
    {
        Assert.Contains(expected, Broken(from, to).Message);
    }
}
