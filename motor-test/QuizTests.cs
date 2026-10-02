using MarsKod.Dunya;

namespace MotorTest;

// Mini sınav dosyaları (oyun/Assets/Resources/Sinavlar): her biri bir bölüme bağlı, dosya okunur ve denetlenir.
public class QuizTests
{
    static readonly string Dir = Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Sinavlar");
    static readonly string LevelDir = Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Bolumler");

    static List<Level> Levels() =>
        Directory.GetFiles(LevelDir, "*.json").Select(f => Level.Parse(File.ReadAllText(f))).ToList();

    public static IEnumerable<object[]> Files() =>
        Directory.GetFiles(Dir, "*.json").Order(StringComparer.Ordinal).Select(f => new object[] { Path.GetFileName(f) });

    static Quiz Load(string file) => Quiz.Parse(File.ReadAllText(Path.Combine(Dir, file)));

    [Theory]
    [MemberData(nameof(Files))]
    public void Sinav_denetimden_gecer(string file)
    {
        var quiz = Load(file);
        Assert.Empty(QuizCheck.Problems(quiz, Levels()));
        // dosya adı bolum numarasıyla uyumlu: sinav-05.json -> 5
        Assert.Equal("sinav-" + quiz.AfterLevel.ToString("00") + ".json", file);
    }

    [Fact]
    public void Her_bes_bolumde_bir_sinav_var()
    {
        var afterLevels = Files().Select(f => Load((string)f[0]).AfterLevel).OrderBy(n => n).ToList();
        var expected = Levels().Select(l => l.Number).Where(n => n % 5 == 0).OrderBy(n => n).ToList();
        Assert.Equal(expected, afterLevels);
    }

    const string Mini = """
        { "bolum": 5, "sorular": [
            { "soru": "move(East) hangi yöne götürür?", "secenekler": ["Kuzeye", "Doğuya"], "dogru": 1, "aciklama": "East doğu demektir." }
        ] }
        """;

    [Fact]
    public void Gecerli_dosya_okunur()
    {
        var quiz = Quiz.Parse(Mini);
        Assert.Equal(5, quiz.AfterLevel);
        var q = Assert.Single(quiz.Questions);
        Assert.Equal(1, q.Correct);
        Assert.Equal(2, q.Choices.Count);
    }

    [Theory]
    [InlineData("\"bolum\": 5,", "", "\"bolum\" alanı eksik")]
    [InlineData("\"sorular\": [", "\"sorular\": [], \"x\": [", "Bilinmeyen alan")]
    [InlineData("\"dogru\": 1,", "\"dogru\": 7,", "seçeneklerden birinin sırası")]
    [InlineData("[\"Kuzeye\", \"Doğuya\"]", "[\"Kuzeye\"]", "en az 2 seçenek")]
    public void Bozuk_dosya_anlasilir_hata_verir(string from, string to, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Quiz.Parse(Mini.Replace(from, to)));
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Bos_sorular_listesi_reddedilir()
    {
        var e = Assert.Throws<DataFormatError>(() => Quiz.Parse("{ \"bolum\": 5, \"sorular\": [] }"));
        Assert.Contains("en az bir soru", e.Message);
    }

    [Fact]
    public void Olmayan_boluma_bagli_sinav_yakalanir()
    {
        var quiz = Quiz.Parse(Mini.Replace("\"bolum\": 5", "\"bolum\": 999"));
        Assert.Contains(QuizCheck.Problems(quiz, Levels()), p => p.Contains("999") && p.Contains("hiçbir bölüm"));
    }
}
