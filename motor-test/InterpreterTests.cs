using MarsKod.Motor;

namespace MotorTest;

public class InterpreterTests
{
    static InterpreterOptions Steps(int maxSteps) => new InterpreterOptions { MaxSteps = maxSteps };

    // --- koruma sınırları ---

    [Fact]
    public void Bitmeyen_donguyu_adim_sinirinda_durdurur()
    {
        var result = PythonEngine.RunPython("x = 0\nwhile True:\n    x += 1\n", Steps(1000));
        Assert.Null(result.Error);
        Assert.Equal(("limit", "steps"), (result.Halt.Kind, result.Halt.Reason));
        Assert.Contains(result.Halt.Line.Value, new[] { 2, 3 });
    }

    [Theory]
    [InlineData("x = 2 ** 100000000")]
    [InlineData("x = \"a\" * 10 ** 12")]
    public void Dev_hesaplari_durdurur(string source)
    {
        var halt = PythonEngine.RunPython(source).Halt;
        Assert.Equal(("limit", "size", 1), (halt.Kind, halt.Reason, halt.Line));
    }

    [Fact]
    public void Yerlesik_fonksiyonlarin_icindeki_uzun_yinelemeyi_de_sayar()
    {
        Assert.Equal("steps", PythonEngine.RunPython("print(sum(range(10 ** 9)))", Steps(1000)).Halt.Reason);
    }

    [Fact]
    public void Durmadan_onceki_ciktiyi_korur()
    {
        var result = PythonEngine.RunPython("print(\"başla\")\nwhile True:\n    pass\n", Steps(100));
        Assert.Equal("başla\n", result.Output);
    }

    [Fact]
    public void Derin_ic_ice_cagri_Python_siniriyla_durur()
    {
        var result = PythonEngine.RunPython("def f(n):\n    return f(n + 1)\nf(0)\n");
        Assert.Equal("RecursionError", result.Error.Type);
    }

    // --- desteklenmeyen özellikler ---

    [Theory]
    [InlineData("x = input()", "builtin", "input")]
    [InlineData("d = {\"a\": 1}", "dict", null)]
    [InlineData("s = \"a\".format()", "method", "str.format")]
    [InlineData("import math", "import", null)]
    [InlineData("print(\"%d\" % 5)", "str-format", null)]
    public void Desteklenmeyen_ozellikler(string source, string feature, string detail)
    {
        var result = PythonEngine.RunPython(source);
        Assert.Null(result.Error);
        Assert.Equal(("unsupported", feature, 1), (result.Halt.Kind, result.Halt.Feature, result.Halt.Line));
        if (detail != null) Assert.Equal(detail, result.Halt.Detail);
    }

    [Fact]
    public void Yazim_hatasi_desteklenmeyen_ozellikten_once_gelir()
    {
        var error = PythonEngine.RunPython("import math\nif x\n").Error;
        Assert.Equal(("SyntaxError", 2), (error.Type, error.Line));
    }

    // --- oyunun dışarıdan verdiği komutlar ---

    [Fact]
    public void Move_gibi_komutlari_cagirir_ve_sonucunu_kullanir()
    {
        var moves = new List<string>();
        var externals = new Dictionary<string, object>
        {
            ["move"] = new PyBuiltin("move", (args, kwargs) =>
            {
                moves.Add(Values.Str(args[0]));
                return null;
            }),
            ["ice_here"] = new PyBuiltin("ice_here", (args, kwargs) => moves.Count == 2),
        };
        var interpreter = new Interpreter(new InterpreterOptions { MaxSteps = 1000, Externals = externals });
        foreach (var _ in interpreter.Run(Parser.Parse("while not ice_here():\n    move(\"north\")\nprint(\"buz bulundu\")\n")))
        {
        }
        Assert.Equal(new[] { "north", "north" }, moves);
        Assert.Equal("buz bulundu\n", interpreter.Output);
    }

    [Fact]
    public void Dis_komut_isimleri_Did_you_mean_onerisine_girer()
    {
        var externals = new Dictionary<string, object> { ["collect"] = new PyBuiltin("collect", (args, kwargs) => null) };
        var interpreter = new Interpreter(new InterpreterOptions { Externals = externals });
        var e = Assert.Throws<PythonException>(() =>
        {
            foreach (var _ in interpreter.Run(Parser.Parse("colect()\n")))
            {
            }
        });
        Assert.Equal("name 'colect' is not defined. Did you mean: 'collect'?", e.PyMessage);
    }

    // --- adım adım çalışma ---

    [Fact]
    public void Her_cumleden_once_satirini_verir()
    {
        var interpreter = new Interpreter(new InterpreterOptions { MaxSteps = 1000 });
        var lines = interpreter.Run(Parser.Parse("x = 1\nfor i in range(2):\n    x += i\nprint(x)\n")).Select(s => s.Line).ToList();
        Assert.Equal(new[] { 1, 2, 3, 2, 3, 2, 4 }, lines);
        Assert.Equal("2\n", interpreter.Output);
    }

    [Fact]
    public void Durakta_o_anki_degiskenleri_gosterir()
    {
        var interpreter = new Interpreter(new InterpreterOptions { MaxSteps = 1000 });
        var snapshots = new List<string>();
        foreach (var step in interpreter.Run(Parser.Parse("enerji = 5\nenerji -= 2\nprint(enerji)\n")))
            snapshots.Add(step.Frame.Vars.TryGetValue("enerji", out var v) ? Values.Str(v) : "-");
        Assert.Equal(new[] { "-", "5", "3" }, snapshots);
    }

    // --- C#'a özgü ayrıntılar (eski motorda JavaScript'in kendisi hallediyordu) ---

    [Theory]
    [InlineData("print(float(10**30))", "1e+30\n")]
    [InlineData("print(float(2**53 + 1))", "9007199254740992.0\n")]
    [InlineData("print(0.1 + 0.2, 1/3, 2.5e-7, 1e16, 123456789.0)", "0.30000000000000004 0.3333333333333333 2.5e-07 1e+16 123456789.0\n")]
    [InlineData("print(-0.0, round(2.675, 2), round(0.5), round(1.5))", "-0.0 2.67 0 2\n")]
    [InlineData("print(10**20 == 1e20, 2**53 + 1 == 2.0**53, 1 < 1.5, -7 // 2, -7 % 3, 7.5 // -2)", "True False True -4 2 -4.0\n")]
    [InlineData("print(\"ß\".upper(), \"İ\".lower() == \"i\\u0307\", \"a b  c \".split(None, 1))", "SS True ['a', 'b  c ']\n")]
    [InlineData("print(\"ab\".replace(\"\", \"-\"), \"aaa\".count(\"a\"), \"çay\".find(\"y\"))", "-a-b- 3 2\n")]
    [InlineData("x = [3, 1, 2]\nx += x\nprint(x, sorted(x, reverse=True))", "[3, 1, 2, 3, 1, 2] [3, 3, 2, 2, 1, 1]\n")]
    public void Sayi_ve_metin_ayrintilari_Python_gibi(string source, string expected)
    {
        var result = PythonEngine.RunPython(source);
        Assert.Null(result.Error);
        Assert.Null(result.Halt);
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public void Siralamada_karisik_turler_Python_mesajini_verir()
    {
        var error = PythonEngine.RunPython("print(sorted([1, 'a']))").Error;
        Assert.Equal("'<' not supported between instances of 'str' and 'int'", error.Message);
    }
}
