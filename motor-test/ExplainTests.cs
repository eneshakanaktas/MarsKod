using System.Text.RegularExpressions;
using MarsKod.Motor;

namespace MotorTest;

public class ExplainTests
{
    static string AllTexts(Explanation e) => string.Join(" ", e.Title, e.Text, e.Hint ?? "");

    /// <summary>Örneklerdeki her farklı hata mesajı (bir kez)</summary>
    public static IEnumerable<object[]> KnownErrors() =>
        Cases.All()
            .Select(c => Cases.Reference(c.group + "/" + c.name).Error)
            .Where(e => e != null)
            .GroupBy(e => e.Message)
            .Select(g => new object[] { g.First().Type, g.Key });

    [Theory]
    [MemberData(nameof(KnownErrors))]
    public void Her_bilinen_hatanin_Turkce_aciklamasi_var(string type, string message)
    {
        var e = Explain.ExplainError(new PythonError { Type = type, Message = message, Line = 1 });
        Assert.False(e.Generic, "özel açıklama yok");
        Assert.DoesNotMatch(new Regex(@"\{\d|undefined|null"), AllTexts(e));
    }

    [Fact]
    public void Did_you_mean_onerisini_ipucuna_cevirir()
    {
        var e = Explain.ExplainError(new PythonError { Type = "NameError", Message = "name 'enerj' is not defined. Did you mean: 'enerji'?", Line = 2 });
        Assert.Equal((ErrorCategory.Isim, "Tanımsız isim", "Belki `enerji` yazmak istedin?"), (e.Category, e.Title, e.Hint));
        Assert.Contains("`enerj`", e.Text);
    }

    [Fact]
    public void Unutulmus_modulu_aciklar()
    {
        var e = Explain.ExplainError(new PythonError
        {
            Type = "NameError", Message = "name 'random' is not defined. Did you forget to import 'random'?", Line = 1,
        });
        Assert.Contains("import random", e.Hint);
    }

    [Fact]
    public void Turleri_Turkcelestirir()
    {
        var e = Explain.ExplainError(new PythonError { Type = "TypeError", Message = "unsupported operand type(s) for +: 'int' and 'str'", Line = 1 });
        Assert.Equal("tam sayı (int) ile metin (str) arasında `+` işlemi yapılamaz.", e.Text);
    }

    [Fact]
    public void Eksik_argumanlari_Turkce_baglacla_siralar()
    {
        var e = Explain.ExplainError(new PythonError
        {
            Type = "TypeError", Message = "dis.<locals>.ic() missing 3 required positional arguments: 'a', 'b', and 'c'", Line = 4,
        });
        Assert.Equal("`ic()` fonksiyonu 3 değer daha bekliyor: 'a', 'b' ve 'c'.", e.Text);
    }

    [Fact]
    public void Kivrik_tirnagi_genel_gecersiz_karakterden_ayirir()
    {
        Assert.Equal("Kıvrık tırnak", Explain.ExplainError(new PythonError { Type = "SyntaxError", Message = "invalid character '“' (U+201C)", Line = 1 }).Title);
        Assert.Equal("Geçersiz karakter", Explain.ExplainError(new PythonError { Type = "SyntaxError", Message = "invalid character '€' (U+20AC)", Line = 1 }).Title);
    }

    [Fact]
    public void Kosuldaki_tek_esittiri_kosul_turune_ayirir()
    {
        var e = Explain.ExplainError(new PythonError { Type = "SyntaxError", Message = "invalid syntax. Maybe you meant '==' or ':=' instead of '='?", Line = 2 });
        Assert.Equal(ErrorCategory.Kosul, e.Category);
    }

    [Fact]
    public void Bilinmeyen_mesajda_turune_gore_genel_aciklama_verir()
    {
        var e = Explain.ExplainError(new PythonError { Type = "SyntaxError", Message = "çok garip bir hata", Line = 1 });
        Assert.True(e.Generic);
        Assert.Equal(ErrorCategory.Yazim, e.Category);
    }

    [Fact]
    public void Bitmeyen_dongu()
    {
        var e = Explain.ExplainHalt(new Halt { Kind = "limit", Reason = "steps", Line = 3 });
        Assert.Equal((ErrorCategory.Dongu, "Kod durmuyor"), (e.Category, e.Title));
    }

    public static IEnumerable<object[]> AllFeatures() => ExplanationsTr.FeatureNames.Keys.Select(k => new object[] { k });

    [Theory]
    [MemberData(nameof(AllFeatures))]
    public void Desteklenmeyen_ozellik_aciklamasi(string feature)
    {
        var e = Explain.ExplainHalt(new Halt { Kind = "unsupported", Feature = feature, Detail = "input", Line = 1 });
        Assert.Equal(ErrorCategory.Desteklenmeyen, e.Category);
        Assert.DoesNotMatch(new Regex(@"\{detail\}|undefined"), AllTexts(e));
    }

    [Fact]
    public void Her_desteklenmeyen_ozelligin_adi_var()
    {
        var ids = typeof(Feature).GetFields().Select(f => (string)f.GetValue(null));
        Assert.All(ids, id => Assert.True(ExplanationsTr.FeatureNames.ContainsKey(id), id));
    }
}
