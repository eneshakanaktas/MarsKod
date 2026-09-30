using MarsKod.Dunya;

namespace MotorTest;

// XP: bolum basina kademe basina bir kez (tasarim belgesi §6). Harfler: B baslangic, D dugme, O oneri, E elle.
public class XpTests
{
    static CodeBuffer Kinds(string code, string kinds)
    {
        var b = new CodeBuffer();
        b.Load(code, kinds);
        return b;
    }

    [Fact]
    public void Cozum_turu_satirlarin_en_dusugu()
        => Assert.Equal(LineKind.Oneri, Xp.SolutionKind(Kinds("a = 1\nmove(East)\ncollect()", "EOE")));

    [Fact]
    public void Bos_ve_yorum_satirlari_sayilmaz()
        => Assert.Equal(LineKind.Elle, Xp.SolutionKind(Kinds("# not\n\nmove(East)", "DDE")));

    [Fact]
    public void Baslangic_kodu_Acemi_sayilir()
        => Assert.Equal(LineKind.Dugme, Xp.SolutionKind(Kinds("move(East)\ncollect()", "BE")));

    [Fact]
    public void Baslangic_kodu_ayari_kapaliysa_yok_sayilir()
        => Assert.Equal(LineKind.Elle, Xp.SolutionKind(Kinds("move(East)\ncollect()", "BE"), startCodeCounts: false));

    [Fact]
    public void Sayilan_satir_yoksa_en_kolay_kademe()
        => Assert.Equal(LineKind.Dugme, Xp.SolutionKind(Kinds("# yalniz yorum", "E")));

    [Fact]
    public void Miktarlar_30_50_100()
    {
        Assert.Equal(30, Xp.Amount(LineKind.Dugme));
        Assert.Equal(50, Xp.Amount(LineKind.Oneri));
        Assert.Equal(100, Xp.Amount(LineKind.Elle));
        Assert.Equal(30, Xp.Amount(LineKind.Baslangic));
    }

    [Fact]
    public void Bolum_basina_en_cok_180_ve_tekrar_XP_yok()
    {
        var xp = new Xp();
        Assert.Equal(30, xp.Award(1, LineKind.Dugme));
        Assert.Equal(0, xp.Award(1, LineKind.Dugme));
        Assert.Equal(50, xp.Award(1, LineKind.Oneri));
        Assert.Equal(100, xp.Award(1, LineKind.Elle));
        Assert.Equal(0, xp.Award(1, LineKind.Elle));
        Assert.Equal(180, xp.Total);
    }

    [Fact]
    public void Baslangic_ile_alinan_Acemi_sayilir()
    {
        var xp = new Xp();
        Assert.Equal(30, xp.Award(1, LineKind.Baslangic));
        Assert.Equal(0, xp.Award(1, LineKind.Dugme));
    }

    [Fact]
    public void Her_bolum_ayri_sayilir()
    {
        var xp = new Xp();
        xp.Award(1, LineKind.Elle);
        Assert.Equal(100, xp.Award(2, LineKind.Elle));
        Assert.Equal(200, xp.Total);
    }

    [Fact]
    public void Bolum_XPsi_yalnizca_o_bolumu_sayar()
    {
        var xp = new Xp();
        xp.Award(1, LineKind.Dugme);
        xp.Award(1, LineKind.Elle);
        xp.Award(2, LineKind.Oneri);
        Assert.Equal(130, xp.LevelTotal(1));
        Assert.Equal(50, xp.LevelTotal(2));
        Assert.Equal(0, xp.LevelTotal(3));
    }

    [Fact]
    public void Sonraki_zor_kademe_alinmamis_olan()
    {
        var xp = new Xp();
        Assert.Equal(LineKind.Oneri, xp.NextBetter(1, LineKind.Dugme));
        xp.Award(1, LineKind.Oneri);
        Assert.Equal(LineKind.Elle, xp.NextBetter(1, LineKind.Dugme));
        xp.Award(1, LineKind.Elle);
        Assert.Null(xp.NextBetter(1, LineKind.Dugme));
        Assert.Null(xp.NextBetter(2, LineKind.Elle));
    }

    [Fact]
    public void Sonraki_kademe_metni()
    {
        var xp = new Xp();
        Assert.Equal("Usta ile çözersen +100 XP daha", xp.NextBetterText(1, LineKind.Oneri));
        Assert.Equal("Orta ile çözersen +50 XP daha", xp.NextBetterText(1, LineKind.Dugme));
        Assert.Equal("", xp.NextBetterText(1, LineKind.Elle));
    }

    [Fact]
    public void Kayit_gidip_geliyor()
    {
        var xp = new Xp();
        xp.Award(5, LineKind.Elle);
        xp.Award(3, LineKind.Oneri);
        xp.Award(3, LineKind.Dugme);
        Assert.Equal("3:DO;5:E", xp.Save());
        var back = Xp.Load(xp.Save());
        Assert.Equal(180, back.Total);
        Assert.Equal(0, back.Award(3, LineKind.Oneri));
        Assert.Equal(100, back.Award(3, LineKind.Elle));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("bozuk")]
    [InlineData("3:DX")]
    [InlineData("a:D")]
    [InlineData("3:")]
    [InlineData("3:D;;5:E")]
    [InlineData("-1:D")]
    public void Bozuk_kayit_sifirdan_baslar(string saved)
    {
        var xp = Xp.Load(saved);
        Assert.Equal(0, xp.Total);
        Assert.Equal("", xp.Save());
    }
}
