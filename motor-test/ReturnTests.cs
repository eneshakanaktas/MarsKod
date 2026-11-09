using MarsKod.Dunya;

namespace MotorTest;

// Dönüş şartı (Bölge 8, return): şarta giren rutin cevabını return ile vermeli; değer döndüren rutin bir kez çağrılsa yeter.
public class ReturnTests
{
    // Robot (0,0); toplanacak (0,3): kuzeye 3 adım
    static World Corridor() => new World(6, 6, new Cell(0, 0), new[] { new Cell(0, 3) }, null, null, new[] { "move", "collect" });
    static readonly RoutineRule Koridor = new RoutineRule(new[] { "koridor" }, 0, needsReturn: true);
    const string Walk = "for i in range(koridor()):\n    move(North)\ncollect()\n";

    static RunReport Run(string code, RoutineRule rule = null) => ProgramRun.Execute(code, Corridor(), routines: rule ?? Koridor);

    [Fact]
    public void Return_eden_rutin_bir_kez_calissa_yeter()
    {
        var report = Run("def koridor():\n    return 3\n" + Walk);
        Assert.True(report.Complete);
        Assert.Contains("koridor", report.ReturningRoutines);
    }

    [Fact]
    public void Return_olmayan_rutin_sayilmaz()
    {
        var report = Run("def koridor():\n    move(North)\nfor i in range(3):\n    koridor()\ncollect()\n");
        Assert.False(report.Complete);
        Assert.Equal("Kod bitti, koridor cevap vermiyor", report.RoutineProblem.Title);
        Assert.Contains("`return`", report.RoutineProblem.Text);
    }

    [Fact]
    public void Kosul_icindeki_return_sayilir()
    {
        Assert.True(Run("def koridor():\n    if True:\n        return 3\n    return 0\n" + Walk).Complete);
    }

    [Fact]
    public void Ic_rutinin_return_u_distakine_sayilmaz()
    {
        var report = Run("def koridor():\n    def ic():\n        return 3\n    for i in range(ic()):\n        move(North)\nkoridor()\ncollect()\n");
        Assert.DoesNotContain("koridor", report.ReturningRoutines);
        Assert.Equal("Kod bitti, koridor cevap vermiyor", report.RoutineProblem.Title);
    }

    [Fact]
    public void Bos_return_sayilmaz()
    {
        var report = Run("def koridor():\n    for i in range(3):\n        move(North)\n    return\nkoridor()\ncollect()\n");
        Assert.Equal("Kod bitti, koridor cevap vermiyor", report.RoutineProblem.Title);
    }

    [Fact]
    public void Donus_sarti_yoksa_iki_kez_kurali_aynen_kalir()
    {
        var plain = new RoutineRule(new[] { "koridor" }, 0);
        Assert.Equal(2, plain.RequiredRuns);
        Assert.Contains("yalnızca 1 kez", Run("def koridor():\n    return 3\n" + Walk, plain).RoutineProblem.Title);
        Assert.Equal(1, Koridor.RequiredRuns);
    }
}
