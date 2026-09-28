using MarsKod.Dunya;
using static MotorTest.CodeBufferTests;

namespace MotorTest;

// Satir turleri: tasarim belgesi §6'daki tablonun her satiri (docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md).
// Harfler: B baslangic, D dugme, O oneri, E elle.
public class LineTagsTests
{
    static CodeBuffer Empty() => Buf("|");

    [Fact]
    public void Paletten_birakilan_satir_Dugme()
    {
        var b = Empty();
        b.DropLine(0, 0, "move(East)");
        b.DropLine(1, 0, "collect()");
        Assert.Equal("DD", b.SaveKinds());
    }

    [Fact]
    public void Baslangic_kodu_satirlari_Baslangic()
        => Assert.Equal("BB", Buf("|move(East)\ncollect()").SaveKinds());

    [Fact]
    public void Klavyeyle_yazilan_yeni_satir_Elle()
    {
        var b = Empty();
        Keys(b, "move(East)\ncollect()");
        Assert.Equal("EE", b.SaveKinds());
    }

    [Fact]
    public void Yeni_satirda_oneri_kullanilirsa_Oneri()
    {
        var b = Empty();
        Keys(b, "mo");
        b.ApplySuggestion("move", true);
        Keys(b, "East)\ncollect()");
        Assert.Equal("OE", b.SaveKinds());
    }

    [Fact]
    public void Dugme_satiri_yaziyla_duzeltilse_de_Dugme_kalir()
    {
        var b = Empty();
        b.DropLine(0, 0, "move(North)");
        b.Select(5, 10);
        Keys(b, "East");
        Assert.Equal("move(East)", b.Text);
        Assert.Equal("D", b.SaveKinds());
    }

    [Fact]
    public void Elle_satirda_oneri_kullanilirsa_Oneriye_duser()
    {
        var b = Empty();
        Keys(b, "move(Ea");
        Assert.Equal("E", b.SaveKinds());
        b.ApplySuggestion("East", false);
        Assert.Equal("O", b.SaveKinds());
    }

    [Fact]
    public void Dugme_satirinda_oneri_Dugme_kalir()
    {
        var b = Empty();
        b.DropLine(0, 0, "move(North)");
        b.SetCaret(7);
        b.ApplySuggestion("North", false);
        Assert.Equal("D", b.SaveKinds());
    }

    [Fact]
    public void Sayi_degistirmek_turu_degistirmez()
    {
        var b = Empty();
        b.DropLine(0, 0, "for i in range(3):");
        b.ChangeNumber(0, 15, 2);
        Assert.Equal("D", b.SaveKinds());
    }

    [Fact]
    public void Satir_tamamen_silinip_yeniden_yazilirsa_yeni_tur_alir()
    {
        var b = Buf("move(East)|");
        Keys(b, "\b\b\b\b\b\b\b\b\b\b");
        Assert.Equal("", b.Text);
        Keys(b, "move(East)");
        Assert.Equal("E", b.SaveKinds());
    }

    [Fact]
    public void Satir_secilip_ustune_yazilirsa_yeni_tur_alir()
    {
        var b = Buf("|move(East)");
        b.Select(0, 10);
        Keys(b, "collect()");
        Assert.Equal("E", b.SaveKinds());
    }

    [Fact]
    public void Girinti_disinda_bir_harf_kalirsa_eski_tur_kalir()
    {
        var b = Buf("    move(East)|");
        Keys(b, "\b\b\b\b\b\b\b\b\b");
        Assert.Equal("    m", b.Text);
        Keys(b, "ove(East)");
        Assert.Equal("B", b.SaveKinds());
    }

    [Fact]
    public void Bolunen_satirin_iki_parcasi_da_eski_turu_alir()
    {
        var b = Empty();
        b.DropLine(0, 0, "move(East)collect()");
        b.SetCaret(10);
        b.Enter();
        Assert.Equal("move(East)\ncollect()", b.Text);
        Assert.Equal("DD", b.SaveKinds());
    }

    [Fact]
    public void Satir_sonunda_Enter_yeni_bos_satir_acar_yazilan_Elle()
    {
        var b = Buf("move(East)|");
        Keys(b, "\ncollect()");
        Assert.Equal("BE", b.SaveKinds());
    }

    [Fact]
    public void Satir_basinda_Enter_ustte_yeni_satir_acar()
    {
        var b = Buf("|move(East)");
        Keys(b, "\n");
        b.SetCaret(0);
        Keys(b, "collect()");
        Assert.Equal("collect()\nmove(East)", b.Text);
        Assert.Equal("EB", b.SaveKinds());
    }

    [Fact]
    public void Birlesen_satirlar_en_dusugu_alir()
    {
        var b = Empty();
        Keys(b, "move(East)");
        b.DropLine(1, 0, "collect()");
        Assert.Equal("ED", b.SaveKinds());
        b.SetCaret(b.Index(1, 0));
        b.Backspace();
        Assert.Equal("move(East)collect()", b.Text);
        Assert.Equal("D", b.SaveKinds());
    }

    [Fact]
    public void Bos_satirla_birlesmek_turu_dusurmez()
    {
        var b = Empty();
        Keys(b, "move(East)\n");
        b.Backspace();
        Assert.Equal("E", b.SaveKinds());
    }

    [Fact]
    public void Tasinan_satir_turunu_goturur()
    {
        var b = Empty();
        Keys(b, "a()");
        b.DropLine(1, 0, "b()");
        b.MoveLine(1, 0, 0);
        Assert.Equal("b()\na()", b.Text);
        Assert.Equal("DE", b.SaveKinds());
    }

    [Fact]
    public void Girinti_tuslari_turu_degistirmez()
    {
        var b = Buf("|move(East)");
        Keys(b, "\t\u0002");
        Assert.Equal("B", b.SaveKinds());
    }

    [Fact]
    public void Bos_ve_yorum_satirlari_hesaba_katilmaz()
    {
        var b = Buf("|move(East)\n\n    # yorum\n    ");
        Assert.True(b.Counts(0));
        Assert.False(b.Counts(1));
        Assert.False(b.Counts(2));
        Assert.False(b.Counts(3));
    }

    [Fact]
    public void Turler_kaydedilip_geri_okunur()
    {
        var b = new CodeBuffer();
        b.Load("a()\nb()\nc()\nd()", "BDOE");
        Assert.Equal("BDOE", b.SaveKinds());
    }

    [Fact]
    public void Bozuk_ya_da_eksik_kayitta_hepsi_Dugme_sayilir()
    {
        var b = new CodeBuffer();
        b.Load("a()\nb()", "E");        // satir sayisi tutmuyor
        Assert.Equal("DD", b.SaveKinds());
        b.Load("a()\nb()", null);       // eski kayit: tur yok
        Assert.Equal("DD", b.SaveKinds());
        b.Load("a()\nb()", "EX");       // bilinmeyen harf
        Assert.Equal("ED", b.SaveKinds());
    }
}
