using MarsKod.Dunya;

namespace MotorTest;

// Yeni yapı kuralı (Enes): elif geldiği bölüm (24) ve onu kullanan sonraki bölümde (26) kodda elif olmalı.
public class ElifZorunluTests
{
    static Level Load(int number) => Level.Parse(File.ReadAllText(Path.Combine(Dir(), "bolum-" + number + ".json")));

    static string Dir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "oyun", "Assets", "Resources", "Bolumler"))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir!, "oyun", "Assets", "Resources", "Bolumler");
    }

    [Theory]
    [InlineData(24)]
    [InlineData(26)]
    public void Elif_olmadan_bolum_gecilmez(int number)
    {
        var level = Load(number);
        Assert.Equal(new[] { "elif" }, level.RequiredConstructs);
        Assert.True(level.Run(level.Solution).Complete);
        Assert.Contains("elif", RequiredConstructs.Names);
    }

    [Fact]
    public void Elif_ayristiricida_else_icinde_tek_if_olarak_taninir()
    {
        var used = RequiredConstructs.Used(MarsKod.Motor.Parser.Parse("if a:\n    pass\nelif b:\n    pass\n").Body);
        Assert.Contains("elif", used);
        Assert.DoesNotContain("elif", RequiredConstructs.Used(MarsKod.Motor.Parser.Parse("if a:\n    pass\nelse:\n    pass\n").Body));
    }
}
