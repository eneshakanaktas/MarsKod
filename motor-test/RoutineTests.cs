using System.Collections.Generic;
using MarsKod.Dunya;

namespace MotorTest;

// Rutin şartı (Bölge 6, Bölüm 51-60, fonksiyonlar): bölüm bir rutin istiyorsa oyuncu onu def ile yazmalı ve
// rutin en az iki kez çalışmalı; yoksa robot işi bitirse bile görev tamam sayılmaz.
public class RoutineTests
{
    // Robot (0,0); doğuda (1,0), (2,0), (3,0) toplanacak; görev üçünü toplamak.
    static World Row() => new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0), new Cell(2, 0), new Cell(3, 0) },
        null, null, new[] { "move", "collect" });

    static readonly RoutineRule Adim = new RoutineRule(new[] { "adim" }, 0);

    const string Routine = "def adim():\n    move(East)\n    collect()\n";

    [Fact]
    public void Uc_kez_cagrilan_rutin_gorevi_bitirir_ve_sayilir()
    {
        var report = ProgramRun.Execute(Routine + "adim()\nadim()\nadim()\n", Row(), routines: Adim);
        Assert.True(report.Complete);
        Assert.Null(report.RoutineProblem);
        Assert.Equal(3, report.RoutineRuns["adim"]);
        Assert.Contains("adim", report.DefinedRoutines);
    }

    [Fact]
    public void Donguden_cagri_her_turda_sayilir()
    {
        var report = ProgramRun.Execute(Routine + "for i in range(3):\n    adim()\n", Row(), routines: Adim);
        Assert.True(report.Complete);
        Assert.Equal(3, report.RoutineRuns["adim"]);
    }

    [Fact]
    public void Rutinsiz_kopyala_yapistir_gorevi_bitirmez()
    {
        var world = Row();
        var report = ProgramRun.Execute("move(East)\ncollect()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", world, routines: Adim);
        Assert.True(world.Complete);   // dünya tamam...
        Assert.False(report.Complete); // ...ama rutin yok
        Assert.Contains("adim rutini yok", report.RoutineProblem.Title);
        Assert.Contains("def adim():", report.RoutineProblem.Text);
    }

    [Fact]
    public void Tanimlanip_cagrilmayan_rutin_hic_calismadi_der()
    {
        var report = ProgramRun.Execute(Routine + "move(East)\ncollect()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", Row(), routines: Adim);
        Assert.False(report.Complete);
        Assert.Contains("hiç çalışmadı", report.RoutineProblem.Title);
    }

    [Fact]
    public void Bir_kez_calisan_rutin_yetmez()
    {
        var report = ProgramRun.Execute(Routine + "adim()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", Row(), routines: Adim);
        Assert.False(report.Complete);
        Assert.Contains("yalnızca 1 kez", report.RoutineProblem.Title);
    }

    [Fact]
    public void Rutin_icinde_rutin_ikisi_de_sayilir()
    {
        var both = new RoutineRule(new[] { "adim", "tur" }, 0);
        var report = ProgramRun.Execute(Routine + "def tur():\n    adim()\n    adim()\ntur()\nadim()\n", Row(), routines: both);
        Assert.Equal(3, report.RoutineRuns["adim"]);
        Assert.Equal(1, report.RoutineRuns["tur"]);
        Assert.Contains("tur", report.RoutineProblem.Title); // tur yalnızca 1 kez
    }

    [Fact]
    public void Serbest_adli_rutin_sayisi()
    {
        var one = new RoutineRule(new string[0], 1);
        Assert.True(ProgramRun.Execute("def git():\n    move(East)\n    collect()\ngit()\ngit()\ngit()\n", Row(), routines: one).Complete);
        var two = new RoutineRule(new string[0], 2);
        var report = ProgramRun.Execute("def git():\n    move(East)\n    collect()\ngit()\ngit()\ngit()\n", Row(), routines: two);
        Assert.False(report.Complete);
        Assert.Contains("2 rutin", report.RoutineProblem.Title);
    }

    [Fact]
    public void Tanimdan_once_cagri_python_hatasidir()
    {
        var report = ProgramRun.Execute("adim()\n" + Routine + "adim()\nadim()\n", Row(), routines: Adim);
        Assert.Equal("NameError", report.Error.Type);
        Assert.False(report.Complete);
        Assert.Null(report.RoutineProblem); // kod durduysa rutin mesajı yok, hata mesajı yeter
    }

    [Fact]
    public void Sart_yoksa_eskisi_gibi()
    {
        Assert.True(ProgramRun.Execute("move(East)\ncollect()\nmove(East)\ncollect()\nmove(East)\ncollect()\n", Row()).Complete);
        Assert.True(RoutineRule.None.IsEmpty);
    }
}
