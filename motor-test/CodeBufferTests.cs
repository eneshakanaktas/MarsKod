using MarsKod.Dunya;

namespace MotorTest;

// Kod tamponu: oyunun kendi klavyesiyle yazma, imlec, secim, girinti, palet islemleri.
public class CodeBufferTests
{
    // | isareti imlecin yeri. Tuslar: \n = Enter, \b = geri silme, \t = girinti (⇥), \u0002 = girinti geri (⇤); digerleri harf.
    internal static CodeBuffer Buf(string withCaret)
    {
        var b = new CodeBuffer();
        int caret = withCaret.IndexOf('|');
        b.LoadStart(withCaret.Remove(caret, 1));
        b.SetCaret(caret);
        return b;
    }

    internal static void Keys(CodeBuffer b, string keys)
    {
        foreach (char ch in keys)
        {
            if (ch == '\n') b.Enter();
            else if (ch == '\b') b.Backspace();
            else if (ch == '\t') b.IndentLines();
            else if (ch == '\u0002') b.DedentLines();
            else b.Type(ch.ToString());
        }
    }

    internal static string Show(CodeBuffer b) => b.Text.Insert(b.Caret, "|");

    static string Type(string before, string keys)
    {
        var b = Buf(before);
        Keys(b, keys);
        return Show(b);
    }

    // ---- Yazma kolayliklari (eski CodeTyping/KodYazmaTests durumlari; ikisi Gorev 5'te silindi) ----

    [Fact]
    public void Iki_noktadan_sonra_Enter_bir_kademe_iceriden_baslar()
        => Assert.Equal("for i in range(5):\n    |", Type("for i in range(5):|", "\n"));

    [Fact]
    public void Enter_onceki_satirin_girintisini_korur()
        => Assert.Equal("for i in range(5):\n    move(East)\n    |", Type("for i in range(5):\n    move(East)|", "\n"));

    [Fact]
    public void Ic_ice_bloklarda_girinti_birikir()
        => Assert.Equal("for i in range(2):\n    for j in range(2):\n        |", Type("for i in range(2):\n    for j in range(2):|", "\n"));

    [Fact]
    public void Girintisiz_satirdan_sonra_Enter_girintisiz()
        => Assert.Equal("move(East)\n|", Type("move(East)|", "\n"));

    [Fact]
    public void Bos_satirda_Enter_art_arda_ayni_girintiyi_verir()
        => Assert.Equal("for i in range(5):\n    \n    |", Type("for i in range(5):|", "\n\n"));

    [Fact]
    public void Satirin_ortasinda_Enter_alttaki_parcayi_girintiler()
        => Assert.Equal("for i in range(5):\n    |move(East)", Type("for i in range(5):|move(East)", "\n"));

    [Fact]
    public void Satirin_ortasinda_Enter_alttaki_parcanin_bosluklarini_atar()
        => Assert.Equal("for i in range(5):\n    |move(East)", Type("for i in range(5):|  move(East)", "\n"));

    [Fact]
    public void Satir_basinda_Enter_girinti_eklemez()
        => Assert.Equal("move(East)\n\n|collect()", Type("move(East)\n|collect()", "\n"));

    [Fact]
    public void Geri_silme_girintiyi_bir_kademe_geri_alir()
        => Assert.Equal("for i in range(5):\n    move(East)\n|", Type("for i in range(5):\n    move(East)\n    |", "\b"));

    [Fact]
    public void Iki_kademe_girintide_geri_silme_bir_kademeye_iner()
        => Assert.Equal("a:\n    b:\n    |", Type("a:\n    b:\n        |", "\b"));

    [Fact]
    public void Dort_un_kati_olmayan_girinti_bir_alt_kata_iner()
        => Assert.Equal("a:\n    |", Type("a:\n      |", "\b"));

    [Fact]
    public void Girintinin_ortasinda_geri_silme_de_kademe_kademe()
        => Assert.Equal("    |    x", Type("        |    x", "\b"));

    [Fact]
    public void Yazinin_icindeki_bosluk_tek_tek_silinir()
        => Assert.Equal("move(East) |", Type("move(East)  |", "\b"));

    [Fact]
    public void Girintiden_sonra_yazi_varsa_bosluk_tek_silinir()
        => Assert.Equal("    x = 1  |", Type("    x = 1   |", "\b"));

    [Fact]
    public void Harf_silme_normal_calisir()
        => Assert.Equal("    mov|", Type("    move|", "\b"));

    [Fact]
    public void Satir_basinda_geri_silme_ustteki_satirla_birlestirir()
        => Assert.Equal("move(East)|collect()", Type("move(East)\n|collect()", "\b"));

    [Fact]
    public void En_basta_geri_silme_bir_sey_yapmaz()
        => Assert.Equal("|move(East)", Type("|move(East)", "\b"));

    [Fact]
    public void Yazilan_kod_bolum_cozumu_gibi_calisir()
    {
        // oyuncu hic bosluk yazmadan (yalnizca Enter ve geri silmeyle) Bolum 3'u cozer
        string code = Type("|", "for i in range(5):\nmove(East)\ncollect()\n\bmove(East)").Replace("|", "");
        Assert.Equal("for i in range(5):\n    move(East)\n    collect()\nmove(East)", code);
    }

    // ---- girinti tuslari ----

    [Fact]
    public void Girinti_tusu_satirin_basina_dort_bosluk_ekler()
        => Assert.Equal("for i in range(5):\n    move(E|ast)", Type("for i in range(5):\nmove(E|ast)", "\t"));

    [Fact]
    public void Girinti_geri_tusu_bir_kademe_azaltir()
        => Assert.Equal("a:\n    mo|ve()", Type("a:\n        mo|ve()", "\u0002"));

    [Fact]
    public void Girinti_geri_tusu_dordun_katina_iner()
        => Assert.Equal("a:\n    |x", Type("a:\n      |x", "\u0002"));

    [Fact]
    public void Girintisiz_satirda_girinti_geri_bir_sey_yapmaz()
        => Assert.Equal("mo|ve()", Type("mo|ve()", "\u0002"));

    [Fact]
    public void Secili_satirlarin_hepsi_girintilenir()
    {
        var b = Buf("|for i in range(2):\nmove(East)\ncollect()");
        b.Select(b.Index(1, 0), b.Index(2, 3));
        b.IndentLines();
        Assert.Equal("for i in range(2):\n    move(East)\n    collect()", b.Text);
        Assert.Equal(b.Index(1, 4), b.Anchor);
        Assert.Equal(b.Index(2, 7), b.Caret);
    }

    // ---- secim ----

    [Fact]
    public void Secim_varken_yazi_secimin_yerine_gecer()
    {
        var b = Buf("|move(East)");
        b.Select(5, 9);
        b.Type("N");
        Assert.Equal("move(N|)", Show(b));
    }

    [Fact]
    public void Secim_varken_geri_silme_secimi_siler()
    {
        var b = Buf("|move(East)\ncollect()");
        b.Select(4, 18);
        b.Backspace();
        Assert.Equal("move|()", Show(b));
    }

    [Fact]
    public void Secim_varken_Enter_secimi_silip_yeni_satir_acar()
    {
        var b = Buf("|for i in range(5): xx");
        b.Select(18, 21);
        b.Enter();
        Assert.Equal("for i in range(5):\n    |", Show(b));
    }

    // ---- imlec ----

    [Fact]
    public void Konum_ve_sira_birbirine_cevrilir()
    {
        var b = Buf("|ab\ncde\n\nf");
        Assert.Equal((1, 2), b.Position(5));
        Assert.Equal((2, 0), b.Position(7));
        Assert.Equal(8, b.Index(3, 0));
        Assert.Equal(6, b.Index(1, 99)); // satir kisaysa sonuna
    }

    [Fact]
    public void Imlec_asagi_yukari_sutunu_korur()
    {
        var b = Buf("abcdef|\nab\nabcdef");
        b.MoveVertical(1);
        Assert.Equal("abcdef\nab|\nabcdef", Show(b));
        b.MoveVertical(1);
        Assert.Equal("abcdef\nab\nab|cdef", Show(b));
    }

    [Fact]
    public void Satir_basi_girintinin_sonudur()
    {
        var b = Buf("a:\n    mo|ve()");
        b.MoveToLineEdge(false);
        Assert.Equal("a:\n    |move()", Show(b));
        b.MoveToLineEdge(true);
        Assert.Equal("a:\n    move()|", Show(b));
    }

    [Fact]
    public void Imlec_kodun_disina_cikmaz()
    {
        var b = Buf("ab|");
        b.SetCaret(99);
        Assert.Equal(2, b.Caret);
        b.MoveHorizontal(-9);
        Assert.Equal(0, b.Caret);
    }

    // ---- oneri ----

    [Fact]
    public void Oneri_yarim_kelimeyi_tamamlar_parantezi_acip_kapatir()
    {
        var b = Buf("mo|");
        Assert.Equal("mo", b.WordBeforeCaret());
        b.ApplySuggestion("move", true);
        Assert.Equal("move(|)", Show(b));
    }

    [Fact]
    public void Kapanan_parantezin_onunde_kapa_parantez_yazmak_ustunden_gecer()
    {
        var b = Buf("mo|");
        b.ApplySuggestion("move", true);
        b.Type("East");
        b.Type(")"); // aliskanlikla basildi: ikinci ")" eklenmez
        Assert.Equal("move(East)|", Show(b));
    }

    [Fact]
    public void Elle_acilan_parantez_kendiliginden_kapanmaz()
    {
        var b = Buf("move|");
        b.Type("(");
        Assert.Equal("move(|", Show(b));
        b.Type("East");
        b.Type(")");
        Assert.Equal("move(East)|", Show(b));
    }

    [Fact]
    public void Bos_parantez_icinde_geri_silme_ikisini_birden_siler()
    {
        var b = Buf("mo|");
        b.ApplySuggestion("move", true);
        b.Backspace();
        Assert.Equal("move|", Show(b));
    }

    [Fact]
    public void Dolu_parantezde_geri_silme_yalnizca_bir_harf_siler()
    {
        var b = Buf("move(E|)");
        b.Backspace();
        Assert.Equal("move(|)", Show(b));
    }

    [Fact]
    public void Oneri_kelimenin_sagdaki_parcasini_da_degistirir_parantezi_ikilemez()
    {
        var b = Buf("mo|vx(East)");
        b.ApplySuggestion("move", true);
        Assert.Equal("move(|East)", Show(b));
    }

    [Fact]
    public void Isim_onerisi_parantez_eklemez()
    {
        var b = Buf("move(Ea|");
        b.ApplySuggestion("East", false);
        Assert.Equal("move(East|", Show(b));
    }

    // ---- palet ----

    [Fact]
    public void Bos_koda_birakilan_satir_kodun_yerine_gecer()
    {
        var b = Buf("|");
        Assert.Equal(0, b.DropLine(0, 0, "move(East)"));
        Assert.Equal("move(East)", b.Text);
    }

    [Fact]
    public void Satir_arasina_girintiyle_birakilir()
    {
        var b = Buf("|for i in range(3):\ncollect()");
        Assert.Equal(1, b.DefaultIndentLevel(1));
        b.DropLine(1, b.DefaultIndentLevel(1), "move(East)");
        Assert.Equal("for i in range(3):\n    move(East)\ncollect()", b.Text);
    }

    [Fact]
    public void Varsayilan_girinti_ustteki_satirla_ayni_bos_satirlar_atlanir()
    {
        var b = Buf("|for i in range(3):\n    move(East)\n\n");
        Assert.Equal(1, b.DefaultIndentLevel(3));
        Assert.Equal(0, b.DefaultIndentLevel(0));
    }

    [Fact]
    public void Satir_tasinir_girintisi_yeniden_verilir()
    {
        var b = Buf("|move(East)\nfor i in range(3):\n    collect()");
        int at = b.MoveLine(0, 3, 1); // en alta, bir kademe iceride
        Assert.Equal(2, at);
        Assert.Equal("for i in range(3):\n    collect()\n    move(East)", b.Text);
    }

    [Fact]
    public void Satir_yukari_tasinir()
    {
        var b = Buf("|a()\nb()\nc()");
        Assert.Equal(0, b.MoveLine(2, 0, 0));
        Assert.Equal("c()\na()\nb()", b.Text);
    }

    [Fact]
    public void Satir_silinir_son_satir_silinince_bos_kod_kalir()
    {
        var b = Buf("|a()\nb()");
        b.DeleteLine(0);
        Assert.Equal("b()", b.Text);
        b.DeleteLine(0);
        Assert.Equal("", b.Text);
        Assert.Equal(1, b.LineCount);
    }

    [Fact]
    public void Sayi_arttirilip_azaltilir()
    {
        var b = Buf("|for i in range(3):");
        Assert.True(b.FindNumber(0, 15, out int s, out int n));
        Assert.Equal((15, 1), (s, n));
        Assert.True(b.ChangeNumber(0, 15, +1));
        Assert.Equal("for i in range(4):", b.Text);
        Assert.True(b.ChangeNumber(0, 16, +6)); // sayinin hemen sagina dokunmak da olur
        Assert.Equal("for i in range(10):", b.Text);
        Assert.True(b.ChangeNumber(0, 16, -10));
        Assert.Equal("for i in range(0):", b.Text);
        Assert.False(b.ChangeNumber(0, 15, -1)); // sifirin altina inmez
    }

    [Fact]
    public void Isimdeki_rakam_sayi_sayilmaz()
    {
        var b = Buf("|x2 = 5");
        Assert.False(b.FindNumber(0, 1, out _, out _));
        Assert.True(b.FindNumber(0, 5, out int s, out _));
        Assert.Equal(5, s);
    }

    [Fact]
    public void Tab_ve_satir_sonu_duzeltilir()
    {
        var b = new CodeBuffer();
        b.LoadStart("for i:\r\n\tx()\n");
        Assert.Equal("for i:\n    x()", b.Text);
    }
}
