using MarsKod.Dunya;

namespace MotorTest;

// Parametre şartı (Bölge 7, Bölüm 61-70, parametreli fonksiyonlar): bölüm bir rutinin sayı almasını ister;
// rutin en az bir parametreyle tanımlanmalı ve en az iki farklı değerle çağrılmalı.
public class ParametricRoutineTests
{
    // Robot (0,0); doğuda (1,0), (2,0), (3,0) toplanacak; görev üçünü toplamak.
    static World Row() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(2, 0), new Cell(3, 0) },
        null, null, new[] { "move", "collect" });

    static readonly RoutineRule Git = new RoutineRule(new string[0], 0, new[] { "git" });

    const string Walk = "def git(n):\n    for i in range(n):\n        move(East)\n        collect()\n";

    [Fact]
    public void Iki_farkli_sayiyla_cagrilan_parametreli_rutin_sarti_saglar()
    {
        var report = ProgramRun.Execute(Walk + "git(1)\ngit(2)\n", Row(), routines: Git);
        Assert.True(report.Complete);
        Assert.Null(report.RoutineProblem);
    }

    [Fact]
    public void Parametresiz_tanim_bir_sayi_almiyor_der()
    {
        var report = ProgramRun.Execute("def git():\n    move(East)\n    collect()\ngit()\ngit()\ngit()\n", Row(), routines: Git);
        Assert.False(report.Complete);
        Assert.Contains("git bir sayı almıyor", report.RoutineProblem.Title);
        Assert.Contains("def git(boy):", report.RoutineProblem.Text);
    }

    [Fact]
    public void Hep_ayni_sayiyla_cagri_yetmez()
    {
        var report = ProgramRun.Execute(Walk + "git(1)\ngit(1)\ngit(1)\n", Row(), routines: Git);
        Assert.False(report.Complete);
        Assert.Contains("git hep aynı sayıyla çağrıldı", report.RoutineProblem.Title);
        Assert.Contains("git(3)", report.RoutineProblem.Text);
    }

    [Fact]
    public void Varsayilan_parametre_hep_ayni_sayilir()
    {
        var report = ProgramRun.Execute("def git(n=1):\n    move(East)\n    collect()\ngit()\ngit()\ngit()\n", Row(), routines: Git);
        Assert.False(report.Complete);
        Assert.Contains("hep aynı sayıyla", report.RoutineProblem.Title);
    }

    [Fact]
    public void Dongu_degiskeni_ile_cagri_gercek_degerleri_sayar()
    {
        var report = ProgramRun.Execute(Walk + "for i in range(1, 3):\n    git(i)\n", Row(), routines: Git);
        Assert.True(report.Complete);
        Assert.Equal(new[] { "1", "2" }, report.RoutineArgs["git"]);
    }

    [Fact]
    public void Degiskenle_cagri_degerini_sayar()
    {
        var report = ProgramRun.Execute(Walk + "k = 1\ngit(k)\nk = 2\ngit(k)\n", Row(), routines: Git);
        Assert.True(report.Complete);
    }

    [Fact]
    public void Tanimsiz_parametreli_rutin_yok_der()
    {
        var report = ProgramRun.Execute("move(East)\ncollect()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", Row(), routines: Git);
        Assert.False(report.Complete);
        Assert.Contains("git rutini yok", report.RoutineProblem.Title);
    }

    [Fact]
    public void Bir_kez_calisan_parametreli_rutin_yetmez()
    {
        var report = ProgramRun.Execute(Walk + "git(3)\n", Row(), routines: Git);
        Assert.False(report.Complete);
        Assert.Contains("yalnızca 1 kez", report.RoutineProblem.Title);
    }

    [Fact]
    public void Rapor_parametre_sayisini_ve_argumanlari_yazar()
    {
        var report = ProgramRun.Execute("def yuru(yon, adim):\n    for i in range(adim):\n        move(yon)\nyuru(East, 1)\nyuru(East, 2)\n", Row());
        Assert.Equal(2, report.RoutineParams["yuru"]);
        Assert.Equal(new[] { "'East'|1", "'East'|2" }, report.RoutineArgs["yuru"]);
    }

    [Fact]
    public void Parametresiz_rutinin_argumani_bos_metindir()
    {
        var report = ProgramRun.Execute("def adim():\n    move(East)\nadim()\nadim()\n", Row());
        Assert.Equal(0, report.RoutineParams["adim"]);
        Assert.Equal(new[] { "", "" }, report.RoutineArgs["adim"]);
    }

    [Fact]
    public void Sart_yazilmadiysa_eskisi_gibi()
    {
        var rule = new RoutineRule(new[] { "git" }, 0);
        Assert.Empty(rule.ParametricNames);
        Assert.False(rule.IsEmpty);
        Assert.False(new RoutineRule(new string[0], 0, new[] { "git" }).IsEmpty); // yalnızca parametre şartı da bir şarttır
        Assert.True(RoutineRule.None.IsEmpty);
    }
}
