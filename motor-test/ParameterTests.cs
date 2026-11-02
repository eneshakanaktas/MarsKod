using MarsKod.Dunya;

namespace MotorTest;

// Parametre şartı (Bölge 7, Bölüm 61-70, parametreli fonksiyonlar): rutin bir bilgi kutusu (parametre) almalı ve
// en az iki farklı bilgiyle çağrılmalı; yoksa robot işi bitirse bile görev tamam sayılmaz.
public class ParameterTests
{
    // Robot (0,0); toplanacaklar (1,0) ve (1,1): önce doğu, sonra kuzey
    static World Corner() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(1, 1) }, null, null, new[] { "move", "collect" });
    // Robot (0,0); toplanacaklar (1,0) ve (2,0): iki kez doğu
    static World Row() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(2, 0) }, null, null, new[] { "move", "collect" });
    // Robot (0,0); toplanacaklar (1,0) ve (3,0): bir, sonra iki adım doğu
    static World Gap() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(3, 0) }, null, null, new[] { "move", "collect" });

    static readonly RoutineRule Git = new RoutineRule(new[] { "git" }, 0, needsParameters: true);

    const string GitYon = "def git(yon):\n    move(yon)\n    collect()\n";
    const string GitKac = "def git(yon, kac=1):\n    for i in range(kac):\n        move(yon)\n    collect()\n";

    [Fact]
    public void Iki_farkli_yonle_cagrilan_rutin_sarti_saglar()
    {
        var report = ProgramRun.Execute(GitYon + "git(East)\ngit(North)\n", Corner(), routines: Git);
        Assert.True(report.Complete);
        Assert.Null(report.RoutineProblem);
        Assert.Equal(1, report.RoutineParameters["git"]);
        Assert.Equal(new[] { "East", "North" }, report.RoutineArguments["git"]);
    }

    [Fact]
    public void Parametresiz_rutin_sayilmaz()
    {
        var report = ProgramRun.Execute("def git():\n    collect()\nmove(East)\ngit()\nmove(North)\ngit()\n", Corner(), routines: Git);
        Assert.False(report.Complete);
        Assert.Contains("parametresiz", report.RoutineProblem.Title);
        Assert.Contains("def git(yon):", report.RoutineProblem.Text);
    }

    [Fact]
    public void Hep_ayni_bilgiyle_cagri_sayilmaz()
    {
        var world = Row();
        var report = ProgramRun.Execute(GitYon + "git(East)\ngit(East)\n", world, routines: Git);
        Assert.True(world.Complete);
        Assert.False(report.Complete);
        Assert.Contains("hep aynı", report.RoutineProblem.Title);
        Assert.Contains("`East`", report.RoutineProblem.Text);
    }

    [Fact]
    public void Varsayilan_deger_bilgiye_dahildir()
    {
        var same = ProgramRun.Execute(GitKac + "git(East)\ngit(East, 1)\n", Row(), routines: Git);
        Assert.Contains("hep aynı", same.RoutineProblem.Title); // kac=1 yazılmasa da 1
        var different = ProgramRun.Execute(GitKac + "git(East)\ngit(East, 2)\n", Gap(), routines: Git);
        Assert.True(different.Complete);
        Assert.Equal(new[] { "East, 1", "East, 2" }, different.RoutineArguments["git"]);
    }

    [Fact]
    public void Bilgi_ilk_adimda_okunur()
    {
        // rutin parametreyi içeride değiştirse de çağrıdaki bilgi sayılır
        var report = ProgramRun.Execute("def git(yon):\n    yon = East\n    move(yon)\n    collect()\ngit(North)\ngit(South)\n", Row(), routines: Git);
        Assert.Equal(new[] { "North", "South" }, report.RoutineArguments["git"]);
    }

    [Fact]
    public void Rutin_sayisinda_parametresiz_rutin_sayilmaz_ve_parametre_mesaji_gelir()
    {
        var one = new RoutineRule(new string[0], 1, needsParameters: true);
        var report = ProgramRun.Execute("def adim():\n    move(East)\n    collect()\nadim()\nadim()\n", Row(), routines: one);
        Assert.False(report.Complete);
        Assert.Contains("adim parametresiz", report.RoutineProblem.Title);
        Assert.True(ProgramRun.Execute(GitYon + "git(East)\ngit(North)\n", Corner(), routines: one).Complete);
    }

    [Fact]
    public void Parametre_sarti_yoksa_eskisi_gibi()
    {
        var plain = new RoutineRule(new[] { "git" }, 0);
        Assert.True(ProgramRun.Execute(GitYon + "git(East)\ngit(East)\n", Row(), routines: plain).Complete);
        Assert.False(plain.NeedsParameters);
    }
}
