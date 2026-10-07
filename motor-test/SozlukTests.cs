using MarsKod.Dunya;

namespace MotorTest;

// Kod sozlugu (oyun/Assets/Resources/Sozluk/sozluk.json): dosya okunur, bolumlerle uyumlu, ornekler gecerli Python.
public class SozlukTests
{
    static readonly string File = Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Sozluk", "sozluk.json");
    static readonly string LevelDir = Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Bolumler");

    static List<Level> Levels() =>
        Directory.GetFiles(LevelDir, "*.json").Select(f => Level.Parse(System.IO.File.ReadAllText(f))).ToList();

    [Fact]
    public void Sozluk_dosyasi_bolumlerle_uyumlu()
    {
        var glossary = Glossary.Parse(System.IO.File.ReadAllText(File));
        Assert.Empty(GlossaryCheck.Problems(glossary, Levels()));
    }

    [Fact]
    public void Sayfa_kelimesinin_ilk_acildigi_bolumde_acilir()
    {
        var glossary = Glossary.Parse(System.IO.File.ReadAllText(File));
        var levels = Levels();
        int? Opens(string title) => Glossary.OpensAt(glossary.Pages.Single(p => p.Title == title), levels);
        Assert.Equal(1, Opens("move"));
        Assert.Equal(1, Opens("Yönler"));
        Assert.Equal(2, Opens("collect"));
        Assert.Equal(3, Opens("for"));
    }

    const string Page = "{ \"baslik\": \"move\", \"kelimeler\": [\"move\"], \"aciklama\": \"yürür\", \"ornek\": [\"move(East)\"] }";

    [Fact]
    public void Eksik_sayfa_ve_bozuk_ornek_bulunur()
    {
        var glossary = Glossary.Parse("{ \"sayfalar\": [" + Page.Replace("move(East)", "move(East") + "] }");
        var problems = GlossaryCheck.Problems(glossary, Levels());
        Assert.Contains(problems, p => p.Contains("\"collect\"") && p.Contains("sayfası yok"));
        Assert.Contains(problems, p => p.Contains("örneği geçerli bir Python kodu değil"));
    }

    [Fact]
    public void Hicbir_bolumde_acilmayan_sayfa_bulunur()
    {
        var glossary = Glossary.Parse("{ \"sayfalar\": [" + Page.Replace("[\"move\"]", "[\"lambda\"]") + "] }");
        Assert.Contains(GlossaryCheck.Problems(glossary, Levels()), p => p.Contains("hiçbir bölümde açılmıyor"));
    }

    [Theory]
    [InlineData("{ \"sayfalar\": [] }", "en az bir sayfası")]
    [InlineData("{ \"sayfa\": [] }", "Bilinmeyen alan")]
    [InlineData("{ \"sayfalar\": [{ \"baslik\": \"x\", \"kelimeler\": [\"move\"], \"aciklama\": \"y\" }] }", "1. sayfa: \"ornek\" alanı eksik")]
    [InlineData("{ \"sayfalar\": [{ \"baslik\": \"x\", \"kelimeler\": [], \"aciklama\": \"y\", \"ornek\": [] }] }", "\"kelimeler\" boş olamaz")]
    public void Bozuk_dosya_anlasilir_hata_verir(string json, string message)
    {
        var e = Assert.Throws<DataFormatError>(() => Glossary.Parse(json));
        Assert.Contains(message, e.Message);
    }
}
