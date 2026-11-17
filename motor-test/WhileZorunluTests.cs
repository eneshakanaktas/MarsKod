using MarsKod.Dunya;

namespace MotorTest;

// Yeni yapı kuralı (Enes): while geldiği bölüm (31) ve onu kullanan sonraki bölümde (32) kodda while olmalı.
public class WhileZorunluTests
{
    static Level Load(int number) => Level.Parse(File.ReadAllText(Path.Combine(Dir(), "bolum-" + number + ".json")));

    static string Dir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "oyun", "Assets", "Resources", "Bolumler"))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir!, "oyun", "Assets", "Resources", "Bolumler");
    }

    [Theory]
    [InlineData(31)]
    [InlineData(32)]
    public void While_zorunlu_ve_cozum_gecer(int number)
    {
        var level = Load(number);
        Assert.Equal(new[] { "while" }, level.RequiredConstructs);
        Assert.True(level.Run(level.Solution).Complete);
    }

    [Fact]
    public void Sayarak_yazilan_kod_while_olmadan_gecmez()
    {
        var level = Load(31);
        var counted = string.Join("\n", new[] { "move(East)", "wait()", "wait()", "wait()", "move(East)", "move(East)", "move(East)", "move(East)" });
        var run = level.Run(counted);
        Assert.False(run.Complete);
    }
}
