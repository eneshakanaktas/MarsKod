using MarsKod.Dunya;

namespace MotorTest;

// Bilgi kartı: yeni bir şey ilk kez göründüğünde oyuncuya açılan kısa açıklama ("bilgi" alanı); i düğmesiyle tekrar açılır.
public class LevelInfoTests
{
    [Fact]
    public void Bilgi_okunur_yoksa_null()
    {
        var level = Level.Parse(LevelJson.Screen(", \"bilgi\": {\"baslik\": \"Soru\", \"metin\": [\"bir\", \"iki\"]}"));
        Assert.Equal("Soru", level.Info.Title);
        Assert.Equal(new[] { "bir", "iki" }, level.Info.Paragraphs);
        Assert.Null(Level.Parse(LevelJson.Screen()).Info);
    }

    [Theory]
    [InlineData("{\"baslik\": \"T\"}", "\"metin\" alanı eksik")]
    [InlineData("{\"baslik\": \"T\", \"metin\": []}", "1-4 paragraf")]
    [InlineData("{\"baslik\": \"T\", \"metin\": [\"a\", \"b\", \"c\", \"d\", \"e\"]}", "1-4 paragraf")]
    [InlineData("{\"baslik\": \"T\", \"metin\": [\"a\", \" \"]}", "boş olamaz")]
    [InlineData("{\"metin\": [\"a\"]}", "\"baslik\" alanı eksik")]
    [InlineData("[]", "bir nesne olmalı")]
    [InlineData("{\"baslik\": \"T\", \"metin\": [\"a\"], \"renk\": 1}", "Bilinmeyen alan")]
    public void Bozuk_bilgi_alani_anlasilir_mesaj_verir(string info, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(LevelJson.Screen(", \"bilgi\": " + info)));
        Assert.Contains(expected, e.Message);
    }
}
