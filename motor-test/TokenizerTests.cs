using MarsKod.Motor;

namespace MotorTest;

public class TokenizerTests
{
    static string[] Brief(List<Token> tokens) =>
        tokens.Select(t => t.Type == TokenType.OP || t.Type == TokenType.NAME ? t.Value : t.Type.ToString()).ToArray();

    [Fact]
    public void Basit_satiri_parcalara_boler()
    {
        var r = Tokenizer.Tokenize("x = 3 + 4\n");
        Assert.Null(r.Error);
        Assert.Equal(new[] { "x", "=", "NUMBER", "+", "NUMBER", "NEWLINE", "ENDMARKER" }, Brief(r.Tokens));
    }

    [Fact]
    public void Girintiyi_INDENT_DEDENT_olarak_verir()
    {
        var r = Tokenizer.Tokenize("if x:\n    y\n    if z:\n        w\nv\n");
        Assert.Equal(new[]
        {
            "if", "x", ":", "NEWLINE",
            "INDENT", "y", "NEWLINE",
            "if", "z", ":", "NEWLINE",
            "INDENT", "w", "NEWLINE",
            "DEDENT", "DEDENT", "v", "NEWLINE", "ENDMARKER",
        }, Brief(r.Tokens));
    }

    [Fact]
    public void Dosya_sonunda_acik_bloklari_kapatir()
    {
        var r = Tokenizer.Tokenize("for i in r:\n    print(i)");
        Assert.Equal(new[] { ")", "NEWLINE", "DEDENT", "ENDMARKER" }, Brief(r.Tokens).TakeLast(4));
    }

    [Fact]
    public void Bos_satirlar_ve_yorumlar_girintiyi_bozmaz()
    {
        var r = Tokenizer.Tokenize("if x:\n\n    # yorum\n  \n    y\n");
        Assert.Null(r.Error);
        Assert.Equal(new[] { "if", "x", ":", "NEWLINE", "INDENT", "y", "NEWLINE", "DEDENT", "ENDMARKER" }, Brief(r.Tokens));
    }

    [Fact]
    public void Parantez_icindeki_satir_sonlarini_yok_sayar()
    {
        var r = Tokenizer.Tokenize("x = (1,\n     2)\n");
        Assert.Equal(new[] { "x", "=", "(", "NUMBER", ",", "NUMBER", ")", "NEWLINE", "ENDMARKER" }, Brief(r.Tokens));
    }

    [Fact]
    public void Metinlerdeki_kacis_dizilerini_cozer()
    {
        var r = Tokenizer.Tokenize(@"""a\tb\n"" 'c\'d' r""\n"" ""\x41\u00e7""");
        var strings = r.Tokens.Where(t => t.Type == TokenType.STRING).Select(t => (t.Prefix, t.Value)).ToArray();
        Assert.Equal(new[] { ("", "a\tb\n"), ("", "c'd"), ("r", "\\n"), ("", "Aç") }, strings);
    }

    [Fact]
    public void Sayi_turlerini_ayirir()
    {
        var r = Tokenizer.Tokenize("1 1_000 0x1F 1.5 .5 2. 1e3 3j");
        var numbers = r.Tokens.Where(t => t.Type == TokenType.NUMBER).Select(t => (t.Value, t.NumberKind)).ToArray();
        Assert.Equal(new[]
        {
            ("1", NumberKind.Int), ("1_000", NumberKind.Int), ("0x1F", NumberKind.Int), ("1.5", NumberKind.Float),
            (".5", NumberKind.Float), ("2.", NumberKind.Float), ("1e3", NumberKind.Float), ("3j", NumberKind.Imaginary),
        }, numbers);
    }

    [Fact]
    public void Turkce_harfli_isimleri_kabul_eder()
    {
        var r = Tokenizer.Tokenize("çalışan_robot = 1");
        Assert.Null(r.Error);
        Assert.Equal(TokenType.NAME, r.Tokens[0].Type);
        Assert.Equal("çalışan_robot", r.Tokens[0].Value);
    }

    [Fact]
    public void Konum_bilgisini_verir()
    {
        var r = Tokenizer.Tokenize("x = 1\n  \nyaz(\"a\")");
        var yaz = r.Tokens.First(t => t.Value == "yaz");
        Assert.Equal((3, 0, 3), (yaz.Line, yaz.Col, yaz.EndCol));
        var s = r.Tokens.First(t => t.Type == TokenType.STRING);
        Assert.Equal((3, 4, 7), (s.Line, s.Col, s.EndCol));
    }

    // Bu örneklerde hata kelime ayırma aşamasında oluşur; mesaj gerçek Python'la aynı olmalı.
    static readonly string[] TokenizerErrorCases =
    {
        "bastaki-sifir", "fazla-parantez", "gecersiz-sayi", "girinti-uyusmuyor", "gorunmez-bosluk", "ic-ice-kapanmamis",
        "kivrik-tirnak", "parantez-kapanmamis", "parantez-uyusmuyor", "parantez-uyusmuyor-coklu-satir", "sekme-bosluk-karisik",
        "sonra-kapanmamis-parantez", "tirnak-kapanmamis", "uclu-tirnak-kapanmamis",
    };

    static readonly string[] SyntaxErrors = { "SyntaxError", "IndentationError", "TabError" };

    [Theory]
    [MemberData(nameof(Cases.AllData), MemberType = typeof(Cases))]
    public void Gercek_Python_ile_ayni(string id)
    {
        string name = id.Split('/')[1];
        var reference = Cases.Reference(id);
        var error = Tokenizer.Tokenize(File.ReadAllText(Path.Combine(Cases.Dir, id + ".py"))).Error;
        if (TokenizerErrorCases.Contains(name))
        {
            Assert.NotNull(error);
            Assert.Equal(reference.Error.ToString(), new RefError { Type = error.PyType, Message = error.PyMessage, Line = error.Line }.ToString());
        }
        else if (reference.Error == null || !SyntaxErrors.Contains(reference.Error.Type))
        {
            // Yazım hatası olmayan örneklerde kelime aşamasında da hata çıkmamalı.
            // (Cümle aşaması hatalarının son hâli parser testlerinde denetlenir.)
            Assert.Null(error);
        }
    }
}
