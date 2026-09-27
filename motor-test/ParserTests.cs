using System.Numerics;
using MarsKod.Motor;

namespace MotorTest;

public class ParserTests
{
    /// <summary>Ağacı okunabilir kısa bir metne çevirir (testleri kısa tutmak için).</summary>
    static string Show(Expr e)
    {
        switch (e)
        {
            case Name n: return n.Id;
            case Constant c:
                return c.Value switch
                {
                    string s => "\"" + s + "\"",
                    null => "None",
                    bool b => b ? "True" : "False",
                    BigInteger i => i.ToString(),
                    double d => Values.FloatRepr(d),
                    _ => c.Value.ToString(),
                };
            case BinOp b: return $"({Show(b.Left)} {b.Op} {Show(b.Right)})";
            case UnaryOp u: return $"({u.Op} {Show(u.Operand)})";
            case BoolOp b: return "(" + string.Join($" {b.Op} ", b.Values.Select(Show)) + ")";
            case Compare c:
                return $"({Show(c.Left)} " + string.Join(" ", c.Ops.Select((op, i) => $"{op} {Show(c.Comparators[i])}")) + ")";
            case Call c:
                return Show(c.Func) + "(" + string.Join(", ", c.Args.Select(Show).Concat(c.Keywords.Select(k => $"{k.Name}={Show(k.Value)}"))) + ")";
            case MarsKod.Motor.Attribute a: return $"{Show(a.Value)}.{a.Attr}";
            case Subscript s: return $"{Show(s.Value)}[{Show(s.Index)}]";
            case Slice s:
            {
                var parts = new List<Expr> { s.Lower, s.Upper };
                if (s.Step != null) parts.Add(s.Step);
                return string.Join(":", parts.Select(x => x != null ? Show(x) : ""));
            }
            case ListExpr l: return "[" + string.Join(", ", l.Elts.Select(Show)) + "]";
            case TupleExpr t: return "tuple(" + string.Join(", ", t.Elts.Select(Show)) + ")";
            case IfExp i: return $"({Show(i.Body)} if {Show(i.Test)} else {Show(i.OrElse)})";
            case DictExpr d: return "{" + string.Join(", ", d.Keys.Select((k, i) => $"{Show(k)}: {Show(d.Values[i])}")) + "}";
            case SetExpr s: return "set{" + string.Join(", ", s.Elts.Select(Show)) + "}";
        }
        throw new InvalidOperationException(e.GetType().Name);
    }

    static string ExprOf(string source) => Show(((ExprStmt)Parser.Parse(source).Body[0]).Value);

    static string ErrorOf(string source)
    {
        try
        {
            Parser.Parse(source);
        }
        catch (PythonException e)
        {
            return new RefError { Type = e.PyType, Message = e.PyMessage, Line = e.Line }.ToString();
        }
        catch (UnsupportedFeature e)
        {
            return "unsupported " + e.FeatureId + " @" + e.Line;
        }
        return null;
    }

    [Theory]
    [InlineData("1 + 2 * 3", "(1 + (2 * 3))")]
    [InlineData("(1 + 2) * 3", "((1 + 2) * 3)")]
    [InlineData("-2 ** 2", "(- (2 ** 2))")]
    [InlineData("2 ** 3 ** 2", "(2 ** (3 ** 2))")]
    [InlineData("10 - 3 - 2", "((10 - 3) - 2)")]
    [InlineData("not a and b or c", "(((not a) and b) or c)")]
    [InlineData("1 < x <= 10", "(1 < x <= 10)")]
    [InlineData("x not in liste", "(x not in liste)")]
    [InlineData("x is not None", "(x is not None)")]
    [InlineData("a if k else b", "(a if k else b)")]
    [InlineData("f(1, x=2)", "f(1, x=2)")]
    [InlineData("robot.move()", "robot.move()")]
    [InlineData("a[1:3]", "a[1:3]")]
    [InlineData("a[::2]", "a[::2]")]
    [InlineData("[1, 2, 3][0]", "[1, 2, 3][0]")]
    [InlineData("\"a\" \"b\"", "\"ab\"")]
    [InlineData("{\"buz\": 1}", "{\"buz\": 1}")]
    [InlineData("{1, 2}", "set{1, 2}")]
    [InlineData("()", "tuple()")]
    [InlineData("(1,)", "tuple(1)")]
    public void Islem_onceligi(string source, string expected)
    {
        Assert.Equal(expected, ExprOf(source));
    }

    [Fact]
    public void Buyuk_tam_sayilari_kaybetmeden_okur()
    {
        var c = (Constant)((ExprStmt)Parser.Parse("12345678901234567890123").Body[0]).Value;
        Assert.Equal(BigInteger.Parse("12345678901234567890123"), c.Value);
    }

    [Fact]
    public void Atamalari_ayirir()
    {
        var body = Parser.Parse("x = 1\na = b = 2\nx += 1").Body;
        var a = Assert.IsType<Assign>(body[0]);
        Assert.Equal("x", ((Name)a.Targets.Single()).Id);
        var b = Assert.IsType<Assign>(body[1]);
        Assert.Equal(new[] { "a", "b" }, b.Targets.Select(t => ((Name)t).Id));
        var c = Assert.IsType<AugAssign>(body[2]);
        Assert.Equal(("+", "x"), (c.Op, ((Name)c.Target).Id));
    }

    [Fact]
    public void If_elif_else_zincirini_kurar()
    {
        var s = Assert.IsType<If>(Parser.Parse("if a:\n    x = 1\nelif b:\n    x = 2\nelse:\n    x = 3\n").Body.Single());
        Assert.Equal(1, s.Line);
        var elif = Assert.IsType<If>(s.OrElse.Single());
        Assert.Equal(3, elif.Line);
        var last = Assert.IsType<Assign>(elif.OrElse.Single());
        Assert.Equal(6, last.Line);
    }

    [Fact]
    public void For_while_ve_def_yapilarini_kurar()
    {
        var f = Assert.IsType<FunctionDef>(Parser.Parse("def f(a, b=2):\n    for i in range(a):\n        while i:\n            i -= 1\n    return b\n").Body[0]);
        Assert.Equal("f", f.Name);
        Assert.Equal(new[] { "a", "b" }, f.Params.Select(p => p.Name));
        Assert.Null(f.Params[0].Default);
        Assert.NotNull(f.Params[1].Default);
        var loop = Assert.IsType<For>(f.Body[0]);
        Assert.Equal("i", ((Name)loop.Target).Id);
        Assert.IsType<While>(loop.Body.Single());
        Assert.IsType<Return>(f.Body[1]);
    }

    [Fact]
    public void Tek_satirlik_bloklari_kabul_eder()
    {
        var s = Assert.IsType<If>(Parser.Parse("if x: y = 1; z = 2\n").Body.Single());
        Assert.All(s.Body, b => Assert.IsType<Assign>(b));
        Assert.Equal(2, s.Body.Count);
    }

    [Theory]
    [InlineData("import math", "import")]
    [InlineData("class Robot:\n    pass", "class")]
    [InlineData("x = [i for i in range(3)]", "comprehension")]
    [InlineData("print(f\"{x}\")", "f-string")]
    [InlineData("f = lambda x: x", "lambda")]
    [InlineData("try:\n    pass\nexcept:\n    pass", "try")]
    public void Desteklenmeyen_ozellikler(string source, string feature)
    {
        Assert.StartsWith("unsupported " + feature + " ", ErrorOf(source));
    }

    static readonly string[] SyntaxErrors = { "SyntaxError", "IndentationError", "TabError" };

    [Theory]
    [MemberData(nameof(Cases.AllData), MemberType = typeof(Cases))]
    public void Gercek_Python_ile_ayni(string id)
    {
        var reference = Cases.Reference(id);
        string expected = reference.Error != null && SyntaxErrors.Contains(reference.Error.Type) ? reference.Error.ToString() : null;
        string actual = ErrorOf(File.ReadAllText(Path.Combine(Cases.Dir, id + ".py")));
        Assert.Equal(expected, actual);
    }
}
