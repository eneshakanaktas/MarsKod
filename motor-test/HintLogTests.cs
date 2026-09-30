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
        Assert.Equal(2, log.Reveal(3, 3));
        Assert.Equal(3, log.Reveal(3, 3));
        Assert.Equal(3, log.Reveal(3, 3));
        Assert.Equal(3, log.Shown(3));
        Assert.Equal(1, log.Shown(2));
    }

    [Fact]
    public void Tek_ipuclu_bolumde_acilacak_bir_sey_yok()
    {
        var log = new HintLog();
        Assert.Equal(1, log.Reveal(1, 1));
        Assert.Equal("", log.Save());
    }

    [Fact]
    public void Kaydedilip_geri_yuklenir()
    {
        var log = new HintLog();
        log.Reveal(5, 3); log.Reveal(5, 3);
        log.Reveal(3, 3);
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
