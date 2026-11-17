using MarsKod.Dunya;

namespace MotorTest;

// Yeni yapı kuralı (Enes): değişken geldiği bölüm (41) ve onu kullanan sonraki bölümde (42) kodda değişken olmalı.
public class DegiskenZorunluTests
{
    static Level Load(int number) => Level.Parse(File.ReadAllText(Path.Combine(Dir(), "bolum-" + number + ".json")));

    static string Dir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "oyun", "Assets", "Resources", "Bolumler"))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir!, "oyun", "Assets", "Resources", "Bolumler");
    }

    [Theory]
    [InlineData(41)]
    [InlineData(42)]
    public void Degisken_zorunlu_ve_cozum_gecer(int number)
    {
        var level = Load(number);
        Assert.Equal(new[] { "degisken" }, level.RequiredConstructs);
        Assert.True(level.Run(level.Solution).Complete);
    }

    [Fact]
    public void Sayiyi_dogrudan_yazan_kod_gecmez()
    {
        var level = Load(41);
        var run = level.Run("for i in range(4):\n    move(East)\n    collect()\nreport(4)");
        Assert.False(run.Complete);
    }

    [Fact]
    public void Atama_ve_arttirma_degisken_sayilir()
    {
        Assert.Contains("degisken", RequiredConstructs.Used(MarsKod.Motor.Parser.Parse("a = 0\n").Body));
        Assert.Contains("degisken", RequiredConstructs.Used(MarsKod.Motor.Parser.Parse("a += 1\n").Body));
        Assert.DoesNotContain("degisken", RequiredConstructs.Used(MarsKod.Motor.Parser.Parse("move(East)\n").Body));
    }
}
