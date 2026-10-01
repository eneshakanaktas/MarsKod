using MarsKod.Dunya;

namespace MotorTest;

// Ipuclari: 1. ipucu hep acik, "bir ipucu daha" ile 2. ve 3. acilir; bolum bolum saklanir.
public class HintLogTests
{
    [Fact]
    public void Ilk_ipucu_hep_acik()
    {
        Assert.Equal(1, new HintLog().Shown(3));
    }

    [Fact]
    public void Ipuclari_sirayla_acilir_ve_fazlasi_acilmaz()
    {
        var log = new HintLog();
        Assert.Equal(2, log.Reach(3, 2, 3));
        Assert.Equal(3, log.Reach(3, 3, 3));
        Assert.Equal(3, log.Reach(3, 4, 3));
        Assert.Equal(3, log.Shown(3));
        Assert.Equal(1, log.Shown(2));
    }

    [Fact]
    public void Daha_dusuk_siraya_donmek_kaydi_dusurmez()
    {
        var log = new HintLog();
        log.Reach(3, 3, 3);
        Assert.Equal(3, log.Reach(3, 1, 3));
        Assert.Equal(3, log.Shown(3));
    }

    [Fact]
    public void Tek_ipuclu_bolumde_acilacak_bir_sey_yok()
    {
        var log = new HintLog();
        Assert.Equal(1, log.Reach(1, 1, 1));
        Assert.Equal("", log.Save());
    }

    [Fact]
    public void Kaydedilip_geri_yuklenir()
    {
        var log = new HintLog();
        log.Reach(5, 3, 3);
        log.Reach(3, 2, 3);
        Assert.Equal("3:2;5:3", log.Save());
        var back = HintLog.Load(log.Save());
        Assert.Equal(2, back.Shown(3));
        Assert.Equal(3, back.Shown(5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("bozuk")]
    [InlineData("3:")]
    [InlineData("3:1")]
    [InlineData("3:2;;5:3")]
    [InlineData("-1:2")]
    public void Bozuk_kayit_sifirdan_baslar(string saved)
    {
        var log = HintLog.Load(saved);
        Assert.Equal(1, log.Shown(3));
        Assert.Equal("", log.Save());
    }
}

// Ampul dongusu: 1. -> 2. -> 3. -> kapali; tekrar acilinca 1.'den baslar.
public class HintViewTests
{
    [Fact]
    public void Baslangicta_kapali()
    {
        Assert.False(new HintView().Open);
    }

    [Fact]
    public void Ampul_sirayla_ipuclarini_acar_sonunda_kapatir()
    {
        var v = new HintView();
        v.Press(3); Assert.Equal(1, v.Current);
        v.Press(3); Assert.Equal(2, v.Current);
        v.Press(3); Assert.Equal(3, v.Current);
        v.Press(3); Assert.False(v.Open);
    }

    [Fact]
    public void Kapaninca_yeniden_acilis_birinciden_baslar()
    {
        var v = new HintView();
        v.Press(3); v.Press(3); v.Press(3); v.Press(3);
        v.Press(3);
        Assert.Equal(1, v.Current);
    }

    [Fact]
    public void Tek_ipuclu_bolumde_ac_kapa()
    {
        var v = new HintView();
        v.Press(1); Assert.Equal(1, v.Current);
        v.Press(1); Assert.False(v.Open);
    }

    [Fact]
    public void Elle_kapatinca_siradaki_basis_birinciyi_acar()
    {
        var v = new HintView();
        v.Press(3); v.Press(3);
        v.Close();
        v.Press(3);
        Assert.Equal(1, v.Current);
    }

    [Fact]
    public void Ipucu_yoksa_kapali_kalir()
    {
        var v = new HintView();
        v.Press(0);
        Assert.False(v.Open);
    }
}
