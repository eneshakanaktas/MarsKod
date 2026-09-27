using MarsKod.Dunya;

namespace MotorTest;

// Kod yazma kolayliklari: otomatik girinti, girintiyi bir kademe silme, Tab.
public class KodYazmaTests
{
    // before'da imlec | isaretinin yerinde; o noktaya "typed" yazilir (\b = geri silme). Sonuc da | ile imlecli doner.
    static string Type(string beforeWithCaret, string typed)
    {
        int caret = beforeWithCaret.IndexOf('|');
        string text = beforeWithCaret.Remove(caret, 1);
        foreach (char ch in typed)
        {
            string after;
            int c;
            if (ch == '\b') { after = text.Remove(caret - 1, 1); c = caret - 1; }
            else { after = text.Insert(caret, ch.ToString()); c = caret + 1; }
            text = CodeTyping.Apply(text, after, ref c);
            caret = c;
        }
        return text.Insert(caret, "|");
    }

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
    public void Geri_silme_girintiyi_bir_kademe_geri_alir()
        => Assert.Equal("for i in range(5):\n    move(East)\n|", Type("for i in range(5):\n    move(East)\n    |", "\b"));

    [Fact]
    public void Iki_kademe_girintide_geri_silme_bir_kademeye_iner()
        => Assert.Equal("a:\n    b:\n    |", Type("a:\n    b:\n        |", "\b"));

    [Fact]
    public void Dort_un_kati_olmayan_girinti_bir_alt_kata_iner()
        => Assert.Equal("a:\n    |", Type("a:\n      |", "\b"));

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
    public void Tab_dort_bosluk_olur()
        => Assert.Equal("for i in range(5):\n    |", Type("for i in range(5):\n|", "\t"));

    [Fact]
    public void Yapistirilan_koddaki_tablar_da_bosluk_olur()
    {
        int caret = 11;
        var fixedText = CodeTyping.Apply("", "for i:\n\tx()", ref caret);
        Assert.Equal("for i:\n    x()", fixedText);
        Assert.Equal(14, caret);
    }

    [Fact]
    public void Satir_basinda_Enter_girinti_eklemez()
        => Assert.Equal("move(East)\n\n|collect()", Type("move(East)\n|collect()", "\n"));

    [Fact]
    public void Telefonda_imlec_gec_gelse_de_Enter_girintiler()
    {
        // telefonda yazi degistiginde imlec henuz eski yerinde (Enter'dan once) olabilir
        string before = "for i in range(5):";
        int caret = before.Length;
        var fixedText = CodeTyping.Apply(before, before + "\n", ref caret);
        Assert.Equal("for i in range(5):\n    ", fixedText);
        Assert.Equal(fixedText.Length, caret);
    }

    [Fact]
    public void Telefonda_imlec_gec_gelse_de_geri_silme_girintiyi_alir()
    {
        string before = "a:\n    b\n    ";
        int caret = before.Length; // silmeden onceki yer
        var fixedText = CodeTyping.Apply(before, before.Substring(0, before.Length - 1), ref caret);
        Assert.Equal("a:\n    b\n", fixedText);
        Assert.Equal(fixedText.Length, caret);
    }

    [Fact]
    public void Yazilan_kod_bolum_cozumu_gibi_calisir()
    {
        // oyuncu telefonda hic bosluk yazmadan (yalnizca Enter ve geri silmeyle) Bolum 3'u cozer
        string code = Type("|", "for i in range(5):\nmove(East)\ncollect()\n\bmove(East)").Replace("|", "");
        Assert.Equal("for i in range(5):\n    move(East)\n    collect()\nmove(East)", code);
    }
}
