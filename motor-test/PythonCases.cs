// Motoru gerçek Python ile karşılaştırır.
// Her tests/python-cases/<grup>/<örnek>.py için yanındaki .json, gerçek Python'un sonucudur
// (py -3.12 scripts/python_referans.py ile üretilir). Motor aynı sonucu birebir vermelidir.

using System.Text.Json;
using MarsKod.Motor;

namespace MotorTest;

/// <summary>Örnek dosyalarını bulan yardımcılar</summary>
public static class Cases
{
    public static readonly string Root = FindRoot();

    public static readonly string Dir = Path.Combine(Root, "tests", "python-cases");

    static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "tests", "python-cases"))) dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("tests/python-cases bulunamadı");
    }

    /// <summary>(grup, örnek adı) çiftleri, sıralı</summary>
    public static IEnumerable<(string group, string name)> All()
    {
        foreach (var group in Directory.GetDirectories(Dir).Select(Path.GetFileName).Order(StringComparer.Ordinal))
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Dir, group), "*.py").Order(StringComparer.Ordinal))
                yield return (group, Path.GetFileNameWithoutExtension(file));
        }
    }

    public static IEnumerable<object[]> AllData() => All().Select(c => new object[] { c.group + "/" + c.name });

    public static string Source(string id) =>
        File.ReadAllText(Path.Combine(Dir, id + ".py")).Replace("\r\n", "\n");

    public static Reference Reference(string id) =>
        JsonSerializer.Deserialize<Reference>(File.ReadAllText(Path.Combine(Dir, id + ".json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
}

public sealed class Reference
{
    public string Python { get; set; }
    public string Output { get; set; }
    public RefError Error { get; set; }
}

public sealed class RefError
{
    public string Type { get; set; }
    public string Message { get; set; }
    public int? Line { get; set; }

    public override string ToString() => $"{Type}: {Message} (satır {Line})";
}

public class PythonCases
{
    [Theory]
    [MemberData(nameof(Cases.AllData), MemberType = typeof(Cases))]
    public void Referans_dosyasi_Python_3_12(string id)
    {
        Assert.True(File.Exists(Path.Combine(Cases.Dir, id + ".json")), $"{id} için referans yok. Çalıştır: py -3.12 scripts/python_referans.py");
        Assert.StartsWith("3.12.", Cases.Reference(id).Python);
    }

    [Theory]
    [MemberData(nameof(Cases.AllData), MemberType = typeof(Cases))]
    public void Motor_gercek_Python_ile_ayni(string id)
    {
        var reference = Cases.Reference(id);
        var result = PythonEngine.RunPython(Cases.Source(id));

        Assert.Null(result.Halt);
        Assert.Equal(reference.Error?.ToString(), result.Error == null
            ? null
            : new RefError { Type = result.Error.Type, Message = result.Error.Message, Line = result.Error.Line }.ToString());
        Assert.Equal(reference.Output, result.Output);
    }

    [Fact]
    public void En_az_194_ornek_var()
    {
        Assert.True(Cases.All().Count() >= 194);
    }
}
